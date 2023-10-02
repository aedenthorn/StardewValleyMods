using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AdvancedDialogueCommands
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Dialogue), nameof(Dialogue.checkForSpecialCharacters))]
        public class Dialogue_checkForSpecialCharacters_Patch
        {
            public static void Prefix(Dialogue __instance, string str, ref string __result)
            {
                if (!Config.ModEnabled)
                    return;
                str = str.Replace("%here", Game1.currentLocation.NameOrUniqueName);
                __result = str;
            }
        }
        [HarmonyPatch(typeof(Dialogue), nameof(Dialogue.getCurrentDialogue))]
        public class Dialogue_getCurrentDialogue_Patch
        {
            public static void Prefix(Dialogue __instance, bool ___finishedLastDialogue)
            {
                if (!Config.ModEnabled)
                    return;

                if (__instance.dialogues.Count == 0 || __instance.currentDialogueIndex >= __instance.dialogues.Count || ___finishedLastDialogue)
                {
                    return;
                }

                var currentDialogue = __instance.dialogues[__instance.currentDialogueIndex];

                Regex pattern = new Regex(@"&([A-Za-z]+)=([A-Za-z0-9,_]+)", RegexOptions.Compiled);
                Match m = pattern.Match(currentDialogue);
                while(m.Success)
                {
                    try
                    {
                        var split = m.Groups[2].Value.Split(',');
                        var key = m.Groups[1].Value;
                        switch (key)
                        {
                            case "sound":
                                if (split.Length > 1 && int.TryParse(split[1], out var pitch))
                                {
                                    Game1.playSoundPitched(split[0], pitch);
                                }
                                else
                                {
                                    Game1.playSound(split[0]);
                                }
                                break;
                            case "emoteNPC":
                                __instance.speaker.doEmote(Convert.ToInt32(split[0]));

                                break;
                            case "emote":
                                Game1.player.EndEmoteAnimation();
                                Game1.player.doEmote(Convert.ToInt32(split[0]));
                                break;
                            case "face":
                                Game1.player.faceDirection(Convert.ToInt32(split[0]));
                                break;
                            case "faceNPC":
                                __instance.speaker.faceDirection(Convert.ToInt32(split[0]));
                                break;
                            case "money":
                                Game1.player.addUnearnedMoney(Convert.ToInt32(split[0]));
                                break;
                            case "health":
                                if(split[0] == "max")
                                {
                                    Game1.player.health = Game1.player.maxHealth;
                                }
                                else
                                {
                                    Game1.player.health = Math.Min(Game1.player.health + Convert.ToInt32(split[0]), Game1.player.maxHealth);
                                }
                                break;
                            case "stamina":
                                if(split[0] == "max")
                                {
                                    Game1.player.stamina = Game1.player.MaxStamina;
                                }
                                else
                                {
                                    Game1.player.stamina = Math.Min(Game1.player.stamina + Convert.ToInt32(split[0]), Game1.player.MaxStamina);
                                }
                                break;
                        }
                    }
                    catch(Exception ex) 
                    {
                        SMonitor.Log($"Error parsing command {m.Value} in dialogue {currentDialogue}:\n\n{ex}", StardewModdingAPI.LogLevel.Warn);
                    }
                    currentDialogue = currentDialogue.Replace(m.Value, "");
                    m = pattern.Match(currentDialogue);
                }
                __instance.dialogues[__instance.currentDialogueIndex] = currentDialogue;
            }
        }
    }
}