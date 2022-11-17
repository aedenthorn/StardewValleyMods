using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace GalaxyWeaponChoice
{
    public partial class ModEntry
    {
        private static int chosenWeapon;
        private static Dictionary<int, string> weaponNames;

        private static void ShowChoiceMenu()
        {
            var dict = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
            string sword = dict[4].Split('/')[0];
            string dagger = dict[23].Split('/')[0];
            string hammer = dict[29].Split('/')[0];
            weaponNames = new Dictionary<int, string>()
            {
                {4, sword },
                {23, dagger },
                {29, hammer }
            };
            Response[] responses =
            {
                new Response("4", sword),
                new Response("23", dagger),
                new Response("29", hammer)
            };
            var action = new GameLocation.afterQuestionBehavior(GetWeapon);
            Game1.currentLocation.createQuestionDialogue("", responses, action);
        }
        private static void GetWeapon(Farmer who, string response)
        {
            chosenWeapon = int.Parse(response);
            SMonitor.Log($"Chose {response}");
            Game1.player.Halt();
            Game1.player.faceDirection(2);
            Game1.player.showCarrying();
            Game1.player.jitterStrength = 1f;
            Game1.pauseThenDoFunction(7000, new Game1.afterFadeFunction(ActuallyGetWeapon));
            Game1.changeMusicTrack("none", false, Game1.MusicContext.Event);
            Game1.currentLocation.playSound("crit", NetAudio.SoundContext.Default);
            Game1.screenGlowOnce(new Color(30, 0, 150), true, 0.01f, 0.999f);
            DelayedAction.playSoundAfterDelay("stardrop", 1500, null, -1);
            Game1.screenOverlayTempSprites.AddRange(Utility.sparkleWithinArea(new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), 500, Color.White, 10, 2000, ""));
            Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate ()
            {
                Game1.stopMusicTrack(Game1.MusicContext.Event);
            }));

        }

        private static void ActuallyGetWeapon()
        {
            Game1.flashAlpha = 1f;
            Game1.player.holdUpItemThenMessage(new MeleeWeapon(chosenWeapon), true);
            Game1.player.reduceActiveItemByOne();
            if (!Game1.player.addItemToInventoryBool(new MeleeWeapon(chosenWeapon), false))
            {
                Game1.createItemDebris(new MeleeWeapon(chosenWeapon), Game1.player.getStandingPosition(), 1, null, -1);
            }
            Game1.player.mailReceived.Add("galaxySword");
            Game1.player.jitterStrength = 0f;
            Game1.screenGlowHold = false;
            ((Multiplayer)AccessTools.Field(typeof(Game1), "multiplayer").GetValue(null)).globalChatInfoMessage("GalaxySword", new string[]
            {
                    Game1.player.Name
            });
        }
    }
}