# Village_KenMuNi1 — 村长家上楼巨树 2 楼 WalkArea2 卡住不动 — 架构溯源报告

**文档版本**：v1.0（2026-09-03）  
**文档性质**：【架构侦探】只读溯源；**本文件不施工**  
**Unity**：2020.3.48f1 / C#  
**现象（用户 + Scene 截图）**：`Village_Chief_House` 楼梯上楼进 `Village_KenMuNi1` 巨树 2 楼后：**一下子卡住主动不了**；**未稳落** `ExitFrom_HomeSceneChief2f`；**无法在 `VillageWalkArea2` 内移动**（枝干上可见 WalkArea2 线框）  
**上游**：0901 楼梯换场 + W1 Override + E3′（已施工，本案 = **验收失败面**）  
**提示词**：`Assets/Doc/提示词/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构侦探提示词.md`  
**对照**：0901 楼梯案报告/施工说明；0901 进屋吸楼梯验收（Flush/权威传送同构）；0819 围栏穿模卡死（障碍焊死参考）

---

## 沟通摘要

### ① 结论一句话

**主因不是「没绑 WalkArea2、也不是该改多边形形状」——是 KenMuNi1 根本没有 `VillageDepthY_Min/Max`，Player Prefab 纵深上限仍是 `depthYMaxWorld=8`，而 2 楼落点 Y≈41.66；权威 Teleport / 每帧 Clamp 把人压在 ≤8，同时 W1 把 ClosestPoint 切到 WalkArea2（Y≈33～45），两套约束每帧撕扯 → 落不稳 ExitFrom、区里像焊死。**

### ② 原因（通俗）

村里走路有两道「尺子」：一道管**能站多高**（纵深 Y 标尺），一道管**脚必须踩在哪块地板多边形里**（WalkArea）。  
2 楼地板和出口点都在很高的树枝上（大约 Y=40），但村里这道「能站多高」的尺子还停在默认 **最高 8**（村长家室内才单独摆过标尺，村外巨树场景漏了）。  
程序一落地就把人往「最高 8」压，同时又按 2 楼地板往上吸——人被两头拽，看起来站在绿线框附近却动不了，也站不稳出口点。

### ③ 用户检查清单（验收 / 复现）

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 楼梯上楼进村后 Pause | 脚世界坐标距 `ExitFrom_HomeSceneChief2f≈(-159.34,41.66)` 很小 |
| 2 | Console | 有 `[Village2f] 已 SetVillageWalkAreaOverride(VillageWalkArea2)` |
| 3 | Hierarchy / 临时日志 | `TownPlayerLocomotion` 的 `DebugDepthYMaxWorld` **≥** WalkArea2 上沿（约 ≥45）；**不是**仍为 8 |
| 4 | `OverlapPoint(VillageWalkArea2)` | true；区内 A/D+W/S 可走，**不被吸回 1 楼** |
| 5 | 对比修前 | `VillageWalkArea2` 点集/尺寸未改 |
| 6 | `LeftDoor` 出门 | 仍落 `ExitFrom_HomeSceneChief`（1f），**不**绑 WalkArea2、不播 2f 路径 |
| 7 | Console | 无相关 Error；过滤 `[Village2f]` / `[TownLocomotion]` / `CLAMP_AT_YMAX` |

### ④ 程序补充

见下文 §①～§⑨。施工默认倾向：**F_D1（摆 KenMuNi1 纵深标尺覆盖 2 楼）为主**；可选 **F_D2 / F_Order** 作防漏与时序加固。**严禁** F5 改 WalkArea2 形状、F6 关 ClosestPoint。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H8′（纵深 Y 标尺缺失）+ H2 同构加重**：KenMuNi1 **无** `VillageDepthY_*` → Prefab `depthYMaxWorld=8`；`SetPos`→`TeleportAuthoritativeVillagePos` 把 41.66 **Clamp→8**；W1 生效后 ClosestPoint 指向 WalkArea2（高 Y）→ **每帧 Clamp↔夹区撕扯** |
| **H1（W1 未绑）** | **弱 / 非主因**：代码路径在 `last==Village_Chief_House` 时会绑；用户站在 2 楼线框附近更符合「Override 已生效但与标尺冲突」 |
| **H2（未到 ExitFrom）** | **✅ 成立（由 Clamp 导致）**：目标 Transform 正确，权威 Y 被削到 ≤8，随后夹区再拽 → 视觉上「没站稳出口」 |
| **H3（形外夹死）** | **次要**：ExitFrom 相对 WalkArea2 **PIP=True**；形外不是主因，撕扯/贴边才是体感 |
| **H4（障碍焊死）** | **弱**：0819 同构可叠加，但解释不了「必现一上楼就卡 + 未到 ExitFrom」；先治标尺 |
| **H5（输入锁）** | **弱**：楼梯键不进送树屋戏；若黑幕/剧情未还控另验 |
| **H6（门配置脏）** | **❌ 否（主路径）**：`StairsDoor_ToTree2f` 在盘；`EnterPosKey` 空；`NextScene=Village_KenMuNi1`；`TriggerWhenMoveIn=1`；`ShowLoadingUI=0`；已进 `sceneObjs`；`m_Enabled=0` 是 **SR**（Setup 关双影），非 `SceneChangeDoor` |
| **H7（LastScene 时机）** | **弱**：楼梯空键 → 记真实 `Village_Chief_House`；与 W1 条件一致 |
| **方案** | **F_D1 必做**（KenMuNi1 摆 `VillageDepthY_Min/Max`，Max 覆盖 WalkArea2）；**F_D2 建议**（W1 内按 poly.bounds 抬 Max 防漏摆）；**F_Order 可选**（先 Override 再权威 Teleport）；否 F5/F6 |

---

## ② 复现与产品期望

### 期望时序（0901 定案）

```
Village_Chief_House
  → StairsDoor_ToTree2f（TriggerWhenMoveIn + 黑幕，EnterPosKey 空）
  → LastSceneName = Village_Chief_House
  → LoadScene(Village_KenMuNi1)
  → EnterPos：Village_Chief_House → ExitFrom_HomeSceneChief2f ≈ (-159.34, 41.66, 0)
  → SetPlayerPos → TeleportAuthoritative（Y 须保留 ≈41.66）
  → W1：SetVillageWalkAreaOverride(VillageWalkArea2) + Flush
  → 区内村式 A/D+W/S；ClosestPoint 只夹 WalkArea2
```

### 现网实际（磁盘 + 代码）

```
CreatePlayer → OnInit → RefreshVillageExploration
  → ApplyVillageMode：绑名 "VillageWalkArea"（1 楼）
  → TryInjectVillageDepthYBounds：KenMuNi1 **无标尺** → 保留 Prefab [-20, 8]
  → Flush：权威旗 false → 跳过夹区
→ SetPlayerPos
  → SetPos(ExitFrom) → TeleportAuthoritative((-159.34, 41.66))
       → _villageWorldY = Clamp(41.66, -20, 8) = **8**   // ⚠ 主断点
       → 旗=true；Flush：仍用 1 楼 WalkArea ClosestPoint
  → W1：Override=VillageWalkArea2；Flush
       → ClosestPoint 拉向 Y≈33～45 的 2 楼多边形
→ FixedUpdate 每帧：
       Clamp(_villageWorldY, …, **8**) → WriteRoot
       → ApplyVillageWalkPolygonPostCorrection(WalkArea2)  // 再往高处吸
       → 撕扯 / 贴边 / 焊死体感
```

---

## ③ 假说表（H1～H8）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H1** | W1 未生效，仍夹 1 楼 WalkArea | **弱** | `TryBind…` 条件 `last == SceneName.Village_Chief_House` 与楼梯空键一致；成功会打 `[Village2f] 已 Set…`。若完全未绑，人应被吸向 Y≈−6 而非停在枝干 WalkArea2 线框旁 |
| **H2** | 落点未到 / 未稳 ExitFrom | **✅** | EnterPos 磁盘正确；`SetPos` 村模式走 Teleport，但 **Clamp 削 Y** → 权威位 ≠ ExitFrom；随后夹区再改位 |
| **H3** | 落在 WalkArea2 形外被夹死 | **次要** | ExitFrom 相对 WalkArea2 原点 `(-20.34, 4.16)`，射线法 **PIP=True**；形外非主因 |
| **H4** | VillageWalkObstacle 重叠焊死 | **弱** | 0819 机制仍在；不解释「必现未到 ExitFrom」；标尺修后若仍贴围栏再验 |
| **H5** | isTalking / 禁止操作 | **弱** | `ShouldPlayLeaveChiefEscort` 要求 `Village_Chief_House_Door`；楼梯键不进。黑幕未关另作验收项 |
| **H6** | 楼梯门 enterPosKey / Setup 脏 | **❌** | 见 §⑤ 门表；SR `m_Enabled=0` ≠ 门脚本禁用 |
| **H7** | LastSceneName 不是 Chief | **弱** | `ChangeSceneComponentGM` 空键用真实场景名；与 E3′ 设计一致 |
| **H8** | Rb / Transform / 权威 Y 不同步 | **✅ 变体成立** | 0901 进屋案已修 `SetPos→Teleport`；本案是 **Teleport 后仍被 depthYMax=8 削高**，再与 WalkArea2 撕扯——同构「权威写回打脸落点」 |

**增补（提示词外，磁盘钉死）**：**H8′ / DepthGap** — KenMuNi1 **零** `VillageDepthY_Min/Max` 命中；Chief 有（Max 本地 Y≈3.26）；Player.prefab `depthYMaxWorld: 8`。

---

## ④ 关键证据（锚点）

### 4.1 场景锚点（KenMuNi1，Map 父级世界 ≈0）

| 物体 | 磁盘 | 备注 |
|------|------|------|
| `ExitFrom_HomeSceneChief2f` | local **`(-159.34, 41.66, 0)`**；fileID `880002002`；Active | EnterPos `Village_Chief_House` → 此 Transform |
| `ExitFrom_HomeSceneChief` | **`(-156.5, -5.5, 0)`** | E3′ 大门键；**不**绑 WalkArea2 |
| `VillageWalkArea2` | **`(-139, 37.5, 0)`** + PolygonCollider2D；**形状锁定** | 世界 AABB 约 X∈[−164,−107]，Y∈[33.6, 45.4] |
| `VillageWalkArea` | ≈ `(0, -5.91)` | 现网默认 `TryBind` 名 |
| `VillageDepthY_Min/Max` | **❌ 场景内无** | 与 Chief 对比鲜明 |
| EnterPos | `Village_Chief_House`→2f；`Village_Chief_House_Door`→1f | ✅ 配对正确 |

ExitFrom ∈ WalkArea2：**相对局部 (−20.34, 4.16)，PIP=True**（侦探脚本射线法）。

### 4.2 纵深 Clamp（根因代码）

```51:55:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
        private float depthYMinWorld = -20f;
        // ...
        private float depthYMaxWorld = 8f;
```

Player.prefab 序列化同为 `depthYMinWorld: -20` / `depthYMaxWorld: 8`。

```496:497:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
            _villageWorldY = Mathf.Clamp(worldXy.y, depthYMinWorld, depthYMaxWorld);
            Vector2 rb = new Vector2(worldXy.x, _villageWorldY);
```

```341:347:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
            _villageWorldY = Mathf.Clamp(_villageWorldY, depthYMinWorld, depthYMaxWorld);
            // ...
            WriteRootTransformWithAuthoritativeDepthY();
            ApplyVillageWalkPolygonPostCorrection();
```

`TryInjectVillageDepthYBoundsFromSceneMarkers`：**缺任一标尺则保留 Prefab 默认**（注释 L-02）。KenMuNi1 全文件无 `VillageDepthY_` 字符串。

### 4.3 W1（已落地，非缺席）

```202:253:Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
        protected override void SetPlayerPos(PlayerLogic playerLogic)
        {
            base.SetPlayerPos(playerLogic);  // 先 Teleport（此时仍可能 maxY=8）
            TryBindVillageWalkArea2AfterChiefStairsLanding(playerLogic);
        }
        // last == Village_Chief_House → SetVillageWalkAreaOverride(WalkArea2) + Flush
```

时序问题：**Override 在首次权威 Teleport 之后** → 首帧 Flush 仍可能用 1 楼多边形；即使顺序对调，**无标尺时 Y=41 仍进不来**。

### 4.4 楼梯门（Chief）

| 项 | 磁盘 |
|----|------|
| 名 | `StairsDoor_ToTree2f`（Stairs.prefab 实例） |
| 位 | ≈ `(-4.51, 4.8, 0)`（与 Setup 常量 `(-10.5,2.2)` 有漂移，仍在楼上） |
| `NextSceneName` | `Village_KenMuNi1` |
| `TriggerWhenMoveIn` | `1` |
| `ShowLoadingUI` | Prefab `0`（黑幕） |
| `EnterPosKey` | 空（→ 真实 `Village_Chief_House`） |
| SR `m_Enabled` | **0**（防双影；**不是**关换场） |
| `sceneObjs` | ✅ 含 stripped `854098133` |
| LeftDoor `EnterPosKey` | `Village_Chief_House_Door` ✅ |

---

## ⑤ 方案对比与推荐

| 方案 | 做法 | 判定 |
|------|------|------|
| **F_D1** | KenMuNi1 `Map` 下摆 `VillageDepthY_Min` / `Max`，**Max ≥ WalkArea2 上沿（建议 ≥45）**，Min 覆盖 1 楼地面带（可对齐现 Prefab −20 或地面标尺） | ✅ **主修**；与室内划区 / `TryInject` 契约一致 |
| **F_D2** | W1 绑 WalkArea2 时用 `poly.bounds` 扩展 `SetDepthYBounds`（仅抬 Max，勿乱缩 Min） | ✅ **建议**防漏摆标尺再炸 |
| **F_Order** | `SetPlayerPos`：**先** Override（或先扩 bounds）**再** `TeleportAuthoritative(ExitFrom)` | ✅ 辅；减首帧错区夹 |
| **F1**（提示词） | 只修 W1 触发条件 | ❌ 非主因；条件已对 |
| **F3** | 障碍挤出保险 | ⏳ 标尺修好后仍卡再验 |
| **F4** | 修门序列化 | ❌ 门配置已通 |
| **F5** | 改 WalkArea2 点集/尺寸 | ❌ **严禁** |
| **F6** | 关 ClosestPoint / 撤白名单 | ❌ **严禁** |

**替代方案（不推荐本期）**：全局把 Player Prefab `depthYMaxWorld` 改成 50——会放开所有未摆标尺村场景的纵深，副作用面大于场景标尺。

---

## ⑥ 与相关案边界

| 案 | 关系 |
|----|------|
| 0901 楼梯上楼换场 | 本案 = **验收失败面**；W1/EnterPos/门 **保留**；补 **深度标尺缺口** |
| 0901 进屋吸楼梯 | Flush / 权威 Teleport **同构参考**；场景不同，**勿**照搬改 Chief |
| 0819 围栏卡死 | H4 后备；**勿**当本期主修 |
| 0901 WalkArea2 宝箱 | **正交**；人能走再谈箱；禁止为走路改多边形 |
| 出村长家送树屋 | 大门键路径；**勿**与楼梯 2f 混修 |

---

## ⑦ 最小施工建议（给施工员）

1. **F_D1**：KenMuNi1 摆 `VillageDepthY_Min` / `VillageDepthY_Max`（空物体，名钉死）；Max 覆盖 2 楼；保存场景。可用 Editor 菜单幂等，勿改 WalkArea2 几何。  
2. **F_D2（建议同 PR）**：`TryBindVillageWalkArea2AfterChiefStairsLanding` 在 `SetOverride` 前 `SetDepthYBounds(min, max(max, poly.bounds.max.y + ε))`，并打 `[Village2f] depthYMax→…` 日志。  
3. **F_Order（可选）**：楼梯路径先绑 Override / 扩 bounds，再对 ExitFrom 做一次 `TeleportAuthoritative`（避免 base.SetPos 先用错区 Flush）。  
4. **禁止**：改 WalkArea2 点集；关 ClosestPoint；1 楼 WalkArea 扩罩 2 楼；动宝箱 / 送树屋 / DayLight / 续聊。  
5. 文档：施工说明 `0903/…施工说明.md`；同步 OPEN（本案 + 上游 Q「2 楼可达+W1」）。

---

## ⑧ 验收（施工后）

- [ ] 楼梯换场后脚距 ExitFrom_HomeSceneChief2f 很小；`OverlapPoint(WalkArea2)=true`  
- [ ] Console：`[Village2f] 已 SetVillageWalkAreaOverride(VillageWalkArea2)`  
- [ ] `DebugDepthYMaxWorld`（或日志）≥ 2 楼高度；无持续 `CLAMP_AT_YMAX` 与夹区对打  
- [ ] 区内 A/D+W/S 可走，不卡死，不被吸回 1 楼高度  
- [ ] WalkArea2 点集/尺寸与修前一致  
- [ ] LeftDoor 出门仍落 1 楼门前且不绑 WalkArea2  
- [ ] 无相关 Error  

---

## ⑨ OPEN 建议

| ID | 问题 | 建议决议 | 状态建议 |
|----|------|----------|----------|
| Q1 | 主因？ | **DepthGap（无 VillageDepthY + max=8）与 WalkArea2 撕扯**；非缺 W1、非改形状 | ✅ 本报告 |
| Q2 | 方案？ | **F_D1 必做 + F_D2 建议 + F_Order 可选** | 待施工 |
| Q3 | 上游「2 楼可达+W1」？ | 标为 **验收失败 → 本补丁后重验** | 改 OPEN |
| Q4 | 同场景 2f→1f 是否切回 WalkArea？ | 维持 0901 Q6：本期仅进 2f 绑 2；下树另案 | ⏳ |
| Q5 | 障碍 H4？ | 标尺通过后再复测；未复现不修 | ⏳ |

---

## ⑩ 程序索引

| 符号 | 路径 |
|------|------|
| W1 | `Village_KenMuNiSceneManager.TryBindVillageWalkArea2AfterChiefStairsLanding` |
| Override / ClosestPoint / Flush / Teleport | `TownPlayerLocomotion.cs` |
| SetPos 村模式权威 | `PlayerLogic.SetPos` |
| 标尺注入 | `PlayerLogic.TryInjectVillageDepthYBoundsFromSceneMarkers` |
| LastScene / enterPosKey | `ChangeSceneComponentGM` / `SceneChangeDoor` |
| Setup 门 | `ChiefHouseStairsToTree2fSetupEditor.cs` |

**硬禁止（施工）**：改 `VillageWalkArea2` 多边形；关 ClosestPoint；撤探索白名单；用 1 楼 WalkArea 扩罩 2 楼；Update 堆业务；抢写 Animator `Run`。
