using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    internal enum Orientation
    {
        Horizontal,
        Vertical,
        HorizontalFlipped,
        VerticalFlipped
    }
    internal static class OrientationMethods
    {
        public static Orientation Rotate90(this Orientation orientation)
        {
            return orientation.Rotate(1);
        }
        public static Orientation RotateBack90(this Orientation orientation)
        {
            return orientation.Rotate(3);
        }
        public static Orientation Rotate180(this Orientation orientation)
        {
            return orientation.Rotate(2);
        }
        public static Orientation Rotate(this Orientation orientation, uint quarterTurns)
        {
            return (Orientation)(((int)orientation + quarterTurns) % 4);
        }
    }
}
