# NewGameStory 开场间隔对齐 KenMuNi — 架构溯源报告

**文档版本**：v1.0（2026-08-07）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 场景 / 图集 / CSV / 台本**）  
**范围**：序章 `NewGameStory` **已有** BG / 大立绘 / 对话框淡入，但**时间间隔不对** → 对齐进村标准（顺序 + 一律 0.5s）  
**本阶段**：只对照差距与改法；**不施工**  
**依据**：
- `Assets/Doc/提示词/0807/NewGameStory_开场间隔对齐KenMuNi_架构侦探提示词.md`
- **标准真源**：`Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`
- 关联：`执行文档/0806/NewGameStory_开场分层对齐KenMuNi标准_架构溯源报告.md`（曾标「已施工」）  
- 关联：`执行文档/0807/NewGameStory_对话卡死与开场异常_架构溯源报告.md`（**方案 B 回退 Prefab 已落地**）

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**间隔不对的根因不是「差 0.2 秒手感」，而是现网前奏仍是 0807-B 回退后的「并行三件套」**（`BlackMask Dur1` + `YaerShow` Clip + `UIAlpha Delay0.5/Dur0.7` 同开）——体感叠化乱、无标准「只见 BG」空拍、立绘非 CanvasGroup 0.5、框时长 0.7≠0.5。  
**只改数字不够**；**必须改成串行结构**。完整对齐（含真·只见 BG）推荐 **方案 D**（Prefab 串行 0.5 + `NewGameSceneManager` Gate/Prepare/HideFade 成对）；若先只拧 Prefab、接受「空拍 A 弱/靠默认闸」可走 **方案 A**，但须处理 `Start.anim` 与 CanvasGroup 抢 alpha（0807 已踩坑）。

---

## ② 原因（生活类比）

灯光 cue（淡入效果）其实都在：黑幕在拉、立绘在动、对话框也在淡。  
问题是**三个人同一拍一起动手**，节拍器又各走各的（1.0 / Clip / 0.7），所以听起来「有渐变，但气口不对」。  
进村已经练成固定节拍：**亮布景 → 停半拍 → 演员淡入半拍 → 停半拍 → 提词板淡入半拍**。序章要把「一起动手」改成「按这个节拍器排队」。

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板（施工前必须选）

1. **只改参数够不够？** → 侦探结论：**不够**；至少去并行、改串行。  
2. **BlackMask / YaerShow 去留？**  
   - 对齐标准：**前奏主路径去掉**对话壳 BlackMask；立绘改 `YaerPainting` CanvasGroup Fade 0.5。  
   - `YaerShow`：若去掉须同时处理 `Start.anim` 仍写 alpha=0（见 0807）；可「Fade 后再触发」或关动画写 alpha。  
3. **是否补 NewGame Gate 旁路？**  
   - **要真·「只见 BG」空拍 A** → **必须**旁路（方案 D）：Reset → Trigger → Prepare → HideFade → Signal。  
   - 仅 Prefab Wait（闸默认 Ready）→ 只有 Hold 0.5，**不等于**村拍1 亮屏后空拍；漫画关 Form 露景时序仍松。  

### 验收清单

1. 新游戏：漫画后 → **BG → 0.5 空拍 → 大立绘 0.5 → 0.5 空拍 → 框 0.5 → 首句**（肉眼对齐进村）。  
2. 分层期间无离谱并行叠化（黑幕/立绘/框不同时抢戏）。  
3. DialogDebug 拖同 Prefab 节奏大体一致、不永久卡死。  
4. 进村 `Village_KenMuNiStart` 无回归；点击仍可推进。  
5. 立绘可见（对抗 `Start.anim` alpha=0，勿再半施工）。

---

## ④ 给程序看的补充

### 4.1 标准清单勾选（NewGame 现网）

| 标准项 | NewGame 现网 | 判定 |
|--------|--------------|------|
| 空拍 A：Wait + Hold **0.5** | **无** Wait 节点 | **缺** |
| 大立绘：CanvasGroup Fade **0.5**（阻塞） | **`MecanimSetTrigger YaerShow`**（跟 Clip） | **机制不同** |
| 空拍 B：对话框 Delay **0.5**（立绘完成后再计） | Delay=**0.5**，但与 BlackMask/YaerShow **并行**，从树开跑就计 | **有但语义错** |
| 对话框：Duration **0.5**（阻塞；按需 PrepareMask） | Duration=**0.7**；`EndActonOnAnimationEnd` 空；PrepareMask 空 | **有但值错 / 不阻塞 / 无 Mask 预亮** |
| 前奏串行 | `executionMode=1` **Parallel** | **缺**（并行） |
| （可选拍1）Prepare + HideFade + Gate Signal | `NewGameSceneManager` **仅** `TriggerStory` + BGM | **缺** |

### 4.2 标准 vs 现网时序图

```
【KenMuNi 标准 · 技术说明 §二～§四】
全黑 Trigger → Prepare 白名单 → CloseFormFade（拍1 露 BG）
  → Gate.Signal
  → Wait(Hold 0.5)                    ← 空拍 A：只见 BG
  → 立绘 CanvasGroup Fade 0.5（并行雅儿/古莎，EndAction=true）
  → UIAlpha Delay 0.5 + Dur 0.5（PrepareMask）  ← 空拍 B + 拍3
  → Statement

【NewGame 现网 · 2026-08-07 方案 B 回退后 · 磁盘核实】
漫画 CloseFormShowFade 全黑
  → onFinish：TriggerStory("NewGameStory") + 龙宫 BGM
       （无 Gate / 无 Prepare / 无 HideFade）
  → 关漫画 Form（遮罩随 Form）
  → 对话壳 Open + 树开跑
  → Node0 ActionList Parallel（executionMode=1）：
       · BlackMask：StartAlpha=1 → 0，Duration≈1.0，EndActon 空
       · MecanimSetTrigger YaerShow（Clip 时长）
       · UIAlpha：Delay=0.5，Duration=0.7，EndActon 空，PrepareMask 空
  → 紧接 Statement
  → ★ 无「只见 BG」；三件叠跑；框 0.7≠0.5；立绘非标准 Fade
```

**秒级体感偏差（估计）**

| 时刻（树开跑≈t0） | 标准（Signal 后） | 现网并行 |
|-------------------|-------------------|----------|
| +0.0 | 只见 BG，开始 Hold | 黑幕开始淡 + YaerShow 触发 + UIAlpha 进 Delay |
| +0.5 | Hold 完，立绘开始 Fade | 框开始淡入（立绘可能已半出）；黑幕还在淡 |
| +1.0 | 立绘落定，进 Delay 空拍 B | 黑幕约完；框仍在淡（至 ≈+1.2） |
| +1.5 | 框开始 Fade | 可能已进 Statement / 框刚完 |
| +2.0 | 框落定 → 首句 | 与标准错位约 **0.5～1s+**，且无清晰气口 |

### 4.3 Prefab 对照表

| 标准职责 | KenMuNi（`Village_KenMuNiStart`） | NewGame 现网（`NewGameStory`） | 差距 |
|----------|----------------------------------|-------------------------------|------|
| 串/并行 | 前奏 **串行**多节点；仅立绘组内 Parallel | 开场 **单一** ActionList `executionMode=1` Parallel | 结构不符 |
| 空拍 A Hold 0.5 | `WaitVillageStartBgReveal` Hold=**0.5** | **无** | 缺 |
| 大立绘 Fade 0.5 | `CanvasGroupAlpha`×2（GoOut/Gusha），Dur=**0.5**，`EndActionOnAnimationEnd=true` | **`YaerShow` Mecanim**；BB 仍有 `YaerPainting` 变量但**前奏未绑 Fade** | 机制不同 |
| 对话框 Delay+Dur 0.5 | Delay=**0.5** Dur=**0.5**，阻塞，PrepareMask 开 | Delay=**0.5** Dur=**0.7**，不阻塞，PrepareMask 关，且并行 | 参数+串行+阻塞 |
| BlackMask | 前奏**不用** | 并行 BlackMask Dur≈**1.0** | 多余且抢拍1语义 |
| Statement 衔接 | 前奏阻塞完成后 | 并行 fire-and-forget 易提前进句 | 风险 |

**YaerShow vs CanvasGroup**：二者是**替代关系（标准路径）**，不是可叠的等价旋钮。保留 YaerShow 当主路径 → **难精确 0.5、难维护**（方案 C）。若保留，仅建议作「Fade 落定后的姿态触发」，不能代替 Duration=0.5。

### 4.4 参数差距表（旋钮）

| 拍 | 字段 | 标准目标 | NewGame 现网 | 动作 |
|----|------|----------|--------------|------|
| 空拍 A | `HoldAfterBgVisibleSeconds` | **0.5** | 无节点 | **加 Wait** |
| 拍2 | `YaerPainting` CanvasGroup `Duration` | **0.5** + EndAction=true | 无（现 YaerShow） | **换节点** |
| 空拍 B | UIAlpha `Delay` | **0.5**（立绘后才跑） | 0.5 但并行提前计 | **串到立绘后** |
| 拍3 | UIAlpha `Duration` | **0.5** | **0.7** | 改 0.5 |
| — | ActionList / 多节点 | Serial 主链 | Parallel 三件套 | **去并行** |
| — | BlackMask / YaerShow | 主路径无 | 主路径有 | **移除或后置** |
| — | Gate/Prepare/HideFade | 村旁路有 | SceneManager **无** | 方案 D 必补 |

### 4.5 方案比选

| 方案 | 摘要 | 能否对齐 0.5 | 风险 | 推荐？ |
|------|------|--------------|------|--------|
| **A** | Prefab 改串行：Wait→YaerPainting Fade0.5→UIAlpha Delay0.5+Dur0.5；去并行 BlackMask/YaerShow；旁路按需 | **节奏数字能**；真·只见 BG **弱**（闸默认 Ready 只 Hold） | 再踩 `Start.anim` 抢 alpha；与旁路不成对则体感仍飘 | 可作「先 Prefab」折中 |
| **B** | 仅 UIAlpha Duration 0.7→0.5，其余不动 | **否** | 并行仍乱 | ❌ |
| **C** | 保留 YaerShow，只调 Clip/Delay | **难精确** | 难维护、难验收对齐 | ❌ |
| **D** | Prefab 串行 0.5 + `NewGameSceneManager` Reset/Prepare/HideFade/Signal 成对（对齐村拍1） | **能** | 改动略大；须处理遮罩随漫画 Form 销毁；须处理 Start.anim | **完整对齐推荐** |

**「只调间隔」最小结论**：  
- **改参数不够** → 必须 **去并行、改串行**（至少方案 A 的 Prefab 结构）。  
- **要肉眼对齐进村含「只见 BG」** → 必须 **方案 D**（0806 半施工教训：Prefab 单边改必翻车）。

### 4.6 施工员终值表（可抄）

| 字段 | 建议值 |
|------|--------|
| `HoldAfterBgVisibleSeconds` | **0.5** |
| `YaerPainting` CanvasGroup `Duration` | **0.5** |
| `EndActionOnAnimationEnd`（立绘 Fade） | **true** |
| UIAlpha `Delay` | **0.5** |
| UIAlpha `Duration` | **0.5** |
| UIAlpha `EndActonOnAnimationEnd` | **true** |
| `PrepareMaskAvatarOnFadeIn` | 按需 **true**（角色 Dress/Yaer；注意室内 Dress 另案） |
| 前奏主链 | **Serial**：Wait → 立绘 Fade → UIAlpha → Statement |
| `NormalDialogueBlackMask` | **从前奏主路径移除**（拍1 改旁路 HideFade） |
| `YaerShow` | **从前奏主路径移除**；若 `Start.anim` 仍压 alpha，Fade 后触发或关动画写 alpha |
| `NewGameSceneManager`（方案 D） | Gate.Reset → Trigger → Prepare 白名单（字幕条 + `YaerPainting`）→ HideFade → Signal；BGM 保留 |

### 4.7 施工员最小改动清单（待拍板后）

| 优先级 | 文件 | 做什么 |
|--------|------|--------|
| 1 | `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab` | 拆并行三件套 → 串行 Wait(0.5) → YaerPainting Fade(0.5) → UIAlpha(0.5/0.5) → Statement；BB 绑 `YaerPainting` |
| 2（D 必做） | `NewGameSceneManager.cs` | 漫画 Finish 旁路成对：Reset / Prepare / HideFade / Signal |
| 3（D） | Prepare 白名单 | 只动 `dialogueUICanvasGroup` + `DialogueSceneContainer/YaerPainting`；**禁止**名字广扫 Panel |
| 4 | `Start.anim` / Animator | 避免与 CanvasGroup Fade 抢 `m_Alpha`（0807 根因之一） |
| 不改 | 台本、漫画分页、进村已落地 Prefab/旁路行为（除非抽公共） | |

### 4.8 文档债说明（0806 ↔ 现网）

| 声称 | 磁盘现网（2026-08-07 再扫） |
|------|------------------------------|
| 0806 报告末「✅ 已施工」方案 A | **误标 / 半施工**：旁路从未在 `NewGameSceneManager` 落盘 |
| 0807 卡死报告方案 B | **已落地**：Prefab 回退并行；与现网一致 |
| 本期「间隔不对」 | = **可玩并行态** vs **KenMuNi 串行 0.5** 的差距，不是玩家错觉 |

### 4.9 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | 是否丢掉前奏 BlackMask / YaerShow？ | **主路径丢掉**；YaerShow 仅抗 anim 时后置 |
| Q2 | NewGame 是否必须补 Gate 旁路才有「只见 BG」？ | **要真对齐则必须**（方案 D）；仅 Hold 不够 |
| Q3 | 0806「已施工」与现网并行是否回退/误标？ | **半施工误标 + 0807-B 有意回退**；见上节 |

---

## 施工员落地（2026-08-07 · 方案 D）

| 文件 | 改动 |
|------|------|
| `NewGameCartoonFormLogic.cs` | `CloseFormShowFade` → 仅 `ShowFade` 回调；关 Form 交旁路 |
| `NewGameSceneManager.cs` | System `BlackPanel` RawShow → 关漫画 → Gate Reset → Trigger → Prepare 白名单（字幕+`YaerPainting`+关 Animator）→ HideFade → Signal |
| `NewGameStory.prefab` | 串行 Wait0.5 → YaerPainting Fade0.5 → UIAlpha Delay0.5+Dur0.5(+PrepareMask)；去 BlackMask/YaerShow；BB 绑 CanvasGroup |
| `Start.anim` | 删除 `Fg/YaerPainting.m_Alpha` 曲线 |
| `NewGameStory.controller` | 默认态 `WriteDefaultValues=0` |

**验收**：漫画后 → BG → 0.5 → 立绘 0.5 → 0.5 → 框 0.5 → 首句；点击可推进；进村无回归；DialogDebug 不永久卡（闸默认 Ready）。
