using System;
using System.Collections.Generic;

namespace GameStateComponents {
    public class GameState {
        private static GameState instance = null;
        private static readonly object padlock = new object();
        private List<Player> players = new List<Player>();

        private GameState() {

        }

        public static GameState Instance {
            get {
                lock (padlock) {
                    if (instance == null) {
                        instance = new GameState();
                    }
                    return instance;
                }
            }
        }

        public void addPlayer() {
            int playerId = players.Count;
            Player newPlayer = new Player(playerId);
            players.Add(newPlayer);
        }

        public void updateHealth(int playerId, int health) {
            players[playerId].setHealth(health);
            Console.WriteLine("Updateing Player Health, playerid: " + playerId + " health: " + health);
        }

        public void updatePosition(int playerId, double x, double y) {
            players[playerId].setPosition(x, y);
        }

        public void updatePosition(int playerId, double[] position) {
            players[playerId].setPosition(position);
        }

        public int getHealth(int playerId) {
            return players[playerId].getHealth();
        }

        public double[] getPosition(int playerId) {
            return players[playerId].getPosition();
        }
    }
}