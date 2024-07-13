using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.Diagramming
{
    internal struct DiagramWireInfo
    {
        public Point Start { get; private set; }
        public Direction StartDirection { get; private set; }
        public Point End { get; private set; }
        public Direction EndDirection { get; private set; }
        public DiagramWireInfo(Point start, Direction startDirection, Point end, Direction endDirection)
        {

            this.Start = start;
            this.StartDirection = startDirection;
            this.End = end;
            this.EndDirection = endDirection;
        }
        public DiagramWireInfo Swap()
        {
            return new DiagramWireInfo(this.End, this.EndDirection, this.Start, this.StartDirection);
        }
    }
}
