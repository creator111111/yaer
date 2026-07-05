# 龙战姬：核心系统技术规范 (v1.0)

## 1. 坐标系与位移规范 (2.5D Village Mode)
- **视觉模型**：采用 DNF 式横版 2.5D 逻辑。
- **Y 轴定义**：在村庄场景中，**Y 轴代表深度（Vertical）**，禁止使用 Z 轴进行位移。
- **物理配置**：
  - 使用 Rigidbody2D。
  - 村庄模式下 `Gravity Scale` 必须为 0。
  - 角色上下移动必须通过修改 `velocity.y` 或 `position.y` 实现。

## 2. 剧情与流程控制 (NodeCanvas)
- **驱动源**：所有非战斗演出必须由 NodeCanvas 驱动。
- **状态同步**：
  - 严禁在脚本中私自使用 `isTalking` 等局部变量锁定逻辑。
  - 必须通过 `StoryComponentGSM.TriggerStory` 启动剧情。
  - 演出结束信号必须返回给 NodeCanvas 的状态机。

## 3. 摄像机系统
- **控制权**：摄像机通过 `CameraComponent.SetFollow` 进行平滑移动。
- **衔接逻辑**：摄像机平移结束后，需通过 `onComplete` 回调触发后续逻辑，严禁使用固定延迟（Wait）来硬匹配。

## 4. 玩家动画系统（Home / Combat 双轨）
两套系统对应**不同 RuntimeAnimatorController**，Bool 参数命名不同；**村庄纵深（`TownPlayerLocomotion`）只参与「Walk」侧补充，不得与 Combat 的 `Run` 抢写**，否则会出现 Animator 状态与 C# 状态机错位、`IsName` 对不上导致整卡死（典型：先 AD 再 W/S）。

### 4.1 Home（非战斗 / 家里）
- **控制器资源**：`Assets/GameRes/RuntimeController/Entity/Player/Home/*.controller`（示例：`Home_Dress_Crown.controller`）。
- **移动体态参数**：Bool **`Walk`**（无 `Run`）。
- **脚本状态机**：`PlayerHomeCsRuntimeController` → `PlayerHomeSM`；地面子状态含 `HomeIdleState` / `HomeBinkState` / `HomeWalkState`（`Assets/Scripts/.../CsAnimator/Home/`）。
- **与 Animator 的契约**：`BaseStateMachine` 仅在 **`AnimatorStateInfo.IsName(当前 C# 状态的 StateName)`** 为真时执行该状态的 `Enter` / `Update`；`RegisterState<T>(argsName, stateName)` 中 `SetAnimatorEnter` 会 `SetBool(argsName, true)`（如 `HomeWalkState` 对应 `Walk`）。
- **村庄 W/S**：`HasMoveInput()` 不含纵轴；Idle/Bink/Walk 通过 `HasVillageExploreDepthMoveIntent()` → `TownPlayerLocomotion.HasVillageDepthMoveForHomeStateMachine()` 与 `Walk` 对齐（见执行说明 §5.2）。
- **`TownPlayerLocomotion`**：在 `Village2_5D` 下 **`SyncWalkAnimatorParameter` 仅写入当前 Animator 上存在的 `Walk`**（若控制器无 `Walk` 则跳过），**不写 `Run`**。

### 4.2 Combat（战斗）
- **控制器资源**：`Assets/GameRes/RuntimeController/Entity/Player/Combat/*.controller`（示例：`Combat_Armor_Crown.controller`、`Combat_Armor_ArmorHead` 等）。
- **移动体态参数**：Bool **`Run`**（无 `Walk`）。
- **脚本状态机**：`PlayerCombatCsRuntimeController` → `PlayerCombatSM`；地面 Idle/Run 为 `CombatIdleState` / `CombatRunState`（`Assets/Scripts/.../CsAnimator/Combat/State/Ground/`）。
- **`Run` 的独占权**：**仅允许**由 Combat 子状态的 `SetAnimatorEnter` / `SetAnimatorExit` 驱动 `Run`；**禁止**在 `TownPlayerLocomotion` 或其它村庄脚本中每帧 `SetBool("Run", …)`，否则会与 `CombatRunState` 抢参，造成「C# 已在 Idle、Animator 仍为 Run」。
- **村庄 W/S（Combat 控制器进村时的约定）**：
  - **`CombatIdleState`**：除 `HasMoveInput()` 外，增加 `HasVillageExploreDepthMoveIntent()` 以进入 `CombatRunState`。
  - **`CombatRunState`**：仅在 **`!HasMoveInput() && !HasVillageExploreDepthMoveIntent()`** 时回 `CombatIdleState`；纯纵深远时可不调 `SetRunSpeed()`，并在需要时用 `StopMoveInX()` 清横向分量，避免与纵深写 Y 叠移。

### 4.3 查询索引（代码入口）
| 概念 | 路径 |
|------|------|
| 纵深与（仅）Walk 同步 | `TownPlayerLocomotion.cs` |
| Home 主状态机 | `PlayerHomeSM.cs`、`HomeWalkState.cs`、`Idle/HomeIdleState.cs` |
| Combat 地面 Idle/Run | `CombatIdleState.cs`、`CombatRunState.cs` |
| Animator 与 C# 状态同步规则 | `BaseStateMachine.cs`（`Update` 内 `StateInfo.IsName`） |
| 运行时切换 Home/Combat 控制器 | `PlayerLogic.UpdateRuntimeController`（依据 `GameSceneManagerConfig.isFightingScene`） |

## 5. 场景管理器（Game Scene Manager）

- **挂载**：可玩场景根上通常有名为 `SceneManager` 的物体，挂 **`BaseGameSceneManager` 的子类**（如 `HomeScene1Manager`、`ForestSceneManager`、`Village_KenMuNiSceneManager` 等），并配置 `GameSceneManagerConfig` 等序列化字段。
- **生命周期**：`Awake` 里走 `OnInit()`（注册 GSM 模块、剧情代理、玩家创建等）；异步初始化计数完成后，**首帧 `Update`** 里调用 `GameManager.Instance.OnGameSceneManagerReady(this)`，之后全局用 **`GameManager.GetGameSceneManager()`** 取当前管理器。切场景销毁物体时会走 `OnShutDown` 等清理并 `RemoveGameSceneManager`。
- **职责（概括）**：本场景内的模块调度（摄像机、剧情、实体、输入、切场景等）、与 `GameSceneManagerConfig` 对齐的战斗/探索规则；**逻辑场景名**由子类设置 `nowSceneName`（与 `SceneName` 常量一致），供切场景、提示等使用。
- **与存档标题的关系（只记结论）**：子类在适当时机调用 **`PlayerHandlerComponentGSM.SetNowPlace(地点内部键)`**，写入 `PlayerMapData`；保存时 **`ArchiveComponentGM` 把该值写入 `ArchiveInfo.currentSceneName`**；读档/存档列表 UI 用 **`PlaceName.GetPlaceChsName(...)`** 把内部键显示成中文（或其它语言表）。**改显示名只改 `PlaceName` 字典即可，不必改存档组件。**
- **同玩法多场景共脚本时**：若两个 Unity 场景玩法相近但地名不同，应**拆子类**（例：`Village_KenMuNi1` 用 `Village_KenMuNiSceneManager`，避免继续挂 `ForestSceneManager` 导致 `SetNowPlace` 仍为森林键）。

### 5.1 代码入口（速查）

| 主题 | 路径 |
|------|------|
| 基类与 Ready 注册 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs` |
| 地点键与界面显示名 | `Assets/Scripts/Game/Static/Enum/Map/PlaceName.cs` |
| 写入当前地点 | `PlayerHandlerComponentGSM.SetNowPlace` → `PlayerMapData` |

### 5.2 场景切换（换场）

- **统一入口**：`LoadSceneComponentGSM.LoadScene(sceneName, …)` → `ChangeSceneComponentGM`（维护 `NowSceneName` / **`LastSceneName`**）→ 加载 `Assets/GameRes/Scenes/{场景名}.unity`。
- **常见触发**：场景内门 `SceneChangeDoor`（填 **`NextSceneName`**）、NodeCanvas `LoadSceneTaskAction`、地图 UI、流程 Procedure；对话结束**不会**自动换场，须在对话图或后续逻辑里显式调用。
- **玩家落点**：在**目标场景** `SceneManager` 的 **`EnterPosConfig`** 中，用 **`lastScene` = 刚离开的场景名** 匹配落点 Transform；读档开局用存档坐标。门上的 **`bornPos` 不参与运行时坐标**。
- **转场表现**：默认 **BlackPanel 黑幕**；门上勾选 `ShowLoadingUI` 时走 **LoadingPanel 假读条**（二者一般不同时主控一次切场）。
- **交互进门**：可交互门须挂在 `SceneEntityComponentGSM.objRoot` 下并列入 **`sceneObjs`**，E 提示靠碰撞盒 **bounds 相交**（与 Trigger 回调无关）；村庄场景交互体 **Z 轴宜为 0**，与玩家锁定 Z 一致。
- **详细说明**：[场景切换.md](技术文档/场景相关/场景切换.md)（调用链、室内外手感、Stairs 案例、SceneManager 踩坑与验收清单）。
