using Circuits.UI;
using DataStructsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.Logic
{
    internal class CircuitEnvironment
    {
        public const double DEFAULT_LIGHT = 50;
        public const double DEFAULT_TEMPERATURE = 20;
        private HashTable<EnvironmentDataType, double> data;
        public HashTable<EnvironmentDataType, double> Data
        {
            get { return data; }
            set
            {
                data = value;
                EnvironmentChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnvironmentChanged;

        public CircuitEnvironment(double light = DEFAULT_LIGHT, double temperature = DEFAULT_TEMPERATURE)
        {
            Data = new HashTable<EnvironmentDataType, double>();
            Data[EnvironmentDataType.Light] = light;
            Data[EnvironmentDataType.Temperature] = temperature;
        }
        public void Change()
        {
            this.Data = HashTableEditorForm<EnvironmentDataType>.GetOptions(this.Data);
        }
    }
    public enum EnvironmentDataType
    {
        Light,
        Temperature
    }
}
