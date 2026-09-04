# Village_Chief_House — 进场飞出 / 不在 DefaultBornPos — 验收排查报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【验收员 / 架构侦探 → 施工已落地】`EnterFrom_Village` 已对齐 `DefaultBornPos` 进 WalkArea；**未改**多边形 / ClosestPoint  
**Unity**：2020.3.48f1  
**现象**：一进 `Village_Chief_House`，玩家**不在** `Map/DefaultBornPos`，并会**直接飞出去**（离开期望点 / 镜头观感飞出）  
**关联**：0901 室内划区 2.5D（`VillageWalkArea` + Town ClosestPoint）刚上线；用户手调过 WalkArea  
**提示词**：`提示词/0901/Village_Chief_House_进场飞出DefaultBornPos_验收排查提示词.md`  
**施工依据**：`施工说明/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_施工说明.md`

---

## 沟通摘要

### ① 结论一句话

**从村进屋本来就不走 `DefaultBornPos`，而走 `EnterFrom_Village (17.42,-3.65)`；该点已在手调后的 `VillageWalkArea` 形外，一开村模式就被 ClosestPoint 吸到进门底带（约 Y≈−6.5，近 DefaultBorn），表现为「不在 DefaultBorn + 飞一下」。主因 H1，期望错位 H2。**

### ② 原因（通俗）

进村长家时，程序按「上一关是村子」去找进门点 `EnterFrom_Village`，不会去读你红箭头指的 `DefaultBornPos`。  
你手调可走区之后，进门点悬在多边形上方；一进屋就开 2.5D 夹区，脚会被一帧吸进绿带——看起来像飞出去。  
`DefaultBornPos` 其实在绿带里，和真正用的进门点差了将近 3 个单位高度。

### ③ 用户检查清单（修复后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 从 KenMuNi1 进屋：脚在裁定锚点附近，**无**大位移飞出 | |
| 2 | Scene：落点 `OverlapPoint==true`（在 WalkArea 形内） | |
| 3 | `EnterFrom_Village` 与（可选对齐后的）`DefaultBornPos` 同区同高 | |
| 4 | 区内 A/D+W/S、楼梯、障碍仍正常 | |
| 5 | 读档进房 / 无 EnterPos 兜底 DefaultBorn：不飞 | |
| 6 | 续聊 / 换古莎 / LeftDoor `EnterPosKey` 回归 | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H1**：`EnterFrom_Village` **在 WalkArea 外** → `ApplyVillageWalkPolygonPostCorrection` / Flush 一帧吸入形内 |
| **伴随** | **H2**：从村进屋走 **EnterPos→EnterFrom**，**不用** `DefaultBornPos`；二者世界坐标 **不一致**（ΔY≈3） |
| 产品落点 | **以 `EnterFrom_Village` 为准**（现网 EnterPos）；建议与 `DefaultBornPos` **对齐到同一形内坐标** |
| 飞出修法 | **优先**把落点深入 WalkArea（移 `EnterFrom` 进底带，或扩多边形盖住现 EnterFrom）；**禁止**关 ClosestPoint / 关白名单 |
| 次要假说 | H3 读档脏坐标可叠加；H4/H7 本路径证据弱；H5/H6/H8 非主因 |

---

## ② 复现

| 路径 | 预期现象 | 说明 |
|------|----------|------|
| **A · 从村进屋**（`House_Chief` / Loading） | **必现** | `LastScene=Village_KenMuNi1` → SetPos `EnterFrom` → 开 Town → 校正 |
| B · 读档进房（`archiveStart`） | **可能** | 存档脚位若在区外，同样被吸；与 EnterFrom 无关 |
| C · 无 EnterPos 匹配 | 走 `DefaultBornPos` | 现网 DefaultBorn **在形内**，兜底相对稳；与「从村进屋」主诉不同 |

**推荐复现 A**：关读档优先；从 KenMuNi1 进村长家；Pause 看脚位 vs Gizmo。

---

## ③ 假说表（H1～H8）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H1** | 落点在 WalkArea 外 → ClosestPoint 吸走 | **✅ 主因** | PIP：`EnterFrom (17.42,-3.65)` → **outside**；同 X 底带 `(17.42,-6.5/-7.0)` → **inside**；施工说明已预警「落点被吸」 |
| **H2** | 期望 DefaultBorn，实际 EnterFrom | **✅ 成立（期望错位）** | EnterPos 仅 `KenMuNi1`→fileID `880003002`=`EnterFrom_Village`；`Map.defaultBornTsf`→`DefaultBornPos (17.1,-6.61)` 仅兜底 |
| **H3** | archiveStart 脏坐标 | **次要可能** | `InitPlayer` 优先存档 pos；须对比关读档。不解释「从村正常进屋」主路径 |
| **H4** | (0,0) 踩门二次 Load | **否（弱）** | `RightDoor` **Inactive**；虽 `TriggerWhenMoveIn:1`→ForestEast，GO 关则进场难踩 |
| **H5** | DepthY 与落点冲突 | **否（主因）** | Min≈**−7.82** Max≈**3.26**；EnterFrom Y=−3.65 在标尺内；飞出主因是 Polygon 非 Depth Clamp |
| **H6** | 障碍保险推出 | **否（主因）** | 障碍在楼梯侧（≈−8/−3 一带）；进门带 x≈17 无重叠证据 |
| **H7** | 楼梯顶门误触发 | **否** | `StairsDoor_ToTree2f`≈(−4.5,4.8)，组件 **Enabled=0**；距进门远 |
| **H8** | 未进 Town 重力坠出 | **否** | 白名单已含 `Village_Chief_House`；进房会 `SetVillageExplorationMode(true)` + Flush——正是 **开了** Town 才触发 H1 |

---

## ④ 证据（坐标表）

### 进场链（现网）

```
CreatePlayer
  → archiveStart? → 存档 pos
  → else SetPlayerPos：
       lastScene==Village_KenMuNi1 → EnterFrom_Village
       未命中 → Map.defaultBornTsf（DefaultBornPos）
  → RefreshVillageExploration → Town + 绑 VillageWalkArea + DepthY
  → FlushAuthoritative… → ApplyVillageWalkPolygonPostCorrection（区外则吸）
```

### 磁盘坐标（`Village_Chief_House.unity`）

| 物体 | 世界/本地（父 Map≈0） | WalkArea 内？ | 用途 |
|------|----------------------|---------------|------|
| **`EnterFrom_Village`** | **(17.42, −3.65, 0)** | **❌ 外** | EnterPos 从村进屋 |
| **`DefaultBornPos`** | **(17.1, −6.61, 0)** | **✅ 内** | 无匹配时的默认出生；`defaultBornTsf` |
| 底带探针 (17.42, −6.5) | — | ✅ | 同 X 形内参考 |
| `VillageWalkArea` | 根 (0,0,0)；9 点 | — | 手调后点集 |
| `VillageDepthY_Min/Max` | (0, **−7.82**) / (0, **3.26**) | — | 已偏离施工初值 −5.25（手调） |

### WalkArea 点集（磁盘，手调后）

| # | (x, y) |
|---|--------|
| 0 | (26.03, −7.28) |
| 1 | (26.06, −6.80) |
| 2 | (−7.37, −5.28) |
| 3 | (−10.38, −3.57) |
| 4 | (−10.31, −2.31) |
| 5 | (−3.05, 3.02) |
| 6 | (−7.39, 3.18) |
| 7 | (−13.80, −2.51) |
| 8 | (−13.90, −7.33) |

进门「底带」上沿在 x≈17 约为 **Y≈−6.4**（边 1→2）。`EnterFrom` 的 Y=−3.65 **高出底带约 2.8** → 形外。

### 校正量级（推断）

| 项 | 值 |
|----|-----|
| 预计吸入方向 | 大致 **向下** 贴近底带上沿（≈(17.4, −6.4)） |
| Δ 相对 EnterFrom | ≈ **2.8～3.0** 世界单位（在 `walkAreaMaxCorrectionWorldDistance=8` 内，**会被采纳**） |
| 吸入后 vs DefaultBorn | 接近 DefaultBorn 高度；用户仍会感到「先错位再飞」 |

### Console / Play 必采（验收员）

| 日志建议 | 内容 |
|----------|------|
| `[ChiefEnterPos]` | `lastScene`、`archiveStart`、目标名、目标世界坐标、DefaultBorn/EnterFrom 坐标 |
| `[VillageWalk]` | 校正前/后脚位、`OverlapPoint` 前后、位移距离、poly 名 |

---

## ⑤ 主因

**`EnterFrom_Village` 落在手调后 `VillageWalkArea` 之外；进屋开启 Village2_5D 后 `Flush`/`ClosestPoint` 合法把角色吸入形内底带，造成「不在 DefaultBorn（因本就不走它）+ 飞一下」。**

不是相机单独坏了；不是必须关村模式。

---

## ⑥ 最小修复清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | **场景**：将 `EnterFrom_Village` 移入 WalkArea **深入内**（建议对齐 `DefaultBornPos≈(17.1,-6.61)` 或略偏门内） | **P0** |
| 2 | **或** 扩/抬 WalkArea 上沿，使 `(17.42,-3.65)` 进入形内（若美术门高必须保留该 Y） | P0 二选一 |
| 3 | **对齐**：`DefaultBornPos` 与 `EnterFrom_Village` **同世界坐标**（避免期望错位 + 兜底再飞） | **P0** |
| 4 | **勿**改 EnterPos 绑到 DefaultBorn 却不同时保证形内——可绑，但坐标须形内 | 可选 |
| 5 | 短日志 `[ChiefEnterPos]`/`[VillageWalk]` 验校正距离＜阈值（如 0.5）后可关 | P1 |
| 6 | **禁止**：关 ClosestPoint；关 Chief 白名单；改 `VillageWalkArea2`；动续聊/古莎/出屋台本 | — |

**原因说明**：施工说明已写「落点被吸到奇怪边角：扩大 WalkArea 使 EnterFrom 深入区内」——与本次磁盘证伪一致；属场景标定问题，非须重写 locomotion。

---

## ⑦ 验收

- [ ] 从村进屋：落在裁定锚点附近，无跨图/大距离一帧飞  
- [ ] 脚位与 `EnterFrom_Village`（已与 DefaultBorn 对齐者）一致  
- [ ] `OverlapPoint(落点)==true`  
- [ ] 划区移动 / 楼梯 / 障碍回归  
- [ ] 读档与 DefaultBorn 兜底不飞  
- [ ] LeftDoor `EnterPosKey=Village_Chief_House_Door` 等回归  

---

## ⑧ OPEN

| ID | 问题 | 决议 / 默认 | 状态 |
|----|------|-------------|------|
| Q1 | 从村进屋落哪？ | **`EnterFrom_Village`**（EnterPos）；与 DefaultBorn **对齐同点** | ✅ |
| Q2 | 飞出主因？ | **H1 区外落点 + ClosestPoint** | ✅ |
| Q3 | 修多边形还是移落点？ | **优先移 EnterFrom 进底带**（少改手调楼梯多边形）；门高必须则抬多边形 | ⏳ 施工选 |
| Q4 | 读档路径是否也飞？ | 验收对比；区外存档同修 | ⏳ |
| Q5 | 是否改 EnterPos 改绑 DefaultBorn？ | **不必须**；对齐坐标即可 | ✅ |

---

## ⑨ 程序补充

### 关键锚点

| 符号 | 路径 |
|------|------|
| `InitPlayer` / `SetPlayerPos` | `BaseGameSceneManager` |
| 白名单 / Flush | `SceneName.IsVillageExplorationScene`；`PlayerLogic.SetVillageExplorationMode` |
| 夹区 | `TownPlayerLocomotion.ApplyVillageWalkPolygonPostCorrection` · `ClampWorldPointToPolygonInterior` |
| 场景 | `EnterFrom_Village` · `DefaultBornPos` · `VillageWalkArea` |

### 建议日志插点（短、可开关）

1. `SetPlayerPos` 末：lastScene、选用 Transform 名与坐标  
2. `FlushAuthoritative…` / `ApplyVillageWalkPolygonPostCorrection`：校正前/后、位移、`OverlapPoint`  

### 硬禁止

- 未量校正前后坐标就猜修  
- `SetVillageWalkAreaOverride(null)` / 关 ClosestPoint 掩盖区外落点  
- 只当相机问题不查脚根  
