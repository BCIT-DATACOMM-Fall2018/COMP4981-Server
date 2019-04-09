using System;
using GameStateComponents;

namespace Server
{
    public static class GameUtility
    {

        public struct Coordinate
        {
            public float x;
            public float z;

            public Coordinate(float x, float z)
            {
                this.x = x;
                this.z = z;
            }

        }
        const int LEVEL1_EXP = 128;
        const int LEVEL2_EXP = 256;
        const int LEVEL3_EXP = 512;

        const int KILL_EXP = 64;

		private static Random random = new Random();
		private static Object _randomLock = new object ();

		public static int RandomNum(int minValue, int maxValue){
			lock (_randomLock) {
				return random.Next (minValue, maxValue);
			}
		}

/*---------------------------------------------------------------------------------------
--  FUNCTION:   FindNewCoordinate
--
--  DATE:       March 25, 2019
--
--  REVISIONS:  March 28, 2019
--
--  DESIGNER:   Ziqian Zhang, Kieran Lee 
--
--  PROGRAMMER: Ziqian Zhang, Kieran Lee 
--
--  INTERFACE:  public static Coordinate FindNewCoordinate(Coordinate c1, Coordinate c2, float distance)
--                               c1: the first coordinate
--                               c2: the second coordinate.      
--                               distance: The distance at direction of c1 to c2.
--
--  RETURNS:    Coordinate: the new coordination.
--
--  NOTES:  This function are given 2 coordinate and at that direction distance to find a new coordinate.
--
---------------------------------------------------------------------------------------*/
        public static Coordinate FindNewCoordinate(Coordinate c1, Coordinate c2, float distance)
        {
			

			float x1 = c1.x;
			float x2 = c2.x;
			float y1 = c1.z;
			float y2 = c2.z;

			if(CoordsWithinDistance(c1, c2, distance)){
				return c2;
			}

			if (x1 == x2 && y1 == y2) {
				return c1;
			}

			float slope =(float) Math.Sqrt ((x2-x1)*(x2-x1) + (y2-y1)*(y2-y1));
			float x3;
			float y3;
			x3 = x1 + (distance / slope) * (x2 - x1);
			y3 = y1 + (distance / slope) * (y2 - y1);
			return new Coordinate (x3, y3);
        }
/*---------------------------------------------------------------------------------------
--  FUNCTION:   AngleBetweenCoordinates
--
--  DATE:       March 25, 2019
--
--  REVISIONS:  March 28, 2019
--
--  DESIGNER:   Ziqian Zhang, Kieran Lee 
--
--  PROGRAMMER: Ziqian Zhang, Kieran Lee 
--
--  INTERFACE:  public static float AngleBetweenCoordinates(Coordinate c1, Coordinate c2)
--                               c1: the first coordinate
--                               c2: the second coordinate.
--          
--                               
--
--  RETURNS:    float: the angle in float
--
--  NOTES:  This function are given 2 coordinate and find the angle between coordinates.
--
---------------------------------------------------------------------------------------*/
        public static float AngleBetweenCoordinates(Coordinate c1, Coordinate c2){
			float xDiff = c2.x - c1.x;
			float yDiff = c2.z - c2.z;
			return (float)Math.Atan2 (yDiff, xDiff);
		}

/*---------------------------------------------------------------------------------------
--  FUNCTION:   AngleBetweenCoordinates
--
--  DATE:       March 25, 2019
--
--  REVISIONS:  March 28, 2019
--
--  DESIGNER:   Ziqian Zhang, Kieran Lee 
--
--  PROGRAMMER: Ziqian Zhang, Kieran Lee 
--
--  INTERFACE:  public static bool CoordsWithinDistance(Coordinate c1, Coordinate c2, float distance)
--                               c1: the first coordinate
--                               c2: the second coordinate.   
--                               distance: The distance at direction of c1 to c2.
--                               
--
--  RETURNS:    bool:
--                  True: the new coordinate is within the 2 coordinate.
--                  False: the new coordinate is not within the 2 coordinate.
--
--  NOTES:  This function are given 2 coordinate and find the angle between coordinates.
--
---------------------------------------------------------------------------------------*/
        public static bool CoordsWithinDistance(Coordinate c1, Coordinate c2, float distance){
			return ((c1.x - c2.x) * (c1.x - c2.x) + (c1.z - c2.z) * (c1.z - c2.z)) <= distance * distance;
		}

/*---------------------------------------------------------------------------------------
--  FUNCTION:   currentLevel
--
--  DATE:       March 28, 2019
--
--  REVISIONS:  
--
--  DESIGNER:   Ziqian Zhang
--
--  PROGRAMMER: Ziqian Zhang
--
--  INTERFACE:  public static int currentLevel(int exp)
--                               exp: the exp of the player.
--                               
--
--  RETURNS:    int: the current level of the player
--                 
--
--  NOTES:  This function determine current level of the client.
--
---------------------------------------------------------------------------------------*/
        public static int currentLevel(int exp) {
            if (exp < LEVEL1_EXP)
                return 1;
            if (exp < LEVEL2_EXP)
                return 2;
            if (exp < LEVEL3_EXP)
                return 3;
            return 4;
        }

/*---------------------------------------------------------------------------------------
--  FUNCTION:   getDistance
--
--  DATE:       March 25, 2019
--
--  REVISIONS:  March 28, 2019
--
--  DESIGNER:   Ziqian Zhang, Kieran Lee 
--
--  PROGRAMMER: Ziqian Zhang, Kieran Lee 
--
--  INTERFACE:  public static float getDistance(Coordinate c1, Coordinate c2)
--                               c1: the first coordinate.
--                               c2: the second coordinate
--                               
--
--  RETURNS:    float: distance between the 2 coordinate.
--                 
--
--  NOTES:  The function calculate the distance between 2 coordinate.
--
---------------------------------------------------------------------------------------*/
        public static float getDistance(Coordinate c1, Coordinate c2)
        {
            return (float)Math.Sqrt(Math.Abs(((c1.x - c2.x)* (c1.x - c2.x)) + ((c1.z - c2.z) * (c1.z - c2.z))));
        }

    }
}