using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Story")]
    [Name("玩家操作提示")]
    // 用于剧情对话系统中的事件处理
    public class PlayerGuideStoryAction : ActionTask
    {
        public BBParameter<string> playerActName; // 玩家操作的名称

        protected override string OnInit()
        {
            
            return base.OnInit();
        }

        protected override string info
        {
            get
            {
                return "提示玩家执行" + playerActName.value + "动作";
            }
        }

        protected override void OnExecute()
        {
            // 事件管理器开始执行对应逻辑
            PlayerGuideMgr.getInstance().PraseActName(playerActName.value);
            
            EndAction();
        }
    }
}