using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using static Core;
using static Screen;
using static Sound;

public class Bridge : MonoBehaviour
{
    #region Program Variables

    //Current function being keybinded
    public string functionBeingKeybinded;

    //New keybind to be asigned
    public Keybind newKeybind;

    //Timer between keys
    public float keyTimer;

    //Animation timer for animating more
    //than one character written in one tile
    public float animationTimer;

    //Lists of save names and map names
    public List<string> saveNames, mapNames;

    //Save file to load
    public string gameToLoad;

    //Current loaded map's name
    public string mapName;

    //Stores old map name before renaming,
    //it is used to delete the old file
    public string oldMapName;

    //Screen to which the next screen should point
    public string screenAfterwards;

    //Coordinates of the camera view on the map
    public int mapViewX, mapViewY;

    //Dialog color modifier
    public string dialogColorMod;

    //Temporary input for typing and
    //avoiding making chanes without confirmation
    public string temporaryInput;

    //Context menu to be drawn on the screen
    public Action contextMenu;

    #endregion

    #region Character Creation Variables

    //Character name during creation
    public string creationName;

    //Character gender during creation
    public string creationGender;

    //Character race during creation
    public string creationRace;

    //Character background during creation
    public string creationBackground;

    #endregion

    void Awake()
    {
        bridge = this;
        random = new();
        dialogColorMod = "DarkElves";

        //Get audio
        var sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sounds = Resources.LoadAll<AudioClip>("Sounds/").ToDictionary(x => x.name, x => x);
        ambience = sources.First(x => x.name == "Ambience");
        soundEffects = sources.First(x => x.name == "SFX");

        //Load user settings
        Serialization.Deserialize(ref Settings.settings, "settings");
        if (Settings.settings == null)
        {
            Settings.settings ??= new();
            Settings.settings.ResetToDefaults();
        }
        Application.runInBackground = Settings.settings.runInBackground;

        //Load the font
        LoadFont();

        //Load game data
        Serialization.Deserialize(ref Race.races,               "races");
        Serialization.Deserialize(ref WallType.wallTypes,       "walls");
        Serialization.Deserialize(ref GroundType.groundTypes,   "ground");
        Serialization.Deserialize(ref Background.backgrounds,   "backgrounds");
        Serialization.Deserialize(ref Size.sizes,               "entity sizes");
        Serialization.Deserialize(ref Layout.layouts,           "layouts");
        Serialization.Deserialize(ref Name.names,               "names");

        //Setup starting races
        Race.startingRaces = Race.races.Where(x => x.starting).ToList();

        //Setup keybinds
        Serialization.Deserialize(ref Keybinds.keybinds,        "keybinds");
        Keybinds.keybinds ??= new();
        Keybinds.RemoveObseleteKeybinds();
        Keybinds.AddMissingKeybinds();

        GenerateScreen();
        SetScreen("MainMenu");
        Redraw();
    }

    void GenerateScreen()
    {
        if (tiles != null)
            foreach (var tile in tiles)
                Destroy(tile.gameObject);
        tiles = new Tile[screenX, screenY];
        for (int i = 0; i < screenX; i++)
            for (int j = 0; j < screenY; j++)
                tiles[i, j] = GenerateTile(i, j);
    }

    void ClearScreen()
    {
        dialogs = new();
        if (tiles != null)
            foreach (var tile in tiles)
                if (tile.foreground.sprite != null)
                    tile.Clear();
    }

    void ClearDialogs()
    {
        dialogs = new();
        if (tiles != null)
            foreach (var tile in tiles)
                if (tile.dialogAsigned != null)
                    tile.dialogAsigned = null;
    }

    void Clear(int x, int y, int length)
    {
        for (int i = 0; i < length; i++)
            tiles[x + i, y].Clear();
    }

    void Update()
    {
        var didSomething = false;

        #region Frame Resets

        //Reset all record of sounds played in the last frame
        soundsPlayedThisFrame = new();

        //Move animation timer forwards
        animationTimer += Time.deltaTime;
        if (animationTimer > 99) animationTimer %= 100;

        //Account for time between key presses
        if (keyTimer > 0) keyTimer -= Time.deltaTime;

        #endregion

        #region Music Control

        //If music is turned off, silence it
        if (!Settings.settings.ambience)
        {
            if (ambience.volume > 0)
                ambience.volume -= 0.1f;
            if (ambience.volume < 0)
                ambience.volume = 0;
        }

        //If the music playing is a wrong song, mute the current one and then switch tracks
        else if (queuedAmbience.Item1 != ambience.clip)
        {
            if (!queuedAmbience.Item3 && ambience.volume > 0) ambience.volume -= 0.1f;
            else
            {
                ambience.clip = queuedAmbience.Item1;
                ambience.Play();
            }
        }

        //If the right music is playing but volume is not the one aimed at, turn it up
        else if (queuedAmbience.Item1 == ambience.clip && ambience.volume < queuedAmbience.Item2)
        {
            if (queuedAmbience.Item3 && ambience.clip != queuedAmbience.Item1) ambience.volume = queuedAmbience.Item2;
            else ambience.volume += 0.1f;
        }

        #endregion

        #region Input

        if (currentScreen.input(false)) didSomething = true;

        #endregion

        if (didSomething) Redraw();
    }
    
    public bool GetDoubleBind(string function1, string function2)
    {
        if (!Keybinds.keybinds.ContainsKey(function1)) return false;
        if (!Keybinds.keybinds.ContainsKey(function2)) return false;
        var key1 = Keybinds.keybinds[function1].key;
        var key2 = Keybinds.keybinds[function2].key;
        if (keyTimer > 0 || !Input.GetKey(key1) || !Input.GetKey(key2)) return false;
        if (!lastKey.Contains("&"))
        {
            if (lastKey != key1.ToString() && lastKey != key2.ToString()) return false;
            else lastKey = key1.ToString() + "&" + key2.ToString();
        }
        else if (lastKey != key1.ToString() + "&" + key2.ToString()) return false;
        keyTimer = keyTimerSameKey;
        return true;
    }

    public bool GetDoubleBindDown(string function1, string function2)
    {
        if (!Keybinds.keybinds.ContainsKey(function1)) return false;
        if (!Keybinds.keybinds.ContainsKey(function2)) return false;
        var key1 = Keybinds.keybinds[function1].key;
        var key2 = Keybinds.keybinds[function2].key;
        if (!Input.GetKeyDown(key1)) return false;
        if (!Input.GetKeyDown(key2)) return false;
        lastKey = key1.ToString() + "&" + key2.ToString();
        keyTimer = keyTimerNewKey;
        return true;
    }

    public bool GetBind(string function)
    {
        if (!Keybinds.keybinds.ContainsKey(function)) return false;
        var key = Keybinds.keybinds[function].key;
        if (keyTimer > 0 || lastKey != key.ToString() || !Input.GetKey(key)) return false;
        keyTimer = keyTimerSameKey;
        return true;
    }

    public bool GetBindDown(string function)
    {
        if (!Keybinds.keybinds.ContainsKey(function)) return false;
        var key = Keybinds.keybinds[function].key;
        if (!Input.GetKeyDown(key)) return false;
        lastKey = key.ToString();
        keyTimer = keyTimerNewKey;
        return true;
    }

    public void SetScreen(string screen)
    {
        var temp = currentScreen;
        currentScreen = screens.Find(x => x.name == screen);
        currentScreen ??= temp;
    }

    public bool DialogInteraction(Dialog dialog)
    {
        var didSomething = false;
        didSomething = dialog.action != null && dialog.action();
        PlaySound(didSomething ? (dialog.text.Any(x => x == '☆' || x == '★') ? "DialogCheckbox" : "DialogSelect") : "DialogFailed");
        return didSomething;
    }

    public void Redraw()
    {
        ClearScreen();
        currentScreen.draw();
        if (contextMenu != null)
        {
            ClearDialogs();
            contextMenu.Invoke();
        }
    }

    public void WriteBoxTile(int X, int Y, char what, string fore, string fill = "?", bool blinking = false)
    {
        var write = what + "";
        if (X >= tiles.GetLength(0) || X < 0 || Y >= tiles.GetLength(1) || Y < 0) return;
        var whatsThere = tiles[X, Y].appearance == null ? ' ' : charset[tiles[X, Y].appearance.id];
        if (what == '╔')
        {
            if (whatsThere == '╗') write = "╦";
            else if (whatsThere == '╝') write = "╬";
            else if (whatsThere == '╚') write = "╠";
            else if (whatsThere == '╦') write = "╦";
            else if (whatsThere == '╠') write = "╠";
            else if (whatsThere == '╣') write = "╬";
            else if (whatsThere == '╩') write = "╬";
            else if (whatsThere == '╬') write = "╬";
            else if (whatsThere == '═') write = "╦";
            else if (whatsThere == '║') write = "╠";
        }
        else if (what == '╗')
        {
            if (whatsThere == '╔') write = "╦";
            else if (whatsThere == '╝') write = "╣";
            else if (whatsThere == '╚') write = "╬";
            else if (whatsThere == '╦') write = "╦";
            else if (whatsThere == '╠') write = "╬";
            else if (whatsThere == '╣') write = "╣";
            else if (whatsThere == '╩') write = "╬";
            else if (whatsThere == '╬') write = "╬";
            else if (whatsThere == '═') write = "╦";
            else if (whatsThere == '║') write = "╣";
        }
        else if (what == '╚')
        {
            if (whatsThere == '╔') write = "╠";
            else if (whatsThere == '╝') write = "╩";
            else if (whatsThere == '╗') write = "╬";
            else if (whatsThere == '╦') write = "╬";
            else if (whatsThere == '╠') write = "╠";
            else if (whatsThere == '╣') write = "╬";
            else if (whatsThere == '╩') write = "╩";
            else if (whatsThere == '╬') write = "╬";
            else if (whatsThere == '═') write = "╩";
            else if (whatsThere == '║') write = "╠";
        }
        else if (what == '╝')
        {
            if (whatsThere == '╔') write = "╬";
            else if (whatsThere == '╚') write = "╩";
            else if (whatsThere == '╗') write = "╣";
            else if (whatsThere == '╦') write = "╬";
            else if (whatsThere == '╠') write = "╬";
            else if (whatsThere == '╣') write = "╣";
            else if (whatsThere == '╩') write = "╩";
            else if (whatsThere == '╬') write = "╬";
            else if (whatsThere == '═') write = "╩";
            else if (whatsThere == '║') write = "╣";
        }
        else if (what == '═')
        {
            if (whatsThere == '╔') write = "╦";
            else if (whatsThere == '╚') write = "╩";
            else if (whatsThere == '╗') write = "╦";
            else if (whatsThere == '╝') write = "╩";
            else if (whatsThere == '╦') write = "╦";
            else if (whatsThere == '╠') write = "╬";
            else if (whatsThere == '╣') write = "╬";
            else if (whatsThere == '╩') write = "╩";
            else if (whatsThere == '╬') write = "╬";
            else if (whatsThere == '║') write = "╬";
        }
        else if (what == '║')
        {
            if (whatsThere == '╔') write = "╠";
            else if (whatsThere == '╚') write = "╠";
            else if (whatsThere == '╗') write = "╣";
            else if (whatsThere == '╝') write = "╣";
            else if (whatsThere == '╦') write = "╬";
            else if (whatsThere == '╠') write = "╠";
            else if (whatsThere == '╣') write = "╣";
            else if (whatsThere == '╩') write = "╬";
            else if (whatsThere == '╬') write = "╬";
            else if (whatsThere == '═') write = "╬";
        }
        Write(X, Y, write, fore, fill, blinking, false, true);
    }

    public void Write(int x, int y, string text, string fore = "?", string fill = "?", bool blinking = false, bool crossed = false, bool replaceLast = false)
    {
        for (int i = 0; i < text.Length && i + x < screenX; i++)
            if (x + i >= 0 && x + i < screenX && y >= 0 && y < screenY)
                if (text[i] != ' ' && text[i] != '∮' || fill != "Transparent")
                    tiles[x + i, y].Write(text[i], fore, fill, blinking, crossed, replaceLast);
    }

    public void WriteDialog(int x, int y, string text, Func<bool> action, Func<string> dialogColor = null, Func<string> backColor = null, Action enter = null, Action exit = null)
    {
        if (x < 0 || x >= screenX || y < 0 || y >= screenY) return;
        var dialog = new Dialog(text, x, y, action, dialogColor);
        dialog.AddHoverEvent(enter, exit);
        dialogs.Add(dialog);
        var isOver = false;
        for (int i = 0; i < text.Length; i++)
        {
            tiles[x + i, y].dialogAsigned = dialog;
            if (tiles[x + i, y].mouseOver) isOver = true;
        }
        if (isOver)
        {
            Write(x, y, text, dialogColor == null ? "DialogActive" : dialogColor(), backColor == null ? "?" : backColor(), true, dialogColor != null && dialogColor() == "DialogDisabled");
            currentDialog = dialog;
        }
        else Write(x, y, text, dialogColor == null ? "Dialog" : dialogColor().StartsWith("Dialog") ? "Dialog" : dialogColor(), backColor == null ? "?" : backColor(), false);
    }

    public Tile GenerateTile(int i, int j)
    {
        var newObj = new GameObject("Tile [" + i + ", " + j + "]", typeof(Tile), typeof(BoxCollider2D)).transform;
        var newTile = newObj.GetComponent<Tile>();
        var collider = newObj.GetComponent<BoxCollider2D>();
        collider.offset = new(fontWidth / 2, -fontHeight / 2);
        collider.size = new(fontWidth, fontHeight);
        newTile.x = i;
        newTile.y = j;
        newTile.foreground = new GameObject("Tile [" + i + ", " + j + "] Foreground", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        newTile.foreground.transform.parent = newTile.transform;
        newTile.foreground.sortingOrder = 1;
        newTile.fill = new GameObject("Tile [" + i + ", " + j + "] Fill", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        newTile.fill.transform.parent = newTile.transform;
        newTile.fill.sortingOrder = -1;
        newTile.crossed = new GameObject("Tile [" + i + ", " + j + "] Crossed", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        newTile.crossed.transform.parent = newTile.transform;
        newTile.crossed.sortingOrder = -2;
        newTile.crossed.sprite = Resources.Load<Sprite>("Font/IBM CGA THIN CROSSED");
        newTile.crossed.enabled = false;
        newObj.position = new Vector3(i * fontWidth, -j * fontHeight);
        newTile.appearances = new();
        newTile.lastEvaluatedAppearance = -1;
        newTile.Write(' ');
        return newTile;
    }

    public void PrintCreationRaceInfo(Race race)
    {
        var templates2 = new List<LineTemplate>();
        var pairs = race.stats.ToList();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Key));
        new Box("BottomLeft", "Stats", "Left", true).SetWidth(18).Write("DoubleLine", templates2);
        templates2 = new();
        for (int i = 0; i < pairs.Count; i++)
            if (pairs[i].Key == "Size") templates2.Add(new(pairs[i].Value + ":" + Size.sizes.Find(x => x.size == pairs[i].Value).name));
            else templates2.Add(new(pairs[i].Value + ""));
        new Box("BottomLeft", "", "Right", true).SetWidth(18).Write("", templates2);
        templates2 = new List<LineTemplate>();
        pairs = race.naturalResistances.ToList();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Key));
        new Box("BottomRight", "Resistances", "Left", true).SetWidth(18).Write("DoubleLine", templates2);
        templates2 = new();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Value + "%"));
        new Box("BottomRight", "", "Right", true).SetWidth(18).Write("", templates2);
    }

    public void PrintCreationBackgroundInfo(Background background)
    {
        var templates2 = new List<LineTemplate>();
        var pairs = background.bodySkills.ToList();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Key));
        new Box("Middle", "Body Skills", "Left", true).SetWidth(18).Offset(0, 0).Write("DoubleLine", templates2);
        templates2 = new();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Value + ""));
        new Box("Middle", "", "Right", true).SetWidth(18).Offset(0, 0).Write("", templates2);
        templates2 = new List<LineTemplate>();
        pairs = background.mindSkills.ToList();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Key));
        new Box("Middle", "Mind Skills", "Left", true).SetWidth(18).Offset(0, -6).Write("DoubleLine", templates2);
        templates2 = new();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Value + ""));
        new Box("Middle", "", "Right", true).SetWidth(18).Offset(0, -6).Write("", templates2);
        templates2 = new List<LineTemplate>();
        pairs = background.soulSkills.ToList();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Key));
        new Box("Bottom", "Soul Skills", "Left", true).SetWidth(18).Write("DoubleLine", templates2);
        templates2 = new();
        for (int i = 0; i < pairs.Count; i++)
            templates2.Add(new(pairs[i].Value + ""));
        new Box("Bottom", "", "Right", true).SetWidth(18).Write("", templates2);
    }
}
