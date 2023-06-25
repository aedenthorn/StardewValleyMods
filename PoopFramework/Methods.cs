using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace PoopFramework
{
    public partial class ModEntry
    {
        private static void TryPoop(Character instance, int timeOfDay, GameLocation l)
        {
            int totalChance = 0;
            List<PoopData> list = new List<PoopData>();
            foreach(var p in poopDict.Values)
            {
                if(p.pooper.Equals(instance.Name) || p.pooper.Equals(instance.GetType().Name))
                {
                    int lastPooped = 600;
                    if(timeOfDay == 610)
                    {
                        instance.modData[dataKey] = "600";
                    }
                    else if(instance.modData.TryGetValue(dataKey, out string dataString)) 
                    {
                        int.TryParse(dataString, out lastPooped);
                    }
                    if ((int)p.poopInterval <= Utility.CalculateMinutesBetweenTimes(lastPooped, timeOfDay))
                    {
                        totalChance += (int)p.poopChance;
                        list.Add(p);
                    }
                }
            }
            if(poopDict.Any())
            {
                int currentChance = 0;
                foreach(var p in list)
                {
                    currentChance += (int)p.poopChance;
                    if (Game1.random.Next(Math.Max(100, totalChance)) < currentChance)
                    {
                        IDictionary<int, string> dict = p.bigCraftablePoop ? Game1.bigCraftablesInformation : Game1.objectInformation;
                        Item i = null;
                        int index = -1;
                        if (!int.TryParse(p.poopItem, out index) || !dict.ContainsKey(index))
                        {
                            foreach (var kvp in dict)
                            {
                                if (kvp.Value.StartsWith(p.poopItem + "/"))
                                {
                                    index = kvp.Key;
                                }
                            }
                        }
                        if (index > -1)
                        {
                            i = p.bigCraftablePoop ? new Object(Vector2.Zero, index) : new Object(index, 1);
                        }
                        if (i != null)
                        {
                            Vector2 offset = Vector2.Zero;
                            switch (instance.FacingDirection)
                            {
                                case 0:
                                    offset = new Vector2(0, 1);
                                    break;
                                case 1:
                                    offset = new Vector2(-1, 0);
                                    break;
                                case 2:
                                    offset = new Vector2(0, -1);
                                    break;
                                case 3:
                                    offset = new Vector2(1, 0);
                                    break;
                            }
                            Debris d = new Debris(i, instance.Position + offset * 16 + new Vector2(0, -32), instance.Position + offset * 64);
                            l.debris.Add(d);
                            if (p.poopSound != null)
                            {
                                l.playSound(p.poopSound);
                            }
                            if(p.poopEmote != null)
                            {
                                foreach(var emote_type in Farmer.EMOTES)
                                {
                                    if (emote_type.emoteString.ToLower() == p.poopEmote.ToLower())
                                    {
                                        if(emote_type.emoteIconIndex >= 0)
                                        {
                                            instance.isEmoting = false;
                                            instance.doEmote(emote_type.emoteIconIndex, false);
                                        }
                                        break;
                                    }
                                }
                            }
                            instance.modData[dataKey] = timeOfDay + "";
                        }
                        else
                        {
                            SMonitor.Log($"Error getting poop {p.poopItem}");
                        }
                        return;
                    }
                }
            }
        }
    }
}