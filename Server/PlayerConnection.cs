using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;


namespace GameStateComponents {
    /// -------------------------------------------------------------------------------------------
    /// Class:          PlayerConnection - A class to track player connection parameters.
    /// 
    /// PROGRAM:        Server
    ///
    ///	CONSTRUCTORS:	public PlayerConnection(int clientId, Destination destination, ReliableUDPConnection connection, string name)
    /// 
    /// FUNCTIONS:	    public void MarkPacketReceive()
    ///                 public bool Disconnected()
    ///
    /// DATE: 		    April 8, 2019
    ///
    /// REVISIONS: 
    ///
    /// DESIGNER: 	    Wayne Huang
    ///
    /// PROGRAMMER:     Wayne Huang
    ///
    /// NOTES:		    Holds all the connection parameters for one client/player.
    /// -------------------------------------------------------------------------------------------
    public class PlayerConnection {
        // PlayerConnection attributes
		public int ClientId { get; private set;}
		public int ActorId { get; set;}
		public Destination Destination { get; private set;}
		public ReliableUDPConnection Connection { get; private set;}
		public bool Ready { get; set; }
        public bool startedGame { get; set; }
		public int Team { get; set; }
		public string Name { get; set; }
		private int timeSinceLastPacket;

        // PlayerConnection constructor
		public PlayerConnection(int clientId, Destination destination, ReliableUDPConnection connection, string name) {
			this.ClientId = clientId;
			this.Destination = destination;
			this.Connection = connection;
			this.Name = name;
        }

		public void MarkPacketReceive() {
			timeSinceLastPacket = 0;
		}

		public bool Disconnected() {
			return timeSinceLastPacket++ > 120;
		}
    }
}
