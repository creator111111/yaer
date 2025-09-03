using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Static.Enum
{
    public enum ControlInputType
    {
        Left,
        Right,
        Squat, // 下蹲
        Jump, // 跳跃
        NormalAttack, // 轻击
        SmashAttack, // 重击
        DashAttack, // 冲锋
        Interact, // 交互
        NextSentence, // 下一文本
        SkipDialogue, // 跳过文本
        SitDown, // 坐下休息
        None, // 无效输入
    }
}

