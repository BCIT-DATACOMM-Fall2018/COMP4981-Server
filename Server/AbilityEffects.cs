using System;
using GameStateComponents;

namespace Server
{
	public class AbilityEffects
	{
        private const int TOWER_DAMAGE = 100;
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
            //TowerAttack
            (useActor, hitActor) => {hitActor.TakeDamage(useActor, TOWER_DAMAGE); }
		};
			
		private AbilityEffects ()
		{
		}
	}
}

