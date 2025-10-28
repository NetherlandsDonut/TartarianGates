using System.Collections.Generic;

public class Background
{
    //Race for which this background is available
    public string race;

    //Name of the background
    public string name;

    //Is this background available for starting a new game 
    public bool starting;

    //Description of the background
    public List<string> description;

    //Starting items provided for this background
    public Dictionary<string, int> startingItems;

    //Body skills of this background
    public Dictionary<string, int> bodySkills;

    //Mind skills of this background
    public Dictionary<string, int> mindSkills;

    //Soul skills of this background
    public Dictionary<string, int> soulSkills;

    //List of all possible backgrounds
    public static List<Background> backgrounds;
}
