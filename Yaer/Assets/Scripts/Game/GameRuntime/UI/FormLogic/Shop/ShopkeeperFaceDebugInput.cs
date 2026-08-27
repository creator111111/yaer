using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// Play 下键盘验收：数字键 1～5 切脸，F1～F3 切身。
    /// 仅 Editor / Development Build 编译；不在 Update 写业务，只读 Input 调 <see cref="ShopkeeperFaceController"/> API。
    /// </summary>
    public class ShopkeeperFaceDebugInput : MonoBehaviour
    {
        [SerializeField] private ShopkeeperFaceController controller;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<ShopkeeperFaceController>();
            }
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
