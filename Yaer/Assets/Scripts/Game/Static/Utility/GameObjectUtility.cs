using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.Static.Utility
{
    public class GameObjectUtility
    {
        public static Transform GetParentTsf(Transform tsf, string rootName = null, string tag = null)
        {
            while (true)
            {
                if (tsf.parent == null)
                {
                    if (tag != null)
                    {
                        if (tsf.CompareTag(tag)) return tsf;

                        return null;
                    }

                    return tsf;
                }

                // 剔除根节点
                if (rootName != null && tsf.parent.name == rootName)
                {
                    if (tag != null)
                    {
                        if (tsf.CompareTag(tag)) return tsf;

                        return null;
                    }

                    return tsf;
                }

                tsf = tsf.parent;
            }
        }

        public static T GetParentComponent<T>(Transform tsf, string rootName = null, string tag = null) where T : ISceneObject
        {
            var parentTsf = GetParentTsf(tsf, rootName, tag);
            if (parentTsf is null) return default;
            return parentTsf.GetComponent<T>();
        }
    }
}