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
