using System.Collections.Generic;

public class Race
{
    //Name of the race
    public string name;

    //Is this race available for starting a new game 
    public bool starting;

    //Racial stats for each entity of this race
    public Dictionary<string, int> stats;

    //Natural resistances of this race
    public Dictionary<string, int> naturalResistances;

    //Racial attitude towards other races
    public Dictionary<string, int> attitude;

    //List of all races
    public static List<Race> races;

    //List of all races that can be selected as starting races
    public static List<Race> startingRaces;
}
