# WestRappRoad 宝箱不能互动 — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】按侦探报告主因 **F** + 次因 **B**  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/WestRappRoad_宝箱不能互动_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**宝箱已挂回 `Objects` 并降到地面 Y≈−6.61，运行时才能 OnInit、走近才有互动。**

### ② 原因（通俗）

箱子原先放在美术 Near 层里，游戏只扫 `Objects` 下的互动物，等于没报名。另外箱子悬在半空，人和碰撞也对不上。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Hierarchy | `Box` 在 **`Objects`** 下，不在 `Design/Near` |
| 2 | 进西境 Console | 有 `[WestRappRoadHpMpBox][OnInit-…]` |
| 3 | 新档走到 x≈66.6 | 有 E 提示 → 开箱 → HP/MP 球 + Tips |
| 4 | 已开档再进 | 造型开、不可再互动 |
| 5 | 对照告示等 | 其它 Objects 互动仍正常 |

### ④ 程序补充

见下文。

---

## 改动清单

| 文件 | 改动 |
|------|------|
| `WestRappRoad.unity` | PrefabInstance `Box`：`m_TransformParent` Near→**Objects**；从 Near 子列表移除、加入 Objects；**Y −3.44 → −6.61**（对齐 LeftBorn）；RootOrder=22 |
| `WestRappRoadHpMpBox` / Interactive / HomeScene2Box 移除态 | **未改** |
| Interactive 全局 / 村巨树箱 | **未改** |

---

## OPEN

| ID | 默认 |
|----|------|
| Q1 类型 1 | Play 见 OnInit 即坐实 |
| Q2 F+B | **都做了** |
| Q3 Y | **−6.61**；美术可微调 |
| Q4 已开档 | 勿清 `WestRappRoadData_hpMpBoxOpened` 强开 |

---

## 剩余风险

- 离开 Near 的 SortingGroup 后若箱被景挡住，再调 Sorting（本案未改）。  
- 本机未 Play。
