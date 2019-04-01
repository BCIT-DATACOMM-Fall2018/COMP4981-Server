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
        //public GameUtility()
        //{

        //}

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

		public static float AngleBetweenCoordinates(Coordinate c1, Coordinate c2){
			float xDiff = c2.x - c1.x;
			float yDiff = c2.z - c2.z;
			return (float)Math.Atan2 (yDiff, xDiff);
		}

		public static bool CoordsWithinDistance(Coordinate c1, Coordinate c2, float distance){
			return ((c1.x - c2.x) * (c1.x - c2.x) + (c1.z - c2.z) * (c1.z - c2.z)) <= distance * distance;
		}

        public static void killHappen(Player player, bool killPlayer) {
  
            //loop all the player,and add EXP
            //tower 2x EXP
            //player who get the kill 1x EXP
            //team get 1/2 EXP
        }

        public static void addExp(Player player, int exp) {
            int preLevel = currentLevel(player.Experience);
            player.Experience += exp;
            int afterLevel = currentLevel(player.Experience);

            if (preLevel > afterLevel) { 
                //levelUp, skill change
            }
        }
        public static int currentLevel(int exp) {
            if (exp < LEVEL1_EXP)
                return 1;
            if (exp < LEVEL2_EXP)
                return 2;
            if (exp < LEVEL3_EXP)
                return 3;
            return 4;

        }

    }
}