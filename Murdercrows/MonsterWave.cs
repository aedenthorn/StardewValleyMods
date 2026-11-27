namespace Murdercrows
{
    public class MonsterWave
    {
        public int slimes;
        public int bigSlimes;
        public int bats;
        public int flies;
        public int dustSpirits;
        public int ghosts;
        public int mummies;
        public int serpents;
        public int shadowBrutes;
        public int shadowShamans;
        public int skeletons;
        public int squidKids;
        public int dinos;
        public int skulls;
        public int dolls;
        private int[] array;

        public MonsterWave(int slimes = 0, int bigSlimes = 0, int bats = 0, int flies = 0, int dustSpirits = 0, int ghosts = 0, int mummies = 0, int serpents = 0, int shadowBrutes = 0, int shadowShamans = 0, int skeletons = 0, int squidKids = 0, int dinos = 0, int skulls = 0, int dolls = 0)
        {
            this.slimes = slimes;
            this.bigSlimes = bigSlimes;
            this.bats = bats;
            this.flies = flies;
            this.dustSpirits = dustSpirits;
            this.ghosts = ghosts;
            this.mummies = mummies;
            this.serpents = serpents;
            this.shadowBrutes = shadowBrutes;
            this.shadowShamans = shadowShamans;
            this.skeletons = skeletons;
            this.squidKids = squidKids;
            this.dinos = dinos;
            this.skulls = skulls;
            this.dolls = dolls;
            this.array = new int[]
            {
                slimes, bigSlimes, bats, flies, dustSpirits, ghosts, mummies, serpents, shadowBrutes, shadowShamans, skeletons, squidKids, dinos, skulls, dolls
            };
        }

        public int totalMonsters()
        {
            return slimes + bigSlimes + bats + flies + dustSpirits + ghosts + mummies + serpents + shadowBrutes + shadowShamans + skeletons + squidKids + dinos + skulls + dolls;
        }
    }
}