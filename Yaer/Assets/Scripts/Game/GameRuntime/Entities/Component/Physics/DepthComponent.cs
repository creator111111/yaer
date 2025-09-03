using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    public class DepthComponent : MonoBehaviour, IDepthComponent
    {
        [SerializeField] private float depthSortingFactor = 100f; // 控制视觉排序的因子
        [SerializeField] private Rigidbody2D bodyRg;
        [SerializeField] private Collider2D bodyCld;
        [SerializeField] private Collider2D footCld;
        private bool applyPhysics; // 标记是否应用物理效果
        private bool isLower;
        private bool isUpper;

        private int layerID;
        private SpriteRenderer spriteRenderer; // 当前对象的SpriteRenderer
        public Rigidbody2D BodyRg => bodyRg;
        public float BoxUpY => footCld.bounds.max.y;
        public float BoxDownY => footCld.bounds.min.y;
        public Collider2D FootCld => footCld;

        public int LayerID
        {
            get => layerID;
            set
            {
                layerID = value;
                gameObject.layer = value;
                bodyCld.gameObject.layer = value;
            }
        }


        public bool IsInSameDepth(float y, float width = 0)
        {
            if (y + width >= BoxDownY && y - width <= BoxUpY) return true; // 说明碰撞盒在y轴上有重叠

            return false;
        }

        public bool IsInSameDepth(IDepthObject other, float multiple = 0)
        {
            if (other.DepthComponent.BoxUpY >= BoxDownY && other.DepthComponent.BoxDownY <= BoxUpY) return true; // 说明碰撞盒在y轴上有重叠
            return false;
        }


        private void UpdateSortingOrder()
        {
            if (isLower || isUpper) return;

            // 根据Y轴调整sortingOrder
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * depthSortingFactor);
        }

        public bool IsInSameDepth(BoxCollider2D cld, float multiple = 0)
        {
            if (cld.bounds.max.y >= BoxDownY && cld.bounds.min.y <= BoxUpY) return true; // 说明碰撞盒在y轴上有重叠

            return false;
        }

        public void SetLower()
        {
            isLower = true;
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-depthSortingFactor * depthSortingFactor);
        }

        public void SetUpper()
        {
            isUpper = true;
            spriteRenderer.sortingOrder = Mathf.RoundToInt(depthSortingFactor * depthSortingFactor);
        }


#if UNITY_EDITOR
        [Space] public bool gizmos;
        public Color col = Color.white;
        private void OnDrawGizmos()
        {
            if (gizmos)
                if (footCld)
                {
                    Gizmos.color = col;
                    Gizmos.DrawWireCube(footCld.bounds.center, footCld.bounds.size);
                }
        }
#endif

        #region Unity

        public void Init(SpriteRenderer sr, Rigidbody2D bodyRg, Collider2D body, Collider2D foot)
        {
            spriteRenderer = sr;
            this.bodyRg = bodyRg;
            bodyCld = body;
            footCld = foot;

            layerID = LayerMask.NameToLayer("SceneObject_Other");
        }

        private void Start()
        {
            if (!spriteRenderer)
            {
                Debug.LogError(name + ": ObjectDepthComponent的SpriteRenderer未初始化");
                return;
            }

            if (!bodyRg)
            {
                Debug.LogError(name + ": ObjectDepthComponent的Rigidbody未初始化");
                return;
            }

            if (!bodyCld)
            {
                Debug.LogError(name + ": ObjectDepthComponent的bodyCld未初始化");
                return;
            }

            if (!footCld)
            {
                Debug.LogError(name + ": ObjectDepthComponent的footCld未初始化");
            }
        }

        private void Update()
        {
            if (!spriteRenderer) return;
            // 每帧根据Y轴调整sortingOrder，Y轴越低，sortingOrder值越高
            UpdateSortingOrder();
        }

        private void OnDestroy()
        {
            spriteRenderer = null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // var component = GameObjectUtility.GetParentTsf(other.transform).GetComponent<IObjectDepthComponent>();
            // if (component)
            // {
            //     // 相同纵深才应用对方的rg进行物理效果
            //     if (IsInSameDepth(component))
            //     {
            //          component.BodyRg.simulated = true;
            //     }
            //     else
            //     {
            //         component.BodyRg.simulated = false;
            //     }
            // }
        }

        #endregion
    }
}