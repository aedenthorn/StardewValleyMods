using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace FurnitureDisplayFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static IDynamicGameAssetsApi apiDGA;
        private static IJsonAssetsApi apiJA;
        public static Dictionary<string, FurnitureDisplayData> furnitureDisplayDict = new Dictionary<string, FurnitureDisplayData>();
        public static readonly string frameworkPath = "Mods/aedenthorn.FurnitureDisplayFramework/dictionary";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw), new Type[] {typeof(SpriteBatch)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(GameLocation_draw_Postfix))
            );
        }
        public override object GetApi()
        {
            return new FurnitureDisplayFrameworkAPI();
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
                name: () => "Place Key",
                getValue: () => Config.PlaceKey,
                setValue: value => Config.PlaceKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Take Key",
                getValue: () => Config.TakeKey,
                setValue: value => Config.TakeKey = value
            );

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            furnitureDisplayDict.Clear();
            var tempDisplayDict = Helper.Content.Load<Dictionary<string, FurnitureDisplayData>>(frameworkPath, ContentSource.GameContent);
            foreach(var kvp in tempDisplayDict)
            {
                if (kvp.Key.Contains(","))
                {
                    var names = kvp.Key.Split(',');
                    foreach(var name in names)
                    {
                        if (name.Contains(":"))
                        {
                            var parts = name.Split(':');
                            var rots = parts[1].Split(',');
                            foreach(var rot in rots)
                            {
                                furnitureDisplayDict.Add(parts[0]+":"+rot, kvp.Value);
                            }
                        }
                        else
                        {
                            furnitureDisplayDict.Add(name, kvp.Value);
                        }
                        Monitor.Log($"Loaded furniture display template for {name}");
                    }
                }
                else
                {
                    if (kvp.Key.Contains(":"))
                    {
                        var parts = kvp.Key.Split(':');
                        var rots = parts[1].Split(',');
                        foreach (var rot in rots)
                        {
                            furnitureDisplayDict.Add(parts[0] + ":" + rot, kvp.Value);
                        }
                    }
                    else
                    {
                        furnitureDisplayDict.Add(kvp.Key, kvp.Value);
                    }
                    Monitor.Log($"Loaded furniture display template for {kvp.Key}");
                }
            }
            Monitor.Log($"Loaded {furnitureDisplayDict.Count} furniture display templates");
        }
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || Game1.activeClickableMenu != null || !Context.IsWorldReady || furnitureDisplayDict.Count == 0)
                return;
            if(e.Button == Config.PlaceKey && Game1.player.CurrentItem is Object && !(Game1.player.CurrentItem as Object).bigCraftable.Value && Game1.player.CurrentItem.GetType().BaseType == typeof(Item))
            {
                foreach (var f in Game1.currentLocation.furniture)
                {
                    var name = f.rotations.Value > 1 ? f.Name + ":" + f.currentRotation.Value : f.Name;
                    if (furnitureDisplayDict.ContainsKey(name))
                    {
                        FurnitureDisplayData data = furnitureDisplayDict[name];
                        for(int i = 0; i < data.slots.Length; i++)
                        {
                            Rectangle slotRect = new Rectangle((int)(f.boundingBox.X + data.slots[i].slotRect.X * 4), (int)(f.boundingBox.Y + data.slots[i].slotRect.Y * 4), (int)(data.slots[i].slotRect.Width * 4), (int)(data.slots[i].slotRect.Height * 4));
                            //Monitor.Log($"Checking if {slotRect} contains {Game1.viewport.X + Game1.getOldMouseX()},{Game1.viewport.Y + Game1.getOldMouseY()}");
                            if (slotRect.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()))
                            {
                                var amount = Config.PlaceAllByDefault ? Game1.player.CurrentItem.Stack : 1;
                                //Monitor.Log($"Clicked on furniture display {name} slot {i}");
                                Monitor.Log($"Placing {Game1.player.CurrentItem.Name} x{amount} in slot {i}");
                                Game1.player.currentLocation.playSound("dwop");
                                if (f.modData.ContainsKey("aedenthorn.FurnitureDisplayFramework/" + i) && f.modData["aedenthorn.FurnitureDisplayFramework/" + i].Contains(","))
                                {
                                    var currentItem = f.modData["aedenthorn.FurnitureDisplayFramework/" + i].Split(',');
                                    Monitor.Log($"Slot has {currentItem[0]}x{currentItem[1]}");
                                    if ((Game1.player.CurrentItem.Name != currentItem[0] && Game1.player.CurrentItem.ParentSheetIndex.ToString() != currentItem[0]) || (Game1.player.CurrentItem as Object).Quality.ToString() != currentItem[2])
                                    {
                                        var obj = GetObjectFromID(currentItem[0], int.Parse(currentItem[1]), int.Parse(currentItem[2]));
                                        if (obj != null && !Game1.player.addItemToInventoryBool(obj, true))
                                        {
                                            Monitor.Log($"Switching with {Game1.player.CurrentItem.Name} x{amount}");
                                            f.modData["aedenthorn.FurnitureDisplayFramework/" + i] = $"{Game1.player.CurrentItem.ParentSheetIndex},{amount},{(Game1.player.CurrentItem as Object).Quality}";

                                            if (amount >= Game1.player.CurrentItem.Stack)
                                                Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                                            else
                                                Game1.player.CurrentItem.Stack -= amount;

                                            Helper.Input.Suppress(e.Button);
                                        }
                                    }
                                    else
                                    {
                                        int slotAmount = int.Parse(currentItem[1]);
                                        int newSlotAmount = Math.Min(Game1.player.CurrentItem.maximumStackSize(), slotAmount + amount);
                                        Monitor.Log($"Adding {Game1.player.CurrentItem.Name} x{newSlotAmount}");
                                        f.modData["aedenthorn.FurnitureDisplayFramework/" + i] = $"{currentItem[0]},{newSlotAmount},{currentItem[2]}";
                                        if (newSlotAmount < slotAmount + amount)
                                        {
                                            Game1.player.CurrentItem.Stack = slotAmount + amount - newSlotAmount;
                                        }
                                        else
                                        {
                                            if (amount >= Game1.player.CurrentItem.Stack)
                                                Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                                            else
                                                Game1.player.CurrentItem.Stack -= amount;
                                        }
                                        Helper.Input.Suppress(e.Button);
                                    }
                                }
                                else
                                {
                                    Monitor.Log($"Adding {Game1.player.CurrentItem.Name} x{amount}");
                                    f.modData["aedenthorn.FurnitureDisplayFramework/" + i] = $"{Game1.player.CurrentItem.ParentSheetIndex},{amount},{(Game1.player.CurrentItem as Object).Quality}";
                                    if (amount >= Game1.player.CurrentItem.Stack)
                                        Game1.player.removeItemFromInventory(Game1.player.CurrentItem);
                                    else
                                        Game1.player.CurrentItem.Stack -= amount;
                                    Helper.Input.Suppress(e.Button);
                                }
                                return;
                            }
                        }
                    }
                }
            }
            if(e.Button == Config.TakeKey)
            {
                foreach (var f in Game1.currentLocation.furniture)
                {
                    var name = f.rotations.Value > 1 ? f.Name + ":" + f.currentRotation.Value : f.Name;

                    if (furnitureDisplayDict.ContainsKey(name))
                    {
                        FurnitureDisplayData data = furnitureDisplayDict[name];
                        for (int i = 0; i < data.slots.Length; i++)
                        {
                            Rectangle slotRect = new Rectangle((int)(f.boundingBox.X + data.slots[i].slotRect.X * 4), (int)(f.boundingBox.Y + data.slots[i].slotRect.Y * 4), (int)(data.slots[i].slotRect.Width * 4), (int)(data.slots[i].slotRect.Height * 4));
                            if (slotRect.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()) && furnitureDisplayDict.ContainsKey(name) && f.modData.ContainsKey("aedenthorn.FurnitureDisplayFramework/" + i) && f.modData["aedenthorn.FurnitureDisplayFramework/" + i].Contains(","))
                            {
                                Game1.player.currentLocation.playSound("dwop");
                                var currentItem = f.modData["aedenthorn.FurnitureDisplayFramework/" + i].Split(',');
                                Monitor.Log($"Slot has {currentItem[0]}x{currentItem[1]}");
                                var obj = GetObjectFromID(currentItem[0], int.Parse(currentItem[1]), int.Parse(currentItem[2]));
                                if (obj != null)
                                {
                                    Game1.player.currentLocation.playSound("dwop");
                                    Monitor.Log($"Taking {obj.Name}x{obj.Stack}");
                                    f.modData.Remove("aedenthorn.FurnitureDisplayFramework/" + i);
                                    Game1.player.addItemToInventoryBool(obj, true);
                                    Helper.Input.Suppress(e.Button);
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }


        private static Object GetObjectFromID(string id, int amount, int quality)
        {
            if (int.TryParse(id, out int index))
            {
                //SMonitor.Log($"Spawning object with index {id}");
                return new Object(index, amount, false, -1, quality);
            }
            foreach (var kvp in Game1.objectInformation)
            {
                if (kvp.Value.StartsWith(id + "/"))
                    return new Object(kvp.Key, amount, false, -1, quality);
            }
            return null;
            /*
            //SMonitor.Log($"Trying to get object {id}, DGA {apiDGA != null}, JA {apiJA != null}");

            Object obj = null;
            try
            {

                if (int.TryParse(id, out int index))
                {
                    //SMonitor.Log($"Spawning object with index {id}");
                    return new Object(index, amount, false, -1, quality);
                }
                else
                {
                    var dict = SHelper.Content.Load<Dictionary<int, string>>("Data/ObjectInformation", ContentSource.GameContent);
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value.StartsWith(id + "/"))
                            return new Object(kvp.Key, amount, false, -1, quality);
                    }
                }
                if (apiDGA != null && id.Contains("/"))
                {
                    object o = apiDGA.SpawnDGAItem(id);
                    if (o is Object)
                    {
                        //SMonitor.Log($"Spawning DGA object {id}");
                        (o as Object).Stack = amount;
                        (o as Object).Quality = quality;
                        return (o as Object);
                    }
                }
                if (apiJA != null)
                {
                    int idx = apiJA.GetObjectId(id);
                    if (idx != -1)
                    {
                        //SMonitor.Log($"Spawning JA object {id}");
                        return new Object(idx, amount, false, -1, quality);

                    }
                }
            }
            catch (Exception ex)
            {
                //SMonitor.Log($"Exception: {ex}", LogLevel.Error);
            }
            //SMonitor.Log($"Couldn't find item with id {id}");
            return obj;
            */
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
            Monitor.Log("Loading furniture display list");

            return (T)(object)new Dictionary<string, FurnitureDisplayData>();
        }
    }

}