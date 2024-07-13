using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Circuits.UI;
using DataStructsLib;
using System.Data.Common;
using Circuits.Diagramming;
using System.Security.Policy;
using System.Runtime.CompilerServices;
using System.Data.Entity.Core.Metadata.Edm;
using System.Xml.Linq;
using System.Data.Entity.Spatial;
using System.Web;

namespace Circuits.Logic
{
    internal static class DatabaseManager
    {
        const string dateFormat = "yyyy-MM-dd HH:mm:ss";
        public static void InitDB()
        {
            DatabaseSession dbSession = new DatabaseSession();
            dbSession.InitDB();
            dbSession.Dispose();
        }
        public static long? Save(CircuitEnvironment environment, DecomposedCircuitGraph decomposedGraph, string? name, string? author, long? saveID = null)
        {
            Component[] components = decomposedGraph.Components;
            Connector[] connectors = decomposedGraph.Connectors;
            Link<Connector, ElectricalProperties>[] wires = decomposedGraph.Wires;

            if (name?.Length > DatabaseSession.NameMaxLength)
            {
                MessageBox.Show($"Name length must be less than {DatabaseSession.NameMaxLength}. Save process cancelled");
                return null;
            }
            if (author?.Length > DatabaseSession.AuthorMaxLength)
            {
                MessageBox.Show($"Author length must be less than {DatabaseSession.AuthorMaxLength}. Save process cancelled");
                return null;
            }

            try
            {
                DatabaseSession dbSession = new DatabaseSession();
                Link<long, double>[] wiresWithPrimaryKeys = new Link<long, double>[wires.Length];
                if (saveID != null && dbSession.GetSave(saveID.Value) != null) //overwrite
                {
                    DatabaseManager.DeleteCircuitOnly(saveID.Value, dbSession);
                    dbSession.ModifyEnvironment(environment.Data, saveID.Value);
                    dbSession.ModifySave(name, author, null, saveID.Value);
                }
                else //new save
                {
                    long environmentID = dbSession.AddEnvironment(environment.Data);
                    saveID = dbSession.AddSave(name ?? $"Circuit_{saveID}", author ?? "_", environmentID, saveID);
                }


                long[] componentIDs = dbSession.AddComponents(components, saveID.Value);
                long[] connectorIDs = dbSession.AddConnectors(connectors, componentIDs);
                for (int i = 0; i < wires.Length; i++)
                {
                    int startIndex = Array.IndexOf(connectors, wires[i].Start);
                    int endIndex = Array.IndexOf(connectors, wires[i].End);
                    wiresWithPrimaryKeys[i] = new Link<long, double>(connectorIDs[startIndex], connectorIDs[endIndex], wires[i].Weight.Resistance);
                }
                dbSession.AddWires(wiresWithPrimaryKeys);
                dbSession.Dispose();
                return saveID.Value;
        } catch
            {
                MessageBox.Show("There was an error saving, please try again");
                return null;
            }
}
        public static (CircuitGraph, CircuitEnvironment)? Load(long saveID)
        {
            DatabaseSession dbSession = new DatabaseSession();
            try
            {
                DBSave? save = dbSession.GetSave(saveID);
                if (save is null)
                {
                    MessageBox.Show($"Save {saveID} does not exist");
                    return null;
                }

                DBComponent[] components = dbSession.GetComponentsWithData(saveID);
                DBWire[] wires = dbSession.GetWires(saveID);

                for (int i = 0; i < components.Length; i++)
                {
                    DBConnector[] connectors = dbSession.GetSingleComponentConnectors(components[i].PrimaryKey);
                    components[i].LeftConnector = connectors.First(c => c.LogicalPosition == Direction.NegativeX);
                    components[i].RightConnector = connectors.First(c => c.LogicalPosition == Direction.PositiveX);
                }


                DBEnvironment dbEnvironment = dbSession.GetEnvironment(saveID);
                dbSession.Dispose();

                CircuitEnvironment environment = new CircuitEnvironment(dbEnvironment.Light, dbEnvironment.Temperature);
                CircuitGraph circuit = BuildCircuit(wires, components, environment);
                return (circuit, environment);
            } catch
            {
                MessageBox.Show("There was an error loading the circuit, please try again");
                return null;
            }
        }
        public static void Delete(long saveID)
        {
            DatabaseSession dbSession = new DatabaseSession();
            try
            {
                if (dbSession.GetSave(saveID) is null)
                {
                    MessageBox.Show("That save does not exist");
                    return;
                }
                DeleteCircuitOnly(saveID, dbSession);
                dbSession.DeleteEnvironment(saveID);
                dbSession.DeleteSave(saveID);
                dbSession.Dispose();
            } catch
            {
                MessageBox.Show("There was an error deleting the circuit, please try again");
            }
        }
        private static void DeleteCircuitOnly(long saveID, DatabaseSession dbSession)
        {
            dbSession.DeleteWires(saveID);
            dbSession.DeleteConnectors(saveID);
            dbSession.DeleteComponentsAndData(saveID);
            
        }
        public static SaveInfo[] GetSaves()
        {
            DatabaseSession dbSession = new DatabaseSession();
            try
            {
                SaveInfo[] saves = dbSession.GetSaves().Select(dbsave => new SaveInfo(
                    dbsave.PrimaryKey,
                    dbsave.Name,
                    dbsave.Author,
                    dbsave.LastModified
                    )).ToArray();
                dbSession.Dispose();
                return saves;
            } catch
            {
                MessageBox.Show("There was an error loading the save files, please try again");
                return Array.Empty<SaveInfo>();
            }
        }
        private static CircuitGraph BuildCircuit(DBWire[] dbWires, DBComponent[] dbComponents, CircuitEnvironment environment)
        {
            CircuitGraph circuit = new CircuitGraph();
            Component[] components = new Component[dbComponents.Length];
            for (int i = 0; i < dbComponents.Length; i++)
            {

                Component newComp = Component.MakeTypeFromString(dbComponents[i].ComponentType, dbComponents[i].Data, environment);
                newComp.MainControl.Location = dbComponents[i].Position;
                while (newComp.Orientation != dbComponents[i].Orientation)
                {
                    newComp.Rotate90();
                }
                components[i] = newComp;
                circuit.AddComponent(newComp);
            }
            foreach (DBWire dbWire in dbWires)
            {
                Connector? start = null;
                Connector? end = null;
                for (int i = 0; i < dbComponents.Length; i++)
                {
                    if (dbComponents[i].LeftConnector.PrimaryKey == dbWire.ConnectorID1)
                    {
                        start = components[i].LConnector;
                    }
                    else if (dbComponents[i].RightConnector.PrimaryKey == dbWire.ConnectorID1)
                    {
                        start = components[i].RConnector;
                    }
                    else if (dbComponents[i].LeftConnector.PrimaryKey == dbWire.ConnectorID2)
                    {
                        end = components[i].LConnector;
                    }
                    else if (dbComponents[i].RightConnector.PrimaryKey == dbWire.ConnectorID2)
                    {
                        end = components[i].RConnector;
                    }
                }
                if (!(start is null || end is null))
                {
                    circuit.Connect(start, end);
                }

            }
            return circuit;
        }

        private class DatabaseSession : IDisposable
        {
            const string connectionString = "Data Source=circuits.db;Version=3;";
            public const int NameMaxLength = 30;
            public const int AuthorMaxLength = 20;
            
            private readonly SQLiteConnection connection;
            public DatabaseSession(bool openConenction = true)
            {
                connection = new SQLiteConnection(connectionString);
                if (openConenction)
                {
                    this.OpenConnection();
                }
            }
            public void OpenConnection()
            {
                try
                {
                    connection.Open();
                    var WALdisable = new SQLiteCommand("PRAGMA journal_mode = 'wal'", connection); //this is required so that the delete function works
                    WALdisable.ExecuteNonQuery();
                    WALdisable.Dispose();
                } catch
                {
                    MessageBox.Show("There was an error connecting to the database");
                }
            }
            public void CloseConnection()
            {
                connection.Close();
                connection.Dispose();
            }
            public void Dispose()
            {
                this.CloseConnection();
                GC.SuppressFinalize(this);
            }
            public void InitDB()
            {
                string[] tableCreationStrings = new string[]
                {
                    $"CREATE TABLE IF NOT EXISTS Environment(EnvironmentID INTEGER PRIMARY KEY AUTOINCREMENT, Light FLOAT(24) NOT NULL, Temperature FLOAT(24) NOT NULL);",
                    $"CREATE TABLE IF NOT EXISTS Save(SaveID INTEGER PRIMARY KEY AUTOINCREMENT,Name VARCHAR({NameMaxLength}) NOT NULL,Author VARCHAR({AuthorMaxLength}) NOT NULL,LastModified DATETIME NOT NULL, EnvironmentID INTEGER NOT NULL, FOREIGN KEY (EnvironmentID) REFERENCES Envrionment(EnvironmentID));",
                    $"CREATE TABLE IF NOT EXISTS Component(ComponentID INTEGER PRIMARY KEY AUTOINCREMENT,SaveID INTEGER NOT NULL,ComponentType VARCHAR(50) NOT NULL, PositionX INTEGER NOT NULL, PositionY INTEGER NOT NULL, Orientation INTEGER NOT NULL, FOREIGN KEY (SaveID) REFERENCES Save(SaveID));",
                    $"CREATE TABLE IF NOT EXISTS ComponentDataElement(ComponentID INTEGER NOT NULL,DataType INTEGER NOT NULL,DataValue FLOAT(24),FOREIGN KEY (ComponentID) REFERENCES Component(ComponentID)PRIMARY KEY (ComponentID, DataType));",
                    $"CREATE TABLE IF NOT EXISTS Connector(ConnectorID INTEGER PRIMARY KEY AUTOINCREMENT,ComponentID INTEGER NOT NULL,LogicalPosition INTEGER NOT NULL,FOREIGN KEY (ComponentID) REFERENCES Component(ComponentID));",
                    $"CREATE TABLE IF NOT EXISTS Wire(WireID INTEGER PRIMARY KEY AUTOINCREMENT,ConnectorID1 NOT NULL,ConnectorID2 NOT NULL,FOREIGN KEY (ConnectorID1) REFERENCES Connector(ConnectorID),FOREIGN KEY (ConnectorID2) REFERENCES Connector(ConnectorID2));"
                };
                SQLiteCommand[] createTables = Array.ConvertAll(tableCreationStrings, str => new SQLiteCommand(str, connection));
                foreach (SQLiteCommand command in createTables)
                {
                    command.ExecuteNonQuery();
                }
            }

            public long[] AddComponents(Component[] components, long saveID)
            {
                DataStructsLib.List<long> primaryKeys = new DataStructsLib.List<long>();
                for (int i = 0; i < components.Length; i++)
                {
                    string AddCommand = $"INSERT INTO Component (SaveID, ComponentType, PositionX, PositionY, Orientation) VALUES (@saveID, @type, @x, @y, @orientation)";
                    SQLiteCommand command = new SQLiteCommand(AddCommand, connection);
                    command.Parameters.AddWithValue("@saveID", saveID);
                    command.Parameters.AddWithValue("@type", components[i].Type);
                    command.Parameters.AddWithValue("@x", components[i].MainControl.Left);
                    command.Parameters.AddWithValue("@y", components[i].MainControl.Top);
                    command.Parameters.AddWithValue("@orientation", components[i].Orientation);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    long primaryKey = GetLastID();
                    primaryKeys.Add(primaryKey);
                    AddComponentDataElements(components[i], primaryKey);
                }
                return primaryKeys.ToArray();
            }
            private long[] AddComponentDataElements(Component component, long componentPrimaryKey)
            {
                DataStructsLib.List<long> primaryKeys = new DataStructsLib.List<long>();
                HashTable<ComponentDataType, double> data = component.GetDataElements();
                ComponentDataType[] keys = data.KeysArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    string AddCommand = $"INSERT INTO ComponentDataElement (ComponentID, DataType, DataValue) VALUES (@compID, @type, @value)";
                    SQLiteCommand command = new SQLiteCommand(AddCommand, connection);
                    command.Parameters.AddWithValue("@compID", componentPrimaryKey);
                    command.Parameters.AddWithValue("@type", (int)keys[i]);
                    command.Parameters.AddWithValue("@value", data[keys[i]]);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    primaryKeys.Add(GetLastID());
                }
                return primaryKeys.ToArray();
            }
            public long[] AddConnectors(Connector[] connectors, long[] componentPrimaryKeys)
            {
                DataStructsLib.List<long> primaryKeys = new DataStructsLib.List<long>();
                for (int i = 0; i < connectors.Length; i++)
                {
                    string AddCommand = $"INSERT INTO Connector (ComponentID, LogicalPosition) VALUES (@componentID, @logicalPosition)";
                    SQLiteCommand command = new SQLiteCommand(AddCommand, connection);
                    command.Parameters.AddWithValue("@componentID", componentPrimaryKeys[i / 2]);
                    command.Parameters.AddWithValue("@logicalPosition", connectors[i].LogicalDirection);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    primaryKeys.Add(GetLastID());
                }
                return primaryKeys.ToArray();
            }

            public long AddEnvironment(HashTable<EnvironmentDataType, double> environmentData)
            {
                string commandText = "INSERT INTO Environment (Light, Temperature) VALUES (@light, @temperature)";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@light", environmentData[EnvironmentDataType.Light]);
                command.Parameters.AddWithValue("@temperature", environmentData[EnvironmentDataType.Temperature]);
                command.ExecuteNonQuery();
                return GetLastID();
            }

            public long AddSave(string name, string author, long environmentID, long? saveID)
            {
                long primaryKey;
                string AddCommand = saveID == null
                    ? $"INSERT INTO Save (Name, Author, LastModified, EnvironmentID) VALUES (@name, @author, @lastModified, @environmentID)"
                    : $"INSERT INTO Save (SaveID, Name, Author, LastModified, EnvironmentID) VALUES ({saveID}, @name, @author, @lastModified, @environmentID)";
                SQLiteCommand command = new SQLiteCommand(AddCommand, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@author", author);
                command.Parameters.AddWithValue("@lastModified", DateTime.UtcNow.ToString(dateFormat));
                command.Parameters.AddWithValue("@environmentID", environmentID);
                command.ExecuteNonQuery();
                command.Dispose();
                primaryKey = GetLastID();
                return primaryKey;
            }

            public long[] AddWires(Link<long, double>[] wires)
            {
                DataStructsLib.List<long> primaryKeys = new DataStructsLib.List<long>();
                for (int i = 0; i < wires.Length; i++)
                {
                    string AddCommand = $"INSERT INTO Wire (ConnectorID1, ConnectorID2) VALUES (@connectorID1, @connectorID2)";
                    SQLiteCommand command = new SQLiteCommand(AddCommand, connection);
                    command.Parameters.AddWithValue("@connectorID1", wires[i].Start);
                    command.Parameters.AddWithValue("@connectorID2", wires[i].End);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    primaryKeys.Add(GetLastID());
                }
                return primaryKeys.ToArray();
            }
            public DBComponent[] GetComponentsWithData(long saveID)
            {
                string commandText = $"SELECT * FROM Component LEFT JOIN ComponentDataElement ON Component.ComponentID = ComponentDataElement.ComponentID WHERE SaveID = @saveID";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                SQLiteDataReader reader = command.ExecuteReader();
                command.Dispose();
                HashTable<long, DBComponent> components = new HashTable<long, DBComponent>();
                while (reader.Read())
                {
                    long componentID = (long)reader["ComponentID"];
                    if (!components.ContainsKey(componentID))
                    {
                        components[componentID] = new DBComponent(
                            componentID,
                            (string)reader["ComponentType"],
                            (int)(long)reader["PositionX"],
                            (int)(long)reader["PositionY"],
                            (Circuits.UI.Orientation)(long)reader["Orientation"]
                        );
                    }
                    object dataValue = reader["DataValue"];
                    if (dataValue is not DBNull)
                    {
                        components[componentID].Data[(ComponentDataType)(long)reader["DataType"]] = (double)dataValue;
                    }
                }
                return components.ValuesArray();
            }
            public DBWire[] GetWires(long saveID)
            {
                string commandText = @"SELECT Wire.*
                                        FROM Component, Connector, Wire
                                        WHERE SaveID = @saveID
                                            AND Connector.ComponentID = Component.ComponentID
                                            AND Connector.ConnectorID = Wire.ConnectorID1";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                SQLiteDataReader reader = command.ExecuteReader();
                command.Dispose();
                DataStructsLib.List<DBWire> wires = new DataStructsLib.List<DBWire>();
                while (reader.Read())
                {
                    wires.Add(new DBWire(
                        (long)reader["WireID"],
                        (long)reader["ConnectorID1"],
                        (long)reader["ConnectorID2"]
                    ));
                }
                return wires.ToArray();
            }
            
            public DBConnector[] GetSingleComponentConnectors(long componentID)
            {
                string commandText = $"SELECT * FROM Connector WHERE ComponentID = @componentID";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@componentID", componentID);
                SQLiteDataReader reader = command.ExecuteReader();
                command.Dispose();
                DataStructsLib.List<DBConnector> connectorList = new DataStructsLib.List<DBConnector>();
                while (reader.Read())
                {
                    connectorList.Add(new DBConnector(reader.GetInt64(reader.GetOrdinal("ConnectorID")), reader.GetInt64(reader.GetOrdinal("ComponentID")), reader.GetBoolean(reader.GetOrdinal("LogicalPosition")) ? Direction.NegativeX : Direction.PositiveX));
                }
                return connectorList.ToArray();
            }

            public DBSave? GetSave(long saveID)
            {
                string commandText = "SELECT * FROM Save WHERE SaveID = @saveID";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                SQLiteDataReader reader = command.ExecuteReader();
                command.Dispose();
                if (!reader.HasRows)
                {
                    return null;
                }
                reader.Read();
                return new DBSave(
                    saveID,
                    (string)reader["Name"],
                    (string)reader["Author"],
                    (DateTime)reader["LastModified"],
                    (long)reader["environmentID"]
                );
            }

            public DBEnvironment GetEnvironment(long saveID)
            {
                string commandText = "SELECT Environment.* FROM Environment, Save WHERE SaveID = @saveID AND Environment.EnvironmentID = Save.EnvironmentID LIMIT 1";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                SQLiteDataReader reader = command.ExecuteReader();
                command.Dispose();
                reader.Read();
                return new DBEnvironment(
                    (long)reader["environmentID"],
                    (double)reader["Light"],
                    (double)reader["Temperature"]
                );
            }
            
            public void DeleteWires(long saveID)
            {
                string commandText = @"DELETE FROM Wire
                                        WHERE ConnectorID1 IN (
                                            SELECT ConnectorID
                                            FROM Component, Connector
                                            WHERE SaveID = @saveID
                                            AND Connector.ComponentID = Component.ComponentID)";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            public void DeleteConnectors(long saveID)
            {
                string commandText = @"DELETE FROM Connector
                                        WHERE ComponentID IN (
                                            SELECT ComponentID
                                            FROM Component
                                            WHERE SaveID = @saveID)";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            public void DeleteComponentsAndData(long saveID)
            {
                DeleteComponentData(saveID);

                //delete components
                string commandText = $"DELETE FROM Component WHERE SaveID = @saveID";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            private void DeleteComponentData(long saveID)
            {
                string commandText = @"DELETE FROM ComponentDataElement
                                        WHERE ComponentID IN (
                                            SELECT ComponentID
                                            FROM Component
                                            WHERE SaveID = @saveID)";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            public void DeleteEnvironment(long saveID)
            {
                string commandText = @"DELETE FROM Environment
                                        WHERE EnvironmentID IN (
                                            SELECT EnvironmentID
                                            FROM Save
                                            WHERE SaveID = @saveID)";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            public void DeleteSave(long saveID)
            {
                string commandText = $"DELETE FROM Save WHERE SaveID = @saveID";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@saveID", saveID);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            
            public DBSave[] GetSaves()
            {
                string commandText = $"SELECT * FROM Save";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                DataStructsLib.List<DBSave> saves = new DataStructsLib.List<DBSave>();
                SQLiteDataReader reader = command.ExecuteReader();
                command.Dispose();
                while (reader.Read())
                {
                    saves.Add(new DBSave(
                        (long)reader["SaveID"],
                        (string)reader["Name"],
                        (string)reader["Author"],
                        (DateTime)reader["LastModified"],
                        (long)reader["environmentID"]
                    ));
                }
                return saves.ToArray();
            }

            public void ModifySave(string? name, string? author, long? environmentID, long saveID)
            {
                string commandText = "UPDATE Save SET LastModified = @lastModified ";
                if (name is not null) commandText += "Name = @name,";
                if (author is not null) commandText += "Author = @author,";
                if (environmentID is not null) commandText += "EnvironmentID = @environmentID,";
                if (commandText.Last() == ',') commandText = commandText.Substring(0, commandText.Length - 1) + " ";
                commandText += "WHERE SaveID = @saveID";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@author", author);
                command.Parameters.AddWithValue("@lastModified", DateTime.UtcNow.ToString(dateFormat));
                command.Parameters.AddWithValue("@environmentID", environmentID);
                command.Parameters.AddWithValue("@saveID", saveID);
                command.ExecuteNonQuery();
                command.Dispose();
            }

            public void ModifyEnvironment(HashTable<EnvironmentDataType, double> environmentData, long saveID)
            {
                string commandText = @"UPDATE Environment
                                        SET Light = @light, Temperature = @temperature
                                        FROM Save
                                        WHERE Save.EnvironmentID = Environment.EnvironmentID
                                        AND Save.SaveID = @saveID";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@light", environmentData[EnvironmentDataType.Light]);
                command.Parameters.AddWithValue("@temperature", environmentData[EnvironmentDataType.Temperature]);
                command.Parameters.AddWithValue("@saveID", saveID);
                command.ExecuteNonQuery();
                command.Dispose();

            }


            private long GetLastID()
            {
                SQLiteCommand cmd = new SQLiteCommand("SELECT last_insert_rowid()", connection);
                return (long)cmd.ExecuteScalar();
            }
        }

        private struct DBWire : IDBDataType
        {
            public long PrimaryKey { get; set; }
            public long ConnectorID1 { get; set; }
            public long ConnectorID2 { get; set; }
            public DBWire(long primaryKey, long connectorID1, long connectorID2)
            {
                PrimaryKey = primaryKey;
                ConnectorID1 = connectorID1;
                ConnectorID2 = connectorID2;
            }
        }
        private struct DBConnector : IDBDataType
        {
            public long PrimaryKey { get; set; }
            public long ComponentID { get; set; }
            public Direction LogicalPosition { get; set; }
            public DBConnector(long primaryKey, long componentID, Direction logicalPosition)
            {
                PrimaryKey = primaryKey;
                ComponentID = componentID;
                LogicalPosition = logicalPosition;
            }
            public DBConnector(long primaryKey, long componentID, int logicalPosition)
            {
                PrimaryKey = primaryKey;
                ComponentID = componentID;
                LogicalPosition = (Direction)logicalPosition;
            }

        }
        private struct DBComponent : IDBDataType
        {
            public long PrimaryKey { get; set; }
            public string ComponentType { get; set; }
            public Point Position { get; set; }
            public Circuits.UI.Orientation Orientation { get; set; }
            public HashTable<ComponentDataType, double> Data { get; set; }
            public DBConnector LeftConnector { get; set; }
            public DBConnector RightConnector { get; set; }
            public DBComponent(long primaryKey, string componentType, int PositionX, int PositionY, Circuits.UI.Orientation orientation, HashTable<ComponentDataType, double>? data = null, DBConnector? leftConnector = null, DBConnector? rightConnector = null)
            {
                PrimaryKey = primaryKey;
                ComponentType = componentType;
                Position = new Point(PositionX, PositionY);
                Orientation = orientation;
                Data = data ?? new HashTable<ComponentDataType, double>();
                LeftConnector = leftConnector ?? new DBConnector();
                RightConnector = rightConnector ?? new DBConnector();
            }
        }
        private struct DBSave : IDBDataType
        {
            public long PrimaryKey { get; set; }
            public string Name { get; set; }
            public string Author { get; set; }
            public DateTime LastModified { get; set; }
            public long EnvironmentID { get; set; }
            public DBSave(long primaryKey, string name, string author, DateTime lastModified, long environmentID)
            {
                PrimaryKey = primaryKey;
                Name = name;
                Author = author;
                LastModified = lastModified;
                EnvironmentID = environmentID;
            }
        }
        private struct DBEnvironment : IDBDataType
        {
            public long PrimaryKey { get; set; }
            public double Light { get; set; }
            public double Temperature { get; set; }
            public DBEnvironment(long primaryKey, double light, double temperature)
            {
                PrimaryKey = primaryKey;
                Light = light;
                Temperature = temperature;
            }
        }
        private interface IDBDataType
        {
            public long PrimaryKey { get; set; }
        }
    }
}

