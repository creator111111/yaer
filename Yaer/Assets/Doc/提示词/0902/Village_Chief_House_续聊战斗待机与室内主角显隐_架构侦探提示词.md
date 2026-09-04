# Cursor Agent Prompt · 进屋续聊：藏室内主角 → 古莎旁战斗待机 → 结束黑幕同藏并恢复室内主角

> **角色**：先【架构侦探】只读定方案与资源身份，再【施工员】最小落地  
> **日期**：2026-09-02  
> **场景 / 对白**：`Village_Chief_House` · 自动 **`Village_村长家继续对话`**  
> **产品期望（钉死）**：  
> 1. **对话开始前/开场**：隐藏主角**室内形象**（Home）  
> 2. 在合层 **`古莎待机` 身边**显示一个**战斗形态待机**（Combat 观感）  
> 3. **对话结束** → 系统 **黑屏淡入淡出**  
> 4. 黑幕内把 **战斗形态待机** 与 **`古莎待机` 一起隐藏**  
> 5. 再**恢复显示室内主角形态**；玩家可继续室内操控  
> **对标样板**：村外 `GushaSidePortrait`（预置场景涂层 + 黑幕内显隐）；续聊结束换人已有 `Village_Chief_HouseSceneManager` BlackPanel 链  
> **上游须和解**：0901「续聊结束关古莎待机 → 开古莎动画合层」——本期要在同一次结束黑幕里并入主角显隐；**是否仍开 `古莎动画合层` 必须问清 / 写 OPEN**（用户本条未点名动画合层，默认倾向**保留**开动画合层，与「关待机」不矛盾）  
> **不是**：门口初次对话也换战斗待机（除非报告证明同壳）；不是改 UI 大立绘；不是 Loading 读条；不是把室内改成 `isFightingScene=true` 整场战斗  
> **报告落盘**：`Assets/Doc/执行文档/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_架构溯源报告.md`  
> **施工落盘**：`Assets/Doc/施工说明/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_施工说明.md`

把「侦探」段先复制给 Agent；拍板后用文末「施工」段。

---

## 提示词助手预梳理（须再证，勿当唯一真相）

### 产品时序（期望）

```
进屋 →（既有）准备播 Village_村长家继续对话
  →【开场】隐藏玩家室内 Home 形象
  →【开场】显示「战斗形态待机」站在 古莎待机 旁
  → 续聊播完（立绘/框分层另案）
  →【结束】BlackPanel 淡入 → 全黑
       → 关：战斗形态待机 + 古莎待机（一起）
       → （默认仍）开：古莎动画合层（0901；若产品否决则 OPEN 改口）
       → 恢复：玩家室内 Home 形象可见
  → BlackPanel 淡出 → 还控
```

| 阶段 | 玩家室内 Home | 战斗形态待机 | 古莎待机 | 古莎动画合层（0901） |
|------|---------------|--------------|----------|---------------------|
| 续聊进行中 | ❌ 隐藏 | ✅ 显示（在待机旁） | ✅ 仍显示（对话氛围） | ❌ 仍关 |
| 结束全黑后 | ✅ 恢复 | ❌ 隐藏 | ❌ 隐藏 | ✅ 打开（默认） |
| 读档已播完续聊 | ✅ 室内 | ❌ | ❌ | ✅（既有静默） |

### 「战斗形态待机」身份假说（侦探必须钉死一种）

| ID | 假说 | 利弊 | 倾向 |
|----|------|------|------|
| **A · 场景预置涂层** | 合层内预置「雅儿战斗待机」GO（SR / 合层 Prefab，类 `GushaSidePortrait` / `古莎待机`），默认关；续聊开时 Active，结束关 | 不碰 Home/Combat 双轨；脚位好摆 | ✅ **推荐默认** |
| **B · 真玩家切 Combat** | `PlayerLogic` 换 Combat 控制器 + 挪到古莎旁 Idle；结束切回 Home | 易踩 `02_SYSTEM_SPEC` Home/Walk vs Combat/Run；室内 `isFightingScene=0` | ⚠️ 次选；须强回归 |
| **C · 克隆实体** | Instantiate 战斗 Avatar，藏真玩家 | 多一套实体生命周期 | ⚠️ 除非 A 无素材 |

**资源排查**：`ArtRes` 是否已有战斗服待机合层 / 帧（OPEN 曾提「战斗服待机 拷贝」）；无则 OPEN 记美术依赖，勿用 UI `GoOutStoryYaerPainting` 冒充场景待机。

### 与 0901 结束换古莎链的合并

现网（助手预扫）：`Village_Chief_HouseSceneManager` 订续聊 `onStoryEnd` → BlackPanel → 关 `古莎待机` → 开 `古莎动画合层`。

| 本期增量 | 挂哪 |
|----------|------|
| 开场藏 Home + 亮战斗待机 | 续聊 **Trigger 成功 / 壳就绪全黑内**（对齐侧面涂层「全黑后启用」） |
| 结束关战斗待机 | **同一** `onStoryEnd` 全黑回调，与关 `古莎待机` **同帧/同回调** |
| 结束恢复 Home | 同上全黑内，**先于或紧随**淡出前完成 |
| 开 `古莎动画合层` | **默认保留**；若用户只要「关待机+恢复主角」、不要正面动画 → 记 OPEN 改口 |

**禁止**：结束开两次 BlackPanel（一次换古莎、一次换主角）造成双黑闪。

### 开场藏主角时机

| 方案 | 做法 | 倾向 |
|------|------|------|
| **S1 · 续聊壳就绪、黑幕未揭前** | 全黑内：藏 Home、亮战斗待机，再揭黑 / 播前奏 | ✅ 无穿帮 |
| S2 · 对话 Prefab 首节点 Action | 图内藏玩家 | ⚠️ 揭黑后才跑易闪室内身 |
| S3 · OnEnterScene 一进房就藏 | 未播续聊也藏 | ❌ 手动再进房无续聊会丢主角 |

### 关键风险

| 风险 | 说明 |
|------|------|
| Home/Combat 抢参 | 若走 B：禁止 Town 写 `Run`；须完整切回 Home |
| 双雅儿 | 未藏 Home 就亮战斗涂层 |
| 结束只关古莎忘关战斗待机 | 黑幕后留战斗壳 |
| 读档 | 续聊已用：应已是「室内主角 + 动画合层、无战斗待机、无古莎待机」；静默 Apply，勿再黑幕 |
| 操控 | 续聊中本应锁操作；恢复 Home 后还控 |
| 落点 | 藏/显不要把脚吸到楼梯（0901 EnterPos 案） |

### 方案倾向（施工默认）

| 步骤 | 默认 |
|------|------|
| 战斗待机载体 | **A 预置合层涂层**（命名建议 `YaerCombatStandby` / 中文「雅儿战斗待机」，侦探按磁盘现名） |
| 开场 | **S1** 全黑内藏玩家 Renderer/根显隐（或官方已有 Hide API）+ Active 战斗待机于 `古莎待机` 旁 |
| 结束 | **扩展**现有 Chief GSM BlackPanel 回调：关战斗待机 + 关古莎待机 +（默认）开动画合层 + 恢复玩家室内显隐；**一次黑幕** |
| 否 | 整场 `isFightingScene=true`；LoadingPanel；改门口戏 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 续聊开/毕主角与战斗待机显隐 | ❌ 门口初次对话同套（除非产品扩） |
| ✅ 与 0901 古莎换人同黑幕合并 | ❌ 拆成两次 BlackPanel |
| ✅ 脚位/Sorting 可验 | ❌ 改续聊 CSV / 三人 UI 立绘摆位主案 |
| ✅ 读档静默正确态 | ❌ 用大立绘 Prefab 当场景待机 |

### 严禁

- 亮战斗待机时室内主角仍可见（双影）  
- 结束只恢复主角却留下战斗待机或双古莎  
- 为切形态把村长家改成战斗场景配置  
- 把村外 `GushaSidePortrait` 逻辑整段硬套却不核对室内 Sorting/Z  
- 结束黑幕里不关 `古莎待机`（与产品「一起隐藏」冲突）  

### 对照文档 / 代码

- `Village_Chief_HouseSceneManager.cs`（续聊 Trigger、`onStoryEnd`、古莎换人）  
- `ChiefNearDoorStoryTrigger.cs`（`GushaSidePortrait` 全黑启用样板）  
- `PlayerLogic.UpdateRuntimeController` / Home vs Combat（若走 B）  
- `02_SYSTEM_SPEC.md` §4 双轨  
- `执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md`  
- `执行文档/0901/Village_Chief_House_续聊结束正面古莎未出现_验收排查报告.md`（场景拆包合层教训）  
- 合层：`ArtRes/Scene/Village/Prefab/村长家合层.prefab` + 场景 `Design/村长家合层` 实例是否拆包  

---

## 侦探 Prompt（复制给 Agent · 先跑）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码/场景/Prefab。只读扫描 + 写溯源报告。
默认中文。沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/提示词/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_架构侦探提示词.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md

## 产品
Village_村长家继续对话：
开场隐藏室内主角，在古莎待机旁显示战斗形态待机；
结束 BlackPanel 内把战斗待机与古莎待机一起隐藏，再恢复室内主角。
须与 0901 古莎动画合层换人链和解（默认仍开动画合层，一次黑幕）。

## 必读
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/ChiefNearDoorStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
（显隐 / UpdateRuntimeController / Home·Combat）
检索：古莎待机、古莎动画合层、GushaSidePortrait、SetActive、战斗服待机、isFightingScene、onStoryEnd、TryTriggerChiefContinue。
场景/合层：Village_Chief_House · Design/村长家合层；Prefab/村长家合层.prefab。
美术：是否已有雅儿战斗待机合层/帧可预置。

## 任务
1. 钉死「战斗形态待机」用 A/B/C 哪种；列出可用资源路径或缺口。
2. 画出开场/结束全时序（含现有续聊黑幕盖、古莎换人）；标藏主角与亮待机的写入点。
3. 查玩家室内形象如何安全隐藏/恢复（整根 / SpriteRenderer / 官方 API）；勿破坏还控与落点。
4. 与 0901 结束回调合并方案；确认是否仍开古莎动画合层 → 写入 OPEN。
5. 读档/再进房静默态表；场景合层是否拆包（正面古莎案教训）。
6. 最小施工清单 + Setup 菜单是否要扩。

## 报告
Assets/Doc/执行文档/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_架构溯源报告.md
同步 OPEN_QUESTIONS.md。
```

---

## 施工 Prompt（根因拍板后复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_架构溯源报告.md
@Assets/Doc/提示词/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_架构侦探提示词.md
@Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md

## 目标
续聊开场：室内主角隐藏 + 古莎待机旁战斗形态待机可见。
续聊结束：一次 BlackPanel 内关闭战斗待机与古莎待机，恢复室内主角；（默认）仍切换古莎动画合层。无双影、无双黑幕。

## 默认施工方向（若报告未改口）
1. 载体默认 A：合层预置战斗待机，脚位旁古莎待机，默认关；场景实例与 Prefab 资产同步（防拆包漏预置）。
2. 开场 S1：续聊全黑/壳就绪回调内藏玩家室内形象 + Active 战斗待机。
3. 结束：扩展现有 onStoryEnd 黑幕回调，同一次全黑内完成显隐；禁止第二次 BlackPanel。
4. 读档已换：静默正确 Active，不再播黑幕换装。
5. 详细注释写原因；复杂逻辑注明 A vs B 替代；同步 OPEN。
6. 若缺美术资源：先记 OPEN，勿用 UI 立绘冒充。

## 约束
- 禁止把村长家 Config 改成 isFightingScene=true 当主修
- 禁止 LoadingPanel；禁止门口初次对话误伤（除非报告扩 scope）
- 禁止结束只关古莎不关战斗待机，或只恢复主角仍留战斗壳
- 若走 B 切真玩家 Combat：必须完整切回 Home，禁止 Town 抢写 Run
- 回归：续聊门闩、针线包 Tips、落点、古莎动画合层正面可见、室内走路

## 落盘
Assets/Doc/施工说明/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_施工说明.md

## 验收
- [ ] 续聊开场：看不见室内主角；古莎待机旁可见战斗形态待机
- [ ] 续聊中：无双雅儿；操作仍锁在对话
- [ ] 结束后一次黑幕：战斗待机与古莎待机皆关；室内主角恢复可见可走
- [ ] （默认）古莎动画合层在村长旁可见；无双古莎；无动画「背景」盖房
- [ ] 同档再进：静默正确态，不重复换装黑幕
- [ ] Console 无空引用；EnterPos / 楼梯不回归
```

---

## 给开发者（一句话）

续聊期间用**场景战斗待机涂层**（优先）站在古莎旁并藏室内主角；结束时在**现有换古莎那一次黑幕**里把战斗待机和古莎待机一起关掉，再亮回室内主角——先跑侦探钉资源与是否仍开「古莎动画合层」。
