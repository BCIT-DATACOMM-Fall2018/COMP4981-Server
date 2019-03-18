using System;

namespace GameStateComponents
{
    public class Tower : Actor
    {

		public Tower(int actorId, int team) : base(actorId, team)
        {
            Health = 1000;
        }
    }
}