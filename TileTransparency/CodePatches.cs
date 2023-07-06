using Microsoft.Xna.Framework;
using System.Globalization;
using xTile.Tiles;

namespace TileTransparency
{
    public partial class ModEntry
    {
        public static void DrawTile_Prefix(Tile tile, ref Color ___m_modulationColour, ref Color? __state)
        {
            float f = 1;
            if (!Config.ModEnabled || !tile.Properties.TryGetValue("@Opacity", out var p) || (p.Type == typeof(string) && !float.TryParse(p, NumberStyles.Any, CultureInfo.InvariantCulture, out f)))
                return;
            __state = ___m_modulationColour;
            ___m_modulationColour *= p.Type == typeof(string) ? f : (float)p;
        }
        public static void DrawTile_Postfix(ref Color ___m_modulationColour, ref Color? __state)
        {
            if (__state is null)
                return;
            ___m_modulationColour = __state.Value;
        }
    }
}