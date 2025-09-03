using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("GameObject")]
    [Name("获取组件")]
    public class GetChildComponentActionTask<T> : ActionTask where T: UnityEngine.Component
    {
        public BBParameter<GameObject> RootGO;

        [RequiredField]
        public BBParameter<string> childPath;

        [BlackboardOnly]
        public BBParameter<T> saveAs;

        protected override string info
        {
            get { return string.Format("获取组件 {0} = {1}.FindChild({2}).GetComponent<{3}>()", saveAs, RootGO, childPath, typeof(T).Name); }
        }

        protected override void OnExecute()
        {
            if (RootGO.value == null)
            {
                Debug.LogError("RootGO.value == null");
            }
            Transform childTf = RootGO.value.transform.Find(childPath.value);
            if (childTf == null)
            {
                Debug.LogError($"没有找到子物体");
            }

            var result = childTf.GetComponent<T>();
            if (result == null)
            {
                Debug.LogError("没有找到组件" + typeof(T).Name);
            }
            saveAs.value = result;
            EndAction();
        }
    }
}