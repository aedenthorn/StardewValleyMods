namespace Murdercrows
{
    internal class MurderCrow
    {
        public string name;
        public int range;
        public int rate;
        public int damage;
        public int ammoIndex;
        public string hitSound;
        public string fireSound;
        public bool useTileSheet;
        public bool explode;

        public MurderCrow(string _name, int _range, int _rate, int _damage, int _ammoIndex, string _hitSound, string _fireSound, bool _useTileSheet, bool _explode)
        {
            name = _name;
            range = _range;
            rate = _rate;
            damage = _damage;
            ammoIndex = _ammoIndex;
            useTileSheet = _useTileSheet;
            explode = _explode;
            hitSound = _hitSound;
            fireSound = _fireSound;

        }
    }
}