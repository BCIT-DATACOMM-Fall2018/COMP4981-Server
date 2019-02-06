using System;

namespace GameStateComponents {
    public class Player {
        private int playerId;
        private int health;
        private double[] position;

        public Player(int playerId) {
            this.playerId = playerId;
            health = 100;
            position = new double[2] {0, 0};
        }

        public int getPlayerId() {
            return playerId;
        }

        public int getHealth() {
            return health;
        }

        public double[] getPosition() {
            return position;
        }

        public void setHealth(int health) {
            this.health = health;
        }

        public void setPosition(double x, double y) {
            position[0] = x;
            position[1] = y;
        }

        public void setPosition(double[] position) {
            this.position[0] = position[0];
            this.position[1] = position[1];
        }
    }
}