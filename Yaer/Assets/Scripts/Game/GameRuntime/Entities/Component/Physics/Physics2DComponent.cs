using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    public class Physics2DComponent : MonoBehaviour, IPhysics2DComponent
    {
        public bool isKinematic = true;
        public bool isCollision;
        public bool startGravity = true;
        public bool xAxisIsTrigger;
        public bool yAxisIsTrigger;
        public float gravity = -9.8f;
        [SerializeField] private Vector2 velocity = new Vector2(0f, 0f); // 初始速度
        private Collider2D cld;
        private readonly HashSet<int> cldIDs = new HashSet<int>();
        private readonly Dictionary<Collider2D, Vector2> cldObjDic = new Dictionary<Collider2D, Vector2>();
        private readonly RaycastHit2D[] hit2Ds = new RaycastHit2D[20];

        private readonly Dictionary<string, Action<GameObject>> onTriggerEnterEvents = new Dictionary<string, Action<GameObject>>();
        private readonly Dictionary<string, Action<GameObject>> onTriggerExitEvents = new Dictionary<string, Action<GameObject>>();
        private readonly Dictionary<string, Action<GameObject>> onTriggerStayEvents = new Dictionary<string, Action<GameObject>>();
        private readonly HashSet<int> removeIDs = new HashSet<int>();

        private Vector3 tarPos;

        private void Update()
        {
            if (cld is null)
            {
                Debug.LogError(name + ": Physics2DComponent未设置 Collider2D 组件");
                return;
            }

            if (isKinematic)
            {
                if (startGravity) velocity.y += gravity * Time.deltaTime;

                if (cldObjDic.Count > 0)
                    // 移动前 检测
                    foreach (var c in cldObjDic.Keys)
                        if (cld.IsTouching(c))
                        {
                            Debug.Log(name + "2");

                            // 禁止移动穿过
                            if (xAxisIsTrigger == false)
                            {
                                if (cldObjDic[c].x == Vector2.left.x && velocity.x < 0)
                                {
                                    transform.position += new Vector3(-velocity.x * Time.deltaTime, velocity.y, 0) * Time.deltaTime;
                                    velocity.x = 0;
                                    return;
                                }

                                if (cldObjDic[c].x == Vector2.right.x && velocity.x > 0)
                                {
                                    transform.position += new Vector3(-velocity.x * Time.deltaTime, velocity.y, 0) * Time.deltaTime;
                                    velocity.x = 0;
                                    return;
                                }
                            }

                            if (yAxisIsTrigger == false)
                            {
                                if (cldObjDic[c].y == Vector2.down.y && velocity.y < 0)
                                {
                                    transform.position += new Vector3(velocity.x, -velocity.y * Time.deltaTime, 0) * Time.deltaTime;
                                    velocity.y = 0;
                                    return;
                                }

                                if (cldObjDic[c].y == Vector2.up.y && velocity.y > 0)
                                {
                                    transform.position += new Vector3(velocity.x, -velocity.y * Time.deltaTime, 0) * Time.deltaTime;
                                    velocity.y = 0;
                                    return;
                                }
                            }
                        }

                // 尝试更新位置
                // Vector3 t = cld.transform.position + new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;
                // if (PreCollisionCheck(t))
                // {
                //     return;
                // }
                // Debug.Log(name + "1");
                transform.position += new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            cldObjDic.Clear();
            onTriggerEnterEvents.Clear();
            onTriggerExitEvents.Clear();
            onTriggerStayEvents.Clear();
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            isCollision = true;

            var o = other.gameObject;
            var layerName = LayerMask.LayerToName(o.layer);
            var tagName = o.tag;

            var v2 = GetClsDir(transform, o.transform);

            Debug.Log(name + "被" + layerName + " " + tagName + " 碰到了");
            if (cldObjDic.ContainsKey(other) == false) cldObjDic.Add(other, v2);

            if (onTriggerEnterEvents.ContainsKey($"{layerName}_{tagName}")) onTriggerEnterEvents[$"{layerName}_{tagName}"](o);

            // 禁止移动穿过
            if (xAxisIsTrigger == false && isCollision) velocity.x = 0;

            if (yAxisIsTrigger == false && isCollision) velocity.y = 0;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var o = other.gameObject;
            var layerName = LayerMask.LayerToName(o.layer);
            var tagName = o.tag;

            if (cldObjDic.ContainsKey(other)) cldObjDic.Remove(other);

            if (cldObjDic.Count == 0) isCollision = false;

            if (onTriggerExitEvents.ContainsKey($"{layerName}_{tagName}")) onTriggerExitEvents[$"{layerName}_{tagName}"](o);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            var layerName = LayerMask.LayerToName(other.gameObject.layer);
            var tagName = other.gameObject.tag;
            if (onTriggerStayEvents.ContainsKey($"{layerName}_{tagName}")) onTriggerStayEvents[$"{layerName}_{tagName}"](other.gameObject);

            GetClsDir(transform, other.transform);
        }

        public bool XAxisIsTrigger
        {
            get => xAxisIsTrigger;
            set => xAxisIsTrigger = value;
        }

        public bool YAxisIsTrigger
        {
            get => yAxisIsTrigger;
            set => yAxisIsTrigger = value;
        }

        public Vector2 Velocity => velocity;
        public Vector2 ClsDir { get; }

        public void Init(Collider2D c)
        {
            cld = c;
        }

        public void SetVelocity(Vector2 v)
        {
            velocity = v;
        }

        public ICollection<Collider2D> GetCollisionObjs()
        {
            return cldObjDic.Keys;
        }

        public void SetCollider2D(Collider2D c)
        {
            cld = c;
        }

        private bool PreCollisionCheck(Vector3 targetPosition)
        {
            // 使用 BoxCast 以检测从当前位置到目标位置是否发生碰撞
            Physics2D.BoxCastNonAlloc(targetPosition, cld.bounds.size, 0, Vector2.zero, hit2Ds);
            foreach (var c in cldObjDic.Keys)
                if (hit2Ds.Any(hit => hit.collider == c))
                    return true;

            return false;
        }

        private void CheckCollision(Vector3 targetPosition)
        {
            // 使用 BoxCast 以检测从当前位置到目标位置是否发生碰撞
            Physics2D.BoxCastNonAlloc(targetPosition, cld.bounds.size, 0, Vector2.zero, hit2Ds);

            // 添加新碰撞
            foreach (var hit in hit2Ds)
            {
                var c = hit.collider;
                if (cldIDs.Contains(c.GetInstanceID()) == false)
                {
                    OnClsEnter(c);
                    cldIDs.Add(c.GetInstanceID());
                }
                else
                {
                    OnClsStay(c);
                }
            }

            removeIDs.Clear();

            // 移除未碰撞
            foreach (var id in cldIDs)
                if (hit2Ds.Any(hit => hit.collider.GetInstanceID() == id) == false)
                    removeIDs.Add(id);

            foreach (var id in removeIDs)
            {
                OnClsExit(cld);
                cldIDs.Remove(id);
            }
        }

        public void RegisterCollisionStayEvent(string layerName, string tagName, Action<GameObject> action)
        {
            onTriggerStayEvents.Add($"{layerName}_{tagName}", action);
        }

        public void RegisterCollisionEnterEvent(string layerName, string tagName, Action<GameObject> action)
        {
            onTriggerEnterEvents.Add($"{layerName}_{tagName}", action);
        }

        public void RegisterCollisionExitEvent(string layerName, string tagName, Action<GameObject> action)
        {
            onTriggerExitEvents.Add($"{layerName}_{tagName}", action);
        }

        private void OnClsEnter(Collider2D hit)
        {
        }

        private void OnClsStay(Collider2D hit)
        {
        }

        private void OnClsExit(Collider2D hit)
        {
        }

        private Vector2 GetClsDir(Transform self, Transform other)
        {
            Vector2 v2;
            if (other.position.x < self.position.x)
                v2 = Vector2.left;
            else
                v2 = Vector2.right;

            if (other.position.y < self.position.y)
                v2 += Vector2.down;
            else
                v2 += Vector2.up;

            return v2;
        }
    }
}