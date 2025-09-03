using UnityEngine;

namespace Game.Static.Utility
{
    public static class MathUtility
    {
        /// <summary>
        /// 计算 p1 在 p2 Y方向上的投影点坐标
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Vector3 GetP1ToP2YProjectionPoint(Vector3 p1, Vector3 p2) => new Vector3(p2.x, p1.y, p2.z);
    }
}