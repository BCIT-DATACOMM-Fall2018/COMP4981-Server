using System;

namespace GameStateComponents {
    class ClientManager {
        private static ClientManager instance = null;
        private static readonly object padlock = new object();
        private PlayerConnection[] connections = new PlayerConnection[10];

        private ClientManager() {

        }

        public static ClientManager Instance {
            get {
                lock (padlock) {
                    if (instance == null) {
                        instance = new ClientManager();
                    }
                    return instance;
                }
            }
        }

        public int addConnection(int actorId, Destination destination, Socket socket) {
            for (int i = 0; i < 10; i++) {
                if (connections[i] == null) {
                    PlayerConnection newPlayer = new PlayerConnection(i, actorId, destination, socket);
                    connections[i] = newPlayer;
                    return i;
                }
            }
            return -1;
        }

        public int getActorId(int clientId) {
            return connections[clientId].getActorId();
        }

        public int getClientId(int actorId) {
            for (int i = 0; i < 10; i++) {
                if (connections[i].getActorId() == actorId) {
                    return connections[i].getClientId();
                }
            }
            return -1;
        }

        public Destination getDestinationFromClient(int clientId) {
            return connections[clientId].getDestination();
        }

        public Destination getDestinationFromActor(int actorId) {
            int clientId = getClientId(actorId);
            return connections[clientId].getDestination();
        }

        public Socket getSocketFromClient(int clientId) {
            return connections[clientId].getSocket();
        }

        public Socket getSocketFromActor(int actorId) {
            int clientId = getClientId(actorId);
            return connections[clientId].getSocket();
        }
    }
}
