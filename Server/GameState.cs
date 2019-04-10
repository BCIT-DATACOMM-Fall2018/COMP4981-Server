/*---------------------------------------------------------------------------------------
--	SOURCE FILE:		GameState.cs - Game state of the game
--
--	PROGRAM:		    program
--
--  Mar 28, 2019	added logger, total game timer, both team loses logic, timer cleanup
--
---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NetworkLibrary;
using NetworkLibrary.MessageElements;
using Server;
using System.Timers;

namespace GameStateComponents
{
    /// -------------------------------------------------------------------------------------------
    /// Class:          GameState - Tracks all gamestate parameters and alters gamestate.
    /// 
    /// PROGRAM:        Server
    ///
    ///	CONSTRUCTORS:	public GameState ()
    /// 
    /// FUNCTIONS:	    public void StartGamePlayTimer ()
    ///                 public void EndGamePlayTimer ()
    ///                 private static void OnTimedEvent (Object source, ElapsedEventArgs e)
    ///                 public bool CheckWinCondition ()
    ///                 public int CheckWinningTeam ()
    ///                 public int MakeCollisionId ()
    ///                 public int AddPlayer (int team)
    ///                 private ActorType RandomHuman ()
    ///                 private ActorType RandomOrc ()
    ///                 public int AddCreep (int team)
    ///                 public int AddTower(GameUtility.Coordinate spawnLoc)
    ///                 public void UpdateHealth(int actorId, int health)
    ///                 public void UpdatePosition(int actorId, float x, float z)
    ///                 public void UpdatePosition(int actorId, GameUtility.Coordinate position)
    ///                 public void UpdateTargetPosition(int actorId, float x, float z)
    ///                 public int GetHealth(int actorId)
    ///                 public GameUtility.Coordinate GetPosition(int actorId)
    ///                 public GameUtility.Coordinate GetTargetPosition(int actorId)
    ///                 public void TickAllActors(State state)
    ///                 public bool ValidateTargetedAbilityUse(int useActorId, AbilityType abilityId, int targetActorId)
    ///                 public bool ValidateAreaAbilityUse(int useActorId, AbilityType abilityId, float x, float z)
    ///                 public void TriggerAbility(AbilityType abilityType, int actorHitId, int actorCastId)
    ///                 public void addEXP(Player killerPlayer, bool isKillPlayer)
    ///                 public void addExp(Player player, int exp)
    ///                 public int getClosestEnemyActorInRange(int actorId, int distance)
    ///                 public void TriggerAbility(AbilityType abilityType, int actorCastId, float x, float z)
    ///
    /// DATE: 		    April 8, 2019
    ///
    /// REVISIONS: 
    ///
    /// DESIGNER: 	    Wayne Huang
    ///
    /// PROGRAMMER:     Wayne Huang
    ///
    /// NOTES:		    Manages only parameters regarding the game state and methods to update
    ///                 the gamestate given events and new client data each tick. 
    /// -------------------------------------------------------------------------------------------
    public class GameState
	{
        // GameState attributes
		private const int MAXTEAMS = 8;
		private const int GAMEPLAY_TIME = 1200000;
		//This means 20 mins for a game
		private static System.Timers.Timer aTimer;
		//Game Play Timer added
		private static int currentTime = 0;
		//start at 0 second
		public ConcurrentDictionary<int, Actor> actors { get; private set; }


		private List<Actor>[] teamActors;

		public int CreatedActorsCount { get; private set; } = 0;

		public int CreatedPlayersCount { get; private set; } = 0;
		private static Logger Log = Logger.Instance;
		public List<Tower> Towers;
		public ConcurrentQueue<UpdateElement> OutgoingReliableElements { get; private set; }
		public CollisionBuffer CollisionBuffer { get; private set; }

		private int CollisionIdCounter;
		private const int COLLISION_ID_MAX = 255;
		private const int KILL_PLAYER_EXP = 64;
		private const int KILL_TOWER_EXP = 128;


		public int[] teamLives;

        // GameState constructor
		public GameState()
		{
			Towers = new List<Tower> ();
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
			Log.D("Game State created.");
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

		public bool CheckWinCondition ()
		{
			try{
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
						eliminated [i] = true;
					}
				}
				int remainingTeams = 0;
				for (int i = 0; i < eliminated.Length; i++) {
					if (!eliminated [i]) {
						remainingTeams++;
					}
				}

				if (currentTime >= GAMEPLAY_TIME) {
					eliminated [0] = false;
					for (int i = 1; i < eliminated.Length; i++) { //skip team 0 since that's server
						eliminated [i] = true;
					}
					return true;
				}

				return remainingTeams == 1;
			} catch (KeyNotFoundException e) {
				return false;
			}
		}

		public int CheckWinningTeam ()
		{
			for (int i = 1; i < MAXTEAMS; i++) {
				bool eliminated = false;
				bool allDead = true;
				foreach (var actor in teamActors[i]) {
					allDead &= actor.Health == 0 && !actor.RespawnAllowed;
				}
				if (allDead) {
					eliminated = teamLives [i] <= 0;
				}
				if (teamActors [i].Count == 0) {
					eliminated = true;
				}
				if (!eliminated) {
					return i;
				}
			}
			return 0;
		}

		public int MakeCollisionId(){
			Log.V("New ability with collision id " + (CollisionIdCounter + 1));
			return CollisionIdCounter++ % COLLISION_ID_MAX;
		}

		public int AddPlayer (int team)
		{
			int actorId = CreatedActorsCount++;
			CreatedPlayersCount++;
			Player newPlayer;
			if (team == 2) {
				newPlayer = new Player (actorId, team, new GameUtility.Coordinate (250, 70));
			} else {
				newPlayer = new Player (actorId, team, new GameUtility.Coordinate (250, 430));
			}
			if (!actors.TryAdd (actorId, newPlayer)) {
				//TODO Handle failure
			}
			teamActors [team].Add (newPlayer);
			Log.V("Adding player actor with id " + actorId);
			ActorType actortype;
			if (team == 1) {
				actortype = RandomHuman ();
			} else {
				actortype = RandomOrc ();
			}

			OutgoingReliableElements.Enqueue (new SpawnElement (actortype, actorId, newPlayer.Team, newPlayer.SpawnLocation.x, newPlayer.SpawnLocation.z));
			return actorId;
		}

		private ActorType RandomHuman ()
		{
			return (ActorType)GameUtility.RandomNum (0, 5);
		}


		private ActorType RandomOrc ()
		{
			return (ActorType)GameUtility.RandomNum (5, 10);
		}


		public int AddTower (GameUtility.Coordinate spawnLoc)
		{
			int actorId = CreatedActorsCount++;
			int team = 0;
			Tower newTower = new Tower (actorId, team, spawnLoc);
			if (!actors.TryAdd (actorId, newTower)) {
				//TODO Handle failure
			}
            Log.V("Adding tower actor with id " + actorId);
            OutgoingReliableElements.Enqueue(new SpawnElement(ActorType.TowerA, actorId, team, newTower.SpawnLocation.x, newTower.SpawnLocation.z));
            return actorId;
        }

		public void UpdateHealth (int actorId, int health)
		{
			try {
				actors [actorId].Health = health;
			} catch (KeyNotFoundException e) {

			}
		}

		public void UpdatePosition (int actorId, float x, float z)
		{
			try {
				actors [actorId].Position = new GameUtility.Coordinate (x, z);
			} catch (KeyNotFoundException e) {

			}

		}

		public void UpdatePosition (int actorId, GameUtility.Coordinate position)
		{
			try {
				actors [actorId].Position = position;
			} catch (KeyNotFoundException e) {

			}
		}

		public void UpdateTargetPosition (int actorId, float x, float z)
		{
			try {
				actors [actorId].TargetPosition = new GameUtility.Coordinate (x, z);
			} catch (KeyNotFoundException e) {

			}
		}

		public int GetHealth (int actorId)
		{
			try {
				return actors [actorId].Health;
			} catch (KeyNotFoundException e) {
				return 0;
			}
		}

		public GameUtility.Coordinate GetPosition (int actorId)
		{
			try {
				return actors [actorId].Position;
			} catch (KeyNotFoundException e) {
				return new GameUtility.Coordinate();
			}
		}

		public GameUtility.Coordinate GetTargetPosition (int actorId)
		{
			try {
				return actors [actorId].TargetPosition;
			} catch (KeyNotFoundException e) {
				return new GameUtility.Coordinate();
			}
		}

		public void TickAllActors (State state)
		{
			for (int i = 0; i < CreatedActorsCount; i++) {

				try {
					actors [i].Tick (state);

					if (actors [i].HasDied ()) {
						Log.V("Actor " + actors[i].ActorId + " has died");
						if (teamLives [actors [i].Team]-- > 0) {
							Log.V("Team " + actors[i].Team + " lives remaining " + teamLives[actors[i].Team]);

							actors [i].RespawnAllowed = true;
						}
					}
				} catch (KeyNotFoundException e) {

				}
			}
		}

		public bool ValidateTargetedAbilityUse (int useActorId, AbilityType abilityId, int targetActorId)
		{
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

			if (!(info.Range == 0) && !GameUtility.CoordsWithinDistance (useActor.Position, targetActor.Position, info.Range + 5)) {
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

			if (abilityId == AbilityType.Banish && targetActor.Stationary) {

				return false;
			}

			return useActor.UseAbility (abilityId);
		}

		public bool ValidateAreaAbilityUse (int useActorId, AbilityType abilityId, float x, float z)
		{
			AbilityInfo info = AbilityInfo.InfoArray [(int)abilityId];
			Actor useActor = actors [useActorId];

			if (!(info.Range == 0) && !GameUtility.CoordsWithinDistance (useActor.Position, new GameUtility.Coordinate (x, z), info.Range + 5)) {
				return false;
			}

			return useActor.UseAbility (abilityId);
		}

		public void TriggerAbility (AbilityType abilityType, int actorHitId, int actorCastId)
		{
			actors [actorCastId].ApplyAbilityEffects (abilityType, actors [actorHitId]);
		}

/*---------------------------------------------------------------------------------------
--  FUNCTION:   addExp
--
--  DATE:       March 28, 2019
--
--  REVISIONS:
--
--  DESIGNER:   Ziqian Zhang
--
--  PROGRAMMER: Ziqian Zhang
--
--  INTERFACE:  public void addEXP (Player killerPlayer, bool isKillPlayer)
--                  killerPlayer: The play who kill the enemy
--                  isKillPlayer: true: kill a player
--                                false: kill a tower       
--
--  RETURNS:    void
--
--  NOTES:  This function add EXP to player who get the kill, and all other 
--          team member with half EXP.
--
---------------------------------------------------------------------------------------*/
        public void addEXP (Player killerPlayer, bool isKillPlayer)
		{//if kill by player, true; if kill by tower , false

			int expAdded = isKillPlayer ? KILL_PLAYER_EXP : KILL_TOWER_EXP;

			for (int i = 0; i < CreatedActorsCount; i++) {
				if (actors [i].Team == killerPlayer.Team) {
					if (i == killerPlayer.ActorId)
						addExp ((Player)actors [i], expAdded);
					else {
						addExp ((Player)actors [i], expAdded / 2);
					}
				}
			}
		}
/*---------------------------------------------------------------------------------------
--  FUNCTION:   addExp
--
--  DATE:       March 28, 2019
--
--  REVISIONS:
--
--  DESIGNER:   Ziqian Zhang
--
--  PROGRAMMER: Ziqian Zhang
--
--  INTERFACE:  public void addExp (Player player, int exp)
--                  player: the player who need to add EXP
--                  exp: the exp need to be added
--
--  RETURNS:    void
--
--  NOTES:  This is helper function to add exp to player
--
---------------------------------------------------------------------------------------*/

        public void addExp (Player player, int exp)
		{
			int preLevel = GameUtility.currentLevel (player.Experience);
			player.Experience += exp;
			int afterLevel = GameUtility.currentLevel (player.Experience);
            
			if (afterLevel > preLevel) {
				//levelUp, skill change
				player.Level++;
				int newSkillId = AbilityEffects.ReturnRandomAbilityId (player);
				while (player.HasAbility((AbilityType)newSkillId)){
					newSkillId = AbilityEffects.ReturnRandomAbilityId (player);
				}
				player.AddAbility(afterLevel, (AbilityType)newSkillId);
				OutgoingReliableElements.Enqueue (new AbilityAssignmentElement (player.ActorId, newSkillId));
			}
		}

		//get actor id of closest actor to the specified actor, within a certain distance
		//if no one within distance, return -1
		//does not consider friendly actors
		public int getClosestEnemyActorInRange (int actorId, int distance)
		{
			float minDistance;
			float tempDistance;
			int closestActor;

			minDistance = GameUtility.getDistance (actors [actorId].Position, actors [0].Position);
			closestActor = 0;
			for (int i = 1; i < CreatedActorsCount; i++) {
				if (actors [actorId].Team != actors [i].Team) { // also prevents considering distance to self
					if ((tempDistance = GameUtility.getDistance (actors [actorId].Position, actors [i].Position)) < minDistance) {
						minDistance = tempDistance;
						closestActor = i;
					}
				}
			}
			if (GameUtility.CoordsWithinDistance (actors [actorId].Position, actors [closestActor].Position, distance)) {
				return closestActor;
			}
			return -1;
            
		}

		public void TriggerAbility (AbilityType abilityType, int actorCastId, float x, float z)
		{
			if (abilityType == AbilityType.Blink) {
				actors [actorCastId].TargetPosition = new GameUtility.Coordinate (x, z);
				actors [actorCastId].Position = new GameUtility.Coordinate (x, z);

			}

		}
	}
}
