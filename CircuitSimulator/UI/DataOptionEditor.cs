using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    internal class DataOptionEditor<T> : ICustomControl where T : Enum
    {
        private const int margin = 5;
        private Label labelBox;
        private PositiveDoubleInput valueBox;

        private Point location;
        public Point Location
        {
            get { return location; }
            set
            {
                location = value;
                labelBox.Location = value;
                valueBox.Location = new Point(value.X + labelBox.Width + margin, value.Y);
            }
        }
        

        public DataOptionEditor(DataElement<T> dataElement)
        {
            labelBox = new Label();
            labelBox.AutoSize = true;
            labelBox.Text = dataElement.Type.ToString().Replace("_", " ");
            labelBox.Tag = dataElement.Type;
            valueBox = new PositiveDoubleInput(dataElement.Value);
        }
        public DataElement<T> GetData()
        {
            return new DataElement<T>((T)labelBox.Tag, valueBox.GetValue()); 
        }
        public void AddToParent(Control.ControlCollection collection)
        {
            collection.Add(labelBox);
            collection.Add(valueBox);
        }
        public void RemoveFromParent(Control.ControlCollection collection)
        {
            collection.Remove(labelBox);
            collection.Remove(valueBox);
        }
        public void Dispose()
        {
            labelBox.Dispose();
            valueBox.Dispose();
        }
    }
}
