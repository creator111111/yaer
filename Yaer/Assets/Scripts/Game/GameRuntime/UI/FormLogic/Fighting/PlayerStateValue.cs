namespace Game.GameRuntime.UI.FormLogic.Fighting
{
    public class PlayerStateValue
    {
        public int hp;
        public int hpMax;
        public int mp;
        public int mpMax;

        public float GetHpPercent()
        {
            return hp * 1.0f / hpMax;
        }

        public float GetMpPercent()
        {
            return mp * 1.0f / mpMax;
        }
    }
}