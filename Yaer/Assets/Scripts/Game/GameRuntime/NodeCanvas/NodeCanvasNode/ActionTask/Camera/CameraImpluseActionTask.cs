using Cysharp.Threading.Tasks;
using Game.GameRuntime.Component;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("Camera")]
    [Name("ÆÁÄ»Õð¶¯")]
    public class CameraImpluseActionTask : ActionTask
    {
        public BBParameter<CameraImpluseTrigger> ImpluseTrigger;
        public BBParameter<float> DelayEndAction;

        protected override void OnExecute()
        {
            Impluse().Forget();
        }

        private async UniTask Impluse()
        {
            ImpluseTrigger.value.CameraImpulse();
            await UniTask.WaitForSeconds(DelayEndAction.value);
            EndAction();
        }
    }
}