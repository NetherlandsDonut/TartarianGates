using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Keybinds
{
    //Removes all keybinds from list that are obselete
    public static void RemoveObseleteKeybinds()
    {
        var binds = keybinds.Select(x => x.Key).ToList();
        foreach (var keybind in binds)
            if (!defaultKeybinds.ContainsKey(keybind))
                keybinds.Remove(keybind);
    }

    //Adds missing keybinds from the default list to the current one
    //Also reorders the keybinds into the default order
    public static void AddMissingKeybinds()
    {
        foreach (var keybind in defaultKeybinds)
            if (!keybinds.ContainsKey(keybind.Key))
                keybinds.Add(keybind.Key, keybind.Value);
        var indexes = defaultKeybinds.Select(x => x.Key).ToList();
        keybinds = keybinds.OrderBy(x => indexes.IndexOf(x.Key)).ToDictionary(x => x.Key, x => x.Value);
    }
    
    //Resets all keybinds to default values
    public static void ResetAllKeybinds()
    {
        var list = keybinds.Select(x => x.Key);
        foreach (var function in list)
            ResetKeybind(function);
    }

    //Resets a keybind to it's default value
    public static void ResetKeybind(string function)
    {
        if (!keybinds.ContainsKey(function)) return;
        if (defaultKeybinds.ContainsKey(function)) keybinds[function].key = defaultKeybinds[function].key;
        else keybinds.Remove(function);
    }

    //List of all keybinds
    public static Dictionary<string, Keybind> keybinds;

    //List of the default keybinds, no interaction from outside
    public static Dictionary<string, Keybind> defaultKeybinds = new()
    {
        //Binds available everywhere
        { "Menu / Back", new() { group = "General", key = KeyCode.Escape } },
        { "Up", new() { group = "General", key = KeyCode.UpArrow } },
        { "Right", new() { group = "General", key = KeyCode.RightArrow } },
        { "Down", new() { group = "General", key = KeyCode.DownArrow } },
        { "Left", new() { group = "General", key = KeyCode.LeftArrow } },
        { "Increase", new() { group = "General", key = KeyCode.KeypadPlus } },
        { "Decrease", new() { group = "General", key = KeyCode.KeypadMinus } },
        { "Move north", new() { group = "General", key = KeyCode.W } },
        { "Move west", new() { group = "General", key = KeyCode.A } },
        { "Move south", new() { group = "General", key = KeyCode.S } },
        { "Move east", new() { group = "General", key = KeyCode.D } },
        { "Switch tab", new() { group = "General", key = KeyCode.Tab } },
    };
}

public class Keybind
{
    public string Key() => Regex.Replace(key.ToString().Replace("Alpha", "").Replace("Slash", "?"), "([a-z])([A-Z])", "$1 $2");

    //What is the group for this keybind
    public string group;

    //Key that is asigned to be pressed to execute the function
    public KeyCode key;
}
