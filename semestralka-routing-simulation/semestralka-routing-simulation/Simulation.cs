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
        ProcessPacket,
        ReceivePacket
    }

    class Packet
    {
        int source, destination;
        int ID;
 
        public Packet(int source, int destination, int ID)
        {
            this.source = source;
            this.destination = destination;
            this.ID = ID;
        }
    }

    abstract class Process
    {
        public int ID;
        public int TimeToProcess;

        public abstract void HandleEvent(SimulationEvent simEvent, Model model);
    }
    
    class Router : Process
    {
        List<Packet> packets;
        public Router(int id, int timeToProcess)
        {
            this.ID = id;
            this.TimeToProcess = timeToProcess;
            packets = new List<Packet>();
        }
        public void addPacket(Packet packet)
        {
            packets.Add(packet);
        }
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {

        }
    }
    
    class Computer : Process
    {
        List<Packet> packets;
        public Computer(int id, int timeToProcess)
        {
            this.ID = id;
            this.TimeToProcess = timeToProcess;
            packets = new List<Packet>();
        }

        public void addPacket(Packet packet)
        {
            packets.Add(packet);
        }
        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            if (simEvent.eventType == EventType.SendPacket)
            {
                if (packets.Count > 0)
                {
                    Packet packet = packets[0];
                    packets.RemoveAt(0);

                    // TODO 

                    // SimulationEvent simEvent = new SimulationEvent();
                    // model.scheduler.Add()
                }
            }
        }
    }
    
    class Firewall : Process
    {
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

        public static List<Router> getRouters()
        {
            List<Router> routers = new List<Router>();
            routers.Add(new Router(1, 1));
            return routers;
        }

        public static List<Computer> getComputers()
        {
            List<Computer> computers = new List<Computer>();
            computers.Add(new Computer(2, 1));
            computers.Add(new Computer(3, 1));
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
                { 1, 2, 3 },
                { 2, 2, 1 },
                { 3, 1, 3 }
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
                simEvent.execute(model);
                simEvent = scheduler.GetFirst();
            }
        }
    }
}
