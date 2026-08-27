# MerchantPainting UI 版 — 商人对话框小表情（Mask 立绘）— 架构溯源报告

**文档版本**：v1.0（2026-08-27）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改 Prefab / 代码 / 场景**）  
**Unity**：2020.3.48f1  
**源 Prefab（SR）**：`Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab`  
**UI 样板**：`GoOutStoryYaerPainting.prefab` · `GushaPainting.prefab`  
**挂点目标**：`NormalDialogueNewPanel.prefab` → `Bottom/Mask/YaerAvatarRoot`  
**关联链**：店句 CSV `Face1～5` + 可选 `BodyType` · `UseShopkeeperPortrait` · 场景 `商店界面合层` Toggle  

关联提示词：`Assets/Doc/提示词/0827/MerchantPainting_UI版_商人对话框小表情_架构侦探提示词.md`  
Body/Face CSV：`0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md`（v1.1）  
Mask 接线：`0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`  
Merchant Actor：`0827/Village_ShopStart_新建Merchant对话NPC_架构溯源报告.md`  

---

## ① 结论一句话

**推荐方案 A：新建 UI 专用 `MerchantMaskPainting.prefab`（`Image` + `RectTransform` + `CanvasGroup`，Body/Face 树与 SR 版同构）+ 独立脚本 `MerchantMaskPainting.Apply(body, face)`（不继承 `StoryFormPainting`）；在 `DialogueTMPUGUI` 店句分支于 `ShopkeeperFaceRegistry` 之后直调 `DialogueMaskAvatarPresenter.ApplyShopkeeperPortrait`；保留 SR 版 `MerchantPainting.prefab` 作场景大立绘参考。勿把 SR Prefab 直接塞进 Mask。**

---

## ② 原因（通俗）

### 2.1 SR ≠ UI，不能直接嵌 Mask

| 项 | `MerchantPainting`（SR 源） | `GushaPainting`（UI 样板） |
|----|----------------------------|---------------------------|
| 根组件 | **`Transform` only** | **`RectTransform`** + CanvasRenderer + **CanvasGroup** + 脚本 |
| Layer | **0（Default）** | **5（UI）** |
| 叶子渲染 | **`SpriteRenderer`** | **`Image`** |
| 树结构 | **`Body`**（Normal/Red/YinXian）+ **`Face`**（Face1～5） | **`Clothes`** + **`Faces`**（枚举名） |
| 脚本 | **无** | **`GuShaPainting : StoryFormPainting`** |
| Mask 裁切 | ❌ 不参与 UI Mask | ✅ 在 `YaerAvatarRoot` 下被裁 |

生活类比：SR 版是「贴在场景墙上的海报」，Mask 窗是「对话框里的圆形头像框」——材质和坐标系不同，必须做 UI 副本。

### 2.2 店句现网：大立绘有、小表情无

```
店句 UseShopkeeperPortrait == true
  → ShopkeeperFaceRegistry.Apply(ShopBody, ShopFace)   ✅ 场景合层
  → OnGetNewStatement(DialogueRoleName.None, …)          ★ Mask Presenter 收到 None 后直接 return
  → Mask 窗：空
```

`DialogueMaskAvatarPresenter.Apply` 在 `role == None` 时 **只 HideAll、不亮任何 Painting**；店句 **故意** 不传 `DialogueRoleName`，因为 `DialogueRoleName` 枚举 **无 Shopkeeper**，且店句 **不走** `StoryFormPainting.UpdateFace(string)` 单维链。

### 2.3 为何要 Body×Face 专用脚本

`StoryFormPainting.UpdateFace(string)` 只切 **`Faces` 下一维**；商人要 **Body × Face 两维 Toggle**，与 `ShopkeeperFaceController` 同构。把 Body 硬塞进 `Clothes`、Face 当 `Faces`（方案 B）会导致 Face 键 `Face1` 与 `Laugh/Cry` 混用、Presenter 的 `ResolveFaceKey` 无法复用。

### 2.4 双轨职责分离

| 轨 | 载体 | 驱动 | 本期 |
|----|------|------|------|
| **大立绘** | `Village_Shop` → `商店界面合层` → ` MerchantPainting`/Body/Face | `ShopkeeperFaceRegistry` | ✅ 已有 |
| **小表情（Mask）** | `NormalDialogueNewPanel` → 新 UI `MerchantMaskPainting` | `DialogueMaskAvatarPresenter` 店句分支 | **本期目标** |

两轨读 **同一 CSV 字段**（`ShopBody`/`ShopFace`），Prefab **分离**（SR 场景 vs UI Mask）；**禁止**让 `ShopkeeperFaceRegistry` 同时驱动 Mask。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 打开 `MerchantPainting.prefab`：根仅 `Transform`；叶子全是 `SpriteRenderer` | |
| 2 | 打开 `GushaPainting.prefab`：根有 `RectTransform` + `CanvasGroup` + `GuShaPainting` | |
| 3 | 打开 `NormalDialogueNewPanel` → `Bottom/Mask/YaerAvatarRoot` | 见 GoOut/Yaer/Amy/Aliy/Gusha；**无 Merchant** |
| 4 | `YaerAvatarRoot` 上 **`DialogueMaskAvatarPresenter`** 已挂；`useMaskAvatar=1` | |
| 5 | 施工后：`YaerAvatarRoot` 下新增 **`MerchantMaskPainting` 实例**，默认 **Active=false** | |
| 6 | 店句 Play：Mask 见商人脸；CSV `Face2` + `BodyType=Red` 时 **Mask 与场景合层一致** | |
| 7 | 雅/古句：仍亮对应 Painting；**Merchant 关** | |
| 8 | **DialogDebug** 仅店句：Mask **可测**（无 Registry 时 Console 可有 Warning，但 Mask 仍应亮） | |
| 9 | **`Village_Shop` Play**：Mask + 场景合层 **双轨同步** | |
| 10 | 首句无 GoOut 式 `SetDefault` 盖脸（Merchant 根 **不要** 在 `Start` 擅自 Reset 覆盖 Presenter 调用） | |

**摆位**：Merchant 小表情 Pos/Scale **独立定稿**（勿抄 Gusha 的 `43,-391 / 0.7`）；只在 `NormalDialogueNewPanel` 实例上 override，不改 SR 母体。

---

## ④ 给程序

### A. 源 Prefab 全量结构表（`MerchantPainting.prefab` · SR）

**根 `MerchantPainting`**

| 项 | 值 |
|----|-----|
| 组件 | **`Transform` only** |
| Layer | 0 |
| 默认 Active | ✅ |
| 脚本 | **无** |
| CanvasGroup | **无** |

**子树（默认 Active）**

| 节点 | 组件 | 默认 Active | Sprite guid → 源 PNG |
|------|------|-------------|----------------------|
| **Body** | Transform | ✅ | — |
| Body/**Normal** | Transform + **SpriteRenderer** (order **21**) | ✅ | `384a369d…` → `ArtRes/Scene/Village/商店界面合层/正常体.png` |
| Body/**Red** | SR (order 21) | ❌ | `297d15d7…` → `脸红体.png` |
| Body/**YinXian** | SR (order 21) | ❌ | `6406c966…` → `阴险体.png` |
| **Face** | Transform | ✅ | — |
| Face/**Face1** | SR (order **22**) | ✅ | `e618f45a…` → `表情1.png` |
| Face/**Face2** | SR (order 22) | ❌ | `5c6ba44a…` → `表情2.png` |
| Face/**Face3** | SR (order 22) | ❌ | `2be838e2…` → `表情3.png` |
| Face/**Face4** | SR (order 22) | ❌ | `4763be79…` → `表情4.png` |
| Face/**Face5** | SR (order 22) | ❌ | `775ac4d4…` → `表情5.png` |

**与场景 `Village_Shop.unity` diff（2026-08-27 磁盘）**

```
商店界面合层                    ← ShopkeeperFaceController + DebugInput
├── 背景                        ← SR · SortingOrder 1
└──  MerchantPainting           ← 名前带前导空格；Body/Face 在其下（非合层根直系）
      ├── Body / Normal|Red|YinXian
      └── Face / Face1～5
```

`ShopkeeperFaceController` 用 `transform.Find("Body")` / `Find("Face")` — Unity **递归**查找，故 Controller 挂合层根仍可命中 ` MerchantPainting/Body`。Toggle 逻辑与 Prefab 一致。

**缺失（相对 UI 样板）**：RectTransform · CanvasRenderer · Image · CanvasGroup · 业务脚本。

---

### B. UI 样板对拍（Gusha / GoOut）

**UI 必备组件清单（Mask 内立绘）**

| 组件 | GushaPainting | Merchant UI 版 |
|------|---------------|----------------|
| Layer | **5（UI）** | **5** |
| RectTransform | ✅ 根 + 叶子 | ✅ |
| CanvasRenderer | ✅ | ✅ |
| CanvasGroup | ✅ 根（alpha 淡入兼容） | ✅ 根 |
| Image | ✅ 叶子（`PreserveAspect` 建议开） | ✅ 叶子 |
| 脚本 | `GuShaPainting : StoryFormPainting` | **`MerchantMaskPainting`（独立，不继承）** |

**SR → UI 迁移映射**

| SR 节点 | UI 对应 | Image 源 Sprite |
|---------|---------|-----------------|
| 根 Transform | RectTransform + CanvasGroup + **MerchantMaskPainting** | — |
| Body（空父） | RectTransform 空父 | — |
| Body/Normal | RectTransform + Image | **同 PNG** `正常体.png` |
| Body/Red | Image | **同 PNG** `脸红体.png` |
| Body/YinXian | Image | **同 PNG** `阴险体.png` |
| Face（空父） | RectTransform 空父 | — |
| Face/Face1～5 | Image | **同 PNG** `表情1～5.png` |

**层级**：根下先 **Body** 后 **Face**（Face 后绘制，叠在 Body 上）；与 SR SortingOrder 21/22 语义一致。

**坐标**：SR 的 `localPosition`（世界空间偏移，如 Body `13.54, 5.23`）**不可直接抄**；UI 版在 Mask 282×282 窗内 **重新定 Pos/Scale**（参考 Gusha 调法：大图缩小、下移只露脸）。

---

### C. 脚本架构裁定（核心）

#### C.1 新类名与继承

| 裁定 | 说明 |
|------|------|
| 类名 | **`MerchantMaskPainting`**（优于 `MerchantUIPainting`：与 Mask Presenter 术语一致） |
| 继承 | **不继承** `StoryFormPainting` |
| 命名空间 | 建议 `Game.GameRuntime.UI.FormLogic.Shop`（与 `ShopkeeperFaceController` 同目录） |

#### C.2 API

```csharp
// 与 ShopkeeperFaceController 对齐
public void Apply(ShopkeeperBodyType body, ShopkeeperFaceType face);
public void ResetDefault(); // Normal + Face1，仅 Editor 校正 / 显式调用
```

**不要**实现 `UpdateFace(string)` 接入 Presenter 通用链。

#### C.3 Presenter / DialogueTMPUGUI 改动（推荐）

| 问题 | 裁定 |
|------|------|
| 扩 `DialogueRoleName.Shopkeeper`？ | **本期不必** — 店句仍 `OnGetNewStatement(None,…)` 供历史记录；Mask 走 **专用入口** |
| Presenter 改动 | 新增 **`ApplyShopkeeperPortrait(ShopkeeperBodyType, ShopkeeperFaceType)`**：HideAll（含 Merchant）→ 亮 Merchant → `Apply` |
| `DialogueTMPUGUI` 店句分支 | 在现有 `ShopkeeperFaceRegistry.Apply` **之后**增加：`GetComponentInChildren<DialogueMaskAvatarPresenter>()?.ApplyShopkeeperPortrait(...)` |
| `HideAllPaintings` | 增加 **`merchantMaskPainting` SetActive(false)** |
| 绑定 | `[SerializeField] MerchantMaskPainting merchantMaskPainting` + `Find("MerchantMaskPainting")` 兜底 |

**不推荐**改 `OnGetNewStatement` 事件签名（`NormalDialogueFormNewLogic` 历史订阅会牵连）。

#### C.4 能否复用 `ShopkeeperFaceController`（方案 C）

| 方案 | 说明 | 裁定 |
|------|------|------|
| **A · 独立 `MerchantMaskPainting`** | 复制 Toggle 字典逻辑（~80 行） | **✅ 本期推荐** |
| B · 继承 StoryFormPainting + 魔改 Clothes/Faces | | ❌ Body 无口；Face 键污染 |
| C · 抽 `ShopkeeperToggleHelper` 基类 SR/UI 双后端 | DRY | P2；本期 scope 过大 |

#### C.5 与 `ShopkeeperFaceRegistry` 隔离

| 保证方式 | 说明 |
|----------|------|
| Registry **仅**注册场景 `ShopkeeperFaceController` | 现有设计保持 |
| Mask **仅** `DialogueMaskAvatarPresenter` → `MerchantMaskPainting` | 不 Register 进 Registry |
| 同源数据 | 两轨均读 `SubtitlesRequestInfoEx.ShopBody/ShopFace`，在 **`DialogueTMPUGUI` 同一 `if (UseShopkeeperPortrait)` 块** 内顺序调用 |

---

### D. Prefab 资产策略

| ID | 策略 | 裁定 |
|----|------|------|
| **P1** | 新建 **`MerchantMaskPainting.prefab`（UI）**；保留 **`MerchantPainting.prefab`（SR）** | **✅ 推荐** |
| P2 | SR Prefab 就地改 UI | ❌ 丢场景参考 |
| P3 | 一个 Prefab SR+UI 双子树 | ❌ 维护重 |

**建议路径**

| 资产 | 路径 |
|------|------|
| UI 母体 | `Assets/Prefabs/DialougeProtrait/MerchantMaskPainting.prefab` |
| UI 脚本 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/MerchantMaskPainting.cs` |
| SR 保留 | `Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab`（guid `0a9d5377…`） |

**`NormalDialogueNewPanel` 嵌实例步骤**

1. 在 `Bottom/Mask/YaerAvatarRoot` 下 **Drag** `MerchantMaskPainting.prefab`  
2. 实例名：**`MerchantMaskPainting`**（与 Presenter `Find` 一致）  
3. **默认 Active=false**  
4. Inspector：`DialogueMaskAvatarPresenter.merchantMaskPainting` 拖引用（或依赖自动 Find）  
5. **Pos/Scale 初值思路**：Anchor 居中；Scale **0.15～0.25** 试起（Body 图 8.36×10.47 世界单位很大）；AnchoredPosition 下移让 **脸** 落在 Mask 中心；对照 Gusha `(43,-391)/0.7` 的「只露脸」原则，**数值独立**  

---

### E. CSV / 运行时双轨同步

| 事件 | 场景合层 | Mask UI |
|------|----------|---------|
| 店句 `ShopBody`+`ShopFace` | `ShopkeeperFaceRegistry.Apply` | **`Presenter.ApplyShopkeeperPortrait`** |
| 雅/古句 | 不变 | 不变（`ResolvePainting` + `UpdateFace`） |
| 旁白 / None | 不变 | HideAll |

| 问题 | 裁定 |
|------|------|
| 同一帧两轨必须一致？ | **是** — 同源 `info.ShopBody/ShopFace`，同帧顺序调用 |
| Registry 未注册（DialogDebug） | Console Warning **可保留**；**Mask 仍应显示**（不依赖 Registry） |
| BodyType CSV 空 | 运行时继承上一句 / `Normal`（v1.1 口径）；Mask 与合层 **同一解析结果** |

**Face 键（勿混）**

| 角色 | CSV FaceType | Mask / Toggle 键 |
|------|--------------|------------------|
| 雅/古 | `Laugh`/`Cry`/… | 枚举名 / `Dress_Crown_*` |
| **店** | **`Face1`～`Face5`** | **`Face1`～`Face5`** + Body `Normal/Red/YinXian` |

**禁止**把 `Face1` 追加进 `DialogueFaceType`。

---

### F. 方案对比总表

| 方案 | Prefab | 脚本 | Presenter | 裁定 |
|------|--------|------|-----------|------|
| **A · 专用 MerchantMaskPainting** | 新建 UI Prefab | `Apply(body,face)` 独立类 | 店句专用分支 | **✅ 推荐** |
| B · 继承 StoryFormPainting | 改 Gusha 结构 | `UpdateFace` 单维 | 扩 `ResolvePainting` | ❌ |
| C · Toggle 抽基类 | SR+UI 双后端 | 重构 Controller | 间接 | P2 |
| D · 只做 Prefab 不接代码 | 美术占位 | 无 | 无 | ❌ 无法验收 |

---

### G. 最小施工清单（施工员 · 侦探不执行）

| # | 模块 | 动作 | 必须？ |
|---|------|------|--------|
| 1 | **新建 UI Prefab** | 按 §A/B 表 SR→Image 迁移；Layer=5；默认 Normal+Face1 Active；根 **Active=false** | ✅ |
| 2 | **`MerchantMaskPainting.cs`** | 复制 `ShopkeeperFaceController` Toggle 逻辑；**无 `Start` 自动 Reset**（防首句竞态） | ✅ |
| 3 | **`NormalDialogueNewPanel`** | 嵌 `MerchantMaskPainting` 实例 + Presenter 引用 | ✅ |
| 4 | **`DialogueMaskAvatarPresenter`** | `merchantMaskPainting` 字段 + `ApplyShopkeeperPortrait` + `HideAll` 扩展 | ✅ |
| 5 | **`DialogueTMPUGUI`** | 店句分支调 Presenter（Registry 之后） | ✅ |
| 6 | **DialogDebug** | 挂含店句 CSV 的树；验 Mask **无 Registry 仍亮** | ✅ |
| 7 | **`Village_Shop` Play** | 验 Mask 与合层 **Face2+Red** 等同 | ✅ |

**排除（本期）**：改 `DialogueFaceType`；Registry 驱动 Mask；SR Prefab 直接嵌 Mask；首次进店存档 / 藏 UI / 黑屏。

---

### H. 验收清单（程序自测）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 店句：Mask 窗内见商人脸（非空窗） | |
| 2 | 店句 CSV `Face2` + `BodyType=Red`：Mask 与场景合层 **一致** | |
| 3 | 雅/古句：原 Mask 行为；**Merchant 不亮** | |
| 4 | 店句：Merchant 亮；GoOut/Gusha **关** | |
| 5 | DialogDebug 仅店句：Mask 可测 | |
| 6 | 首句：无默认脸覆盖 CSV 指定脸 | |
| 7 | `HideAll` / 旁白句：Merchant 关 | |

---

### I. 开放问题（待策划/用户拍板）

| # | 问题 | 侦探倾向 |
|---|------|----------|
| 1 | 命名 `MerchantMaskPainting` vs `MerchantUIPainting` | **`MerchantMaskPainting`** |
| 2 | Mask 是否只做 Face、Body 固定 Normal | **建议 3 Body 全做**，与 CSV/合层同步 |
| 3 | 是否需 `DialogueRoleName.Shopkeeper` | **本期不需要** |
| 4 | SR 版 `MerchantPainting` 未来用途 | **保留** — 场景 ` MerchantPainting` 大立绘真源 / 美术参考；**不**进对话 Mask |
| 5 | Toggle 逻辑 DRY（抽 Helper） | **P2**，本期复制可接受 |

---

### J. 相关文件速查

| 用途 | 路径 |
|------|------|
| SR 源 Prefab | `Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab` |
| UI 目标（待建） | `Assets/Prefabs/DialougeProtrait/MerchantMaskPainting.prefab` |
| UI 样板 | `Assets/Prefabs/DialougeProtrait/GushaPainting.prefab` |
| 对话 UI 壳 | `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` |
| Mask Presenter | `Assets/Scripts/.../DialogueMaskAvatarPresenter.cs` |
| 店句入口 | `Assets/Scripts/.../DialogueTMPUGUI.cs` L225–242 |
| 场景 Toggle | `Assets/Scripts/.../Shop/ShopkeeperFaceController.cs` |
| 枚举 | `ShopkeeperBodyType.cs` · `ShopkeeperFaceType.cs` |
| PNG 源 | `Assets/ArtRes/Scene/Village/商店界面合层/*.png` |
| 店 CSV | `Assets/Dialog/Village_商店首次对话.csv` |
| Mask 技术说明 | `Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md` §4 |

---

**报告结束 · 待用户拍板方案 A 后交施工员执行**
