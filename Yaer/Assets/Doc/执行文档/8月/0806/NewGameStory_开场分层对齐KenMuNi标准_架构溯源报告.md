# NewGameStory 开场分层对齐 KenMuNi 标准 — 架构溯源报告

**文档版本**：v1.0（2026-08-06）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 场景 / 图集 / CSV / 台本**）  
**范围**：序章 `NewGameStory` 开场显现节奏，**统一**到 `Village_KenMuNiStart` 已落地标准（顺序 + 0.5s 间隔）。不改台本、不拆漫画流程本身。  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0806/NewGameStory_开场分层对齐KenMuNi标准_架构侦探提示词.md`
- **标准真源**：`Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`
- 漫画：`NewGameCartoonPanel漫画开场策划案.md`；`NewGameSceneManager` / `NewGameCartoonFormLogic`
- Prefab：`NewGameStory.prefab` vs `Village_KenMuNiStart.prefab`

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**NewGame 缺标准整套：Prepare 白名单、亮屏闸门+Hold、立绘 CanvasGroup Fade、框 Delay+Fade 串行、一律 0.5s；现网是漫画 `CloseFormShowFade` 全黑后立刻 `TriggerStory`，Prefab 开场把对话 BlackMask / `YaerShow` / 对话框 UIAlpha（0.7+Delay0.5）塞进同一并行 ActionList，且 Fade 多不阻塞——体感非 KenMuNi 分层。推荐方案 A：漫画仍遮罩时 Trigger+Prepare，再 HideFade（或等价关遮罩）=拍1 并 Signal；Prefab 按标准重排 0.5；闸门可复用现 `VillageStartLayerRevealGate`（或抽通用名，方案 D 加厚）。**

---

## ② 原因（生活类比）

进村开场已练成固定灯光 cue：幕布拉开只见布景 → 停半拍 → 演员淡入 → 停半拍 → 提词板淡入。  
序章换了入口（先演漫画再进对话），但灯光员还在用旧 cue：漫画一结束就黑一下，然后黑幕/触发器/对话框一起动手——**剧场入口不同，cue 表没换成同一套标准**。

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板

1. **漫画结束 = 拍1？**  
   - 现网：`CloseFormShowFade` 先把黑幕拉满 → `onFinish` 里 Trigger+BGM → 再关漫画。  
   - 建议：**保留「结束时全黑」**，在全黑中 Trigger+Prepare，再 **HideFade 露 BG**（拍1）+ Gate Signal（对齐 KenMuNi）。  
   - 若黑幕挂在漫画 Form 上、关 Form 即露景：须改为 System 黑幕贯穿，或关漫画前切到 System 遮罩，避免裸景。  
2. **Gate / Wait 是否去 Village 前缀抽通用？**（建议：功能先复用现闸门；命名可另小步改通用）  
3. **NewGame 是否丢掉前奏里的对话 BlackMask + `YaerShow`？**（建议：**改纯 CanvasGroup Fade** 对齐标准；`YaerShow` 若只是出场动画，淡入后再触发或并入立绘落定后）

### 验收清单

1. 新游戏：漫画结束 → **BG → 0.5 空拍 → 大立绘 0.5 → 0.5 空拍 → 框+小头像 0.5 → 首句**（肉眼对齐进村）。  
2. 分层期间无裸景齐出；无对话框空名闪一下。  
3. BGM 仍在漫画结束后按现设计播。  
4. DialogDebug 拖 `NewGameStory` 分层大体一致、不卡死。  
5. 进村 `Village_KenMuNiStart` 无回归；其它换场黑幕无回归。

---

## ④ 给程序看的补充

### 4.1 标准 vs 现网时序图

```
【KenMuNi 标准 · 技术说明】
全黑 Trigger → Prepare 白名单 → CloseFormFade（拍1 露 BG）
  → Gate.Signal
  → Wait(Hold 0.5) → 立绘并行 Fade 0.5 → Delay 0.5 → 框 Fade 0.5(+PrepareMask) → Statement

【NewGame 现网】
OnEnterScene → Open NewGameCartoonPanel（HideRow 去条）
  → 漫画播完/跳过
  → CloseFormShowFade：System/Form 黑幕淡入满
  → onFinish：TriggerStory("NewGameStory") + 龙宫 BGM
  → Close 漫画 UI
  → 对话壳 Open + 树开跑（无 Prepare / 无 Gate / 无 Wait）
  → Node0 ActionList Parallel：
       · NormalDialogueBlackMask：1→0 Duration=1（对话壳 BlackMask，非 System）
       · MecanimSetTrigger YaerShow
       · NormalDialogueUIAlpha：Delay=0.5 Duration=0.7（PrepareMask 字段空≈关）
       · EndActonOnAnimationEnd 多为空 → 易 fire-and-forget
  → 紧接 Statement 首句
  → ★ 无「只见 BG」空拍；立绘非标准 CanvasGroup 前奏；间隔非一律 0.5
```

**漫画关完瞬间**：黑幕已满；`TriggerStory` 在关漫画**同一回调链**上。若遮罩随漫画 Form 销毁而消失，可能在壳未 Ready 时露 NewGame 场景——须施工时用 System 黑幕或「壳 Ready + BG 盖满再 Hide」钉死零漏缝。

### 4.2 标准清单勾选（NewGame）

| 标准项 | NewGame |
|--------|---------|
| 全黑或等价遮罩下 Trigger / Prepare | **半有**：全黑后 Trigger；**无 Prepare** |
| Prepare 白名单（字幕条 + 场景立绘） | **缺** |
| 亮屏完成闸门 → Wait + Hold 0.5 | **缺** |
| 场景大立绘并行 CanvasGroup Fade 0.5（阻塞） | **缺**（现 `YaerShow` Mecanim） |
| 对话框 Delay 0.5 + Fade 0.5（清字、按需 PrepareMask） | **部分**：有 UIAlpha 但 Delay0.5+Duration**0.7**、与其它并行、EndActon 空、PrepareMask 空 |
| 禁止名字广扫 Panel | N/A（尚未做 Prepare） |
| DialogDebug 不永久卡闸 | 现无闸；对齐后须默认 Ready |

### 4.3 Prefab 对照表

| 标准职责 | KenMuNi 节点 | NewGame 现节点 | 差距 |
|----------|--------------|----------------|------|
| 藏战斗/其它 | FightingPanelVisible | （无对等前奏） | 可选补 |
| Wait BG + Hold 0.5 | `WaitVillageStartBgReveal` Hold=**0.5** | **无** | 必补 |
| 大立绘 Fade 0.5 | GoOut/Gusha `CanvasGroupAlpha`×2 并行，EndAction=true，Duration=**0.5** | **`MecanimSetTrigger YaerShow`** | 机制不同；场景立绘名 **`YaerPainting`**（有 BG 1920×1080） |
| 对话框 Delay+Fade 0.5 | UIAlpha Delay0.5 Duration0.5，EndActon=true，PrepareMask 开 | UIAlpha Delay0.5 Duration**0.7**，与 BlackMask/YaerShow **并行**，EndActon 空，PrepareMask 空 | 参数+串行+阻塞+Mask |
| 对话壳 BlackMask | 前奏不用 | BlackMask 1→0 Duration=1 **并行** | 建议从前奏移除，改走拍1 System HideFade |
| Statement | 前奏后 | 前奏节点后直接 Statement | 须等前奏阻塞完成 |

### 4.4 方案比选表

| 方案 | 挂点摘要 | 能否对齐 0.5 节奏 | 风险 | 推荐？ |
|------|----------|------------------|------|--------|
| **A** | 漫画 `onFinish`（仍全黑）：Reset Gate → Trigger → onTriggered Prepare 白名单（字幕+`YaerPainting`）→ **HideFade** 作拍1 → 完成时 Signal；Prefab 按 KenMuNi 重排 0.5 | **能** | 须确认 ShowFade 用的黑幕关漫画后是否仍在；必要时改 System 黑幕贯穿 | **推荐** |
| B | 漫画结束后立刻用对话 BG 占位 + Prepare，再分层（不依赖 System HideFade） | 能 | 异步壳未起时易露景；多一套占位 | 次选 |
| C | 仅改 Prefab 参数/节点，SceneManager 不旁路 | **难** | 无闸门则 Delay 与漫画黑幕/露景叠；易无「只见 BG」 | ❌ 单独不够 |
| D | 抽公共 `LayerRevealGate` + Prepare 工具，村与 NewGame 共用 | 能 | 改动面略大；可与 A 同轮或二期 | **加厚优选** |

**方案 A 漫画衔接要点**

1. 保留：跳过/播完 → `ShowFade` 全黑（遮换场感）。  
2. 改：`OnFinish` 内顺序建议：  
   - `Gate.Reset`  
   - `TriggerStory("NewGameStory")`  
   - 订一次 `onStoryTriggered`：Prepare → `HideFade`（拍1）→ `Signal`（对齐村 `Finalize…`）  
   - BGM 仍可在 Finish 时播（现设计）  
3. Prefab：删并行旧三件套主路径 → `Wait` → `YaerPainting` CanvasGroup Fade 0.5 → UIAlpha Delay0.5+Dur0.5（PrepareMask 开，角色 Dress/Yaer）→ Statement。  
4. **禁止**广扫 Panel；白名单立绘名：`YaerPainting`（DialogueScene 下）。

### 4.5 YaerShow / BlackMask 与标准关系

| 现网手段 | 与标准 | 建议 |
|----------|--------|------|
| `NormalDialogueBlackMask` 1→0 | 对话壳内黑幕，**不是** KenMuNi 拍1（System 露 BG） | 前奏**去掉**；拍1 用 HideFade |
| `MecanimSetTrigger YaerShow` | 动画出场，非 CanvasGroup 0.5 Fade | **改为**（或后接）`CanvasGroupAlpha` on `YaerPainting`；YaerShow 仅当落定后仍需姿态动画再保留 |
| UIAlpha 0.7 并行 | 时长/串行/阻塞均不符 | 串到立绘后，Duration=**0.5**，EndActon=true |

### 4.6 施工员最小改动清单（只建议）

| 优先级 | 文件 | 做什么 |
|--------|------|--------|
| 1 | `NewGameSceneManager.cs` | 漫画 Finish 旁路：Gate Reset → Trigger → Prepare → HideFade+Signal；BGM 保留 |
| 2 | Prepare 实现 | **拷贝/抽**村白名单逻辑：只动 `dialogueUICanvasGroup` + `DialogueSceneContainer/YaerPainting`；可薄公共方法（方案 D） |
| 3 | `NewGameStory.prefab` | 前奏对齐 KenMuNi：Wait(Hold0.5) → YaerPainting Fade0.5 → UIAlpha Delay0.5+Dur0.5(+PrepareMask) → Statement；BB 绑 `YaerPainting` CanvasGroup |
| 4 | （可选）Gate/Wait 改通用名 | 去 Village 前缀；村 Prefab 同步引用 |
| 不改 | 漫画分页内容；台本；村已落地逻辑行为（除非抽公共）；其它对话 Prefab | |

**服装交叉**：NewGame 大立绘为 Dress；框 Fade 预亮 Mask 时注意 `yaerUseGoOutOnly`（另案 Dress 启用）——本期主目标分层；验收勿引入黑窗。

### 4.7 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | 漫画结束与拍1：关漫画=露 BG，还是 System HideFade？ | **全黑中 Prepare → HideFade=拍1**（对齐 KenMuNi） |
| Q2 | Gate/Wait 是否抽通用名？ | 功能先复用；命名可二期 |
| Q3 | 是否保留 BlackMask / YaerShow？ | **前奏改 CanvasGroup 标准**；YaerShow 仅必要时后置 |

---

## 施工员下一轮最小化清单（建议 · 待拍板后开）

1. NewGame 旁路：漫画全黑 → Trigger+Prepare → HideFade+Signal。  
2. Prefab 重排为标准四拍，参数一律 **0.5**。  
3. 双验收：序章分层 = 进村；村与换场无回归。  

**✅ 已施工（2026-08-06）**：方案 A 落地——`NewGameSceneManager` 全黑旁路 + Prepare 白名单；`NewGameStory.prefab` 串行 Wait→YaerPainting Fade→UIAlpha（一律 0.5，PrepareMask 开）；Gate 复用村闸门。验收见上文清单。
