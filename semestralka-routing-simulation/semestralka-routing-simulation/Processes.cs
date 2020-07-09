// Discrete simulation of routing
// Jan Ruman, 1st year of study
// Summer term, 2019 / 2020
// NPRG031

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace semestralka_routing_simulation
{
    /// <summary>
    /// Class responsible for acting upon events.
    /// </summary>
    abstract class Process
    {
        public int ID;
        protected List<Packet> packetsOut;
        
        // It might've happened that during manipulation with packet a Timeout event has occured and packets own 
        // timeout field has been rewritten - because of this, we keep list of original timeouts of packets, so that
        // when packets timeout changes, we will know about it.
        protected List<ulong> packetsOutTimeouts;

        /// <summary>
        /// Act upon event.
        /// </summary>
        public abstract void HandleEvent(SimulationEvent simEvent, Model model);

        /// <summary>
        /// Add packet to set of outgoing packets.
        /// </summary>
        public void AddPacketOut(Packet packetOut)
        {
            packetsOut.Add(packetOut);
        }

        /// <summary>
        /// Add timeout to set of timeouts corresponding to outgoing packets.
        /// </summary>
        public void AddPacketOutTimeout(ulong timeout)
        {
            packetsOutTimeouts.Add(timeout);
        }
    }

    /// <summary>
    /// Process representing connection between two devices, controls packet transfers between them.
    /// </summary>
    /// <remarks>
    /// Links are one-directional, i.e. for two connected devices there are two links.
    /// </remarks>
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

                    // Check if packet didn't already time out.
                    if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                    {
                        SimulationEvent finishSending = new SimulationEvent(checked(model.time + (ulong)timeToTransfer), this, EventType.FinishSending);
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

                // Check if packet didn't already time out.
                if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                {
                    // Delegate packet to next hop device on its' path to destination computer
                    int destinationRoutingIndex = model.routing.deviceIndexToRoutingIndex[packet.destination];
                    int nextHopRoutingIndex = model.routing.routingTableSuccessors[sourceDevice.routingTableIndex, destinationRoutingIndex];
                    Device nextHopDevice = model.routing.routingIndexToDevice[nextHopRoutingIndex];

                    SimulationEvent receiveEvent = new SimulationEvent(model.time, nextHopDevice, EventType.ProcessPacket);
                    model.scheduler.Add(receiveEvent);
                    nextHopDevice.AddPacketIn(packet);
                    nextHopDevice.AddPacketInTimeout(packet.timeout);
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

    /// <summary>
    /// Process representing firewall on router, discards malicious packets.
    /// </summary>
    class Firewall : Process
    {
        public int timeToProcess;
        public bool processing;
        private readonly Router router;
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

                    // Check if packet didn't already time out.
                    if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                    {
                        processing = true;
                        SimulationEvent processingFinished = new SimulationEvent(checked(model.time + (ulong)timeToProcess), this, EventType.FinishProcessing);
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

                // Check if packet isn't malicious or didn't already time out.
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
                    router.AddPacketOut(packet);
                    router.AddPacketOutTimeout(packetTimeout);
                    model.scheduler.Add(sendPacket);
                }

                SimulationEvent tryProcessAnother = new SimulationEvent(model.time, this, EventType.ProcessPacket);
                model.scheduler.Add(tryProcessAnother);
            }
        }
    }

    /// <summary>
    /// Represents a device. 
    /// Aside from being a process, it also has a set of links, a routing index and set of incoming packets.
    /// </summary>
    abstract class Device : Process
    {
        public int routingTableIndex;
        protected List<Link> links;
        protected List<Packet> packetsIn;
        protected List<ulong> packetsInTimeouts;

        /// <summary>
        /// Add link to devices' set of links.
        /// </summary>
        public void AddLink(Link link)
        {
            links.Add(link);
        }

        /// <summary>
        /// Add packet to devices' set of incoming packets.
        /// </summary>
        public void AddPacketIn(Packet packetIn)
        {
            packetsIn.Add(packetIn);
        }

        /// <summary>
        /// Add timeout corresponding to incoming packet.
        /// </summary>
        public void AddPacketInTimeout(ulong timeout)
        {
            packetsInTimeouts.Add(timeout);
        }

        /// <summary>
        /// Looks for link connected to next hop device for given packet and returns it.
        /// </summary>
        protected Link GetLink(Packet packet, Model model)
        {
            int destinationRoutingIndex = model.routing.deviceIndexToRoutingIndex[packet.destination];
            int nextHopRoutingIndex = model.routing.routingTableSuccessors[routingTableIndex, destinationRoutingIndex];
            Device nextHopDevice = model.routing.routingIndexToDevice[nextHopRoutingIndex];

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

    /// <summary>
    /// Represents a physical router, prime responsibility is forwarding packets to next hop device on path to destination computer.
    /// </summary>
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

        /// <summary>
        /// Assign a firewall to router.
        /// </summary>
        public void SetFirewall(Firewall firewall)
        {
            this.firewall = firewall;
        }

        public override void HandleEvent(SimulationEvent simEvent, Model model)
        {
            if (simEvent.eventType == EventType.SendPacket)
            {
                // Delegate packet to corresponding link.
                Packet packet = packetsOut[0];
                packetsOut.RemoveAt(0);

                ulong packetTimeout = packetsOutTimeouts[0];
                packetsOutTimeouts.RemoveAt(0);

                Link link = GetLink(packet, model);

                SimulationEvent sendPacket = new SimulationEvent(model.time, link, EventType.SendPacket);
                link.AddPacketOut(packet);
                link.AddPacketOutTimeout(packetTimeout);
                model.scheduler.Add(sendPacket);
            }
            else if (simEvent.eventType == EventType.ProcessPacket)
            {
                if (packetsIn.Count > 0 && !processing)
                {
                    Packet packet = packetsIn[0];
                    ulong packetTimeout = packetsInTimeouts[0];

                    // Check if packet didn't already time out.
                    if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                    {
                        processing = true;
                        SimulationEvent processingFinished = new SimulationEvent(checked(model.time + (ulong)timeToProcess), this, EventType.FinishProcessing);
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

                // Check if packet didn't already time out.
                if (model.time <= packet.timeout && packetTimeout == packet.timeout)
                {
                    // If there is firewall associated with this router, delegate it this packet
                    if (firewall != null)
                    {
                        SimulationEvent processPacket = new SimulationEvent(model.time, firewall, EventType.ProcessPacket);
                        firewall.AddPacketOut(packet);
                        firewall.AddPacketOutTimeout(packetTimeout);
                        model.scheduler.Add(processPacket);
                    }
                    else
                    {
                        SimulationEvent sendPacket = new SimulationEvent(model.time, this, EventType.SendPacket);
                        AddPacketOut(packet);
                        AddPacketOutTimeout(packetTimeout);
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

    /// <summary>
    /// Represents physical computer, that is source and destination for packets in network.
    /// </summary>
    /// <remarks>
    /// Computer cannot forward packet, as opposed to router.
    /// </remarks>
    class Computer : Device
    {

        List<Packet> packetsSent;
        public bool malicious;

        public Computer(int id, int routingTableIndex, bool malicious)
        {
            this.ID = id;
            this.routingTableIndex = routingTableIndex;
            this.malicious = malicious;

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

                // Used to measure how long it took to deliver packet
                packet.timeFirstSent = model.time;

                SendPacket(packet, model);

                model.statistics.sentPackets += 1;
                if (packet.malicious)
                {
                    model.statistics.sentPacketsMalicious += 1;
                }

                Debug.WriteLine($"{model.time}: Computer {ID} sends packet {packet.ID}");
            }
            else if (simEvent.eventType == EventType.ProcessPacket)
            {
                Packet packet = packetsIn[0];
                packetsIn.RemoveAt(0);

                ulong packetTimeout = packetsInTimeouts[0];
                packetsInTimeouts.RemoveAt(0);

                // Check if packet didn't already time out.
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
                    // If the packet isn't delivered in time and hasn't been sent too many times, send it again.
                    if (packet.attemptNumber < model.timeoutAttempts)
                    {
                        SendPacket(packet, model);
                        packet.attemptNumber += 1;
                    }
                    else
                    {
                        Debug.WriteLine($"Packet {packet.ID} from computer {ID} couldn't be delivered in {model.timeoutAttempts} attempts");
                    }
                }

            }
        }

        /// <summary>
        /// Prepare packet, delegate it to proper link and setup timeout.
        /// </summary>
        private void SendPacket(Packet packet, Model model)
        {
            // Update packets' timeout
            packet.timeout = checked(model.time + model.timeout);

            Link link = GetLink(packet, model);

            SimulationEvent sendPacket = new SimulationEvent(model.time, link, EventType.SendPacket);
            link.AddPacketOut(packet);
            link.AddPacketOutTimeout(packet.timeout);
            model.scheduler.Add(sendPacket);

            // Adding a single tick because timeout is inclusive (i.e. if packet arrives precisely in time of timeout,
            // it is still considered to be received and it shouldn't be resent).
            SimulationEvent resolveTimeout = new SimulationEvent(checked(packet.timeout + 1), this, EventType.Timeout);
            packetsSent.Add(packet);
            model.scheduler.Add(resolveTimeout);
        }
    }
}
