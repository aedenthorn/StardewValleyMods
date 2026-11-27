using Microsoft.Xna.Framework;
using StardewValley;
using System.Linq;

namespace LightSwitches
{
    public interface ILSAPI
    {
        public bool AreLightsOn(GameLocation l);
        public Color GetLightColor(GameLocation l); 
    }
    public class LSAPI : ILSAPI
    {
        public bool AreLightsOn(GameLocation l)
        {
            return l.modData[ModEntry.onOffKey] == "on";
        }

        public Color GetLightColor(GameLocation l)
        {
            return ModEntry.GetLightColor(l);
        }
    }
}