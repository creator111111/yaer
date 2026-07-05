using UnityEngine;

namespace GameDebug
{
    /// <summary>
    /// 历史验收用组件：曾在 <see cref="Update"/> 中每帧输出 <c>[VillageMoveDebug]</c> 控制台日志。
    /// 该行为已移除，仅保留空壳 <see cref="MonoBehaviour"/>，避免破坏 Easy Save 全局引用、旧场景等对脚本 GUID 的登记；
    /// 若需再次验收输入，请改用 Profiler / 临时断点或在独立分支恢复实现。
    /// </summary>
    public class VillageMovementInputDebug : MonoBehaviour
    {
    }
}
