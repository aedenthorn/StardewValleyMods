
using System.Collections.Generic;

namespace CoinCollector
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public string BlipAudioPath { get; set; } = "assets/blip.wav";
        public float BlipAudioVolume { get; set; } = 1f;
        public bool BlipAudioIncreasePitch { get; set; } = true;
        public bool RequireMetalDetector { get; set; } = true;
        public string MetalDetectorID { get; set; } = "MetalDetector";
        public bool RequireMetalDetectorSwing { get; set; } = false;
        public bool EnableIndicator { get; set; } = true;
        public int SecondsPerPoll { get; set; } = 1;
        public float MapHasCoinsChance { get; set; } = 0.5f;
        public int MinCoinsPerMap { get; set; } = 1;
        public int MaxCoinsPerMap { get; set; } = 5;
        public int IndicatorSprite { get; set; } = 7;
        public float IndicatorLength { get; set; } = 3;
        public float IndicatorSpeed { get; set; } = 10;
        public float LuckFactor { get; set; } = 0.1f;
        public float MaxPixelPingDistance { get; set; } = 800;
    }
}
