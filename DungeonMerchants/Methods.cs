using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

namespace DungeonMerchants
{
    public partial class ModEntry
    {
        public static List<int> dwarfWallTiles = new (){
            119,
            120,
            121,
            122,
            123,
            124,
            133,
            134,
            157,
            158
        };
        public static List<int> merchantWallTiles = new (){
            121,
            122,
            123,
            124,
            133,
            158
        };
        private static int merchantSpriteID = 42997;

        private static void SpawnDwarf(MineShaft mineShaft)
        {
            Point tile = GetBottomWallTile(mineShaft, dwarfWallTiles);
            if (tile.X < 0)
                return;
            mineShaft.modData[dwarfKey] = tile.X + "," + tile.Y;
            mineShaft.addCharacter(new NPC(new AnimatedSprite("Characters\\Dwarf", 0, 16, 24), tile.ToVector2() * 64 + new Vector2(0, 64), "MineShaft", 2, "Dwarf", false, null, Game1.content.Load<Texture2D>("Portraits\\Dwarf"))
            {
                Breather = false
            });
        }
        private static void SpawnMerchant(MineShaft mineShaft)
        {
            Point tile = GetBottomWallTile(mineShaft, merchantWallTiles);
            if (tile.X < 0)
                return;

            mineShaft.modData[merchantKey] = tile.X + "," + tile.Y;
        }

        private static Point GetBottomWallTile(MineShaft mineShaft, List<int> wallTiles)
        {
            List<Point> list = new List<Point>();
            var build = mineShaft.Map.GetLayer("Buildings");
            var back = mineShaft.Map.GetLayer("Back");
            for (int y = 0; y < build.LayerHeight; y++) 
            {
                for (int x = 0; x < build.LayerWidth; x++)
                {
                    if (build.Tiles[x, y] is not null && wallTiles.Contains(build.Tiles[x, y].TileIndex) && y < build.LayerHeight - 1 && build.Tiles[x, y + 1] is null)
                    {
                        list.Add(new Point(x, y));
                    }
                }
            }
            return list.Any() ? list[Game1.random.Next(list.Count)] : new Point(-1,-1);
        }
        public static bool boughtTraderItem(ISalable s, Farmer f, int i)
        {
            if (s.Name == "Magic Rock Candy")
            {
                Desert.boughtMagicRockCandy = true;
            }
            return false;
        }
    }
}