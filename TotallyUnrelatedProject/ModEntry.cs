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
				original: AccessTools.Method(typeof(Dialogue), nameof(Dialogue.convertToDwarvish)),
				prefix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.convertToDwarvish_Prefix))
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

            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;


		}

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
			string text = "I was just thinking about the last %year years...\"/pause 1000/speak spouse \"We've been through a lot together, haven't we?$h";
			DialoguePatches.AddWrapperToString("Data\\ExtraDialogue:SummitEvent_Dialogue2_Spouse", ref text);
			Monitor.Log($"prefixed: {text}");
			DialoguePatches.FixString(Game1.getCharacterFromName("Shane"), ref text);
			Monitor.Log($"fixed: {text}");
		}
	}
}