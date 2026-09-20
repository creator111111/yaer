# Cursor Agent Prompt · ForestScene 演出结束：摄像机位置不对 + 屏闪

> **角色**：先【架构侦探】只读溯源；根因拍板后再【施工员】最小化修复  
> **日期**：2026-09-12  
> **场景（钉死）**：`ForestScene`（`Assets/GameRes/Scenes/ForestScene.unity`）  
> **Hierarchy 锚点（用户截图）**：`ForestScene` 下 `Camera` / `SceneManager` / `NPC`（灰=未激活）/ `Map` / `Objects`；另有 `InitScene`、`DontDestroyOnLoad`  
> **现象（用户实测）**：  
> 1. **演出结束时**摄像机 **位置不对**  
> 2. 伴随 **屏闪**（穿帮闪一下）  
> **产品期望（钉死）**：演出结束后镜头应立刻（或黑幕掩护下）落在 **跟玩家的正确机位**；无错误机位闪帧、无可见乱滑再追上  
> **不是**：改 ForestEast 树桥相机；改村庄 Part3 双 VCam；改龙宫 Stairs 案（可作对照）；改演出台词/立绘  
> **报告落盘**：`Assets/Doc/执行文档/0912/ForestScene_演出结束相机位置与屏闪_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 森林场景某段演出播完，镜头该回到跟着雅儿的位置，结果机位错了，还闪一下屏幕。  
> 要查清：**是哪一段演出结束**、结束时谁在改相机、为何会露出错误机位。

### 先钉「是哪段演出」（侦探必做）

用户未点名剧情 Prefab。ForestScene 多条 Dialogue；**门口大演出**最可疑，须先确认复现路径：

| 候选 | 触发 | Prefab / 脚本 |
|------|------|----------------|
| **A. 门口林恩线（首选）** | `HomeDoorStoryTrigger`（`homeDoorStoryComplete==false`） | `ForestSceneLaiFlyStory` → `ForestSceneLinEnStory` →（`CameraMoveEnd`）→ `ForestSceneYaerAfterLinEnStory`；收束脚本 `ForestSceneLinEnStory.cs` |
| B. 其它 ForestScene 对白 | 路牌/兔子/石头等 | `ForestScene*.prefab`（预扫：**仅 LinEn 链**显式挂 `OnDialogueEnd`/`OnCameraMoveEnd` 相机收束） |
| C. 进场即锁镜、未播完 | `ForestSceneManager.OnEnterScene` | `homeDoorStoryComplete==false` 时 `CancelFollow` + `SetLock(true)` |

**复现最短路径（建议）**：新档/清 `ForestSceneData.homeDoorStoryComplete` → 进 `ForestScene` → 走完门口演出至 **林恩对白结束 / CameraMoveEnd 前后** → 看是否位置错+闪。  
若用户指的是别的演出，报告开头改写「实际复现剧情名」。

### 现网相机契约（Forest 门口）

```
进场（homeDoor 未完成）
  ForestSceneManager.OnEnterScene
    → CameraComponentGSM.CancelFollow()
    → SetLock(true)                         ← 演出前锁死、不跟玩家

演出中
  NodeCanvas 可能 CameraMove / 跟 NPC / 锁机位（侦探扫 LaiFly + LinEn 图）

林恩对白结束（高度可疑收束点）
  ForestSceneLinEnStory.OnDialogueEnd
    → SetLock(false)
    → SetFollow(player, onComplete: OnCameraMoveEnd, forceSnapToTarget: true)
    → homeDoorStoryComplete = true
    → （BGM/SFX 显隐）
  → SetFollow.onComplete / 2s 兜底 → OnCameraMoveEnd
    → 发 AnimationEvent「CameraMoveEnd」给 NodeCanvas 等待节点
    → 可能再次 SetFollow(forceSnap)（有 _skipPlayerResnap 防二次 SmoothDamp）

之后
  NodeCanvas 在 CameraMoveEnd 后 Trigger 下一段（如 YaerAfterLinEn）
```

**规范**：`02_SYSTEM_SPEC.md` §3 — 跟拍走 `SetFollow`，禁固定 Wait 硬匹配镜头；收束应用 `onComplete`。

### 场景磁盘预扫（须 YAML 复核）

| 项 | 预扫值 | 含义 |
|----|--------|------|
| `ForestScene` `CameraComponent.smoothTime` | **0.3** | `forceSnap` 时走手推 SmoothDamp，**多帧可见位移** |
| Framing Transposer `m_XDamping` | **0.7** | Follow 绑上后仍可能横向追 |
| 龙宫 Stairs 对照（0912 已结） | 同源：`SetFollow` 手推 + 过早露景 → 闪/滑 | **可对照方案，勿直接改龙宫** |

### 嫌疑优先级（可推翻）

| # | 嫌疑 | 更像解释 |
|---|------|----------|
| **A** | 演出结束 `SetFollow(forceSnap)` + `smoothTime=0.3`：机位从演出停点手推到玩家，**过程露在画面上** | 位置「不对→再挪」+ 屏闪 |
| **B** | 对白 UI / 黑幕 / Fighting 立绘与 `OnStoryEnd` 同帧抢显（`StoryComponentGSM` 注释提过闪屏） | 偏 UI 闪，机位也可能错 |
| **C** | `OnCameraMoveEnd` 二次 `forceSnap` 或 2s 兜底提前触发，镜头跳两次 | 顿挫/双闪 |
| **D** | `SetLock` 仍为 true 时 SetFollow 被吞，兜底乱收；或 Follow 仍指 NPC（Hierarchy 里 NPC 灰掉） | 机位钉在错误点 |
| **E** | CameraArea Confiner 把瞬切机位夹到边界外再弹回 | 闪一下 |

### 侦探须回答的核心问题

1. **复现钉死**：哪条 Story Prefab、哪一帧「演出结束」（LinEn `OnDialogueEnd`？整段 homeDoor？其它）？  
2. **结束时相机链**：谁 `CancelFollow` / `SetFollow` / `SetLock` / CameraMove？`smoothTime`、Damping、是否黑幕掩护？  
3. **错误机位从哪来**：演出末 VCam 世界坐标 vs 玩家坐标 vs Confiner；闪的是手推过程还是 UI/黑幕？  
4. **与 0912 龙宫 Stairs 是否同源**：都是 `smoothTime` 手推未收束就露景？差异在哪（有无黑幕、有无 onComplete）？  
5. **修复方案（≥2，只方案）**：例如演出结束瞬切 `smoothTime=0` / 当帧对齐、收束完成前保持黑幕、仅 LinEn 路径零平滑、修二次 snap；列利弊与是否符合 SPEC §3。

### 必读 / 扫描范围

**必读**

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md` §3 摄像机  
3. `ForestSceneManager.cs`（进场锁镜）  
4. `ForestSceneLinEnStory.cs`（对白结束回跟）  
5. `CameraComponent.cs` / `CameraComponentGSM.cs`（`SetFollow` / `smoothTime`）  
6. 对照：`Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md`（同源嫌疑，勿混场景施工）  
7. 本提示词预梳理  

**扫描**

- `ForestScene.unity`：`Camera` / `CameraComponent` / VCam / CameraArea  
- `HomeDoorStoryTriggerLogic.cs`、`ForestSceneLaiFlyStory.prefab`、`ForestSceneLinEnStory.prefab`、`ForestSceneYaerAfterLinEnStory.prefab`  
- NodeCanvas：`CameraFollowPlayerActionTask`、`CameraMoveTaskAction`、等待 `CameraMoveEnd`  
- `StoryComponentGSM.OnStoryEnd`（立绘延迟与闪屏注释）  
- Console 已有 `[CHAIN]` / `[ForestSceneLinEnStory]` Debug（可开 `debugLogCameraMoveEndFlow`）

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity 2020.3.48f1 / C#。只读；**禁止**改代码、Prefab、场景、Git。

### 输出

写到：`Assets/Doc/执行文档/0912/ForestScene_演出结束相机位置与屏闪_架构溯源报告.md`

固定结构：

1. **结论一句话**（哪段演出 + 位置错/屏闪的最可能根因；是否同源）  
2. **调用链**（触发 → 演出中机位 → 结束收束 → 跟玩家；标出「错误机位可能露出」的窗口）  
3. **证据表**（场景字段、代码行、时序；已证实 / 可疑 / 已排除）  
4. **与龙宫 Stairs 案对照**（同/不同，避免误搬施工）  
5. **修复方案对比**（≥2）+ 推荐 + 验收标准  
6. 设计不清记入 `Assets/Doc/OPEN_QUESTIONS.md`

### 限制

- 不改代码；不提交 Git  
- 不扩大到 ForestEast / 村庄双 VCam，除非证明共用同一全局字段且本案必须动  
- 默认中文；大白话 + 路径/字段名  

---

## 【施工员】Prompt（侦探报告拍板后再复制；现在不要执行）

> **前置**：溯源报告已确认是哪段演出、根因与推荐方案。  
> **目标**：`ForestScene` 该演出结束后机位正确，无屏闪/乱滑。  
> **优先**：保持 NodeCanvas 驱动演出；收束仍走 `SetFollow` + `onComplete`（符合 SPEC §3）；最小化改 `ForestSceneLinEnStory` / 本场景 `CameraComponent.smoothTime` / 黑幕时序之一。  
> **禁止**：在 Update 堆相机逻辑；重写整套 Cinemachine；顺手改龙宫/村庄相机默认值（除非报告证明必须共享修复且已批准）。  
> **文档**：`Assets/Doc/施工说明/0912/ForestScene_演出结束相机消穿帮_施工说明.md`  
> **验收**：门口演出（或报告钉死的那条）结束 ≥2 次；揭幕/还控时无错误机位闪帧；日常跟拍不回归；Console `[CHAIN]` 显示 OnCameraMoveEnd 正常一次触发。  
