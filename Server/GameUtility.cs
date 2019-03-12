using System;
namespace Server
{
    public static class GameUtility
    {
        public struct coordinate
        {
            public float x;
            public float z;

            public coordinate(float x, float z)
            {
                this.x = x;
                this.z = z;
            }

        }

        //public GameUtility()
        //{

        //}

        public static coordinate findNewCoordinate(coordinate c1, coordinate c2, float distance)
        {
            float length = (float)Math.Sqrt((c2.x - c1.x) * (c2.x - c1.x) + (c1.z - c2.z * (c1.z - c2.z)));


            float ratioEndToNew = Math.Abs(distance / length);

            coordinate newCoordinate;

            newCoordinate.z = c1.z + Math.Abs(c2.z - c1.z) * ratioEndToNew;
            newCoordinate.x = c1.x + Math.Abs(c2.x - c1.x) * ratioEndToNew;


            return newCoordinate;

        }
    }

}