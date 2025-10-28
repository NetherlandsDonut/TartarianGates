using System.Linq;
using System.Collections.Generic;

using static Core;

public class Box
{
    public Box(string anchor, string title = "", string textAlign = "Left", bool fullLine = false)
    {
        this.anchor = anchor;
        this.title = title;
        this.textAlign = textAlign;
        this.fullLine = fullLine;
    }

    //Moves the box away from the initial anchor
    public Box Offset(int X, int Y)
    {
        x += X;
        y -= Y;
        return this;
    }

    //Sets the width of the box to be static
    public Box SetWidth(int width)
    {
        setWidth = width;
        return this;
    }

    //Sets the height of the box to be static
    public Box SetHeight(int height)
    {
        setHeight = height;
        return this;
    }

    //Writes the box on the screen
    public void Write(string style, List<LineTemplate> lineTemplates)
    {
        //Calculate the size of the box and it's offset in the screen space
        var width = 0;
        if (setWidth != 0) width = setWidth;
        else width = lineTemplates.Max(x => x.text.Length) + 2;
        var height = 0;
        if (setHeight != 0) height = setHeight;
        else height = lineTemplates.Count + 2;
        int offsetX = x, offsetY = y;

        //Decide where the box should be drawn on the screen
        if (anchor == "TopLeft") (x, y) = (1, 1);
        else if (anchor == "Top") (x, y) = (screenX / 2 - width / 2 - 1, 1);
        else if (anchor == "TopRight") (x, y) = (screenX - width - 2 - 1, 1);
        else if (anchor == "MiddleLeft") (x, y) = (1, screenY / 2 - height / 2 - 1);
        else if (anchor == "Middle") (x, y) = (screenX / 2 - width / 2 - 1, screenY / 2 - height / 2 - 1);
        else if (anchor == "MiddleRight") (x, y) = (screenX - width - 2 - 1, screenY / 2 - height / 2 - 1);
        else if (anchor == "BottomLeft") (x, y) = (1, screenY - height - 2 - 1);
        else if (anchor == "Bottom") (x, y) = (screenX / 2 - width / 2 - 1, screenY - height - 2 - 1);
        else if (anchor == "BottomRight") (x, y) = (screenX - width - 2 - 1, screenY - height - 2 - 1);
        else (x, y) = (1, 1);
        (x, y) = (x + offsetX, y + offsetY);

        //If the box has any style then clear all
        //the area under the soon to be printed box
        if (style != "")
            for (int i = -1; i < width + 3; i++)
                for (int j = -1; j < height + 3; j++)
                    if (x + i >= 0 && x + i < tiles.GetLength(0) && y + j >= 0 && y + j < tiles.GetLength(1))
                        if ((i != -1 || j != -1) && (i != width + 2 || j != -1) && (i != -1 || j != height + 2) && (i != width + 2 || j != height + 2))
                            if (tiles[x + i, y + j].appearance != null && !IsBoxDrawing(tiles[x + i, y + j].appearance.id))
                                tiles[x + i, y + j].Clear();

        //Write down the box borders
        if (style != "")
        {
            bridge.WriteBoxTile(x, y, style == "SingleLine" ? '┌' : '╔', "Box");
            for (int i = 0; i < width; i++)
            {
                bridge.WriteBoxTile(x + 1 + i, y, style == "SingleLine" ? '─' : '═', "Box");
                bridge.WriteBoxTile(x + 1 + i, y + height + 1, style == "SingleLine" ? '─' : '═', "Box");
            }
            bridge.WriteBoxTile(x, y + height + 1, style == "SingleLine" ? '└' : '╚', "Box");
            bridge.WriteBoxTile(x + 1 + width, y, style == "SingleLine" ? '┐' : '╗', "Box");
            bridge.WriteBoxTile(x + 1 + width, y + height + 1, style == "SingleLine" ? '┘' : '╝', "Box");
        }
        for (int i = 0; i < height; i++)
        {
            if (style != "") bridge.WriteBoxTile(x, y + 1 + i, style == "SingleLine" ? '│' : '║', "Box");
            if (style != "") bridge.WriteBoxTile(x + 1 + width, y + 1 + i, style == "SingleLine" ? '│' : '║', "Box");
        }

        //If there is a title to write
        //Write it on the top of the box
        if (title != null && title.Length > 0)
            bridge.Write(x + width / 2 - title.Length / 2, y, "∮" + title + "∮", "Title", "?", false, false, true);

        //Write the contents of the box
        var currentY = y + 2;
        for (int i = 0; i < lineTemplates.Count && i < height - 2; i++)
        {
            var line = lineTemplates[i];
            var centerAlignment = x + 1 + width / 2 - line.text.Length / 2;
            var X = fullLine || textAlign == "Left" ? x + 2 : (textAlign == "Right" ? x + width - line.text.Length : centerAlignment);
            var text = fullLine ? Whitespace(textAlign == "Center" ? centerAlignment - x - 2 : (textAlign == "Right" ? width - line.text.Length - 2 : 0)) + line.text + Whitespace(textAlign == "Center" ? x + width - centerAlignment - line.text.Length : (textAlign == "Left" ? width - line.text.Length - 2 : 0)) : line.text;
            if (line.action != null) bridge.WriteDialog(X, currentY++, text, line.action, line.foreColor, line.backColor, line.enter, line.exit);
            else bridge.Write(X, currentY++, text, line.foreColor == null ? "Text" : line.foreColor(), line.backColor == null ? "Transparent" : line.backColor(), false, false, true);
        }
    }

    //Position of the box on the screen
    public int x, y;

    //Set width and height of the box, by default it is flexible to it's contents
    public int setWidth, setHeight;

    //Anchor of the box telling it where it should be on the screen
    public string anchor;

    //Title of the box on the top
    public string title;

    //What way will the text align itself ["Left", "Center", "Right"]
    public string textAlign;

    //Will dialog options be printed as full line of equal length?
    public bool fullLine;

    //Amount of space left in the box for new lines
    public int spaceLeft;
}
