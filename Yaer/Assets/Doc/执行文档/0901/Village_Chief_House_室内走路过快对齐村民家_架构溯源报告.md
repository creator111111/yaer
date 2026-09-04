# Village_Chief_House — 室内走路过快对齐村民家 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探 → 施工已落地 S1】仅 Chief 覆写 planar→walkSpeed；村街仍 11.2；未撤白名单 / 未改 Animator  
**Unity**：2020.3.48f1  
**现象**：村长家室内走路 **太快**  
**期望**：与其它 **NPC 村民家室内** 速度一致（Home walk 手感）；**保留** 2.5D 划区 / W/S / 楼梯；**不拖慢** 村街 `Village_KenMuNi1`  
**背景**：0901 划区将 `Village_Chief_House` 加入 `IsVillageExplorationScene` → 开 `Village2_5D`  
**提示词**：`提示词/0901/Village_Chief_House_室内走路过快对齐村民家_架构侦探提示词.md`

---

## 沟通摘要

### ① 结论一句话

**主因 H1：村长家为楼梯开了村街同款 `Village2_5D`，平面目标速吃 `villagePlanarMoveSpeed=11.2`；其它村民家仍 Default+HomeWalk 用 `walkSpeed=4.2`（约 2.7×）。推荐 S1：仅在 Chief_House 把 Town 平面目标速覆写为 walkSpeed 量级，离场恢复 11.2；禁止撤白名单或全局改 11.2。**

### ② 原因（通俗）

其它家里是「室内走」油门（约 4.2）。  
村长家为了能上下楼梯，临时接上了「村里逛街」那套油门（约 11.2），楼梯功能有了，走路也跟村街一样冲。  
要慢下来，只改村长家里的目标时速即可，别把整条村街拖慢，也别拆掉 2.5D。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村长家：体感/测速接近其它 Home 室内 walk（≈4.2），明显慢于改前 | |
| 2 | 纯横 / 纯纵 / 斜向合速度仍一致（0818 归一不坏） | |
| 3 | 出村长家回 KenMuNi1：恢复村街速（≈11.2） | |
| 4 | 其它 HomeScene 仍为原 Home walk，未误伤 | |
| 5 | 楼梯 W/S、WalkArea、禁跳回归 | |
| 6 | 续聊 / 换古莎 / 门换场回归 | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H1**：Chief ∈ 村探索白名单 → `Village2_5D` → `ResolveVillagePlanarMoveSpeed()` = **11.2**；Home 未开 Town → **`SetWalkSpeed()` = 4.2** |
| **方案** | **S1**：室内村模式场景覆写 planar 目标速为 **`walkSpeed` 量级**；KenMuNi1 仍 11.2 |
| **数值来源** | **读 `PlayerMoveComponent` 的 walkSpeed**（经 public getter）；避免魔法数与 Prefab 双源漂移；fallback 常量 **4.2** 仅当读不到时 |
| **Animator** | **本期不动片子**（仍可能 Combat Run 视觉）；只改位移目标速（OPEN Q1） |
| **禁止** | S2 全局改 11.2→4.2；S3 撤白名单；S4 只改动画；`ChangeMoveSpeed` 永久改 Prefab 三项 |

---

## ② 假说证伪（H1～H5）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H1** | Chief 开 2.5D 吃 11.2；家里吃 4.2 | **✅ 主因** | 白名单仅 `KenMuNi1`∪`Chief_House`；Prefab `villagePlanarMoveSpeed=11.2`、`walkSpeed=4.2`、`runSpeed=11.2`；Town FixedUpdate 用 `ResolveVillagePlanarMoveSpeed` 合速 |
| **H2** | 斜向未归一再叠加速 | **否（主因）** | 0818 已在 Town 做平面归一；村长家与村街共用该路径。验收仍须确认改速后欧氏一致 |
| **H3** | timeScale / 测试改速 | **待测排除** | 非架构默认真相；查 AA_TestPanel / timeScale |
| **H4** | 相机造成「看起来快」 | **弱** | 目标速差 2.7× 已足够解释；可用同镜头位移/秒证伪 |
| **H5** | 仅楼梯快、平地不快 | **否（预期）** | 平面目标速全局作用于 Town FixedUpdate，非分区 |

---

## ③ 证据链

### 速度双轨（磁盘）

| 环境 | Locomotion | 位移目标速 | Animator 倾向 |
|------|------------|------------|---------------|
| **村民家室内**（Home1/2/23/45 等） | **Default**（不在白名单） | `HomeWalkState` → `SetWalkSpeed()` → **`walkSpeed=4.2`** | Home Walk |
| **村街 KenMuNi1** | **Village2_5D** | Town `ResolveVillagePlanarMoveSpeed` → **`villagePlanarMoveSpeed=11.2`** | Combat Run + 合速 |
| **村长家（0901 后）** | **Village2_5D**（白名单） | **同村街 11.2** | Combat 轨 + Town |

比值：11.2 / 4.2 ≈ **2.67×**。

### 关键代码

```83:85:Assets/Scripts/Game/Static/Name/Res/SceneName.cs
        public static bool IsVillageExplorationScene(string sceneName)
        {
            return sceneName == Village_KenMuNi1 || sceneName == Village_Chief_House;
```

```307:326:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
            float planarSpeed = ResolveVillagePlanarMoveSpeed();
            // ...
            string planarBranch = ApplyVillagePlanarMoveSpeedNormalization(input, planarSpeed);
```

```496:499:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
        private float ResolveVillagePlanarMoveSpeed()
        {
            return villagePlanarMoveSpeed > 0f ? villagePlanarMoveSpeed : VillagePlanarMoveSpeedFallback;
        }
```

- `HomeWalkState.Enter` → `moveComponent.SetWalkSpeed()`（家里）  
- `CombatRunState` → `SetRunSpeed()`（村模式横移灌速，再被 Town 归一成 planar）  
- Player Prefab：`walkSpeed: 4.2` · `runSpeed: 11.2` · `villagePlanarMoveSpeed: 11.2`  
- Home GSM：`GetCurSceneTerrainType() => IndoorType`，**不**进村白名单 → Town 关  

### Play 日志点（施工/验收）

| 标签建议 | 内容 |
|----------|------|
| `[ChiefMoveSpeed]` | `scene`、`LocomotionMode`、`ResolveVillagePlanarMoveSpeed()`、`moveSpeedX`、`depthVelocity`、欧氏 `sqrt(vx²+vy²)` |

---

## ④ 方案对比

| 方案 | 做法 | 判定 |
|------|------|------|
| **S1 · 室内村模式覆写 planar** | `ResolveVillagePlanarMoveSpeed`：若活动场景为 Chief_House（或小白名单 `IsIndoorVillageExplorationScene`）→ 返回 **walkSpeed**；否则仍 11.2 | **✅ 推荐** |
| S2 · 全局 `villagePlanarMoveSpeed=4.2` | 村街变慢 | ❌ |
| S3 · 撤 Chief 白名单 | 速度回 Home，**丢 W/S 楼梯** | ❌ |
| S4 · 只改 Animator 播 Walk | 位移仍 11.2 | ❌ |
| S5 · GSM 乘系数 | 可；易与 Town 归一打架 | 次选 |

### S1 数值来源（拍板）

| 选项 | 裁定 |
|------|------|
| 读 `PlayerMoveComponent.walkSpeed` | **✅** 与家里同源；须加 **只读公开属性**（现字段 private） |
| 写死常量 4.2 | 仅作 fallback；注释写明与 Prefab walkSpeed 对齐 |
| 新 SerializeField `indoorVillagePlanarMoveSpeed` | 可选 Inspector 微调；默认跟 walkSpeed，防双源则优先读组件 |

### 进/离场恢复

- **无需**手动缓存：按 **每帧/每次 Resolve 读当前活动场景名** 分支即可。  
- 在 Chief → 4.2 量级；切回 KenMuNi1 → 自动 11.2。  
- **替代方案**（不推荐）：`OnEnterScene` 改字段、`OnShutDown` 写回——易漏路径（读档/黑幕切场）。

---

## ⑤ 施工清单

1. `PlayerMoveComponent`：增加 `public float WalkSpeed => walkSpeed;`（或等价只读）。  
2. `TownPlayerLocomotion.ResolveVillagePlanarMoveSpeed`：  
   - 若 `SceneName` 为 `Village_Chief_House`（建议抽 `IsIndoorVillageExplorationScene`，**仅 Chief**，勿扩其它 Home）→ 返回 `WalkSpeed`（≤0 则 4.2）；  
   - 否则原逻辑 `villagePlanarMoveSpeed` / 11.2 fallback。  
3. 详细注释：原因=0901 室内开 Town 误吃村街速；替代=撤白名单（否决）。  
4. 可选短日志开关验速。  
5. **不改**：白名单、WalkArea、KenMuNi1 字段初值、续聊/古莎/出门、Animator 双轨。  
6. 回归：Home 室内、村街、斜向归一、楼梯。  
7. 落盘施工说明 + OPEN。

---

## ⑥ 验收

- [ ] 村长家平面目标速 / 体感 ≈ Home walk（约 4.2）  
- [ ] 纯横 / 纯纵 / 斜向欧氏一致（0818）  
- [ ] 回 KenMuNi1 ≈ 11.2  
- [ ] 其它 Home 未变慢/未开 Town  
- [ ] 楼梯 W/S、WalkArea、禁跳 OK  
- [ ] 续聊 / 换古莎 / 门换场 OK  

---

## ⑦ OPEN

| ID | 问题 | 决议 / 默认 | 状态 |
|----|------|-------------|------|
| Q1 | 同期改 Animator 为 Home Walk 片？ | **本期否**；只改 planar；若仍「像跑」另案 | ✅ |
| Q2 | 白名单扩到其它将来开 2.5D 的 Home？ | **本期仅 Chief_House** | ✅ |
| Q3 | 室内目标速是否允许 Inspector 另调？ | 默认同 walkSpeed；另字段可选 | ⏳ |
| Q4 | H3 测试面板改速？ | 验收前排除 | ⏳ |

---

## ⑧ 程序补充

### 关键锚点

| 符号 | 说明 |
|------|------|
| `IsVillageExplorationScene` | 为何 Chief 开 Town |
| `ResolveVillagePlanarMoveSpeed` | S1 唯一覆写点 |
| `ApplyVillagePlanarMoveSpeedNormalization` | 0818 合速；改目标速后斜向仍走此函数 |
| `HomeWalkState` / `CombatRunState` | 家里 4.2 vs 村里灌跑速 |
| 0818 报告 | 「村民家不会开 Town」— 0901 后 **Chief 例外** |

### 硬禁止

- 关闭 `IsVillageExplorationScene(Chief_House)` 降速  
- 全局 `villagePlanarMoveSpeed` / `runSpeed` → 4.2  
- `ChangeMoveSpeed` 永久改 Player Prefab 冒充只修村长家  
- 改 WalkArea2 / 进场飞出另案混改  

### 与「不是」对齐

| 不是 | 说明 |
|------|------|
| 拖慢村街 | KenMuNi1 仍 11.2 |
| 关掉划区 | 2.5D / 楼梯保留 |
| 重做 Home/Combat 双轨 | 本期只动 planar 目标速 |
