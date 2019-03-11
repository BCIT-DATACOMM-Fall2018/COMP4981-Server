using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;


namespace GameStateComponents {
    class PlayerConnection {
        private int clientId;
        private int actorId;
        private Destination destination;
        private Socket socket;
        private ReliableUDPConnection connection;

        public PlayerConnection(int clientId, int actorId, Destination destination, Socket socket, ReliableUDPConnection connection) {
            this.clientId = clientId;
            this.actorId = actorId;
            this.destination = destination;
            this.socket = socket;
            this.connection = connection;
        }

        public int getClientId() {
            return clientId;
        }

        public int getActorId() {
            return actorId;
        }

        public Destination getDestination() {
            return destination;
        }

        public Socket getSocket() {
            return socket;
        }

        public ReliableUDPConnection getConnection()
        {
            return connection;
        }
    }
}
