using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using static Core;
using static Sound;

public class Screen
{
    public Screen(string name, Action draw, Func<bool, bool> input)
    {
        this.name = name;
        this.draw = draw;
        this.input = input;
    }

    //Name of the screen
    public string name;

    //Draw action of the screen
    public Action draw;

    //Input action of the screen
    public Func<bool, bool> input;

    public static bool WentBack(string prevScreen = "None")
    {
        var didSomething = false;
        if (Input.GetMouseButtonDown(1) && bridge.contextMenu != null)
        {
            PlaySound("DialogReturn");
            bridge.contextMenu = null;
            didSomething = true;
        }
        else if (Input.GetMouseButtonDown(1) && prevScreen != "None" && prevScreen != currentScreen.name)
        {
            PlaySound("DialogReturn");
            bridge.SetScreen(prevScreen);
            didSomething = true;
        }
        return didSomething;
    }

    public static void WriteInputField(int x, int y, string text)
    {
        bridge.Write(x, y, text + " ", "Title");
        bridge.Write(x + text.Length, y, "_");
    }

    public static bool HandleInputField(ref string text, int maxLength)
    {
        var didSomething = false;
        foreach (char c in Input.inputString)
            if (c == '\b' && text.Length > 0)
            {
                text = text[..^1];
                didSomething = true;
            }
            else if (c != '\b' && c != '\n' && c != '\r' && text.Length < maxLength)
            {
                text += c;
                didSomething = true;
            }
        return didSomething;
    }

    //Currently active screen
    public static Screen currentScreen;

    //List of all in game screens
    public static List<Screen> screens = new()
    {
        //MainMenu
        new("MainMenu",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            new Box("Top", "Menu", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new("Play now", () =>
                {
                    Save.NewSave().Load();
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
                new("Start a new game", () =>
                {
                    Save.NewSave().Load();
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
                new("Load ongoing game", () =>
                {
                    var prefix = "";
                    if (useUnityData) prefix = @"C:\Users\ragan\Documents\Projects\Unity\TartarianGates\";
                    bridge.saveNames = Directory.GetFiles(prefix + @"TartarianGates_Data_Saves\").ToList();
                    if (bridge.saveNames.Count > 0) bridge.SetScreen("LoadGame");
                    return true;
                },
                () =>
                {
                    var prefix = "";
                    if (useUnityData) prefix = @"C:\Users\ragan\Documents\Projects\Unity\TartarianGates\";
                    return Directory.GetFiles(prefix + @"TartarianGates_Data_Saves\").ToList().Count > 0 ? "DialogActive" : "DialogDisabled";
                }),
                new("Settings", () =>
                {
                    bridge.screenAfterwards = "MainMenu";
                    bridge.SetScreen("Settings");
                    return true;
                }),
                new("Keybinds", () =>
                {
                    bridge.screenAfterwards = "MainMenu";
                    bridge.SetScreen("Keybinds");
                    return true;
                }),
                new("Quit", () =>
                {
                    Application.Quit();
                    return true;
                })
            });
        },
        (didSomething) =>
        {
            if (WentBack()) didSomething = true;
            return didSomething;
        }),

        #region Settings & Keybinds
        
        //Keybinds
        new("Keybinds",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            var functionList = Keybinds.keybinds.Select(x => x.Key).ToList();
            var templates = new List<LineTemplate>();
            for (int i = 0; i < functionList.Count(); i++)
            {
                var function = functionList[i];
                templates.Add(new(function, () =>
                {
                    bridge.functionBeingKeybinded = function;
                    bridge.SetScreen("ChangeKeybind");
                    return true;
                }));
            }
            new Box("Top", "Keybinds", "Left", true).SetWidth(30).Write("DoubleLine", templates);
            templates = new();
            var bindsList = Keybinds.keybinds.Select(x => x.Value).ToList();
            for (int i = 0; i < bindsList.Count(); i++)
                templates.Add(new(bindsList[i].Key(), null, () => dialogs.Count > i && currentDialog == dialogs[i] ? "" : ""));
            new Box("Top", "", "Right", true).SetWidth(30).Write("", templates);
        },
        (didSomething) =>
        {
            if (WentBack(bridge.screenAfterwards))
            {
                if (currentScreen.name != "Keybinds")
                    Serialization.Serialize(Keybinds.keybinds, "keybinds");
                didSomething = true;
            }
            return didSomething;
        }),

        //ChangeKeybind
        new("ChangeKeybind",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            new Box("Top", "Change Keybind", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new("Press a key to bind it to \"" + bridge.functionBeingKeybinded + "\""),
            });
        },
        (didSomething) =>
        {
            for (int i = 0; i < 350; i++)
                if ((i < 303 || i > 308) && (i < 323 || i > 325) && (i != 13) && Input.GetKeyDown((KeyCode)i))
                {
                    bridge.newKeybind = new Keybind() { key = (KeyCode)i };
                    if (Keybinds.keybinds[bridge.functionBeingKeybinded].key == bridge.newKeybind.key) bridge.SetScreen("TheSameChangeKeybind");
                    else if (Keybinds.keybinds.Any(x => x.Value.key == bridge.newKeybind.key)) bridge.SetScreen("ErrorChangeKeybind");
                    else bridge.SetScreen("ConfirmChangeKeybind");
                    didSomething = true;
                }
            return didSomething;
        }),

        //ErrorChangeKeybind
        new("ErrorChangeKeybind",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            new Box("Top", "Duplicated Key", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new(bridge.newKeybind.Key() + " is already binded to \"" + Keybinds.keybinds.First(x => x.Value.key == bridge.newKeybind.key).Key + "\""),
                new(""),
                new("Ok", () => { bridge.SetScreen("Keybinds"); return true; }),
            });
        },
        (didSomething) =>
        {
            if (WentBack("Keybinds")) didSomething = true;
            return didSomething;
        }),

        //TheSameChangeKeybind
        new("TheSameChangeKeybind",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            new Box("Top", "Key Already Bound", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new(bridge.newKeybind.Key() + " is already binded to \"" + bridge.functionBeingKeybinded + "\""),
                new(""),
                new("Ok", () => { bridge.SetScreen("Keybinds"); return true; }),
            });
        },
        (didSomething) =>
        {
            if (WentBack("Keybinds")) didSomething = true;
            return didSomething;
        }),

        //ConfirmChangeKeybind
        new("ConfirmChangeKeybind",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            new Box("Top", "Confrim Change", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new("Do you want to bind " + bridge.newKeybind.Key() + " to \"" + bridge.functionBeingKeybinded + "\"?"),
                new("Currently \"" + bridge.functionBeingKeybinded + "\" is binded to " + Keybinds.keybinds[bridge.functionBeingKeybinded].Key()),
                new(""),
                new("Yes", () =>
                {
                    Keybinds.keybinds[bridge.functionBeingKeybinded] = bridge.newKeybind;
                    bridge.SetScreen("Keybinds");
                    return true;
                }),
                new("No", () => { bridge.SetScreen("Keybinds"); return true; }),
            });
        },
        (didSomething) =>
        {
            if (WentBack("Keybinds")) didSomething = true;
            return didSomething;
        }),

        //Settings
        new("Settings",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            new Box("Top", "Settings", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                //new("General"),
                //new(""),
                new((Settings.settings.ambience ? "★" : "☆") + " Ambience", () => { Settings.settings.ambience ^= true; return true; }),
                new((Settings.settings.soundEffects ? "★" : "☆") + " Sound effects", () => { Settings.settings.soundEffects ^= true; return true; }),
                new((Settings.settings.runInBackground ? "★" : "☆") + " Run in background", () => { Settings.settings.runInBackground ^= true; Application.runInBackground = Settings.settings.runInBackground; return true; }),
                new((Settings.settings.pixelPerfectVision ? "★" : "☆") + " Pixel perfect vision", () => { Settings.settings.pixelPerfectVision ^= true; return true; }),
            });
        },
        (didSomething) =>
        {
            if (WentBack(bridge.screenAfterwards))
            {
                if (currentScreen.name != "Settings")
                    Serialization.Serialize(Settings.settings, "settings");
                didSomething = true;
            }
            return didSomething;
        }),
        
        //GameMenuEditScreen
        new("GameMenuEditScreen",
        () =>
        {
            new Box("Top", "Screen Elements", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new((Settings.settings.showTurn ? "★" : "☆") + " Turn indicator", () => { Settings.settings.showTurn ^= true; return true; }),
                new((Settings.settings.showFullMoon ? "★" : "☆") + " Full moon information", () => { Settings.settings.showFullMoon ^= true; return true; }),
                new((Settings.settings.showDepth ? "★" : "☆") + " Depth indicator", () => { Settings.settings.showDepth ^= true; return true; }),
            });
        },
        (didSomething) =>
        {
            if (WentBack("GameMenu"))
            {
                if (currentScreen.name != "Settings")
                    Serialization.Serialize(Settings.settings, "settings");
                didSomething = true;
            }
            return didSomething;
        }),
        
        #endregion

        #region Game
        
        //Game
        new("Game",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            var mapSizeX = screenY - 11;
            var mapSizeY = screenY - 11;
            new Box("TopLeft", "Map View", "", false).SetWidth(mapSizeX + 2).SetHeight(mapSizeY + 2).Write("DoubleLine", new());
            Save.save.map.Print(3, 3, mapSizeX, mapSizeY);
            var templates = new List<LineTemplate>();
            var templatesNames = new List<LineTemplate>();
            var cell = Save.save.map.cells.XY(Save.save.map.mapViewX, Save.save.map.mapViewY);
            foreach (var entity in cell.entities)
            {
                var print = entity.GetPrint();
                templates.Add(new(print.symbol[(cell.x + cell.y / 2) % print.symbol.Length] + "", null, () => print.foreColor, () => print.backColor));
                templatesNames.Add(new(entity.name,
                    () =>
                    {
                        return true;
                    })
                );
            }
            if (cell.wall != null)
            {
                var print = cell.wall.GetPrint();
                templates.Add(new(print.symbol[(cell.x + cell.y / 2) % print.symbol.Length] + "", null, () => print.foreColor, () => print.backColor));
                templatesNames.Add(new(cell.wall.name,
                    () =>
                    {
                        return true;
                    })
                );
            }
            if (cell.ground != null)
            {
                var print = cell.ground.GetPrint();
                templates.Add(new(print.symbol[(cell.x + cell.y / 2) % print.symbol.Length] + "", null, () => print.foreColor, () => print.backColor));
                templatesNames.Add(new(cell.ground.name,
                    () =>
                    {
                        return true;
                    })
                );
            }
            new Box("TopRight", "Inspector", "Left", false).SetWidth(screenX - 7 - mapSizeX).Write("DoubleLine", templates);
            new Box("TopRight", "", "Left", false).SetWidth(screenX - 7 - mapSizeX - 2).Write("", templatesNames);
            var offset = templates.Count + 2;
            templates = new()
            {
                new("Move up", () => { Save.save.map.entities[0].OpenDoor(0, -1); Save.save.map.entities[0].Move(0, -1); return true; }),
                new("Move right", () => { Save.save.map.entities[0].OpenDoor(1, 0); Save.save.map.entities[0].Move(1, 0); return true; }),
                new("Move down", () => { Save.save.map.entities[0].OpenDoor(0, 1); Save.save.map.entities[0].Move(0, 1); return true; }),
                new("Move left", () => { Save.save.map.entities[0].OpenDoor(-1, 0); Save.save.map.entities[0].Move(-1, 0); return true; })
            };
            new Box("BottomRight", "Actions", "Left", false).Offset(0, 5).SetWidth(screenX - 7 - mapSizeX).Write("DoubleLine", templates);
        },
        (didSomething) =>
        {
            if (WentBack("GameMenu")) didSomething = true;
            return didSomething;
        }),

        //GameMenu
        new("GameMenu",
        () =>
        {
            new Box("Top", "Menu", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new("Customise screen", () =>
                {
                    bridge.SetScreen("GameMenuEditScreen");
                    return true;
                }),
                new("Settings", () =>
                {
                    bridge.screenAfterwards = "GameMenu";
                    bridge.SetScreen("Settings");
                    return true;
                }),
                new("Keybinds", () =>
                {
                    bridge.screenAfterwards = "GameMenu";
                    bridge.SetScreen("Keybinds");
                    return true;
                }),
                new(""),
                new("Save and continue playing", () =>
                {
                    bridge.screenAfterwards = "GoBack";
                    bridge.SetScreen("SavingGame");
                    return true;
                }),
                new("Save and return to main menu", () =>
                {
                    bridge.screenAfterwards = "MainMenu";
                    bridge.SetScreen("SavingGame");
                    return true;
                }),
                new("Save and quit", () =>
                {
                    bridge.screenAfterwards = "Quit";
                    bridge.SetScreen("SavingGame");
                    return true;
                }),
            });
        },
        (didSomething) =>
        {
            if (WentBack("Game")) didSomething = true;
            return didSomething;
        }),
        
        //LoadGame
        new("LoadGame",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            var templates = new List<LineTemplate>();
            foreach (var game in bridge.saveNames)
            {
                templates.Add(new LineTemplate(game.Replace("/", "\\").Split("\\").Last(),
                    () =>
                    {
                        StopAmbience(true);
                        bridge.gameToLoad = game.Replace("/", "\\").Split("\\").Last();
                        bridge.SetScreen("LoadingGame");
                        return true;
                    })
                );
            }
            new Box("Top", "Load game", "Left", true).SetWidth(30).Write("DoubleLine", templates);
        },
        (didSomething) =>
        {
            if (WentBack("MainMenu")) didSomething = true;
            return didSomething;
        }),
        
        //LoadingGame
        new("LoadingGame",
        () =>
        {
            new Box("Top", "", "Left", true).SetWidth(30).Write("DoubleLine", new () { new("Loading..") });
        },
        (didSomething) =>
        {
            Save loadedSave = null;
            Serialization.Deserialize(ref loadedSave, bridge.gameToLoad, true, "Saves");
            loadedSave.Load();
            bridge.SetScreen("GameMap");
            return true;
        }),
        
        //SavingGame
        new("SavingGame",
        () =>
        {
            new Box("Top", "", "Left", true).SetWidth(30).Write("DoubleLine", new () { new("Saving..") });
        },
        (didSomething) =>
        {
            Serialization.Serialize(Save.save, Save.save.SaveName(), false, true, "Saves");
            if (bridge.screenAfterwards == "MainMenu")
            {
                Save.save.Close();
                bridge.SetScreen("MainMenu");
            }
            else if (bridge.screenAfterwards == "Quit")
                Application.Quit();
            else if (bridge.screenAfterwards == "GoBack")
                bridge.SetScreen("Game");
            return true;
        }),

        #endregion

        #region Game Setup

        //GameSetup
        new("GameSetup",
        () =>
        {
            PlayAmbience("Wind", 0.2f, true);
            new Box("Top", "Character setup", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new("Name", () =>
                {
                    bridge.temporaryInput = bridge.creationName;
                    bridge.SetScreen("GameSetupName");
                    return true;
                }),
                new("Gender", () =>
                {
                    bridge.SetScreen("GameSetupGender");
                    return true;
                }),
                new("Race", () =>
                {
                    bridge.SetScreen("GameSetupRace");
                    return true;
                }),
                new("Background", () =>
                {
                    if (bridge.creationRace == "") return false;
                    bridge.SetScreen("GameSetupBackground");
                    return true;
                },
                () =>
                {
                    return bridge.creationRace != "" ? "DialogActive" : "DialogDisabled";
                }),
                new(""),
                new("Finalize", () =>
                {
                    Save.save.AddEntityToPlayerParty(Save.save.FinalizeStartingCharacter());
                    Save.save.map = Map.GenerateMap(new()
                    {
                        new()
                        {
                            layout = "Cave", distribution = new()
                            {
                                { "Room", 20 },
                                { "Hall", 20 }
                            }
                        },
                        new()
                        {
                            layout = "Crypt", distribution = new()
                            {
                                { "Room", 20 },
                                { "Hall", 20 }
                            }
                        }
                    });
                    Save.save.map.PrepareMap();
                    Save.save.map.SpawnPlayerParty();
                    bridge.SetScreen("Game");
                    return true;
                }),
                new("Cancel", () =>
                {
                    Save.save = null;
                    bridge.SetScreen("MainMenu");
                    return true;
                })
            });
            new Box("Top", "", "Right", true).SetWidth(30).Write("",
            new()
            {
                new(bridge.creationName == "" ? "Random" : bridge.creationName, null, () => dialogs.Count > 0 && currentDialog == dialogs[0] ? "Title" : ""),
                new(bridge.creationGender == "" ? "Random" : bridge.creationGender, null, () => dialogs.Count > 1 && currentDialog == dialogs[1] ? "Title" : ""),
                new(bridge.creationRace == "" ? "Random" : bridge.creationRace, null, () => dialogs.Count > 2 && currentDialog == dialogs[2] ? "Title" : ""),
                new(bridge.creationBackground == "" ? "Random" : bridge.creationBackground, null, () => dialogs.Count > 3 && currentDialog == dialogs[3] ? "Title" : ""),
                new(""),
                new(""),
                new(""),
            });
            if (bridge.creationRace != "") bridge.PrintCreationRaceInfo(Race.races.Find(x => x.name == bridge.creationRace));
            if (bridge.creationBackground != "") bridge.PrintCreationBackgroundInfo(Background.backgrounds.Find(x => x.name == bridge.creationBackground));
        },
        (didSomething) =>
        {
            if (WentBack("MainMenu")) didSomething = true;
            return didSomething;
        }),

        //GameSetupName
        new("GameSetupName",
        () =>
        {
            new Box("Top", "Enter character name", "Left", true).SetWidth(30).Write("DoubleLine",
            new()
            {
                new(""),
                new(""),
                new("Confirm", () =>
                {
                    if (bridge.temporaryInput.Length < 2) return false;
                    bridge.creationName = bridge.temporaryInput;
                    bridge.SetScreen("GameSetup");
                    return true;
                },
                () => bridge.temporaryInput.Length < 2 ? "DialogDisabled" : "DialogActive"),
                new("Cancel", () =>
                {
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
            });
            WriteInputField(26, 3, bridge.temporaryInput);
            if (bridge.creationRace != "") bridge.PrintCreationRaceInfo(Race.races.Find(x => x.name == bridge.creationRace));
            if (bridge.creationBackground != "") bridge.PrintCreationBackgroundInfo(Background.backgrounds.Find(x => x.name == bridge.creationBackground));
        },
        (didSomething) =>
        {
            if (HandleInputField(ref bridge.temporaryInput, 20)) didSomething = true;
            if (WentBack("GameSetup")) didSomething = true;
            return didSomething;
        }),
        
        //GameSetupGender
        new("GameSetupGender",
        () =>
        {
            var templates = new List<LineTemplate>
            {
                new("Man", () =>
                {
                    bridge.creationGender = "Man";
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
                new("Woman", () =>
                {
                    bridge.creationGender = "Woman";
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
                new("Trans man", () =>
                {
                    bridge.creationGender = "Trans man";
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
                new("Trans woman", () =>
                {
                    bridge.creationGender = "Trans woman";
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
                new("Non binary", () =>
                {
                    bridge.creationGender = "Non binary";
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
                new(""),
                new("Set to random", () =>
                {
                    bridge.creationGender = "";
                    bridge.SetScreen("GameSetup");
                    return true;
                }),
                new("Cancel", () =>
                {
                    bridge.SetScreen("GameSetup");
                    return true;
                })
            };
            new Box("Top", "Available genders", "Left", true).SetWidth(30).Write("DoubleLine", templates);
            if (bridge.creationRace != "") bridge.PrintCreationRaceInfo(Race.races.Find(x => x.name == bridge.creationRace));
            if (bridge.creationBackground != "") bridge.PrintCreationBackgroundInfo(Background.backgrounds.Find(x => x.name == bridge.creationBackground));
        },
        (didSomething) =>
        {
            if (WentBack("GameSetup")) didSomething = true;
            return didSomething;
        }),
        
        //GameSetupRace
        new("GameSetupRace",
        () =>
        {
            var templates = new List<LineTemplate>();
            for (int i = 0; i < Race.startingRaces.Count; i++)
            {
                var race = Race.startingRaces[i];
                templates.Add(new LineTemplate(race.name, () =>
                {
                    bridge.creationRace = race.name;
                    bridge.creationBackground = "";
                    bridge.SetScreen("GameSetup");
                    return true;
                }).
                AddHoverEvent(() => bridge.PrintCreationRaceInfo(race)));
            }
            templates.Add(new(""));
            templates.Add(new("Set to random", () =>
            {
                bridge.creationRace = "";
                bridge.creationBackground = "";
                bridge.SetScreen("GameSetup");
                return true;
            }));
            templates.Add(new("Cancel", () =>
            {
                bridge.SetScreen("GameSetup");
                return true;
            }));
            new Box("Top", "Available races", "Left", true).SetWidth(30).Write("DoubleLine", templates);
            //bridge.Write(10, 03, "Body┐             - /", "?", "?", false, false, true);
            //bridge.Write(10, 04, "    ├Head┐        - /", "?", "?", false, false, true);
            //bridge.Write(10, 05, "    │    └Face    - /", "?", "?", false, false, true);
            //bridge.Write(10, 06, "    ├Torso┐       - /", "?", "?", false, false, true);
            //bridge.Write(10, 07, "    │     ├Chest  - /", "?", "?", false, false, true);
            //bridge.Write(10, 08, "    │     ├Attire - /", "?", "?", false, false, true);
            //bridge.Write(10, 09, "    │     └Back   - /", "?", "?", false, false, true);
            //bridge.Write(10, 10, "    ├Hands        - /", "?", "?", false, false, true);
            //bridge.Write(10, 11, "    └Legs┐        - /", "?", "?", false, false, true);
            //bridge.Write(10, 12, "         └Feet    - /", "?", "?", false, false, true);

            //bridge.Write(30, 03, "Robe / Dress", "DimGray", "?", false, false, true);
            //bridge.Write(30, 04, "Black Steel Bascinet", "Epic", "?", false, false, true);
            //bridge.Write(30, 05, "X", "Poor", "?", false, false, true);
            //bridge.Write(30, 06, "Red Woolen Pourpoint", "Rare", "?", false, false, true);
            //bridge.Write(30, 07, "Black Steel Breastplate", "Epic", "?", false, false, true);
            //bridge.Write(30, 08, "White Linen Shirt", "Common", "?", false, false, true);
            //bridge.Write(30, 09, "Red Dragon Scale Cloak", "Epic", "?", false, false, true);
            //bridge.Write(30, 10, "Black Steel Gauntlets", "Epic", "?", false, false, true);
            //bridge.Write(30, 11, "Black Steel Greaves", "Epic", "?", false, false, true);
            //bridge.Write(30, 12, "Black Steel Chausses", "Epic", "?", false, false, true);
        },
        (didSomething) =>
        {
            if (WentBack("GameSetup")) didSomething = true;
            return didSomething;
        }),
        
        //GameSetupBackground
        new("GameSetupBackground",
        () =>
        {
            var templates = new List<LineTemplate>();
            var backgrounds = Background.backgrounds.FindAll(x => x.race == bridge.creationRace);
            for (int i = 0; i < backgrounds.Count; i++)
            {
                var background = backgrounds[i];
                templates.Add(new LineTemplate(background.name, () =>
                {
                    bridge.creationBackground = background.name;
                    bridge.SetScreen("GameSetup");
                    return true;
                }).
                AddHoverEvent(() => bridge.PrintCreationBackgroundInfo(background)));
            }
            templates.Add(new(""));
            templates.Add(new("Set to random", () =>
            {
                bridge.creationBackground = "";
                bridge.SetScreen("GameSetup");
                return true;
            }));
            templates.Add(new("Cancel", () =>
            {
                bridge.SetScreen("GameSetup");
                return true;
            }));
            new Box("Top", "Available backgrounds", "Left", true).SetWidth(30).Write("DoubleLine", templates);
            if (bridge.creationRace != "") bridge.PrintCreationRaceInfo(Race.races.Find(x => x.name == bridge.creationRace));
        },
        (didSomething) =>
        {
            if (WentBack("GameSetup")) didSomething = true;
            return didSomething;
        }),

        #endregion
    };
}
