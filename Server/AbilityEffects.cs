﻿using System;
using System.Collections.Generic;
using GameStateComponents;

namespace Server
{
	/// ----------------------------------------------
	/// Class: 			AbilityEffects - Class that contains functionalities to use 
	///														and receive new abilities based on player level. 
	/// 
	/// PROGRAM:		Server
	///
	/// 
	/// FUNCTIONS:	public GameEndElement (int actorId, int health)
	/// 						public GameEndElement (BitStream bitstream)
	///							public override ElementIndicatorElement GetIndicator ()
	///							protected override void Serialize (BitStream bitStream)
	/// 						protected override void Deserialize (BitStream bitstream)
	/// 						public override void UpdateState (IStateMessageBridge bridge)
	/// 						protected override void Validate ()
	/// 
	/// DATE: 			March 31, 2019
	///
	/// REVISIONS: 
	///
	/// DESIGNER: 	Daniel Shin
	///
	/// PROGRAMMER: Daniel Shin, Kieran Lee, Cameron Roberts
	///
	/// NOTES:			
	///							This class is responsible for holding all the ability function pointers,
	///							initializing all the available player abilities, and randomly assigning the
	///							abilities when a player levels up.
	///							Level 1:       an ability from basic abilities
	///							Level 2 and 3: an ability from normal abilities
	///							Level 4:       an ability from ultimate abilities
	/// ----------------------------------------------		
	public static class AbilityEffects
	{		
		public delegate void Ability(Actor useActor, Actor hitActor);
		
		private const int TOWER_DAMAGE = 100;

		private const int DEBUG_ABILITIES_START = 0;
		private const int DEBUG_ABILITIES_NUMBER = 4;
		private const int AUTOATTACK_ABILITIES_START = 4;
		private const int AUTOATTACK_ABILITIES_NUMBER = 2;
		private const int BASIC_ABILITIES_START = 6;
		private const int BASIC_ABILITIES_NUMBER = 5;
		private const int NORMAL_ABILITIES_START = 11;
		private const int NORMAL_ABILITIES_NUMBER = 7;
		private const int ULTIMATE_ABILITIES_START = 18;
		private const int ULTIMATE_ABILITIES_NUMBER = 3;

		public delegate void ApplyAbilityEffect(Actor useActor, Actor hitActor);

		public static ApplyAbilityEffect[] Apply = new ApplyAbilityEffect[] {
			///DEBUG ABILITIES
			// TestProjectile
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 50);},
			// TestTargeted
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 500);},
			// TestHomingTargeted
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 100);},
			// TestAreaOfEffect
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 200);},

			///AUTOATTACK ABILITIES
			// AutoAttack
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 50);},
			//TowerAttack
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, TOWER_DAMAGE); },

			///BASIC ABILITIES
			//PewPew
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 100);},
			//Sploosh
			(useActor, hitActor) => {
				hitActor.TakeDamage(useActor, 75);
				useActor.PushAndBoostShield(0.4f, 90);
			},
			// Dart
			(useActor, hitActor) => {
				hitActor.TakeDamage(useActor, 20);
				hitActor.PushToDemageOverTime(5, 30, useActor);
			},
			//WeebOut
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 50);},
			// Slash
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 400);},

			///NORMAL ABILITIES\
			// Purification
			(useActor, hitActor) => {hitActor.Health+=250;},
			//Blink
			(useActor, hitActor) => {},
			// UwuImScared
			(useActor, hitActor) => {hitActor.invincible=true;
				hitActor.startInvincibilityTimer();},
			// Wall
			(useActor, hitActor) => {},
			// Bullet Ability
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 250);},
			// Banish
			(useActor, hitActor) => {
				Random rnd = new Random();
				GameUtility.Coordinate randomPosition = new GameUtility.Coordinate(rnd.Next(100,400), rnd.Next(100,400));
				hitActor.Position = randomPosition;
				hitActor.TargetPosition = randomPosition;
			},
			// Pork Chop
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 250);},

			/// ULTIMATE ABILITIES
			// Fireball
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 400);},
			// Gungnir
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 300);},
			// Whale
			(useActor, hitActor) => {hitActor.Health+=400;}
        };


		/// ----------------------------------------------
		/// FUNCTION:		ReturnRandomAbilityId
		/// 
		/// DATE:				March 31, 2019
		/// 
		/// REVISIONS:	(none)
		/// 
		/// DESIGNER:	  Daniel Shin
		/// 
		/// PROGRAMMER:	Daniel Shin, Kieran Lee, Cameron Roberts
		/// 
		/// INTERFACE: 	public static int ReturnRandomAbilityId(Player player)
		/// 
		/// RETURNS: 		int; the index of the ability in the abilities list.
		/// 
		/// NOTES:		  This function returns a random index of the ability based on
		///							player level.
		///							Level 1:       an ability from basic abilities
		///							Level 2 and 3: an ability from normal abilities
		///							Level 4:       an ability from ultimate abilities
		/// ----------------------------------------------
		public static int ReturnRandomAbilityId(Player player)
		{
			switch (player.Level)
			{
				case 1:
				return GameUtility.RandomNum(BASIC_ABILITIES_START, BASIC_ABILITIES_START+BASIC_ABILITIES_NUMBER);
				case 2:
				case 3:
				return GameUtility.RandomNum(NORMAL_ABILITIES_START, NORMAL_ABILITIES_START+NORMAL_ABILITIES_NUMBER);
				case 4:
				return GameUtility.RandomNum(ULTIMATE_ABILITIES_START, ULTIMATE_ABILITIES_START+ULTIMATE_ABILITIES_NUMBER);
				default:
					return -1;
			}
		}
	}
}
