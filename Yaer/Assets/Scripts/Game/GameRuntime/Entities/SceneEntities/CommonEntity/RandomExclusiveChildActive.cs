using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities
{
    /// <summary>
    /// 两个子物体互斥显隐，按概率只亮其中一个（0922 HomeScene1 墙上画彩蛋）。
    /// <para>
    /// 默认：进场景 / 本物体 OnEnable 时掷一次；<c>easterChance=0.2</c> → 20% 开彩蛋、80% 开正常。
    /// 禁止两张同时 Active；缺引用则 Error 并尽量只开正常画。
    /// </para>
    /// <para>
    /// 原因：场景内「彩蛋画」「正常画」曾双开叠图；每次进门重掷是经典彩蛋，本期不写存档。
    /// 替代（否决）：靠 SortingOrder 盖住——仍叠影；塞进 SceneManager Find 名字——难复用。
    /// </para>
    /// </summary>
    public class RandomExclusiveChildActive : MonoBehaviour
    {
        [Header("互斥目标（必填）")]
        [Tooltip("彩蛋侧（概率命中时显示）")]
        [SerializeField]
        private GameObject easterEgg;

        [Tooltip("常态侧（未命中彩蛋时显示）")]
        [SerializeField]
        private GameObject normalArt;

        [Header("概率")]
        [Tooltip("显示彩蛋的概率，产品钉死 0.2；验收可临时改 0 / 1 / 0.5")]
        [SerializeField]
        [Range(0f, 1f)]
        private float easterChance = 0.2f;

        [Tooltip("为 true：每次 OnEnable（进场景等）重掷一次；关则保持当前显隐直至手动 Roll")]
        [SerializeField]
        private bool rollOnEnable = true;

        private void OnEnable()
        {
            if (rollOnEnable)
            {
                RollAndApply();
            }
        }

        /// <summary>掷骰并互斥 SetActive。可供验收或其它系统手动再掷。</summary>
        public void RollAndApply()
        {
            if (easterEgg == null || normalArt == null)
            {
                Debug.LogError(
                    "[RandomExclusiveChildActive] 缺引用：须同时绑 easterEgg 与 normalArt。已回退尽量只开正常画。",
                    this);
                ApplyExclusive(showEaster: false);
                return;
            }

            // Random.value ∈ [0,1)；&lt; chance 出彩蛋。与 Range(0,100)&lt;20 等价。
            bool showEaster = Random.value < easterChance;
            ApplyExclusive(showEaster);
        }

        /// <summary>强制互斥：最多亮一侧；另一侧必关。</summary>
        private void ApplyExclusive(bool showEaster)
        {
            if (easterEgg != null)
            {
                easterEgg.SetActive(showEaster);
            }

            if (normalArt != null)
            {
                normalArt.SetActive(!showEaster);
            }
            else if (!showEaster)
            {
                // 正常画缺失且本应显示常态：已在上面 Error；此处无额外动作
            }
        }
    }
}
