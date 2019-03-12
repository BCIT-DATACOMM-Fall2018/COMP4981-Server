using System;

namespace GameStateComponents
{
    public class Creep : Actor
    {

        public Creep(int actorId) : base(actorId)
        {
            Health = 25;
        }
    }
}