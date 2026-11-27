using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.HomeRenovations;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace DoorFurniture
{
    public partial class ModEntry
    {
        public static bool CheckRotatedDoor(bool value, Furniture f)
        {
            if (!Config.ModEnabled || !IsDoor(f))
                return value;
            if (f.currentRotation.Value == 2)
                return true;
            if (f.currentRotation.Value == 3 && f.Flipped)
                return true;
            if (f.currentRotation.Value == 1 && !f.Flipped)
                return true;
            return false;
        }
        public static bool CheckForDoorCollision(Furniture f, Rectangle position, Character c)
        {
            if (!Config.ModEnabled || !TryGetDoorData(f, out var data))
                return f.IntersectsForCollision(position);
            if (!f.modData.TryGetValue(openKey, out var open))
            {
                open = "closed";
                f.modData[openKey] = open;
            }
            else if (open != "closed")
            {
                return false;
            }

            var rot = f.currentRotation.Value;
            var loc = f.GetBoundingBox().Location + data.Bounds[rot].Location;
            var bounds = new Rectangle(loc, data.Bounds[rot].Size);
            bool __result = bounds.Intersects(position);
            var bb = c.GetBoundingBox();
            bool colliding = bounds.Intersects(bb);
            if (colliding)
            {
                if ((c.IsVillager || c is Farmer) && (data.AutoOpen || Config.AutoOpen))
                {
                    return !OpenDoor(f, data, true);
                }
                else
                {
                    return !c.IsVillager || !OpenDoor(f, data, true);
                }
            }
            return __result && !c.IsVillager;

        }
        public static bool IsDoor(Furniture f)
        {
            return f != null && DoorData.ContainsKey(f.ItemId);
        }
        public static bool TryGetDoorData(Furniture f, out DoorData data)
        {
            data = null;
            if(f == null || !DoorData.TryGetValue(f.ItemId, out data))
                return false;
            return true;
        }
        public static Dictionary<string, DoorData> DoorData => SHelper.GameContent.Load<Dictionary<string, DoorData>>(dictPath);
        public static bool OpenDoor(Furniture f, DoorData data = null, bool npc = false)
        {
            if (data == null && !TryGetDoorData(f, out data))
                return false;
            if(!npc && data.Lockable && f.modData.TryGetValue(lockKey, out var str) && str == "True")
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8175"));
                return false;
            }
            f.Location.playSound(data.OpenSound, f.TileLocation);
            f.modData[openKey] = "1";
            int delay = npc ? Config.NPCCloseDelay : (data.AutoCloseDelay > -1 ? data.AutoCloseDelay : Config.AutoCloseDelay);
            if (delay > -1)
                f.modData[closeKey] = delay.ToString();
            return true;
        }
        public static void CloseDoor(Furniture f, DoorData data)
        {
            if (data == null && !TryGetDoorData(f, out data))
                return;
            f.Location.playSound(data.CloseSound);
            f.modData[openKey] = "-1";
        }
        public static void TryReturnObject(Item obj, Farmer who)
        {
            if (obj is null)
                return;
            if (!who.addItemToInventoryBool(obj))
            {
                who.currentLocation.debris.Add(new Debris(obj, who.Position));
            }
        }
        public static Object CombineKeys(Object obj, Object obj2, string key, string ring)
        {

            List<string> keys = new();
            if (!obj.modData.TryGetValue(guidKey, out var cs) || !obj2.modData.TryGetValue(guidKey, out var cs2))
                return null;

            keys.AddRange(cs.Split('|')); 
            keys.AddRange(cs2.Split('|'));
            Object newObj = new Object(ring, 1);
            newObj.modData[guidKey] = string.Join('|', keys);
            newObj.modData[nameKey] = $"{newObj.DisplayName} ({keys.Count})";
            return newObj;
        }
        public static string ColorToHexString(Color value)
        {
            return $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        }
        public static Color GetNewColor(List<Color> tintColors, Color oldColor, int delta)
        {
            int which = tintColors.IndexOf(oldColor);
            if (which == -1)
                which = 0;
            which += Math.Sign(delta);
            if (which < 0)
                which = tintColors.Count - 1;
            else if (which >= tintColors.Count)
                which = 0;
            return tintColors[which];
        }
    }
}