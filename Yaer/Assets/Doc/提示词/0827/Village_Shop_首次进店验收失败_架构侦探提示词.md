# Cursor Agent Prompt · Village_Shop 首次进店验收失败 · 五项反馈根因排查

> **角色**：【架构侦探】只读溯源 → 输出 **最小修复清单** 给【施工员】  
> **日期**：2026-08-27（验收员反馈 · 续 0827 总装）  
> **场景**：`Village_Shop.unity` · 进店 `Door_Shop`  
> **对话 Prefab**：`Village_ShopStart.prefab`  
> **前置报告**：`0827/Village_Shop_首次进店Village_ShopStart联调_架构溯源报告.md`（v1.0 · MVP 曾拍板 **不做黑屏**）  
> **验收截图现象**：对白 **已能触发**（字幕「欢迎啊，咦？生面孔？」= CSV **ID2 店**）；Mask 左框 **全黑**；买卖 UI 仍可见；大立绘 **无表情切换**  
> **本阶段**：只读；禁止改代码 / 场景 / Prefab

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 验收员五项反馈（原文 · 须逐条裁定根因）

| # | 反馈 | 截图/现象 | 侦探优先查 |
|---|------|-----------|------------|
| **F1** | **没有黑屏渐入渐出** | 进店后直接进对白，无 BlackFade | 0629 §4 vs MVP「无黑屏」；换场黑幕是否已被 `OnEnterScene` 提前结束 |
| **F2** | **没有隐藏商店 UI** | 对白时仍见买卖 Bar/Tab | `Village_ShopSceneManager` 已写 `UI_Shop.SetActive(false)` —— **时序/Find 失败/又被打开**？ |
| **F3** | **没有商人小头像** | 对话框左侧 Mask **黑块** | 店句 `UseShopkeeperPortrait` → `MerchantMaskPainting` 未亮 / 未绑 / 被 HideAll / Scale=0 |
| **F4** | **大立绘也没有对应变化表情** | 三角色画面静态 | **店**：`ShopkeeperFaceRegistry` / 合层 Toggle；**雅/古**：Prefab Painting + Mask Presenter |
| **F5** | **结束之后没有黑幕恢复商店 UI** | 对白结束直接回买卖或无过渡 | `onStoryEnd` 仅 `SetActive(true)`，**无 BlackFade**（0629 要求） |

### 截图关键推断（待证伪）

- 字幕 **ID2 店**（`Face1`）→ 应走 **双轨**：合层 `Face1` + Mask `MerchantMaskPainting`。  
- Mask **黑** ≠ 「没做 Merchant」一种原因；也可能是 **Active=false / alpha=0 / 脸节点未 Toggle / Image 无 Sprite**。  
- 屏上三角色：可能来自 **Prefab 大立绘（雅/古淡入）+ 场景合层（老板娘）**；若 **全静态**，须分角色查 **是都没切** 还是 **仅店没切**。

### 现网施工状态（磁盘预扫 · 2026-08-27 晚）

| 项 | 预扫 |
|----|------|
| `Village_ShopSceneManager.TryTriggerShopStartStoryOnce` | **已有**：藏 `UI_Shop` → `TriggerStory("Village_ShopStart")` → `onStoryEnd` 显 UI |
| `DialogueTMPUGUI` 店句 | **已有**：Registry + `ApplyShopkeeperPortrait` |
| `MerchantMaskPainting` | Panel 内已嵌；Presenter 有引用 |
| MVP 黑屏 | 报告 **明确排除**；验收 **现要求补** → 本期须 **升格为 P0/P1** |

### 与上轮报告 diff（验收驱动）

| 上轮 MVP | 验收要求 | 侦探须输出 |
|----------|----------|------------|
| 无进店/结束黑幕 | **F1 + F5 要黑幕** | 对齐 `Village_KenMuNiStart` / `BlackFadeComponent` / NodeCanvas Action |
| 对白结束直接显 UI | **黑幕后恢复 UI** | 时序：`onStoryEnd` → Fade → Show `UI_Shop` |
| Trigger 未做 | **已做**（截图证明能播） | 改查 **藏 UI / 表情 / 黑幕** |
| 店句 Mask 理论已接 | **F3 黑块** | **运行时证伪** Presenter 链 |

### 严禁

- 把五项合成一句「都没做」——Trigger **已部分成功**  
- 未分雅/古/店就改 `DialogueFaceType`  
- 用 SR `MerchantPainting` 直接修 Mask  
- 侦探阶段改代码

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店Village_ShopStart联调_架构溯源报告.md
@Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/Doc/执行文档/0804/对话框小表情_首句未跟FaceType_架构溯源报告.md
@Assets/Dialog/Village_商店首次对话.csv
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab
@Assets/Prefabs/DialougeProtrait/MerchantMaskPainting.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/MerchantMaskPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/StoryComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialoguePanelTaskAction.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab。只读 + 写「验收失败根因报告 + 最小修复清单」。

---

## 背景

0827 首次进店总装已施工：`Village_ShopSceneManager` Trigger + 藏显 `UI_Shop`。  
验收员 Play 后反馈 **五项**（见上表）；截图显示 **对白能播、店句 Mask 黑、UI 未藏、表情不变、无黑幕**。  
请 **逐条 F1～F5 找根因**，输出 **可施工的最小 diff 清单**（按优先级 P0/P1/P2）。

---

## 侦探任务清单

### A. 复现与日志（必做）

Play 路径：**新档 → Door_Shop → Village_Shop**。记录 Console 过滤：

- `[ShopStart]` / `[VillageShopDebug]` / `[MerchantMask]` / `[MaskAvatar]` / `[ShopkeeperFace]`

确认：

| 项 | 填 |
|----|-----|
| `TriggerStory` 是否成功 | |
| `UI_Shop` 藏/显日志 | |
| 店句 ID2 是否有 Registry / Mask 日志 | |
| 是否有 Warning「未绑定 / 未注册 / Face 找不到」 | |

### B. F1 · 进店无黑屏渐入

1. 换场 `LoadScene` 黑幕 vs 对白前 BlackFade 是否 **两层不同**？  
2. `Village_KenMuNiStart` 进店黑幕 + 分层亮屏 **哪段可复用到 Shop**？  
3. `Village_ShopStart` 图内是否 **缺** BlackFade / `NormalDialogueUIAlphaAnimationTaskAction` 节点？  
4. **修复建议**：GSM 侧 vs Prefab 图侧 vs 两者都要？

### C. F2 · 对白未藏商店 UI

1. `Find("UI_Shop")` 能否找到？Hide **时机**是否在 UI 已渲染一帧之后？  
2. `ShopFormLogic.Awake/Start` 是否 **重新 SetActive(true)**？  
3. 验收员看到的「商店 UI」是 **`UI_Shop` Bar** 还是 **对话 Panel 的 save/load**（勿误判）？  
4. **修复建议**：提前到 `OnInit`？CanvasGroup alpha？禁用 `GraphicRaycaster`？

### D. F3 · 商人小头像黑块（店句 · 截图 ID2）

1. `NormalDialogueNewPanel.useMaskAvatar` 是否为 **true**？  
2. `MerchantMaskPainting` 实例：Active？CanvasGroup alpha？Image 有 Sprite？  
3. `ApplyShopkeeperPortrait` 是否被调用？`HideAll` 后是否 **SetActive(true)**？  
4. 店句 `OnGetNewStatement(None,…)` 是否 **误触发 Presenter 清空**？  
5. Scale/Pos 是否把脸裁到 Mask 外（看起来黑）？  
6. **修复建议**：代码 / Prefab / 两者？

### E. F4 · 大立绘表情不随 CSV 变

**分三角色填表**（至少 ID1 雅 / ID9 古 / ID2·ID34 店）：

| ID | Speaker | CSV Face/Body | 大立绘载体 | 期望 | 现网 |
|----|---------|---------------|------------|------|------|
| 1 | 雅 | Surprised | Prefab GoOut + Mask | | |
| 9 | 古 | ForcedSmile | Prefab Gusha + Mask | | |
| 2 | 店 | Face1 | 合层 | | |
| 34 | 店 | Face2+Red | 合层+Mask | | |

查：

1. Prefab 内 SayEx **`UseShopkeeperPortrait` / ShopBody / ShopFace** 是否与 CSV 一致  
2. `ShopkeeperFaceRegistry` 注册时机 vs 首句店白  
3. 雅/古 Prefab Painting **`UpdateFace`** 是否被 `SetDefaultPainting` 盖住（0804 竞态）  
4. 场景合层 **Body/Face 默认 Active** 是否只有 Face2 误开导致「看起来不变」  
5. 屏上三角色是否 **全是合层静态图** 而非 Prefab Painting？

### F. F5 · 结束后无黑幕恢复 UI

1. 现网 `OnShopStartStoryEnd` 仅 `UI_Shop.SetActive(true)` —— 与 0629 差距  
2. 对齐样板：KenMuNi 结束是否 BlackFade → 再亮场景  
3. **修复建议**：`onStoryEnd` 订阅内串 BlackFade 回调再显 UI；或 Prefab 图末节点 Action  
4. 与 F1 是否 **同一套 BlackFade** 组件（避免双实现）

### G. 最小修复清单（给施工员 · 按 P0/P1/P2）

| 优先级 | 对应反馈 | 文件/模块 | 动作（一句话） |
|--------|----------|-----------|----------------|
| P0 | | | |
| P1 | | | |
| P2 | | | |

**必须标注**：每项是 **代码 / Prefab / 场景 / NodeCanvas 图** 哪一类改动。

### H. 验收员回归表（修完后）

| # | 操作 | 通过标准 |
|---|------|----------|
| 1 | 新档首次进店 | **F1** 有黑幕渐入（或拍板等价） |
| 2 | 对白中 | **F2** 不见买卖 Bar |
| 3 | 店句 ID2 | **F3** Mask 见商人脸（非黑） |
| 4 | 雅/古/店各一句 | **F4** 大立绘+Mask 与 CSV 一致 |
| 5 | 对白结束 | **F5** 黑幕 → 恢复 `UI_Shop` |
| 6 | 二进宫 | 不播对白，直接 UI |

### I. 开放问题（写入报告）

- 0629 黑幕 **是否本期必做**（验收已要求，建议升格）？  
- F2 验收截图里的 save/load 是否算「商店 UI」？  
- 是否需 **对白期间禁 ESC 开菜单**？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/Village_Shop_首次进店验收失败_架构溯源报告.md`

结构（MASTER 四段式）：

① **结论一句话**（五项里几项真 bug / 几项是 MVP 与 0629 差距 / 最大 P0 是什么）  
② **原因**（逐条 F1～F5，通俗 + 技术锚点）  
③ **用户/验收员复测清单**  
④ **给程序**：P0/P1/P2 施工表 + 时序图（进店→黑幕→藏UI→对白→表情→结束→黑幕→显UI）

口头汇报同样四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店验收失败_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab

你现在是【施工员】。严格按验收失败报告 P0→P1→P2 修复，修一条验一条。

必须遵守：
- 五项反馈逐条对账，报告外不扩 scope；
- 店句：Registry + MerchantMaskPainting 双轨同步；
- 雅/古：不回退 Mask/Prefab 链；注意 0804 首句竞态；
- 0629 黑幕若报告 P0：对齐 KenMuNi/BlackFade 现有组件，禁止 Update 堆逻辑；
- 代码含详细注释。

提交说明：每项 F1～F5 修复点、回归表结果、Console 关键 Log。
```
