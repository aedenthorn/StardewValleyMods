using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {

        private Vector2 GetTileFromName(string name)
        {
            var split = name.Split('_');
            return new Vector2(int.Parse(split[1]), int.Parse(split[2]));
        }

        private void WarpToOpenWorldTile(float x, float y, Vector2 newPosition)
        {
            Game1.locationRequest = Game1.getLocationRequest($"{tilePrefix}_{x}_{y}", false);

            GameLocation previousLocation = Game1.player.currentLocation;
            Multiplayer mp = AccessTools.FieldRefAccess<Game1, Multiplayer>(Game1.game1, "multiplayer");
            if (Game1.emoteMenu != null)
            {
                Game1.emoteMenu.exitThisMenuNoSound();
            }
            if (Game1.client != null && Game1.currentLocation != null)
            {
                Game1.currentLocation.StoreCachedMultiplayerMap(mp.cachedMultiplayerMaps);
            }
            Game1.currentLocation.cleanupBeforePlayerExit();
            mp.broadcastLocationDelta(Game1.currentLocation);
            bool hasResetLocation = false;
            Game1.displayFarmer = true;
            
            Game1.player.Position = newPosition;
            
            Game1.currentLocation = Game1.locationRequest.Location;
            if (!Game1.IsClient)
            {
                Game1.locationRequest.Loaded(Game1.locationRequest.Location);
                Game1.currentLocation.resetForPlayerEntry();
                hasResetLocation = true;
            }
            Game1.currentLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);
            if (!Game1.viewportFreeze && Game1.currentLocation.Map.DisplayWidth <= Game1.viewport.Width)
            {
                Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
            }
            if (!Game1.viewportFreeze && Game1.currentLocation.Map.DisplayHeight <= Game1.viewport.Height)
            {
                Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
            }
            Game1.checkForRunButton(Game1.GetKeyboardState(), true);
            Game1.player.FarmerSprite.PauseForSingleAnimation = false;
            if (Game1.player.ActiveObject != null)
            {
                Game1.player.showCarrying();
            }
            else
            {
                Game1.player.showNotCarrying();
            }
            if (Game1.IsClient)
            {
                if (Game1.locationRequest.Location != null && Game1.locationRequest.Location.Root.Value != null && mp.isActiveLocation(Game1.locationRequest.Location))
                {
                    Game1.currentLocation = Game1.locationRequest.Location;
                    Game1.locationRequest.Loaded(Game1.locationRequest.Location);
                    if (!hasResetLocation)
                    {
                        Game1.currentLocation.resetForPlayerEntry();
                    }
                    Game1.player.currentLocation = Game1.currentLocation;
                    Game1.locationRequest.Warped(Game1.currentLocation);
                    Game1.currentLocation.updateSeasonalTileSheets(null);
                    if (Game1.IsDebrisWeatherHere(null))
                    {
                        Game1.populateDebrisWeatherArray();
                    }
                    Game1.warpingForForcedRemoteEvent = false;
                    Game1.locationRequest = null;
                }
                else
                {
                    Game1.requestLocationInfoFromServer();
                }
            }
            else
            {
                Game1.player.currentLocation = Game1.locationRequest.Location;
                Game1.locationRequest.Warped(Game1.locationRequest.Location);
                Game1.locationRequest = null;
            }
            ReloadOpenWorldTiles();
        }

        private void ReloadOpenWorldTiles()
        {
            List<string> tileNames = new List<string>();
            foreach(Farmer f in Game1.getAllFarmers())
            {
                if (!f.currentLocation.Name.StartsWith(tilePrefix))
                    continue;
                var t = GetTileFromName(f.currentLocation.Name);
                if (!tileNames.Contains($"{tilePrefix}_{t.X}_{t.Y}"))
                    tileNames.Add($"{tilePrefix}_{t.X}_{t.Y}");
                var ts = Utility.getSurroundingTileLocationsArray(t);
                foreach(var v in ts)
                {
                    if (!tileNames.Contains($"{tilePrefix}_{v.X}_{v.Y}"))
                        tileNames.Add($"{tilePrefix}_{v.X}_{v.Y}");
                }
            }
            for (int i = Game1.locations.Count; i >= 0; i--)
            {
                if (!Game1.locations[i].Name.StartsWith(tilePrefix))
                    continue;
                if (!tileNames.Contains(Game1.locations[i].Name))
                {
                    Game1.locations.RemoveAt(i);
                }
                else
                {
                    tileNames.Remove(Game1.locations[i].Name);
                }
            }
            foreach(var name in tileNames)
            {
                Game1.locations.Add(new GameLocation(SHelper.ModContent.GetInternalAssetName("assets/StardewOpenWorldTile.tmx").BaseName, name));
            }
        }
    }
}