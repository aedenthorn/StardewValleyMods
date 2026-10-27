using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace BuildingShift
{
    public partial class ModEntry
    {

        public static bool TryGetShift(Building building, out Vector2 shift)
        {
            shift = Vector2.Zero;
            if (!building.modData.TryGetValue(shiftKey, out var shiftString))
                return false;
            string[] split = shiftString.Split(',');
            if (split.Length != 2 || !int.TryParse(split[0], out var x) || !int.TryParse(split[1], out var y))
                return false;
            shift = new Vector2(x, y);
            return true;
        }
        public static Vector2 ShiftVector(Vector2 vector, Building building)
        {
            if(!Config.EnableMod || !TryGetShift(building, out var amount)) 
                return vector;
            return vector + amount * 4;
        }

        public void ShiftBuilding(Building b, int x, int y)
        {
            if (SHelper.Input.IsDown(Config.ModKey))
            {
                x *= Config.ShiftAmountMod;
                y *= Config.ShiftAmountMod;
            }
            else
            {
                x *= Config.ShiftAmountNormal;
                y *= Config.ShiftAmountNormal;
            }
            TryGetShift(b, out var shift);
            shift += new Vector2(x, y);
            bool moved = false;
            if(shift.X > 15)
            {
                moved = true;
                b.tileX.Value++;
                shift.X %= 16;
            }
            else if(shift.X < -15)
            {
                moved = true;
                b.tileX.Value--;
                shift.X %= -16;
            }
            if(shift.Y > 15)
            {
                moved = true;
                b.tileY.Value++;
                shift.Y %= 16;
            }
            else if(shift.Y < -15)
            {
                moved = true;
                b.tileY.Value--;
                shift.Y %= -16;
            }
            if (moved)
            {
                SMonitor.Log($"{b.buildingType.Value} moved to tile {b.tileX.Value},{b.tileY.Value}");
                b.GetParentLocation().buildStructure(b, new Vector2(b.tileX.Value, b.tileY.Value), Game1.player, true);
            }
            SMonitor.Log($"{b.buildingType.Value} is shifted by {shift}");
            b.modData[shiftKey] = $"{shift.X},{shift.Y}";
        }
    }
}