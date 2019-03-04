using System;
using NetworkLibrary;
using GameStateComponents;

namespace Server
{
	public class ServerStateMessageBridge : IStateMessageBridge
	{
		public ServerStateMessageBridge ()
		{
			

		}

		public void UpdateActorPosition (int actorId, double x, double y){

		}

		public void UpdateActorHealth (int actorId, int newHealth){
			Console.WriteLine("Actor Id: " + actorId + ", Health: " + newHealth);
            //get instance of the gameState
            GameState gamestate = GameState.Instance;
            gamestate.addPlayer();
            gamestate.updateHealth(0, newHealth);
        }

		public void UseActorAbility (int actorId, int abilityId, int targetId, int x, int y){

		}

	}
}

