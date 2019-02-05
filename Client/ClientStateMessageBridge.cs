using System;
using NetworkLibrary;

namespace Client
{
	public class ClientStateMessageBridge : IStateMessageBridge
	{
		public ClientStateMessageBridge ()
		{
			

		}

		public void UpdateActorPosition (int actorId, double x, double y){

		}

		public void UpdateActorHealth (int actorId, int newHealth){
			Console.WriteLine("Actor Id: " + actorId + ", Health: " + newHealth);
		}

		public void UseActorAbility (int actorId, int abilityId, int targetId, int x, int y){

		}

	}
}

