using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Story")]
    [Name("章节结束")]
    // 用于剧情对话系统中的事件处理
    public class ChapterEndAction : ActionTask
    {
        public BBParameter<int> chapterId; // 章节ID

        protected override string OnInit()
        {
            
            return base.OnInit();
        }

        protected override string info { 
            get
            {
                return chapterId.value == 0 ? "序章结束" : "章节" + chapterId.value + "结束";
            }
        }

        protected override void OnExecute()
        {
            // 打开章节结束界面
            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("ChapterEndPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs()
            {

            });

            EndAction();
        }
    }
}