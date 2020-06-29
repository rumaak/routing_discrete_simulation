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
        FinishProcessing
    }

    public enum Distribution
    {
        Uniform,
        DiscreteGaussian
    }

    class Packet
    {
        public int source, destination;
        public int ID;
        public bool malicious;
        public ulong timeout;
 
        public Packet(int source, int destination, int ID, bool malicious)
        {
            this.source = source;
            this.destination = destination;
            this.ID = ID;
            this.malicious = malicious;
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
        }

        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            if (simEvent.eventType == EventType.SendPacket)
            {
                if (packetsOut.Count > 0 && !busy)
                {
                    Packet packet = packetsOut[0];
                    
                    if (model.time <= packet.timeout)
                    {
                        SimulationEvent finishSending = new SimulationEvent(model.time + (ulong)timeToTransfer, this, EventType.FinishSending);
                        model.scheduler.Add(finishSending);
                        busy = true;
                    }
                    else
                    {
                        packetsOut.RemoveAt(0);
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

                if (model.time <= packet.timeout)
                {
                    int destinationRoutingIndex = Simulation.deviceIndexToRoutingIndex[packet.destination];
                    int nextHopRoutingIndex = model.routingTable[sourceDevice.routingTableIndex, destinationRoutingIndex];
                    Device nextHopDevice = Simulation.routingIndexToDevice[nextHopRoutingIndex];

                    SimulationEvent receiveEvent = new SimulationEvent(model.time, nextHopDevice, EventType.ProcessPacket);
                    model.scheduler.Add(receiveEvent);
                    nextHopDevice.addPacketIn(packet);
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

    // All processes share the same ID pool, packets have their own ID pool
    abstract class Process
    {
        public int ID;
        protected List<Packet> packetsOut;

        public abstract void HandleEvent(SimulationEvent simEvent, Model model);
        public void addPacketOut(Packet packetOut)
        {
            packetsOut.Add(packetOut);
        }
    }

    abstract class Device : Process
    {
        public int routingTableIndex;
        protected List<Link> links;
        protected List<Packet> packetsIn;

        public void addLink(Link link)
        {
            links.Add(link);
        }
        public void addPacketIn(Packet packetIn)
        {
            packetsIn.Add(packetIn);
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

                SimulationEvent sendPacket = new SimulationEvent(model.time, link, EventType.SendPacket);
                link.addPacketOut(packet);
                model.scheduler.Add(sendPacket);
            }
            else if (simEvent.eventType == EventType.ProcessPacket)
            {
                if (packetsIn.Count > 0 && !processing)
                {
                    Packet packet = packetsIn[0];

                    if (model.time <= packet.timeout)
                    {
                        processing = true;
                        SimulationEvent processingFinished = new SimulationEvent(model.time + (ulong)timeToProcess, this, EventType.FinishProcessing);
                        model.scheduler.Add(processingFinished);
                    }
                    else
                    {
                        packetsIn.RemoveAt(0);
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

                if (model.time <= packet.timeout)
                {
                    if (firewall != null)
                    {
                        SimulationEvent processPacket = new SimulationEvent(model.time, firewall, EventType.ProcessPacket);
                        firewall.addPacketOut(packet);
                        model.scheduler.Add(processPacket);
                    }
                    else
                    {
                        SimulationEvent sendPacket = new SimulationEvent(model.time, this, EventType.SendPacket);
                        addPacketOut(packet);
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
        public Computer(int id, int routingTableIndex)
        {
            this.ID = id;
            this.routingTableIndex = routingTableIndex;
            packetsIn = new List<Packet>();
            packetsOut = new List<Packet>();
            links = new List<Link>();
        }
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            if (simEvent.eventType == EventType.SendPacket)
            {
                Packet packet = packetsOut[0];
                packetsOut.RemoveAt(0);

                packet.timeout = model.time + model.timeout;

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

                SimulationEvent sendPacket = new SimulationEvent(model.time, link, EventType.SendPacket);
                link.addPacketOut(packet);
                model.scheduler.Add(sendPacket);
            }
            else if (simEvent.eventType == EventType.ProcessPacket)
            {
                Packet packet = packetsIn[0];
                packetsIn.RemoveAt(0);
                
                if (model.time <= packet.timeout)
                {
                    Debug.WriteLine($"Computer {ID} received packet with id {packet.ID} from computer {packet.source} in time {model.time}, malicious: {packet.malicious}");
                }
                else
                {
                    Debug.WriteLine($"Packet {packet.ID} timed out");
                }
                
            }
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

                    if (model.time <= packet.timeout)
                    {
                        processing = true;
                        SimulationEvent processingFinished = new SimulationEvent(model.time + (ulong)timeToProcess, this, EventType.FinishProcessing);
                        model.scheduler.Add(processingFinished);
                    }
                    else
                    {
                        packetsOut.RemoveAt(0);
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

                if (packet.malicious)
                {
                    Debug.WriteLine($"Firewall {this.ID} found out that packet {packet.ID} is malicious and discarded it.");
                }
                else if (model.time > packet.timeout)
                {
                    Debug.WriteLine($"Packet {packet.ID} timed out");
                }
                else
                {
                    SimulationEvent sendPacket = new SimulationEvent(model.time, router, EventType.SendPacket);
                    router.addPacketOut(packet);
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

            Debug.WriteLine($"{time}: Process {process.ID} is handling event of type {eventType}");
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
        public List<Router> routers;
        public List<Computer> computers;
        public List<Firewall> firewalls;
        public Scheduler scheduler;
        public int[,] routingTable;

        public Model(Scheduler scheduler, ulong timeout)
        {
            time = 0;
            routers = Simulation.getRouters();
            computers = Simulation.getComputers();
            firewalls = Simulation.getFirewalls(routers);
            routingTable = Simulation.getRouting();
            this.scheduler = scheduler;
            this.timeout = timeout;

            int totalPackets = 5;
            ulong maxTime = 100;
            double maliciousProbability = 0.5;
            Simulation.generatePackets(computers, scheduler, totalPackets, maxTime, maliciousProbability, Distribution.DiscreteGaussian);
        }
    }
    
    static class Simulation
    {
        public static int routingIndex = 0;
        public static Dictionary<int, Device> routingIndexToDevice = new Dictionary<int, Device>(); 
        public static Dictionary<int, int> deviceIndexToRoutingIndex = new Dictionary<int, int>();
        public static List<Router> getRouters()
        {
            List<Router> routers = new List<Router>();
            Router router = new Router(1, 1, routingIndex);

            Link link = new Link(5, 2, 5, router);
            router.addLink(link);
            link = new Link(6, 3, 3, router);
            router.addLink(link);

            routers.Add(router);
            routingIndexToDevice.Add(routingIndex, router);
            deviceIndexToRoutingIndex.Add(router.ID, routingIndex);
            routingIndex += 1;
            return routers;
        }

        public static List<Computer> getComputers()
        {
            List<Computer> computers = new List<Computer>();
            
            Computer computer = new Computer(2, routingIndex);

            Link link = new Link(7, 1, 5, computer);
            computer.addLink(link);

            computers.Add(computer);
            routingIndexToDevice.Add(routingIndex, computer);
            deviceIndexToRoutingIndex.Add(computer.ID, routingIndex);
            routingIndex += 1;

            computer = new Computer(3, routingIndex);

            link = new Link(8, 1, 3, computer);
            computer.addLink(link);

            computers.Add(computer);
            routingIndexToDevice.Add(routingIndex, computer);
            deviceIndexToRoutingIndex.Add(computer.ID, routingIndex);
            routingIndex += 1;
            
            return computers;
        }

        public static List<Firewall> getFirewalls(List<Router> routers)
        {
            Router router = routers[0];
            List<Firewall> firewalls = new List<Firewall>();
            Firewall firewall = new Firewall(4, 1, router);
            firewalls.Add(firewall);
            router.setFirewall(firewall);
            return firewalls;
        }

        public static int[,] getRouting()
        {
            int[,] routing_table = {
                { 0, 1, 2 },
                { 1, 1, 0 },
                { 2, 0, 2 }
            };
            return routing_table;
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
 
        public static void generatePackets(List<Computer> computers, Scheduler scheduler, int totalPackets, ulong maxTime, double maliciousProbability, Distribution distribution)
        {
            Random rnd = new Random(123);

            int i = 0;
            while (i < totalPackets)
            {
                int from = rnd.Next(computers.Count);
                int to = rnd.Next(computers.Count);
                ulong when = 0;
                bool malicious = rnd.NextDouble() < maliciousProbability;

                if (distribution == Distribution.Uniform)
                {
                    when = Get64BitRandom(maxTime, rnd);
                }
                else if (distribution == Distribution.DiscreteGaussian)
                {
                    when = getNextGaussian(maxTime, rnd);
                }

                if (from == to) continue;

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

        public static void RunSimulation()
        {
            Debug.WriteLine("Simulation started");

            ulong timeout = 15;
            Scheduler scheduler = new Scheduler();
            Model model = new Model(scheduler, timeout);
            SimulationEvent simEvent = scheduler.GetFirst();
            while (simEvent != null)
            {
                model.time = simEvent.time;
                simEvent.execute(model);
                simEvent = scheduler.GetFirst();
            }
        }
    }
}
