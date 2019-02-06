using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Create a list of elements to send. Using the same list for unreliable and reliable
			List<UpdateElement> elements = new List<UpdateElement>();
			elements.Add(new HealthElement(15,6));

			// Create a UDPSocket
			UDPSocket socket = new UDPSocket ();
			// Bind the socket. Address must be in network byte order
			socket.Bind ((ushort)System.Net.IPAddress.HostToNetworkOrder ((short)8000));

			// Create a ReliableUDPConnection
			ReliableUDPConnection connection = new ReliableUDPConnection ();

			// Create a ServerStateMessageBridge to use later
			ServerStateMessageBridge bridge = new ServerStateMessageBridge ();

            

			while (true) {
				// Receive a packet. Receive calls block
				Packet packet = socket.Receive ();

				Console.WriteLine ("Got packet.");

				// Unpack the packet using the ReliableUDPConnection
				UnpackedPacket unpacked = connection.ProcessPacket (packet, new ElementId[] {ElementId.HealthElement});
                
                //send unpacked packet to the threadpool
                ThreadPool.QueueUserWorkItem(processIncomingPacket, unpacked);

				Console.WriteLine ("Sending response packet.");
				// Create a new packet
				packet = connection.CreatePacket (elements, elements);

				// Send the packet
				socket.Send (packet, socket.LastReceivedFrom);
			}
        }

        static void processIncomingPacket(Object packetInfo)
        {
            UnpackedPacket up = (UnpackedPacket)packetInfo; //may not work if not serializable, will have to look into that
            foreach (var element in up.UnreliableElements)
            {
                element.UpdateState(new ServerStateMessageBridge()); //maybe should also keep a single bridge object in state instead of making a new one every time?
            }

        }
	}
}

