using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FriendlyDivorce
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
	{

        public static ModConfig Config;

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();

			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
			ObjectPatches.Initialize(Monitor);
			harmony.Patch(
			   original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
			   prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Farmer_doDivorce_Prefix)),
			   postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Farmer_doDivorce_Postfix))
			);
		}
    }
}