using System;
using Server;
using NetworkLibrary.MessageElements;
using NetworkLibrary;

namespace GameStateComponents
{
    /// -------------------------------------------------------------------------------------------
    /// Class:          Tower - A class to create tower objects.
    /// 
    /// PROGRAM:        Server
    ///
    ///	CONSTRUCTORS:	public Tower(int actorId, int team, GameUtility.Coordinate spawnLocation) 
    /// 
    /// FUNCTIONS:	    public override void Tick(State state)
    ///                 private void attack(State state)
    ///
    /// DATE: 		    April 8, 2019
    ///
    /// REVISIONS: 
    ///
    /// DESIGNER: 	    Wayne Huang
    ///
    /// PROGRAMMER:     Wayne Huang
    ///
    /// NOTES:		    Extends from the Actor class.
    /// -------------------------------------------------------------------------------------------
    public class Tower : Actor
    {
        // Tower attributes
        private const int TOWER_RANGE = 45;

        // Tower constructor
		public Tower(int actorId, int team, GameUtility.Coordinate spawnLocation) : base(actorId, team, spawnLocation)
        {
            Health = 1000;
            Attack = 1;
            Defense = 1.5f;
            RespawnAllowed = false;
			Abilities = new AbilityType[1];
            Abilities[0] = AbilityType.TowerAttack;
            Cooldowns = new int[1];
            Cooldowns[0] = AbilityInfo.InfoArray[(int)AbilityType.TowerAttack].Cooldown;
			Stationary = true;
        }

        // Call every game server tick
        public override void Tick(State state)
        {
				base.Tick(state);
			if (!dead) {
				attack (state);
			}
        }

        // Call for tower to attack
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
			Console.WriteLine("Tower Attack");
			state.GameState.OutgoingReliableElements.Enqueue(new TargetedAbilityElement(ActorId, NetworkLibrary.AbilityType.TowerAttack, targetActorId, state.GameState.MakeCollisionId()));
        }
    }
}