# Cursor Agent Prompt · Village_Shop 首次进店 · 第二波遗留（开场黑屏 + 大立绘表情）

> **角色**：【架构侦探】只读溯源 → 输出 **最小修复清单** 给【施工员】  
> **日期**：2026-08-27（第一波修复后 · 验收员复测）  
> **场景**：`Village_Shop.unity` · 进店 `Door_Shop`  
> **对话 Prefab**：`Village_ShopStart.prefab`  
> **前置报告**：`0827/Village_Shop_首次进店验收失败_架构溯源报告.md`（v1.0）  
> **本阶段**：只读；禁止改代码 / 场景 / Prefab

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 第一波已修 vs 本轮仅查两项

| 验收项 | 第一波状态 | 本轮 |
|--------|------------|------|
| F2 藏商店 UI | 已施工（`OnInit` 提前藏 + 序列化） | **不查**（验收标 Fix） |
| F3 商人 Mask 黑块 | 已施工（Presenter `_shopkeeperMaskActive`） | **不查**（复测截图 **小头像已出现**） |
| F5 结束黑幕恢复 UI | 已施工（`OnShopStartStoryEnd` + `ShowShopBlackFade`） | **不查**（验收未再提） |
| **R1 开场黑屏** | **仍有问题** | **✅ 本轮 P0** |
| **R2 大立绘表情** | **仍有问题** | **✅ 本轮 P0** |

### 验收员第二轮反馈（原文要点）

| # | 反馈 | 现象 / 要求 |
|---|------|-------------|
| **R1** | **开场黑屏不对** | 「**一进去就应该是黑屏**，不要先出现商店背景再黑屏」—— 说明 **换场淡出已结束、商店画面已露**，才又叠一层演出黑幕 |
| **R2** | **大立绘表情不变** | 对话框 **Mask 小头像已能变脸**（截图：店句 Mask 有紫发商人表情），但 **右侧/背景大立绘仍静态**，与小头像 **不同步** |

### 截图关键推断（待证伪）

- **Mask 链已通** → R2 **不是** F3 竞态复现；重点查 **大立绘轨道**（Prefab 雅/古 vs 场景合层店）。  
- 店句：**小头像变、合层不变** → `ApplyShopkeeperPortrait` OK，`ShopkeeperFaceRegistry.Apply` **可能未生效 / 被 Reset / 节点名错 / 合层被挡**。  
- 雅/古句：须查 Prefab 内 `GoOutStoryYaerPainting` / `GushaPainting` 的 **`RefreshAvatar`→`UpdateFace`** 是否驱动 **大立绘实例**（不是只驱动 Mask）。  
- R1：**时序 Bug**，不是「完全没有黑幕」—— 现有 `ShowShopBlackFade` 在 **`OnEnterScene`（换场黑幕已淡出后）** 才调，必然 **先闪商店**。

### 现网时序（磁盘预扫 · 2026-08-27 第二波前）

```
LoadScene BlackPanel FadeShow
  → 场景加载 OnInit（可藏 UI_Shop）
  → CloseFormFade（换场黑幕淡出）     ← 此处开始能看见商店合层
  → OnBlackFadeEnd → OnEnterScene
      → FocusMainCameraOnShopComposite（合层 Active + 相机对准）
      → TryTriggerShopStartStoryOnce
          → ShowShopBlackFade（再 FadeShow）  ← 验收员看到的「先露店再黑」
          → TriggerStory → CloseFormFade
```

**KenMuNi 对照**：`Village_KenMuNiSceneManager.TryDeferBlackFadeForCover` —— **换场黑幕仍全黑时 Trigger**，BG/立绘就绪后再 `CloseFormFade`，**不二次闪村景**。

### R1 修复方向候选（侦探须裁定推荐项）

| 方案 | 做法 | 风险 |
|------|------|------|
| **A · DeferCover** | `Village_ShopSceneManager` 覆写 `TryDeferBlackFadeForCover`：首次进店 **不换场淡出**，黑幕内 Trigger + 藏 UI | 须对齐 KenMuNi 超时/回调；二进宫仍走默认淡出 |
| **B · 推迟 OnEnterScene 露景** | `FocusMainCameraOnShopComposite` 挪到演出黑幕 `onShowEnd` 之后 | 黑幕期间相机/合层状态要定义 |
| **C · 保持换场黑不淡出** | 首次进店 `LoadScene` 侧 hold 黑幕直到 Story Ready | 动 `LoadSceneComponentGSM` 影响面大 |
| **D · 仅加快第二次 FadeShow** | 仍 OnEnterScene 再黑 | **不能**满足「一进去就黑」—— 应 **否决** |

### R2 三轨大立绘（侦探须分轨填表）

| 角色 | CSV Speaker | 大立绘载体 | 驱动入口 | Mask 载体 | 截图推断 |
|------|-------------|------------|----------|-----------|----------|
| 雅 | `雅` | Prefab `GoOutStoryYaerPainting` | `DialogueActorEx.RefreshAvatar` | Mask GoOut | 待 Play |
| 古 | `古` | Prefab `GushaPainting` | 同上 | Mask Gusha | 待 Play |
| 店 | `店` | 场景 `商店界面合层` Body/Face | `ShopkeeperFaceController.Apply` | `MerchantMaskPainting` | **Mask 变、合层不变** |

**R2 优先怀疑（店）**：

1. `ShopkeeperFaceController.Start` → `ResetDefault()` **晚于** 首句 `Apply`，把脸打回 Face1  
2. Registry 未注册（Awake 顺序 / 合层 inactive）  
3. Hierarchy `Body/Red`、`Face/Face2` 与 `ShopkeeperBodyType.Blush` 映射  
4. 合层在 Prefab 对话 Canvas **下层**，肉眼以为「大立绘」其实是 **静态 Prefab 层** 而非合层  
5. CSV `UseShopkeeperPortrait` / `ShopBody` / `ShopFace` 解析与 SayEx 字段不一致（仅 Mask 对、Registry 参数错）

**R2 优先怀疑（雅/古）**：

1. Prefab 大立绘 **无** `DialogueActorEx` 绑定 → `RefreshAvatar` 空跑  
2. 图内仅 **CanvasGroup 淡入**，未接 Actor 事件 → 大立绘永远默认脸  
3. 0804 首句竞态：`SetDefaultPainting` Smile 盖 Surprised（Mask 已修，大立绘仍可能）  
4. 大立绘与 Mask **不是同一套 UpdateFace 键**（GoOut 键名 vs DialogueFaceType 枚举）

### 严禁

- 把 R1 当成「再叠一层 BlackPanel 就行」—— 问题是 **换场已淡出**  
- 把 R2 再修 Mask / F3 —— 验收已确认小头像 OK  
- 未分雅/古/店就改 `DialogueFaceType` 或 CSV  
- 侦探阶段改代码

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店验收失败_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店Village_ShopStart联调_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md
@Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md
@Assets/Doc/执行文档/0804/对话框小表情_首句未跟FaceType_架构溯源报告.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/Dialog/Village_商店首次对话.csv
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab
@Assets/Prefabs/DialougeProtrait/MerchantMaskPainting.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/LoadSceneComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceRegistry.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/MerchantMaskPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/GoOutStoryYaerPainting.cs
@Assets/Scripts/Game/GameRuntime/Story/NodeCanvasExtend/DialogueActorEx.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab。只读 + 写「第二波遗留根因报告 + 最小修复清单」。

---

## 背景

0827 首次进店 **第一波** 已修：F2 藏 UI、F3 Mask 竞态、F5 结束黑幕（见 `Village_ShopSceneManager` / `DialogueMaskAvatarPresenter` 磁盘现状）。
验收员 **第二轮** 仅提 **两项遗留**：

1. **R1**：进店应先全黑，**禁止**先闪商店背景再黑  
2. **R2**：Mask 小头像已正常，**大立绘**（尤其老板娘合层）**不随 CSV 变脸**

请 **只查 R1、R2**，输出可施工最小 diff。

---

## 侦探任务清单

### A. Play 复现（必做 · 新档首次进店）

记录 **帧级观感**（可用录屏或分步描述）：

| 时刻 | 画面 | 对应代码路径 |
|------|------|--------------|
| T0 | 点 Door_Shop | LoadScene BlackPanel |
| T1 | 是否 **先见** 商店合层/背景？ | OnBlackFadeEnd 前/后 |
| T2 | 是否 **再次** 黑屏渐入？ | ShowShopBlackFade |
| T3 | 对白开始 | TriggerStory |
| T4 | 店句 ID2 / ID5 / ID34 | Mask vs 合层 |
| T5 | 雅 ID1 / 古 ID9 | Prefab 大立绘 vs Mask |

Console 过滤：`[ShopStart]` · `[ShopkeeperFace]` · `[MaskAvatar]` · `[MerchantMask]` · `[SceneLoad]`

### B. R1 · 开场黑屏时序

1. 画 **现网 vs 0629 期望** 时序图（换场黑幕 / 演出黑幕 / OnEnterScene / Trigger）。  
2. `Village_ShopSceneManager` **是否未实现** `TryDeferBlackFadeForCover`？与 KenMuNi diff 列表。  
3. `FocusMainCameraOnShopComposite` 在 **哪一帧** 让合层可见？是否 **早于** 任何黑幕？  
4. 首次进店 vs 二进宫：黑幕策略是否应 **分支**？  
5. **推荐方案**（A/B/C 选一或组合）+ 为何否决「OnEnterScene 再 ShowShopBlackFade」。  
6. 若采用 DeferCover：  
   - 何时 TriggerStory？  
   - 何时 CloseFormFade（换场黑幕）？  
   - 是否仍需要 `ShowShopBlackFade` 开场第二次 FadeShow？（倾向 **否**）  
   - 超时兜底秒数参考 KenMuNi `VillageStartCoverTimeoutSeconds`

### C. R2 · 大立绘表情不同步

#### C1. 店（老板娘 · 场景合层）—— 截图主问题

1. 店句时 Console 是否有 `[ShopkeeperFace] SetBody/SetFace`？  
2. `ShopkeeperFaceRegistry.Instance` 非 null 时序（Awake vs 首句）？  
3. `Start` → `ResetDefault()` 是否 **覆盖** 句内 `Apply`？（Start 顺序竞态）  
4. 场景 Hierarchy：`商店界面合层/Body/Red`、`Face/Face2` 与代码映射是否一致？  
5. **验句**：ID5（店 Face2）、ID34（Red+Face2）—— 合层 **肉眼** 是否仍不变？  
6. Mask `MerchantMaskPainting` 与合层 **是否同一套 Body/Face 语义**？参数来源是否同一 `SubtitlesRequestInfoEx`？  
7. 屏上「右侧紫发老板娘」究竟来自 **合层 Sprite** 还是 **其它静态图**？（Hierarchy 层序）

#### C2. 雅 / 古（Prefab 大立绘）

1. `Village_ShopStart` 内 `GoOutStoryYaerPainting` / `GushaPainting`：  
   - 是否挂 `DialogueActorEx` / 是否订阅 `RefreshAvatar`？  
   - 图内 Action 是否 **只有 alpha 淡入** 无换脸？  
2. ID1 `Surprised` / ID9 `ForcedSmile`：大立绘 `UpdateFace` 键 vs Mask Presenter 键是否一致？  
3. 0804 首句竞态：大立绘是否仍被 `SetDefaultPainting` Smile 覆盖？  
4. Prefab 大立绘与 Mask **是否为两个实例**（一个变一个不变属预期错误）？

#### C3. 三轨对照表（Play 后必填）

| ID | Speaker | CSV 脸/身 | Mask 实际 | 大立绘实际 | 根因一句话 |
|----|---------|-----------|-----------|------------|------------|
| 1 | 雅 | | | | |
| 9 | 古 | | | | |
| 2 | 店 | Face1 | （截图已有） | | |
| 5 | 店 | Face2 | | | |
| 34 | 店 | Red+Face2 | | | |

### D. 最小修复清单（给施工员 · 仅 R1/R2）

| 优先级 | 项 | 类型 | 文件/模块 | 动作（一句话） |
|--------|-----|------|-----------|----------------|
| P0 | R1 | | | |
| P0 | R2-店 | | | |
| P1 | R2-雅/古 | | | |

标注：**代码 / Prefab / 场景 / NodeCanvas 图**。

### E. 验收回归表（修完后）

| # | 操作 | 通过标准 |
|---|------|----------|
| 1 | 新档 Door_Shop 进店 | **R1**：全程 **不见** 商店背景闪帧；首帧即黑或黑幕内直接进对白 |
| 2 | 对白店句 ID5 或 ID34 | **R2**：合层 Body/Face **与 Mask 一致** |
| 3 | 对白雅 ID1 / 古 ID9 | **R2**：Prefab 大立绘与 Mask **同表情** |
| 4 | 对白结束 | F5 仍正常（黑幕 → UI_Shop）—— **回归，非本轮主查** |
| 5 | 二进宫 | 无对白，换场淡出正常，不卡黑 |

### F. 开放问题

- R1 是否 **必须** 与 KenMuNi 同构 DeferCover，还是 Shop 纯 UI 场景有更简方案？  
- R2 若仅店有问题，雅/古是否 **可标 P1**？  
- 0629「左侧两女主 + 右侧老板娘」大立绘：雅/古是否 **只用 Prefab Canvas**，店 **只用合层**—— 验收期望是否如此？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/Village_Shop_首次进店第二波遗留_架构溯源报告.md`

结构（MASTER 四段式）：

① **结论一句话**（R1 根因 + R2 最大根因 · 是否 Start 竞态 / DeferCover 缺失）  
② **原因**（R1 时序图 + R2 分雅/古/店）  
③ **验收员复测清单**  
④ **给程序**：P0/P1 施工表 + **目标时序**（进店→黑→对白→表情双轨→结束）

口头汇报同样四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店第二波遗留_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab

你现在是【施工员】。严格按第二波报告 P0→P1 修复，**只动 R1、R2**，不回归改 F3 Mask。

必须遵守：
- R1：优先 TryDeferBlackFadeForCover 或报告裁定方案；**禁止** OnEnterScene 露店后再 FadeShow 当最终解；
- R2-店：Registry.Apply 与 Mask 同源参数；排查 Start ResetDefault 竞态；
- R2-雅/古：大立绘 RefreshAvatar 链与 Mask 对齐；注意 0804 首句；
- 代码含详细注释；修一条验一条。

提交说明：R1/R2 各修复点、C3 对照表 Play 结果、目标时序是否达成。
```
