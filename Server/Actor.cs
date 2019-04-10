using System;
using System.Timers;
using Server;
using NetworkLibrary;
using System.Collections;
namespace GameStateComponents
{
    /// -------------------------------------------------------------------------------------------
    /// Class:          Actor - An abstract class for actor game objects.
    /// 
    /// PROGRAM:        Server
    ///
    ///	CONSTRUCTORS:	public Actor (int actorId, int team, GameUtility.Coordinate spawnLocation)
    /// 
    /// FUNCTIONS:	    public virtual void Tick (State state)
    ///                 private void Move ()
    ///                 private void DecrementCooldowns ()
    ///                 public bool UseAbility (AbilityType abilityId)
    ///                 public void startInvincibilityTimer()
    ///                 public void OnTimedEvent(Object source, ElapsedEventArgs e)
    ///                 public void ApplyAbilityEffects (AbilityType abilityId, Actor hitActor)
    ///                 public int TakeDamage(Actor attacker, int baseDamage)                
    ///
    /// DATE: 		    March 11, 2019
    ///
    /// REVISIONS: 
    ///
    /// DESIGNER: 	    Wayne Huang, Cameron Roberts
    ///
    /// PROGRAMMER:     Wayne Huang, Cameron Roberts
    ///
    /// NOTES:		    Abstract class for all actors.
    /// -------------------------------------------------------------------------------------------
    public abstract class Actor
	{
        // Actor parameters
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
		public bool Stationary { get; protected set; }
		protected bool dead;

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

		/// ----------------------------------------------
		/// CONSTRUCTOR: Actor
		/// 
		/// DATE:		March 11th, 2019
		/// 
		/// REVISIONS:	
		/// 
		/// DESIGNER:	Cameron Roberts
		/// 
		/// PROGRAMMER:	Cameron Roberts
		/// 
		/// INTERFACE: 	public Actor (int actorId, int team, GameUtility.Coordinate spawnLocation)
		/// 
		/// NOTES: 	
		/// ----------------------------------------------
        public Actor (int actorId, int team, GameUtility.Coordinate spawnLocation)
		{
			ActorId = actorId;
			Team = team;
			SpawnLocation = spawnLocation;
			Position = SpawnLocation;
			TargetPosition = SpawnLocation;
		}


		/// ----------------------------------------------
		/// FUNCTION:	Tick
		/// 
		/// DATE:		March 17th, 2019
		/// 
		/// REVISIONS:	
		/// 
		/// DESIGNER:	Cameron Roberts
		/// 
		/// PROGRAMMER:	Cameron Roberts
		/// 
		/// INTERFACE: 	private virtual void Tick ()
		/// 
		/// RETURNS: 	void.
		/// 
		/// NOTES:		Calls all other functions that should activate every
		/// 			server tick.
		/// ----------------------------------------------
		public virtual void Tick (State state)
		{
			if (Health > 0) {
				dead = false;
				RespawnAllowed = false;
			}
			if (Health == 0 && !dead) {
				turnsDead = 0;
				dead = true;
                
				Actor killer = state.GameState.actors [state.GameState.actors [ActorId].LastDamageSourceActorId];
				if (killer.Team != 0) {
					//award exp to actor's killer here
					//seperate case for team 0 to award more
					if (state.GameState.actors[ActorId].Team == 0) //tower got killed
					{
						state.GameState.addEXP((Player)state.GameState.actors[state.GameState.actors[ActorId].LastDamageSourceActorId], false);
					} else
					{
						state.GameState.addEXP((Player)state.GameState.actors[state.GameState.actors[ActorId].LastDamageSourceActorId], true);
					}
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
			BoostShieldPerTick();
			DemageOverTimePerTick();

		}

		/// ----------------------------------------------
		/// FUNCTION:	Move
		/// 
		/// DATE:		March 13th, 2019
		/// 
		/// REVISIONS:	
		/// 
		/// DESIGNER:	Cameron Roberts
		/// 
		/// PROGRAMMER:	Cameron Roberts
		/// 
		/// INTERFACE: 	private void Move ()
		/// 
		/// RETURNS: 	void.
		/// 
		/// NOTES:		Moves the actor towards their target position.
		/// 			To be called every tick.
		/// ----------------------------------------------
		private void Move ()
		{
			if (Health <= 0) {
				Position = deadArea;
				TargetPosition = deadArea;
				return;
			}
			Position = GameUtility.FindNewCoordinate (Position, TargetPosition, Speed);
		}

		/// ----------------------------------------------
		/// FUNCTION:	DecrementCooldowns
		/// 
		/// DATE:		March 17th, 2019
		/// 
		/// REVISIONS:	
		/// 
		/// DESIGNER:	Cameron Roberts
		/// 
		/// PROGRAMMER:	Cameron Roberts
		/// 
		/// INTERFACE: 	private void DecrementCooldowns ()
		/// 
		/// RETURNS: 	void.
		/// 
		/// NOTES:		Decrements ability cooldowns
		/// ----------------------------------------------
		private void DecrementCooldowns ()
		{
			for (int i = 0; i < Cooldowns.Length; i++) {
				if (Cooldowns [i] > 0) {
					Cooldowns [i]--;
				}
			}
		}

		/// ----------------------------------------------
		/// FUNCTION:	UseAbility
		/// 
		/// DATE:		March 17th, 2019
		/// 
		/// REVISIONS:	
		/// 
		/// DESIGNER:	Cameron Roberts
		/// 
		/// PROGRAMMER:	Cameron Roberts
		/// 
		/// INTERFACE: 	public bool UseAbility (AbilityType abilityId)
		/// 				AbilityType abilityId: The ability to be used
		/// 
		/// RETURNS: 	True if the ability could be used. False if the abiliy
		/// 			could not be used.
		/// 
		/// NOTES:		Uses the given ability.
		/// ----------------------------------------------
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
					//Console.WriteLine ("Attempted to use ability that is on cooldown");
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
			timer.Interval = 3000;
			timer.AutoReset = false;
			timer.Enabled = true;
		}
		// Stops invincibility once timer is done
		public void OnTimedEvent(Object source, ElapsedEventArgs e) {
			invincible = false;
		}

		/// ----------------------------------------------
		/// FUNCTION:	ApplyAbilityEffects
		/// 
		/// DATE:		March 17th, 2019
		/// 
		/// REVISIONS:	
		/// 
		/// DESIGNER:	Cameron Roberts
		/// 
		/// PROGRAMMER:	Cameron Roberts
		/// 
		/// INTERFACE: 	public void ApplyAbilityEffects (AbilityType abilityId, Actor hitActor)
		/// 				AbilityType abilityId: The ability to apply the effect of
		/// 				Actor hitActor: The actor to use the ability on
		/// 
		/// RETURNS: 	void.
		/// 
		/// NOTES:		Applies the given ability to the given user.
		/// ----------------------------------------------
		public void ApplyAbilityEffects (AbilityType abilityId, Actor hitActor)
		{
			AbilityEffects.Apply [(int)abilityId] (this, hitActor);
		}

        /// ----------------------------------------------
		/// FUNCTION:	ApplyAbilityEffects
		/// 
		/// DATE:		March 25th, 2019
		/// 
		/// REVISIONS:	
		/// 
		/// DESIGNER:	Wayne Huang
		/// 
		/// PROGRAMMER:	Wayne Huang
		/// 
		/// INTERFACE: 	public void TakeDamage (Actor attacker, int baseDamage)
		/// 				Actor attacker: the actor who is dealing damage
		/// 				int baseDamage: the amount of damage the ability does before modifiers
		/// 
		/// RETURNS: 	void.
		/// 
		/// NOTES:		decreases the actor's health based on the base damage, 
        ///             the attacker's attack stat, and the actor's defense stat
		/// ----------------------------------------------
        public int TakeDamage(Actor attacker, int baseDamage)
        {
			int damage = 0;
			if (!invincible) {
                double damageRatio = attacker.Attack / this.Defense;
                damage = (int)(baseDamage * damageRatio);
                this.Health -= damage;
                this.LastDamageSourceActorId = attacker.ActorId;
            } else {
			}
            return damage;
        }

/*---------------------------------------------------------------------------------------
--  FUNCTION:   BoostShieldPerTick
--
--  DATE:       April 3, 2019
--
--  REVISIONS:
--
--  DESIGNER:   Ziqian Zhang
--
--  PROGRAMMER: Ziqian Zhang
--
--  INTERFACE:   public void BoostShieldPerTick()
--                  
--
--  RETURNS:    void
--
--  NOTES:  This function check shieldPerTick queue, if it have message in
--          queue, it will apply the shield to the Actor by increast defense
--          and decrese duration by 1. If the duration of specific skill is 0,
--          the skill will remove from queue.
--          Should call this function per tick.
--
---------------------------------------------------------------------------------------*/
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
/*---------------------------------------------------------------------------------------
--  FUNCTION:   PushAndBoostShield
--
--  DATE:       April 3, 2019
--
--  REVISIONS:
--
--  DESIGNER:   Ziqian Zhang
--
--  PROGRAMMER: Ziqian Zhang
--
--  INTERFACE:   public void PushAndBoostShield(float shield, int duration)
--                      shield: how much defense need to increase.
--                      duration: how long will the skill last.
--                  
--
--  RETURNS:    void
--
--  NOTES:  This function will simply push the incoming skill with duration to queue.
--          This function should be call if someone cast the shield skill.
--
---------------------------------------------------------------------------------------*/
        public void PushAndBoostShield(float shield, int duration)
        {
            this.Defense += shield;
            shieldOverTimeList.Add(MakePairShieldOverTime(shield, duration-1));
        }
/*---------------------------------------------------------------------------------------
--  FUNCTION:   MakePairShieldOverTime
--
--  DATE:       April 3, 2019
--
--  REVISIONS:
--
--  DESIGNER:   Ziqian Zhang
--
--  PROGRAMMER: Ziqian Zhang
--
--  INTERFACE:   public ArrayList MakePairShieldOverTime(float shield, int duration)
--                      shield: how much defense need to increase.
--                      duration: how long will the skill last.
--                  
--
--  RETURNS:    void
--
--  NOTES:  This function will pair shield with duration.
--
---------------------------------------------------------------------------------------*/
        public ArrayList MakePairShieldOverTime(float shield, int duration)
        {
            ArrayList pair = new ArrayList();
            pair.Add(shield);
            pair.Add(duration);
 
            return pair;
        }
/*---------------------------------------------------------------------------------------
--  FUNCTION:   DemageOverTimePerTick
--
--  DATE:       April 3, 2019
--
--  REVISIONS:
--
--  DESIGNER:   Ziqian Zhang
--
--  PROGRAMMER: Ziqian Zhang
--
--  INTERFACE:   public void DemageOverTimePerTick()
--                  
--
--  RETURNS:    void
--
--  NOTES:  This function do demage each tick. This function check DemagePerTick
--          queue, if it have message in queue, it will apply the demage to the 
--          Actor by decrease health and decrese duration by 1. If the duration 
--          of specific skill is 0, the skill will remove from queue.
--          This function should be called each tick.
--
---------------------------------------------------------------------------------------*/
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

            }

            //remove
            foreach (ArrayList remove in removeList) {
                demageOverTimeList.Remove(remove);
            }

        }
/*---------------------------------------------------------------------------------------
--  FUNCTION:   PushToDemageOverTime
--
--  DATE:       April 3, 2019
--
--  REVISIONS:
--
--  DESIGNER:   Ziqian Zhang
--
--  PROGRAMMER: Ziqian Zhang
--
--  INTERFACE:   public void PushToDemageOverTime(int demagePerTick, int duration, Actor attacker)
--                      demagePerTick: how much demage.
--                      duration: how long will the skill last.
--                      attacker: the accacker actor.
--                  
--
--  RETURNS:    void
--
--  NOTES:  This function will simply push the incoming skill with duration to queue.
--          This function should be call if someone cast the skill.
--
---------------------------------------------------------------------------------------*/
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
