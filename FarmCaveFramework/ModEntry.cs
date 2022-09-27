using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace FarmCaveFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static IDynamicGameAssetsApi apiDGA;
        private static IJsonAssetsApi apiJA;

        public static CaveChoice caveChoice;

        public static readonly string frameworkPath = "farm_cave_choices";
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Content.AssetRequested += Content_AssetRequested;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmCave), nameof(FarmCave.UpdateWhenCurrentLocation)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmCave_UpdateWhenCurrentLocation_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmCave), "resetLocalState"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmCave_resetLocalState_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmCave), nameof(FarmCave.DayUpdate)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmCave_DayUpdate_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmCave), nameof(FarmCave.UpdateReadyFlag)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmCave_UpdateReadyFlag_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.command_cave)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Event_command_cave_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.answerDialogue)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Event_answerDialogue_Prefix))
            );


        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Farm"))
            {
                e.Edit(delegate (IAssetData obj) {
                    var editor = obj.AsDictionary<string, string>();
                    string[] v = editor.Data["65/m 25000/t 600 1200/H"].Split('"');
                    v[1] = Helper.Translation.Get("event-intro");
                    Dictionary<string, CaveChoice> choices = Helper.GameContent.Load<Dictionary<string, CaveChoice>>(frameworkPath);
                    foreach (var cc in choices)
                    {
                        if (cc.Value.description != null)
                            v[1] += "#$b#" + cc.Value.description;
                    }
                    editor.Data["65/m 25000/t 600 1200/H"] = string.Join("\"", v);
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(frameworkPath))
            {
                e.LoadFrom(LoadFramework, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private object LoadFramework()
        {
            Monitor.Log("Loading choice list");

            Dictionary<string, CaveChoice> choices = new Dictionary<string, CaveChoice>(){
                {
                    "Bats",
                    new CaveChoice()
                    {
                        id = "Bats",
                        choice = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1222"),
                        description = Helper.Translation.Get("bat-description"),
                        resources = new List<CaveResource>()
                        {
                            new CaveResource()
                            {
                                id = "296",
                                weight = 450
                            },
                            new CaveResource()
                            {
                                id = "396",
                                weight = 450
                            },
                            new CaveResource()
                            {
                                id = "406",
                                weight = 450
                            },
                            new CaveResource()
                            {
                                id = "410",
                                weight = 450
                            },
                            new CaveResource()
                            {
                                id = "613",
                                weight = 45
                            },
                            new CaveResource()
                            {
                                id = "634",
                                weight = 81
                            },
                            new CaveResource()
                            {
                                id = "635",
                                weight = 81
                            },
                            new CaveResource()
                            {
                                id = "636",
                                weight = 81
                            },
                            new CaveResource()
                            {
                                id = "637",
                                weight = 81
                            },
                            new CaveResource()
                            {
                                id = "638",
                                weight = 81
                            },
                        },
                        animations = new List<CaveAnimation>()
                        {
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 0,
                                Y = 0,
                                interval = 3000,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                light = true,
                                lightRadius = 0.5f
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 8,
                                Y = 0,
                                interval = 3000,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 320,
                                Y = -64,
                                interval = 2000,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 500,
                                light = true,
                                lightRadius = 0.5f
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 328,
                                Y = -64,
                                interval = 2000,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 500
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 128,
                                Y = -64,
                                interval = 1600,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 250,
                                light = true,
                                lightRadius = 0.5f,
                                bottom = true
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 136,
                                Y = -64,
                                interval = 1600,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 250,
                                bottom = true
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 68,
                                Y = 192,
                                interval = 2800,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 750,
                                light = true,
                                lightRadius = 0.5f,
                                right = true
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 76,
                                Y = 192,
                                interval = 2800,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 750,
                                right = true
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 68,
                                Y = 576,
                                interval = 2200,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 750,
                                light = true,
                                lightRadius = 0.5f,
                                right = true
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = 76,
                                Y = 576,
                                interval = 2200,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 750,
                                right = true
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = -60,
                                Y = 128,
                                interval = 2600,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 750,
                                light = true,
                                lightRadius = 0.5f
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = -52,
                                Y = 128,
                                interval = 2600,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 750
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = -64,
                                Y = 384,
                                interval = 3400,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 650,
                                light = true,
                                lightRadius = 0.5f
                            },
                            new CaveAnimation()
                            {
                                sourceFile = "LooseSprites\\Cursors",
                                sourceX = 374,
                                sourceY = 358,
                                width = 1,
                                height = 1,
                                X = -52,
                                Y = 384,
                                interval = 3400,
                                length = 3,
                                loops = 99999,
                                scale = 4,
                                delay = 650
                            },

                        },
                        periodics = new List<CavePeriodicEffect>()
                        {
                            new CavePeriodicEffect()
                            {
                                randomSounds = new List<CaveSound>()
                                {
                                    new CaveSound()
                                    {
                                        id = "batScreech",
                                        chance = 15
                                    }
                                },
                                animations = new List<CaveAnimation>()
                                {
                                    new CaveAnimation()
                                    {
                                        sourceFile = "LooseSprites\\Cursors",
                                        sourceX = 640,
                                        sourceY = 1664,
                                        width = 16,
                                        height = 16,
                                        interval = 80,
                                        length = 4,
                                        loops = 9999,
                                        randomX = true,
                                        bottom = true,
                                        loopTIme = 2000,
                                        range = 64,
                                        motionX = 0,
                                        motionY = -8
                                    }
                                },
                                chance = 0.2f,
                                repeatedSounds = new List<CaveSound>()
                                {
                                    new CaveSound()
                                    {
                                        id = "batFlap",
                                        chance = 100,
                                        delayMult = 320,
                                        delayAdd = -80,
                                        count = 5
                                    }
                                }
                            },
                            new CavePeriodicEffect()
                            {
                                chance = 0.5f,
                                specials = new List<string>()
                                {
                                    "BatTemporarySprite"
                                }
                            }
                        },
                        ambientLight = new Color(70, 90, 0)
                    }
                },
                {
                    "Mushrooms",
                    new CaveChoice()
                    {
                        id = "Mushrooms",
                        choice = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1220"),
                        description = Helper.Translation.Get("mushroom-description"),
                        objects = new List<CaveObject>()
                        {
                            new CaveObject()
                            {
                                X = 4,
                                Y = 5,
                                id = "128"
                            },
                            new CaveObject()
                            {
                                X = 6,
                                Y = 5,
                                id = "128"
                            },
                            new CaveObject()
                            {
                                X = 8,
                                Y = 5,
                                id = "128"
                            },
                            new CaveObject()
                            {
                                X = 4,
                                Y = 7,
                                id = "128"
                            },
                            new CaveObject()
                            {
                                X = 6,
                                Y = 7,
                                id = "128"
                            },
                            new CaveObject()
                            {
                                X = 8,
                                Y = 7,
                                id = "128"
                            }
                        }
                    }
                }
            };
            return choices;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;
            caveChoice = null;
            if(Config.EnableMod && Config.ResetEvent)
            {
                Config.ResetEvent = false;
                Helper.WriteConfig(Config);
                Game1.player.eventsSeen.Remove(65);
                string choiceId = SHelper.Data.ReadSaveData<string>("farm-cave-framework-choice");
                Game1.getLocationFromName("FarmCave").objects.Clear();
                if (choiceId != null && choiceId.Length > 0)
                {
                    SHelper.Data.WriteSaveData("farm-cave-framework-choice", "");
                }
                Monitor.Log("Reset farm cave and event");
            }
            else
                LoadCaveChoice();
        }

        private static void LoadCaveChoice()
        {
            string choiceId = SHelper.Data.ReadSaveData<string>("farm-cave-framework-choice");
            if (choiceId != null && choiceId.Length > 0)
            {
                Dictionary<string, CaveChoice> choices = SHelper.GameContent.Load<Dictionary<string, CaveChoice>>(frameworkPath);
                if (choices.ContainsKey(choiceId))
                {
                    caveChoice = choices[choiceId];
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            apiDGA = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            apiJA = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");


            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Reset Farm Cave Event",
                tooltip: () => "Will reset the cave the next time you load a save. REMOVES ALL OBJECTS IN CAVE",
                getValue: () => Config.ResetEvent,
                setValue: value => Config.ResetEvent = value
            );
        }


    }

}