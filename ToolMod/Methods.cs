using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;

namespace ToolMod
{
    public partial class ModEntry
    {
        private static List<Vector2> GetTilesAffected(Vector2 tileLocation, Point size, int facingDirection)
        {
            List<Vector2> result = new List<Vector2>();
            Point tile = Utility.Vector2ToPoint(tileLocation);
            var xr = (size.X - 1) / 2;
            switch (facingDirection)
            {
                case 0:
                    for(int x = tile.X - xr; x <= tile.X + xr; x++)
                    {
                        for(int y = tile.Y - size.Y + 1; y <= tile.Y; y++)
                        {
                            result.Add(new Vector2(x, y));
                        }
                    }
                    break;
                case 1:
                    for(int y = tile.Y - xr; y <= tile.Y + xr; y++)
                    {
                        for(int x = tile.X; x < tile.X + size.Y; x++)
                        {
                            result.Add(new Vector2(x, y));
                        }
                    }
                    break;
                case 2:
                    for(int x = tile.X - xr; x <= tile.X + xr; x++)
                    {
                        for(int y = tile.Y; y < tile.Y + size.Y; y++)
                        {
                            result.Add(new Vector2(x, y));
                        }
                    }
                    break;
                case 3:
                    for(int y = tile.Y - xr; y <= tile.Y + xr; y++)
                    {
                        for(int x = tile.X - size.Y + 1; x <= tile.X; x++)
                        {
                            result.Add(new Vector2(x, y));
                        }
                    }
                    break;
            }
            return result;
        }
        public static int GetToolMaxPower(int level)
        {
            if (!Config.EnableMod)
                return level;
            if(Game1.player.CurrentTool is Hoe)
            {
                if(Config.HoeMaxPower.TryGetValue(level, out int power))
                    return power;
            }
            else if(Game1.player.CurrentTool is WateringCan)
            {
                if(Config.WateringCanMaxPower.TryGetValue(level, out int power))
                    return power;
            }
            return level;
        }
        private static float GetToolDamage(float damage, Tool t)
        {
            if (!Config.EnableMod)
                return damage;
            if(t is Pickaxe)
            {
                return damage * Config.PickaxeDamageMult;
            }
            else if(t is Axe)
            {
                return damage * Config.AxeDamageMult;
            }
            return damage;
        }
    }
}