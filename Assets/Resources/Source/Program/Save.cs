using System;
using System.Collections.Generic;

using static Core;

public class Save
{
    public static Save NewSave()
    {
        var newSave = new Save { };
        return newSave;
    }

    //Date and time of creating the save file
    public DateTime date;

    //Current map loaded on the save
    public Map map;

    //Party of the player
    public List<Entity> playerParty;

    //Finalizes creation of the new save file
    public void FinalizeCreation()
    {
        playerParty = new() { new Entity(bridge.creationName, bridge.creationGender, bridge.creationRace, bridge.creationBackground) };
        bridge.creationName = "";
        bridge.creationGender = "";
        bridge.creationRace = "";
        bridge.creationBackground = "";
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
