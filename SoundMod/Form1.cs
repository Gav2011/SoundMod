using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SoundMod
{
    public partial class Form1 : Form
    {
        private string soundDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SoundBoard");
        private List<WasapiOut> activeOutputs = new List<WasapiOut>();
        private List<AudioFileReader> activeAudioFiles = new List<AudioFileReader>();
        private MMDeviceEnumerator deviceEnumerator;
        private MMDevice selectedDevice;
        private string lastPlayedFilePath;
        private bool isPaused = false;

        private WaveInEvent waveIn;
        private BufferedWaveProvider waveProvider;
        private WasapiOut micOutputDevice;
        private float micVolume = 1.0f; // Default mic volume is 100%

        public Form1()
        {
            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;
            InitializeComponent();
            InitializeDeviceList();
            LoadSoundButtons();
            InitializeControls();
            InitializeMicrophoneList();
            StartMic();
            LoadLoadout();
            VersionInfomation();
        }

        private void VersionInfomation()
        {
            const string CurrentVersion = "1.0";
            const string VersionFileUrl = "https://raw.githubusercontent.com/Gav2011/Versions/refs/heads/main/SoundMod";
            const string LatestReleaseUrl = "https://github.com/Gav2011/InfernoInjector/releases/latest/download/InfernoInjector.exe";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string latestVersion = (await client.GetStringAsync(VersionFileUrl)).Trim();

                    if (latestVersion != CurrentVersion)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            $"A new version ({latestVersion}) is available.\nDo you want to update now?",
                            "Update Available",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                            string exeDir = Path.GetDirectoryName(exePath);
                            string updatePath = Path.Combine(exeDir, "SoundMod.exe");
                            string vbsPath = Path.Combine(exeDir, "update.vbs");
                            string vbs = $@"
Set WshShell = CreateObject(""WScript.Shell"")
WshShell.Run ""cmd /c taskkill /f /im SoundMod.exe"", 0, True
WScript.Sleep 5000 ' Increased sleep time to ensure process is killed
Set fso = CreateObject(""Scripting.FileSystemObject"")
If fso.FileExists(""{exePath}"") Then
    fso.DeleteFile ""{exePath}"", True
Else
    WshShell.Popup ""Old executable not found"", 0, ""Debug"", 0
End If
Set objXML = CreateObject(""WinHTTP.WinHTTPRequest.5.1"")
objXML.Open ""GET"", ""{LatestReleaseUrl}"", False
objXML.Send
Set objStream = CreateObject(""ADODB.Stream"")
objStream.Open
objStream.Type = 1 ' Binary
objStream.Write objXML.ResponseBody
objStream.SaveToFile ""{updatePath}"", 2 ' Overwrite
objStream.Close
WshShell.Run ""{updatePath}"", 0, False
WScript.Sleep 2000
fso.DeleteFile ""{vbsPath}"", True
";

                            File.WriteAllText(vbsPath, vbs);
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "wscript.exe",
                                Arguments = $"\"{vbsPath}\"",
                                CreateNoWindow = true,
                                UseShellExecute = false
                            });
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for updates:\n{ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string configPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Options.txt");
        private void LoadLoadout()
        {
            if (!File.Exists(configPath)) return;

            var lines = File.ReadAllLines(configPath);

            foreach (var line in lines)
            {
                if (line.StartsWith("comboBoxOutputDevices="))
                {
                    var deviceName = line.Substring("comboBoxOutputDevices=".Length);
                    int index = comboBoxOutputDevices.Items.IndexOf(deviceName);
                    if (index >= 0)
                    {
                        comboBoxOutputDevices.SelectedIndex = index;
                    }
                }
                else if (line.StartsWith("comboBoxOutputDevices2="))
                {
                    var deviceName = line.Substring("comboBoxOutputDevices2=".Length);
                    int index = comboBoxOutputDevices2.Items.IndexOf(deviceName);
                    if (index >= 0)
                    {
                        comboBoxOutputDevices2.SelectedIndex = index;
                    }
                }
                else if (line.StartsWith("comboBoxMicrophones="))
                {
                    var micName = line.Substring("comboBoxMicrophones=".Length);
                    int index = comboBoxMicrophones.Items.IndexOf(micName);
                    if (index >= 0)
                    {
                        comboBoxMicrophones.SelectedIndex = index;
                    }
                }
                else if (line.StartsWith("volumeControl="))
                {
                    if (int.TryParse(line.Substring("volumeControl=".Length), out int vol))
                    {
                        volumeControl.Value = Math.Min(Math.Max(vol, volumeControl.Minimum), volumeControl.Maximum);
                        SetVolume(volumeControl.Value / 100f);
                    }
                }
                else if (line.StartsWith("MicVolControl="))
                {
                    if (int.TryParse(line.Substring("MicVolControl=".Length), out int micVol))
                    {
                        MicVolControl.Value = Math.Min(Math.Max(micVol, MicVolControl.Minimum), MicVolControl.Maximum);
                        micVolume = MicVolControl.Value / 100f;
                    }
                }
            }
        }

        private void SaveLoadout()
        {
            try
            {
                var lines = new List<string>();

                if (comboBoxOutputDevices.SelectedIndex >= 0)
                {
                    string deviceName = comboBoxOutputDevices.SelectedItem.ToString();
                    lines.Add($"comboBoxOutputDevices={deviceName}");
                }

                if (comboBoxOutputDevices2.SelectedIndex >= 0)
                {
                    string deviceName = comboBoxOutputDevices2.SelectedItem.ToString();
                    lines.Add($"comboBoxOutputDevices2={deviceName}");
                }

                if (comboBoxMicrophones.SelectedIndex >= 0)
                {
                    string micName = comboBoxMicrophones.SelectedItem.ToString();
                    lines.Add($"comboBoxMicrophones={micName}");
                }

                lines.Add($"volumeControl={volumeControl.Value}");
                lines.Add($"MicVolControl={MicVolControl.Value}");

                File.WriteAllLines(configPath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save config: " + ex.Message);
            }
        }

        private void InitializeMicrophoneList()
        {
            comboBoxMicrophones.Items.Clear();
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var deviceInfo = WaveInEvent.GetCapabilities(i);
                comboBoxMicrophones.Items.Add(deviceInfo.ProductName);
            }

            if (comboBoxMicrophones.Items.Count > 0)
                comboBoxMicrophones.SelectedIndex = 0;

            comboBoxMicrophones.SelectedIndexChanged += (s, e) =>
            {
                if (waveIn != null)
                {
                    waveIn.DeviceNumber = comboBoxMicrophones.SelectedIndex;
                }
            };
        }

        private void StartMic()
        {
            if (selectedDevice == null)
            {
                selectedDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }

            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(44100, 1);
            waveProvider = new BufferedWaveProvider(waveIn.WaveFormat);

            waveIn.DataAvailable += (s, a) =>
            {
                byte[] buffer = a.Buffer;
                int bytesRecorded = a.BytesRecorded;

                for (int i = 0; i < bytesRecorded; i += 2)
                {
                    short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                    float adjusted = Math.Max(-1.0f, Math.Min(1.0f, sample / 32768f * micVolume));
                    short newSample = (short)(adjusted * 32768);
                    buffer[i] = (byte)(newSample & 0xFF);
                    buffer[i + 1] = (byte)((newSample >> 8) & 0xFF);
                }

                waveProvider.AddSamples(buffer, 0, bytesRecorded);
            };

            micOutputDevice = new WasapiOut(selectedDevice, AudioClientShareMode.Shared, true, 200);
            micOutputDevice.Init(waveProvider);
            micOutputDevice.Play();

            waveIn.StartRecording();
        }

        private void RestartMic()
        {
            micOutputDevice?.Stop();
            micOutputDevice?.Dispose();
            micOutputDevice = null;

            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveIn = null;

            StartMic();
        }

        private void InitializeDeviceList()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            comboBoxOutputDevices.Items.Clear();
            comboBoxOutputDevices2.Items.Clear();

            foreach (var device in devices)
            {
                comboBoxOutputDevices.Items.Add(device.FriendlyName);
                comboBoxOutputDevices2.Items.Add(device.FriendlyName);
            }

            comboBoxOutputDevices2.Items.Insert(0, "None"); // Add None as the first option

            if (devices.Count > 0)
            {
                comboBoxOutputDevices.SelectedIndex = 0;
                comboBoxOutputDevices2.SelectedIndex = 0; // default to None
                selectedDevice = devices[0];
            }

            comboBoxOutputDevices.SelectedIndexChanged += ComboBoxOutputDevices_SelectedIndexChanged;
            comboBoxOutputDevices2.SelectedIndexChanged += (s, e) =>
            {
                // Optional: Add logic to react when the second device is changed
            };
        }


        private void ComboBoxOutputDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedDeviceIndex = comboBoxOutputDevices.SelectedIndex;
            if (selectedDeviceIndex >= 0)
            {
                selectedDevice = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList()[selectedDeviceIndex];
                RestartMic();
            }
        }

        private Image CreateBlackIcon()
        {
            Bitmap blackIcon = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(blackIcon))
            {
                g.Clear(Color.Black);
            }
            return blackIcon;
        }

        private Image GetIconForFile(string filePath)
        {
            try
            {
                Icon fileIcon = Icon.ExtractAssociatedIcon(filePath);
                return fileIcon?.ToBitmap() ?? CreateBlackIcon();
            }
            catch
            {
                return CreateBlackIcon();
            }
        }

        private void LoadSoundButtons()
        {
            if (!Directory.Exists(soundDirectory))
            {
                Directory.CreateDirectory(soundDirectory);
            }

            var soundFiles = Directory.GetFiles(soundDirectory)
                .Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".mkv"))
                .ToArray();

            soundPanel.Controls.Clear();

            if (soundFiles.Length == 0)
            {
                MessageBox.Show("No audio files found in the SoundBoard folder.");
                return;
            }

            foreach (var file in soundFiles)
            {
                var btn = new Button
                {
                    Text = Path.GetFileNameWithoutExtension(file),
                    Width = 200,
                    Height = 60,
                    ImageAlign = ContentAlignment.MiddleLeft,
                    TextAlign = ContentAlignment.MiddleRight,
                    Image = GetIconForFile(file),
                    Tag = file
                };

                btn.Click += (s, e) => PlaySound((string)((Button)s).Tag);
                soundPanel.Controls.Add(btn);
            }
        }

        private void PlaySound(string filePath)
        {
            try
            {
                var volume = volumeControl.Value / 100f;

                void PlayToDevice(MMDevice device)
                {
                    var audioFile = new AudioFileReader(filePath) { Volume = volume };
                    var outputDevice = new WasapiOut(device, AudioClientShareMode.Shared, true, 200);
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    activeOutputs.Add(outputDevice);
                    activeAudioFiles.Add(audioFile);

                    outputDevice.PlaybackStopped += (s, e) =>
                    {
                        outputDevice.Dispose();
                        audioFile.Dispose();
                        activeOutputs.Remove(outputDevice);
                        activeAudioFiles.Remove(audioFile);
                    };
                }

                // First selected device
                if (comboBoxOutputDevices.SelectedIndex >= 0)
                {
                    var deviceName = comboBoxOutputDevices.SelectedItem.ToString();
                    var device = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                        .FirstOrDefault(d => d.FriendlyName == deviceName);
                    if (device != null)
                        PlayToDevice(device);
                }

                // Second selected device (if not None)
                if (comboBoxOutputDevices2.SelectedItem?.ToString() != "None")
                {
                    var deviceName2 = comboBoxOutputDevices2.SelectedItem.ToString();
                    var device2 = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                        .FirstOrDefault(d => d.FriendlyName == deviceName2);
                    if (device2 != null)
                        PlayToDevice(device2);
                }

                lastPlayedFilePath = filePath;
                isPaused = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing sound: " + ex.Message);
            }
        }

        private void InitializeControls()
        {
            playButton.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(lastPlayedFilePath))
                {
                    PlaySound(lastPlayedFilePath);
                }
                else
                {
                    MessageBox.Show("No sound has been selected yet.");
                }
            };

            stopButton.Click += (s, e) => StopSound();
            pauseButton.Click += (s, e) => PauseSound();
            volumeControl.Scroll += (s, e) => SetVolume(volumeControl.Value / 100f);

            MicVolControl.Scroll += (s, e) =>
            {
                micVolume = MicVolControl.Value / 100f;
            };

            refreshButton.Click += (s, e) => LoadSoundButtons();
        }

        private void PauseSound()
        {
            foreach (var output in activeOutputs)
            {
                if (output.PlaybackState == PlaybackState.Playing)
                {
                    output.Pause();
                }
                else if (output.PlaybackState == PlaybackState.Paused)
                {
                    output.Play();
                }
            }

            isPaused = !isPaused;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
            {
                if (file.EndsWith(".mp3") || file.EndsWith(".wav") || file.EndsWith(".mkv"))
                {
                    string destPath = Path.Combine(soundDirectory, Path.GetFileName(file));
                    File.Copy(file, destPath, true);
                }
            }

            LoadSoundButtons();
        }

        private void StopSound()
        {
            foreach (var output in activeOutputs.ToList())
            {
                output.Stop();
            }

            activeOutputs.Clear();
            activeAudioFiles.Clear();
            isPaused = false;
        }

        private void SetVolume(float volume)
        {
            foreach (var audio in activeAudioFiles)
            {
                audio.Volume = volume;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveLoadout();
            StopSound();
            micOutputDevice?.Stop();
            micOutputDevice?.Dispose();
            waveIn?.StopRecording();
            waveIn?.Dispose();
            base.OnFormClosing(e);
        }

        private void Startup_Click(object sender, EventArgs e)
        {
            string exePath = Application.ExecutablePath;
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("SoundMod", $"\"{exePath}\" -minimized");
            }

            MessageBox.Show("Added to startup with -minimized flag.");
        }

        private void Nostartup_Click(object sender, EventArgs e)
        {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (key.GetValue("SoundMod") != null)
                {
                    key.DeleteValue("SoundMod");
                    MessageBox.Show("Removed from startup.");
                }
                else
                {
                    MessageBox.Show("Not found in startup.");
                }
            }
        }
    }
}
