using HarmonyLib;
using StardewValley;

namespace WizardShop
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.ShowConstructOptions))]
        public static class GameLocation_performAction_Patch
        {
            public static bool Prefix(GameLocation __instance, ref string builder)
            {
                if (!Config.ModEnabled)
                    return true;
                if(builder == "WizardReal")
                {
                    builder = "Wizard";
                    return true;
                }
                if (builder != "Wizard")
                    return true;
                var responses = new Response[]
                {
                    new Response("Shop", SHelper.Translation.Get("shop")),
                    new Response("Build", SHelper.Translation.Get("buildings")),
                    new Response("Leave", SHelper.Translation.Get("leave"))
                };
                __instance.createQuestionDialogue("", responses, "WizardBook");
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
        public static class GameLocation_answerDialogueAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string questionAndAnswer)
            {
                if (!Config.ModEnabled || !questionAndAnswer.StartsWith("WizardBook_"))
                    return true;
                switch (questionAndAnswer)
                {
                    case "WizardBook_Shop":
                        if (!DataLoader.Shops(Game1.content).TryGetValue("Wizard", out var shop))
                        {
                            return false;
                        }
                        Utility.TryOpenShopMenu("Wizard", "Wizard", true);
                        break;
                    case "WizardBook_Build":
                        __instance.ShowConstructOptions("WizardReal");
                        break;
                    case "WizardBook_Leave":
                        break;
                }
                return false;
            }
        }
    }
}