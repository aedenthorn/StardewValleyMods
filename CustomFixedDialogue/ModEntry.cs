using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;

namespace CustomFixedDialogue
{
    public partial class ModEntry : Mod
    {
        private static IMonitor SMonitor;
        private static IModHelper SHelper;

        private static string spaces = "                                                                                                                                                                                                                                                                                                            ";
        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = Helper;
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Constructor(typeof(Dialogue), new Type[] { typeof(string), typeof(NPC) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Dialogue_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Constructor(typeof(DialogueBox), new Type[] { typeof(Dialogue) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Dialogue_Box_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Dialogue), nameof(Dialogue.convertToDwarvish)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.convertToDwarvish_Prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object), typeof(object), typeof(object) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Prefix3)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Postfix3))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object), typeof(object) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Prefix2)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Postfix2))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Prefix1)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Postfix1))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string) }),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.LoadStringByGender), new Type[] { typeof(int),  typeof(string) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_LoadStringByGender_Prefix1)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_LoadStringByGender_Postfix1))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.LoadStringByGender), new Type[] { typeof(int),  typeof(string), typeof(object[]) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_LoadStringByGender_Prefix2)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_LoadStringByGender_Postfix2))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.showTextAboveHead)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_showTextAboveHead_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.getHi)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_getHi_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.getTermOfSpousalEndearment)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_getTermOfSpousalEndearment_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Summit), nameof(Summit.GetSummitDialogue)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GetSummitDialogue_Patch))
            );

            //helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.NumLock)
			{
				var person = Game1.getCharacterFromName("Maru");
                Game1.warpCharacter(person, Game1.player.currentLocation, Game1.player.getTileLocation() + new Microsoft.Xna.Framework.Vector2(0, 1));
                
                string relativeTitle = "father";

                string nameAndTitle = Game1.LoadStringByGender(0, "Strings\\StringsFromCSFiles:NPC.cs.4079", new object[]
                {
                    relativeTitle
                });

                var message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4100", nameAndTitle, "Purple Shorts") + Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4126") + "%revealtaste" + "Demetrius0";
                try
                {
                    message = message.Substring(0, 1).ToUpper() + message.Substring(1, message.Length - 1);
                }
                catch (Exception)
                {
                }
                Game1.drawDialogue(person, message);
                return;
            }
            if (e.Button == SButton.F3)
            {
                var person = Game1.getCharacterFromName("Marnie");
                person.sayHiTo(Game1.getCharacterFromName("Lewis"));
                return;
            }
        }
    }
}