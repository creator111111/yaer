using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Component;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("UIPanel")]
    [Name("œ‘ æTipsInfo")]
    public class AddTipsInfoActionTask : ActionTask
    {
        public BBParameter<string> TipKey;

        protected override void OnExecute()
        {
            GameManager.GetGameSceneManager().GetModule<TipsComponentGSM>().OpenTipsForm(TipKey.value, UI.FormLogic.Tips.ETipsType.Info);
            EndAction();
        }

        protected override string info
        {
            get
            {
                return string.Format("<i>' œ‘ æTipsInfo: {0}'</i>", TipKey);
            }
        }
    }
}