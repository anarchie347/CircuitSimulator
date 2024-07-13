using Circuits.UI;
using DataStructsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Circuits.Logic
{
    internal partial class CircuitGraph
    {
        public void Simulate(bool manual)
        {
            try
            {
                foreach (Component comp in this.ListComponents())
                {
                    comp.PreSimulation(manual);
                }
            } catch (NonSimComponent.NonSimComponentSimulationAttemptException ex)
            {
                MessageBox.Show($"Simulation failed because component: '{ex.Type}' cannot be simulated");
                return;
            }
            

            CircuitGraph prepared = PrepareForSimulation();
            try
            {
                bool simSuccessful = prepared.Calculate();
                if (!simSuccessful)
                {
                    return;
                }
                TransferSimulatedValues(prepared);


                foreach (Component comp in this.ListComponents())
                {
                    comp.WhenSimulated(this);
                }
            } catch
            {
                MessageBox.Show("There was an error during the simulation");
            }
            
        }
        private void TransferSimulatedValues(CircuitGraph simulated)
        {
            foreach (Component comp in simulated.ListComponents())
            {
                if (simulated.graph.DoesEdgeExist(comp.LConnector, comp.RConnector))
                {
                    ElectricalProperties elecProps = simulated.graph.GetEdge(comp.LConnector, comp.RConnector);
                    this.graph.Connect(comp.LConnector, comp.RConnector, elecProps);
                }
            }
        }
        private CircuitGraph PrepareForSimulation()
        {
            CircuitGraph prepared = (CircuitGraph)this.Clone();
            foreach (Connector connector in this.graph.ListNodes())
            {
                foreach (Connector neighbour in this.RecursiveNeighbours(connector))
                {
                    if (!prepared.AreDirectlyConnected(connector, neighbour))
                    {
                        prepared.Connect(connector, neighbour);
                    }
                }
            }
            return prepared;
        }
        private bool Calculate()
        {
            ClearSimulatedProperties();
            Component[] components = this.ListInternallyConnectedComponents();

            if (components.Length == 0)
            {
                MessageBox.Show("The circuit is not connected!");
                return false;
            }

            DataStructsLib.List<ComponentCrossing[]> cycles = FindCycles();

            if (CheckForZeroResistanceLoops(cycles))
            {
                MessageBox.Show("Circuit not simulatable due to having a loop with zero resistance");
                return false;
            }

            ApplyPowerSupplyVoltages();

            if (cycles.Count > 0)
            {
                ComponentCrossing[][] loops = cycles.ToArray();
                var (loopExpressions, results) = FormLoopEqns(components, loops);
                var pointExpressions = this.CurrentLaw(components);
                var allExpressions = ConcatExpressions(loopExpressions, pointExpressions);
                var resultCurrents = Matrix.SolveSystemOfEquations(allExpressions, results.Concat(Enumerable.Repeat<double>(0, pointExpressions.Length).ToArray()).ToArray(), true);

                for (int i = 0; i < resultCurrents.Length; i++)
                {
                    Component comp = components[i];
                    //the graph is cleared of disconnected components before simulation, so the components will always be internally connected
                    ElectricalProperties edge = graph.GetEdge(comp.LConnector, comp.RConnector);
                    double resistance = edge.Resistance;
                    double emf = edge.Simulated?.Voltage ?? 0;

                    this.graph.Connect(comp.LConnector, comp.RConnector, new ElectricalProperties((resultCurrents[i] * resistance) + emf, resultCurrents[i], resistance));
                }
            }
            return true;
        }
        private bool CheckForZeroResistanceLoops(DataStructsLib.List<ComponentCrossing[]> cycles)
        {
            for (int i = 0; i < cycles.Count; i++)
            {
                if (IsZeroResistanceCycle(cycles[i]))
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsZeroResistanceCycle(ComponentCrossing[] cycle)
        {
            foreach (ComponentCrossing crossing in cycle)
            {
                if (crossing.Component.GetResistance() != 0)
                {
                    return false;
                }
            }
            return true;
        }
        private DataStructsLib.List<ComponentCrossing[]> FindCycles()
        {
            var path = new DataStructsLib.Stack<Connector>();
            var cycles = new DataStructsLib.List<ComponentCrossing[]>();
            var exploredLinks = new DataStructsLib.List<Link<Connector, ElectricalProperties>>();
            var includedComponents = new DataStructsLib.HashTable<Component, bool>();
            foreach (Component comp in this.ListComponents())
            {
                includedComponents[comp] = false;
            }
            Component? nextStart;
            while ((nextStart = includedComponents.FirstKeyOfValue(false)) is not null)
            {
                this.DFS_Cycles(nextStart.LConnector, ref path, ref cycles, ref exploredLinks, ref includedComponents);
            }

            return cycles;
        }
        private double[,] ConcatExpressions(double[,] loops, double[][] points)
        {
            int NoOfLoops = loops.GetLength(0);
            var expressions = new double[NoOfLoops + points.Length, loops.GetLength(1)];
            for (int i = 0; i < NoOfLoops; i++)
            {
                for (int j = 0; j < loops.GetLength(1); j++)
                {
                    expressions[i, j] = loops[i, j];
                }
            }

            for (int i = 0; i < points.Length; i++)
            {
                for (int j = 0; j < loops.GetLength(1); j++)
                {
                    expressions[i + NoOfLoops, j] = points[i][j];
                }
            }
            return expressions;
        }
        private double[][] CurrentLaw(Component[] components)
        {
            DataStructsLib.List<double[]> p = new DataStructsLib.List<double[]>();
            double[] expression;
            for (int i = 0; i < components.Length; i++)
            {
                //left connector
                expression = GetSingleCurrentEquation(components[i].LConnector, components);
                if (!AlreadyHasEquation(p, expression))
                {
                    p.Add(expression);
                }
                expression = GetSingleCurrentEquation(components[i].RConnector, components);
                if (!AlreadyHasEquation(p, expression))
                {
                    p.Add(expression);
                }
            }
            return p.ToArray();

        }
        private double[] GetSingleCurrentEquation(Connector connector, Component[] components)
        {
            double[] expression = new double[components.Length];
            foreach (Connector con in this.DirectNeighbours(connector).Append(connector))
            {
                Component comp = con.Component;
                int sign = (con.LogicalDirection == Direction.NegativeX) ? 1 : -1;
                int index = Array.IndexOf(components, comp);
                if (index == -1)
                {
                    continue;
                }
                expression[index] = sign;
            }
            return expression;

        }
        private static bool AlreadyHasEquation(DataStructsLib.List<double[]> expressions, double[] newExpression)
        {
            foreach (double[] exprsn in expressions.ToArray())
            {
                if (EquationsMatch(exprsn, newExpression))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool EquationsMatch(double[] expr1, double[] expr2)
        {
            for (int i = 0; i < expr1.Length; i++)
            {
                if (expr1[i] == 0 ^ expr2[i] == 0)
                {
                    return false;
                }
            }
            return true;
        }
        private void DFS_Cycles(Connector current, ref DataStructsLib.Stack<Connector> path,
            ref DataStructsLib.List<ComponentCrossing[]> cycles, ref DataStructsLib.List<Link<Connector, ElectricalProperties>> exploredLinks,
            ref DataStructsLib.HashTable<Component, bool> includedComponents)
        {
            if (path.Contains(current))
            {
                Connector[] arr = path.ToArray();
                DataStructsLib.List<ComponentCrossing> cycleList = new DataStructsLib.List<ComponentCrossing>();
                for (int i = 0; i < arr.Length; i++)
                {
                    Connector c = arr[i];
                    cycleList.Add(new ComponentCrossing(c.Component, c.LogicalDirection == Direction.PositiveX));
                    if (arr[i] == current)
                    {
                        break;
                    }
                }
                cycles.Add(cycleList.ToArray());
                return;
            }
            Connector? hopped = this.TryComponentHop(current);
            if (hopped != null && path.Contains(hopped))
            {
                return;
            }
            path.Push(current);
            includedComponents[current.Component] = true;
            Connector[] neighbours = this.DirectNeighbours(current);
            foreach (Connector n in neighbours)
            {
                if (exploredLinks.ToArray().Any(l => (l.Start == current && l.End == n) || (l.Start == n && l.End == current)))
                {
                    continue;
                }
                exploredLinks.Add(new Link<Connector, ElectricalProperties>(current, n, new ElectricalProperties(0)));
                hopped = this.TryComponentHop(n);
                if (hopped != null)
                {
                    DFS_Cycles(hopped, ref path, ref cycles, ref exploredLinks, ref includedComponents);
                }
            }
            path.Pop();

        }



        private void ApplyPowerSupplyVoltages()
        {
            foreach (Component comp in this.ListComponents())
            {
                if (comp is PowerSupply powerSupply)
                {
                    double resistance = graph.GetEdge(comp.LConnector, comp.RConnector).Resistance;
                    graph.Connect(comp.LConnector, comp.RConnector, new ElectricalProperties(powerSupply.EMF, 0, resistance));
                }
            }
        }
        private void ClearSimulatedProperties()
        {
            foreach (Component comp in this.ListComponents())
            {
                if (this.AreDirectlyConnected(comp.LConnector, comp.RConnector))
                {
                    ElectricalProperties props = this.graph.GetEdge(comp.LConnector, comp.RConnector);
                    this.graph.Connect(comp.LConnector, comp.RConnector, new ElectricalProperties(props.Resistance));
                }
            }
        }
        private (double[,] expressions, double[] results) FormLoopEqns(Component[] components, ComponentCrossing[][] loops)
        {
            double[,] expressions = new double[loops.Length, components.Length];
            double[] results = new double[loops.Length];
            for (int i = 0; i < loops.Length; i++)
            {
                ComponentCrossing[] loop = loops[i];
                double vIn = 0;
                for (int j = 0; j < loop.Length; j++)
                {
                    Component comp = loop[j].Component;
                    int sign = loop[j].LeftStart ? 1 : -1;
                    int index = Array.IndexOf(components, comp);
                    if (comp is PowerSupply power)
                    {
                        vIn += sign * power.EMF;

                    }
                    expressions[i, index] = sign * comp.GetResistance();
                }
                results[i] = -vIn;

            }
            return (expressions, results);


        }
        private struct ComponentCrossing
        {
            public Component Component;
            public bool LeftStart;
            public ComponentCrossing(Component component, bool leftStart)
            {
                this.Component = component;
                this.LeftStart = leftStart;
            }
            public ComponentCrossing(Connector start)
            {
                this.Component = start.Component;
                this.LeftStart = start.LogicalDirection == Direction.NegativeX;
            }
        }
    }
}
