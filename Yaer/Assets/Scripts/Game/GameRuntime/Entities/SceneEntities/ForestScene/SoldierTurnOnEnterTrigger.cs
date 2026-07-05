using UnityEngine;



namespace Game.GameRuntime.Entities.SceneEntities.ForestScene

{

    /// <summary>

    /// 已弃用：士兵「回头」现仅由 HomeDoor/国王演出相关 <b>TimeLine</b>（Animation 轨、Signal 等）控制，

    /// 勿再用触发器在代码里 <c>SetTrigger("Turn")</c>。本组件保留空实现，避免已挂接场景报 MissingScript；可安全从物体上移除。

    /// </summary>

    [DisallowMultipleComponent]

    public class SoldierTurnOnEnterTrigger : MonoBehaviour

    {

    }

}


