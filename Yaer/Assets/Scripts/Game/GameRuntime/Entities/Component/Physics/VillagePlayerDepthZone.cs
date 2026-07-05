using System.Collections.Generic;
using Game.GameRuntime.Entities.Player;
using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// 树屋 / 楼梯等「策划体积」深度区：当 <b>PlayerFoot</b> 物理层上的碰撞体进入本物体的 Trigger 时，
    /// 将玩家全部身体 <see cref="SpriteRenderer"/> 的 <c>sortingLayerName</c> 切到配置层；离开全部相关区后回到 <see cref="SortingLayerName.Player"/>。
    /// <para>须挂在本物体且配合 <c>Collider2D.isTrigger=true</c>；与 <see cref="VillageSceneObjectDepthSort"/> 解耦，不改其源码。</para>
    /// </summary>
    [DisallowMultipleComponent]
    public class VillagePlayerDepthZone : MonoBehaviour
    {
        [Header("Sorting")]
        [Tooltip("进入本区后写入的 Sprite Sorting Layer 名，须与 TagManager 一致（如 Default / SceneObject / Player 等）。")]
        [SerializeField]
        private string targetSortingLayer = SortingLayerName.SceneObject;

        [Header("叠层")]
        [Tooltip("多区重叠时数值越大越优先；同优先级时实例 ID 大者生效（见 VillagePlayerDepthZoneListener 说明）。")]
        [SerializeField]
        private int zonePriority;

        [Header("Order in Layer（与 TownPlayerLocomotion 解耦）")]
        [Tooltip(
            "为 true 时：本区在「胜出」期间由 Listener 在 LateUpdate 中把 sortingOrder 固定为下方数值，覆盖 TownPlayerLocomotion 按世界 Y 每帧写入的 Order。\n" +
            "进入区域后 Order 不再随纵轴变化；离开本区且无其它锁定区时恢复为仅由 Y 驱动。")]
        [SerializeField]
        private bool lockSortingOrderInZone;

        [Tooltip("与 lockSortingOrderInZone 配套：固定写入的 Sorting Order（同一 Sorting Layer 内相对前后）。")]
        [SerializeField]
        private int sortingOrderInZone;

        [Header("校验")]
        [Tooltip("为 true 时若本物体无 Trigger 的 Collider2D 会在 Awake 报错。")]
        [SerializeField]
        private bool requireTriggerCollider = true;

        private Collider2D[] _ownColliders;

        /// <summary>
        /// 曾向哪些 Listener 注册过本 Zone；在 Zone 被禁用/销毁时用于 O(1) 量级注销，避免 FindObjectsOfType。
        /// </summary>
        private readonly HashSet<VillagePlayerDepthZoneListener> _listenersTouched =
            new HashSet<VillagePlayerDepthZoneListener>();

        private void Awake()
        {
            _ownColliders = GetComponents<Collider2D>();
            if (!requireTriggerCollider)
            {
                return;
            }

            bool anyTrigger = false;
            if (_ownColliders != null)
            {
                for (int i = 0; i < _ownColliders.Length; i++)
                {
                    var c = _ownColliders[i];
                    if (c != null && c.isTrigger)
                    {
                        anyTrigger = true;
                        break;
                    }
                }
            }

            if (!anyTrigger)
            {
                Debug.LogError($"[VillageDepthZone] 「{name}」需要至少一个 isTrigger 的 Collider2D 才能接收 PlayerFoot。", this);
            }
        }

        private void OnDisable()
        {
            // 体积被关时等价于区内玩家全部离开，主动注销，避免 Listener 字典泄漏。
            NotifyUnregisterAll();
        }

        private void OnDestroy()
        {
            NotifyUnregisterAll();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryRegister(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TryUnregister(other);
        }

        private void TryRegister(Collider2D other)
        {
            if (other == null || !VillagePlayerDepthZoneListener.IsPlayerFootLayer(other.gameObject.layer))
            {
                return;
            }

            var playerLogic = other.GetComponentInParent<PlayerLogic>();
            if (playerLogic == null)
            {
                return;
            }

            var listener = playerLogic.GetComponent<VillagePlayerDepthZoneListener>();
            if (listener == null)
            {
                listener = playerLogic.gameObject.AddComponent<VillagePlayerDepthZoneListener>();
            }

            listener.EnsureInitialized(playerLogic);
            listener.RegisterZone(this, zonePriority, targetSortingLayer, lockSortingOrderInZone, sortingOrderInZone);
            _listenersTouched.Add(listener);
        }

        private void TryUnregister(Collider2D other)
        {
            if (other == null || !VillagePlayerDepthZoneListener.IsPlayerFootLayer(other.gameObject.layer))
            {
                return;
            }

            var playerLogic = other.GetComponentInParent<PlayerLogic>();
            if (playerLogic == null)
            {
                return;
            }

            var listener = playerLogic.GetComponent<VillagePlayerDepthZoneListener>();
            listener?.UnregisterZone(this);
        }

        /// <summary>
        /// Zone 被关/删时，Unity 未必对仍重叠的 Collider 补发 Exit；此处对曾注册过的 Listener 主动注销，防止字典残留。
        /// </summary>
        private void NotifyUnregisterAll()
        {
            foreach (var listener in _listenersTouched)
            {
                if (listener != null)
                {
                    listener.UnregisterZone(this);
                }
            }

            _listenersTouched.Clear();
        }
    }
}
