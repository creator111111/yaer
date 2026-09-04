# Cursor Agent Prompt · Village_ShopHead：点头对白补齐雅儿大立绘

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-29  
> **目标 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab`  
> **产品目标（白话）**：播点头特殊对白时，屏幕上要有 **雅儿的大立绘**（跟句变脸），不能只有对话框小头像 / 或完全没有雅立绘  
> **对照样板**：首次进店 `Village_ShopStart.prefab`（雅 = Prefab 内 `GoOutStoryYaerPainting`；店 = 场景合层，**不**嵌老板娘 Painting）  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 载体 | **`Village_ShopHead` 点头对白**播放期间 |
| 视觉 | **雅儿大立绘**可见（村线 GoOut 甲胄立绘，非仅 Mask 小头像） |
| 表情 | 雅句随 CSV / 图内 `FaceType`（`DialogueFaceType`）切换大立绘脸 |
| 店句 | 仍走场景 `MerchantPainting` 合层 + Mask（**勿**在 Prefab 再嵌老板娘大立绘） |
| 古莎 | 点头台本 **无古莎** → 大立绘 **不需要** Gusha（若 Prefab 残留 BB/节点，写清是否删除） |

### 现网三角色立绘分工（勿混）

| 角色 | 大立绘真源 | 小表情 | 点头线要不要大立绘 |
|------|------------|--------|-------------------|
| **雅儿** | 对话 Prefab 内嵌 **`GoOutStoryYaerPainting`**（Canvas） | Mask Presenter | ✅ **本期要有** |
| **老板娘** | 场景 `商店界面合层/MerchantPainting` | `MerchantMaskPainting` | ✅ 已有合层；Prefab **禁止**再嵌一份 |
| **古莎** | Prefab `GushaPainting` | Mask | ❌ 点头线不需要 |

生活类比：首次进店是「左边两女主立牌 + 右边老板娘海报」；点头线只要「左边雅儿立牌 + 右边老板娘海报」——缺的是 **雅儿立牌有没有嵌进 `Village_ShopHead` 并接线**。

### Prefab 缺口假说（2026-08-29 预扫 · 须证伪）

| 层 | `Village_ShopStart`（样板） | `Village_ShopHead`（预扫） | 侦探任务 |
|----|----------------------------|---------------------------|----------|
| Actor GO | Yaer + Merchant（+Gusha） | Yaer + Merchant；无 Gusha GO | 齐否 |
| 雅大立绘子物体 | Yaer 下嵌 `GoOutStoryYaerPainting` | ✅ **已嵌**同 guid 实例（名 `GoOutStoryYaerPainting`） | 勿误判「完全没嵌」 |
| 默认 Alpha | 进店前 Prepare→0，再图内淡入到 1 | override **`m_Alpha: 0`** | **高概率：嵌了但一直透明** |
| BB `GoOutStoryYaerPainting` | CanvasGroup **已绑定** `_value:1` | 变量名在，JSON **未见 `_value` 绑定** | 淡入 Action 可能绑空 |
| 图内淡入 Action | 有 `CanvasGroupAlpha` 拉雅/古到 1 | 预扫图头节点多为 FightingVisible + UIAlpha；**未见**对雅 Painting 的 Alpha 拉起 | **高概率缺口** |
| 雅句换脸 | `DialogueActorEx.RefreshAvatar` → GoOut 键 | Actor 下已有 Painting；换脸链或通但看不见 | 核实 |
| 店句 | UseShopkeeperPortrait → 合层 | 已有 Merchant Actor | 勿动合层架构 |

> **推论（待证伪）**：用户说「需要有雅儿大立绘」更像是 **已嵌但 alpha=0 且图未淡入 / BB 未绑**，不是从零新建表情系统，也未必缺 Prefab 实例。侦探必须区分：**缺物体 / 缺绑定 / 缺显隐 / 仅 Mask**。禁止空喊「已有 Mask 就算大立绘」。

### 与老板娘合层共存（构图）

0629 / 0827 约定商店构图：

- **右侧**：场景合层老板娘（始终在场景里）  
- **左侧**：Prefab 雅（古）大立绘  

侦探须回答：

1. ShopHead 播时合层是否仍可见？（特殊交互 Hide 的是 `UI_Shop`，**不是**合层——须核实）  
2. 雅大立绘 Rect/Pos/Scale 是否直接复制 ShopStart 的 GoOut 实例？（开放：店内点头是否要微调位置避免挡脸）  
3. 开对白瞬间雅立绘默认 alpha=0 再淡入，还是直接显示？（对照 ShopStart 分层 vs 特殊交互「无进店黑幕」——**特殊交互倾向可直接显或短淡入**，侦探拍板）

### 方案候选（侦探必选）

| 方案 | 做法 | 优点 | 风险 | 倾向 |
|------|------|------|------|------|
| **A · 复用已嵌 GoOut + 绑 BB + 补显隐/淡入** | 确认实例；BB 绑 CanvasGroup；图内 Alpha 0→1（或默认 Alpha=1） | 与 ShopStart 同构；少动层级 | 须理清特殊交互要否淡入 | **✅ 若预扫「已嵌 alpha=0」成立则首选** |
| **A′ · 从 ShopStart 重嵌雅 Painting** | 仅当磁盘实际无实例时 | — | — | 仅缺物体时 |
| **B · 场景里常驻雅大立绘，对白只 Toggle** | 合层旁再挂雅 | 与店合层统一 | 破坏「雅在 Prefab」惯例 | ❌ |
| **C · 只靠 Mask 小头像充大立绘** | — | — | **不满足产品** | ❌ |
| **D · 新建 Dress `YaerPainting`** | — | — | 店内村线应用 GoOut | ❌ |

### 表情 / CSV（挂钩，非本期重做台本）

- 雅脸：`DialogueFaceType` → `Armor_NoHeadWear_{Face}`（GoOut 键规则）  
- 若 ShopHead 图尚未用 `Village_商店点头交互.csv` Import（0829 Head 安装报告已指出图/CSV 不一致）：大立绘接线与 **CSV 重 Import 可同单或分优先级**——侦探在施工清单标明依赖，**勿把缺大立绘误诊成缺枚举**。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 查清 ShopHead 缺什么才能显示雅大立绘 | ❌ 重做老板娘合层 / Mask |
| ✅ 对齐 ShopStart 雅 Painting 最小壳 | ❌ 点胸 Prefab（除非共享壳说明） |
| ✅ 雅句大立绘+Mask 双轨是否已通 | ❌ 扩 DialogueFaceType |
| ✅ 显隐/淡入时序最小方案 | ❌ 重做首次进店分层亮屏全套到点头线 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 把老板娘大立绘嵌进对话 Prefab  
- 宣称 Mask 小头像 = 大立绘已满足  
- 并进 `Village_ShopStart` 同一棵图  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_ShopHead.prefab` | **缺口真源** |
| `Village_ShopStart.prefab` | 雅大立绘嵌法样板 |
| `GoOutStoryYaerPainting.prefab` / `.cs` | 立绘组件与换脸键 |
| `DialogueActorEx` / `RefreshAvatar` | 雅句驱动 |
| `DialogueMaskAvatarPresenter` | Mask 对照（非大立绘） |
| `0827/Village_ShopStart_新建Merchant…` | 店不嵌 Painting 的裁定 |
| `0829/…Head热区安装Village_ShopHead…` | Prefab/CSV 现状 |
| `Village_商店点头交互.csv` | 雅句 Face 列 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0829/Village_Shop_Head热区安装Village_ShopHead对话_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_ShopStart_新建Merchant对话NPC_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店Village_ShopStart联调_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店第二波遗留_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/GoOutStoryYaerPainting.cs
@Assets/Dialog/Village_商店点头交互.csv

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写「Village_ShopHead 雅儿大立绘」溯源报告。

---

## 背景（策划白话）

1. `Village_ShopHead` 是点头特殊对白。  
2. 播的时候屏幕上要有 **雅儿大立绘**（能看见整身/大半身立牌，并能跟句换脸）。  
3. 本阶段只查：Prefab 里有没有嵌立绘、黑板有没有绑、图有没有显隐、跟首次进店差在哪、最小怎么补。

---

## 侦探任务清单

### A. 钉死 ShopHead 现状（大立绘相关）

| 项 | 填 |
|----|-----|
| Hierarchy：Yaer 下是否有 `GoOutStoryYaerPainting`（或其它雅 Painting）？ | |
| BB 变量 `GoOutStoryYaerPainting` / `GushaPainting`：有无、是否绑定 CanvasGroup？ | |
| Actor「雅尔」绑定的 GO / 是否带 Scene Painting？ | |
| 图内是否有 CanvasGroupAlpha / 显隐雅立绘的 Action？ | |
| Play 推论：雅句时屏幕左侧会不会出现大立绘？ | 有 / 无 / 仅 Mask |

对照拍一张「与 ShopStart 差异表」。

### B. 钉死「大立绘」驱动链（雅）

```
雅句 Say
  → DialogueActorEx.RefreshAvatar(FaceType)
  → GoOutStoryYaerPainting.UpdateFace(Armor_NoHeadWear_*)
  →（并行）Mask Presenter 小头像
```

回答：ShopHead 断在哪一环？嵌物体 / BB / Actor / 图未 Import / alpha=0 未拉起？

### C. 方案拍板（A/B/C/D）

推荐方案 + 理由；明确：

1. 嵌哪个 Prefab（GoOut vs Dress）  
2. 是否复制 ShopStart 的 Pos/Scale  
3. 点头线要不要淡入；要不要 Prepare 藏 alpha（特殊交互无进店黑幕）  
4. 无用的 Gusha BB / 引用是否删除  
5. 与「CSV 重 Import 点头台本」的施工顺序

### D. 与老板娘合层 / UI

| 问 | 答 |
|----|-----|
| 点头对白时合层是否保持可见？ | |
| Hide `UI_Shop` 是否误伤雅立绘？ | |
| 雅立绘会否被老板娘合层挡住？ | 层级/排序建议 |

### E. 最小施工清单（本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | ShopHead Prefab 嵌雅 GoOut 大立绘 + BB 绑定 | | **P0** |
| 2 | 显隐/淡入（按 §C） | | P0/P1 |
| 3 | 验收雅句换脸（大立绘+Mask） | | P0 |
| 4 | 清理 Gusha 残留（若有） | | P1 |
| 5 | 依赖：点头 CSV Import（若图未对齐） | | 与 Head 安装报告对齐 |
| 6 | 代码 | 仅当嵌好仍不换脸才动 GoOut/Actor | 默认 **不改代码** |

**排除**：嵌 MerchantPainting；改合层；做 Chest；扩枚举。

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Idle 点 Head 开对白 | 左侧（或约定位）出现 **雅儿大立绘** |
| 2 | 雅句换 FaceType | 大立绘脸变；Mask 同步 |
| 3 | 店句 | 合层老板娘变脸/身；**无**第二份老板娘 Prefab 立绘 |
| 4 | 对白结束 | 雅大立绘随对白卸掉/隐藏；合层回默认（现网 Reset） |
| 5 | Console | 无 Missing Painting / 未绑 BB / Face 键 Warning |

### G. 开放问题

- 点头线雅立绘要否短淡入，还是直接显示？  
- Pos/Scale 是否必须与 ShopStart 像素级一致？  
- Prefab 图与 `Village_商店点头交互.csv` 不一致时，是否本单顺带 Import？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（缺嵌/缺绑/缺显哪个 + 方案 A/B + 要否改代码）  
② 原因（通俗：大立绘 vs Mask vs 老板娘合层；与 ShopStart 差在哪）  
③ 用户检查清单（Play 时左/右各应看到谁）  
④ 给程序：差异表 + 最小 Prefab 步骤 + 文件 diff + 开放问题

口头汇报同样用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab

你现在是【施工员】。只按报告给 `Village_ShopHead` 补上雅儿大立绘最小闭环。

必须遵守：
- 雅用 GoOutStoryYaerPainting（或报告指定载体），禁止用 Mask 冒充大立绘；
- 禁止在对话 Prefab 嵌老板娘 MerchantPainting；
- 优先 Prefab 装配，无报告依据不改表情中枢代码；
- 点头台本 CSV Import 仅当报告列入本期；
- 代码/Prefab 含必要注释或施工说明；重要取舍写清原因。

提交说明：Prefab 改了什么、雅立绘如何显隐/换脸、如何验收、未做项。
```
