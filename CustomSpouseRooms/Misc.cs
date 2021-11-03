using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace CustomSpouseRooms
{
    /// <summary>The mod entry point.</summary>
    public class Misc
    {
        private static Dictionary<string, int> topOfHeadOffsets = new Dictionary<string, int>();

        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static Dictionary<string, NPC> GetSpouses(Farmer farmer, int all)
        {
            Dictionary<string, NPC> spouses = new Dictionary<string, NPC>();
            if (all < 0)
            {
                NPC ospouse = farmer.getSpouse();
                if (ospouse != null)
                {
                    spouses.Add(ospouse.Name, ospouse);
                }
            }
            foreach (string friend in farmer.friendshipData.Keys)
            {
                if (Game1.getCharacterFromName(friend, true) != null && farmer.friendshipData[friend].IsMarried() && (all > 0 || friend != farmer.spouse))
                {
                    spouses.Add(friend, Game1.getCharacterFromName(friend, true));
                }
            }

            return spouses;
        }

        public static void ExtendMap(FarmHouse farmHouse, int w, int h)
        {
            List<Layer> layers = AccessTools.Field(typeof(Map), "m_layers").GetValue(farmHouse.map) as List<Layer>;
            for (int i = 0; i < layers.Count; i++)
            {
                Tile[,] tiles = AccessTools.Field(typeof(Layer), "m_tiles").GetValue(layers[i]) as Tile[,];
                Size size = (Size)AccessTools.Field(typeof(Layer), "m_layerSize").GetValue(layers[i]);
                if (size.Width >= w && size.Height >= h)
                    continue;

                ModEntry.PMonitor.Log($"Extending layer {layers[i].Id} from {size.Width},{size.Height} ({tiles.GetLength(0)},{tiles.GetLength(1)}) to {w},{h}");

                size = new Size(w, h);
                AccessTools.Field(typeof(Layer), "m_layerSize").SetValue(layers[i], size);
                AccessTools.Field(typeof(Map), "m_layers").SetValue(farmHouse.map, layers);

                Tile[,] newTiles = new Tile[w, h];

                for (int k = 0; k < tiles.GetLength(0); k++)
                {
                    for (int l = 0; l < tiles.GetLength(1); l++)
                    {
                        newTiles[k, l] = tiles[k, l];
                    }
                }
                AccessTools.Field(typeof(Layer), "m_tiles").SetValue(layers[i], newTiles);
                AccessTools.Field(typeof(Layer), "m_tileArray").SetValue(layers[i], new TileArray(layers[i], newTiles));

            }
            AccessTools.Field(typeof(Map), "m_layers").SetValue(farmHouse.map, layers);
        }
    }
}