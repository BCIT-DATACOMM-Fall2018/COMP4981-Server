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
			// AutoAttack
			(useActor, hitActor) => {hitActor.Health-=50;},
			// UwuImScared -> will work once TakeDamage() is being used instead for damaging
			(useActor, hitActor) => {hitActor.invincible=true;},
			// Fireball
			(useActor, hitActor) => {hitActor.Health-=300;}
		};

		private AbilityEffects ()
		{
		}
	}
}
