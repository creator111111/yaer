# Cursor Agent Prompt · Village_村长家门口初次对话 Prefab（三人大立绘 + 村长 Face1～3 Import）

> **角色**：先【架构侦探】只读溯源与落地方案，报告后再【施工员】  
> **日期**：2026-08-31  
> **台本 CSV**：`Assets/Dialog/Village_村长家门口初次对话.csv`（Speaker：古 / 村 / 雅；村句 FaceType 已写 **Face1 / Face2 / Face3**）  
> **目标 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab`（Generated → 成品图）  
> **产品硬需求**：  
> 1. 做出门口初次对话 **对话 Prefab**（可 Import + 可 Play）  
> 2. Prefab 要有 **三个角色的大立绘**（雅 + 古 + 村长），不是只 Mask 小窗  
> 3. 村长相关设置补齐：CSV **Face1～3** 能过 Import；大立绘 / Mask 都能跟脸  
> **用户卡点（截图）**：CSV→DialogueTree 报  
> `ID 2 的 FaceType 非法（「Face3」），须为 DialogueFaceType 枚举名`  
> （工具顶栏写 Face1～5，但现网只对 **店行「店」** 分流；**「村」→村长** 仍走 DialogueFaceType 校验 → Face3 被拒）  
> **本阶段（侦探）**：只读；禁止改代码 / Prefab / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品目标（白话）

> 村长家**门口初次对话**：做成完整对话 Prefab；场上同时有 **雅尔、古莎、村长** 三张大立绘；村长用 **Face1/2/3**（与 Mask 三脸一致）。  
> 现在 Import 过不去，村长链路也还没接到这张门口对白上。

### Import 红字根因（预扫 · 须磁盘证伪）

| 层 | 行为 |
|----|------|
| CSV ID2 | Speaker=`村`，FaceType=`Face3` |
| `DialogueCsvParser` | 仅 `ShopkeeperCsvDefaults.IsShopkeeperRow`（店）才允许 Face1～5；**否则** `Enum.TryParse<DialogueFaceType>` |
| 「村」映射 | → Actor「村长」≠「店」→ **Face3 非法** |
| UI 文案 | Import 窗写 Face1～5，易误解为全角色通用 |

**与 Mask 施工现状**：

| 已有（0831） | 缺口（本需求） |
|--------------|----------------|
| `ChiefMaskPainting` + `ChiefFaceType` + Presenter `ApplyChiefPortrait` | CSV 直写 Face1～3 **过不了 Import** |
| `MapToChiefFace(DialogueFaceType)`：CloseEyes→Face2，Smile→Face3… | 门口 CSV **已经**写 Face1/2/3，不是 Smile |
| `DialogueRoleName.Chief`；晚宴 Leader→Chief | **大立绘**三机位 Prefab 未建 |
| `ChiefPainting.prefab` 磁盘有（须核实是否 UI、能否挂场景大立绘） | GraphBuilder 无「村长 ShopFace 式」BB；大立绘谁 `Apply(Face3)` |

### 三人大立绘（钉死）

| 角色 | CSV Speaker | 大立绘倾向 | Mask |
|------|-------------|------------|------|
| 雅尔 | 雅 | `GoOutStoryYaerPainting`（村线铠甲） | 现网 Presenter |
| 古莎 | 古 | `GushaPainting` | 现网 |
| 村长 | 村 | **`ChiefPainting`（UI 大立绘）** 或报告定名 | `ChiefMaskPainting` |

样板：`Village_KenMuNiStart` 仅 **雅+古** 两台 CanvasGroup 淡入。  
本期须 **三台** 进 `DialogueSceneContainer`（或等价挂点），前奏淡入 **三路并行**（勿只抄 KenMuNiStart 两路参考就完事）。

用户 Import 窗「立绘参考 Prefab = Village_KenMuNiStart」→ 侦探须写清：可借前奏壳，但 **必须加第三路村长立绘**，不能只生成双人图。

### Face1～3 导入方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **C1 · 村长行分流（对齐店）** | Parser：Actor「村长」允许 Face1～3 → `ChiefFaceType`；GraphBuilder 写节点 BB / 运行时驱动大立绘+Mask | ✅ 推荐（CSV 已写 Face1～3） |
| C2 · CSV 改回 Smile/CloseEyes，靠 `MapToChiefFace` | 少改 Parser；与现门口 CSV 冲突 | ⚠️ 要改整份 CSV |
| C3 · Face1～3 塞进全局 `DialogueFaceType` | 污染雅/古 | ❌（0831 已否） |

运行时双轨（侦探写清）：

```
村长句 Face3
  → 大立绘 ChiefPainting.Apply(Face3)   // 场景 Dialogue 容器
  → Mask ChiefMaskPainting.Apply(Face3) // Presenter（若仍走 DialogueFaceType，须能从节点拿到 Face3）
```

若节点只存 `DialogueFaceType`，Face3 进不了枚举 → **必须**像店一样存 `ChiefFace` BB，或 TMP 村长分支直读。

### Prefab 制作流水线（施工清单骨架）

1. **修 Import**：村长行认 Face1～3（C1）→ 再导 `Village_村长家门口初次对话` Generated  
2. **大立绘**：确认/完工 `ChiefPainting` UI（Face1 底+Face2/3 贴脸，对齐 Mask 叠法）；挂进对话 Prefab 容器  
3. **成品 Prefab**：三立绘摆位 + Actor 参数（雅尔/古莎/村长）+ RoleName.Chief + 前奏三路淡入（可选对齐 KenMuNi 分层，**勿误伤 Mask**）  
4. **场景挂点**：村长家门口 / `Village_KenMuNi1` 门前谁 `TriggerStory`（侦探定位；可 OPEN）  
5. **验收**：Import 无红字；Play 三立绘同场；村句 Face1/2/3 大+小一致  

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 门口初次对话 Prefab + 三人大立绘 | ❌ 重做晚宴全文 CSV（除非共用 Import 修复） |
| ✅ Import 支持村长 Face1～3 | ❌ Face1 进全局 DialogueFaceType |
| ✅ 村长大立绘跟脸 + Mask 同步 | ❌ 商人 Body×Face 大改 |
| ✅ 前奏/摆位最小可用 | ❌ 强绑必须用 KenMuNiStart 分层黑幕（可选对齐） |

### 严禁

- 为过 Import 把村长 Face3 改成乱填的 Laugh 却不接线  
- 成品 Prefab 只有雅+古、缺村长大立绘  
- SR「精灵村长游戏中立绘」不转 UI 就塞 DialogueSceneContainer  
- Prepare 广扫名字误伤 Mask（0806 KenMuNi 教训）  

### 开放（报告写入）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 门口对白触发场景/物体？ | 侦探扫 Chief 门 / KenMuNi1 |
| Q2 | 三立绘左右站位？ | 产品/美术；施工可先占位 |
| Q3 | 大立绘脚本复用 `ChiefMaskPainting` 还是独立 `ChiefPainting`+同叠法？ | 可抽共用 Apply；Prefab 分离 |
| Q4 | 前奏是否淡入三立绘？ | 倾向是（对齐用户勾选） |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改任何代码、Prefab、CSV、场景。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
1. 落地对话 Prefab：`Village_村长家门口初次对话`（台本 CSV 已有）。
2. Prefab 内要有 **三个角色大立绘**：雅、古、村长。
3. 补齐村长设置：CSV Face1/Face2/Face3 能 Import；大立绘与 Mask 小表情跟脸。
4. 解释并给出修复：Import 报 ID2 FaceType「Face3」非法（须 DialogueFaceType）。

## 必读
@Assets/Dialog/Village_村长家门口初次对话.csv
@Assets/Editor/Tool/Dialogue/DialogueCsvParser.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs
@Assets/Editor/Tool/Dialogue/ShopkeeperCsvDefaults.cs
@Assets/Editor/Tool/Dialogue/DialogueFaceTypeCsvDefaults.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvImportWindow.cs
@Assets/Prefabs/DialougeProtrait/ChiefPainting.prefab
@Assets/Prefabs/DialougeProtrait/ChiefMaskPainting.prefab
@Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab
@Assets/Prefabs/DialougeProtrait/GushaPainting.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/ChiefMaskPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab
@Assets/Doc/施工说明/0831/精灵村长立绘_UI版Mask小表情Face123_施工说明.md
@Assets/Doc/执行文档/0831/精灵村长立绘_UI版Mask小表情Face123_架构溯源报告.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_分层后小头像不显示_架构溯源报告.md

检索：TriggerStory、村长家、House_Chief、门口、Village_村长家门口。

## 侦探任务
1. 钉死 Import 红字调用链；对比店行 Face1～5 与村长行缺口。
2. 盘点 ChiefPainting / ChiefMaskPainting / RoleName.Chief / MapToChiefFace 现状 vs 门口 CSV（已写 Face1～3）。
3. 设计三人大立绘 Prefab 结构（容器、BB CanvasGroup、Actor、前奏三路淡入）；对照 KenMuNiStart 差第三路。
4. 推荐 C1/C2 + 运行时如何让大立绘+Mask 同吃 Face3。
5. 最小施工清单（Import → Generated → 成品 Prefab → 场景触发）+ 验收表 + OPEN。

## 报告落盘
Assets/Doc/执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md

结构：①结论 ②Import 红字 ③三立绘架构 ④Face123 双轨 ⑤与 Mask 0831 关系 ⑥最小清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md

## 目标
1. Import：村长行 Face1/2/3 合法通过（对齐报告 C1）；再导 Village_村长家门口初次对话。
2. 成品对话 Prefab：DialogueSceneContainer（或报告挂点）内 **雅 + 古 + 村长** 三张大立绘；前奏按报告淡入。
3. 村长句：大立绘与 Mask 均按 Face1/2/3 切换（叠法对齐 ChiefMask：Face1 底常亮 + Face2/3 互斥贴脸）。
4. 场景 TriggerStory 挂点按报告；写施工说明。

## 约束
- 勿把 Face1～3 塞进全局 DialogueFaceType。
- 勿 SR 直嵌；勿 Prepare 广扫误伤 Mask。
- 详细注释；施工说明：
  Assets/Doc/施工说明/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_施工说明.md
- 更新 OPEN_QUESTIONS。

## 验收
- [ ] CSV Import 无「Face3 非法」红字，Generated 出盘
- [ ] Prefab 内同时可见/可淡入三张大立绘（雅、古、村）
- [ ] 村句 Face1/2/3：大立绘与 Mask 一致
- [ ] 雅/古句表情仍走 DialogueFaceType，不坏
- [ ] 商人店句 Face1～5 回归 OK
- [ ] Play 从头走到「快进屋」无 Missing / 空窗

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先**跑「侦探 Prompt」（尤其：Import 村长分流 + 三立绘怎么挂）。  
2. 拍板后跑「施工 Prompt」。  
3. Import 前先别指望参考 Prefab=KenMuNiStart 自动变三人——报告会要求 **显式加村长大立绘**。  
4. 相关已有：`ChiefMaskPainting`（小表情）；本需求补 **门口 Prefab + 大立绘 + Face123 导入**。
