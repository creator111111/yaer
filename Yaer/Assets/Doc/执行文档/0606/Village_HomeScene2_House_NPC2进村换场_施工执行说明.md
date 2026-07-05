# Village_HomeScene2 — House_NPC2 进村换场 — 施工执行说明

**施工员交付（待实施）** | 日期：2026-06-06  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【施工员】最小化修改、先可运行
- 架构侦探结论（本对话）：`House_NPC2` → `Village_HomeScene2` 换场链路
- 户外样板：`Village_KenMuNi1` 中已有 `House4` / `House_NPC2`（`Stairs.prefab` + `SceneChangeDoor`）
- 室内仿照基准：`Assets/Doc/执行文档/0530/Village_House4场景管理器_施工执行说明.md`、`Village_House4SceneManager.cs`
- 换场总览：`Assets/Doc/技术文档/场景相关/场景切换.md`、`Assets/Doc/执行文档/0518/SceneChangeDoor场景切换_架构溯源与执行说明.md`

**Unity 版本**：2020.3.48f1  

---

## 1. 目标（一句话）

打通 **`Village_KenMuNi1` / `House_NPC2` 按 E 进门 → `Village_HomeScene2` 室内游玩 → 左门回村** 整条换场链路：户外入口已基本就位，**重点补齐屋内场景管理器、落点表、出门回村与 Build 登记**。

---

## 2. 背景与现状缺口

### 2.1 你在 Unity 里期望看到的现象

| 步骤 | 期望 |
|------|------|
| 村里走到第二户门口 | 出现 **E** 提示 |
| 按 E | 黑幕换场，进入 `Village_HomeScene2` |
| 进屋后 | 玩家站在门口内侧；**室内走姿** + **室内脚步** |
| 室内左门出村 | 回到 `Village_KenMuNi1`，落在 `House_NPC2` 门外 |

### 2.2 静态阅读结论（2026-06-06）

#### 户外侧 `Village_KenMuNi1` — **已基本完成**

| 项目 | 当前状态 | 是否阻塞 |
|------|----------|----------|
| 入口物体 | `Objects`（`objRoot`）下已有 **`House_NPC2`**（`Stairs.prefab` 实例） | 否 |
| `NextSceneName` | 已填 **`Village_HomeScene2`** | 否 |
| 实体登记 | `House_NPC2` 的 `SceneEntity` 已在 **`SceneEntityComponentGSM.sceneObjs`** | 否 |
| 从屋里返回落点 | `EnterPosConfig` **仅有** `Village_House4`、`ForestEastScene`，**无 `Village_HomeScene2`** | **是** |

#### 屋内侧 `Village_HomeScene2` — **缺口较多**

| 项目 | 当前状态 | 是否阻塞 |
|------|----------|----------|
| 场景文件 | `Assets/GameRes/Scenes/Village_HomeScene2.unity` 存在 | — |
| `SceneName.Village_HomeScene2` | **未**在 `SceneName.cs` 登记 | 建议补（非硬阻塞） |
| Build Settings | 登记的是 **`Village_House2.unity`**（磁盘上已不存在） | **是** |
| `SceneManager` 脚本 | 仍挂 **`HomeScene1Manager`** | **是**（`nowSceneName` 会报 `HomeScene1`） |
| `config` | 引用 **`HomeScene1.asset`**（`isFightingScene: 0`，动画尚可） | 建议换专用 Config |
| `EnterPosConfig` | 仅有 `ForestScene`、`HomeScene2`，**无 `Village_KenMuNi1`** | **是**（进门落点错） |
| `MapLeft/LeftDoor` | `NextSceneName` **为空**，组件 **Disabled** | **是**（出不了村） |
| `MapRight/RightDoor` | `NextSceneName: ForestScene`（模板残留） | **是**（误触发出错图） |
| `Village_HomeScene2SceneManager` | **不存在** | **是** |

### 2.3 与 `House4` / `Village_HomeScene4` 的关系

本任务与 House4 链路**同架构、不同场景名**：

| 维度 | House4 样板 | 本任务 House_NPC2 |
|------|-------------|-------------------|
| 村里物体名 | `House4` | **`House_NPC2`** |
| 目标场景 | `Village_HomeScene4` | **`Village_HomeScene2`** |
| 户外预制体 | `Assets/Prefabs/Stairs.prefab` | 同左 |
| 交互方式 | 靠近 E → 点击换场 | 同左 |

**重要**：`House_NPC2` 是**换场门**（`SceneChangeDoor`），不是屋内对话 NPC（`SimpleStoryTrigger`）。屋内 NPC 对白另见 `Village_HomeScene4_屋内NPC对白台本_执行说明.md` 一类文档。

---

## 3. 换场链路（施工验收对照）

```
[Village_KenMuNi1] House_NPC2（SceneChangeDoor.NextSceneName = Village_HomeScene2）
  → LoadSceneComponentGSM.LoadScene("Village_HomeScene2")
  → ChangeSceneComponentGM.LastSceneName = "Village_KenMuNi1"
  → 加载 Village_HomeScene2.unity
  → Village_HomeScene2SceneManager.OnInit（待建）
      → nowSceneName = Village_HomeScene2
      → Config.isFightingScene == false → Home 行走动画
      → GetCurSceneTerrainType → IndoorType（室内脚步）
  → SetPlayerPos：EnterPosConfig 匹配 lastScene == Village_KenMuNi1 → 室内门口 Transform

[Village_HomeScene2] MapLeft/LeftDoor
  → LoadScene("Village_KenMuNi1")
  → LastSceneName = Village_HomeScene2
  → Village_KenMuNi1.EnterPosConfig 匹配 lastScene == Village_HomeScene2 → House_NPC2 户外落点
```

**再次强调**：门上的 `bornPos` **不参与**运行时坐标；落点以目标场景 **`EnterPosConfig`** 为准。

---

## 4. 仿照对照表（Village_House4 → Village_HomeScene2）

| 维度 | `Village_House4`（已交付样板） | `Village_HomeScene2`（本任务） |
|------|-------------------------------|--------------------------------|
| 场景文件 | `Village_HomeScene4.unity` | **`Village_HomeScene2.unity`** |
| 管理器类 | `Village_House4SceneManager` | **`Village_HomeScene2SceneManager`**（新建） |
| `nowSceneName` | `SceneName.Village_House4` | **`SceneName.Village_HomeScene2`** |
| Config | `Village_House4.asset` | **`Village_HomeScene2.asset`**（复制 HomeScene1 或 House4 改参） |
| `GetCurSceneTerrainType` | `IndoorType` | 同左 |
| `SetNowPlace` | `PlaceName.KenMuNi` | 同左 |
| 进村来源 | `Village_KenMuNi1` / `House4` | **`Village_KenMuNi1` / `House_NPC2`** |
| 室内出门 | `LeftDoor` → `Village_KenMuNi1` | 同左 |
| 首版剧情 / BGM | 不做 | 不做 |

---

## 5. 代码侧施工清单

### 5.1 `SceneName.cs` 登记常量

**文件**：`Assets/Scripts/Game/Static/Name/Res/SceneName.cs`

在 `Village_House4` 常量附近增加：

```csharp
/// <summary>
/// 肯姆尼第二户民居室内（<c>Assets/GameRes/Scenes/Village_HomeScene2.unity</c>）；由村里 House_NPC2 进入。
/// </summary>
public const string Village_HomeScene2 = "Village_HomeScene2";
```

> **替代方案**：门上直接填字符串 `"Village_HomeScene2"` 可临时试跑，但存档 / `LastSceneName` 匹配易写错，**不推荐长期用**。

### 5.2 新建 `Village_HomeScene2SceneManager.cs`

| 项 | 建议值 |
|----|--------|
| **目录** | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/`（与 `Village_House4SceneManager` 并列） |
| **类名** | `Village_HomeScene2SceneManager` |
| **命名空间** | `Game.GameRuntime.GameSceneManager.Scene.Village_House` |

**推荐代码骨架**（照抄 `Village_House4SceneManager`，仅改场景名与注释）：

```csharp
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.ChangeScene;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_House
{
    /// <summary>
    /// 肯姆尼第二户民居 <see cref="SceneName.Village_HomeScene2"/> 室内场景管理器。
    /// 行为对齐 <see cref="Village_House4SceneManager"/> 的「室内」最小集。
    /// </summary>
    /// <remarks>
    /// 替代方案：继续挂 HomeScene1Manager 可临时试玩，但 nowSceneName 仍为 HomeScene1，落点/存档/任务易错，故不采用。
    /// </remarks>
    public class Village_HomeScene2SceneManager : BaseGameSceneManager
    {
        public override void OnInit()
        {
            base.OnInit();
            nowSceneName = SceneName.Village_HomeScene2;
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            var lastScene = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageHomeScene2Debug] lastScene={lastScene} place={PlaceName.KenMuNi}");
        }

        /// <summary>室内脚步资源：室内走{0}.mp3</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
```

### 5.3 新建 `Village_HomeScene2.asset`（SceneManager Config）

| 步骤 | 操作 |
|------|------|
| 1 | 复制 `Assets/GameRes/Config/SceneManagerConfig/Village_House4.asset` |
| 2 | 重命名为 **`Village_HomeScene2.asset`** |
| 3 | 确认 **`is Fighting Scene` = 0**、`can Create Player` = 1 |
| 4 | 在 `Village_HomeScene2.unity` 的 `SceneManager` 上把 `config` 指向新 asset |

### 5.4 Editor Inspector（可选）

可复制 `Village_House4MgrInsp.cs` 为 `Village_HomeScene2MgrInsp.cs`，空继承 `BaseGameSceneMgrInspector`，仅改 `[CustomEditor]` 目标类。

---

## 6. Unity 场景侧施工清单

### 6.1 户外 `Village_KenMuNi1` — 核对 + 补返回落点

#### 6.1.1 核对 `House_NPC2`（若已存在则跳过创建）

1. 打开 **`Village_KenMuNi1.unity`**。  
2. Hierarchy：`SceneManager` → `SceneEntityComponentGSM` → **`Objects`** 下应有 **`House_NPC2`**。  
3. 选中 `House_NPC2`，Inspector 核对：

| 检查项 | 期望值 |
|--------|--------|
| 组件 | `SceneEntity`、`SceneChangeDoor`、`InteractiveComponent`、`BaseEntityControll` |
| **Next Scene Name** | `Village_HomeScene2` |
| **Trigger When Move In** | 不勾选（按 E 进门） |
| **Position Z** | `0` |
| `InteractiveComponent.interactiveCollider` | 根物体 BoxCollider2D（Is Trigger 已勾） |

4. 选中 `SceneEntityComponentGSM`：`Scene Objs` 列表含 `House_NPC2` 的 `SceneEntity`。

> 若需**新建**（当前场景无该物体）：复制 `House4` 实例 → 重命名 `House_NPC2` → 改 `NextSceneName` → 摆到第二户门口 → 刷新 `sceneObjs`。

#### 6.1.2 补「从屋里出来」落点

1. 在 `House_NPC2` 门外摆空物体，命名如 **`ExitFrom_HomeScene2`**。  
2. 选中根 **`SceneManager`** → **Enter Pos Config** → **Add**：  
   - **last Scene** = `Village_HomeScene2`（与屋内 `nowSceneName` **完全一致**）  
   - **pos** = 拖入 `ExitFrom_HomeScene2`  

未配置时，从屋里左门出来会落到 **`DefaultBornPos`**，表现为「出门瞬移异常」。

---

### 6.2 屋内 `Village_HomeScene2` — 主施工区

#### 6.2.1 SceneManager

1. 打开 **`Village_HomeScene2.unity`**。  
2. 根 **`SceneManager`**：  
   - 脚本改为 **`Village_HomeScene2SceneManager`**（替换 `HomeScene1Manager`）  
   - **config** → `Village_HomeScene2.asset`  
   - **is Can Touch With Other** = 勾选  

#### 6.2.2 进村落点 `EnterPosConfig`

1. 在 `Map` 门口内侧摆空物体，命名如 **`EnterFrom_Village`**。  
2. `SceneManager` → **Enter Pos Config**：  
   - **新增**一条：**last Scene** = `Village_KenMuNi1`，**pos** = `EnterFrom_Village`  
   - **删除或忽略**模板残留项 `ForestScene`、`HomeScene2`（避免策划误读；无匹配时不会用到，但建议清理）  

#### 6.2.3 左门回村

路径：`SceneManager` → `Map` → `MapLeft` → **`LeftDoor`**

| 检查项 | 期望值 |
|--------|--------|
| `SceneChangeDoor` **Enabled** | 勾选 |
| **Next Scene Name** | `Village_KenMuNi1` |
| **Trigger When Move In** | 不勾选（与 House4 室内左门一致，按 E 出门） |

#### 6.2.4 右门（模板残留）

路径：`Map` → `MapRight` → **`RightDoor`**

| 做法 | 说明 |
|------|------|
| **推荐** | 将物体 **Inactive**，或清空 `NextSceneName` |
| 现状 | 指向 `ForestScene` 且 `TriggerWhenMoveIn = true`，走进即误换场 |

#### 6.2.5 屋内 NPC（本任务范围外）

若屋内要可对话 NPC，另按 `Village_HomeScene4_NPC对话配置_执行说明.md` 复制 `Entity/Npc1` + `SimpleStoryTrigger`。**与本换场任务无依赖关系**。

---

## 7. 资源与 Build 登记

| 步骤 | 操作 |
|------|------|
| 1 | **File → Build Settings**：移除失效的 `Village_House2.unity`（若仍登记） |
| 2 | 将 **`Assets/GameRes/Scenes/Village_HomeScene2.unity`** 加入 **Scenes In Build** |
| 3 | 按根目录 `README.md` 将场景加入 **Resource Editor / AB**（若项目用 GF 资源包加载） |

> **风险**：Build 里仍是 `Village_House2` 而磁盘只有 `Village_HomeScene2` 时，Console 会报加载失败。

---

## 8. 验收清单（Play 模式）

**必须从 `InitScene` 进游戏**（不要单独 Open `Village_HomeScene2` 再 Play）。

| # | 操作 | 通过标准 |
|---|------|----------|
| Q1 | 进村，走到 `House_NPC2` | 出现 **E**；Console 无 `GetFirstCanTouchEntiy result=null` |
| Q2 | 按 E | 黑幕换场；无「加载资源失败: Village_HomeScene2」 |
| Q3 | 进屋后 | 站在 `EnterFrom_Village` 附近；**室内走姿**（非战斗跑） |
| Q4 | Console | `[VillageHomeScene2Debug] lastScene=Village_KenMuNi1` |
| Q5 | 室内左门按 E 出村 | 回到 `Village_KenMuNi1`，落在 `ExitFrom_HomeScene2` 附近 |
| Q6 | 存档标题 | 地点仍显示肯姆尼相关（`PlaceName.KenMuNi`） |

---

## 9. 故障排查

| 现象 | 优先检查 |
|------|----------|
| 无 E、按 E 无反应 | `House_NPC2` 是否在 `objRoot` / `sceneObjs`；Z 是否为 0；碰撞盒是否够大 |
| 加载失败 | Build / AB 是否含 `Village_HomeScene2`；`NextSceneName` 与文件名一致 |
| 进门落点错 | `Village_HomeScene2.EnterPosConfig` 是否有 `lastScene: Village_KenMuNi1` |
| 出门落点错 | `Village_KenMuNi1.EnterPosConfig` 是否有 `lastScene: Village_HomeScene2` |
| 仍是战斗跑姿 | `SceneManager` 是否仍挂 `HomeScene1Manager`；Config `isFightingScene` 是否为 0 |
| 走进室内右侧出门区就换图 | `RightDoor` 是否仍指向 `ForestScene` 且 `TriggerWhenMoveIn` 为 true |

---

## 10. 施工文件一览（完成后应改动）

| 类型 | 路径 |
|------|------|
| 常量 | `Assets/Scripts/Game/Static/Name/Res/SceneName.cs` |
| 管理器（新建） | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene2SceneManager.cs` |
| Config（新建） | `Assets/GameRes/Config/SceneManagerConfig/Village_HomeScene2.asset` |
| 户外场景 | `Assets/GameRes/Scenes/Village_KenMuNi1.unity`（`EnterPosConfig` 补返回落点；核对 `House_NPC2`） |
| 屋内场景 | `Assets/GameRes/Scenes/Village_HomeScene2.unity`（Manager、EnterPosConfig、LeftDoor、RightDoor） |
| Build | `ProjectSettings/EditorBuildSettings.asset` |

---

## 11. 替代方案说明

| 方案 | 适用 | 风险 |
|------|------|------|
| **本文：专用 SceneManager + EnterPosConfig 双向表**（推荐） | 正式进村 / 出村 | 改动面小，与 House4 一致 |
| 仅改门上 `NextSceneName`，屋内不改 | 临时看室内美术 | 落点错、`nowSceneName` 错、出不了村 |
| 对话 `LoadSceneTaskAction` 代替门 | 「先对白再进屋」 | 与 `House_NPC2` 换场门语义不同，需另挂 `SimpleStoryTrigger` |
| 继续用 `HomeScene1Manager` | 极短期演示 | `LastSceneName` 匹配与存档场景名长期混乱 |

---

## 12. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-06-06 | 初版：基于架构侦探静态阅读；确认 `House_NPC2` 已在 `Village_KenMuNi1` 就位；列出 `Village_HomeScene2` 与 Build 缺口及施工顺序 |

**文档路径**：`Assets/Doc/执行文档/0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md`
