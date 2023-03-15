using Interpolation;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace InterpolationViewer
{
    class DoubleClickManipulator : MouseManipulator
    {
        private InterpolationViewModel _model;

        public DoubleClickManipulator(InterpolationViewModel model, IPlotView plotView)
            : base(plotView)
        {
            _model = model;
        }

        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);

            DataPoint newDataPoint = _model.ControlSeries.InverseTransform(e.Position);

            List<Vector2D> newPoints;
            newPoints = new List<Vector2D>(_model.Points);
            newPoints.Add(new Vector2D(newDataPoint.X, newDataPoint.Y));

            _model.Points = newPoints;
            _model.PointsSetType = PointsSet.Type.Custom;
        }
    }
}
