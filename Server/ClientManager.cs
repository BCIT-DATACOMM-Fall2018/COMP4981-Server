using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;


namespace GameStateComponents {
    public class ClientManager {
        public const int MAX_PLAYERS = 40;
		public PlayerConnection[] Connections { get; private set; } = new PlayerConnection[MAX_PLAYERS];
		public int CountCurrConnections { get; private set; }

        //when checking connections elswhere in the code, we are looking at connections[i] where 0 <= i < countCurrConnections 

        public ClientManager() {
        }

		public int AddConnection(Destination destination, string name) {
            for (int i = 0; i < MAX_PLAYERS; i++) {
				if (Connections[i] == null) {
					PlayerConnection newPlayer = new PlayerConnection(i, i, destination, new ReliableUDPConnection(i), name);
					Connections[i] = newPlayer;
					CountCurrConnections++;
                    return i;
                }
            }
			throw new OutOfMemoryException ();
        }

        public int GetActorId(int clientId) {
			return Connections[clientId].ActorId;
        }

        public int GetClientId(int actorId) {
            for (int i = 0; i < MAX_PLAYERS; i++) {
				if (Connections[i].ActorId == actorId) {
					return Connections[i].ClientId;
                }
            }
            return -1;
        }

        public Destination GetDestinationFromClient(int clientId) {
			return Connections[clientId].Destination;
        }

        public Destination GetDestinationFromActor(int actorId) {
            int clientId = GetClientId(actorId);
			return Connections[clientId].Destination;
        }
    }
}
