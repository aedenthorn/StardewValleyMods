using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using Object = StardewValley.Object;

namespace GuitardewValleyHero
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(MiniJukebox), nameof(MiniJukebox.OnSongChosen))]
        public class MiniJukebox_OnSongChosen_Patch
        {
            public static bool Prefix(string selection)
            {
                if (!Config.EnableMod || isShredding || !songDataDict.TryGetValue(selection, out SongData data))
                    return true;
                Game1.currentSong?.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
                Game1.currentSong = null;
                Game1.player.canMove = false;
                currentSong = selection;
                currentData = data;
                timePressed = new float[4];
                isShredding = true;
                isIntro = true;
                int width = (int)(currentData.noteScale * 16);
                targetStart = (Game1.viewport.Height / width - 2) * width;
                introLength = data.introLength + targetStart;
                timeShredded = 0;
                shredScore = 0;
                CreateTextures();
                Game1.activeClickableMenu?.exitThisMenuNoSound();
                return false;
            }
        }
    }
}