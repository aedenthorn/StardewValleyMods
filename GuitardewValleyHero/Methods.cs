using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace GuitardewValleyHero
{
    public partial class ModEntry : Mod
    {
        private void ReloadSongData()
        {
            Monitor.Log("Reloading song data");
            songDataDict.Clear();
            var dict = SHelper.Content.Load<Dictionary<string, SongData>>(dictPath, ContentSource.GameContent);
            foreach(var data in dict.Values)
            {
                if (data.packID != null && !loadedContentPacks.Contains(data.packID))
                    loadedContentPacks.Add(data.packID);
                foreach (var beat in data.beats)
                {
                    BeatData beatData = new BeatData(beat);
                    data.beatDataList.Add(beatData);
                }
                songDataDict[data.songName] = data;
            }
            Monitor.Log($"Loaded {songDataDict.Count} song data(s)");
        }
    }
}