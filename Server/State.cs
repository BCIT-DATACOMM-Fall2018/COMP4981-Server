using System;

namespace GameStateComponents {
    class State {
        private static State instance = null;
        private static readonly object padlock = new object();
		public GameState GameState { get; private set; }
		public ClientManager ClientManager { get; private set; }

        private State() {
			GameState = new GameState();
			ClientManager = new ClientManager();
        }

        public static State Instance {
            get {
                lock (padlock) {
                    if (instance == null) {
                        instance = new State();
                    }
                    return instance;
                }
            }
        }
    }
}
