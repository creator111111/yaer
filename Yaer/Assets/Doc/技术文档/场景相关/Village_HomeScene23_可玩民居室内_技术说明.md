# Village_HomeScene23 可玩民居室内 — 技术说明

> 文档日期：2026-08-04  
> 状态：**现网已可进屋游玩**（对齐 HomeScene2 室内清单；黑屏根因已修）  
> **曾用名**：`Village_HomeScene4`（2026-08-04 全量改名为本场景名）  
> Unity：2020.3.48f1  
> 关联：
>
> - 架构溯源：`Assets/Doc/执行文档/0804/Village_HomeScene23_进屋主角不出现_架构溯源报告.md`
> - NPC 对白配置（内容侧）：`Assets/Doc/执行文档/6月/0601/Village_HomeScene23_NPC对话配置_执行说明.md`
> - 样板场景：`Village_HomeScene2` / `Village_HomeScene2SceneManager`
> - OPEN：`Assets/Doc/OPEN_QUESTIONS.md`（Village_HomeScene23 节）
> - 对照：`场景相关/搭建新场景手册.md`、`场景相关/场景切换.md`

---

## 一、一句话定位

`Village_HomeScene23` 是肯姆尼村内 **可玩民居室内**（由 `House_Npc4` 进入），不是纯 UI 商店（`Village_Shop`），也不是磁盘缺失的 `Village_House4`。  
本期把半成品屋补成与 HomeScene2 同级的室内闭环：**能进屋、主角可见可走、右门回村**（左门已关）。

---

## 二、产品与场景身份

| 项 | 现网 |
|----|------|
| 场景文件 | `Assets/GameRes/Scenes/Village_HomeScene23.unity` |
| 场景常量 | `SceneName.Village_HomeScene23` |
| 村内入口 | `House_Npc4` → `NextSceneName = Village_HomeScene23` |
| SceneManager | `Village_HomeScene23SceneManager`（`nowSceneName` 对齐文件名） |
| Config | `Assets/GameRes/Config/SceneManagerConfig/Village_HomeScene23.asset`（`canCreatePlayer=1`，`isFightingScene=0`） |
| Build Settings | 已登记本场景 |
| 与 `Village_House4` | **不同目标**。村内多数 `House4*` 仍指向 `Village_House4`（场景文件当前可能缺失）——**另案**，勿与本屋混修 |
| 与 `Village_Shop` | **分离**：商店是纯 UI 场景，本屋是带玩家的室内 |

---

## 三、进屋 / 回村调用链

```
村 House_Npc4
  → LoadScene("Village_HomeScene23")
  → Village_HomeScene23SceneManager.OnInit（nowSceneName / PlaceName.KenMuNi）
  → InitPlayer（canCreatePlayer=1）
  → SetPlayerPos：EnterPos 命中 lastScene=Village_KenMuNi1 → RightBorn
  → Camera Follow

屋内 RightDoor（SceneChangeDoor，主出口）
  → NextSceneName = Village_KenMuNi1
  → 村 EnterPos：lastScene=Village_HomeScene23 → 门外 LeftBorn（与 House4 回村落点同 Transform）

LeftDoor：SceneChangeDoor 已禁用，不换场
```

---

## 四、室内交付清单（对齐 HomeScene2）

施工 / 验收时按此表核对：

| 项 | 要求 | HomeScene4 现网 |
|----|------|-----------------|
| `SceneName` 常量 | 与场景文件名一致 | `Village_HomeScene23` |
| 专用 SceneManager | `nowSceneName` 写对；`GetCurSceneTerrainType=IndoorType` | `Village_HomeScene23SceneManager` |
| Config 资产 | 本场景专用；`canCreatePlayer/canMove=1`，室内 `isFightingScene=0` | `Village_HomeScene23.asset` |
| Build Settings | 正式包可进 | 已加入 |
| `EnterPosConfig` | 从村进门 → **RightBorn**（近右门，约 `24.68,-1.3`） | `Village_KenMuNi1` → RightBorn |
| DefaultBorn | 建议与门口 Born 同高，作兜底 | Y 已对齐 `-1.3` |
| 左门 | **禁用** SceneChangeDoor（只走右门） | 已关、Next 清空 |
| 右门 | 启用；`NextSceneName=Village_KenMuNi1`；须有 Interactive；**本地坐标须为 MapRight 下 (0,0,0)** | **现网主出口**（勿再写成 local x≈-30，否则世界坐标落到房中） |
| Npc4 | 仿 Npc1 全套 + SimpleStoryTrigger | `StoryPrefabName=HomeScene1Npc4` |
| 村回村表 | `lastScene: Village_HomeScene23` + 门外落点 | 已补（pos 同 House4 LeftBorn） |
| 场景实体 | `SceneEntityComponentGSM.objRoot` 下 NPC 能被扫到并 OnInit | 运行时重扫 objRoot |

**不必**：重写 `LoadScene` / `InitPlayer` 总管线。

---

## 五、关键文件

| 类别 | 路径 |
|------|------|
| 场景 | `Assets/GameRes/Scenes/Village_HomeScene23.unity` |
| Manager | `…/Scene/Village_House/Village_HomeScene23SceneManager.cs` |
| Config | `Assets/GameRes/Config/SceneManagerConfig/Village_HomeScene23.asset` |
| 常量 | `Assets/Scripts/Game/Static/Name/Res/SceneName.cs` |
| 门逻辑 | `…/CommonEntity/SceneChangeDoor.cs` |
| 实体注册 | `…/SceneEntityComponentGSM.cs` |
| 村场景回村表 | `Assets/GameRes/Scenes/Village_KenMuNi1.unity`（`EnterPosConfig`） |
| 样板 Manager | `Village_HomeScene2SceneManager.cs` |

---

## 六、黑屏事故与修复（2026-08-04）

### 6.1 现象

进屋后 **Game 全黑**；Console：

1. `GameFrameworkException: 没有找到组件 InteractiveComponent`
2. `场景对象未到场景管理器注册并初始化=>Npc1`
3. 同上 `=>NpcXiaer`

### 6.2 根因

1. 为回村**启用**了 `LeftDoor` 的 `SceneChangeDoor`，但门上 **没有** `InteractiveComponent`（`Components` 空、`componentsList` 空）。  
2. `Map.OnInit` → `leftDoor.OnInit` → `componentSystem.GetComponent<InteractiveComponent>()` **抛异常**。  
3. SceneManager / 模块初始化中断 → 黑幕/相机/玩家链路未跑完 → **黑屏**。  
4. Npc「未注册」是连带：实体 `OnInit` 未完成或列表漏挂，`Start` 检测到 `isInit=false`。

> 生活类比：门修好了开关，但没装感应器；门禁程序一查就崩溃，整栋楼灯全灭。

### 6.3 修复

| 改动 | 说明 |
|------|------|
| 左门补齐交互三件套 | `InteractiveComponent` + `EntityControl` + 碰撞 Listener；挂入 `ComponentSystem.componentsList`（对齐 HomeScene2） |
| `SceneChangeDoor.OnInit` | 改为 `TryGetComponent`；缺件打 Error 并跳过，**不再抛崩整场景** |
| `SceneEntityComponentGSM.OnInit` | 运行时按 `objRoot.GetComponentsInChildren<SceneEntity>` **重扫**，避免 YAML `sceneObjs` 漏挂 NpcXiaer |

### 6.4 门交互硬规则（后续民居必遵）

启用 `SceneChangeDoor` 前必须齐：

1. 门物体：`SceneEntity` + `ComponentSystemMono` + `SceneChangeDoor`  
2. 子节点或同体：`InteractiveComponent`（碰撞引用门 Trigger）  
3. `EntityControl`（`entityType` 门类、指向 SceneEntity / Interactive）  
4. `componentsList` 含该 Interactive  
5. `NextSceneName` 非空（回村填 `Village_KenMuNi1`）

缺 Interactive 时现网不再黑屏，但门**不能用**——仍须按上表补齐。

### 6.5 右门回不去村（2026-08-04）

**现象：** 走到画面右侧出不去 / 无 E 提示。

**根因：** `MapRight` 在世界 `x=28.8`，但 `RightDoor` / `RightWall` 本地坐标误为 `(-30.55,0)` / `(-30.56,…)` → 世界约 **`x≈-1.75`（房中）**。玩家走到右侧地图边界时，交互盒根本不在那里。样板 `Village_HomeScene2` 的 RightDoor/RightWall 本地均为 `(0,0,0)`。

**修复：**

| 物体 | 修正 |
|------|------|
| `RightDoor` | local → `(0,0,0)`（世界 ≈28.8）；`NextSceneName=Village_KenMuNi1` 不变 |
| `RightWall` | local → `(0,0,0)`；碰撞对齐样板 `offset=(1,0) size=(2,20)` |
| `RightBorn` / `DefaultBornPos` | → `(24.68,-1.3)`，进门落在右门内侧 |

**替代方案：** 不改本地坐标、改把 `MapRight` 挪到门所在处——会打乱左右边界与相机夹限，不采用。

---

## 七、NPC 备注（非黑屏主因）

| 物体 | 说明 |
|------|------|
| `Npc1` | `SimpleStoryTrigger`，`StoryPrefabName=HomeScene1Npc1`；须在实体注册后才能交互 |
| `NpcXiaer` | 曾挂龙宫专用 `HomeScene1Xiaer`；对话资源路径可能仍指向 `HomeScene1/` 子目录约定外路径——**对白内容另案**（见 0601 执行说明），与「能进屋」无关 |

可对话 NPC 最小件：交互碰撞 + 触发脚本 + `Dialogue/{名}.prefab`（根目录约定）。

---

## 八、验收清单

1. InitScene → 村 → `House_Npc4` → 进屋主角出现在门口可走。  
2. Hierarchy 有 Player；相机跟随。  
3. Console **无** InteractiveComponent 异常、无整场景初始化中断。  
4. **右门**回 `Village_KenMuNi1`，落在门外合理位置；左门不换场。  
5. 靠近 Npc4 出 E，按 E 触发 `HomeScene1Npc4`（占位对白）。

---

## 九、常见坑

| 坑 | 说明 |
|----|------|
| 误当 `Village_House4` | 文件名 / 常量 / 村门目标三者必须一致；House4 场景缺失另开任务 |
| EnterPos 空 | 落到错误 DefaultBorn 高度 → 「看不见 / 卡死」像没生成玩家 |
| 只启用门脚本不补 Interactive | 曾直接导致黑屏；现降级为 Error，仍须补件才能出门 |
| `sceneObjs` 只写一部分 | 漏挂的 NPC Start 报未注册；现网 OnInit 会重扫 objRoot |
| 与 Shop 混用 | Shop 不生成玩家；本屋必须 `canCreatePlayer=1` |

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-04 | 初版：民居可玩闭环 + 左门 Interactive / 实体重扫 / SceneChangeDoor 防崩黑屏 |
| 2026-08-04 | 改右门主链回村；左门关闭；EnterPos→RightBorn；Npc4 绑齐对话组件（`HomeScene1Npc4`） |
