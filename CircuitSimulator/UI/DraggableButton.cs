

using System.Runtime.InteropServices;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class DraggableButton : Button
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private bool isBeingDragged;
        public DraggableButton() : base()
        {
            isBeingDragged = false;
        }
        public DraggableButton(bool startDragging) : base() {
            isBeingDragged = startDragging;
            if (isBeingDragged)
            {
                this.ParentChanged += InitialDragParentChangeEventHandler;
            }
        }
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            this.ParentChanged -= InitialDragParentChangeEventHandler;
        }
        private void InitialDragParentChangeEventHandler(object? sender, EventArgs e)
        {
            if (Parent is null)
            {
                return;
            }
            this.BringToFront();
            this.ParentChanged -= InitialDragParentChangeEventHandler;
            Point cursor = Parent.PointToClient(Cursor.Position);
            this.SetCentre(cursor);
            SimualteLeftMouseUp();
            SimulateLeftMouseDown(); //simulate releasing and clicking on the component to allw immediate dragging
        }
        
        private static void SimualteLeftMouseUp()
        {
            mouse_event(0x04, 0, 0, 0, 0);
        }
        private static void SimulateLeftMouseDown()
        {
            mouse_event(0x02, 0, 0, 0, 0);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            isBeingDragged = false;
            if (e.Button == MouseButtons.Left)
            {
                Point cursor = Parent.PointToClient(Cursor.Position);
                Point mouseDragOffset = new Point(cursor.X - Left, cursor.Y - Top);
                
                void WhenMouseMove(object? sender, MouseEventArgs e)
                {
                    Point newCursor = Parent.PointToClient(Cursor.Position);
                    Location = new Point(newCursor.X - mouseDragOffset.X, newCursor.Y - mouseDragOffset.Y);
                    BringToFront();
                    isBeingDragged=true;
                }
                void WhenMouseUp(object? sender, MouseEventArgs e)
                {
                    MouseMove -= WhenMouseMove;
                    MouseMove -= WhenMouseUp;
                }

                base.MouseMove += WhenMouseMove;
                base.MouseUp += WhenMouseUp;
            }
        }
        protected override void OnClick(EventArgs e)
        {
            if (isBeingDragged)
            {
                return; //cancel
            }

            base.OnClick(e);

        }
    }
}
