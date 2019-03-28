using System;

namespace GameStateComponents {
    public class State {
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
