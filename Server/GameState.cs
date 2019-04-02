using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NetworkLibrary;
using NetworkLibrary.MessageElements;
using Server;
using System.Timers;

namespace GameStateComponents
{
    public class GameState
    {
        private const int MAXTEAMS = 8;
        private const int GAMEPLAY_TIME = 1200000;  //This means 20 mins for a game
        private static System.Timers.Timer aTimer; //Game Play Timer added
        private static int currentTime = 0; //start at 0 second
        public ConcurrentDictionary<int, Actor> actors { get; private set; }
        private List<Actor>[] teamActors;
        public int CreatedActorsCount { get; private set; } = 0;
		public int CreatedPlayersCount { get; private set; } = 0;

        public ConcurrentQueue<UpdateElement> OutgoingReliableElements { get; private set; }
        public CollisionBuffer CollisionBuffer { get; private set; }

		private int CollisionIdCounter;
		private const int COLLISION_ID_MAX = 255;
        private const int KILL_PLAYER_EXP = 64;
        private const int KILL_TOWER_EXP = 128;


        private int[] teamLives;

        public GameState()
        {
            actors = new ConcurrentDictionary<int, Actor>();
            OutgoingReliableElements = new ConcurrentQueue<UpdateElement>();
            CollisionBuffer = new CollisionBuffer(this);
            teamLives = new int[MAXTEAMS];
            for (int i = 0; i < teamLives.Length; i++) {
                teamLives[i] = 5;
            }
            teamActors = new List<Actor>[MAXTEAMS];
            for (int i = 0; i < teamActors.Length; i++) {
                teamActors[i] = new List<Actor>();
            }
        }

        //Used for keeping track of game play timer, if 20 mins is up then both team loses
        public void StartGamePlayTimer() {
            aTimer = new System.Timers.Timer(1000); //every second we update game time
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true; //We want timer to only run once
            aTimer.Enabled = true;
        }

        public void EndGamePlayTimer()
        {
            aTimer.Dispose();
        }

        //Added for updating Game time for the game state. This runs on different thread by the C# Timer
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //We want to update the gameTime here 
            currentTime++;

        }

        public bool CheckWinCondition(){
			bool[] eliminated = new bool[MAXTEAMS];
			for (int i = 0; i < MAXTEAMS; i++) {
				bool allDead = true;
				foreach (var actor in teamActors[i]) {
					allDead &= actor.Health == 0 && !actor.RespawnAllowed;
				}
				if (allDead) {
					eliminated [i] = teamLives [i] <= 0;
				}
				if (teamActors [i].Count == 0) {
					eliminated[i] = true;
				}
			}
			int remainingTeams = 0;
			for (int i = 0; i < eliminated.Length; i++) {
				if (!eliminated [i]) {
					remainingTeams++;
				}
			}

            if (currentTime >= GAMEPLAY_TIME) {
                eliminated[0] = false;
                for (int i = 1; i < eliminated.Length; i++) //skip team 0 since that's server
                {
                    eliminated[i] = true;
                }
                return true;
            }
			//TODO Change to remainingTeams == 1
			return remainingTeams == 0;
		}

		public int MakeCollisionId(){
			Console.WriteLine("New ability with collision id {0}", CollisionIdCounter + 1);
			return CollisionIdCounter++ % COLLISION_ID_MAX;
		}

		public int AddPlayer (int team)
		{
			int actorId = CreatedActorsCount++;
			CreatedPlayersCount++;
			Player newPlayer = new Player (actorId, team, new GameUtility.Coordinate(310, 90));
			if (!actors.TryAdd (actorId, newPlayer)) {
				//TODO Handle failure
			}
			teamActors [team].Add (newPlayer);
			Console.WriteLine ("Adding player actor with id {0}", actorId);
			ActorType actortype;
			if(team == 1){
				actortype = RandomHuman ();
			} else{
				actortype = RandomOrc ();
			}

			OutgoingReliableElements.Enqueue (new SpawnElement (actortype, actorId, newPlayer.Team,  newPlayer.SpawnLocation.x, newPlayer.SpawnLocation.z));
			return actorId;
		}

		private ActorType RandomHuman(){
			Random rand = new Random ();
			return (ActorType)rand.Next (0, 5);;
		}


		private ActorType RandomOrc(){
			Random rand = new Random ();
			return (ActorType)rand.Next (5, 10);;
		}

		public int AddCreep(int team)
        {
            int actorId = CreatedActorsCount++;
			Creep newCreep = new Creep(actorId, team, new GameUtility.Coordinate(310, 90));
			if (!actors.TryAdd (actorId, newCreep)) {
				//TODO Handle failure
			}
			Console.WriteLine("Adding creep actor with id {0}", actorId);
            //SpawnQueue.Enqueue(new SpawnElement(ActorType.AlliedPlayer, actorId, 0, 0));
            return actorId;
        }

		public int AddTower(GameUtility.Coordinate spawnLoc)
        {
			int actorId = CreatedActorsCount++;
            int team = 0;
			Tower newTower = new Tower(actorId, team, spawnLoc);
			if (!actors.TryAdd (actorId, newTower)) {
				//TODO Handle failure
			}
			Console.WriteLine("Adding tower actor with id {0}", actorId);
            OutgoingReliableElements.Enqueue(new SpawnElement(ActorType.TowerA, actorId, team, newTower.SpawnLocation.x, newTower.SpawnLocation.z));
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

		public void TickAllActors(State state){
			for (int i = 0; i < CreatedActorsCount; i++) {
				actors [i].Tick (state);

				if (actors [i].HasDied ()) {
					Console.WriteLine ("Actor {0} has died", actors [i].ActorId);
					if (teamLives [actors [i].Team]-- > 0) {
						Console.WriteLine ("Team {0} lives remaining {1}", actors [i].Team, teamLives [actors [i].Team]);

						actors [i].RespawnAllowed = true;
					}
				}
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

			if (!(info.Range == 0) && !GameUtility.CoordsWithinDistance(useActor.Position, targetActor.Position, info.Range + 5)) {
				return false;
			}

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
			AbilityInfo info = AbilityInfo.InfoArray [(int)abilityId];
			Actor useActor = actors [useActorId];

			if (!(info.Range == 0) && !GameUtility.CoordsWithinDistance(useActor.Position, new GameUtility.Coordinate(x, z), info.Range + 5)) {
				return false;
			}

			return useActor.UseAbility (abilityId);
		}

		public void TriggerAbility(AbilityType abilityType, int actorHitId, int actorCastId){
			actors [actorCastId].ApplyAbilityEffects (abilityType, actors [actorHitId]);
		}


        public void addEXP(Player killerPlayer, bool isKillPlayer)
        {//if kill by player, true; if kill by tower , false

            int expAdded = isKillPlayer ? KILL_PLAYER_EXP : KILL_TOWER_EXP;

            for (int i = 0; i < CreatedActorsCount; i++)
            {
                if (actors[i].Team == killerPlayer.Team)
                {
                    if (i == killerPlayer.ActorId)
                        addExp((Player)actors[i], expAdded);
                    else
                    {
                        addExp((Player)actors[i], expAdded / 2);
                    }
                }
            }
        }

        public void addExp(Player player, int exp)
        {
            int preLevel = GameUtility.currentLevel(player.Experience);
            player.Experience += exp;
            int afterLevel = GameUtility.currentLevel(player.Experience);
            
            if (preLevel > afterLevel)
            {
                //levelUp, skill change
                int newSkillId = AbilityEffects.ReturnRandomAbilityId(player);
                OutgoingReliableElements.Enqueue(new AbilityAssignmentElement(player.ActorId, );
            }
        }

        //get actor id of closest actor to the specified actor, within a certain distance
        //if no one within distance, return -1
        //does not consider friendly actors
        public int getClosestEnemyActorInRange(int actorId, int distance)
        {
            float minDistance;
            float tempDistance;
            int closestActor;

            minDistance = GameUtility.getDistance(actors[actorId].Position, actors[0].Position);
            closestActor = 0;
            for (int i = 1; i < CreatedActorsCount; i++)
            {
                if (actors[actorId].Team != actors[i].Team) // also prevents considering distance to self
                {
                    if ((tempDistance = GameUtility.getDistance(actors[actorId].Position, actors[i].Position)) < minDistance)
                    {
                        minDistance = tempDistance;
                        closestActor = i;
                    }
                }
            }
            if (GameUtility.CoordsWithinDistance(actors[actorId].Position, actors[closestActor].Position, distance))
            {
                return closestActor;
            }
            return -1;
            
        }

		public void TriggerAbility(AbilityType abilityType, int actorCastId, float x, float z){
			if (abilityType == AbilityType.Blink) {
				actors [actorCastId].TargetPosition = new GameUtility.Coordinate(x, z);
				actors [actorCastId].Position = new GameUtility.Coordinate(x, z);

			}

		}
	}
}
