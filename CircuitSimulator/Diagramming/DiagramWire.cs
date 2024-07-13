using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Circuits.Diagramming
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class DiagramWire : Panel
    {
        public const int WireThickness = 2;
        public const int WireLoopExtendBackDistance = 30; //defines the back and out distance for looping around components
        public const int WireLoopExtendOutDistance = 60;
        public static readonly Color WireColor = Color.Black;
        private Point[] corners;

        public DiagramWire(DiagramWireInfo wireInfo) : base()
        {
            this.BackColor = WireColor;

            corners = Interpolate(wireInfo);
            Render();
        }

        public void Render()
        {
            const int offsetFromWireThickness = WireThickness / 2;
            Point[] offsetPoints = new Point[corners.Length];
            int minX = corners.Select(p => p.X).Min();
            int minY = corners.Select(p => p.Y).Min();
            offsetPoints = corners.Select(p => new Point(p.X - minX + offsetFromWireThickness, p.Y - minY + offsetFromWireThickness)).ToArray();
            int maxX = corners.Select(p => p.X).Max();
            int maxY = corners.Select(p => p.Y).Max();
            
            this.Location = new Point(minX - offsetFromWireThickness, minY - offsetFromWireThickness);
            this.Size = new Size(maxX - minX + WireThickness, maxY - minY + WireThickness);

            GraphicsPath line = new GraphicsPath();
            for (int i = 0; i < offsetPoints.Length - 1; i++)
            {
                line.AddCurve(new Point[] { offsetPoints[i], offsetPoints[i + 1] });
            }
            line.Widen(new Pen(Color.Red, WireThickness)); //colour is irrelevant, but ios required to widen the path

            this.Region = new Region(line);
        }

        public static Point[] Interpolate(DiagramWireInfo info)
        {
            return CaseDetection.InterpolateCommutative(info)
                ?? CaseDetection.InterpolateNonCommutative(info)
                ?? CaseDetection.InterpolateNonCommutative(info.Swap())
                ?? throw new Exception();
        }
        

        private static class CaseDetection
        {
            public static Point[]? InterpolateCommutative(DiagramWireInfo info)
            {
                if (IsBasicHorizontal(info))
                {
                    return CaseInterpolation.BasicHorizontal(info);
                }
                if (IsBasicVertical(info))
                {
                    return CaseInterpolation.BasicVertical(info);
                }
                
                return null;
            }
            public static Point[]? InterpolateNonCommutative(DiagramWireInfo info)
            {
                if (IsBasicHorizToVert(info))
                {
                    return CaseInterpolation.BasicHorizToVert(info);
                }
                if (IsSingleLoopBackStartHorizontal(info))
                {
                    return CaseInterpolation.SingleLoopBackStartHorizontal(info);
                }
                if (IsSingleLoopBackStartVertical(info))
                {
                    return CaseInterpolation.SingleLoopBackStartVertical(info);
                }
                if (IsDoubleLoopBackHorizontal(info))
                {
                    return CaseInterpolation.DoubleLoopBackHorizontal(info);
                }
                if (IsDoubleLoopBackVertical(info))
                {
                    return CaseInterpolation.DoubleLoopBackVertical(info);
                }
                if (IsVertToHorizLoopVert(info))
                {
                    return CaseInterpolation.VertToHorizLoopVert(info);
                }
                if (IsHorizToVertLoopHoriz(info))
                {
                    return CaseInterpolation.HorizToVertLoopHoriz(info);
                }
                return null;
            }

            private static bool IsBasicHorizontal(DiagramWireInfo info)
            {
                if (info.Start.X == info.End.X)
                {
                    return false;
                }
                if (info.Start.X < info.End.X)
                {
                    return info.StartDirection == Direction.PositiveX && info.EndDirection == Direction.NegativeX;
                }
                return info.StartDirection == Direction.NegativeX && info.EndDirection == Direction.PositiveX;
            }
            private static bool IsBasicVertical(DiagramWireInfo info)
            {
                if (info.Start.Y == info.End.Y)
                {
                    return false;
                }
                if (info.Start.Y < info.End.Y)
                {
                    return info.StartDirection == Direction.PositiveY && info.EndDirection == Direction.NegativeY;
                }
                return info.StartDirection == Direction.NegativeY && info.EndDirection == Direction.PositiveY;
            }
            private static bool IsBasicHorizToVert(DiagramWireInfo info)
            {
                if (info.StartDirection.IsVertical() || info.StartDirection.ParallelTo(info.EndDirection))
                {
                    return false;
                }
                if (info.Start.X == info.End.X || info.Start.Y == info.End.Y)
                {
                    return false;
                }
                if (info.Start.X < info.End.X)
                {

                    if (info.Start.Y < info.End.Y)
                    {
                        return info.StartDirection == Direction.PositiveX && info.EndDirection == Direction.NegativeY;
                    }
                    return info.StartDirection == Direction.PositiveX && info.EndDirection == Direction.PositiveY;
                }
                if (info.Start.Y < info.End.Y)
                {
                    return info.StartDirection == Direction.NegativeX && info.EndDirection == Direction.NegativeY;
                }
                return info.StartDirection == Direction.NegativeX && info.EndDirection == Direction.PositiveY;
            }
            private static bool IsSingleLoopBackStartHorizontal(DiagramWireInfo info)
            {
                if (!(info.StartDirection == info.EndDirection && info.StartDirection.IsHorizontal()))
                {
                    return false;
                }

                return info.Start.X == info.End.X || (info.Start.X > info.End.X ^ info.StartDirection == Direction.PositiveX);// && info.Start.X != info.End.X;
            }
            private static bool IsSingleLoopBackStartVertical(DiagramWireInfo info)
            {
                if (!(info.StartDirection == info.EndDirection && info.StartDirection.IsVertical()))
                {
                    
                    return false;
                }
                return (info.Start.Y > info.End.Y ^ info.StartDirection == Direction.PositiveY) && info.Start.Y != info.End.Y;
            }
            private static bool IsDoubleLoopBackHorizontal(DiagramWireInfo info)
            {
                if (!(info.StartDirection.Rotate180() == info.EndDirection && info.StartDirection.IsHorizontal()))
                {
                    return false;
                }
                return info.Start.X >= info.End.X ^ info.StartDirection == Direction.NegativeX;
            }
            private static bool IsDoubleLoopBackVertical(DiagramWireInfo info)
            {
                if (!(info.StartDirection.Rotate180() == info.EndDirection && info.StartDirection.IsVertical()))
                {

                    return false;
                }
                return info.Start.Y >= info.End.Y ^ info.StartDirection == Direction.NegativeY;
            }
            private static bool IsVertToHorizLoopVert(DiagramWireInfo info)
            {
                if (info.StartDirection.IsHorizontal() || info.StartDirection.ParallelTo(info.EndDirection))
                {
                    return false;
                }
                if (info.Start.Y > info.End.Y)
                {
                    return info.StartDirection == Direction.PositiveY;
                }
                return info.StartDirection == Direction.NegativeY;
            }
            private static bool IsHorizToVertLoopHoriz(DiagramWireInfo info)
            {
                if (info.StartDirection.IsVertical() || info.StartDirection.ParallelTo(info.EndDirection))
                {
                    return false;
                }
                if (info.Start.X > info.End.X)
                {
                    return info.StartDirection == Direction.PositiveX;
                }
                return info.StartDirection == Direction.NegativeX;
            }

            private static class CaseInterpolation
            {
                //commutative
                public static Point[] BasicHorizontal(DiagramWireInfo info)
                {
                    int midX = (info.Start.X + info.End.X) / 2;
                    return new Point[]
                    {
                    info.Start,
                    new Point(midX, info.Start.Y),
                    new Point(midX, info.End.Y),
                    info.End,
                    };
                }
                public static Point[] BasicVertical(DiagramWireInfo info)
                {
                    int midY = (info.Start.Y + info.End.Y) / 2;
                    return new Point[]
                    {
                    info.Start,
                    new Point(info.Start.X, midY),
                    new Point(info.End.X, midY),
                    info.End,
                    };
                }
                //Non commutitave
                public static Point[] BasicHorizToVert(DiagramWireInfo info)
                {
                    Point mid = new Point(info.End.X, info.Start.Y);
                    return new Point[] { info.Start, mid, info.End };
                }
                public static Point[] SingleLoopBackStartHorizontal(DiagramWireInfo info)
                {
                    Point[] points = new Point[6];
                    points[0] = info.Start;
                    points[5] = info.End;

                    (Direction loopDirection, int loopWireY) = info.Start.Y <= info.End.Y
                        ? (Direction.NegativeY, Math.Min(info.Start.Y, info.End.Y - WireLoopExtendOutDistance))
                        : (Direction.PositiveY, Math.Max(info.Start.Y, info.End.Y + WireLoopExtendOutDistance));

                    points[1] = points[0].AdvanceInDirection(info.StartDirection, WireLoopExtendBackDistance);
                    points[2] = new Point(points[1].X, loopWireY);

                    points[4] = points[5].AdvanceInDirection(info.EndDirection, WireLoopExtendBackDistance);
                    points[3] = new Point(points[4].X, loopWireY);
                    return points;
                }
                public static Point[] SingleLoopBackStartVertical(DiagramWireInfo info)
                {
                    Point[] points = new Point[6];
                    points[0] = info.Start;
                    points[5] = info.End;

                    (Direction loopDirection, int loopWireX) = info.Start.X <= info.End.X
                        ? (Direction.NegativeX, Math.Min(info.Start.X, info.End.X - WireLoopExtendOutDistance))
                        : (Direction.PositiveX, Math.Max(info.Start.X, info.End.X + WireLoopExtendOutDistance));

                    points[1] = points[0].AdvanceInDirection(info.StartDirection, WireLoopExtendBackDistance);
                    points[2] = new Point(loopWireX, points[1].Y);

                    points[4] = points[5].AdvanceInDirection(info.EndDirection, WireLoopExtendBackDistance);
                    points[3] = new Point(loopWireX, points[4].Y);
                    return points;
                }
                public static Point[] DoubleLoopBackHorizontal(DiagramWireInfo info)
                {
                    Point[] points = new Point[6];
                    points[0] = info.Start;
                    points[5] = info.End;

                    (Direction loopDirection, int loopWireY) = info.Start.Y <= info.End.Y
                        ? (Direction.NegativeY, Math.Min(info.Start.Y, info.End.Y) - WireLoopExtendOutDistance)
                        : (Direction.PositiveY, Math.Max(info.Start.Y, info.End.Y) + WireLoopExtendOutDistance);

                    points[1] = points[0].AdvanceInDirection(info.StartDirection, WireLoopExtendBackDistance);
                    points[2] = new Point(points[1].X, loopWireY);

                    points[4] = points[5].AdvanceInDirection(info.EndDirection, WireLoopExtendBackDistance);
                    points[3] = new Point(points[4].X, loopWireY);
                    return points;
                }
                public static Point[] DoubleLoopBackVertical(DiagramWireInfo info)
                {
                    Point[] points = new Point[6];
                    points[0] = info.Start;
                    points[5] = info.End;

                    (Direction loopDirection, int loopWireX) = info.Start.X <= info.End.X
                        ? (Direction.NegativeX, Math.Min(info.Start.X, info.End.X) - WireLoopExtendOutDistance)
                        : (Direction.PositiveX, Math.Max(info.Start.X, info.End.X) + WireLoopExtendOutDistance);

                    points[1] = points[0].AdvanceInDirection(info.StartDirection, WireLoopExtendBackDistance);
                    points[2] = new Point(loopWireX, points[1].Y);

                    points[4] = points[5].AdvanceInDirection(info.EndDirection, WireLoopExtendBackDistance);
                    points[3] = new Point(loopWireX, points[4].Y);
                    return points;
                }
                public static Point[] VertToHorizLoopVert(DiagramWireInfo info)
                {
                    Point[] points = new Point[5];
                    points[0] = info.Start;
                    points[4] = info.End;

                    points[1] = points[0].AdvanceInDirection(info.StartDirection, WireLoopExtendBackDistance);
                    points[3] = points[4].AdvanceInDirection(info.EndDirection, WireLoopExtendBackDistance);
                    points[2] = new Point(points[3].X, points[1].Y);
                    return points;
                }
                public static Point[] HorizToVertLoopHoriz(DiagramWireInfo info)
                {
                    Point[] points = new Point[5];
                    points[0] = info.Start;
                    points[4] = info.End;

                    points[1] = points[0].AdvanceInDirection(info.StartDirection, WireLoopExtendBackDistance);
                    points[3] = points[4].AdvanceInDirection(info.EndDirection, WireLoopExtendBackDistance);
                    points[2] = new Point(points[1].X, points[3].Y);
                    return points;
                }
            }
        }

        
    }
}
