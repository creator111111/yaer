# Village_House4 场景管理器 — 架构溯源与执行说明

**文档性质**：架构侦探产出（只读分析 + 施工清单；**本文档不改工程代码/场景**）  
**依据**：`Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】；关联 `Assets/Doc/执行文档/0518/SceneChangeDoor场景切换_架构溯源与执行说明.md`、`Assets/Doc/执行文档/0512/新建肯尼姆场景管理器_程序施工执行说明.md`  
**Unity 版本**：2020.3.48f1  
**任务范围**：村庄 `Village_KenMuNi1` → 村民家 `Village_House4` 的室内场景接入；核心交付为 **`Village_House4SceneManager`**（参考 `HomeScene1Manager`）。

---

## 1. 结论（一句话）

**村里 House4 的门已指向 `Village_House4`，但室内场景仍挂着森林用的 `ForestSceneManager` + 战斗 Config，落点与出门仍写着 `HomeScene1` / `ForestEastScene`——必须新建室内专用 `Village_House4SceneManager`（仿 `HomeScene1Manager`），并同步改 Config、`EnterPosConfig` 与 `Map` 左右门，否则进门后是战斗跑姿、落点错误、出门回错图。**

---

## 2. 现状事实（静态阅读）

### 2.1 场景资源

| 项目 | 路径 / 值 |
|------|-----------|
| 室内场景 | `Assets/GameRes/Scenes/Village_House4.unity` |
| 户外村庄 | `Assets/GameRes/Scenes/Village_KenMuNi1.unity` |
| 场景名常量 | **`SceneName.cs` 中尚无 `Village_House4`**（门上已用字符串 `Village_House4`） |

`Village_House1`～`Village_House3` 同目录存在，但**本任务仅覆盖 House4**；1～3 若仍为森林管理器，可复用本文模板。

### 2.2 `Village_KenMuNi1`（来源侧）— 已具备

| 配置项 | 事实 |
|--------|------|
| 进村入口 | 预制体 `Assets/Prefabs/Stairs.prefab` 实例 **`House4`**，`NextSceneName = Village_House4`（PrefabInstance 覆盖） |
| 实体登记 | `House4` 父节点为 `SceneEntityComponentGSM.objRoot`（fileID `1948841490`），且在 `sceneObjs` 列表中 → **E 键交互链可用**（见 SceneChangeDoor 文档 §9） |
| 从室内返回落点 | `SceneManager.EnterPosConfig` 含 `lastScene: Village_House4` → `pos` 指向户外 Transform（与 `LeftBorn` 同侧坐标系） |

### 2.3 `Village_House4`（目标侧）— 缺口

| 配置项 | 当前值 | 问题 |
|--------|--------|------|
| `SceneManager` 脚本 | **`ForestSceneManager`**（guid `ed5ec3a145aaed44dab4717ce714e5a7`） | `OnInit` 写死 `nowSceneName = ForestScene`；户外战斗/森林 BGM 逻辑不适用室内 |
| `config` | **`ForestScene.asset`**（`isFightingScene: 1`） | 玩家加载 **Combat** 动画控制器，非 Home 室内走姿 |
| `GetCurSceneTerrainType` | 森林默认 **LandType**（若不改写） | 脚步声为土地跑，非室内 |
| `EnterPosConfig` | `lastScene: HomeScene1`、`ForestEastScene` | 从村里进门时 `LastSceneName = Village_KenMuNi1` **无匹配** → 落到 `DefaultBornPos`，门口落点不对 |
| `Map/MapRight/RightDoor` | `NextSceneName: ForestEastScene`，`TriggerWhenMoveIn: 1` | 走出触发区会加载错误场景 |
| `Map/MapLeft/LeftDoor` | `NextSceneName: HomeScene1` | 应回 **`Village_KenMuNi1`** |
| 场景内容 | 大量 `ForestScene*` 剧情触发器、怪物逻辑残留 | 室内 MVP 可暂留，但需知可能误触发森林对话；后续关卡清理 |

### 2.4 与 `HomeScene1` 的对照（仿照基准）

| 维度 | `HomeScene1` | 目标 `Village_House4` |
|------|--------------|------------------------|
| 管理器 | `HomeScene1Manager` | **`Village_House4SceneManager`**（新建） |
| Config | `HomeScene1.asset`（`isFightingScene: 0`） | **`Village_House4.asset`**（复制 HomeScene1 改参） |
| 地形音 | `IndoorType` | 同左 |
| 存档地点 | `PlaceName.Home` | 建议 **`PlaceName.KenMuNi`**（仍在肯姆尼村内，读档标题保持「肯姆尼」） |
| 场景专属存档类 | `HomeScene1Data` | **首版可不建**（无室内剧情旗标时） |
| 左右门解锁 | `SetSceneUnlockCondition` | 室内单出口可省略 |

---

## 3. 换场链路（本需求相关段）

```
Village_KenMuNi1 / House4 (Stairs + SceneChangeDoor)
  → LoadSceneComponentGSM.LoadScene("Village_House4", …)
  → ChangeSceneComponentGM.LastSceneName = "Village_KenMuNi1"
  → 加载 Village_House4.unity
  → Village_House4SceneManager.OnInit（待建）
      → nowSceneName = Village_House4
      → Config.isFightingScene == false → PlayerLogic 使用 Home 控制器
      → GetCurSceneTerrainType → IndoorType
  → SetPlayerPos：EnterPosConfig 匹配 lastScene == Village_KenMuNi1 → 室内门口 Transform

室内 LeftDoor（改配置后）
  → LoadScene("Village_KenMuNi1", …)
  → LastSceneName = Village_House4
  → Village_KenMuNi1.EnterPosConfig 已有 Village_House4 项 → 户外 House4 门口落点
```

**再次强调**：门上 `bornPos` **不参与**运行时坐标，仅作策划对照；落点以目标场景 `EnterPosConfig` 为准（见 SceneChangeDoor 文档 §4）。

---

## 4. `Village_House4SceneManager` 设计（施工员实现）

### 4.1 文件与命名

| 项 | 建议 |
|----|------|
| 目录 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/`（新建文件夹，与 `Home1/`、`Village_KenMuNi/` 并列） |
| 类名 | `Village_House4SceneManager` |
| 命名空间 | `Game.GameRuntime.GameSceneManager.Scene.Village_House` |
| Editor（可选） | `Village_House4MgrInsp.cs` — 空继承 `BaseGameSceneMgrInspector`，与 `HomeScene1MgrInsp` 相同 |

### 4.2 参考实现：`HomeScene1Manager` 应保留 / 应删减

**建议保留（室内最小集）**：

- `base.OnInit()` / `base.OnEnterScene()`
- `nowSceneName = SceneName.Village_House4`（常量需在 `SceneName.cs` 登记）
- `GetCurSceneTerrainType() => TerrainType.IndoorType`
- `initAllSceneMonster()` 空实现
- `GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi)` — 进村仍算肯姆尼区域存档名

**首版建议不做（无需求则不加）**：

- `HomeScene1Data` / 首次进入剧情 `TriggerStory`
- `SetSceneUnlockCondition` 右门解锁
- 雅尔显隐、`HomeScene1Xiaer` 类实体
- 龙宫 BGM（`龙宫内BGM.ogg`）— 若室内要独立 BGM，另在 `OnInit` 播放并在 `OnExitScene` 停播；否则沿用村庄户外 BGM 亦可

### 4.3 推荐代码骨架（含注释，施工员照抄后按 Inspector 微调）

```csharp
using Game.GameMgr.Component;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
// TerrainType 定义于 BaseGameSceneManager.cs 顶层（与 namespace 同级）

namespace Game.GameRuntime.GameSceneManager.Scene.Village_House
{
    /// <summary>
    /// 肯姆尼村民居 <see cref="SceneName.Village_House4"/> 室内场景管理器。
    /// 行为对齐 <see cref="Game.GameRuntime.GameSceneManager.Scene.Home1.HomeScene1Manager"/> 的「室内」最小集：
    /// Home 动画（Config.isFightingScene=false）、室内脚步、正确 nowSceneName。
    /// </summary>
    public class Village_House4SceneManager : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();

            // 逻辑自报场景名：供 LastSceneName 匹配、全局查询；勿使用 ForestScene。
            nowSceneName = SceneName.Village_House4;

            // 存档「当前地点」仍显示肯姆尼；若将来要「某某的家」单独地名，再增 PlaceName 常量。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();
            // 与 Village_KenMuNiSceneManager 类似，可在加载完成后再写一次 SetNowPlace，避免切场顺序覆盖。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        /// <summary>室内脚步资源：室内走{0}.mp3</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
```

**替代方案**：

| 方案 | 适用 | 风险 |
|------|------|------|
| **新建 `Village_House4SceneManager`（推荐）** | 正式村民家 | 需改场景挂载与 Config |
| 继续用 `ForestSceneManager` 仅改 Config 为 `isFightingScene=0` | 极临时试玩 | `nowSceneName` 仍为 `ForestScene`，存档/任务判断易错 |
| 与 `HomeScene1` 共用 `HomeScene1Manager` | — | **不可行**（`nowSceneName`、地点键、实体逻辑均写死 Home） |

---

## 5. 代码侧施工清单（程序）

- [ ] **`SceneName.cs`** 增加 `public const string Village_House4 = "Village_House4";`（与 `.unity` 文件名一致）。
- [ ] 新建 **`Village_House4SceneManager.cs`**（§4.3）。
- [ ] （可选）新建 **`Village_House4MgrInsp.cs`** CustomEditor。
- [ ] 复制 **`Assets/GameRes/Config/SceneManagerConfig/HomeScene1.asset`** → **`Village_House4.asset`**，确认 `isFightingScene: 0`，其余与 Home 室内一致。
- [ ] **不要修改** `ForestSceneManager.cs` 正文（与肯姆尼任务卡同原则）。
- [ ] 首版**可不建** `Village_House4Data` / `Village_House4ResConfig`（换场不依赖；预加载 asset 见 §7）。

---

## 6. Unity 场景侧施工清单（关卡 / 程序）

打开 **`Village_House4.unity`**：

### 6.1 SceneManager 组件替换

1. 选中 **`SceneManager`**。
2. **移除** `ForestSceneManager`。
3. **添加** `Village_House4SceneManager`。
4. **`config`** 改为 `Village_House4.asset`（勿再用 `ForestScene.asset`）。
5. 核对序列化字段：`canShowSaveGame`、`isCanTouchWithOther`、`EnterPosConfig` 等与换组件前一致，避免 `config == null`。

### 6.2 `EnterPosConfig`（室内落点）

| 操作 | 说明 |
|------|------|
| **删除或覆盖** | 原 `lastScene: HomeScene1`、`ForestEastScene` 两项（森林复制残留） |
| **新增** | `lastScene: Village_KenMuNi1`（与 `SceneName` 常量一致） |
| **`pos`** | 拖室内门口内侧空节点；可复用现有 **`LeftBorn`**（约 x=-4.23）或新建 `EnterFrom_Village_KenMuNi1` 便于辨认 |

未配置时玩家落在 **`DefaultBornPos`**，表现为「进门瞬移异常」。

### 6.3 `Map` 出门（回村）

| 物体 | 字段 | 目标值 |
|------|------|--------|
| **`MapLeft/LeftDoor`** | `NextSceneName` | **`Village_KenMuNi1`** |
| | `TriggerWhenMoveIn` | 建议 `0`（与 Home 一致，靠近按 E）；若保持 `1` 须确认碰撞盒与室内布局 |
| | `ShowLoadingUI` | 建议 `0`（黑幕换场，与村里进门体验一致） |
| **`MapRight/RightDoor`** | — | **关闭物体**或清空 `NextSceneName`，避免误触加载 `ForestEastScene` |

`bornPos` 可指向 `Village_KenMuNi1` 上户外 House4 门口 Transform（**仅对照**，不参与坐标）。

### 6.4 `Village_KenMuNi1` 复核（一般无需改代码）

| 检查项 | 期望 |
|--------|------|
| `House4` → `SceneChangeDoor.NextSceneName` | `Village_House4` |
| `House4` 在 `objRoot` 下 | 是 |
| `EnterPosConfig` 含 `Village_House4` | 是（已存在） |

### 6.5 资源构建

按根目录 **`README.md`**：将 `Village_House4` 纳入 **Resource Editor / AB** 与 **Scenes in Build**（`Game Framework/Scenes in Build Settings/All Scenes`）。

---

## 7. 可选后续（非 MVP 阻塞）

| 项 | 说明 |
|----|------|
| `SceneResPreLoad/Village_House4.asset` | 与其它场景一样可减少首次进门卡顿；`GameSceneResManager` 注册为可选 |
| 清理森林剧情触发器 | `ForestSceneGuideBoardStoryTrigger` 等避免室内误触发 |
| `Village_House1`～`3` | 复制本套 Manager + Config 命名规则 |
| 室内 BGM | 在 `OnInit` 播放专用 BGM，退出时恢复村庄 BGM |
| `PlaceName` 独立「村民家」键 | 仅当读档标题要区分「村内 / 某户」时再加字典项 |

---

## 8. 验收步骤（Play Mode）

1. 进入 **`Village_KenMuNi1`**，走到 **House4**，出现 **E**，按 E 黑幕进入 **`Village_House4`**。  
2. Console 无场景加载失败；`ChangeSceneComponentGM.LastSceneName == Village_KenMuNi1`。  
3. 玩家站在 **`EnterPosConfig`** 配置的门口 Transform，而非远处 `DefaultBornPos`。  
4. 玩家动画为 **Home 行走**（非战斗跑），脚步为 **室内** 音效。  
5. 室内 **LeftDoor** 回村 → 落在 **`Village_KenMuNi1`** House4 户外门口；`LastSceneName == Village_House4`。  
6. 存档 → 读档标题地名仍为 **肯姆尼**（`PlaceName.KenMuNi`）。  
7. **RightDoor** 不可误加载 `ForestEastScene` / `HomeScene1`。

**建议日志（验收员可选）**：在 `SetPlayerPos` 或 Manager `OnEnterScene` 打 `[VillageHouse4Debug] lastScene=… pos=…`。

---

## 9. 相关文档与代码索引

| 主题 | 路径 |
|------|------|
| 换场与落点总览 | `Assets/Doc/执行文档/0518/SceneChangeDoor场景切换_架构溯源与执行说明.md` |
| 新场景搭建 | `Assets/Doc/技术文档/场景相关/搭建新场景手册.md` |
| 仿照室内管理器 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Home1/HomeScene1Manager.cs` |
| 户外村庄管理器 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs` |
| 场景名 | `Assets/Scripts/Game/Static/Name/Res/SceneName.cs` |
| 地点显示名 | `Assets/Scripts/Game/Static/Enum/Map/PlaceName.cs` |
| 室内 Config 范例 | `Assets/GameRes/Config/SceneManagerConfig/HomeScene1.asset` |
| 门组件 | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs` |
| 村里入口预制体 | `Assets/Prefabs/Stairs.prefab` |

---

## 10. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-05-18 | 初版：基于 `Village_House4.unity`、`Village_KenMuNi1.unity`、`HomeScene1Manager` 静态阅读；明确 Forest 残留与施工清单。 |

**文档路径**：`Assets/Doc/执行文档/0518/Village_House4场景管理器_架构溯源与执行说明.md`
