# ForestScene · 玩家跳出整个场景 — 架构溯源报告

**文档版本**：v1.3（2026-07-22）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码**）  
**触发现象**：`ForestScene` 普通跳跃后物理飞出可玩场景  
**依据**：静态代码 + Forest 碰撞；运行时 Inspector 多轮截图（见 §②）

---

## ① 结论（一句话）

**根因已钉死：人在半空，`Is Grounded` 仍为 true → 自定义重力整段不执行 → `Velocity.y` 卡在跳跃初速 60 → 直飞出场景。自定义 `Gravity=(-300)` 与 `Can Gravity` 都正常；RB `Gravity Scale=0` 是设计。地面误判来自 `CapsuleGroundChecker` 用「身体旁重叠」当落地，且 `GroundLayerMask` 含 Default(0)，Forest 的 `LeftWall`/`RightWall`（Default、高约 20）被当成地面——截图时人在 X≈203 右缘，正好贴墙。**

生活类比：脚底感应器不朝下看地板，旁边蹭到墙也算「站着」。跳起来贴着墙，系统还以为站地上，就不踩刹车（重力），油门（向上 60）一直挂着，人就飞出场地。

---

## ② 运行时证据（已齐）

| 来源 | 读数 | 解读 |
|------|------|------|
| Player Transform | Y≈3.56～7.77；**X≈203** | 半空 + **贴右缘**（墙/门区） |
| RB Gravity Scale | **0** | **设计**；不是根因 |
| Move `Gravity` | **(0, -300)** | 自定义重力数值正常 |
| Move `Can Gravity` | **勾选** | 开关开着，但被 IsGrounded 挡掉 |
| Move **`Is Grounded`** | **勾选（半空仍 true）** | **根因开关** |
| Move **`Velocity`** | **(18, 60)** | 恰为 `SetParabolaSpeed(6,7.2)` 初速；**从未被重力减速** |

推算：`time=sqrt(2*6/300)=0.2` → `vy=60`，`vx=7.2/(2*0.2)=18`。与截图完全一致 → 跳了一次，之后重力分支一次都没进。

---

## ③ 因果链（完整）

```
空格跳跃
  → JumpAddSpeed → SetParabolaSpeed(6, 7.2)
  → Velocity = (18, 60)

每帧 FixedUpdate（MoveComponent）:
  GroundCheck()
    → CapsuleGroundChecker.GroundCheck()
    → Physics2D.CapsuleCast(中心, size(2,0.3), Horizontal, 方向右, 距离0, mask)
    → 距离 0 ≈ 身体旁重叠检测（不是朝下探地板）
    → mask 含 Default(0) + GroundCenter(14) + Interactive(20)
    → 贴 RightWall/LeftWall（Layer Default，盒高 20）→ hit ≠ null
    → IsGrounded = true   ← 半空仍成立

  if (!IsGrounded && canGravity)   ← IsGrounded 为 true，整段跳过
      Velocity += dt * Gravity;    ← 永不执行

  rg.velocity = Velocity;          ← 一直灌 (18, 60)
  → 匀速冲出场景（MapLimit 还是关的，无天花板兜底）
```

关键代码：

```167:171:Assets/Scripts/Game/GameRuntime/Entities/Component/Move/MoveComponent.cs
            GroundCheck();
            if (!IsGrounded && canGravity)
            {
                Velocity += Time.fixedDeltaTime * Gravity;
            }
```

```64:68:Assets/Scripts/Game/GameRuntime/Entities/Component/PhysicsDetect/GroundCheck/CapsuleGroundChecker.cs
        public override bool GroundCheck()
        {
            var hit = Physics2D.CapsuleCast(CapsuleCenter, CapsuleSize, CapsuleDirection, 0, Vector2.right, 0, GroundLayerMask);
            return hit.collider != null;
        }
```

Forest 墙体（Default = 在 mask 内）：

| 物体 | Layer | 碰撞盒 | Tag |
|------|-------|--------|-----|
| `LeftWall` | **0 Default** | 约 2×**20** | Wall |
| `RightWall` | **0 Default** | 约 2×**20** | Wall |
| `RightDoor` 等 | 0 Default | 约 2×20（Trigger） | MapDoor |

玩家截图 **X≈203** 落在右缘墙/门带 → 与「贴墙误判 grounded」吻合。

---

## ④ 检查清单（结案）

| # | 项 | 结果 |
|---|-----|------|
| 1 | JumpForce / PlayerMovement 被改 | 无；`jumpHeight=6` 只决定初速 60 |
| 2 | RB Gravity Scale | **0 = 设计**；已排除为根因 |
| 3 | 自定义 Gravity / Can Gravity | **运行时正常**；已排除 |
| 4 | SO / 进场改参 / 相机 | 已排除为飞出场景根因 |
| 5 | **半空 Is Grounded** | **成立 = 根因** |
| 6 | 墙体进地面 Mask | **成立 = 误判来源** |

---

## ⑤ 给施工员的修复建议（本报告不改代码）

**主修（推荐组合）：**

1. **地面检测只朝下**  
   `CapsuleGroundChecker.GroundCheck` 改为向下 `Raycast` / `CapsuleCast(Vector2.down, 短距离)`，禁止用「水平距离 0 的旁侧重叠」当落地。

2. **收紧 `GroundLayerMask`**  
   去掉 **Default(0)**；只保留 `GroundCenter` / `GroundUp` / `GroundDown` / `GroundCommon` 等地面层。  
   墙应走 Wall 用途，不应进落地 Mask。

3. **场景侧（可选加固）**  
   `LeftWall`/`RightWall` 若不需要参与落地，移出 Default，或确保不在落地 Mask 内。

4. **验收**  
   - 贴左右墙普通跳：半空 `Is Grounded=false`，`Velocity.y` 应递减。  
   - 场地中央跳：照常落地。  
   - 开门 Trigger 旁跳跃：不得当 grounded。

**禁止：**

- 把 RB `Gravity Scale` 改成 1（双重力）  
- 只降 `jumpHeight` 当飞出场景的修复（治标不治本；贴墙仍会飞）  
- 在 `ForestSceneManager` 写临时跳跃补丁

**可选兜底：** 启用/修正 `MapLimit`；或起跳时强制 `IsGrounded=false`（治标，仍建议修检测）。

**验收日志建议：**  
`[GroundDebug] grounded={0} hit={1} playerY={2} vy={3}`

---

## ⑥ OPEN_QUESTIONS

| ID | 问题 | 状态 |
|----|------|------|
| Q1 | 相机是根因？ | ✅ 否 |
| Q1b | RB Gravity Scale=0 是根因？ | ✅ 否（设计） |
| Q2 | 半空 IsGrounded + vy=60？ | ✅ **已证实** |
| Q3 | 误判是否贴墙触发？ | ✅ **高度吻合（X≈203 + 高墙 Default）**；施工时建议 Log `hit.name` 再确认一次 |
| Q4 | 是否另调 jumpHeight 手感？ | 与本案解耦；可选 |

---

## ⑦ 关键文件

| 路径 | 角色 |
|------|------|
| `CapsuleGroundChecker.cs` | 误用旁侧 CapsuleCast；**主修点** |
| `MoveComponent.cs` | `!IsGrounded && canGravity` 才施重力 |
| `Player.prefab` | GroundLayerMask 含 Default；Capsule 参数 |
| `ForestScene.unity` | `LeftWall`/`RightWall` Layer0 高 20 |
| `JunpUpState.cs` / `PlayerMoveComponent.cs` | 只负责给出初速 (18,60)，行为正确 |

---

**文档路径**：`Assets/Doc/执行文档/0722/ForestScene_普通跳跃跳出屏幕_架构溯源报告.md`

| 版本 | 日期 | 说明 |
|------|------|------|
| v1.0 | 2026-07-22 | jumpHeight + 相机（已撤回） |
| v1.1 | 2026-07-22 | 飞出场景；查重力失效 |
| v1.2 | 2026-07-22 | Gravity Scale=0 为设计；Y 飞高 |
| v1.3 | 2026-07-22 | **结案**：半空 IsGrounded + 墙体进 Default Mask + 旁侧检测 |
| v1.4 | 2026-07-22 | **已施工（解耦）**：`CapsuleGroundChecker` 只向下探；Prefab Mask 去掉 Default；**未改** `TownPlayerLocomotion`。见同目录施工说明 |
