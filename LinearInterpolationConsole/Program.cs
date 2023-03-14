using System;
using System.Collections.Generic;
using System.Linq;

namespace Interpolation
{
    class Program
    {
        static void Main(string[] args)
        {
            const float interpolationRate = 1.0f;

            List<Vector2D> points = PointsSet.GetSet(PointsSet.Type.Simple);

            double min = points.First().X;
            double max = points.Last().X;

            List<Vector2D> controlPoints = Interpolations.GetControlPoints(points, Interpolations.InterpolationMode.CalmullRom, Interpolations.ControlPointContrainst.C0);

            List<Vector2D> interpolatedPoints = Interpolations.Chain(Interpolations.CatmulRom, points, controlPoints, min, max, interpolationRate);

            foreach (Vector2D interpolatedPoint in interpolatedPoints)
            {
                Console.ForegroundColor = points.Contains(interpolatedPoint) ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.WriteLine(interpolatedPoint);
            }
        }
    }
}
