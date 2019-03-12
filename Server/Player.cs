using System;

namespace GameStateComponents {
    public class Player : Actor {
		public int Health { get; set; }

        public Player(int actorId) : base(actorId) {
            Health = 100;
        }
    }
}