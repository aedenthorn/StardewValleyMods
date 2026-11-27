using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace CustomAchievements
{
    public class MyPatches
    {
        public static IMonitor Monitor { get; private set; }
        public static IModHelper Helper { get; private set; }
        public static ModConfig Config { get; private set; }

        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        public static void MakePatches(string id)
        {

            var harmony = new Harmony(id);

            // CollectionsPage patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(CollectionsPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
               postfix: new HarmonyMethod(typeof(MyPatches), nameof(MyPatches.CollectionsPage_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(CollectionsPage), nameof(CollectionsPage.createDescription)),
               prefix: new HarmonyMethod(typeof(MyPatches), nameof(MyPatches.CollectionsPage_createDescription_Prefix))
            );

            if (Config.AllowFaceSkipping)
            {
                harmony.Patch(
                   original: AccessTools.Method(typeof(CollectionsPage), nameof(CollectionsPage.draw), new Type[] { typeof(SpriteBatch) }),
                   transpiler: new HarmonyMethod(typeof(MyPatches), nameof(MyPatches.CollectionsPage_draw_Transpiler))
                );
            }
        }
        private static void CollectionsPage_Postfix(CollectionsPage __instance)
        {
            if (!Config.EnableMod)
                return;

            int widthUsed = __instance.collections[5][0].Count;
            int baseX = __instance.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
            int baseY = __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;

            using var dict = Helper.GameContent.Load<Dictionary<string, CustomAcheivementData>>(ModEntry.dictPath).GetEnumerator();
            List<ClickableTextureComponent> list = __instance.collections[5].Last();
            while (dict.MoveNext())
            {
                var a = dict.Current.Value;
                ModEntry.currentAchievements[a.ID.GetHashCode()] = a;

                int xPos = baseX + widthUsed % 10 * 68;
                int yPos = baseY + widthUsed / 10 * 68;
                if (yPos > __instance.yPositionOnScreen + __instance.height - 128)
                {
                    __instance.collections[5].Add(new List<ClickableTextureComponent>());
                    widthUsed = 0;
                    xPos = baseX;
                    yPos = baseY;
                    list = __instance.collections[5].Last();
                }
                if (!string.IsNullOrEmpty(a.iconPath))
                {
                    var icon = Game1.content.Load<Texture2D>(a.iconPath);
                    list.Add(new ClickableTextureComponent($"{a.ID.GetHashCode()} {a.achieved}", new Rectangle(xPos, yPos, 64, 64), null, "", icon, a.iconRect == null ? new Rectangle(0, 0, icon.Width, icon.Height) : a.iconRect.Value, 1f, false));
                }
                else
                {
                    list.Add(new ClickableTextureComponent($"{a.ID.GetHashCode()} {a.achieved}", new Rectangle(xPos, yPos, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25, -1, -1), 1f, false));
                }
                widthUsed++;
            }

        }

        private static IEnumerable<CodeInstruction> CollectionsPage_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            bool skipping = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (skipping)
                {
                    if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i-1].opcode == OpCodes.Ldc_I4_0)
                    {
                        skipping = false;
                        i++;
                        newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1, null));
                        newCodes.Add(new CodeInstruction(OpCodes.Ldloc_3, null));
                        newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MyPatches), "DrawFace")));
                    }
                    continue;
                }
                newCodes.Add(codes[i]);
                if (codes[i].opcode == OpCodes.Stloc_S && ((LocalBuilder)codes[i].operand).LocalType == typeof(int))
                {
                    skipping = true;
                }
            }

            return newCodes.AsEnumerable();
        }

        public static void DrawFace(SpriteBatch b, ClickableTextureComponent c)
        {
            int id = c.name.Split(' ')[0].GetHashCode();
            if (!Config.EnableMod || !ModEntry.currentAchievements.TryGetValue(id, out var value) || value.drawFace)
            {
                int StarPos = new Random(id).Next(12);
                b.Draw(Game1.mouseCursors, new Vector2((float)(c.bounds.X + 16 + 16), (float)(c.bounds.Y + 20 + 16)), new Rectangle?(new Rectangle(256 + StarPos % 6 * 64 / 2, 128 + StarPos / 6 * 64 / 2, 32, 32)), Color.White, 0f, new Vector2(16f, 16f), c.scale, SpriteEffects.None, 0.88f);
            }
        }

        private static bool CollectionsPage_createDescription_Prefix(CollectionsPage __instance, string id, ref string __result)
        {
            if (!Config.EnableMod || __instance.currentTab != 5 || !int.TryParse(id, out var iid) || !ModEntry.currentAchievements.TryGetValue(iid, out var a))
                return true;
            __result = a.name + "\n\n" + a.description;
            return false;
        }
    }
}