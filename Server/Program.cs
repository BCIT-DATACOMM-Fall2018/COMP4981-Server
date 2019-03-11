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

        private static ElementId[] unpackingArr = new ElementId[]
        {
            ElementId.PositionElement 
        };

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
			//ReliableUDPConnection connection = new ReliableUDPConnection (0);

			// Create a ServerStateMessageBridge to use later
			ServerStateMessageBridge bridge = new ServerStateMessageBridge ();


            //Create Game State ? 
            State state = State.Instance;
            

            // Fire Timer.Elapsed event every 1/30th second (sending Game State at 30 fps)
            SetGSTimer(socket, state); 
           

            
			while (true) {
				// Receive a packet. Receive calls block
				Packet packet = socket.Receive ();

				Console.WriteLine ("Got packet.");


                int clientId = ReliableUDPConnection.GetPlayerID(packet);
                ReliableUDPConnection connection = state.getClientManger().getAllPlayerConnections()[clientId].getConnection();
                // Unpack the packet using the ReliableUDPConnection

                if (ReliableUDPConnection.GetPacketType(packet) == PacketType.GameplayPacket)
                {
                    UnpackedPacket unpacked;
                    unpacked = connection.ProcessPacket(packet, unpackingArr);
                    ThreadPool.QueueUserWorkItem(processIncomingPacket, unpacked);

                }
                else
                {
                    //do logging here
                }
				
                
                //send unpacked packet to the threadpool

				
			}
        }

        static void processIncomingPacket(Object packetInfo)
        {
            UnpackedPacket up = (UnpackedPacket)packetInfo; //may not work if not serializable, will have to look into that
            foreach (var element in up.UnreliableElements)
            {
                element.UpdateState(new ServerStateMessageBridge()); //maybe should also keep a single bridge object in state instead of making a new one every time?
            }
            foreach (var element in up.ReliableElements)
            {
                element.UpdateState(new ServerStateMessageBridge()); //maybe should also keep a single bridge object in state instead of making a new one every time?
            }



        }

        private static void SetGSTimer(UDPSocket socket, State state)
        {
            // Create Timer with a 1/30 second interval
            sendGSTimer = new System.Timers.Timer(33);

            // Set func on timer event (autoreset for continuous sending)
            sendGSTimer.Elapsed += (source, e) => sendGameState(source, e, socket, state);
            sendGSTimer.AutoReset = true;
            sendGSTimer.Enabled = true; 
        }


        private static void sendGameState(Object source, ElapsedEventArgs e, UDPSocket socket, State state)
        {
            ClientManager cm = state.getClientManger();
            GameState gs = state.getGameState();
            List<UpdateElement> unreliable = new List<UpdateElement>();
            List<UpdateElement> reliable = new List<UpdateElement>();

            int clientId;
            int actorId;
            for (int i = 0; i < cm.getCountCurrConnections(); i++)
            {
                clientId = cm.getAllPlayerConnections()[i].getClientId();
                actorId = cm.getAllPlayerConnections()[i].getActorId();
                unreliable.Add(new HealthElement(clientId, actorId));
                unreliable.Add(new MovementElement(actorId, gs.getPosition(actorId)[0], gs.getPosition(actorId)[1], gs.getTargetPosition(actorId)[0], gs.getTargetPosition(actorId)[1]));
            }



            //Send packet to connected socket
            //Console.WriteLine("Sending response packet.");
            for (int i = 0; i < cm.getCountCurrConnections(); i++)
            {
                Packet packet = state.getClientManger().getAllPlayerConnections()[i].getConnection().CreatePacket(unreliable, reliable);
                socket.Send(packet, cm.getAllPlayerConnections()[i].getDestination());
            }


        }
	}
}

//SENDING UNRELIABLE
// numPlayers * (HEALTH, POSTION) , RELIABLE ELEMENTS (WILL BE PROCESSED IN THE ORDER WE ADD THEM)

//STARTING GAME
//send gameStart packet
//send packet with spawn info
//no longer receive new connections requests


//