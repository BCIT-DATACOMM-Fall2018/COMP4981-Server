using System;

namespace GameStateComponents {
    /// -------------------------------------------------------------------------------------------
    /// Class:          State - The entire state of the server.
    /// 
    /// PROGRAM:        Server
    ///
    ///	CONSTRUCTORS:	public State()
    /// 
    /// FUNCTIONS:	    None
    ///
    /// DATE: 		    April 8, 2019
    ///
    /// REVISIONS: 
    ///
    /// DESIGNER: 	    Wayne Huang
    ///
    /// PROGRAMMER:     Wayne Huang
    ///
    /// NOTES:		    Contains both the gamestate which tracks all game related events and parameters
    ///                 as well as the connection manager which holds all the connections.
    /// -------------------------------------------------------------------------------------------
    public class State {
        // State attributes
		public GameState GameState { get; private set; }
		public ClientManager ClientManager { get; private set; }
		public bool GameOver { get; set; } = false;
		public int TimesEndGameSent { get; set; }

        public State() {
			GameState = new GameState ();
			ClientManager = new ClientManager ();
		}
    }
}
