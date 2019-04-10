using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;
using System.Collections.Generic;


using GameStateComponents;

namespace Server
{
    /// ----------------------------------------------
    /// Class: 		ServerStateMessageBridge - provides implementations of functions for message elements to call
    /// 
    /// PROGRAM:	Server
    ///
    /// 
    /// FUNCTIONS:	public ServerStateMessageBridge (State state)
    ///             public void UpdateActorPosition(int actorId, float x, float z)
    ///             public void UpdateActorHealth (int actorId, int newHealth)
    ///             public void UpdateActorExperience(int actorId, int newExp)
    ///             public void UpdateActorSpeed(int actorId, int newSpeed)
    ///             public void UseTargetedAbility(int actorId, AbilityType abilityId, int targetId, int collisionId)
    ///             public void UseAreaAbility(int actorId, AbilityType abilityId, float x, float z, int collisionId)
    ///             public void ProcessCollision(AbilityType abilityId, int actorHitId, int actorCastId, int collisionId)
    ///             public void SpawnActor(ActorType actorType, int ActorId, int actorTeam, float x, float z)
    ///             public void SetActorMovement(int actorId, float x, float z, float targetX, float targetZ)
    ///             public void SetReady(int clientId, bool ready, int team)
    ///             public void StartGame (int playerNum)
    ///             public void EndGame(int winningTeam)
    ///             public void UpdateAbilityAssignment(int actorId, int abilityId)
    ///             public void UpdateLifeCount (List<RemainingLivesElement.LivesInfo> livesInfo)
    /// 
    /// DATE: 		March 5, 2019
    ///
    /// REVISIONS: 
    ///
    /// DESIGNER: 	Kieran Lee, Cameron Roberts
    ///
    /// PROGRAMMER: Kieran Lee, Cameron Roberts
    /// 
    /// NOTES:		Implementation of the iStateMessageBridge in the c# networking library
    ///				
    /// ----------------------------------------------
	public class ServerStateMessageBridge : IStateMessageBridge
	{
        State state;
        GameState gamestate;
        ClientManager clientmanager;
		readonly CollisionBuffer collisionBuffer;
		private static Logger Log = Logger.Instance;

        public ServerStateMessageBridge (State state)
		{
            this.state = state;
            gamestate = state.GameState;
            clientmanager = state.ClientManager;
			collisionBuffer = gamestate.CollisionBuffer;
        }

        public void UpdateActorPosition(int actorId, float x, float z)
        {
            //TARGET POSITION FOR CLIENT TO MOVE TO.
			if (x == 0 && z == 0) {
				return;
			}
			//Console.WriteLine ("Moved actor {0} to x={1} z={2}", actorId, x, z);
            gamestate.UpdateTargetPosition(actorId, x, z);

        }

        public void UpdateActorHealth (int actorId, int newHealth){
            //send to all clients
        }

        public void UpdateActorExperience(int actorId, int newExp)
        {

        }

        public void UpdateActorSpeed(int actorId, int newSpeed)
        {

        }

		public void UseTargetedAbility(int actorId, AbilityType abilityId, int targetId, int collisionId)
        {
			// Validate that the ability can be used by the actor
			if (gamestate.ValidateTargetedAbilityUse (actorId, abilityId, targetId)) {


				// Check if the ability is instantly applied and apply it if it is
				if (!AbilityInfo.InfoArray [(int)abilityId].RequiresCollision) {
                    Log.V("Instantly activating ability effects " + abilityId + " used by " + actorId + " on " + targetId);

                    gamestate.TriggerAbility (abilityId, targetId, actorId);
				}


				// Queue the ability use to be sent to all clients
				gamestate.OutgoingReliableElements.Enqueue (new TargetedAbilityElement (actorId, abilityId, targetId, gamestate.MakeCollisionId ()));
			} else {
                Log.V("Invalid ability use "+ abilityId + " by " + actorId + " on " + targetId);
			}
        }

		public void UseAreaAbility(int actorId, AbilityType abilityId, float x, float z, int collisionId)
        {
			// Validate that the ability can be used by the actor
			if (gamestate.ValidateAreaAbilityUse (actorId, abilityId, x, z)) {

				if (!AbilityInfo.InfoArray [(int)abilityId].RequiresCollision) {
					gamestate.TriggerAbility (abilityId, actorId, x, z);
				}
				// Queue the ability use to be sent to all clients
				gamestate.OutgoingReliableElements.Enqueue (new AreaAbilityElement (actorId, abilityId, x, z, gamestate.MakeCollisionId ()));
			} else {
                Log.V("Invalid ability use " + abilityId + " by " + actorId + " on location " + x + " " + z);
			}
        }

		public void ProcessCollision(AbilityType abilityId, int actorHitId, int actorCastId, int collisionId)
        {
            Log.V("Received Collision " + abilityId + ", " + actorHitId + ", " + actorCastId);
			collisionBuffer.Insert(new CollisionItem(abilityId, actorHitId, actorCastId, collisionId));
        }

		public void SpawnActor(ActorType actorType, int ActorId, int actorTeam, float x, float z)
        {
            //send to all clients

        }

        public void SetActorMovement(int actorId, float x, float z, float targetX, float targetZ)
        {
            //send to all clients
        }

		public void SetReady(int clientId, bool ready, int team){

            state.ClientManager.Connections [clientId].Team = team;
            if (team != 0)
                state.ClientManager.Connections [clientId].Ready = ready;

			Log.V("Set ready status of " + clientId + " to " + ready + " on team " + team);
		}

		public void StartGame (int playerNum){

		}

		public void SetLobbyStatus(List<LobbyStatusElement.PlayerInfo> playerInfo){

		}

		public void EndGame(int winningTeam){

		}

		public void UpdateAbilityAssignment(int actorId, int abilityId){

		}

		public void UpdateLifeCount (List<RemainingLivesElement.LivesInfo> livesInfo){

		}
    }
}