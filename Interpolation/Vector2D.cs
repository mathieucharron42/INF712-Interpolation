using System;
using System.Globalization;

namespace Interpolation
{
    public struct Vector2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Length
        {
            get { return Math.Sqrt(LengthSquared); }
        }

        public double LengthSquared
        {
            get { return Math.Abs(Math.Pow(X, 2) + Math.Pow(Y, 2)); }
        }

        public Vector2D Normalized
        {
            get
            {
                double lenght = Length;
                return new Vector2D(X / lenght, Y / lenght);
            }
        }

        public override bool Equals(object other)
        {
            if (other is Vector2D)
            {
                Vector2D otherVector2D = (Vector2D)other;
                return otherVector2D.X == X && otherVector2D.Y == Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Vector2D operator +(Vector2D v1, Vector2D v2)
        {
            return new Vector2D() { X = v1.X + v2.X, Y = v1.Y + v2.Y };
        }

        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            return new Vector2D() { X = v1.X - v2.X, Y = v1.Y - v2.Y };
        }

        public static Vector2D operator *(Vector2D vector, double scalar)
        {
            return new Vector2D() { X = vector.X * scalar, Y = vector.Y * scalar };
        }

        public static Vector2D operator *(double scalar, Vector2D vector)
        {
            return new Vector2D() { X = vector.X * scalar, Y = vector.Y * scalar };
        }

        public static Vector2D operator /(Vector2D vector, double scalar)
        {
            return new Vector2D() { X = vector.X / scalar, Y = vector.Y / scalar };
        }

        public static Vector2D operator /(double scalar, Vector2D vector)
        {
            return new Vector2D() { X = vector.X / scalar, Y = vector.Y / scalar };
        }
        public override string ToString()
        {
            return string.Format("({0},{1})", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture));
        }
    }
}
