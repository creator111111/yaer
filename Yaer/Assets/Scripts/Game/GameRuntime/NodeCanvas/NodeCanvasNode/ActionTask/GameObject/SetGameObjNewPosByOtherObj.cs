using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("GameObject")]
    [Name("设置某个GameObj到另一个GameObj的坐标")]
    public class SetGameObjNewPosByOtherObj : ActionTask
    {

        [RequiredField]
        public BBParameter<GameObject> myObj;
        [RequiredField]
        public BBParameter<GameObject> targetObj;
        public BBParameter<bool> isOnlySetPosX; // 是否只设置X坐标
        protected override string info
        {
            get { return "将" + myObj + "设置到" + targetObj + "的位置"; }
        }

        protected override void OnExecute()
        {
            if (myObj.value == null ||  targetObj.value == null) {
                Debug.LogError("====================设置GameObject坐标时未输入正确的对象");
                return; 
            }
            var targetPos = targetObj.value.transform.position;
            if (isOnlySetPosX.value)
            {
                var oldPos = myObj.value.transform.position;
                myObj.value.transform.position = new Vector2(targetObj.value.transform.position.x, oldPos.y);
            }
            myObj.value.transform.position = targetPos;
            EndAction();
        }
    }
}