using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Story")]
    [Name("序章BOSS战事件")]
    // 用于剧情对话系统中的事件处理
    public class WestRappRoadBossBattleAction : ActionTask
    {
        public BBParameter<string> eventArgs; // 是否开始事件

        protected override string OnInit()
        {
            
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            // 事件管理器开始执行对应逻辑
            WestRappRoadBossBattleMgr.getInstance().CheckEventArgs(eventArgs.value);
            EndAction();
        }
    }
}