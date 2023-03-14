using System;
using System.Collections.Generic;

namespace Interpolation
{
    public static class PointsSet
    {
        public enum Type
        {
            Single,
            Simple,
            Convex,
            Intense,
            CornerCase,
        }

        public static List<Vector2D> GetSet(Type type)
        {
            if (type == Type.Single)
            {
                return new List<Vector2D>()
                {
                    new Vector2D(0, 0),
                    new Vector2D(3, 1),
                };
            }
            if (type == Type.Simple)
            {
                return new List<Vector2D>()
                {
                    new Vector2D(0, 0),
                    new Vector2D(3, 1),
                    new Vector2D(6, 6),
                    new Vector2D(8, 12),
                    new Vector2D(13, 15),
                };
            }
            else if(type == Type.Convex)
            {
                return new List<Vector2D>()
                { 
                    new Vector2D(0, 0),
                    new Vector2D(2, 4),
                    new Vector2D(4, 20),
                    new Vector2D(6, 50),
                    new Vector2D(8, 60),
                    new Vector2D(11, 50),
                    new Vector2D(14, 57),
                    new Vector2D(16, 62),
                };
            }
            else if(type == Type.Intense)
            {
                return new List<Vector2D>()
                {
                    new Vector2D(0, 0),
                    new Vector2D(2, 9),
                    new Vector2D(3, 32),
                    new Vector2D(6, -1),
                    new Vector2D(7, 60),
                    new Vector2D(12, 22),
                    new Vector2D(13, 29),
                };
            }
            else if (type == Type.CornerCase)
            {
                return new List<Vector2D>()
                {
                    new Vector2D(0, 0),
                    new Vector2D(2, 0.5),
                    new Vector2D(3, 90),
                    new Vector2D(4, 0),
                    new Vector2D(5, 60),
                    new Vector2D(8, 0),
                };
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
