using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Text;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class ComponentActionButton : Button
    {
        public const int GridWidth = 4;
        public const int GridHeight = 4;
        private Point gridLocation;
        private Control? previousParent;
        public Point GridLocation
        {
            get { return gridLocation; }
            set
            {
                gridLocation = value;
                UpdateSizeLocation();
            }
        }
        private bool hidden;
        public bool Hidden
        {
            get { return hidden; }
            set
            {
                hidden = value;
                if (value)
                {
                    if (this.Parent is not null)
                    {
                        Parent.MouseEnter += Parent_MouseEnter;
                        Parent.MouseLeave += Parent_MouseLeave;
                    }
                    
                    if (!this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
                    {
                        Hide();
                    }
                } else
                {
                    if (this.Parent is not null)
                    {
                        Parent.MouseEnter -= Parent_MouseEnter;
                        Parent.MouseLeave -= Parent_MouseLeave;
                    }
                    Show();
                }
            }
        }
        public ComponentActionButton(bool hidden)
        {
            Hidden = hidden;
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (previousParent is not null)
            {
                previousParent.SizeChanged -= Parent_SizeChange;
                if (hidden)
                {
                    Parent.MouseEnter -= Parent_MouseEnter;
                    Parent.MouseLeave -= Parent_MouseLeave;
                }
            }
            previousParent = this.Parent;
            if (this.Parent is null)
            {
                return;
            }

            this.Parent.SizeChanged += Parent_SizeChange;
            if (hidden)
            {
                Parent.MouseEnter += Parent_MouseEnter;
                Parent.MouseLeave += Parent_MouseLeave;
            }
            UpdateSizeLocation();
        }
        private void Parent_SizeChange(object? sender, EventArgs e)
        {
            UpdateSizeLocation();
        }
        private void Parent_MouseEnter(object? sender, EventArgs e)
        {
            Show();
        }
        private void Parent_MouseLeave(object? sender, EventArgs e)
        {
            //prevent the control hiding if the thing moved over is the child control
            if (!this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
            {
                Hide();
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (base.Parent is null || !hidden)
            {
                return;
            }
            if (!this.Parent.ClientRectangle.Contains(this.Parent.PointToClient(Cursor.Position)))
            {
                Hide();
            }

        }
        private void UpdateSizeLocation()
        {
            if (this.Parent is null)
            {
                return;
            }
            int gridSquareWidth = this.Parent.Width / GridWidth;
            int gridSquareHeight = this.Parent.Height / GridHeight;
            base.Location = new Point(gridLocation.X * gridSquareWidth, gridLocation.Y * gridSquareHeight);
            base.Size = new Size(gridSquareWidth, gridSquareHeight);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.Parent is not null)
            {
                Parent.SizeChanged -= Parent_SizeChange;
                if (hidden)
                {
                    Parent.MouseEnter -= Parent_MouseEnter;
                    Parent.MouseLeave -= Parent_MouseLeave;
                }
            }
            base.Dispose(disposing);
        }
        public static ComponentActionButton NewDelete()
        {
            ComponentActionButton actionBtn = new ComponentActionButton(true);
            actionBtn.BackColor = Color.Red;
            actionBtn.ForeColor = Color.Black;
            actionBtn.Text = "\U0001F5D1";
            actionBtn.gridLocation = new Point(GridWidth - 1 , GridHeight - 1);
            return actionBtn;
        }
    }
}