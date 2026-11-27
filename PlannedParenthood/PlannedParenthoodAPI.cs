using Microsoft.Xna.Framework;
using StardewValley;

namespace PlannedParenthood
{
    public class PlannedParenthoodAPI
    {
        public string GetPartnerTonight()
        {
            return ModEntry.Config.ModEnabled ? ModEntry.partnerName : null;
        }
    }
}