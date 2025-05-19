using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private float micVolume = 1.0f;
        string[] supportedExtensions = new[] { ".mp3", ".wav", ".mkv", ".ogg", ".mp4", ".flac", ".aac", ".wma" };
        public static DateTime AppStartTime = DateTime.UtcNow;
        private bool isSpamModeOn = false;

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
            StartDiscord();
            VersionInformation();
            isSpamModeOn = false;
            SpamModeLabel.Text = "Spam Mode: Off";
        }

        private async void VersionInformation()
        {
            const string CurrentVersion = "1.0";
            const string VersionFileUrl = "https://raw.githubusercontent.com/Gav2011/Versions/refs/heads/main/SoundMod";
            const string LatestReleaseUrl = "https://github.com/Gav2011/SoundMod/releases/latest/download/SoundMod.exe";

            VersionTag.Text = $"Version {CurrentVersion}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string latestVersion = (await client.GetStringAsync(VersionFileUrl)).Trim();

                    if (latestVersion != CurrentVersion)
                    {
                        DialogResult result = MessageBox.Show(
                            $"A new version ({latestVersion}) is available.\nDo you want to update now?",
                            "Update Available",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            string exePath = Application.ExecutablePath;
                            string exeDir = Path.GetDirectoryName(exePath);
                            string updatePath = Path.Combine(exeDir, "SoundMod.exe");
                            string vbsPath = Path.Combine(exeDir, "update.vbs");
                            string vbs = $@"
Set WshShell = CreateObject(""WScript.Shell"")
WshShell.Run ""cmd /c taskkill /f /im SoundMod.exe"", 0, True
WScript.Sleep 5000
Set fso = CreateObject(""Scripting.FileSystemObject"")
If fso.FileExists(""{exePath}"") Then
    fso.DeleteFile ""{exePath}"", True
End If
Set objXML = CreateObject(""WinHTTP.WinHTTPRequest.5.1"")
objXML.Open ""GET"", ""{LatestReleaseUrl}"", False
objXML.Send
Set objStream = CreateObject(""ADODB.Stream"")
objStream.Open
objStream.Type = 1
objStream.Write objXML.ResponseBody
objStream.SaveToFile ""{updatePath}"", 2
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
                            Application.Exit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for updates:\n{ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            comboBoxOutputDevices2.Items.Insert(0, "None");

            if (devices.Count > 0)
            {
                comboBoxOutputDevices.SelectedIndex = 0;
                comboBoxOutputDevices2.SelectedIndex = 0;
                selectedDevice = devices[0];
            }

            comboBoxOutputDevices.SelectedIndexChanged += ComboBoxOutputDevices_SelectedIndexChanged;
            comboBoxOutputDevices2.SelectedIndexChanged += (s, e) =>
            {
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
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
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
                if (!isSpamModeOn)
                {
                    StopSound();
                }

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

                        if (activeOutputs.Count == 0 && !isPaused)
                        {
                            StartDiscord();
                        }
                    };
                }

                if (comboBoxOutputDevices.SelectedIndex >= 0)
                {
                    var deviceName = comboBoxOutputDevices.SelectedItem.ToString();
                    var device = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                        .FirstOrDefault(d => d.FriendlyName == deviceName);
                    if (device != null)
                        PlayToDevice(device);
                }

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

                string songName = Path.GetFileNameWithoutExtension(filePath);
                using (var tempReader = new AudioFileReader(filePath))
                {
                    UpdatePresence(songName);
                }
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
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (supportedExtensions.Contains(ext))
                {
                    try
                    {
                        string destPath = Path.Combine(soundDirectory, Path.GetFileName(file));
                        File.Copy(file, destPath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {file}\n{ex.Message}", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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

        private void StartDiscord()
        {
            ExtractDiscordRpcDll();
            InitializeDiscord();
            UpdatePresence2();
        }

        private void UpdateDiscord(string songName, TimeSpan totalDuration)
        {
            InitializeDiscord();
            UpdatePresence(songName);
        }   

        private void playButton_Click(object sender, EventArgs e)
        {
            SaveLoadout();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DiscordRichPresence
        {
            public string state;
            public string details;
            public long startTimestamp;
            public long endTimestamp;
            public string largeImageKey;
            public string largeImageText;
            public string smallImageKey;
            public string smallImageText;
            public string partyId;
            public int partySize;
            public int partyMax;
            public string matchSecret;
            public string joinSecret;
            public string spectateSecret;
            public bool instance;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DiscordEventHandlers
        {
            public IntPtr ready;
            public IntPtr disconnected;
            public IntPtr errored;
        }

        [DllImport("discord-rpc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Discord_Initialize(
            [MarshalAs(UnmanagedType.LPStr)] string applicationId,
            ref DiscordEventHandlers handlers,
            bool autoRegister,
            [MarshalAs(UnmanagedType.LPStr)] string optionalSteamId);

        [DllImport("discord-rpc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Discord_UpdatePresence(ref DiscordRichPresence presence);

        [DllImport("discord-rpc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Discord_Shutdown();

        public static void InitializeDiscord()
        {
            try
            {
                DiscordEventHandlers handlers = new DiscordEventHandlers();
                Discord_Initialize("1373882277976866946", ref handlers, true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing Discord: " + ex.Message);
            }
        }

        public static void UpdatePresence2()
        {
            try
            {
                long startTimestamp = ((DateTimeOffset)AppStartTime).ToUnixTimeSeconds();
                DiscordRichPresence presence = new DiscordRichPresence
                {
                    largeImageKey = "soundmodlogo",
                    startTimestamp = startTimestamp,
                    endTimestamp = 0,
                };
                Discord_UpdatePresence(ref presence);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating Discord presence: " + ex.Message);
            }
        }

        public static void UpdatePresence(string songName)
        {
            try
            {
                long startTimestamp = ((DateTimeOffset)AppStartTime).ToUnixTimeSeconds();
                DiscordRichPresence presence = new DiscordRichPresence
                {
                    details = $"Playing {songName}",
                    largeImageKey = "soundmodlogo",
                    startTimestamp = startTimestamp,
                    endTimestamp = 0,
                };
                Discord_UpdatePresence(ref presence);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating Discord presence: " + ex.Message);
            }
        }

        public static void ShutdownDiscord()
        {
            try
            {
                Console.WriteLine("Shutting down Discord...");
                Discord_Shutdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error shutting down Discord: " + ex.Message);
            }
        }

        public static void ExtractDiscordRpcDll()
        {
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "discord-rpc.dll");
            if (!File.Exists(outputPath))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SoundMod.discord-rpc.dll"))
                {
                    if (stream == null)
                    {
                        throw new Exception("Embedded discord-rpc.dll resource not found.");
                    }
                    using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                        Console.WriteLine("Made discord rpc dll file");
                    }
                }
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            StartDiscord();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            StartDiscord();
        }
        private void BtnSpamMode_Click(object sender, EventArgs e)
        {
            isSpamModeOn = !isSpamModeOn;
            SpamModeLabel.Text = $"Spam Mode: {(isSpamModeOn ? "On" : "Off")}";
        }
    }
}