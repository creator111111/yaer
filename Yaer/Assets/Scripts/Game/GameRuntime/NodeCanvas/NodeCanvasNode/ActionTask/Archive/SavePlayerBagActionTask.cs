using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// 对话树 Action：立刻将 <c>PlayerBagData</c> 写入当前存档（<see cref="QuestManager.SavePlayerBag"/>）。
    /// <para>
    /// 重要原因：<c>AcceptQuest</c> 会马上 <c>SaveQuestProgress</c>（整份盘），若随后
    /// <c>GetItemActionTask</c> 只改内存不落盘，读档会出现「已接任务但空桶没了」。
    /// 老农线：挂在发空桶×4 / Tips 之后，对齐商店买完与井换桶后的存包。
    /// </para>
    /// 替代方案：给 GetItem 加 <c>saveAfter</c>——会动全局节点面；本 Task 显式、少误伤。
    /// </summary>
    [Category("Archive")]
    [Name("保存玩家背包")]
    public class SavePlayerBagActionTask : ActionTask
    {
        protected override string info => "<i>' SavePlayerBag '</i>";

        protected override void OnExecute()
        {
            QuestManager.getInstance().SavePlayerBag();
            EndAction(true);
        }
    }
}
