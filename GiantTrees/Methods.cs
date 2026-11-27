using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;

namespace GiantTrees
{
	public partial class ModEntry : Mod
	{

        public static Vector2? GetMainTile(Vector2 tile, string str)
        {
            Vector2 main = tile;
            switch (str)
            {
                case "0":
                    break;
                case "1":
                    main += new Vector2(-1, 0);
                    break;
                case "2":
                    main += new Vector2(0, -1);
                    break;
                case "3":
                    main += new Vector2(-1, -1);
                    break;
                default:
                    return null;
            }
            return main;
        }
        public static Vector2[] GetTreeTiles(Vector2 tile, string str)
        {
            var main = GetMainTile(tile, str);
            return new Vector2[]
            {
                main.Value,
                main.Value + new Vector2(1, 0),
                main.Value + new Vector2(0, 1),
                main.Value + new Vector2(1, 1)
            };
        }
        public static Vector2[] GetOtherTreeTiles(Vector2 tile)
        {
            var main = tile;
            return new Vector2[]
            {
                main + new Vector2(1, 0),
                main + new Vector2(0, 1),
                main + new Vector2(1, 1)
            };
        }
        public static Tree GetMainTree(string str, Vector2 tile, GameLocation location)
        {
            if (location == null)
            {
                return null;
            }
            var main = GetMainTile(tile, str);
            if (main == null)
                return null;
            if (location.terrainFeatures.TryGetValue(main.Value, out var tf) && tf is Tree && tf.modData.ContainsKey(modKey))
                return tf as Tree;
            return null;
        }
        public static bool IsGiantTree(Tree tree, string str)
        {
            bool result = true;
            var mainTile = GetMainTile(tree.Tile, str);
            TerrainFeature dep = null;
            if (!tree.Location.terrainFeatures.TryGetValue(mainTile.Value, out dep) || !dep.modData.TryGetValue(modKey, out str) || str != "0")
            {
                if (dep != null)
                {
                    dep.modData.Remove(modKey);
                }
                result = false;
            }
            dep = null;
            if (!tree.Location.terrainFeatures.TryGetValue(mainTile.Value + new Vector2(1, 0), out dep) || !dep.modData.TryGetValue(modKey, out str) || str != "1")
            {
                if (dep != null)
                {
                    dep.modData.Remove(modKey);
                }
                result = false;
            }
            dep = null;
            if (!tree.Location.terrainFeatures.TryGetValue(mainTile.Value + new Vector2(0, 1), out dep) || !dep.modData.TryGetValue(modKey, out str) || str != "2")
            {
                if (dep != null)
                {
                    dep.modData.Remove(modKey);
                }
                result = false;
            }
            dep = null;
            if (!tree.Location.terrainFeatures.TryGetValue(mainTile.Value + new Vector2(1, 1), out dep) || !dep.modData.TryGetValue(modKey, out str) || str != "3")
            {
                if (dep != null)
                {
                    dep.modData.Remove(modKey);
                }
                result = false;
            }
            return result;
        }
        private static void CreateGiantTree(GameLocation location, Vector2[] array)
        {
            for(int i = 0; i < array.Length; i++)
            {
                //(location.terrainFeatures[array[i]] as Tree).growthStage.Value = 5;
                location.terrainFeatures[array[i]].modData[modKey] = i + "";
            }
        }
    }
}
