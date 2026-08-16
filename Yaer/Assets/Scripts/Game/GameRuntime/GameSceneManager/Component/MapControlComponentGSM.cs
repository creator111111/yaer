using System;
using Game.GameRuntime.Entities.Component.Map;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class MapControlComponentGSM : BaseComponentGSM
    {
        [SerializeField] private Map map;

        public Transform RightDefaultBornTsf => map != null ? map.rightBornTsf : null;
        public Transform LeftDefaultBornTsf => map != null ? map.leftBornTsf : null;
        public Transform DefaultBornTsf => map != null ? map.defaultBornTsf : null;

        /// <summary>纯 UI 场景（如 Village_Shop）可不挂 Map。</summary>
        public bool HasMap => map != null;

        private void OnValidate()
        {
            if (map == null) Debug.LogWarning("map引用丢失哦（纯 UI 场景可忽略）", gameObject);
        }

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);

            // 纯 UI / 无行走场景可不配 Map。绝不能在这里 NRE：
            // InitModules 按序初始化，MapControl 在 Input 之前，一旦抛异常会导致 InputComponentGSM 未订阅 ESC。
            if (map == null)
            {
                Debug.LogWarning(
                    "[MapControlComponentGSM] map 未绑定，跳过 Map.OnInit（纯 UI 场景预期行为）。",
                    this);
                return;
            }

            map.OnInit();
        }

        public void SetSceneUnlockCondition(Func<bool> left = null, Func<bool> right = null)
        {
            if (map == null)
            {
                return;
            }

            if (map.LeftDoor != null)
            {
                map.LeftDoor.CheckNextSceneUnlock = left;
            }

            if (map.RightDoor != null)
            {
                map.RightDoor.CheckNextSceneUnlock = right;
            }
        }
    }
}
