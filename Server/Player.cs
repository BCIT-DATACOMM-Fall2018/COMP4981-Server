using System;

namespace GameStateComponents {
    public class Player : Actor {
        private int health;

        public Player(int actorId) : base(actorId) {
            health = 100;
        }

        public int getHealth() {
            return health;
        }

        public void setHealth(int health) {
            this.health = health;
        }
    }
}