using Game.GameRuntime.GameSceneManager.Base;

namespace Game.GameRuntime.Entities.Base.BaseSceneObj.Base
{
    public interface ISceneObject : IMonoObject
    {
        string Name { get; }
        bool IsInit { set; }
        bool IsExist { get; }
        IGameSceneManager SceneManager { get; }

        /// <summary>
        ///     场景管理器Awake调用时手动管理初始化
        /// </summary>
        void Init(IGameSceneManager m);

        void OnSaveGame();
    }
}