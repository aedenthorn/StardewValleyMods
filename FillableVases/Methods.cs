using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Linq;
using Object = StardewValley.Object;

namespace FillableVases
{
    public partial class ModEntry : Mod
    {
        public static string MakeColorString(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        public static void ReturnFlowers(string flowerData, Farmer who)
        {
            var list = flowerData.Split('|').ToList();
            foreach (var item in list)
            {
                var split = item.Split(',');
                var color = Utility.StringToColor(split[1]);
                Object obj;
                if(color is Color c)
                {
                    obj = new ColoredObject(split[0], 1, c)
                    {
                        Quality = int.Parse(split[2])
                    };
                    if (split.Length > 3)
                    {
                        obj.modData[prismaticKey] = split[3];
                    }
                }
                else
                {
                    obj = new Object(split[0], 1, false, -1, int.Parse(split[2]));
                }
                if (!Game1.player.addItemToInventoryBool(obj))
                {
                    Game1.createItemDebris(obj, Game1.player.getStandingPosition(), Game1.player.FacingDirection, Game1.player.currentLocation);
                }
            }
        }
    }
}
