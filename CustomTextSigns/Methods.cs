
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CustomTextSigns
{
    public partial class ModEntry
    {

        private static void DrawSmallTextBubble(SpriteBatch batch, string text, Vector2 position, int width, float layer, bool pointer, Object obj)
        {
            if(Config.ModEnabled)
            {
                if (obj.modData.TryGetValue(offsetKey, out string offsetStr) && int.TryParse(offsetStr, out int offset))
                {
                    position.Y += offset;
                }
                if (obj.modData.TryGetValue(widthKey, out string widthStr) && int.TryParse(widthStr, out int widthOffset))
                {
                    width += widthOffset;
                }
            }
            SpriteText.drawSmallTextBubble(batch, text, position, width, layer, pointer);
        }
        private static int GetMaxLength(sbyte value)
        {
            if (!Config.ModEnabled)
                return value;
            return Config.MaxLength;
        }
    }
}