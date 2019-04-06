using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;


namespace GameStateComponents {
    public class PlayerConnection {
		public int ClientId { get; private set;}
		public int ActorId { get; set;}
		public Destination Destination { get; private set;}
		public ReliableUDPConnection Connection { get; private set;}
		public bool Ready { get; set; }
        public bool startedGame { get; set; }
		public int Team { get; set; }
		public string Name { get; set; }
		private int timeSinceLastPacket;

		public PlayerConnection(int clientId, Destination destination, ReliableUDPConnection connection, string name) {
			this.ClientId = clientId;
			this.Destination = destination;
			this.Connection = connection;
			this.Name = name;
        }

		public void MarkPacketReceive(){
			timeSinceLastPacket = 0;
		}

		public bool Disconnected(){
			return timeSinceLastPacket++ > 30;
		}
    }
}
