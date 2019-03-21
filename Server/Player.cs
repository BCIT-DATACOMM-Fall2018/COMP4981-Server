using System;
using NetworkLibrary;
using Server;

namespace GameStateComponents {
    public class Player : Actor {

		public Player(int actorId, int team, GameUtility.Coordinate spawnLocation) : base(actorId, team, spawnLocation) {
            Health = 1000;
			Abilities = new AbilityType[] {
				AbilityType.AutoAttack,
				AbilityType.TestProjectile,
				AbilityType.TestTargeted,
				AbilityType.TestTargetedHoming,
				AbilityType.TestAreaOfEffect
			};
			Cooldowns = new int[Abilities.Length];
        }
    }
}