using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalJukeBox
{
    public partial class ModEntry
    {

        public static void OnSongChosen()
        {
            var song = PlayerSong;
            if (song != null)
            {
                playingSong.Value = null;
                if (Game1.currentSong?.Name == song)
                {
                    if (Game1.currentSong.IsPaused)
                    {
                        Game1.currentSong.Resume();
                    }
                    else if(Game1.currentSong.IsStopped)
                        Game1.currentSong.Play();
                    return;
                }
                Game1.hudMessages.Clear();
                Game1.addHUDMessage(new HUDMessage($"Now Playing {Utility.getSongTitleFromCueName(song)}")
                {
                    number = 0,
                    type = PlayerSong,
                    messageSubject = ItemRegistry.Create("(BC)209")
                });
                SMonitor.Log($"Playing track {song}");
            }
            Game1.currentSong?.Stop(AudioStopOptions.Immediate);
            AccessTools.StaticFieldRefAccess<Game1, bool>("requestedMusicDirty") = true;
            AccessTools.StaticFieldRefAccess<Game1, string>("requestedMusicTrack") = song;
            AccessTools.StaticFieldRefAccess<Game1, bool>("requestedMusicTrackOverrideable") = false;
            if (Menu is not null)
            {
                Menu.RebuildElements();
             
            }
        }

        public static List<string> GetTracks(bool playlist)
        {
            List<string> list;
            if (playlist && PlayerList.Any())
            {
                list = PlayerList.ToList();
            }
            else
            {
                list = Utility.GetJukeboxTracks(Game1.player, Game1.player.currentLocation);
                if (!Config.LimitToKnown)
                {
                    foreach(var str in Game1.jukeboxTrackData.Keys.Select(s => GetValidTrack(s)).Where(s => s != null))
                    {
                        if (!list.Contains(str))
                        {
                            list.Add(str);
                        }
                    }
                }
            }
            list.Sort((string a, string b) =>
            {
                return Utility.getSongTitleFromCueName(a).CompareTo(Utility.getSongTitleFromCueName(b));
            });
            return list;
        }

        public static string GetValidTrack(string name)
        {
            if (Game1.soundBank.Exists(name))
                return name;
            var b = AccessTools.FieldRefAccess<SoundBankWrapper, SoundBank>(Game1.soundBank as SoundBankWrapper, "soundBank");
            var cues = AccessTools.FieldRefAccess<SoundBank, Dictionary<string, CueDefinition>>(b, "_cues");
            string result = cues.Keys.FirstOrDefault(k => k.ToLower() == name);
            return result;
        }
        public static void ToggleMenu()
        {
            if (Menu == null)
            {
                Game1.playSound("bigSelect");
                Menu = new JukeBoxMenu();
            }
            else
            {
                Game1.playSound("bigDeSelect");
                Menu = null;
            }
        }
        public static bool ToggleFavorite(string track, bool sound = true)
        {
            bool added = false;
            var list = PlayerList;
            if (!list.Contains(track))
            {
                if (sound)
                {
                    Game1.playSound("bigSelect");
                }
                list.Add(track);
                added = true;
            }
            else
            {
                if (sound)
                {
                    Game1.playSound("bigDeSelect");
                }
                list.Remove(track);
            }
            PlayerList = list;
            return added;
        }

    }
}