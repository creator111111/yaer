using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.SubManager.Buff;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class BuffComponentGSM : BaseComponentGSM
    {
        public T AddBuff<T>(ISceneObject obj) where T : class, IBuff, new()
        {
            return SceneManager.GetSubManager<BuffManager>().AddBuff<T>(obj);
        }

        public void RemoveBuff(ISceneObject obj, string buffName)
        {
            SceneManager.GetSubManager<BuffManager>().RemoveBuff(obj, buffName);
        }

        public T GetBuff<T>(ISceneObject obj) where T : IBuff
        {
            return SceneManager.GetSubManager<BuffManager>().GetBuff<T>(obj);
        }
    }
}