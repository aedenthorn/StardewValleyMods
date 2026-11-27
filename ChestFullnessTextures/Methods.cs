using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using System;

namespace ChestFullnessTextures
{
    public partial class ModEntry
    {
        private static ChestTextureData GetChestData(Chest instance, ChestTextureDataShell dataList)
        {
            foreach (var data in dataList.Entries)
            {
                if(data.items <= instance.items.Count)
                    return data;
            }
            return null;
        }
    }
}