using DataStructsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class HashTableEditorForm<T> : Form where T : Enum
    {
        private const int elementHeightIncMargin = 50;
        private const int elementWidthIncMargin = 230;
        DataOptionEditor<T>[] optionSelectors;
        private HashTableEditorForm(HashTable<T, double> data) : base()
        {
            this.ClientSize = new Size(elementWidthIncMargin, elementHeightIncMargin * data.Count + 100);
            optionSelectors = new DataOptionEditor<T>[data.Count];
            T[] keys = data.KeysArray();
            for (int i = 0; i < keys.Length; i++)
            {
                optionSelectors[i] = new DataOptionEditor<T>(new DataElement<T>(keys[i], data[keys[i]]));
                optionSelectors[i].Location = new Point(0, i * elementHeightIncMargin);
                this.Controls.Add(optionSelectors[i]);
            }

            Button SubmitBtn = new Button()
            {
                Location = new Point(this.ClientSize.Width / 2 - 50, data.Count * elementHeightIncMargin),
                Size = new Size(100, 30),
                Text = "Apply"
            };
            this.Controls.Add(SubmitBtn);
            SubmitBtn.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }
        private HashTable<T, double> GetValues()
        {
            HashTable<T, double> results = new();
            foreach (DataOptionEditor<T> option in optionSelectors)
            {
                DataElement<T> data = option.GetData();
                results[data.Type] = data.Value;
            }
            return results;
        }

        public static HashTable<T, double> GetOptions(HashTable<T, double> currentData)
        {
            HashTableEditorForm<T> form = new HashTableEditorForm<T>(currentData);
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                HashTable<T, double> data = form.GetValues();
                form.Dispose();
                return data;
            }
            form.Dispose();
            return currentData;

        }

        
    }
}
