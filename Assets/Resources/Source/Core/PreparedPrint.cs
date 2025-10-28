
public class PreparedPrint
{
    public PreparedPrint() { }
    public PreparedPrint(string symbol, string foreColor = "?", string backColor = "?", bool blinking = false)
    {
        this.symbol = symbol;
        this.foreColor = foreColor;
        this.backColor = backColor;
        this.blinking = blinking;
    }

    //Symbol from a font used for display
    public string symbol;

    //Colors of the print
    public string foreColor, backColor;

    //Whether the tile blinks
    public bool blinking;
}
