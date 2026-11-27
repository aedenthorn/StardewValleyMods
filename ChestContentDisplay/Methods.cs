using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.IO;
using System.Linq;

namespace ChestContentDisplay
{
	public partial class ModEntry : Mod
    {

        private Point GetChestPos()
        {
            var pos = Game1.GlobalToLocal(chestTile.Value * 64).ToPoint();
            return pos + new Point(-(chestMenu.Value?.width ?? 0) / 2 + 32, -(chestMenu.Value?.height ?? 0) - 64);
        }
    }
}
