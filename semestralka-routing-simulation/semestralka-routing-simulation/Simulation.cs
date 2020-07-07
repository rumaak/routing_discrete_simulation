using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace semestralka_routing_simulation
{
    public enum EventType
    {
        SendPacket,
        FinishSending,
        ProcessPacket,
        FinishProcessing,
        Timeout
    }

    public enum Distribution
    {
        Uniform,
        DiscreteGaussian
    }

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

        public void Execute(Model model)
        {

            Debug.WriteLine($"{time}: Process {process.GetType().Name}{process.ID} is handling event of type {eventType}");
            process.HandleEvent(this, model);
        }
    }

    class Scheduler
    {
        List<SimulationEvent> events;

        public Scheduler()
        {
            events = new List<SimulationEvent>();
        }

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

        public void Add(SimulationEvent simEvent) 
        {
            events.Add(simEvent);
        }
    }

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
            linkIndex = 1;
            firewallIndex = 1;

            this.scheduler = scheduler;
            this.timeout = timeout;
            this.timeoutAttempts = timeoutAttempts;
            this.statistics = statistics;
            this.routing = routing;
        }

        public void ExtractDevices(ListBox.ObjectCollection devices)
        {
            foreach (FormDevice device in devices)
            {
                if (device.deviceType == DeviceType.Computer)
                {
                    Computer computer = new Computer(device.ID, routing.deviceIndexToRoutingIndex[device.ID]);

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

                    for (int i = 0; i < device.connections.Count; i++)
                    {
                        Link link = new Link(linkIndex, device.connections[i].ID, device.transferTimes[i], router);
                        linkIndex += 1;
                        router.AddLink(link);
                    }

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
                bool malicious = rnd.NextDouble() < simulationParameters.ProbabilityMalicious;

                if (from == to) continue;

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
    
    class Simulation
    {
        public delegate void SafeCallDelegateTextBox(TextBox output, string text);
        public delegate void SafeCallDelegatePanel(Panel panelInput);

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

        public void Run(SimulationParametersDto simulationParameters, ResultControls controls, Panel panelInput)
        {
            Debug.WriteLine("Simulation started");

            Scheduler scheduler = new Scheduler();
            Statistics statistics = new Statistics();
            Routing routing = new Routing(simulationParameters.Devices);
            Model model = new Model(scheduler, simulationParameters.Timeout, simulationParameters.NumberAttempts, statistics, routing);

            routing.AssignRoutingIndices();
            model.ExtractDevices(simulationParameters.Devices);
            routing.InitializeRoutingTables();
            routing.ComputeRoutingTables();

            if (routing.ExistsUnreachable())
            {
                MessageBox.Show("Some devices are unreachable.", "Devices unreachable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Some devices are unreachable");
                EnableInput(panelInput);
                return;
            }

            model.GeneratePackets(simulationParameters);

            SimulationEvent simEvent = scheduler.GetFirst();
            ulong lastTime = 0;
            while (simEvent != null)
            {
                model.time = simEvent.time;
                simEvent.Execute(model);
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

            EnableInput(panelInput);
        }
    }

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

        public void AssignRoutingIndices()
        {
            foreach (FormDevice device in devices)
            {
                deviceIndexToRoutingIndex.Add(device.ID, routingIndex);
                routingIndexToDeviceIndex.Add(routingIndex, device.ID);
                routingIndex += 1;
            }
        }

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

        // Can be infinity, includes malicious packets
        public double GetAverageDeliveryTime()
        {
            return totalDeliveryTime / deliveredPackets;
        }

        // Can be infinity, doesn't include malicious packets
        public double GetAverageNumberAttempts()
        {
            return totalNumberAttempts / (deliveredPackets - deliveredPacketsMalicious);
        }
    }
}
