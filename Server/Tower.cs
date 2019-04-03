﻿using System;
using Server;
using NetworkLibrary.MessageElements;
using NetworkLibrary;

namespace GameStateComponents
{
    public class Tower : Actor
    {
        private const int TOWER_RANGE = 200;

		public Tower(int actorId, int team, GameUtility.Coordinate spawnLocation) : base(actorId, team, spawnLocation)
        {
            Health = 1000;
            Attack = 1;
            Defense = 1;
            RespawnAllowed = false;
			Abilities = new AbilityType[1];
            Abilities[0] = AbilityType.TowerAttack;
            Cooldowns = new int[1];
            Cooldowns[0] = AbilityInfo.InfoArray[(int)AbilityType.TowerAttack].Cooldown;
        }

        public override void Tick(State state)
        {
            attack(state);

        }

        private void attack(State state)
        {
            int targetActorId = state.GameState.getClosestEnemyActorInRange(ActorId, TOWER_RANGE);

			if (targetActorId == -1) {
				return;
			}

            if (!state.GameState.ValidateTargetedAbilityUse(ActorId, NetworkLibrary.AbilityType.TowerAttack, targetActorId))
            {
                return;
            }
            state.GameState.OutgoingReliableElements.Enqueue(new TargetedAbilityElement(ActorId, NetworkLibrary.AbilityType.TowerAttack, targetActorId));
        }

        
    }
}