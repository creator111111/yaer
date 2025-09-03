using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("GameObject")]
    [Name("修改某个GameObj的旋转度")]
    public class ChangeGameObjRotation : ActionTask
    {

        [RequiredField]
        public BBParameter<GameObject> myObj;
        public BBParameter<float> targetRotationX;
        public BBParameter<float> targetRotationY;
        public BBParameter<float> targetRotationZ;
        protected override string info
        {
            get { return "将" + myObj + "的旋转度设置为" + "(" + targetRotationX + "," + targetRotationY + "," + targetRotationZ + ")"; }
        }

        protected override void OnExecute()
        {
            if (myObj.value == null) {
                Debug.LogError("====================设置GameObject旋转度时未输入正确的对象");
                return; 
            }
            myObj.value.transform.eulerAngles = new Vector3(targetRotationX.value, targetRotationY.value, targetRotationZ.value);
            EndAction();
        }
    }
}