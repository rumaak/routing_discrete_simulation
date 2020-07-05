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

    // Links are one-directional, i.e. for two connected devices there are two links.
    // The network itself isn't directed, but this representation is more practical.
    class Link : Process
    {
        public int toID, timeToTransfer;
        public bool busy;
        public Device sourceDevice;

        public Link(int ID, int toID, int timeToTransfer, Device sourceDevice)
        {
            this.ID = ID;
            this.toID = toID;
            this.timeToTransfer = timeToTransfer;
            this.sourceDevice = sourceDevice;
            busy = false;
            packetsOut = new List<Packet>();
            packetsOutTimeouts = new List<ulong>();
        }

        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            if (simEvent.eventType == EventType.SendPacket)
            {
                if (packetsOut.Count > 0 && !busy)
                {
                    Packet packet = packetsOut[0];
                    ulong packetTimeout = packetsOutTimeouts[0];
                    
                    if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                    {
                        SimulationEvent finishSending = new SimulationEvent(model.time + (ulong)timeToTransfer, this, EventType.FinishSending);
                        model.scheduler.Add(finishSending);
                        busy = true;
                    }
                    else
                    {
                        packetsOut.RemoveAt(0);
                        packetsOutTimeouts.RemoveAt(0);
                        SimulationEvent trySendAnother = new SimulationEvent(model.time, this, EventType.SendPacket);
                        model.scheduler.Add(trySendAnother);

                        Debug.WriteLine($"Packet {packet.ID} timed out");
                    }
                }
            }
            else if (simEvent.eventType == EventType.FinishSending)
            {
                Packet packet = packetsOut[0];
                packetsOut.RemoveAt(0);

                ulong packetTimeout = packetsOutTimeouts[0];
                packetsOutTimeouts.RemoveAt(0);

                if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                {
                    int destinationRoutingIndex = Simulation.deviceIndexToRoutingIndex[packet.destination];
                    int nextHopRoutingIndex = model.routingTable[sourceDevice.routingTableIndex, destinationRoutingIndex];
                    Device nextHopDevice = Simulation.routingIndexToDevice[nextHopRoutingIndex];

                    SimulationEvent receiveEvent = new SimulationEvent(model.time, nextHopDevice, EventType.ProcessPacket);
                    model.scheduler.Add(receiveEvent);
                    nextHopDevice.addPacketIn(packet);
                    nextHopDevice.addPacketInTimeout(packet.timeout);
                }
                else
                {
                    Debug.WriteLine($"Packet {packet.ID} timed out");
                }

                SimulationEvent trySendAnother = new SimulationEvent(model.time, this, EventType.SendPacket);
                model.scheduler.Add(trySendAnother);
                busy = false;
            }
        }
    }

    abstract class Process
    {
        public int ID;
        protected List<Packet> packetsOut;
        protected List<ulong> packetsOutTimeouts;

        public abstract void HandleEvent(SimulationEvent simEvent, Model model);
        public void addPacketOut(Packet packetOut)
        {
            packetsOut.Add(packetOut);
        }
        public void addPacketOutTimeout(ulong timeout)
        {
            packetsOutTimeouts.Add(timeout);
        }
    }

    abstract class Device : Process
    {
        public int routingTableIndex;
        protected List<Link> links;
        protected List<Packet> packetsIn;
        protected List<ulong> packetsInTimeouts;

        public void addLink(Link link)
        {
            links.Add(link);
        }
        public void addPacketIn(Packet packetIn)
        {
            packetsIn.Add(packetIn);
        }
        public void addPacketInTimeout(ulong timeout)
        {
            packetsInTimeouts.Add(timeout);
        }
        protected Link getLink(Packet packet, Model  model)
        {
            int destinationRoutingIndex = Simulation.deviceIndexToRoutingIndex[packet.destination];
            int nextHopRoutingIndex = model.routingTable[routingTableIndex, destinationRoutingIndex];
            Device nextHopDevice = Simulation.routingIndexToDevice[nextHopRoutingIndex];

            Link link = null;
            foreach (Link l in links)
            {
                if (l.toID == nextHopDevice.ID)
                {
                    link = l;
                }
            }

            return link;
        }
    }
    
    class Router : Device
    {
        public int timeToProcess;
        public bool processing;
        Firewall firewall;
        public Router(int id, int timeToProcess, int routingTableIndex)
        {
            this.ID = id;
            this.timeToProcess = timeToProcess;
            this.routingTableIndex = routingTableIndex;
            packetsIn = new List<Packet>();
            packetsOut = new List<Packet>();
            links = new List<Link>();
            packetsOutTimeouts = new List<ulong>();
            packetsInTimeouts = new List<ulong>();
            processing = false;
            firewall = null;
        }

        public void setFirewall(Firewall firewall)
        {
            this.firewall = firewall;
        }
        
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            if (simEvent.eventType == EventType.SendPacket)
            {
                Packet packet = packetsOut[0];
                packetsOut.RemoveAt(0);

                ulong packetTimeout = packetsOutTimeouts[0];
                packetsOutTimeouts.RemoveAt(0);

                Link link = getLink(packet, model);

                SimulationEvent sendPacket = new SimulationEvent(model.time, link, EventType.SendPacket);
                link.addPacketOut(packet);
                link.addPacketOutTimeout(packetTimeout);
                model.scheduler.Add(sendPacket);
            }
            else if (simEvent.eventType == EventType.ProcessPacket)
            {
                if (packetsIn.Count > 0 && !processing)
                {
                    Packet packet = packetsIn[0];
                    ulong packetTimeout = packetsInTimeouts[0];

                    if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                    {
                        processing = true;
                        SimulationEvent processingFinished = new SimulationEvent(model.time + (ulong)timeToProcess, this, EventType.FinishProcessing);
                        model.scheduler.Add(processingFinished);
                    }
                    else
                    {
                        packetsIn.RemoveAt(0);
                        packetsInTimeouts.RemoveAt(0);
                        SimulationEvent tryProcessAnother = new SimulationEvent(model.time, this, EventType.ProcessPacket);
                        model.scheduler.Add(tryProcessAnother);

                        Debug.WriteLine($"Packet {packet.ID} timed out");
                    }
                }
            }
            else if (simEvent.eventType == EventType.FinishProcessing)
            {
                processing = false;
                Packet packet = packetsIn[0];
                packetsIn.RemoveAt(0);

                ulong packetTimeout = packetsInTimeouts[0];
                packetsInTimeouts.RemoveAt(0);

                if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                {
                    if (firewall != null)
                    {
                        SimulationEvent processPacket = new SimulationEvent(model.time, firewall, EventType.ProcessPacket);
                        firewall.addPacketOut(packet);
                        firewall.addPacketOutTimeout(packetTimeout);
                        model.scheduler.Add(processPacket);
                    }
                    else
                    {
                        SimulationEvent sendPacket = new SimulationEvent(model.time, this, EventType.SendPacket);
                        addPacketOut(packet);
                        addPacketOutTimeout(packetTimeout);
                        model.scheduler.Add(sendPacket);
                    }
                }
                else
                {
                    Debug.WriteLine($"Packet {packet.ID} timed out");
                }

                SimulationEvent tryProcessAnother = new SimulationEvent(model.time, this, EventType.ProcessPacket);
                model.scheduler.Add(tryProcessAnother);
            }
        }
    }
    
    class Computer : Device
    {

        List<Packet> packetsSent;
        
        public Computer(int id, int routingTableIndex)
        {
            this.ID = id;
            this.routingTableIndex = routingTableIndex;
            packetsIn = new List<Packet>();
            packetsOut = new List<Packet>();
            packetsSent = new List<Packet>();
            links = new List<Link>();
            packetsOutTimeouts = new List<ulong>();
            packetsInTimeouts = new List<ulong>();
        }
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            if (simEvent.eventType == EventType.SendPacket)
            {
                Packet packet = packetsOut[0];
                packetsOut.RemoveAt(0);

                packet.timeFirstSent = model.time;
                
                sendPacket(packet, model);

                Debug.WriteLine($"{model.time}: Computer {ID} sends packet {packet.ID}");
            }
            else if (simEvent.eventType == EventType.ProcessPacket)
            {
                Packet packet = packetsIn[0];
                packetsIn.RemoveAt(0);

                ulong packetTimeout = packetsInTimeouts[0];
                packetsInTimeouts.RemoveAt(0);
                
                if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                {
                    packet.received = true;

                    model.statistics.deliveredPackets += 1;
                    model.statistics.totalDeliveryTime += model.time - packet.timeFirstSent;
                    if (packet.malicious)
                    {
                        model.statistics.deliveredPacketsMalicious += 1;
                    }
                    else
                    {
                        model.statistics.totalNumberAttempts += packet.attemptNumber;
                    }

                    Debug.WriteLine($"Computer {ID} received packet with id {packet.ID} from computer {packet.source} in time {model.time}, malicious: {packet.malicious}, attempt number: {packet.attemptNumber}");
                }
                else
                {
                    Debug.WriteLine($"Packet {packet.ID} timed out");
                }
                
            }
            else if (simEvent.eventType == EventType.Timeout)
            {
                // Packets timeout in the order they are sent
                Packet packet = packetsSent[0];
                packetsSent.RemoveAt(0);

                if (!packet.received)
                {
                    if (packet.attemptNumber < model.timeoutAttempts)
                    {
                        sendPacket(packet, model);
                        packet.attemptNumber += 1;
                    }
                    else
                    {
                        Debug.WriteLine($"Packet {packet.ID} from computer {ID} couldn't be delivered in {model.timeoutAttempts} attempts");
                    }
                }

            }
        }
        private void sendPacket(Packet packet, Model model)
        {
            packet.timeout = model.time + model.timeout;

            Link link = getLink(packet, model);

            SimulationEvent sendPacket = new SimulationEvent(model.time, link, EventType.SendPacket);
            link.addPacketOut(packet);
            link.addPacketOutTimeout(packet.timeout);
            model.scheduler.Add(sendPacket);

            // Adding a single tick because timeout is inclusive (i.e. if packet arrives precisely in time of timeout,
            // it is still considered to be received and it shouldn't be resent)
            SimulationEvent resolveTimeout = new SimulationEvent(packet.timeout + 1, this, EventType.Timeout);
            packetsSent.Add(packet);
            model.scheduler.Add(resolveTimeout);
        }
    }
    
    class Firewall : Process
    {
        public int timeToProcess;
        public bool processing;
        Router router;
        public Firewall(int id, int timeToProcess, Router router)
        {
            this.ID = id;
            this.timeToProcess = timeToProcess;
            packetsOut = new List<Packet>();
            packetsOutTimeouts = new List<ulong>();
            processing = false;
            this.router = router;
        }
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            if (simEvent.eventType == EventType.ProcessPacket)
            {
                if (packetsOut.Count > 0 && !processing)
                {
                    Packet packet = packetsOut[0];
                    ulong packetTimeout = packetsOutTimeouts[0];

                    if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                    {
                        processing = true;
                        SimulationEvent processingFinished = new SimulationEvent(model.time + (ulong)timeToProcess, this, EventType.FinishProcessing);
                        model.scheduler.Add(processingFinished);
                    }
                    else
                    {
                        packetsOut.RemoveAt(0);
                        packetsOutTimeouts.RemoveAt(0);
                        SimulationEvent tryProcessAnother = new SimulationEvent(model.time, this, EventType.ProcessPacket);
                        model.scheduler.Add(tryProcessAnother);

                        Debug.WriteLine($"Packet {packet.ID} timed out");
                    }
                }
            }
            else if (simEvent.eventType == EventType.FinishProcessing)
            {
                processing = false;
                Packet packet = packetsOut[0];
                packetsOut.RemoveAt(0);

                ulong packetTimeout = packetsOutTimeouts[0];
                packetsOutTimeouts.RemoveAt(0);

                if (packet.malicious)
                {
                    Debug.WriteLine($"Firewall {this.ID} found out that packet {packet.ID} is malicious and discarded it.");
                }
                else if (model.time > packet.timeout || packetTimeout != packet.timeout)
                {
                    Debug.WriteLine($"Packet {packet.ID} timed out");
                }
                else
                {
                    SimulationEvent sendPacket = new SimulationEvent(model.time, router, EventType.SendPacket);
                    router.addPacketOut(packet);
                    router.addPacketOutTimeout(packetTimeout);
                    model.scheduler.Add(sendPacket);
                }

                SimulationEvent tryProcessAnother = new SimulationEvent(model.time, this, EventType.ProcessPacket);
                model.scheduler.Add(tryProcessAnother);
            }
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
        public static int routingIndex = 0;
        public static int linkIndex = 1;
        public static int firewallIndex = 1;
        public static Dictionary<int, Device> routingIndexToDevice = new Dictionary<int, Device>(); 
        public static Dictionary<int, int> deviceIndexToRoutingIndex = new Dictionary<int, int>();
        public static Dictionary<int, int> routingIndexToDeviceIndex = new Dictionary<int, int>();

        public static void extractDevices(Model model, ListBox devices)
        {
            foreach (FormDevice device in devices.Items)
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

        public static void initializeRoutingTables(Model model, ListBox devices)
        {
            int[,] routingTableSuccesors = new int[devices.Items.Count, devices.Items.Count];
            ulong[,] routingTableWeights = new ulong[devices.Items.Count, devices.Items.Count];
            
            for (int i = 0; i < devices.Items.Count; i++)
            {
                for (int j = 0; j < devices.Items.Count; j++)
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

            foreach (FormDevice device in devices.Items)
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

        public static void assignRoutingIndices(ListBox devices)
        {
            foreach (FormDevice device in devices.Items)
            {
                deviceIndexToRoutingIndex.Add(device.ID, routingIndex);
                routingIndexToDeviceIndex.Add(routingIndex, device.ID);
                routingIndex += 1;
            }
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

                if (malicious)
                {
                    statistics.sentPacketsMalicious += 1;
                }

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

        public static void RunSimulation(ListBox devices, NumericUpDown totalPackets, NumericUpDown maxTime, RadioButton distributionUniform, 
            NumericUpDown probabilityMalicious, NumericUpDown timeoutAttempts, NumericUpDown timeout, NumericUpDown randomSeed)
        {
            Debug.WriteLine("Simulation started");

            Scheduler scheduler = new Scheduler();
            Statistics statistics = new Statistics();
            Model model = new Model(scheduler, (ulong)timeout.Value, (int)timeoutAttempts.Value, statistics);

            Distribution distribution;
            if (distributionUniform.Checked)
            {
                distribution = Distribution.Uniform;
            }
            else
            {
                distribution = Distribution.DiscreteGaussian;
            }

            statistics.sentPackets = (ulong)totalPackets.Value;
            assignRoutingIndices(devices);
            extractDevices(model, devices);
            initializeRoutingTables(model, devices);
            generatePackets(model.computers, scheduler, (ulong)totalPackets.Value, (ulong)maxTime.Value, (double)probabilityMalicious.Value, 
                statistics, distribution, (int)randomSeed.Value);

            // TODO remove this line when Floyd-Warshall is implemented
            model.routingTable = getRouting();

            SimulationEvent simEvent = scheduler.GetFirst();
            while (simEvent != null)
            {
                // TODO update GUI values
                model.time = simEvent.time;
                simEvent.execute(model);
                simEvent = scheduler.GetFirst();
            }
            statistics.lengthOfSimulation = model.time;

            Debug.WriteLine($"Length of simulation: {statistics.lengthOfSimulation}");
            Debug.WriteLine($"Sent / delivered packets: {statistics.sentPackets} / {statistics.deliveredPackets}");
            Debug.WriteLine($"Sent / delivered malicious packets: {statistics.sentPacketsMalicious} / {statistics.deliveredPacketsMalicious}");
            Debug.WriteLine($"Average time of delivery: {statistics.getAverageDeliveryTime()}");
            Debug.WriteLine($"Average number of attempts: {statistics.getAverageNumberAttempts()}");

            routingIndex = 0;
            linkIndex = 1;
            firewallIndex = 1;
            routingIndexToDevice = new Dictionary<int, Device>();
            deviceIndexToRoutingIndex = new Dictionary<int, int>();
            routingIndexToDeviceIndex = new Dictionary<int, int>();
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
