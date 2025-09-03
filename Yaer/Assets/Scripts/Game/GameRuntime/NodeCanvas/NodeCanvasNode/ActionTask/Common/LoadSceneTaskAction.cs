using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Name("加载场景")]
    public class LoadSceneTaskAction : ActionTask
    {
        public BBParameter<string> SceneName;
        public BBParameter<bool> EndActionOnFinishLoad;

        protected override void OnExecute()
        {
            base.OnExecute();
            var sceneMgr = GameManager.GetGameSceneManager().GetModule<LoadSceneComponentGSM>();
            if (EndActionOnFinishLoad.value)
            {
                sceneMgr.LoadScene(SceneName.value, EndAction);
            }
            else
            {
                sceneMgr.LoadScene(SceneName.value);
                EndAction();    
            }
        }

        protected override string info
        {
            get
            {
                return string.Format("<i>' 加载场景: {0} '</i>", SceneName);
            }
        }
    }
}