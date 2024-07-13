using DataStructsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Circuits
{
    internal struct ElectricalProperties : IStringableDataStruct<double>
    {
        public double Resistance { get; private set; }
        public SimulatedElectricalProperties? Simulated { get; set; }
        public ElectricalProperties(double voltage, double current, double resistance)
        {
            Simulated = new SimulatedElectricalProperties(voltage, current);
            Resistance= resistance;
        }
        public ElectricalProperties(double resistance)
        {
            Simulated = null;
            Resistance = resistance;
        }
        public string Stringify(bool withNewLines, Func<double, string> transform)
        {
            string possibleNewLine = withNewLines ? "\n" : ", ";
            double? v = this.Simulated?.Voltage;
            double? i = this.Simulated?.Current;
            double r = this.Resistance;
            return $"V: {(v.HasValue ? transform(v.Value) : "NULL")}{possibleNewLine}I: {(i.HasValue ? transform(i.Value) : "NULL")}{possibleNewLine}R: {this.Resistance}";
        }
        public override string ToString()
        {
            return this.Stringify();
        }
    }

    internal struct SimulatedElectricalProperties
    {
        public double Current { get; private set; }
        public double Voltage { get; private set; }
        public SimulatedElectricalProperties(double voltage, double current)
        {
            Voltage = voltage;
            Current = current;
        }
    }
}