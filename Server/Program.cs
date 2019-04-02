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
        private static System.Timers.Timer sendGameStateTimer;
        private static System.Timers.Timer sendHeartBeatPing;
        // Send Heart Beat Ping to all connected clients every 5 seconds (lobby state)
        public const int HeartBeatInterval = 100;

        private static ElementId[] lobbyUnpackingArr = new ElementId[]{ ElementId.ReadyElement };
        private static ElementId[] unpackingArr = new ElementId[]{ ElementId.PositionElement };
        private static State state;
        private static Logger Log;

        public static void Main (string[] args)
        {
            Logger Log = Logger.Instance;
            Log.D("Server started.");
            // Create a list of elements to send. Using the same list for unreliable and reliable
            List<UpdateElement> elements = new List<UpdateElement> ();
            elements.Add (new HealthElement (15, 6));

            // Create a UDPSocket
            UDPSocket socket = new UDPSocket (0);
            // Bind the socket. Address must be in network byte order
            socket.Bind ((ushort)System.Net.IPAddress.HostToNetworkOrder ((short)8000));

            // Create a ServerStateMessageBridge to use later

            // Create Game State ? 
            while (true) {

                state = new State();
                ServerStateMessageBridge bridge = new ServerStateMessageBridge (state);

                LobbyState (socket, state, bridge);
            }
        }


        private static void StartHeartBeat (UDPSocket socket, State state)
        {
            // Create Timer with a 1/30 second interval
            sendHeartBeatPing = new System.Timers.Timer (HeartBeatInterval);

            // Set func on timer event (autoreset for continuous sending)
            sendHeartBeatPing.Elapsed += (source, e) => SendHeartBeat (source, e, socket, state);
            sendHeartBeatPing.AutoReset = true;
            sendHeartBeatPing.Enabled = true; 
        }

        private static void SendHeartBeat (Object source, ElapsedEventArgs e, UDPSocket socket, State state)
        {
            List<UpdateElement> unreliable = new List<UpdateElement>();
            List<UpdateElement> reliable = new List<UpdateElement>();
            Console.WriteLine("Count current connections {0}", state.ClientManager.CountCurrConnections);

            List<LobbyStatusElement.PlayerInfo> listPI = new List<LobbyStatusElement.PlayerInfo>();
            try
            {
                for (int i = 0; i < state.ClientManager.CountCurrConnections; i++)
                {
                    Console.WriteLine("Creating LobbyInfo packet");
                    PlayerConnection client = state.ClientManager.Connections[i];
                    listPI.Add(new LobbyStatusElement.PlayerInfo(client.ActorId, client.Name, client.Team, client.Ready));

                }

                for (int i = 0; i < state.ClientManager.CountCurrConnections; i++)
                {
                    Console.WriteLine("Sending Lobby update to client");
                    PlayerConnection client = state.ClientManager.Connections[i];
                    unreliable.Add(new LobbyStatusElement(listPI));
                    Packet startPacket = client.Connection.CreatePacket(unreliable, reliable, PacketType.HeartbeatPacket);
                    socket.Send(startPacket, client.Destination);
                    Console.WriteLine("Sent lobby update to client");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }
            
        private static void LobbyState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
        {
            // TODO
            // Send heartbeat ping to connected clients every Heart Beat Ping Interval (5 seconds)

            StartHeartBeat(socket, state);        


            while (state.TimesEndGameSent < 80) {

                Console.WriteLine ("Waiting for packet in lobby state");
                Packet packet = socket.Receive ();
                switch (ReliableUDPConnection.GetPacketType (packet)) {
                case PacketType.HeartbeatPacket:
                        //TODO Timeout stuff
                    Console.WriteLine ("Got heartbeat packet");
                    int clientId = ReliableUDPConnection.GetPlayerID (packet);
                    UnpackedPacket unpacked = state.ClientManager.Connections [clientId].Connection.ProcessPacket (packet, lobbyUnpackingArr);
                    foreach (var element in unpacked.UnreliableElements) {
                        element.UpdateState (bridge);
                    }
                    foreach (var element in unpacked.ReliableElements) {
                        element.UpdateState (bridge);
                    }

                    break;


                case PacketType.RequestPacket:
                    Console.WriteLine ("Got request packet");
                        // TODO Catch exception thrown by AddConnection
                    string name = ReliableUDPConnection.GetClientNameFromRequestPacket (packet);
                    int newClient = state.ClientManager.AddConnection (socket.LastReceivedFrom, name);
                    socket.Send (ReliableUDPConnection.CreateConfirmationPacket (newClient), state.ClientManager.Connections [newClient].Destination);
                    Console.WriteLine ("Sent confirmation packet to client " + newClient + " with name " + state.ClientManager.Connections [newClient].Name);

                    break;
                default:
                    Console.WriteLine ("Got unexpected packet type, discarding");
                    break;
                }
                //TODO Check for timeouts

                //TODO If all players ready start game send start packet and go to gamestate.
                Console.WriteLine ("Checking if all players ready");

                bool allReady = state.ClientManager.CountCurrConnections > 0;
                for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
                    PlayerConnection client = state.ClientManager.Connections [i];
                    Console.WriteLine ("Client {0}, {1}", i, client.Ready);
                    allReady &= client.Ready; //bitwise operater to check that every connection is ready
                }
                Console.WriteLine ("Current connections {0}", state.ClientManager.CountCurrConnections);

                // Force game to wait until 2 players have connected
                if (state.ClientManager.CountCurrConnections < 2) {
                    //allReady = false;
                }

                if (allReady) {
                    Console.WriteLine ("All are ready sending startgame packet");
                    List<UpdateElement> unreliableElements = new List<UpdateElement> ();
                    List<UpdateElement> reliableElements = new List<UpdateElement> ();
                    reliableElements.Add (new GameStartElement (state.ClientManager.CountCurrConnections));
                    var playerInfo = new List<LobbyStatusElement.PlayerInfo> ();
                    for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
                        playerInfo.Add (new LobbyStatusElement.PlayerInfo (state.ClientManager.Connections [i].ClientId,
                            state.ClientManager.Connections [i].Name,
                            state.ClientManager.Connections [i].Team,
                            state.ClientManager.Connections [i].Ready));
                    }
                    unreliableElements.Add(new LobbyStatusElement(playerInfo));

                    for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
                        PlayerConnection client = state.ClientManager.Connections [i];
                        Packet startPacket = client.Connection.CreatePacket (unreliableElements, reliableElements, PacketType.HeartbeatPacket);
                        socket.Send (startPacket, client.Destination);
                    }

                    //wait for each client to start sending gameplay packets, which indicates
                    //that each client has received the gamestart Message
                    bool allSendGame = false;
                    int clientId;
                    while (!allSendGame) {
                        packet = socket.Receive ();
                        switch (ReliableUDPConnection.GetPacketType (packet)) {
                        case PacketType.GameplayPacket:
                            clientId = ReliableUDPConnection.GetPlayerID (packet);
                            state.ClientManager.Connections [clientId].startedGame = true;
                            allSendGame = true;
                            Console.WriteLine ("Received packet from {0}", clientId);
                            for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
                                PlayerConnection client = state.ClientManager.Connections [i];
                                Console.WriteLine ("Client {0}, {1} sent a game packet while server in lobby", i, client.startedGame);
                                allSendGame &= client.startedGame; //bitwise operater to check that every connection is ready
                            }
                            break;
                        default:
                            break;
                        }
                    }
                    sendHeartBeatPing.Enabled = false;
                    GameState (socket, state, bridge);
                    return;


                } else {
                    Console.WriteLine ("All players not ready");
                }
            }
        }


        private static void GameState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
        {

            // Add all players to the game state
            for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
                state.GameState.AddPlayer (state.ClientManager.Connections [i].Team);
            }

            //give each player their starting ability
            for (int i = 0; i < state.GameState.CreatedPlayersCount; i++)
            {
                Player player = (Player)state.GameState.actors[state.ClientManager.Connections[i].ActorId];
                int newSkillId = AbilityEffects.ReturnRandomAbilityId(player);
                state.GameState.OutgoingReliableElements.Enqueue(new AbilityAssignmentElement(player.ActorId, newSkillId));
            }

            //spawn test tower at 100,100
            state.GameState.AddTower(new GameUtility.Coordinate(150, 150));

            // Fire Timer.Elapsed event every 1/30th second (sending Game State at 30 fps)
            StartGameStateTimer (socket, state);

            
            //Timer for keeping track of the game progress
            state.GameState.StartGamePlayTimer();

            while (state.TimesEndGameSent < 80) {
                if (!state.GameOver) {
                    //Console.WriteLine ("Waiting for packet in game state");
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
        }

        static void ProcessIncomingPacket (Object packetInfo)
        {
            UnpackedPacket up = (UnpackedPacket)packetInfo; //may not work if not serializable, will have to look into that
            foreach (var element in up.UnreliableElements) {
                element.UpdateState (new ServerStateMessageBridge (state)); //maybe should also keep a single bridge object in state instead of making a new one every time?
            }
            foreach (var element in up.ReliableElements) {
                element.UpdateState (new ServerStateMessageBridge (state)); //maybe should also keep a single bridge object in state instead of making a new one every time?
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

        //added for cleaning up gamestateTimer, also clean up the game state timer
        private static void EndGameStateTimer() {
            sendGameStateTimer.Dispose();
            state.GameState.EndGamePlayTimer(); // used for cleaning up timer
        }

        private static void SendGameState (Object source, ElapsedEventArgs e, UDPSocket socket, State state)
        {
            //Console.WriteLine ("Forming packet");
            try {
                ClientManager cm = state.ClientManager;
                GameState gs = state.GameState;
                List<UpdateElement> unreliable = new List<UpdateElement> ();
                List<UpdateElement> reliable = new List<UpdateElement> ();

                // Get new update elements from game state
                UpdateElement updateElement;
                while (gs.OutgoingReliableElements.TryDequeue (out updateElement)) {
                    reliable.Add (updateElement);
                }

                gs.TickAllActors (state);
                gs.CollisionBuffer.DecrementTTL ();
                if(state.GameOver || gs.CheckWinCondition()){
                    state.GameOver = true;
                    if(state.TimesEndGameSent++ < 80){
                        //TODO end the game
                        reliable.Add(new GameEndElement(1));
                        Console.WriteLine("Game is over");
                    } else {
                        sendGameStateTimer.Enabled = false;
                    }
                }
                    
                // Create unreliable elements that will be sent to all clients
                int actorId;
                for (int i = 0; i < gs.CreatedPlayersCount; i++) {
                    actorId = cm.Connections [i].ActorId;
                    unreliable.Add (new HealthElement (actorId, gs.GetHealth (actorId)));
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