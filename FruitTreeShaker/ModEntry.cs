using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace FruitTreeShaker
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ModConfig Config;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            helper.Events.GameLoop.DayStarted += this.DayStarted;
        }

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (GameLocation gl in Game1.locations)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> keyValuePair in gl.terrainFeatures.Pairs)
                {
                    FruitTree fruitTree;
                    Tree tree;
                    if ((fruitTree = (keyValuePair.Value as FruitTree)) != null && fruitTree.fruitsOnTree > 0)
                    {
                        fruitTree.shake(keyValuePair.Key, false, gl);
                    }
                    else if ((tree = (keyValuePair.Value as Tree)) != null && tree.hasSeed && ((Config.ShakePalmTrees && tree.treeType == 6) || (Config.ShakeNormalTrees && tree.treeType != 6)))
                    {
                        Helper.Reflection.GetMethod(tree, "shake").Invoke(keyValuePair.Key, false, gl);
                    }
                }
            }
        }
    }
}