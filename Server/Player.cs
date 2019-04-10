using System;
using NetworkLibrary;
using Server;

namespace GameStateComponents {
    /// -------------------------------------------------------------------------------------------
	/// Class:          Player - A class to track player parameters.
	/// 
	/// PROGRAM:        Server
	///
	///	CONSTRUCTORS:	public Player(int actorId, int team, GameUtility.Coordinate spawnLocation)
	/// 
	/// FUNCTIONS:	    None
	///
	/// DATE: 		    April 8, 2019
	///
	/// REVISIONS: 
	///
	/// DESIGNER: 	    Wayne Huang
	///
	/// PROGRAMMER:     Wayne Huang
	///
	/// NOTES:		    Extends from the Actor class.
	/// -------------------------------------------------------------------------------------------
    public class Player : Actor {
        // Player attributes
        public int Experience { get; set; }
        public int Level { get; set; }
        public float Speed { get; set; } = 0.82f;

        // Player constructor
        public Player(int actorId, int team, GameUtility.Coordinate spawnLocation) : base(actorId, team, spawnLocation) {
            Health = 1000;
			Abilities = new AbilityType[5];
			Abilities[0] = AbilityType.AutoAttack;
			Cooldowns = new int[Abilities.Length];
            Experience = 0;
            Level = 1;
            Attack = 1;
            Defense = 1;
        }

		public void AddAbility(int slot, AbilityType abilityId){
			if(slot < 0 || slot > Abilities.Length){
				return;
			}
			Abilities[slot] = abilityId;
		}

		public bool HasAbility(AbilityType abilityId){
			return Array.IndexOf(Abilities, abilityId) != -1;
		}
    }
}