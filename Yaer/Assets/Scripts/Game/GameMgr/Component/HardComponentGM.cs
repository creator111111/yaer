using Game.GameMgr.Component.Base;
using Game.Static.Enum;

namespace Game.GameMgr.Component
{
    /// <summary>
    /// 游戏难度
    /// </summary>
    public class HardComponentGM : BaseComponentGM
    {
        private EGameHard hard;
        public EGameHard Hard => hard;

        public void SetHard(EGameHard hard) => this.hard = hard;
    }
}