using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityGrid
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private Vector2 lastCursorTile = new Vector2(-1,-1);

        public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            string location = Game1.player.currentLocation.NameOrUniqueName;

            if (e.Button == Config.ToggleGrid)
            {
                Helper.Input.Suppress(e.Button);
                ShowingGrid = !ShowingGrid;
                Monitor.Log($"Showing grid: {ShowingGrid}");
                EventHandler<KeyValuePair<GameLocation, int>> handler = null;
                if (ShowingGrid)
                {
                     handler = showEventHandler;
                }
                else
                {
                     handler = hideEventHandler;
                }
                if (handler != null)
                {
                    GameLocation gl = Game1.getLocationFromName(location);
                    if (gl == null)
                    {
                        gl = Game1.getLocationFromName(location, true);
                        if (gl == null)
                            return;
                    }
                    KeyValuePair<GameLocation, int> args = new KeyValuePair<GameLocation, int>(gl, (int)CurrentGrid);
                    handler(context, args);
                }

                return;
            }
            if (!ShowingGrid)
                return;
            if(e.Button == Config.ToggleEdit)
            {
                Helper.Input.Suppress(e.Button);
                ShowingEdit = !ShowingEdit;
                Monitor.Log($"Showing edit: {ShowingEdit}");
                return;
            }
            if (e.Button == Config.SwitchGrid)
            {
                Helper.Input.Suppress(e.Button);
                CurrentGrid = CurrentGrid == GridType.water ? GridType.electric : GridType.water;
                Monitor.Log($"Showing grid: {CurrentGrid}");
                return;
            }
            if (!ShowingEdit || Game1.player.CurrentItem is StardewValley.Object && (Game1.player.CurrentItem as StardewValley.Object).bigCraftable.Value)
                return;
            if (e.Button == Config.SwitchTile)
            {
                Helper.Input.Suppress(e.Button);
                CurrentTile++;
                CurrentTile %= 5;
                CurrentRotation = 0;
                //Monitor.Log($"Showing tile: {CurrentTile},{CurrentRotation}");
            }
            else if (e.Button == Config.RotateTile)
            {
                Helper.Input.Suppress(e.Button);
                CurrentRotation++;
                if (CurrentTile == 1)
                    CurrentRotation %= 2;
                else if (CurrentTile == 4)
                    CurrentRotation = 0;
                else
                    CurrentRotation %= 4;
                //Monitor.Log($"Showing tile: {CurrentTile},{CurrentRotation}");
            }
            else if (e.Button == Config.DestroyTile)
            {
                Helper.Input.Suppress(e.Button);
                Dictionary<Vector2, GridPipe> pipeDict = utilitySystemDict[location][CurrentGrid].pipes;
                if (!pipeDict.ContainsKey(Game1.lastCursorTile))
                {
                    Monitor.Log($"No tile to remove at {Game1.currentCursorTile}");
                    return;
                }
                Monitor.Log($"Removing tile at {Game1.currentCursorTile}");
                pipeDict.Remove(Game1.lastCursorTile);
                PipeDestroyed();
                if (Config.DestroySound?.Length > 0)
                {
                    Game1.player.currentLocation.playSound(Config.DestroySound);
                }
                RemakeGroups(location, CurrentGrid);
            }
        }
        public void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !ShowingGrid || Game1.activeClickableMenu != null)
                return;

            string location = Game1.player.currentLocation.NameOrUniqueName;

            if (!utilitySystemDict.ContainsKey(location))
            {
                RemakeAllGroups(location);
            }
            Dictionary<Vector2, GridPipe> pipeDict = utilitySystemDict[location][CurrentGrid].pipes;
            List<PipeGroup> groupList = utilitySystemDict[location][CurrentGrid].groups;
            Dictionary<Vector2, UtilityObjectInstance> objectDict = utilitySystemDict[location][CurrentGrid].objects;
            Color color = CurrentGrid == GridType.electric ? Config.ElectricityColor : Config.WaterColor;

            foreach (var group in groupList)
            {
                Vector2 power = GetGroupPower(location, group, CurrentGrid);
                Vector2 storedPower = GetGroupStoragePower(location, group, CurrentGrid);
                float netPower = power.X + power.Y;
                bool powered = power.X > 0 && netPower + storedPower.X >= 0;
                foreach (var pipe in group.pipes)
                {
                    if (Utility.isOnScreen(new Vector2(pipe.X * 64 + 32, pipe.Y * 64 + 32), 32))
                    {
                        if (pipeDict[pipe].index == 4)
                            DrawTile(e.SpriteBatch, pipe, new GridPipe() { index = 1, rotation = 2 }, powered ? color : Config.UnpoweredGridColor);
                        else
                            DrawTile(e.SpriteBatch, pipe, pipeDict[pipe], powered ? color : Config.UnpoweredGridColor);
                    }
                }

            }
            foreach (var kvp in objectDict)
            {
                DrawAmount(e.SpriteBatch, kvp, kvp.Value.CurrentPowerVector.X + kvp.Value.CurrentPowerVector.Y, color);
                DrawCharge(e.SpriteBatch, kvp, color);
            }
            if (ShowingEdit)
            {
                if (CurrentTile == 4)
                {
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile + new Vector2(-2, 2) / 64, new GridPipe() { index = 1, rotation = 2 }, Config.ShadowColor);
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile + new Vector2(-2,-2) / 64, new GridPipe() { index = 1, rotation = 2 }, Config.ShadowColor);
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile + new Vector2(2, -2) / 64, new GridPipe() { index = 1, rotation = 2 }, Config.ShadowColor);
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile + new Vector2(2, 2) / 64, new GridPipe() { index = 1, rotation = 2 }, Config.ShadowColor);
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile, new GridPipe() { index = 1, rotation = 2 }, color);
                }
                else if (CurrentTile != 5)
                {
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile + new Vector2(-2, 2) / 64, new GridPipe() { index = CurrentTile, rotation = CurrentRotation }, Config.ShadowColor);
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile + new Vector2(-2, -2) / 64, new GridPipe() { index = CurrentTile, rotation = CurrentRotation }, Config.ShadowColor);
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile + new Vector2(2, -2) / 64, new GridPipe() { index = CurrentTile, rotation = CurrentRotation }, Config.ShadowColor);
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile + new Vector2(2, 2) / 64, new GridPipe() { index = CurrentTile, rotation = CurrentRotation }, Config.ShadowColor);
                    DrawTile(e.SpriteBatch, Game1.currentCursorTile, new GridPipe() { index = CurrentTile, rotation = CurrentRotation }, color);
                }
                if (Helper.Input.IsDown(Config.PlaceTile) || Helper.Input.IsSuppressed(Config.PlaceTile))
                {
                    if(lastCursorTile != Game1.lastCursorTile)
                    {
                        Helper.Input.Suppress(Config.PlaceTile);
                        lastCursorTile = Game1.lastCursorTile;
                        if (pipeDict.ContainsKey(Game1.lastCursorTile))
                        {
                            if (!PayForPipe(true))
                                return;
                            Monitor.Log($"Removing tile at {Game1.currentCursorTile}");
                        }
                        else if (!PayForPipe(false))
                            return;

                        Monitor.Log($"Placing tile {CurrentTile},{CurrentRotation} at {Game1.currentCursorTile}");
                        pipeDict[Game1.lastCursorTile] = new GridPipe() { index = CurrentTile, rotation = CurrentRotation };
                        if (Config.PipeSound?.Length > 0)
                        {
                            Game1.player.currentLocation.playSound(Config.PipeSound);
                        }
                        RemakeGroups(location, CurrentGrid);
                    }
                }
                else
                {
                    lastCursorTile = new Vector2(-1,-1);
                }
            }
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            float elapsed = (e.NewTime / 100 - e.OldTime / 100) + (e.NewTime % 100 - e.OldTime % 100) / 60f;
            foreach (var key in utilitySystemDict.Keys)
            {
                var val = utilitySystemDict[key];
                for (int i = 0; i < val[GridType.electric].groups.Count; i++)
                {
                    val[GridType.electric].groups[i].powerVector = GetGroupPower(key, val[GridType.electric].groups[i], GridType.electric);
                    val[GridType.electric].groups[i].storageVector = GetGroupStoragePower(key, val[GridType.electric].groups[i], GridType.electric);
                }
                for (int i = 0; i < val[GridType.water].groups.Count; i++)
                {
                    val[GridType.water].groups[i].powerVector = GetGroupPower(key, val[GridType.water].groups[i], GridType.water);
                    val[GridType.water].groups[i].storageVector = GetGroupStoragePower(key, val[GridType.water].groups[i], GridType.water);
                }
                foreach (var group in val[GridType.electric].groups)
                {
                    ChangeStorageObjects(key, group, GridType.electric, elapsed);
                }
                foreach (var group in val[GridType.water].groups)
                {
                    ChangeStorageObjects(key, group, GridType.water, elapsed);
                }
            }
        }
        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            float elapsed = 6 - (Game1.timeOfDay / 100 + Game1.timeOfDay % 100 / 60f);
            if (elapsed <= 0)
                elapsed += 24;
            foreach (var key in utilitySystemDict.Keys)
            {
                var val = utilitySystemDict[key];
                for(int i = 0; i < val[GridType.electric].groups.Count; i++)
                {
                    val[GridType.electric].groups[i].powerVector = GetGroupPower(key, val[GridType.electric].groups[i], GridType.electric);
                    val[GridType.electric].groups[i].storageVector = GetGroupStoragePower(key, val[GridType.electric].groups[i], GridType.electric);
                }
                for (int i = 0; i < val[GridType.water].groups.Count; i++)
                {
                    val[GridType.water].groups[i].powerVector = GetGroupPower(key, val[GridType.water].groups[i], GridType.water);
                    val[GridType.water].groups[i].storageVector = GetGroupStoragePower(key, val[GridType.water].groups[i], GridType.water);
                }
                foreach (var group in val[GridType.electric].groups)
                {
                    ChangeStorageObjects(key, group, GridType.electric, elapsed);
                }
                foreach (var group in val[GridType.water].groups)
                {
                    ChangeStorageObjects(key, group, GridType.water, elapsed);
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            utilitySystemDict = new Dictionary<string, Dictionary<GridType, UtilitySystem>>();
            ShowingGrid = false;
            CurrentRotation = 0;
            CurrentTile = 0;
            try
            {
                utilityObjectDict = SHelper.Content.Load<Dictionary<string, UtilityObject>>(dictPath, ContentSource.GameContent);
                UtilitySystemDictData systemDataDict = Helper.Data.ReadSaveData<UtilitySystemDictData>(saveKey) ?? new UtilitySystemDictData();

                Monitor.Log($"reading grid data for {systemDataDict.dict.Count} locations from save");
                foreach (var kvp in systemDataDict.dict)
                {
                    Monitor.Log($"reading {kvp.Value.electricData.Count} ep and {kvp.Value.waterData.Count} wp for location {kvp.Key}");
                    utilitySystemDict[kvp.Key] = new Dictionary<GridType, UtilitySystem>();
                    utilitySystemDict[kvp.Key][GridType.electric] = new UtilitySystem();
                    utilitySystemDict[kvp.Key][GridType.water] = new UtilitySystem();
                    foreach (var arr in kvp.Value.waterData)
                    {
                        utilitySystemDict[kvp.Key][GridType.water].pipes[new Vector2(arr[0], arr[1])] = new GridPipe() { index = arr[2], rotation = arr[3] };
                    }
                    foreach (var arr in kvp.Value.electricData)
                    {
                        utilitySystemDict[kvp.Key][GridType.electric].pipes[new Vector2(arr[0], arr[1])] = new GridPipe() { index = arr[2], rotation = arr[3] };
                    }
                    RemakeAllGroups(kvp.Key);
                }
            }
            catch 
            {
                Monitor.Log("Invalid utility system in save file... starting fresh", LogLevel.Warn);
            }
        }
        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            UtilitySystemDictData dict = new UtilitySystemDictData();
            Monitor.Log($"writing grid data for {utilitySystemDict.Count} locations from save");
            foreach (var kvp in utilitySystemDict)
            {
                UtilitySystemData gridData = new UtilitySystemData(kvp.Value[GridType.water].pipes, kvp.Value[GridType.electric].pipes);
                dict.dict[kvp.Key] = gridData;
                Monitor.Log($"writing {gridData.electricData.Count} ep and {gridData.waterData.Count} wp for location {kvp.Key} to save");
            }
            Helper.Data.WriteSaveData(saveKey, dict);
        }
        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.IsLoaded("spacechase0.DynamicGameAssets"))
            {
                Monitor.Log($"patching DGA methods");
                harmony.Patch(
                   original: AccessTools.Method(Helper.ModRegistry.GetApi("spacechase0.DynamicGameAssets").GetType().Assembly.GetType("DynamicGameAssets.Game.CustomBigCraftable"), "minutesElapsed"),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_minutesElapsed_Prefix)),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Method_Postfix))
                );
                harmony.Patch(
                   original: AccessTools.Method(Helper.ModRegistry.GetApi("spacechase0.DynamicGameAssets").GetType().Assembly.GetType("DynamicGameAssets.Game.CustomObject"), "placementAction"),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_placementAction_Postfix))
                );
            }

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

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Options"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Pipe Create Sound",
                getValue: () => Config.PipeSound,
                setValue: value => Config.PipeSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Pipe Destroy Sound",
                getValue: () => Config.DestroySound,
                setValue: value => Config.DestroySound = value
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Pipe Costs"
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Pipe Gold Cost",
                getValue: () => Config.PipeCostGold,
                setValue: value => Config.PipeCostGold  = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Pipe Gold Destroy",
                getValue: () => Config.PipeDestroyGold,
                setValue: value => Config.PipeDestroyGold = value
            );
            
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Pipe Item Cost",
                tooltip: () => "Comma-separated pairs of index:amount",
                getValue: () => Config.PipeCostItems,
                setValue: value => Config.PipeCostItems = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Pipe Item Destroy",
                tooltip: () => "Comma-separated pairs of index:amount",
                getValue: () => Config.PipeDestroyItems,
                setValue: value => Config.PipeDestroyItems = value
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Keys"
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Toggle Grid Key",
                getValue: () => Config.ToggleGrid,
                setValue: value => Config.ToggleGrid = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch Grid Key",
                getValue: () => Config.SwitchGrid,
                setValue: value => Config.SwitchGrid = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Change Tile Key",
                getValue: () => Config.SwitchTile,
                setValue: value => Config.SwitchTile = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Rotate Tile Key",
                getValue: () => Config.RotateTile,
                setValue: value => Config.RotateTile = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Place Tile Key",
                getValue: () => Config.PlaceTile,
                setValue: value => Config.PlaceTile = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Destroy Tile Key",
                getValue: () => Config.DestroyTile,
                setValue: value => Config.DestroyTile = value
            );
        }
    }
}