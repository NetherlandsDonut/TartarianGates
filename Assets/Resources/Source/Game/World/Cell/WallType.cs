using System.Collections.Generic;

public class WallType
{
    //ID of this wall type
    public int id;

    //Name of the wall during inspection
    public string name;

    //Is this wall see through
    public bool seeThrough;

    //Is this wall a tree
    public bool isTree;

    //Is this wall a door
    public bool isDoor;

    //Amount of health this wall has remaining
    public int health;

    //Visual of the wall when it's around
    public string symbol, foreColor, fillColor;

    //Dropped items on destroy
    public List<Drop> yields;

    //Provides the ID for the wall that coresponds to the provided name
    public static int WallID(string name) => wallTypes.Find(x => x.name == name).id;

    //List of wall types
    public static List<WallType> wallTypes;
}
