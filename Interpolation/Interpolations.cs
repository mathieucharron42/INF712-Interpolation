using System;
using System.Collections.Generic;
using System.Linq;

namespace Interpolation
{
    using InterpolationFunc = Func<List<Vector2D>, List<Vector2D>, double, Vector2D>;

    public static class Interpolations
    {
        public enum InterpolationMode
        {
            Linear,
            CalmullRom,
            BezierLineaire,
            BezierQuadratique,
            BezierCubique
        }

        public enum ControlPointContrainst
        {
            C0, C1, C2_Buggé, Custom
        }

        public static List<Vector2D> Chain(InterpolationFunc interpolationFunc, List<Vector2D> points, List<Vector2D> controlPoints, double min, double max, double interpolationRate)
        {
            List<Vector2D> interpolatedPoints = new List<Vector2D>();
            if (interpolationRate > 0.0001f)
            {
                for (double x = min; x <= max; x += interpolationRate)
                {
                    Vector2D interpolatedPoint = interpolationFunc(points, controlPoints, x);
                    interpolatedPoints.Add(interpolatedPoint);
                }
            }
            return interpolatedPoints;
        }

        public static List<ControlPointContrainst> GetPossibleConstrainsts(InterpolationMode mode)
        {
            List<ControlPointContrainst> Contraints = new List<ControlPointContrainst>();

            Contraints.Add(ControlPointContrainst.Custom);

            if (mode == InterpolationMode.Linear)
            {
                Contraints.Add(ControlPointContrainst.C0);
            }
            else if (mode == InterpolationMode.CalmullRom)
            {
                Contraints.Add(ControlPointContrainst.C0);
                Contraints.Add(ControlPointContrainst.C1);
            }
            else if (mode == InterpolationMode.BezierLineaire)
            {
                Contraints.Add(ControlPointContrainst.C0);
            }
            else if (mode == InterpolationMode.BezierQuadratique)
            {
                Contraints.Add(ControlPointContrainst.C0);
                Contraints.Add(ControlPointContrainst.C1);
            }
            else if (mode == InterpolationMode.BezierCubique)
            {
                Contraints.Add(ControlPointContrainst.C0);
                Contraints.Add(ControlPointContrainst.C1);
                // Bugged :(
                 Contraints.Add(ControlPointContrainst.C2_Buggé);
            }

            return Contraints;
        }

        public static Vector2D Linear(List<Vector2D> points, List<Vector2D> controlPoints, double x)
        {
            // https://en.wikipedia.org/wiki/Linear_interpolation

            Vector2D prev;
            Vector2D next;

            int index;
            FindBounds(points, x, out index, out prev, out next);

            double p = (next.Y - prev.Y) / (next.X - prev.X);

            double y = prev.Y + (x - prev.X) * p;

            return new Vector2D(x, y);
        }

        public static Vector2D CatmulRom(List<Vector2D> points, List<Vector2D> controlPoints, double x)
        {
            // https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
            Vector2D p0;
            Vector2D p1;
            Vector2D p2;
            Vector2D p3;

            int index;
            FindBounds(points, x, out index, out p1, out p2);
            FindControlPoints(controlPoints, index, out p0, out p3);

            double t0 = 0.0f;
            double t1 = GetT(t0, p0, p1);
            double t2 = GetT(t1, p1, p2);
            double t3 = GetT(t2, p2, p3);

            double ratio = (x - p1.X) / (p2.X - p1.X);
            double t = t1 + (t2 - t1) * ratio;

            Vector2D a1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
            Vector2D a2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
            Vector2D a3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

            Vector2D b1 = (t2 - t) / (t2 - t0) * a1 + (t - t0) / (t2 - t0) * a2;
            Vector2D b2 = (t3 - t) / (t3 - t1) * a2 + (t - t1) / (t3 - t1) * a3;

            Vector2D c = (t2 - t) / (t2 - t1) * b1 + (t - t1) / (t2 - t1) * b2;

            return c;
        }

        public static Vector2D BezierLineaire(List<Vector2D> points, List<Vector2D> controlPoints, double x)
        {
            // https://en.wikipedia.org/wiki/Bézier_curve

            Vector2D prev;
            Vector2D next;

            int index;
            FindBounds(points, x, out index, out prev, out next);

            double t = (x - prev.X) / (next.X - prev.X);

            Vector2D b = prev + t * (next - prev);

            return b;
        }

        public static Vector2D BezierQuadratique(List<Vector2D> points, List<Vector2D> controlPoints, double x)
        {
            // https://en.wikipedia.org/wiki/Bézier_curve

            Vector2D p0;
            Vector2D p1;
            Vector2D p2;

            int index;
            FindBounds(points, x, out index, out p0, out p2);
            FindControlPoint(controlPoints, index, out p1);

            double t = (x - p0.X) / (p2.X - p0.X);

            Vector2D b2 = Math.Pow(1 - t, 2) * p0;
            Vector2D b1 = 2 * (1 - t) * t * p1;
            Vector2D b0 = Math.Pow(t, 2) * p2;

            return b2 + b1 + b0;
        }

        public static Vector2D BezierCubique(List<Vector2D> points, List<Vector2D> controlPoints, double x)
        { // https://en.wikipedia.org/wiki/Bézier_curve

            Vector2D p0;
            Vector2D p1;
            Vector2D p2;
            Vector2D p3;

            int index;
            FindBounds(points, x, out index, out p0, out p3);
            FindControlPoints(controlPoints, index, out p1, out p2);

            double t = (x - p0.X) / (p3.X - p0.X);

            Vector2D b3 = Math.Pow(1 - t, 3) * p0;
            Vector2D b2 = 3 * Math.Pow(1 - t, 2) * t * p1;
            Vector2D b1 = 3 * (1-t) * Math.Pow(t, 2) * p2;
            Vector2D b0 = Math.Pow(t, 3) * p3;

            return b3 + b2 + b1 + b0;
        }

        public static List<Vector2D> GetControlPoints(List<Vector2D> points, Interpolations.InterpolationMode interpolationMode, ControlPointContrainst constraint)
        {
            List<Vector2D> controlPoints = new List<Vector2D>();
            if (interpolationMode == Interpolations.InterpolationMode.CalmullRom)
            {
                // Catmull-Rom requiert 2 points de contrôle p0 et p3 pour la forme de la courbe.
                for (int i = 0; i < points.Count - 1; ++i)
                {
                    Vector2D p1 = points[i];
                    Vector2D p2 = points[i + 1];

                    Vector2D p0;
                    Vector2D p3;

                    if(constraint == ControlPointContrainst.C0)
                    {
                        // Pour continuité C0, p1 = o2 et p2 = q1
                        // p0 = anything
                        // p3 = anything
                        p0 = p1 + new Vector2D(0, 1);
                        p3 = p2 - new Vector2D(2, 2);
                    }
                    else if (constraint == ControlPointContrainst.C1)
                    {
                        // Pour continuité C1, p0 = o2 et p3 = q1
                        Vector2D o1 = (i >= 1) ? points[i - 1] : points[0] - new Vector2D(1, 1);
                        Vector2D q1 = (i < points.Count - 2) ? (points[i + 2]) : (points.Last() + new Vector2D(1, 1));
                        p0 = o1;
                        p3 = q1;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }

                    controlPoints.Add(p0);
                    controlPoints.Add(p3);
                }
            }
            else if (interpolationMode == Interpolations.InterpolationMode.BezierQuadratique)
            {
                // Bezier quadratique requiert 1 point de contrôle p1.
                for (int i = 0; i < points.Count - 1; ++i)
                {
                    Vector2D p0 = points[i];
                    Vector2D p2 = points[i + 1];

                    Vector2D p1;
                    if (constraint == ControlPointContrainst.C0)
                    {
                        // Pour continuité C0, p0 == o2
                        // p1 = anything
                        p1 = p0 + new Vector2D(0, 1);
                    }
                    else if(constraint == ControlPointContrainst.C1)
                    {
                        // Pour continuité C1, C0 && p1 - p0 == o2 - o1

                        // p1 = 2*o2-o1
                        Vector2D o1 = controlPoints.Count > 0 ? controlPoints[controlPoints.Count - 1] : p0;
                        Vector2D o2 = p0;

                        p1 = 2 * o2 - o1;
                    }
                    else
                    {
                        throw new ArgumentException("Unknown type");
                    }

                    controlPoints.Add(p1);
                }
            }
            else if (interpolationMode == Interpolations.InterpolationMode.BezierCubique)
            {
                // Bezier cubique requiert 2 point de contrôle p1 et p2.
                for (int i = 0; i < points.Count - 1; ++i)
                {
                    Vector2D p0 = points[i];
                    Vector2D p3 = points[i + 1];

                    Vector2D p1;
                    Vector2D p2;
                    if (constraint == ControlPointContrainst.C0)
                    {
                        // Pour continuité C0, p0 == o3
                        // p1 = anything
                        // p2 = anything
                        p1 = p0 + new Vector2D(0, 1);
                        p2 = p3 - new Vector2D(2, 2);
                    }
                    else if (constraint == ControlPointContrainst.C1)
                    {
                        // Pour continuité C1, C0 && p1 - p0 == o3 - o2

                        // p1 = 2*o3-o2
                        // p2 = anything
                        Vector2D o2 = controlPoints.Count > 0 ? controlPoints[controlPoints.Count - 1] : p0;
                        Vector2D o3 = p0;

                        p1 = 2 * o3 - o2;
                        p2 = p3 - new Vector2D(2, 2);
                    }
                    else if(constraint == ControlPointContrainst.C2_Buggé)
                    {
                        // Pour continuité C2, C1 && p2 - 2*p1 + p0 == o3 - 2*o2 + o1

                        // #0      p0 == o3
                        // #1 p1 - p0 == o3 - o2
                        // #2 p2 - 2*p1 + p0 == o3 - 2*o2 + o1

                        // #1 p1 - o3 ==   o3 - o2
                        // #1      p1 == 2*o3 - o2 (DONE)
                        // #2 p2 - 2*p1 + o3 == o3 - 2*o2 + o1
                        // #2             p2 == o3 - 2*o2 + o1 + 2*p1 - o3
                        // #2             p2 == -2*o2 + o1 + 2*p1 (DONE)

                        // w2 = v1 + 4*v3 - 4*v2
                        // p2 = o1 + 4*o3 - 4*o2

                        // p1 = 2*o3-o2
                        // p2 = -2*o2+o1+2*p1
                        Vector2D o1 = controlPoints.Count > 1 ? controlPoints[controlPoints.Count - 2] : p0;
                        Vector2D o2 = controlPoints.Count > 0 ? controlPoints[controlPoints.Count - 1] : p0;
                        Vector2D o3 = p0;

                        p1 = 2 * o3 - o2;
                        p2 = (-2 * o2) + (o1) + (2 * p1);

                        // p2 - 2*p1 == - 2*o2 + o1
                        //     -2*p1 == -2*o2 + o1 - p2
                        //        p1 == o2 - (o1 / 2) - (p2 / 2)
                        //p2 = p3 + new Vector2D(2, 2);
                        //p1 = o2 - (o1 / 2) - (p2 / 2);
                    }
                    else
                    {
                        throw new ArgumentException("Unknown type");
                    }

                    controlPoints.Add(p1);
                    controlPoints.Add(p2);
                }
            }
            return controlPoints;
        }

        private static double GetT(double t, Vector2D p0, Vector2D p1)
        {
            const double alpha = 0.5;

            double d1 = Math.Pow((p1.X - p0.X), 2.0f);
            double d2 = Math.Pow((p1.Y - p0.Y), 2.0f);
            double d3 = Math.Pow(Math.Sqrt(d1 + d2), alpha);

            return d3 + t;
        }

        private static void FindBounds(List<Vector2D> points, double x, out int pivot, out Vector2D prev, out Vector2D next)
        {
            pivot = 0;
            for (int i = 0; i < points.Count - 2; ++i)
            {
                if (x > points[i + 1].X)
                {
                    pivot = i + 1;
                }
            }
            prev = points[pivot];
            next = points[pivot + 1];
        }

        private static void FindControlPoint(List<Vector2D> controlPoints, int index, out Vector2D p1)
        {
            const int kNumberOfControlPoints = 1;
            if (index * kNumberOfControlPoints < controlPoints.Count - 1)
            {
                p1 = controlPoints[index * kNumberOfControlPoints];
            }
            else
            {
                p1 = controlPoints.Last();
            }
        }

        private static void FindControlPoints(List<Vector2D> controlPoints, int index, out Vector2D p1, out Vector2D p2)
        {
            const int kNumberOfControlPoints = 2;
            if(index * kNumberOfControlPoints < controlPoints.Count - 1)
            {
                p1 = controlPoints[index * kNumberOfControlPoints];
                p2 = controlPoints[index * kNumberOfControlPoints + 1];
            }
            else
            {
                p1 = controlPoints.Last();
                p2 = controlPoints.Last();
            }
        }

        private static void FindBounds(List<Vector2D> points, double x, out Vector2D p0, out Vector2D p1, out Vector2D p2, out Vector2D p3)
        {
            int pivot = 1;
            for (int i = 1; i < points.Count - 3; ++i)
            {
                if (x > points[i + 1].X)
                {
                    pivot = i + 1;
                }
            }
            p0 = points[pivot - 1];
            p1 = points[pivot];
            p2 = points[pivot + 1];
            p3 = points[pivot + 2];
        }
    }
}
