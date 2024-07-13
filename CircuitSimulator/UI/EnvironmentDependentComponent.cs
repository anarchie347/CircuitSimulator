using Circuits.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.UI
{
    internal abstract class EnvironmentDependentComponent : Component
    {
        private CircuitEnvironment environment;
        public CircuitEnvironment Environment
        {
            get { return environment; }
            set
            {
                if (environment is not null)
                {
                    environment.EnvironmentChanged -= EnvironmentChangedEventHandler;
                }
                environment = value;
                if (value is not null)
                {
                    environment.EnvironmentChanged += EnvironmentChangedEventHandler;
                    OnEnvironmentChanged(value);
                }
                
            }
        }
        
        public EnvironmentDependentComponent(string type) : base(type) { }

        private void EnvironmentChangedEventHandler(object? sender, EventArgs e)
        {
            OnEnvironmentChanged((CircuitEnvironment)sender);
        }
        protected abstract void OnEnvironmentChanged(CircuitEnvironment environment);

        public override void Dispose()
        {
            base.Dispose();
            Environment = null;
        }
    }

    internal abstract class SingleVariableEnvironmentDependentComponent : EnvironmentDependentComponent
    {
        protected readonly EnvironmentDataType DependentValueType;
        protected double DependentValue { get { return base.Environment.Data[this.DependentValueType]; } }
        private double? oldValue;
        public SingleVariableEnvironmentDependentComponent(string type, EnvironmentDataType dependentValueType) : base(type)
        {
            DependentValueType = dependentValueType;
        }

        protected override void OnEnvironmentChanged(CircuitEnvironment environment)
        {
            double newValue = DependentValue;
            if (oldValue is null || newValue != oldValue)
            {
                oldValue = newValue;
                OnDependentVariableChanged(newValue);
            }
        }
        protected abstract void OnDependentVariableChanged(double newValue);
    }
}
