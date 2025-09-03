using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.Static.Utility
{
    public abstract class Physics2DUtility
    {
        /// <summary>
        /// 获取单个碰撞到的对象
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="layerName">层级</param>
        /// <returns></returns>
        public static Collider2D CircleDetectSingle(Vector2 center, float radius, string layerName = null)
        {
            var colliders = new Collider2D[1];
            int size;
            if (layerName != null)
                size = Physics2D.OverlapCircleNonAlloc(center, radius, colliders, 1 << LayerMask.NameToLayer(layerName));
            else
                size = Physics2D.OverlapCircleNonAlloc(center, radius, colliders);

            if (size > 0) return colliders[0];

            return null;
        }

        /// <summary>
        ///     获取多个碰撞到的对象
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="layerName">层级</param>
        /// <returns></returns>
        public static int CircleDetectMulti(Vector2 center, float radius, ref Collider2D[] results, string layerName = null)
        {
            if (layerName != null)
                return Physics2D.OverlapCircleNonAlloc(center, radius, results, 1 << LayerMask.NameToLayer(layerName));
            return Physics2D.OverlapCircleNonAlloc(center, radius, results);
        }

        public static T Raycast<T>(Vector2 pos, Vector2 dir, float distance, string layerName, string tag = null) where T : ISceneObject
        {
            var hit2Ds = new RaycastHit2D[1];
            var count = Physics2D.RaycastNonAlloc(pos, dir, hit2Ds, distance, 1 << LayerMask.NameToLayer(layerName));
            if (count > 0)
            {
                var t = hit2Ds[1].transform;
                T result;

                if (tag != null)
                {
                    do
                    {
                        result = t.GetComponent<T>();
                        if (t.CompareTag(tag) || result != null) return result;
                        t = t.parent;
                    } while (t != null);

                    return default;
                }

                do
                {
                    result = t.GetComponent<T>();
                    if (result != null) break;
                    t = t.parent;
                } while (t != null);

                return result;
            }

            return default;
        }

        public static bool Raycast(Vector2 pos, Vector2 dir, float distance, string layerName, string tag = null)
        {
            var hit2Ds = new RaycastHit2D[1];
            var count = Physics2D.RaycastNonAlloc(pos, dir, hit2Ds, distance, 1 << LayerMask.NameToLayer(layerName));
            if (count > 0)
            {
                if (tag != null)
                {
                    var t = hit2Ds[0].transform;
                    do
                    {
                        if (t.CompareTag(tag)) return true;
                        t = t.parent;
                    } while (t != null);

                    return false;
                }

                return true;
            }

            return false;
        }

        public static int GetLayerID(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }

        /// <summary>
        /// 抛物线运动上升部分
        /// </summary>
        /// <param name="startPos">当前位置</param>
        /// <param name="apexPos">最高点</param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 CalculateParabolicPositionUp(Vector2 startPos, Vector2 apexPos, float t)
        {
            // 横向位置：使用线性插值，从起点到最高点的 x 坐标
            float x = Mathf.Lerp(startPos.x, apexPos.x, t);

            // 纵向位置：利用正弦函数模拟上升抛物线，t=0时为起点，t=1时正好到达最高点
            float y = startPos.y + Mathf.Sin(t * Mathf.PI / 2) * (apexPos.y - startPos.y);
            return new Vector2(x, y);
        }

        /// <summary>
        /// 抛物线运动下降部分
        /// </summary>
        /// <param name="apexPos">当前位置</param>
        /// <param name="endPos"> 落点</param>
        /// <param name="t"> 时间归一化</param>
        /// <param name="gravity">重力</param>
        /// <returns></returns>
        public static Vector2 CalculateParabolicPositionFall(Vector2 apexPos, Vector2 endPos, float t, float gravity = -9.8f)
        {
            float deltaY = apexPos.y - endPos.y;

            if (t <= 1f)
            {
                // 曲线下降部分（t ∈ [0,1]）
                float curveValue = Mathf.Cos(t * Mathf.PI / 2); // t=0时为1，t=1时为0
                float y = endPos.y + curveValue * deltaY;       // 从 apexPos.y 平滑过渡到 endPos.y
                float x = Mathf.Lerp(apexPos.x, endPos.x, t);   // 水平线性插值
                return new Vector2(x, y);
            }
            else
            {
                // 自由落体（t > 1）
                float extraTime = t - 1f;
                float y = endPos.y + 0.5f * gravity * extraTime * extraTime;
                float x = endPos.x;
                return new Vector2(x, y);
            }
        }
        
        /// <summary>
        ///  线性插值
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 CalculateLinearPosition(Vector2 startPos, Vector2 endPos, float t)
        {
            // Lerp handles clamping t between 0 and 1.
            return Vector2.Lerp(startPos, endPos, t);
        }
    }
}