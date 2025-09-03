using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("GameObject")]
    public class FindWithTransform : ActionTask
    {

        [RequiredField]
        public BBParameter<string> transformName;
        [RequiredField]
        public BBParameter<string> gameObjectName;
        [BlackboardOnly]
        public BBParameter<GameObject> saveAs;

        protected override string info
        {
            get { return "Find Object " + gameObjectName + " by " + transformName + " as " + saveAs; }
        }

        protected override void OnExecute()
        {

            saveAs.value = GameObject.Find(transformName.value).transform.Find(gameObjectName.value).gameObject;
            EndAction();
        }
    }
}