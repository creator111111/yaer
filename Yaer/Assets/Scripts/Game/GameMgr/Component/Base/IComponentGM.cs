namespace Game.GameMgr.Component.Base
{
    public interface IComponentGM
    {
        void OnInit();
        void OnEnter();
        void OnUpdate();
        void OnExit();
        void OnShutDown();
    }
}