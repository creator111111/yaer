using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.CommonEntity
{
    /// <summary>
    /// 挂在「父物体」上（Physics 2D）：当与名为 PlayerFoot 的物体上的 Collider2D 接触时，
    /// 将 <see cref="targetToActivate"/> 激活；当脚底离开本物体的触发/碰撞范围时再取消激活。
    /// <para>
    /// 同时实现 Enter/Exit 的 Trigger 与 Collision 版本，与脚底是否勾选 Is Trigger 的配置兼容。
    /// </para>
    /// <para>
    /// 使用前提：本物体与对方至少一方带 <see cref="Rigidbody2D"/>，双方均为 <see cref="Collider2D"/>；
    /// Layer 的 Collision Matrix 不要互相禁用。
    /// </para>
    /// </summary>
    public class ActivateChildOnPlayerFootTrigger : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("玩家脚底进入时激活、离开时取消激活的目标；可直接拖子物体")]
        private GameObject targetToActivate;

        [SerializeField]
        [Tooltip("脚底碰撞体所在物体的名字，需与 Hierarchy 里该物体名称一致")]
        private string playerFootObjectName = "PlayerFoot";

        /// <summary>2D 触发进入。</summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            TrySetActiveIfPlayerFoot(other, true);
        }

        /// <summary>2D 触发离开：脚底移出触发范围后取消激活。</summary>
        private void OnTriggerExit2D(Collider2D other)
        {
            TrySetActiveIfPlayerFoot(other, false);
        }

        /// <summary>2D 实体碰撞进入。</summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision == null || collision.collider == null)
            {
                return;
            }

            TrySetActiveIfPlayerFoot(collision.collider, true);
        }

        /// <summary>2D 实体碰撞分离：脚底与本物体不再接触后取消激活。</summary>
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision == null || collision.collider == null)
            {
                return;
            }

            TrySetActiveIfPlayerFoot(collision.collider, false);
        }

        /// <summary>
        /// 若对方是配置的脚底物体，则对目标设置激活状态；否则忽略。
        /// </summary>
        /// <param name="other">对方 2D 碰撞体</param>
        /// <param name="active">true 进入时激活，false 离开时关闭</param>
        private void TrySetActiveIfPlayerFoot(Collider2D other, bool active)
        {
            if (targetToActivate == null || other == null)
            {
                return;
            }

            // 通过物体名称识别脚底；若改用 Tag，可改为 other.CompareTag("PlayerFoot")
            if (other.gameObject.name != playerFootObjectName)
            {
                return;
            }

            targetToActivate.SetActive(active);
        }
    }
}
