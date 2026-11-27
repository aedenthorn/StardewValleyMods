using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace PrismaticFlowers
{
    public partial class ModEntry : Mod
    {
        public static bool MakePrismatic(object obj)
        {
            if(obj is Crop crop)
            {
                crop.modData[prismaticKey] = Game1.random.Next(Utility.PRISMATIC_COLORS.Length) + (SHelper.GameContent.Load<Dictionary<string, PrismaticData>>(dictPath).TryGetValue(crop.indexOfHarvest.Value, out var pData) && Game1.random.Next(100) < pData.Chance ? "," + crop.indexOfHarvest.Value : "");
                return true;
            }
            if(obj is ColoredObject co)
            {
                co.modData[prismaticKey] = Game1.random.Next(Utility.PRISMATIC_COLORS.Length) + "";
                return true;
            }
            return false;
        }

        public static Color GetPrismaticColorForObject(Color fallback, object obj)
        {
            if (!Config.ModEnabled)
                return fallback;
            ColoredObject co = obj as ColoredObject;
            Crop cr = obj as Crop;
            if (co?.modData.TryGetValue(prismaticKey, out var val) == true || cr?.modData.TryGetValue(prismaticKey, out val) == true)
            {
                bool isCrop = cr != null;
                int offset = GetOffset(isCrop ? Config.CropPattern : Config.ObjectPattern, isCrop ? (int)cr.tilePosition.X : 0, isCrop ? (int)cr.tilePosition.Y : 0, int.TryParse(val, out var i) ? i : 0);
                return GetPrismaticColorForID(isCrop ? cr.indexOfHarvest.Value : co.ItemId, offset, isCrop, isCrop ? (int)cr.tilePosition.X : 0, isCrop ? (int)cr.tilePosition.Y : 0);
            }
            return fallback;
        }

        public static Color GetPrismaticColorForID(string itemId, int offset, bool isCrop, int x = 0, int y = 0)
        {
            float speed = isCrop ? Config.CropSpeed : Config.ObjectSpeed;
            if (SHelper.GameContent.Load<Dictionary<string, PrismaticData>>(dictPath).TryGetValue(itemId, out var data))
            {
                offset = data.Offset;
                if (isCrop && data.CropType != null && Enum.TryParse(typeof(PrismaticPattern), data.CropType, out var pattern))
                {
                    offset = GetOffset((PrismaticPattern)pattern, x, y, offset);
                }
                if (!isCrop && data.ObjectType != null && Enum.TryParse(typeof(PrismaticPattern), data.ObjectType, out pattern))
                {
                    offset = GetOffset((PrismaticPattern)pattern, x, y, offset);
                }
                if (data.Color1 != null && data.Color2 != null)
                {
                    return Utility.Get2PhaseColor(Utility.StringToColor(data.Color1).Value, Utility.StringToColor(data.Color2).Value, offset, 1, data.TimeOffset);
                }
                return Utility.GetPrismaticColor(offset, data.Speed);
            }
            if (isCrop)
            {
                GetOffset(Config.CropPattern, x, y, offset);
            }
            return Utility.GetPrismaticColor(offset, speed);
        }

        private static int GetOffset(PrismaticPattern pattern, int x, int y, int fallback)
        {
            switch (pattern)
            {
                case PrismaticPattern.Random:
                    return fallback;
                case PrismaticPattern.Down:
                    return Utility.PRISMATIC_COLORS.Length - (y % Utility.PRISMATIC_COLORS.Length);
                case PrismaticPattern.Up:
                    return y;
                case PrismaticPattern.Right:
                    return Utility.PRISMATIC_COLORS.Length - (x % Utility.PRISMATIC_COLORS.Length);
                case PrismaticPattern.Left:
                    return x;
                default:
                    return fallback;
            }
        }

        public static ColoredObject CheckPrismaticHarvest(ColoredObject obj, Crop crop)
        {
            if (!Config.ModEnabled)
                return obj;
            if (crop.modData.TryGetValue(prismaticKey, out var val))
            {
                obj.modData[prismaticKey] = val;
            }
            return obj;
        }
    }
}
