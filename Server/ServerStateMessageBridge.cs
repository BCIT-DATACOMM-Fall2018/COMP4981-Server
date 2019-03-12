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

        public ServerStateMessageBridge ()
		{
            state = State.Instance;
            gamestate = state.GameState;
            clientmanager = state.ClientManager; 
        }

        public void UpdateActorPosition(int actorId, float x, float z)
        {
            //TARGET POSITION FOR CLIENT TO MOVE TO.
			Console.WriteLine ("Moved actor {0} to x={1} z={2}", actorId, x, z);
            gamestate.UpdateTargetPosition(actorId, x, z);

            //CREATE MOVE FUNCTION FOR THE ACTOR TO DO THE MOVEMENT
            GameUtility.coordinate start = gamestate.GetPosition(actorId);
            GameUtility.coordinate target = gamestate.GetTargetPosition(actorId);
            GameUtility.coordinate result = GameUtility.findNewCoordinate(start, target, (float)0.11);
            gamestate.UpdatePosition(actorId, result);
        }

        public void UpdateActorHealth (int actorId, int newHealth){
            //send to all clients 
        }


        public void UseTargetedAbility(int actorId, AbilityType abilityType, int targetID)
        {
            Console.WriteLine("NOT YET IMPLEMENTED");
        }

        public void UseAreaAbility(int actorId, AbilityType abilityId, float x, float z)
        {
            Console.WriteLine("NOT YET IMPLEMENTED");
        }

        public void ProcessCollision(AbilityType abilityId, int actorHitId, int actorCastId)
        {
            Console.WriteLine("NOT YET IMPLEMENTED");
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

   
