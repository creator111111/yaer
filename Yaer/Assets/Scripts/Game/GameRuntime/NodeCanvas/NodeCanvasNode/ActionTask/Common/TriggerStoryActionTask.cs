using Cysharp.Threading.Tasks;
using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component.Story;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Dialogue")]
    [Name("触发剧情")]
    public class TriggerStoryActionTask : ActionTask
    {
        public BBParameter<string> StoryPrefabName;

        protected override void OnExecute()
        {
            base.OnExecute();
            TriggerStory().Forget();
            EndAction();
        }

        private async UniTask TriggerStory()
        {
            await UniTask.WaitForSeconds(0.75f);
            GameManager.GetGameSceneManager().GetModule<StoryComponentGSM>().TriggerStory(StoryPrefabName.value, true);
        }

        protected override string info
        {
            get => string.Format("<i>' 触发剧情: {0}'</i>", StoryPrefabName);
        }
    }
}