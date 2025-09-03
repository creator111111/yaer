using Game.GameMgr.Component.Base;
using Game.GameMgr.Component.PureMVC.Base;

namespace Game.GameMgr.Component.PureMVC
{
    public class MVCComponentGM : BaseComponentGM
    {
        private GameFacade Facade => GameFacade.Instance;

        public T GetProxy<T>() where T : BaseProxy, new()
        {
            if (Facade.RetrieveProxy(typeof(T).Name) is T proxy)
            {
                // 每次获取都enter
                proxy.OnEnter();
                return proxy;
            }

            proxy = new T();
            proxy.OnInit();
            proxy.OnEnter();
            Facade.RegisterProxy(proxy);

            return proxy;
        }

        public void RemoveProxy<T>()
        {
            Facade.RemoveProxy(typeof(T).Name);
        }

        public void RemoveProxy(BaseProxy proxy)
        {
            Facade.RemoveProxy(proxy.ProxyName);
        }
    }
}