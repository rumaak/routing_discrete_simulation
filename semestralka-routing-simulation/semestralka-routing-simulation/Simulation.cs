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

        public void execute(Model model)
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
        public int timeoutAttempts;
        public List<Router> routers;
        public List<Computer> computers;
        public List<Firewall> firewalls;
        public Scheduler scheduler;
        public int[,] routingTable;
        public ulong[,] routingTableWeights;
        public Statistics statistics;

        public Model(Scheduler scheduler, ulong timeout, int timeoutAttempts, Statistics statistics)
        {
            time = 0;
            routers = new List<Router>();
            computers = new List<Computer>();
            firewalls = new List<Firewall>();
            
            this.scheduler = scheduler;
            this.timeout = timeout;
            this.timeoutAttempts = timeoutAttempts;
            this.statistics = statistics;
        }
    }
    
    static class Simulation
    {
        public delegate void SafeCallDelegate(TextBox output, string text);
        public delegate void SafeCallDelegateInput(Panel panelInput);
        public static int routingIndex = 0;
        public static int linkIndex = 1;
        public static int firewallIndex = 1;
        public static Dictionary<int, Device> routingIndexToDevice = new Dictionary<int, Device>(); 
        public static Dictionary<int, int> deviceIndexToRoutingIndex = new Dictionary<int, int>();
        public static Dictionary<int, int> routingIndexToDeviceIndex = new Dictionary<int, int>();

        public static void extractDevices(Model model, ListBox.ObjectCollection devices)
        {
            foreach (FormDevice device in devices)
            {
                if (device.deviceType == DeviceType.Computer)
                {
                    Computer computer = new Computer(device.ID, deviceIndexToRoutingIndex[device.ID]);

                    for (int i = 0; i < device.connections.Count; i++)
                    {
                        Link link = new Link(linkIndex, device.connections[i].ID, device.transferTimes[i], computer);
                        linkIndex += 1;
                        computer.addLink(link);

                    }

                    model.computers.Add(computer);
                    routingIndexToDevice.Add(deviceIndexToRoutingIndex[computer.ID], computer);
                }
                else
                {
                    Router router = new Router(device.ID, device.timeToProcess, deviceIndexToRoutingIndex[device.ID]);

                    for (int i = 0; i < device.connections.Count; i++)
                    {
                        Link link = new Link(linkIndex, device.connections[i].ID, device.transferTimes[i], router);
                        linkIndex += 1;
                        router.addLink(link);
                    }

                    if (device.firewall)
                    {
                        Firewall firewall = new Firewall(firewallIndex, device.firewallTimeToProcess, router);
                        firewallIndex += 1;
                        model.firewalls.Add(firewall);
                        router.setFirewall(firewall);
                    }

                    model.routers.Add(router);
                    routingIndexToDevice.Add(deviceIndexToRoutingIndex[router.ID], router);
                }
            }
        }

        public static int[,] getRouting()
        {
            int[,] routing_table = {
                { 0, 1, 2 },
                { 0, 1, 0 },
                { 0, 0, 2 }
            };
            return routing_table;
        }

        public static void initializeRoutingTables(Model model, ListBox.ObjectCollection devices)
        {
            int[,] routingTableSuccesors = new int[devices.Count, devices.Count];
            ulong[,] routingTableWeights = new ulong[devices.Count, devices.Count];
            
            for (int i = 0; i < devices.Count; i++)
            {
                for (int j = 0; j < devices.Count; j++)
                {
                    if (i == j)
                    {
                        routingTableSuccesors[i, j] = i;
                        routingTableWeights[i, j] = 0;
                    }
                    else
                    {
                        routingTableSuccesors[i, j] = -1;
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
                    routingTableSuccesors[sourceIndex, destinationIndex] = destinationIndex;
                    routingTableWeights[sourceIndex, destinationIndex] = (ulong)device.transferTimes[i];
                }
            }

            model.routingTable = routingTableSuccesors;
            model.routingTableWeights = routingTableWeights;
        }

        public static void assignRoutingIndices(ListBox.ObjectCollection devices)
        {
            foreach (FormDevice device in devices)
            {
                deviceIndexToRoutingIndex.Add(device.ID, routingIndex);
                routingIndexToDeviceIndex.Add(routingIndex, device.ID);
                routingIndex += 1;
            }
        }

        public static void updateResults(Statistics statistics, ResultControls controls)
        {
            changeText(controls.simulationLength, statistics.lengthOfSimulation.ToString());
            changeText(controls.packetsSent, statistics.sentPackets.ToString());
            changeText(controls.packetsDelivered, statistics.deliveredPackets.ToString());
            changeText(controls.packetsSentMalicious, statistics.sentPacketsMalicious.ToString());
            changeText(controls.packetsDeliveredMalicious, statistics.deliveredPacketsMalicious.ToString());
            changeText(controls.averageTimeDelivered, statistics.getAverageDeliveryTime().ToString());
            changeText(controls.averageAttempts, statistics.getAverageNumberAttempts().ToString());
        }

        public static void changeText(TextBox output, string text)
        {
            if (output.InvokeRequired)
            {
                var d = new SafeCallDelegate(changeText);
                output.Invoke(d, new object[] { output, text });
            }
            else
            {
                output.Text = text;
            }
        }

        public static void enableInput(Panel panelInput)
        {
            if (panelInput.InvokeRequired)
            {
                var d = new SafeCallDelegateInput(enableInput);
                panelInput.Invoke(d, new object[] { panelInput });
            }
            else
            {
                panelInput.Enabled = true;
            }
        }

        public static void resetSimulationParameters()
        {
            routingIndex = 0;
            linkIndex = 1;
            firewallIndex = 1;
            routingIndexToDevice = new Dictionary<int, Device>();
            deviceIndexToRoutingIndex = new Dictionary<int, int>();
            routingIndexToDeviceIndex = new Dictionary<int, int>();
        }

        // Normal distribution with mean = maxTime / 2 and std = maxTime / 4
        public static ulong getNextGaussian(ulong maxTime, Random rnd)
        {
            // When generated numbers gets out of bounds, regenerate
            double randNormal = -1;
            while (randNormal < 0 || randNormal > maxTime)
            {
                double mean = ((double)maxTime) / 2;
                double std = ((double)maxTime) / 4;

                double u1 = 1.0 - rnd.NextDouble();
                double u2 = 1.0 - rnd.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                randNormal = mean + std * randStdNormal;
            }

            return (ulong)randNormal;
        }
 
        public static void generatePackets(List<Computer> computers, Scheduler scheduler, ulong totalPackets, ulong maxTime, double maliciousProbability, 
            Statistics statistics, Distribution distribution, int randomSeed)
        {
            Random rnd = new Random(randomSeed);

            ulong i = 0;
            while (i < totalPackets)
            {
                int from = rnd.Next(computers.Count);
                int to = rnd.Next(computers.Count);
                ulong when = 0;
                bool malicious = rnd.NextDouble() < maliciousProbability;

                if (from == to) continue;

                if (distribution == Distribution.Uniform)
                {
                    when = Get64BitRandom(maxTime, rnd);
                }
                else if (distribution == Distribution.DiscreteGaussian)
                {
                    when = getNextGaussian(maxTime, rnd);
                }

                Packet packet = new Packet(computers[from].ID, computers[to].ID, i + 1, malicious);
                computers[from].addPacketOut(packet);

                // Note that the time of sending doesn't need to correspond to this particular packet, i.e.
                // at timestep `when` some packet of `from` computer will be sent, not neccessarily this one.
                SimulationEvent simEvent = new SimulationEvent(when, computers[from], EventType.SendPacket);
                scheduler.Add(simEvent);

                Debug.WriteLine($"Created packet with id {i + 1} from computer {computers[from].ID} to computer {computers[to].ID}, malicious: {malicious}");
                Debug.WriteLine($"Computer {computers[from].ID} is going to attempt to send some packet at time {when}");

                i += 1;
            }
        }
        
        public static ulong Get64BitRandom(ulong maxValue, Random rnd)
        {
            byte[] buffer = new byte[sizeof(ulong)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0) % (maxValue + 1);
        }

        public static void RunSimulation(SimulationParametersDto simulationParameters, ResultControls controls, Panel panelInput)
        {
            Debug.WriteLine("Simulation started");

            Scheduler scheduler = new Scheduler();
            Statistics statistics = new Statistics();
            Model model = new Model(scheduler, simulationParameters.timeout, simulationParameters.numberAttempts, statistics);

            Distribution distribution;
            if (simulationParameters.distributionUniform)
            {
                distribution = Distribution.Uniform;
            }
            else
            {
                distribution = Distribution.DiscreteGaussian;
            }

            assignRoutingIndices(simulationParameters.devices);
            extractDevices(model, simulationParameters.devices);
            initializeRoutingTables(model, simulationParameters.devices);
            generatePackets(model.computers, scheduler, simulationParameters.totalPackets, simulationParameters.sendUntil, simulationParameters.probabilityMalicious, 
                statistics, distribution, simulationParameters.randomSeed);

            // TODO remove this line when Floyd-Warshall is implemented
            model.routingTable = getRouting();

            SimulationEvent simEvent = scheduler.GetFirst();
            ulong lastTime = 0;
            while (simEvent != null)
            {
                model.time = simEvent.time;
                simEvent.execute(model);
                simEvent = scheduler.GetFirst();
                if (model.time != lastTime)
                {
                    statistics.lengthOfSimulation = model.time;
                    updateResults(statistics, controls);
                    lastTime = model.time;
                }
            }

            Debug.WriteLine($"Length of simulation: {statistics.lengthOfSimulation}");
            Debug.WriteLine($"Sent / delivered packets: {statistics.sentPackets} / {statistics.deliveredPackets}");
            Debug.WriteLine($"Sent / delivered malicious packets: {statistics.sentPacketsMalicious} / {statistics.deliveredPacketsMalicious}");
            Debug.WriteLine($"Average time of delivery: {statistics.getAverageDeliveryTime()}");
            Debug.WriteLine($"Average number of attempts: {statistics.getAverageNumberAttempts()}");

            resetSimulationParameters();
            enableInput(panelInput);
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
        public double getAverageDeliveryTime()
        {
            return totalDeliveryTime / deliveredPackets;
        }

        // Can be infinity, doesn't include malicious packets
        public double getAverageNumberAttempts()
        {
            return totalNumberAttempts / (deliveredPackets - deliveredPacketsMalicious);
        }
    }
}
