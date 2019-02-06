using System;
using System.Threading;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;
using System.Collections.Generic;
using System.Collections.Concurrent;    //Thread-safe collections

namespace Server
{
    class MainClass
    {
        private static ConcurrentQueue<Packet> packets = new ConcurrentQueue<Packet>();     //used for read loop
        private static List<UpdateElement> elements = new List<UpdateElement>();            //used for process loop that goes together with library
        private static UDPSocket socket;
        private static ReliableUDPConnection connection;
        private static ServerStateMessageBridge bridge;

        //Program Start
        public static void Main(string[] args)
        {
            // Create a list of elements to send. Using the same list for unreliable and reliable
            elements = new List<UpdateElement>();
            elements.Add(new HealthElement(15, 6));

            // Create a UDPSocket
            socket = new UDPSocket();

            // Bind the socket. Address must be in network byte order
            socket.Bind((ushort)System.Net.IPAddress.HostToNetworkOrder((short)8000));

            // Create a ReliableUDPConnection
            connection = new ReliableUDPConnection();

            // Create a ServerStateMessageBridge to use later
            bridge = new ServerStateMessageBridge();

            ThreadStart readJob = new ThreadStart(ReadLoop);
            Thread readThread = new Thread(readJob);
            ThreadStart processJob = new ThreadStart(ProcessLoop);
            Thread processThread = new Thread(processJob);
            
            //ThreadStart unreliableJob = new ThreadStart(UnreliableLoop);
            //Thread unreliableThread = new Thread(unreliableJob);
            //ThreadStart reliableJob = new ThreadStart(ReliableLoop);
            //Thread reliableThread = new Thread(reliableJob);
            readThread.Start();
            processThread.Start();
            //unreliableThread.Start();
            //reliableThread.Start();

        }

        //Purpose: Only add packets into 
        private static void ReadLoop()
        {
            while (true)
            {
                // Receive a packet. Receive calls block
                Packet packet = socket.Receive();

                Console.WriteLine("Got packet.");
                packets.Enqueue(packet);

            }
        }

        //Purpose: Check queue and call responding unreliable reliable stuff
        private static void ProcessLoop()
        {
            Packet packet;
            while (true)
            {
                if (!packets.IsEmpty)
                {

                    if (!packets.TryDequeue(out packet))
                    {
                        Console.WriteLine("Dequeue error.");
                    }
                    else
                    {
                        // Unpack the packet using the ReliableUDPConnection
                        UnpackedPacket unpacked = connection.ProcessPacket(packet, new ElementId[] { ElementId.HealthElement });

                        // Iterate through the unreliable elements and call their UpdateState function.
                        foreach (var element in unpacked.UnreliableElements)
                        {
                            element.UpdateState(bridge);
                        }
                    }

                    //No Send loop yet so keeping these inside here.

                    Console.WriteLine("Sending response packet.");
                    // Create a new packet
                    packet = connection.CreatePacket(elements, elements);

                    // Send the packet
                    socket.Send(packet, socket.LastReceivedFrom);
                }

            }

        }

        //Purpose: Unreliable processing
        private static void UnreliableLoop() {

        }

        //Purpose: Reliable processing
        private static void ReliableLoop()
        {

        }
    }
}
