using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 曾用于 Play 验收：数字键 1～5 切脸，F1～F3 切身。
    /// <para>
    /// 产品已关闭：与商店数量输入抢数字键，表现为「商人莫名切表情」。
    /// 默认不轮询；仅当 Inspector 勾选 <see cref="enableDebugHotkeys"/> 且组件启用时才生效。
    /// </para>
    /// </summary>
    public class ShopkeeperFaceDebugInput : MonoBehaviour
    {
        [SerializeField] private ShopkeeperFaceController controller;

        /// <summary>
        /// 默认 false。原因：开着会让 Alpha1～5 改商人脸，干扰购买/贩卖数量输入。
        /// 若需临时验收切脸，再勾选并启用本组件。
        /// </summary>
        [SerializeField] private bool enableDebugHotkeys = false;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<ShopkeeperFaceController>();
            }

            // 双保险：未显式打开热键时直接停用，避免场景里遗留 m_Enabled=1。
            if (!enableDebugHotkeys)
            {
                enabled = false;
            }
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!enableDebugHotkeys)
            {
                return;
            }

            PollDebugKeys();
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void PollDebugKeys()
        {
            if (controller == null)
            {
                controller = ShopkeeperFaceRegistry.Instance;
            }

            if (controller == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                controller.SetFace(ShopkeeperFaceType.Face1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                controller.SetFace(ShopkeeperFaceType.Face2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                controller.SetFace(ShopkeeperFaceType.Face3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                controller.SetFace(ShopkeeperFaceType.Face4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                controller.SetFace(ShopkeeperFaceType.Face5);
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                controller.SetBody(ShopkeeperBodyType.Normal);
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                controller.SetBody(ShopkeeperBodyType.Blush);
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                controller.SetBody(ShopkeeperBodyType.Sinister);
            }
        }
#endif
    }
}
