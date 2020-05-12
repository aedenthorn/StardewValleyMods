using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WitcherMod
{
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        public override void Entry(IModHelper helper)
        {
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Characters/Dialogue/Elliott") ||asset.AssetNameEquals("Portraits/Abigail") || asset.AssetNameEquals("Portraits/Elliott") || asset.AssetNameEquals("Portraits/Penny") || asset.AssetNameEquals("Characters/Abigail") || asset.AssetNameEquals("Characters/Elliott") || asset.AssetNameEquals("Characters/Penny"))
            {
                return true;
            }

            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Portraits/Abigail") || asset.AssetNameEquals("Portraits/Elliott") || asset.AssetNameEquals("Portraits/Penny") || asset.AssetNameEquals("Characters/Abigail") || asset.AssetNameEquals("Characters/Elliott") || asset.AssetNameEquals("Characters/Penny"))
            {
                return this.Helper.Content.Load<T>($"assets/{asset.AssetName}.png", ContentSource.ModFolder);
            }
            else if (asset.AssetNameEquals("Characters/Dialogue/Elliott"))
            {
                return this.Helper.Content.Load<T>($"assets/{asset.AssetName}.json", ContentSource.ModFolder);
            }

            throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.DataType == typeof(Dictionary<int,string>) || asset.DataType == typeof(Dictionary<string,string>))
            {
                return false;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.DataType == typeof(Dictionary<int, string>) && !asset.AssetNameEquals("Data/Events/ElliottHouse")) 
            {
                IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
                List<int> keys = new List<int>(data.Keys);
                foreach (int key in keys)
                {
                    data[key] = Regex.Replace(data[key], @"\bElliott\b", "Geralt");
                    data[key] = Regex.Replace(data[key], @"\bAbigail\b", "Yennifer");
                    data[key] = Regex.Replace(data[key], @"\bAbby\b", "Yenn");
                    data[key] = Regex.Replace(data[key], @"\bPenny\b", "Triss");
                }
            }
            else if (asset.DataType == typeof(Dictionary<string, string>))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                List<string> keys = new List<string>(data.Keys);
                foreach (string key in keys)
                {
                    data[key] = Regex.Replace(data[key], @"\bElliott\b", "Geralt");
                    data[key] = Regex.Replace(data[key], @"\bAbigail\b", "Yennifer");
                    data[key] = Regex.Replace(data[key], @"\bAbby\b", "Yenn");
                    data[key] = Regex.Replace(data[key], @"\bPenny\b", "Triss");
                }
            }
        }
    }
}
