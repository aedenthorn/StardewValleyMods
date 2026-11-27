
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace PetCoats
{
    public class PetCoatData
    {
        public string DisplayName;
        public string PetType;
        public string PetBreed;
        public Color? Tint;
        public string TexturePath;
        public string IconTexturePath;
        public Dictionary<string, string> Swap;
        public Dictionary<Color, Color> realSwap;
        public Dictionary<Color, Color> RealSwap {
            get
            {
                if (Swap == null)
                    return null;
                if (realSwap == null)
                {
                    realSwap = new Dictionary<Color, Color>();
                    foreach(var kvp in Swap)
                    {
                        realSwap[GetColor(kvp.Key)] = GetColor(kvp.Value);
                    }
                }
                return realSwap;
            }
        }

        private Color GetColor(string value)
        {
            Color color = Color.Transparent;
            string[] values = value.Split(',');
            if (values.Length == 3 && int.TryParse(values[0], out var R) && int.TryParse(values[1], out var G) && int.TryParse(values[2], out var B))
            {
                color = new Color(R, G, B);
            }
            else if (values.Length == 4 && int.TryParse(values[0], out var R2) && int.TryParse(values[1], out var G2) && int.TryParse(values[2], out var B2) && int.TryParse(values[3], out var A2))
            {
                color = new Color(R2, G2, B2, A2);
            }
            else if (values.Length == 1)
            {
                var prop = typeof(Color).GetProperties().FirstOrDefault(x => x.Name == value).GetValue(null);
                if(prop is Color)
                    color = (Color)prop;
            }
            return color;
        }

        public Texture2D IconTexture;
        public Texture2D Texture;
    }
}