using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace RoastingMarshmallows
{
    public partial class ModEntry
    {
        public static int GetRoastProgress(Farmer farmer, bool inc, bool start)
        {
            if (farmer.IsSitting())
            {
                var t = farmer.Tile;
                switch (farmer.FacingDirection)
                {
                    case 0:
                        t += new Vector2(0,-1);
                        break;
                    case 1:
                        t += new Vector2(1, 0);
                        break;
                    case 2:
                        t += new Vector2(0, 1);
                        break;
                    case 3:
                        t += new Vector2(-1, 0);
                        break;
                }

                if (farmer.currentLocation.Objects.TryGetValue(t, out var obj) && obj is Torch && Config.Campfires.Contains(obj.QualifiedItemId) && (obj as Torch).IsOn)
                {
                    if (farmer.modData.TryGetValue(modKey, out var pstring))
                    {
                        var percent = int.Parse(pstring);
                        if (inc)
                        {
                            percent++;
                            if (percent >= Config.RoastFrames)
                            {
                                farmer.currentLocation.playSound("fireball");
                                farmer.modData.Remove(modKey);
                                return -1;
                            }
                            farmer.modData[modKey] = percent + "";
                        }
                        return percent;
                    }
                    else if (start && farmer.Items.CountId(rawItem) > 0)
                    {
                        farmer.Items.ReduceId(rawItem, 1);
                        farmer.currentLocation.playSound("grassyStep");
                        farmer.modData[modKey] = "1";
                        return 1;
                    }
                }
            }
            if (farmer.modData.TryGetValue(modKey, out var pstring2))
            {
                RemoveMarshmallow(farmer, int.Parse(pstring2));
            }
            return -1;
        }

        private static void RemoveMarshmallow(Farmer farmer, int progress)
        {
            var texture = SHelper.GameContent.Load<Texture2D>(texturePath);
            int frames = texture.Width / (texture.Height / 2);
            int intervalLength = Config.RoastFrames / frames;
            farmer.currentLocation.playSound("dwoop");
            if (progress > intervalLength * (frames - 1))
            {
                if (!farmer.recipesCooked.TryGetValue(burntItem, out int curValue))
                {
                    curValue = 0;
                }
                farmer.recipesCooked[burntItem] = curValue + 1;
                Game1.createObjectDebris(burntItem, farmer.TilePoint.X, farmer.TilePoint.Y, farmer.currentLocation);
            }
            else if (progress > intervalLength * (frames - 2))
            {
                if (!farmer.recipesCooked.TryGetValue(cookedItem, out int curValue))
                {
                    curValue = 0;
                }
                farmer.recipesCooked[cookedItem] = curValue + 1;
                Game1.createObjectDebris(cookedItem, farmer.TilePoint.X, farmer.TilePoint.Y, farmer.currentLocation);
            }
            else
            {
                Game1.createObjectDebris(rawItem, farmer.TilePoint.X, farmer.TilePoint.Y, farmer.currentLocation);
            }
            farmer.modData.Remove(modKey);
        }
    }
}