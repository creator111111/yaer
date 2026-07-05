using Game.GameRuntime.GameSceneManager.Base.Editor;
using UnityEditor;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_House.Editor
{
    /// <summary>
    /// <see cref="Village_HomeScene2SceneManager"/> 场景 Inspector；复用 GSM 基类绘制。
    /// </summary>
    [CustomEditor(typeof(Village_HomeScene2SceneManager), true)]
    public class Village_HomeScene2MgrInsp : BaseGameSceneMgrInspector
    {
    }
}
