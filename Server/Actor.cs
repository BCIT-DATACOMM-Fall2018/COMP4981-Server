using System;

namespace GameStateComponents {
    public abstract class Actor {

		public struct Location{
			public float X;
			public float Z;

			public Location(float x, float z){
				this.X = x;
				this.Z = z;
			}

		}

		public int ActorId { get; private set; }
		public Location Position { get; set; }
		public Location TargetPosition { get; set; }

        public Actor(int actorId) {
            ActorId = actorId;
        }
    }
}