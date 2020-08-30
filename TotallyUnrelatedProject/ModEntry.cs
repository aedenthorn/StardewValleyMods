using Harmony;
using StardewModdingAPI;
using StardewValley;
using System;

namespace CustomFixedDialogue 
{
    public class ModEntry : Mod
    {

        public override void Entry(IModHelper helper)
        {
			DialoguePatches.Initialize(Monitor, helper);

			var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

			HarmonyMethod hm = new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.Dialogue_Prefix));
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

        private static void Farmer_isMarried1()
        {
        }
        private static void Farmer_isMarried2()
        {
        }
    }
}