using Cysharp.Threading.Tasks;
using Game.GameRuntime.UI.FormLogic.Story;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Dialogue")]
    public class MetaDialogueMoveNextActionTask : ActionTask
    {
        public BBParameter<MetaDialogue> metaDialogue;
        public BBParameter<float> FadeTime;
        public BBParameter<float> StayTime;

        protected override void OnExecute()
        {
            MoveNext().Forget();
        }

        private async UniTask MoveNext()
        {
            metaDialogue.value.Next(FadeTime.value, StayTime.value);
            await UniTask.WaitForSeconds(2 * FadeTime.value + StayTime.value);
            EndAction();
        }
    }
}