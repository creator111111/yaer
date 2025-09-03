using System;
using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Battle.Buff;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Battle.Attack
{
    // 攻击数据基类（ScriptableObject）
    public abstract class AttackData : ScriptableObject
    {
        public string attackName;
        public float cooldown;
        public string effectPrefabPath;
        public string audioClipPath;
        public List<BuffEffect> statusEffects;
        [Header("攻击逻辑脚本名 (要带上命名空间)")]
        public string LogicScriptName;

        public AttackLogic CreateLogic()
        {
            if (string.IsNullOrEmpty(LogicScriptName))
            {
                Debug.LogError($"未配置攻击逻辑脚本: {this.name}");
                return null;
            }
            Type logicType = Type.GetType(LogicScriptName);
            if (logicType == null)
            {
                Debug.LogError($"未找到攻击逻辑脚本: {LogicScriptName}");
                return null;
            }
            else
            {
                AttackLogic logic = (AttackLogic)Activator.CreateInstance(logicType);
                OnCreateLogic(logic);
                return logic;
            }
        }

        protected virtual void OnCreateLogic(AttackLogic logic)
        {

        }
    }
}