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
		private Dictionary<int, Actor> actors = new Dictionary<int, Actor> ();
		public int CreatedActorsCount { get; private set;} = 0;
		public ConcurrentQueue<SpawnElement> SpawnQueue {get; private set;}

		public GameState ()
		{
			SpawnQueue = new ConcurrentQueue<SpawnElement> ();
		}

		public int AddPlayer ()
		{
			int actorId = CreatedActorsCount++;
			Player newPlayer = new Player (actorId);
			actors.Add (actorId, newPlayer);
			Console.WriteLine ("Adding player actor with id {0}", actorId);
			SpawnQueue.Enqueue (new SpawnElement (ActorType.AlliedPlayer, actorId, 0, 0));
			return actorId;
		}

        public int AddCreep()
        {
            int actorId = CreatedActorsCount++;
            Creep newCreep = new Creep(actorId);
            actors.Add(actorId, newCreep);
            Console.WriteLine("Adding creep actor with id {0}", actorId);
            //SpawnQueue.Enqueue(new SpawnElement(ActorType.AlliedPlayer, actorId, 0, 0));
            return actorId;
        }

        public int AddTower()
        {
			int actorId = CreatedActorsCount++;
            Tower newTower = new Tower(actorId);
            actors.Add(actorId, newTower);
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
	}
}