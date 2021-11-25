using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace UtilityGrid
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {


        public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !(Game1.currentLocation is Farm) || !Game1.currentLocation.IsOutdoors || Game1.activeClickableMenu != null)
                return;

            if (e.Button == Config.ToggleGrid)
            {
                Helper.Input.Suppress(e.Button);
                ShowingGrid = !ShowingGrid;
                Monitor.Log($"Showing grid: {ShowingGrid}");
            }
            if (!ShowingGrid)
                return;
            if (e.Button == Config.SwitchGrid)
            {
                Helper.Input.Suppress(e.Button);
                CurrentGrid = CurrentGrid == GridType.water ? GridType.electric : GridType.water;
                Monitor.Log($"Showing grid: {CurrentGrid}");
            }
            else if (e.Button == Config.SwitchTile)
            {
                Helper.Input.Suppress(e.Button);
                CurrentTile++;
                CurrentTile %= 6;
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
            else if (e.Button == Config.PlaceTile)
            {
                Helper.Input.Suppress(e.Button);
                Dictionary<Vector2, GridPipe> pipeDict;
                if (CurrentGrid == GridType.electric)
                {
                    pipeDict = electricPipes;
                }
                else
                {
                    pipeDict = waterPipes;
                }
                if(CurrentTile == 5)
                {
                    Monitor.Log($"Removing tile at {Game1.currentCursorTile}");
                    pipeDict.Remove(Game1.lastCursorTile);
                }
                else
                {
                    Monitor.Log($"Placing tile {CurrentTile},{CurrentRotation} at {Game1.currentCursorTile}");
                    pipeDict[Game1.lastCursorTile] = new GridPipe() { index = CurrentTile, rotation = CurrentRotation };
                }
                RemakeGroups(CurrentGrid);
            }
        }
        public void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !ShowingGrid)
                return;

            if(!(Game1.currentLocation is Farm) || !Game1.currentLocation.IsOutdoors || Game1.activeClickableMenu != null)
            {
                ShowingGrid = false;
                return;
            }

            List<PipeGroup> groupList;
            Color color;
            Dictionary<Vector2, GridPipe> pipeDict;
            if (CurrentGrid == GridType.electric)
            {
                groupList = electricGroups;
                pipeDict = electricPipes;
                color = Config.ElectricityColor;
            }
            else
            {
                groupList = waterGroups;
                pipeDict = waterPipes;
                color = Config.WaterColor;
            }
            foreach (var group in groupList)
            {
                Vector2 power = GetGroupPower(group, CurrentGrid);
                float netPower = power.X + power.Y;
                bool powered = power.X > 0 && netPower >= 0;
                foreach (var pipe in group.pipes)
                {
                    if (Utility.isOnScreen(new Vector2(pipe.X * 64 + 32, pipe.Y * 64 + 32), 32))
                    {
                        if (pipe != Game1.currentCursorTile)
                        {
                            if (pipeDict[pipe].index == 4)
                                DrawTile(e.SpriteBatch, pipe, new GridPipe() { index = 1, rotation = 2 }, powered ? color : Color.White);
                            else
                                DrawTile(e.SpriteBatch, pipe, pipeDict[pipe], powered ? color : Color.White);
                        }
                    }
                }
                foreach (var kvp in group.objects)
                {
                    float objPower;
                    bool enough = netPower >= 0;
                    if (CurrentGrid == GridType.electric)
                    {
                        objPower = kvp.Value.electric;
                    }
                    else
                    {
                        objPower = kvp.Value.water;
                        if (kvp.Value.electric < 0 && GetTileNetElectricPower(kvp.Key) < 0)
                            enough = false;
                    }

                    if (objPower == 0)
                        continue;


                    string str = "" + Math.Round(objPower);

                    e.SpriteBatch.DrawString(Game1.dialogueFont, str, Game1.GlobalToLocal(Game1.viewport, kvp.Key * 64) - new Vector2(16, 16) + new Vector2(-2, 2), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.999999f);
                    e.SpriteBatch.DrawString(Game1.dialogueFont, str, Game1.GlobalToLocal(Game1.viewport, kvp.Key * 64) - new Vector2(16, 16), enough ? color : Config.InsufficientColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
                }

            }
            if (CurrentTile == 4)
                DrawTile(e.SpriteBatch, Game1.currentCursorTile, new GridPipe() { index = 1, rotation = 2 }, color);
            else if(CurrentTile != 5)
                DrawTile(e.SpriteBatch, Game1.currentCursorTile, new GridPipe() { index = CurrentTile, rotation = CurrentRotation }, color);
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.EnableMod || !(Game1.currentLocation is Farm))
                return;

            RemakeAllGroups();

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            waterPipes = new Dictionary<Vector2, GridPipe>();
            electricPipes = new Dictionary<Vector2, GridPipe>();
            ShowingGrid = false;
            CurrentRotation = 0;
            CurrentTile = 0;

            objectDict = SHelper.Content.Load<Dictionary<string, UtilityObject>>(dictPath, ContentSource.GameContent);
            UtilityGridData gridData = Helper.Data.ReadSaveData<UtilityGridData>(saveKey) ?? new UtilityGridData();
            Monitor.Log($"reading {gridData.electricData.Count} ep and {gridData.waterData.Count} wp from save");
            foreach (var arr in gridData.waterData)
            {
                waterPipes[new Vector2(arr[0], arr[1])] = new GridPipe() { index = arr[2], rotation = arr[3] };
            }
            foreach(var arr in gridData.electricData)
            {
                electricPipes[new Vector2(arr[0], arr[1])] = new GridPipe() { index = arr[2], rotation = arr[3] };
            }
            RemakeAllGroups();
        }
        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {

            UtilityGridData gridData = new UtilityGridData(waterPipes, electricPipes);
            Monitor.Log($"writing {gridData.electricData.Count} ep and {gridData.waterData.Count} wp to save");
            Helper.Data.WriteSaveData(saveKey, gridData);
        }
        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                name: () => "Toggle Grid Key",
                getValue: () => Config.ToggleGrid,
                setValue: value => Config.ToggleGrid = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch Grid Key",
                getValue: () => Config.ToggleGrid,
                setValue: value => Config.ToggleGrid = value
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
        }
    }
}