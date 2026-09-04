# Village_KenMuNi1 — 巨树 2 楼 WalkArea2 宝箱看不见 / 未摆放 — 施工说明

**文档版本**：v1.0（2026-09-03）  
**文档性质**：【施工员】按验收排查报告最小落地  
**Unity**：2020.3.48f1  
**验收报告**：`执行文档/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查报告.md`  
**上游**：0901 宝箱 B1（脚本/Data/实例已在）

---

## 沟通摘要

### ① 结论一句话

**箱子磁盘本来就有；本期只做 V1——把 `Tree2fHpMpBox` 的 `SortingOrder` 抬到 50，避免合层树干盖住；本机 Hierarchy 无箱再跑 Setup（V2）。**

### ② 原因（通俗）

箱子在 2 楼出口点东边大约 7 步，场景文件里早有。  
以前精灵排序跟树干一样是 0，Game 里容易被挡住；再加若人卡在西侧走不到东侧，更像「没摆」。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Hierarchy 搜 `Tree2fHpMpBox` → Frame（F） | 约 `(-152, 41.2)`，ExitFrom 东侧 |
| 2 | Inspector → SpriteRenderer | `Sorting Order = 50`（非 0） |
| 3 | 有 `VillageKenMuNi1HpMpBox`；无 HomeScene2Box / 西境箱 | |
| 4 | 若 Hierarchy **无**此名 | 跑 `Tools / Scene / Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3` |
| 5 | Play：能走到后 E/点击开箱 | +3/+3 + GetHpBall→GetMpBall |
| 6 | 勿改 | `VillageWalkArea2` 点集 |

开箱走不动 → 先看 0903 DepthGap 施工是否已生效，勿当本案「未摆」。

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **V1** | `Village_KenMuNi1.unity` PrefabInstance `Tree2fHpMpBox` | 覆写 `m_SortingOrder=50`（SpriteRenderer `…4222`） |
| **V1 Setup** | `KenMuNi1Tree2fHpMpBoxSetupEditor` | 摆箱时 `ApplyVisibleSortingOrder`；日志带 Order |

**未改**：`VillageWalkArea2` 多边形；`Box.prefab` 全局默认 Order；坐标 `(-152,41.2)`；村脚本/Data；挂西境箱。

---

## Sorting 取值

| 项 | 值 | 理由 |
|----|-----|------|
| Layer | Default（0） | 与 Prefab 一致，不另开 Layer |
| Order | **50** | 附近合层最高约 35；只抬实例，不动全局 Prefab |

**替代方案（否决）**：专用 SortingLayer——要动 Project Settings，面更大。

---

## 验收对照（报告 §⑦）

- [ ] Hierarchy 有箱；Frame 在枝干东侧  
- [ ] Game 可见（Order=50）  
- [ ] OverlapPoint(WalkArea2)=true  
- [ ] 无 Missing；无 Home/西境箱  
- [ ] 能走后开箱 +3/+3  
- [ ] WalkArea2 未改  
- [ ] 无 `[Tree2fBox]` Error  
