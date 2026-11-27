using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace GalaxyWeaponChoice
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Multiplayer), "receiveChatInfoMessage")]
        public class Multiplayer_receiveChatInfoMessage_Patch
        {
            public static bool Prefix(Multiplayer __instance, Farmer sourceFarmer, string messageKey, string[] args)
            {
                if (!Config.ModEnabled || messageKey != "GalaxySword" || Game1.chatBox is null)
                    return true;

                try
                {
                    ChatBox chatBox = Game1.chatBox;
                    LocalizedContentManager content = Game1.content;
                    string path = "Strings\\UI:Chat_" + messageKey;
                    object[] substitutions = args;
                    var message = content.LoadString(path, substitutions);
                    if(chosenWeapon != 4)
                    {
                        message = message.Replace(weaponNames[4], weaponNames[chosenWeapon]);
                    }

                    chatBox.addInfoMessage(message);
                    return false;
                }
                catch (ContentLoadException)
                {
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
                catch (KeyNotFoundException)
                {
                }

                return true;
            }

        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performTouchAction))]
        public class GameLocation_performTouchAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
            {
                if (!Config.ModEnabled)
                    return true;
                string text = fullActionString.Split(' ', StringSplitOptions.None)[0];
                if (text == "legendarySword" && Game1.player.ActiveObject != null && Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, 74) && !Game1.player.mailReceived.Contains("galaxySword"))
                {
                    ShowChoiceMenu();
                    return false;
                }
                return true;
            }

        }
    }
}