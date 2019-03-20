using System;
using Server;

namespace GameStateComponents
{
    public class Creep : Actor
    {

		public Creep(int actorId, int team, GameUtility.Coordinate spawnLocation) : base(actorId, team, spawnLocation)
        {
            Health = 25;
        }
    }
}