using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace LetterBlocks
{
    public partial class ModEntry : Mod
    {
        public static bool TryGetBlockData(string itemId, out BlockData data)
        {
            if (!SHelper.GameContent.Load<Dictionary<string, BlockData>>(dictPath).TryGetValue(itemId, out data))
            {
                data = null;
                return false;
            }
            return true;
        }

    }
}
