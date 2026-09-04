# Cursor Agent Prompt · 村长家上楼巨树 2 楼：卡在 WalkArea2 动不了 / 未稳落 ExitFrom

> **角色**：先【架构侦探】只读溯源；报告拍板后再【施工员】 / 必要时【验收员】加 `[Village2f]` 日志  
> **日期**：2026-09-03  
> **场景链路**：`Village_Chief_House`（楼梯上楼换场）→ `Village_KenMuNi1` 巨树 **2 楼**  
> **现象（用户 + Scene 截图）**：  
> 1. 从村长家上树屋二楼后**一下子卡住，主动不了**  
> 2. **没有成功**落到 / 站稳在 **`ExitFrom_HomeSceneChief2f`**  
> 3. **无法在 `VillageWalkArea2` 里面移动**（截图可见枝干上绿多边形 = WalkArea2 线框，人站在区内或贴边）  
> **产品期望（钉死）**：楼梯换场进村后，脚落在 `ExitFrom_HomeSceneChief2f` 附近；生效可走区为 **`VillageWalkArea2`**；在区内可正常村式 A/D+W/S，**不卡死、不被吸回 1 楼**  
> **上游已施工（须验收、勿当已通过）**：0901 楼梯换场 + **W1** Override 绑 WalkArea2 + E3′ 大门拆键  
> **不是**：改 `VillageWalkArea2` 多边形点集/尺寸「腾地」；不是改宝箱案（宝箱「看不见/未摆」另案 `提示词/0903/…宝箱看不见未摆放_验收排查提示词.md`）；不是改村长家室内划区主链；不是回退村模式白名单  
> **并行**：0903 宝箱可见性验收与本案解耦——卡住可解释「走不到开箱」，不解释「Hierarchy/Scene 完全无箱」  
> **报告落盘**：`Assets/Doc/执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 从村长家走楼梯上去，应该出现在巨树 2 楼那个出口点，人能在 2 楼树枝可走区里正常走。  
> 现在一上去就卡死，也没好好站在出口点上，2 楼地板区等于废了。

### 期望时序（0901 已定案）

```
Village_Chief_House
  → StairsDoor_ToTree2f（TriggerWhenMoveIn + 黑幕）
  → LastSceneName = Village_Chief_House（楼梯门 enterPosKey 空）
  → LoadScene(Village_KenMuNi1)
  → EnterPos：Village_Chief_House → ExitFrom_HomeSceneChief2f ≈ (-159.34, 41.66, 0)
  → Village_KenMuNiSceneManager.SetPlayerPos
       → base 落点
       → W1：last==Village_Chief_House → SetVillageWalkAreaOverride(VillageWalkArea2)
       → FlushAuthoritativeVillageTransform…
  → 区内可走；ClosestPoint 只夹 WalkArea2，不夹 1 楼 VillageWalkArea
```

### 截图锚点（用户 Scene 视图）

| 可见物 | 预扫含义 |
|--------|----------|
| 枝干上绿色封闭多边形 | 高度可疑 = **`VillageWalkArea2`** PolygonCollider2D Gizmo |
| 角色脚附近矩形绿框 | 玩家 Collider；脚似贴在多边形上沿 / 略偏内 |
| 场景非 2D 视图按钮态 | 仍是 2.5D 村语义；Z 宜为 0 |
| 未见明确 ExitFrom Gizmo | 落点物体可能在左侧/未选中；侦探须对 Transform 距离 |

**用户原话对照**：卡主动不了 + 未到 ExitFrom + WalkArea2 内不能动 → 优先拆成「**落点错**」与「**绑区错 / 夹死 / 障碍焊死 / 输入锁**」两条线，勿只猜一个。

### 现网机制（助手预扫 · 高度可疑）

| 环节 | 磁盘现网 | 若失败的体感 |
|------|----------|--------------|
| **W1** | `Village_KenMuNiSceneManager.TryBindVillageWalkArea2AfterChiefStairsLanding`：仅 `LastSceneName == Village_Chief_House` 才 `SetVillageWalkAreaOverride(WalkArea2)` | 仍绑 **`VillageWalkArea`（1 楼）** → ClosestPoint 把人往 Y≈-6 吸 / 与当前 Y≈41 撕扯 → **卡死或瞬移** |
| EnterPos | `lastScene: Village_Chief_House` → `ExitFrom_HomeSceneChief2f` | 落错点 / 落区外 → 夹区抽搐 |
| E3′ | 1 楼 `LeftDoor` 用 `enterPosKey=Village_Chief_House_Door` **不**绑 WalkArea2 | 若楼梯误填了 Door 键 → 落 1 楼门前且不绑 2（与「站在树枝上」可能不符，须证伪） |
| ClosestPoint | `TownPlayerLocomotion` 每帧夹有效多边形；凹角有渐进逼近 | 形外 / 凹坑 / Override 空 → 焊死或飞点 |
| 障碍挤出 | 0819 围栏穿模：壳内 vx=0 + 只沿 Y 挤 → **卡死不动** | 2 楼枝干旁若有 `VillageWalkObstacle` 重叠脚 |
| 权威 Flush | W1 后 `FlushAuthoritativeVillageTransformAfterSceneDepthInject` | 与 SetPos / Rb / `_villageWorldY` 不同步时出现「看得见在 2 楼、物理仍错」类 0901 进屋吸楼梯同构 |

### 假说表（须并列证伪，按优先级写进报告）

| ID | 假说 | 证伪手段 |
|----|------|----------|
| **H1** | **W1 未生效**：Override 仍空，生效多边形仍是 1 楼 `VillageWalkArea` | Console 无 `[Village2f] 已 SetVillageWalkAreaOverride`；读 `ResolveEffectiveWalkPolygon` / override 引用名；Pause 看脚被夹向的目标 Y |
| **H2** | **落点未到 ExitFrom**：SetPlayerPos / EnterPos 未命中或随后被 Flush/ClosestPoint 拽走 | 进场后脚世界坐标 vs `ExitFrom_HomeSceneChief2f`；距离与 OverlapPoint(WalkArea2) |
| **H3** | **落在 WalkArea2 形外或贴凹边**：ClosestPoint 每帧抽 → 看起来「区里也走不动」 | `OverlapPoint`；凹角渐进路径日志 |
| **H4** | **脚与 VillageWalkObstacle 重叠焊死**（0819 同构） | Foot Overlap 障碍层；vx 被置 0；挤出失败 |
| **H5** | **输入/全局锁**：`isTalking` / Pause / 禁止操作 / 剧情未还控 | `GameManager` 对话锁、GSM 输入组件、黑幕未关 |
| **H6** | **楼梯门配置脏**：`enterPosKey` 误填 Door、未进 `sceneObjs`、Setup 未跑、NextScene 错 | 查 `StairsDoor_ToTree2f` Inspector vs 施工说明 |
| **H7** | **LastSceneName 时机**：卸场后 Last 不是 `Village_Chief_House` → W1 early-return | `[SceneLoad]` / ChangeScene 记 Last 日志 |
| **H8** | **权威 Y / Rb 与 Transform 不同步**（进屋吸楼梯同构） | SetPos 后 transform vs Rb vs `_villageWorldY` |

### 方案倾向（仅倾向，侦探可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1** | 修 W1 触发条件 / 调用时机（保证楼梯路径必绑 WalkArea2，且在首次 ClosestPoint 前） | ✅ 若 H1/H7 |
| **F2** | 落点权威传送对齐 ExitFrom（Transform+Rb+权威 Y），必要时 defer ClosestPoint 至有效落点后 | ✅ 若 H2/H8（对齐 0901 进屋 F1/F2） |
| **F3** | 障碍挤出/进区分离（复用 0819 保险，勿只加厚 Collider） | ✅ 若 H4 |
| **F4** | 修楼梯门序列化 / 补跑 Setup / 清错误 enterPosKey | ✅ 若 H6 |
| **F5** | **改 WalkArea2 点集/尺寸** | ❌ **严禁当主修**（0901 锁定） |
| **F6** | 关 ClosestPoint / 撤村白名单 | ❌ |

### 与相关案边界

| 案 | 关系 |
|----|------|
| 0901 楼梯上楼换场巨树 2 楼 | **本现象 = 该案验收失败面**；先证 W1/落点是否真通，再最小补丁 |
| 0901 进屋落点吸楼梯 | Flush/ClosestPoint 时序 **同构参考**，场景不同勿照搬改 Chief |
| 0819 树屋围栏穿模卡住 | 若 H4 成立可复用保险思路 |
| 0901 WalkArea2 宝箱 | **正交**；人都能走再谈箱；禁止为走路改多边形腾地 |
| 出村长家送树屋戏 | 大门键路径；**勿**与楼梯 2f 路径混修 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 证 H1～H8；钉死「落点 / 绑区 / 夹死 / 障碍 / 输入锁」哪条主因 | ❌ 改 `VillageWalkArea2` 形状 |
| ✅ 最小修复建议对齐现网 W1 + EnterPos + Town | ❌ 新造第三套 2 楼移动 |
| ✅ 验收清单含 Console `[Village2f]` / 坐标距离 | ❌ 顺手改宝箱、续聊、DayLight |
| ✅ 更新 OPEN：2 楼可达 + W1 验收状态 | ❌ 关 ClosestPoint 当根治 |

### 严禁

- 改 `VillageWalkArea2` 多边形点集/尺寸当主修  
- 用关掉 ClosestPoint / 撤探索白名单「治卡」  
- 把 1 楼 `VillageWalkArea` 扩成罩 2 楼代替 Override  
- Update 堆业务；抢写 Animator `Run`  
- 与大门 E3′、送树屋戏混成一个大改

### 对照文档 / 代码（侦探必读）

- `Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md`  
- `Assets/Doc/施工说明/0901/Village_Chief_House_楼梯上楼换场巨树2楼_施工说明.md`  
- `Assets/Doc/执行文档/0901/Village_Chief_House_进场落点吸到楼梯_验收排查报告.md`（Flush 时序参考）  
- `Assets/Doc/执行文档/8月/0819/Village_KenMuNi1_树屋下边围栏穿模卡住_架构溯源报告.md`（卡死参考）  
- `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs`（W1）  
- `Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs`（Override / ClosestPoint / Flush）  
- `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs`  
- `Assets/Editor/Tool/Scene/ChiefHouseStairsToTree2fSetupEditor.cs`  
- 场景：`Village_KenMuNi1` 下 `ExitFrom_HomeSceneChief2f`、`VillageWalkArea2`、`VillageWalkArea`  
- 检索：`SetVillageWalkAreaOverride`、`TryBindVillageWalkArea2`、`[Village2f]`、`ExitFrom_HomeSceneChief2f`、`enterPosKey`

---

## 侦探 Prompt（复制给 Agent · 先跑）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码 / Animator / 场景 / Prefab。只读扫描 + 写溯源报告。
默认中文。沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/提示词/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构侦探提示词.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md
@Assets/Doc/施工说明/0901/Village_Chief_House_楼梯上楼换场巨树2楼_施工说明.md
@Assets/Doc/执行文档/0901/Village_Chief_House_进场落点吸到楼梯_验收排查报告.md

## 产品
从村长家楼梯上楼进 KenMuNi1 巨树 2 楼后：须落在 ExitFrom_HomeSceneChief2f 附近；生效 WalkArea=VillageWalkArea2；区内可村式移动、不卡死、不被吸回 1 楼。
用户现网：一上去就卡主动不了；未稳到 ExitFrom；无法在 WalkArea2 内移动（Scene 截图见枝干 WalkArea2 线框）。

## 必读脚本 / 场景锚点
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameMgr/Component/ChangeScene/ChangeSceneComponentGM.cs
@Assets/Editor/Tool/Scene/ChiefHouseStairsToTree2fSetupEditor.cs
场景物体：ExitFrom_HomeSceneChief2f、VillageWalkArea2、VillageWalkArea、StairsDoor_ToTree2f（Chief）、LeftDoor enterPosKey。
检索：SetVillageWalkAreaOverride、TryBindVillageWalkArea2、[Village2f]、LastSceneName、FlushAuthoritative、ClosestPoint、enterPosKey。

## 任务
1. 按 H1～H8 并列证伪；钉死主因（可多因同构，须排主次）。
2. 画出现网 vs 期望时序：换场 → EnterPos → SetPlayerPos → W1 Override → 首帧 ClosestPoint/障碍。
3. 明确：人是否曾接近 ExitFrom；Override 是否指向 WalkArea2；卡死是夹区、障碍焊死、还是输入锁。
4. 推荐最小方案（默认倾向 F1/F2/F3/F4 按根因）；严禁 F5 改 WalkArea2 形状、F6 关 ClosestPoint。
5. 写清与 0901 楼梯案 / 进屋吸楼梯 / 0819 围栏卡死的边界；更新 OPEN 里「2 楼可达+W1」验收状态建议。

## 报告
Assets/Doc/执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md
```

---

## 施工 Prompt（根因拍板后复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md
@Assets/Doc/提示词/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构侦探提示词.md
@Assets/Doc/施工说明/0901/Village_Chief_House_楼梯上楼换场巨树2楼_施工说明.md

## 目标
村长家楼梯 → KenMuNi1 巨树 2 楼：落稳 ExitFrom_HomeSceneChief2f；WalkArea2 生效；区内可走不卡死。

## 约束
- 禁止改 VillageWalkArea2 多边形点集/尺寸
- 禁止关 ClosestPoint / 撤村探索白名单当主修
- 禁止用 1 楼 VillageWalkArea 扩罩 2 楼代替 Override
- 禁止顺手改宝箱 / 送树屋戏 / DayLight / 续聊
- 保持 E3′：LeftDoor 用 Village_Chief_House_Door，不绑 WalkArea2
- 代码须含详细注释；重要改动说明原因；复杂逻辑写清替代方案

## 落盘
Assets/Doc/施工说明/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_施工说明.md
同步 OPEN_QUESTIONS.md

## 验收
- [ ] 楼梯换场后脚距 ExitFrom_HomeSceneChief2f 很小；OverlapPoint(WalkArea2)=true
- [ ] Console 有 [Village2f] 已 SetVillageWalkAreaOverride(VillageWalkArea2)
- [ ] 区内 A/D+W/S 可走，不卡死，不被吸回 1 楼高度
- [ ] WalkArea2 点集/尺寸与修前一致
- [ ] LeftDoor 出门仍落 1 楼门前且不绑 WalkArea2
- [ ] 无相关 Error
```

---

## 给开发者（一句话）

0901 已经做了「上楼落 2 楼 + W1 绑 WalkArea2」；你现在这表现多半是 **W1/落点/夹区时序没真正跑通，或脚被障碍焊死**——先丢侦探 Prompt 钉根因，**不要先去改 WalkArea2 形状**。
