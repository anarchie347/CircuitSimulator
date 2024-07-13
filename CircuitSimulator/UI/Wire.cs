using Circuits.Logic;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ObjectiveC;

namespace Circuits.UI
{
    internal class Wire : ICustomControl
    {
        private static bool IsDrawingWire = false;
        public const int Width = 5;
        private readonly Color RegularColour = Color.Gray;

        private Button panel;
        private Point startPoint, endPoint;
        public Connector Start { get; private set; }
        public Connector End { get; private set; }
        public event EventHandler Deleted;
        public Wire(Point start, Point end)
        {
            Init(start, end);
        }
        public Wire(Connector start, Connector end)
        {
            Init(start.GetCentre(), end.GetCentre());
            SetStart(start);
            SetEnd(end);
        }
        private void Init(Point start, Point end)
        {
            this.startPoint = start;
            this.endPoint = end;
            panel = new Button();
            panel.BackColor = RegularColour;
            panel.FlatStyle = FlatStyle.Flat;
            panel.FlatAppearance.BorderSize = 0;
            panel.MouseDown += OnMouseDown;
            SetValuesAndDraw();
        }

        public static void NewMouseDrawWire(Connector start, Form form, CircuitGraph circuit)
        {
            if (IsDrawingWire)
            {
                return;
            }
            IsDrawingWire = true;

            Point startPos = start.GetCentre();
            Wire wire = new Wire(startPos, startPos);
            wire.SetStart(start);
            form.Controls.Add(wire);

            Connector[] connectors = form.Controls.OfType<Connector>().ToArray();
            Connector[] dissallowedConnectors = circuit.NeighboursAndHop(start).Append(start).ToArray();
            Connector[] allowedConnectors = connectors.Except(dissallowedConnectors).ToArray();


            start.Component.Deleted += wire.ComponentDeleteEventHandler;
            start.Component.Deleted += componentDeleteWhileDrawingHandler; //in case the component is deleted while the wire is being drawn
            




            void mouseMoveHandler(object? sender, MouseEventArgs e)
            {
                wire.SetEnd(e.Location);
            }
            form.MouseMove += mouseMoveHandler;

            void connectorClickHandler(object? sender, EventArgs e)
            {
                if (sender is null || sender is not Connector end)
                {
                    return;
                }
                wire.SetEnd(end);
                circuit.Connect(wire);
                wire.Deleted += (sender, e) => circuit.Disconnect(wire);
                end.Component.Deleted += wire.ComponentDeleteEventHandler;

                endWireDraw();
            }

            void cancelDrawHandler(object? sender, EventArgs e)
            {
                endWireDraw();
                wire?.Dispose();
            }
            void componentDeleteWhileDrawingHandler(object? sender, EventArgs e)
            {
                start.Component.Deleted -= cancelDrawHandler;
                endWireDraw();
            }
            void endWireDraw()
            {
                form.MouseMove -= mouseMoveHandler;
                form.Click -= cancelDrawHandler;
                wire.panel.Click -= cancelDrawHandler; 

                Array.ForEach(allowedConnectors, connectorChangesOnEndWireDraw);
                IsDrawingWire = false;
            }

            void connectorChangesOnEndWireDraw(Connector connector)
            {
                connector.RemoveHighlight();
                connector.Click -= connectorClickHandler;
            }

            for (int i = 0; i < allowedConnectors.Length; i++)
            {
                Connector connector = allowedConnectors[i];
                connector.Highlight();
                connector.Click += connectorClickHandler;
            }

            form.Click += cancelDrawHandler;
            wire.panel.Click += cancelDrawHandler;

        }
        public void SetStart(Point start)
        {
            this.startPoint = start;
            SetValuesAndDraw();
        }
        public void SetEnd(Point end)
        {
            this.endPoint = end;
            SetValuesAndDraw();
        }
        public void SetStart(Connector start)
        {
            Start = start;
            AutoUpdateStart(null, new ConnectorMoveEventArgs(start.GetCentre()));
            start.Move += AutoUpdateStart;
        }
        public void SetEnd(Connector end)
        {
            End = end;
            AutoUpdateEnd(null, new ConnectorMoveEventArgs(end.GetCentre()));
            end.Move += AutoUpdateEnd;
        }
        public void ComponentDeleteEventHandler(object? sender, EventArgs e)
        {
            this.Dispose();
        }
        private void AutoUpdateStart(object? sender, ConnectorMoveEventArgs e)
        {
            SetStart(e.NewLocation);
        }
        private void AutoUpdateEnd(object? sender, ConnectorMoveEventArgs e)
        {
            SetEnd(e.NewLocation);
        }
        public void Dispose()
        {
            if (Start is not null)
            {
                Start.Move -= AutoUpdateStart;
                Start.Component.Deleted -= ComponentDeleteEventHandler;
            }
            if (End is not null)
            {
                End.Move -= AutoUpdateStart;
                End.Component.Deleted -= ComponentDeleteEventHandler;
            }
            Deleted?.Invoke(this, EventArgs.Empty);
            panel.Parent.Controls.Remove(panel);
            panel.Dispose();
        }
        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.Dispose();
            }
        }
        private void SetValuesAndDraw()
        {
            Point location = CalculateLocation();
            //offset points based on where (0,0) will be for the panel
            Point relativeStart = new Point(startPoint.X - location.X, startPoint.Y - location.Y);
            Point relativeEnd = new Point(endPoint.X - location.X, endPoint.Y - location.Y);
            panel.Size = CalculateSize();
            panel.Location = location;
            DrawLine(relativeStart, relativeEnd);
        }
        private void DrawLine(Point relativeStart, Point relativeEnd)
        {
            if (relativeStart == relativeEnd)
            {
                return;
            }
            GraphicsPath gp = new();
            //these two points make the wire curved.
            //they are positioned 1/4 distance from the point horizontally
            //and 1/8 distance vertically
            Point startMid = new Point(((relativeStart.X * 3) + relativeEnd.X) / 4, (relativeStart.Y * 7 + relativeEnd.Y) / 8);
            Point endMid = new Point(((relativeEnd.X * 3) + relativeStart.X) / 4, (relativeEnd.Y * 7 + relativeStart.Y) / 8);
            gp.AddCurve(new Point[] { relativeStart, startMid, endMid, relativeEnd }, 0.4f);
            gp.Widen(new Pen(Color.Red, Width));
            panel.Region = new Region(gp);
            
           
        }
        
        public void AddToParent(Control.ControlCollection collection)
        {
            collection.Add(panel);
        }
        public void RemoveFromParent(Control.ControlCollection collection)
        {
            collection.Remove(panel);
        }
        private Size CalculateSize()
        {
            return new Size(Math.Abs(startPoint.X - endPoint.X) + Width * 2, Math.Abs(startPoint.Y - endPoint.Y) + Width * 2);
        }
        private Point CalculateLocation()
        {
            return new Point(Math.Min(startPoint.X, endPoint.X) - Width, Math.Min(startPoint.Y, endPoint.Y) - Width);
        }
    }
}
