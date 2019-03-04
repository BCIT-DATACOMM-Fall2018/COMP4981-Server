﻿using System;

namespace GameStateComponents {
    class PlayerConnection {
        private int clientId;
        private int actorId;
        private Destination destination;
        private Socket socket;

        public PlayerConnection(int clientId, int actorId, Destination destination, Socket socket) {
            this.clientId = clientId;
            this.actorId = actorId;
            this.destination = destination;
            this.socket = socket;
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
    }
}
