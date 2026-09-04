# Village_Chief_House — 进场落点吸到楼梯 — 验收排查报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【验收员 / 架构侦探 → 施工已落地 F1+F2+F3】权威 Teleport + 落点前跳过夹区；**未关** ClosestPoint / 白名单  
**Unity**：2020.3.48f1  
**现象（复测）**：进 `Village_Chief_House` 后玩家**站在楼梯上**（非进门底带）  
**已做修复**：`EnterFrom_Village` 已对齐 `DefaultBornPos≈(17.1,-6.61)` 且 **Overlap 形内**（治「区外吸底带」）；**未消**「吸楼梯」  
**上游**：`执行文档/0901/…进场飞出DefaultBornPos_验收排查报告.md` · `施工说明/0901/…进场飞出DefaultBornPos_施工说明.md`  
**提示词**：`提示词/0901/Village_Chief_House_进场落点吸到楼梯_修复提示词.md`  
**对白**：续聊 **不** SetPos；问题在进场 / 村模式 Flush  

---

## 沟通摘要

### ① 结论一句话

**主因 H1+H2：OnInit 在 SetPlayerPos 前就开 2.5D 并 Flush，脚在原点被 ClosestPoint 吸到楼梯段；随后 `SetPos` 只改 Transform、不改 Rigidbody2D / Town 权威 Y，Loading 再 Flush 时 `WriteRoot` 用刚体 X + 权威 Y 把人写回楼梯。EnterFrom 坐标已对，再挪点治不好。**

### ② 原因（通俗）

进门点已经摆对了，但程序先在「还没传送」时就按可走区夹了一次脚——原点离楼梯比离大门近，人被吸到楼梯。  
接着只把显示位置挪到门口，物理刚体还停在楼梯；下一轮校正又按刚体把人拽回去。  
所以看起来永远站在楼梯上。

### ③ 用户检查清单（修复后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 从 KenMuNi1 进屋：脚在 EnterFrom≈(17.1,-6.61)，目视**不在**楼梯上 | |
| 2 | `[ChiefEnterPos]`：kind=EnterPos，dist 小，OverlapWalkArea=True | |
| 3 | SetPos 后 transform 与 Rb **一致**；Loading 结束后不被拉回楼梯 | |
| 4 | 楼梯仍可走；WalkArea / 障碍仍有效 | |
| 5 | 村街进出 / 其它 Home / 续聊换古莎回归 | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H1 + H2 同构**：过早 Flush + `SetPos` 不同步 Rb/权威 Y |
| **H4** | **否（本轮）**：EnterFrom/DefaultBorn 均为 **(17.1,-6.61)**，PIP **形内** |
| **方案** | **F1 必做**（权威传送：Transform+Rb+`_villageWorldY`）+ **F2 建议**（无有效落点前跳过/推迟首次 ClosestPoint）+ **F3 可选**（Chief 落点后再 Teleport+Flush 双保险） |
| **禁止** | 关 ClosestPoint；撤白名单；只改多边形挖楼梯；续聊里再 SetPos 掩盖；绑降速案 |

---

## ② 复现

1. 从 `Village_KenMuNi1` 经门 / Loading 进 `Village_Chief_House`（非读档脏楼梯档优先）。  
2. Pause：脚是否叠在合层「楼梯」斜面/上层（约 x∈[−14,−3]、y 偏高）。  
3. 对比 `EnterFrom_Village (17.1,-6.61)`。  
4. 续聊开始时脚位仍错 → 证伪「对白改站位」。

---

## ③ 假说表（H1～H6）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H1** | OnInit 在 SetPos 前 Flush，原点→楼梯段 | **✅ 成立** | `PlayerLogic.OnInit` L171 即 `RefreshVillageExploration`；`(0,0)` **形外**；距楼梯侧 `(-3,3)`≈**4.3**，距进门 `(17.1,-6.61)`≈**18.3** → ClosestPoint 偏向楼梯带；`(-8,1)` PIP **内** |
| **H2** | SetPos 只写 Transform，不写 Rb / 权威 Y | **✅ 成立** | `SetPos` 仅 `transform.position`；`ApplyVillageWalkPolygonPostCorrection` 读 **`_playerRootRb2D.position`**；`WriteRootTransformWithAuthoritativeDepthY` **保留 Rb.x**，只写 `_villageWorldY` → 刚体仍在楼梯 X 时会把 Transform **写回楼梯** |
| **H3** | LoadingEnd 再 Refresh/Flush 打回 | **✅ 加重** | `LoadingSceneEndHandle` → 再 `RefreshVillageExploration` → Flush；若 Rb 未对齐 EnterFrom，必重夹 |
| **H4** | EnterFrom 仍形外 | **❌ 本轮否** | 磁盘两者 `(17.1,-6.61)`；PIP **True**（上一轮施工已对齐） |
| **H5** | archiveStart 脏档 | **次要** | 可叠加；不解释「正常从村进屋」主路径 |
| **H6** | 障碍保险推进门→楼梯 | **弱** | 进门带与楼梯侧障碍空间分离；主因仍是多边形校正 + Rb |

---

## ④ 证据（时序 + 代码）

### 进场时间线（期望钉死）

```
CreatePlayer → PlayerLogic.OnInit
  → RefreshVillageExplorationFromActiveScene()
  → SetVillageExplorationMode(true)
       → ApplyVillageMode：绑 WalkArea；_villageWorldY ← Rb.y
       → ApplyVillageWalkPolygonPostCorrection()   // ⚠ 脚尚在默认/(0,0) → 吸到楼梯
       → Flush…（Depth 注入后再夹一次）
→ InitPlayer 回调 SetPlayerPos
  → SetPos(EnterFrom)  // 仅 transform → (17.1,-6.61)；Rb 仍可能在楼梯
→ Loading 结束 → Refresh + Flush
  → WriteRoot：newRb = (rb.x楼梯, authY楼梯…)  // ⚠ Transform 被写回楼梯
→ OnEnterScene → 续聊（不改坐标）
```

### 关键实现

```575:579:Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
        public void SetPos(Vector2 pos)
        {
            Vector3 p = transform.position;
            transform.position = new Vector3(pos.x, pos.y, p.z);
        }
```

```631:639:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
                Vector2 rbPos = _playerRootRb2D.position;
                // 纵深只动 Y；X 跟刚体当前模拟位置
                Vector2 newRb = new Vector2(rbPos.x, _villageWorldY);
                _playerRootRb2D.position = newRb;
                PlayerLogic.transform.position = new Vector3(newRb.x, newRb.y, _frozenWorldZ);
```

```427:440:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
            // ApplyVillageMode(active=true)：立刻 Bind + 多边形校正
                _villageWorldY = _playerRootRb2D != null ? _playerRootRb2D.position.y : …;
                …
                ApplyVillageWalkPolygonPostCorrection();
```

### 场景锚点（磁盘，上一轮施工后）

| 物体 | 坐标 | WalkArea |
|------|------|----------|
| `EnterFrom_Village` | **(17.1, −6.61, 0)** | ✅ 内 |
| `DefaultBornPos` | **(17.1, −6.61, 0)** | ✅ 内 |
| 原点 (0,0) | — | ❌ 外（最近可走侧偏楼梯） |

### 建议日志时间线（施工保留至验收）

| 时刻 | 打点 |
|------|------|
| OnInit Flush 前/后 | transform、Rb、authY、距楼梯锚 / 距 EnterFrom |
| SetPos 后立即 | 同上；**transform≠Rb 则 H2 指纹** |
| LoadingEnd Flush 后 | 是否被拉回楼梯 |

---

## ⑤ 主因

**进场权威坐标与物理刚体脱节，且首次多边形校正发生在 EnterPos 之前：先被吸到楼梯，再「只摆皮」到门口，随后 Flush 按刚体把人写回楼梯。**

上一轮只修 EnterFrom 进形内，解决的是「从正确门口点吸到底带」；**不能**解决「校正读的是未同步的楼梯刚体」。

---

## ⑥ 最小修复清单（给施工员）

| # | 方案 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | **F1** | Village2_5D 下传送：同步 **Transform + `Rigidbody2D.position` + `_villageWorldY`**（抽 `TeleportAuthoritativeVillagePos` 或扩展 `SetPos`）；再 **可选一次** Flush；注释写明「只改 transform 会被 WriteRoot/多边形打回」 | **P0** |
| 2 | **F2** | OnInit / 首次 `ApplyVillageMode`：在「尚未完成场景 SetPlayerPos」前 **跳过** `ApplyVillageWalkPolygonPostCorrection`（或 `pendingSpawnFlush` 标志）；落点后再 Flush | **P0 建议** |
| 3 | **F3** | Chief：`SetPlayerPos` / `OnEnterScene` 末再权威 Teleport+Flush（防 Loading 二次 Refresh） | P1 双保险 |
| 4 | 日志 | 保留 `[ChiefEnterPos]` / 扩 `[VillageEnterFlush]` 至验收 | P1 |
| 5 | 回归 | KenMuNi1 落点、BlackFadeTeleport、其它 Home、楼梯可走 | 须 |

**替代方案说明**：仅 F3 可减轻症状但漏其它村模式传送；仅 F2 不修 SetPos 脱节，LoadingEnd 仍可能打回 → **F1 为根治**。

**否决**：F4 关夹区；F5 只挖楼梯多边形（原点仍可能吸别处）。

---

## ⑦ 验收

- [ ] 进屋脚在 EnterFrom 进门底带，不在楼梯美术上  
- [ ] SetPos 后 transform≡Rb；Loading 后仍在门口  
- [ ] OverlapWalkArea=True；无跨图级二次吸楼梯  
- [ ] 楼梯仍可走；村街 / Home / 续聊换古莎回归  

---

## ⑧ OPEN

| ID | 问题 | 决议 / 默认 | 状态 |
|----|------|-------------|------|
| Q1 | 主因？ | **H1+H2**（过早 Flush + SetPos/Rb 脱节） | ✅ |
| Q2 | F1/F2/F3？ | **F1 必做 + F2 建议 + F3 可选** | ✅ |
| Q3 | SetPos 全局改 vs 仅 Town Teleport API？ | **荐 Town 权威 API**，村模式调用；慎改全场景 SetPos 副作用 | ⏳ 施工选 |
| Q4 | 读档 archiveStart？ | 权威 Teleport 同样覆盖；验收对比 | ⏳ |

---

## ⑨ 程序补充

### 关键锚点

| 符号 | 说明 |
|------|------|
| `PlayerLogic.OnInit` → Refresh | H1 过早夹 |
| `PlayerLogic.SetPos` | H2 只写 Transform |
| `TownPlayerLocomotion.ApplyVillageMode` / `Flush…` / `WriteRoot…` | Rb.x 保留 + 多边形读 Rb |
| `LoadingSceneEndHandle` | H3 二次 Refresh |
| EnterFrom `(17.1,-6.61)` | 已形内；勿再当主修只挪点 |

### 硬禁止

- 续聊 Action 再 SetPos 掩盖  
- 只移 EnterFrom 不修 Transform/Rb  
- 关 ClosestPoint / 撤白名单  
- 绑室内降速 / 古莎 / 出屋送树屋 / WalkArea2  
