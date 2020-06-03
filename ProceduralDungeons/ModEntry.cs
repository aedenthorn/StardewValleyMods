using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using xTile;

namespace ProceduralDungeons
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetEditor
	{

		public static ModEntry context;

		internal static ModConfig Config;
		/// <summary>Get whether this instance can edit the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanEdit<T>(IAssetInfo asset)
		{
			string assetName = asset.AssetName;
			Regex rgx = new Regex(@"^Maps/Mines/[0-9]{1,2}$");

			if (rgx.IsMatch(assetName))
			{
				return true;
			}

			return false;
		}

		/// <summary>Edit a matched asset.</summary>
		/// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
		public void Edit<T>(IAssetData asset)
		{
			IAssetDataForMap mapAsset = asset.AsMap();
			Map map = mapAsset.Data;

		}


		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			context = this;
			Config = this.Helper.ReadConfig<ModConfig>();
		}


	}

}