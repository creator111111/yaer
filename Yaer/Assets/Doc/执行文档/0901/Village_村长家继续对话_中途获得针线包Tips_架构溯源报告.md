# Village_村长家继续对话 — 中途获得针线包 + Tips 横幅 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探】只读核实（**本阶段未改代码 / Prefab / 图集 / CSV**）  
**Unity**：2020.3.48f1  
**产品**：续聊播到村长「我这里有针线包，一会帮你把衣服补一补。」之后 → **入包针线包×1** + **同款 Tips 横幅「获得了针线包」**（Item 音效）→ 再播下一句  
**提示词**：`提示词/0901/Village_村长家继续对话_中途获得针线包Tips_架构侦探提示词.md`  
**样板**：0830 艾琳之剑 / 老农空桶（`GetItem` + `OpenTipsFormActionTask` + `SavePlayerBag`）  
**上游依赖**：续聊能 Play（0901 进屋自动播续聊；成品 Prefab **现已落盘**）

---

## 沟通摘要

### ① 结论一句话

**在续聊 Prefab 锚句（图节点 `$id:36` / CSV ID 34）之后插入：`GetItem(SewingKit,1)` → `OpenTipsForm(GetSewingKit, Item)` → `SavePlayerBag`；须新增枚举/库行/Icon，并把中文源图「获得了针线包.png」覆盖为三语 `GetSewingKit.png` 再 Pack——中文文件名不会被运行时取到。**

### ② 原因（通俗）

入包和花边横幅是两步；横幅字印在图里，要认英文 TipKey。  
美术已经放了「获得了针线包.png」，但游戏去找的是 `GetSewingKit`——不换文件名就静默不弹。  
针线包道具表里还没有，不入库背包也显示不了。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 播续聊至锚句后 | Tips 横幅 **「获得了针线包」**（非血珠/剑错字） |
| 2 | 背包 | 针线包 ×1；读档不丢 |
| 3 | 音效 | 有「获得物品音效」 |
| 4 | 对白 | 横幅后继续 ID 35「古莎，这些天…」，不卡死 |
| 5 | Console | 无「未找到Tips图片：GetSewingKit」 |
| 6 | 回归 | 空桶 / 剑 Tips 仍正常 |
| 7 | 同档 | 续聊只播一次 → 不重复发两份 |

### ④ 程序补充

见下文 §①～§⑩。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 枚举 | **`EMainItemName.SewingKit`**（追加末尾） |
| TipKey | **`GetSewingKit`** |
| 挂法 | 图内 **三连**：`GetItemActionTask` → `OpenTipsFormActionTask(Item)` → `SavePlayerBagActionTask` |
| 锚点 | Statement **`$id:36`**（文案=CSV ID 34）之后、**`$id:37`**（CSV ID 35）之前 |
| Quest | **本期不做** Accept / CollectItem；只入包+Tips；后续可 `targetItem=SewingKit` |
| 英日 Tip 图 | 暂共用中文像素 |
| 续聊 Prefab | ✅ **已落盘**（可直接挂节点；无需再 Setup 壳） |

---

## ② 锚点时序

### 产品期望

```
Village_村长家继续对话
  → Statement「我这里有针线包，一会帮你把衣服补一补。」  // CSV ID 34
  →【本期】GetItem(SewingKit,1) + OpenTipsForm(GetSewingKit, Item) + SavePlayerBag
  → Statement「古莎，这些天就带雅尔在村里住下。」      // CSV ID 35
  → …
```

**禁止**：对白全结束后才发；只入包不弹横幅；新做 Tips UI / TMP 拼字。

### 磁盘核实（成品 Prefab）

| 项 | 值 |
|----|-----|
| Prefab | `Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab` ✅ 存在 |
| 锚句节点 | `$id":"36"` · 村长 · 文案含 `\u9488\u7ebf\u5305`（针线包） |
| 现网边 | **`36 → 37`**（37=下一句「古莎…」） |
| 已有发奖节点 | ❌ 无 `GetItem` / `OpenTipsForm` / `SavePlayerBag` |

Generated：`DialogueTrees/Generated/Village_村长家继续对话.asset` 有同文案；**挂点改成品 Prefab 图**（运行时走 Prefab），Import 勿冲掉手工 Action。

CSV 对照：

| CSV ID | Speaker | Text |
|--------|---------|------|
| 34 | 村 | 我这里有针线包，一会帮你把衣服补一补。 |
| 35 | 村 | 古莎，这些天就带雅尔在村里住下。 |

---

## ③ 道具三态

| 层 | 现状 | 缺口 |
|----|------|------|
| `EMainItemName` | 末项 `FullWaterBucket` | ❌ **无** `SewingKit` |
| `MainItemDatabase.asset` | itemId 0～16（空桶=15 / 满桶=16） | ❌ 无针线包行 |
| `MainItemConfig.json` | 有空/满桶 | ❌ 无针线包（Database 为主源；建议同步补一行防工具漂移） |
| Icon 源图 | ✅ `ArtRes/UI/Item/Icon/针线包_.png`（≈14KB） | 须绑 Database `icon`；必要时入 `MainItem_Icon` 图集 / 依赖 Provider PNG 兜底 |
| 背包类型 | 用户称「任务道具」 | **`BagItemType.TaskItem`（0）**；买卖 **-1**（对齐空桶入库形态） |

**入库样板（空桶）**：枚举追加末尾 → Database 新 entry（displayName / detail / icon / itemType / buy·sell=-1）→（可选）JSON。

**`GetItemActionTask`**：`ItemName` 字符串 = 枚举名（老农：`"EmptyWaterBucket"`）；本期 **`"SewingKit"`**，`Num=1`。

---

## ④ Tips Key / 图

### 契约（0830 钉死 · 勿重发明）

```
TipKey == png 文件名（无扩展名）== tipsInfo*.spriteatlas 内 Sprite 名
OpenTipsForm(TipKey) → 缺图则 LogError「未找到Tips图片」+ 静默不弹
文案在图里，非动态 TMP
ETipsType.Item →「获得物品音效.mp3」
```

### 磁盘现状（2026-09-01）

| 路径 | 状态 |
|------|------|
| `TipInfoAtlas/获得了针线包.png` | ✅ 源图真源（≈29KB，文案正确） |
| `TipInfoAtlas/GetSewingKit.png` | ❌ **不存在** |
| `tipsInfo{,_en,_jp}` 登记 `GetSewingKit` | ❌ **未登记** |
| 中文文件名能否被 OpenTipsForm 取到 | ❌ **不能**（空桶翻车同构） |

### 资源施工（对齐空桶换图）

1. 将「获得了针线包.png」**内容**写入三语：  
   `TipInfoAtlas{,_en,_jp}/GetSewingKit.png`  
2. 新建文件时 Import 对齐 `GetAiLinSword`（Sprite）；若先占位再覆盖则 **保留 .meta guid**  
3. Pack `tipsInfo` / `tipsInfo_en` / `tipsInfo_jp`  
4. 中文源图可留备份或移出 Atlas 目录（防多余 Sprite）  
5. **禁止**把运行时 Key 改成中文「获得了针线包」

| 用户文案 | TipKey | 触发 |
|----------|--------|------|
| 获得了针线包 | **`GetSewingKit`** | 续聊锚句后 `OpenTipsFormActionTask` |

---

## ⑤ 挂点方案

### 推荐（对话内 · 对齐老农）

老农 `Village_老农打水任务` 已验证顺序：

```
… → GetItemActionTask(EmptyWaterBucket, 4)
  → OpenTipsFormActionTask(GetEmptyWaterBucketx4, Item)
  → SavePlayerBagActionTask
  → …
```

本期在续聊 Prefab：

```
Statement $id:36（针线包句）
  → Action: GetItemActionTask(ItemName="SewingKit", Num=1)
  → Action: OpenTipsFormActionTask(TipKey="GetSewingKit", TipsType=Item)
  → Action: SavePlayerBagActionTask
  → Statement $id:37（古莎句）
```

改边：拆掉现网 **`36→37`**，串入三 Action。

### 否决 / 易混

| 做法 | 结果 |
|------|------|
| 只挂 `GetItem` | 有道具、**无**花边横幅 |
| 只挂 `AddTipsInfoActionTask` | 默认 **Info**，无获得物品音效 |
| C# 在 GSM `OnEnterScene` 发奖 | 时机错（应对白中途） |
| TipKey=中文文件名 | 取不到 Sprite → 静默不弹 |

**不改**：TipsPanel UI；空桶/剑 TipKey；晚宴台本 Prefab。

---

## ⑥ 与 Quest 关系

| 问题 | 裁定 |
|------|------|
| 本期是否 Accept / 新建 Quest 行？ | **否** — 先入包+Tips |
| 「任务道具」含义 | 入库 **`TaskItem`**，可被后续 CollectItem 引用 |
| 后续钩子 | 新 Quest 行 `objectiveType=CollectItem`，`targetItem=SewingKit`；交付走既有 `TryTurnInCollectQuest` |

勿把发奖绑死在尚不存在的 QuestId 上。

---

## ⑦ 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `EMainItemName` 追加 **`SewingKit`** | **P0** |
| 2 | `MainItemDatabase` 新行：displayName=针线包、TaskItem、买卖-1、绑 `针线包_.png`；建议同步 JSON | **P0** |
| 3 | 三语写入 **`GetSewingKit.png`**（内容←获得了针线包.png）+ Pack 图集 | **P0** |
| 4 | 续聊 Prefab：`36` 后插入 GetItem → OpenTips → SaveBag → `37` | **P0** |
| 5 | 对拍老农：`SavePlayerBag` 防读档丢物 | **P0** |
| 6 | 验收依赖：进屋自动播续聊已通（另案）；本点也可 DialogDebug 直播 Prefab | — |

**不做**：新 Tips UI；改空桶/剑 Key；晚宴台本；本期建 Quest；Update 堆发奖。

---

## ⑧ 验收清单

- [ ] 锚句后出现 Tips「获得了针线包」（非血珠/非剑）  
- [ ] 背包针线包 ×1；存读档不丢  
- [ ] 有获得物品音效（Item）  
- [ ] 横幅后对白继续「古莎，这些天…」，图不卡死  
- [ ] Console 无「未找到Tips图片：GetSewingKit」  
- [ ] 空桶 / 剑 Tips 回归正常  
- [ ] 同档续聊不重播 → 不重复发两份  

---

## ⑨ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 枚举名？ | **`SewingKit`** | ✅ |
| Q2 | TipKey？ | **`GetSewingKit`** | ✅ |
| Q3 | 本期建 Quest？ | **否**；只入包+Tips | ✅ |
| Q4 | 续聊 Prefab？ | **已存在**，可直接挂 | ✅（相对提示词预扫已更新） |
| Q5 | 英日 Tip 图？ | 暂共用中文像素 | ✅ |
| Q6 | GetItem 后 Save？ | **要** `SavePlayerBagActionTask`（对拍老农） | ✅ |
| Q7 | 中文源图是否移出 Atlas？ | 施工后建议移走防多余 Sprite | ⏳ |

---

## ⑩ 程序补充（速查）

| 锚点 | 用途 |
|------|------|
| `GetItemActionTask` | 入包；`ItemName`=`SewingKit` |
| `OpenTipsFormActionTask` | 横幅；默认 Item |
| `SavePlayerBagActionTask` | 立刻落盘背包 |
| `TipsComponentGSM.OpenTipsForm` | 与剑同入口 |
| `Village_老农打水任务.prefab` | 三连挂点金样 |
| `Village_村长家继续对话.prefab` `$id:36→37` | 本期插入点 |
| `EMainItemName` / `MainItemDatabase` | 道具定义 |
| `ArtRes/.../TipInfoAtlas/获得了针线包.png` | 文案真源（非运行时 Key） |
| `ArtRes/UI/Item/Icon/针线包_.png` | 背包 Icon 真源 |

**与「进屋自动播续聊」关系**：本点改的是续聊**图内发奖**；全链 Play 验需进屋自动 Trigger 已通。缺自动播时可用 DialogDebug / 临时 Trigger 验本点。

**一句话**：空桶怎么发，针线包就怎么发——换枚举、换 TipKey 图、换挂点到续聊锚句后，并强制 Save。
