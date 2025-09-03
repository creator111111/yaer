using System;

namespace Game.GameMgr.Manager.Config
{
    public class IConfigManager
    {
        public bool ParseData(byte[] bytes, object userData)
        {
            throw new NotImplementedException();
        }

        public bool ParseData(string bytes, object userData)
        {
            throw new NotImplementedException();
        }

        public bool ParseData(byte[] configBytes, int startIndex, int length, object userData)
        {
            throw new NotImplementedException();
        }

        public bool AddConfig(string configName, string configValue)
        {
            throw new NotImplementedException();
        }
    }
}