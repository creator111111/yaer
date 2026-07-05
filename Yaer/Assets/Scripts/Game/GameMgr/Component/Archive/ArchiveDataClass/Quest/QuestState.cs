namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Quest
{
    /// <summary>
    /// 任务运行时状态枚举。
    /// 首版接取追踪仅需 InProgress / Complete；其余状态预留给前置、交付等后续阶段。
    /// </summary>
    public enum QuestState
    {
        /// <summary>前置未满足，暂不可接（阶段 2 简化可跳过）。</summary>
        Locked = 0,

        /// <summary>可接未接（可选，NPC 对话前状态）。</summary>
        Available = 1,

        /// <summary>已接取、进行中；击杀计数仅在此状态下累加。</summary>
        InProgress = 2,

        /// <summary>已达 targetCount，待领奖或回 NPC 交付（阶段 6 裁定）。</summary>
        Complete = 3,

        /// <summary>已交付或已领奖励。</summary>
        TurnedIn = 4,
    }
}
