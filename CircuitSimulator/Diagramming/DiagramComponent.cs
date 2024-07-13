using Circuits.UI;
using DataStructsLib;
using System.Runtime.CompilerServices;

namespace Circuits.Diagramming
{
    internal class DiagramComponent : ICustomControl
    {
        Button mainControl;
        Label? dataLabel;

        public Direction LeftConnectorDirection { get; private set; }
        public Direction RightConnectorDirection { get; private set; }
        public Point LeftConnectorLocation { get; private set; }
        public Point RightConnectorLocation { get; private set; }

        public DiagramComponent(Point centreLocation, Size size, Circuits.UI.Orientation orientation, Image image, HashTable<ComponentDataType, double>? data = null)
        {
            mainControl = new Button();
            mainControl.BackgroundImageLayout = ImageLayout.Stretch;
            mainControl.BackgroundImage = image;
            mainControl.Size = size;
            mainControl.SetCentre(centreLocation);
            
            mainControl.FlatStyle = FlatStyle.Flat;
            mainControl.FlatAppearance.BorderSize = 0;

            LeftConnectorLocation = CalculateLeftPoint(centreLocation, size, orientation);
            RightConnectorLocation = CalculateLeftPoint(centreLocation, size, orientation.Rotate180());

            LeftConnectorDirection = (Direction)orientation.Rotate180();
            RightConnectorDirection = (Direction)orientation;

            if (data is not null && data.Count > 0)
            {
                dataLabel = new Label();
                dataLabel.Text = string.Join(Environment.NewLine, StringData(data));
                dataLabel.AutoSize = true;
                dataLabel.Resize += (object? sender, EventArgs e) =>
                {
                    if (sender is null)
                    {
                        return;
                    }
                    ((Control)sender).Location = CalculateLabelPosition(orientation);
                };
                dataLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
                dataLabel.Left = mainControl.Left;
            }
            
        }
        private Point CalculateLabelPosition(Circuits.UI.Orientation orientation)
        {
            if (this.dataLabel is null)
            {
                return new Point(0,0);
            }
            int x = orientation switch
            {
                Circuits.UI.Orientation.Horizontal or UI.Orientation.HorizontalFlipped => mainControl.Left,
                _ => mainControl.Left - dataLabel.Width
            };
            int y = orientation switch
            {
                Circuits.UI.Orientation.Horizontal or UI.Orientation.HorizontalFlipped => mainControl.Top - dataLabel.Height,
                _ => mainControl.Top
            };
            return new Point(x, y);
        }
        private Point CalculateLeftPoint(Point centreLocation, Size size, Circuits.UI.Orientation orientation)
        {
            int x = orientation switch
            {
                Circuits.UI.Orientation.Horizontal => centreLocation.X - size.Width / 2,
                Circuits.UI.Orientation.HorizontalFlipped => centreLocation.X + size.Width / 2,
                _ => centreLocation.X
            };
            int y = orientation switch
            {
                Circuits.UI.Orientation.Vertical => centreLocation.Y - size.Height / 2,
                Circuits.UI.Orientation.VerticalFlipped => centreLocation.Y + size.Height / 2,
                _ => centreLocation.Y
            };
            return new Point(x, y);
        }
        private string[] StringData(HashTable<ComponentDataType, double> data)
        {
            string[] dataAsString = new string[data.Count];
            ComponentDataType[] keys = data.KeysArray();
            for (int i = 0; i < keys.Length; i++)
            {
                string unit = GetUnit(keys[i]);
                string value = unit == "" ? data[keys[i]].ToString() : Utils.FormatValueWithPrefix(data[keys[i]]); //if no units, dont adjust for prefix
                dataAsString[i] = $"{keys[i].ToString().Replace("_", " ")}: {value}{unit}";
            }
            return dataAsString;
        }
        private static string GetUnit(ComponentDataType dataType)
        {
            return dataType switch
            {
                ComponentDataType.Resistance => "Ω",
                ComponentDataType.Voltage => "V",
                ComponentDataType.Tripping_Current => "A",
                _ => ""
            };
        }

        public void AddToParent(Control.ControlCollection collection)
        {
            collection.Add(mainControl);
            if (dataLabel is not null)
            {
                collection.Add(dataLabel);
            }
            
        }
        public void RemoveFromParent(Control.ControlCollection collection)
        {
            collection.Remove(mainControl);
            if (dataLabel is not null)
            {
                collection.Remove(dataLabel);
            }
        }
        public void Dispose()
        {
            mainControl?.Dispose();
            mainControl = null;
            dataLabel?.Dispose();
            dataLabel = null;
        }
    }
}