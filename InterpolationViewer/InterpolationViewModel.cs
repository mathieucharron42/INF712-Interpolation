using Interpolation;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace InterpolationViewer
{
    using InterpolationFunc = Func<List<Vector2D>, List<Vector2D>, double, Vector2D>;

    public class InterpolationViewModel : INotifyPropertyChanged
    {
        public enum PlotScaleType
        {
            OriginalPoints,
            InterpolatedPoints,
            All,
            Fixed
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public InterpolationViewModel()
        {
            InterpolationRate = 0.01f;
            InterpolationMode = Interpolations.InterpolationMode.Linear;
            PointsSetType = PointsSet.Type.Simple;
            PlotScale = PlotScaleType.OriginalPoints;
        }

        public PointsSet.Type PointsSetType
        {
            get
            {
                return _pointsSetType;
            }
            set
            {
                _pointsSetType = value;
                if (_pointsSetType != PointsSet.Type.Custom)
                {
                    Points = null;
                    ControlPointConstraint = BestControlPointConstraints;
                }
                NotifyPropertyChanged("PointsSetType");
                NotifyPropertyChanged("Plot");
            }
        }

        public List<Vector2D> Points
        {
            get
            {
                if (_points == null)
                {
                    return PointsSet.GetSet(PointsSetType);
                }
                else
                {
                    return _points;
                }
            }
            set
            {
                _points = value;
                NotifyPropertyChanged("Points");
                NotifyPropertyChanged("Plot");
            }
        }

        public Interpolations.InterpolationMode InterpolationMode
        {
            get
            {
                return _interpolationMode;
            }
            set
            {
                _interpolationMode = value;
                ControlPointConstraint = BestControlPointConstraints;
                NotifyPropertyChanged("InterpolationMode");
                NotifyPropertyChanged("ControlPointConstraint");
                NotifyPropertyChanged("ControlPointConstraints");
                NotifyPropertyChanged("Plot");
            }
        }
        
        public Interpolations.ControlPointContrainst ControlPointConstraint
        {
            get
            {
                return _controlPointConstraint;
            }
            set
            {
                _controlPointConstraint = value;
                if (_controlPointConstraint != Interpolations.ControlPointContrainst.Custom)
                {
                    AdditionalControlPoints = null;
                }
                NotifyPropertyChanged("ControlPointConstraint");
                NotifyPropertyChanged("Plot");
            }
        }

        public List<Vector2D> AdditionalControlPoints
        {
            get
            {
                if (_additionalControlPoints != null)
                {
                    return _additionalControlPoints;
                }
                else
                {
                    return Interpolations.GetControlPoints(Points, InterpolationMode, ControlPointConstraint);
                }
            }
            set
            {
                _additionalControlPoints = value;
                NotifyPropertyChanged("AdditionalControlPoints");
                NotifyPropertyChanged("Plot");
            }
        }

        public float InterpolationRate
        {
            get
            {
                return _interpolationRate;
            }
            set
            {
                _interpolationRate = value;
                NotifyPropertyChanged("InterpolationRate");
                NotifyPropertyChanged("Plot");
            }
        }

        public PlotScaleType PlotScale
        {
            get
            {
                return _plotScale;
            }
            set
            {
                _plotScale = value;
                NotifyPropertyChanged("PlotScale");
                NotifyPropertyChanged("Plot");
            }
        }

        public ScatterSeries ControlSeries
        {
            get
            {
                return _controlSeries;
            }
        }

        public ScatterSeries AdditionalControlSeries
        {
            get
            {
                return _additionalControlSeries;
            }
        }

        public PlotModel Plot
        {
            get
            {
                List<Vector2D> points = Points;

                double inputMin = points.First().X;
                double inputMax = points.Last().X;

                List<Vector2D> controlPoints = AdditionalControlPoints;

                InterpolationFunc interpolationFunction = kInterpolationModeMapping[InterpolationMode];

                List<Vector2D> interpolatedPoints = Interpolations.Chain(interpolationFunction, points, controlPoints, inputMin, inputMax, InterpolationRate);

                PlotModel model = new PlotModel { Title = string.Format("Interpolation {0} {1}", InterpolationMode, ControlPointConstraint) };
                ScatterSeries interpolatedSeries = new ScatterSeries();
                interpolatedSeries.MarkerType = MarkerType.Circle;
                interpolatedSeries.MarkerSize = 5;
                foreach (Vector2D point in interpolatedPoints)
                {
                    interpolatedSeries.Points.Add(new ScatterPoint(point.X, point.Y));
                }
                interpolatedSeries.Title = "Valeurs interpolée";
                interpolatedSeries.MarkerFill = OxyColor.FromRgb(0, 255, 0);

                _controlSeries = new ScatterSeries();
                _controlSeries.MarkerType = MarkerType.Circle;
                _controlSeries.MarkerSize = 5;
                foreach (Vector2D point in points)
                {
                    _controlSeries.Points.Add(new ScatterPoint(point.X, point.Y));
                }
                _controlSeries.Title = "Point de contrôle de passage";
                _controlSeries.MarkerFill = OxyColor.FromRgb(255, 0, 0);

                _additionalControlSeries = new ScatterSeries();
                _additionalControlSeries.MarkerType = MarkerType.Triangle;
                _additionalControlSeries.MarkerSize = 5;

                foreach (Vector2D point in controlPoints)
                {
                    _additionalControlSeries.Points.Add(new ScatterPoint(point.X, point.Y));
                }

                _additionalControlSeries.Title = "Point de contrôle additionel";
                _additionalControlSeries.MarkerFill = OxyColor.FromRgb(0, 0, 255);

                model.Series.Add(interpolatedSeries);
                model.Series.Add(_controlSeries);
                model.Series.Add(_additionalControlSeries);
                model.IsLegendVisible = false;

                
                double minimumX = 0;
                double maximumX = 0;
                double minimumY = 0;
                double maximumY = 0;
                if (PlotScale == PlotScaleType.OriginalPoints)
                {
                    minimumX = points.Min(p => p.X);
                    maximumX = points.Max(p => p.X);

                    minimumY = points.Min(p => p.Y);
                    maximumY = points.Max(p => p.Y);
                }
                else if(PlotScale == PlotScaleType.InterpolatedPoints)
                {
                    minimumX = interpolatedPoints.Min(p => p.X);
                    maximumX = interpolatedPoints.Max(p => p.X);

                    minimumY = interpolatedPoints.Min(p => p.Y);
                    maximumY = interpolatedPoints.Max(p => p.Y);
                }

                if (PlotScale != PlotScaleType.All)
                {
                    model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = minimumX - 2, Maximum = maximumX + 2 });
                    model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = minimumY - 2, Maximum = maximumY + 2 });
                }

                return model;
            }
        }

        public PlotController Controller
        {
            get
            {
                PlotController controller = new PlotController();
                controller.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                    (view, _, args) => { controller.AddMouseManipulator(view, new DragAndDropManipulator(this, view), args); }
                ));
                return controller;
            }
        }

        public List<Interpolations.InterpolationMode> InterpolationModes
        {
            get
            {
                return Enum.GetValues(typeof(Interpolations.InterpolationMode)).Cast<Interpolations.InterpolationMode>().ToList();
            }
        }

        public List<PointsSet.Type> PointsSetTypes
        {
            get
            {
                List<PointsSet.Type> list = Enum.GetValues(typeof(PointsSet.Type)).Cast<PointsSet.Type>().ToList();
                return list;
            }
        }

        public List<Interpolations.ControlPointContrainst> ControlPointConstraints
        {
            get
            {
                List<Interpolations.ControlPointContrainst> list = Interpolations.GetPossibleConstrainsts(InterpolationMode);
                return list;
            }
        }

        public Interpolations.ControlPointContrainst BestControlPointConstraints
        {
            get
            {
                return ControlPointConstraints.Last(cc => cc != Interpolations.ControlPointContrainst.C2_Buggé && cc != Interpolations.ControlPointContrainst.Custom);
            }
        }

        public List<PlotScaleType> PlotScaleTypes
        {
            get
            {
                return Enum.GetValues(typeof(PlotScaleType)).Cast<PlotScaleType>().ToList();
            }
        }

        private static Dictionary<Interpolations.InterpolationMode, InterpolationFunc> kInterpolationModeMapping = new Dictionary<Interpolations.InterpolationMode, InterpolationFunc>()
        {
            { Interpolations.InterpolationMode.Linear, Interpolations.Linear },
            { Interpolations.InterpolationMode.CalmullRom, Interpolations.CatmulRom },
            { Interpolations.InterpolationMode.BezierLineaire, Interpolations.BezierLineaire },
            { Interpolations.InterpolationMode.BezierQuadratique, Interpolations.BezierQuadratique },
            { Interpolations.InterpolationMode.BezierCubique, Interpolations.BezierCubique },
        };

        private PointsSet.Type _pointsSetType;
        private List<Vector2D> _points;
        private Interpolations.InterpolationMode _interpolationMode;
        private Interpolations.ControlPointContrainst _controlPointConstraint;
        private List<Vector2D> _additionalControlPoints;
        private float _interpolationRate;
        private PlotScaleType _plotScale;

        private ScatterSeries _controlSeries;
        private ScatterSeries _additionalControlSeries;
    }
}
