using System;
using Server;

namespace GameStateComponents
{
	public abstract class Actor
	{

		private const int MAX_HEALTH = 1000;



		private int _health;

		public int Health {
			get { return _health; } 
			set { 
				_health = value; 
				if (_health > MAX_HEALTH) {
					_health = MAX_HEALTH;
				} else if (_health < 0) {
					_health = 0;
				}
			}
		}

		public int ActorId { get; private set; }

		public float Speed { get; private set; } = 0.82f;


		public GameUtility.Coordinate Position { get; set; }

		public GameUtility.Coordinate TargetPosition { get; set; }

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