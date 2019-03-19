using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;


namespace GameStateComponents {
    class PlayerConnection {
		public int ClientId { get; private set;}
		public int ActorId { get; private set;}
		public Destination Destination { get; private set;}
		public ReliableUDPConnection Connection { get; private set;}
		public bool Ready { get; set; }
        public bool startedGame { get; set; }
		public int Team { get; set; }
		public string Name { get; set; }

		public PlayerConnection(int clientId, int actorId, Destination destination, ReliableUDPConnection connection, string name) {
			this.ClientId = clientId;
			this.ActorId = actorId;
			this.Destination = destination;
			this.Connection = connection;
			this.Name = name;
        }
    }
}
