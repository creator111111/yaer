using PureMVC.Patterns.Proxy;

namespace Game.GameMgr.Component.PureMVC.Base
{
    public class BaseProxy : Proxy
    {
        public BaseProxy() : base(NAME)
        {
            ProxyName = GetType().Name;
        }
        public BaseProxy(string proxyName, object data = null) : base(proxyName, data)
        {
            ProxyName = GetType().Name;
        }

        public virtual void OnInit()
        {
            
        }

        public virtual void OnEnter()
        {
            
        }

        public virtual void OnDispose()
        {
            
        }
    }
}