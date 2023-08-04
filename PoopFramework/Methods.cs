using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Object = StardewValley.Object;

namespace PoopFramework
{
    public partial class ModEntry
    {
        private static void TryPoop(Character instance, int timeOfDay, GameLocation l)
        {
            if(!Config.ModEnabled) 
                return;

            var toiletDict = SHelper.GameContent.Load<Dictionary<string, ToiletData>>(toiletDictPath);
            var poopDict = SHelper.GameContent.Load<Dictionary<string, PoopData>>(dataKey);
            
            int totalChance = 0;
            List<PoopData> list = new List<PoopData>();
            bool poopWarning = false;
            foreach(var kvp in poopDict)
            {
                if (kvp.Value.pooper.Equals(instance.Name) || kvp.Value.pooper.Equals(instance.GetType().Name))
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
                    if (kvp.Value.poopInterval <= Utility.CalculateMinutesBetweenTimes(lastPooped, timeOfDay))
                    {
                        totalChance += kvp.Value.poopChance;
                        list.Add(kvp.Value);
                    }
                    else if (kvp.Value.poopInterval == Utility.CalculateMinutesBetweenTimes(lastPooped, timeOfDay) + Config.PoopWarningTime)
                    {
                        poopWarning = true;
                    }
                }
            }
            if(list.Any())
            {
                int currentChance = 0;
                foreach(var p in list)
                {
                    currentChance += p.poopChance;
                    if (Game1.random.Next(Math.Max(100, totalChance)) < currentChance)
                    {
                        IDictionary<int, string> dict = p.bigCraftablePoop ? Game1.bigCraftablesInformation : Game1.objectInformation;
                        Item item = null;
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
                            item = p.bigCraftablePoop ? new Object(Vector2.Zero, index) : new Object(index, 1);
                        }
                        if (item != null)
                        {
                            if (p.poopSound != null)
                            {
                                l.playSound(p.poopSound);
                            }
                            if (p.poopEmote != null)
                            {
                                foreach (var emote_type in Farmer.EMOTES)
                                {
                                    if (emote_type.emoteString.ToLower() == p.poopEmote.ToLower())
                                    {
                                        if (emote_type.emoteIconIndex >= 0)
                                        {
                                            instance.isEmoting = false;
                                            instance.doEmote(emote_type.emoteIconIndex, false);
                                        }
                                        break;
                                    }
                                }
                            }
                            instance.modData[dataKey] = timeOfDay + "";
                            foreach (var t in toiletDict.Values)
                            {
                                if (t.sit && instance is Farmer && ((Farmer)instance).IsSitting() && ((Farmer)instance).sittingFurniture is Furniture && ((((Farmer)instance).sittingFurniture as Furniture).Name == t.toiletNameOrId || (((Farmer)instance).sittingFurniture as Furniture).ParentSheetIndex + "" == t.toiletNameOrId || Config.ToiletKeywords.Split(',').Where(s => (((Farmer)instance).sittingFurniture as Furniture).Name.ToLower().Contains(s.Trim().ToLower())).Any()) && (t.poopTypes is null || t.poopTypes.Contains(item.Name) || t.poopTypes.Contains(item.ParentSheetIndex + "")))
                                {
                                    SMonitor.Log("Pooping in toilet");
                                    return;
                                }
                            }
                            SMonitor.Log("Pooping on ground");
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
                            Debris d = new Debris(item, instance.Position + offset * 16 + new Vector2(0, -32), instance.Position + offset * 64);
                            l.debris.Add(d);

                        }
                        else
                        {
                            SMonitor.Log($"Error getting poop {p.poopItem}");
                        }
                        return;
                    }
                }
                
            }
            else if (poopWarning)
            {
                foreach (var emote_type in Farmer.EMOTES)
                {
                    if (emote_type.emoteString.ToLower() == Config.PoopWarning.ToLower())
                    {
                        if (emote_type.emoteIconIndex >= 0)
                        {
                            instance.isEmoting = false;
                            instance.doEmote(emote_type.emoteIconIndex, false);
                        }
                        break;
                    }
                }
            }
        }
    }
}