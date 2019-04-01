using System;
using NetworkLibrary;
using Server;

namespace GameStateComponents {
    public class Player : Actor {
        public int Experience { get; set; }
        public int Level { get; set; }
        public float Speed { get; set; } = 0.82f;

        public Player(int actorId, int team, GameUtility.Coordinate spawnLocation) : base(actorId, team, spawnLocation) {
            Health = 1000;
			Abilities = new AbilityType[] {
				AbilityType.AutoAttack,
				AbilityType.TestProjectile,
				AbilityType.TestTargeted,
				AbilityType.TestTargetedHoming,
				AbilityType.TestAreaOfEffect,
				AbilityType.Wall,
				AbilityType.Banish,
				AbilityType.BulletAbility,
				AbilityType.PorkChop,
				AbilityType.Dart,
				AbilityType.Purification
			};
			Cooldowns = new int[Abilities.Length];
            Experience = 0;
            Level = 1;
            Attack = 1;
            Defense = 1;
        }
    }
}