# Cursor Agent Prompt · 村长家继续对话中途发「针线包」+ Tips 横幅

> **角色**：先【架构侦探】只读核实资源与挂点，报告后再【施工员】  
> **日期**：2026-09-01  
> **产品设定（钉死）**：  
> 1. 在 **`Village_村长家继续对话`** 进行中  
> 2. 村长说完 **「我这里有针线包，一会帮你把衣服补一补。」** 之后  
> 3. 玩家 **获得任务道具「针线包」**，并弹出与剑/空桶 **同款** Tips 横幅（用户截图 / 源图「获得了针线包」）  
> **流程样板**：艾琳之剑 / 老农空桶 —— **入包 + `OpenTipsForm(TipKey, Item)` 两步**，不是只入包  
> **本阶段（侦探）**：只读；禁止改代码 / Prefab / 图集 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_村长家继续对话_中途获得针线包Tips_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望）

```
Village_村长家继续对话 播到：
  Statement「我这里有针线包，一会帮你把衣服补一补。」（CSV ID 34 · Speaker 村）
  →【本需求】GetItem（针线包 ×1）+ OpenTipsForm(TipKey, ETipsType.Item)
  → TipsPanel 横幅「获得了针线包」+ 获得物品音效
  → 继续下一句「古莎，这些天就带雅尔在村里住下。」（CSV ID 35）…
```

**禁止**理解成：对白全部结束后再发；或只入包不弹横幅；或新做一套 Tips UI。

### 用户视觉（钉死）

| 源 | 内容 |
|----|------|
| 用户截图 | 黑底 + 白字描边 + 紫粉外辉「**获得了针线包**」 |
| 工程已有图 | `ArtRes/.../TipInfoAtlas/获得了针线包.png`（中文文件名） |
| 道具 Icon | `ArtRes/UI/Item/Icon/针线包_.png`（预扫） |

### 现网同款流程（0830 已钉死 · 勿重发明）

```
入包：PlayerBagData.AddMainItem / GetItemActionTask
横幅：TipsComponentGSM.OpenTipsForm(TipKey)  // 默认 Item
字图：TipKey == png 文件名 == tipsInfo*.spriteatlas 内 Sprite 名
音效：ETipsType.Item →「获得物品音效.mp3」
```

| 样板 | TipKey | 挂法 |
|------|--------|------|
| 艾琳之剑 | `GetAiLinSword` | ExecuteFunction / C# 双调 |
| 空桶×4 | `GetEmptyWaterBucketx4` | 对白图 `GetItem` + `OpenTipsFormActionTask` |
| 满桶 | `GetFullWaterBucket` | 井 Logic C# |

**对话内推荐（已有 Action）**

```
Statement（针线包句）
  → GetItemActionTask(针线包枚举, 1)
  → OpenTipsFormActionTask(TipKey, Item)   // A1 已施工，勿再用强制 Info 的 AddTipsInfo
  → 下一 Statement
```

### 易混 / 空桶教训（本期必防）

| 坑 | 真相 |
|----|------|
| 只有中文文件名「获得了针线包.png」 | 运行时按 **英文 TipKey** 取 Sprite → **取不到 = 静默不弹** |
| 只挂 `GetItemActionTask` | 有道具、**无**花边横幅 |
| 只挂 `AddTipsInfoActionTask` | 默认 **Info**，无获得物品音效 |
| 动态 TMP 拼「获得了针线包」 | 现网 **不支持**；字在图里 |

**资源施工倾向（对齐空桶换图）**

| 步骤 | 做法 |
|------|------|
| TipKey 命名 | 倾向 **`GetSewingKit`** 或 **`GetNeedleworkKit`**（侦探拍板一词并全链统一） |
| 正式 png | 三语目录写入 `GetXxx.png`（**内容**来自「获得了针线包.png」）；**保留 .meta guid** |
| 图集 | Pack `tipsInfo` / `_en` / `_jp` |
| 英日 | 可暂共用中文像素（对齐空桶 Q1） |

### 道具定义缺口（预扫）

| 项 | 现状（助手预扫） | 侦探须核实 |
|----|------------------|------------|
| `EMainItemName` | ❌ **无**针线包枚举（末项仍是空/满桶） | 须新增，如 `SewingKit` |
| MainItemDatabase / Icon | Icon 图或已有；库行？ | 补 Def + 绑 `针线包_.png` |
| 任务系统 | 用户称「任务道具」 | 本期是否只入包，还是同步 Accept/`CollectItem` Quest？→ OPEN |
| 续聊 Prefab | 可能仍缺成品壳 | 无 Prefab 则无法挂 Action；依赖「继续对话」Setup |

### 挂点钉死

| 项 | 值 |
|----|-----|
| 对白名 | **`Village_村长家继续对话`** |
| CSV | `Assets/Dialog/Village_村长家继续对话.csv` |
| 锚句 | ID **34**：「我这里有针线包，一会帮你把衣服补一补。」 |
| 插入位置 | **该 Statement 节点之后、ID 35 之前**（图上边序） |
| Generated | `DialogueTrees/Generated/Village_村长家继续对话.asset` |
| 成品 Prefab | `Prefabs/Dialogue/Village_村长家继续对话.prefab`（缺则 P0 先 Setup） |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 锚句后：入包针线包 + Item Tips 横幅 | ❌ 新 Tips UI / 动态拼字大改 |
| ✅ TipKey 英文正式文件 + 三语图集 Pack | ❌ 指望中文文件名被 OpenTipsForm 取到 |
| ✅ 复用 `GetItemActionTask` + `OpenTipsFormActionTask` | ❌ 只入包；❌ 误用 Info 类型 |
| ✅ 枚举 + MainItem 库 + Icon（任务道具可识别） | ❌ 重做村长家整段台本 |
| ✅ 与「进屋自动播续聊」共存（续聊能播才能验本点） | ❌ 改空桶/剑 TipKey |

### 严禁

- 锚句前/对白全结束后才发奖（时机错）  
- TipKey 用中文「获得了针线包」当运行时 Key（空桶翻车复刻）  
- 缺图宣「Tips 坏了」而不查图集  
- 混用晚宴台本 Prefab  
- Update 堆发奖逻辑  

### 开放（侦探须答）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 枚举名 `SewingKit` vs `NeedleworkKit`？ | **`SewingKit`**（短、语义清） |
| Q2 | TipKey？ | **`GetSewingKit`**（与 GetAiLinSword 同型） |
| Q3 | 本期是否新建 Quest 行 / Accept？ | **先只入包+Tips**；正式任务卡另案（用户已说任务道具，报告写清后续钩子） |
| Q4 | 续聊 Prefab 未就绪时？ | **P0 依赖**：先 Setup 续聊壳，再挂 Action；或报告拆两阶段 |
| Q5 | 英日 Tip 图？ | 暂共用中文像素 |
| Q6 | 存档：`GetItem` 后是否须 `SavePlayerBag`？ | 对拍老农空桶施工；缺则补，防读档丢道具 |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab、图集、CSV。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
Village_村长家继续对话中，村长说完
「我这里有针线包，一会帮你把衣服补一补。」之后，
玩家获得任务道具「针线包」，并弹出与艾琳之剑/空桶同款 Tips 横幅「获得了针线包」。

## 必读（同款流程 · 已落地）
@Assets/Doc/执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md
@Assets/Doc/施工说明/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_施工说明.md
@Assets/Doc/执行文档/0830/Village_老农打水_Tips新图替换空桶与满桶_架构溯源报告.md
@Assets/Doc/施工说明/0830/Village_老农打水_Tips新图替换空桶与满桶_施工说明.md
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/UIPanel/OpenTipsFormActionTask.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/GetItemActionTask.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs

## 必读（本期锚点 / 资源）
@Assets/Dialog/Village_村长家继续对话.csv
@Assets/GameRes/DialogueTrees/Generated/Village_村长家继续对话.asset
@Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas/获得了针线包.png
@Assets/ArtRes/UI/Item/Icon/针线包_.png
@Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs
@Assets/Scripts/Game/DataTable/MainItem/MainItemDatabase.cs
（及 DefProvider / 老农 EmptyWaterBucket 入库样板）

检索：GetSewingKit、针线包、GetEmptyWaterBucketx4、OpenTipsFormActionTask、
Village_村长家继续对话、AddMainItem、SavePlayerBag。

用户视觉：横幅「获得了针线包」（紫辉白字）。

## 侦探任务
1. 核实续聊 Prefab 是否存在；锚句在图中的节点位置（CSV ID 34 后）。
2. 核实 EMainItemName / MainItem 库 / Icon：针线包是否已入库；缺口清单。
3. 核实 Tip 图：中文文件名 vs 可被 OpenTipsForm 取到的 Key；三语图集是否已有正式 Key。
4. 拍板：枚举名、TipKey、挂法（GetItem + OpenTipsFormActionTask）、是否本期建 Quest。
5. 对照空桶：写清「覆盖 GetXxx.png + Pack」最小资源步骤；存档是否要 Save。
6. 最小施工清单 + 验收 + OPEN；依赖「进屋自动播续聊」写清。

## 报告落盘
Assets/Doc/执行文档/0901/Village_村长家继续对话_中途获得针线包Tips_架构溯源报告.md

结构：①结论 ②锚点时序 ③道具三态 ④Tips Key/图 ⑤挂点方案 ⑥与 Quest 关系
⑦最小清单 ⑧验收 ⑨OPEN ⑩程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_村长家继续对话_中途获得针线包Tips_架构溯源报告.md

## 目标
1. 在 Village_村长家继续对话 中，锚句
   「我这里有针线包，一会帮你把衣服补一补。」之后：
   入包针线包 + OpenTipsForm(报告 TipKey, Item)。
2. 复用 GetItemActionTask + OpenTipsFormActionTask；禁止新 Tips UI；禁止只入包。
3. 按报告补：EMainItemName / MainItem 库与 Icon、三语 GetXxx.png（内容来自「获得了针线包.png」）并 Pack 图集。
4. 若续聊 Prefab 缺失：按报告先 Setup 再挂节点；勿改晚宴台本。
5. 存档按报告（对拍老农 GetItem 后 Save）。

## 落盘
Assets/Doc/施工说明/0901/Village_村长家继续对话_中途获得针线包Tips_施工说明.md
同步 OPEN_QUESTIONS。

## 验收
- [ ] 播到锚句后出现 Tips 横幅「获得了针线包」（非血珠/非剑错字）
- [ ] 背包有针线包 ×1；读档不丢（若报告要求 Save）
- [ ] 有获得物品音效（Item）
- [ ] 横幅后对白继续 ID 35，不卡死
- [ ] Console 无「未找到Tips图片：{TipKey}」
- [ ] 空桶/剑 Tips 回归正常
- [ ] 同段对白不重复发两份（除非报告允许）

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先跑侦探** → 定 `SewingKit` / `GetSewingKit` 等命名，并确认续聊 Prefab 能否挂点。  
2. 再跑施工：图要用 **英文 TipKey 文件名**（内容拷中文「获得了针线包」），与空桶换图同一套路。  
3. 上游：续聊须能进村长家自动播（0901 进屋续聊提示词）；否则本点 Play 验不到。  
4. 「任务道具」本期倾向 **先入包+Tips**；正式 Quest 卡若需要另开需求。
