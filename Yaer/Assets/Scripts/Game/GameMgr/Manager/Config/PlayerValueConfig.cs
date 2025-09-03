namespace Game.GameMgr.Manager.Config
{
    public class PlayerValueConfig
    {
        public readonly int hp;
        public readonly int maxHp;
        public readonly int maxMp;
        public readonly int mp;

        public PlayerValueConfig(int hp, int maxHp, int mp, int maxMp)
        {
            this.hp = hp;
            this.maxHp = maxHp;
            this.mp = mp;
            this.maxMp = maxMp;
        }
    }
}