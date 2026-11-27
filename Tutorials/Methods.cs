using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace Tutorials
{
    public partial class ModEntry
    {
        public static bool OpenTutorial(string key = null, string cat = null, List<string> cats = null)
        {
            if (!Context.IsWorldReady)
                return true;
            foreach (var k in Config.OpenTutorialKey.Keybinds)
            {
                foreach (var b in k.Buttons)
                {
                    SHelper.Input.Suppress(b);
                }
            }
            if (!TutorialDict.Any() || (key is not null && !TutorialDict.ContainsKey(key))  || (key is null && cats is not null && !TutorialDict.Values.Any(t => cats.Contains(t.Category))))
                return false;
            Game1.playSound("bigSelect");
            Game1.activeClickableMenu = new CustomTutorialMenu(key, cat, cats);
            return true;
        }
    }
}