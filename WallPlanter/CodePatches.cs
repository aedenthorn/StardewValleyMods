using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace WallPlanter
{
    public partial class ModEntry
    {
        public static bool drawingWallPot;
        public static int drawingWallPotOffset;
        public static int drawingWallPotInnerOffset;

        [HarmonyPatch(typeof(Utility), nameof(Utility.playerCanPlaceItemHere))]
        public class playerCanPlaceItemHere_Patch
        {
            public static bool Prefix(GameLocation location, Item item, int x, int y, Farmer f, ref bool __result)
            {
                if (!Config.EnableMod || item is not Object || !(item as Object).bigCraftable.Value || item.ParentSheetIndex != 62 || !typeof(DecoratableLocation).IsAssignableFrom(location.GetType()) || !(location as DecoratableLocation).isTileOnWall(x / 64, y / 64) || !Utility.isWithinTileWithLeeway(x, y, item, f))
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class IndoorPot_draw_Patch
        {
            public static void Prefix(IndoorPot __instance, int x, int y)
            {
                if (!Config.EnableMod || !typeof(DecoratableLocation).IsAssignableFrom(Game1.currentLocation.GetType()) || !(Game1.currentLocation as DecoratableLocation).isTileOnWall(x, y))
                    return;
                if (!__instance.modData.TryGetValue("aedenthorn.WallPlanter/offset", out string offsetString))
                {
                    __instance.modData["aedenthorn.WallPlanter/offset"] = Config.OffsetY + "";
                }
                if (!__instance.modData.TryGetValue("aedenthorn.WallPlanter/innerOffset", out string innerOffsetString))
                {
                    __instance.modData["aedenthorn.WallPlanter/innerOffset"] = Config.InnerOffsetY + "";
                }
                drawingWallPotOffset = int.Parse(__instance.modData["aedenthorn.WallPlanter/offset"]);
                drawingWallPotInnerOffset = int.Parse(__instance.modData["aedenthorn.WallPlanter/innerOffset"]);
                drawingWallPot = true;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling IndoorPot.draw");
                bool found1 = false;
                bool found2 = false;
                bool found3 = false;
                bool found4 = false;
                bool found5 = false;
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) }))
                    {
                        SMonitor.Log("replacing first draw method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawIndoorPot));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        found1 = true;
                    }
                    if (!found2 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }))
                    {
                        SMonitor.Log("replacing second draw method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawIndoorPotFertilizer));
                        found2 = true;
                    }
                    if (!found3 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Crop), nameof(Crop.drawWithOffset)))
                    {
                        SMonitor.Log("replacing third draw method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawIndoorPotCrop));
                        found3 = true;
                    }
                    if (!found4 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float) }))
                    {
                        SMonitor.Log("replacing fourth draw method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawIndoorPotObject));
                        found4 = true;
                    }
                    if (!found5 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Bush), nameof(Bush.draw), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float) }))
                    {
                        SMonitor.Log("replacing fifth draw method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawIndoorPotBush));
                        found5 = true;
                    }
                    if (found1 && found2 && found3 && found4 && found5)
                        break;
                }

                return codes.AsEnumerable();
            }
            public static void Postfix()
            {
                drawingWallPot = false;
            }
        }

        private static void DrawIndoorPot(SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, IndoorPot pot)
        {
            if (texture is null || pot is null)
                return;
            int x = (destinationRectangle.X + Game1.viewport.X) / 64;
            int y = (destinationRectangle.Y + Game1.viewport.Y) / 64 + 1;
            if (!Config.EnableMod || !drawingWallPot)
            {
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
            }
            else
            {
                texture = pot.showNextIndex.Value ? wallPotTextureWet : (pot.bush.Value != null && pot.hoeDirt.Value.state.Value == 1 ? wallPotTextureWet : wallPotTexture);
                if (texture is null)
                    return;
                destinationRectangle = new Rectangle(destinationRectangle.Location - new Point(0, drawingWallPotOffset), destinationRectangle.Size);
                spriteBatch.Draw(texture, destinationRectangle, null, color, rotation, origin, effects, layerDepth);
            }
        }
        private static void DrawIndoorPotFertilizer(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            if (drawingWallPot)
            {
                position -= new Vector2(0, drawingWallPotOffset + drawingWallPotInnerOffset - 24);
                sourceRectangle = new Rectangle(sourceRectangle.Value.Location, new Point(13, 5));
            }
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }
        private static void DrawIndoorPotCrop(Crop crop, SpriteBatch spriteBatch, Vector2 tileLocation, Color color, float rotation, Vector2 offset)
        {
            if (drawingWallPot)
            {
                offset -= new Vector2(0, drawingWallPotOffset + drawingWallPotInnerOffset);
            }
            crop.drawWithOffset(spriteBatch, tileLocation, color, rotation, offset);
        }
        private static void DrawIndoorPotObject(Object obj, SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha)
        {
            if (drawingWallPot)
            {
                yNonTile -= drawingWallPotOffset + drawingWallPotInnerOffset;
            }
            obj.draw(spriteBatch, xNonTile, yNonTile, layerDepth, alpha);
        }
        private static void DrawIndoorPotBush(Bush bush, SpriteBatch spriteBatch, Vector2 tileLocation, float yDrawOffset)
        {
            if (drawingWallPot)
            {
                yDrawOffset -= drawingWallPotOffset + drawingWallPotInnerOffset;
            }
            bush.draw(spriteBatch, tileLocation, yDrawOffset);
        }
    }
}