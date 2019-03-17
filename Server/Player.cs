using System;
using NetworkLibrary;

namespace GameStateComponents {
    public class Player : Actor {

        public Player(int actorId) : base(actorId) {
            Health = 1000;
			Abilities = new AbilityType[] {
				AbilityType.TestProjectile,
				AbilityType.TestTargeted,
				AbilityType.TestTargetedHoming,
				AbilityType.TestAreaOfEffect
			};
			Cooldowns = new int[Abilities.Length];
        }
    }
}