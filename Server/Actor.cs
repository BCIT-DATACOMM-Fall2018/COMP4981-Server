using System;
using Server;
using NetworkLibrary;

namespace GameStateComponents
{
	public abstract class Actor
	{

		private const int MAX_HEALTH = 1000;

		// Used to make sure an Actor can only use 1 ability at a time to 
		// prevent race condition with cooldown being set.
		private static readonly object abilityLock = new object();

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

		public int Team { get; private set; }

		protected AbilityType[] Abilities;
		protected int[] Cooldowns;

		public GameUtility.Coordinate Position { get; set; }

		public GameUtility.Coordinate TargetPosition { get; set; }

		public Actor (int actorId, int team)
		{
			ActorId = actorId;
			Team = team;
		}

		public void Tick(){
			Move ();
			DecrementCooldowns ();
		}

		private void Move ()
		{
			Position = GameUtility.FindNewCoordinate (Position, TargetPosition, Speed);
		}

		private void DecrementCooldowns(){
			for (int i = 0; i < Cooldowns.Length; i++) {
				if (Cooldowns [i] > 0) {
					Cooldowns [i]--;
				}
			}
		}

		public bool UseAbility(AbilityType abilityId){
			lock (abilityLock) {
				// Check that the actor has the ability in their ability array
				int abilityIndex = Array.IndexOf (Abilities, abilityId);
				if (abilityIndex == -1) {
					return false;
				}

				// Check that the ability isn't on cooldown
				if (Cooldowns [abilityIndex] > 0) {
					Console.WriteLine ("Attempted to use ability that is on cooldown");
					return false;
				}

				// Ability Can be used. Set it to be on cooldown
				Cooldowns [abilityIndex] = AbilityInfo.InfoArray [(int)abilityId].Cooldown;
				return true;
			}
		}

		public void ApplyAbilityEffects(AbilityType abilityId, Actor hitActor){
			AbilityEffects.Apply [(int)abilityId] (this, hitActor);
		}
	}
}