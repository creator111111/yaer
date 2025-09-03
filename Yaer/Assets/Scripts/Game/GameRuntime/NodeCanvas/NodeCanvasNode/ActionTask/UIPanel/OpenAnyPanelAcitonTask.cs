using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;

namespace Game.GameRuntime.Story.Node
{
    [Category("UI")]
    [Name("打开任意界面")]
    public class OpenAnyPanelAcitonTask : ActionTask
    {
        public BBParameter<string> panelName;
        public BBParameter<List<int>> argsListInt;

        object panelArgsDatas;
        protected override string OnInit()
        {
            switch (panelName.value) {
                case "ControlTipsPanel": // 打开人物操作提示界面
                    panelArgsDatas = argsListInt.value;
                    break;
                default:
                    panelArgsDatas = null;
                    break;
            }


            return base.OnInit();
        }
        protected override string info
        {
            get
            {
                return string.Format("<i>' 打开界面: {0}'</i>", panelName.value);
            }
        }
        protected override void OnExecute()
        {
            string panelPath = UIPrefabPath.GetUIPrefabPath(panelName.value);
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(panelPath);
            if (uiForm == null)
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(panelPath, EUIGroup.Top, new OpenFormArgs()
                {
                    userData = panelArgsDatas,
                });
            }
            EndAction();
        }
    }
}