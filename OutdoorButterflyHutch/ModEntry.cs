using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using xTile.Dimensions;

namespace OutdoorButterflyHutch
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
    {

        public static ModEntry context;
        internal static ModConfig Config;

        private int customTexturesWidth;
        private int customTexturesHeight;
        private int spriteIndex;
        private Random myRand;

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("TileSheets/Craftables") || asset.AssetNameEquals("Data/CraftingRecipes") || asset.AssetNameEquals("Data/BigCraftablesInformation"))
            {
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            int extension = Config.SpriteSheetOffsetRows * 32;
            if (asset.AssetNameEquals("TileSheets/Craftables"))
            {
                Texture2D customTexture = this.Helper.Content.Load<Texture2D>("assets/ButteryflyHutch.png", ContentSource.ModFolder);
                var editor = asset.AsImage();
                editor.ExtendImage(minWidth: editor.Data.Width, minHeight: customTexturesHeight + extension + 32);
                editor.PatchImage(customTexture, sourceArea: new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 32), targetArea: new Microsoft.Xna.Framework.Rectangle(0, customTexturesHeight + extension, 16, 32));
            }
            else if (asset.AssetNameEquals("Data/CraftingRecipes"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data.Add("Outdoor Butterfly Hutch", $"{Config.HutchCost}/Field/{spriteIndex}/true/{Config.SkillReq}");
            }
            else if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
            {
                IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
                data.Add(spriteIndex, "Outdoor Butterfly Hutch/0/-300/Crafting -9/Attracts butterflies./true/true/0/Outdoor Butterfly Hutch");
            }
        }


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = this.Helper.ReadConfig<ModConfig>();

            Texture2D customTexture = this.Helper.Content.Load<Texture2D>("TileSheets/Craftables", ContentSource.GameContent);
            customTexturesWidth = customTexture.Width;
            customTexturesHeight = customTexture.Height;
            spriteIndex = (customTexturesWidth * customTexturesHeight / 16 / 32) + (Config.SpriteSheetOffsetRows * customTexturesWidth / 16);
            myRand = new Random();
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.Input.ButtonReleased += Input_ButtonReleased;
        }

        private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft && e.Button != SButton.MouseRight)
                return;
            //AddButterflies(Game1.currentLocation);
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            GameLocation location = e.NewLocation;
            AddButterflies(location);
        }

        private void AddButterflies(GameLocation location)
        {
            int hutches = 0;
            using (OverlaidDictionary.ValuesCollection.Enumerator enumerator = location.objects.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.bigCraftable && enumerator.Current.parentSheetIndex == spriteIndex)
                    {
                        hutches++; 
                        location.instantiateCrittersList();
                        location.addCritter(new Butterfly(location.getRandomTile()).setStayInbounds(true));
                        int num1 = (int)(Config.MinButterfliesDensity * location.map.Layers[0].TileSize.Area);
                        int num2 = (int)(Math.Max(Config.MinButterfliesDensity,Config.MaxButterfliesDensity)*location.map.Layers[0].TileSize.Area)+1;
                        int num = myRand.Next(num1, Math.Max(num1+1,num2));
                        Monitor.Log($"{num1} {num2} Number of butterflies: {num}");
                        int count = 0;
                        while (count < num)
                        {
                            location.addCritter(new Butterfly(location.getRandomTile()).setStayInbounds(true));
                            count++;
                        }
                    }
                }
            }
            if(hutches == 0)
            {
                //foreach(Critter c in location.critt)
            }
        }
    }
}