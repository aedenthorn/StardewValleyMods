using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using System;
using static StardewValley.Minigames.CraneGame;
using Object = StardewValley.Object;

namespace SimpleCooking
{
    public interface ICookingData
    {
        public int MinutesProgress { get; set; }
        public int MinutesToCook { get; set; }
        public int LastCheckTime { get; set; }
        public float Progress { get; }
        public bool Burned { get; }
        public float BurntAt { get; set; }
        public int Quality { get; set; }
        public string InputID { get; set; }
        public string ProductID { get; set; }
        public string BurntID { get; set; }
        public bool Smoke { get; set; }
        public string PlacedSound { get; set; }
        public string CookedSound { get; set; }
        public string BurntSound { get; set; }
        public Object GetProduct();
        public bool WillCook(int time);
        public bool WillBurn(int time);
        public void Update(Object obj, int timeOfDay);
    }
    public class CookingData : ICookingData
    {
        public int MinutesProgress { get; set; }
        public int MinutesToCook { get; set; }
        public int LastCheckTime { get; set; }
        public float Progress 
        {
            get
            {
                return MinutesProgress / (float)MinutesToCook;
            }
        }
        public bool Burned
        {
            get
            {
                return Progress >= Math.Max(Progress, BurntAt);
            }
        }
        public float BurntAt { get; set; } = 2;
        public int Quality { get; set; }
        public string InputID { get; set; }
        public string ProductID { get; set; }
        public string BurntID { get; set; }
        public bool Smoke { get; set; }
        public string PlacedSound { get; set; } = "cut";
        public string CookedSound { get; set; } = "fireball";
        public string BurntSound { get; set; } = "furnace";

        public Object GetProduct()
        {
            if (Progress < 1)
                return null;
            
            if (ProductID == null)
            {
                var temp = ItemRegistry.Create(InputID, 1, Quality) as Object;
                Object item = new ColoredObject("SmokedFish", 1, Color.Orange)
                {
                    Edibility = (int)Math.Round(temp.Edibility * (Burned ? 1 / ModEntry.Config.GrilledEdibilityMult : ModEntry.Config.GrilledEdibilityMult)),
                    Price =  (int)Math.Round(temp.Price * (Burned ? 1 / ModEntry.Config.GrilledPriceMult : ModEntry.Config.GrilledEdibilityMult)),
                    Name = Burned ? "Burnt " : "Grilled " + temp.Name,
                    Quality = Quality
                };
                item.preservedParentSheetIndex.Value = InputID;
                item.modData[ModEntry.grilledKey] = "true";
                return item;
            }
            return ItemRegistry.Create<Object>(Burned ? BurntID : ProductID, 1, Quality);
        }

        public bool WillCook(int time)
        {
            if (Progress >= 1)
                return false;
            var min = ModEntry.TimeToMinutes(time) - ModEntry.TimeToMinutes(LastCheckTime);
            return (MinutesProgress + min) / (float)MinutesToCook >= 1;
        }

        public bool WillBurn(int time)
        {
            if (Burned)
                return false;
            var min = ModEntry.TimeToMinutes(time) - ModEntry.TimeToMinutes(LastCheckTime);
            return (MinutesProgress + min) / (float)MinutesToCook >= BurntAt;
        }

        public void Update(Object obj, int timeOfDay)
        {
            if (timeOfDay < LastCheckTime)
                timeOfDay += 2400;
            if (WillBurn(timeOfDay))
            {
                obj.Location.playSound(BurntSound, obj.TileLocation);
            }
            else if (WillCook(timeOfDay))
            {
                obj.Location.playSound(CookedSound, obj.TileLocation);
            }
            var min = ModEntry.TimeToMinutes(timeOfDay) - ModEntry.TimeToMinutes(LastCheckTime);
            MinutesProgress += min;
            LastCheckTime = timeOfDay;
            obj.modData[ModEntry.cookingKey] = JsonConvert.SerializeObject(this);
        }
    }
}