using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
            initializeDevices();
            initializeSimulationParameters();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Simulation.RunSimulation();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDevices.SelectedItem is null)
            {
                groupBoxDeviceProperties.Visible = false;
                return;
            }

            groupBoxDeviceProperties.Visible = true;
            buttonRemove.Enabled = true;
            listBoxDeviceConnections.Items.Clear();
            FormDevice selectedDevice = (FormDevice) ((ListBox)sender).SelectedItem;

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
            textBoxDeviceTimeProcess.Text = selectedDevice.timeToProcess.ToString();
            if (selectedDevice.firewall)
            {
                checkBoxDeviceFirewall.Enabled = true;
                checkBoxDeviceFirewall.Checked = true;
                textBoxDeviceTimeProcessFirewall.Text = selectedDevice.firewallTimeToProcess.ToString();
                textBoxDeviceTimeProcessFirewall.Enabled = true;
            }
            else
            {
                checkBoxDeviceFirewall.Checked = false;
                textBoxDeviceTimeProcessFirewall.Text = "";
                textBoxDeviceTimeProcessFirewall.Enabled = false;
            }

            if (selectedDevice.deviceType == DeviceType.Computer)
            {
                checkBoxDeviceFirewall.Enabled = false;
                textBoxDeviceTimeProcess.Enabled = false;
                textBoxDeviceTimeProcess.Text = "";
            }
            else
            {
                checkBoxDeviceFirewall.Enabled = true;
                textBoxDeviceTimeProcess.Enabled = true;
                textBoxDeviceTimeProcess.Text = selectedDevice.timeToProcess.ToString();
            }

            listBox2_SelectedIndexChanged(listBoxDeviceConnections, e);
        }

        private void initializeDevices()
        {
            listBoxDevices.Items.Clear();

            listBoxDevices.Items.Add(new FormDevice(DeviceType.Router, 1, true, 1, nextID));
            nextID += 1;
            listBoxDevices.Items.Add(new FormDevice(DeviceType.Computer, 1, false, 1, nextID));
            nextID += 1;
            listBoxDevices.Items.Add(new FormDevice(DeviceType.Computer, 1, false, 1, nextID));
            nextID += 1;

            ((FormDevice)listBoxDevices.Items[0]).addConnection((FormDevice) listBoxDevices.Items[1], 5);
            ((FormDevice)listBoxDevices.Items[0]).addConnection((FormDevice) listBoxDevices.Items[2], 3);
        }

        private void initializeSimulationParameters()
        {
            ulong totalPackets = 5;
            ulong sendUntil = 100;
            double maliciousProbability = 0.1;
            int numberAttempts = 3;
            ulong timeout = 10;
            int seed = 123;

            textBoxTotalPackets.Text = totalPackets.ToString();
            textBoxSendUntil.Text = sendUntil.ToString();
            // Uniform distribution
            radioButtonDistributionUniform.Checked = true;

            textBoxProbabilityMalicious.Text = maliciousProbability.ToString();
            textBoxNumberAttempts.Text = numberAttempts.ToString();
            textBoxTimeout.Text = timeout.ToString();
            textBoxRandomSeed.Text = seed.ToString();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDeviceConnections.SelectedItem is null)
            {
                checkBoxDeviceConnected.Enabled = false;
                checkBoxDeviceConnected.Checked = false;
                textBoxDeviceTransferTime.Enabled = false;
                textBoxDeviceTransferTime.Text = "";
                return;
            }

            checkBoxDeviceConnected.Enabled = true;
            checkBoxDeviceConnected.Checked = false;
            textBoxDeviceTransferTime.Text = "";
            textBoxDeviceTransferTime.Enabled = false;

            FormDevice selectedDevice = (FormDevice)listBoxDevices.SelectedItem;
            FormDevice selectedDeviceConnections = (FormDevice)((ListBox)sender).SelectedItem;

            int transfer_time = selectedDevice.getTransferTime(selectedDeviceConnections);
            
            // non-positive transfer time indicates there is no link
            if (transfer_time > 0)
            {
                checkBoxDeviceConnected.Checked = true;
                textBoxDeviceTransferTime.Enabled = true;
                textBoxDeviceTransferTime.Text = transfer_time.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormDevice device = new FormDevice(DeviceType.Computer, 1, false, 1, nextID);
            nextID += 1;
            listBoxDevices.Items.Add(device);
            listBox1_SelectedIndexChanged(listBoxDevices, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FormDevice selectedDevice = (FormDevice)listBoxDevices.SelectedItem;
            // This removes this device from connections of other devices
            while (selectedDevice.connections.Count != 0)
            {
                selectedDevice.removeConnection(selectedDevice.connections[0]);
            }

            listBoxDevices.Items.Remove(selectedDevice);
            buttonRemove.Enabled = false;
        }

        // Actually used for click event
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            FormDevice device1 = (FormDevice)listBoxDevices.SelectedItem;
            FormDevice device2 = (FormDevice)listBoxDeviceConnections.SelectedItem;
            if (((CheckBox) sender).Checked)
            {
                textBoxDeviceTransferTime.Enabled = true;
                int timeToTransfer = 1;
                device1.addConnection(device2, timeToTransfer);
                textBoxDeviceTransferTime.Text = timeToTransfer.ToString();
            }
            else
            {
                textBoxDeviceTransferTime.Enabled = false;
                device1.removeConnection(device2);
                textBoxDeviceTransferTime.Text = "";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text != "")
            {
                FormDevice device1 = (FormDevice)listBoxDevices.SelectedItem;
                FormDevice device2 = (FormDevice)listBoxDeviceConnections.SelectedItem;

                device1.changeTransferTime(device2, int.Parse(((TextBox)sender).Text));
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeviceType dt;
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0:
                    dt = DeviceType.Router;
                    break;
                case 1:
                default:
                    dt = DeviceType.Computer;
                    break;
            }

            FormDevice device = (FormDevice)listBoxDevices.SelectedItem;
            device.setType(dt);
            listBox1_SelectedIndexChanged(listBoxDevices, e);
            listBoxDevices.Items[listBoxDevices.Items.IndexOf(device)] = listBoxDevices.Items[listBoxDevices.Items.IndexOf(device)];
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            FormDevice device = (FormDevice)listBoxDevices.SelectedItem;

            if (((CheckBox)sender).Checked)
            {
                textBoxDeviceTimeProcessFirewall.Enabled = true;
                device.firewall = true;
                textBoxDeviceTimeProcessFirewall.Text = device.firewallTimeToProcess.ToString();
            }
            else
            {
                textBoxDeviceTimeProcessFirewall.Enabled = false;
                device.firewall = false;
                textBoxDeviceTimeProcessFirewall.Text = "";
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text != "")
            {
                FormDevice device = (FormDevice)listBoxDevices.SelectedItem;
                device.firewallTimeToProcess = int.Parse(((TextBox)sender).Text);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text != "")
            {
                FormDevice device = (FormDevice)listBoxDevices.SelectedItem;
                device.timeToProcess = int.Parse(((TextBox)sender).Text);
            }
        }
    }

    class FormDevice
    {
        public List<FormDevice> connections;
        public List<int> transferTimes;
        public DeviceType deviceType;
        public int timeToProcess;
        public bool firewall;
        public int firewallTimeToProcess;
        public int ID;

        public FormDevice(DeviceType deviceType, int timeToProcess, bool firewall, int firewallTimeToProcess, int ID)
        {
            this.deviceType = deviceType;
            this.timeToProcess = timeToProcess;
            this.firewall = firewall;
            this.firewallTimeToProcess = firewallTimeToProcess;
            this.ID = ID;

            connections = new List<FormDevice>();
            transferTimes = new List<int>();
        }

        public int getTransferTime(FormDevice device)
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
        public void addConnection(FormDevice device, int transferTime)
        {
            connections.Add(device);
            transferTimes.Add(transferTime);

            device.connections.Add(this);
            device.transferTimes.Add(transferTime);
        }

        // Removes connection from both devices
        public void removeConnection(FormDevice device)
        {
            int index = connections.IndexOf(device);
            connections.Remove(device);
            transferTimes.RemoveAt(index);

            index = device.connections.IndexOf(this);
            device.connections.Remove(this);
            device.transferTimes.RemoveAt(index);
        }

        // Changes transferTime for both
        public void changeTransferTime(FormDevice device, int transferTime)
        {
            int index = connections.IndexOf(device);
            transferTimes[index] = transferTime;

            index = device.connections.IndexOf(this);
            device.transferTimes[index] = transferTime;
        }

        public void setType(DeviceType type)
        {
            if (deviceType == DeviceType.Router && type == DeviceType.Computer)
            {
                firewall = false;
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
}
