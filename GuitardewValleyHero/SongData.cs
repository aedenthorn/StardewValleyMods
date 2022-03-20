using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GuitardewValleyHero
{
    public class SongData
    {
        public float speed;
        public float noteScale;
        public string packID;
        public string songName;
        public int[] defaultIconIndexes;
        public string[] beats;
        public List<BeatData> beatDataList = new List<BeatData>();
    }

    public class BeatData
    {
        public int fret;
        public int length = 1;
        public int index = -1;

        public BeatData(string beat)
        {
            var parts = beat.Split(',');
            int.TryParse(parts[0], out fret);
            fret -= 1;
            if (fret > 3)
                fret = 3;
            if (parts.Length > 1)
                int.TryParse(parts[1], out length);
            if(length < 0)
                length = 0;
            if (parts.Length > 2)
                int.TryParse(parts[2], out index);
        }
    }
}