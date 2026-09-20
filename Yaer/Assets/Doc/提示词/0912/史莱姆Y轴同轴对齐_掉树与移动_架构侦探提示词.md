# Cursor Agent Prompt · 史莱姆 Y 轴上下偏差：掉树 / 场景移动须与玩家同轴

> **角色**：先【架构侦探】只读溯源；根因拍板后再【施工员】最小化修复  
> **日期**：2026-09-12  
> **现象（用户实测）**：  
> 1. 史莱姆 **从树上掉下来** 时，相对玩家有 **Y 轴上下偏差**  
> 2. 史莱姆在 **场景中移动** 时，同样有 **Y 轴上下偏差**  
> **产品期望（钉死）**：史莱姆与玩家在 **同一条水平战斗轴线上**（同一地面 Y / 同一「前后」层），**不要**再出现相对玩家的上下错位；掉落落地后、巡逻/追击移动中都应保持同轴  
> **不是**：改村庄 2.5D 玩家纵深（`TownPlayerLocomotion`）；改史莱姆攻击数值/血量；改 0723「站上卡住」物理案（可对照碰撞，但本案是 **Y 对齐**，不是卡死）  
> **场景（须侦探钉死）**：优先查 `ForestEastScene`（树上史莱姆 / 树洞一带）；若 ForestScene 也有同 prefab 行为，一并写影响面  
> **报告落盘**：`Assets/Doc/执行文档/0912/史莱姆Y轴同轴对齐_掉树与移动_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 史莱姆掉下来、在地上跑的时候，看起来会比玩家高一截或低一截，不在一条线上。  
> 现在不要这种上下错位：**和玩家站同一条轴**（横版同地面高度），掉完、走完都对齐。

### 术语钉死

| 词 | 本案含义 |
|----|----------|
| **Y 轴偏差 / 上下偏差** | 史莱姆 `transform.position.y`（或脚底/精灵视觉中心）相对玩家 **地面站立 Y** 有可见偏移，不是跳跃抛物线过程中的瞬时差 |
| **同一轴线** | 落地后与玩家 **同地面 Y**（允许极小落地误差）；移动时 **不主动改 Y 去追纵深**；视觉上站同一条「跑道」 |
| **掉树** | 树上休眠/触发 → `Born` 子状态机下落（`SlimeBornFallState` 等）→ 落地 |
| **场景移动** | Idle/Move 巡逻、索敌追击、逃跑等地面移动 |

> 生活类比：横版格斗里双方应站同一条地板线；现在史莱姆像站在另一条平行跑道上，要并回一条。

### 现网线索（助手预扫 · 须核实）

```
掉树：
  触发出生/惊醒 → SlimeBornSubSM
    → SlimeBornFallState：canGravity=true；SetVelocity(0, FallSpeed)
    → downY = position.y + BornDownY（Prefab 预扫 bornDownY: -10）
    → IsGrounded → SlimeBornDownState → 之后进 Idle/Move

地面移动：
  SlimeMoveState：索敌后 TurnToRight → MoveLeft/MoveRight（主要写 moveSpeedX）
  MoveComponent.OnFixedUpdate：未落地则 Gravity 累加 Velocity.y
  DepthComponent：按 Foot bounds / position.y 做 IsInSameDepth、sortingOrder
  旧注释：曾有「相同纵深」判断 + 寻路 GetPathfindingPos（现多已注释）
```

| 嫌疑 | 预扫 | 若成立的体感 |
|------|------|--------------|
| **A. 场景摆放 / 掉落落点地面高度 ≠ 玩家走道 Y** | 树上实例初始 Y 高；落地碰到另一层 Ground / 平台 | 掉完就偏，之后一直偏 |
| **B. `bornDownY` / 落地判定** | Prefab `bornDownY: -10`；落地改靠 `IsGrounded`，旧 `y < downY` 已注释 | 落点与设计线不一致 |
| **C. 脚底碰撞 / GroundCld / Pivot** | 0723：史莱姆有大盒 `GroundCld`；精灵 pivot 与脚底不一致 | Transform 同 Y 仍「看起来」上下偏，或落地悬浮 |
| **D. 移动时 Y 被改写** | Move 主路径只 Turn X；但 Gravity、JumpAtk 落地、`SetVelocity`、物理挤开可能改 Y | 跑着跑着偏高/偏低 |
| **E. 残留「纵深」设计** | `DepthComponent` / 注释掉的 `IsInSameDepth` 追击 | 故意不同 Y；产品现要求取消 |
| **F. 追击用三维 `normalized` 含 Y** | `targetDir = (playerPos - slimePos).normalized` 后只用 `targetDir.x` 转向 | 一般不直接写 Y；仍须确认无别处用 `targetDir.y` |

### 侦探须回答的核心问题

1. **复现钉死**：哪个场景、哪只树上史莱姆、玩家站立 Y vs 史莱姆落地后 Y（World）差多少？移动 N 秒后差值是否变大？  
2. **掉树链**：Born 全链路谁决定最终 Y？地面 Layer、`BornDownY`、`IsGrounded`、落地后是否 Snap 到玩家 Y？  
3. **移动链**：Idle/Move/Escape/JumpAtk 结束是否写 Y？Gravity 在已落地时是否仍改 Y？有无每帧对齐或刻意保持纵深差？  
4. **「同轴」权威定义**：以玩家 `transform.y`、Foot bounds、还是场景一条 Ground 标尺为准？报告须拍板一种，供施工使用。  
5. **修复方案（≥2，只方案）**：  
   - 落地/移动时 **锁 Y = 玩家地面 Y**（或场景轴线常量）  
   - 只改场景摆放与落地平台，使自然落到同 Y  
   - 冻 `Rigidbody2D.constraints` 的 Y（落地后）+ 禁止 Move 写 Y  
   列利弊：对 JumpAtk 跳跃、Depth 排序、`IsInSameDepth` 攻击判定的影响；推荐一种最小化路径。

### 必读 / 扫描范围

**必读**

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md`（坐标系：森林/战斗为横版；勿与村庄 Y=纵深混淆——本案是 **战斗同轴**）  
3. `Assets/Doc/执行文档/7月/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`（碰撞结构对照，非本案目标）  
4. `Slime.cs`、`SlimeBornFallState.cs`、`SlimeMoveState.cs`、`MoveComponent.cs`、`DepthComponent.cs`  
5. Prefab：`Assets/GameRes/Prefabs/Entity/Monster/Slime.prefab`（`bornDownY` 等）  
6. 本提示词预梳理  

**扫描**

- `ForestEastScene`（及必要时 `ForestScene`）树上史莱姆实例初始 Transform / 所属地面  
- Born 子状态：`SlimeBornSubSM`、`SlimeBornDownState`、触发 `triggerBornRange`  
- JumpAtk 落地是否留下 Y 残差  
- `IsInSameDepth` 在攻击判定中的用法（对齐 Y 后是否反而修/坏攻击）  
- 是否存在「史莱姆对齐玩家 Y」的旧 API / 注释方案可复用  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity 2020.3.48f1 / C#。只读；**禁止**改代码、Prefab、场景、Git。

### 输出

写到：`Assets/Doc/执行文档/0912/史莱姆Y轴同轴对齐_掉树与移动_架构溯源报告.md`

固定结构：

1. **结论一句话**（掉树偏 / 移动偏是否同源；根因）  
2. **调用链**（掉树一条 + 地面移动一条；标出写入 Y 的每一处）  
3. **证据表**（World Y 差值、字段、代码行；已证实 / 可疑 / 已排除）  
4. **「同轴」权威定义**（施工必须遵守的一句话）  
5. **修复方案对比**（≥2）+ 推荐 + 验收标准 + 对跳跃攻击/Depth 的风险评估  
6. 设计不清记入 `Assets/Doc/OPEN_QUESTIONS.md`

### 限制

- 不改代码；不提交 Git  
- 不把村庄 `TownPlayerLocomotion` 纵深方案套到森林史莱姆（除非证据证明共用且必须）  
- 默认中文；大白话 + 路径/字段名  

---

## 【施工员】Prompt（侦探报告拍板后再复制；现在不要执行）

> **前置**：报告已钉死写入 Y 的位置，并确认「同轴」权威（玩家 Y / 场景标尺）。  
> **目标**：树上掉落落地后、以及场景地面移动时，史莱姆与玩家无可见上下错位（同战斗轴线）。  
> **优先**：最小化——优先锁地面态 Y 或落地 Snap，避免重写整套 AI；保持 JumpAtk 跳跃过程可用，落地后回到同轴。  
> **禁止**：在 Update 堆砌无注释的 Y 魔法数；顺手改村庄纵深；为对齐而关掉全部重力导致无法掉树。  
> **文档**：`Assets/Doc/施工说明/0912/史莱姆Y轴同轴对齐_施工说明.md`  
> **验收**：  
> 1. 树上史莱姆掉落后与玩家站同一水平线（目视 + 可选 Debug 打 Y）  
> 2. 追击/巡逻移动 ≥10s 不出现相对玩家持续上下漂  
> 3. 普通攻击 / 跳跃攻击仍可打中（`IsInSameDepth` 不误伤）  
> 4. 不回归 0723「站上卡住」  
