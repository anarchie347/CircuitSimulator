using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Circuits.UI;
using Circuits;
using Circuits.Diagramming;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal abstract class Connector : Button, IHighlightableControl, IDisposable
    {
        private readonly Color HighlightColour = Color.Blue;
        private readonly Color NormalColour = Color.Gray;
        public Component Component { get; private set; }
        public abstract Direction LogicalDirection { get; }


        private Control unchangedComponentParent;
        public new event EventHandler<ConnectorMoveEventArgs> Move;
        protected Connector(Component component, Color colour)
        {
            Component = component;
            base.Size = new Size(20, 20);
            BackColor = colour;
            base.FlatStyle = FlatStyle.Flat;
            base.FlatAppearance.BorderColor = NormalColour;
            base.FlatAppearance.BorderSize = 4;
            component.MainControl.Move += OnComponentMove;
            component.MainControl.ParentChanged += OnComponentParentChange;
            this.Click += OnClick;
            UpdatePosition();
        }
        public void Highlight()
        {
            base.FlatAppearance.BorderColor = HighlightColour;
        }
        public void RemoveHighlight()
        {
            base.FlatAppearance.BorderColor = NormalColour;
        }
        public bool IsHighlighted()
        {
            return base.FlatAppearance.BorderColor == HighlightColour;
        }
        private void OnComponentMove(object? sender, EventArgs e)
        {
            if (sender is null || sender is not Control)
            {
                return;
            }
            UpdatePosition();
            
        }
        public new void Dispose()
        {
            unchangedComponentParent = new Control();
            Component.MainControl.Move -= OnComponentMove;
            Component.MainControl.ParentChanged -= OnComponentParentChange;
            base.Dispose();
        }

        private void OnComponentParentChange(object? sender, EventArgs e)
        {
            if (sender is null || sender is not Control)
            {
                return;
            }
            Control parentParent = (sender as Control).Parent;
            if (unchangedComponentParent is not null)
            {
                unchangedComponentParent.Controls.Remove(this);
            }
            unchangedComponentParent = parentParent;
            if (parentParent is not null)
            {
                parentParent.Controls.Add(this);
            }
            
            
        }

        public void UpdatePosition()
        {
            this.SetCentre(CalculatePosition());
            base.BringToFront();
            Move?.Invoke(this, new ConnectorMoveEventArgs(this.GetCentre()));
        }

        private void OnClick(object? sender, EventArgs e)
        {
            if (base.Parent is CircuitEditorForm form1)
            {
                form1.AddWire(this);
            }
        }
        public abstract Connector Hop();
        protected abstract Point CalculatePosition();
    }

    internal class LeftConnector : Connector
    {
        public override Direction LogicalDirection {get { return Direction.NegativeX; } }
        public LeftConnector(Component parent) : base(parent, Color.White)
        {
        }
        protected override Point CalculatePosition()
        {
            int x = Component.Orientation switch
            {
                Orientation.Horizontal => base.Component.MainControl.Left,
                Orientation.HorizontalFlipped => base.Component.MainControl.Right,
                _ => base.Component.MainControl.GetCentre().X
            };
            int y = Component.Orientation switch
            {
                Orientation.Vertical => base.Component.MainControl.Top,
                Orientation.VerticalFlipped => base.Component.MainControl.Bottom,
                _ => base.Component.MainControl.GetCentre().Y
            };
            return new Point(x, y);
        }
        public override Connector Hop()
        {
            return this.Component.RConnector;
        }
    }

    internal class RightConnector : Connector
    {
        public override Direction LogicalDirection { get { return Direction.PositiveX; } }
        public RightConnector(Component parent) : base(parent, Color.HotPink)
        {
        }
        protected override Point CalculatePosition()
        {

            int x = Component.Orientation switch
            {
                Orientation.Horizontal => base.Component.MainControl.Right,
                Orientation.HorizontalFlipped => base.Component.MainControl.Left,
                _ => base.Component.MainControl.GetCentre().X
            };
            int y = Component.Orientation switch
            {
                Orientation.Vertical => base.Component.MainControl.Bottom,
                Orientation.VerticalFlipped => base.Component.MainControl.Top,
                _ => base.Component.MainControl.GetCentre().Y
            };
            return new Point(x, y);
        }

        public override Connector Hop()
        {
            return this.Component.LConnector;
        }
    }
    internal class ConnectorMoveEventArgs : EventArgs
    {
        public Point NewLocation { get; private set; }
        public ConnectorMoveEventArgs(Point newLocation)
        {
            NewLocation = newLocation;
        }
    }
}
