using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Effect
{
    public class ShadowComponent : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer bodySr;
        private bool isInit;
        private SpriteRenderer shadowSr;

        [SerializeField]
        private bool FollowXPos;

        private void Awake()
        {
            shadowSr = GetComponent<SpriteRenderer>();
            if (bodySr != null && shadowSr != null)
            {
                isInit = true;
            }
        }

        private void Start()
        {
            if (isInit == false) Debug.LogError(name + "的ShadowComponent未初始化成功");
        }

        private void Update()
        {
            if (isInit)
            {
                shadowSr.sprite = bodySr.sprite;
                if (FollowXPos)
                {
                    var pos = shadowSr.transform.position;
                    pos.x = bodySr.transform.position.x;
                    shadowSr.transform.position = pos;
                }
            }
        }

        public void Init(SpriteRenderer bodySr)
        {
            this.bodySr = bodySr;
            shadowSr = GetComponent<SpriteRenderer>();
            isInit = true;
        }
    }
}