# Cursor Agent Prompt · NewGameStory 主角大立绘不出现（Prepare 与 Prefab 不成对）

> **角色**：【架构侦探】只读溯源；**禁止改代码 / Prefab / 动画 / 场景 / Git 提交**  
> **日期**：2026-09-11  
> **现象（用户实测已钉死）**：  
> 1. 新游戏流程进入 **`NewGameStory`** 对白后，**主角大立绘全程不出现**  
> 2. 对话字幕 / 推进仍可进行（非整段卡死；问题聚焦「大立绘不可见」）  
> 3. 用户已确认范围是 **序章 `NewGameStory`**，不是进村 `Village_KenMuNiStart`  
> **产品期望（钉死）**：漫画结束后进入 `NewGameStory`，屏幕上能看见主角大立绘（`YaerPainting`，连衣裙+皇冠），表情可切；点击可推进至树尾；进村开场无回归  
> **不是**：换立绘素材 PNG；改村线 `GoOutStoryYaerPainting`；改 Mask 小头像；改漫画分页；一次性重写全部对话分层系统  
> **并行**：与 0911 StartScene Build Settings 案解耦；与村长家/巨树案解耦——本案是 **序章大立绘显隐链路断裂**  
> **报告落盘**：`Assets/Doc/执行文档/0911/NewGameStory_主角大立绘不出现_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 新游戏播完漫画，开始龙宫对话了，字有，**雅儿大立绘没有**。  
> 不是进村那一段，就是 `NewGameStory`。

### 链路锚点（助手预扫）

| 项 | 值 |
|----|-----|
| 场景 | `NewGameScene`（`NewGameSceneManager`） |
| 漫画结束 → 剧情 | `TriggerStory("NewGameStory")`（旁路现为分层：Gate / Prepare / HideFade） |
| 对话 Prefab | `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab` |
| 大立绘物体名 | **`YaerPainting`**（嵌套母体 `Assets/Prefabs/DialougeProtrait/YaerPainting.prefab`） |
| 立绘脚本族 | `YaerPainting` / `StoryFormPainting`（**不是** `GoOutStoryYaerPainting`） |
| 恢复 Animator 的设计入口 | `CanvasGroupAlphaActionTask.TryRestoreStoryAnimatorAfterPortraitFade`（淡完后开 Animator 并落到 `YaerShow` 末帧） |

### 现网机制（助手预扫 · 高度可疑）

| 环节 | 预扫结果 | 若失败的体感 |
|------|----------|--------------|
| **旁路 Prepare** | `NewGameSceneManager.PrepareNewGameLayeredReveal`：白名单把 `DialogueSceneContainer` 下 **`YaerPainting` CanvasGroup.alpha=0**；并 **Disable** 带 `YaerShow`/`KingMove` 参数的故事根 Animator | 立绘被「藏起」且动画播不了 |
| **旁路注释期望** | 淡入应由 **`CanvasGroupAlpha`** 完成，淡完再开 Animator、落到 YaerShow 末帧供 KingMove | Prefab 必须有成对的 YaerPainting Fade |
| **Prefab 前奏（现网序列化）** | 首 ActionList **仍是并行旧三件套**：`NormalDialogueBlackMask` + **`MecanimSetTrigger("YaerShow")`** + `NormalDialogueUIAlpha`(Delay0.5/Dur0.7) | **没有** `WaitVillageStartBgReveal` / **没有** 对 `YaerPainting` 的 `CanvasGroupAlphaActionTask` |
| **文档声称** | `OPEN_QUESTIONS` 0807-D：**已去前奏 BlackMask/YaerShow**，改串行 Wait→YaerPainting Fade→UIAlpha；旁路成对 | **文档写已施工，磁盘 Prefab 仍像 0807-B 回退态** → 半施工/回退漂移高度同构 |
| **对照村线（成对样板）** | `Village_KenMuNiStart`：Wait → `CanvasGroupAlpha(GoOutStoryYaerPainting)` 等 | 村线有淡回；序章缺淡回则「只有序章没立绘」 |
| **Prefab 默认 alpha** | 磁盘上 `YaerPainting` 节点 CanvasGroup 默认可为 1；**运行时被 Prepare 盖成 0** 才是主战场 | 「Prefab 里看着有」≠ Play 时可见 |
| **历史坑** | 0807 报告：去掉 YaerShow 却未成对旁路 / Start.anim 抢 alpha → 立绘永不出 | 本案是 **反向**：旁路 D 已上、Prefab 仍靠 YaerShow |

### 假说表（须并列证伪，按优先级写进报告）

| ID | 假说 | 证伪手段 |
|----|------|----------|
| **H1（首选）** | **旁路 Prepare 藏立绘+关 Animator**，Prefab 仍只靠 `YaerShow`，**无 CanvasGroup 淡回** → alpha 永 0 | 对照 `PrepareNewGameLayeredReveal` 与 Prefab 首节点序列化；Play 时看 `YaerPainting.alpha` 与 Animator.enabled |
| **H2** | Prefab 已有 CanvasGroupAlpha，但 **Blackboard 未绑** / 名字搜错 / 绑到 Mask 下同名节点 | 搜 Prefab JSON：`CanvasGroupAlpha`、`YaerPainting` BB；Hierarchy 确认淡入目标是否场景大立绘 |
| **H3** | `Start.anim` / Write Defaults 仍每帧把 alpha 写回 0（即使开了 Animator） | 查 `NewGameStory.controller`、`Start.anim`、`YaerShow.anim` 对 `Fg/YaerPainting` 的 `m_Alpha` 曲线；默认态 Write Defaults |
| **H4** | Prepare 白名单找错物体 / 未找到 `DialogueSceneContainer`，行为与预期不符 | 看 Console `[NewGameStory][Prepare]` 日志；核对查找路径 |
| **H5** | 立绘 Active=false / 被别的节点藏、或服装 Faces 键错导致「整人消失」（体感像没立绘） | Hierarchy Active；`UpdateFace` / Faces 子节点是否缺 `Dress_Crown_*` |
| **H6** | 只在真机/某入口复现，DialogDebug 拖 Prefab 正常（说明纯旁路差） | DialogDebug 拖 `NewGameStory` vs 完整新游戏链路对比 |
| **H7** | 文档 0807-D「已施工」与磁盘不一致（Prefab 回退、脚本保留 D） | `git log` / 对比 0807 施工说明与当前 Prefab 图；标清「谁落地、谁漂移」 |

### 方案倾向（仅倾向，侦探可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A（成对修 Prefab·对齐 D）** | Prefab 改串行：Wait(Gate)→`CanvasGroupAlpha(YaerPainting 0→1, Dur0.5)`→UIAlpha(Delay0.5+Dur0.5)；去掉前奏主路径 BlackMask/`YaerShow` Trigger；BB 绑场景 `YaerPainting` | **首选**（与现网旁路、村线样板、OPEN 决议一致） |
| **B（回退旁路保可玩）** | 临时去掉 Prepare 藏立绘/关 Animator，恢复纯 YaerShow 可玩 | 仅紧急 unblock；与分层标准冲突，须标 OPEN |
| **C** | 只在 Prepare 后强制 `alpha=1` / 强开 Animator | **禁止作终局**（破坏分层空拍，易复发） |
| **D** | 改 GameFramework / 通用 Dialogue 点击类 | **禁止**；本案非通用点击坏 |

### 侦探禁止事项

- 禁止修改任何 `.cs` / Prefab / `.anim` / `.controller`「先修绿再查」。
- 禁止擅自 `git add` / `commit` / `push`。
- 禁止借机改村线 Prefab、换立绘素材、改 Mask 头像。
- 禁止只改旁路或只改 Prefab 就宣称结案——**必须写清成对关系**。

---

## 【架构侦探】执行段（复制给 Agent）

你是【架构侦探】。只读溯源，**禁止改代码**。

### 目标

查清：为何完整新游戏链路下 **`NewGameStory` 主角大立绘不出现**；给出可证伪根因，以及与 `NewGameSceneManager` Prepare / Prefab 前奏 / Animator 的成对结论；输出最小修复建议（优先成对，拒绝单边临时修补）。

### 必查清单

1. **触发与旁路**  
   - 读清：`NewGameCartoon` 结束 → `OnCartoonFinishedFullyBlack` → `TriggerStory("NewGameStory")` → `PrepareNewGameLayeredReveal` → HideFade → Gate Signal。  
   - 逐条写出 Prepare **对 `YaerPainting` / Animator 做了什么**（alpha、enabled）。

2. **Prefab 前奏真源**  
   - 解析 `NewGameStory.prefab` 对话树**首段 Action**（及是否串行 Wait）。  
   - 表格列出：有无 BlackMask、有无 `YaerShow` Trigger、有无 `CanvasGroupAlpha(YaerPainting)`、UIAlpha 参数。  
   - 对照 `Village_KenMuNiStart` 成对样板差在哪。

3. **运行时证据（能读则写；须用户复验则列清单）**  
   - Console：`[NewGameStory][Prepare] hide YaerPainting`、`disable Animator`、`[CanvasGroupAlpha] … YaerShow` 是否出现。  
   - Hierarchy：对白首句时 `YaerPainting` 的 `CanvasGroup.alpha`、Animator.enabled。

4. **动画抢写**  
   - `Start.anim` / `YaerShow.anim` / controller Write Defaults：是否仍会在 Animator 重新启用后把 alpha 打回 0。  
   - 说明：H1 成立时，即使修好 Fade，是否还须防 H3。

5. **文档 vs 磁盘**  
   - 对照 `Assets/Doc/OPEN_QUESTIONS.md` 0807-D「已施工」表述与当前 Prefab；若漂移，报告中明确「脚本侧 D / Prefab 侧 B」或等价结论。

6. **排除项一句话**  
   - 素材丢失、Mask 小头像、村线 GoOut、漫画本身——是否本案主因。

### 报告结构（写入落盘路径）

1. 结论一句话（根因 + 是否 H1）  
2. 证据链（路径、方法名、Prefab 节点类型、日志关键词）  
3. 假说表（H1～H7：成立 / 排除 / 待 Play 确认）  
4. 用户侧 1 分钟自查清单（大白话）  
5. 建议施工步骤（**成对**最小改动；禁止项）  
6. 剩余风险（King 立绘、KingMove 衔接、DialogDebug 与真链路差异）  
7. 若设计不清：记入 `Assets/Doc/OPEN_QUESTIONS.md`（例如：是否允许再回退 B）  
8. 文末附「可复制给施工员」的 5～10 条 bullet（仍不直接改）

### 输出要求

- 中文；大白话优先；结构：结论 → 原因 → 检查清单 →（可选）程序补充。  
- 报告落盘：`Assets/Doc/执行文档/0911/NewGameStory_主角大立绘不出现_架构溯源报告.md`  
- 可引用：  
  - `Assets/Doc/执行文档/8月/0807/NewGameStory_对话卡死与开场异常_架构溯源报告.md`  
  - `Assets/Doc/执行文档/8月/0807/NewGameStory_开场间隔对齐KenMuNi_架构溯源报告.md`  
  - `Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`  
  - `Assets/Doc/执行文档/6月/0614/雅儿对话立绘素材替换_执行说明.md`（大立绘路径对照）

---

## 【施工员】Prompt（根因拍板后再用 · 预稿）

> 仅当侦探确认 **H1（Prepare 藏立绘 + Prefab 无 CanvasGroup 淡回）** 或等价「旁路/Prefab 不成对」后使用。

1. **优先方案 A**：按村线标准改 `NewGameStory.prefab` 前奏为串行  
   - Wait（复用村 Gate / Hold 0.5，以侦探报告为准）  
   - `CanvasGroupAlpha`：`YaerPainting` 0→1，Duration 0.5，`EndActionOnAnimationEnd=true`（确保走恢复 Animator）  
   - 再 UIAlpha 对话框（Delay/Dur 按 KenMuNi 0.5；PrepareMask 按报告）  
   - **去掉**前奏主路径 `NormalDialogueBlackMask` + `MecanimSetTrigger(YaerShow)`（KingMove 仍依赖淡完落到 YaerShow 末帧）  
2. Blackboard 绑定场景大立绘 `YaerPainting` CanvasGroup（勿绑 Mask 下同名）。  
3. **禁止**只删 Prepare、或只强制 alpha=1 当终局；**禁止**改 GameFramework；**禁止**动村线 Prefab。  
4. 若拍板走紧急 B：只回退 NewGame Prepare 藏立绘/关 Animator，并写 OPEN_QUESTIONS，标注非终局。  
5. 施工说明落盘：`Assets/Doc/施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md`。  
6. 验收：新游戏漫画后可见 `YaerPainting` → 可点多句 → King 出场/KingMove 不崩；DialogDebug 拖同 Prefab 不永久卡；`Village_KenMuNiStart` 无回归。

---

## 【验收员】Prompt（施工后 · 预稿）

1. 可临时加 `[NewGamePortraitDebug]` 日志：Prepare 后 alpha、Fade 后 alpha、Animator.enabled、是否进入 YaerShow 末帧。  
2. 优先环境：完整新游戏链路（不要只测 DialogDebug）。  
3. 输出验证结果 + 剩余风险（动画曲线、第二角色立绘）。

---

## 给用户的一句话

> 很大概率不是图坏了：开场脚本先把大立绘藏成透明并关了动画，但对话树还在用旧的「播 YaerShow」——两边没对上，所以字有、人没有。
