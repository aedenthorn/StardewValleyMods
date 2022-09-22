using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace OutfitSets
{
    public partial class ModEntry
    {
        private static void SwitchSet(int which, bool force = false)
        {
            if(Game1.player.modData.TryGetValue(keyPrefix + "currentSet", out string data) && int.TryParse(data, out int oldSet))
            {
                if (which == oldSet)
                {
                    SMonitor.Log("Same set, not switching", StardewModdingAPI.LogLevel.Warn);
                    return;
                }
                if(Game1.player.hat.Value != null)
                {
                    string hatData = Game1.player.hat.Value.ToXmlString();
                    Game1.player.modData[keyPrefix + "hat" + oldSet] = hatData;
                }
                else
                {
                    Game1.player.modData.Remove(keyPrefix + "hat" + oldSet);
                }
                if(Game1.player.boots.Value != null)
                {
                    string bootsData = Game1.player.boots.Value.ToXmlString();
                    Game1.player.modData[keyPrefix + "boots" + oldSet] = bootsData;
                }
                else
                {
                    Game1.player.modData.Remove(keyPrefix + "boots" + oldSet);
                }
                if(Game1.player.leftRing.Value != null)
                {
                    string leftRingData = Game1.player.leftRing.Value.ToXmlString();
                    Game1.player.modData[keyPrefix + "leftRing" + oldSet] = leftRingData;
                    Game1.player.leftRing.Value.onUnequip(Game1.player, Game1.currentLocation);
                }
                else
                {
                    Game1.player.modData.Remove(keyPrefix + "leftRing" + oldSet);
                }
                if(Game1.player.rightRing.Value != null)
                {
                    string rightRingData = Game1.player.rightRing.Value.ToXmlString();
                    Game1.player.modData[keyPrefix + "rightRing" + oldSet] = rightRingData;
                    Game1.player.rightRing.Value.onUnequip(Game1.player, Game1.currentLocation);
                }
                else
                {
                    Game1.player.modData.Remove(keyPrefix + "rightRing" + oldSet);
                }
                if (Game1.player.shirtItem.Value != null)
                {
                    string shirtData = Game1.player.shirtItem.Value.ToXmlString();
                    Game1.player.modData[keyPrefix + "shirt" + oldSet] = shirtData;
                }
                else
                {
                    Game1.player.modData.Remove(keyPrefix + "shirt" + oldSet);
                }
                if (Game1.player.pantsItem.Value != null)
                {
                    string pantsData = Game1.player.pantsItem.Value.ToXmlString();
                    Game1.player.modData[keyPrefix + "pants" + oldSet] = pantsData;
                }
                else
                {
                    Game1.player.modData.Remove(keyPrefix + "pants" + oldSet);
                }
                SMonitor.Log($"Saved set {oldSet}");
            }

            SMonitor.Log($"Switching to set {which}");

            Game1.player.modData[keyPrefix + "currentSet"] = which+"";

            if (Game1.player.modData.TryGetValue(keyPrefix + "hat" + which, out string hat))
            {
                Game1.player.hat.Value = hat.FromXml<Hat>();
            }
            else
                Game1.player.hat.Value = null;
            if (Game1.player.modData.TryGetValue(keyPrefix + "shirt" + which, out string shirt))
            {
                Game1.player.shirtItem.Value = shirt.FromXml<Clothing>();
            }
            else
                Game1.player.shirtItem.Value = null;
            if (Game1.player.modData.TryGetValue(keyPrefix + "pants" + which, out string pants))
            {
                Game1.player.pantsItem.Value = pants.FromXml<Clothing>();
            }
            else
                Game1.player.pantsItem.Value = null;
            if (Game1.player.modData.TryGetValue(keyPrefix + "leftRing" + which, out string leftRing))
            {
                Game1.player.leftRing.Value = leftRing.FromXml<Ring>();
                Game1.player.leftRing.Value.onEquip(Game1.player, Game1.currentLocation);
            }
            else
                Game1.player.leftRing.Value = null;
            if (Game1.player.modData.TryGetValue(keyPrefix + "rightRing" + which, out string rightRing))
            {
                Game1.player.rightRing.Value = rightRing.FromXml<Ring>();
                Game1.player.rightRing.Value.onEquip(Game1.player, Game1.currentLocation);
            }
            else
                Game1.player.rightRing.Value = null;
            if (Game1.player.modData.TryGetValue(keyPrefix + "boots" + which, out string boots))
            {
                Game1.player.boots.Value = boots.FromXml<Boots>();
            }
            else
                Game1.player.boots.Value = null;

            SMonitor.Log($"Switched to set {which}");
            Game1.playSound("shwip");
        }

        private static Point GetCenterPoint(InventoryPage __instance, int i)
        {
            var strToDraw = (1 + i) + "";
            Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
            return new Point(__instance.xPositionOnScreen + 48 + (int)Math.Round((i + 0.5f) * 270 / Config.Sets), __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 384 - 12 + 64 + 20 - (int)strSize.Y / 2);
        }
    }
    public static class XmlTools
    {
        public static string ToXmlString<T>(this T input)
        {
            using (var writer = new StringWriter())
            {
                input.ToXml(writer);
                return writer.ToString();
            }
        }
        public static T FromXml<T>(this string value)
        {
            using TextReader reader = new StringReader(value);
            return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
        }
        public static void ToXml<T>(this T objectToSerialize, Stream stream)
        {
            new XmlSerializer(typeof(T)).Serialize(stream, objectToSerialize);
        }

        public static void ToXml<T>(this T objectToSerialize, StringWriter writer)
        {
            new XmlSerializer(typeof(T)).Serialize(writer, objectToSerialize);
        }
    }
}