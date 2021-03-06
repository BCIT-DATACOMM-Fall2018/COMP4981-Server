﻿/*---------------------------------------------------------------------------------------
--	SOURCE FILE:		Program.cs - server of the game
--
--	PROGRAM:		    program
--  Allan Hsu
--	Mar 28, 2019 added Logger to server, cleanup for timer
--	Apr 03, 2019 changed console.log to logging, removed unimportant logging		
---------------------------------------------------------------------------------------*/
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
    /// ----------------------------------------------
    /// Class: 		Main - Drives the program
    /// 
    /// PROGRAM:	Server
    ///
    /// 
    /// FUNCTIONS:	public static void Main (string[] args)
    ///             private static void StartHeartBeat (UDPSocket socket, State state)
    ///             private static void SendHeartBeat (Object source, ElapsedEventArgs e, UDPSocket socket, State state)
    ///             private static void LobbyState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
    ///             private static void GameState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
    ///             static void ProcessIncomingPacket (Object packetInfo)
    ///             private static void StartGameStateTimer (UDPSocket socket, State state)
    ///             private static void EndGameStateTimer ()
    ///             private static void SendGameState (Object source, ElapsedEventArgs e, UDPSocket socket, State state)
    /// 
    /// DATE: 		Febuary 5, 2019
    ///
    /// REVISIONS: 
    ///
    /// DESIGNER: 	Kieran Lee, Cameron Roberts
    ///
    /// PROGRAMMER: Kieran Lee, Cameron Roberts, Wayne Huang, Segal Au, Allan Hsu
    ///
    /// NOTES:		Entry point and driver for the program	
    ///				
    /// ----------------------------------------------
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
		private static Logger Log = Logger.Instance;

        /// ----------------------------------------------
        /// FUNCTION:		Main
        /// 
        /// DATE:			Feburaury 5, 2019
        /// 
        /// REVISIONS:		
        /// 
        /// DESIGNER: 	Kieran Lee, Cameron Roberts
        ///
        /// PROGRAMMER: Kieran Lee, Cameron Roberts, Wayne Huang, Segal Au, Allan Hsu
        /// 
        /// INTERFACE: 		public static void Main (string[] args)
        /// 
        /// RETURNS: 		void
        /// 
        /// NOTES:		  	entry point and drives the program
        /// ----------------------------------------------
        public static void Main (string[] args)
		{
			Log.D ("Server started.");

			// Create a list of elements to send. Using the same list for unreliable and reliable

			// Create a UDPSocket
			UDPSocket socket = new UDPSocket (4);
			// Bind the socket. Address must be in network byte order
			try{
				socket.Bind ((ushort)System.Net.IPAddress.HostToNetworkOrder ((short)8000));
			} catch (System.IO.IOException e) {
				Console.WriteLine("Error binding port. Please retry.");
				Environment.Exit(0);
			}

			// Create a ServerStateMessageBridge to use later

			// Create Game State ?
			while (true) {

				state = new State ();
				ServerStateMessageBridge bridge = new ServerStateMessageBridge (state);

				LobbyState (socket, state, bridge);
			}
		}


		/*================================================================
		Author : Segal Au

		Date : April 9, 2019

		Function: StartHeartBeat

		Params :
			UDPSocket socket
				- Socket to send heart beat Packet
			State state
				- Game state and client manager

		Purpose :
			Starts timer to periodically send heart beat ping while in lobby state.
			Notifies client and server that connection is still valid until
			game start

		=================================================================*/

		private static void StartHeartBeat (UDPSocket socket, State state)
		{
            Log.V("Start sending heart beat.");
            // Create Timer with a 1/30 second interval
            sendHeartBeatPing = new System.Timers.Timer (HeartBeatInterval);

			// Set func on timer event (autoreset for continuous sending)
			sendHeartBeatPing.Elapsed += (source, e) => SendHeartBeat (source, e, socket, state);
			sendHeartBeatPing.AutoReset = true;
			sendHeartBeatPing.Enabled = true;
		}

		/*================================================================
		Author : Segal Au

		Date : April 9, 2019

		Function: SendHeartBeat

		Params :
			Object source
				- object source
			ElapsedEventArgs e
				- event arguments
			UDPSocket Socket
				- Socket to send heart beat Packet
			State state
				- Game state and client manager

		Purpose :
			Sends heart beat packet function (triggered by timer).

		=================================================================*/
		private static void SendHeartBeat (Object source, ElapsedEventArgs e, UDPSocket socket, State state)
		{
			List<UpdateElement> unreliable = new List<UpdateElement> ();
			List<UpdateElement> reliable = new List<UpdateElement> ();
			Console.WriteLine ("Count current connections {0}", state.ClientManager.CountCurrConnections);


			List<LobbyStatusElement.PlayerInfo> listPI = new List<LobbyStatusElement.PlayerInfo> ();
			try {
				for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
					if (state.ClientManager.Connections [i] == null) {
						continue;
					}
					Console.WriteLine ("Creating LobbyInfo packet");
					PlayerConnection client = state.ClientManager.Connections [i];
					listPI.Add (new LobbyStatusElement.PlayerInfo (client.ActorId, client.Name, client.Team, client.Ready));

				}

				for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
					if (state.ClientManager.Connections [i] == null) {
						continue;
					}
					Console.WriteLine ("Sending Lobby update to client");
					PlayerConnection client = state.ClientManager.Connections [i];
					if (client.Disconnected ()) {
						state.ClientManager.Connections [i] = null;
					}
					unreliable.Add (new LobbyStatusElement (listPI));
					Packet startPacket = client.Connection.CreatePacket (unreliable, reliable, PacketType.HeartbeatPacket);
					socket.Send (startPacket, client.Destination);
					Console.WriteLine ("Sent lobby update to client");
				}
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
			}

		}


      
        /// ----------------------------------------------
        /// FUNCTION:		LobbyState
        /// 
        /// DATE:			April 2 , 2019
        /// 
        /// REVISIONS:		
        /// 
        /// DESIGNER:  Cameron Roberts, Kieran Lee
        ///
        /// PROGRAMMER: Cameron Roberts, Kieran Lee, Segal Au
        /// 
        /// INTERFACE: 		private static void LobbyState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
        ///                 socket: socket to send and receive from
        ///                 state:  gives access to client manager
        ///                 ServerStateMessageBridge: allows packets to be processed properly
        /// 
        /// RETURNS: 		void
        /// 
        /// NOTES:		  	function that server remains in until lobyy is left and game is started.
        ///                 Expect request packets and hearbeat packets, discard all else.
        ///                 waits until all players are ready, then sends start game message.
        ///                 does not move to game state until all clients have started sending game packets
        /// ----------------------------------------------
        private static void LobbyState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
		{
			StartHeartBeat (socket, state);
			while (state.TimesEndGameSent < 80) {

				Packet packet;
				try {
					Console.WriteLine ("Waiting for packet in lobby state");
					packet = socket.Receive ();
				} catch (TimeoutException e){
					Console.WriteLine ("Timeout");
					continue;
				}
				switch (ReliableUDPConnection.GetPacketType (packet)) {
				case PacketType.HeartbeatPacket:
					Console.WriteLine ("Got heartbeat packet");
					int clientId = ReliableUDPConnection.GetPlayerID (packet);
					state.ClientManager.Connections [clientId].MarkPacketReceive ();
					if (state.ClientManager.Connections [clientId] == null) {
						continue;
					}
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
					try {
						string name = ReliableUDPConnection.GetClientNameFromRequestPacket (packet);
						int newClient = state.ClientManager.AddConnection (socket.LastReceivedFrom, name);
						socket.Send (ReliableUDPConnection.CreateConfirmationPacket (newClient), state.ClientManager.Connections [newClient].Destination);
						Console.WriteLine ("Sent confirmation packet to client " + newClient + " with name " + state.ClientManager.Connections [newClient].Name);
					} catch (OutOfMemoryException e) {

					}

					break;
				default:
					Log.V("Got unexpected packet type, discarding");
					break;
				}

				Console.WriteLine ("Checking if all players ready");
				//TODO If all players ready start game send start packet and go to gamestate.
				Log.V("Checking if all players are ready");

				bool allReady = state.ClientManager.CountCurrConnections > 0;
				for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
					if (state.ClientManager.Connections [i] == null) {
						continue;
					}
					PlayerConnection client = state.ClientManager.Connections [i];
					Log.V("Client " + i + ", " + client.Ready);
					allReady &= client.Ready; //bitwise operater to check that every connection is ready
				}
                Log.V("Current connections " + state.ClientManager.CountCurrConnections);


				if (allReady) {
					Log.D("All are ready sending startgame packet");
					List<UpdateElement> unreliableElements = new List<UpdateElement> ();
					List<UpdateElement> reliableElements = new List<UpdateElement> ();
					int players = 0;
					for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
						if (state.ClientManager.Connections [i] == null) {
							continue;
						}
						state.ClientManager.Connections [i].ActorId = state.GameState.AddPlayer (state.ClientManager.Connections [i].Team);
						players++;
					}

					reliableElements.Add (new GameStartElement (players));
					var playerInfo = new List<LobbyStatusElement.PlayerInfo> ();
					for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
						if (state.ClientManager.Connections [i] == null) {
							continue;
						}
						playerInfo.Add (new LobbyStatusElement.PlayerInfo (state.ClientManager.Connections [i].ClientId,
							state.ClientManager.Connections [i].Name,
							state.ClientManager.Connections [i].Team,
							state.ClientManager.Connections [i].Ready));
					}
					unreliableElements.Add (new LobbyStatusElement (playerInfo));

					for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
						if (state.ClientManager.Connections [i] == null) {
							continue;
						}
						PlayerConnection client = state.ClientManager.Connections [i];
						Packet startPacket = client.Connection.CreatePacket (unreliableElements, reliableElements, PacketType.HeartbeatPacket);
						socket.Send (startPacket, client.Destination);
					}

					//wait for each client to start sending gameplay packets, which indicates
					//that each client has received the gamestart Message
					bool allSendGame = false;
					int playerId;
					while (!allSendGame) {
						try{
							packet = socket.Receive ();
							switch (ReliableUDPConnection.GetPacketType (packet)) {
								case PacketType.GameplayPacket:
								playerId = ReliableUDPConnection.GetPlayerID (packet);
								state.ClientManager.FindClientByActorId(playerId).startedGame = true;
								allSendGame = true;
								Console.WriteLine ("Received packet from {0}", playerId);
								for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
									if (state.ClientManager.Connections [i] == null) {
										continue;
									}
									PlayerConnection client = state.ClientManager.Connections [i];
									Console.WriteLine ("Client {0}, {1} sent a game packet while server in lobby", i, client.startedGame);
									allSendGame &= client.startedGame; //bitwise operater to check that every connection is ready
								}
								break;
								default:
								break;
							}
						} catch (TimeoutException e) {
							continue;
						}
					}
					sendHeartBeatPing.Enabled = false;
					GameState (socket, state, bridge);
					return;


				} else {
					Log.V("All players not ready");
				}
			}
		}

        /// ----------------------------------------------
        /// FUNCTION:		GameState
        /// 
        /// DATE:			April 2 , 2019
        /// 
        /// REVISIONS:		
        /// 
        /// DESIGNER:  Cameron Roberts, Kieran Lee
        ///
        /// PROGRAMMER: Cameron Roberts, Kieran Lee
        /// 
        /// INTERFACE: 		private static void GameState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
        ///                 socket: socket to send and receive from
        ///                 state:  gives access to client manager
        ///                 ServerStateMessageBridge: allows packets to be processed properly
        /// 
        /// RETURNS: 		void
        /// 
        /// NOTES:		  	function that server remains while game is in progress
        ///                 Expect gameplay packets, discard all else.
        ///                 does initial game setup then
        ///                 receives and processes packets
        /// ----------------------------------------------
		private static void GameState (UDPSocket socket, State state, ServerStateMessageBridge bridge)
		{
			state.GameState.CollisionBuffer.requiredValidity = Math.Max ((int)(state.ClientManager.CountCurrConnections * 0.8), 1);
			Console.WriteLine ("set required validity to {0}", state.GameState.CollisionBuffer.requiredValidity);

			//give each player their starting ability
			for (int i = 0; i < state.GameState.CreatedPlayersCount; i++) {
				Player player = (Player)state.GameState.actors [i];
				int newSkillId = AbilityEffects.ReturnRandomAbilityId (player);
				state.GameState.OutgoingReliableElements.Enqueue (new AbilityAssignmentElement (i, newSkillId));
				player.AddAbility(1, (AbilityType)newSkillId);
			}

			//spawn test tower at 100,100
			state.GameState.AddTower (new GameUtility.Coordinate (355, 187));
			state.GameState.AddTower (new GameUtility.Coordinate (356, 302));
			state.GameState.AddTower (new GameUtility.Coordinate (150, 312));
			state.GameState.AddTower (new GameUtility.Coordinate (152, 193));

			// Fire Timer.Elapsed event every 1/30th second (sending Game State at 30 fps)
			StartGameStateTimer (socket, state);


			//Timer for keeping track of the game progress
			state.GameState.StartGamePlayTimer ();

			while (state.TimesEndGameSent < 80) {
				if (!state.GameOver) {
					try {
						Packet packet = socket.Receive ();
						if (ReliableUDPConnection.GetPacketType (packet) != PacketType.GameplayPacket) {
							continue;
						}
						int actorId = ReliableUDPConnection.GetPlayerID (packet);
						state.ClientManager.FindClientByActorId (actorId).MarkPacketReceive ();

						UnpackedPacket unpacked = state.ClientManager.FindClientByActorId (actorId).Connection.ProcessPacket (packet, unpackingArr);
						ThreadPool.QueueUserWorkItem (ProcessIncomingPacket, unpacked);
					} catch (Exception e) {
						Console.WriteLine (e);
						continue;
					}
				}
			}
		}

        /// ----------------------------------------------
        /// FUNCTION:		Process Incoming packet
        /// 
        /// DATE:			March 5 , 2019
        /// 
        /// REVISIONS:		
        /// 
        /// DESIGNER:  Cameron Roberts,
        ///
        /// PROGRAMMER: Kieran Lee
        /// 
        /// INTERFACE: 		static void ProcessIncomingPacket (Object packetInfo)
        /// 
        /// RETURNS: 		void
        /// 
        /// NOTES:		  	
        /// ----------------------------------------------
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

		/*================================================================
		Author : Segal Au

		Date : April 9, 2019

		Function: StartGameStateTimer

		Params :

			UDPSocket Socket
				- Socket to send heart beat Packet
			State state
				- Game state and client manager

		Purpose :
			Starts timer to periodically send game state packets (30 frames per second)

		=================================================================*/
		private static void StartGameStateTimer (UDPSocket socket, State state)
		{
            Log.V("Start total game timer and game state send timer");
			// Create Timer with a 1/30 second interval
			sendGameStateTimer = new System.Timers.Timer (33);

			// Set func on timer event (autoreset for continuous sending)
			sendGameStateTimer.Elapsed += (source, e) => SendGameState (source, e, socket, state);
			sendGameStateTimer.AutoReset = true;
			sendGameStateTimer.Enabled = true;
		}

        //added for cleaning up gamestateTimer, also clean up the game state timer
        private static void EndGameStateTimer() {
            Log.V("Disposing total game timer and game state send timer");
            sendGameStateTimer.Dispose();
            state.GameState.EndGamePlayTimer(); // used for cleaning up timer
        }

        /// ----------------------------------------------
        /// FUNCTION:		Send Game State
        /// 
        /// DATE:			March 12 , 2019
        /// 
        /// REVISIONS:		
        /// 
        /// DESIGNER:  Cameron Roberts, Kieran Lee
        ///
        /// PROGRAMMER: Kieran Lee
        /// 
        /// INTERFACE: 		private static void SendGameState (Object source, ElapsedEventArgs e, UDPSocket socket, State state)
        ///                 source: needed for execution on timer thread
        ///                 e: needed for execution on timer thread
        ///                 socket: socket for sending the game state on
        ///                 state: to access the game state and client manager
        /// 
        /// RETURNS: 		void
        /// 
        /// NOTES:		  	Takes reliable elements from reliable element queue, 
        ///                 generate unreliable elements from the game state
        ///                 
        /// ----------------------------------------------
        private static void SendGameState (Object source, ElapsedEventArgs e, UDPSocket socket, State state)
		{
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
				if (state.GameOver || gs.CheckWinCondition ()) {
					state.GameOver = true;
					if (state.TimesEndGameSent++ < 80) {
						//TODO end the game
						reliable.Add (new GameEndElement (gs.CheckWinningTeam ()));
						Console.WriteLine ("Game is over");
                        Log.D("Game is over");
					} else {
						sendGameStateTimer.Enabled = false;
					}
				}

				// Check if all players are not connected
				bool allDisconnected = true;
				for (int i = 0; i < state.ClientManager.CountCurrConnections; i++) {
					if (state.ClientManager.Connections [i] == null) {
						continue;
					}
					allDisconnected &= state.ClientManager.Connections [i].Disconnected ();
				}
				if (allDisconnected) {
					state.TimesEndGameSent = 80;
					state.GameOver = true;
					sendGameStateTimer.Enabled = false;
				}

				// Create unreliable elements that will be sent to all clients
				int actorId;
				for (int i = 0; i < gs.CreatedPlayersCount; i++) {
					actorId = cm.Connections [i].ActorId;
					unreliable.Add (new HealthElement (actorId, gs.GetHealth (actorId)));
					unreliable.Add (new MovementElement (actorId, gs.GetPosition (actorId).x, gs.GetPosition (actorId).z, gs.GetTargetPosition (actorId).x, gs.GetTargetPosition (actorId).z));
					unreliable.Add (new ExperienceElement(actorId, ((Player)gs.actors[actorId]).Experience));
				}

				var livesInfo = new List<RemainingLivesElement.LivesInfo> ();
				livesInfo.Add (new RemainingLivesElement.LivesInfo (1, Math.Max(state.GameState.teamLives[1], 0)));
				livesInfo.Add (new RemainingLivesElement.LivesInfo (2, Math.Max(state.GameState.teamLives[2], 0)));
				unreliable.Add (new RemainingLivesElement (livesInfo));

				var towerInfo = new List<TowerHealthElement.TowerInfo> ();
				foreach (var tower in state.GameState.Towers) {
					towerInfo.Add (new TowerHealthElement.TowerInfo (tower.ActorId, tower.Health));
				}
				unreliable.Add (new TowerHealthElement (towerInfo));

				// Create and send packets to all clients
				for (int i = 0; i < cm.CountCurrConnections; i++) {
					if (state.ClientManager.Connections [i] == null) {
						continue;
					}
					Packet packet = state.ClientManager.Connections [i].Connection.CreatePacket (unreliable, reliable);
					socket.Send (packet, cm.Connections [i].Destination);
				}
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
				Log.E(ex.Message);
				Log.E(ex.StackTrace);
			}

		}
	}
}
