using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms.VisualStyles;
using Circuits.Diagramming;
using Circuits.Logic;
using Circuits.UI;
using DataStructsLib;

namespace Circuits
{
    public partial class CircuitEditorForm : Form
    {
        private Circuits.Logic.CircuitGraph circuit;
        private CircuitEnvironment environment;
        
        public CircuitEditorForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            base.AutoScaleMode = AutoScaleMode.Font;
            base.AutoSize = false;
            base.Text = "Circuit editor";

            environment = new CircuitEnvironment();

            circuit = new CircuitGraph();

            DatabaseManager.InitDB();

            MenuBar mb = new MenuBar();
            this.Controls.Add(mb);

            ComponentSelectionPanel cc = ComponentSelectionPanel.Default(this, circuit, MenuBar.Height, environment);
            this.Controls.Add(cc);

            #region MenuBtnActions
            void menuSaveClick()
            {
                long? id = SaveSelectorOrAddForm.SaveSelection();
                string? name = null;
                string? author = null;
                if (id is null)
                {
                    return;
                } else if (id == -1) //new save
                {
                    id = null;
                    name = Microsoft.VisualBasic.Interaction.InputBox("Name of circuit:", "Save - Circuit name");
                    if (name == "") name = null;
                    author = Microsoft.VisualBasic.Interaction.InputBox("Name of author:", "Save - Author name");
                    if (author== "") author = null;
                };
                DecomposedCircuitGraph decomposed = circuit.Decompose();
                _ = DatabaseManager.Save(environment, circuit.Decompose(), name, author, id);
            }
            void menuClearClick()
            {
                foreach (Component comp in circuit.ListComponents())
                {
                    comp.Dispose();
                }
                ClearComponents();
                circuit = new CircuitGraph();
                cc.ChangeGraph(circuit);
            }
            void menuLoadClick()
            {
                long? id = SaveSelectorForm.LoadSelection();
                if (id is null)
                {
                    return;
                }
                ClearComponents();
                (circuit, CircuitEnvironment newEnv) = DatabaseManager.Load(id.Value) ?? (circuit, environment);
                environment.Data = newEnv.Data;
                circuit.RenderInControl(this.Controls);
                cc.ChangeGraph(circuit);
                foreach (Component comp in circuit.ListComponents()) //update to the new reference of environment
                {
                    if (comp is EnvironmentDependentComponent envDependent)
                    {
                        envDependent.Environment = environment;
                    }
                }
            }
            void menuSimulateClick()
            {
                circuit.Simulate(true);
            }
            void menuDiagramClick()
            {
                _ = new DiagramForm(circuit, false);
            }
            void menuDetailDiagramClick()
            {
                _ = new DiagramForm(circuit, true);
            }
            void menuEnvironmentClick()
            {
                environment.Change();
            }
            #endregion
            #region AddMenuBtns
            mb.AddButton("Clear", menuClearClick);
            mb.AddButton("Load", menuLoadClick);
            mb.AddButton("Save", menuSaveClick);
            mb.AddButton("Diagram", menuDiagramClick);
            mb.AddButton("Detailed Diagram", menuDetailDiagramClick);
            mb.AddButton("Environment", menuEnvironmentClick);
            mb.AddButton("Simulate", menuSimulateClick);
            #endregion
            
        }

        internal void AddWire(Connector start)
        {
            Wire.NewMouseDrawWire(start, this, circuit);
        }

        private void ClearComponents()
        {
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                if (!Controls[i].HasOneProps(Props.Persistent))
                    this.Controls.RemoveAt(i);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Middle)
            {
                //panning
                Point mouseLocation = e.Location;
                void WhenMouseMiddleButtonMove(object? sender, MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Middle)
                    {
                        Point change = new Point(e.X - mouseLocation.X, e.Y - mouseLocation.Y);
                        foreach (Control control in base.Controls)
                        {
                            if (!(control?.HasOneProps(Props.Static) ?? false))
                                control.Location = new Point(control.Left + change.X, control.Top + change.Y);
                        }
                        mouseLocation = e.Location;
                    }
                }
                void WhenMouseMiddleButtonUp(object? sender, MouseEventArgs e)
                {
                    base.MouseMove -= WhenMouseMiddleButtonMove;
                    base.MouseUp -= WhenMouseMiddleButtonMove;
                }
                base.MouseMove += WhenMouseMiddleButtonMove;
                base.MouseUp += WhenMouseMiddleButtonUp;
            }
        }

    }
    

    public enum Props
    {
        Static,
        Persistent
    }

    public static class Extensions
    {
        public static void Add(this Control.ControlCollection collection, ICustomControl control)
        {
            control.AddToParent(collection);
        }
        public static void Remove(this Control.ControlCollection collection, ICustomControl control)
        {
            control.RemoveFromParent(collection);
        }
        public static void SetCentre(this Control control, Point location)
        {
            Point topLeft = new Point(location.X - (control.Width / 2), location.Y - (control.Height / 2));
            control.Location = topLeft;
        }
        public static Point GetCentre(this Control control)
        {
            return new Point(control.Left + (control.Width / 2), control.Top + (control.Height / 2));
        }
        public static void SetProps(this Control control, params Props[] props)
        {
            control.Tag = new DataStructsLib.List<Props>(props);
        }
        public static void AddProps(this Control control, params Props[] props)
        {
            DataStructsLib.List<Props> oldProps = (DataStructsLib.List<Props>)control.Tag ?? new DataStructsLib.List<Props>();
            foreach (Props prop in props)
            {
                oldProps.Add(prop);
            }
            control.Tag = oldProps;
        }
        public static void RemoveProps(this Control control, params Props[] props)
        {
            DataStructsLib.List<Props> oldProps = (DataStructsLib.List<Props>)control.Tag ?? new DataStructsLib.List<Props>();
            foreach (Props prop in props)
            {
                oldProps.Remove(prop);
            }
            control.Tag = oldProps;
        }
        public static bool HasAllProps(this Control control, params Props[] props)
        {
            DataStructsLib.List<Props> ctrlProps = (DataStructsLib.List<Props>)control.Tag ?? new DataStructsLib.List<Props>();
            foreach (Props prop in props)
            {
                if (!ctrlProps.Contains(prop)) {
                    return false;
                }
            }
            return true;
        }
        public static bool HasOneProps(this Control control, params Props[] props)
        {
            DataStructsLib.List<Props> ctrlProps = (DataStructsLib.List<Props>)control.Tag ?? new DataStructsLib.List<Props>();
            foreach (Props prop in props)
            {
                if (ctrlProps.Contains(prop)) {
                    return true;
                }
            }
            return false;
        }
    }
    public interface ICustomControl : IDisposable
    {
        void AddToParent(Control.ControlCollection parentCollection);
        void RemoveFromParent(Control.ControlCollection parentCollection);
    }
}