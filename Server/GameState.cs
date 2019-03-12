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
		private int createdActorsCount = 0;
		public ConcurrentQueue<SpawnElement> SpawnQueue {get; private set;}

		public GameState ()
		{
			SpawnQueue = new ConcurrentQueue<SpawnElement> ();
		}

		public int AddPlayer ()
		{
			int actorId = createdActorsCount++;
			Player newPlayer = new Player (actorId);
			actors.Add (actorId, newPlayer);
			Console.WriteLine ("Adding player actor with id {0}", actorId);
			SpawnQueue.Enqueue (new SpawnElement (ActorType.AlliedPlayer, actorId, 0, 0));
			return actorId;
		}

		public void UpdateHealth (int actorId, int health)
		{
			((Player)actors [actorId]).Health = health;
		}

		public void UpdatePosition (int actorId, float x, float z)
		{
			
			actors [actorId].Position = GameUtility.coordinate (x, z);
		}

		public void UpdatePosition (int actorId, Actor.Location position)
		{
			actors [actorId].Position = position;
		}

		public void UpdateTargetPosition (int actorId, float x, float z)
		{
			actors [actorId].TargetPosition = new Actor.Location (x, z);
		}

		public int GetHealth (int actorId)
		{
			return ((Player)actors [actorId]).Health;
		}

		public Actor.Location GetPosition (int actorId)
		{
			return actors [actorId].Position;
		}

		public Actor.Location GetTargetPosition (int actorId)
		{
			return actors [actorId].Position;
		}
	}
}