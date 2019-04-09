using System;
using System.Collections.Generic;
using GameStateComponents;

namespace Server
{
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
