
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace ToolMod
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public Dictionary<int, Point> HoePowerLevelRange { get; set; } = new()
        {
            { 1, new Point(1, 1)},
            { 2, new Point(1, 3)},
            { 3, new Point(1, 5)},
            { 4, new Point(3, 3)},
            { 5, new Point(3, 6)},
            { 6, new Point(5, 5)},
            { 7, new Point(7, 7)},
            { 8, new Point(9, 9)},
            { 9, new Point(11, 11)},
            { 10, new Point(13, 13)},
            { 11, new Point(15, 15)},
            { 12, new Point(17, 17)},
        };
        public Dictionary<int, Point> WateringCanPowerLevelRange { get; set; } = new()
        {
            { 1, new Point(1, 1)},
            { 2, new Point(1, 3)},
            { 3, new Point(1, 5)},
            { 4, new Point(3, 3)},
            { 5, new Point(3, 6)},
            { 6, new Point(5, 5)},
            { 7, new Point(7, 7)},
            { 8, new Point(9, 9)},
            { 9, new Point(11, 11)},
            { 10, new Point(13, 13)},
            { 11, new Point(15, 15)},
            { 12, new Point(17, 17)},
        };
        public Dictionary<int, int> HoeMaxPower { get; set; } = new()
        {
            { 1, 1},
            { 2, 2},
            { 3, 3},
            { 4, 4},
            { 5, 5},
            { 6, 6}
        };
        public Dictionary<int, int> WateringCanMaxPower { get; set; } = new()
        {
            { 1, 1},
            { 2, 2},
            { 3, 3},
            { 4, 4},
            { 5, 5},
            { 6, 6}
        };
        public float PickaxeDamageMult { get; set; } = 1;
        public float AxeDamageMult { get; set; } = 1;
    }
}
