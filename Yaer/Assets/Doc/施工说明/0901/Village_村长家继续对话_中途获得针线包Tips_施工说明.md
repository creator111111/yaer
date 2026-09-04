# Village_村长家继续对话 — 中途获得针线包 + Tips — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_村长家继续对话_中途获得针线包Tips_架构溯源报告.md`  
**产品**：续聊锚句「我这里有针线包…」之后 → 入包×1 + Tips「获得了针线包」→ 再播下一句

---

## 沟通摘要

### ① 结论一句话

**已追加 `SewingKit` 入库、三语 `GetSewingKit.png`，并用菜单在续聊 Prefab 锚句后挂 GetItem→Tips→SaveBag；须等 Unity 跑完 Setup/Pack 后验收。**

### ② 原因（通俗）

入包和花边横幅是两步；横幅按英文 TipKey 取图，中文「获得了针线包.png」不会被用到。  
道具表原先没有针线包，不入库背包也显示不了。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Unity：`Tools / Dialogue / Setup Village 续聊针线包奖励`（或等 `Library/ChiefContinueSewingKitSetup.request`） | Console：`[SewingKitSetup] 完成…` |
| 2 | 播续聊至锚句后 | Tips **「获得了针线包」**（非血珠/剑） |
| 3 | 背包 | 针线包 ×1；存读档不丢 |
| 4 | 音效 | 有「获得物品音效」 |
| 5 | 对白 | 横幅后继续「古莎，这些天…」，不卡死 |
| 6 | Console | 无「未找到Tips图片：GetSewingKit」 |
| 7 | 回归 | 空桶 / 剑 Tips 仍正常 |
| 8 | 同档续聊只播一次 → 不重复发两份 | |

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 / 动作 | 说明 |
|---|-------------|------|
| 1 | `EMainItemName.SewingKit` | 枚举追加末尾 |
| 2 | `MainItemDatabase.asset` + `MainItemConfig.json` | 针线包行；TaskItem；买卖 -1；Icon=`针线包_.png` |
| 3 | `TipInfoAtlas{,_en,_jp}/GetSewingKit.png` | 内容←「获得了针线包.png」 |
| 4 | `VillageChiefContinueSewingKitSetupEditor.cs` | 插三连 + Pack tipsInfo* |
| 5 | 续聊 Prefab | 锚句后 GetItem→OpenTips→SaveBag（Setup 写入） |

**未改**：TipsPanel UI；空桶/剑 TipKey；晚宴台本；本期不建 Quest。

---

## ② 图内挂点（对齐老农）

```
Statement「我这里有针线包…」
  → GetItemActionTask(ItemName="SewingKit", Num=1)
  → OpenTipsFormActionTask(TipKey="GetSewingKit", TipsType=Item)
  → SavePlayerBagActionTask
  → Statement「古莎，这些天…」
```

---

## ③ Setup 菜单

| 项 | 值 |
|----|-----|
| 菜单 | `Tools / Dialogue / Setup Village 续聊针线包奖励` |
| 自动 | `Library/ChiefContinueSewingKitSetup.request` |
| 幂等 | 已有 GetSewingKit Tips 则跳过插节点，仍 Pack |

---

## ④ 剩余风险

| 风险 | 处置 |
|------|------|
| Setup 未跑 / 图集未 Pack | Play 仍「未找到Tips图片」→ 先跑菜单 |
| 中文源图仍在 Atlas 目录 | Q7 可移走防多余 Sprite；不影响 Key |
| CSV Re-Import 冲掉手工节点 | **禁止**无脑 Import；丢了再跑本 Setup |
