using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace GenieLamp
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), nameof(Object.performUseAction))]
        public class Object_performUseAction_Patch
        {
            public static bool Prefix(Object __instance)
            {
                if (!Config.ModEnabled || (!__instance.Name.Equals(Config.LampItem) && !__instance.QualifiedItemId.Equals(Config.LampItem)))
                    return true;
                int wishes = __instance.modData.TryGetValue(modKey, out var w) ? int.Parse(w) : 0;
                if (wishes >= Config.WishesPerItem)
                {
                    Game1.playSound("cancel", null);
                    Game1.showRedMessage(SHelper.Translation.Get("NoMoreWishes"));
                    return true;
                }
                try
                {
                    Game1.playSound(Config.MenuSound, null);
                }
                catch { }
                AccessTools.Method(typeof(ItemRegistry), "RebuildCache").Invoke(null, new object[0]);

                Game1.activeClickableMenu = new ObjectPickMenu( new NamingMenu.doneNamingBehavior(delegate (string target)
                {
                    SpawnItem(target);
                }), string.Format(SHelper.Translation.Get("WishMenuTitle"), Config.WishesPerItem - wishes));
                return false;
            }
        }
    }
}