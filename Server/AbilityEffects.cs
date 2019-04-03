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
            //PewPew
            (useActor, hitActor) => {hitActor.Health-=150; },
            //Sploosh
            //TODO After stats modifier have been implemented +0.2 defence modifier for 3 seconds
            (useActor, hitActor) => {hitActor.Health-=75; }
		};
			
		private AbilityEffects ()
		{
		}
	}
}

