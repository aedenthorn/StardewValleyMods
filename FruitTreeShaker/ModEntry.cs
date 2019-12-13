using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace FruitTreeShaker
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			helper.Events.GameLoop.DayStarted += this.DayStarted;
		}

		private void DayStarted(object sender, DayStartedEventArgs e)
		{
			foreach (GameLocation gl in Game1.locations)
			{
				if (gl.GetType() != typeof(Farm) && !gl.IsGreenhouse)
					continue;
				List<Vector2> list = new List<Vector2>();
				foreach (KeyValuePair<Vector2, TerrainFeature> keyValuePair in gl.terrainFeatures.Pairs)
				{
					Vector2 key = keyValuePair.Key;
					FruitTree tree;
					if ((tree = (keyValuePair.Value as FruitTree)) != null && tree.fruitsOnTree > 0)
					{
						tree.shake(keyValuePair.Key,false,gl);
					}
				}
			}
		}
	}
}