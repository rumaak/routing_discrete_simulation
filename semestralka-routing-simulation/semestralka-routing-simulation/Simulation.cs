// Discrete simulation of routing
// Jan Ruman, 1st year of study
// Summer term, 2019 / 2020
// NPRG031

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace semestralka_routing_simulation
{
    /// <summary>
    /// Possible types of events that can occur throughout simulation.
    /// </summary>
    enum EventType
    {
        SendPacket,
        FinishSending,
        ProcessPacket,
        FinishProcessing,
        Timeout
    }

    /// <summary>
    /// Distribution of timestamps of generated packets.
    /// </summary>
    enum Distribution
    {
        Uniform,
        DiscreteGaussian
    }

    /// <summary>
    /// Represents real packets, stores values related to its' current state.
    /// </summary>
    class Packet
    {
        public int source, destination, attemptNumber;
        public bool malicious, received;
        public ulong ID, timeout, timeFirstSent;
 
        public Packet(int source, int destination, ulong ID, bool malicious)
        {
            this.source = source;
            this.destination = destination;
            this.ID = ID;
            this.malicious = malicious;
            received = false;
            attemptNumber = 1;
        }
    }

    /// <summary>
    /// Class representing a simulation event, stores event related data.
    /// </summary>
    class SimulationEvent 
    {
        public ulong time;
        public Process process;
        public EventType eventType;

        public SimulationEvent(ulong time, Process process, EventType eventType)
        {
            this.time = time;
            this.process = process;
            this.eventType = eventType;
        }

        /// <summary>
        /// Inform corresponding process that this event is taking place.
        /// </summary>
        public void Execute(Model model)
        {
            Debug.WriteLine($"{time}: Process {process.GetType().Name}{process.ID} is handling event of type {eventType}");
            process.HandleEvent(this, model);
        }
    }

    /// <summary>
    /// Holds and manages events.
    /// </summary>
    class Scheduler
    {
        List<SimulationEvent> events;

        public Scheduler()
        {
            events = new List<SimulationEvent>();
        }

        /// <summary>
        /// Return event with smallest timestamp.
        /// </summary>
        public SimulationEvent GetFirst()
        {
            SimulationEvent first = null;
            foreach (SimulationEvent simEvent in events)
            {
                if (first is null)
                {
                    first = simEvent;
                } else if (first.time > simEvent.time) 
                {
                    first = simEvent;
                }
            }
            events.Remove(first);
            return first;
        }

        /// <summary>
        /// Add an event.
        /// </summary>
        public void Add(SimulationEvent simEvent) 
        {
            events.Add(simEvent);
        }
    }

    /// <summary>
    /// Class representing simulated environment, holds all the data related to it.
    /// </summary>
    class Model
    {
        public ulong time, timeout;
        public int timeoutAttempts, linkIndex, firewallIndex;
        public List<Router> routers;
        public List<Computer> computers;
        public List<Firewall> firewalls;
        public Scheduler scheduler;
        public Statistics statistics;
        public Routing routing;

        public Model(Scheduler scheduler, ulong timeout, int timeoutAttempts, Statistics statistics, Routing routing)
        {
            time = 0;
            routers = new List<Router>();
            computers = new List<Computer>();
            firewalls = new List<Firewall>();

            // These are used only for creation of links and firewalls, not during simulation itself
            linkIndex = 1;
            firewallIndex = 1;

            this.scheduler = scheduler;
            this.timeout = timeout;
            this.timeoutAttempts = timeoutAttempts;
            this.statistics = statistics;
            this.routing = routing;
        }

        /// <summary>
        /// Creates processes corresponding to supplied data.
        /// </summary>
        public void ExtractDevices(ListBox.ObjectCollection devices)
        {
            foreach (FormDevice device in devices)
            {
                if (device.deviceType == DeviceType.Computer)
                {
                    Computer computer = new Computer(device.ID, routing.deviceIndexToRoutingIndex[device.ID], device.malicious);

                    // Add links to computer that correspond with connected neighboring devices.
                    for (int i = 0; i < device.connections.Count; i++)
                    {
                        Link link = new Link(linkIndex, device.connections[i].ID, device.transferTimes[i], computer);
                        linkIndex += 1;
                        computer.AddLink(link);

                    }

                    computers.Add(computer);
                    routing.routingIndexToDevice.Add(routing.deviceIndexToRoutingIndex[computer.ID], computer);
                }
                else
                {
                    Router router = new Router(device.ID, device.timeToProcess, routing.deviceIndexToRoutingIndex[device.ID]);

                    // Add links to router that correspond with connected neighboring devices.
                    for (int i = 0; i < device.connections.Count; i++)
                    {
                        Link link = new Link(linkIndex, device.connections[i].ID, device.transferTimes[i], router);
                        linkIndex += 1;
                        router.AddLink(link);
                    }

                    // If specified, assign firewall to router.
                    if (device.firewall)
                    {
                        Firewall firewall = new Firewall(firewallIndex, device.firewallTimeToProcess, router);
                        firewallIndex += 1;
                        firewalls.Add(firewall);
                        router.SetFirewall(firewall);
                    }

                    routers.Add(router);
                    routing.routingIndexToDevice.Add(routing.deviceIndexToRoutingIndex[router.ID], router);
                }
            }
        }

        /// <summary>
        /// Creates and assigns packets based on simulation parameters.
        /// </summary>
        public void GeneratePackets(SimulationParametersDto simulationParameters)
        {
            Random rnd = new Random(simulationParameters.RandomSeed);

            Distribution distribution;
            if (simulationParameters.DistributionUniform)
            {
                distribution = Distribution.Uniform;
            }
            else
            {
                distribution = Distribution.DiscreteGaussian;
            }

            ulong i = 0;
            while (i < simulationParameters.TotalPackets)
            {
                int from = rnd.Next(computers.Count);
                int to = rnd.Next(computers.Count);
                ulong when = 0;

                // Loopbacks are forbidden hahahaa
                if (from == to) continue;

                bool malicious = false;
                if (computers[from].malicious)
                {
                    malicious = rnd.NextDouble() < simulationParameters.ProbabilityMalicious;
                }

                // Assign time of sending for packet with respect to selected probability mass function.
                if (distribution == Distribution.Uniform)
                {
                    when = Helpers.GetNextUniform(simulationParameters.SendUntil, rnd);
                }
                else if (distribution == Distribution.DiscreteGaussian)
                {
                    when = Helpers.GetNextGaussian(simulationParameters.SendUntil, rnd);
                }

                Packet packet = new Packet(computers[from].ID, computers[to].ID, i + 1, malicious);
                computers[from].AddPacketOut(packet);

                // Note that the time of sending doesn't need to correspond to this particular packet, i.e.
                // at timestep `when` some packet of `from` computer will be sent, not neccessarily this one.
                SimulationEvent simEvent = new SimulationEvent(when, computers[from], EventType.SendPacket);
                scheduler.Add(simEvent);

                Debug.WriteLine($"Created packet with id {i + 1} from computer {computers[from].ID} to computer {computers[to].ID}, malicious: {malicious}");
                Debug.WriteLine($"Computer {computers[from].ID} is going to attempt to send some packet at time {when}");

                i += 1;
            }
        }
    }

    /// <summary>
    /// Class managing simulation setup, run and post-run tasks.
    /// </summary>
    class Simulation
    {
        public delegate void SafeCallDelegateTextBox(TextBox output, string text);
        public delegate void SafeCallDelegatePanel(Panel panelInput);

        /// <summary>
        /// Updates GUI with latest simulation statistics.
        /// </summary>
        private void UpdateResults(Statistics statistics, ResultControls controls)
        {
            ChangeText(controls.SimulationLength, statistics.lengthOfSimulation.ToString());
            ChangeText(controls.PacketsSent, statistics.sentPackets.ToString());
            ChangeText(controls.PacketsDelivered, statistics.deliveredPackets.ToString());
            ChangeText(controls.PacketsSentMalicious, statistics.sentPacketsMalicious.ToString());
            ChangeText(controls.PacketsDeliveredMalicious, statistics.deliveredPacketsMalicious.ToString());
            ChangeText(controls.AverageTimeDelivered, statistics.GetAverageDeliveryTime().ToString());
            ChangeText(controls.AverageAttempts, statistics.GetAverageNumberAttempts().ToString());
        }

        /// <summary>
        /// Changes TextBox text in thread-safe manner.
        /// </summary>
        private void ChangeText(TextBox output, string text)
        {
            if (output.InvokeRequired)
            {
                var d = new SafeCallDelegateTextBox(ChangeText);
                output.Invoke(d, new object[] { output, text });
            }
            else
            {
                output.Text = text;
            }
        }

        /// <summary>
        /// Enables input controls in thread-safe manner.
        /// </summary>
        private void EnableInput(Panel panelInput)
        {
            if (panelInput.InvokeRequired)
            {
                var d = new SafeCallDelegatePanel(EnableInput);
                panelInput.Invoke(d, new object[] { panelInput });
            }
            else
            {
                panelInput.Enabled = true;
            }
        }

        /// <summary>
        /// Saves simulation results into file.
        /// </summary>
        /// <remarks>
        /// Attempts to save data to folder specified in textBoxFolder, if it was provided. File name corresponds to current date and time.
        /// If writing to file doesn't end successfuly, a MessageBox with error message is shown.
        /// </remarks>
        private void SaveToFile(Statistics statistics, string folderPath)
        {
            if (folderPath != "")
            {
                try
                {
                    string[] lines = 
                    { 
                        $"Simulation length: {statistics.lengthOfSimulation}",
                        $"Packets sent: {statistics.sentPackets}",
                        $"Packets delivered: {statistics.deliveredPackets}",
                        $"Packets sent malicious: {statistics.sentPacketsMalicious}",
                        $"Packets delivered malicious: {statistics.deliveredPacketsMalicious}",
                        $"Average time delivered: {statistics.GetAverageDeliveryTime()}",
                        $"Average number attempts: {statistics.GetAverageNumberAttempts()}",
                    };

                    string fileName = DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss");
                    string path = @$"{folderPath}\{fileName}.txt";
                    File.WriteAllLines(path, lines);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error during saving results to file:\n{e.Message}", "Error saving results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Check if number of computers is greater than 2.
        /// </summary>
        private bool CheckNumberComputers(ListBox.ObjectCollection devices)
        {
            int numberComputers = 0;
            foreach (FormDevice device in devices)
            {
                if (device.deviceType == DeviceType.Computer)
                {
                    numberComputers += 1;
                }
            }

            return numberComputers > 1;
        }

        /// <summary>
        /// Simulation entry point.
        /// Sets up model and related objects, carries out the simulation, publishes simulation results.
        /// </summary>
        public void Run(SimulationParametersDto simulationParameters, ResultControls controls, string folderPath, Panel panelInput)
        {
            Debug.WriteLine("Simulation started");

            // Initialize model and related objects.
            Scheduler scheduler = new Scheduler();
            Statistics statistics = new Statistics();
            Routing routing = new Routing(simulationParameters.Devices);
            Model model = new Model(scheduler, simulationParameters.Timeout, simulationParameters.NumberAttempts, statistics, routing);

            // Check whether there are at least 2 computers.
            if (!CheckNumberComputers(simulationParameters.Devices))
            {
                MessageBox.Show("There need to be at least 2 computers in network.", "Not enough computers", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Less than 2 computers in network");
                EnableInput(panelInput);
                return;
            }

            // Create routing tables and extract device information from GUI.
            routing.AssignRoutingIndices();
            model.ExtractDevices(simulationParameters.Devices);
            routing.InitializeRoutingTables();
            routing.ComputeRoutingTables();

            // If there is a computer not reachable from another computer, exit.
            if (routing.ExistsUnreachable())
            {
                MessageBox.Show("Some devices are unreachable.", "Devices unreachable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Some devices are unreachable");
                EnableInput(panelInput);
                return;
            }

            model.GeneratePackets(simulationParameters);

            // Main simulation loop.
            SimulationEvent simEvent = scheduler.GetFirst();
            ulong lastTime = 0;
            while (simEvent != null)
            {
                model.time = simEvent.time;

                // Check overflow
                try
                {
                    simEvent.Execute(model);
                }
                catch (OverflowException)
                {
                    MessageBox.Show("Overflow has occured. Consider lowering Timeout and Send until simulation parameters", 
                        "Overflow", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Overflow");
                    EnableInput(panelInput);
                    return;
                }
                
                simEvent = scheduler.GetFirst();
                if (model.time != lastTime)
                {
                    statistics.lengthOfSimulation = model.time;
                    UpdateResults(statistics, controls);
                    lastTime = model.time;
                }
            }

            Debug.WriteLine($"Length of simulation: {statistics.lengthOfSimulation}");
            Debug.WriteLine($"Sent / delivered packets: {statistics.sentPackets} / {statistics.deliveredPackets}");
            Debug.WriteLine($"Sent / delivered malicious packets: {statistics.sentPacketsMalicious} / {statistics.deliveredPacketsMalicious}");
            Debug.WriteLine($"Average time of delivery: {statistics.GetAverageDeliveryTime()}");
            Debug.WriteLine($"Average number of attempts: {statistics.GetAverageNumberAttempts()}");

            SaveToFile(statistics, folderPath);
            EnableInput(panelInput);
        }
    }

    /// <summary>
    /// Class responsible for routing in model.
    /// Creates, fills and manages routing tables and provides other functionalities that concern them.
    /// </summary>
    class Routing
    {
        private int routingIndex;
        public int[,] routingTableSuccessors;
        public ulong[,] routingTableWeights;
        public Dictionary<int, int> deviceIndexToRoutingIndex;
        public Dictionary<int, int> routingIndexToDeviceIndex;
        public Dictionary<int, Device> routingIndexToDevice;
        private readonly ListBox.ObjectCollection devices;

        public Routing (ListBox.ObjectCollection devices)
        {
            routingIndex = 0;
            routingTableSuccessors = new int[devices.Count, devices.Count];
            routingTableWeights = new ulong[devices.Count, devices.Count];
            deviceIndexToRoutingIndex = new Dictionary<int, int>();
            routingIndexToDeviceIndex = new Dictionary<int, int>();
            routingIndexToDevice = new Dictionary<int, Device>();

            this.devices = devices;
        }

        /// <summary>
        /// Assignes unique routing index to every device corresponding to its' row / column in routing table.
        /// </summary>
        public void AssignRoutingIndices()
        {
            foreach (FormDevice device in devices)
            {
                deviceIndexToRoutingIndex.Add(device.ID, routingIndex);
                routingIndexToDeviceIndex.Add(routingIndex, device.ID);
                routingIndex += 1;
            }
        }

        /// <summary>
        /// Fills routing tables with starting values.
        /// </summary>
        /// <remarks>
        /// For successors, either neighbors index or -1 is used. For weights, either weight of corresponding connection or ulong.MaxValue is used.
        /// </remarks>
        public void InitializeRoutingTables()
        {
            for (int i = 0; i < devices.Count; i++)
            {
                for (int j = 0; j < devices.Count; j++)
                {
                    if (i == j)
                    {
                        routingTableSuccessors[i, j] = i;
                        routingTableWeights[i, j] = 0;
                    }
                    else
                    {
                        routingTableSuccessors[i, j] = -1;

                        // Each connections' transfer time is limited by int.MaxValue, the number of possible indices for devices
                        // is also limited by int.MaxValue, and because int.MaxValue * int.MaxValue < ulong.MaxValue, this is safe
                        routingTableWeights[i, j] = ulong.MaxValue;
                    }
                }
            }

            // Fill in values for directly connected devices
            foreach (FormDevice device in devices)
            {
                int sourceIndex = deviceIndexToRoutingIndex[device.ID];

                for (int i = 0; i < device.connections.Count; i++)
                {
                    FormDevice connectedDevice = device.connections[i];
                    int destinationIndex = deviceIndexToRoutingIndex[connectedDevice.ID];
                    routingTableSuccessors[sourceIndex, destinationIndex] = destinationIndex;
                    routingTableWeights[sourceIndex, destinationIndex] = (ulong)device.transferTimes[i];
                }
            }
        }

        /// <summary>
        /// Uses Floyd-Warshall algorithm to compute final static routing tables.
        /// </summary>
        public void ComputeRoutingTables()
        {
            int n = routingTableSuccessors.GetLength(1);
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        // Prevent overflow
                        if (routingTableWeights[i, k] != ulong.MaxValue && routingTableWeights[k, j] != ulong.MaxValue)
                        {
                            ulong originalWeight = routingTableWeights[i, j];
                            ulong newWeight = routingTableWeights[i, k] + routingTableWeights[k, j];
                            if (newWeight < originalWeight)
                            {
                                Device intermediateDevice = routingIndexToDevice[k];
                                // Compouter cannot forward a packet, therefore can't serve as an intermediate device during
                                // routing
                                if (intermediateDevice.GetType() == typeof(Router))
                                {
                                    routingTableWeights[i, j] = newWeight;
                                    routingTableSuccessors[i, j] = routingTableSuccessors[i, k];
                                }                            
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if there exists pair of computers that can't reach each other.
        /// </summary>
        public bool ExistsUnreachable()
        {
            for (int i = 0; i < routingTableSuccessors.GetLength(1); i++)
            {
                // No need to check connectivity to / from router
                Device sourceDevice = routingIndexToDevice[i];
                if (sourceDevice.GetType() == typeof(Router)) { continue; }
                for (int j = 0; j < routingTableSuccessors.GetLength(1); j++)
                {
                    // No need to check connectivity to / from router
                    Device destinationDevice = routingIndexToDevice[j];
                    if (destinationDevice.GetType() == typeof(Router)) { continue; }
                    if (routingTableSuccessors[i, j] == -1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Class for storing and manipulating simulation results.
    /// </summary>
    class Statistics
    {
        public ulong lengthOfSimulation;
        public ulong sentPackets, deliveredPackets;
        public ulong sentPacketsMalicious, deliveredPacketsMalicious;
        public double totalDeliveryTime, totalNumberAttempts;

        public Statistics()
        {
            lengthOfSimulation = 0;
            sentPackets = 0;
            deliveredPackets = 0;
            sentPacketsMalicious = 0;
            deliveredPacketsMalicious = 0;
            totalDeliveryTime = 0;
            totalNumberAttempts = 0;
        }

        /// <summary>
        /// Compute how long did it take on average for packet to go from source to destination computer.
        /// </summary>
        /// <remarks>
        /// Includes malicious packets. Can return infinity.
        /// </remarks>
        public double GetAverageDeliveryTime()
        {
            return totalDeliveryTime / deliveredPackets;
        }

        /// <summary>
        /// Compute how long did it take on average for packet to go from source to destination computer.
        /// </summary>
        /// <remarks>
        /// Doesn't include malicious packets (they are resent after being rejected by firewall). Can return infinity.
        /// </remarks>
        public double GetAverageNumberAttempts()
        {
            return totalNumberAttempts / (deliveredPackets - deliveredPacketsMalicious);
        }
    }
}
