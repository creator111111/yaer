# Cursor Agent Prompt · 精灵村长立绘 UI 化 + Mask 小表情（Face1/2/3）

> **角色**：先【架构侦探】只读溯源与方案拍板，报告后再【施工员】  
> **日期**：2026-08-31  
> **源 Prefab（现网 · 非 UI）**：`Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘.prefab`  
> **对照样板（已落地）**：商人 `MerchantPainting.prefab`（SR）→ `MerchantMaskPainting.prefab`（UI）+ `MerchantMaskPainting.cs` + Presenter `ApplyShopkeeperPortrait`  
> **挂点目标**：`NormalDialogueNewPanel` → `Bottom/Mask/YaerAvatarRoot`（对话框 **小表情 / Mask 立绘**）  
> **产品规则（钉死 · 表情）**：村长 **只有三个表情**  
> - **Face1** = 默认图（现网子物体「组 2」假说 = 底图）  
> - 打开 **Face2** → 显示 Face2  
> - 打开 **Face3** → 显示 Face3  
> - **Face2、Face3 都不开** → 默认 **Face1**  
> **本阶段（侦探）**：只读；禁止改 Prefab / 代码 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0831/精灵村长立绘_UI版Mask小表情Face123_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末，**须等报告拍板后再开**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> 村长立绘要像商人一样做成 **UI 层**，接到对话框 **小表情（Mask）** 系统。  
> 表情只有三档：**Face1 默认**；要 Face2 就开 Face2；要 Face3 就开 Face3；两个附加脸都关 = Face1。

### 源 Prefab 预扫（`精灵村长游戏中立绘.prefab`）

| 项 | 预扫 |
|----|------|
| 根 | **`Transform` only**，Layer **0**，无 RectTransform / CanvasGroup |
| 叶子 | 全是 **`SpriteRenderer`**（与商人 SR 版同病） |
| 子物体 | **`组 2`**（底图 · SortingOrder 0，假说=**Face1**）、**`Face2`**、**`Face3`** |
| 脚本 | **无** Toggle / StoryFormPainting |
| 推论 | **不能**直接拖进 `YaerAvatarRoot`；须做 UI 副本（Image + RectTransform + Layer UI） |

**命名**：施工倾向把「组 2」**改名 `Face1`**（与商人 Face 树一致），避免脚本按中文乱名找节点。

### 商人对照（须复用模式，勿照搬 Body×5）

| 层 | 商人（已施工） | 村长（本期） |
|----|----------------|--------------|
| SR 源 | `MerchantPainting` | `精灵村长游戏中立绘` |
| UI Mask | `MerchantMaskPainting` | 新建如 **`ChiefMaskPainting`** / `村长MaskPainting`（名侦探定） |
| 结构 | **Body×3 × Face×5** | **仅 Face×3**（无 Body 维） |
| 驱动 | `ApplyShopkeeperPortrait(body, face)` | `ApplyChiefPortrait(face)` 或走 Presenter 角色分支 |
| Registry | 场景合层 `ShopkeeperFaceRegistry` | 村长若暂无场景大立绘合层，**本期可只做 Mask**（报告写清） |

### 小表情链路缺口（预扫）

```
DialogueTMPUGUI.OnGetNewStatement(role, faceType, text)
  → DialogueMaskAvatarPresenter.Apply(role, faceType)
  → ResolvePainting(role) → Yaer / Gusha / Amy / Aliy …
```

| 检查项 | 预扫 |
|--------|------|
| `DialogueRoleName` | **无** Chief / 村长 |
| Speaker「村」 | Import 映射 → Actor「村长」；**Mask 无槽位** |
| 晚宴 CSV `Village_村长家晚宴对白台本.csv` | Speaker=`村`，FaceType 现为 **Smile/Normal/CloseEyes/Sad/Laugh** 等（雅式枚举名） |
| 产品表情键 | **Face1 / Face2 / Face3**（Toggle） |

**冲突**：台本现写 Smile 等，产品只要 Face1～3 —— 侦探必须拍板映射或改 CSV（见开放问题）。

### 表情 Toggle 语义（钉死）

| 台本/API 意图 | Active | 说明 |
|---------------|--------|------|
| Face1 / 默认 / 空 | 仅 **Face1** 开；Face2、Face3 **关** | 「两个都不开就是默认 Face1」= 仍显示 Face1 层，不是全关空白 |
| Face2 | Face2 开；Face1、Face3 关（或 Face1 底+Face2 叠 —— **侦探对 Sprite 叠法拍板**） | 用户说「打开 Face2」 |
| Face3 | Face3 开；Face1、Face2 关（或底+Face3） | 同上 |

**叠法假说**：若 Face2/3 只是「换脸贴图」盖在「组 2」身体上，则 Face1 底可常亮、只互斥 Face2/Face3；若三张都是完整立绘，则三者互斥只亮一张。侦探打开三张 Sprite 裁定。

### 方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · UI Prefab + 专用脚本（对齐商人）** | 新建 Mask Prefab；`Apply(Face1\|2\|3)`；Presenter 增加村长分支 | ✅ 推荐 |
| B · 继承 `StoryFormPainting`，Faces 下挂 Face1/2/3 | 扩 `DialogueRoleName.Chief` + `UpdateFace("Face1")` | 可行；Face 键勿混 Laugh |
| C · SR 源直接嵌 Mask | — | ❌ |
| D · 把 Face1 塞进 `DialogueFaceType` 枚举给全家用 | — | ❌（商人已否决同类做法） |

### CSV / FaceType 策略（开放 · 须报告拍板）

| ID | 策略 | 说明 |
|----|------|------|
| **F1** | 村长句 CSV 改写为 `Face1`/`Face2`/`Face3`，Import 直驱 | 与产品键一致；晚宴旧 Smile 行要改或映射表 |
| F2 | 保留 Smile 等，运行时 **映射表** → Face1/2/3（如 Normal/Smile→Face1，CloseEyes→Face2…） | 少改 CSV；映射须产品确认 |
| F3 | 新列 `ChiefFace` 仿店 `ShopFace` | 偏重；仅当要与 DialogueFaceType 完全隔离 |

助手倾向：**F1 或 F2**；晚宴现网大量 Smile/CloseEyes，若产品未给对照表，报告先列「旧 FaceType → Face?」待确认表，**勿擅自猜完施工**。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 村长立绘 UI 化 + 挂 Mask + 三表情 Toggle | ❌ 商人 Body/Face 大改 |
| ✅ Presenter / TMP 接线能亮村长小表情 | ❌ 把 SR Prefab 直接塞进 Mask |
| ✅ 命名 Face1（原「组 2」）对齐 | ❌ 无产品对照就批量改晚宴 CSV 语义脸 |
| ✅ 技术/施工说明按报告落盘 | ❌ 扩 DialogueFaceType 加入 Face1 污染雅/古 |

### 严禁

- SR 版不转 UI 就嵌 `YaerAvatarRoot`  
- Face2、Face3 同时开导致脏脸  
- 默认态全关导致 Mask 空白（须保证 Face1 可见）  
- Start 里擅自 Reset 盖住 Presenter 首句（对齐商人「无 Start Reset」）  

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改任何代码、Prefab、CSV、场景。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
1. 将 @Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘.prefab 做成与商人一样的 **UI 层**立绘，接到对话框 Mask **小表情**系统。
2. 表情仅三档：Face1=默认图；开 Face2→Face2；开 Face3→Face3；Face2/Face3 都不开→Face1。
3. Speaker「村」/ Actor「村长」说话时 Mask 能显示并切换这三脸。

## 必读（商人样板 + Mask + 台本）
@Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘.prefab
@Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab
@Assets/Prefabs/DialougeProtrait/MerchantMaskPainting.prefab
@Assets/Prefabs/DialougeProtrait/GushaPainting.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/MerchantMaskPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/Static/Enum/Role/RoleName.cs
@Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset
@Assets/Dialog/Village_村长家晚宴对白台本.csv
@Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md
@Assets/Doc/提示词/0827/MerchantPainting_UI版_商人对话框小表情_架构侦探提示词.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab

## 侦探任务
1. 钉死源 Prefab 树：组2 / Face2 / Face3 的 Sprite 关系（互斥完整图 vs 底图+贴脸）。
2. 对照商人：UI Prefab 命名、脚本 API、Presenter 挂点、HideAll 行为。
3. 设计村长 Mask 接入：是否扩 DialogueRoleName、还是仿 ApplyShopkeeperPortrait 专用分支。
4. 解决 CSV FaceType（Smile 等）与产品 Face1/2/3 冲突；给出 F1/F2/F3 推荐与待产品确认映射表。
5. 最小改动清单 + 验收步骤；开放问题写入报告并建议更新 OPEN_QUESTIONS。

## 报告落盘
Assets/Doc/执行文档/0831/精灵村长立绘_UI版Mask小表情Face123_架构溯源报告.md

结构：①结论 ②SR≠UI ③表情 Toggle 语义与叠法 ④Mask 接线方案 ⑤CSV/Face 映射 ⑥最小清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0831/精灵村长立绘_UI版Mask小表情Face123_架构溯源报告.md

## 目标
按侦探报告落地：
1. 新建村长 UI Mask Prefab（RectTransform + Image + CanvasGroup + Layer UI），表情树 Face1/Face2/Face3；「组 2」按报告改名为 Face1。
2. 脚本 Apply：Face1 默认；Face2/Face3 互斥；两附加脸都不开时显示 Face1（禁止空白）。
3. 挂到 NormalDialogueNewPanel/YaerAvatarRoot，默认 Active=false；Presenter / TMP 村长句能驱动小表情。
4. CSV/Face 映射按报告决议执行（改 CSV 或映射表）；勿污染雅/古 DialogueFaceType。
5. 保留原 SR Prefab 作参考或按报告处理；勿 SR 直嵌 Mask。

## 约束
- 对齐商人：无 Start 自动 Reset 盖首句。
- 详细注释；重要修改写原因。
- 施工说明：
  Assets/Doc/施工说明/0831/精灵村长立绘_UI版Mask小表情Face123_施工说明.md
- 同步 OPEN_QUESTIONS。

## 验收
- [ ] Mask 窗村长句可见立绘（非空白）
- [ ] 默认 / Face1 → 仅默认脸
- [ ] Face2 → 开 Face2；Face3 → 开 Face3
- [ ] Face2/3 都关 → Face1
- [ ] 切到雅/古句时村长 Mask 关闭
- [ ] DialogDebug 或晚宴对白可测三脸
- [ ] 商人小表情回归不坏

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先**复制「侦探 Prompt」→ 确认叠法（底+贴脸 vs 三张互斥）和 **Smile→Face?** 映射。  
2. 映射表拍板后，再复制「施工 Prompt」。  
3. 源资源：`Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘.prefab`（现为 SR，须 UI 化）。
