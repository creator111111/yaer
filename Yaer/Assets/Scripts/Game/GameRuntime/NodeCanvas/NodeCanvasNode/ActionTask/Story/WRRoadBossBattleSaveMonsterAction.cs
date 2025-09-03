using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Story")]
    [Name("记录序章是否拯救哥布林")]
    // 用于剧情对话系统中的事件处理
    public class WRRoadBossBattleSaveMonsterAction : ActionTask
    {
        public BBParameter<bool> hasSave;

        protected override string OnInit()
        {
            
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            // 事件管理器开始执行对应逻辑
            WestRappRoadBossBattleMgr.getInstance().SetHasSaveMonster(hasSave.value);
            EndAction();
        }
    }
}