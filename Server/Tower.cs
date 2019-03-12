using System;

namespace GameStateComponents
{
    public class Tower : Actor
    {

        public Tower(int actorId) : base(actorId)
        {
            Health = 1000;
        }
    }
}