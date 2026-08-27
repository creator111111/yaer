# Cursor Agent Prompt · Village_ShopStart：新建对话 NPC `Merchant`（老板娘 Actor 接线）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-27  
> **目标 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab`  
> **用户截图（Inspector）**：`Dialogue Tree Controller` → **Dialogue Actor Parameters** 第 2 项 **`老板娘` = None（空）**；已有 **雅尔 / 古莎** 已绑  
> **用户 Hierarchy（截图）**：`Village_ShopStart` 下 **`BG` / `Yaer` / `Gusha`**；**无 Merchant / 老板娘 GO**  
> **关联**：首次进店对白 · 店行 Body/Face Toggle · CSV Speaker **`店`**  
> **本阶段**：只读；禁止改 Prefab / 场景 / 代码

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> 在 **`Village_ShopStart`** 对话预制体里**新建一个对话 NPC**，Hierarchy 名叫 **`Merchant`**（或用户指定英文名），把 Inspector 里空的 **`老板娘` Actor 槽**接上，让 CSV/NodeCanvas 里 **Speaker=店 / Actor=老板娘** 的句能正常播字幕，并驱动 **`Village_Shop` 场景里合层老板娘** 的 Body/Face 切换。

### 现网 Prefab 快照（磁盘 · 2026-08-27）

| 项 | 现网 |
|----|------|
| 路径 | `Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab` |
| 子 GO | `BG`（默认 Active=0）、`Yaer`+`GoOutStoryYaerPainting`、`Gusha`+`GushaPainting` |
| Actor 参数（图内名） | 雅尔、古莎、**老板娘（未绑 GO）** |
| Blackboard 变量 | `GoOutStoryYaerPainting`、`GushaPainting`（CanvasGroup 淡入用） |
| 前奏节点 | 雅/古立绘 CanvasGroup Alpha 淡入（**无老板娘变量**） |

### 老板娘 ≠ 雅/古 Painting 链（钉死）

```
雅尔 / 古莎：
  DialogueActorEx + RoleName
  → 嵌套 GoOutStoryYaerPainting / GushaPainting（对话 Prefab 内 UI 大立绘）
  → RefreshAvatar / Mask 小头像

老板娘（店）：
  CSV Speaker「店」→ Actor「老板娘」（ShopkeeperCsvDefaults）
  → StatementNodeEx.UseShopkeeperPortrait = true
  → SubtitlesRequestInfoEx → ShopkeeperFaceRegistry → 场景「商店界面合层」Body/Face Toggle
  → 不走 DialogueActorEx.RefreshAvatar，不走 StoryFormPainting
```

**推论（侦探须裁定）**：`Merchant` **大概率不是**再嵌一套 `*Painting.prefab` 的雅/古同款结构；而是 **Actor 绑定壳 + 运行时靠场景合层出图**。若复制 Yaer 结构会 **双份立绘 / 位置对不上**。

### 命名三角（必写清）

| 层 | 现网约定 | 用户新名 `Merchant` |
|----|----------|---------------------|
| Hierarchy GO 名 | （缺失） | 用户要 **`Merchant`** |
| NodeCanvas Actor 参数名 | **`老板娘`**（`ShopkeeperCsvDefaults.ShopkeeperActorName`） | **须与 CSV 映射一致**，改 Merchant 则动代码+映射 |
| CSV Speaker 简称 | **`店`** | 一般不改 |
| 字幕条显示名 | `DialogueActor._name` 字段 | 可填「老板娘」中文 |

**开放裁定**：GO 叫 `Merchant`、Actor 参数仍叫 **`老板娘`** 是否 OK？（倾向：**可以**，二者不必同名。）

### 立绘「位置」在哪调（用户常见困惑）

| 角色 | 调位置改哪里 |
|------|--------------|
| 雅/古（Prefab 内大立绘） | `Village_ShopStart` 里 `GoOutStoryYaerPainting` / `GushaPainting` 的 **RectTransform** |
| **老板娘（Merchant）** | **`Village_Shop.unity` → `商店界面合层`** 的 **Transform**（世界空间），**不是**对话 Prefab 里 |
| 字幕条小头像 | `NormalDialogueNewPanel.prefab` → Mask 内各 Painting |

**DialogDebug 局限**：可测雅/古 + 字幕；**店句需 `ShopkeeperFaceRegistry` 注册** → 须在 **`Village_Shop` 场景 Play** 或专门测试场景，不能只在 DialogDebug 里验 Body/Face。

### 须比较的方案

| 方案 | Hierarchy / 组件 | 优点 | 风险 |
|------|-------------------|------|------|
| **A · 轻量 Actor 壳（推荐倾向）** | 新建 `Merchant` GO + `DialogueActorEx`（或最小 `DialogueActor`）；**无 Painting 子节点**；绑到 Actor 参数 **`老板娘`** | 符合现网 `UseShopkeeperPortrait` 链；最小 diff | 无 Prefab 内立绘预览；依赖场景合层 |
| B · 嵌合层 Prefab 实例 | 把 `商店界面合层` 嵌进对话 Prefab | Editor 内可摆位 | 与 `Village_Shop` 双份；0713/0704 双轨冻结；Registry 绑哪份？ |
| C · 完全复制 Yaer 结构 + MerchantPainting | 新建 UGUI Painting | 与 DialogDebug 一致 | **与 Toggle 链重复**；FaceType 分流失效 |
| D · 仅绑空 Actor，不建 GO | 图内 dummy | ❌ NodeCanvas 通常要 Transform 引用 | 运行时 actor 名为空 |

### 严禁（本阶段）

- 把 `ShopkeeperActorName` 改成 `Merchant` 而不评估 CSV/GraphBuilder/全引用  
- 在对话 Prefab 里再嵌一套 `商店界面合层` 当「立绘」  
- 给 Merchant 挂 `GoOutStoryYaerPainting` 并指望 Body/Face CSV 生效  
- 未接 Actor 就 Import CSV（店句 SayEx 会红）  
- 本期扩成首次进店存档 / 藏 UI / 黑屏（若用户未提则写开放问题）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md
@Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/Doc/技术文档/演出相关/DialogDebug对话测试场景_技术说明.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/Editor/Tool/Dialogue/ShopkeeperCsvDefaults.cs
@Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueActorEx.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/StatementNodeEx.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/SubtitlesRequestInfoEx.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceRegistry.cs
@Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改 Prefab / 场景 / 代码 / CSV。只读 + 写溯源报告。

---

## 背景

用户已在 **`Village_ShopStart`** 对话 Prefab 上 Import/Bind 图，Inspector 显示三个 Actor：**古莎、老板娘（空）、雅尔**。  
需求：新建 Hierarchy 节点 **`Merchant`**，完成 **老板娘 Actor 接线**，使首次进店对白中 **店** 的台词能播且驱动场景合层表情。  
用户曾困惑「对话 Prefab 里立绘位置怎么调」——报告须区分 **Merchant vs 雅/古** 的摆位位置。

---

## 侦探任务清单

### A. 核实 Prefab 现网（磁盘 + 与用户截图对拍）

| 项 | 填 |
|----|-----|
| `Village_ShopStart` 完整 Hierarchy | |
| 三个 Actor 参数名与绑定 GO | |
| 已 Import 的 DialogueTree 路径 | |
| 店句 SayEx 是否已设 `UseShopkeeperPortrait` | |
| 前奏 Action 是否只淡入雅/古（无 Merchant） | |

### B. `Merchant` NPC 最小结构裁定（必填推荐方案 A/B/C/D）

1. **Hierarchy 名** `Merchant` vs **Actor 参数名** `老板娘` 是否分离？  
2. **组件清单**：要不要 `DialogueActorEx`？`_roleName` 填什么（现网 `DialogueRoleName` **无 Shopkeeper**）？  
3. **要不要 Painting 子节点**？若不要，Editor 里如何预览老板娘？  
4. **RectTransform**：Merchant 根节点是否仅占位（100×100 像 Yaer/Gusha 父节点）？  
5. **`_name` 字幕显示名**建议（「老板娘」/「店主」）  
6. **Blackboard**：是否**不需要** Merchant CanvasGroup 变量？

### C. Actor 绑定与 CSV 契约

| 检查项 | 现网 | 施工后应有 |
|--------|------|------------|
| `DialogueSpeakerMapping`：`店`→? | 预扫 `老板娘` | |
| `ShopkeeperCsvDefaults.IsShopkeeperActor` | 硬编码 `老板娘` | 改 Merchant 是否动代码？ |
| GraphBuilder 店句节点 | UseShopkeeperPortrait + ShopBody/ShopFace | |
| 未绑 Actor 时 Play 行为 | 字幕名空 / 节点报错？ | |

### D. 运行时双轨（首次进店构图）

0629 文档：左雅/古 + 右老板娘。报告须写清：

| 句 Speaker | 视觉轨 | Prefab / 场景 |
|------------|--------|---------------|
| 雅 / 古 | Mask + 可选 Prefab 大立绘淡入 | Prefab 内 Painting |
| 店 | 合层 Body/Face Toggle | **`Village_Shop` 场景** |

同屏时 **互不干扰** 的验收点。

### E. 立绘位置编辑指南（给用户 § 检查清单）

分别写清：

1. **Merchant/老板娘**：改 **`Village_Shop` → `商店界面合层` Transform**  
2. **雅/古**：改 **`Village_ShopStart` 内 Painting RectTransform**  
3. **小头像**：改 **`NormalDialogueNewPanel`**  
4. **预览路径**：DialogDebug（雅/古） vs **Village_Shop Play**（含店句）

### F. 最小施工清单（给施工员 · 本阶段不执行）

| # | 动作 | 必须？ |
|---|------|--------|
| 1 | 在 `Village_ShopStart` 下新建 **`Merchant`** GO | |
| 2 | 挂 **`DialogueActorEx`**（或裁定组件）并设 `_name` | |
| 3 | Inspector **老板娘 Actor 槽 → 拖 `Merchant`** | |
| 4 | **不要**嵌 Painting（若方案 A） | |
| 5 | Rebind / 保存 Prefab；CSV Import 回归 | |
| 6 | `Village_Shop` 场景确认 `ShopkeeperFaceRegistry` 已注册 | |
| 7 | 首次进店触发链（若已有）挂 `Village_ShopStart` | 开放问题 |

**排除**：改 `ShopkeeperActorName` 为 Merchant（除非报告论证必须且列全引用）。

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Prefab：Actor **老板娘** 不再 None | |
| 2 | NodeCanvas 图：店句 SayEx 无红 Actor | |
| 3 | **Village_Shop Play** 播对白：店句字幕名正确 | |
| 4 | 店句 Body/Face 随 CSV 切换 | |
| 5 | 雅/古句：Mask/大立绘仍正常；不被 Merchant 影响 | |
| 6 | Console：无 `ShopkeeperFaceController 未注册` / NullRef | |

### H. 开放问题

- GO 必须叫 `Merchant` 还是也可 `老板娘`？  
- 是否本期接 **首次进店只播一次** 触发？  
- 店句要不要 **CanvasGroup 淡入**（像雅/古）？默认倾向 **否**（合层常驻可见）  
- 是否在 `DialogueRoleName` 追加 `Shopkeeper`/`Merchant`（本期是否必须）？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/Village_ShopStart_新建Merchant对话NPC_架构溯源报告.md`

结构（MASTER 四段式）：

① 结论一句话（Merchant 怎么建 + Actor 绑谁 + 位置在哪调）  
② 原因（店句走 Registry，不是 Painting 链）  
③ 用户检查清单（Inspector 拖线 + 两个场景怎么测）  
④ 给程序：方案对比 + 最小施工步骤 + 与 0827 Body/Face 报告衔接

口头汇报同样四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/Village_ShopStart_新建Merchant对话NPC_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【施工员】。按报告在 Village_ShopStart 新建 Merchant 并完成老板娘 Actor 绑定。

必须遵守：
- 优先报告推荐方案（倾向轻量 Actor 壳，不嵌合层/不复制 Yaer Painting）；
- Hierarchy 名 Merchant；Actor 参数名保持「老板娘」除非报告要求改代码；
- 店句仍走 UseShopkeeperPortrait + 场景 ShopkeeperFaceRegistry；
- 不改 CSV Speaker 映射、不扩首次进店存档，除非报告明确要求；
- 改完保存 Prefab；Village_Shop Play 冒烟店/雅/古各一句。

提交说明：Inspector 截图描述、绑了哪些组件、验收结果、位置应去哪调。
```
