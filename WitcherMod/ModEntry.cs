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
            if (asset.AssetNameEquals("Characters/Dialogue/Elliott") || asset.AssetNameEquals("Portraits/Abigail") || asset.AssetNameEquals("Portraits/Elliott") || asset.AssetNameEquals("Portraits/Penny") || asset.AssetNameEquals("Characters/Abigail") || asset.AssetNameEquals("Characters/Elliott") || asset.AssetNameEquals("Characters/Penny"))
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
            if (asset.DataType == typeof(Dictionary<int, string>) || asset.DataType == typeof(Dictionary<string, string>))
            {
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/NPCDispositions"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data["Elliott"] = Regex.Replace(data["Elliott"],@"/[^/]+$", "/Geralt");
                data["Abigail"] = Regex.Replace(data["Abigail"],@"/[^/]+$", "/Yennifer");
                data["Penny"] = Regex.Replace(data["Penny"],@"/[^/]+$", "/Triss");
            }
            else if (asset.AssetName.StartsWith("Characters/Dialogue/") || asset.AssetName.StartsWith("Characters\\Dialogue\\"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                List<string> keys = new List<string>(data.Keys);
                foreach (string key in keys)
                {
                    data[key] = Regex.Replace(data[key], @"Elliott", "Geralt");
                    data[key] = Regex.Replace(data[key], @"Abigail", "Yennifer");
                    data[key] = Regex.Replace(data[key], @"Abby", "Yenn");
                    data[key] = Regex.Replace(data[key], @"Penny", "Triss");
                }
            }
        }
    }
}
