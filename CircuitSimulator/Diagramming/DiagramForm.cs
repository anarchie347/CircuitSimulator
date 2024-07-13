using Circuits.Logic;
using Circuits.UI;
using DataStructsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.Diagramming
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class DiagramForm : Form
    {
        const int gridSquareSize = 60;
        public DiagramForm(CircuitGraph circuit, bool showInfos) : base()
        {
            base.BackColor = Color.White;
            DiagramComponent[] diagramComponents = AddComponents(circuit.ListComponents(), showInfos);
            DiagramWireInfo[] wireInfos = MakeWireInfos(diagramComponents, circuit.ListComponents(), circuit.ListWires());
            Array.ForEach(wireInfos, wi =>
            {
                DiagramWire dw = new(wi);
                this.Controls.Add(dw);
            });
            base.AutoSize = true;
            this.Show();
        }



        private DiagramWireInfo[] MakeWireInfos(DiagramComponent[] diagramComponents, Component[] components, Link<Connector, ElectricalProperties>[] wires)
        {
            DiagramWireInfo[] wireInfos = new DiagramWireInfo[wires.Length];
            for (int i = 0; i < wires.Length; i++)
            {
                Link<Connector, ElectricalProperties> wire = wires[i];
                int startIndex = Array.IndexOf(components, wire.Start.Component);
                int endIndex = Array.IndexOf(components, wire.End.Component);
                DiagramComponent startDiagramComponent = diagramComponents[startIndex];
                DiagramComponent endDiagramComponent = diagramComponents[endIndex];
                var (startPoint, startDirection) = (wire.Start.LogicalDirection == Direction.NegativeX) switch
                {
                    true => (startDiagramComponent.LeftConnectorLocation, startDiagramComponent.LeftConnectorDirection),
                    false => (startDiagramComponent.RightConnectorLocation, startDiagramComponent.RightConnectorDirection)
                };
                var (endPoint, endDirection) = (wire.End.LogicalDirection == Direction.NegativeX) switch
                {
                    true => (endDiagramComponent.LeftConnectorLocation, endDiagramComponent.LeftConnectorDirection),
                    false => (endDiagramComponent.RightConnectorLocation, endDiagramComponent.RightConnectorDirection)
                };

                wireInfos[i] = new DiagramWireInfo(startPoint, startDirection, endPoint, endDirection);
            }
            return wireInfos;
        }

        private DiagramComponent[] AddComponents(Component[] components, bool showInfos)
        {
            DiagramComponent[] diagramComponents = new DiagramComponent[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                DiagramComponent diagramComponent = CreateSingleComponent(components[i], showInfos);
                this.Controls.Add(diagramComponent);
                diagramComponents[i] = diagramComponent;
            }
            return diagramComponents;
        }
        private DiagramComponent CreateSingleComponent(Component component, bool showInfo)
        {
            Point snapped = SnapToGrid(component.MainControl.GetCentre());
            Image image = FilePictures.FromType(component.Type, true, false);
            switch (component.Orientation)
            {
                case Circuits.UI.Orientation.Vertical: image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                case Circuits.UI.Orientation.HorizontalFlipped: image.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                case Circuits.UI.Orientation.VerticalFlipped: image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
            }
            HashTable<ComponentDataType, double>? data = showInfo ? component.GetDataElements() : null;
            DiagramComponent diagramComponent = new DiagramComponent(snapped, component.MainControl.Size, component.Orientation, image, data);
            return diagramComponent;
        }

        private Point SnapToGrid(Point point)
        {
            return new Point(point.X / gridSquareSize * gridSquareSize, point.Y / gridSquareSize * gridSquareSize);
        }
    }
}
