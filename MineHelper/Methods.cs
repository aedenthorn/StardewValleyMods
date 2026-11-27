using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Linq;

namespace MineHelper
{
    public partial class ModEntry
    {
        public static Chest openingChest;
        public static bool OpenChest(GameLocation loc)
        {
            var chest = loc.Objects.Pairs.FirstOrDefault(kvp => kvp.Value.modData.ContainsKey(chestKey)).Value as Chest;
            openingChest = chest;
            if (chest == null)
                return false;
            if (chest.synchronized.Value)
            {
                chest.GetMutex().RequestLock(new Action(ShowMenu), null);
            }
            else
            {
                ShowMenu();
            }
            return true;
        }

        private static void ShowMenu()
        {
            if(openingChest != null)
            {
                Game1.playSound(openingChest.fridge.Value ? "doorCreak" : "openChest", null);

                openingChest.ShowMenu();
            }
        }


        private void CheckChest(string name)
        {
            var loc = Game1.getLocationFromName(name);
            if(loc == null) 
                return;
            var kvp = loc.Objects.Pairs.FirstOrDefault(kvp => kvp.Value.modData.ContainsKey(chestKey));
            if (kvp.Value is Chest chest)
            {
                if (chest.ItemId != Config.ChestType)
                {
                    SMonitor.Log($"Updating chest in {loc.Name} at {kvp.Key} to type {Config.ChestType}");
                    var newChest = new Chest(true, kvp.Key, Config.ChestType);
                    if (chest.Items.Any(i => i is not null))
                    {
                        newChest.Items.AddRange(chest.Items);
                    }
                    newChest.modData[chestKey] = "true";
                    loc.Objects[kvp.Key] = newChest;
                }
                return;
            }
            var tiles = loc.map.RequireLayer("Buildings").Tiles.Array;
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] != null && tiles[x, y].Properties.TryGetValue("Action", out var str) && str == "MineElevator")
                    {
                        SMonitor.Log($"Found elevator at {x}, {y}");
                        var tile = new Vector2(x, y - 1);
                        var newChest = new Chest(true, tile, Config.ChestType);
                        newChest.modData[chestKey] = "true";
                        loc.Objects[tile] = newChest;
                    }
                }
            }
        }
    }
}