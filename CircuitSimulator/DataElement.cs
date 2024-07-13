using Circuits.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits
{
    internal struct DataElement<T> where T : Enum
    {
        public T Type { get; private set; }
        public double Value { get; private set; }
        public DataElement(T type, double value)
        {
            Type = type;
            Value = value;
        }
        public override string ToString()
        {
            return $"({Type}: {Value})";
        }
    }
}
