using System;
using System.Timers;
using Server;
using NetworkLibrary;
using System.Collections;
namespace GameStateComponents
{
	public abstract class Actor
	{
		private static readonly GameUtility.Coordinate deadArea = new GameUtility.Coordinate(-10, -10);

		private const int MAX_HEALTH = 1000;
		private const int RESPAWN_TIME = 100;
		// Used to make sure an Actor can only use 1 ability at a time to
		// prevent race condition with cooldown being set.
		private readonly object abilityLock = new object ();

        private ArrayList demageOverTimeList = new ArrayList();
        private ArrayList shieldOverTimeList = new ArrayList();
        private int _health;
		private int deaths;
		private int reportedDeaths;
		private int turnsDead;
		private bool dead;

		public bool invincible = false;

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

		public GameUtility.Coordinate SpawnLocation { get; private set; }

        public int LastDamageSourceActorId { get; private set; }

		protected AbilityType[] Abilities;
		protected int[] Cooldowns;

		public GameUtility.Coordinate Position { get; set; }

		public GameUtility.Coordinate TargetPosition { get; set; }

        public float Attack { get; set; }

        public float Defense { get; set; }

        public Actor (int actorId, int team, GameUtility.Coordinate spawnLocation)
		{
			ActorId = actorId;
			Team = team;
			SpawnLocation = spawnLocation;
			Position = SpawnLocation;
			TargetPosition = SpawnLocation;
		}

		public virtual void Tick (State state)
		{
			if (Health > 0) {
				dead = false;
				RespawnAllowed = false;
			}
			if (Health == 0 && !dead) {
				turnsDead = 0;
				dead = true;
                //award exp to actor's killer here
                //seperate case for team 0 to award more
                if (state.GameState.actors[ActorId].Team == 0) //tower got killed
                {
                    state.GameState.addEXP((Player)state.GameState.actors[state.GameState.actors[ActorId].LastDamageSourceActorId], false);
                } else
                {
                    state.GameState.addEXP((Player)state.GameState.actors[state.GameState.actors[ActorId].LastDamageSourceActorId], true);
                }
				deaths++;
            }
			if (dead) {
                if (turnsDead++ == RESPAWN_TIME && RespawnAllowed) {
					Health = MAX_HEALTH;
					Position = SpawnLocation;
					TargetPosition = SpawnLocation;
				}
			}

			Move ();
			DecrementCooldowns ();

		}

		private void Move ()
		{
			if (Health <= 0) {
				Position = deadArea;
				TargetPosition = deadArea;
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

		// Starts timer of 5 seconds when invincibility ability is used
		public void startInvincibilityTimer() {
			Timer timer = new Timer();
			timer.Elapsed += OnTimedEvent;
			timer.Interval = 5000;
			timer.AutoReset = false;
			timer.Enabled = true;
		}
		// Stops invincibility once timer is done
		public void OnTimedEvent(Object source, ElapsedEventArgs e) {
			invincible = false;
		}

		public void ApplyAbilityEffects (AbilityType abilityId, Actor hitActor)
		{
			AbilityEffects.Apply [(int)abilityId] (this, hitActor);
		}

        public int TakeDamage(Actor attacker, int baseDamage)
        {
			int damage = 0;
			if (!invincible) {
				Console.WriteLine("oh no i'm not invincible");
                double damageRatio = attacker.Attack / this.Defense;
                damage = (int)(baseDamage * damageRatio);
                this.Health -= damage;
                this.LastDamageSourceActorId = attacker.ActorId;
            } else {
				Console.WriteLine("i'm invincible!");
			}
            return damage;
        }
        public void BoostShieldPerTick() {
            ArrayList removeList = new ArrayList();
            //
            foreach (ArrayList eachSOT in shieldOverTimeList)
            {
                //if 0 remove the boost
                if ((int)eachSOT[1] <= 0) {
                    //add to remove list
                    this.Defense -= (float)eachSOT[0];
                    removeList.Add(eachSOT);
                }
                else {
                    //else -- duration
                    eachSOT[1] = (int)eachSOT[1]-1;
                }


            }

                //remove
                foreach (ArrayList remove in removeList)
            {
                shieldOverTimeList.Remove(remove);
            }
        }
        //you should call this function if someone cast the shield skill
        public void PushAndBoostShield(float shield, int duration)
        {
            this.Defense += shield;
            shieldOverTimeList.Add(MakePairShieldOverTime(shield, duration-1));
        }
        public ArrayList MakePairShieldOverTime(float shield, int duration)
        {
            ArrayList pair = new ArrayList();
            pair.Add(shield);
            pair.Add(duration);
 
            return pair;
        }
        public void DemageOverTimePerTick()
        {
            ArrayList removeList = new ArrayList();
            foreach (ArrayList eachDOT in demageOverTimeList)
            {
                //do demage
                TakeDamage((Actor)eachDOT[2],(int)eachDOT[0]);
                //minus time
                int tempDuration = (int)eachDOT[1];
                eachDOT[1] = --tempDuration;
                //if time is 0, delete the item
                if ((int)eachDOT[1] <= 0) {
                    removeList.Add(eachDOT);
                }

                Console.Write("Demage{0} duration::{1}\n", eachDOT[0], eachDOT[1]);
                Console.WriteLine();
            }

            //remove
            foreach (ArrayList remove in removeList) {
                demageOverTimeList.Remove(remove);
            }

        }
        //you should call this function if someone cast the DOT skill
        public void PushToDemageOverTime(int demagePerTick, int duration, Actor attacker)
        {
            demageOverTimeList.Add(MakePairDemageOverTime(demagePerTick, duration, attacker));
        }

        public ArrayList MakePairDemageOverTime(int demage, int duration, Actor attacker)
        {
            ArrayList pair = new ArrayList();
            pair.Add(demage);
            pair.Add(duration);
            pair.Add(attacker);
            return pair;
        }

    }
}
