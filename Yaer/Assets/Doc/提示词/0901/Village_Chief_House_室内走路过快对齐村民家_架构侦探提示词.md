# Cursor Agent Prompt · 村长家室内走路过快 · 对齐其它村民家室内速度

> **角色**：先【架构侦探】只读定根因与方案，报告后再【施工员】最小修复  
> **日期**：2026-09-01  
> **现象（用户）**：玩家在 **村长家**（`Village_Chief_House`）里面走路 **太快**  
> **期望（产品钉死）**：与 **其它 NPC 村民家室内** 走路速度 **保持一致**（手感对齐 Home 室内，不是村街）  
> **背景**：0901 已为楼梯/划区把 `Village_Chief_House` 加入村探索白名单并开 `Village2_5D`  
> **本阶段（侦探）**：只读；禁止改场景 / 代码 / Prefab  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_Chief_House_室内走路过快对齐村民家_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品钉死

| 项 | 要求 |
|----|------|
| 场景 | 仅 **`Village_Chief_House`** 室内手感（本期）；村街 `Village_KenMuNi1` **不得**被拖慢 |
| 对标 | **其它村民家室内**（如 HomeScene1/2/23/45）：现网 **Home + `walkSpeed`** |
| 保留 | 村长家 **2.5D 划区 / W/S / 楼梯 / WalkArea** 功能不回退 |
| 不改 | 出屋送树屋台本、续聊、古莎换人、WalkArea2 形状 |

### 速度双轨（预扫 · 须再证）

| 环境 | Locomotion | 动画轨 | 平面目标速（Prefab 预扫） |
|------|------------|--------|---------------------------|
| **村民家室内**（未开村模式） | Default | **Home** `HomeWalkState` → `SetWalkSpeed()` | **`walkSpeed = 4.2`** |
| **村街 KenMuNi1** | `Village2_5D` | **Combat** `CombatRunState` + Town 合速 | **`villagePlanarMoveSpeed = 11.2`**（≈ `runSpeed`） |
| **村长家（0901 后）** | **已开 `Village2_5D`**（白名单） | 倾向 Combat + Town | **同村街 11.2** ← 体感「太快」高概率主因 |

文档旁证：`执行文档/8月/0818/村庄斜向移动速度叠加_架构溯源报告.md` 曾写「村民家不会开 Town」；**0901 划区后村长家例外开了 Town**，室内却仍吃村街跑速。

```
其它村民家：IndoorType + 非白名单 → Home Walk → 4.2
村长家：    IndoorType + 白名单    → Village2_5D + planar 11.2  → 比家里快约 2.7×
```

### 方案倾向（侦探拍板，可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **S1 · 室内村模式场景覆写 planar 目标速** | 进 `Village_Chief_House`（或「室内+Village2_5D」白名单）时，Town 目标速改用 **`walkSpeed`（4.2）** 或独立字段初值 4.2；离场恢复 11.2 | ✅ 保楼梯 2.5D，对齐家里速；**不改村街** |
| S2 · 全局把 `villagePlanarMoveSpeed` 改成 4.2 | 村街一并变慢 | ❌ |
| S3 · 撤掉 Chief_House 村模式白名单 | 速度变回 Home，但 **丢 W/S 楼梯** | ❌ |
| S4 · 只改 Animator 播 Walk、不改数值 | 动画像走、位移仍 11.2 | ❌ 不满足「速度」 |
| S5 · GSM/场景乘系数 | 可；须防与 Town 归一公式打架 | 次选 |

**推荐默认 S1**：在 `TownPlayerLocomotion.GetPlanarTargetSpeed()`（或等价入口）按 **活动场景名**（或小白名单 `IsIndoorVillageExplorationScene`）返回室内目标速；数值对齐 `PlayerMoveComponent.walkSpeed`（读组件或常量 4.2，侦探须定一种，避免双源漂移）。

### Animator 是否同期改（OPEN）

村长家开 2.5D 后可能仍走 **Combat Run 片子**，家里是 **Home Walk**。  
产品本次只提 **速度**；默认 **本期只改 planar 数值**；若验收仍觉「像在跑」，另开「室内村模式强制 Home 片 / 降 Animator.speed」任务，**勿**与本案绑死大改双轨。

### 假说表（须逐条证伪）

| ID | 假说 | 怎么证伪 |
|----|------|----------|
| **H1** | Chief 开 `Village2_5D`，平面速吃 `villagePlanarMoveSpeed=11.2`；家里吃 `walkSpeed=4.2` | 进房打日志：LocomotionMode、GetPlanarTargetSpeed、moveSpeedX |
| **H2** | 斜向未归一又叠加速（0818 已修村街） | 纯 A/D vs 斜向欧氏速度是否仍一致 |
| **H3** | Time.timeScale / 测试面板改过速 | 查 AA_TestPanel ChangeMoveSpeed、timeScale |
| **H4** | 相机跟随/FOV 造成「看起来快」 | 对比同镜头下位移/秒 |
| **H5** | 仅楼梯带快、平地不快 | 分区测速 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 定为何比其它村民家快 | ❌ 改 KenMuNi1 村街手感 |
| ✅ 最小方案对齐室内 `walkSpeed` 量级 | ❌ 关掉 WalkArea / 白名单逃避 |
| ✅ 写清进/离村长家速度恢复 | ❌ 重做 Home/Combat 双轨 |
| ✅ OPEN：动画片子是否另案 | ❌ 改 WalkArea2 / 进场飞出另案（除非同文件误伤） |

### 严禁

- 为降速关闭 `IsVillageExplorationScene(Chief_House)`  
- 把全局 `villagePlanarMoveSpeed` / `runSpeed` 改成 4.2  
- 用 `ChangeMoveSpeed` 永久改 Player Prefab 三项速冒充「只修村长家」  

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。只读；禁止改场景/代码/Prefab。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/8月/0818/村庄斜向移动速度叠加_架构溯源报告.md
@Assets/Doc/执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md
@Assets/Doc/施工说明/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_施工说明.md

## 产品
村长家室内走路太快；应与其它 NPC 村民家室内速度一致。
保留村长家 2.5D 划区与楼梯；不得拖慢村街 KenMuNi1。

## 必读
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
（villagePlanarMoveSpeed / GetPlanarTargetSpeed / 合速）
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/PlayerMoveComponent.cs
（walkSpeed / runSpeed / SetWalkSpeed / SetRunSpeed）
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Home/HomeWalkState.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Combat/State/Ground/CombatRunState.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
（SetVillageExplorationMode / RefreshVillageExplorationFromActiveScene）
@Assets/Scripts/Game/Static/Name/Res/SceneName.cs
（IsVillageExplorationScene）
@Assets/GameRes/Prefabs/Entity/Player/Player.prefab
（walkSpeed / runSpeed / villagePlanarMoveSpeed）
对照样板 GSM：任意 Village_HomeScene*SceneManager（未开村模式）
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs

检索：villagePlanarMoveSpeed、walkSpeed、SetWalkSpeed、SetRunSpeed、
Village2_5D、IsVillageExplorationScene、Village_Chief_House、HomeWalkState。

## 侦探任务
1. 量：家里 vs 村长家 vs 村街，平面目标速与实际 |v|（可写「应在 Play 验证」的日志点）。
2. 证伪 H1～H5；确认主因是否「室内开了村模式却仍用 11.2」。
3. 方案对比 S1～S5，推荐默认；定目标数值来源（读 walkSpeed vs 常量 4.2）。
4. Animator 是否本期动：写入 OPEN，默认不动片子只改速。
5. 施工清单：改哪些 API/白名单/进出恢复；回归村街与其它 Home。
6. 验收标准：村长家左右/斜向欧氏速度 ≈ 其它村民家 walk 手感；村街仍约 11.2。

## 报告落盘
Assets/Doc/执行文档/0901/Village_Chief_House_室内走路过快对齐村民家_架构溯源报告.md

结构：①结论一句话 ②原因 ③用户检查清单 ④证据链 ⑤方案对比 ⑥施工清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_室内走路过快对齐村民家_架构溯源报告.md

## 目标
Village_Chief_House 内玩家平面移动速度对齐其它村民家室内（walkSpeed 量级）；
保留 2.5D WalkArea / W/S / 楼梯；Village_KenMuNi1 村街速度不变。

## 约束
- 禁止撤白名单 / 关 ClosestPoint 来「变慢」
- 禁止全局改 villagePlanarMoveSpeed / runSpeed 为 4.2
- 禁止改 WalkArea2、续聊、古莎、出屋送树屋
- 进村长家覆写、离场必须恢复村街目标速
- 生成代码含详细注释；重要修改写明原因；复杂逻辑注明替代方案（见报告）

## 落盘
Assets/Doc/施工说明/0901/Village_Chief_House_室内走路过快对齐村民家_施工说明.md
同步 OPEN_QUESTIONS.md。

## 验收
- [ ] 村长家：体感/测速接近其它 Home 室内 walk（约 4.2），明显慢于改前
- [ ] 纯横 / 纯纵 / 斜向合速度仍一致（不破坏 0818 归一）
- [ ] 出村长家回 KenMuNi1：速度恢复村街（约 11.2）
- [ ] 其它 HomeScene 仍为原 Home walk，未误伤
- [ ] 楼梯 W/S、WalkArea、禁跳回归
- [ ] 续聊 / 换古莎 / 门换场回归

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者（一句话）

村长家为了楼梯开了「村街那套 2.5D」，速度也跟村街跑速（约 **11.2**）走了；其它村民家仍是室内走速（约 **4.2**）。提示词要求：**只在村长家把平面目标速降到家里量级，村街不动、楼梯功能保留。**
