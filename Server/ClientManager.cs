using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;


namespace GameStateComponents {
    /// -------------------------------------------------------------------------------------------
    /// Class:          ClientManager - Holds all the client connections for the game.
    /// 
    /// PROGRAM:        Server
    ///
    ///	CONSTRUCTORS:	public ClientManager()
    /// 
    /// FUNCTIONS:	    public int AddConnection(Destination destination, string name)
    ///                 public int GetActorId(int clientId)
    ///                 public int GetClientId(int actorId)
    ///                 public Destination GetDestinationFromClient(int clientId)
    ///                 public Destination GetDestinationFromActor(int actorId)
    ///                 public PlayerConnection FindClientByActorId(int actorId)
    ///
    /// DATE: 		    April 8, 2019
    ///
    /// REVISIONS: 
    ///
    /// DESIGNER: 	    Wayne Huang
    ///
    /// PROGRAMMER:     Wayne Huang
    ///
    /// NOTES:		    Holds and manages all the connections for the game.
    /// -------------------------------------------------------------------------------------------
    public class ClientManager {
        // ClientManager attributes
        public const int MAX_PLAYERS = 40;
		public PlayerConnection[] Connections { get; private set; } = new PlayerConnection[MAX_PLAYERS];
		public int CountCurrConnections { get; private set; }

        //when checking connections elswhere in the code, we are looking at connections[i] where 0 <= i < countCurrConnections 

        // ClientManager constuctor
        public ClientManager() {
        }

        // Used to add a connection to the ConnectionManager
		public int AddConnection(Destination destination, string name) {
            for (int i = 0; i < MAX_PLAYERS; i++) {
				if (Connections[i] == null) {
					PlayerConnection newPlayer = new PlayerConnection(i, destination, new ReliableUDPConnection(i), name);
					Connections[i] = newPlayer;
					if (i == CountCurrConnections) {
						CountCurrConnections++;
					}
                    return i;
                }
            }
			throw new OutOfMemoryException ();
        }

        // Gets the actor id given the client id
        public int GetActorId(int clientId) {
			return Connections[clientId].ActorId;
        }

        // Gets the client id given the actor id
        public int GetClientId(int actorId) {
            for (int i = 0; i < MAX_PLAYERS; i++) {
				if (Connections [i] == null) {
					continue;
				}
				if (Connections[i].ActorId == actorId) {
					return Connections[i].ClientId;
                }
            }
            return -1;
        }

        // Gets the destination given client id
        public Destination GetDestinationFromClient(int clientId) {
			return Connections[clientId].Destination;
        }

        // Gets the destination given actor id
        public Destination GetDestinationFromActor(int actorId) {
            int clientId = GetClientId(actorId);
			return Connections[clientId].Destination;
        }

        // Returns the connection object given the actor id
		public PlayerConnection FindClientByActorId(int actorId){
			int clientId = GetClientId(actorId);
			return Connections [clientId];;

		}
    }
}
