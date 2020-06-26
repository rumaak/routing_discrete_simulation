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

        public Link(int ID, int toID, int timeToTransfer)
        {
            this.ID = ID;
            this.toID = toID;
            this.timeToTransfer = timeToTransfer;
            busy = false;
            packets = new List<Packet>();
        }

        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {

        }
    }

    // All processes share the same ID pool, packets have their own ID pool
    abstract class Process
    {
        public int ID;
        protected List<Packet> packets;

        public abstract void HandleEvent(SimulationEvent simEvent, Model model);
        public void addPacket(Packet packet)
        {
            packets.Add(packet);
        }
    }
    
    class Router : Process
    {
        public int timeToProcess, routingTableIndex;
        List<Link> links;
        public Router(int id, int timeToProcess, int routingTableIndex)
        {
            this.ID = id;
            this.timeToProcess = timeToProcess;
            this.routingTableIndex = routingTableIndex;
            this.packets = new List<Packet>();
            links = new List<Link>();
        }
        public void addLink(Link link)
        {
            links.Add(link);
        }
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            
        }
    }
    
    class Computer : Process
    {
        public int routingTableIndex;
        List<Link> links;
        public Computer(int id, int routingTableIndex)
        {
            this.ID = id;
            this.routingTableIndex = routingTableIndex;
            packets = new List<Packet>();
            links = new List<Link>();
        }
        public void addLink(Link link)
        {
            links.Add(link);
        }
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            // TODO redelegate tasks to Links
            // - when computer is supposed to send a packet, its sole purpose will be to determine which
            //   link is to be used and forward it
            // - everything else is done inside link


            if (simEvent.eventType == EventType.SendPacket)
            {
                if (packets.Count > 0)
                {
                    Packet packet = packets[0];
                    int destinationRoutingIndex = Simulation.deviceIndexToRoutingIndex[packet.destination];
                    int nextHopRoutingIndex = model.routingTable[routingTableIndex, destinationRoutingIndex];
                    Process nextHopDevice = Simulation.routingIndexToDevice[nextHopRoutingIndex];

                    Link link = null;
                    foreach (Link l in links)
                    {
                        if (l.toID == nextHopDevice.ID)
                        {
                            link = l;
                        }
                    }
                    
                    if (!link.busy)
                    {
                        link.busy = true;
                        SimulationEvent finishSending = new SimulationEvent(model.time + (ulong)link.timeToTransfer, this, EventType.FinishSending);
                        model.scheduler.Add(finishSending);
                    }
                }
            }
            else if (simEvent.eventType == EventType.FinishSending)
            {
                Packet packet = packets[0];
                packets.RemoveAt(0);

                int destinationRoutingIndex = Simulation.deviceIndexToRoutingIndex[packet.destination];
                int nextHopRoutingIndex = model.routingTable[routingTableIndex, destinationRoutingIndex];
                Process nextHopDevice = Simulation.routingIndexToDevice[nextHopRoutingIndex];

                Link link = null;
                foreach (Link l in links)
                {
                    if (l.toID == nextHopDevice.ID)
                    {
                        link = l;
                    }
                }

                link.busy = false;

                SimulationEvent receiveEvent = new SimulationEvent(model.time, nextHopDevice, EventType.ReceivePacket);
                model.scheduler.Add(receiveEvent);
                nextHopDevice.addPacket(packet);

                SimulationEvent trySendAnother = new SimulationEvent(model.time, this, EventType.SendPacket);
                model.scheduler.Add(trySendAnother);
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
        public static Dictionary<int, Process> routingIndexToDevice = new Dictionary<int, Process>(); 
        public static Dictionary<int, int> deviceIndexToRoutingIndex = new Dictionary<int, int>();
        public static List<Router> getRouters()
        {
            List<Router> routers = new List<Router>();
            Router router = new Router(1, 1, routingIndex);

            Link link = new Link(1, 2, 5);
            router.addLink(link);
            link = new Link(2, 3, 3);
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

            Link link = new Link(3, 1, 5);
            computer.addLink(link);

            computers.Add(computer);
            routingIndexToDevice.Add(routingIndex, computer);
            deviceIndexToRoutingIndex.Add(computer.ID, routingIndex);
            routingIndex += 1;

            computer = new Computer(3, routingIndex);

            link = new Link(4, 1, 3);
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
                computers[from].addPacket(packet);
                SimulationEvent simEvent = new SimulationEvent(when, computers[from], EventType.SendPacket);
                scheduler.Add(simEvent);

                Debug.WriteLine($"Created packet with id {i + 1} from computer {computers[from].ID} to computer {computers[to].ID} at time {when}");

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
