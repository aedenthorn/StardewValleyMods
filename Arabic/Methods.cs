using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Arabic
{
    public partial class ModEntry
    {
        private static void FixForArabic(ref SpriteFont spriteFont, ref string text)
        {
            if (!Config.ModEnabled || LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod || LocalizedContentManager.CurrentModLanguage.LanguageCode != "ar")
                return;
            string inter = "";
            for (int i = text.Length - 1; i >= 0; i--)
            {
                inter += text[i];
            }
            text = inter;
            spriteFont = arabicFont;
        }

    }
}