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
                    var list = new List<NoteData>();
                    foreach (var note in beat)
                    {
                        list.Add(new NoteData(note));
                    }
                    data.beatDataList.Add(list);
                }
                songDataDict[data.songName] = data;
            }
            Monitor.Log($"Loaded {songDataDict.Count} song data(s)");
        }

        private static void CreateTextures()
        {
            barTexture = new Texture2D(Game1.graphics.GraphicsDevice, (int)(currentData.noteScale * 16 * 4), 8);
            Color[] data = new Color[barTexture.Width * barTexture.Height];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Config.BarColor;
            }
            barTexture.SetData(data);
        }
        private int GetNoteScore(float yPos)
        {
            var distance = Math.Abs(targetStart - yPos);
            int score = 0;
            float width = currentData.noteScale * 16;
            if (distance < Config.PerfectScoreLeeway)
            {
                Game1.playSound("yoba");
                score = currentData.perfectScore;
            }
            else if(distance < width / 2)
            {
                score = (int)(currentData.noteScore * (width - distance) / width);
            }
            else
            {
                score = currentData.missScore;
            }
            Monitor.Log($"scored: {score}, yPos {yPos} vs {targetStart}, distance {distance}");
            return score;
        }
    }
}