using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using System;

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

            SMonitor.Log("Pet is moving");

            if (!isFetching && !isBringing)
            {
                Farmer tfetchee = null;
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (farmer.currentLocation.Name == currentLocation.Name)
                    {
                        tfetchee = farmer;
                        SMonitor.Log("Got fetchee");
                        break;
                    }
                }
                if (tfetchee == null)
                    return;
                Debris tfetched = null;
                foreach (var debris in currentLocation.debris)
                {
                    if (debris.debrisType.Value == Debris.DebrisType.RESOURCE)
                    {
                        tfetched = debris;
                        SMonitor.Log($"Got debris {debris.debrisType} {debris.chunkType}");
                        break;
                    }
                }
                if (tfetched == null)
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
                    isFetching = false;
                    isBringing = true;
                }
            }
            Vector2 diff = Vector2.Zero;
            if (isBringing)
            {
                fetched.Chunks[0].position.Value = __instance.getTileLocation() * 64;
                float distance = Vector2.Distance(fetched.Chunks[0].position, fetchee.getTileLocation() * 64);
                if (distance <= Config.GrabDistance)
                {
                    SMonitor.Log("Brought to fetchee");
                    isFetching = false;
                    isBringing = false;
                    return;
                }
                diff = fetchee.getTileLocation() - __instance.getTileLocation();
            }
            else if (isFetching)
            {
                diff = fetched.Chunks[0].position.Value - __instance.getTileLocation();
            }
            if (Math.Abs(diff.X) > Math.Abs(diff.Y))
            {
                if (diff.X > 0)
                    __instance.SetMovingRight(true);
                else
                    __instance.SetMovingLeft(true);
            }
            else if (Math.Abs(diff.Y) > Math.Abs(diff.X))
            {
                if (diff.Y > 0)
                    __instance.SetMovingDown(true);
                else
                    __instance.SetMovingUp(true);
            }
            else if (Game1.random.NextDouble() > 0.5)
            {
                if (diff.Y > 0)
                    __instance.SetMovingDown(true);
                else
                    __instance.SetMovingUp(true);
            }
            else
            {
                if (diff.X > 0)
                    __instance.SetMovingRight(true);
                else
                    __instance.SetMovingLeft(true);
            }

        }

    }
}