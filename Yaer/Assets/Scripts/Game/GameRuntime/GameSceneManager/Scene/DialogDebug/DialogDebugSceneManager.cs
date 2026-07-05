using System;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Name.Res;

namespace Game.GameRuntime.GameSceneManager.Scene.DialogDebug
{
    /// <summary>
    /// 对话预制体专用测试场景 <see cref="SceneName.DialogDebug"/>。
    /// 职责：接入 GF 场景管线并提供 <see cref="Component.Story.StoryComponentGSM"/>；
    /// 不创建玩家、不播 BGM、不在进场景时自动触发主线剧情。
    /// </summary>
    /// <remarks>
    /// 替代方案：复用 <c>SelectClothesSceneManager</c> 仅改场景名 —— 会带上换装模块与进场景自动对话，污染测试，故不采用。
    /// </remarks>
    [Obsolete("DialogDebug 已改为解耦沙盒（DialogDebugPlayground），不再使用 GF SceneManager。见架构文档 2026-05-25 修订。")]
    public class DialogDebugSceneManager : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();

            // 与 SceneName 常量、DialogDebug.unity 文件名一致（方案 A：扁平路径）
            nowSceneName = SceneName.DialogDebug;
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            // 不在此触发 TriggerStory —— 由 DialogDebugStoryTester / UI 按钮控制，避免与 Inspector 配置耦合
        }

        /// <summary>测试场景无怪物生成。</summary>
        public override void initAllSceneMonster()
        {
        }
    }
}
