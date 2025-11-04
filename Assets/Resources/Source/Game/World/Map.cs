using System;
using System.Linq;
using System.Collections.Generic;

using static Core;

public class Map
{
    public Map() { }

    //Coordinates of the camera view on the map
    public int mapViewX, mapViewY;

    //Map seed for everything in it
    public int seed;

    //Cells of the map
    public Cell[,] cells;

    //Cells of the map before initialization
    [NonSerialized] public Dictionary<(int, int), RoomCell> preCells;

    //Tells the id for a new entity
    public int newEntityID;

    //List of all dynamic entities on the map
    public List<Entity> entities;

    public static Map GenerateMap(List<Plan> plans)
    {
        int counter, currentPlan;
        List<(int, int)> freeConnections = new();
        List<Dictionary<string, int>> overallDistribution;
        Map map;
        do
        {
            counter = currentPlan = 0;
            overallDistribution = plans.Select(x => new Dictionary<string, int>()).ToList();
            map = new Map { preCells = new() };
            for (; currentPlan < plans.Count; currentPlan++)
                if (!GeneratePlan(plans[currentPlan])) break;
        }
        while (plans[^1].distribution.Any(x => x.Value > (overallDistribution[^1].ContainsKey(x.Key) ? overallDistribution[^1][x.Key] : 0)));
        foreach (var connection in freeConnections) ConvertConnection(connection);
        FixFurniture(map);
        RemoveInaccessibleWalls(map);
        ConvertCells(map);
        map.PrepareMap();
        return map;

        //Converts the pre cells into real cells on the map
        void ConvertCells(Map map)
        {
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (var preCell in map.preCells.ToList())
            {
                if (minX > preCell.Key.Item1) minX = preCell.Key.Item1;
                if (maxX < preCell.Key.Item1) maxX = preCell.Key.Item1;
                if (minY > preCell.Key.Item2) minY = preCell.Key.Item2;
                if (maxY < preCell.Key.Item2) maxY = preCell.Key.Item2;
            }
            map.cells = new Cell[maxX - minX + 3, maxY - minY + 3];
            for (int i = 0; i < map.cells.GetLength(0); i++)
                for (int j = 0; j < map.cells.GetLength(1); j++)
                    if (map.preCells.ContainsKey((i + minX - 1, j + minY - 1)))
                        map.cells[i, j] = new Cell(map.preCells[(i + minX - 1, j + minY - 1)]);
                    else map.cells[i, j] = new Cell(null);
        }

        //Checks if a connection can be made into a solid wall
        //This happens only if the connection wasn't used and leads to void
        void ConvertConnection((int, int) connection)
        {
            //Get connection coords
            var i = connection.Item1;
            var j = connection.Item2;

            //If the connection points to null, quit immediately
            //if (map.preCells.ContainsKey((i, j))) return;

            //Get neighboring cells of that connection cell
            var neighbors = new RoomCell[4]
            {
                map.preCells.ContainsKey((i, j - 1)) ? map.preCells[(i, j - 1)] : null,
                map.preCells.ContainsKey((i, j + 1)) ? map.preCells[(i, j + 1)] : null,
                map.preCells.ContainsKey((i + 1, j)) ? map.preCells[(i + 1, j)] : null,
                map.preCells.ContainsKey((i - 1, j)) ? map.preCells[(i - 1, j)] : null
            };

            //If any neighbor of this connection tile is a null..
            for (int k = 0; k < neighbors.Length; k++)
                if (neighbors[k] == null)
                {
                    //Change this floor tile into a wall
                    var cell = map.preCells[(i, j)];
                    cell.terrain = cell.terrain.Replace("Floor", "Wall");
                    cell.prop = string.Empty;
                    cell.meta = string.Empty;
                    break;
                }
        }

        //Remove doors that don't neighbor with exactly two empty cells
        void FixFurniture(Map map)
        {
            //Sadly this loop has to be run twice as some doors are double doors
            for (int z = 0; z < 2; z++)
                foreach (var preCell in map.preCells.ToList())
                {
                    var cell = preCell.Value;
                    if (cell != null && cell.prop != null && cell.prop.StartsWith("Door #"))
                    {   
                        var i = preCell.Key.Item1;
                        var j = preCell.Key.Item2;

                        //Get neighboring cells of that connection cell
                        var neighbors = new bool[4]
                        {
                            map.preCells.ContainsKey((i, j - 1)) && map.preCells[(i, j - 1)].prop == null && map.preCells[(i, j - 1)].terrain.StartsWith("Floor"),
                            map.preCells.ContainsKey((i, j + 1)) && map.preCells[(i, j + 1)].prop == null && map.preCells[(i, j + 1)].terrain.StartsWith("Floor"),
                            map.preCells.ContainsKey((i + 1, j)) && map.preCells[(i + 1, j)].prop == null && map.preCells[(i + 1, j)].terrain.StartsWith("Floor"),
                            map.preCells.ContainsKey((i - 1, j)) && map.preCells[(i - 1, j)].prop == null && map.preCells[(i - 1, j)].terrain.StartsWith("Floor")
                        };

                        var paths = 0;
                        var emptyNeighbors = 0;
                        if (neighbors[0] && neighbors[1]) paths++;
                        else if (neighbors[0] || neighbors[1]) emptyNeighbors++;
                        if (neighbors[3] && neighbors[2]) paths++;
                        else if (neighbors[3] || neighbors[2]) emptyNeighbors++;
                        if (paths != 1 || emptyNeighbors > 0) cell.prop = null;
                    }
                }
        }

        void RemoveInaccessibleWalls(Map map)
        {
            foreach (var preCell in map.preCells.ToList())
                if (preCell.Value.terrain.StartsWith("Wall"))
                {
                    var i = preCell.Key.Item1;
                    var j = preCell.Key.Item2;

                    //Get neighboring cells of that connection cell
                    if (map.preCells.ContainsKey((i, j - 1)) && map.preCells[(i, j - 1)].terrain.StartsWith("Floor")) continue;
                    if (map.preCells.ContainsKey((i, j + 1)) && map.preCells[(i, j + 1)].terrain.StartsWith("Floor")) continue;
                    if (map.preCells.ContainsKey((i + 1, j)) && map.preCells[(i + 1, j)].terrain.StartsWith("Floor")) continue;
                    if (map.preCells.ContainsKey((i - 1, j)) && map.preCells[(i - 1, j)].terrain.StartsWith("Floor")) continue;
                    map.preCells.Remove((i, j));
                }
        }

        bool GeneratePlan(Plan plan)
        {
            var layout = Layout.layouts.Find(x => x.name == plan.layout);
            var rooms = layout.rooms.GroupBy(x => x.type).ToDictionary(x => x.Key, x => x.ToList());
            while (plans[currentPlan].distribution.Any(x => x.Value > (overallDistribution[currentPlan].TryGetValue(x.Key, out int value) ? value : 0)))
            {
                //If failure counter reached it's limit, cancel the operation
                if (counter == 100000) return false;

                //Generate what kind of room to generate now
                var roomType = "";
                if ((overallDistribution[currentPlan].TryGetValue("Room", out int r1) ? r1 : 0) < (plan.distribution.TryGetValue("Room", out int a1) ? a1 : 0))
                    if ((overallDistribution[currentPlan].TryGetValue("Hall", out int r2) ? r2 : 0) < (plan.distribution.TryGetValue("Hall", out int a2) ? a2 : 0))
                        roomType = random.Next(2) == 0 ? "Room" : "Hall";
                    else roomType = "Room";
                else if ((overallDistribution[currentPlan].TryGetValue("Hall", out int r2) ? r2 : 0) < (plan.distribution.TryGetValue("Hall", out int a2) ? a2 : 0)) roomType = "Hall";
                else if ((overallDistribution[currentPlan].TryGetValue("Treasury", out int r3) ? r3 : 0) < (plan.distribution.TryGetValue("Treasury", out int a3) ? a3 : 0)) roomType = "Treasury";
                else if ((overallDistribution[currentPlan].TryGetValue("Elite", out int r4) ? r4 : 0) < (plan.distribution.TryGetValue("Elite", out int a4) ? a4 : 0)) roomType = "Elite";

                //Roll a room from the pool
                var newRoom = rooms[roomType][random.Next(rooms[roomType].Count)];

                //If this isn't the first plan and no connections are available, close it
                if (currentPlan > 0 && freeConnections.Count == 0) return false;

                //If no rooms have been generated yet from this plan
                else if (overallDistribution[currentPlan].Sum(x => x.Value) == 0)
                {
                    var connectionsToBeFlushed = !plan.carryOverConnections ? freeConnections.ToList() : new();
                    var randomConnection = freeConnections.Count == 0 ? (1500, 1500) : freeConnections[random.Next(freeConnections.Count)];
                    var success = AddRoom(map, layout, newRoom, random.Next(4), random.Next(4), randomConnection.Item1, randomConnection.Item2);
                    if (!success) counter++;
                    else
                    {
                        if (!overallDistribution[currentPlan].TryAdd(roomType, 1))
                            overallDistribution[currentPlan][roomType]++;
                        if (!plan.carryOverConnections)
                        {
                            connectionsToBeFlushed.ForEach(x => ConvertConnection(x));
                            connectionsToBeFlushed.ForEach(x => freeConnections.Remove(x));
                        }
                    }
                }

                //If this is the first plan and no connections are available, close it
                else if (currentPlan == 0 && freeConnections.Count == 0) return false;

                //Otherwise generate normally
                else
                {
                    var randomConnection = freeConnections[random.Next(freeConnections.Count)];
                    var success = AddRoom(map, layout, newRoom, random.Next(4), random.Next(4), randomConnection.Item1, randomConnection.Item2);
                    if (!success) counter++;
                    else if (!overallDistribution[currentPlan].TryAdd(roomType, 1))
                        overallDistribution[currentPlan][roomType]++;
                }
            }
            return true;
        }

        bool AddRoom(Map map, Layout layout, Room room, int rR, int rF, int x, int y)
        {
            //Rotate the room by the specified amount (r * 90 degrees)
            var roomCells = room.cells.Rotate(rR).Flip(rF % 2 == 1, rF % 4 % 3 > 0);

            //Get all connections from the new room to be placed
            var connections = new List<(int, int)>();
            for (int i = 0; i < roomCells.GetLength(0); i++)
                for (int j = 0; j < roomCells.GetLength(1); j++)
                    if (roomCells[i, j].meta == "Connection")
                        connections.Add((i, j));

            //Find a random connection from the new room
            var randomConnection = connections[random.Next(connections.Count)];

            //Offset the placement spot for the new room
            //by the offset of the connection in the room
            x -= randomConnection.Item1;
            y -= randomConnection.Item2;

            //Check if the room can be placed here
            //If not, abandon the whole operation
            for (int i = 0; i < roomCells.GetLength(0); i++)
                for (int j = 0; j < roomCells.GetLength(1); j++)
                {
                    var newCell = roomCells[i, j];
                    map.preCells.TryGetValue((x + i, y + j), out var oldCell);
                    if (newCell.terrain != null && oldCell != null)
                    {
                        //Can't place a room if a wall lands on something else than a wall
                        if (newCell.terrain.StartsWith("Wall") && !oldCell.terrain.StartsWith("Wall")) return false;

                        //Can't place a room if a floor lands on floor without both being connections
                        if (newCell.terrain.StartsWith("Floor") && (!oldCell.terrain.StartsWith("Floor") || newCell.meta != "Connection" || oldCell.meta != "Connection")) return false;
                    }
                }

            //If all the checks were successfully passed, place the room on the map
            for (int i = 0; i < roomCells.GetLength(0); i++)
                for (int j = 0; j < roomCells.GetLength(1); j++)
                {
                    var newCell = roomCells[i, j];
                    if (newCell.terrain != null)
                    {
                        if (newCell.meta == "Connection")
                            if (freeConnections.Contains((x + i, y + j)))
                                freeConnections.Remove((x + i, y + j));
                            else freeConnections.Add((x + i, y + j));
                        var newObj = new RoomCell()
                        {
                            terrain = newCell.terrain,
                            prop = newCell.prop,
                            meta = newCell.meta,
                            layoutUsed = layout
                        };
                        if (!map.preCells.TryAdd((x + i, y + j), newObj))
                            map.preCells[(x + i, y + j)] = newObj;
                    }
                }
            return true;
        }
    }

    //Prints the map on the chosen coords with specific size
    public void Print(int x, int y, int sizeX, int sizeY)
    {
        for (int i = -sizeX / 2; i <= sizeX / 2; i++)
            for (int j = -sizeY / 2; j <= sizeY / 2; j++)
            {
                //Get the cell
                var cell = cells.XY(i + mapViewX, j + mapViewY);

                //Get print of the cell
                var print = cell == null ? new() : cell.GetPrint();

                //Draw the entities of this cell
                if (cell != null && cell.entities.Count > 0)
                    foreach (var entity in cell.entities)
                        WriteCell(entity.GetPrint());

                //Draw the cell
                else WriteCell(print);

                //Writes the single tile cell of the map on the screen
                void WriteCell(PreparedPrint print)
                {
                    if (cell != null)
                        bridge.WriteDialog(x + i + sizeX / 2, y + j + sizeY / 2, print.symbol[(cell.x + cell.y / 2) % print.symbol.Length] + "", () =>
                        {
                            mapViewX = cell.x;
                            mapViewY = cell.y;
                            return true;
                        },
                        () => print.foreColor, () => print.backColor);
                    else if ((i + mapViewX == cells.GetLength(0) || i + mapViewX == -1) && j + mapViewY >= -1 && j + mapViewY <= cells.GetLength(1) || (j + mapViewY == cells.GetLength(1) || j + mapViewY == -1) && i + mapViewX >= -1 && i + mapViewX <= cells.GetLength(0)) bridge.Write(x + i + sizeX / 2, y + j + sizeY / 2, "X");
                    else bridge.Write(x + i + sizeX / 2, y + j + sizeY / 2, "a");
                }
            }
    }

    //Spawns into the world an entity at position XY
    public Entity SpawnNewEntity(int x, int y, Entity newEntity)
    {
        newEntity.x = x;
        newEntity.y = y;
        entities.Add(newEntity);
        newEntity.AsignCell(this);
        newEntity.Prepare();
        newEntity.CalculateLOS();
        return newEntity;
    }

    //Loads coordinates into cells
    public void SpawnPlayerParty()
    {
        foreach (var entity in Save.save.playerParty)
            foreach (var cell in cells)
                if (cell.ground != null)
                {
                    SpawnNewEntity(cell.x, cell.y, entity);
                    mapViewX = cell.x;
                    mapViewY = cell.y;
                    break;
                }
    }

    //Prepares the map for the game
    public void PrepareMap()
    {
        entities = new();
        for (var i = 0; i < cells.GetLength(0); i++)
            for (var j = 0; j < cells.GetLength(1); j++)
            {
                var cell = cells[i, j];
                cell.x = i;
                cell.y = j;
                cell.entities = new();
                cell.seenBy = new();
            }
    }

    //Updates whether the cells can be seen from the active entities
    public void UpdateCellVisibility()
    {
        foreach (var entity in entities)
            entity.CalculateLOS();
    }

    //Moves camera view by amounts
    public void MoveView(int X, int Y, int Z)
    {
        mapViewX += X;
        mapViewY += Y;
    }

    //Loads coordinates into cells
    public void PrepareEntities()
    {
        foreach (var entity in entities)
            entity.Prepare();
    }

    //Loads entities into cells
    public void AsignEntities()
    {
        entities.ForEach(x => x.AsignCell(this));
    }

    //Loads entities into cells
    public void AsignPropData()
    {
        foreach (var cell in cells)
        {
            cell.wall?.LoadData();
            cell.ground?.LoadData();
        }
    }

    //Unloads unnessecary data from the world
    public void FlushData()
    {
        foreach (var cell in cells)
        {
            cell.wall?.FlushData();
            cell.ground?.FlushData();
        }
    }
}

public class Plan
{
    //What layout is used for this plan
    public string layout;

    //Distribution of rooms for this plan
    public Dictionary<string, int> distribution;

    //Are connections carried over from the previous plan?
    public bool carryOverConnections;

    public int waysIn;

    public int waysDeeper;
}

public class Layout
{
    //Name of this layout
    public string name;

    //This is a list of ID's for spawning objects in maps with this layout
    public Dictionary<string, int> convertion;

    //List of possible rooms in this layout
    public List<Room> rooms;

    //List of possible layouts and their rooms
    public static List<Layout> layouts;
}

public class Room
{
    //Room type
    public string type;

    //Cells of this room
    public RoomCell[,] cells;
}

public class RoomCell
{
    //What kind of terrain is in the cell
    public string terrain;

    //What kind of prop is in the cell
    public string prop;

    //What kind of meaning is behind this cell
    public string meta;

    //What kind of layout is used here during map generation
    [NonSerialized] public Layout layoutUsed;
}
