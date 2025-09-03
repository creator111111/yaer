using UnityEngine;

namespace Game.GameMgr.Manager.Res.UI.interf
{
    public interface IUIResManager
    {
        GameObject Get(params string[] strings);
    }
}