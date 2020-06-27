using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Schema;

namespace semestralka_routing_simulation
{
    public enum EventType
    {
        SendPacket,
        FinishSending,
        ProcessPacket,
        ReceivePacket
    }

    class Packet
    {
        public int source, destination;
        public int ID;
 
        public Packet(int source, int destination, int ID)
        {
            this.source = source;
            this.destination = destination;
            this.ID = ID;
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
                    SimulationEvent finishSending = new SimulationEvent(model.time + (ulong) timeToTransfer, this, EventType.FinishSending);
                    model.scheduler.Add(finishSending);
                    busy = true;
                }
            }
            else if (simEvent.eventType == EventType.FinishSending)
            {
                Packet packet = packetsOut[0];
                packetsOut.RemoveAt(0);

                int destinationRoutingIndex = Simulation.deviceIndexToRoutingIndex[packet.destination];
                int nextHopRoutingIndex = model.routingTable[sourceDevice.routingTableIndex, destinationRoutingIndex];
                Device nextHopDevice = Simulation.routingIndexToDevice[nextHopRoutingIndex];

                SimulationEvent receiveEvent = new SimulationEvent(model.time, nextHopDevice, EventType.ReceivePacket);
                model.scheduler.Add(receiveEvent);
                nextHopDevice.addPacketIn(packet);

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
        public Router(int id, int timeToProcess, int routingTableIndex)
        {
            this.ID = id;
            this.timeToProcess = timeToProcess;
            this.routingTableIndex = routingTableIndex;
            packetsIn = new List<Packet>();
            packetsOut = new List<Packet>();
            links = new List<Link>();
        }
        
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            // TODO receive event

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
        }
    }
    
    class Firewall : Process
    {
        public int TimeToProcess;
        public Firewall(int id, int timeToProcess)
        {
            this.ID = id;
            this.TimeToProcess = timeToProcess;
        }
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {

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
        public ulong time;
        public List<Router> routers;
        public List<Computer> computers;
        public List<Firewall> firewalls;
        public Scheduler scheduler;
        public int[,] routingTable;

        public Model(Scheduler scheduler)
        {
            time = 0;
            routers = Simulation.getRouters();
            computers = Simulation.getComputers();
            firewalls = Simulation.getFirewalls();
            routingTable = Simulation.getRouting();
            this.scheduler = scheduler;
            Simulation.generatePackets(computers, scheduler, 5, 100);
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

            Link link = new Link(1, 2, 5, router);
            router.addLink(link);
            link = new Link(2, 3, 3, router);
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

            Link link = new Link(3, 1, 5, computer);
            computer.addLink(link);

            computers.Add(computer);
            routingIndexToDevice.Add(routingIndex, computer);
            deviceIndexToRoutingIndex.Add(computer.ID, routingIndex);
            routingIndex += 1;

            computer = new Computer(3, routingIndex);

            link = new Link(4, 1, 3, computer);
            computer.addLink(link);

            computers.Add(computer);
            routingIndexToDevice.Add(routingIndex, computer);
            deviceIndexToRoutingIndex.Add(computer.ID, routingIndex);
            routingIndex += 1;
            
            return computers;
        }

        public static List<Firewall> getFirewalls()
        {
            List<Firewall> firewalls = new List<Firewall>();
            firewalls.Add(new Firewall(4, 1));
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
 
        public static void generatePackets(List<Computer> computers, Scheduler scheduler, int totalPackets, ulong maxTime)
        {
            Random rnd = new Random(123);

            int i = 0;
            while (i < totalPackets)
            {
                int from = rnd.Next(computers.Count);
                int to = rnd.Next(computers.Count);
                ulong when = Get64BitRandom(maxTime, rnd);

                if (from == to) continue;

                Packet packet = new Packet(computers[from].ID, computers[to].ID, i + 1);
                computers[from].addPacketOut(packet);

                // Note that the time of sending doesn't need to correspond to this particular packet, i.e.
                // at timestep `when` some packet of `from` computer will be sent, not neccessarily this one.
                SimulationEvent simEvent = new SimulationEvent(when, computers[from], EventType.SendPacket);
                scheduler.Add(simEvent);

                Debug.WriteLine($"Created packet with id {i + 1} from computer {computers[from].ID} to computer {computers[to].ID}");

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
            
            Scheduler scheduler = new Scheduler();
            Model model = new Model(scheduler);
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
