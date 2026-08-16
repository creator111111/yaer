# SceneChangeDoor 场景切换 — 架构溯源与执行说明

**文档性质**：架构侦探产出（只读分析 + 配置指引，**不改代码**）  
**依据**：`00_MASTER_PROMPT.md`【架构侦探】模式；关联 `Assets/Doc/执行文档/0512/场景切换与对话触发跳转_架构溯源报告.md`、`Assets/Doc/技术文档/场景相关/搭建新场景手册.md`  
**Unity 版本**：2020.3.48f1  

---

## 1. 结论（一句话）

**门只负责填「要去哪」并调用统一换场接口；玩家落点由「目标场景」上的 `EnterPosConfig`（按来源场景名匹配）决定；室内手感由目标场景的 `GameSceneManagerConfig.isFightingScene` + 场景管理器 `GetCurSceneTerrainType()` 共同决定——与门上的 `bornPos` 字段无运行时关联。**

---

## 2. 换场调用链（给程序看的）

玩家与门交互（点击 / 走进触发区）后，链路如下：

```
SceneChangeDoor.EnterDoor
  → LoadSceneComponentGSM.LoadScene(NextSceneName, stayAction, blackFade)
      →（可选）BlackPanel 黑幕 FadeShow
      → ChangeSceneComponentGM.LoadScene(LoadSceneArgs)
          → 卸载当前场景，记录 LastSceneName = 原场景名
          → 加载 Assets/GameRes/Scenes/{NextSceneName}.unity
  → 新场景 BaseGameSceneManager 就绪
      → InitPlayer → SetPlayerPos（见 §4）
      → PlayerLogic 按 Config 换 Home/Combat 动画控制器
      → LoadingSceneEndHandle → RefreshVillageExplorationFromActiveScene（仅 Village_KenMuNi1）
```

| 类 / 资源 | 路径 | 职责 |
|-----------|------|------|
| `SceneChangeDoor` | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs` | 门交互、填 `NextSceneName`、调 `LoadScene` |
| `LoadSceneComponentGSM` | `.../GameSceneManager/Component/LoadSceneComponentGSM.cs` | 黑幕、退出旧 GSM、触发 GF 加载 |
| `ChangeSceneComponentGM` | `.../GameMgr/Component/ChangeScene/ChangeSceneComponentGM.cs` | 维护 `NowSceneName` / **`LastSceneName`** |
| `BaseGameSceneManager` | `.../GameSceneManager/Base/BaseGameSceneManager.cs` | **`SetPlayerPos`**、`EnterPosConfig` |
| 场景资源路径 | `SceneAssetPath.GetSceneAssetPath` | `Assets/GameRes/Scenes/{场景名}.unity` |

**预制体**：`Assets/GameRes/Prefabs/MapInteractive/SceneChangeDoor.prefab`（可挂在 `Map` 的 `LeftDoor` / `RightDoor`，或场景内任意门物体上）。

---

## 3. 如何选择目标场景（NextSceneName）

### 3.1 门上的配置项

在 `SceneChangeDoor` 组件 Inspector 中：

| 字段 | 含义 | 注意 |
|------|------|------|
| **NextSceneName** | 目标场景逻辑名（字符串） | 必须与 `.unity` 文件名一致，且能被 `SceneAssetPath` 解析 |
| **TriggerWhenMoveIn** | 走进触发区即换场（无需点击） | 序章 `WestRappRoad` 左门为 `true` |
| **ShowLoadingUI** | 先开 Loading 面板再换场（无黑幕） | 默认 `false`，一般用黑幕 |
| **CheckNextSceneUnlock** | 代码注入的解锁委托 | 常由 `MapControlComponentGSM.SetSceneUnlockCondition` 绑定左右门 |

`EnterDoor` 核心逻辑：非空 `NextSceneName` 且（无解锁检查或检查通过）→ `LoadScene(NextSceneName, null, blackFade)`。

### 3.2 场景名从哪里来

1. **推荐**：在 `Assets/Scripts/Game/Static/Name/Res/SceneName.cs` 增加 `public const string`，全项目引用，避免魔法字符串。  
2. **资源约束**：磁盘路径固定为 `Assets/GameRes/Scenes/{SceneName}.unity`。  
3. **构建**：新场景须按根目录 `README.md` 加入 Resource Editor / AB 与 Scenes in Build。  
4. **常量表未登记的场景**（如仅存在于 `GameRes/Scenes` 的 `Village_House1`）：门上仍可填字符串，但需自行保证 GF 能加载该资源。

### 3.3 与 Map 左右门的约定

若场景使用标准 `Map` 层级（见 `Map.cs` / 搭建手册 §3.3）：

- `MapLeft/LeftDoor`、`MapRight/RightDoor` 上挂 `SceneChangeDoor`
- 左门通常指向「左侧邻接场景」，右门指向「右侧邻接场景」
- 示例（`ForestScene`）：左门 `NextSceneName = ForestEastScene`，右门 `NextSceneName = HomeScene1`

**替代方案**：不经过门，由对话 NodeCanvas `LoadSceneTaskAction`、地图 UI `MapFormLogic`、Procedure 等同样调用 `LoadSceneComponentGSM.LoadScene`——目标场景名规则相同。

---

## 4. 如何确定切换后玩家位置

### 4.1 运行时真实逻辑（重要）

玩家坐标在 **目标场景** 加载后，由 `BaseGameSceneManager.SetPlayerPos` 写入：

1. **读档开局**：若 `ProcedureComponentGM.archiveStart`，直接用存档 `PlayerSceneData.pos`，**忽略** `EnterPosConfig`。  
2. **正常换场**：取 `ChangeSceneComponentGM.LastSceneName`（即**刚离开的场景名**），在目标场景 `EnterPosConfig` 列表中找 `lastScene` 相等的项 → 使用该项 `pos` 的 `Transform.position`。  
3. **无匹配**：使用 `MapControlComponentGSM.DefaultBornTsf`（`Map` 下 `DefaultBornPos`）。  

**`SceneChangeDoor.bornPos` 在 C# 中仅暴露属性，没有任何脚本读取它来设坐标**——现有场景里把它指向目标侧出生点，属于**策划对照 / 编辑器备忘**，不能代替目标场景的 `EnterPosConfig`。

`Map` 上的 `LeftBorn` / `RightBorn` 同样**不会**被 `SetPlayerPos` 自动选用；若要从「左门进」和「右门进」落点不同，应在目标场景配置**两条** `EnterPosConfig`（`lastScene` 填不同来源场景名），或两条都指向不同 Transform（可挂在 `LeftBorn` / `RightBorn` 节点上）。

### 4.2 配置步骤（新场景接入）

**在「被进入」的场景**（目标场景）根物体 `SceneManager` 上：

1. 展开 **`Enter Pos Config`** 列表。  
2. 增加元素：  
   - **last Scene**：填写**来源场景**名（与 `SceneName` 常量一致，例如从 `ForestScene` 进门则填 `ForestScene`）。  
   - **pos**：拖入场景中作为落点的空物体（建议在门口内侧摆一个 `EnterFrom_XXX` 空节点）。  
   - **Date Pass**：可选，经过该入口时推进游戏内日期。  
3. 若仅有单一入口，可只配一条；未配置时玩家落在 `DefaultBornPos`。

**在「当前」场景的门**上：

- **Next Scene Name** = 目标场景名。  
- （可选）**born Pos** 拖同一落点 Transform，便于对照，**不参与运行**。

### 4.3 参考实例

| 目标场景 | EnterPosConfig 片段（事实） |
|----------|----------------------------|
| `HomeScene1` | `lastScene: ForestScene` → 门口 Transform；`lastScene: HomeScene2` → 另一 Transform |
| `Village_KenMuNi1` | `lastScene: HomeScene1`；`lastScene: ForestEastScene`（第二条带 DatePass） |
| `ForestScene` 左门 | `NextSceneName: ForestEastScene`，门上 `bornPos` 与目标场景 `ForestEastScene` 项 pos 的 fileID 一致（仅文档关系） |

### 4.4 替代方案说明

| 方案 | 适用 | 风险 |
|------|------|------|
| **EnterPosConfig + LastSceneName**（现行） | 多入口、多来源场景 | 忘记在目标场景配表 → 落到 `DefaultBornPos` |
| 重写 `SetPlayerPos` | 特殊演出、固定坐标 | 需新建 SceneManager 子类，改动面大 |
| 仅依赖 `DefaultBornPos` | 单入口测试场景 | 无法区分从左/右门进入 |
| 指望门上 `bornPos` | — | **当前架构无效**，除非后续施工接线 |

---

## 5. 如何确定「室内」状态

本项目的「室内」分三层，**不要混为一谈**：

### 5.1 动画与移动：Home 控制器（核心）

`PlayerLogic` 初始化时：

```csharp
GetAnimatorController(UpdateRuntimeController, sceneManager.Config.isFightingScene);
```

- **`GameSceneManagerConfig.isFightingScene == false`** → 加载 **Home** 向 `RuntimeAnimatorController`（行走、待机等非战斗状态机）。  
- **`isFightingScene == true`** → 加载 **Combat** 向控制器（跑攻、跳跃等）。  

**室内场景范例**：`HomeScene1.asset` / `HomeScene2.asset` 中 **`isFightingScene: 0`**。  
**户外战斗范例**：`ForestScene.asset`、`ForestEastScene.asset` 为 **`1`**。

**你要做**：为新室内场景复制或新建 `GameSceneManagerConfig`，将 **`is Fighting Scene` 取消勾选**，并挂到该场景 `SceneManager` 的 `config` 字段。

### 5.2 脚步声：TerrainType.IndoorType

`BaseGameSceneManager.GetCurSceneTerrainType()` 默认返回 `IndoorType`；户外场景管理器会 **override**：

| 场景管理器 | 返回值 | 脚步资源 |
|------------|--------|----------|
| `HomeScene1Manager` / `HomeScene2Manager` | `IndoorType` | `室内走{0}.mp3` |
| `ForestSceneManager` | `LandType` | `土地跑{0}.mp3` |
| `Village_KenMuNiSceneManager` | `LandType` | 土地跑（户外村庄） |

**你要做**：室内专用 SceneManager 子类中重写：

```csharp
public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;
```

（与 `HomeScene1Manager` 相同。）

### 5.3 村庄 2.5D 探索（与室内无关）

仅当 **激活场景名 == `SceneName.Village_KenMuNi1`** 时，`PlayerLogic.RefreshVillageExplorationFromActiveScene()` 会开启 `TownPlayerLocomotion`、纵深 Walk 区等——这是**肯尼姆户外村庄**规则，**不是**室内 Home 规则。  
室内场景 **不要** 使用该场景名，除非刻意要做村庄移动。

### 5.4 室内场景接入检查清单

- [ ] 根节点 `*SceneManager` 继承 `BaseGameSceneManager`（可参考 `HomeScene1Manager`）。  
- [ ] `config` → `isFightingScene = false`，`canCreatePlayer = true`。  
- [ ] 管理器 **`GetCurSceneTerrainType` → IndoorType**。  
- [ ] 目标场景 **`EnterPosConfig`** 含来源场景一条，`pos` 落在室内门口内侧。  
- [ ] 来源场景门 **`NextSceneName`** 指向该室内场景常量。  
- [ ] 换场后确认：玩家为 Home 动画（非战斗跑姿）、脚步为室内音效。  

**反例提醒**：`Village_House1` 等若仍挂 `ForestSceneManager` + `ForestScene.asset`（`isFightingScene: 1`），进门后仍是**战斗控制器**，不算室内配置完成。

---

## 6. 在新场景中从零搭门（执行顺序）

1. **登记场景名**：`SceneName.cs` + `Assets/GameRes/Scenes/YourScene.unity` + 资源构建。  
2. **场景管理器**：复制最接近的场景（室内复制 `HomeScene1`，户外横版复制 `ForestScene` 或 `Village_KenMuNi1`），改脚本类与 `nowSceneName`。  
3. **Config**：室内用 `isFightingScene = false` 的 asset。  
4. **Map（若需要标准左右门）**：按 `Map.FindObject` 命名摆 `MapLeft/LeftDoor`、`MapRight/RightDoor`、`DefaultBornPos`、`LeftBorn`、`RightBorn`。  
5. **EnterPosConfig**：在**本场景**配置「从哪来落哪」的 `lastScene` + `pos`。  
6. **邻接场景的门**：`NextSceneName` 指向本场景；邻接场景若有反向入口，也需配对应 `EnterPosConfig`。  
7. **解锁（可选）**：在 SceneManager `OnInit` 中 `MapControlComponentGSM.SetSceneUnlockCondition(...)`。  
8. **验证**：Play 从 A 进门到 B → Console 无加载失败；`LastSceneName` 为 A；玩家站在 `pos`；室内则 Home 动画 + 室内脚步。

---

## 7. SceneChangeDoor 其它行为

| 行为 | 说明 |
|------|------|
| `OnEnterSuccess` / `OnEnterFail` | 虚方法；子类如 `HomeScene2Door` 用于开门动画、音效 |
| `isEnter` 防重入 | 同门重复触发会 Warning |
| 黑幕 | `ShowLoadingUI=false` 时默认 `blackFade=true`；`MapFormLogic` 等可传 `stayAction` 在黑幕中关 UI |
| 交互 | 依赖 `InteractiveComponent`（点击 / 进入事件） |

---

## 8. 相关文档索引

| 文档 | 路径 |
|------|------|
| 换场总览与对话跳转 | `Assets/Doc/执行文档/0512/场景切换与对话触发跳转_架构溯源报告.md` |
| 新场景必备清单 | `Assets/Doc/技术文档/场景相关/搭建新场景手册.md` |
| 肯尼姆 SceneManager | `Assets/Doc/执行文档/0512/新建肯尼姆场景管理器_程序施工执行说明.md` |

---

## 9. 按 E 交互不弹出提示：机制与排查（含 `Stairs` 案例）

### 9.1 现象与结论

**现象**：在 `Village_KenMuNi1` 蓝门（`蓝门`）下挂了 `Assets/Prefabs/Stairs.prefab`，玩家走进 Trigger 区域，**不出现 E 键提示**，按 E 也无换场。

**结论（根因）**：`Stairs` 挂在场景**美术层级**（`蓝门` → 地图装饰父节点）下，**不在** `SceneEntityComponentGSM.objRoot` 子树内，也**未**出现在 `sceneObjs` 列表 → 运行时 `GetAllSceneEntities()` 扫不到它 → `GetFirstCanTouchEntiy` 永远找不到该交互体 → **E 提示链路不会启动**。  
预制体本身组件齐全（`SceneEntity` + `SceneChangeDoor` + `InteractiveComponent` + `BaseEntityControll`），问题在**场景注册位置**，不是 Trigger 没勾 `Is Trigger`。

### 9.2 E 提示完整链路（给程序看的）

```
PlayerLogic.Update
  → checkCanAddKeyTipsInOtherEntity()
      → BaseGameSceneManager.GetFirstCanTouchEntiy()
          → 遍历 SceneEntityComponentGSM.sceneObjs（仅 objRoot 下登记的 SceneEntity）
          → 玩家 InteractiveComponent 与目标 InteractiveCollider 的 bounds 相交
      → 若 canTouchObj.entityControll != null && canTouchWithPlayer
          → BaseEntityControll.AddKeyTipsNode() 显示 E
按 E / 交互键
  → BasePlayerState.InteractAciton()
      → GetFirstCanTouchEntiy() 同上
      → closestComponent.OnInteractive()
          → SceneChangeDoor 已在 OnInit 里订阅的 onClickInteractiveEvent → EnterDoor
```

| 环节 | 脚本 / 配置 | 失败时表现 |
|------|-------------|------------|
| 实体登记 | `SceneEntityComponentGSM`：`objRoot` + `sceneObjs` | **永远检测不到**（本次 Stairs） |
| 逻辑初始化 | `SceneEntity.OnInit` → `SceneChangeDoor.OnInit` 订阅点击 | 有 E 但按 E 无反应 |
| 范围判定 | `InteractiveComponent.AreCollidersOverlapping`（bounds + 0.2 容差） | 站很近仍无 E → 查碰撞盒大小/位置 |
| 提示门禁 | `BaseEntityControll.canTouchWithPlayer` + `entityControll` 非空 | 相交有日志但无 E → 查 `BaseEntityControll.interactiveComponent` 引用 |
| 场景总开关 | `BaseGameSceneManager.isCanTouchWithOther` | 全村无法交互（肯尼姆当前为 `true`） |

**重要**：E 提示**不依赖** `InteractiveComponent.cldListeners` / `CldInteractiveListener` 的 Trigger 回调；那是进入/离开事件用的。检测用的是 **Collider2D.bounds 相交**（玩家 `Event` 子物体上的 BoxCollider2D ↔ 物体 `interactiveCollider`）。

### 9.3 `Stairs` 预制体与场景事实对照

| 项目 | `Stairs.prefab` | `Village_KenMuNi1` 中实例 | `HomeScene1` 可工作的 Stairs |
|------|-----------------|---------------------------|------------------------------|
| 组件 | `SceneEntity`、`SceneChangeDoor`、`InteractiveComponent`、`BaseEntityControll` | 同预制体（实例在 `蓝门` 下） | 场景内直接摆放 |
| 父节点 | — | `蓝门` → 地图装饰层（**非** `objRoot`） | `objRoot`（`SceneEntityComponentGSM` 引用） |
| `sceneObjs` 列表 | — | **未包含** Stairs 的 `SceneEntity` | **包含**（`fileID` 对应 Stairs 的 `SceneEntity`） |
| `NextSceneName` | 预制体默认 `HomeScene1` | 实例可改 | 已配置 |
| `CldInteractiveListener` | **无**（`cldListeners` 为空） | 同左 | 有 Listener 时主要用于进入事件，**非 E 显示必要条件** |

`SceneEntityComponentGSM` 初始化逻辑（`OnValidate` 仅在编辑器改 Hierarchy 时刷新列表）：

```csharp
// SceneEntityComponentGSM.cs — sceneObjs 只收集 objRoot 下 SceneEntity
sceneObjs = objRoot.GetComponentsInChildren<SceneEntity>(true).ToList();
```

### 9.4 修复步骤（策划 / 关卡在 Editor 中操作）

1. 在 `Village_KenMuNi1` 中选中 **`SceneManager` → `SceneEntityComponentGSM`**，确认 **`Obj Root`** 指向场景实体根（与其它 NPC、箱子同级的那棵子树，fileID 对应场景里的 `objRoot`）。  
2. 将 **`Stairs` 实例**从 `蓝门` 下**拖到 `Obj Root` 下**（与 `HomeScene1` 一致：可交互物与 `SceneEntityComponentGSM` 同树）。  
3. 选中 `SceneEntityComponentGSM` 组件，在 Inspector 点一下任意字段触发 **`OnValidate`**，或手动把 Stairs 上的 **`SceneEntity`** 拖进 **`Scene Objs`** 列表。  
4. 确认 Stairs 上：  
   - `InteractiveComponent.interactiveCollider` → 根物体 **BoxCollider2D**（Trigger 已勾）  
   - `BaseEntityControll.interactiveComponent` → 子物体 **InteractiveComponent**  
   - `SceneChangeDoor.NextSceneName` → 目标场景（如室内 `HomeScene1`）  
5. **Play 验证**：  
   - Console 出现 `[BaseGameSceneManager] GetFirstCanTouchEntiy result=InteractiveComponent`（或子物体名）而非 `null`  
   - 靠近后出现 E；按 E 触发黑幕换场  

**目标场景别忘了**：`HomeScene1`（或你填的场景）的 **`EnterPosConfig`** 增加 `lastScene: Village_KenMuNi1` + 落点 Transform（见 §4）。

### 9.5 仍无 E 时的次级排查

| 检查项 | 做法 |
|--------|------|
| 碰撞盒是否够大 | 村庄 2.5D 下玩家 `Event` 碰撞中心偏高，Trigger 若只有脚底一条，bounds 可能不相交；放大 Stairs 的 BoxCollider2D 或在门口站定测试 |
| `SceneChangeDoor` 是否 `OnInit` | 仅 `Map.LeftDoor/RightDoor` 会在 `Map.OnInit` 里调 `OnInit`；**独立 Stairs 必须靠 `SceneEntity.OnInit`**，故 §9.4 登记必不可少 |
| `entityControll` 为空 | `BaseEntityControll.Start` 里赋值；首帧前可能短暂无 E，一般下一帧恢复。Inspector 中 `BaseEntityControll.interactiveComponent` 不能为空 |
| 剧情/暂停 | `StoryTriggeredHandle` 会 `PauseGameHandle`，期间不更新交互提示 |
| 村庄输入模式 | `Village_KenMuNi1` 仍走 Home 状态机 + 交互检测，与 `TownPlayerLocomotion` 不冲突；`isCanTouchWithOther` 已为 `true` |

### 9.6 替代方案说明

| 方案 | 适用 | 注意 |
|------|------|------|
| **挪到 objRoot + 刷新 sceneObjs**（推荐） | 与现有 `SceneChangeDoor` / `Stairs` 预制体一致 | 零代码，与 `HomeScene1` 相同做法 |
| 挂在 `Map` 下专门 `LeftDoor` 子物体 | 标准左右门切场 | 需符合 `Map.FindObject` 命名，并由 `Map.OnInit` 注入 GSM |
| 自定义 `BaseSceneEntityLogic` + 代码 `sceneObjs.Add` | 极特殊动态生成门 | 需施工员改 `SceneEntityComponentGSM`，超出本文「配置修复」范围 |
| 仅加 Trigger + 自建脚本 | 不推荐 | 绕过统一交互/E 提示/换场链，易与存档、黑幕不一致 |

---

## 10. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-05-18 | 初版：基于 `SceneChangeDoor`、`LoadSceneComponentGSM`、`BaseGameSceneManager.SetPlayerPos` 及 `HomeScene1` / `ForestScene` / `Village_KenMuNi1` 场景 YAML 静态阅读整理。 |
| 2026-05-18 | 增补 **§9**：`Village_KenMuNi1` 蓝门 `Stairs` 无法按 E 交互之根因（未登记 `SceneEntityComponentGSM.sceneObjs`）、E 提示链路与 Editor 修复步骤。 |

**文档路径**：`Assets/Doc/执行文档/0518/SceneChangeDoor场景切换_架构溯源与执行说明.md`
