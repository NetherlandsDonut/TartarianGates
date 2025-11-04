using System;

public class Wall
{
    public Wall() { }
    public Wall(int id)
    {
        this.id = id;
        LoadData(WallType.wallTypes.Find(x => x.id == id));
    }

    //Loads data from the prop type
    public void LoadData(WallType find = null)
    {
        if (name != null) return;
        find ??= WallType.wallTypes.Find(x => x.id == id);
        if (health == 0) health = find.health;
        name = find.name;
        seeThrough = find.seeThrough;
        isTree = find.isTree;
        isDoor = find.isDoor;
        symbol = find.symbol;
        foreColor = find.foreColor;
        fillColor = find.fillColor;
    }

    //Gets the visual of the wall
    public PreparedPrint GetPrint() => new(symbol, foreColor, fillColor, false);

    //ID of the prop this refers to
    public int id;

    //Amount of health this prop type has
    public int health;

    //Was this door opened
    public bool opened;

    //Is this wall see through
    [NonSerialized] public bool seeThrough;

    //Is this wall a tree
    [NonSerialized] public bool isTree;

    //Is this wall a door
    [NonSerialized] public bool isDoor;

    //Name of the prop during inspection
    [NonSerialized] public string name;

    //Visual of the prop when it's around
    [NonSerialized] public string symbol, foreColor, fillColor;
}
