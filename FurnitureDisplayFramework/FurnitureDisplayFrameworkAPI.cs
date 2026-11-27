using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FurnitureDisplayFramework
{
    public class FurnitureDisplayFrameworkAPI
    {
        public int GetTotalSlots(Furniture f)
        {
            var name = f.rotations.Value > 1 ? f.Name + ":" + f.currentRotation.Value : f.Name;
            if (!ModEntry.FurnitureDisplayDict.TryGetValue(name, out var data))
                return 0;
            return data.slots.Length;
        }
        public Rectangle? GetSlotRect(Furniture f, int i)
        {
            var name = f.rotations.Value > 1 ? f.Name + ":" + f.currentRotation.Value : f.Name;
            if (!ModEntry.FurnitureDisplayDict.TryGetValue(name, out var data) || data.slots.Length <= i)
                return null;
            var rect = data.slots[i].slotRect;
            return new Rectangle?(new Rectangle(rect.X * 4, rect.Y * 4, rect.Width * 4, rect.Height * 4));
        }
        public List<Object> GetSlotObjects(Furniture f)
        {
            var name = f.rotations.Value > 1 ? f.Name + ":" + f.currentRotation.Value : f.Name;
            if (!ModEntry.FurnitureDisplayDict.TryGetValue(name, out var data))
                return null;
            List<Object> list = new List<Object>();
            for(int i = 0; i < data.slots.Length; i++)
            {
                list.Add(ModEntry.GetObjectFromSlot(f.modData.TryGetValue("aedenthorn.FurnitureDisplayFramework/" + i, out string slotString) ? slotString : null));
            }
            return list;
        }
    }
}