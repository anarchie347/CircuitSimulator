using Circuits.Logic;
using DataStructsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class ComponentSelectionPanel : Panel
    {
        private const int VerticalMargin = 5;
        private const int HorizontalMargin = 5;
        private const int ButtonSize = 100;
        private DataStructsLib.List<Control> elements;
        private Point position;

        public static ComponentSelectionPanel Default(Control componentParent, CircuitGraph graph, int top, CircuitEnvironment environment)
        {
            ComponentSelectionPanel container = new ComponentSelectionPanel();
            container.Location = new Point(0, top);

            void possibleEnvironmentAdd(Component comp) //eliminates the need to create an anonymous function for each different AddComponent call
            {
                AddToEnvironmentIfRequired(environment, comp);
            }

            container.AddText("Energy Source");
            container.AddComponent(FilePictures.Cell(false, false), PowerSupply.NewCell, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Battery(false, false), PowerSupply.NewBattery, componentParent, graph.AddComponent, possibleEnvironmentAdd);

            container.AddText("Resistors");
            container.AddComponent(FilePictures.FixedResistor(false, false), EditableResistor.NewFixedResistor, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.VariableResistor(false, false), EditableResistor.NewVariableResistor, componentParent, graph.AddComponent, possibleEnvironmentAdd);

            container.AddText("Environment resistors");
            container.AddComponent(FilePictures.Thermistor(false, false), DependentResistor.NewThermistor, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.LDR(false, false), DependentResistor.NewLDR, componentParent, graph.AddComponent, possibleEnvironmentAdd);

            container.AddText("Measuring devices");
            container.AddComponent(FilePictures.Ammeter(false, false), Ammeter.NewAmmeter, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Voltmeter(false, false), Voltmeter.NewVoltmeter, componentParent, graph.AddComponent, possibleEnvironmentAdd);

            container.AddText("Outputs");
            container.AddComponent(FilePictures.Lamp(false, false), OutputtingResistor.NewLamp, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Heater(false, false), OutputtingResistor.NewHeater, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Bell(false, false), OutputtingResistor.NewBell, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Loudspeaker(false, false), OutputtingResistor.NewLoudspeaker, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.LED(false, false), LED.NewLED, componentParent, graph.AddComponent, possibleEnvironmentAdd);

            container.AddText("Switches");
            container.AddComponent(FilePictures.Switch(false, false), BreakableConnection.NewSwitch, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Microphone(false, false), BreakableConnection.NewMicrophone, componentParent, graph.AddComponent, possibleEnvironmentAdd);

            container.AddText("Misc");
            container.AddComponent(FilePictures.Fuse(false, false), Fuse.NewFuse, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Diode(false, false), Diode.NewDiode, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Junction(false, false), Junction.NewJunction, componentParent, graph.AddComponent, possibleEnvironmentAdd);

            container.AddText("Not simulateable");
            container.AddComponent(FilePictures.DC(false, false), NonSimComponent.NewDCPower, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.AC(false, false), NonSimComponent.NewACPower, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Transformer(false, false), NonSimComponent.NewTransformer, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Earth(false, false), NonSimComponent.NewEarth, componentParent, graph.AddComponent, possibleEnvironmentAdd);
            container.AddComponent(FilePictures.Generator(false, false), NonSimComponent.NewGenerator, componentParent, graph.AddComponent, possibleEnvironmentAdd);

            return container;
        }
        public ComponentSelectionPanel() : base()
        {
            elements = new DataStructsLib.List<Control>();
            position = new Point(HorizontalMargin, VerticalMargin);
            base.BackColor = Color.Gray;
            base.AutoScroll = true;
            this.SetProps(Props.Persistent, Props.Static);
        }

        public void ChangeGraph(CircuitGraph graph)
        {
            foreach (ComponentDisplay display in this.Controls.OfType<ComponentDisplay>())
            {
                display.GraphAddAction = graph.AddComponent;
            }
        }
        public void ChangeEnvironment(CircuitEnvironment environment)
        {
            foreach (ComponentDisplay display in this.Controls.OfType<ComponentDisplay>())
            {
                display.EnvironmentAddAction = comp => AddToEnvironmentIfRequired(environment, comp);
            }
        }
        private static void AddToEnvironmentIfRequired(CircuitEnvironment environment, Component comp)
        {
            if (comp is EnvironmentDependentComponent envDependent)
            {
                envDependent.Environment = environment;
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null)
            {
                Parent.SizeChanged += Parent_SizeChanged;
                Parent_SizeChanged(null, EventArgs.Empty);
            }
        }

        private void Parent_SizeChanged(object? sender, EventArgs e)
        {
            if (base.Parent != null)
            {
                Size ParentSize = base.Parent.ClientSize;
                base.Size = new Size(Math.Min(ParentSize.Width, (2 * ButtonSize) + (3 * HorizontalMargin) + SystemInformation.VerticalScrollBarWidth), ParentSize.Height - this.Top);
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i] is Label)
                    {
                        elements[i].Width = base.ClientSize.Width;
                    }
                }
                this.BringToFront();
            }
        }

        public void AddText(string text)
        {
            Label textButton = new Label();
            textButton.Text = text;
            textButton.BackColor = Color.Black;
            textButton.ForeColor = Color.White;
            if (position.X > HorizontalMargin) //textboxes do not fill spaces on the right
            {
                position = new Point(HorizontalMargin, position.Y + ButtonSize + VerticalMargin);
            }
            textButton.Location = position;
            position = new Point(position.X, position.Y + textButton.Height + VerticalMargin);
            textButton.Width = base.ClientSize.Width;
            
            this.Controls.Add(textButton);
            
            elements.Add(textButton);
        }
        public void AddComponent(Image image, Func<Component> constructor, Control componentParent, Action<Component> graphAddAction, Action<Component> environmentAddAction)
        {
            ComponentDisplay display = new(image, constructor, componentParent, graphAddAction, environmentAddAction);
            this.Controls.Add(display);
            display.Location = position;
            if (position.X > HorizontalMargin)
            {
                position = new Point(HorizontalMargin, position.Y + display.Height + VerticalMargin);
            } else
            {
                position = new Point(HorizontalMargin * 2 + display.Width, position.Y);
            }
            elements.Add(display);
        }

        private class ComponentDisplay : Button
        {
            private Func<Component> ComponentConstruction;
            private Control ComponentParent;
            public Action<Component> GraphAddAction { get; set; }
            public Action<Component> EnvironmentAddAction { get; set; }

            public ComponentDisplay(Image image, Func<Component> componentConstruction, Control componentParent, Action<Component> graphAddAction, Action<Component> environmentAddAction) : base()
            {
                base.Size = new Size(ButtonSize, ButtonSize);
                base.BackgroundImage = image;
                base.BackgroundImageLayout = ImageLayout.Stretch;
                this.ComponentConstruction = componentConstruction;
                this.ComponentParent = componentParent;
                this.GraphAddAction = graphAddAction;
                this.EnvironmentAddAction = environmentAddAction;
            }
            protected override void OnMouseDown(MouseEventArgs mevent)
            {
                base.OnMouseDown(mevent);
                Component newComp = ComponentConstruction();
                ComponentParent.Controls.Add(newComp);
                newComp.MainControl.BringToFront();
                GraphAddAction(newComp);
                EnvironmentAddAction(newComp);
            }
        }
    }
}