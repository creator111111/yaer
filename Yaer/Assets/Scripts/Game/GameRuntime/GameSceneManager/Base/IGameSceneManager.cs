using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Config;

namespace Game.GameRuntime.GameSceneManager.Base
{
    public interface IGameSceneManager
    {
        GameSceneManagerConfig Config { get; }
        // gm
        bool Pause { get; }

        public void SetSceneObjIsPause(bool value);
        public bool GetSceneObjIsPause();
        
        T GetModule<T>() where T : class, IComponentGSM;

        // ------------------------------------------------------------------------
        // data
        T GetArchiveData<T>() where T : BaseArchiveData, new();

        // ------------------------------------------------------------------------
        // 生命周期
        void OnInit();
        void OnEnterScene();
        void OnExitScene();
        void OnUpdate();
        void OnShutDown();
    }
}