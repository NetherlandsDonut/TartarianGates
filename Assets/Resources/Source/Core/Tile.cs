using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using static Core;

public class Tile : MonoBehaviour
{
    //Coordinates on the screen
    public int x, y;

    //Foreground, the fill and the crossed sprite of the tile
    public SpriteRenderer foreground, fill, crossed;

    //List of appearances on this tile
    public List<Appearance> appearances;

    //Current appearance used by this tile
    public Appearance appearance;

    //Number at which the animation timer should be skipped as it was evaluated already
    public int lastEvaluatedAppearance;

    //Dailog asigned
    public Dialog dialogAsigned;

    //Is mouse still over?
    public bool mouseOver;

    void FixedUpdate()
    {
        if (appearances.Count == 0) return;
        else UpdateVisual(false);
    }

    void OnMouseEnter()
    {
        mouseOver = true;
        if (dialogAsigned != currentDialog)
        {
            if (dialogAsigned == null) currentDialog = null;
            bridge.Redraw();
            if (dialogAsigned != null)
            {
                dialogAsigned.enter?.Invoke();
                Sound.PlaySound("DialogChange", 0.7f, false);
            }
        }
    }

    void OnMouseExit()
    {
        dialogAsigned?.exit?.Invoke();
        mouseOver = false;
    }

    void OnMouseDown()
    {
        if (dialogAsigned != null)
            if (bridge.DialogInteraction(dialogAsigned))
                bridge.Redraw();
    }

    public void Write(char glyph, string fore = "?", string fill = "?", bool blinking = false, bool crossed = false, bool replaceLast = false)
    {
        var newDraw = new Appearance();
        int id = charset.IndexOf(glyph);
        if (id >= 0 && id < glyphs.Length) newDraw.id = id;
        else Debug.Log("Didnt find \"" + glyph + "\"");
        if (fore.StartsWith("Dialog") && fore != "Dialog") fore += bridge.dialogColorMod;
        if (palette.ContainsKey(fore)) newDraw.foreColor = palette[fore];
        else if (fore.Count(x => x == ':') == 2) newDraw.foreColor = StrToColor(fore);
        else if (fore != "-") newDraw.foreColor = palette[defaultFore];
        if (fill.StartsWith("Dialog") && fill != "Dialog") fill += bridge.dialogColorMod;
        if (palette.ContainsKey(fill)) newDraw.fillColor = palette[fill];
        else if (fill.Count(x => x == ':') == 2) newDraw.fillColor = StrToColor(fill);
        else if (fill != "-") newDraw.fillColor = palette[defaultFill];
        if (replaceLast && appearances.Count > 0) appearances.RemoveAt(appearances.Count - 1);
        newDraw.blinking = blinking;
        newDraw.crossed = glyph != '∮' && crossed;
        //this.dialogAsigned = null;
        appearances.Add(newDraw);
        UpdateVisual(true);
    }

    //Updates the look of the tile
    public void UpdateVisual(bool forceChange)
    {
        var evaluatedAppearance = (int)(bridge.animationTimer * 2) % appearances.Count;
        if (forceChange || lastEvaluatedAppearance != evaluatedAppearance)
        {
            lastEvaluatedAppearance = evaluatedAppearance;
            var predictedAppearance = appearances[0];
            if (appearances.Count > 1) predictedAppearance = appearances[evaluatedAppearance];
            if (appearance == predictedAppearance) return;
            appearance = predictedAppearance;
            foreground.sprite = glyphs[appearance.id];
            foreground.color = appearance.foreColor;
            fill.sprite = fills[appearance.id];
            fill.color = appearance.fillColor;
            crossed.enabled = appearance.crossed;
        }
        if (appearance.blinking)
        {
            foreground.color = (int)(bridge.animationTimer * 4) % 2 == 0 ? appearance.foreColor - new Color(0.05f, 0.05f, 0.05f, 0) : appearance.foreColor;
            fill.color = (int)(bridge.animationTimer * 4) % 2 == 0 ? appearance.fillColor - new Color(0.05f, 0.05f, 0.05f, 0) : appearance.fillColor;
            crossed.enabled = appearance.crossed;
        }
    }

    public void Clear()
    {
        appearance = null;
        appearances = new();
        dialogAsigned = null;
        fill.sprite = foreground.sprite = null;
        crossed.enabled = false;
    }
}
