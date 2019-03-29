using System;
using Server;
using NetworkLibrary.MessageElements;

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
        }

        public override void Tick(State state)
        {
            attack(state);

        }

        private void attack(State state)
        {
            int targetActorId = state.GameState.getClosestEnemyActorInRange(ActorId, TOWER_RANGE);
            if (!state.GameState.ValidateTargetedAbilityUse(ActorId, NetworkLibrary.AbilityType.TowerAttack, targetActorId))
            {
                return;
            }
            state.GameState.OutgoingReliableElements.Enqueue(new TargetedAbilityElement(ActorId, NetworkLibrary.AbilityType.TowerAttack, targetActorId));
        }

        
    }
}