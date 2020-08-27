using Harmony;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

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

			//string test = "CustomFixedDialogueNPC.cs.4279^Oh... It's for my birthday? ... Thanks.$s/Oh... It's for my birthday? ... Thanks.$s^EndCustomFixedDialogueNPC.cs.4279";
			//DialoguePatches.FixString(new NPC() { Name = "Jas" }, ref test);
			//Monitor.Log($"test dialogue {test}");

			var harmony = HarmonyInstance.Create(ModManifest.UniqueID); 

			HarmonyMethod hm = new HarmonyMethod(typeof(DialoguePatches), "Dialogue_Prefix");
			hm.prioritiy = Priority.First;
			harmony.Patch(
				original: AccessTools.Constructor(typeof(Dialogue), new Type[] { typeof(string), typeof(NPC) }),
				prefix: hm
			);

			harmony.Patch(
				original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string) }),
				postfix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.LocalizedContentManager_LoadString_Postfix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(NPC), nameof(NPC.showTextAboveHead)),
				prefix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.NPC_showTextAboveHead_Prefix))
			);
			harmony.Patch(
				original: AccessTools.Method(typeof(NPC), nameof(NPC.getHi)),
				postfix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.NPC_getHi_Postfix))
			);
		}
	}
}
