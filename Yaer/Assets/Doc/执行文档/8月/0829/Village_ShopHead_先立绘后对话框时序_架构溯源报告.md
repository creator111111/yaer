# Village_ShopHead — 先雅儿大立绘、后对话框（时序）— 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**目标**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab`

关联提示词：`Assets/Doc/提示词/0829/Village_ShopHead_先立绘后对话框时序_架构侦探提示词.md`  
前序显隐：`0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md`（**本节改拍其 A1 节点序**）  
标准真源：`Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`

---

## ① 结论一句话

**推荐时序方案 T1：在 `FightingPanelVisible` 之后、现网 `NormalDialogueUIAlpha` 之前，串行插入雅儿 `CanvasGroupAlpha` 0→1（`EndActionOnAnimationEnd=true`），再出对话框；明确否决前序「雅儿大立绘」报告 A1「先框后立绘」，并否决 T3/T4。BB 现网已绑 CanvasGroup（相对前序已变），`m_Alpha` 仍为 0、图内仍无立绘淡入——施工只改 Prefab 图序/参数，默认不改 C#。**

---

## ② 原因（通俗）

### 2.1 「跟其它的一样」是什么意思？

技术说明定稿节奏（可跳过黑幕/BG 拍）：

```
【大立绘】→ 空拍 → 【对话框 + Mask】→ 首句
```

不是「对话框先跳出来，立绘后补」。  
Mask 小头像跟**第 2 拍对话框**，**不算**第 1 拍大立绘。

### 2.2 现网 ShopHead 为什么像「先出框」？

现序（磁盘 2026-08-29 再核）：

```
0 FightingPanelVisible
  → 1 NormalDialogueUIAlpha（Duration=1.0，PrepareMask 未开）
    → 首句 Statement…
```

- **没有** `CanvasGroupAlpha(GoOutStoryYaerPainting)`  
- 立绘 override **`m_Alpha: 0`** → 框淡入时立牌仍不可见（或永远看不见）  
- 观感：对话框抢先；立绘缺席/后补  

生活类比：报幕先把字幕条推上来，演员立牌还蒙着黑布。

### 2.3 相对前序「雅儿大立绘」报告的变化与改口

| 项 | 前序报告（当时） | **本期磁盘** | 处置 |
|----|------------------|--------------|------|
| 嵌 GoOut | 已有 | 已有 | 继承 |
| BB 绑定 | ❌ 空 | ✅ `"_value":1` + `_objectReferences` 已绑；Gusha BB 已清 | **继承「已绑」；施工勿再当空** |
| `m_Alpha` | 0 | **仍 0** | 靠淡入拉到 1 |
| 图内雅 Alpha | 无 | **仍无** | 本期插入 |
| 前序 A1：「对话框淡入**后**再加雅 Alpha」 | 曾推荐 | 与产品「先立绘再框」冲突 | **❌ 改拍作废** |

---

## ③ 用户检查清单（肉眼数拍）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Idle 点 Head | **第 1 拍**：左侧雅儿大立绘淡入/出现（尚可无对话框） |
| 2 | 立绘大致落定后 | **第 2 拍**：对话框淡入；Mask 可跟出 |
| 3 | 之后 | 首句可读；店合层表情正常 |
| 4 | 对比体感 | 接近 KenMuNi/ShopStart 的「先立牌后出框」（无进店黑幕） |
| 5 | Console | 无 BB 空引用 / NRE |

施工前预期：框先出或只有框、立绘看不见。

---

## ④ 给程序

### A. 样板对照表（标准序）

| 样板 | 开场节点序（摘要） | 立绘在框前？ | 点头线要抄？ |
|------|-------------------|--------------|--------------|
| **KenMuNiStart**（技术说明） | Fighting → `WaitVillageStartBg` Hold**0.5** → 雅/古 **并行** `CanvasGroupAlpha` Duration**0.5** EndAction → `NormalDialogueUIAlpha` Delay**0.5** Duration**0.5** + PrepareMask → Statement | ✅ | 抄 **立绘→框**；**不要** WaitVillage / 古莎 |
| **ShopStart** | Fighting → `WaitShopStartBg` Hold**0.4** → 雅/古并行 CG Alpha Duration**1.0** →（再）对话框… | ✅ | 抄 **立绘→框**；**不要** WaitShop / 古莎 |
| NewGameStory | 路径不同（黑幕/Mecanim 等） | 分层精神同 | 仅参考，非店内主样板 |

**点头线最小公式（无黑幕）**：

```
FightingPanelVisible
  → 雅 CanvasGroupAlpha 0→1（串行，EndAction=true）
  →（可选空拍 = 对话框 Delay）
  → NormalDialogueUIAlpha（对话框 + 可选 PrepareMask）
  → Statement
```

### B. ShopHead 现序钉死

| 步 | 节点 | 备注 |
|----|------|------|
| 0 | `FightingPanelVisible` | 保留 |
| 1 | `NormalDialogueUIAlpha` Duration=**1.0** | **过早**；PrepareMask 空 |
| 2+ | Statements | 首句起 |

| 检查 | 结论 |
|------|------|
| 雅 `CanvasGroupAlpha` | ❌ 无 |
| BB `GoOutStoryYaerPainting` | ✅ 已绑（fileID `8828290829082908290`） |
| `m_Alpha` override | **0** |

### C. 时序方案拍板

| 方案 | 节点序 | 裁定 |
|------|--------|------|
| **T1 · 串行对齐标准** | Fighting → **雅 CG Alpha** → UIAlpha → Statement | **✅ 推荐** |
| T2 · 并行框 Delay≥立绘 | 易漂 | 次选 |
| T3 · 仅 m_Alpha=1、框仍先淡 | 分层感弱 | ❌ |
| T4 · 先框后立绘（前序 A1） | 与产品冲突 | ❌ **否决** |

**T1 参数建议（对齐 KenMuNi 试调 0.5；店内特殊交互）**

| 节点 | Duration | Delay | EndActionOnAnimationEnd | 其它 |
|------|----------|-------|-------------------------|------|
| 雅 `CanvasGroupAlpha` | **0.5**（备选跟 ShopStart 用 1.0） | 0 | **true**（挡住下一节点） | 仅 Yaer；绑 BB `GoOutStoryYaerPainting` |
| `NormalDialogueUIAlpha` | **0.5**（现网 1.0 可改为 0.5） | **0.5** 空拍（立绘落定→出框） | true | **建议** `PrepareMaskAvatarOnFadeIn`=true，Role=雅，Face 对齐首句前需要（对照 KenMuNi） |

插入位置：

```
现：  Fighting → [UIAlpha] → Statement
目标：Fighting → [雅 CanvasGroupAlpha] → [UIAlpha] → Statement
```

- 把现有 UIAlpha 节点**移到**新立绘节点之后（或删后重建），禁止保留「框在立绘前」的边。  
- **不要** `WaitShopStartBgReveal` / 古莎并行 / 老板娘合层淡入当第 1 拍。  
- 默认 **不改 C#**。

### D. 对前序报告的改口（必读）

划掉 / 取代 `Village_ShopHead_雅儿大立绘_架构溯源报告.md` 中：

| 旧条款 | 新条款 |
|--------|--------|
| A1：对话框淡入**后**加雅 Alpha | **作废** → 雅 Alpha **必须在** UIAlpha **之前** |
| 施工清单「补雅 Alpha」未写顺序 | 必须写明 **T1 串行先立绘后框** |
| Q1 仅「短淡入 vs Alpha=1」 | 补充：即便淡入，也须 **先于对话框**；纯 A2 瞬显仍建议保留串行框 Delay，避免「框动画像抢先」 |

显隐目标不变：立绘须到 alpha=1；BB 现已绑，施工以「插节点 + 参数」为主。

### E. 特殊交互边界

| 问 | 答 |
|----|-----|
| WaitShopStartBgReveal？ | **不要** |
| 古莎并行淡入？ | **不要** |
| 老板娘合层淡入当第 1 拍？ | **不要**（场景已在） |
| 第 1 拍含 Mask？ | **否**；Mask 跟对话框 |

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 确认 BB 仍绑雅 CanvasGroup（已绑则跳过） | P0 核验 |
| 2 | 插入雅 `CanvasGroupAlpha` **于** `NormalDialogueUIAlpha` **之前**，串行 EndAction | **P0** |
| 3 | UIAlpha：Delay≈0.5、Duration≈0.5；建议开 PrepareMask | P0 |
| 4 | 删/避免「先框后立绘」边；勿双份雅淡入 | P0 |
| 5 | 肉眼验收两拍 | P0 |
| 6 | 备注前序报告 A1 改口（本报告即可） | P1 |

**排除**：进店黑幕闸门；嵌 MerchantPainting；改全局 UIAlpha 任务类；点胸（可复用同序一句）。

**预期 diff**：仅 `Village_ShopHead.prefab`（DialogueTree 节点序 + 字段）。

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 点 Head | **先**雅大立绘 |
| 2 | 随后 | **再**对话框 |
| 3 | 出框时 | Mask 可跟；首句可读 |
| 4 | 对比 | 分层感≈ KenMuNi/ShopStart 无黑幕段 |
| 5 | Console | 无 BB 空 / NRE |

### H. 开放问题

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 立绘与对话框空拍 0 / 0.5？ | **Delay=0.5**（对齐 KenMuNi 空拍 B） | 待确认 |
| Q2 | Duration 用 0.5（KenMuNi）还是 1.0（ShopStart/现 UIAlpha）？ | **立绘+框均 0.5** | 待确认 |
| Q3 | 若前序已按错误「先框后立绘」施工？ | **本单纠正为 T1** | ✅ 默认 |
| Q4 | PrepareMask 是否必须开？ | **建议开**（跟其它一样）；关则 Mask 可能晚一拍 | 待确认 |

（已追加 `OPEN_QUESTIONS.md`。）

---

## 附录 · 关键锚点

| 主题 | 路径 |
|------|------|
| 标准节奏 | `技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md` §二～四 |
| 店内样板 | `Village_ShopStart.prefab`（略过 WaitBg） |
| 缺口 Prefab | `Village_ShopHead.prefab` |
| 前序显隐（改口 A1） | `0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md` |
