using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using System;
using System.Linq;

namespace FishingChestTweaks
{
    public partial class ModEntry
    {
        
        public static void SetChestPosition(BobberBar bar, bool legend)
        {
            float difficulty = bar.difficulty;
            int motionType = bar.motionType;
            if (legend)
            {
                string[] fishData = Game1.random.ChooseFrom(DataLoader.Fish(Game1.content).Where(kvp => kvp.Value.Split('/').Length == 14 && int.TryParse(kvp.Value.Split('/')[1], out var d) && d >= 80).Select(kvp => kvp.Value.Split('/')).ToList());
                difficulty = float.Parse(fishData[1]);
                switch(fishData[2])
                {
                    case "mixed":
                        motionType = 0;
                        break;
                    case "dart":
                        motionType = 1;
                        break;
                    case "smooth":
                        motionType = 2;
                        break;
                    case "sinker":
                        motionType = 3;
                        break;
                    case "floater":
                        motionType = 4;
                        break;
                    default:
                        motionType = 0;
                        break;
                }
            }

            if (Game1.random.NextDouble() < (double)(difficulty * (float)((motionType == 2) ? 20 : 1) / 4000f) && (motionType != 2 || ChestTargetPosition.Value == -1f))
            {
                float spaceBelow = 548f - ChestPosition.Value;
                float spaceAbove = ChestPosition.Value;
                float percent = Math.Min(99f, difficulty + (float)Game1.random.Next(10, 45)) / 100f;
                ChestTargetPosition.Value = ChestPosition.Value + (float)Game1.random.Next((int)Math.Min(-spaceAbove, spaceBelow), (int)spaceBelow) * percent;
            }
            if (motionType == 4)
            {
                FSAcceleration.Value = Math.Max(FSAcceleration.Value - 0.01f, -1.5f);
            }
            else if(motionType == 3)
            {
                FSAcceleration.Value = Math.Min(FSAcceleration.Value + 0.01f, 1.5f);
            }
            if (Math.Abs(ChestPosition.Value - ChestTargetPosition.Value) > 3f && ChestTargetPosition.Value != -1f)
            {
                ChestAcceleration.Value = (ChestTargetPosition.Value - ChestPosition.Value) / ((float)Game1.random.Next(10, 30) + (100f - Math.Min(100f, difficulty)));
                ChestSpeed.Value += (ChestAcceleration.Value - ChestSpeed.Value) / 5f;
            }
            else if (motionType != 2 && Game1.random.NextDouble() < (double)(difficulty / 2000f))
            {
                ChestTargetPosition.Value = ChestPosition.Value + (float)(Game1.random.NextBool() ? Game1.random.Next(-100, -51) : Game1.random.Next(50, 101));
            }
            else
            {
                ChestTargetPosition.Value = -1f;
            }
            if (motionType == 1 && Game1.random.NextDouble() < (double)(difficulty / 1000f))
            {
                ChestTargetPosition.Value = ChestPosition.Value + (float)(Game1.random.NextBool() ? SafeNext(Game1.random, -100 - (int)difficulty * 2, -51) : SafeNext(Game1.random, 50, 101 + (int)difficulty * 2));
            }
            ChestTargetPosition.Value = Math.Max(-1f, Math.Min(ChestTargetPosition.Value, 548f));
            ChestPosition.Value += ChestSpeed.Value + FSAcceleration.Value;
            ChestPosition.Value = Math.Clamp(ChestPosition.Value, 0, 532f);
        }

         
        public static int SafeNext(Random random, int minValue, int maxValue)
        {
            if (minValue >= maxValue)
            {
                return maxValue;
            }
            return random.Next(minValue, maxValue);
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
    }
}