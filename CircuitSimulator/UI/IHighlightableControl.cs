using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    internal interface IHighlightableControl
    {
        public void Highlight();
        public void RemoveHighlight();
        public bool IsHighlighted();
    }
}
