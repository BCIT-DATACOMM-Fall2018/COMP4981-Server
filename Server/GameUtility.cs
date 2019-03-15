﻿using System;
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

        //public GameUtility()
        //{

        //}

        public static Coordinate FindNewCoordinate(Coordinate c1, Coordinate c2, float distance)
        {

			float x1 = c1.x;
			float x2 = c2.x;
			float y1 = c1.z;
			float y2 = c2.z;

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
    }

}