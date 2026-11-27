using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace FurnitureRecolor
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static readonly string saveKey = "furniture-recolor-data";
        public static Dictionary<string, Dictionary<Vector2, RecolorData>> colorDict = new Dictionary<string, Dictionary<Vector2, RecolorData>>();

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

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.Saving += GameLoop_Saving;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Furniture_draw_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.performRemoveAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Furniture_performRemoveAction_Postfix))
            );
        }
        public static IEnumerable<CodeInstruction> Furniture_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Chest.updateWhenCurrentLocation");

            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            int found = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if (found < 4 && codes.Count > i + 1 && codes[i].opcode == OpCodes.Call && codes[i + 1].opcode == OpCodes.Ldarg_S && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Color), "White"))
                {
                    SMonitor.Log("Intercepting Color.White");
                    newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0, null)); // Furniture instance is Ldarg_0
                    newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchColor)))); // call with the previous IL code as argument
                    i++; // skip IL code we are replacing
                    found++;
                }
                newCodes.Add(codes[i]);
            }
            return newCodes.AsEnumerable();
        }
        public static void Furniture_performRemoveAction_Postfix(GameLocation environment)
        {
            if (colorDict.ContainsKey(environment.Name))
            {
                foreach (var k in colorDict[Game1.currentLocation.Name].Keys)
                {
                    bool found = false;
                    foreach (var f in Game1.currentLocation.furniture)
                    {
                        if (f.TileLocation == k)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        SMonitor.Log($"Removing old recolor at {k}");
                        colorDict[Game1.currentLocation.Name].Remove(k);
                    }
                }
            }
        }
        public static Color SwitchColor(Furniture f)
        {
            Vector2 pos = f.TileLocation;

            if (!Config.EnableMod || !colorDict.ContainsKey(Game1.currentLocation.Name) || !colorDict[Game1.currentLocation.Name].ContainsKey(pos))
            {
                return Color.White;
            }
            if(colorDict[Game1.currentLocation.Name][pos].name != f.Name)
            {
                colorDict[Game1.currentLocation.Name].Remove(pos);
                return Color.White;
            }
            return colorDict[Game1.currentLocation.Name][pos].color;
        }
        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady)
                return;
            if (Helper.Input.IsDown(Config.RedButton) || Helper.Input.IsSuppressed(Config.RedButton))
            {
                AdjustColor(1, 0, 0, Config.RedButton);
            }
            else if (Helper.Input.IsDown(Config.GreenButton) || Helper.Input.IsSuppressed(Config.GreenButton))
            {
                AdjustColor(0, 1, 0, Config.GreenButton);
            }
            else if (Helper.Input.IsDown(Config.BlueButton) || Helper.Input.IsSuppressed(Config.BlueButton))
            {
                AdjustColor(0, 0, 1, Config.BlueButton);
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady)
                return;
            if (e.Button == Config.RedButton)
            {
                AdjustColor(1, 0, 0, Config.RedButton);
            }
            else if (e.Button == Config.GreenButton)
            {
                AdjustColor(0, 1, 0, Config.GreenButton);
            }
            else if (e.Button == Config.BlueButton)
            {
                AdjustColor(0, 0, 1, Config.BlueButton);
            }
            else if (e.Button == Config.ResetButton)
            {
                AdjustColor(0, 0, 0, Config.ResetButton);
            }
        }

        private void ResetColor()
        {
            throw new NotImplementedException();
        }

        private void AdjustColor(int r, int g, int b, SButton button)
        {
            int mod = (Helper.Input.IsDown(Config.ModKey) ? -1 : 1);

            foreach (var f in Game1.currentLocation.furniture)
            {
                if (f.boundingBox.Value.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()))
                {
                    //Monitor.Log($"Adjusting color of {f.Name} by {r * mod},{b * mod},{g * mod}");
                    AdjustColor(f, r * mod, g * mod, b * mod);
                    Helper.Input.Suppress(button);
                    return;
                }
            }
            //Monitor.Log($"no wall furniture at {Game1.viewport.X + Game1.getOldMouseX()},{Game1.viewport.Y + Game1.getOldMouseY()}");
        }
        private void AdjustColor(Furniture f, int r, int g, int b)
        {

            Vector2 pos = f.TileLocation;
            if (!colorDict.ContainsKey(Game1.currentLocation.Name))
                colorDict[Game1.currentLocation.Name] = new Dictionary<Vector2, RecolorData>();
            if ((r == 0 && g == 0 && b == 0) || !colorDict[Game1.currentLocation.Name].ContainsKey(pos) || colorDict[Game1.currentLocation.Name][pos].name != f.Name)
            {
                colorDict[Game1.currentLocation.Name][pos] = new RecolorData()
                {
                    X = (int)pos.X,
                    Y = (int)pos.Y,
                    name = f.Name,
                    color = Color.White
                };
            }

            Color c = colorDict[Game1.currentLocation.Name][pos].color;

            if (r != 0)
            {
                if((c.R >= 255 && r > 0) || (c.R <= 0 && r < 0))
                {
                    c.G = (byte)Math.Clamp(c.G - r, 0, 255);
                    c.B = (byte)Math.Clamp(c.B - r, 0, 255);
                }
                else
                    c.R = (byte)Math.Clamp(c.R + r, 0, 255);
            }
            else if (g != 0)
            {
                if ((c.G >= 255 && g > 0) || (c.G <= 0 && g < 0))
                {
                    c.R = (byte)Math.Clamp(c.R - g, 0, 255);
                    c.B = (byte)Math.Clamp(c.B - g, 0, 255);
                }
                else
                    c.G = (byte)Math.Clamp(c.G + g, 0, 255);
            }
            else if (b != 0)
            {
                if ((c.B >= 255 && b > 0) || (c.B <= 0 && b < 0))
                {
                    c.R = (byte)Math.Clamp(c.R - b, 0, 255);
                    c.G = (byte)Math.Clamp(c.G - b, 0, 255);
                }
                else
                    c.B = (byte)Math.Clamp(c.B + b, 0, 255);
            }
            colorDict[Game1.currentLocation.Name][pos].color = c;
            //Monitor.Log($"New tint for {f.Name}: {c}");
        }

        private byte ModifyByte(byte r1, int r2)
        {
            throw new NotImplementedException();
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
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Red Button",
                getValue: () => Config.RedButton,
                setValue: value => Config.RedButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Green Button",
                getValue: () => Config.GreenButton,
                setValue: value => Config.GreenButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Blue Button",
                getValue: () => Config.BlueButton,
                setValue: value => Config.BlueButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Reset Button",
                getValue: () => Config.ResetButton,
                setValue: value => Config.ResetButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Key",
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
        }
        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            colorDict.Clear();
            try
            {
                ColorDictData colorDictData = Helper.Data.ReadSaveData<ColorDictData>(saveKey) ?? new ColorDictData();

                Monitor.Log($"reading recolor data for {colorDictData.colorDict.Count} locations from save");
                foreach (var kvp in colorDictData.colorDict)
                {
                    colorDict[kvp.Key] = new Dictionary<Vector2, RecolorData>();
                    Monitor.Log($"reading {kvp.Value.Count} recolors for location {kvp.Key}");
                    foreach(var kvp2 in kvp.Value)
                    {
                        colorDict[kvp.Key][new Vector2(kvp2.X, kvp2.Y)] = kvp2;
                    }
                }
            }
            catch(Exception ex)
            {
                Monitor.Log($"Invalid color dict in save file... starting fresh. \n\n{ex}", LogLevel.Warn);
            }
        }
        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            ColorDictData dict = new ColorDictData();
            Monitor.Log($"writing recolor data for {colorDict.Count} locations from save");
            foreach (var kvp in colorDict)
            {
                Monitor.Log($"writing {kvp.Value.Count} recolors for location {kvp.Key} to save");
                dict.colorDict[kvp.Key] = new List<RecolorData>();
                foreach(var kvp2 in kvp.Value)
                {
                    dict.colorDict[kvp.Key].Add(kvp2.Value);
                }
            }
            Helper.Data.WriteSaveData(saveKey, dict);
        }
    }
}