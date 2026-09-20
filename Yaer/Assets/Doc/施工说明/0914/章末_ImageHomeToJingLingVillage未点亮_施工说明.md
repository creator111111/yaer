# 章末 ImageHomeToJingLingVillage 未点亮 — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】按侦探报告方案 **R3（R1+R2）**  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/章末_ImageHomeToJingLingVillage未点亮_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**章末开图前补写 `UnlockRoad(HomeToJingLingVillage)`，路线贴图会随 `ShowUnlockRoad` 点亮；出门 GetMap 保留。**

### ② 原因（通俗）

关卡钮能不能点、路线图亮不亮是两本账。0721 只开了关卡点。开地图会先关掉所有路线图，再按存档里的路一条条打开——章末以前从不写这条路，图就一直灰。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 正规流程到章末出图 | Hierarchy `ImageHomeToJingLingVillage` **非灰 / Active** |
| 2 | Console | `[MapSelect] 章末 UnlockRoad=HomeToJingLingVillage newlyAdded=…` |
| 3 | 存档 / 调试看道路列表 | 含 `HomeToJingLingVillage` |
| 4 | `ButtonJingLingVillage` | 仍可点；点选进村不变（不自动进村） |
| 5 | 再开地图 | 路线仍亮 |

### ④ 程序补充

见下文。

---

## 改动清单

| 文件 | 改动 |
|------|------|
| `ChapterEndFormLogic.cs` | `UnlockChapterEndMapPlace`：在 UnlockPlace 后调用 `UnlockRoad(PlaceName.HomeToJingLingVillage)` + 日志；注释撤销「本期不做 UnlockRoad」 |
| `HomeScene1GoOutStoryCollider` / GetMap | **未改**（R2 保留） |
| `MapFormLogic.ShowUnlockRoad` / Prefab Active / 进村 LoadScene | **未改** |

---

## OPEN

| ID | 默认 |
|----|------|
| Q1 撤销 0721 不做 UnlockRoad | **是** |
| Q2 R3 | **已施工** |
| Q3 PlayerMapData 空 Serialize | 另票；本期章末 R1 兜底 |
| Q4 ImageAllRoad | **否** |

---

## 剩余风险

- 若列表已有仍灰 → 再查 `roadImageDic` 键名（现网不像）。  
- 本机未 Play。
