using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace CustomStarterFurniture
{
    public partial class ModEntry
    {
        private static bool IsFarm(int farmType)
        {
            return Game1.whichFarm == farmType || farmType < 0;
        }

        private static int GetObjectIndex(string nameOrIndex, string dictPath)
        {
            int index = -1;
            if (!int.TryParse(nameOrIndex, out index))
            {
                var dict = SHelper.GameContent.Load<Dictionary<int, string>>(dictPath);
                try
                {
                    index = dict.First(p => p.Value.StartsWith(nameOrIndex + "/")).Key;
                }
                catch
                {
                    SMonitor.Log($"Object {nameOrIndex} not found in {dictPath}", StardewModdingAPI.LogLevel.Warn);
                }
            }
            return index;
        }
        private static Object GetObject(string nameOrIndex, string itemType)
        {
            int index = -1;
            Object obj = null;
            switch (itemType)
            {
                case "Object":
                    index = GetObjectIndex(nameOrIndex, "Data/ObjectInformation");
                    if (index != -1)
                    {
                        obj = new Object(index, 1);
                    }
                    break;
                case "Furniture":
                    index = GetObjectIndex(nameOrIndex, "Data/Furniture");
                    if (index != -1)
                    {
                        obj = new Object(index, 1);
                    }
                    obj = Furniture.GetFurnitureInstance(index, Vector2.Zero);
                    break;
                default:
                    SMonitor.Log($"Object type {itemType} not recognized", StardewModdingAPI.LogLevel.Warn);
                    break;
            }
            return obj;
        }
    }
}