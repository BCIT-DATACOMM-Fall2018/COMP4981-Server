using System;
using NetworkLibrary;

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
		}

		public void UseActorAbility (int actorId, int abilityId, int targetId, int x, int y){

		}

	}
}

