# Village_House4 场景管理器 — 施工执行说明

**施工员交付（待实施）** | 日期：2026-05-30  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【施工员】最小化修改、先可运行
- `Assets/Doc/执行文档/0518/Village_House4场景管理器_架构溯源与执行说明.md`（架构侦探结论，本文在其基础上转为可执行清单）
- 室内仿照基准：`Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Home1/HomeScene1Manager.cs`
- 户外来源场景：`Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs`
- 换场总览：`Assets/Doc/技术文档/场景相关/场景切换.md` §8

**Unity 版本**：2020.3.48f1  

---

## 1. 目标（一句话）

为 **`Village_House4`**（肯姆尼村民居室内）**新建并正确挂载** `Village_House4SceneManager`：行为对齐 **`HomeScene1Manager` 的室内最小集**（Home 动画、室内脚步、正确场景名与存档地点），替换当前场景中失效的 `ForestSceneManager` 残留，使「村里进门 → 室内游玩 → 左门回村」整条换场链路可验收。

---

## 2. 背景与现状缺口

### 2.1 你在 Unity 里看到的现象

| 现象 | 含义 |
|------|------|
| Hierarchy 根下有 **`SceneManager`** 物体 | **位置正确**，即本场景管理器挂载点 |
| Inspector 脚本显示 **`None (Mono Script)`** | 场景序列化引用的脚本 **未能被 Unity 加载**（常见：脚本未真正创建 / `.meta` GUID 异常 / 项目编译错误） |
| 子节点含 `DepthManager`、`BGM`、`Map`、`Entity`、`Story` 等 | 从森林或 Home 模板复制来的标准 GSM 层级，**可保留** |

### 2.2 静态阅读结论（2026-05-30）

| 项目 | 当前状态 | 是否阻塞 |
|------|----------|----------|
| 场景文件 | `Assets/GameRes/Scenes/Village_House4.unity` | — |
| `SceneName.Village_House4` | **已在** `SceneName.cs` 登记 | 否 |
| `Village_House4SceneManager.cs` | 磁盘上存在一份骨架，但 **Unity 未成功识别**；命名空间 `Village_House` 与物理路径 `Village_KenMuNi/` **不一致** | **是** |
| `Village_House4.asset` | 已存在，`isFightingScene: 0` | 需核对场景引用 |
| `EnterPosConfig` | 已有 `lastScene: Village_KenMuNi1` | 否 |
| `MapLeft/LeftDoor` | `NextSceneName: Village_KenMuNi1` | 否 |
| `MapRight/RightDoor` | 已 **Inactive**，`NextSceneName` 为空 | 否 |
| 森林剧情触发器 | 场景内仍有多处 `ForestScene*` 触发器 | **非 MVP 阻塞**，后续清理 |

### 2.3 与 `HomeScene1Manager` 的关系

`Village_House4` 与 `HomeScene1` 同属 **室内非战斗场景**，玩家应使用 **Home 行走动画** + **室内脚步**，而非战斗跑姿。

**仿照原则**：复制 Home 的「室内底座」，**不要**复制 Home 专属剧情 / 实体 / BGM 逻辑。

---

## 3. 仿照对照表（HomeScene1 → Village_House4）

| 维度 | `HomeScene1Manager` | `Village_House4SceneManager`（本任务） |
|------|---------------------|----------------------------------------|
| 基类 | `BaseGameSceneManager` | 同左 |
| `nowSceneName` | `SceneName.HomeScene1` | **`SceneName.Village_House4`** |
| `GetCurSceneTerrainType` | `IndoorType` | **同左** |
| `initAllSceneMonster` | 空实现 | **同左** |
| `SetNowPlace` | `PlaceName.Home` | **`PlaceName.KenMuNi`**（仍在肯姆尼村内，读档显示「肯尼姆」） |
| Config | `HomeScene1.asset`（`isFightingScene: 0`） | **`Village_House4.asset`** |
| 场景专属存档 `*Data` | `HomeScene1Data` + 首次进入剧情 | **首版不做** |
| 实体显隐（雅尔等） | `HomeScene1Xiaer` | **不做** |
| 右门解锁 | `SetSceneUnlockCondition` | **不做**（单出口室内） |
| BGM | 龙宫内 BGM | **首版不做**（沿用村庄户外 BGM 或静音均可；有需求再单独立项） |
| CustomEditor | `HomeScene1MgrInsp` | **可选**，空继承 `BaseGameSceneMgrInspector` |

---

## 4. 代码侧施工清单

### 4.1 新建 / 修正 `Village_House4SceneManager.cs`

| 项 | 建议值 |
|----|--------|
| **推荐目录** | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/`（新建，与 `Home1/`、`Village_KenMuNi/` 并列；便于 House1～3 复用） |
| **备选目录** | 与 `Village_KenMuNiSceneManager` 同目录 `Scene/Village_KenMuNi/`（若仅 House4 单点交付可暂放此处，但命名空间须与文件夹一致） |
| **类名** | `Village_House4SceneManager` |
| **命名空间** | `Game.GameRuntime.GameSceneManager.Scene.Village_House`（与目录一致） |

> **重要**：若磁盘上已有旧文件且 Unity 报 `None (Mono Script)`，建议 **删除旧 `.cs` + `.meta` 后在 Unity 内右键 Create → C# Script** 重新生成，让 Unity 分配合法 GUID，再在场景中重新挂载。勿手填 `.meta` 的 guid。

**推荐代码骨架**（含注释，施工员照抄后微调）：

```csharp
using Game.GameMgr.Component;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_House
{
    /// <summary>
    /// 肯姆尼村民居 <see cref="SceneName.Village_House4"/> 室内场景管理器。
    /// 行为对齐 <see cref="Game.GameRuntime.GameSceneManager.Scene.Home1.HomeScene1Manager"/> 的「室内」最小集：
    /// Home 动画（Config.isFightingScene=false）、室内脚步、正确 nowSceneName。
    /// </summary>
    /// <remarks>
    /// 替代方案：继续挂 ForestSceneManager 仅改 Config 可临时试玩，但 nowSceneName 仍为 ForestScene，存档/任务易错，故不采用。
    /// </remarks>
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

            // 与 Village_KenMuNiSceneManager 类似，加载完成后再写一次 SetNowPlace，避免切场顺序覆盖。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        /// <summary>室内脚步资源：室内走{0}.mp3</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
```

### 4.2 `SceneName.cs`（核对）

- 路径：`Assets/Scripts/Game/Static/Name/Res/SceneName.cs`
- 确认已有：`public const string Village_House4 = "Village_House4";`（与 `.unity` 文件名一致）
- **若缺失则补一行**；已存在则 **不要改字符串**。

### 4.3 `GameSceneManagerConfig`（核对 / 新建）

| 项 | 说明 |
|----|------|
| 路径 | `Assets/GameRes/Config/SceneManagerConfig/Village_House4.asset` |
| 创建方式 | 复制 `HomeScene1.asset`，改名 `Village_House4` |
| 关键字段 | `isFightingScene: 0`（**必须为 0**，否则玩家用战斗跑姿） |
| 其余 | `canMove / canCreatePlayer / isPlayingScene / canSave` 与 HomeScene1 保持一致即可 |

### 4.4 CustomEditor（可选）

| 项 | 说明 |
|----|------|
| 路径 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Editor/Village_House4MgrInsp.cs` |
| 内容 | 空继承 `BaseGameSceneMgrInspector`，与 `HomeScene1MgrInsp` 相同 |
| 优先级 | **非验收必需**，可后续补 |

### 4.5 禁止修改

- **不要改** `ForestSceneManager.cs` 正文
- **不要改** 存档系统（`ArchiveComponentGM` 等）
- **不要改** `HomeScene1Manager.cs`（仅作只读参考）

---

## 5. Unity 场景侧施工清单

打开 **`Assets/GameRes/Scenes/Village_House4.unity`**：

### 5.1 SceneManager 组件（必做）

1. 选中根物体 **`SceneManager`**。
2. 若存在 **Missing Script** 或 **`ForestSceneManager`**：**移除**。
3. **Add Component** → **`Village_House4SceneManager`**（须等 §4.1 编译通过后再操作）。
4. **`config`** 拖入 **`Village_House4.asset`**（勿用 `ForestScene.asset`）。
5. 核对序列化字段与换组件前一致：`canShowSaveGame`、`isCanTouchWithOther`、`EnterPosConfig` 等，避免 `config == null` 警告。

### 5.2 `EnterPosConfig`（室内落点）

| 操作 | 说明 |
|------|------|
| **保留** | `lastScene: Village_KenMuNi1` → `pos` 指向室内门口 Transform（可用现有 **`LeftBorn`** 或新建 `EnterFrom_Village_KenMuNi1`） |
| **删除** | 若仍有 `HomeScene1`、`ForestEastScene` 等森林复制残留项 |

未配置匹配项时，玩家会落在 **`DefaultBornPos`**，表现为「进门瞬移异常」。

### 5.3 `Map` 出门（回村）

| 物体 | 字段 | 目标值 |
|------|------|--------|
| **`MapLeft/LeftDoor`** | `NextSceneName` | **`Village_KenMuNi1`** |
| | `TriggerWhenMoveIn` | 建议 `0`（靠近按 E，与 Home 一致） |
| | `ShowLoadingUI` | 建议 `0` |
| **`MapRight/RightDoor`** | 物体 | 保持 **Inactive** 或清空 `NextSceneName` |

### 5.4 `Village_KenMuNi1` 复核（一般无需改）

| 检查项 | 期望 |
|--------|------|
| `House4` → `SceneChangeDoor.NextSceneName` | `Village_House4` |
| `House4` 在 `SceneEntityComponentGSM.objRoot` 下 | 是 |
| `EnterPosConfig` 含 `Village_House4` 落点 | 是 |

### 5.5 构建配置

按根目录 **`README.md`**：将 `Village_House4` 纳入 **Resource Editor / AB** 与 **Scenes in Build**。

---

## 6. 换场链路（验收对照）

```
Village_KenMuNi1 / House4 (Stairs + SceneChangeDoor)
  → LoadSceneComponentGSM.LoadScene("Village_House4", …)
  → ChangeSceneComponentGM.LastSceneName = "Village_KenMuNi1"
  → 加载 Village_House4.unity
  → Village_House4SceneManager.OnInit
      → nowSceneName = Village_House4
      → Config.isFightingScene == false → Home 控制器
      → GetCurSceneTerrainType → IndoorType
  → SetPlayerPos：EnterPosConfig 匹配 lastScene == Village_KenMuNi1 → 门口 Transform

室内 LeftDoor
  → LoadScene("Village_KenMuNi1", …)
  → LastSceneName = Village_House4
  → Village_KenMuNi1.EnterPosConfig 匹配 → 户外 House4 门口落点
```

---

## 7. 验收步骤（Play Mode）

1. 进入 **`Village_KenMuNi1`**，走到 **House4**，按 **E** 进入 **`Village_House4`**。  
2. Console **无** 场景加载失败、**无** `SceneManager` 相关 NRE。  
3. `SceneManager` Inspector 显示 **`Village_House4SceneManager`**（非 `None`）。  
4. 玩家落在 **`EnterPosConfig`** 门口，非远处 `DefaultBornPos`。  
5. 玩家动画为 **Home 行走**（非战斗跑），脚步为 **室内** 音效。  
6. 室内 **LeftDoor** 回村 → 落在 **`Village_KenMuNi1`** House4 户外门口。  
7. 存档 → 读档标题地名仍为 **肯尼姆**（`PlaceName.KenMuNi`）。  
8. **RightDoor** 不可误加载其它场景。

**建议日志（验收员可选）**：在 `OnEnterScene` 打 `[VillageHouse4Debug] lastScene=… place=KenMuNi`。

---

## 8. 施工自检清单（程序勾选）

- [ ] `Village_House4SceneManager.cs` 已创建，**命名空间与目录一致**，Unity 编译无错  
- [ ] 场景 `SceneManager` 已挂载新组件，`config == Village_House4.asset`  
- [ ] `isFightingScene == 0`  
- [ ] `EnterPosConfig` 含 `Village_KenMuNi1` 且无森林/Home 残留  
- [ ] `LeftDoor.NextSceneName == Village_KenMuNi1`  
- [ ] `RightDoor` 已禁用或清空目标场景  
- [ ] Play Mode §7 全部通过  
- [ ] （可选）CustomEditor、`SceneResPreLoad/Village_House4.asset`  

---

## 9. 可选后续（非 MVP）

| 项 | 说明 |
|----|------|
| 清理 `ForestScene*` 剧情触发器 | 避免室内误触发森林对话 |
| `Village_House1`～`3` | 复制本套 Manager + Config 命名规则 |
| 室内独立 BGM | 在 `OnInit` 播放，`OnExitScene` 停播 |
| `PlaceName` 独立「村民家」键 | 仅当读档标题要区分「村内 / 某户」时再加 |

---

## 10. 相关文档与代码索引

| 主题 | 路径 |
|------|------|
| 架构侦探（历史） | `Assets/Doc/执行文档/0518/Village_House4场景管理器_架构溯源与执行说明.md` |
| 室内仿照基准 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Home1/HomeScene1Manager.cs` |
| 户外村庄管理器 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs` |
| GSM 基类 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs` |
| 室内 Config 范例 | `Assets/GameRes/Config/SceneManagerConfig/HomeScene1.asset` |
| 换场与落点 | `Assets/Doc/技术文档/场景相关/场景切换.md` |
| 门组件 | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs` |

---

## 11. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-05-30 | 初版：基于 HomeScene1 室内最小集，将 0518 架构结论转为可执行施工清单；补充脚本 GUID / Missing Script 修复指引。 |

**文档路径**：`Assets/Doc/执行文档/0530/Village_House4场景管理器_施工执行说明.md`
