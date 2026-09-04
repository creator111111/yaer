# Village_KenMuNi1 — 巨树 2 楼 WalkArea2 宝箱看不见 / 未摆放 — 验收排查报告

**文档版本**：v1.0（2026-09-03）  
**文档性质**：【验收员 + 架构侦探】只读核实；**本文件不施工**  
**Unity**：2020.3.48f1 / C#  
**场景**：`Village_KenMuNi1` · 区 **`VillageWalkArea2`**  
**现象（用户）**：树屋 2 楼 WalkArea2 一带 **看不到可互动宝箱**；感觉 **道具没摆上**（枝干截图为空）  
**产品期望（对齐 0901）**：`Objects/Tree2fHpMpBox`（`Box.prefab` + `VillageKenMuNi1HpMpBox`）在区内可见可互动；开箱 Hp×3+Mp×3 + `GetHpBall`→`GetMpBall`  
**上游**：0901 脚本/Data/Setup **已落地**；本案 = **场景验收失败面**，非从零重开 B1  
**并行**：0903 上楼卡死（DepthGap）——解释「走不到开箱」，**不解释**「磁盘/Hierarchy 有无物体」  
**提示词**：`Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查提示词.md`

---

## 沟通摘要

### ① 结论一句话

**仓库磁盘上宝箱已经摆好了（`Objects/Tree2fHpMpBox` @ `(-152, 41.2, 0)`，在 WalkArea2 形内，村脚本/sceneObjs/去 HomeScene2Box 齐全）——不是「没做过」；用户「看不见」优先是本机场景未同步 / Scene 没 Frame 到箱位 / Game 被合层挡住 / 以及卡住案导致走不到东侧箱旁误判为空。**

### ② 原因（通俗）

箱子其实在巨树 2 楼出口点**东边大约 7 步**的位置，程序和场景文件里都有。  
如果你只盯着脚下那截树枝、或者本机还没打开带箱子的那份场景，就会觉得「没摆」。  
就算 Scene 里能对上箱子，人现在还经常卡在 2 楼走不动（另一案），也到不了箱子旁边去开，更容易觉得「道具没上」。

### ③ 用户检查清单（30 秒 + 验收）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 打开 **磁盘这份** `Village_KenMuNi1` → Hierarchy 搜 **`Tree2fHpMpBox`** | 必须有 |
| 2 | 选中 → **Frame Selected（F）** | 应在枝干平台、约 `(-152, 41.2)`，ExitFrom 东侧 |
| 3 | Inspector | 有 **`VillageKenMuNi1HpMpBox`**（非 Missing）；hp/mp=3；**无** HomeScene2Box / 西境箱 |
| 4 | 父链 | `Objects` Active；箱 Active |
| 5 | 若 Hierarchy **无**此名 | 跑菜单 `Tools / Scene / Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3` 后再搜 |
| 6 | Play：走到箱旁 E/点击 | 开箱 +3/+3 + 双 Tips（若走不动 → 记「Scene 可见、互动阻塞在卡住案」） |
| 7 | 勿改 | `VillageWalkArea2` 点集/尺寸 |

### ④ 程序补充

见下文。施工默认：**本机无箱 → V2 重跑 Setup**；**有箱但 Game 被挡 → V1 抬 Sorting**；**走不到 → 并行修 0903 DepthGap，勿当本案主修**。严禁改 WalkArea2 / 挂西境脚本。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| **磁盘有无箱** | **✅ 有**：PrefabInstance `1154813231`，名 `Tree2fHpMpBox`，父 `Objects`（`1948841490`） |
| **坐标** | local/world **`(-152, 41.2, 0)`**（Setup Preferred）；相对 ExitFrom≈`(-159.34,41.66)` **东约 +7.34 X、略低 0.46 Y** |
| **WalkArea2** | 相对多边形原点 `(-13, 3.7)`，射线法 **PIP=True**（备选 `(-165,40.8)` PIP=**False**，现网未用备选） |
| **组件** | 已 `m_RemovedComponents` 去掉 Prefab `HomeScene2Box`；附加 **`VillageKenMuNi1HpMpBox`**（guid 与 `.meta` 一致 `d9f5a2b3…`）；`hpBallCount/mpBallCount=3`，`useStoryOnOpen=0` |
| **sceneObjs** | ✅ 含 `1154813234`（SceneEntity） |
| **Active** | Objects `m_IsActive=1`；实例未禁用 |
| **Sprite** | Prefab 指向 `Assets/ArtRes/Scene/Home2/宝箱.png`（guid `7edd79c2…`）**存在**，非粉图缺资源 |
| **主因归类** | **非「未摆」**；用户体感 = **H0（本机不同步）∪ H6（视口/卡住误判）∪ 可选 H1（Sorting 被合层盖）** |
| **与卡住案** | **解耦**：DepthGap 解释走不到；**不**解释 Hierarchy/磁盘无物体 |

---

## ② 磁盘钉死证据

### PrefabInstance（节选）

| 字段 | 值 |
|------|-----|
| 源 | `Assets/Prefabs/Box.prefab` guid `3d24231045a0614438f37d7cba4b5649` |
| 名 | `Tree2fHpMpBox` |
| 父 | `Objects` Transform `1948841490`（世界原点；`m_Children` 含 `1154813232`） |
| 坐标 | x=-152, y=41.2, z=0 |
| 移除 | `HomeScene2Box`（fileID `6983132279513824226`） |
| 附加逻辑 | `!u!114 &1154813238` → `VillageKenMuNi1HpMpBox` |

### 脚本 / 存档 / Setup

| 项 | 路径 / 状态 |
|----|-------------|
| 逻辑 | `VillageKenMuNi1HpMpBox.cs`（OnInit 订 Interactive；开箱读 `VillageKenMuNi1Data.tree2fHpMpBoxOpened`） |
| Data | `tree2fHpMpBoxOpened` 字段存在 |
| Setup | `KenMuNi1Tree2fHpMpBoxSetupEditor`；菜单路径与 Preferred 坐标一致 |
| Sorting（Prefab） | `SortingLayer=0`（Default），`SortingOrder=0`，Scale=1 |

---

## ③ 假说表（H0～H7）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H0** | 磁盘有、用户本机场景无 | **可能（用户侧）** | 仓库已有实例；若 Hierarchy 搜不到 → 未拉最新 / 打开了副本 / 未保存覆盖。**侦探侧磁盘 ≠ 用户未摆** |
| **H1** | 物体在但看不见（Sorting/挡） | **可疑加重** | SR Order=0、Default；合层树干大量同层 Order0 → Game 易被盖。Scene **Frame** 仍应看见选中轮廓；若 Frame 有、Game 无 → V1 抬 Order |
| **H2** | 坐标不在 WalkArea2 / 错位 | **❌ 否** | Preferred PIP=True；在 ExitFrom 东侧可辨认 |
| **H3** | Active 假 / 父关掉 | **❌ 否** | Objects Active；无 DepthZone 挂在箱父链上 |
| **H4** | 看得见但不能互动 | **次要 / 依赖卡住案** | sceneObjs✅；脚本订 `onClickInteractiveEvent`。若已开脏档则 Open 态不可点——另验存档。走不到箱旁时互动验收阻塞 |
| **H5** | Missing Script | **❌ 否** | 场景 guid = `VillageKenMuNi1HpMpBox.cs.meta`；字段完整 |
| **H6** | 卡在西侧、箱在东侧 → 误以为没摆 | **✅ 加重** | 箱相对 ExitFrom **+7.34 X**；0903 DepthGap 使人贴在左侧/撕扯；截图「枝干空」可未包含箱位 |
| **H7** | Setup 未跑 / 脏实例 | **❌ 对当前仓库** | 实例与 Setup 规格一致（坐标/去 Home/挂村脚本/sceneObjs） |

---

## ④ 期望 vs 用户体感（分条）

```
磁盘期望：Objects/Tree2fHpMpBox @ (-152,41.2) ∈ WalkArea2，可交互
用户体感：Scene/Play 枝干上空无箱

拆开：
  A. Hierarchy 搜得到 + Frame 能看见精灵
     → 「已摆」成立；体感问题转 H1/H6/卡住
  B. Hierarchy 搜不到
     → H0/H7：拉最新或 V2 跑 Setup（勿重写 B1 架构）
  C. Frame 看得见但 Play 走不到 / 无 E
     → 0903 DepthGap（走不到）± H4（脏档）；非「未摆放」
```

**禁止**用「先修好走路」关闭 A/B 的可见性验收；也禁止用「箱在磁盘」搪塞 C 的互动验收——报告分条即可。

---

## ⑤ 方案倾向

| 方案 | 做法 | 判定 |
|------|------|------|
| **用户手检** | Hierarchy 搜名 + F；确认坐标 | ✅ **先做** |
| **V2** | 本机无箱 → 跑 `Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3`（幂等） | ✅ 若 H0/H7 |
| **V1** | Frame 有、Game 被合层挡 → 抬箱 `SortingOrder`（或专用 SortingLayer）；微挪须保持 OverlapPoint=true | ✅ 若 H1 |
| **V3** | 补 sceneObjs / 清 `tree2fHpMpBoxOpened` 脏档 | ⏳ 仅当 H4 成立（磁盘 sceneObjs 已 OK） |
| **并行** | 0903 F_D1 纵深标尺 → 能走到箱旁再验开箱 | ✅ 互动闭环依赖 |
| **V4** | 重写宝箱 / 改 WalkArea2 腾地 | ❌ |
| **V5** | 挂 `WestRappRoadHpMpBox` | ❌ |

---

## ⑥ 与相关案边界

| 案 | 关系 |
|----|------|
| 0901 WalkArea2 宝箱 HpMp×3 | **本案 = 场景验收**；脚本/Data 保留；只补「看见/本机实例/Sorting」 |
| 0903 上楼卡 WalkArea2 | **并行**；卡住 ≠ 未摆；开箱 Play 验收依赖其修复 |
| 0819 围栏 | 正交 |

---

## ⑦ 验收清单（施工后 / 手检后）

- [ ] Hierarchy 有 `Objects/Tree2fHpMpBox`；Scene Frame 在枝干、约 `(-152, 41.2)`  
- [ ] 世界坐标 OverlapPoint(WalkArea2)=true；相对 ExitFrom 东侧可辨认  
- [ ] 无 Missing Script；无 HomeScene2Box / 西境箱  
- [ ] sceneObjs 含箱 SceneEntity  
- [ ] Play（能走后）：开箱 +3/+3；GetHpBall→GetMpBall；同档不可再开  
- [ ] WalkArea2 多边形未改  
- [ ] Console 无 `[Tree2fBox]` Error  

---

## ⑧ OPEN 建议

| ID | 问题 | 建议 | 状态建议 |
|----|------|------|----------|
| Q1 | 场景箱是否已摆？ | **磁盘已摆**；用户本机须 Hierarchy 核实 | ✅ 本报告 |
| Q2 | 「看不见」主因？ | **H0∪H6（±H1）**；非未开工 | ✅ |
| Q3 | 是否改 WalkArea2？ | **否** | ✅ |
| Q4 | 开箱 Play 验收？ | **依赖 0903 DepthGap 修复后** | ⏳ |
| Q5 | 是否重跑 Setup？ | 仅本机 Hierarchy 无箱时 V2 | 待用户手检 |

同步：0901 宝箱案 OPEN 增「场景实例磁盘已在；可见性/互动验收见 0903」。

---

## ⑨ 程序索引

| 符号 | 路径 |
|------|------|
| 场景实例 | `Village_KenMuNi1.unity` PrefabInstance `&1154813231` |
| Setup | `KenMuNi1Tree2fHpMpBoxSetupEditor.cs` |
| 逻辑 | `VillageKenMuNi1HpMpBox.cs` |
| 存档 | `VillageKenMuNi1Data.tree2fHpMpBoxOpened` |
| Prefab | `Assets/Prefabs/Box.prefab` |
| 精灵 | `Assets/ArtRes/Scene/Home2/宝箱.png` |

**硬禁止**：改 `VillageWalkArea2` 点集；挂西境箱；恢复 HomeScene2Box 当主逻辑；TipKey 用中文；把箱摆到区外。
