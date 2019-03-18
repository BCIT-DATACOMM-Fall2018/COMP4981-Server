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
		public CollisionBuffer CollisionBuffer { get; private set; }

		private int CollisionIdCounter;
		private const int COLLISION_ID_MAX = 255;

		public GameState ()
		{
			OutgoingReliableElements = new ConcurrentQueue<UpdateElement> ();
			CollisionBuffer = new CollisionBuffer (this);
		}

		public int MakeCollisionId(){
			Console.WriteLine("New ability with collision id {0}", CollisionIdCounter + 1);
			return CollisionIdCounter++ % COLLISION_ID_MAX;
		}

		public int AddPlayer (int team)
		{
			int actorId = CreatedActorsCount++;
			Player newPlayer = new Player (actorId, team);
			if (!actors.TryAdd (actorId, newPlayer)) {
				//TODO Handle failure
			}
			Console.WriteLine ("Adding player actor with id {0}", actorId);
			OutgoingReliableElements.Enqueue (new SpawnElement (ActorType.Player, actorId, newPlayer.Team, 310f, 90f));
			actors [actorId].Position = new GameUtility.Coordinate (310, 90);
			actors [actorId].TargetPosition = new GameUtility.Coordinate (310, 90);
			return actorId;
		}

		public int AddCreep(int team)
        {
            int actorId = CreatedActorsCount++;
			Creep newCreep = new Creep(actorId, team);
			if (!actors.TryAdd (actorId, newCreep)) {
				//TODO Handle failure
			}            
			Console.WriteLine("Adding creep actor with id {0}", actorId);
            //SpawnQueue.Enqueue(new SpawnElement(ActorType.AlliedPlayer, actorId, 0, 0));
            return actorId;
        }

		public int AddTower(int team)
        {
			int actorId = CreatedActorsCount++;
			Tower newTower = new Tower(actorId, team);
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

		public void TickAllActors(){
			for (int i = 0; i < CreatedActorsCount; i++) {
				actors [i].Tick ();
			}
		}

		public bool ValidateTargetedAbilityUse(int useActorId, AbilityType abilityId, int targetActorId){
			AbilityInfo info = AbilityInfo.InfoArray [(int)abilityId];

			// Only valid if it's targeted or self
			if (!(info.IsTargeted || info.IsSelf)) {
				return false;
			}
			// Not valid if the ability targets self and the use and target actor id are not the same
			if (info.IsSelf && useActorId != targetActorId) {
				return false;
			}

			Actor useActor = actors [useActorId];
			Actor targetActor = actors [targetActorId];

			// If the user and target are on the same and ally target isn't allowed it's invalid
			if (useActor.Team == targetActor.Team && !info.AllyTargetAllowed) {
				return false;
			}
			// If the user and target anr't on the same time and enemy target itn't allowed it's invalid
			if (useActor.Team != targetActor.Team && !info.EnemyTargetAllowed) {
				return false;
			}

			return useActor.UseAbility (abilityId);
		}

		public bool ValidateAreaAbilityUse(int useActorId, AbilityType abilityId, float x, float z){
			Actor useActor = actors [useActorId];
			return useActor.UseAbility (abilityId);
		}

		public void TriggerAbility(AbilityType abilityType, int actorHitId, int actorCastId){
			actors [actorCastId].ApplyAbilityEffects (abilityType, actors [actorHitId]);
		}
	}
}