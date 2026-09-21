# VerdantCorridor 史莱姆吃羊补对白 — 施工说明

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【施工员】按侦探报告方案 **A** 最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊无对白_架构溯源报告.md`  
**根因**：走廊 Dialogue Prefab 从入库起 **0× StatementNodeEx**（空台词壳）；触发/Action2 链通，故「无对话框」

---

## 沟通摘要

### ① 结论一句话

**只改了走廊对话 Prefab：在镜头段后插入东城郊同款 6 句台词，刷怪仍走 Action2；不是文件丢了。**

### ② 原因（通俗）

走廊这条线接线是通的，但对话图里从来没有台词节点，只有推镜和黑幕刷怪。现在把东城郊的 6 句（含三语和语音）接到「镜头完 → 说话 → 黑幕 → 刷怪」上。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 新档走进触发器，看镜头与对白 | **推到羊尸处定格后立刻出对话框**；说完再拉回跟玩家；可见 6 句 |
| 2 | 对白结束后 | 走廊史莱姆出现可打（Mgr2） |
| 3 | 打完 | 死羊/坟墓链不毁 |
| 4 | 同存档再进 | 不重复主对白（SingleUse） |
| 5 | 抽测 `VerdantCorridorFirstEnter` | 其它走廊对白仍正常 |
| 6 | 抽测东城郊吃羊线 | 仍 6 句 + Action1（未误伤） |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A 核心** | `Assets/GameRes/Prefabs/Dialogue/VerdantCorridorSlimeEatSheep.prefab` | 6× Statement；链 **Cam→Pos2 → Wait → 台词×6 → Cam回 → Follow → 黑幕 → Action2** |
| **VO** | 同上 `_boundGraphObjectReferences` | 对齐东城郊 4 条 AudioClip（guid 同 ForestEast）；演员索引改 5/6 |
| **文案** | OPEN Q1 默认 | 照搬东城郊 6 句三语（含「龙城郊外」） |

### 未改（本期禁止 / 不做）

- 场景 `StoryPrefabName`（禁止改成东城郊 Prefab）
- `SlimeEatSheepStoryAction2` → Action1
- `InitSomeEventState` → Mgr2（OPEN Q3 本期否）
- Mgr1/Mgr2 共用存档键拆分（OPEN Q4 另案）
- 东城郊 `ForestEastSceneSlimeEatSheep.prefab`
- 任何 C# 业务代码

### 节点链（施工后 · **时序已被后续案覆盖**）

```
（作废）Cam→Pos2 → Wait → 台词×6 → Cam回 → Follow → 黑幕 → Action2
（现行）见 VerdantCorridor_史莱姆吃羊_镜头对白时序对齐_施工说明.md
         0→「那是」→Cam→Wait→Cam回→Follow→其余台词→黑幕→Action2
```

> **2026-09-13**：曾按「定格 Pos2 说话」改线；产品已钉东城郊金标准，**以时序对齐施工说明为准**。

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 | 与东城郊 **完全同文**（含「龙城郊外」） |
| Q2 | ~~镜头段后插台词~~ → **已被时序对齐案覆盖**：第一句在推镜前，其余在 Follow 后 |
| Q3 | **不**补 InitSomeEventState |
| Q4 | 存档键串台 **另案** |

---

## 验收标准（对齐侦探 §5）

1. 新档走进：有对话框台词（6 句或产品确认句数）  
2. 对白结束后史莱姆可打  
3. 死羊/坟墓链不毁  
4. 同存档再进不重复主对白  
5. 其它走廊对白 / 东城郊吃羊线未误伤  
