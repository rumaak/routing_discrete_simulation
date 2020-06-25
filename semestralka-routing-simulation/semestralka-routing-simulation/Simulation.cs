using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace semestralka_routing_simulation
{
    abstract class Process
    {
        public int ID;
        public int TimeToProcess;
        public abstract void HandleEvent(SimulationEvent simEvent);
    }
    class Router : Process
    {
        public override void HandleEvent(SimulationEvent simEvent)
        {

        }
    }

    class Computer : Process
    {
        public override void HandleEvent(SimulationEvent simEvent)
        {

        }
    }

    class Firewall : Process
    {
        public override void HandleEvent(SimulationEvent simEvent)
        {

        }
    }

    class SimulationEvent 
    {
        public ulong time;
        public Process process;

        public void execute()
        {
            process.HandleEvent(this);
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
        List<Router> routers;
        List<Computer> computers;
        List<Firewall> firewalls;

        // TODO add parameters with routers, computers, etc
        public Model(Scheduler scheduler)
        {
            // TODO generate packets from the very start
            time = 0;
        }
    }
    
    static class Simulation
    {
        public static void RunSimulation()
        {
            Debug.WriteLine("Simulation started");
            
            Scheduler scheduler = new Scheduler();

            // TODO pass arguments
            Model model = new Model(scheduler);
            
            SimulationEvent simEvent = scheduler.GetFirst();
            while (simEvent != null)
            {
                simEvent.execute();
                simEvent = scheduler.GetFirst();
            }
        }
    }
}
