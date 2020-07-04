namespace semestralka_routing_simulation
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonStart = new System.Windows.Forms.Button();
            this.listBoxDevices = new System.Windows.Forms.ListBox();
            this.groupBoxDevices = new System.Windows.Forms.GroupBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.groupBoxDeviceProperties = new System.Windows.Forms.GroupBox();
            this.groupBoxDeviceFirewall = new System.Windows.Forms.GroupBox();
            this.textBoxDeviceTimeProcessFirewall = new System.Windows.Forms.TextBox();
            this.checkBoxDeviceFirewall = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxDeviceTimeProcess = new System.Windows.Forms.TextBox();
            this.comboBoxDeviceType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxDeviceConnections = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDeviceTransferTime = new System.Windows.Forms.TextBox();
            this.checkBoxDeviceConnected = new System.Windows.Forms.CheckBox();
            this.listBoxDeviceConnections = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBoxSimulationProperties = new System.Windows.Forms.GroupBox();
            this.textBoxRandomSeed = new System.Windows.Forms.TextBox();
            this.textBoxTimeout = new System.Windows.Forms.TextBox();
            this.textBoxProbabilityMalicious = new System.Windows.Forms.TextBox();
            this.textBoxNumberAttempts = new System.Windows.Forms.TextBox();
            this.textBoxTotalPackets = new System.Windows.Forms.TextBox();
            this.textBoxSendUntil = new System.Windows.Forms.TextBox();
            this.panelDistribution = new System.Windows.Forms.Panel();
            this.radioButtonDistributionGaussian = new System.Windows.Forms.RadioButton();
            this.radioButtonDistributionUniform = new System.Windows.Forms.RadioButton();
            this.groupBoxResults = new System.Windows.Forms.GroupBox();
            this.textBoxPacketsDeliveredMalicious = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBoxPacketsDelivered = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.textBoxAverageAttempts = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxAverageTimeDelivered = new System.Windows.Forms.TextBox();
            this.textBoxSimulationLength = new System.Windows.Forms.TextBox();
            this.textBoxPacketsSentMalicious = new System.Windows.Forms.TextBox();
            this.textBoxPacketsSent = new System.Windows.Forms.TextBox();
            this.groupBoxDevices.SuspendLayout();
            this.groupBoxDeviceProperties.SuspendLayout();
            this.groupBoxDeviceFirewall.SuspendLayout();
            this.groupBoxDeviceConnections.SuspendLayout();
            this.groupBoxSimulationProperties.SuspendLayout();
            this.panelDistribution.SuspendLayout();
            this.groupBoxResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(419, 413);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // listBoxDevices
            // 
            this.listBoxDevices.FormattingEnabled = true;
            this.listBoxDevices.ItemHeight = 15;
            this.listBoxDevices.Location = new System.Drawing.Point(30, 22);
            this.listBoxDevices.Name = "listBoxDevices";
            this.listBoxDevices.ScrollAlwaysVisible = true;
            this.listBoxDevices.Size = new System.Drawing.Size(124, 169);
            this.listBoxDevices.TabIndex = 2;
            this.listBoxDevices.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // groupBoxDevices
            // 
            this.groupBoxDevices.Controls.Add(this.listBoxDevices);
            this.groupBoxDevices.Location = new System.Drawing.Point(12, 12);
            this.groupBoxDevices.Name = "groupBoxDevices";
            this.groupBoxDevices.Size = new System.Drawing.Size(180, 211);
            this.groupBoxDevices.TabIndex = 3;
            this.groupBoxDevices.TabStop = false;
            this.groupBoxDevices.Text = "Devices";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(209, 21);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(81, 30);
            this.buttonAdd.TabIndex = 4;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.button2_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Location = new System.Drawing.Point(209, 57);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(81, 30);
            this.buttonRemove.TabIndex = 5;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBoxDeviceProperties
            // 
            this.groupBoxDeviceProperties.Controls.Add(this.groupBoxDeviceFirewall);
            this.groupBoxDeviceProperties.Controls.Add(this.textBoxDeviceTimeProcess);
            this.groupBoxDeviceProperties.Controls.Add(this.comboBoxDeviceType);
            this.groupBoxDeviceProperties.Controls.Add(this.label2);
            this.groupBoxDeviceProperties.Controls.Add(this.groupBoxDeviceConnections);
            this.groupBoxDeviceProperties.Location = new System.Drawing.Point(323, 12);
            this.groupBoxDeviceProperties.Name = "groupBoxDeviceProperties";
            this.groupBoxDeviceProperties.Size = new System.Drawing.Size(628, 211);
            this.groupBoxDeviceProperties.TabIndex = 7;
            this.groupBoxDeviceProperties.TabStop = false;
            this.groupBoxDeviceProperties.Text = "Device Properties";
            this.groupBoxDeviceProperties.Visible = false;
            // 
            // groupBoxDeviceFirewall
            // 
            this.groupBoxDeviceFirewall.Controls.Add(this.textBoxDeviceTimeProcessFirewall);
            this.groupBoxDeviceFirewall.Controls.Add(this.checkBoxDeviceFirewall);
            this.groupBoxDeviceFirewall.Controls.Add(this.label3);
            this.groupBoxDeviceFirewall.Location = new System.Drawing.Point(355, 87);
            this.groupBoxDeviceFirewall.Name = "groupBoxDeviceFirewall";
            this.groupBoxDeviceFirewall.Size = new System.Drawing.Size(255, 103);
            this.groupBoxDeviceFirewall.TabIndex = 10;
            this.groupBoxDeviceFirewall.TabStop = false;
            this.groupBoxDeviceFirewall.Text = "Firewall";
            // 
            // textBoxDeviceTimeProcessFirewall
            // 
            this.textBoxDeviceTimeProcessFirewall.Location = new System.Drawing.Point(9, 40);
            this.textBoxDeviceTimeProcessFirewall.Name = "textBoxDeviceTimeProcessFirewall";
            this.textBoxDeviceTimeProcessFirewall.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.textBoxDeviceTimeProcessFirewall.Size = new System.Drawing.Size(103, 23);
            this.textBoxDeviceTimeProcessFirewall.TabIndex = 9;
            this.textBoxDeviceTimeProcessFirewall.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // checkBoxDeviceFirewall
            // 
            this.checkBoxDeviceFirewall.AutoSize = true;
            this.checkBoxDeviceFirewall.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.checkBoxDeviceFirewall.Location = new System.Drawing.Point(100, 15);
            this.checkBoxDeviceFirewall.Name = "checkBoxDeviceFirewall";
            this.checkBoxDeviceFirewall.Size = new System.Drawing.Size(66, 19);
            this.checkBoxDeviceFirewall.TabIndex = 8;
            this.checkBoxDeviceFirewall.Text = "Firewall";
            this.checkBoxDeviceFirewall.UseVisualStyleBackColor = true;
            this.checkBoxDeviceFirewall.Click += new System.EventHandler(this.checkBox2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Time to process packet";
            // 
            // textBoxDeviceTimeProcess
            // 
            this.textBoxDeviceTimeProcess.Location = new System.Drawing.Point(364, 57);
            this.textBoxDeviceTimeProcess.Name = "textBoxDeviceTimeProcess";
            this.textBoxDeviceTimeProcess.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.textBoxDeviceTimeProcess.Size = new System.Drawing.Size(103, 23);
            this.textBoxDeviceTimeProcess.TabIndex = 9;
            this.textBoxDeviceTimeProcess.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // comboBoxDeviceType
            // 
            this.comboBoxDeviceType.FormattingEnabled = true;
            this.comboBoxDeviceType.Items.AddRange(new object[] {
            "Router",
            "Computer"});
            this.comboBoxDeviceType.Location = new System.Drawing.Point(364, 22);
            this.comboBoxDeviceType.Name = "comboBoxDeviceType";
            this.comboBoxDeviceType.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.comboBoxDeviceType.Size = new System.Drawing.Size(103, 23);
            this.comboBoxDeviceType.TabIndex = 8;
            this.comboBoxDeviceType.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(473, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "Time to process packet";
            // 
            // groupBoxDeviceConnections
            // 
            this.groupBoxDeviceConnections.Controls.Add(this.label1);
            this.groupBoxDeviceConnections.Controls.Add(this.textBoxDeviceTransferTime);
            this.groupBoxDeviceConnections.Controls.Add(this.checkBoxDeviceConnected);
            this.groupBoxDeviceConnections.Controls.Add(this.listBoxDeviceConnections);
            this.groupBoxDeviceConnections.Location = new System.Drawing.Point(22, 22);
            this.groupBoxDeviceConnections.Name = "groupBoxDeviceConnections";
            this.groupBoxDeviceConnections.Size = new System.Drawing.Size(322, 169);
            this.groupBoxDeviceConnections.TabIndex = 8;
            this.groupBoxDeviceConnections.TabStop = false;
            this.groupBoxDeviceConnections.Text = "Connections";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(232, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 30);
            this.label1.TabIndex = 8;
            this.label1.Text = "Time to \r\ndeliver packet";
            // 
            // textBoxDeviceTransferTime
            // 
            this.textBoxDeviceTransferTime.Location = new System.Drawing.Point(160, 58);
            this.textBoxDeviceTransferTime.Name = "textBoxDeviceTransferTime";
            this.textBoxDeviceTransferTime.Size = new System.Drawing.Size(66, 23);
            this.textBoxDeviceTransferTime.TabIndex = 8;
            this.textBoxDeviceTransferTime.Text = "5";
            this.textBoxDeviceTransferTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxDeviceTransferTime.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // checkBoxDeviceConnected
            // 
            this.checkBoxDeviceConnected.AutoSize = true;
            this.checkBoxDeviceConnected.Location = new System.Drawing.Point(214, 30);
            this.checkBoxDeviceConnected.Name = "checkBoxDeviceConnected";
            this.checkBoxDeviceConnected.Size = new System.Drawing.Size(84, 19);
            this.checkBoxDeviceConnected.TabIndex = 9;
            this.checkBoxDeviceConnected.Text = "Connected";
            this.checkBoxDeviceConnected.UseVisualStyleBackColor = true;
            this.checkBoxDeviceConnected.Click += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // listBoxDeviceConnections
            // 
            this.listBoxDeviceConnections.FormattingEnabled = true;
            this.listBoxDeviceConnections.ItemHeight = 15;
            this.listBoxDeviceConnections.Location = new System.Drawing.Point(15, 22);
            this.listBoxDeviceConnections.Name = "listBoxDeviceConnections";
            this.listBoxDeviceConnections.ScrollAlwaysVisible = true;
            this.listBoxDeviceConnections.Size = new System.Drawing.Size(120, 124);
            this.listBoxDeviceConnections.TabIndex = 8;
            this.listBoxDeviceConnections.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Total number of packets";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(53, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 15);
            this.label5.TabIndex = 9;
            this.label5.Text = "Send packets until";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(454, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(203, 15);
            this.label6.TabIndex = 10;
            this.label6.Text = "Probability of packet being malicious";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(570, 90);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 15);
            this.label7.TabIndex = 11;
            this.label7.Text = "Packet timeout";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(462, 60);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(195, 15);
            this.label8.TabIndex = 12;
            this.label8.Text = "Number of attempts to send packet";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(578, 118);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 15);
            this.label9.TabIndex = 13;
            this.label9.Text = "Random seed";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(50, 90);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(106, 15);
            this.label10.TabIndex = 14;
            this.label10.Text = "Packet distribution";
            // 
            // groupBoxSimulationProperties
            // 
            this.groupBoxSimulationProperties.Controls.Add(this.textBoxRandomSeed);
            this.groupBoxSimulationProperties.Controls.Add(this.textBoxTimeout);
            this.groupBoxSimulationProperties.Controls.Add(this.textBoxProbabilityMalicious);
            this.groupBoxSimulationProperties.Controls.Add(this.label4);
            this.groupBoxSimulationProperties.Controls.Add(this.textBoxNumberAttempts);
            this.groupBoxSimulationProperties.Controls.Add(this.label9);
            this.groupBoxSimulationProperties.Controls.Add(this.label10);
            this.groupBoxSimulationProperties.Controls.Add(this.textBoxTotalPackets);
            this.groupBoxSimulationProperties.Controls.Add(this.textBoxSendUntil);
            this.groupBoxSimulationProperties.Controls.Add(this.label7);
            this.groupBoxSimulationProperties.Controls.Add(this.label8);
            this.groupBoxSimulationProperties.Controls.Add(this.label5);
            this.groupBoxSimulationProperties.Controls.Add(this.label6);
            this.groupBoxSimulationProperties.Controls.Add(this.panelDistribution);
            this.groupBoxSimulationProperties.Location = new System.Drawing.Point(11, 233);
            this.groupBoxSimulationProperties.Name = "groupBoxSimulationProperties";
            this.groupBoxSimulationProperties.Size = new System.Drawing.Size(940, 153);
            this.groupBoxSimulationProperties.TabIndex = 15;
            this.groupBoxSimulationProperties.TabStop = false;
            this.groupBoxSimulationProperties.Text = "Simulation parameters";
            // 
            // textBoxRandomSeed
            // 
            this.textBoxRandomSeed.Location = new System.Drawing.Point(663, 116);
            this.textBoxRandomSeed.Name = "textBoxRandomSeed";
            this.textBoxRandomSeed.Size = new System.Drawing.Size(154, 23);
            this.textBoxRandomSeed.TabIndex = 19;
            // 
            // textBoxTimeout
            // 
            this.textBoxTimeout.Location = new System.Drawing.Point(663, 87);
            this.textBoxTimeout.Name = "textBoxTimeout";
            this.textBoxTimeout.Size = new System.Drawing.Size(154, 23);
            this.textBoxTimeout.TabIndex = 20;
            // 
            // textBoxProbabilityMalicious
            // 
            this.textBoxProbabilityMalicious.Location = new System.Drawing.Point(663, 29);
            this.textBoxProbabilityMalicious.Name = "textBoxProbabilityMalicious";
            this.textBoxProbabilityMalicious.Size = new System.Drawing.Size(154, 23);
            this.textBoxProbabilityMalicious.TabIndex = 21;
            // 
            // textBoxNumberAttempts
            // 
            this.textBoxNumberAttempts.Location = new System.Drawing.Point(663, 58);
            this.textBoxNumberAttempts.Name = "textBoxNumberAttempts";
            this.textBoxNumberAttempts.Size = new System.Drawing.Size(154, 23);
            this.textBoxNumberAttempts.TabIndex = 17;
            // 
            // textBoxTotalPackets
            // 
            this.textBoxTotalPackets.Location = new System.Drawing.Point(162, 29);
            this.textBoxTotalPackets.Name = "textBoxTotalPackets";
            this.textBoxTotalPackets.Size = new System.Drawing.Size(154, 23);
            this.textBoxTotalPackets.TabIndex = 18;
            // 
            // textBoxSendUntil
            // 
            this.textBoxSendUntil.Location = new System.Drawing.Point(162, 57);
            this.textBoxSendUntil.Name = "textBoxSendUntil";
            this.textBoxSendUntil.Size = new System.Drawing.Size(154, 23);
            this.textBoxSendUntil.TabIndex = 16;
            // 
            // panelDistribution
            // 
            this.panelDistribution.Controls.Add(this.radioButtonDistributionGaussian);
            this.panelDistribution.Controls.Add(this.radioButtonDistributionUniform);
            this.panelDistribution.Location = new System.Drawing.Point(162, 86);
            this.panelDistribution.Name = "panelDistribution";
            this.panelDistribution.Size = new System.Drawing.Size(154, 53);
            this.panelDistribution.TabIndex = 16;
            // 
            // radioButtonDistributionGaussian
            // 
            this.radioButtonDistributionGaussian.AutoSize = true;
            this.radioButtonDistributionGaussian.Location = new System.Drawing.Point(9, 28);
            this.radioButtonDistributionGaussian.Name = "radioButtonDistributionGaussian";
            this.radioButtonDistributionGaussian.Size = new System.Drawing.Size(117, 19);
            this.radioButtonDistributionGaussian.TabIndex = 17;
            this.radioButtonDistributionGaussian.TabStop = true;
            this.radioButtonDistributionGaussian.Text = "Discrete Gaussian";
            this.radioButtonDistributionGaussian.UseVisualStyleBackColor = true;
            // 
            // radioButtonDistributionUniform
            // 
            this.radioButtonDistributionUniform.AutoSize = true;
            this.radioButtonDistributionUniform.Location = new System.Drawing.Point(9, 5);
            this.radioButtonDistributionUniform.Name = "radioButtonDistributionUniform";
            this.radioButtonDistributionUniform.Size = new System.Drawing.Size(69, 19);
            this.radioButtonDistributionUniform.TabIndex = 16;
            this.radioButtonDistributionUniform.TabStop = true;
            this.radioButtonDistributionUniform.Text = "Uniform";
            this.radioButtonDistributionUniform.UseVisualStyleBackColor = true;
            // 
            // groupBoxResults
            // 
            this.groupBoxResults.Controls.Add(this.textBoxPacketsDeliveredMalicious);
            this.groupBoxResults.Controls.Add(this.label16);
            this.groupBoxResults.Controls.Add(this.textBoxPacketsDelivered);
            this.groupBoxResults.Controls.Add(this.label17);
            this.groupBoxResults.Controls.Add(this.label14);
            this.groupBoxResults.Controls.Add(this.label15);
            this.groupBoxResults.Controls.Add(this.label11);
            this.groupBoxResults.Controls.Add(this.textBoxAverageAttempts);
            this.groupBoxResults.Controls.Add(this.label12);
            this.groupBoxResults.Controls.Add(this.label13);
            this.groupBoxResults.Controls.Add(this.textBoxAverageTimeDelivered);
            this.groupBoxResults.Controls.Add(this.textBoxSimulationLength);
            this.groupBoxResults.Controls.Add(this.textBoxPacketsSentMalicious);
            this.groupBoxResults.Controls.Add(this.textBoxPacketsSent);
            this.groupBoxResults.Location = new System.Drawing.Point(11, 453);
            this.groupBoxResults.Name = "groupBoxResults";
            this.groupBoxResults.Size = new System.Drawing.Size(940, 182);
            this.groupBoxResults.TabIndex = 16;
            this.groupBoxResults.TabStop = false;
            this.groupBoxResults.Text = "Results";
            // 
            // textBoxPacketsDeliveredMalicious
            // 
            this.textBoxPacketsDeliveredMalicious.Location = new System.Drawing.Point(622, 82);
            this.textBoxPacketsDeliveredMalicious.Name = "textBoxPacketsDeliveredMalicious";
            this.textBoxPacketsDeliveredMalicious.ReadOnly = true;
            this.textBoxPacketsDeliveredMalicious.Size = new System.Drawing.Size(154, 23);
            this.textBoxPacketsDeliveredMalicious.TabIndex = 23;
            this.textBoxPacketsDeliveredMalicious.Text = "0";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(604, 85);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(12, 15);
            this.label16.TabIndex = 24;
            this.label16.Text = "/";
            // 
            // textBoxPacketsDelivered
            // 
            this.textBoxPacketsDelivered.Location = new System.Drawing.Point(622, 54);
            this.textBoxPacketsDelivered.Name = "textBoxPacketsDelivered";
            this.textBoxPacketsDelivered.ReadOnly = true;
            this.textBoxPacketsDelivered.Size = new System.Drawing.Size(154, 23);
            this.textBoxPacketsDelivered.TabIndex = 21;
            this.textBoxPacketsDelivered.Text = "0";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(604, 57);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(12, 15);
            this.label17.TabIndex = 25;
            this.label17.Text = "/";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(271, 114);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(167, 15);
            this.label14.TabIndex = 20;
            this.label14.Text = "Average time to deliver packet";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(189, 145);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(249, 15);
            this.label15.TabIndex = 21;
            this.label15.Text = "Average number of attempts to deliver packet";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(321, 28);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(117, 15);
            this.label11.TabIndex = 17;
            this.label11.Text = "Length of simulation";
            // 
            // textBoxAverageAttempts
            // 
            this.textBoxAverageAttempts.Location = new System.Drawing.Point(444, 142);
            this.textBoxAverageAttempts.Name = "textBoxAverageAttempts";
            this.textBoxAverageAttempts.ReadOnly = true;
            this.textBoxAverageAttempts.Size = new System.Drawing.Size(154, 23);
            this.textBoxAverageAttempts.TabIndex = 22;
            this.textBoxAverageAttempts.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(306, 57);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(132, 15);
            this.label12.TabIndex = 18;
            this.label12.Text = "Sent / delivered packets";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(252, 85);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(186, 15);
            this.label13.TabIndex = 19;
            this.label13.Text = "Sent / delivered malicious packets";
            // 
            // textBoxAverageTimeDelivered
            // 
            this.textBoxAverageTimeDelivered.Location = new System.Drawing.Point(444, 111);
            this.textBoxAverageTimeDelivered.Name = "textBoxAverageTimeDelivered";
            this.textBoxAverageTimeDelivered.ReadOnly = true;
            this.textBoxAverageTimeDelivered.Size = new System.Drawing.Size(154, 23);
            this.textBoxAverageTimeDelivered.TabIndex = 20;
            this.textBoxAverageTimeDelivered.Text = "0";
            // 
            // textBoxSimulationLength
            // 
            this.textBoxSimulationLength.Location = new System.Drawing.Point(444, 25);
            this.textBoxSimulationLength.Name = "textBoxSimulationLength";
            this.textBoxSimulationLength.ReadOnly = true;
            this.textBoxSimulationLength.Size = new System.Drawing.Size(154, 23);
            this.textBoxSimulationLength.TabIndex = 17;
            this.textBoxSimulationLength.Text = "0";
            // 
            // textBoxPacketsSentMalicious
            // 
            this.textBoxPacketsSentMalicious.Location = new System.Drawing.Point(444, 82);
            this.textBoxPacketsSentMalicious.Name = "textBoxPacketsSentMalicious";
            this.textBoxPacketsSentMalicious.ReadOnly = true;
            this.textBoxPacketsSentMalicious.Size = new System.Drawing.Size(154, 23);
            this.textBoxPacketsSentMalicious.TabIndex = 18;
            this.textBoxPacketsSentMalicious.Text = "0";
            // 
            // textBoxPacketsSent
            // 
            this.textBoxPacketsSent.Location = new System.Drawing.Point(444, 54);
            this.textBoxPacketsSent.Name = "textBoxPacketsSent";
            this.textBoxPacketsSent.ReadOnly = true;
            this.textBoxPacketsSent.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBoxPacketsSent.Size = new System.Drawing.Size(154, 23);
            this.textBoxPacketsSent.TabIndex = 19;
            this.textBoxPacketsSent.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(962, 644);
            this.Controls.Add(this.groupBoxResults);
            this.Controls.Add(this.groupBoxSimulationProperties);
            this.Controls.Add(this.groupBoxDeviceProperties);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.groupBoxDevices);
            this.Controls.Add(this.buttonStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBoxDevices.ResumeLayout(false);
            this.groupBoxDeviceProperties.ResumeLayout(false);
            this.groupBoxDeviceProperties.PerformLayout();
            this.groupBoxDeviceFirewall.ResumeLayout(false);
            this.groupBoxDeviceFirewall.PerformLayout();
            this.groupBoxDeviceConnections.ResumeLayout(false);
            this.groupBoxDeviceConnections.PerformLayout();
            this.groupBoxSimulationProperties.ResumeLayout(false);
            this.groupBoxSimulationProperties.PerformLayout();
            this.panelDistribution.ResumeLayout(false);
            this.panelDistribution.PerformLayout();
            this.groupBoxResults.ResumeLayout(false);
            this.groupBoxResults.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ListBox listBoxDevices;
        private System.Windows.Forms.GroupBox groupBoxDevices;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.GroupBox groupBoxDeviceProperties;
        private System.Windows.Forms.GroupBox groupBoxDeviceConnections;
        private System.Windows.Forms.ListBox listBoxDeviceConnections;
        private System.Windows.Forms.CheckBox checkBoxDeviceConnected;
        private System.Windows.Forms.TextBox textBoxDeviceTransferTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxDeviceType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDeviceTimeProcess;
        private System.Windows.Forms.CheckBox checkBoxDeviceFirewall;
        private System.Windows.Forms.GroupBox groupBoxDeviceFirewall;
        private System.Windows.Forms.TextBox textBoxDeviceTimeProcessFirewall;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBoxSimulationProperties;
        private System.Windows.Forms.TextBox textBoxRandomSeed;
        private System.Windows.Forms.TextBox textBoxTimeout;
        private System.Windows.Forms.TextBox textBoxProbabilityMalicious;
        private System.Windows.Forms.TextBox textBoxNumberAttempts;
        private System.Windows.Forms.TextBox textBoxTotalPackets;
        private System.Windows.Forms.TextBox textBoxSendUntil;
        private System.Windows.Forms.Panel panelDistribution;
        private System.Windows.Forms.RadioButton radioButtonDistributionGaussian;
        private System.Windows.Forms.RadioButton radioButtonDistributionUniform;
        private System.Windows.Forms.GroupBox groupBoxResults;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxSimulationLength;
        private System.Windows.Forms.TextBox textBoxPacketsSentMalicious;
        private System.Windows.Forms.TextBox textBoxPacketsSent;
        private System.Windows.Forms.TextBox textBoxAverageTimeDelivered;
        private System.Windows.Forms.TextBox textBoxPacketsDelivered;
        private System.Windows.Forms.TextBox textBoxAverageAttempts;
        private System.Windows.Forms.TextBox textBoxPacketsDeliveredMalicious;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
    }
}

