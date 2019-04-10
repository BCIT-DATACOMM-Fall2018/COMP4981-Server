using System;
using Server;

namespace GameStateComponents
{
    public class Creep : Actor
    {
        /// -------------------------------------------------------------------------------------------
        /// Class:          Creep - A class to create creep objects.
        /// 
        /// PROGRAM:        Server
        ///
        ///	CONSTRUCTORS:	public Creep(int actorId, int team, GameUtility.Coordinate spawnLocation)
        /// 
        /// FUNCTIONS:	    None
        ///
        /// DATE: 		    April 8, 2019
        ///
        /// REVISIONS: 
        ///
        /// DESIGNER: 	    Wayne Huang
        ///
        /// PROGRAMMER:     Wayne Huang
        ///
        /// NOTES:		    Extends from the Actor class.
        /// -------------------------------------------------------------------------------------------
        public Creep(int actorId, int team, GameUtility.Coordinate spawnLocation) : base(actorId, team, spawnLocation)
        {
            Health = 25;
            Attack = 1;
            Defense = 1;
        }
    }
}