# Cursor Agent Prompt · Bug 排查：村长家续聊结束后「正面古莎 / 古莎动画合层」未出现

> **角色**：先【验收员 / 架构侦探】只读复现 + 定根因（可加短日志），报告后再【施工员】最小修复  
> **日期**：2026-09-01  
> **现象（用户测试）**：村长家对话（`Village_村长家继续对话`）结束后，**新的正面古莎没有出现**  
> **期望（0901 已施工）**：续聊结束 → BlackPanel → 关 **`古莎待机`** → 开 **`古莎动画合层`**（内含 **`古莎正面`**）站在村长旁  
> **施工依据**：  
> - `执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md`  
> - `施工说明/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_施工说明.md`  
> **本阶段**：以排查为主；禁止大范围重构；可加 `[ChiefGushaSwap]` 诊断日志  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束正面古莎未出现_验收排查报告.md`

把下面「排查」整段复制给 Cursor Agent（Agent Mode）执行。修复 Prompt 见文末。

---

## 提示词助手预梳理（排查须核实，勿当唯一真相）

### 期望时序（钉死）

```
OnEnterScene
  → ApplyGushaVisualFromArchive()
  → TryTriggerChiefContinueOnce() 成功 → += onStoryEnd
  → 续聊播完
  → OnChiefContinueStoryEnd
  → BlackPanel FadeShow
  → onShowEnd 全黑：待机关 + 动画合层开 + 记旗 Village_ChiefHouse_GushaAnimStandby
  → CloseFormFade
  → 可见「古莎动画合层 / 古莎正面」
```

### 关键假说（按优先级查）

| ID | 假说 | 预扫线索 | 怎么证伪 |
|----|------|----------|----------|
| **H1** | 续聊 **未** `TriggerStory` 成功 → **从未订阅** `onStoryEnd` → 换人链不跑 | `ShouldPlayChiefContinue` 要门口已用∧续聊未用；测档若续聊已用则进房不订约 | Console 有无 `[ChiefContinue] OnEnterScene TriggerStory`；有无订约 |
| **H2** | 续聊已用、换人旗未立：进房应走 **Q7 静默 Apply**，但 **找不到「古莎动画合层」** | 找名 `FindDeepInactive`；未跑 Setup / 改错合层源 | Console `[ChiefGushaSwap] 未找到「古莎动画合层」`；Hierarchy 有无实例 |
| **H3** | 黑幕 `onShowEnd` 未到 / BlackPanel 失败 → 未执行 `ApplyGushaVisual(true)` | `OpenSystemBlackFade` | 有无黑幕；有无 `[ChiefGushaSwap] 全黑内已切换` |
| **H4** | Active 已开但 **看不见**：Sorting/Z/脚位/子层「古莎正面」关、Scale、被房间层挡住 | 合层预置 Pos≈待机；子 SR order 9～11 | Hierarchy：根 Active、古莎正面 Active、Sprite 非空、相机能看到 |
| **H5** | `FindDeepInactive` 命中 **错误实例**（多合层 PrefabInstance / 未加载场景） | 场景多处 `村长家合层` | 日志打印找到的 instanceId / 路径 |
| **H6** | SerializeField 绑空 + 按名失败 | GSM Inspector 引用 | 查 SceneManager 组件槽 |
| **H7** | 用户测的是 **晚宴** 或其它对白结束，未订约 | 仅续聊 Trigger 成功才 `+=` | 对白名是否 `Village_村长家继续对话` |
| **H8** | 改错合层源（`Village/村长家合层` vs **`Prefab/村长家合层`**） | 场景真源 guid `5cad…` | 场景 PrefabInstance guid |

### 磁盘预扫（助手 · 须再证）

| 项 | 预扫 |
|----|------|
| GSM 换人代码 | ✅ `Village_Chief_HouseSceneManager` 已有订约 / 黑幕 / Apply |
| 合层预置 | ✅ `Prefab/村长家合层.prefab` 内嵌 `古莎动画合层` guid `4271a266…`，**默认 `m_IsActive: 0`**，脚位≈待机，背景子关 |
| Setup 菜单 | ✅ `Tools / Scene / Setup Chief House 古莎动画合层预置` |
| 动画表现 | **无 Animator**；靠多层 SR「古莎正面」——「没出现」可能是 **未 Active**，也可能是 **Active 但看不见** |

### 复现档注意（极易踩 H1）

| 测档状态 | 行为 |
|----------|------|
| 门口未播 | 不进续聊 → 无换人 |
| 门口已播、续聊未播 | 应 Trigger + 订约 + 结束换人 |
| 续聊已播、换人旗未立 | **不订约**；应 **进房静默开动画**（Q7）——若仍无，偏 H2/H4/H5 |
| 换人旗已立 | 进房静默动画；不应再黑幕 |

排查须记录：存档里 `Village_村长家门口初次对话` / `Village_村长家继续对话` / `Village_ChiefHouse_GushaAnimStandby` 是否已用。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 复现 + 假说表裁定根因 | ❌ 重做整段续聊 |
| ✅ 可加短 `[ChiefGushaSwap]` / `[ChiefContinue]` 日志 | ❌ 改村外侧面 / UI 立绘 |
| ✅ 最小修复（订约时机 / Find / Active / Sorting） | ❌ 改 WalkArea2 / 出屋送树屋另案 |

### 严禁

- 未看 Console / Hierarchy 就改脚位猜修  
- 把 UI `GushaPainting` 当场景正面古莎  
- 亮屏硬切当「修复」却不查为何 onStoryEnd 未到  

---

## 排查 Prompt（复制给 Agent）

```text
你是【验收员 + 架构侦探】。Unity 2020.3.48f1 / C#。
默认只读排查；允许添加可开关的短诊断日志（标签 [ChiefGushaSwap]/[ChiefContinue]）。
禁止大范围重构。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md
@Assets/Doc/施工说明/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_施工说明.md

## 现象
测试：村长家对话结束后，新的正面古莎（古莎动画合层 / 古莎正面）没有出现。
期望：关「古莎待机」，开「古莎动画合层」。

## 必读代码 / 资源
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
@Assets/Editor/Tool/Scene/ChiefHouseGushaAnimStandbySetupEditor.cs
@Assets/ArtRes/Scene/Village/Prefab/村长家合层.prefab
@Assets/ArtRes/Animation/古莎动画合层.prefab
@Assets/GameRes/Scenes/Village_Chief_House.unity
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/ChiefNearDoorStoryTrigger.cs
（黑幕样板）

检索：ChiefGushaSwap、古莎动画合层、古莎待机、GushaAnimStandby、
onStoryEnd、ApplyGushaVisual、FindDeepInactive、4271a266。

## 排查任务
1. 写清复现步骤（含：应用哪份存档、是否首次续聊）。
2. 按 H1～H8 逐条证伪，填「成立/否/证据」。
3. 必查 Console：是否有 TriggerStory、未找到动画合层、全黑内已切换、BlackPanel Error。
4. 必查 Hierarchy（续聊结束后 Pause）：古莎待机 Active？古莎动画合层 Active？古莎正面及子 SR Sprite？世界坐标是否在镜头内？
5. 核实场景合层实例是否来自 Prefab/村长家合层（guid 5cad…），动画实例是否在。
6. 裁定唯一主因 + 最小修复建议（订约是否应在「续聊已用但未换人」时补一次黑幕/静默 Apply 已足够？）。
7. 若需日志：列出建议插点，勿刷屏。

## 报告落盘
Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束正面古莎未出现_验收排查报告.md

结构：①结论一句话 ②复现 ③假说表 ④证据（日志/Hierarchy）⑤主因 ⑥最小修复清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 修复 Prompt（根因拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束正面古莎未出现_验收排查报告.md
@Assets/Doc/施工说明/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_施工说明.md

## 目标
按排查报告主因修复：续聊结束后正面古莎（古莎动画合层）可靠出现；
关古莎待机；黑幕时序符合原设计（或报告改口的静默路径）。

## 约束
- 禁止改 WalkArea2 / 出屋送树屋 / UI GushaPainting
- 合层真源：Prefab/村长家合层；勿改错另一份合层
- 保留 [ChiefGushaSwap] 关键日志直至验收通过（可开关）

## 落盘
Assets/Doc/施工说明/0901/Village_Chief_House_续聊结束正面古莎未出现_修复施工说明.md
同步 OPEN_QUESTIONS。

## 验收
- [ ] 首次续聊播完 → 黑幕 → 待机关、动画合层（正面）可见在村长旁
- [ ] Console 有全黑切换成功日志；无「未找到古莎动画合层」
- [ ] 同档再进：静默显示动画合层，不双古莎
- [ ] 续聊已用、换人未成功的旧档：进房也能看到正面古莎（若报告要求修 Q7）
- [ ] 针线包 Tips / 续聊台本回归

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者（先自查 30 秒）

1. 过滤 Console：`ChiefContinue` / `ChiefGushaSwap`。  
2. 续聊结束后看 Hierarchy：有没有 **`古莎动画合层`**，是不是还是灰的。  
3. 若从未跑过：`Tools / Scene / Setup Chief House 古莎动画合层预置`。  
4. 再用 **续聊未播过** 的档完整走一遍（续聊已播完的旧档不会再订 `onStoryEnd`，只能靠进房静默 Apply）。
