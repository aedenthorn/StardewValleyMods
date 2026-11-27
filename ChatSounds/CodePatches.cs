using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Input;
using Object = StardewValley.Object;

namespace ChatSounds
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(ChatBox), "runCommand")]
        public class ChatBox_runCommand_Patch
        {
            public static bool Prefix(ChatBox __instance, string command)
            {
                if (!Config.EnableMod)
                    return true;

                if (command.ToLower().StartsWith("sound "))
                {
                    var split = command.Split(' ');
                    var sound = split[1];
                    int pitch = 0;
                    if (split.Length > 2)
                    {
                        int.TryParse(split[2], out pitch);
                    }
                    try
                    {
                        if (Config.ProximityBased)
                        {
                            Game1.player.currentLocation.playSoundAt(sound, Game1.player.getTileLocation());
                        }
                        else if (pitch > 0)
                        {
                            Game1.player.currentLocation.playSoundPitched(sound, pitch);
                        }
                        else
                        {
                            Game1.player.currentLocation.playSound(sound);
                        }
                        //__instance.addMessage($"Playing sound {sound}", Color.White);
                    }
                    catch
                    {
                        SMonitor.Log($"Error playing sound {sound}", StardewModdingAPI.LogLevel.Error);
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ChatBox), nameof(ChatBox.receiveKeyPress))]
        public class ChatBox_receiveKeyPress_Patch
        {
            public static void Postfix(ChatBox __instance, Keys key)
            {
                end = "";
                if (!Config.EnableMod || __instance.chatBox.finalText.Count == 0 || cues is null)
                {
                    return;
                }
                string message = "";
                foreach (var t in __instance.chatBox.finalText)
                {
                    message += t.message;
                    if (message.Length > "/sound".Length && !message.ToLower().StartsWith("/sound "))
                        return;
                }
                if (message.Length <= "/sound ".Length || !message.ToLower().StartsWith("/sound "))
                    return;
                string sound = message.Split(' ', 2)[1];
                var keys = cues.Keys.Where(k => k.StartsWith(sound)).ToList();
                if (keys.Any())
                {
                    keys.Sort();
                    end = keys[0].Substring(sound.Length);
                    if(end.Length > 0 && key == Keys.Tab)
                    {
                        __instance.chatBox.finalText.Add(new ChatSnippet(end, LocalizedContentManager.CurrentLanguageCode));
                        end = "";
                        __instance.chatBox.updateWidth();
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ChatTextBox), nameof(ChatTextBox.Draw))]
        public class ChatTextBox_Draw_Patch
        {
            public static void Postfix(ChatTextBox __instance, SpriteBatch spriteBatch)
            {
                if (!Config.EnableMod || __instance.finalText.Count == 0 || cues is null)
                    return;
                if (end.Length > 0)
                {
                    float xPositionSoFar = 0f;

                    for (int i = 0; i < __instance.finalText.Count; i++)
                    {
                        xPositionSoFar += __instance.finalText[i].myLength;
                    }
                    spriteBatch.DrawString(ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode), end, new Vector2((float)__instance.X + xPositionSoFar + 12f, (float)(__instance.Y + 12)), ChatMessage.getColorFromName(Game1.player.defaultChatColor), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
                }
            }
        }
    }
}