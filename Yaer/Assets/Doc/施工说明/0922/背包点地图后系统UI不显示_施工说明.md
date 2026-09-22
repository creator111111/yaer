# 背包点地图后系统 UI 不显示 — 施工说明

**文档版本**：v1.0（2026-09-22）  
**文档性质**：【施工员】按报告 P0 A+B + P1 D  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0922/背包点地图后系统UI不显示_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**关地图后再按 ESC，系统菜单（MenuPanel）必能打开；店内 ESC 仍离店。**

### ② 原因（通俗）

背包点地图会关菜单并锁 ESC（`cantOpenMenu`）。若菜单 Active 位没清干净，或地图已开时二次点击只关了菜单，ESC 会被 `isOpenMenu || cantOpenMenu` 直接挡掉，看起来像「系统 UI 没了」。关图时若 Resume 还卡在音效组件非空，人还会动不了。

### ③ 用户检查清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | ESC 开菜单 → 背包点地图 → **只关地图** → 再 ESC | 菜单显示可操作；无持续 `ESC ignored` |
| 2 | 开图关图 ESC ×3 | 稳定 |
| 3 | 开图 → 点肯姆尼正当进村 | Loading 走完；进村后 ESC 可用 |
| 4 | 未持地图时 ESC/背包 | 正常 |
| 5 | Village_Shop 店内 ESC | **仍离店**，不误开菜单 |
| 6 | 章末出图进村 | 不回潮 |
| 7 | （附图）若仍卡 xx% Loading | 记 OPEN 另票 C |

### ④ 程序补充

见下节。

---

## 改动

| 路径 | 改动 |
|------|------|
| `BagPack/ItemMap.cs` | 关菜单后 `OnMenuActive(false)`；地图已在则关开刷新，禁止只关菜单 |
| `Map/MapFormLogic.cs` | OnClose：`AllowOpenMenu(true)` + 清菜单 Active；Resume **去掉** `commonSfxCpn!=null` 门闩 |

**未改**：章末进村 / Unlock；商店 ESC 离店；Loading 杀残留（附图 C 待 Play 坐实另票）；`InputComponentGSM` 门闩语义。

---

## 为什么这样改

报告主因是 ESC 门闩两旗未清 / 二次点击只关菜单；Resume 不对称为次因。最小动 ItemMap + Map OnClose，不动店锁与换场 Loading 契约。
