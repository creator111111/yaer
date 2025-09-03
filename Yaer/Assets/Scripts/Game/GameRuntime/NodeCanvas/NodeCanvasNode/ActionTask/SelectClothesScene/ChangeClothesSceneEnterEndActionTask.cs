using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("SelectClothesScene")]
    public class ChangeClothesSceneEnterEndActionTask : ActionTask
    {
        protected override void OnExecute()
        {
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SelectClothesPanel"), EUIGroup.Bottom, new OpenFormArgs());
            EndAction();
        }
    }
}