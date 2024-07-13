using Circuits.Logic;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("code")]
    internal class SaveSelectorForm : Form
    {
        protected long pressedID;
        protected SaveSelectorForm(int verticalOffset = 0) : base()
        {
            SaveInfo[] saves = DatabaseManager.GetSaves();
            LoadDisplayButton[] buttons = saves.Select(save => new LoadDisplayButton(save)).ToArray();
            ButtonStackContainer<LoadDisplayButton, SaveInfo> container = new ButtonStackContainer<LoadDisplayButton, SaveInfo>(buttons);
            container.ButtonClick += (object? sender, ButtonClickEventArgs<SaveInfo> e) =>
            {
                this.pressedID = e.Data.ID;
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(container);
            container.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - verticalOffset);
            container.Location = new Point(0, verticalOffset);
            container.PerformLayout();
            this.AutoSize = true;
        }

        public static long? LoadSelection()
        {
            SaveSelectorForm form = new SaveSelectorForm();
            var s = form.ShowDialog();
            if (s == DialogResult.OK)
            {
                return form.pressedID;
            } else
            {
                return null;
            }
        }

        private class LoadDisplayButton : Button, IStackableButton<SaveInfo>
        {
            public SaveInfo Data { get; }
            public const int SeparationHeight = 50;
            public event EventHandler Delete;
            public LoadDisplayButton(SaveInfo save)
            {
                Data = save;
                this.Text = ParseSave(save) + "        ";
                this.AutoSize = true;
                Button delete = new Button()
                {
                    BackColor = Color.Red,
                    Text = "\U0001F5D1",
                    Size = new Size(this.Height, this.Height),
                    AutoSize = false,
                    Location = new Point((int)(this.Width * 0.9F), 0),
                    Name = "delete"
                };
                delete.Click += (s, e) =>
                {
                    DatabaseManager.Delete(Data.ID);
                    Delete?.Invoke(this, EventArgs.Empty);
                };
                this.Controls.Add(delete);
            }
            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                Control? delete = this.Controls.Find("delete", false).FirstOrDefault();
                if (delete is not null)
                {
                    delete.Location = new Point((int)(this.Width * 0.9F), 0);
                    delete.Size = new Size(this.Height, this.Height);
                }
                
            }
            public void UpdateLocation(int i)
            {
                this.Location = new Point(0, SeparationHeight * i);
            }
            protected override void OnParentChanged(EventArgs e)
            {
                base.OnParentChanged(e);
                if (this.Parent is null)
                {
                    return;
                }
                this.Width = this.Parent.Width;
            }

            private static string ParseSave(SaveInfo save)
            {
                return $"{save.ID}   {save.Name}   {save.Author}   {save.LastModified}";
            }
        }
        private class ButtonStackContainer<TButton, TData> : Panel where TButton : Control, IStackableButton<TData>
        {
            public event EventHandler<ButtonClickEventArgs<TData>> ButtonClick;
            private DataStructsLib.List<TButton> buttons;
            public ButtonStackContainer(TButton[] buttons)
            {
                this.buttons = new DataStructsLib.List<TButton>(buttons);
                foreach (TButton button in buttons)
                {
                    this.Controls.Add(button);
                    button.Delete += ValueDelete;
                    button.Click += Button_Click;
                }
                UpdateAllLocations();
                this.AutoSize = true;
            }

            private void Button_Click(object? sender, EventArgs e)
            {
                if (sender is null)
                {
                    return;
                }
                TData data = ((IStackableButton<TData>)sender).Data;
               ButtonClick?.Invoke(this, new ButtonClickEventArgs<TData>(data));
            }

            private void Remove(TButton btn)
            {
                buttons.Remove(btn);
                this.Controls.Remove(btn);
                btn.Delete -= ValueDelete;
                UpdateAllLocations();
            }
            private void ValueDelete(object? sender, EventArgs e)
            {
                if (sender is null)
                {
                    return;
                }           
                this.Remove((TButton)sender);
            }
            
            private void UpdateAllLocations()
            {
                //return;
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].UpdateLocation(i);
                }
            }
        }
        private interface IStackableButton<T>
        {
            public event EventHandler Delete;
            public void UpdateLocation(int index);
            public T Data { get; }
        }
        private class ButtonClickEventArgs<TData> : EventArgs
        {
            public TData Data { get; private set; }
            public ButtonClickEventArgs(TData data)
            {
                this.Data = data;
            }
        }


    }

    internal class SaveSelectorOrAddForm : SaveSelectorForm
    {
        private SaveSelectorOrAddForm() : base(50)
        {
            Button add = new Button()
            {
                Text = "New Save",
                BackColor = Color.Aqua
            };
            add.Click += (s, e) =>
            {
                pressedID = -1;
                DialogResult = DialogResult.OK;

            };
            this.Controls.Add(add);
        }
        public static long? SaveSelection()
        {
            SaveSelectorOrAddForm form = new SaveSelectorOrAddForm();
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                return form.pressedID;
            }
            else
            {
                return null;
            }
        }
    }
    public struct SaveInfo
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public DateTime LastModified { get; set; }
        public SaveInfo(long id, string name, string author, DateTime lastModified)
        {
            ID = id;
            Name = name;
            Author = author;
            LastModified = lastModified;
        }
    }
}
