using Game.GameRuntime.GameSceneManager.Base.Editor;
using UnityEditor;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_House.Editor
{
    /// <summary>
    /// <see cref="Village_House4SceneManager"/> 场景 Inspector；与 <c>HomeScene1MgrInsp</c> 一致，复用 GSM 基类绘制。
    /// </summary>
    [CustomEditor(typeof(Village_House4SceneManager), true)]
    public class Village_House4MgrInsp : BaseGameSceneMgrInspector
    {
    }
}
