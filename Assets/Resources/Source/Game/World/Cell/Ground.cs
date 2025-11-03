using System;

public class Ground
{
    public Ground() { }
    public Ground(int id)
    {
        this.id = id;
        LoadData();
    }

    //Loads data from the prop type
    public void LoadData(GroundType find = null)
    {
        find ??= GroundType.groundTypes.Find(x => x.id == id);
        name = find.name;
        seeThrough = find.seeThrough;
        if (find.liquid != null) liquid = find.liquid;
        symbol = find.symbol;
        foreColor = find.foreColor;
        fillColor = find.fillColor;
        if (health == 0) health = find.health;
    }

    //Unloads unnessecary data from the cell
    public void FlushData()
    {
        var find = GroundType.groundTypes.Find(x => x.id == id);
        if (liquid == find.liquid) liquid = null;
        if (health == find.health) health = 0;
    }

    //Gets the visual of the ground
    public PreparedPrint GetPrint() => new(symbol, foreColor, fillColor, false);

    //ID of the ground this refers to
    public int id;

    //Amount of health this prop type has
    public int health;

    //Liquid that is flowing in this ground
    public string liquid;

    //Is this wall see through
    [NonSerialized] public bool seeThrough;

    //Name of the prop during inspection
    [NonSerialized] public string name;

    //Visual of the prop when it's around
    [NonSerialized] public string symbol, foreColor, fillColor;
}
