# Cursor Agent Prompt · Village_Shop 首次进店播 Village_ShopStart · 三角色小表情/立绘联调

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-27  
> **场景**：`Assets/GameRes/Scenes/Village_Shop.unity`（用户截图：`Main Camera` / `商店界面合层` / `UI_Shop` / `SceneManager`）  
> **对话 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab`  
> **台本 CSV**：`Assets/Dialog/Village_商店首次对话.csv` → 生成图 `Village_商店首次对话.asset`  
> **产品目标**：玩家 **第一次** 正常进店 → 播首次对白 → **雅尔 / 古莎 / 商人（店）** 字幕 + **小表情（Mask）** + **立绘变化** 全对；第二次进店 **跳过** 对白直接买卖  
> **本阶段**：只读；禁止改场景 / Prefab / 代码

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话 · 拼在一起）

> 前面已做好：`Village_ShopStart` 对话 Prefab、CSV、Merchant Actor、Body/Face Toggle、**MerchantMaskPainting** 小表情。  
> 现在要 **串起来**：从 `Door_Shop` 进 `Village_Shop` 后，**同档首次** 自动播 `Village_ShopStart`，并保证：
> - **主角（雅）**、**古莎**、**商人（店/老板娘）** 对话框 **小表情** 跟 CSV `FaceType` 变  
> - **商人** 场景 **大立绘**（`商店界面合层` Body/Face）与 **Mask 小表情** 双轨同步（含 `BodyType=Red` 等行）  
> - **雅/古** 大立绘（Prefab 内 Painting 淡入）+ Mask 小表情一致  
> - 对白结束后进入正常 **`UI_Shop` 买卖**（0629 理想态还含藏 UI / 黑屏，见开放问题）

### 前置资产现网（0827 链 · 侦探须证伪）

| 模块 | 路径 / 键 | 预扫状态 |
|------|-----------|----------|
| 对话 Prefab | `Village_ShopStart.prefab` | 含 `BG`/`Yaer`/`Gusha`/`Merchant`；雅/古 Painting 淡入前奏 |
| CSV | `Village_商店首次对话.csv` | 雅/古 `FaceType`；店 `Face1～5` + 行 34+ **`BodyType=Red`** |
| 生成图 | `DialogueTrees/Generated/Village_商店首次对话.asset` | Prefab 已 Bind？ |
| Actor | 雅尔 / 古莎 / **老板娘**→`Merchant` | Merchant 是否已绑？ |
| 场景合层大立绘 | `Village_Shop` → `商店界面合层` | `ShopkeeperFaceController` + Registry |
| Mask 小表情 | `NormalDialogueNewPanel` → `MerchantMaskPainting` | `ApplyShopkeeperPortrait` 已接？ |
| 雅/古 Mask | 同 Panel → GoOut/Gusha Painting | `DialogueMaskAvatarPresenter` |
| GSM | `Village_ShopSceneManager` | **无** 首次进店 Trigger（预扫） |
| 存档只播一次 | `StoryTriggerCountData.CheckStoryUsed` | 键名待定为 **`Village_ShopStart`**？ |
| 运行时引用 | 全工程搜 `Village_ShopStart` | 0827 报告：**未搜到** Trigger |

### 三角色 · 四轨对照（验收必写清）

| 角色 | CSV Speaker | 大立绘（屏幕） | 小表情（Mask） | Face 键 |
|------|-------------|----------------|----------------|---------|
| **雅尔** | `雅` | Prefab `GoOutStoryYaerPainting` 淡入 | `Presenter` → GoOut/Yaer | `DialogueFaceType` |
| **古莎** | `古` | Prefab `GushaPainting` 淡入 | `Presenter` → Gusha | `DialogueFaceType` |
| **商人** | `店` | 场景 `商店界面合层` Toggle | `MerchantMaskPainting` | `Face1～5` + Body |

**店句特殊**：`UseShopkeeperPortrait=true` → **不走** `DialogueActorEx.RefreshAvatar`；须 **Registry + Presenter 双调**。

### 0629 首次进店演出 vs 本期 MVP

| 项 | 0629 策划 §4 | 本期最低可验收（侦探须拍板范围） |
|----|--------------|--------------------------------|
| 只播一次 | `StoryTriggerCountData` 或独立 bool | **必须** |
| 触发时机 | 打开商店 UI **之前** | 倾向：**对白期间藏 `UI_Shop`** |
| 对白中 | **隐藏** 买卖 UI | 侦探定 MVP：藏 Bar/Tab 还是整 `UI_Shop` |
| 对白结束 | **黑屏** → 再出完整 UI | **开放问题**：本期做黑屏还是直接显 UI |
| 构图 | 左雅/古 + 右老板娘 | 靠 Prefab Painting + 场景合层 |

### 触发方案对比（侦探必选）

| 方案 | 触发点 | 优点 | 风险 |
|------|--------|------|------|
| **T1 · GSM `OnEnterScene`（推荐倾向）** | `Village_ShopSceneManager`：`CheckStoryUsed("Village_ShopStart")` → `TriggerStory` | 对齐 `Village_KenMuNiStart`；进门即播 | 与换场黑幕时序要协调 |
| T2 · `ShopFormLogic` 首次 Awake | 打开 UI 前判断 | 符合 0629「UI 前触发」 | 逻辑散在 Form；GSM 无玩家 |
| T3 · 场景 `SimpleStoryTrigger` | 挂 SceneManager | 复用组件 | 商店无玩家碰撞，不适用 |
| T4 · DialogDebug 手动拖 Prefab | 仅测 | 不能替代正式 Trigger | 验收用 |

### 对白期间 UI / 合层可见性

| 物体 | 对白中期望（0629） | 侦探核实现网 |
|------|-------------------|--------------|
| `UI_Shop`（买卖 Bar） | **隐藏** | 默认 Active？ |
| `商店界面合层` | **可见**（老板娘大立绘） | GSM 进场 `SetActive(true)` |
| `NormalDialogueNewPanel` | **由 TriggerStory 打开** | `StoryComponentGSM` 标准链 |
| Prefab `BG` | 前奏节点控制 | Prefab 内 `BG` 默认 Active=0？ |

### 严禁（本阶段）

- 改 CSV 台本内容（除非侦探发现 Import 致命错）  
- 把店句改回 `DialogueFaceType` 或去掉 `UseShopkeeperPortrait`  
- 首次进店与「点头/点胸特殊交互」(`0601`) 混同一 Trigger  
- 未验三角色就只做 Trigger 壳

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/Doc/执行文档/0827/Village_ShopStart_新建Merchant对话NPC_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md
@Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md
@Assets/Doc/技术文档/演出相关/DialogDebug对话测试场景_技术说明.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Dialog/Village_商店首次对话.csv
@Assets/GameRes/DialogueTrees/Generated/Village_商店首次对话.asset
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/StoryComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Story/StoryTriggerCountData.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/MerchantMaskPainting.cs
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改场景 / Prefab / 代码 / CSV。只读 + 写溯源报告。

---

## 背景

用户要把 **`Village_ShopStart`** 与 **`Village_Shop` 首次进店** 拼成完整链路，并验收 **雅尔 / 古莎 / 商人** 的 **对话框小表情** 与 **立绘（含 Body/Face）变化** 全正确。  
0827 各子系统（Actor、Body/Face CSV、MerchantMaskPainting）已分轮施工；本单做 **Trigger + 时序 + 三角色联调 + 只播一次** 的总装方案。

---

## 侦探任务清单

### A. 现网缺口表（必出）

| # | 项 | 现网 | 阻塞首次进店？ |
|---|-----|------|----------------|
| 1 | `TriggerStory("Village_ShopStart")` 调用点 | | |
| 2 | `StoryTriggerCountData` 键名 | | |
| 3 | `Merchant` Actor 已绑 | | |
| 4 | `MerchantMaskPainting` 在 Panel 内 | | |
| 5 | `ShopkeeperFaceRegistry` 场景注册 | | |
| 6 | Prefab 图与 CSV 同步 | | |
| 7 | 对白中 `UI_Shop` 可见性 | | |
| 8 | 对白结束 → 显 UI / 写存档 | | |

### B. 触发链路设计（拍板 T1～T4）

1. 推荐方案 + 调用栈（进门 → 黑幕 → Trigger → StartDialogue → 结束 → UI）  
2. 对齐样板：`Village_KenMuNiSceneManager.TryTriggerVillageStartStoryOnce` 可复用哪些模式  
3. `StoryComponentGSM.OnStoryEnd` 时：显 `UI_Shop`、是否黑屏、是否 `CheckStoryUsed` 已自动 +1  
4. **第二次进店**：`CheckStoryUsed` 为 true → 跳过 Trigger，直接买卖

### C. 三角色表现 · 逐轨验收表

对 CSV **抽样行**（至少各 1 句）：

| ID | Speaker | FaceType | BodyType | 大立绘期望 | Mask 小表情期望 |
|----|---------|----------|----------|------------|-----------------|
| 1 | 雅 | Surprised | | | |
| 9 | 古 | ForcedSmile | | | |
| 2 | 店 | Face1 | | 合层 | MerchantMask |
| 34 | 店 | Face2 | Red | 合层脸红 | Mask 脸红 |

**首句竞态**：雅/古 Mask 是否被 `SetDefaultPainting` 盖脸；店句是否被 `ResetDefault` 盖脸。

### D. 对白 Prefab 与前奏节点

1. `Village_ShopStart` 图：FightingPanel 隐藏 → 雅/古 CanvasGroup 淡入 → …  
2. 是否需 **Merchant/合层** 淡入节点（现网仅雅/古 Blackboard 变量）  
3. `BG` Active 与相机 / 合层关系  
4. Prefab 内 Painting **RectTransform** 与场景合层 **Transform** 分工（给用户摆位指南）

### E. UI 藏显与 0629 差距

| 阶段 | UI_Shop | 合层 | 对话 Panel |
|------|---------|------|------------|
| 换场进店 | | | |
| 首次对白 | | | |
| 对白结束 | | | |
| 再次进店 | | | |

MVP 与完整 0629 分两步写施工清单。

### F. 最小施工清单（给施工员 · 本阶段不执行）

| # | 模块 | 动作 |
|---|------|------|
| 1 | `Village_ShopSceneManager` | 首次进店 Trigger + 藏/显 UI |
| 2 | 存档键 | `Village_ShopStart` + `StoryTriggerCountData` |
| 3 | Prefab/Actor | 复核 Merchant 绑定 |
| 4 | Panel | 复核 MerchantMaskPainting + Presenter |
| 5 | 场景 | 复核 Registry + 合层默认 Active |
| 6 | NodeCanvas | 对白结束 Action（显 UI / 可选黑屏） |
| 7 | 文档 | 验收表 |

### G. 验收清单（用户可照做）

| # | 操作 | 通过 |
|---|------|------|
| 1 | **新档** Init → 村 → `Door_Shop` 进店 | 自动播首次对白 |
| 2 | 对白中 | `UI_Shop` 买卖不可用/不可见（按拍板） |
| 3 | 雅句 | Mask 小表情 = CSV FaceType |
| 4 | 古句 | 同上 |
| 5 | 店句 | 合层 Body/Face + Mask **一致**（测 ID34 Red+Face2） |
| 6 | 对白结束 | 出现买卖 UI；ESC 可用 |
| 7 | **同档第二次** 进店 | **不播** 对白，直接 UI |
| 8 | 读档后再进 | 仍不播（存档一致） |
| 9 | Console | 无 Missing Actor / Registry 未注册 / Face 键找不到 |

### H. 开放问题

- 本期是否做 0629 **黑屏后再出 UI**？  
- 触发点在 **OnEnterScene** 还是 **UI 首次 Awake 前**？  
- 对白中是否禁用 ESC 开菜单？  
- `Village_ShopStart` 与生成图 asset 名不一致是否统一？  
- DialogDebug 能否部分验三角色（店句需合层）？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/Village_Shop_首次进店Village_ShopStart联调_架构溯源报告.md`

结构（MASTER 四段式）：

① 结论（Trigger 方案 + 三角色是否已具备 / 还缺啥）  
② 原因（四轨对照 + 0629 与 MVP 差距）  
③ 用户验收清单  
④ 给程序：时序图 + 施工步骤 + 与 0827 子报告衔接表

口头汇报同样四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店Village_ShopStart联调_架构溯源报告.md
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs

你现在是【施工员】。按报告实现首次进店 Trigger + 三角色联调验收。

必须遵守：
- 存档键 `StoryTriggerCountData` 只播一次；
- 店句双轨：Registry + MerchantMaskPainting 同步；
- 雅/古走现有 Mask + Prefab Painting，不回退；
- 对白期间按报告藏 `UI_Shop`；结束后再显；
- 最小 diff；代码含详细注释；
- 新档/二进宫/读档三条路径冒烟。

提交说明：触发点、UI 藏显、抽样句 ID 验收、Console 过滤词。
```
