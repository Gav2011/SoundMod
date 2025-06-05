namespace SoundMod
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ComboBox comboBoxOutputDevices;
        private System.Windows.Forms.ComboBox comboBoxMicrophones;
        private System.Windows.Forms.FlowLayoutPanel soundPanel;
        private System.Windows.Forms.GroupBox groupBoxControls;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button pauseButton;
        private System.Windows.Forms.TrackBar volumeControl;
        private System.Windows.Forms.Button refreshButton;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.comboBoxOutputDevices = new System.Windows.Forms.ComboBox();
            this.comboBoxMicrophones = new System.Windows.Forms.ComboBox();
            this.soundPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBoxControls = new System.Windows.Forms.GroupBox();
            this.SoundFolder = new System.Windows.Forms.Button();
            this.ah = new System.Windows.Forms.Label();
            this.SpamModeLabel = new System.Windows.Forms.Label();
            this.BtnSpamMode = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.VersionTag = new System.Windows.Forms.Label();
            this.comboBoxOutputDevices2 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Nostartup = new System.Windows.Forms.Button();
            this.Startup = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.MicVolControl = new System.Windows.Forms.TrackBar();
            this.playButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.pauseButton = new System.Windows.Forms.Button();
            this.volumeControl = new System.Windows.Forms.TrackBar();
            this.groupBoxControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MicVolControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.volumeControl)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxOutputDevices
            // 
            this.comboBoxOutputDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOutputDevices.Location = new System.Drawing.Point(10, 42);
            this.comboBoxOutputDevices.Name = "comboBoxOutputDevices";
            this.comboBoxOutputDevices.Size = new System.Drawing.Size(200, 21);
            this.comboBoxOutputDevices.TabIndex = 0;
            // 
            // comboBoxMicrophones
            // 
            this.comboBoxMicrophones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMicrophones.Location = new System.Drawing.Point(220, 42);
            this.comboBoxMicrophones.Name = "comboBoxMicrophones";
            this.comboBoxMicrophones.Size = new System.Drawing.Size(200, 21);
            this.comboBoxMicrophones.TabIndex = 1;
            // 
            // soundPanel
            // 
            this.soundPanel.AutoScroll = true;
            this.soundPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.soundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.soundPanel.Location = new System.Drawing.Point(0, 0);
            this.soundPanel.Name = "soundPanel";
            this.soundPanel.Size = new System.Drawing.Size(800, 400);
            this.soundPanel.TabIndex = 0;
            // 
            // groupBoxControls
            // 
            this.groupBoxControls.Controls.Add(this.SoundFolder);
            this.groupBoxControls.Controls.Add(this.ah);
            this.groupBoxControls.Controls.Add(this.SpamModeLabel);
            this.groupBoxControls.Controls.Add(this.BtnSpamMode);
            this.groupBoxControls.Controls.Add(this.label7);
            this.groupBoxControls.Controls.Add(this.VersionTag);
            this.groupBoxControls.Controls.Add(this.comboBoxOutputDevices2);
            this.groupBoxControls.Controls.Add(this.label1);
            this.groupBoxControls.Controls.Add(this.Nostartup);
            this.groupBoxControls.Controls.Add(this.Startup);
            this.groupBoxControls.Controls.Add(this.refreshButton);
            this.groupBoxControls.Controls.Add(this.MicVolControl);
            this.groupBoxControls.Controls.Add(this.comboBoxOutputDevices);
            this.groupBoxControls.Controls.Add(this.comboBoxMicrophones);
            this.groupBoxControls.Controls.Add(this.playButton);
            this.groupBoxControls.Controls.Add(this.stopButton);
            this.groupBoxControls.Controls.Add(this.pauseButton);
            this.groupBoxControls.Controls.Add(this.volumeControl);
            this.groupBoxControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxControls.Location = new System.Drawing.Point(0, 400);
            this.groupBoxControls.Name = "groupBoxControls";
            this.groupBoxControls.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxControls.Size = new System.Drawing.Size(800, 200);
            this.groupBoxControls.TabIndex = 1;
            this.groupBoxControls.TabStop = false;
            this.groupBoxControls.Text = "Audio Controls";
            // 
            // SoundFolder
            // 
            this.SoundFolder.Location = new System.Drawing.Point(476, 19);
            this.SoundFolder.Name = "SoundFolder";
            this.SoundFolder.Size = new System.Drawing.Size(100, 21);
            this.SoundFolder.TabIndex = 25;
            this.SoundFolder.Text = "SoundFolder";
            this.SoundFolder.Click += new System.EventHandler(this.SoundFolder_Click);
            // 
            // ah
            // 
            this.ah.AutoSize = true;
            this.ah.Location = new System.Drawing.Point(34, 26);
            this.ah.Name = "ah";
            this.ah.Size = new System.Drawing.Size(153, 13);
            this.ah.TabIndex = 24;
            this.ah.Text = "Second audio ouput is delayed";
            // 
            // SpamModeLabel
            // 
            this.SpamModeLabel.AutoSize = true;
            this.SpamModeLabel.Location = new System.Drawing.Point(445, 131);
            this.SpamModeLabel.Name = "SpamModeLabel";
            this.SpamModeLabel.Size = new System.Drawing.Size(88, 13);
            this.SpamModeLabel.TabIndex = 23;
            this.SpamModeLabel.Text = "Spam Mode: ???";
            // 
            // BtnSpamMode
            // 
            this.BtnSpamMode.Location = new System.Drawing.Point(432, 147);
            this.BtnSpamMode.Name = "BtnSpamMode";
            this.BtnSpamMode.Size = new System.Drawing.Size(114, 40);
            this.BtnSpamMode.TabIndex = 22;
            this.BtnSpamMode.Text = "Spam Mode";
            this.BtnSpamMode.Click += new System.EventHandler(this.BtnSpamMode_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(722, 178);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "By Gav2011";
            // 
            // VersionTag
            // 
            this.VersionTag.AutoSize = true;
            this.VersionTag.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VersionTag.Location = new System.Drawing.Point(622, 173);
            this.VersionTag.Name = "VersionTag";
            this.VersionTag.Size = new System.Drawing.Size(94, 20);
            this.VersionTag.TabIndex = 20;
            this.VersionTag.Text = "Version ???";
            // 
            // comboBoxOutputDevices2
            // 
            this.comboBoxOutputDevices2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOutputDevices2.Location = new System.Drawing.Point(10, 69);
            this.comboBoxOutputDevices2.Name = "comboBoxOutputDevices2";
            this.comboBoxOutputDevices2.Size = new System.Drawing.Size(200, 21);
            this.comboBoxOutputDevices2.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(688, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Not Recommended";
            // 
            // Nostartup
            // 
            this.Nostartup.Location = new System.Drawing.Point(688, 18);
            this.Nostartup.Name = "Nostartup";
            this.Nostartup.Size = new System.Drawing.Size(100, 21);
            this.Nostartup.TabIndex = 10;
            this.Nostartup.Text = "Remove Startup";
            this.Nostartup.Click += new System.EventHandler(this.Nostartup_Click);
            // 
            // Startup
            // 
            this.Startup.Location = new System.Drawing.Point(582, 19);
            this.Startup.Name = "Startup";
            this.Startup.Size = new System.Drawing.Size(100, 21);
            this.Startup.TabIndex = 9;
            this.Startup.Text = "Startup - mini";
            this.Startup.Click += new System.EventHandler(this.Startup_Click);
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(326, 148);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(100, 40);
            this.refreshButton.TabIndex = 6;
            this.refreshButton.Text = "Refresh";
            // 
            // MicVolControl
            // 
            this.MicVolControl.Location = new System.Drawing.Point(216, 69);
            this.MicVolControl.Maximum = 100;
            this.MicVolControl.Name = "MicVolControl";
            this.MicVolControl.Size = new System.Drawing.Size(200, 45);
            this.MicVolControl.TabIndex = 5;
            this.MicVolControl.Value = 50;
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(10, 147);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(100, 40);
            this.playButton.TabIndex = 1;
            this.playButton.Text = "Play";
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(116, 147);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(100, 40);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "Stop";
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // pauseButton
            // 
            this.pauseButton.Location = new System.Drawing.Point(220, 147);
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(100, 40);
            this.pauseButton.TabIndex = 3;
            this.pauseButton.Text = "Pause";
            this.pauseButton.Click += new System.EventHandler(this.pauseButton_Click);
            // 
            // volumeControl
            // 
            this.volumeControl.Location = new System.Drawing.Point(10, 96);
            this.volumeControl.Maximum = 100;
            this.volumeControl.Name = "volumeControl";
            this.volumeControl.Size = new System.Drawing.Size(200, 45);
            this.volumeControl.TabIndex = 4;
            this.volumeControl.Value = 50;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.soundPanel);
            this.Controls.Add(this.groupBoxControls);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "SoundMod";
            this.groupBoxControls.ResumeLayout(false);
            this.groupBoxControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MicVolControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.volumeControl)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TrackBar MicVolControl;
        private System.Windows.Forms.Button Startup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Nostartup;
        private System.Windows.Forms.ComboBox comboBoxOutputDevices2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label VersionTag;
        private System.Windows.Forms.Label SpamModeLabel;
        private System.Windows.Forms.Button BtnSpamMode;
        private System.Windows.Forms.Label ah;
        private System.Windows.Forms.Button SoundFolder;
    }
}
