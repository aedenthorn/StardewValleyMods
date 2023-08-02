using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Alarms
{
    public partial class ModEntry
    {
        private void CheckForSound(int time)
        {
            foreach (var sound in ClockSoundMenu.soundList)
            {
                if (sound.enabled
                    && (sound.hours < 0 || sound.hours == time / 100)
                    && (sound.minutes < 0 || sound.minutes == time % 100)
                    && (sound.daysOfWeek is null || sound.daysOfWeek[(Game1.dayOfMonth - 1) % 7])
                    && (sound.daysOfMonth is null || sound.daysOfMonth[Game1.dayOfMonth - 1])
                    && (sound.seasons is null || sound.seasons[Utility.getSeasonNumber(Game1.currentSeason)])
                    )
                {
                    if (sound.notification is not null)
                        Game1.addHUDMessage(new HUDMessage(sound.notification, 2));
                    if(sound.sound is not null)
                    {
                        try
                        {
                            Game1.playSound(sound.sound);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}