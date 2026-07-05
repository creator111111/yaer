namespace Game.Static.Enum
{
    /// <summary>
    /// 玩家移动/输入语义模式（与《村庄DNF式2.5D移动_迁移方案》4.3 节一致）。
    /// <see cref="Village2_5D"/> 下由 <see cref="Game.GameRuntime.Entities.Player.Components.PlayerInputComponent"/> 丢弃战斗向指令，
    /// 纵深（世界 Y）由 <see cref="Game.GameRuntime.Entities.Player.Components.TownPlayerLocomotion"/> 单独处理；根 Z 冻结。
    /// </summary>
    public enum PlayerLocomotionMode
    {
        /// <summary>默认：全量键位与战斗输入按原逻辑入队。</summary>
        Default = 0,

        /// <summary>
        /// 村庄 KenMuNi1 探索：W/S 走世界 Y 纵深，并屏蔽蹲/普攻/重击/冲刺攻击等（策划验收 AC-04）。
        /// </summary>
        Village2_5D = 1,
    }
}
