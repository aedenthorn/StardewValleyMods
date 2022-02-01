using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;

namespace Fetch
{
    public partial class ModEntry
    {

        private static bool isFetching;
        private static bool isBringing;
        private static Farmer fetchee;
        private static Debris fetched;
        private static void Character_MovePosition_Prefix(Character __instance, GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            if (!Config.EnableMod || !(__instance is Pet))
                return;

            if (!isFetching && !isBringing)
            {
                Farmer tfetchee = null;
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (farmer.currentLocation.Name == currentLocation.Name)
                    {
                        tfetchee = farmer;
                        break;
                    }
                }
                if (tfetchee == null)
                    return;
                Debris tfetched = null;
                foreach (var debris in currentLocation.debris)
                {
                    if (debris.item != null)
                    {
                        tfetched = debris;
                        break;
                    }
                }
                if (tfetchee == null)
                    return;
                SMonitor.Log("Fetching");

                isFetching = true;
                fetchee = tfetchee;
                fetched = tfetched;
            }
            if (!isFetching && !isBringing)
                return;
            Vector2 direction = Vector2.Zero;
            if (isFetching)
            {
                float distance = Vector2.Distance(fetched.Chunks[0].position, __instance.getTileLocation() * 64);
                if(distance <= Config.GrabDistance)
                {
                    SMonitor.Log("Bringing to fetchee");
                    isBringing = true;
                    direction = Vector2.Normalize(fetchee.position - __instance.getTileLocation() * 64);
                }
                else
                {
                    direction = Vector2.Normalize(fetched.Chunks[0].position - __instance.getTileLocation() * 64);
                }
            }
            else if (isBringing)
            {
                fetched.Chunks[0].xVelocity.Value = direction.X;
                fetched.Chunks[0].xVelocity.Value = direction.Y;
                direction = Vector2.Normalize(fetchee.position - __instance.getTileLocation() * 64);
            }
            __instance.xVelocity = direction.X;
            __instance.yVelocity = direction.Y;
        }

    }
}