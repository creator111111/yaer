namespace Game.GameMgr.Component.Story
{
    public interface IStory
    {
        void OnInit(object userData);
        void OnEnter(object userData);
        void OnUpdate();
        void OnExit();
    }
}