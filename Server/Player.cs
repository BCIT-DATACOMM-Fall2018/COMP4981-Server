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
			Abilities = new AbilityType[] {
				AbilityType.AutoAttack,
				AbilityType.TestProjectile,
				AbilityType.TestTargeted,
				AbilityType.TestTargetedHoming,
				AbilityType.TestAreaOfEffect,
				AbilityType.Wall,
				AbilityType.Banish,
				AbilityType.BulletAbility,
				AbilityType.PorkChop,
				AbilityType.Dart,
				AbilityType.Purification,
                AbilityType.UwuImScared,
                AbilityType.Fireball,
                AbilityType.WeebOut,
                AbilityType.Whale,
				AbilityType.Blink,
				AbilityType.PewPew,
				AbilityType.Sploosh,
				AbilityType.Gungnir,
				AbilityType.Slash
			};
			Cooldowns = new int[Abilities.Length];
            Experience = 0;
            Level = 1;
            Attack = 1;
            Defense = 1;
        }
    }
}