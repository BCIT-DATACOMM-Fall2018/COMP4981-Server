using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;
using System.Collections.Generic;


using GameStateComponents;

namespace Server
{
	public class ServerStateMessageBridge : IStateMessageBridge
	{
        State state;
        GameState gamestate;
        ClientManager clientmanager;
		readonly CollisionBuffer collisionBuffer;

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


		public void UseTargetedAbility(int actorId, AbilityType abilityId, int targetId, int collisionId)
        {
			// Validate that the ability can be used by the actor
			if (gamestate.ValidateTargetedAbilityUse (actorId, abilityId, targetId)) {


				// Check if the ability is instantly applied and apply it if it is
				if (!AbilityInfo.InfoArray [(int)abilityId].RequiresCollision) {
					Console.WriteLine ("Instantly activating ability effects {0} used by {1} on {2}", abilityId, actorId, targetId);
					gamestate.TriggerAbility (abilityId, targetId, actorId);
				}


				// Queue the ability use to be sent to all clients
				gamestate.OutgoingReliableElements.Enqueue (new TargetedAbilityElement (actorId, abilityId, targetId, gamestate.MakeCollisionId ()));
			} else {
				Console.WriteLine ("Invalid ability use {0} by {1} on {2}", abilityId, actorId, targetId);
			}
        }

		public void UseAreaAbility(int actorId, AbilityType abilityId, float x, float z, int collisionId)
        {
			// Validate that the ability can be used by the actor
			if (gamestate.ValidateAreaAbilityUse (actorId, abilityId, x, z)) {

				// Queue the ability use to be sent to all clients
				gamestate.OutgoingReliableElements.Enqueue (new AreaAbilityElement (actorId, abilityId, x, z, gamestate.MakeCollisionId ()));
			} else {
				Console.WriteLine ("Invalid ability use {0} by {1} on location {2},{3}", abilityId, actorId, x, z);
			}
        }

		public void ProcessCollision(AbilityType abilityId, int actorHitId, int actorCastId, int collisionId)
        {
			Console.WriteLine ("Received Collision {0}, {1}, {2}", abilityId, actorHitId, actorCastId);
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
			state.ClientManager.Connections [clientId].Ready = ready;

			Console.WriteLine ("Set ready status of {0} to {1} on team {2}", clientId, ready, team);
		}

		public void StartGame (int playerNum){

		}

		public void SetLobbyStatus(List<LobbyStatusElement.PlayerInfo> playerInfo){
			
		}

		public void EndGame(int winningTeam){

		}
        public void UpdateActorExperience(int actorId, int exp)
        {

        }

    }
}

//SERVER ONLY SENDS MOVEMENT (UNRELIABLE)
// -- calculate distance moved per tick along a straight line
// send updated postion + target position every tick
// if a player doesnt request movement then target pos will be same as current position

//HEALTH (UNRELIABLE)

//SPAWN (RELIABLE)
//send at start of game right now

//RECEIVE USE ABLILITY (EITHER)
//validate, then echo to every client
