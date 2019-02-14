using System;

namespace GameStateComponents {
    class State {
        private static State instance = null;
        private static readonly object padlock = new object();
        private static GameState gamestate;
        private static ClientManager clientmanager;

        private State() {
            gamestate = GameState.Instance;
            clientmanager = ClientManager.Instance;
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

        public GameState getGameState() {
            return gamestate;
        }

        public ClientManager getClientManger() {
            return clientmanager;
        }
    }
}
