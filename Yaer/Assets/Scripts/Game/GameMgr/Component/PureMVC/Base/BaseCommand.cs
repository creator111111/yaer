using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

namespace Game.GameMgr.Component.PureMVC.Base
{
    public class BaseCommand<TK> : SimpleCommand where TK : class
    {
        public GameManager gameManager;

        public BaseCommand()
        {
            gameManager = GameManager.Instance;
        }

        protected T GetProxy<T>() where T : class
        {
            return GameFacade.Instance.RetrieveProxy(nameof(T)) as T;
        }

        protected T GetMediator<T>() where T : class
        {
            return GameFacade.Instance.RetrieveMediator(nameof(T)) as T;
        }

        protected TK GetBody(INotification notification)
        {
            return notification.Body as TK;
        }

        public override void Execute(INotification notification)
        {
        }
    }
}