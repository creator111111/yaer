# Cursor Agent Prompt · Village_ShopHead：先雅儿大立绘、后对话框（对齐现网分层出场）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-29  
> **目标 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab`  
> **产品目标（白话）**：点头对白开场时，出场顺序必须是 **先出雅儿大立绘 → 再出对话框**（含 Mask 小头像跟框），**跟其它正规对白一样**（KenMuNi / NewGame / ShopStart 同类分层），禁止「对话框先跳出来、立绘后补或一直没有」。  
> **前序**：`0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md` 已钉「已嵌、alpha=0、缺拉起」；**本期专攻时序**，纠正前序 A1「对话框淡入后再拉立绘」若与产品冲突则改拍。  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0829/Village_ShopHead_先立绘后对话框时序_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 第 1 拍 | **雅儿大立绘**出现（`GoOutStoryYaerPainting` 可见 / 淡入完成） |
| 第 2 拍 | **对话框**出现（`NormalDialogueUIAlpha` / 字幕条 CanvasGroup） |
| 之后 | 首句 Say（店或雅）正常播；表情跟句 |
| 对标 | 「跟其它的一样」→ 以 **KenMuNi / 技术说明标准** + **ShopStart 店内样板** 为准，写出应对齐的节点序 |
| 不做 | 抄进店黑幕全套（`WaitShopStartBgReveal` / DeferCover）；点头是特殊交互，无换场黑幕分层需求 |

### 标准时序（技术真源 · 须对齐精神）

`Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`：

```
BG（点头线可跳过）→ 空拍 → 【大立绘】→ 空拍 → 【对话框 + Mask】→ 首句
```

NewGame / KenMuNi Prefab 常见串行：

```
… → CanvasGroupAlpha(雅立绘) 0→1
  → NormalDialogueUIAlpha(对话框) Delay+Fade（可 PrepareMask）
  → Statement
```

**产品原话「先立绘再对话框」= 上序，不是反过来。**

### 与前序 0829「雅儿大立绘」报告的关系（重要改口点）

| 前序报告 | 本期 |
|----------|------|
| 缺口：alpha=0、BB 未绑、缺拉起 Action | **继承**：BB 绑定 + 立绘必须能到 alpha=1 |
| 曾写 A1：「**对话框淡入后**再加雅 CanvasGroupAlpha」 | ⚠️ **与本期产品顺序冲突** → 侦探须 **改拍** 为「立绘节点在对话框节点之前」 |
| A2：直接 m_Alpha=1 | 若用 A2，仍须保证 **观感上** 立绘先于/不晚于对话框（或接受「同时可见」是否算「先」——倾向仍要串行淡入） |

### ShopHead 现网假说（预扫 · 须证伪）

现网图头大致：

```
FightingPanelVisible
  → NormalDialogueUIAlpha（对话框）   ← 现网先出框
  → Statements…
```

缺：

```
  → CanvasGroupAlpha(GoOutStoryYaerPainting)  ← 应插在对话框之前
```

BB：前序报告写未绑；若磁盘已施工绑上，以现网为准。

ShopStart 对照（预扫）：`FightingVisible` → `WaitShopStartBg` → **雅/古 CanvasGroupAlpha** →（再）对话框…  
点头线：**不要 WaitShopStart**；只要 **立绘 → 框** 两段。

### 方案候选（侦探必选时序方案）

| 方案 | 节点序 | 裁定倾向 |
|------|--------|----------|
| **T1 · 串行对齐标准** | FightingVisible → **雅 CanvasGroupAlpha 0→1**（EndAction 等结束）→（可选短 Delay/空拍）→ **UIAlpha 对话框** → Statement | **✅ 推荐**（「先…再…」字面满足） |
| **T2 · 并行后出框** | 立绘 Fade 与框 Fade 并行但框 Delay≥立绘 Duration | 可接近；易漂 | 次选 |
| **T3 · 仅改 m_Alpha=1，框仍先淡入** | 立绘瞬显、框后出 | 可能「同时已有立绘」但框动画仍像先出框 | ❌ 不满足「跟其它一样」的分层感 |
| **T4 · 框先、立绘后（前序 A1）** | UIAlpha → 雅 Alpha | ❌ **本期否决** |

参数建议（对齐 KenMuNi/NewGame 常用 0.5，侦探用样板实测钉死）：

| 节点 | Duration | Delay | EndActionOnAnimationEnd |
|------|----------|-------|-------------------------|
| 雅 `CanvasGroupAlpha` | 约 0.5～1.0 | 0 | true（挡住下一节点） |
| 对话框 `NormalDialogueUIAlpha` | 约 0.5～1.0 | 约 0～0.5 空拍 | true |
| PrepareMask | 首句前按样板开/关 | | 对照 KenMuNi/ShopStart |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 钉死 ShopHead 现序 vs 标准序差异表 | ❌ 重做进店黑幕 / WaitShopStart |
| ✅ 拍板节点插入位置与参数 | ❌ 嵌老板娘 Painting |
| ✅ BB 绑定是否仍缺（继承前序） | ❌ 改全局 `NormalDialogueUIAlpha` 任务类（除非样板证明必须且影响面可接受） |
| ✅ 最小 Prefab 改图清单 | ❌ 点胸 Prefab（可写「同序可复用」一句） |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 维持「对话框先于立绘」的施工建议  
- 用 Mask 小头像出现冒充「大立绘先出」  
- 把 ShopStart 的 WaitBg / 古莎并行淡入整包抄进点头线  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_ShopHead.prefab` | 现序真源 |
| `Village_ShopStart.prefab` | 店内立绘→框样板（可忽略 WaitBg） |
| `Village_KenMuNiStart.prefab` | 标准分层 |
| `技术文档/…Village_KenMuNiStart_开场分层显现_技术说明.md` | 产品节奏真源 |
| `0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md` | 显隐/BB；**时序须改拍处写明** |
| `CanvasGroupAlphaActionTask` / `NormalDialogueUIAlphaAnimationTaskAction` | 任务语义 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md
@Assets/Doc/执行文档/0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md
@Assets/Doc/执行文档/0806/NewGameStory_开场分层对齐KenMuNi标准_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写「ShopHead 先立绘后对话框」时序溯源报告。

---

## 背景（策划白话）

1. `Village_ShopHead` 开场要 **先出现雅儿大立绘，再出现对话框**。  
2. 节奏要和项目里其它正规对白（进村 / 开场 / 首次进店）一样，不要对话框抢先。  
3. 前序已说明立绘物体在、alpha 常为 0；本期把 **节点顺序** 钉死，并纠正任何「先框后立绘」的旧建议。

---

## 侦探任务清单

### A. 钉死「其它的一样」标准序（至少 2 个样板）

| 样板 | 开场节点序（Action 名 + 时长） | 立绘在框前？ |
|------|-------------------------------|--------------|
| KenMuNiStart | | |
| ShopStart（可略过 WaitBg） | | |
| （可选）NewGameStory | | |

抽出点头线应遵守的 **最小公式**（不含黑幕）：

```
? → 雅大立绘 Fade → ? → 对话框 Fade → Statement
```

### B. 钉死 ShopHead 现序

列出图头到首句 Statement 的实际节点链；标出：

- 对话框 UIAlpha 在第几步  
- 有无雅 CanvasGroupAlpha；若有，在框前还是框后  
- BB 是否已绑（相对前序报告是否已变）

### C. 时序方案拍板

在预梳理 T1～T4 中选推荐；**必须否决「先框后立绘」**（除非证伪产品原话——不可）。

写清：

1. 插入/移动哪个节点  
2. Duration / Delay / EndAction 建议值（对照样板）  
3. 是否要短空拍（立绘落定→出框）  
4. PrepareMask 是否开  
5. 与前序「雅儿大立绘」报告哪些条款 **改拍**（明确划掉 A1 旧序）  
6. 默认是否改 C#（倾向否）

### D. 与特殊交互边界

| 问 | 答 |
|----|-----|
| 要不要 WaitShopStartBgReveal？ | 点头线默认 **不要** |
| 古莎并行淡入？ | 点头 **不要** |
| 老板娘合层要不要 alpha 淡入？ | 场景已在；默认 **不要** 当「第一拍立绘」 |
| 第一拍「雅儿大立绘」是否含 Mask？ | **否**；Mask 跟对话框第 2 拍 |

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | BB 绑雅 CanvasGroup（若仍空） | P0 |
| 2 | 图：雅 `CanvasGroupAlpha` **置于** `NormalDialogueUIAlpha` **之前**，串行等结束 | **P0** |
| 3 | 参数对齐样板（Duration/Delay/EndAction） | P0 |
| 4 | 删错序节点 / 避免双份淡入 | P0 |
| 5 | 验收时序（肉眼：先立牌后出框） | P0 |
| 6 | 更新/备注前序报告 A1 改口 | P1 |

**排除**：进店黑幕闸门；嵌 MerchantPainting；改全局 UIAlpha 任务（无依据时）。

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 点 Head 开对白 | **先**看到雅儿大立绘淡入/出现 |
| 2 | 立绘大致落定后 | **再**看到对话框淡入 |
| 3 | 对话框出现时 | Mask 小头像可跟出；首句可读 |
| 4 | 对比 ShopStart/KenMuNi | 分层感一致（无黑幕部分） |
| 5 | Console | 无 BB 空引用 / NRE |

### G. 开放问题

- 立绘与对话框之间空拍要 0 / 0.5？  
- Duration 跟 ShopStart 的 1.0 还是 KenMuNi 的 0.5？  
- 前序若已按错误顺序施工，是否本单纠正？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0829/Village_ShopHead_先立绘后对话框时序_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（推荐 T? + 节点序一句话 + 前序 A1 是否改拍）  
② 原因（通俗：现网为何先出框；标准是先立绘）  
③ 用户检查清单（Play 时用眼睛数拍：1 立绘 2 框）  
④ 给程序：样板对照表 + 目标节点链 + 最小 diff + 对前序报告的改口说明

口头汇报同样用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_ShopHead_先立绘后对话框时序_架构溯源报告.md
@Assets/Doc/执行文档/0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab

你现在是【施工员】。只按时序报告把 Village_ShopHead 改成：先雅儿大立绘，后对话框（对齐样板参数）。

必须遵守：
- 串行：雅 CanvasGroupAlpha 完成后再 NormalDialogueUIAlpha；禁止先框后立绘；
- 继承前序：BB 须绑定；不抄 WaitShopStart / 古莎；
- 默认只改 Prefab 图/BB，无报告依据不改 C#；
- 若前序已按错误顺序改过，本单纠正；
- 重要取舍写清原因（含相对前序 A1 的改口）。

提交说明：节点序前后对比、Duration/Delay、如何肉眼验收两拍、未做项。
```
