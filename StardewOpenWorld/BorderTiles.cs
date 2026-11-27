namespace StardewOpenWorld
{

    public class BorderTiles
    {
        public static BorderTiles meadow = new BorderTiles()
        {
            TL = 377,
            TR = 375,
            TLC = 353,
            TRC = 403,
            T = 376,
            BL = 327,
            BR = 325,
            BLC = 378,
            BRC = 328,
            B = 326,
            L = 352,
            R = 350,
        };
        public static BorderTiles dirt = new BorderTiles()
        {
            TL = 202,
            TR = 177,
            TLC = 200,
            TRC = 203,
            T = 201,
            BL = 252,
            BR = 178,
            BLC = 250,
            BRC = 253,
            B = 251,
            L = 225,
            R = 228

        };

        public int TL;
        public int TR;
        public int TLC;
        public int TRC;
        public int T;
        public int BL;
        public int BR;
        public int BLC;
        public int BRC;
        public int B;
        public int L;
        public int R;
    }
}