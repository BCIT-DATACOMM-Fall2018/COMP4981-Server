using System;
using GameStateComponents;

namespace Server
{
	public class AbilityEffects
	{
		
		public delegate void ApplyAbilityEffect(Actor useActor, Actor hitActor);

		public static ApplyAbilityEffect[] Apply = new ApplyAbilityEffect[] {
			// TestProjectile
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 50);},
			// TestTargeted
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 500);},
			// TestHomingTargeted
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 100);},
			// TestAreaOfEffect
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 200);},
			// AutoAttack
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 50);},
			// Wall
			(useActor, hitActor) => {},
			// Banish
			(useActor, hitActor) => {
				Random rnd = new Random();
				GameUtility.Coordinate randomPosition = new GameUtility.Coordinate(rnd.Next(100,400), rnd.Next(100,400));
				hitActor.Position = randomPosition;
				hitActor.TargetPosition = randomPosition;
				},
			// Bullet Ability
			(useActor, hitActor) => {hitActor.Health-=80;},
			// Pork Chop
			(useActor, hitActor) => {hitActor.Health-=250;},
			// Dart
			(useActor, hitActor) => {hitActor.Health-=20;},
			// Purification
			(useActor, hitActor) => {hitActor.Health+=250;},
			// UwuImScared
			(useActor, hitActor) => {hitActor.invincible=true;
									 hitActor.startInvincibilityTimer();},
			// Fireball
			(useActor, hitActor) => {hitActor.TakeDamage(useActor, 300);}
		};

		private AbilityEffects ()
		{
		}
	}
}
