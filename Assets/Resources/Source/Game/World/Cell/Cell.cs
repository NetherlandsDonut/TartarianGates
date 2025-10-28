using System;
using System.Collections.Generic;
using System.Linq;

using static Core;

public class Cell
{
    public Cell() { }

    public Cell(RoomCell roomCell)
    {
        if (roomCell == null) return;
        var layout = roomCell.layoutUsed;
        if (roomCell.prop != null && roomCell.prop.StartsWith("Door #"))
            wall = new Wall(layout.convertion[roomCell.prop]);
        else if (roomCell.terrain != null && roomCell.terrain.StartsWith("Wall #"))
            wall = new Wall(layout.convertion[roomCell.terrain]);
        else if (roomCell.terrain != null && roomCell.terrain.StartsWith("Floor #"))
            ground = new Ground(layout.convertion[roomCell.terrain]);
    }

    //The ground that is in this cell
    public Ground ground;

    //The wall that is in this cell
    public Wall wall;

    //Cell this entity can see where it is
    [NonSerialized] public List<Entity> seenBy;

    public bool WillFit(Entity entity) => entities.Sum(x => x.stats["Size"]) + entity.stats["Size"] <= 6;
    public bool IsWalkable() => wall == null && ground != null && ground.liquid == null;
    public bool IsSwimmable() => wall == null && ground != null && ground.liquid != null;
    public bool CanSeeThrough() => wall == null || wall.seeThrough;

    //Get all cells you can reach from this one
    public List<Cell> GetAdjacentLocations(bool cardinals, bool diagonals)
    {
        var list = new List<Cell>();
        Cell cell;
        if (cardinals)
        {
            cell = NeighboringCell(0, 1); if (cell != null) list.Add(cell);
            cell = NeighboringCell(1, 0); if (cell != null) list.Add(cell);
            cell = NeighboringCell(-1, 0); if (cell != null) list.Add(cell);
            cell = NeighboringCell(0, -1); if (cell != null) list.Add(cell);
        }
        if (diagonals)
        {
            cell = NeighboringCell(1, 1); if (cell != null) list.Add(cell);
            cell = NeighboringCell(1, -1); if (cell != null) list.Add(cell);
            cell = NeighboringCell(-1, 1); if (cell != null) list.Add(cell);
            cell = NeighboringCell(-1, -1); if (cell != null) list.Add(cell);
        }
        return list;
    }

    //Coordinates of this cell in the world
    [NonSerialized] public int x, y;

    //List of all entities on this tile
    [NonSerialized] public List<Entity> entities;

    #region Visual Representation

    //Get print
    public PreparedPrint GetPrint()
    {
        var temp = new PreparedPrint();
        //if (!seenBy.Any(x => x.team == "Player")) temp = new(" ");
        if (wall != null) temp = new(wall.symbol, wall.foreColor, wall.fillColor, false);
        else if (ground != null && ground.mined && ground.liquid != null) { var liquidFind = GroundType.groundTypes.Find(x => x.liquid == ground.liquid); temp = new(liquidFind.symbol, liquidFind.foreColor, liquidFind.fillColor, false); }
        else if (ground != null && ground.mined) temp = new(ground.symbolFilling ?? "·", ground.foreColorFilling ?? ground.foreColor, ground.foreColorFilling ?? ground.fillColor, false);
        else if (ground != null) temp = new(ground.symbol, ground.foreColor, ground.fillColor, false);
        else temp = new(" ");
        return temp;
    }

    #endregion

    #region Support

    //Gets a neighboring cell to this one by XY
    public Cell NeighboringCell(int X, int Y) => Save.save.map.cells.XY(x + X, y + Y);

    #endregion
}