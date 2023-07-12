using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Text;

namespace Arabic
{
    public partial class ModEntry
    {
        private static void FixForArabic(ref SpriteFont spriteFont, ref string text, ref Vector2 position)
        {
            if (!Config.ModEnabled || text?.Length == 0 || LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod || LocalizedContentManager.CurrentModLanguage.LanguageCode != "ar" || !arabicFont.Characters.Contains(text[0]))
                return;
            string inter = "";
            for (int i = text.Length - 1; i >= 0; i--)
            {
                inter += text[i];
            }
            text = inter;
            spriteFont = arabicFont;
        }
        private static void FixForArabic(ref SpriteFont spriteFont, ref StringBuilder text, ref Vector2 position)
        {
            if (!Config.ModEnabled || text?.Length == 0 || LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod || LocalizedContentManager.CurrentModLanguage.LanguageCode != "ar" || !arabicFont.Characters.Contains(text[0]))
                return;
            StringBuilder inter = new StringBuilder();
            for (int i = text.Length - 1; i >= 0; i--)
            {
                inter.Append(text[i]);
            }
            text = inter;
            spriteFont = arabicFont;
        }

    }
}