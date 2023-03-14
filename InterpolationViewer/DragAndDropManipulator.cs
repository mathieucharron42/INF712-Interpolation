using Interpolation;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace InterpolationViewer
{
    class DragAndDropManipulator : MouseManipulator
    {
        private InterpolationViewModel _model;
        private TrackerHitResult _currentPoint;

        public DragAndDropManipulator(InterpolationViewModel model, IPlotView plotView)
            : base(plotView)
        {
            _model = model;
        }

        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);
            
            if (_currentPoint == null)
            {
                return;
            }

            Debug.WriteLine(String.Format("Release {0}", _currentPoint.ToString()));

            ScatterSeries currentSeries = _currentPoint.Series as ScatterSeries;
            if (currentSeries == null)
            {
                return;
            }

            if (_model.ControlSeries == currentSeries)
            {
                _model.Points = currentSeries.Points.ConvertAll(p => new Vector2D(p.X, p.Y));
                _model.PointsSetType = PointsSet.Type.Custom;
            }
            else if(_model.AdditionalControlSeries == currentSeries)
            {
                _model.AdditionalControlPoints = currentSeries.Points.ConvertAll(p => new Vector2D(p.X, p.Y));
                _model.ControlPointConstraint = Interpolations.ControlPointContrainst.Custom;
            }
            else
            {
                throw new Exception("Unhandled series");
            }
            _currentPoint = null;
            PlotView.HideTracker();

            PlotView.InvalidatePlot(true);

            _model.NotifyPropertyChanged("Plot");
        }

        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);

            if (_currentPoint == null)
            {
                return;
            }

            ScatterSeries currentSeries = _currentPoint.Series as ScatterSeries;
            if (currentSeries == null)
            {
                return;
            }

            DataPoint currentPosition = currentSeries.InverseTransform(e.Position);
            int currentPointIndex = (int)_currentPoint.Index;

            if (currentPointIndex < 0 && currentPointIndex >= currentSeries.Points.Count)
            {
                return;
            }

            currentSeries.Points[currentPointIndex] = new ScatterPoint(currentPosition.X, currentPosition.Y);
            TrackerHitResult point = currentSeries.GetNearestPoint(e.Position, false);
            PlotView.ShowTracker(point);
            PlotView.InvalidatePlot(true);
        }

        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);

            List<ScatterSeries> seriesOfInterest = new List<ScatterSeries> { _model.ControlSeries, _model.AdditionalControlSeries };

            foreach (ScatterSeries series in seriesOfInterest)
            {
                if (series != null)
                {
                    HitTestResult hit = series.HitTest(new HitTestArguments(e.Position, 10));
                    if (hit != null)
                    {
                        TrackerHitResult point = series.GetNearestPoint(e.Position, false);
                        if (point != null)
                        {
                            _currentPoint = point;
                            Debug.WriteLine(string.Format("Pick {0}", _currentPoint.ToString()));
                        }
                    }
                }
            }
        }
    }
}
