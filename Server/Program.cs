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
        private static Logger Log;
		// Timer to send game state
		private static System.Timers.Timer sendGameStateTimer;
		private static ElementId[] lobbyUnpackingArr = new ElementId[]{ ElementId.ReadyElement };
		private static ElementId[] unpackingArr = new ElementId[]{ ElementId.PositionElement };

		public static void Main (string[] args)
		{
            Log = Logger.Instance;
            Log.D("Server started.");
			// Create a list of elements to send. Using the same list for unreliable and reliable
			List<UpdateElement> elements = new List<UpdateElement> ();
			elements.Add (new HealthElement (15, 6));

			// Create a UDPSocket
			UDPSocket socket = new UDPSocket (10);
			// Bind the socket. Address must be in network byte order
			socket.Bind ((ushort)System.Net.IPAddress.HostToNetworkOrder ((short)8000));

			// Create a ServerStateMessageBridge to use later
			ServerStateMessageBridge bridge = new ServerStateMessageBridge ();

			// Create Game State ? 
			State state = State.Instance;
            

			LobbyState(socket, state, bridge);

            Log.Dispose();
		}

        private static void LobbyState(UDPSocket socket, State state, ServerStateMessageBridge bridge)
        {
            while (true)
            {
                Console.WriteLine("Waiting for packet in lobby state");
                Packet packet = socket.Receive();
                switch (ReliableUDPConnection.GetPacketType(packet))
                {
                    case PacketType.HeartbeatPacket:
                        //TODO Timeout stuff
                        Console.WriteLine("Got heartbeat packet");
                        int clientId = ReliableUDPConnection.GetPlayerID(packet);
                        UnpackedPacket unpacked = state.ClientManager.Connections[clientId].Connection.ProcessPacket(packet, lobbyUnpackingArr);
                        foreach (var element in unpacked.UnreliableElements)
                        {
                            element.UpdateState(bridge);
                        }
                        foreach (var element in unpacked.ReliableElements)
                        {
                            element.UpdateState(bridge);
                        }

                        break;


                    case PacketType.RequestPacket:
                        Console.WriteLine("Got request packet");
                        // TODO Catch exception thrown by AddConnection
                        int newClient = state.ClientManager.AddConnection(socket.LastReceivedFrom);
                        socket.Send(ReliableUDPConnection.CreateConfirmationPacket(newClient), state.ClientManager.Connections[newClient].Destination);
                        Console.WriteLine("Sent confirmation packet to client " + newClient);
                        state.GameState.AddPlayer();

                        break;
                    default:
                        Console.WriteLine("Got unexpected packet type, discarding");
                        break;
                }
                //TODO Check for timeouts

                //TODO If all players ready start game send start packet and go to gamestate.
                Console.WriteLine("Checking if all players ready");

                bool allReady = true;
                for (int i = 0; i < state.ClientManager.CountCurrConnections; i++)
                {
                    PlayerConnection client = state.ClientManager.Connections[i];
                    Console.WriteLine("Client {0}, {1}", i, client.Ready);
                    allReady &= client.Ready; //bitwise operater to check that every connection is ready
                }
                if (allReady)
                {
                    Console.WriteLine("All are ready sending startgame packet");
                    List<UpdateElement> readyElement = new List<UpdateElement>();
                    readyElement.Add(new GameStartElement(state.ClientManager.CountCurrConnections));
                    for (int i = 0; i < state.ClientManager.CountCurrConnections; i++)
                    {
                        PlayerConnection client = state.ClientManager.Connections[i];
                        Packet startPacket = client.Connection.CreatePacket(new List<UpdateElement>(), readyElement, PacketType.HeartbeatPacket);
                        socket.Send(startPacket, client.Destination);
                    }
                    
                    //wait for each client to start sending gameplay packets, which indicates
                    //that each client has received the gamestart Message
                    bool allSendGame = false;
                    int clientId;
                    while (!allSendGame)
                    {
                        packet = socket.Receive();
                        switch (ReliableUDPConnection.GetPacketType(packet))
                        {
                            case PacketType.GameplayPacket:
                                clientId = ReliableUDPConnection.GetPlayerID(packet);
                                state.ClientManager.Connections[clientId].startedGame = true;
                                allSendGame = true;
                                for (int i = 0; i < state.ClientManager.CountCurrConnections; i++)
                                {
                                    PlayerConnection client = state.ClientManager.Connections[i];
                                    Console.WriteLine("Client {0}, {1} sent a game packet while server in lobby", i, client.startedGame);
                                    allSendGame &= client.startedGame; //bitwise operater to check that every connection is ready
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    GameState(socket, state, bridge);



                }
                else
                {
                    Console.WriteLine("All players not ready");
                }
            }
        }


		private static void GameState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
		{

			// Fire Timer.Elapsed event every 1/30th second (sending Game State at 30 fps)
			StartGameStateTimer (socket, state); 

			while (true) {
				Console.WriteLine ("Waiting for packet in game state");
				Packet packet = socket.Receive ();
				if (ReliableUDPConnection.GetPacketType (packet) != PacketType.GameplayPacket) {
					continue;
				}
				int clientId = ReliableUDPConnection.GetPlayerID (packet);

				//TODO Catch exceptions thrown by ProcessPacket. Possibly move processing of packet to threadpool?
				UnpackedPacket unpacked = state.ClientManager.Connections [clientId].Connection.ProcessPacket (packet, unpackingArr);
				ThreadPool.QueueUserWorkItem (ProcessIncomingPacket, unpacked);
			}
		}

		static void ProcessIncomingPacket (Object packetInfo)
		{
			UnpackedPacket up = (UnpackedPacket)packetInfo; //may not work if not serializable, will have to look into that
			foreach (var element in up.UnreliableElements) {
				element.UpdateState (new ServerStateMessageBridge ()); //maybe should also keep a single bridge object in state instead of making a new one every time?
			}
			foreach (var element in up.ReliableElements) {
				element.UpdateState (new ServerStateMessageBridge ()); //maybe should also keep a single bridge object in state instead of making a new one every time?
			}



		}

		private static void StartGameStateTimer (UDPSocket socket, State state)
		{
			// Create Timer with a 1/30 second interval
			sendGameStateTimer = new System.Timers.Timer (33);

			// Set func on timer event (autoreset for continuous sending)
			sendGameStateTimer.Elapsed += (source, e) => SendGameState (source, e, socket, state);
			sendGameStateTimer.AutoReset = true;
			sendGameStateTimer.Enabled = true; 
		}


		private static void SendGameState (Object source, ElapsedEventArgs e, UDPSocket socket, State state)
		{
			Console.WriteLine ("Forming packet");
			try {
				ClientManager cm = state.ClientManager;
				GameState gs = state.GameState;
				List<UpdateElement> unreliable = new List<UpdateElement> ();
				List<UpdateElement> reliable = new List<UpdateElement> ();

				// Get new spawn elements from game state
				SpawnElement spawnElement;
				while(gs.SpawnQueue.TryDequeue(out spawnElement)){
					reliable.Add(spawnElement);
				}

				// Create unreliable elements that will be sent to all clients
				int actorId;
				for (int i = 0; i < cm.CountCurrConnections; i++) {
					actorId = cm.Connections [i].ActorId;
					unreliable.Add (new HealthElement (actorId, gs.GetHealth(actorId)));
					unreliable.Add (new MovementElement (actorId, gs.GetPosition (actorId).x, gs.GetPosition (actorId).z, gs.GetTargetPosition (actorId).x, gs.GetTargetPosition (actorId).z));
				}

				// Create and send packets to all clients
				for (int i = 0; i < cm.CountCurrConnections; i++) {
					Packet packet = state.ClientManager.Connections [i].Connection.CreatePacket (unreliable, reliable);
					socket.Send (packet, cm.Connections [i].Destination);
				}
			} catch (Exception ex) {
				//TODO Add expected exceptions. All exceptions being caught for debugging purposes. 
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
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