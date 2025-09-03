using System.Collections.Generic;
using Game.GameMgr.Manager.Settings.interf;
using UnityEngine;

namespace Game.GameMgr.Manager.Settings.Helper
{


    public class SettingManagerVisionHelper : MonoBehaviour, ISettingManagerVisionHelper
    {
        private Dictionary<string, string> infoDic = new Dictionary<string, string>();

        public virtual void UpdateVision(object info)
        {
            if (info is Dictionary<string, string>) infoDic = (Dictionary<string, string>)info;
        }

        public void AddInfo(string key, object value)
        {
            if (key.Contains(key))
                infoDic[key] = value.ToString();
            else
                infoDic.Add(key, value.ToString());
        }
    }
}