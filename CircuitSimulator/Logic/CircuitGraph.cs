using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructsLib;
using Circuits.UI;
using System.Diagnostics;
using System.Data;
using System.Runtime.CompilerServices;

namespace Circuits.Logic
{
    internal partial class CircuitGraph : IStringableDataStruct<Connector>, ICloneable, IDisposable
    {
        protected Graph<Connector, ElectricalProperties> graph;
        public CircuitGraph()
        {
            graph = new Graph<Connector, ElectricalProperties>();
        }

        /// <summary>
        /// Saves the circuit to the database
        /// </summary>
        public DecomposedCircuitGraph Decompose()
        {
            Connector[] connectors = graph.ListNodes();
            Component[] components = new Component[connectors.Length / 2];
            for (int i = 0; i < connectors.Length; i += 2)
            {
                components[i / 2] = connectors[i].Component;
            }
            DataStructsLib.List<Link<Connector, ElectricalProperties>> wires = new DataStructsLib.List<Link<Connector, ElectricalProperties>>(graph.ListEdges());
            //remove internal wire inside component
            for (int i = wires.Count - 1; i > -1; i--)
            {
                if (wires[i].Start.Component == wires[i].End.Component)
                {
                    wires.RemoveAt(i);
                }
            }
            return new DecomposedCircuitGraph(components, connectors, wires.ToArray());
        }

        /// <summary>
        /// Returns if a link is between two connectors from teh same component
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        protected bool IsInternalLink(Link<Connector, ElectricalProperties> link)
        {
            return link.Start.Component == link.End.Component;
        }
        /// <summary>
        /// Returns all the components in the circuit
        /// </summary>
        /// <returns></returns>
        public Component[] ListComponents()
        {
            Connector[] connectors = graph.ListNodes();
            DataStructsLib.List<Component> components = new DataStructsLib.List<Component>();
            if (connectors.Length == 0)
            {
                return Array.Empty<Component>();
            }
            components.Add(connectors[0].Component);
            for (int i = 1; i < connectors.Length; i++)
            {
                Component comp = connectors[i].Component;
                if (comp != components[components.Count - 1])
                {
                    components.Add(comp);
                }
            }
            return components.ToArray();
        }
        public Component[] ListInternallyConnectedComponents()
        {
            DataStructsLib.List<Component> components = new DataStructsLib.List<Component>(this.ListComponents());
            for (int i = components.Count - 1; i > -1; i--)
            {
                if (!this.graph.DoesEdgeExist(components[i].LConnector, components[i].RConnector))
                {
                    components.RemoveAt(i);
                }
            }
            return components.ToArray();
        }
        /// <summary>
        /// Lists all wires leaving a Connector. Excludes the internal connection for the component
        /// </summary>
        /// <returns></returns>
        public Link<Connector, ElectricalProperties>[] ListWires()
        {
            DataStructsLib.List<Link<Connector, ElectricalProperties>> wires = new DataStructsLib.List<Link<Connector, ElectricalProperties>>(graph.ListEdges());
            for (int i = wires.Count - 1; i > -1; i--)
            {
                if (IsInternalLink(wires[i]))
                {
                    wires.RemoveAt(i);
                }
            }
            return wires.ToArray();
        }
        public Connector? TryComponentHop(Connector connector)
        {
            Connector hopped = connector.Hop();
            return graph.DoesEdgeExist(connector, hopped) ? hopped : null;
        }
        /// <summary>
        /// Returns all neighbours reachable by traversing 1 wire
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Connector[] NeighboursAndHop(Connector node)
        {
            return graph.Neighbours(node);
        }
        /// <summary>
        /// Returns all different component neighbours reachable by traversing 1 wire
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Connector[] DirectNeighbours(Connector node)
        {
            Connector[] allNeighbours = graph.Neighbours(node);
            DataStructsLib.List<Connector> neighboursList = new DataStructsLib.List<Connector>(allNeighbours);
            for (int i = neighboursList.Count - 1; i > -1; i--)
            {
                if (neighboursList[i].Component == node.Component)
                {
                    neighboursList.RemoveAt(i);
                }
            }
            return neighboursList.ToArray();
        }

        /// <summary>
        /// Returns if two connectors are connected by a single wire or internal connection
        /// </summary>
        /// <param name="connector1"></param>
        /// <param name="connector2"></param>
        /// <returns></returns>
        public bool AreDirectlyConnected(Connector connector1, Connector connector2)
        {

            return graph.DoesEdgeExist(connector1, connector2);
        }
        /// <summary>
        /// Returns the resistance between two adjacent components
        /// </summary>
        /// <param name="connector1"></param>
        /// <param name="connector2"></param>
        /// <returns></returns>
        public double ResistanceBetween(Connector connector1, Connector connector2)
        {
            if (!AreDirectlyConnected(connector1, connector2))
            {
                throw new ArgumentException("The connectors where not directly connected");
            }
            return graph.GetEdge(connector1, connector2).Resistance;
        }
        /// <summary>
        /// Finds and returns all conenctors that can be reached from a start without crossing a component
        /// </summary>
        /// <param name="connector"></param>
        /// <returns></returns>
        public Connector[] RecursiveNeighbours(Connector connector)
        {
            DataStructsLib.List<Connector> discovered = new DataStructsLib.List<Connector>();
            RecursiveDFSForNeighbours(connector, ref discovered);
            return discovered.ToArray();
        }
        /// <summary>
        /// Recursive Depth First Search to find all neighbours
        /// </summary>
        /// <param name="current"></param>
        /// <param name="discovered"></param>
        private void RecursiveDFSForNeighbours(Connector current, ref DataStructsLib.List<Connector> discovered)
        {
            discovered.Add(current);
            Connector[] directNeighbours = this.DirectNeighbours(current);
            for (int i = 0; i < directNeighbours.Length; i++)
            {
                if (!discovered.Contains(directNeighbours[i]))
                {
                    RecursiveDFSForNeighbours(directNeighbours[i], ref discovered);
                }
            }
        }
        public SimulatedElectricalProperties CurrentVoltageBetween(Connector connector1, Connector connector2)
        {
            if (!AreDirectlyConnected(connector1, connector2))
            {
                throw new ArgumentException("The connectors where not directly connected");
            }
            return graph.GetEdge(connector1, connector2).Simulated ?? new SimulatedElectricalProperties(0, 0);

        }
        public double FindVoltageAcross(Connector connector1, Connector connector2)
        {
            if (this.TryComponentHop(connector1) == connector2)
            {
                return graph.GetEdge(connector1, connector2).Simulated?.Voltage ?? 0;
            }
            DataStructsLib.Stack<Connector> path = new DataStructsLib.Stack<Connector>();
            Connector[] nodes = this.graph.ListNodes();
            HashTable<Connector, bool> visited = new HashTable<Connector, bool>();
            foreach (Connector n in nodes)
            {
                visited[n] = false;
            }
            double voltage = 0;
            _ = this.DFSVoltageAcross(connector2, connector1, ref voltage, ref visited);
            return voltage;

        }

        private bool DFSVoltageAcross(Connector lookingFor, Connector current, ref double cumulativeVoltage, ref HashTable<Connector, bool> visited)
        {

            visited[current] = true;
            Connector[] toVisit = this.DirectNeighbours(current);

            foreach (Connector neighbour in toVisit)
            {
                if (neighbour == lookingFor)
                {
                    return true;
                }
                Connector? neighbourHop = this.TryComponentHop(neighbour);
                if (neighbourHop is null || visited[neighbourHop])
                {
                    continue;
                }
                int sign = (neighbour.LogicalDirection == Direction.NegativeX) ? 1 : -1;
                cumulativeVoltage += sign * FindVoltageAcross(neighbour, neighbourHop);
                if (DFSVoltageAcross(lookingFor, neighbourHop, ref cumulativeVoltage, ref visited))
                {
                    return true;
                }
                cumulativeVoltage -= sign * FindVoltageAcross(neighbour, neighbourHop);
            }

            return false;
        }



        public string Stringify(bool withnewLines, Func<Connector, string> transform)
        {
            return graph.Stringify(withnewLines, transform);
        }
        public string Stringify(bool withNewLines)
        {
            return graph.Stringify(withNewLines);
        }
        /// <summary>
        /// Creates a clone of the circuit
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            CircuitGraph newCircuit = new CircuitGraph();
            foreach (Component component in this.ListComponents())
            {
                newCircuit.AddComponent(component);
                if (this.graph.DoesEdgeExist(component.LConnector, component.RConnector))
                {
                    ElectricalProperties props = this.graph.GetEdge(component.LConnector, component.RConnector);
                    newCircuit.graph.Connect(component.LConnector, component.RConnector, props);
                }
            }
            foreach (Link<Connector, ElectricalProperties> link in this.ListWires())
            {
                newCircuit.graph.Connect(link.Start, link.End, link.Weight);
            }
            return newCircuit;
        }

        public virtual void Dispose()
        {
            graph = null;
        }

        /// <summary>
        /// Adds a component to the circuit
        /// </summary>
        /// <param name="component"></param>
        public void AddComponent(Component component)
        {
            graph.AddNode(component.LConnector);
            graph.AddNode(component.RConnector);
            if (component.GetResistance() != double.PositiveInfinity)
            {
                graph.Connect(component.LConnector, component.RConnector, new ElectricalProperties(component.GetResistance()));
            }
            component.ResistanceChanged += UpdateResistance;
            component.Deleted += ComponentDeleteEventHandler;
        }
        /// <summary>
        /// Automatically updates the graph weight when a components resistance changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateResistance(object? sender, EventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            Component component = (Component)sender;
            double resistance = component.GetResistance();
            if (resistance == double.PositiveInfinity)
            {
                graph.Disconnect(component.LConnector, component.RConnector);
            } else
            {
                graph.Connect(component.LConnector, component.RConnector, new ElectricalProperties(resistance));
            }

        }
        
        /// <summary>
        /// Connect two connectors
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void Connect(Connector start, Connector end)
        {
            graph.Connect(start, end, new ElectricalProperties(0D));
        }
        /// <summary>
        /// Connect two connectors
        /// </summary>
        /// <param name="wire"></param>
        public void Connect(Wire wire)
        {
            graph.Connect(wire.Start, wire.End, new ElectricalProperties(0D));
        }
        public void Disconnect(Connector start, Connector end)
        {
            graph.Disconnect(start, end);
        }
        /// <summary>
        /// Remove a connection between two connectors
        /// </summary>
        /// <param name="wire"></param>
        public void Disconnect(Wire wire)
        {
            graph.SafeTryDisconnect(wire.Start, wire.End);
        }
        protected void ComponentDeleteEventHandler(object? sender, EventArgs e)
        {
            RemoveComponent((Component)sender);
        }

        public void RemoveComponent(Component component)
        {
            graph.RemoveNode(component.LConnector);
            graph.RemoveNode(component.RConnector);
            component.ResistanceChanged -= UpdateResistance;
            component.Deleted -= ComponentDeleteEventHandler;

        }
        /// <summary>
        /// Renders the circuit in a given control collection
        /// </summary>
        /// <param name="ctrlCollection"></param>
        public void RenderInControl(Control.ControlCollection ctrlCollection)
        {
            Connector[] connectors = graph.ListNodes();
            for (int i = 0; i < connectors.Length; i += 2)
            {
                ctrlCollection.Add(connectors[i].Component);
            }
            Link<Connector, ElectricalProperties>[] links = graph.ListEdges();
            for (int i = 0; i < links.Length; i++)
            {
                if (!IsInternalLink(links[i]))
                {
                    Wire wire = new Wire(links[i].Start, links[i].End);
                    wire.Deleted += (s, e) => this.Disconnect(wire);
                    links[i].Start.Component.Deleted += wire.ComponentDeleteEventHandler;
                    links[i].End.Component.Deleted += wire.ComponentDeleteEventHandler;
                    ctrlCollection.Add(wire);
                }
            }
        }
      
    }
    internal struct DecomposedCircuitGraph
    {
        public Component[] Components { get; private set; }
        public Connector[] Connectors { get; private set; }
        public Link<Connector, ElectricalProperties>[] Wires { get; private set; }

        public DecomposedCircuitGraph(Component[] components, Connector[] connectors, Link<Connector, ElectricalProperties>[] wires)
        {
            Components = components;
            Connectors = connectors;
            Wires = wires;
        }
    }
}
