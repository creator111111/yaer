namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// CSV 单行对白数据，对应表头：ID, Type, Speaker, Text, Next, Extra, FaceType。
    /// 由 <see cref="DialogueCsvParser"/> 填充，供 <see cref="DialogueCsvGraphBuilder"/> 消费。
    /// </summary>
    public class DialogueRow
    {
        /// <summary>行 ID，全表唯一，用于 Next 列引用。</summary>
        public int id;

        /// <summary>节点类型：Dialogue（对白）或 Choice（分支选项）。</summary>
        public string type;

        /// <summary>策划简称说话人（如「雅」「古」），需经 <see cref="DialogueSpeakerMapping"/> 映射为图内 Actor 名。</summary>
        public string speaker;

        /// <summary>对白正文；Choice 行可作节点注释。</summary>
        public string text;

        /// <summary>
        /// 下一跳：单个 ID、「4|5」多分支、END 或空（表示结束，不连出边）。
        /// </summary>
        public string next;

        /// <summary>Choice 专用：选项文案，竖线分隔，如「商店|离开」。</summary>
        public string extra;

        /// <summary>
        /// CSV 第 7 列：DialogueFaceType 枚举名（如 Smile）。空串表示走说话人默认。
        /// Choice 行可忽略。
        /// </summary>
        public string faceType;
    }
}
