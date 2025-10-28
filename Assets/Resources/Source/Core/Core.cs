using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public static class Core
{
    public static System.Random random;

    public static bool useUnityData = true;

    public static int renderWidth = 480;
    public static int screenX = 60;
    public static int fontWidth = 8;

    public static int renderHeight = 270;
    public static int screenY = 34;
    public static int fontHeight = 8;

    public static string charset = @"!""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~a ¡¢£¤¥¦§¨©ª«¬-®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿĀāĂăĄąĆćĈĉĊċČčĎďĐđĒēĔĕĖėĘęĚěĜĝĞğĠġĢģĤĥĦħĨĩĪīĬĭĮįİıĲĳĴĵĶķĸĹĺĻļĽľĿŀŁłŃńŅņŇňŉŊŋŌōŎŏŐőŒœŔŕŖŗŘřŚśŜŝŞşŠšŢţŤťŦŧŨũŪūŬŭŮůŰűŲųŴŵŶŷŸŹźŻżŽžſƒơƷǺǻǼǽǾǿȘșȚțɑɸˆˇˉ˘˙˚˛˜˝;΄΅Ά·ΈΉΊΌΎΏΐΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩΪΫάέήίΰαβγδεζηθικλμνξοπρςστυφχψωϊϋόύώϐϴЀЁЂЃЄЅІЇЈЉЊЋЌЍЎЏАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюяѐёђѓєѕіїјљњћќѝўџҐґ־אבגדהוזחטיךכלםמןנסעףפץצקרשתװױײ׳״ᴛᴦᴨẀẁẂẃẄẅẟỲỳ‐‒–—―‗‘’‚‛“”„‟†‡•…‧‰′″‵‹›‼‾‿⁀⁄⁔⁴⁵⁶⁷⁸⁹⁺⁻ⁿ₁₂₃₄₅₆₇₈₉₊₋₣₤₧₪€℅ℓ№™Ω℮⅐⅑⅓⅔⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞←↑→↓↔↕↨∂∅∆∈∏∑−∕∙√∞∟∩∫≈≠≡≤≥⊙⌀⌂⌐⌠⌡─│┌┐└┘├┤┬┴┼═║╒╓╔╕╖╗╘╙╚╛╜╝╞╟╠╡╢╣╤╥╦╧╨╩╪╫╬▀▁▄█▌▐░▒▓■□▪▫▬▲►▼◄◊○●◘◙◦☺☻☼♀♂♠♣♥♦♪♫✓ﬁﬂ☆★∧≮≯∨╱╲╳╭╮╰╯↗↖↘↙︾︽≒∮⿹⿽⿺⿸";

    public static string defaultFore = "Text";
    public static string defaultFill = "Transparent";

    public static string lastKey;

    public static float keyTimerNewKey = 0.4f;
    public static float keyTimerSameKey = 0.05f;
    public static int simulationCurrentSpeed = 1;
    public static float[] simulationTimerSpeeds = new[] { 1f, 0.1f, 0.01f };

    public static List<Dialog> dialogs;
    public static Dialog currentDialog;
    
    public static List<string> fontsAvailable = new() { "IBM CGA THIN" };

    //Font sprites
    public static Sprite[] glyphs, fills;

    //Color palette for the game
    public static Dictionary<string, Color32> palette = new()
    {
        { "White",          new Color32(234, 234, 234, 255) },
        { "LightGray",      new Color32(202, 202, 202, 255) },
        { "Gray",           new Color32(183, 183, 183, 255) },
        { "DimGray",        new Color32(050, 050, 050, 255) },
        { "Black",          new Color32(020, 020, 020, 255) },
        { "FullBlack",      new Color32(000, 000, 000, 255) },
        { "Transparent",    new Color32(000, 000, 000, 000) },
        { "DarkGray",       new Color32(114, 114, 114, 255) },
        { "DarkGrayDarker", new Color32(084, 084, 084, 255) },

        { "Box",            new Color32(071, 071, 071, 255) },
        { "Text",           new Color32(127, 127, 127, 255) },
        { "Title",          new Color32(160, 160, 160, 255) },
        { "Dialog",         new Color32(155, 155, 143, 255) },

        { "Copper",         new Color32(184, 080, 041, 255) },
        { "Silver",         new Color32(170, 188, 210, 255) },
        { "Gold",           new Color32(255, 210, 011, 255) },
        { "Poor",           new Color32(114, 114, 114, 255) },
        { "Common",         new Color32(183, 183, 183, 255) },
        { "Uncommon",       new Color32(026, 201, 000, 255) },
        { "Rare",           new Color32(000, 117, 226, 255) },
        { "Epic",           new Color32(163, 053, 238, 255) },
        { "Legendary",      new Color32(221, 110, 000, 255) },

        { "Blue",           new Color32(010, 010, 220, 255) },
        { "Purple",         new Color32(220, 010, 220, 255) },
        { "Red",            new Color32(220, 010, 010, 255) },
        { "Yellow",         new Color32(220, 220, 010, 255) },
        { "Green",          new Color32(010, 220, 010, 255) },
        { "Cyan",           new Color32(010, 220, 220, 255) },

        { "DarkBlue",       new Color32(000, 000, 120, 255) },
        { "DarkPurple",     new Color32(120, 000, 120, 255) },
        { "DarkRed",        new Color32(120, 000, 000, 255) },
        { "DarkYellow",     new Color32(120, 120, 000, 255) },
        { "DarkGreen",      new Color32(000, 120, 000, 255) },
        { "DarkCyan",       new Color32(000, 120, 120, 255) },

        { "LightBlue",      new Color32(110, 110, 220, 255) },
        { "LightPurple",    new Color32(220, 110, 220, 255) },
        { "LightRed",       new Color32(220, 110, 220, 255) },
        { "LightYellow",    new Color32(220, 220, 110, 255) },
        { "LightGreen",     new Color32(110, 220, 110, 255) },
        { "LightCyan",      new Color32(110, 220, 220, 255) },

        { "DialogActive",   new Color32(207, 207, 137, 255) },
        { "DialogDisabled", new Color32(207, 207, 137, 255) },

        { "DialogActiveHighElves",      new Color32(136, 179, 204, 255) },
        { "DialogDisabledHighElves",    new Color32(136, 179, 204, 255) },

        { "DialogActiveBeastmen",       new Color32(204, 177, 136, 255) },
        { "DialogDisabledBeastmen",     new Color32(204, 177, 136, 255) },

        { "DialogActiveDraconians",     new Color32(136, 204, 200, 255) },
        { "DialogDisabledDraconians",   new Color32(136, 204, 200, 255) },

        { "DialogActiveOrc",            new Color32(206, 138, 141, 255) },
        { "DialogDisabledOrc",          new Color32(206, 138, 141, 255) },

        { "DialogActiveHighMen",        new Color32(138, 153, 206, 255) },
        { "DialogDisabledHighMen",      new Color32(138, 153, 206, 255) },

        { "DialogActiveDwarfs",         new Color32(207, 207, 137, 255) },
        { "DialogDisabledDwarfs",       new Color32(207, 207, 137, 255) },

        { "DialogActiveDarkElves",      new Color32(206, 138, 205, 255) },
        { "DialogDisabledDarkElves",    new Color32(206, 138, 205, 255) },

        { "DialogActiveTrolls",         new Color32(150, 206, 138, 255) },
        { "DialogDisabledTrolls",       new Color32(150, 206, 138, 255) },

        { "DialogActiveLizardmen",      new Color32(138, 206, 170, 255) },
        { "DialogDisabledLizardmen",    new Color32(138, 206, 170, 255) },
    };

    public static List<char> vowels = new()
    {
        'a', 'A', 'ą', 'Ą', 'e', 'E', 'ę', 'Ę', 'i', 'I', 'o', 'O', 'u', 'U', 'ó', 'Ó', 'y', 'Y'
    };

    //Screen object
    public static Bridge bridge;

    //Screen tiles
    public static Tile[,] tiles;

    //Loads the current selected font in the settings
    public static void LoadFont()
    {
        glyphs = Resources.LoadAll<Sprite>("Font/" + fontsAvailable[0] + " 1");
        fills = Resources.LoadAll<Sprite>("Font/" + fontsAvailable[0] + " 2");
    }

    //Converts a coded string into a color
    public static Color32 StrToColor(string color)
    {
        if (palette.ContainsKey(color))
            return palette[color];
        else if (color.Contains(":"))
        {
            var split = color.Split(':').Select(x => (byte)int.Parse(x)).ToList();
            return new Color32(split[0], split[1], split[2], 255);
        }
        return palette["Transparent"];
    }

    //Darkens a color by a specific amount
    public static string ShiftColor(string color, int darken)
    {
        var darkenedColor = color;
        if (palette.ContainsKey(color))
            darkenedColor = palette[color].r + ":" + palette[color].g + ":" + palette[color].b;
        var split = darkenedColor.Split(':').Select(x => int.Parse(x) + darken).Select(x => x > 255 ? 255 : (x > 0 ? x : 0));
        return string.Join(':', split);
    }

    //Darkens a color by a specific amount
    public static string ColorMultiplier(string color, float multiplier)
    {
        var darkenedColor = color;
        if (palette.ContainsKey(color))
            darkenedColor = palette[color].r + ":" + palette[color].g + ":" + palette[color].b;
        var split = darkenedColor.Split(':').Select(x => (int)(float.Parse(x) * multiplier)).Select(x => x > 255 ? 255 : (x > 0 ? x : 0));
        return string.Join(':', split);
    }

    //Darkens a color by a specific amounts
    public static string ShiftColor(string color, int r, int g, int b)
    {
        var darkenedColor = color;
        if (palette.ContainsKey(color))
            darkenedColor = palette[color].r + ":" + palette[color].g + ":" + palette[color].b;
        var split = darkenedColor.Split(':').Select(x => int.Parse(x)).ToArray();
        var split2 = new int[3] { split[0] + r, split[1] + g, split[2] + b }.Select(x => x > 255 ? 255 : (x > 0 ? x : 0));
        return string.Join(':', split2);
    }

    //Converts a number into the roman notation
    public static string ToRoman(int number)
    {
        if (number < 1 || number > 3999) return "";
        if (number >= 1000) return "M" + ToRoman(number - 1000);
        if (number >= 900) return "CM" + ToRoman(number - 900);
        if (number >= 500) return "D" + ToRoman(number - 500);
        if (number >= 400) return "CD" + ToRoman(number - 400);
        if (number >= 100) return "C" + ToRoman(number - 100);
        if (number >= 90) return "XC" + ToRoman(number - 90);
        if (number >= 50) return "L" + ToRoman(number - 50);
        if (number >= 40) return "XL" + ToRoman(number - 40);
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);
        return "";
    }

    //Converts a number into a word
    public static string ToWord(int number)
    {
        if (number == 0) return "None";
        else if (number == 1) return "One";
        else if (number == 2) return "Two";
        else if (number == 3) return "Three";
        else if (number == 4) return "Four";
        else if (number == 5) return "Five";
        else if (number == 6) return "Six";
        else if (number == 7) return "Seven";
        else if (number == 8) return "Eight";
        else if (number == 9) return "Nine";
        return "";
    }

    //Rolls a chance with a provided % of something happening [0 - 100]
    public static bool Roll(double chance) => random.Next(0, 100000) < chance * 1000;

    //Rotates an array by 90 degress times n
    public static T[,] Rotate<T>(this T[,] what, int n)
    {
        n %= 4;
        T[,] foo = new T[what.GetLength(n % 2), what.GetLength((n + 1) % 2)];
        for (int i = 0; i < what.GetLength(0); i++)
            for (int j = 0; j < what.GetLength(1); j++)
                if (n == 0) foo[i, j] = what[i, j];
                else if (n == 1) foo[j, foo.GetLength(1) - 1 - i] = what[i, j];
                else if (n == 2) foo[foo.GetLength(0) - 1 - i, foo.GetLength(1) - 1 - j] = what[i, j];
                else if (n == 3) foo[foo.GetLength(0) - 1 - j, i] = what[i, j];
        return foo;
    }

    //Flips an array on any of the two axis
    public static T[,] Flip<T>(this T[,] what, bool flipX, bool flipY)
    {
        int x = what.GetLength(0), y = what.GetLength(1);
        T[,] foo = new T[x, y];
        for (int i = 0; i < x; i++)
            for (int j = 0; j < y; j++)
                foo[i, j] = what[flipY ? x - i - 1 : i, flipX ? y - j - 1 : j];
        return foo;
    }

    //Gives whitespace in a specified amount
    public static string Whitespace(int count) => count < 1 ? "" : new string('∮', count);
    
    //Checks whether the provided symbol is a box drawing
    public static bool IsBoxDrawing(int symbol) => symbol >= 669 && symbol <= 708;

    //Loads an image into the program from a file
    public static Texture2D LoadImage(string file, bool encoded = false, string prefix = "")
    {
        if (useUnityData) prefix = @"C:\Users\ragan\Documents\Projects\Unity\TartarianGates\";
        if (!Directory.Exists(prefix + "TartarianGates_Data_Source"))
            Directory.CreateDirectory(prefix + "TartarianGates_Data_Source");
        var imagePath = prefix + @"TartarianGates_Data_Source\" + file + (encoded ? "" : ".png");
        if (!File.Exists(imagePath)) return null;
        byte[] byteArray = File.ReadAllBytes(imagePath);
        Texture2D tex = new(1, 1, TextureFormat.ARGB32, false);
        tex.LoadImage(byteArray);
        return tex;
    }

    public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Point)
    {
        Rect texR = new(0, 0, width, height);
        GpuScale(src, width, height, mode);
        Texture2D result = new(width, height, TextureFormat.ARGB32, true);
        result.Reinitialize(width, height);
        result.ReadPixels(texR, 0, 0, true);
        return result;
    }

    public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Point)
    {
        Rect texR = new(0, 0, width, height);
        GpuScale(tex, width, height, mode);
        tex.Reinitialize(width, height);
        tex.ReadPixels(texR, 0, 0, true);
        tex.Apply(true);
    }

    // Internal unility that renders the source texture into the RTT - the scaling method itself.
    static void GpuScale(Texture2D src, int width, int height, FilterMode fmode)
    {
        src.filterMode = fmode;
        src.Apply(true);
        RenderTexture rtt = new(width, height, 32);
        Graphics.SetRenderTarget(rtt);
        GL.LoadPixelMatrix(0, 1, 1, 0);
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
    }

    //Indexes single dim array
    public static T X<T>(this T[] array, int x)
    {
        if (x > array.GetLength(0) - 1 || x < 0) return default;
        else return array[x];
    }

    //Indexes two dim array
    public static T XY<T>(this T[,] array, int x, int y)
    {
        if (x > array.GetLength(0) - 1 || x < 0 || y > array.GetLength(1) - 1 || y < 0) return default;
        else return array[x, y];
    }

    //Shuffles a list
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = list.Count, rnd = random.Next(i--); i >= 1; rnd = random.Next(i--))
            (list[i], list[rnd]) = (list[rnd], list[i]);
    }
}
