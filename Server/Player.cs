using System;

namespace GameStateComponents {
    public class Player : Actor {

        public Player(int actorId) : base(actorId) {
            Health = 1000;
        }
    }
}