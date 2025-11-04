using System;
using System.Collections.Generic;

using static Core;

public class Save
{
    public static Save NewSave()
    {
        var newSave = new Save() { date = DateTime.Now };
        return newSave;
    }

    //Date and time of creating the save file
    public DateTime date;

    //Current map loaded on the save
    public Map map;

    //Party of the player
    public List<Entity> playerParty;

    //Finalizes creation of the new save file and the player character
    public Entity FinalizeStartingCharacter()
    {
        playerParty = new();
        var playerChar = new Entity(bridge.creationName, bridge.creationGender, bridge.creationRace, bridge.creationBackground);
        bridge.creationName = "";
        bridge.creationGender = "";
        bridge.creationRace = "";
        bridge.creationBackground = "";
        return playerChar;
    }

    //Adds a new entity to the player party
    public void AddEntityToPlayerParty(Entity entity)
    {
        entity.team = "Player";
        playerParty.Add(entity);
    }

    //Name of this save
    public string SaveName()
    {
        return date.ToString("yyyy-MM-dd HH-mm-ss");
    }

    #region Management

    //Loads the game as the current one
    public void Load()
    {
        //Set this as the new current save
        save = this;

        //Prepares the map if it's active
        if (map != null)
        {
            map.PrepareMap();
            map.AsignMapData();
            map.PrepareEntities();
        }
    }

    //Closes the game
    //This step is nessecary to save the game
    //As this process flushes data that shouldnt be in the file
    public void Close()
    {
        save = null;
    }

    #endregion

    //Currently active game
    public static Save save;
}
