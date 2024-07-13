using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class ValueDisplay : Button
    {
        double value;
        char unit;
        public double Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                this.Text = $"{Utils.FormatValueWithPrefix(value)}{unit}";
            }
        }
        public ValueDisplay(char unit)
        {
            this.unit = unit;
            Value = 0;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            this.Width = Parent.Width;
            this.Height = Parent.Height / 3;
        }

        

    }
}
