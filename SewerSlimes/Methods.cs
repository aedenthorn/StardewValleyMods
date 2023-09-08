using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SewerSlimes
{
    public partial class ModEntry
    {
        private static string GetSlimeName(Dictionary<string, int> weights)
        {
            int currentWeight = 0;
            int totalWeight = 0;
            foreach(var w in weights) 
            {
                totalWeight += w.Value;
            }
            int targetWeight = Game1.random.Next(totalWeight);
            foreach (var w in weights)
            {
                currentWeight += w.Value;
                if(targetWeight < currentWeight)
                {
                    return w.Key;
                }
            }
            return weights.First().Key;
        }
        private static Monster MakeSlime(Vector2 pos, string[] slimes)
        {
            Dictionary<string, int> weights = new();
            foreach (var s in slimes)
            {
                try
                {
                    weights.Add(s, Config.SlimeWeights[s]);
                }
                catch { }
            }
            return MakeSlime(pos, weights);
        }
        private static Monster MakeSlime(Vector2 pos, Dictionary<string, int> slimeWeights)
        {
            Monster m;
            string name = GetSlimeName(slimeWeights);
            switch (name)
            {
                case "Frost Jelly":
                    m = new GreenSlime(pos, 40); 
                    if (!m.hasSpecialItem.Value && Game1.random.NextDouble() < Config.SpecialChancePercent / 100f)
                    {
                        if (Game1.random.NextDouble() < 0.5)
                            (m as GreenSlime).color.Value = new Color(0, 0, 0) * 0.7f;
                        m.hasSpecialItem.Value = true;
                        m.Health *= 3;
                        m.DamageToFarmer *= 2;
                    }
                    break;
                case "Red Sludge":
                    m = new GreenSlime(pos, 80);
                    if(!m.hasSpecialItem.Value && Game1.random.NextDouble() < Config.SpecialChancePercent / 100f)
                    {
                        if(Game1.random.NextDouble() < 0.5) 
                            (m as GreenSlime).color.Value = new Color(50, 10, 50) * 0.7f;
                        m.hasSpecialItem.Value = true;
                        m.Health *= 3;
                        m.DamageToFarmer *= 2;
                    }
                    break;
                case "Purple Sludge":
                    m = new GreenSlime(pos, 121);
                    break;
                case "Yellow Slime":
                    m = MakeSlime(pos, BaseSlimes);
                    if (Game1.random.NextDouble() < 0.5)
                        (m as GreenSlime).color.Value = new Color(255, 255, 50);
                    m.coinsToDrop.Value = 10;
                    break;
                case "Black Slime":
                    m = MakeSlime(pos, BaseSlimes);
                    (m as GreenSlime).color.Value = new Color(40 + Game1.random.Next(10), 40 + Game1.random.Next(10), 40 + Game1.random.Next(10));
                    break;
                case "Copper Slime":
                    m = new GreenSlime(pos, 77377);
                    int red = Game1.random.Next(120, 200);
                    (m as GreenSlime).color.Value = new Color(red, red / 2, red / 4);
                    while (Game1.random.NextDouble() < 0.33)
                    {
                        m.objectsToDrop.Add(378);
                    }
                    m.Health = (int)((float)m.Health * 0.5f);
                    m.Speed += 2;
                    break;
                case "Iron Slime":
                    m = new GreenSlime();
                    int colorBase = Game1.random.Next(120, 200);
                    (m as GreenSlime).color.Value = new Color(colorBase, colorBase, colorBase);
                    while (Game1.random.NextDouble() < 0.33)
                    {
                        m.objectsToDrop.Add(380);
                    }
                    m.Speed = 1;
                    break;
                case "Aqua Slime":
                    m = new GreenSlime(pos, 9999899);
                    break;
                case "Tiger Slime":
                    m = MakeSlime(pos, BaseSlimes); 
                    (m as GreenSlime).makeTigerSlime();
                    break;
                case "Prismatic Slime":
                    m = MakeSlime(pos, BaseSlimes);
                    (m as GreenSlime).makePrismatic();
                    break;
                case "Green Slime":
                default:
                    m = new GreenSlime(pos, 0);
                    if (!m.hasSpecialItem.Value && Game1.random.NextDouble() < Config.SpecialChancePercent / 100f)
                    {
                        (m as GreenSlime).color.Value = new Color(205, 255, 0) * 0.7f;
                        m.hasSpecialItem.Value = true;
                        m.Health *= 3;
                        m.DamageToFarmer *= 2;
                    }
                    break;
            }
            return m;
        }
    }
}