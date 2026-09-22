# Village · 纵深移动发抖与室内拖慢 · 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（**只读**，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 范围：`Village_KenMuNi1` + `Village_Chief_House`（凡 `SceneName.IsVillageExplorationScene` → `Village2_5D`）  
> 产品钉死：屋里抖；村外也抖；**带「Z 轴」（工程=权威世界 Y）移动才抖**；村长家速度明显拖慢  
> 提示词：`Assets/Doc/提示词/0922/Village_纵深移动发抖与室内拖慢_架构侦探提示词.md`  
> 工程语义：产品「Z 轴」= Town 权威纵深 **世界 Y**；根 **Z 冻结**。勿当 `Transform.z` 乱改。

---

## ① 结论一句话

**抖与拖慢是两刀。** 抖：0919 横移「少写坐标」在**纯左右**仍成立；**一带纵深**，`WriteRoot` 虽改用 `vy` 追权威 Y，但同帧 / 物理后 `WalkArea` 多边形与障碍分离仍会**硬写** `rb.position` + `transform.position`，掐死插值 → 人台阶 + CM 阻尼追抽——村外与屋里同一条 `TownPlayerLocomotion`。速：村长家 `ResolveVillagePlanarMoveSpeed` → `walkSpeed≈4.2` 相对村街 `≈11.2` 是 **0901 有意设计**，不是抖的修法；勿擅自改回 11.2。0922 键族只换读键，**未**破坏 WriteRoot 契约，不作抖主因。推荐 **方案 A（收口写位）** 治抖；P3 仅当相对 4.2 仍异常更慢再开 **方案 B**；**否决**先动 CM 阻尼 / 室内改回街速。

---

## ② 复现矩阵（代码推演；Play 验收前）

本阶段未进 Play。下表按现网 `TownPlayerLocomotion` 写位/追 Y 契约推演；**人抖 / 镜头抖**须施工后按验收表实机勾选。

### `Village_KenMuNi1`（村街 ≈11.2）

| 操作 | 人是否抖 | 镜头是否抖 | 体感速度 |
|------|----------|------------|----------|
| 纯左右（无上下） | **推演：否**（0919：`depthVelocity≈0` → 吸权威 Y、清 `vy`、**不写坐标**） | **推演：否**（跟拍点顺） | ≈11.2 满横 |
| 纯前后/纵深（产品 Z） | **推演：是**（积分权威 Y + `vy` 追；WalkArea/障碍硬写易触发） | **推演：是**（Follow 目标台阶 → XDamping 追抽） | 纵深封顶同 planar≈11.2 |
| 斜向 | **推演：是**（纵深分量同上） | **推演：是** | 合速归一 ≈0.707×11.2（0818 预期） |
| 贴墙/贴 Walk 障碍 | **推演：加重**（Cast 夹紧 / 脚分离 / 保险硬写更频） | **推演：加重** | 可能被横速夹 / 发闷感 |

### `Village_Chief_House`（室内 ≈4.2）

| 操作 | 人是否抖 | 镜头是否抖 | 体感速度 |
|------|----------|------------|----------|
| 纯左右 | **推演：否**（同 KenMuNi1 写位分支） | **推演：否** | ≈4.2（0901） |
| 纯前后/纵深 | **推演：是**（同公共 Town 路径） | **推演：是** | ≈4.2 |
| 斜向 | **推演：是** | **推演：是** | ≈0.707×4.2 |
| 贴墙/贴家具障碍 | **推演：加重** | **推演：加重** | 若无纵深意图仍追 Y → 异常更慢（0919 发闷缝；现网门控已挡主路径） |

### 对照：其它村民家（`LocomotionMode.Default`，无 Town）

| 操作 | 预期 |
|------|------|
| 纯走 | **不走**本票写位/权威 Y；体感 walk≈4.2、无「带 Z」纵深。用于证明「只有 2.5D 白名单抖」。 |

**矩阵裁定（产品对拍）**

| 产品句 | 代码是否吻合 |
|--------|----------------|
| 屋里抖 + 村外也抖 | ✅ 同属 `IsVillageExplorationScene` → 同一 `TownPlayerLocomotion` |
| 带 Z（纵深）才抖 | ✅ 纯左右少写；纵深步 `vy` + 后段硬写更易打台阶 |
| 村长家明显拖慢 | ✅ 相对村街 **设计差**（4.2 vs 11.2）；须 Play 再判是否相对 4.2 仍更慢 |

---

## ③ 对拍 0919 / 0901 / 0922

### B. 抖：0919 契约是否被破坏

| 检查项 | 现网 | 裁定 |
|--------|------|------|
| Y 不变时少写死坐标、左右靠速度+插值 | `WriteRootTransformWithAuthoritativeDepthY(false)`：`depthVelocity≈0` → 吸 `_villageWorldY`、清 `vy`、**return 不写 position** | **纯左右：0919 v1.2 仍成立** |
| 有纵深时是否每步写死 X+Y | WriteRoot 纵深分支：只设 `velocity.y = clamp(dy/dt)`，**不写坐标**（注释明确「不要写坐标」） | WriteRoot **本身**未回退到 0919 前「每帧钉坐标」 |
| 同帧后段是否硬写 | `ApplyVillageWalkPolygonPostCorrection`：有修正则写 `rb.position` + `transform.position` 且 `vy=0`；脚穿透分离 / 保险同构硬写 | **纵深/贴边时易每物理步清插值** → 与「带 Z 才抖」吻合 |
| 物理后再写一次 | `PostPhysicsResyncDepthCoroutine`：`Settle…` 后再跑多边形 + 障碍硬写 | **双次收口**放大台阶感 |
| 0922 是否改写位 | 仅 `OnFixedUpdate` / Home 意图：`GetAxisRaw("Vertical")` → `GetVillageExploreVerticalSign()` 等；WriteRoot / 多边形逻辑未改 | **不作抖主因**（方案 C 低优先） |
| 抖源 | Follow 目标（人）被硬写成台阶；CM XDamping 追抽 | **禁止**一上来 XDamping=0 |

关键片段（现网）：

```828:851:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
                // 没按上下：不要用速度追权威 Y。贴地时追会把左右速度蹭慢。
                if (Mathf.Abs(depthVelocity) <= VillageDepthYWriteEpsilon)
                {
                    _villageWorldY = Mathf.Clamp(rbPos.y, depthYMinWorld, depthYMaxWorld);
                    _playerRootRb2D.velocity = new Vector2(v.x, 0f);
                    ZeroVillageMoveSpeedY();
                    return;
                }
                // ...
                // 正在按上下：用速度补这一步。不要写坐标，碰撞才能挡住，镜头才顺。
                float vy = Mathf.Clamp(dy / dt, -maxSpeed, maxSpeed);
                _playerRootRb2D.velocity = new Vector2(v.x, vy);
```

```1320:1331:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
            Vector2 corrected = ClampWorldPointToPolygonInterior(poly, p, walkPolygonInsetEpsilon);
            if ((corrected - p).sqrMagnitude <= 1e-10f)
            {
                return;
            }

            _villageWorldY = corrected.y;
            _playerRootRb2D.position = corrected;
            // ...
            PlayerLogic.transform.position = new Vector3(corrected.x, corrected.y, _frozenWorldZ);
```

**抖根因归纳**：0919 只收口了 `WriteRoot` 走路路径；**纵深参与时**权威 Y 每步在动，WalkArea/障碍「几何以 Polygon 为准」的硬写路径更容易命中 → 与产品「带 Z 才抖」同源于「写死坐标掐插值」，不是新相机盒问题。

### C. 速：0901 设计 vs 异常拖慢

| 检查项 | 现网 | 裁定 |
|--------|------|------|
| Chief `ResolveVillagePlanarMoveSpeed` | `IsIndoorVillageExplorationScene` → `move.WalkSpeed`（fallback **4.2**） | **0901 仍在** |
| KenMuNi1 | `villagePlanarMoveSpeed` 默认 **11.2** | 村街不变 |
| 产品「明显拖慢」 | 相对村街约 **2.7×** 慢 | **先裁定为设计差**；Play 再测是否相对 4.2 仍更慢 |
| 无纵深是否仍追权威 Y | `depthVelocity≈0` 分支吸 Y、不追 | 0919 发闷主路径**已挡**；贴障硬写夹 `vx` 另记 |
| HomeWalk vs CombatRun | Chief=`Village2_5D` → Home 子状态机；`HomeWalkState` `SetWalkSpeed()`；Town 同帧用 `WriteVillagePlanarHorizontalSpeed(…, planar)` 对齐目标 | **无二次街速缩放**；室内目标就是 4.2 |
| 斜向变慢 | `ApplyVillagePlanarMoveSpeedNormalization` DIAGONAL 归一 | **0818 预期**，勿当 P3 主因 |

```637:658:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
        private float ResolveVillagePlanarMoveSpeed()
        {
            string active = SceneManager.GetActiveScene().name;
            if (SceneName.IsIndoorVillageExplorationScene(active))
            {
                // ... WalkSpeed 或 IndoorVillagePlanarMoveSpeedFallback(4.2)
            }
            return villagePlanarMoveSpeed > 0f ? villagePlanarMoveSpeed : VillagePlanarMoveSpeedFallback;
        }
```

若产品现网要推翻 0901「室内对齐村民家 4.2」→ 单列 OPEN，**勿与抖绑死一票**。

---

## ④ 方案对比与推荐

| 方案 | 做法 | 何时选 | 裁决 |
|------|------|--------|------|
| **A** | 收口写位：纵深变化优先 `vy`/权威积分；WalkArea/障碍修正**尽量少写死** `transform`（或仅 snap 阈值外、避免每帧清插值）；保持纯左右 0919 | 抖 = 硬写掐插值 | **推荐（治抖）** |
| **B** | 收口「无纵深意图不追 Y」；贴边夹速复核 | 相对 4.2 仍异常更慢 / 发闷 | **条件开**（P3 异常支） |
| **C** | 回滚/修正 0922 双写或错误门控 | 键族后回归写位 | **否决为主因**；键族验收不回退 |
| **D** | 只调 CM XDamping / SoftZone | 写位已顺仍追不上 | **最后手段** |
| **E** | 室内改回 11.2 | 产品书面推翻 0901 | **禁止治抖**；速另票 |

### 必答

1. **抖与拖慢同一根因？**  
   **否。** 抖 = 纵深路径硬写坐标 / 插值死；拖慢（相对村街）= 0901 平面速分支。可同文件改，须分验收。

2. **最小改动文件**  
   预期集中：`Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs`  
   （`WriteRoot…`、`ApplyVillageWalkPolygonPostCorrection`、障碍分离/保险、必要时 `PostPhysicsResyncDepthCoroutine` 顺序）。**不改**场景相机 YAML、不改 `SceneName` 速白名单，除非产品推翻 0901。

3. **如何验收「带纵深不抖」且「纯左右不回退 0919」**  
   先勾验收 #1（村外纯左右顺）→ 再勾 #2/#3（纯前后/斜向无人+镜台阶抽）→ #4/#5 速分支分开验 → #7 键族不回退。

---

## ⑤ 要改文件（路径级；本阶段未改）

| 全路径 | 预期改动 |
|--------|----------|
| `…/TownPlayerLocomotion.cs` | 方案 A：纵深步减少/合并硬写；多边形与障碍修正优先不杀插值；复核无纵深不追 Y（方案 B 若需要） |
| `…/SceneName.cs` / `ResolveVillagePlanarMoveSpeed` | **默认不动**（0901 保留） |
| `…/PlayerInputComponent.cs` | **不动**（0922 键族已落地） |
| Chief / KenMuNi1 场景 CM YAML | **不动**（方案 D 否决为先手） |
| 其它 Home 场景 | **不误伤**（不开 Town） |

施工说明建议（侦探闭环 + 产品确认 P3 后）：  
`Assets/Doc/施工说明/0922/Village_纵深移动发抖与室内拖慢_施工说明.md`

---

## ⑥ 验收表

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 村外路中间纯左右 | 不抖（0919 不回退） |
| 2 | 村外纯前后 / 斜向（纵深） | 人+镜头无明显台阶抽 |
| 3 | 村长家纯左右 / 纯前后 / 斜向 | 同上 |
| 4 | 村长家平面速 | **默认**对齐 walk≈4.2（0901）；无贴墙异常更慢；若产品改目标另写 OPEN |
| 5 | 村街仍约 11.2 体感 | 不被室内逻辑误伤 |
| 6 | 其它村民家（无 Town） | 不误伤 |
| 7 | 0922 键族验收 1～7 | 不回退（WASD 族 / 方向键族互斥仍成立） |

---

## ⑦ 风险与回滚

| 风险 | 说明 | 回滚 |
|------|------|------|
| 少写硬坐标后穿出 WalkArea | 多边形策略 A 曾依赖 ClosestPoint 钉位 | 保留「越界才 snap」；禁止为顺镜头关掉全部夹紧 |
| 障碍穿模 / 焊死 | 保险与 last-free 依赖硬写 | 纵深自由走少写；命中障碍仍允许 snap |
| 误把室内改回 11.2 | 治不了抖，破坏 0901 | 禁止；速另票 |
| 误动 CM 阻尼 | 0919 已否「抄东郊数字」 | 禁止先手 |
| 回退 0922 键族 | 双通道回归 | 禁止；与写位正交 |
| 权威 Y 与 `vy` 双轨振荡 | 积分领先 + 硬写拉回 | 施工时统一「权威以 rb 或积分其一为准」写清 |

**0901 速是否动**：本票**默认不动**。产品若要求室内手感接近村街 → OPEN Q1，单列施工，不与方案 A 绑死。

---

## 附录 · 强制排除复核

| 勿当主因 | 本票证据 |
|----------|----------|
| 换场 `smoothTime` | 日常跟拍不读；0919 已证 |
| 只改 Chief 相机盒子 | 村外也抖 → 公共 Town |
| DepthSort / Order | 换层 ≠ 位移抖 |
| 室内改回 11.2 治抖 | 0901 设计；与写位无关 |
| 0922 键族为主因 | 只换 Sign 源，未改 WriteRoot |
