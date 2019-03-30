using System;
using GameStateComponents;

namespace Server
{
	public class AbilityEffects
	{
		
		public delegate void ApplyAbilityEffect(Actor useActor, Actor hitActor);

		public static ApplyAbilityEffect[] Apply = new ApplyAbilityEffect[] {
			// TestProjectile
			(useActor, hitActor) => {hitActor.Health-=50;},
			// TestTargeted
			(useActor, hitActor) => {hitActor.Health-=500;},
			// TestHomingTargeted
			(useActor, hitActor) => {hitActor.Health-=100;},
			// TestAreaOfEffect
			(useActor, hitActor) => {hitActor.Health-=200;},
			// UwuImScared
			(useActor, hitActor) => {
				hitActor.invincible=true;
				hitActor.Health+=200;},
			// Fireball
			(useActor, hitActor) => {hitActor.Health-=300;},
			// AutoAttack
			(useActor, hitActor) => {hitActor.Health-=50;},
			// Wall
			(useActor, hitActor) => {},
			// Banish
			(useActor, hitActor) => {
				Random rnd = new Random();
				GameUtility.Coordinate randomPosition = new GameUtility.Coordinate(rnd.Next(100,400), rnd.Next(100,400));
				hitActor.Position = randomPosition;
				hitActor.TargetPosition = randomPosition;
				}
		};

		private AbilityEffects ()
		{
		}
	}
}
