using System;
using System.Collections.Generic;

namespace GameStateComponents {
    public class GameState {
        private static GameState instance = null;
        private static readonly object padlock = new object();
        private Dictionary<int, Actor> actors = new Dictionary<int, Actor>();
        private int createdActorsCount = 0;

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

        public int addPlayer() {
            int actorId = createdActorsCount++;
            Player newPlayer = new Player(actorId);
            actors.Add(actorId, newPlayer);
            return actorId;
        }

        public void updateHealth(int actorId, int health) {
            ((Player) actors[actorId]).setHealth(health);
        }

        public void updatePosition(int actorId, double x, double y) {
            actors[actorId].setPosition(x, y);
        }

        public void updatePosition(int actorId, double[] position) {
            actors[actorId].setPosition(position);
        }

        public int getHealth(int actorId) {
            return ((Player)actors[actorId]).getHealth();
        }

        public double[] getPosition(int actorId) {
            return actors[actorId].getPosition();
        }
    }
}