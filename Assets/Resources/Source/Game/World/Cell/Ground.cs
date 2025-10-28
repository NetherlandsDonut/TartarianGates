using System;

public class Ground
{
    public Ground() { }
    public Ground(int id)
    {
        this.id = id;
        mined = false;
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
        if (idFilling != 0)
        {
            find = GroundType.groundTypes.Find(x => x.id == idFilling);
            if (health == 0) health = find.health;
            nameFilling = find.name;
            symbolFilling = find.symbol;
            foreColorFilling = find.foreColor;
            fillColorFilling = find.fillColor;
        }
        else if (health == 0) health = find.health;
    }

    //Unloads unnessecary data from the cell
    public void FlushData()
    {
        var find = GroundType.groundTypes.Find(x => x.id == id);
        if (liquid == find.liquid) liquid = null;
        if (idFilling != 0)
        {
            find = GroundType.groundTypes.Find(x => x.id == idFilling);
            if (health == find.health) health = 0;
        }
        else if (health == find.health) health = 0;
    }

    //ID of the ground this refers to
    public int id, idFilling;

    //Amount of health this prop type has
    public int health;

    //Was this ground mined out to leave a hole
    public bool mined;

    //Liquid that is flowing in this ground
    public string liquid;

    //Is this wall see through
    [NonSerialized] public bool seeThrough;

    //Name of the prop during inspection
    [NonSerialized] public string name, nameFilling;

    //Visual of the prop when it's around
    [NonSerialized] public string symbol, symbolFilling, foreColor, foreColorFilling, fillColor, fillColorFilling;
}
