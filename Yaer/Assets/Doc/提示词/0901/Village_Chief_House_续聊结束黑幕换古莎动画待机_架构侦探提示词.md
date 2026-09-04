# Cursor Agent Prompt · 村长家续聊结束 → 黑幕 → 关「古莎待机」换「古莎动画合层」旁站

> **角色**：先【架构侦探】只读定方案，报告后再【施工员】  
> **日期**：2026-09-01  
> **场景**：`Assets/GameRes/Scenes/Village_Chief_House.unity`  
> **Hierarchy 锚点（用户截图）**：`Map → Design → 村长家合层 →` **`古莎待机`**（红箭头；旁近 **`村长`**）  
> **产品设定（钉死）**：  
> 1. **室内对话**（`Village_村长家继续对话`）**结束之后**  
> 2. 接一次 **黑屏淡入淡出**（系统 BlackPanel，非 Loading）  
> 3. 黑幕内/全黑时：**关闭** 现有 **`古莎待机`**  
> 4. **新增/启用** 古莎动画待机，站在 **村长旁边**；资源钉死  
>    `@Assets/ArtRes/Animation/古莎动画合层.prefab`  
> **不是**：对话进行中途换人；不是 UI `GushaPainting`；不是村外 `GushaSidePortrait` 侧面涂层那条  
> **本阶段（侦探）**：只读；禁止改场景 / 代码 / Prefab  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望）

```
Village_Chief_House
  →（既有）OnEnterScene 自动 TriggerStory("Village_村长家继续对话")
  → 续聊播完（含针线包等中途逻辑）
  → onStoryEnd
  →【本需求】BlackPanel ShowFade（全黑）
      → 关 古莎待机（合层内现网静态待机）
      → 开/摆好 古莎动画合层（站村长旁）
  → HideFade
  → 可见：动画合层古莎站在村长旁边；旧「古莎待机」不可见
  → 还控（存档记「已换过」倾向单次）
```

**禁止**理解成：无黑幕硬切闪一下；或只 Instantiate 不关旧待机导致双古莎。

### 场景锚点（用户截图）

| 物体 | 角色假说 |
|------|----------|
| **`古莎待机`** | 合层内 **现网静态古莎**；续聊结束前可见；结束后 **SetActive(false)** |
| **`村长`** | 合层美术村长；新古莎对齐其身旁脚位 |
| **`村长家合层`** | 父 Prefab：`ArtRes/Scene/Village/村长家合层.prefab`（磁盘已有 `古莎待机`/`村长`） |
| **`古莎动画合层`** | 动画资源 Prefab；预扫含子：`背景` / `古莎正面`（图层）——**入场景时是否剥掉「背景」**须侦探裁定 |

### 资源预扫（须证伪）

| 项 | 现状 |
|----|------|
| 续聊结束挂点 | `Village_Chief_HouseSceneManager` 现只 `OnEnterScene` Trigger；**尚无** onStoryEnd 换装逻辑 |
| 黑幕 API | 对齐门口 `ChiefNearDoorStoryTrigger`：`ShowFade` / `HideFade`（`BlackPanel`） |
| `古莎待机` | 在 **`村长家合层.prefab`** 内（非仅场景 YAML 散落） |
| `古莎动画合层.prefab` | `ArtRes/Animation/`；预扫 **未见 Animator**（合层 SR 图层）——侦探核实是否还需挂 Animator/Clip，或「动画合层」= 分层静态/帧动画另有子资源 |
| 村外侧面 | `GushaSidePortrait`：**勿混**；本期是 **室内合层换人** |

### 挂点方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **G1 · Chief_House GSM 订续聊 onStoryEnd** | `TriggerStory` 成功后订阅 `onStoryEnd` → 黑幕换人 | ✅ 与进屋自动续聊同组件收束 |
| G2 · 对话图末 Action | Prefab 尾挂自定义 Action：开黑+换人 | 须扩 Action；跨场景物体引用难 |
| G3 · 仅场景 Trigger Collider | 对白完再走格子触发 | ❌ 产品要「对话结束自动」 |

黑幕内换人（对齐侧面涂层教训）：

```
onStoryEnd(ContinueStory):
  ShowFade
  onShowEnd(全黑):
    古莎待机.SetActive(false)
    古莎动画合层.SetActive(true)  // 或首次 Instantiate 后常驻引用
    // Sorting / 脚位已预摆好
  HideFade
  存档旗：已换过（同档不重切）
```

### 摆放方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **P1 · 场景/合层预置** | 在 `村长家合层`（或 Design 下）预摆 `古莎动画合层` 实例，默认 **关**；脚位对齐村长旁；运行时只 Active 切换 | ✅ 少运行时定位；排序可 Scene 调 |
| P2 · 运行时 Instantiate | onStoryEnd 生成并拷贝 `古莎待机` 坐标微调 | 灵活；销毁/重复进房要小心 |
| P3 · 改 `古莎待机` 换 Sprite | 不换 Prefab | ❌ 用户要新动画合层资源 |

**合层 Prefab 改动注意**：`村长家合层` 可能被多处引用；侦探写清改源 Prefab vs 场景 Override。

### 「背景」子物体（OPEN）

`古莎动画合层` 根下有 **`背景`** SR——室内已有房间底图时再亮一层会穿帮。

| 假说 | 倾向 |
|------|------|
| **入场景实例关掉/删掉「背景」子节点** | ✅ |
| 保留 | 仅当美术确认需要 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 续聊结束 → 黑幕 → 关 `古莎待机` → 开动画合层旁站村长 | ❌ 改续聊台本内容 |
| ✅ 系统 BlackPanel 一次淡入淡出 | ❌ LoadingPanel 当这次换人表现 |
| ✅ 同档单次（倾向） | ❌ 每次进房重复黑幕换人 |
| ✅ Sorting/脚位可测 | ❌ 村外侧面涂层逻辑搬进来硬套 |

### 严禁

- 亮屏下硬切双古莎闪现  
- 不关 `古莎待机` 只加新实例  
- 把 UI 大立绘 `GushaPainting` 当场景待机  
- Update 轮询换人；须事件驱动（onStoryEnd + Fade 回调）  
- 误绑晚宴台本结束  

### 开放（侦探须答）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 对话名是否钉死续聊？ | **`Village_村长家继续对话`** |
| Q2 | 动画合层要不要 Animator？ | 以 Prefab 实况为准；无则先 SR 合层待机，帧动画另案 |
| Q3 | 改合层源 Prefab 还是场景 Override？ | **预置进合层或 Design**；报告写清 |
| Q4 | 关「背景」子节点？ | **是** |
| Q5 | 读档进房：若续聊已播，直接显示动画合层、跳过黑幕？ | ✅ 倾向（旗已换则静默正确 Active） |
| Q6 | 雅儿玩家角色与合层古莎关系？ | 合层古莎是 **场景 NPC 涂层**；勿替玩家 Controllable |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
Village_Chief_House 内 Village_村长家继续对话 结束后：
黑屏淡入淡出 → 关闭合层「古莎待机」→ 启用/摆放
Assets/ArtRes/Animation/古莎动画合层.prefab
站在「村长」旁边。用户 Hierarchy 红箭头：古莎待机（旁近村长）。

## 必读
@Assets/GameRes/Scenes/Village_Chief_House.unity
@Assets/ArtRes/Scene/Village/村长家合层.prefab
@Assets/ArtRes/Scene/Village/Prefab/村长家合层.prefab
@Assets/ArtRes/Animation/古莎动画合层.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
@Assets/Doc/施工说明/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_施工说明.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/ChiefNearDoorStoryTrigger.cs
（ShowFade / onStoryEnd / 黑幕内改场景物样板）
@Assets/Doc/施工说明/0831/Village_KenMuNi1_靠近黑幕插入女二侧面涂层_施工说明.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Black/BlackFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/BlackFade/BlackFadeComponent.cs

检索：古莎待机、村长、村长家合层、古莎动画合层、ShowFade、HideFade、onStoryEnd、
Village_村长家继续对话、StoryTriggerCountData。

## 侦探任务
1. 定位「古莎待机」「村长」在合层 Prefab / 场景实例中的路径、坐标、Sorting。
2. 解剖「古莎动画合层」结构（背景/正面/是否有 Animator）；入场景是否剥背景。
3. 设计：续聊 onStoryEnd → 黑幕 → 关旧开新 → 淡出；挂点 G1/G2；单次与读档态。
4. 摆放 P1/P2；脚位相对村长；Sorting 与村长/玩家不穿帮。
5. 最小改动清单（GSM + 场景/合层预置 + 可选小组件）+ 验收 + OPEN。
6. 写清与村外 GushaSidePortrait、UI GushaPainting 的解耦。

## 报告落盘
Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md

结构：①结论 ②锚点 ③动画合层结构 ④时序与黑幕 ⑤挂点/摆放 ⑥读档
⑦最小清单 ⑧验收 ⑨OPEN ⑩程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md

## 目标
1. Village_村长家继续对话 结束后：BlackPanel 淡入淡出一次。
2. 全黑时关闭「古莎待机」，启用「古莎动画合层」站在村长旁
   （资源：Assets/ArtRes/Animation/古莎动画合层.prefab）。
3. 按报告处理「背景」子节点、Sorting、读档已换过则跳过黑幕直接正确显示。
4. 禁止亮屏硬切；禁止双古莎并存；禁止 Update 堆业务；禁止改村外侧面涂层逻辑当主修。

## 落盘
Assets/Doc/施工说明/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_施工说明.md
同步 OPEN_QUESTIONS。

## 验收
- [ ] 续聊完整结束后自动出黑幕再亮
- [ ] 亮后「古莎待机」关闭，「古莎动画合层」在村长旁可见
- [ ] 无双古莎；无房间底图被动画 Prefab「背景」盖住（若报告要求关背景）
- [ ] 同档再进房：不重复黑幕换人（或符合读档决议）
- [ ] 续聊/针线包 Tips 回归正常
- [ ] Console 无 BlackPanel / 空引用 Error

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先跑侦探** → 定挂点（倾向 GSM `onStoryEnd`）和动画合层怎么摆、要不要关「背景」。  
2. 再跑施工。  
3. Hierarchy 真源：**`村长家合层 / 古莎待机`**；新资源钉死 **`古莎动画合层.prefab`**。  
4. 上游：室内续聊须能播完（0901 进屋自动续聊已合入）。
