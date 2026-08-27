# Cursor Agent Prompt · MerchantPainting UI 版 · 商人对话框小表情（Mask 立绘）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-27  
> **源 Prefab（非 UI）**：`Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab`  
> **对照样板（UI 大/小立绘）**：`GoOutStoryYaerPainting.prefab` · `GushaPainting.prefab`  
> **挂点目标**：`NormalDialogueNewPanel.prefab` → `Bottom/Mask/YaerAvatarRoot`（对话框 **小表情 / Mask 立绘**）  
> **关联链**：店句 CSV `Face1～5` + 可选 `BodyType` · `UseShopkeeperPortrait` · 场景 `商店界面合层` Toggle（大立绘真源）  
> **本阶段**：只读；禁止改 Prefab / 代码 / 场景

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> 已有 **`MerchantPainting.prefab`**，结构是 **世界空间 `SpriteRenderer` + Body/Face 子物体 Toggle**（与场景 `商店界面合层` 同构），**不是**雅/古那种 **UGUI `Image` + `StoryFormPainting`**。  
> 现要做 **UI 版本**，接到 **对话框左侧 Mask 小表情**，店（老板娘）说话时能在字幕条头像窗里看到 **3 Body × 5 Face** 组合变化。

### 源 Prefab vs 样板 Prefab（磁盘预扫 · 2026-08-27）

| 项 | **`MerchantPainting`**（源 · 非 UI） | **`GushaPainting`**（UI 样板） |
|----|--------------------------------------|--------------------------------|
| 根组件 | **`Transform`** only | **`RectTransform`** + CanvasRenderer + **CanvasGroup** + **`GuShaPainting`** |
| Layer | **0**（Default） | **5（UI）** |
| 叶子渲染 | **`SpriteRenderer`** | **`Image`** |
| 树结构 | **`Body`**（Normal/Red/YinXian）+ **`Face`**（Face1～5） | **`Clothes`** + **`Faces`**（枚举名子节点） |
| 脚本 | **无** `StoryFormPainting` | **`GuShaPainting : StoryFormPainting`** |
| 用途假说 | 场景合层参考 / 待嵌对话大立绘？ | Mask 小表情 + 对话 Prefab 大立绘 |

**推论**：不能直接把 `MerchantPainting` 拖进 `YaerAvatarRoot`——**缺 RectTransform/Image/CanvasGroup**，Mask 裁切无效。

### 现网小表情链路（Mask · 0803 已接线）

```
DialogueTMPUGUI.OnGetNewStatement(role, faceType, text)
  → DialogueMaskAvatarPresenter.Apply(role, faceType)
  → ResolvePainting(role) → GoOut / Yaer / Gusha / Amy / Aliy
  → painting.UpdateFace(faceKey)   // 单维 Faces 名
```

**店句现网（0827 Body/Face 报告）**：

```
UseShopkeeperPortrait == true
  → ShopkeeperFaceRegistry.Apply(body, face)   // 场景合层大立绘
  → OnGetNewStatement(DialogueRoleName.None, …)   // ★ Mask 不驱动
```

**缺口**：Mask **无 Merchant 槽位**；店句 **故意传 None**，小表情窗 **不会亮商人**。

### 双轨目标（钉死）

| 轨 | 载体 | 驱动 | 本期 |
|----|------|------|------|
| **大立绘** | `Village_Shop` → `商店界面合层` | `ShopkeeperFaceRegistry` | 已有 Toggle 链 |
| **小表情（Mask）** | `NormalDialogueNewPanel` → 新 **UI Merchant Painting** | 侦探须设计 | **本期目标** |

两轨 **同一 CSV Body/Face** 应同步，但 **Prefab 分离**（SR 场景 vs UI Mask）。

### 结构差异：Body+Face vs StoryFormPainting.Faces

`StoryFormPainting.UpdateFace(string)` 只切 **`Faces` 下一维**；商人要 **Body × Face 两维 Toggle**。

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A · 专用 `MerchantMaskPainting`（推荐倾向）** | 新 UI Prefab + 脚本 **不继承** StoryFormPainting；API `Apply(body, face)` 复制 Toggle 逻辑；Presenter **店句专用分支** | 与 `ShopkeeperFaceController` 同构；不污染 Faces 键 | 要改 Presenter / DialogueTMPUGUI |
| B · 继承 StoryFormPainting + 魔改 | 把 Body 当 Clothes、Face 当 Faces | 复用 Presenter 框架 | Face 键 `Face1` vs `Laugh` 混用；Body 切换无口 |
| C · 共用 `ShopkeeperFaceController` 抽基类 | SR/UI 双后端 | DRY | 重构面大 |
| D · 仅做 UI Prefab 不接代码 | 美术占位 | ❌ 无法验收 | — |

### Face 键与 CSV（勿混）

| 角色 | CSV FaceType | Mask 键 |
|------|--------------|---------|
| 雅/古 | `Laugh` / `Cry` … `DialogueFaceType` | 枚举名 / `Armor_NoHeadWear_*` |
| **店** | **`Face1`～`Face5`** | **`Face1`～`Face5`**（+ Body Normal/Red/YinXian） |

**不要**把 `Face1` 追加进 `DialogueFaceType` 给 Mask 用（0827 已否决）。

### 须比较的 UI Prefab 命名 / 资产策略

| ID | 策略 | 说明 |
|----|------|------|
| P1 | 新建 **`MerchantMaskPainting.prefab`**（UI），保留原 **`MerchantPainting`**（SR 参考） | 双 Prefab，职责清晰 |
| P2 | 原 Prefab **就地改 UI**，删 SR 版 | 丢场景参考 |
| P3 | 一个 Prefab 两套子树 SR+UI | ❌ 维护重 |

### 挂点与摆位（参考 Gusha §4.1）

- 实例化于：`NormalDialogueNewPanel` → `Bottom/Mask/YaerAvatarRoot/MerchantMaskPainting`（名待定）  
- **默认 Active=false**；Presenter `HideAll` 时一并关  
- Pos/Scale **独立定稿**（勿抄 Gusha/Yaer 数值）  
- 技术说明：`NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md` §4

### 严禁（本阶段）

- 把 SR 版 `MerchantPainting` 直接嵌 Mask 不转换  
- 店句改走 `DialogueFaceType` 或扩枚举 Face1  
- 用场景 `ShopkeeperFaceRegistry` 同时驱动 Mask（Registry 只应管合层）  
- 本期接首次进店存档 / 藏 UI / 黑屏（除非用户另提）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_ShopStart_新建Merchant对话NPC_架构溯源报告.md
@Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/Doc/技术文档/演出相关/DialogDebug对话测试场景_技术说明.md
@Assets/Doc/执行文档/6月/0614/雅儿对话立绘素材替换_执行说明.md
@Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab
@Assets/Prefabs/DialougeProtrait/GushaPainting.prefab
@Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/StoryFormPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/GuShaPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/SubtitlesRequestInfoEx.cs
@Assets/Scripts/Game/Static/Enum/Role/RoleName.cs
@Assets/Editor/Tool/Dialogue/ShopkeeperCsvDefaults.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改 Prefab / 代码 / 场景。只读 + 写溯源报告。

---

## 背景

`MerchantPainting.prefab` 是 **SpriteRenderer + Transform** 的 Body/Face 树，与 UI 样板 **GushaPainting**（Image + RectTransform + StoryFormPainting）不同。  
策划要做 **商人对话框小表情**：在 Mask 头像窗显示店句的 Body/Face。请给出 **UI Prefab 怎么做、脚本怎么接、与场景合层大立绘如何双轨同步** 的最小方案。

---

## 侦探任务清单

### A. 源 Prefab 全量结构表（MerchantPainting）

| 节点 | 组件 | 默认 Active | 源图 guid/路径 |
|------|------|-------------|----------------|
| Body/Normal… | | | |
| Face/Face1… | | | |

是否缺 CanvasGroup / 脚本？与场景 `商店界面合层` diff。

### B. UI 样板对拍（Gusha / GoOut）

列出 **UI 必备组件清单**（Layer、RectTransform、Image、CanvasGroup、脚本）及 **Merchant 迁移映射**：

| SR 节点 | UI 对应 | Image 源 Sprite 是否复用同 PNG |
|---------|---------|--------------------------------|

### C. 脚本架构裁定（核心）

1. 新类名与是否继承 `StoryFormPainting`？  
2. API：`Apply(ShopkeeperBodyType, ShopkeeperFaceType)` vs `UpdateFace(string)`？  
3. **Presenter 改动**：  
   - 扩 `DialogueRoleName`？还是 `UseShopkeeperPortrait` 分支直接调 MerchantMask？  
   - `DialogueTMPUGUI` 店句是否改 `OnGetNewStatement` 参数？  
4. 能否复用 `ShopkeeperFaceController` 逻辑（抽 ToggleHelper）？  
5. 与 `ShopkeeperFaceRegistry` **隔离**（Mask vs 场景）如何保证？

### D. Prefab 资产策略（P1/P2/P3 拍板）

- 新 Prefab 路径名建议  
- 原 `MerchantPainting.prefab` 保留还是废弃  
- `NormalDialogueNewPanel` 嵌实例步骤 + 建议 Pos/Scale 初值思路

### E. CSV / 运行时双轨同步

| 事件 | 场景合层 | Mask UI |
|------|----------|---------|
| 店句 ShopBody+ShopFace | Registry | ? |
| 雅/古句 | 不变 | 不变 |

同一帧两轨是否必须一致？若 Registry 未注册（DialogDebug）Mask 是否仍应显示？

### F. 最小施工清单（本阶段不执行）

| # | 模块 | 动作 |
|---|------|------|
| 1 | 新建 UI Prefab | SR→Image 迁移 |
| 2 | 新脚本 MerchantMaskPainting（或报告命名） | Toggle Body/Face |
| 3 | NormalDialogueNewPanel | 嵌实例 + Presenter 引用 |
| 4 | DialogueMaskAvatarPresenter / DialogueTMPUGUI | 店句驱动 Mask |
| 5 | DialogDebug / Village_Shop | 验收 |

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Mask 窗内见商人脸（非空窗） | |
| 2 | 店句 CSV Face2 + Body Red：Mask 与场景合层 **一致** | |
| 3 | 雅/古句：仍走原 Mask，不亮 Merchant | |
| 4 | 店句：Merchant 亮，Gusha/Yaer 关 | |
| 5 | DialogDebug 仅店句时 Mask 可测（无 Registry 时行为按报告） | |
| 6 | 首句竞态：无 GoOut 式 SetDefault 盖脸 | |

### H. 开放问题

- UI 版命名：`MerchantMaskPainting` vs `MerchantUIPainting`？  
- 小表情是否要 **3 Body 全做** 还是 Mask 仅 Face、Body 固定 Normal？  
- 是否需 `DialogueRoleName.Shopkeeper` 枚举？  
- SR 版 `MerchantPainting` 未来用途（对话大立绘 / 仅美术参考）？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md`

结构（MASTER 四段式）：

① 结论（UI Prefab 怎么做 + 脚本/Presenter 怎么接）  
② 原因（SR≠UI；店句现网 Mask 不驱动）  
③ 用户检查清单（Prefab 编辑 + 两个场景怎么测）  
④ 给程序：节点映射表 + 方案 A/B/C + 施工文件清单

口头汇报同样四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md
@Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs

你现在是【施工员】。按报告实现 Merchant UI 版 + Mask 小表情接线。

必须遵守：
- 新建 UI Prefab（勿直接把 SR MerchantPainting 塞进 Mask）；
- Body/Face Toggle 与 CSV ShopBody/ShopFace 同步；
- 与场景 ShopkeeperFaceRegistry 职责分离；
- 雅/古 Mask 行为不回退；
- 代码含详细注释；摆位只在 NormalDialogueNewPanel 实例上 override。

提交说明：新 Prefab 路径、Presenter 改动、DialogDebug/Village_Shop 验收结果。
```
