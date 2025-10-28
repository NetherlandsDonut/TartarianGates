using System;

public class Dialog
{
    public Dialog(string text, int x, int y, Func<bool> action, Func<string> dialogColor)
    {
        this.text = text;
        this.x = x;
        this.y = y;
        this.action = action;
    }

    public void AddHoverEvent(Action enter, Action exit = null)
    {
        this.enter = enter;
        this.exit = exit;
    }

    //Text that was written as part of the dialog
    public string text;

    //Position of the dialog on the screen
    public int x, y;

    //Action that is connected to the dialog
    public Func<bool> action;

    //Action that happens when the dialog is hovered over
    public Action enter, exit;
}

public class LineTemplate
{
    public LineTemplate(string text, Func<bool> action = null, Func<string> foreColor = null, Func<string> backColor = null)
    {
        this.text = text;
        this.action = action;
        this.foreColor = foreColor;
        this.backColor = backColor;
    }

    public LineTemplate AddHoverEvent(Action enter, Action exit = null)
    {
        this.enter = enter;
        this.exit = exit;
        return this;
    }

    //Text to be shown
    public string text;

    //Action that is connected to the line, making it a dialog
    public Func<bool> action;

    //Action that happens when the dialog is hovered over
    public Action enter, exit;

    //Check whether the highlighted dialog should be red instead of default blue
    public Func<string> foreColor;

    //Check whether the highlighted dialog should be red instead of default blue
    public Func<string> backColor;
}
