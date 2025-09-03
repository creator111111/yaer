using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Story")]
    [Name("东城郊-史莱姆吸食死羊事件处理")]
    // 用于剧情对话系统中的事件处理
    public class SlimeEatSheepStoryAction : ActionTask
    {
        public BBParameter<string> eventArgs; // 事件参数

        protected override string OnInit()
        {
            
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            // 事件管理器开始执行对应逻辑
            SlimeEatSheepStoryMgr.getInstance().ParseStoryAcitonArgs(eventArgs.value);
            EndAction();
        }
    }
}