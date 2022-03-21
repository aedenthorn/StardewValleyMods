using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GuitardewValleyHero
{
    public class SongData
    {
        public float speed;
        public float noteScale;
        public int noteScore = 100;
        public int perfectScore = 500;
        public int holdScore = 100;
        public int missScore = -100;
        public string packID;
        public string songName;
        public int introLength;
        public int[] defaultIconIndexes;
        public string[][] beats;
        public List<List<NoteData>> beatDataList = new List<List<NoteData>>();
    }

    public class NoteData
    {
        public int fret;
        public int length = 1;
        public int index = -1;
        public int pointMult = 1;

        public NoteData(string beat)
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
            if (parts.Length > 3)
                int.TryParse(parts[3], out pointMult);
        }
    }
}