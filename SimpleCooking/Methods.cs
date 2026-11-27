using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Extensions;
using Object = StardewValley.Object;

namespace SimpleCooking
{
    public partial class ModEntry
    {
        public static Color SwitchColor(Color color, Object obj)
        {
            if (!Config.ModEnabled) { return color; }
            if (obj.Name.StartsWith("Grilled "))
            {
                return color.R == color.G && color.R == color.B ? new Color(200,200,200) : Config.GrilledColor;
            }
            else if (obj.Name.StartsWith("Burnt "))
            {
                return color.R == color.G && color.R == color.B ? new Color(30,30,30) : Color.Black;
            }
            return color;
        }
        public static bool TryGetCookingDataForCookable(Item item, out ICookingData data)
        {
            data = null;
            if (item is not Object obj || !obj.HasTypeObject())
                return false;
            if (CookableDict.TryGetValue(obj.ItemId, out var cdata))
            {
                data = new CookingData()
                {
                    LastCheckTime = Game1.timeOfDay,
                    MinutesToCook = cdata.CookTime,
                    BurntAt = cdata.BurntAt > 1 ? cdata.BurntAt : cdata.Burned,
                    Smoke = cdata.ShowSmoke,
                    InputID = obj.QualifiedItemId,
                    ProductID = cdata.ProductID,
                    Quality = obj.Quality,
                    PlacedSound = cdata.PlacedSound ?? Config.PlacedSound,
                    CookedSound = cdata.CookedSound ?? Config.CookedSound,
                    BurntSound = cdata.BurntSound ?? Config.BurntSound,
                    BurntID = cdata.BurntID
                };
            }
            else if(obj.Edibility > 0 && (Config.GrillableCategories.Contains(obj.Category) || Config.GrillableItems.Contains(obj.ItemId)))
            {
                data = new CookingData()
                {
                    LastCheckTime = Game1.timeOfDay,
                    MinutesToCook = Config.GrillTime,
                    BurntAt = Config.BurntAt,
                    Smoke = true,
                    InputID = obj.ItemId,
                    Quality = obj.Quality,
                    PlacedSound = Config.PlacedSound,
                    CookedSound = Config.CookedSound,
                    BurntSound = Config.BurntSound
                };
            }
            else
            {
                return false;
            }
            return true;
        }
        public static bool TryGetCookingDataForCooker(Object obj, out ICookingData data)
        {
            if (obj?.modData.TryGetValue(cookingKey, out var str) == true)
            {
                data = JsonConvert.DeserializeObject<CookingData>(str);
                return true;
            }
            data = null;
            return false;
        }
        public static int AddMinutes(int time, int min)
        {
            var hours = time / 100 + min / 60;
            var mins = time % 100 + min % 60;
            while(mins > 60)
            {
                mins -= 60;
                hours++;
            }
            return hours * 100 + mins;
        }

        private static CookingData GetCookingData(string str)
        {
            return JsonConvert.DeserializeObject<CookingData>(str);
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
        public static int TimeToMinutes(int time)
        {
            return (time / 100 * 60 + time % 100);
        }
        public static int MinutesToTime(int time)
        {
            return (time / 60 * 100  + time % 60);
        }

    }
}