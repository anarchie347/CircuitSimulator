using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Circuits
{
    internal enum Direction
    {
        PositiveX, PositiveY, NegativeX, NegativeY
    };
    internal static class DirectionExtension
    {
        public static Direction Rotate180(this Direction d)
        {
            return d.RotateACW(2);
        }
        public static Direction Rotate90ACW(this Direction d)
        {
            return d.RotateACW(1);
        }
        public static Direction Rotate90CW(this Direction d)
        {
            return d.RotateACW(3);
        }
        public static Direction RotateACW(this Direction d, uint quarterTurns)
        {
            return (Direction)((int)d + quarterTurns % 4);
        }
        public static bool ParallelTo(this Direction d1, Direction d2)
        {
            return (int)d1 % 2 == (int)d2 % 2;
        }
        public static bool PerpendicularTo(this Direction d1, Direction d2)
        {
            return !d1.ParallelTo(d2);
        }
        public static bool IsHorizontal(this Direction d1)
        {
            return d1.ParallelTo(Direction.PositiveX);
        }
        public static bool IsVertical(this Direction d1)
        {
            return d1.ParallelTo(Direction.PositiveY);
        }

        public static Point AdvanceInDirection(this Point point, Direction direction, int distance )
        {
            return direction switch
            {
                Direction.PositiveX => new Point(point.X + distance, point.Y),
                Direction.PositiveY => new Point(point.X, point.Y + distance),
                Direction.NegativeX => new Point(point.X - distance, point.Y),
                _ => new Point(point.X, point.Y - distance),
            };
        }
    }
}
