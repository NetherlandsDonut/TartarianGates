using System.Collections.Generic;

public class GroundType
{
    //ID of this ground type
    public int id;

    //Name of the ground during inspection
    public string name;

    //Is this ground see through
    public bool seeThrough;

    //Whether this ground is treated like a body of liquid that spills
    public string liquid;

    //Amount of health this ground type has
    public int health;

    //Visual of the ground when it's around
    public string symbol, foreColor, fillColor;

    //Provides the ID for the ground that coresponds to the provided name
    public static int GroundID(string name) => groundTypes.Find(x => x.name == name).id;

    //List of ground types
    public static List<GroundType> groundTypes;
}
