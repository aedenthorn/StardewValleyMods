using Harmony;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Reflection;

namespace CustomFixedDialogue
{
    public class ModEntry : Mod
	{
		public static ModEntry context;

		internal static ModConfig Config;
		 

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();

			DialoguePatches.Initialize(Monitor, helper);

			var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

			harmony.Patch(
				original: AccessTools.Method(typeof(Dialogue), "parseDialogueString"),
				prefix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.Dialogue_parseDialogueString_Prefix))
			);

			harmony.Patch(
				original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string) }),
				postfix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.LocalizedContentManager_LoadString_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object) }),
				postfix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.LocalizedContentManager_LoadString_Postfix2))
			);
		}
	}
}
