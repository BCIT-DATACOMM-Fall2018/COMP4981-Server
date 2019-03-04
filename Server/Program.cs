using System;
using System.Timers; 
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;
using System.Collections.Generic;
using System.Threading;
using GameStateComponents;


namespace Server
{
	class MainClass
	{
        // Timer to send game state
        private static System.Timers.Timer sendGSTimer;

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


            //Create Game State ? 
            GameState gameState = GameState.Instance; 


            // Fire Timer.Elapsed event every 1/30th second (sending Game State at 30 fps)
            SetGSTimer(socket, gameState, connection); 
           

			while (true) {
				// Receive a packet. Receive calls block
				Packet packet = socket.Receive ();

				Console.WriteLine ("Got packet.");

				// Unpack the packet using the ReliableUDPConnection
				UnpackedPacket unpacked = connection.ProcessPacket (packet, new ElementId[] {ElementId.HealthElement});
                
                //send unpacked packet to the threadpool
                ThreadPool.QueueUserWorkItem(processIncomingPacket, unpacked);

				
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

        private static void SetGSTimer(UDPSocket socket, GameState gs, ReliableUDPConnection connect)
        {
            // Create Timer with a 1/30 second interval
            sendGSTimer = new System.Timers.Timer(33);

            // Set func on timer event (autoreset for continuous sending)
            sendGSTimer.Elapsed += (source, e) => sendGameState(source, e, socket, gs, connect);
            sendGSTimer.AutoReset = true;
            sendGSTimer.Enabled = true; 
        }


        private static void sendGameState(Object source, ElapsedEventArgs e, UDPSocket socket, GameState gs, ReliableUDPConnection connect)
        {
            List<UpdateElement> el = new List<UpdateElement>();
            el.Add(new HealthElement(0, gs.getHealth(0)));

            Packet packet = connect.CreatePacket(el, el);

            //Send packet to connected socket
            Console.WriteLine("Sending response packet.");
            socket.Send(packet, socket.LastReceivedFrom); 


        }
	}
}

