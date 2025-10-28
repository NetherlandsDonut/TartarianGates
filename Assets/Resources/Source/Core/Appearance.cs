using UnityEngine;

using static Core;

public class Appearance
{
    public Appearance() { }
    public Appearance(char symbol, string fore = "?", string fill = "?", bool blinking = false, bool crossed = false)
    {
        id = charset.IndexOf(symbol);
        foreColor = StrToColor(fore);
        fillColor = StrToColor(fill);
        this.symbol = symbol + "";
        this.fore = fore;
        this.fill = fill;
        this.blinking = blinking;
        this.crossed = crossed;
    }

    //Symbol shown on the tile
    public int id; public string symbol;

    //Color of the foreground and the fill of the tile
    public Color foreColor, fillColor; public string fore, fill;

    //Whether this tile is flashing
    public bool blinking;

    //Whether this tile is crossed out
    public bool crossed;
}
