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
            public static void Postfix(string selection)
            {
                if (!Config.EnableMod || !songDataDict.TryGetValue(selection, out SongData data))
                    return;
                isShredding = true;
                currentSong = selection;
                currentData = data;
                timeShredded = 0;
            }
        }
    }
}