using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class PositiveDoubleInput : TextBox
    {
        public PositiveDoubleInput(double start) : base()
        {
            base.Text = start.ToString();
        }
        public double GetValue()
        {
            double val;
            if (double.TryParse(base.Text, out val))
            {
                return val;
            } else
            {
                return 0;
            }
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            int selectionStart = base.SelectionStart;
            int selectionLength = base.SelectionLength;

            string newText = PositiveDoubleInput.FormatText(base.Text);
            int difference = Math.Max(0, base.Text.Length - newText.Length);
            base.Text = newText;

            base.SelectionStart = selectionStart - difference;
            base.SelectionLength = selectionLength;
        }
        private static string FormatText(string text)
        {
            StringBuilder currentText = new StringBuilder(text);
            bool hasHadDecimalPoint = false;
            for (int i = 0; i < currentText.Length; i++)
            {
                if (currentText[i] == '.')
                {
                    if (hasHadDecimalPoint)
                    {
                        currentText.Remove(i, 1);
                        i--;
                    }
                    hasHadDecimalPoint = true;
                }
                else if (!char.IsDigit(currentText[i]))
                {
                    currentText.Remove(i, 1);
                    i--;
                }
            }
            return currentText.ToString();
        }
    }
}
