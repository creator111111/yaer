# Cursor Agent Prompt · 进屋续聊开场：对齐门口初次对话的黑屏→立绘→对话框分层淡入

> **角色**：先【架构侦探】只读钉门口 vs 续聊时序差，再【施工员】最小对齐  
> **日期**：2026-09-02  
> **对白**：进屋后自动播的 **`Village_村长家继续对话`**  
> **对标真理源**：**`Village_村长家门口初次对话`**（玩家体感：黑屏 → 立绘淡入 → 对话框淡入 → 再说话；结束侧若有淡出也对齐）  
> **现象（用户）**：进村长家后的对话是**直接蹦出来**，没有「黑屏 / 立绘 / 对话框」依次淡入淡出  
> **产品期望（钉死）**：进屋续聊开场观感 **与门口初次对话保持一致**  
> **不是**：改台本文案；不是改三人摆位定稿；不是取消自动播续聊；不是再开蛋糕 LoadingPanel  
> **报告落盘**：`Assets/Doc/执行文档/0902/Village_村长家继续对话_开场分层淡入对齐门口_架构溯源报告.md`  
> **施工落盘**：`Assets/Doc/施工说明/0902/Village_村长家继续对话_开场分层淡入对齐门口_施工说明.md`

把「侦探」段先复制给 Agent；拍板后用文末「施工」段。

---

## 提示词助手预梳理（须再证，勿当唯一真相）

### 产品期望时序（对齐门口体感）

```
（可有系统黑幕压住）
  → 黑屏阶段：壳/立绘已准备但玩家看不见穿帮
  → 黑幕淡出 或 黑幕后
  →【立绘】三人 CanvasGroup 淡入（雅 / 古 / 村长）
  →【对话框】字幕条 UIAlpha 淡入（空字；小头像按门口现网决议——框出勿预亮，首句再出）
  → 首句 / 后续正常播
  →（若门口有结束淡出）续聊结束也对齐淡出；侦探须对照门口是否真有「出」再定范围
```

| 阶段 | 门口初次（期望体感） | 进屋续聊（用户现状） |
|------|----------------------|----------------------|
| 黑屏 | ✅ 靠近后 BlackPanel 压住再播 | ⚠ 换场黑幕/Loading 一揭 → 对白已齐？ |
| 立绘淡入 | ✅ 前奏三路 CanvasGroupAlpha | ❌ 像直接出现 |
| 对话框淡入 | ✅ UIAlpha | ❌ 像直接出现 |
| 依次感 | ✅ 有层次 | ❌ 无 |

### 门口现网（已知样板 · 侦探须画全链）

```
Npc_Chief Enter
  → BlackPanel ShowFade → 全黑
  → TriggerStory("Village_村长家门口初次对话")
  → 壳就绪 →（短 hold）HideFade
  → Prefab 前奏：三路 CanvasGroupAlpha(立绘) 并行 0→1
  → NormalDialogueUIAlpha（对话框 FadeIn；PrepareMask 门口已定关预亮见 0902）
  → Statement 首句…
```

关键脚本：`ChiefNearDoorStoryTrigger.cs`（Show → Trigger → 壳就绪 HideFade）。

### 进屋续聊现网（助手预扫）

```
门口戏结束 → 切场进 Village_Chief_House（黑幕或曾 Loading）
  → Village_Chief_HouseSceneManager.OnEnterScene
  → TryTriggerChiefContinueOnce()
  → TriggerStory("Village_村长家继续对话")
  → Prefab 图内似有三路淡入 + UIAlpha（磁盘预扫两边都有 CanvasGroupAlpha）
```

| 可疑差 | 说明 |
|--------|------|
| **H1 · 淡入在遮罩下跑完** | 续聊在换场黑幕/Loading 仍盖住时 Trigger；Fade 在幕后播完，揭开时 alpha 已是 1 → **体感硬切** |
| **H2 · StartAlpha 空=当前 1** | Prefab 立绘/框默认 alpha=1，`StartAlpha` 未显式 0 → DOFade(1→1) 无动画 |
| **H3 · 图连接/阻塞不同** | 续聊 Setup 从门口拷壳但节点序、`EndActionOnAnimationEnd`、串并行与门口不一致 |
| **H4 · 缺「壳就绪再揭黑」成对** | 门口有 Trigger 专属 BlackPanel 门控；续聊只靠换场黑幕，揭开时机与前奏不同步 |
| **H5 · 换场改黑幕后更糟/更好** | 0902「门口进屋改黑屏」若已施工，须重测续聊开场；可能需 **进屋后再垫一层对话专用黑幕**（对齐门口，非 Loading） |

### 方案倾向（施工默认，侦探拍板）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **T1 · 续聊开场补「对话专用 BlackPanel」成对** | 进屋后：ShowFade 全黑 → Trigger 续聊 → 壳就绪且前奏节点就位 → HideFade；让玩家看见立绘/框淡入 | ✅ 最对齐门口触发器精神 |
| **T2 · 等换场黑幕淡出后再 Trigger + Prefab 保证 StartAlpha=0** | 不二次黑幕；揭开后再播前奏，玩家能看见淡入 | ✅ 若换场黑幕时序可控；须防露景一帧 |
| **T3 · 只改 Prefab：显式 StartAlpha=0、串行立绘→框、加长 Duration** | 不改 GSM | ⚠️ 若 H1 成立（幕下播完）仍看不见 |
| T4 · 整 Prefab 覆盖成门口再 Import | 易丢针线包 Tips 等续聊专有节点 | ❌（0901 已否） |
| T5 · 用 LoadingPanel 拖时间给淡入 | 产品刚否读条 | ❌ |

**推荐组合**：先证 H1/H2 → 多半要 **T1 或 T2（揭开后再淡）+ Prefab StartAlpha=0 与门口节点序对齐**。  
**禁止**为分层重新开蛋糕读条。

### 与相关案边界

| 案 | 关系 |
|----|------|
| 0901 摆位对齐门口 | **布局已齐**；本期专攻 **开场时序/淡入**，勿回潮改 Pos |
| 0902 门口框出空头像 | 门口 `PrepareMaskAvatarOnFadeIn=false`；续聊对齐时 **勿重新打开预亮** |
| 0902 进屋改黑屏切场 | 换场皮换了；续聊开场分层须在新换场上重验 |
| 续聊结束换古莎待机 | **结束**黑幕另案；本期主攻 **开场**；结束淡出仅在门口确有「出」且产品要一致时才扩 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 钉门口 vs 续聊全链时序差 | ❌ 改 CSV 台词 / Face |
| ✅ 让续聊开场有黑屏+立绘+框依次淡入感 | ❌ 取消自动续聊 / 改门闩 |
| ✅ Prefab 与/或 GSM/Trigger 最小改 | ❌ 整壳覆盖丢掉 GetItem/Tips |
| ✅ 回归门口初次对话不被改坏 | ❌ 蛋糕 LoadingPanel |

### 严禁

- 用「立绘默认 alpha=1 硬切」冒充淡入  
- 幕下播完 Fade 再揭开还说「已经有淡入」  
- 为对齐时序重开 `LoadSceneWithLoadingPanel`  
- 改门口 Prefab 去迁就续聊（真理源是门口体感，续聊向门口靠）  
- 广扫 Prepare 误伤 Mask（KenMuNi 教训）  

### 对照文档 / 资源

- `Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab`（真理）  
- `Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab`（被改方）  
- `ChiefNearDoorStoryTrigger.cs`（门口黑幕成对样板）  
- `Village_Chief_HouseSceneManager.cs`（续聊 Trigger 挂点）  
- `NormalDialogueUIAlphaAnimationTaskAction.cs` / `CanvasGroupAlphaActionTask.cs`  
- `技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`  
- `执行文档/0831/...靠近村长黑幕播门口初次对话_架构溯源报告.md`  
- `执行文档/0901/...三人大立绘摆位对齐门口_架构溯源报告.md`  
- `执行文档/0902/...框出时空头像_架构溯源报告.md`  
- Setup：`VillageChiefDoorDialogueSetupEditor` / `VillageChiefContinueDialogueSetupEditor`  

---

## 侦探 Prompt（复制给 Agent · 先跑）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码/Prefab/场景/CSV。只读扫描 + 写溯源报告。
默认中文。沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/提示词/0902/Village_村长家继续对话_开场分层淡入对齐门口_架构侦探提示词.md
@Assets/Doc/OPEN_QUESTIONS.md

## 产品
进入村长家后的 Village_村长家继续对话 开场是硬切出现；
须与 Village_村长家门口初次对话一致：黑屏 → 立绘淡入 → 对话框淡入（依次），需要时含淡出。

## 必读
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/ChiefNearDoorStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
@Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/CanvasGroupAlphaActionTask.cs
检索：TryTriggerChiefContinueOnce、CanvasGroupAlpha、UIAlpha、StartAlpha、HideFade、壳就绪、PrepareMaskAvatarOnFadeIn。

## 任务
1. 画清门口初次对话全链（BlackPanel + Prefab 前奏节点序、Duration/StartAlpha、是否并行）。
2. 画清进屋续聊全链（换场遮罩 → OnEnterScene Trigger → Prefab 前奏）；标玩家「看得见的淡入」窗口。
3. 按 H1～H5 证伪；对比两 Prefab 图头是否真同构（勿只看「有没有节点」）。
4. 门口结束是否有淡出？本期是否必须做续聊结束淡出？写入 OPEN。
5. 推荐 T1/T2/T3 组合；最小改动清单；强调勿丢续聊 Tips/针线包节点；勿开 LoadingPanel。
6. 与 0902 空头像决议衔接：续聊框出勿 PrepareMask 预亮。

## 报告
Assets/Doc/执行文档/0902/Village_村长家继续对话_开场分层淡入对齐门口_架构溯源报告.md
```

---

## 施工 Prompt（根因拍板后复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0902/Village_村长家继续对话_开场分层淡入对齐门口_架构溯源报告.md
@Assets/Doc/提示词/0902/Village_村长家继续对话_开场分层淡入对齐门口_架构侦探提示词.md

## 目标
Village_村长家继续对话 开场观感对齐门口初次对话：
黑屏压住 → 立绘淡入 → 对话框淡入 → 再出首句；禁止硬切齐活。

## 默认施工方向（若报告未改口）
1. 按报告采纳 T1 或 T2，保证玩家能「看见」淡入（禁止只在遮罩下播完）。
2. Prefab：三立绘 + 对话框 StartAlpha 显式 0→1；节点序/阻塞对齐门口；PrepareMaskAvatarOnFadeIn 保持关（对齐 0902 空框）。
3. 若动 GSM：注释写清与 ChiefNearDoorStoryTrigger 成对关系；勿用 LoadingPanel。
4. 保留续聊专有节点（针线包 Tips / GetItem / Save 等）。
5. Setup 防回潮：Continue Setup 勿冲掉分层参数。
6. 代码/图改动写详细注释与原因；同步 OPEN_QUESTIONS.md。

## 约束
- 禁止改门口初次对话去迁就续聊（续聊向门口靠）
- 禁止蛋糕读条；禁止取消自动续聊
- 禁止整 Prefab 覆盖导致丢 Tips
- 禁止打开续聊 PrepareMask 预亮导致空字有头像
- 回归：门口初次分层仍正常；进屋落点；续聊摆位；续聊结束换古莎（若已有）

## 落盘
Assets/Doc/施工说明/0902/Village_村长家继续对话_开场分层淡入对齐门口_施工说明.md

## 验收
- [ ] 进屋续聊：可见黑屏（或等价压黑）后再见立绘淡入、再对话框淡入，非三件套瞬现
- [ ] 体感与门口初次对话同级（时长手感接近）
- [ ] 首句前框可空字；小头像不早于首句乱亮（0902）
- [ ] 三人立绘摆位/Scale 仍对齐门口定稿
- [ ] 针线包 Tips 等续聊中段节点仍在
- [ ] 门口初次对话回归未坏
- [ ] 无 LoadingPanel 蛋糕读条
```

---

## 给开发者（一句话）

续聊图里**可能已有淡入节点**，但多半在**换场遮罩下播完**或 **StartAlpha 没从 0 起**，所以看起来像硬切。要对齐门口：让玩家在亮屏后**真的看见**「黑屏 → 立绘 → 对话框」这一串。
