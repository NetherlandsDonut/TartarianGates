using System.Collections.Generic;

public class Settings
{
    //Resets the settings to the default values
    public void ResetToDefaults()
    {
        ambience = true;
        soundEffects = true;
        runInBackground = true;
        pixelPerfectVision = true;

        showGrid = false;

        showTurn = true;
        showDepth = true;
        showFullMoon = true;
    }

    #region General

    //Whether the music is turned on
    public bool ambience;

    //Whether the sound effects are turned on
    public bool soundEffects;

    //Whether the application runs while not focused
    public bool runInBackground;

    //Whether the graphics are rendered to pixel perfect
    public bool pixelPerfectVision;

    //Whether the grid should be shown on the map
    public bool showGrid;

    //Whether the line of sight cuts corners or not
    public bool includeCorners;

    #endregion

    #region Game Screen

    //Whether turn should be shown in game
    public bool showTurn;

    //Whether layer info should be shown in game
    public bool showDepth;

    //Whether it should be shown in game that it's full moon
    public bool showFullMoon;

    #endregion

    //List of settings for the program
    public static Settings settings;
}
