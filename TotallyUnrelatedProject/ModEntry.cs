using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace CustomFixedDialogue 
{
    public class ModEntry : Mod
    {

        public override void Entry(IModHelper helper)
        {
            DialoguePatches.Initialize(Monitor, helper);

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Constructor(typeof(Dialogue), new Type[] { typeof(string), typeof(NPC) }),
                prefix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.Dialogue_Prefix))
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
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.getTermOfSpousalEndearment)),
                postfix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.NPC_getTermOfSpousalEndearment_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Summit), nameof(Summit.GetSummitDialogue)),
                postfix: new HarmonyMethod(typeof(DialoguePatches), nameof(DialoguePatches.GetSummitDialogue_Patch))
            );

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            return;
            if (e.Button == SButton.F2)
            {
                var person = Game1.getCharacterFromName("Marnie");

                Game1.warpCharacter(person, Game1.player.currentLocation, Game1.player.getTileLocation() + new Microsoft.Xna.Framework.Vector2(0, 1));
                Friendship friends;
                int heartLevel = Game1.player.friendshipData.TryGetValue(person.Name, out friends) ? (friends.Points / 250) : 0;
                Stack<Dialogue> currentDialogue = new Stack<Dialogue>();
                Random r = new Random((int)(Game1.stats.DaysPlayed * 77U + (uint)((int)Game1.uniqueIDForThisGame / 2) + 2U + (uint)((int)person.DefaultPosition.X * 77) + (uint)((int)person.DefaultPosition.Y * 777)));
                Dictionary<string, string> npcDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
                npcDispositions.TryGetValue(person.Name, out string disposition);
                string[] relatives = disposition.Split('/', StringSplitOptions.None)[9].Split(' ', StringSplitOptions.None);
                int index = r.Next(relatives.Length / 2) * 2;
                string relativeName = relatives[index];
                string relativeDisplayName = relativeName;
                if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en && Game1.getCharacterFromName(relativeName, true, false) != null)
                {
                    relativeDisplayName = Game1.getCharacterFromName(relativeName, true, false).displayName;
                }
                string relativeTitle = relatives[index + 1].Replace("'", "").Replace("_", " ");
                string relativeProps;
                bool relativeIsMale = npcDispositions.TryGetValue(relativeName, out relativeProps) && relativeProps.Split('/', StringSplitOptions.None)[4].Equals("male");
                string nameAndTitle = (relativeTitle.Length > 2 && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ja) ? (relativeIsMale ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4079", new object[]
                {
                                relativeTitle
                }) : Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4080", new object[]
                {
                                relativeTitle
                })) : relativeDisplayName;
                string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", nameAndTitle);

                person.CurrentDialogue.Clear();
                person.setNewDialogue(message, true, false);
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