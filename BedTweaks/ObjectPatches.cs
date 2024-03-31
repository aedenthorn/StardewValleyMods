using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Threading;
using Object = StardewValley.Object;

namespace BedTweaks
{
    public partial class ModEntry
    {
        public static bool Object_draw_Prefix(Object __instance, SpriteBatch spriteBatch, float alpha)
        {
            if (!(__instance is BedFurniture) || (__instance is BedFurniture && (__instance as BedFurniture)?.bedType != BedFurniture.BedType.Double))
                return true;

            if (!Config.EnableMod)
            {
                __instance.boundingBox.Width = 3 * 64;
                return true;
            }

            Vector2 drawPosition = SHelper.Reflection.GetField<NetVector2>(__instance as BedFurniture, "drawPosition").GetValue().Value;

            return drawBedAtLocation(__instance as BedFurniture, spriteBatch, drawPosition, alpha);
        }
    }
}