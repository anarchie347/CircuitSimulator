using Circuits.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class MenuBar : Panel
    {
        private const int buttonSize = 100;
        private const int margin = 15;
        private readonly Color backColour = Color.FromArgb(0x26, 0x23, 0x22);
        private readonly Color textColour = Color.FromArgb(0xBC, 0x9C, 0xB0);
        private int nextButtonX;

        public new const int Height = buttonSize + 2 * margin;
       
        public MenuBar() : base()
        {
            base.BackColor = Color.FromArgb(0x32, 0x0A, 0x28);
            base.Height = buttonSize + 2 * margin;
            nextButtonX = margin;
            this.AddProps(Props.Persistent, Props.Static);
        }
        protected override void OnParentChanged(EventArgs e)
        {
            if (base.Parent is not null)
            {
                base.Parent.SizeChanged -= Parent_Resized;
            }
            
            base.OnParentChanged(e);

            if (base.Parent is not null)
            {
                this.Width = base.Parent.ClientSize.Width;
                base.Parent.SizeChanged += Parent_Resized;
            }
            
        }

        private void Parent_Resized(object? sender, EventArgs e)
        {
            this.Width = base.Parent?.ClientSize.Width ?? this.Width;
        }

        public void AddButton(string text, Action onClick)
        {
            AddButton(text, (object? sender, EventArgs e) => onClick());
        }
        public void AddButton(string text, EventHandler onClick)
        {
            Button button = new Button()
            {
                BackColor = backColour,
                ForeColor = textColour,
                Text = text,
                Location = new Point(nextButtonX, margin),
                Size = new Size(buttonSize, buttonSize)
            };
            button.Click += onClick;
            this.Controls.Add(button);

            nextButtonX += buttonSize + margin;
        }
    }
}
