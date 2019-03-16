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

        public ServerStateMessageBridge ()
		{
            state = State.Instance;
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


        public void UseTargetedAbility(int actorId, AbilityType abilityId, int targetId)
        {
			// Validate that the ability can be used by the actor
			if (gamestate.ValidateAbilityUse (actorId, abilityId)) {

				// Check if the ability is instantly applied and apply it if it is
				if (!AbilityInfo.InfoArray [(int)abilityId].RequiresCollision) {
					Console.WriteLine ("Instantly activating ability effects {0} used by {1} on {2}", abilityId, actorId, targetId);
					gamestate.TriggerAbilityEffects (abilityId,targetId, actorId);
				}

				// Queue the ability use to be sent to all clients
				gamestate.OutgoingReliableElements.Enqueue(new TargetedAbilityElement(actorId, abilityId, targetId));
			}
        }

        public void UseAreaAbility(int actorId, AbilityType abilityId, float x, float z)
        {
			// Validate that the ability can be used by the actor
			if (gamestate.ValidateAbilityUse (actorId, abilityId)) {

				// Queue the ability use to be sent to all clients
				gamestate.OutgoingReliableElements.Enqueue(new AreaAbilityElement(actorId, abilityId, x, z));
			}
        }

        public void ProcessCollision(AbilityType abilityId, int actorHitId, int actorCastId)
        {
			Console.WriteLine ("Received Collision {0}, {1}, {2}", abilityId, actorHitId, actorCastId);
			collisionBuffer.Insert(new CollisionItem(abilityId, actorHitId, actorCastId));
        }

        public void SpawnActor(ActorType actorType, int ActorId, float x, float z)
        {
            //send to all clients 

        }

        public void SetActorMovement(int actorId, float x, float z, float targetX, float targetZ)
        {
            //send to all clients 
        }

		public void SetReady(int clientId, bool ready){
			state.ClientManager.Connections [clientId].Ready = ready;
			Console.WriteLine ("Set ready status of {0} to {1}", clientId, ready);
		}

		public void StartGame (int playerNum){

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
