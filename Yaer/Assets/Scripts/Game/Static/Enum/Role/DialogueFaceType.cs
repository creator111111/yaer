namespace Game.Static.Enum.Dialogue
{
    /// <summary>
    /// 对话表情类型。名称须与各角色 Painting Prefab 的 Faces 子物体名、
    /// SayEx/CSV FaceType 字符串一致（古莎等角色键 = ToString()）。
    /// 新增成员只能追加在末尾，禁止插入中间，以免打乱已序列化的 int 值。
    /// </summary>
    public enum DialogueFaceType
    {
        None,
        Unhappy,
        Angry,
        Smug,
        Smile,
        Surprised,
        Laugh,
        Sad,
        VerySurprised,
        Daze,
        Happy,
        CloseEyes,
        Normal,
        NormalSmile,
        CloseEyesSmile,
        Shout,
        Amazed,
        AmazedShout,
        OEyesOMouth,
        CEyesOMouth,
        OEyesCMouth,
        CEyesCMouth,
        ChiBie,
        Awkward,
        Cry,
        ForcedSmile,
        Hurt,
        Scared,
        /// <summary>古莎「落寞」；Faces/LuoMo，绑图落寞.png。不入 spcFaces（正常衣）。</summary>
        LuoMo,
        /// <summary>古莎「落寞2」变体；Faces/LuoMo2，绑图落寞2.png。不入 spcFaces（正常衣）。</summary>
        LuoMo2,
        // —— 以下为雅儿 GoOut 批量新表情（0804）；键 = Armor_NoHeadWear_{名}，与 Prefab Faces 一致 ——
        /// <summary>雅儿吃瘪2；Faces/Armor_NoHeadWear_ChiBie2。</summary>
        ChiBie2,
        /// <summary>雅儿吃瘪3；Faces/Armor_NoHeadWear_ChiBie3。</summary>
        ChiBie3,
        /// <summary>雅儿尴尬；Faces/Armor_NoHeadWear_GanGa。勿与古莎 Awkward 混淆。</summary>
        GanGa,
        /// <summary>雅儿忍耐；Faces/Armor_NoHeadWear_RenNai。</summary>
        RenNai,
        /// <summary>雅儿难过；Faces/Armor_NoHeadWear_NanGuo。独立于 Sad。</summary>
        NanGuo,
        /// <summary>雅儿难过2；Faces/Armor_NoHeadWear_NanGuo2。</summary>
        NanGuo2,
        /// <summary>雅儿难过3；Faces/Armor_NoHeadWear_NanGuo3。</summary>
        NanGuo3,
        /// <summary>雅儿难过4；Faces/Armor_NoHeadWear_NanGuo4。</summary>
        NanGuo4,
        /// <summary>雅儿震惊；Faces/Armor_NoHeadWear_ZhenJing。独立于 VerySurprised。</summary>
        ZhenJing,
        /// <summary>雅儿震惊2；Faces/Armor_NoHeadWear_ZhenJing2。</summary>
        ZhenJing2
    }
}