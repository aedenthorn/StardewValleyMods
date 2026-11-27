using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeLove
{
    public static class EventPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        public static bool startingLoadActors = false;

        public static bool Event_answerDialogueQuestion_Prefix(Event __instance, NPC who, string answerKey)
        {
            try
            {

                if (answerKey == "danceAsk" && !who.HasPartnerForDance && Game1.player.friendshipData[who.Name].IsMarried())
                {
                    Game1.player.dancePartner.Value = who;
                    Dialogue dialogue2;
                    if ((dialogue2 = who.TryGetDialogue("FlowerDance_Accept_" + (Game1.player.isRoommate(who.Name) ? "Roommate" : "Spouse"))) == null)
                    {
                        dialogue2 = who.TryGetDialogue("FlowerDance_Accept") ?? new Dialogue(who, "Strings\\StringsFromCSFiles:Event.cs.1632", false);
                    }
                    who.setNewDialogue(dialogue2, false, false);
                    using (List<NPC>.Enumerator enumerator = __instance.actors.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            NPC i = enumerator.Current;
                            Stack<Dialogue> currentDialogue = i.CurrentDialogue;
                            if (currentDialogue != null && currentDialogue.Count > 0 && i.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
                            {
                                i.CurrentDialogue.Clear();
                            }
                        }
                    }
                    Game1.drawDialogue(who);
                    who.immediateSpeak = true;
                    who.facePlayer(Game1.player);
                    who.Halt();
                    return false;
                }
            }

            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_answerDialogueQuestion_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        public static void Event_command_loadActors_Prefix()
        {
            try
            {
                startingLoadActors = true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_command_loadActors_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Event_command_loadActors_Postfix()
        {
            try
            {
                startingLoadActors = false;
                Game1Patches.lastGotCharacter = null;

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_command_loadActors_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}