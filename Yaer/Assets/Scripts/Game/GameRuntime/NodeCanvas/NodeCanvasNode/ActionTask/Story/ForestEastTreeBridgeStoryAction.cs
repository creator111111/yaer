using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Story")]
    [Name("树洞事件开始或结束")]
    // 用于剧情对话系统中的事件处理
    public class ForestEastTreeBridgeStoryAction : ActionTask
    {
        public BBParameter<bool> isStart; // 是否开始事件

        protected override string OnInit()
        {
            
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            // 事件管理器开始执行对应逻辑
            ForestEastTreeBridgeStoryMgr.getInstance().OnSceneStoryTrigger(isStart.value);
            EndAction();
        }
    }
}