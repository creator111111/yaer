using PureMVC.Patterns.Facade;

namespace Game.GameMgr.Component.PureMVC
{
    public class GameFacade : Facade
    {
        // 提供外部访问方法
        public static GameFacade Instance
        {
            get
            {
                if (instance == null) instance = new GameFacade();
                return instance as GameFacade;
            }
        }
    }
}