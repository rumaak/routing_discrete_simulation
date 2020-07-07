using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace semestralka_routing_simulation
{
    abstract class Process
    {
        public int ID;
        protected List<Packet> packetsOut;
        protected List<ulong> packetsOutTimeouts;

        public abstract void HandleEvent(SimulationEvent simEvent, Model model);
        public void AddPacketOut(Packet packetOut)
        {
            packetsOut.Add(packetOut);
        }
        public void AddPacketOutTimeout(ulong timeout)
        {
            packetsOutTimeouts.Add(timeout);
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
                    router.AddPacketOut(packet);
                    router.AddPacketOutTimeout(packetTimeout);
                    model.scheduler.Add(sendPacket);
                }

                SimulationEvent tryProcessAnother = new SimulationEvent(model.time, this, EventType.ProcessPacket);
                model.scheduler.Add(tryProcessAnother);
            }
        }
    }

    abstract class Device : Process
    {
        public int routingTableIndex;
        protected List<Link> links;
        protected List<Packet> packetsIn;
        protected List<ulong> packetsInTimeouts;

        public void AddLink(Link link)
        {
            links.Add(link);
        }
        public void AddPacketIn(Packet packetIn)
        {
            packetsIn.Add(packetIn);
        }
        public void AddPacketInTimeout(ulong timeout)
        {
            packetsInTimeouts.Add(timeout);
        }
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

        public void SetFirewall(Firewall firewall)
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
        private void SendPacket(Packet packet, Model model)
        {
            packet.timeout = model.time + model.timeout;

            Link link = GetLink(packet, model);

            SimulationEvent sendPacket = new SimulationEvent(model.time, link, EventType.SendPacket);
            link.AddPacketOut(packet);
            link.AddPacketOutTimeout(packet.timeout);
            model.scheduler.Add(sendPacket);

            // Adding a single tick because timeout is inclusive (i.e. if packet arrives precisely in time of timeout,
            // it is still considered to be received and it shouldn't be resent)
            SimulationEvent resolveTimeout = new SimulationEvent(packet.timeout + 1, this, EventType.Timeout);
            packetsSent.Add(packet);
            model.scheduler.Add(resolveTimeout);
        }
    }
}
