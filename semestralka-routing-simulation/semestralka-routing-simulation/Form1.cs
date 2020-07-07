﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace semestralka_routing_simulation
{
    public enum DeviceType
    {
        Router,
        Computer
    }

    public partial class Form1 : Form
    {
        int nextID;

        public Form1()
        {
            InitializeComponent();
            nextID = 1;
            SetInputLimits();
            InitializeDevices();
            InitializeSimulationParameters();
        }

        private void StartSimulation()
        {
            SimulationParametersDto simulationParameters = new SimulationParametersDto()
            {
                Devices = listBoxDevices.Items,
                TotalPackets = (ulong)numericUpDownTotalPackets.Value,
                SendUntil = (ulong)numericUpDownSendUntil.Value,
                DistributionUniform = radioButtonDistributionUniform.Checked,
                ProbabilityMalicious = (double)numericUpDownProbabilityMalicious.Value,
                NumberAttempts = (int)numericUpDownNumberAttempts.Value,
                Timeout = (ulong)numericUpDownTimeout.Value,
                RandomSeed = (int)numericUpDownRandomSeed.Value
            };

            ResultControls controls = new ResultControls()
            {
                SimulationLength = textBoxSimulationLength,
                PacketsSent = textBoxPacketsSent,
                PacketsDelivered = textBoxPacketsDelivered,
                PacketsSentMalicious = textBoxPacketsSentMalicious,
                PacketsDeliveredMalicious = textBoxPacketsDeliveredMalicious,
                AverageTimeDelivered = textBoxAverageTimeDelivered,
                AverageAttempts = textBoxAverageAttempts
            };

            string folderPath = textBoxFolderPath.Text;

            Simulation simulation = new Simulation();
            simulation.Run(simulationParameters, controls, folderPath, panelInput);
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            panelInput.Enabled = false;
            Thread newThread = new Thread(new ThreadStart(StartSimulation));
            newThread.Start();
        }

        private void ListBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDevices.SelectedItem is null)
            {
                groupBoxDeviceProperties.Visible = false;
                return;
            }

            groupBoxDeviceProperties.Visible = true;
            buttonRemove.Enabled = true;
            listBoxDeviceConnections.Items.Clear();
            FormDevice selectedDevice = (FormDevice)((ListBox)sender).SelectedItem;

            // Update connected devices
            foreach (FormDevice device in listBoxDevices.Items)
            {
                if (device != selectedDevice)
                {
                    listBoxDeviceConnections.Items.Add(device);
                }
            }

            // Update device properties
            switch (selectedDevice.deviceType)
            {
                case DeviceType.Computer:
                    comboBoxDeviceType.SelectedIndex = 1;
                    break;
                case DeviceType.Router:
                    comboBoxDeviceType.SelectedIndex = 0;
                    break;
            }
            numericUpDownDeviceTimeProcess.Text = selectedDevice.timeToProcess.ToString();
            if (selectedDevice.firewall)
            {
                checkBoxDeviceFirewall.Enabled = true;
                checkBoxDeviceFirewall.Checked = true;
                numericUpDownDeviceTimeProcessFirewall.Text = selectedDevice.firewallTimeToProcess.ToString();
                numericUpDownDeviceTimeProcessFirewall.Enabled = true;
            }
            else
            {
                checkBoxDeviceFirewall.Checked = false;
                numericUpDownDeviceTimeProcessFirewall.Text = "";
                numericUpDownDeviceTimeProcessFirewall.Enabled = false;
            }

            if (selectedDevice.malicious)
            {
                checkBoxDeviceMalicious.Checked = true;
            }
            else
            {
                checkBoxDeviceMalicious.Checked = false;
            }

            if (selectedDevice.deviceType == DeviceType.Computer)
            {
                checkBoxDeviceMalicious.Enabled = true;
                checkBoxDeviceFirewall.Enabled = false;
                numericUpDownDeviceTimeProcess.Enabled = false;
                numericUpDownDeviceTimeProcess.Text = "";
            }
            else
            {
                checkBoxDeviceMalicious.Enabled = false;
                checkBoxDeviceFirewall.Enabled = true;
                numericUpDownDeviceTimeProcess.Enabled = true;
                numericUpDownDeviceTimeProcess.Text = selectedDevice.timeToProcess.ToString();
            }

            ListBoxDeviceConnections_SelectedIndexChanged(listBoxDeviceConnections, e);
        }

        private void InitializeDevices()
        {
            listBoxDevices.Items.Clear();

            listBoxDevices.Items.Add(new FormDevice(DeviceType.Router, 1, true, 1, nextID, false));
            nextID += 1;
            listBoxDevices.Items.Add(new FormDevice(DeviceType.Computer, 1, false, 1, nextID, true));
            nextID += 1;
            listBoxDevices.Items.Add(new FormDevice(DeviceType.Computer, 1, false, 1, nextID, false));
            nextID += 1;

            ((FormDevice)listBoxDevices.Items[0]).AddConnection((FormDevice) listBoxDevices.Items[1], 5);
            ((FormDevice)listBoxDevices.Items[0]).AddConnection((FormDevice) listBoxDevices.Items[2], 3);
        }

        private void InitializeSimulationParameters()
        {
            ulong totalPackets = 5;
            ulong sendUntil = 100;
            double maliciousProbability = 0.1;
            int numberAttempts = 3;
            ulong timeout = 10;
            int seed = 123;

            numericUpDownTotalPackets.Text = totalPackets.ToString();
            numericUpDownSendUntil.Text = sendUntil.ToString();
            // Uniform distribution
            radioButtonDistributionUniform.Checked = true;

            numericUpDownProbabilityMalicious.Text = maliciousProbability.ToString();
            numericUpDownNumberAttempts.Text = numberAttempts.ToString();
            numericUpDownTimeout.Text = timeout.ToString();
            numericUpDownRandomSeed.Text = seed.ToString();
        }

        private void SetInputLimits()
        {
            numericUpDownDeviceTransferTime.Maximum = int.MaxValue;
            numericUpDownDeviceTimeProcess.Maximum = int.MaxValue;
            numericUpDownDeviceTimeProcessFirewall.Maximum = int.MaxValue;

            numericUpDownTotalPackets.Maximum = ulong.MaxValue;
            numericUpDownSendUntil.Maximum = ulong.MaxValue;
            numericUpDownProbabilityMalicious.Maximum = 1;
            numericUpDownNumberAttempts.Maximum = 100;
            numericUpDownTimeout.Maximum = ulong.MaxValue;
            numericUpDownRandomSeed.Maximum = int.MaxValue;

            numericUpDownProbabilityMalicious.Increment = 0.01M;

            numericUpDownNumberAttempts.Minimum = 1;
            numericUpDownTimeout.Minimum = 1;
        }

        private void ListBoxDeviceConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDeviceConnections.SelectedItem is null)
            {
                checkBoxDeviceConnected.Enabled = false;
                checkBoxDeviceConnected.Checked = false;
                numericUpDownDeviceTransferTime.Enabled = false;
                numericUpDownDeviceTransferTime.Text = "";
                return;
            }

            checkBoxDeviceConnected.Enabled = true;
            checkBoxDeviceConnected.Checked = false;
            numericUpDownDeviceTransferTime.Text = "";
            numericUpDownDeviceTransferTime.Enabled = false;

            FormDevice selectedDevice = (FormDevice)listBoxDevices.SelectedItem;
            FormDevice selectedDeviceConnections = (FormDevice)((ListBox)sender).SelectedItem;

            int transfer_time = selectedDevice.GetTransferTime(selectedDeviceConnections);
            
            // non-positive transfer time indicates there is no link
            if (transfer_time > 0)
            {
                checkBoxDeviceConnected.Checked = true;
                numericUpDownDeviceTransferTime.Enabled = true;
                numericUpDownDeviceTransferTime.Text = transfer_time.ToString();
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            if (nextID < int.MaxValue)
            {
                FormDevice device = new FormDevice(DeviceType.Computer, 1, false, 1, nextID, false);
                nextID += 1;
                listBoxDevices.Items.Add(device);
                ListBoxDevices_SelectedIndexChanged(listBoxDevices, e);
            }
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            FormDevice selectedDevice = (FormDevice)listBoxDevices.SelectedItem;
            // This removes this device from connections of other devices
            while (selectedDevice.connections.Count != 0)
            {
                selectedDevice.RemoveConnection(selectedDevice.connections[0]);
            }

            listBoxDevices.Items.Remove(selectedDevice);
            buttonRemove.Enabled = false;
        }

        private void CheckBoxDeviceConnected_Click(object sender, EventArgs e)
        {
            FormDevice device1 = (FormDevice)listBoxDevices.SelectedItem;
            FormDevice device2 = (FormDevice)listBoxDeviceConnections.SelectedItem;
            if (((CheckBox) sender).Checked)
            {
                numericUpDownDeviceTransferTime.Enabled = true;
                int timeToTransfer = 1;
                device1.AddConnection(device2, timeToTransfer);
                numericUpDownDeviceTransferTime.Text = timeToTransfer.ToString();
            }
            else
            {
                numericUpDownDeviceTransferTime.Enabled = false;
                device1.RemoveConnection(device2);
                numericUpDownDeviceTransferTime.Text = "";
            }
        }

        private void NumericUpDownDeviceTransferTime_ValueChanged(object sender, EventArgs e)
        {
            if (((NumericUpDown)sender).Text != "")
            {
                FormDevice device1 = (FormDevice)listBoxDevices.SelectedItem;
                FormDevice device2 = (FormDevice)listBoxDeviceConnections.SelectedItem;

                device1.ChangeTransferTime(device2, (int) ((NumericUpDown)sender).Value);
            }
        }

        private void ComboBoxDeviceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeviceType dt = ((ComboBox)sender).SelectedIndex switch
            {
                0 => DeviceType.Router,
                1 => DeviceType.Computer,
                _ => DeviceType.Computer
            };

            FormDevice device = (FormDevice)listBoxDevices.SelectedItem;
            device.SetType(dt);
            ListBoxDevices_SelectedIndexChanged(listBoxDevices, e);
            listBoxDevices.Items[listBoxDevices.Items.IndexOf(device)] = listBoxDevices.Items[listBoxDevices.Items.IndexOf(device)];
        }

        private void CheckBoxDeviceFirewall_Click(object sender, EventArgs e)
        {
            FormDevice device = (FormDevice)listBoxDevices.SelectedItem;

            if (((CheckBox)sender).Checked)
            {
                numericUpDownDeviceTimeProcessFirewall.Enabled = true;
                device.firewall = true;
                numericUpDownDeviceTimeProcessFirewall.Text = device.firewallTimeToProcess.ToString();
            }
            else
            {
                numericUpDownDeviceTimeProcessFirewall.Enabled = false;
                device.firewall = false;
                numericUpDownDeviceTimeProcessFirewall.Text = "";
            }
        }
        
        private void NumericUpDownDeviceTimeProcessFirewall_ValueChanged(object sender, EventArgs e)
        {
            if (((NumericUpDown)sender).Text != "")
            {
                FormDevice device = (FormDevice)listBoxDevices.SelectedItem;
                device.firewallTimeToProcess = (int) ((NumericUpDown)sender).Value;
            }
        }

        private void NumericUpDownDeviceTimeProcess_ValueChanged(object sender, EventArgs e)
        {
            if (((NumericUpDown)sender).Text != "")
            {
                FormDevice device = (FormDevice)listBoxDevices.SelectedItem;
                device.timeToProcess = (int)((NumericUpDown)sender).Value ;
            }
        }

        private void CheckBoxDeviceMalicious_Click(object sender, EventArgs e)
        {
            FormDevice device = (FormDevice)listBoxDevices.SelectedItem;

            if (((CheckBox)sender).Checked)
            {
                device.malicious = true;
            }
            else
            {
                device.malicious = false;
            }
        }

        private void ButtonSelectFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFolderPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void ButtonResetFolder_Click(object sender, EventArgs e)
        {
            textBoxFolderPath.Text = "";
        }
    }

    class FormDevice
    {
        public List<FormDevice> connections;
        public List<int> transferTimes;
        public DeviceType deviceType;
        public int timeToProcess;
        public bool firewall, malicious;
        public int firewallTimeToProcess;
        public int ID;

        public FormDevice(DeviceType deviceType, int timeToProcess, bool firewall, int firewallTimeToProcess, int ID, bool malicious)
        {
            this.deviceType = deviceType;
            this.timeToProcess = timeToProcess;
            this.firewall = firewall;
            this.firewallTimeToProcess = firewallTimeToProcess;
            this.ID = ID;
            this.malicious = malicious;

            connections = new List<FormDevice>();
            transferTimes = new List<int>();
        }

        public int GetTransferTime(FormDevice device)
        {
            foreach (FormDevice connectedDevice in connections)
            {
                if (connectedDevice == device)
                {
                    int index = connections.IndexOf(device);
                    return transferTimes[index];
                }
            }
            return -1;
        }

        // Adds connection to both devices
        public void AddConnection(FormDevice device, int transferTime)
        {
            connections.Add(device);
            transferTimes.Add(transferTime);

            device.connections.Add(this);
            device.transferTimes.Add(transferTime);
        }

        // Removes connection from both devices
        public void RemoveConnection(FormDevice device)
        {
            int index = connections.IndexOf(device);
            connections.Remove(device);
            transferTimes.RemoveAt(index);

            index = device.connections.IndexOf(this);
            device.connections.Remove(this);
            device.transferTimes.RemoveAt(index);
        }

        // Changes transferTime for both
        public void ChangeTransferTime(FormDevice device, int transferTime)
        {
            int index = connections.IndexOf(device);
            transferTimes[index] = transferTime;

            index = device.connections.IndexOf(this);
            device.transferTimes[index] = transferTime;
        }

        public void SetType(DeviceType type)
        {
            if (deviceType == DeviceType.Router && type == DeviceType.Computer)
            {
                firewall = false;
            }
            else if (deviceType == DeviceType.Computer && type == DeviceType.Router)
            {
                malicious = false;
            }
            deviceType = type;
        }

        public override string ToString()
        {
            if (deviceType == DeviceType.Router)
            {
                return $"R{ID}";
            }
            else
            {
                return $"PC{ID}";
            }
        }
    }

   class SimulationParametersDto
    {
        public ListBox.ObjectCollection Devices { get; set; }
        public ulong TotalPackets { get; set; }
        public ulong SendUntil { get; set; }
        public ulong Timeout { get; set; }
        public bool DistributionUniform { get; set; }
        public double ProbabilityMalicious { get; set; }
        public int NumberAttempts { get; set; }
        public int RandomSeed { get; set; }
    }

   class ResultControls
    {
        public TextBox SimulationLength { get; set; }
        public TextBox PacketsSent { get; set; }
        public TextBox PacketsDelivered { get; set; }
        public TextBox PacketsSentMalicious { get; set; }
        public TextBox PacketsDeliveredMalicious { get; set; }
        public TextBox AverageTimeDelivered { get; set; }
        public TextBox AverageAttempts { get; set; }
    }
}
