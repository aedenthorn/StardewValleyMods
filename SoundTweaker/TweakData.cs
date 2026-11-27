using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace SoundTweaker
{
    public class TweakData
    {
        public List<SoundInfo> sounds;
        public int? maxInstances = 1;
        public CueDefinition.LimitBehavior? limitBehavior = CueDefinition.LimitBehavior.ReplaceOldest;
    }

    public class SoundInfo
    {
        public VariationType? variationType;
        public string category;
        public List<string> filePaths = new();
        public List<string> cuePaths = new();
        public List<int> soundIndexes = new();
        public bool loop;
        public bool reverb;
        public float? pitch;
        public float? minPitch;
        public float? maxPitch;
        public float? volume;
        public float? minVolume;
        public float? maxVolume;
        public float? minFrequency;
        public float? maxFrequency;
        public float? minQ;
        public float? maxQ;
        public int[] rpcCurves;
        public FilterMode filterMode;
    }
}