using System;
using Server;

namespace GameStateComponents
{
	public abstract class Actor
	{

		public struct Location
		{
			public float X;
			public float Z;

			public Location (float x, float z)
			{
				this.X = x;
				this.Z = z;
			}

		}

		public int Health { get; set; }

		public int ActorId { get; private set; }

		public float Speed { get; private set; } = 0.82f;


		public GameUtility.Coordinate Position { get; set; }

		public GameUtility.Coordinate TargetPosition { get; set;}

		public Actor (int actorId)
		{
			ActorId = actorId;
		}

		public void Move ()
		{
			Position = GameUtility.FindNewCoordinate (Position, TargetPosition, Speed);
		}
	}
}