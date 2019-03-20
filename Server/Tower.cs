using System;
using Server;

namespace GameStateComponents
{
    public class Tower : Actor
    {

		public Tower(int actorId, int team, GameUtility.Coordinate spawnLocation) : base(actorId, team, spawnLocation)
        {
            Health = 1000;
        }
    }
}