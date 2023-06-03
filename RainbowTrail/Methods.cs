using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace RainbowTrail
{
    public partial class ModEntry
    {
        private void ResetTrail()
        {
            trailDict.Remove(Game1.player.UniqueMultiplayerID);
            rainbowTexture = SHelper.GameContent.Load<Texture2D>(rainbowTrailKey);
        }

        private static bool RainbowTrailStatus(Farmer player)
        {
            if (!Config.ModEnabled || !player.modData.TryGetValue(rainbowTrailKey, out string str))
                return false;
            return true;

        }
    }
}