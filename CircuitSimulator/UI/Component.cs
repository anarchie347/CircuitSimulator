using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Circuits.Logic;
using Circuits.UI;
using DataStructsLib;

namespace Circuits.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal abstract class Component : ICustomControl
    {
        
        public event EventHandler ResistanceChanged;
        public event EventHandler Deleted;
        
        public DraggableButton MainControl { get { return mainControl; } }
        public string Type { get; private set; }
        public Orientation Orientation { get; private set; }
        protected bool HasConfig;

        private DraggableButton mainControl;
        public Connector LConnector { get; private set; }
        public Connector RConnector { get; private set; }
        private ComponentActionButton delete;
        protected readonly Color DefaultBorderColour = Color.Gray;
        protected Component(string type)
        {
            mainControl = new DraggableButton(true)
            {
                BackColor = Color.White,
                Size = new(100, 100)
            };
            mainControl.BackgroundImage = FilePictures.FromType(type, false, true);
            mainControl.BackgroundImageLayout = ImageLayout.Stretch;
            mainControl.MouseUp += WhenMouseUp;
            mainControl.Click += Rotate;
            mainControl.FlatStyle = FlatStyle.Flat;
            mainControl.FlatAppearance.BorderSize = 5;
            mainControl.FlatAppearance.BorderColor = DefaultBorderColour;
            

            LConnector = new LeftConnector(this);
            RConnector = new RightConnector(this);
            LConnector.Move += (sender, e) => mainControl.BringToFront();
            RConnector.Move += (sender, e) => mainControl.BringToFront();

            delete = ComponentActionButton.NewDelete();
            mainControl.Controls.Add(delete);
            delete.Click += (sender, e) => Dispose();

            Type = type;
            HasConfig = false;
        }

        public void AddToParent(Control.ControlCollection collection)
        {
            collection.Add(mainControl);
        }
        public void RemoveFromParent(Control.ControlCollection collection)
        {
            collection.Remove(mainControl);
        }
        private void WhenMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && HasConfig)
            {

                SetDataElements(HashTableEditorForm<ComponentDataType>.GetOptions(this.GetDataElements()));
            }
        }
        public void Rotate90()
        {
            Rotate(null, EventArgs.Empty);
        }
        private void Rotate(object? sender, EventArgs e)
        {
            Orientation = Orientation.Rotate90();
            LConnector.UpdatePosition();
            RConnector.UpdatePosition();
            Image newImage = (Image)mainControl.BackgroundImage.Clone();
            newImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            mainControl.BackgroundImage = newImage;
        }
        public virtual void Dispose()
        {
            Deleted?.Invoke(this, EventArgs.Empty);
            ResistanceChanged = null;
            Deleted = null;
            mainControl.Parent?.Controls.Remove(mainControl);
            LConnector.Dispose();
            RConnector.Dispose();
            mainControl.Dispose();

        }

        public virtual HashTable<ComponentDataType, double> GetDataElements()
        {
            return new HashTable<ComponentDataType, double>();
        }
        public virtual void SetDataElements(HashTable<ComponentDataType, double> data) { }
        public abstract double GetResistance();

        public static Component MakeTypeFromString(string type, HashTable<ComponentDataType, double>? data = null, CircuitEnvironment? environment = null)
        {
            Component newComp;
            switch (type)
            {
                case "cell":
                    newComp = PowerSupply.NewCell();
                    break;
                case "battery":
                    newComp = PowerSupply.NewBattery();
                    break;
                case "fixedresistor":
                    newComp = EditableResistor.NewFixedResistor();
                    break;
                case "variableresistor":
                    newComp = EditableResistor.NewVariableResistor();
                    break;
                case "lamp":
                    newComp = OutputtingResistor.NewLamp();
                    break;
                case "heater":
                    newComp = OutputtingResistor.NewHeater();
                    break;
                case "bell":
                    newComp = OutputtingResistor.NewBell();
                    break;
                case "loudspeaker":
                    newComp = OutputtingResistor.NewLoudspeaker();
                    break;
                case "thermistor":
                    newComp = DependentResistor.NewThermistor();
                    break;
                case "ldr":
                    newComp = DependentResistor.NewLDR();
                    break;
                case "dc":
                    newComp = NonSimComponent.NewDCPower();
                    break;
                case "ac":
                    newComp = NonSimComponent.NewACPower();
                    break;
                case "transformer":
                    newComp = NonSimComponent.NewTransformer();
                    break;
                case "earth":
                    newComp = NonSimComponent.NewEarth();
                    break;
                case "generator":
                    newComp = NonSimComponent.NewGenerator();
                    break;
                case "junction":
                    newComp = Junction.NewJunction();
                    break;
                case "diode":
                    newComp = Diode.NewDiode();
                    break;
                case "led":
                    newComp = LED.NewLED();
                    break;
                case "ammeter":
                    newComp = Ammeter.NewAmmeter();
                    break;
                case "voltmeter":
                    newComp = Voltmeter.NewVoltmeter();
                    break;
                case "switch":
                    newComp = BreakableConnection.NewSwitch();
                    break;
                case "microphone":
                    newComp = BreakableConnection.NewMicrophone();
                    break;
                case "fuse":
                    newComp = Fuse.NewFuse();
                    break;
                default:
                    throw new ArgumentException($"{type} was not a valid component type");
            }
            if (environment is not null && newComp is EnvironmentDependentComponent envDependent)
            {
                envDependent.Environment = environment;
            }
            newComp.SetDataElements(data ?? new HashTable<ComponentDataType, double>());
            return newComp;
        }

        protected virtual void OnResistanceChanged(EventArgs e)
        {
            ResistanceChanged?.Invoke(this, e);
        }

        public virtual void WhenSimulated(CircuitGraph sender) { }
        public virtual void PreSimulation(bool manual) { }
       
    }
    
    public enum ComponentDataType
    {
        Resistance = 0,
        Voltage = 1,
        Tripping_Current = 2,
        Scaling = 3
    }
}
