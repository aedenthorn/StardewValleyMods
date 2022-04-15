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

namespace ConnectedGardenPots
{
    public partial class ModEntry
    {
        public static bool drawingConnectedPot;
        public static int potIndex;
        public static int topIndex;
        public static int leftIndex;
        public static int rightIndex;

        [HarmonyPatch(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class IndoorPot_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling IndoorPot.draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 13 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Game1), nameof(Game1.bigCraftableSpriteSheet)) && codes[i + 13].opcode == OpCodes.Call && (MethodInfo)codes[i + 13].operand == AccessTools.Method(typeof(Object), nameof(Object.getSourceRectForBigCraftable)))
                    {
                        SMonitor.Log("replacing sprite sheet method and source for big craftable");
                        codes[i + 13].opcode = OpCodes.Ldarg_0;
                        codes[i + 13].operand = null;
                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetSourceRect))));
                        codes[i].opcode = OpCodes.Ldarg_0;
                        codes[i].operand = null;
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetSpriteSheet))));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(IndoorPot __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (drawingConnectedPot && gardenPotspriteSheet is not null)
                {
                    Vector2 scaleFactor = __instance.getScale();
                    scaleFactor *= 4f;
                    Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                    Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                    Rectangle destination1 = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f) / 2);
                    Rectangle destination2 = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0) + 64, (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f) / 2);
                    if (topIndex >= 0)
                    {
                       spriteBatch.Draw(gardenPotspriteSheet, destination1, new Rectangle(topIndex * 16, 32, 16, 16), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f);
                    }
                    if (leftIndex >= 0)
                    {
                        spriteBatch.Draw(gardenPotspriteSheet, destination2, new Rectangle(leftIndex * 16, 48, 16, 16), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f + 1 / 10000f);
                    }
                    if (rightIndex >= 0)
                    {
                       spriteBatch.Draw(gardenPotspriteSheet, destination2, new Rectangle(rightIndex * 16, 64, 16, 16), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f + 1 / 10000f);
                    }
                    drawingConnectedPot = false;
                }
            }
        }

        private static Texture2D GetSpriteSheet(IndoorPot __instance)
        {
            if(!Config.EnableMod || __instance.modData.ContainsKey("aedenthorn.WallPlanter/offset") || !IsConnectedPot(__instance))
                return Game1.bigCraftableSpriteSheet;
            return gardenPotspriteSheet;
        }
        private static Rectangle GetSourceRect(int index, IndoorPot __instance)
        {
            if(!Config.EnableMod || __instance.modData.ContainsKey("aedenthorn.WallPlanter/offset") || !IsConnectedPot(__instance))
                return Object.getSourceRectForBigCraftable(index);

            drawingConnectedPot = false;

            bool[] potTiles = new bool[9];
            Vector2 tile = __instance.TileLocation;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    int i = 3 * y + x;
                    potTiles[i] = Game1.currentLocation.objects.TryGetValue(tile + new Vector2(x - 1, y - 1), out Object obj) && obj is IndoorPot && !obj.modData.ContainsKey("aedenthorn.WallPlanter/offset");
                }
            }
            topIndex = -2;
            leftIndex = -2;
            rightIndex = -2;
            drawingConnectedPot = true;

            if (!potTiles[1]) // none top
            {
                if (!potTiles[3]) // none left
                {
                    if (!potTiles[5]) // pot below
                    {
                        topIndex = 6;
                        potIndex = 0;
                    }
                    else if (!potTiles[7]) // pot right
                    {
                        topIndex = 0;
                        potIndex = 2;
                    }
                    else // pot right pot below
                    {
                        topIndex = 0;
                        potIndex = 0;
                    }
                }
                else if (!potTiles[5]) // pot left none right
                {
                    if (!potTiles[7]) // pot left
                    {
                        topIndex = 4;
                        potIndex = 6;
                    }
                    else // pot left pot below
                    {
                        topIndex = 4;
                        potIndex = 0;
                    }
                }
                else if (!potTiles[7]) // pot left pot right
                {
                    topIndex = 2;
                    potIndex = 4;
                }
                else // pot left pot right pot below
                {
                    topIndex = 2;
                    potIndex = 0;
                }
            }
            else // pot top
            {
                if (!potTiles[3]) // none left
                {
                    if (!potTiles[5]) // none right
                    {
                        if (!potTiles[7]) // pot top
                        {
                            potIndex = 8;
                        }
                        else // pot top pot bottom
                        {
                            potIndex = 0;
                        }
                    }
                    else // pot right
                    {
                        if (!potTiles[7]) // pot top pot right
                        {
                            potIndex = 2;
                        }
                        else // pot top pot right pot bottom
                        {
                            potIndex = 0;
                        }
                    }
                }
                else if (!potTiles[5]) // pot left none right
                {
                    if (!potTiles[7]) // pot top pot left
                    {
                        potIndex = 6;
                    }
                    else // pot top pot left pot below
                    {
                        potIndex = 0;
                    }
                }
                else if (!potTiles[7]) // pot top pot left pot right
                {
                    potIndex = 4;
                }
                else // pot top pot left pot right pot below
                {
                    potIndex = 0;
                }
            }
            if(potIndex == 0)
            {
                if (!potTiles[3])
                {
                    leftIndex = potTiles[6] ? 2 : 0;
                }
                else if(!potTiles[6])
                {
                    leftIndex =  4;
                }
                if (!potTiles[5])
                {
                    rightIndex = potTiles[8] ? 2 : 0;
                }
                else if (!potTiles[8])
                {
                    rightIndex = 4;
                }
            }
            if (__instance.showNextIndex.Value)
            {
                potIndex++;
                topIndex++;
                leftIndex++;
                rightIndex++;
            }

            return new Rectangle(potIndex * 16, 0, 16, 32);
        }

        private static bool IsConnectedPot(IndoorPot instance)
        {
            Vector2 tile = instance.TileLocation;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (Game1.currentLocation.objects.TryGetValue(tile + new Vector2(x - 1, y - 1), out Object obj) && obj is IndoorPot && !obj.modData.ContainsKey("aedenthorn.WallPlanter/offset"))
                    {
                        int i = 3 * y + x;
                        if (i % 2 == 1)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}