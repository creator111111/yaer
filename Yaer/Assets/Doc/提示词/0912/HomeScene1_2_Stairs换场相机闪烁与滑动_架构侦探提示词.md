# Cursor Agent Prompt · HomeScene1 ↔ HomeScene2：Stairs 换场相机闪烁 / 左右滑动

> **角色**：先【架构侦探】只读溯源；根因拍板后再【施工员】最小化修复  
> **日期**：2026-09-12  
> **场景（钉死）**：龙宫室内 `HomeScene1` / `HomeScene2`（**不是** `Village_HomeScene1/2`）  
> **场景文件**：`Assets/GameRes/Scenes/HomeScene1.unity`、`Assets/GameRes/Scenes/HomeScene2.unity`  
> **触发物**：Hierarchy `Object/Stairs`（两侧均有；红箭头指向该物体）  
> **同场景关键节点**：`SceneManager`、`Camera`（与 `Object` 同级）  
> **现象（用户实测）**：  
> 1. **1 → 2**：换场后摄像机会 **闪一下**（穿帮闪烁）  
> 2. **2 → 1**：摄像机不是立刻定格，而是 **从左往右滑一下**（可见运动）  
> **产品期望（钉死）**：Stairs 换场后，黑幕揭开时镜头已在正确机位；**无闪帧、无可见滑动/追镜头**；像「直接定格」  
> **不是**：改楼梯美术/碰撞形状；改村庄 CameraDepth；改村长家 Stairs；改剧情 NodeCanvas（除非证据证明换场被剧情二次改相机）  
> **报告落盘**：`Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 龙宫一楼和二楼用楼梯切场景。  
> 上楼（1→2）镜头会闪一下；下楼（2→1）镜头会从左往右挪一下，不像直接卡在该停的位置。  
> 要查清：**相机是谁在什么时机跟谁、有没有平滑/阻尼、黑幕和落点谁先谁后**，再定怎么消穿帮。

### 术语钉死（避免和村屋混淆）

| 词 | 本案含义 |
|----|----------|
| `HomeScene1` / `HomeScene2` | 龙宫室内楼层；Manager = `HomeScene1Manager` / `HomeScene2Manager` |
| `Village_HomeScene*` | 肯姆尼民居，**本案禁止当主线分析**（仅可作「室内换场对照」） |
| `Stairs` | `Object` 下交互换场体，预期挂 `SceneChangeDoor`（或等价换场逻辑） |
| 「闪一下」 | 黑幕揭开前后出现错误机位/默认机位/上一帧跟随残影，再跳到正确位 |
| 「从左到右运动」 | 可见的相机平移（SmoothDamp / Cinemachine Damping / Framing 追目标），不是玩家位移本身 |

### Hierarchy 锚点（用户截图）

**HomeScene1**  
`HomeScene1` → `Map` / `Object`(含 `Stairs`、Npc、花、石、`NpcXiaer`、`GoOutStoryCollider`) / `SceneManager` / `Camera`

**HomeScene2**  
`HomeScene2` → `Map` / `Object`(含 `Box`、`Stairs`、花、`Door`、`NpcXiaer`) / `SceneManager` / `Camera`

### 现网换场 + 相机链（助手预扫 · 高度可疑）

```
Stairs(SceneChangeDoor?) → OnInteractive / EnterDoor
  → LoadSceneComponentGSM.LoadScene(目标场景, blackFade 默认 true?)
  → ChangeSceneComponentGM：LastSceneName = 卸场前 NowSceneName
  → 卸载旧场景 → 加载 Assets/GameRes/Scenes/{HomeScene1|2}.unity
  → 新场景 BaseGameSceneManager.Awake → OnInit → Ready
  → InitPlayer → SetPlayerPos（EnterPosConfig 按 LastSceneName 匹配）
  → CameraComponentGSM.SetFollow(player)   ← 默认 forceSnapToTarget=true
       → CameraComponent.SetFollow
            · 若 smoothTime > 0：手推 SmoothDamp「对齐」再绑 Follow  ← ★ 左右滑动头号嫌疑
            · 若 smoothTime ≈ 0：当帧 ApplyFollowWithCinemachineStateAligned
  → Framing Transposer X/Y Damping / DeadZone  ← ★ 二次追镜头嫌疑
  → BlackPanel 渐亮时机 vs 相机首帧机位  ← ★ 闪烁头号嫌疑
```

**规范锚点（须对照）**

- `Assets/Doc/02_SYSTEM_SPEC.md` §3：跟拍走 `CameraComponent.SetFollow`，禁固定 Wait 硬匹配  
- `Assets/Doc/技术文档/场景相关/场景切换.md`：Stairs = 交互门变体；落点靠目标场景 `EnterPosConfig`；`bornPos` 运行时不读  
- `BaseGameSceneManager.InitPlayer`：`SetPlayerPos` 后立刻 `SetFollow(player)`  
- `CameraComponent.SetFollow(..., forceSnapToTarget)`：`smoothTime>0` 时走 `_isSmoothingSnap` 手推

**关键代码 / 资源（侦探须打开核实）**

| 主题 | 路径 |
|------|------|
| 门/楼梯换场 | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs` |
| 黑幕换场 | `.../LoadSceneComponentGSM.cs`、`ChangeSceneComponentGM` |
| 玩家落点 | `BaseGameSceneManager.SetPlayerPos` + 两侧 `SceneManager.EnterPosConfig` |
| 进场跟拍 | `BaseGameSceneManager.InitPlayer` → `CameraComponentGSM.SetFollow` |
| 平滑/瞬切 | `CameraComponent.cs`（`smoothTime` / `followSnapOffset` / `_isSmoothingSnap`） |
| Manager | `HomeScene1Manager.cs`、`HomeScene2Manager.cs` |
| Stairs Prefab 参考 | `Assets/Prefabs/Stairs.prefab`（场景实例可能覆盖） |
| 场景 | `HomeScene1.unity`、`HomeScene2.unity` 上 `Stairs`、`Camera`、`EnterPosConfig`、Cinemachine 参数 |

### 嫌疑优先级（侦探按证据排序，可推翻）

| # | 嫌疑 | 更像解释哪种现象 |
|---|------|------------------|
| A | `SetFollow` 手推平滑（`smoothTime>0`）或 CM `XDamping` | 2→1「从左到右滑」 |
| B | 黑幕 `FadeHide` 早于相机对齐完成（先露出错误机位再跳/再追） | 1→2「闪一下」；也可能双向都有 |
| C | 新场景 `Camera`/`vcam` 默认世界坐标远离落点，Follow 绑定前渲了一帧 | 闪烁 |
| D | `EnterPosConfig` 落点与楼梯口视觉不一致，镜头从「错误落点」追到正确感 | 滑动；需对比两侧 lastScene 配置 |
| E | 两侧 `Camera` OrthoSize / BoundingArea / Follow offset 不一致，切场后校正过程可见 | 闪或滑 |
| F | `OnEnterScene` 剧情（`HomeScene1FirstEnter` 等）二次改相机（仅特定存档条件） | 偶发；须排除「非首次」仍复现 |

### 复现矩阵（侦探须实测填）

| 方向 | 操作 | 闪烁？ | 左右滑动？ | Console 要点 |
|------|------|--------|------------|--------------|
| 1→2 | 在 HomeScene1 靠近 Stairs 按 E（或 Trigger）进 HomeScene2 | | | `[SceneChangeDoor]` / LastSceneName / 相机 Log |
| 2→1 | 在 HomeScene2 用 Stairs 回 HomeScene1 | | | 同上 |
| 对照 | 同场景内不换场，只走路 | — | — | 确认「滑」不是日常 Follow 阻尼 |
| 对照 | 若有其它门（如 HomeScene2 `Door`）换场 | | | 判断是否 **仅 Stairs** |

**最短路径**：从 Init/读档进到龙宫可走状态 → Stairs 上楼 → 立刻 Stairs 下楼，各录一次。

### 侦探须回答的核心问题

1. **Stairs 实际脚本与 Inspector**：是否 `SceneChangeDoor`？`NextSceneName`、`ShowLoadingUI`、`TriggerWhenMoveIn`、是否在 `sceneObjs`？  
2. **换场表现**：默认黑幕还是 Loading？黑幕 Show/Hide 与 `InitPlayer`/`SetFollow` 的时序谁先谁后？  
3. **落点**：两侧 `EnterPosConfig` 中 `lastScene=HomeScene1/HomeScene2` 的 Transform 世界坐标；与 Stairs / DefaultBorn 距离；`SetPlayerPos` 是否命中？  
4. **跟拍契约**：进场 `SetFollow` 的 `forceSnapToTarget`、场景 `CameraComponent.smoothTime`、CM FramingTransposer `m_XDamping/m_YDamping`、DeadZone；手推是否跨过多帧？  
5. **闪与滑是否同源**：同一根因双向不同表现，还是 1→2 偏「揭幕时机」、2→1 偏「平滑追位」？  
6. **修复策略（只方案不施工）**：优先哪条最小化路径？例如：换场瞬间强制 `smoothTime=0` 瞬切、黑幕揭开延后到 `SetFollow` onComplete、进场前预置 vcam 到落点、仅 Stairs 路径特殊处理等。列出利弊与是否违反 `02_SYSTEM_SPEC` §3。

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity 2020.3.48f1 / C#。只读分析，**禁止**改代码、Prefab、场景、Git。

### 必读

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md`（尤其摄像机 §3）  
3. `Assets/Doc/技术文档/场景相关/场景切换.md`（Stairs / EnterPos / 黑幕）  
4. 本提示词「预梳理」——须核实，勿当唯一真相  

### 扫描范围（按优先级）

1. 场景 YAML：`HomeScene1.unity` / `HomeScene2.unity` 中 `Stairs`、`SceneManager`（EnterPosConfig）、`Camera`/`CinemachineVirtualCamera`/`CameraComponent` 序列化字段  
2. `SceneChangeDoor` → `LoadSceneComponentGSM` → `ChangeSceneComponentGM` → `BaseGameSceneManager.InitPlayer` / `SetPlayerPos`  
3. `CameraComponentGSM` / `CameraComponent.SetFollow` 平滑与瞬切分支  
4. `HomeScene1Manager` / `HomeScene2Manager` 的 `OnEnterScene` 是否条件触发改相机  
5. 必要时对照村屋室内换场（**仅对照**，勿把村案当本案根因）

### 输出要求

写到：`Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md`

报告固定结构：

1. **结论一句话**（闪 / 滑各自最可能根因；是否同源）  
2. **调用链**（Stairs → 落点 → SetFollow → 黑幕揭开；标出「错误机位可能露出」的帧窗口）  
3. **证据表**（场景字段值、代码行、时序；区分「已证实 / 高度可疑 / 已排除」）  
4. **双向差异**：为何 1→2 偏闪、2→1 偏左右滑（落点距离、smooth、阻尼、黑幕时长等）  
5. **修复方案对比**（≥2 种）：改动量、风险、是否符合现有架构；**推荐一种**并写清验收标准  
6. **OPEN_QUESTIONS**：设计不清处记入 `Assets/Doc/OPEN_QUESTIONS.md`，勿擅自改核心设计  

### 限制

- 不改代码；不提交 Git  
- 不扩大到战斗镜头 / 村庄纵深相机 / 其它场景 Stairs，除非证明共用同一 `CameraComponent` 全局字段且改动会影响本案  
- 默认中文；大白话 + 给程序的路径/字段名  

---

## 【施工员】Prompt（侦探报告拍板后再复制；现在不要执行）

> **前置**：已有溯源报告，且推荐方案已获开发者确认。  
> **角色**：【施工员】最小化修改；保持现有架构；禁止在 Update 堆砌业务逻辑。  
> **目标**：`HomeScene1` ↔ `HomeScene2` 经 `Stairs` 换场后，揭幕时镜头已在正确机位；无闪烁、无可见左右滑动。  
> **允许**：在换场/进场相机对齐路径做最小改动（例如瞬切、对齐完成后再揭幕、进场预置机位）；必要时只调本案场景 `CameraComponent`/`EnterPos` 配置。  
> **禁止**：重写整套 Cinemachine；为消症状关掉全部场景跟拍；改 `Village_*` 纵深相机逻辑；擅自改无关剧情。  
> **文档**：施工说明写入 `Assets/Doc/施工说明/0912/HomeScene1_2_Stairs换场相机消穿帮_施工说明.md`；若形成可复用约定，补一句到 `技术文档/场景相关`（或场景切换.md 短节）。  
> **验收**：双向 Stairs 各走 ≥2 次；黑幕揭开无错误机位闪帧；无左→右可见追镜；日常同场景走路跟拍不被破坏。  

---

## 【验收员】Prompt（施工后可选）

> 加短生命周期 Debug（如 `[HomeStairsCamDebug]`）：记录 LastSceneName、落点世界坐标、SetFollow 时 vcam 位置、smoothTime、手推开始/结束、BlackPanel Hide 时刻。  
> 优先查配置：EnterPos、CameraComponent.smoothTime、CM Damping、Stairs NextSceneName、黑幕参数。  
> 输出：通过/失败项 + 剩余风险。  
