using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NetworkLibrary;
using NetworkLibrary.MessageElements;
using Server;

namespace GameStateComponents
{
	public class GameState
	{
		private ConcurrentDictionary<int, Actor> actors = new ConcurrentDictionary<int, Actor> ();
		public int CreatedActorsCount { get; private set;} = 0;
		public ConcurrentQueue<UpdateElement> OutgoingReliableElements {get; private set;}

		public GameState ()
		{
			OutgoingReliableElements = new ConcurrentQueue<UpdateElement> ();
		}

		public int AddPlayer ()
		{
			int actorId = CreatedActorsCount++;
			Player newPlayer = new Player (actorId);
			if (!actors.TryAdd (actorId, newPlayer)) {
				//TODO Handle failure
			}
			Console.WriteLine ("Adding player actor with id {0}", actorId);
			OutgoingReliableElements.Enqueue (new SpawnElement (ActorType.AlliedPlayer, actorId, 310f, 90f));
			actors [actorId].Position = new GameUtility.Coordinate (310, 90);
			actors [actorId].TargetPosition = new GameUtility.Coordinate (310, 90);
			return actorId;
		}

        public int AddCreep()
        {
            int actorId = CreatedActorsCount++;
            Creep newCreep = new Creep(actorId);
			if (!actors.TryAdd (actorId, newCreep)) {
				//TODO Handle failure
			}            
			Console.WriteLine("Adding creep actor with id {0}", actorId);
            //SpawnQueue.Enqueue(new SpawnElement(ActorType.AlliedPlayer, actorId, 0, 0));
            return actorId;
        }

        public int AddTower()
        {
			int actorId = CreatedActorsCount++;
            Tower newTower = new Tower(actorId);
			if (!actors.TryAdd (actorId, newTower)) {
				//TODO Handle failure
			}            
			Console.WriteLine("Adding tower actor with id {0}", actorId);
            //SpawnQueue.Enqueue(new SpawnElement(ActorType.AlliedPlayer, actorId, 0, 0));
            return actorId;
        }

        public void UpdateHealth (int actorId, int health)
		{
			actors [actorId].Health = health;
		}

		public void UpdatePosition (int actorId, float x, float z)
		{
			
			actors [actorId].Position = new GameUtility.Coordinate (x, z); //is this a memory leak waiting to happen?
		}

		public void UpdatePosition (int actorId, GameUtility.Coordinate position)
		{
			actors [actorId].Position = position;
		}

		public void UpdateTargetPosition (int actorId, float x, float z)
		{
			actors [actorId].TargetPosition = new GameUtility.Coordinate(x, z); //is this a memory leak waiting to happen?
		}

		public int GetHealth (int actorId)
		{
			return actors [actorId].Health;
		}

		public GameUtility.Coordinate GetPosition (int actorId)
		{
			return actors [actorId].Position;
		}

		public GameUtility.Coordinate GetTargetPosition (int actorId)
		{
			return actors [actorId].TargetPosition;
		}

		public void MoveActors(){
			for (int i = 0; i < CreatedActorsCount; i++) {
				actors [i].Move ();
			}
		}

		public bool ValidateAbilityUse(int actorId, AbilityType abilityId){
			return true;
		}
	}
}