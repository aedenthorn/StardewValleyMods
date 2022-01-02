using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;

namespace FarmCaveFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetEditor, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
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

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
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
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/Events/Farm"))
            {
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/Events/Farm"))
            {

                var editor = asset.AsDictionary<string, string>();
                string[] v = editor.Data["65/m 25000/t 600 1200/H"].Split('"');
                v[1] = Helper.Translation.Get("event-intro");
                Dictionary<string, CaveChoice> choices = Helper.Content.Load<Dictionary<string, CaveChoice>>(frameworkPath, ContentSource.GameContent);
                foreach(var cc in choices)
                {
                    if(cc.Value.description != null)
                        v[1] += "#$b#" + cc.Value.description;
                }
                editor.Data["65/m 25000/t 600 1200/H"] = string.Join("\"", v);
            }
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(frameworkPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
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
                                index = 296,
                                weight = 450
                            },
                            new CaveResource()
                            {
                                index = 396,
                                weight = 450
                            },
                            new CaveResource()
                            {
                                index = 406,
                                weight = 450
                            },
                            new CaveResource()
                            {
                                index = 410,
                                weight = 450
                            },
                            new CaveResource()
                            {
                                index = 613,
                                weight = 45
                            },
                            new CaveResource()
                            {
                                index = 634,
                                weight = 81
                            },
                            new CaveResource()
                            {
                                index = 635,
                                weight = 81
                            },
                            new CaveResource()
                            {
                                index = 636,
                                weight = 81
                            },
                            new CaveResource()
                            {
                                index = 637,
                                weight = 81
                            },
                            new CaveResource()
                            {
                                index = 638,
                                weight = 81
                            },
                        }
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
                                index = 128
                            },
                            new CaveObject()
                            {
                                X = 6,
                                Y = 5,
                                index = 128
                            },
                            new CaveObject()
                            {
                                X = 8,
                                Y = 5,
                                index = 128
                            },
                            new CaveObject()
                            {
                                X = 4,
                                Y = 7,
                                index = 128
                            },
                            new CaveObject()
                            {
                                X = 6,
                                Y = 7,
                                index = 128
                            },
                            new CaveObject()
                            {
                                X = 8,
                                Y = 7,
                                index = 128
                            }
                        }
                    }
                }
            };
            return (T)(object)choices;
        }
    }

}