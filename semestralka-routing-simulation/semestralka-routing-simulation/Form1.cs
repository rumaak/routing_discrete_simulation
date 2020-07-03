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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Simulation.RunSimulation();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is null)
            {
                groupBox2.Visible = false;
                return;
            }

            groupBox2.Visible = true;
            button3.Enabled = true;
            listBox2.Items.Clear();
            FormDevice selectedDevice = (FormDevice) ((ListBox)sender).SelectedItem;

            // Update connected devices
            foreach (FormDevice device in listBox1.Items)
            {
                if (device != selectedDevice)
                {
                    listBox2.Items.Add(device);
                }
            }

            // Update device properties
            switch (selectedDevice.deviceType)
            {
                case DeviceType.Computer:
                    comboBox1.SelectedIndex = 1;
                    break;
                case DeviceType.Router:
                    comboBox1.SelectedIndex = 0;
                    break;
            }
            textBox2.Text = selectedDevice.timeToProcess.ToString();
            if (selectedDevice.firewall)
            {
                checkBox2.Enabled = true;
                checkBox2.Checked = true;
                textBox3.Text = selectedDevice.firewallTimeToProcess.ToString();
                textBox3.Enabled = true;
            }
            else
            {
                checkBox2.Checked = false;
                textBox3.Text = "";
                textBox3.Enabled = false;
            }

            if (selectedDevice.deviceType == DeviceType.Computer)
            {
                checkBox2.Enabled = false;
                textBox2.Enabled = false;
                textBox2.Text = "";
            }
            else
            {
                checkBox2.Enabled = true;
                textBox2.Enabled = true;
                textBox2.Text = selectedDevice.timeToProcess.ToString();
            }

            listBox2_SelectedIndexChanged(listBox2, e);
        }

        private void initializeDevices()
        {
            listBox1.Items.Clear();

            listBox1.Items.Add(new FormDevice(DeviceType.Router, 1, true, 1, nextID));
            nextID += 1;
            listBox1.Items.Add(new FormDevice(DeviceType.Computer, 1, false, 1, nextID));
            nextID += 1;
            listBox1.Items.Add(new FormDevice(DeviceType.Computer, 1, false, 1, nextID));
            nextID += 1;

            ((FormDevice)listBox1.Items[0]).addConnection((FormDevice) listBox1.Items[1], 5);
            ((FormDevice)listBox1.Items[0]).addConnection((FormDevice) listBox1.Items[2], 3);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem is null)
            {
                checkBox1.Enabled = false;
                checkBox1.Checked = false;
                textBox1.Enabled = false;
                textBox1.Text = "";
                return;
            }

            checkBox1.Enabled = true;
            checkBox1.Checked = false;
            textBox1.Text = "";
            textBox1.Enabled = false;

            FormDevice selectedDevice = (FormDevice)listBox1.SelectedItem;
            FormDevice selectedDeviceConnections = (FormDevice)((ListBox)sender).SelectedItem;

            int transfer_time = selectedDevice.getTransferTime(selectedDeviceConnections);
            
            // non-positive transfer time indicates there is no link
            if (transfer_time > 0)
            {
                checkBox1.Checked = true;
                textBox1.Enabled = true;
                textBox1.Text = transfer_time.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormDevice device = new FormDevice(DeviceType.Computer, 1, false, 1, nextID);
            nextID += 1;
            listBox1.Items.Add(device);
            listBox1_SelectedIndexChanged(listBox1, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FormDevice selectedDevice = (FormDevice)listBox1.SelectedItem;
            // This removes this device from connections of other devices
            while (selectedDevice.connections.Count != 0)
            {
                selectedDevice.removeConnection(selectedDevice.connections[0]);
            }

            listBox1.Items.Remove(selectedDevice);
            button3.Enabled = false;
        }

        // Actually used for click event
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            FormDevice device1 = (FormDevice)listBox1.SelectedItem;
            FormDevice device2 = (FormDevice)listBox2.SelectedItem;
            if (((CheckBox) sender).Checked)
            {
                textBox1.Enabled = true;
                int timeToTransfer = 1;
                device1.addConnection(device2, timeToTransfer);
                textBox1.Text = timeToTransfer.ToString();
            }
            else
            {
                textBox1.Enabled = false;
                device1.removeConnection(device2);
                textBox1.Text = "";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text != "")
            {
                FormDevice device1 = (FormDevice)listBox1.SelectedItem;
                FormDevice device2 = (FormDevice)listBox2.SelectedItem;

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

            FormDevice device = (FormDevice)listBox1.SelectedItem;
            device.setType(dt);
            listBox1_SelectedIndexChanged(listBox1, e);
            listBox1.Items[listBox1.Items.IndexOf(device)] = listBox1.Items[listBox1.Items.IndexOf(device)];
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            FormDevice device = (FormDevice)listBox1.SelectedItem;

            if (((CheckBox)sender).Checked)
            {
                textBox3.Enabled = true;
                device.firewall = true;
                textBox3.Text = device.firewallTimeToProcess.ToString();
            }
            else
            {
                textBox3.Enabled = false;
                device.firewall = false;
                textBox3.Text = "";
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text != "")
            {
                FormDevice device = (FormDevice)listBox1.SelectedItem;
                device.firewallTimeToProcess = int.Parse(((TextBox)sender).Text);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text != "")
            {
                FormDevice device = (FormDevice)listBox1.SelectedItem;
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
