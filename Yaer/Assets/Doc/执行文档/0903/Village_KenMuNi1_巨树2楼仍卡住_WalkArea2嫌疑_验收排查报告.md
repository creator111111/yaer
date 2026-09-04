# Village_KenMuNi1 — 巨树 2 楼仍卡住 · WalkArea2 嫌疑 — 验收排查报告

**文档版本**：v1.0（2026-09-03）  
**文档性质**：【验收员】磁盘核实施工是否落地 + 回答「是不是 WalkArea2 坏了」；**本阶段无 Play 录屏，Play 项交用户 30 秒自检**；**禁止改 WalkArea2 点集**  
**Unity**：2020.3.48f1  
**用户原话**：「上来树屋 2 楼之后还是动不了，难道是 VillageWalkArea2 有问题吗？」  
**上游**：  
- 侦探：`执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md`（主因 **DepthGap**，非形状）  
- 施工：`施工说明/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_施工说明.md`（F_D1/F_D2/F_Order）  
**提示词**：`Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查提示词.md`

---

## 沟通摘要

### ① 结论一句话

**不是 VillageWalkArea2 画坏了——主因仍是「能站多高」的尺子曾只有 8、和 2 楼绿线框打架；仓库磁盘已摆标尺 Max=46 且楼梯路径会抬 Max+绑 WalkArea2+Teleport，形状点集未动。你若修后仍卡，先看本机有没有带上这些改动、Console 有没有 `[Village2f]`。**

### ② 原因（通俗）

绿线框只规定「脚要踩在这块地板上」，本身没坏（出口点仍在框里面）。  
真正把人焊死的是另一把尺子：人最高只能站到 Y=8，而 2 楼在 Y≈40——地板往上吸、尺子往下压。  
施工是把尺子抬到 46，并保证上楼时先绑 2 楼地板再传送，**不是去改绿线框形状**。  
若你本机场景里还搜不到 `VillageDepthY_Max`，或 Play 时 Max 仍是 8，就会觉得「修了还是动不了」。

### ③ 用户检查清单（30 秒 · 必做）

| # | 操作 | 通过 = 施工在本机生效 |
|---|------|------------------------|
| 1 | 打开 **磁盘这份** `Village_KenMuNi1` → Hierarchy 搜 **`VillageDepthY_Max`** | 有；Position.y ≈ **46**（同搜 Min ≈ **−20**） |
| 2 | 若无标尺 | 跑菜单 `Tools / Scene / Setup KenMuNi1 巨树纵深标尺 DepthY` → 保存场景 |
| 3 | Play：村长家楼梯上楼 | Console 过滤 `[Village2f]`：有 **`depthYMax→`** 且 **`已 SetVillageWalkAreaOverride(VillageWalkArea2)`** |
| 4 | Pause | 脚距 `ExitFrom_HomeSceneChief2f`（现网约 **(-157.65, 41.66)**）很小；区内 A/D+W/S 可走 |
| 5 | 若仍无两条日志 / Max 仍 8 | **先同步仓库场景与脚本**，勿改 WalkArea2 |
| 6 | 若两条日志都有、Max≥45 仍卡死 | 记现象 → A4（障碍/输入锁）；**仍禁止先改多边形** |

### ④ 程序补充

见下文。磁盘验收：**A1（仓库）否**（标尺已在）、**A5 否**（形状未改且 PIP 真）、**A2/A3 代码路径已就绪**——Play 证据由用户补；仍卡且日志齐全再开 A4。

---

## ① 正面回答：「是不是 WalkArea2 有问题？」

| 问法 | 答案 |
|------|------|
| **绿线框（多边形）画错/坏了，所以走不动？** | **否（主因不是它）**。ExitFrom 相对 WalkArea2 **PIP=True**；点集与 0903 侦探报告一致；改形状是 **严禁主修** |
| **为什么感觉「在 WalkArea2 里动不了」？** | Override 绑上 2 楼区后，若 **`depthYMax` 仍≈8**，会与 ClosestPoint **每帧撕扯**——体感像区坏了，实为 **DepthGap** |
| **那还要不要改 WalkArea2？** | **不要**。先确认标尺注入与 `[Village2f]` 日志 |

---

## ② 磁盘验收（相对施工说明）

### A1 · F_D1 标尺是否在仓库场景

| 物体 | 父 | local Y | Active | 裁定 |
|------|-----|---------|--------|------|
| `VillageDepthY_Min` | `Map`（世界≈0） | **−20** | 1 | ✅ |
| `VillageDepthY_Max` | 同 | **46** | 1 | ✅ |

→ **仓库 A1 = 否（标尺已落地）**。用户本机若 Hierarchy 无 → **本机 A1**（未同步 / 开了旧场景）→ 跑 Setup 菜单。

Editor：`KenMuNi1VillageDepthYSetupEditor.cs` 存在（幂等菜单）。

### A2 / A3 · 楼梯路径代码是否具备抬 Max + W1

`Village_KenMuNiSceneManager.TryApplyChiefStairsLandingToTree2f`（`LastScene==Village_Chief_House`）：

1. `ExpandDepthYMaxForWalkArea2` → `SetDepthYBounds` + 日志 **`[Village2f] depthYMax→…`**（F_D2）  
2. `SetVillageWalkAreaOverride(WalkArea2)` + **`已 SetVillageWalkAreaOverride`**（W1）  
3. `TeleportAuthoritativeVillagePos(ExitFrom)`（F_Order）  

`PlayerLogic.TryInjectVillageDepthYBoundsFromSceneMarkers`：有 Min+Max 时写入场景标尺（F_D1 进村即注）。

→ **磁盘上 A2/A3「代码缺失」= 否**。Play 时若无日志 / Max 仍 8 → 运行时 A2/A3（路径未走楼梯键、旧 DLL、场景未带标尺）。

### A5 · WalkArea2 形状是否被当成锅

| 项 | 磁盘 |
|----|------|
| 位 | `(-139, 37.5, 0)`（未改） |
| 点集 | 与侦探案一致（15 点路径未改） |
| ExitFrom2f | 现 **`(-157.65, 41.66, 0)`**（相对旧 −159.34 略东移）；相对区 **PIP=True** |
| 结论 | **A5 否**；禁止以「仍卡」为由改点集 |

### A4 / A6 · 本阶段

| ID | 磁盘能证？ | 说明 |
|----|------------|------|
| **A4** 障碍/输入锁 | ⏳ 需 Play | 仅当 A1～A3 Play 已通过仍卡再查；进层对白 Prefab **尚未建**，暂非主疑 |
| **A6** 非楼梯键 | ⏳ 需 Play | 须 `LastScene=Village_Chief_House`；读档脏坐标不走 F_D2 |

---

## ③ 假说表（修后仍卡）

| ID | 假说 | 磁盘裁定 | Play 证伪 |
|----|------|----------|-----------|
| **A1** | 本机无标尺 | 仓库 **有**；用户机未知 | Hierarchy 搜 `VillageDepthY_Max` y=46 |
| **A2** | 运行时 Max 仍 8 | 代码会抬 Max | `depthYMax→`；`DebugDepthYMaxWorld`≥45 |
| **A3** | W1 未绑 | 代码会 Override | `已 SetVillageWalkAreaOverride` |
| **A4** | 障碍/对白/黑幕锁 | 未证 | 日志齐全仍卡 → 0819/输入 |
| **A5** | 形状真坏 | **否** | PIP/点集未变；禁改形状 |
| **A6** | 未走楼梯键 | 门闩清晰 | LastScene / 是否楼梯进场 |

---

## ④ 若 Play 仍卡 · 下一修（默认仍不改形状）

| Play 结果 | 动作 |
|-----------|------|
| Hierarchy 无 DepthY | **A1 补**：跑 DepthY Setup；保存；重进 |
| 无 `[Village2f]` 两条 | 确认楼梯路径；确认脚本已编译；禁改 WalkArea2 |
| 有日志、Max≥45、脚在 ExitFrom，仍 vx=0 | **A4**：障碍 Overlap / `isTalking` / 黑幕；可加 `[Village2fMove]` 帧日志 |
| 一切过仍贴边焊死 | 新假说进 OPEN；**仍优先微挪落点/障碍**，最后才谈点集（须另案产品批准） |

---

## ⑤ 与相关案边界

| 案 | 关系 |
|----|------|
| 0903 DepthGap 侦探/施工 | 本案 = **修后复测**；结论延续「非形状」 |
| 宝箱看不见 / 进层对白 | **正交**；勿混修 |
| 0819 围栏 | 仅 A4 成立时参考 |

---

## ⑥ 验收状态建议（OPEN）

| 项 | 建议 |
|----|------|
| WalkArea2 是否主因 | **否**（形状） |
| 磁盘施工 F_D1/D2/Order | **已在仓库** |
| 用户「仍卡」 | **待 Play 自检**（优先 A1 本机同步） |
| 改 WalkArea2 | **继续禁止** |

---

## ⑦ 程序索引

| 符号 | 路径 |
|------|------|
| F_D1 场景 | `Village_KenMuNi1` · `VillageDepthY_Min/Max` |
| F_D1 菜单 | `KenMuNi1VillageDepthYSetupEditor.cs` |
| F_D2/F_Order/W1 | `Village_KenMuNiSceneManager.TryApplyChiefStairsLandingToTree2f` |
| 标尺注入 | `PlayerLogic.TryInjectVillageDepthYBoundsFromSceneMarkers` |
| Clamp / Override | `TownPlayerLocomotion` |

**硬禁止**：先改 `VillageWalkArea2` 点集「试试」；关 ClosestPoint；1 楼区扩罩 2 楼；与宝箱/进层戏混修。
