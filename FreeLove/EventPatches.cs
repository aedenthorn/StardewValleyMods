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
                    string accept = "";
                    int gender = who.Gender;
                    if (gender != 0)
                    {
                        if (gender == 1)
                        {
                            accept = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1634");
                        }
                    }
                    else
                    {
                        accept = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1633");
                    }
                    try
                    {
                        Game1.player.changeFriendship(250, Game1.getCharacterFromName(who.Name, true));
                    }
                    catch
                    {
                    }
                    Game1.player.dancePartner.Value = who;
                    who.setNewDialogue(accept, false, false);
                    using (List<NPC>.Enumerator enumerator = __instance.actors.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            NPC j = enumerator.Current;
                            if (j.CurrentDialogue != null && j.CurrentDialogue.Count > 0 && j.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
                            {
                                j.CurrentDialogue.Clear();
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