# VerdantCorridor 史莱姆吃羊 · 镜头对白时序对齐 — 施工说明

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【施工员】按侦探报告方案 **A** 最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序错乱_架构溯源报告.md`  
**根因**：走廊 Prefab **connections** 把推镜+Wait 放在第一句前、把拉回+Follow 放在 6 句全说完后；与东城郊金标准相反  
**覆盖前案**：前案「定格 Pos2 再说话」施工默认作废，以东城郊序为准

---

## 沟通摘要

### ① 结论一句话

**只重接了走廊对话图连线**：先说「那是……」再推镜停看，拉回 Follow 后再说后面 5 句；刷怪仍是 Action2。

### ② 原因（通俗）

补对白时把 6 句全接在「镜头冲到羊尸」后面，听起来像镜头和台词对不上。东城郊是：玩家这边先惊呼 → 推过去看一眼 → 拉回来再吐槽。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 新档走进走廊触发器 | 第一句出现时镜头 **仍在玩家侧**，尚未到 CameraPos2 |
| 2 | 推镜后约 1s | **无新对白**（纯看点） |
| 3 | 拉回 + Follow 后 | 才出「它们居然在吃…」及后续句 |
| 4 | 黑幕后 | 走廊史莱姆刷出（Mgr2 / Action2） |
| 5 | 抽测东城郊同触发器 | 序未变 |
| 6 | 同存档再进 | SingleUse 不重播 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A** | `Assets/GameRes/Prefabs/Dialogue/VerdantCorridorSlimeEatSheep.prefab` | 只改 `connections`（+ 可选 `_position` 排版） |

### 施工后连线

```
0 Setup
→ 8 「那是……」
→ 1 Cam Pos1→Pos2
→ 2 Wait 1s
→ 3 Cam Pos2→Pos1
→ 4 Follow
→ 9→10→11→12→13 其余台词
→ 5 黑幕
→ 6 Action2("start")
→ 7 收场
```

即：`0→8→1→2→3→4→9→10→11→12→13→5→6→7`

### 未改

- 文案 / Face / audio / VO guid  
- `SlimeEatSheepStoryAction2`（禁止改成 Action1）  
- 东城郊 Prefab / 场景 CameraPos2 / CameraMove 源码 / C#  

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 | 以东城郊 connections 为唯一金标准（覆盖「定格 Pos2 说话」） |
| Q2 | 只改 connections，不改 CameraPos2 |
| Q3 | 标点「那是。。。。」本期不改 |

---

## 与前案施工说明关系

`VerdantCorridor_史莱姆吃羊补对白_施工说明.md` 中「Cam→Pos2 → Wait → 台词 → Cam回」节点链 **已由本案覆盖作废**；台词节点本身保留。
