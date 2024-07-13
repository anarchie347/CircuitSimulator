using Circuits.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using DataStructsLib;

namespace Circuits.UI
{
    internal class PowerSupply : EditableResistor
    {
        public double EMF { get; protected set; }
        public static PowerSupply NewCell()
        {
            return new PowerSupply("cell", 6);
        }

        public static PowerSupply NewBattery()
        {
            return new PowerSupply("battery", 12);
        }
        protected PowerSupply(string type, double emf) : base(type)
        {
            Resistance = 0D;
            EMF = emf;
        }
        public override HashTable<ComponentDataType, double> GetDataElements()
        {
            HashTable<ComponentDataType, double> fromBase = base.GetDataElements();
            fromBase[ComponentDataType.Voltage] = EMF;
            return fromBase;
        }
        public override void SetDataElements(HashTable<ComponentDataType, double> data)
        {
            base.SetDataElements(data);
            if (data.ContainsKey(ComponentDataType.Voltage))
            {
                EMF = data[ComponentDataType.Voltage];
            }
            
        }
    }

    internal class EditableResistor : Component
    {
        protected double Resistance { get; set; }
        public static EditableResistor NewFixedResistor()
        {
            return new EditableResistor("fixedresistor");
        }
        public static EditableResistor NewVariableResistor()
        {
            return new EditableResistor("variableresistor");
        }
        protected EditableResistor(string type) : base(type)
        {
            Resistance = 10D;
            HasConfig = true;
        }
        public override double GetResistance()
        {
            return Resistance;
        }
        public override HashTable<ComponentDataType, double> GetDataElements()
        {
            HashTable<ComponentDataType, double> fromBase = base.GetDataElements();
            fromBase[ComponentDataType.Resistance] = Resistance;
            return fromBase;
        }
        public override void SetDataElements(HashTable<ComponentDataType, double> data)
        {
            base.SetDataElements(data);
            if (data.ContainsKey(ComponentDataType.Resistance))
            {
                Resistance = data[ComponentDataType.Resistance];
                OnResistanceChanged(EventArgs.Empty);
            }  
        }
    }
    internal class OutputtingResistor : EditableResistor, IHighlightableControl
    {
        private readonly Color ActivatedColour;
        public static OutputtingResistor NewLamp()
        {
            return new OutputtingResistor("lamp", Color.Yellow);
        }
        public static OutputtingResistor NewHeater()
        {
            return new OutputtingResistor("heater", Color.Red);
        }
        public static OutputtingResistor NewBell()
        {
            return new OutputtingResistor("bell", Color.Blue);
        }
        public static OutputtingResistor NewLoudspeaker()
        {
            return new OutputtingResistor("loudspeaker", Color.Goldenrod);
        }
        protected OutputtingResistor(string type, Color activatedColour) : base(type)
        {
            ActivatedColour = activatedColour;
        }
        public override void WhenSimulated(CircuitGraph sender)
        {
            base.WhenSimulated(sender);
            double voltage = sender.FindVoltageAcross(base.LConnector, base.RConnector);
            if (voltage != 0)
            {
                Highlight();
            } else
            {
                RemoveHighlight();
            }
        }

        public void Highlight()
        {
            base.MainControl.FlatAppearance.BorderColor = ActivatedColour;
        }
        public void RemoveHighlight()
        {
            base.MainControl.FlatAppearance.BorderColor = base.DefaultBorderColour;
        }
        public bool IsHighlighted()
        {
            return base.MainControl.FlatAppearance.BorderColor == ActivatedColour;
        }
    }

    internal class DependentResistor : SingleVariableEnvironmentDependentComponent
    {
        protected double Scaling;
        protected readonly double ExponentBase;

        protected double CurrentResistance;  
        public static DependentResistor NewThermistor()
        {
            return new DependentResistor("thermistor", EnvironmentDataType.Temperature, 1.5D);
        }
        public static DependentResistor NewLDR()
        {
            return new DependentResistor("ldr", EnvironmentDataType.Light, 1.2D);
        }
        protected DependentResistor(string type, EnvironmentDataType dependentDataType, double exponentBase) : base(type, dependentDataType)
        {
            Scaling = 1;
            base.HasConfig = true;
            this.ExponentBase = exponentBase;
        }
        protected override void OnDependentVariableChanged(double newValue)
        {
            UpdateResistance(newValue);
        }
        private void UpdateResistance(double newValue)
        {
            CurrentResistance = CalculateResistance(newValue);
            base.OnResistanceChanged(EventArgs.Empty);
        }
        public override double GetResistance()
        {
            return CurrentResistance;
        }

        public override HashTable<ComponentDataType, double> GetDataElements()
        {
            HashTable<ComponentDataType, double> fromBase = base.GetDataElements();
            fromBase[ComponentDataType.Scaling] = Scaling;
            
            return fromBase;
        }
        public override void SetDataElements(HashTable<ComponentDataType, double> data)
        {
            base.SetDataElements(data);
            if (data.ContainsKey(ComponentDataType.Scaling))
            {
                Scaling = data[ComponentDataType.Scaling];
                UpdateResistance(base.DependentValue);
                OnResistanceChanged(EventArgs.Empty);
            }   
        }

        protected double CalculateResistance(double dependentValue)
        {
            double exponent = -(Scaling * dependentValue) / 10;
            return 100 * Math.Pow(ExponentBase,exponent);
        }
    }

    internal class NonSimComponent : Component
    {
        public static NonSimComponent NewDCPower()
        {
            return new NonSimComponent("dc");
        }
        public static NonSimComponent NewACPower()
        {
            return new NonSimComponent("ac");
        }
        public static NonSimComponent NewTransformer()
        {
            return new NonSimComponent("transformer");
        }
        public static NonSimComponent NewEarth()
        {
            return new NonSimComponent("earth");
        }
        public static NonSimComponent NewGenerator()
        {
            return new NonSimComponent("generator");
        }
        protected NonSimComponent(string type) : base(type)
        {
        }
        public override double GetResistance()
        {
            return 0;
        }
        public override void PreSimulation(bool manual)
        {
            throw new NonSimComponentSimulationAttemptException(this.Type);
        }


        [Serializable]
        public class NonSimComponentSimulationAttemptException : Exception
        {
            public string Type { get; private set; }
            public NonSimComponentSimulationAttemptException(string type) : base($"{type} was not a simulatable component")
            {
                Type = type;
            }
            public NonSimComponentSimulationAttemptException(string type, Exception inner) : base($"{type} was not a simulatable component", inner)
            {
                Type = type;
            }
            protected NonSimComponentSimulationAttemptException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context)
            {
                Type = "";
            }
        }
    }

    internal class Junction : Component
    {
        public static Junction NewJunction()
        {
            return new Junction("junction");
        }
        protected Junction(string type) : base(type)
        {
        }
        public override double GetResistance()
        {
            return 0;
        }
    }

    internal class Diode : BreakableConnection
    {
        public static Diode NewDiode()
        {
            return new Diode("diode");
        }
        protected Diode(string type) : base(type, "")
        {
            base.Toggle.Hide();
            base.Connected = true;
        }
        public override void PreSimulation(bool manual)
        {
            base.PreSimulation(manual);
            if (manual)
            {
                this.Connected = true;
            }
        }
        public override void WhenSimulated(CircuitGraph circuit)
        {
            base.WhenSimulated(circuit);
            if (circuit is null)
            {
                return;
            }
            if (!Connected)
            {
                return;
            }
            if (circuit.CurrentVoltageBetween(base.LConnector, base.RConnector).Current < 0)
            {
                Connected = false;
                base.OnResistanceChanged(EventArgs.Empty);
                circuit.Simulate(false);
            }
        }
    }

    internal class LED : Diode, IHighlightableControl
    {
        private readonly Color ActivatedColour = Color.White;
        protected double ForwardResistance { get; set; }
        public static LED NewLED()
        {
            return new LED("led");
        }
        protected LED(string type) : base(type)
        {
            ForwardResistance = 5D;
            HasConfig = true;
        }
        public override void WhenSimulated(CircuitGraph circuit)
        {
            base.WhenSimulated(circuit);
            if (base.Connected)
            {
                Highlight();
            } else
            {
                RemoveHighlight();
            }
        }
        public void Highlight()
        {
            base.MainControl.FlatAppearance.BorderColor = ActivatedColour;
        }
        public void RemoveHighlight()
        {
            base.MainControl.FlatAppearance.BorderColor = base.DefaultBorderColour;
        }
        public bool IsHighlighted()
        {
            return base.MainControl.FlatAppearance.BorderColor == ActivatedColour;
        }

        public override double GetResistance()
        {
            return base.Connected ? ForwardResistance : double.PositiveInfinity;
        }
        public override HashTable<ComponentDataType, double> GetDataElements()
        {
            HashTable<ComponentDataType, double> fromBase = base.GetDataElements();
            fromBase[ComponentDataType.Resistance] = ForwardResistance;
            return fromBase;
        }
        public override void SetDataElements(HashTable<ComponentDataType, double> data)
        {
            
            base.SetDataElements(data);
            if (data.ContainsKey(ComponentDataType.Resistance))
            {
                ForwardResistance = data[ComponentDataType.Resistance];
                OnResistanceChanged(EventArgs.Empty);
            }
            
        }
    }

    internal abstract class DisplayMeter : Component
    {
        private ValueDisplay Display;
        protected double Value
        {
            get { return Display.Value; }
            set { Display.Value = value; }
        }
        protected DisplayMeter(string type, char unit, Color colour) : base(type)
        {
            Display = new ValueDisplay(unit);
            base.MainControl.Controls.Add(Display);
            Display.Width = Display.Parent.Width;
            Display.Height = Display.Parent.Height / 3;
            Display.BackColor = colour;
        }
    }

    internal class Ammeter : DisplayMeter
    {
        public static Ammeter NewAmmeter()
        {
            return new Ammeter("ammeter");
        }
        protected Ammeter(string type) : base(type, 'A', Color.Lime)
        {
        }
        public override double GetResistance()
        {
            return 0;
        }
        public override void WhenSimulated(CircuitGraph circuit)
        {
            base.WhenSimulated(circuit);
            if (circuit is null)
            {
                return;
            }
            double current = circuit.CurrentVoltageBetween(base.LConnector, base.RConnector).Current;
            base.Value = current;
        }
    }

    internal class Voltmeter : DisplayMeter
    {
        public static Voltmeter NewVoltmeter()
        {
            return new Voltmeter("voltmeter");
        }
        protected Voltmeter(string type) : base(type, 'V', Color.Orange)
        {
        }
        public override double GetResistance()
        {
            return double.PositiveInfinity;
        }
        public override void WhenSimulated(CircuitGraph circuit)
        {
            base.WhenSimulated(circuit);
            if (circuit is null)
            {
                return;
            }
            double voltage = circuit.FindVoltageAcross(base.LConnector, base.RConnector);
            base.Value = voltage;
        }
    }

    internal class BreakableConnection : Component
    {
        private bool connected;
        protected bool Connected
        {
            get { return connected; }
            set
            {
                connected = value;
                this.ConnectedDisplay.BackColor = value ? Color.Lime : Color.Red;
                OnResistanceChanged(EventArgs.Empty);
            }
        }
        protected ComponentActionButton Toggle { get; set; }
        private Button ConnectedDisplay;
        public static BreakableConnection NewSwitch()
        {
            return new BreakableConnection("switch", "\U0001F517");
        }
        public static BreakableConnection NewMicrophone()
        {
            return new BreakableConnection("microphone", "\U0001F3A4");
        }
        protected BreakableConnection(string type, string toggleSymbol) : base(type)
        {
            Toggle = new ComponentActionButton(false);
            Toggle.GridLocation = new Point(0, ComponentActionButton.GridHeight - 1);
            Toggle.BackColor = Color.Cyan;
            Toggle.Text = toggleSymbol;
            Toggle.Click += (sender, e) => Connected = !Connected;
            base.MainControl.Controls.Add(Toggle);

            ConnectedDisplay = new Button();
            base.MainControl.Controls.Add(ConnectedDisplay);
            ConnectedDisplay.Location = new Point(0, 0);
            ConnectedDisplay.Size = new Size(ConnectedDisplay.Parent.Width, ConnectedDisplay.Parent.Height / 3);

            Connected = false;
        }
        public override double GetResistance()
        {
            return Connected ? 0 : double.PositiveInfinity;
        }
    }

    internal class Fuse : BreakableConnection
    {
        protected double TrippingCurrent { get; private set; }
        public static Fuse NewFuse()
        {
            return new Fuse("fuse");
        }
        protected Fuse(string type) : base(type, "\u2692")
        {
            HasConfig = true;
            Connected = true;
            base.Toggle.Hide();
            base.Toggle.Click += (s, e) => ((ComponentActionButton?)s)?.Hide();
        }
        public override HashTable<ComponentDataType, double> GetDataElements()
        {
            HashTable<ComponentDataType, double> fromBase = base.GetDataElements();
            fromBase[ComponentDataType.Tripping_Current] = TrippingCurrent;
            return fromBase;
        }
        public override void SetDataElements(HashTable<ComponentDataType, double> data)
        {
            base.SetDataElements(data);
            if (data.ContainsKey(ComponentDataType.Tripping_Current))
            {
                TrippingCurrent = data[ComponentDataType.Tripping_Current];
            }
        }
        public override void WhenSimulated(CircuitGraph circuit)
        {
            base.WhenSimulated(circuit);
            if (circuit is null || !Connected)
            {
                return;
            }
            if (Math.Abs(circuit.CurrentVoltageBetween(base.LConnector, base.RConnector).Current) < TrippingCurrent)
            {
                return;
            }

            //break the fuse
            Connected = false;
            base.Toggle.Show();


            //call another sim
            circuit.Simulate(false);
        }
    }
}
