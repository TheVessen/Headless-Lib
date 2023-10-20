using System;
using Rhino.Geometry;
using SkiaSharp;

namespace Headless.Lib
{
    public static class RhCurveToSkia
    {
        public static SKPath ConvertNurbsToSkia(Curve curve)
        {
            var path = new SKPath();

            var bezier = BezierCurve.CreateCubicBeziers(curve, 0.1, 0.1);

            foreach (var b in bezier)
            {
                var controlPoints = b.ToNurbsCurve().Points;

                // Start at the first point
                path.MoveTo(
                    (float)controlPoints[0].Location.X, 
                    -(float)controlPoints[0].Location.Y  // negate Y-coordinate
                );

                // Add a cubic Bezier segment
                path.CubicTo(
                    (float)controlPoints[1].Location.X, -(float)controlPoints[1].Location.Y,
                    (float)controlPoints[2].Location.X, -(float)controlPoints[2].Location.Y,
                    (float)controlPoints[3].Location.X, -(float)controlPoints[3].Location.Y
                );
            }

            // If the curve is closed, then close the path as well
            if (curve.IsClosed)
            {
                path.Close();
            }

            return path;
        }
        
        public static SKPath ConvertNurbsToSkia(PolyCurve curve)
        {
            var path = new SKPath();

            var bezier = BezierCurve.CreateCubicBeziers(curve, 0.1, 0.1);

            foreach (var b in bezier)
            {
                var controlPoints = b.ToNurbsCurve().Points;

                // Start at the first point
                path.MoveTo(
                    (float)controlPoints[0].Location.X, 
                    -(float)controlPoints[0].Location.Y  // negate Y-coordinate
                );

                // Add a cubic Bezier segment
                path.CubicTo(
                    (float)controlPoints[1].Location.X, -(float)controlPoints[1].Location.Y,
                    (float)controlPoints[2].Location.X, -(float)controlPoints[2].Location.Y,
                    (float)controlPoints[3].Location.X, -(float)controlPoints[3].Location.Y
                );
            }

            // If the curve is closed, then close the path as well
            if (curve.IsClosed)
            {
                path.Close();
            }

            return path;
        }
        
        public static SKPath ConvertPolylineToSkiaPath(PolylineCurve polylineCurve)
        {
            if (polylineCurve == null)
                throw new ArgumentNullException("polylineCurve");

            var path = new SKPath();

            if (polylineCurve.PointCount > 0)
            {
                Point3d startPt = polylineCurve.Point(0);
                path.MoveTo((float)startPt.X, -(float)startPt.Y);
        
                for (int i = 1; i < polylineCurve.PointCount; i++)
                {
                    Point3d pt = polylineCurve.Point(i);
                    path.LineTo((float)pt.X, -(float)pt.Y);
                }

                if (polylineCurve.IsClosed)
                    path.Close();
            }

            return path;
        }
        
        public static SKPath ConvertArcCurveToSkiaPath(ArcCurve arcCurve)
        {
            if (arcCurve == null)
                throw new ArgumentNullException("arcCurve");

            var path = new SKPath();

            // Extract the arc's defining properties
            Arc arc = arcCurve.Arc;
    
            // Define bounding rectangle for the circle
            float radius = (float)arc.Radius;
            SKRect oval = new SKRect(
                (float)arc.Center.X - radius,
                -(float)arc.Center.Y - radius, // negate Y-coordinate
                (float)arc.Center.X + radius,
                -(float)arc.Center.Y + radius  // negate Y-coordinate
            );

            // Add full circle to the path
            path.AddOval(oval);

            return path;
        }
        
    }
}