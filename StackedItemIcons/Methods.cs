
using StardewValley;
using System.Collections.Generic;
using System.IO;

namespace StackedItemIcons
{
    public partial class ModEntry
    {
        public static bool AllowedToShow(Object instance)
        {
            if(instance.bigCraftable.Value)
            {
                return false;
            }
            if (!allowList.TryGetValue(instance.Name, out bool allowed))
            {
                allowList[instance.Name] = true;
                writeAllowed = true;
                return true;
            }
            else
                return allowed;
        }

    }
}