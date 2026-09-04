namespace Game.Static.Name.Res
{
    public static class SceneName
    {
        public const string Editor = "Editor";
        public const string Archive = "Archive";
        public const string InitScene = "InitScene";
        public const string StartScene = "StartScene";
        public const string NewGameScene = "NewGameScene";
        public const string HomeScene1 = "HomeScene1";
        public const string HomeScene2 = "HomeScene2";
        public const string SelectClothesScene = "SelectClothesScene";
        /// <summary>
        /// ���ǽ�
        /// </summary>
        public const string ForestScene = "ForestScene";
        /// <summary>
        /// ���Ƕ���
        /// </summary>
        public const string ForestEastScene = "ForestEastScene";
        /// <summary>
        /// �Դ�����
        /// </summary>
        public const string VerdantCorridor = "VerdantCorridor";
        /// <summary>
        /// ����·��
        /// </summary>
        public const string WestRappRoad = "WestRappRoad";

        /// <summary>
        /// ������ߣ���ķ���ׯ��������Դ���� <c>Assets/GameRes/Scenes/Village_KenMuNi1.unity</c> һ�£���ͼ <c>ButtonJingLingVillage</c> ֱ��Ŀ�꣬���������л���Ի�������ת_�ܹ���Դ���桷��7����
        /// </summary>
        public const string Village_KenMuNi1 = "Village_KenMuNi1";

        /// <summary>
        /// 肯姆尼村民居室内（<c>Assets/GameRes/Scenes/Village_House4.unity</c>）；由村里 House4 门进入。
        /// </summary>
        public const string Village_House4 = "Village_House4";

        /// <summary>
        /// 肯姆尼第二户民居室内（<c>Assets/GameRes/Scenes/Village_HomeScene2.unity</c>）；由村里 House_NPC2 进入。
        /// </summary>
        public const string Village_HomeScene2 = "Village_HomeScene2";

        /// <summary>
        /// 肯姆尼第一户民居室内（<c>Assets/GameRes/Scenes/Village_HomeScene1.unity</c>）；由村里 House_Npc1 进入。
        /// 勿与龙宫 <see cref="HomeScene1"/> / <c>HomeScene1Manager</c> 混淆。
        /// </summary>
        public const string Village_HomeScene1 = "Village_HomeScene1";

        /// <summary>
        /// 肯姆尼民居室内（<c>Assets/GameRes/Scenes/Village_HomeScene23.unity</c>）；由村里 House_Npc4 进入。
        /// 曾用名 Village_HomeScene4（2026-08-04 改名）。勿与 <see cref="Village_House4"/>、
        /// <see cref="Village_HomeScene2"/> 或 <see cref="Village_HomeScene45"/> 混淆。
        /// </summary>
        public const string Village_HomeScene23 = "Village_HomeScene23";

        /// <summary>
        /// 肯姆尼 45 号民居室内（<c>Assets/GameRes/Scenes/Village_HomeScene45.unity</c>）；由村里 House_Npc45 进入。
        /// 曾用名 Village_HomeScene3（2026-08-20 改名）。专用 <c>Village_HomeScene45SceneManager</c>，
        /// 勿挂龙宫 <c>HomeScene1Manager</c>。
        /// </summary>
        public const string Village_HomeScene45 = "Village_HomeScene45";

        /// <summary>
        /// 肯姆尼村长家室内（<c>Assets/GameRes/Scenes/Village_Chief_House.unity</c>）；由村里 House_Chief 进入。
        /// </summary>
        public const string Village_Chief_House = "Village_Chief_House";

        /// <summary>
        /// 从村长家 <c>LeftDoor</c>（1 楼大门）回村时的 EnterPos 键（E3′）。
        /// 与真实场景名 <see cref="Village_Chief_House"/>（楼梯上楼→2 楼）拆开，避免抢同一落点。
        /// </summary>
        public const string Village_Chief_House_Door = "Village_Chief_House_Door";

        /// <summary>
        /// 是否启用村庄探索移动（Town / WalkArea / 纵深 Y）。
        /// 原因（0901）：原闸仅认 <see cref="Village_KenMuNi1"/>，进屋无 W/S；
        /// 现白名单仅再加 <see cref="Village_Chief_House"/>，其它 Home 仍 Default。
        /// <para>替代方案：各场景脚本地名字符串比对——易散落魔法字符串，故集中于此。</para>
        /// </summary>
        /// <param name="sceneName">激活场景名（通常 <c>SceneManager.GetActiveScene().name</c>）</param>
        public static bool IsVillageExplorationScene(string sceneName)
        {
            return sceneName == Village_KenMuNi1 || sceneName == Village_Chief_House;
        }

        /// <summary>
        /// 是否「室内」村探索（开 Town/2.5D 但平面速应对齐 Home walk）。
        /// 原因（0901）：<see cref="Village_Chief_House"/> 为楼梯进白名单后误吃村街 <c>villagePlanarMoveSpeed=11.2</c>；
        /// 其它 Home 不在此列。勿把 Home1/2 等扩进来除非产品明确要室内 2.5D。
        /// </summary>
        public static bool IsIndoorVillageExplorationScene(string sceneName)
        {
            return sceneName == Village_Chief_House;
        }

        /// <summary>
        /// 肯姆尼村外（<c>Assets/GameRes/Scenes/Village_OutSide.unity</c>）；由村里 MapRight/RightDoor 进入。
        /// </summary>
        public const string Village_OutSide = "Village_OutSide";

        /// <summary>
        /// 肯姆尼村商店（纯 UI 场景，<c>Assets/GameRes/Scenes/Village_Shop.unity</c>）；
        /// 由村里 Door_Shop 进入；不生成玩家。
        /// </summary>
        public const string Village_Shop = "Village_Shop";

        /// <summary>
        /// 对话预制体 Debug 场景（<c>Assets/GameRes/Scenes/DialogDebug.unity</c>，方案 A 扁平路径）。
        /// </summary>
        public const string DialogDebug = "DialogDebug";
    }
}