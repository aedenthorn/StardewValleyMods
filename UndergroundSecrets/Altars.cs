using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using static StardewValley.Network.NetAudio;

namespace UndergroundSecrets
{
    internal class Altars
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int cornerY = 15;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        internal static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            if (Game1.random.NextDouble() >= config.AltarBaseChance * Math.Pow(shaft.mineLevel, config.PuzzleChanceIncreaseRate) || superClearCenters.Count == 0)
                return;

            monitor.Log($"adding an altar");

            Vector2 spot = superClearCenters[Game1.random.Next(0, superClearCenters.Count)];

            Layer front = shaft.map.GetLayer("Front");
            Layer buildings = shaft.map.GetLayer("Buildings");
            if (shaft.map.TileSheets.FirstOrDefault(s => s.Id == ModEntry.tileSheetId) == null)
                shaft.map.AddTileSheet(new TileSheet(ModEntry.tileSheetId, shaft.map, ModEntry.tileSheetPath, new Size(16, 18), new Size(16, 16)));
            TileSheet tilesheet = shaft.map.GetTileSheet(ModEntry.tileSheetId);

            int type = Game1.random.Next(0, 3);

            front.Tiles[(int)spot.X - 1, (int)spot.Y - 1] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 128 + type * 3);
            front.Tiles[(int)spot.X, (int)spot.Y - 1] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 129 + type * 3);
            front.Tiles[(int)spot.X + 1, (int)spot.Y - 1] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 130 + type * 3);
            front.Tiles[(int)spot.X - 1, (int)spot.Y] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 144 + type * 3);
            front.Tiles[(int)spot.X, (int)spot.Y] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 145 + type * 3);
            front.Tiles[(int)spot.X + 1, (int)spot.Y] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 146 + type * 3);
            buildings.Tiles[(int)spot.X - 1, (int)spot.Y + 1] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: 160 + type * 3);
            buildings.Tiles[(int)spot.X, (int)spot.Y + 1] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: 161 + type * 3);
            buildings.Tiles[(int)spot.X + 1, (int)spot.Y + 1] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: 162 + type * 3);

            shaft.setTileProperty((int)spot.X, (int)spot.Y + 1, "Buildings", "Action", $"undergroundAltar_{type}_{spot.X}_{spot.Y + 1}");
            
            foreach (Vector2 v in Utils.GetSurroundingTiles(spot, 4))
            {
                superClearCenters.Remove(v);
                if (Math.Abs(v.X - spot.X) < 3 && Math.Abs(v.Y - spot.Y) < 3)
                {
                    clearCenters.Remove(v);
                    if (Math.Abs(v.X - spot.X) < 2 && Math.Abs(v.Y - spot.Y) < 2)
                        clearSpots.Remove(v);
                }
            }
        }

        internal static void OfferObject(MineShaft shaft, string action, Location tileLocation, Farmer who)
        {
            string[] parts = action.Split('_').Skip(1).ToArray();

            int type = int.Parse(parts[0]);
            int cx = int.Parse(parts[1]);
            int cy = int.Parse(parts[2]);

            if (who.ActiveObject == null)
            {
                Game1.activeClickableMenu = new DialogueBox(helper.Translation.Get($"altar-explain-{type}"));
                return;
            }

            int value = who.ActiveObject.salePrice();
            who.reduceActiveItemByOne();
            if (value < 10)
            {
                if (type == 0)
                {
                    CollapsingFloors.collapseFloor(shaft, who.getTileLocation());
                    return;
                }
                else if (type == 1)
                {
                    Traps.TriggerRandomTrap(shaft, who.getTileLocation(), false);
                    return;
                }
            }

            string sound = "yoba";
            if(type == 0)
            {
                sound = "grunt";
            }
            else if(type == 1)
            {
                sound = "debuffSpell";
            }
            shaft.playSound(sound, SoundContext.Default);

            BuffsDisplay buffsDisplay = Game1.buffsDisplay;
            Buff buff2 = GetBuff(value, who, shaft, type);
            buffsDisplay.addOtherBuff(buff2);
        }

        private static Buff GetBuff(int value, Farmer who, MineShaft shaft, int type)
        {
            int buffAmount = (int)(Math.Sqrt(value / 10f + who.LuckLevel + who.DailyLuck) * config.AltarBuffMult);
            int which = Game1.random.Next(10);
            int[] buffs = new int[10];
            for (int i = 0; i < 10; i++)
            {
                buffs[i] = which == i ? buffAmount : 0;

            }
            int buffDuration = value + shaft.mineLevel;
            Buff buff = new Buff(buffs[0], buffs[1], buffs[2], buffs[3], buffs[4], buffs[5], buffs[6], 0, 0, buffs[7], buffs[8], buffs[9], buffDuration, $"Altar{type}", helper.Translation.Get($"altar-type-{type}"));
            buff.which = 424200 + which;
            monitor.Log($"buff: {which}, amount: {buffAmount}, duration: {buffDuration}");
            return buff;
        }
    }
}