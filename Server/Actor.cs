using System;
using Server;
using NetworkLibrary;

namespace GameStateComponents
{
	public abstract class Actor
	{

		private const int MAX_HEALTH = 1000;
		private const int RESPAWN_TIME = 100;
		// Used to make sure an Actor can only use 1 ability at a time to
		// prevent race condition with cooldown being set.
		private readonly object abilityLock = new object ();

		private int _health;
		private int deaths;
		private int reportedDeaths;
		private int turnsDead;
		private bool dead;

		public int Health {
			get { return _health; } 
			set { 
				_health = value; 
				if (_health > 0) {
					dead = false;
				}
				if (_health > MAX_HEALTH) {
					_health = MAX_HEALTH;
				} else if (_health <= 0) {
					_health = 0;
				}
				
			}
		}

		public bool HasDied ()
		{
			if (deaths > reportedDeaths) {
				reportedDeaths++;
				return true;
			}
			return false;
		}

		public int ActorId { get; private set; }

		public float Speed { get; private set; } = 0.82f;

		public int Team { get; private set; }

		public bool RespawnAllowed { get; set; } = false;

		protected AbilityType[] Abilities;
		protected int[] Cooldowns;

		public GameUtility.Coordinate Position { get; set; }

		public GameUtility.Coordinate TargetPosition { get; set; }

		public Actor (int actorId, int team)
		{
			ActorId = actorId;
			Team = team;
		}

		public void Tick ()
		{
			if (Health > 0) {
				dead = false;
				RespawnAllowed = false;
			}
			if (Health == 0 && !dead) {
				turnsDead = 0;
				dead = true;
				deaths++;
			}
			if (dead) {
				if (turnsDead++ == RESPAWN_TIME && RespawnAllowed) {
					Health = MAX_HEALTH;
					Position = new GameUtility.Coordinate (310, 90);
					TargetPosition = new GameUtility.Coordinate (310, 90);
				}
			}
			Move ();
			DecrementCooldowns ();
		}

		private void Move ()
		{
			if (Health <= 0) {
				Position = new GameUtility.Coordinate (-10, -10);
				TargetPosition = new GameUtility.Coordinate (-10, -10);
				return;
			}
			Position = GameUtility.FindNewCoordinate (Position, TargetPosition, Speed);
		}

		private void DecrementCooldowns ()
		{
			for (int i = 0; i < Cooldowns.Length; i++) {
				if (Cooldowns [i] > 0) {
					Cooldowns [i]--;
				}
			}
		}

		public bool UseAbility (AbilityType abilityId)
		{
			if (Health <= 0) {
				return false;
			}
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

		public void ApplyAbilityEffects (AbilityType abilityId, Actor hitActor)
		{
			AbilityEffects.Apply [(int)abilityId] (this, hitActor);
		}
	}
}