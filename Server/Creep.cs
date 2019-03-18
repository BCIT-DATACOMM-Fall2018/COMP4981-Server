using System;

namespace GameStateComponents
{
    public class Creep : Actor
    {

		public Creep(int actorId, int team) : base(actorId, team)
        {
            Health = 25;
        }
    }
}