namespace Game.GameMgr.Manager.Config
{
    public class SlimeValueConfig
    {
        public readonly int hp;
        public readonly int maxHp;
        public readonly int maxMp;
        public readonly int mp;

        public SlimeValueConfig(int hp, int maxHp, int mp, int maxMp)
        {
            this.hp = hp;
            this.maxHp = maxHp;
            this.mp = mp;
            this.maxMp = maxMp;
        }
    }
}