# Cursor Agent Prompt · 巨树 2 楼 WalkArea2：宝箱看不见 / 道具未摆放（验收排查）

> **角色**：先【验收员 / 架构侦探】只读核实「到底有没有摆、为何看不见/不能互动」；确认缺口后再【施工员】最小补摆  
> **日期**：2026-09-03  
> **场景**：`Village_KenMuNi1` · 摆放区 **`VillageWalkArea2`**（巨树 2 楼）  
> **现象（用户 + Scene 截图）**：在树屋 2 楼 WalkArea2 一带 **看不到可互动宝箱**；感觉 **道具没有摆放上去**（截图：枝干上空，无箱/无交互物）  
> **产品期望（钉死 · 对齐 0901）**：WalkArea2 内有 **`Tree2fHpMpBox`**（`Box.prefab` + `VillageKenMuNi1HpMpBox`），脚能走到；点 E / 点击可开 → HpBall×3 + MpBall×3 + Tips `GetHpBall`→`GetMpBall`  
> **上游**：0901 脚本/存档/Setup 菜单 **已声称落地**；OPEN 写「场景箱靠 Setup」——**本期当验收失败面，勿当未开工重写整案**  
> **并行案**：0903「上楼卡死/走不动」——**移动卡住不解释「Scene 里完全看不见箱」**；两案解耦，可同会话对照，禁止用「先能走再谈箱」搪塞「Hierarchy 有没有物体」  
> **不是**：改 `VillageWalkArea2` 形状腾地；不是挂西境 `WestRappRoadHpMpBox`；不是新做第二套 Tips  
> **报告落盘**：`Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查报告.md`

把下面「验收/侦探」整段复制给 Agent。施工 Prompt 见文末（根因拍板后）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 2 楼树枝可走区里应该有一个能开的宝箱（生命球×3、体力球×3）。  
> 人站在枝干上（或 Scene 对着这块看）时，箱要么看不见，要么根本没摆上。

### 助手磁盘预扫（须再证 · 勿当「用户胡说」）

当前仓库 `Village_KenMuNi1.unity` **已存在** PrefabInstance：

| 项 | 预扫值 |
|----|--------|
| 名 | **`Tree2fHpMpBox`** |
| 父 | `Objects`（Transform ≈ 世界原点） |
| local/world 坐标 | **`(-152, 41.2, 0)`**（建议点；ExitFrom≈`(-159.34, 41.66)` 东侧约 7 单位） |
| 组件 | 已移除 `HomeScene2Box`；挂 **`VillageKenMuNi1HpMpBox`**（hp/mp=3，`useStoryOnOpen=0`） |
| `sceneObjs` | ✅ 含 `1154813234`（SceneEntity） |
| Prefab | `Assets/Prefabs/Box.prefab`（guid `3d242310…`） |

**含义**：若用户「完全没摆」，更像是 **看不见 / 错位 / 运行时未激活 / 本地场景与磁盘不一致 / 走不到误判**，而不是「脚本从未写过」。侦探第一步必须 **Hierarchy 搜 `Tree2fHpMpBox`**，不要上来重跑整案当从零施工。

### 期望 vs 用户体感

```
期望：Objects/Tree2fHpMpBox 在 WalkArea2 内可见可交互
用户：Scene/Play 枝干上空；无可互动宝箱
```

### 假说表（须并列证伪）

| ID | 假说 | 证伪 |
|----|------|------|
| **H0** | **磁盘有、用户场景无**（未拉最新 / 未保存 / 打开了别的场景副本） | Hierarchy 搜名；对比 git/磁盘 fileID；Console 无 `[Tree2fBox]` |
| **H1** | **物体在但看不见**：SortingLayer/Order 被合层树干挡住；Scale 过小；Sprite 丢/粉；Z 偏差 | Scene 选中箱 → Frame；对比枝干 SR order；Game 相机是否框到 `(-152,41)` |
| **H2** | **坐标不在可见平台 / 不在 WalkArea2 内**：OverlapPoint 假；落在枝外或 1 楼高度 | `OverlapPoint((-152,41.2))`；世界坐标 vs 枝干网格；ExitFrom 相对方位 |
| **H3** | **Active 假 / 父级关掉 / DepthZone 门控误关** | Play 下 `activeInHierarchy`；父链 Objects |
| **H4** | **看得见但不能互动**：未进 `sceneObjs` / Interactive 废 / canTouch 假 / 已开档 `tree2fHpMpBoxOpened` | 磁盘预扫 sceneObjs 已含——须 Play 再证；存档布尔；E 提示 |
| **H5** | **Missing Script / 组件丢**：村脚本 GUID 断 → 无逻辑且可能 Inspector 报缺 | 选中箱看 `VillageKenMuNi1HpMpBox` 是否 Missing |
| **H6** | **人卡死在左侧、箱在右侧稍远 → 误以为没摆** | 与 0903 卡住案解耦：即使用户走不动，Scene 中选中/Frame 箱仍应可见 |
| **H7** | **Setup 未在本机跑过 / 跑失败仍留脏实例** | 菜单重跑幂等；Console `[Tree2fBox]` Warning（坐标不在区内） |

### 方案倾向（按根因）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **V1** | Hierarchy 确认有箱 → 修 **Sorting / 位移微挪**（仍须 Overlap WalkArea2）让可见可站 | ✅ 若 H1/H2 |
| **V2** | 本机跑菜单 `Tools / Scene / Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3`（幂等） | ✅ 若 H0/H7 无箱或组件烂 |
| **V3** | 补 `sceneObjs` / 交互组件 / 清已开脏档 | ✅ 若 H4 |
| **V4** | 重写宝箱系统 / 改 WalkArea2 形状腾地 | ❌ |
| **V5** | 挂西境 HpMpBox 图省事 | ❌（读错存档） |

### 与相关案边界

| 案 | 关系 |
|----|------|
| 0901 巨树 2 楼 WalkArea2 宝箱 HpMp×3 | **本案 = 该案验收**；脚本/Data 已有则只补场景可见性与交互 |
| 0903 上楼卡 WalkArea2 动不了 | **并行**；卡住解释「走不到开箱」，**不解释**「Scene 全空」；报告须分条写 |
| 0819 围栏卡死 | 正交 |
| 禁止 | 改 WalkArea2 点集「好放箱」 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 钉死：有无 `Tree2fHpMpBox`、世界坐标、Overlap、Sorting、sceneObjs、开箱链路 | ❌ 当从零重做 B1 架构 |
| ✅ 最小让箱在 2 楼可见可互动（数量/Tips 对齐 0901） | ❌ 改多边形；新 Tips UI |
| ✅ 用户检查：Hierarchy 搜名 + Frame Selected | ❌ 用「先修好走路」关闭本案 |
| ✅ 更新 OPEN Q7「上游 2 楼可达+W1」与宝箱场景验收状态 | ❌ 顺手改卡住案主链（除非同根因） |

### 严禁

- 改 `VillageWalkArea2` 多边形尺寸/点集  
- 挂 `WestRappRoadHpMpBox` / 写回 `HomeScene2Box` 当主修  
- 只 `AddMainItem` 不 Tips；TipKey 用中文  
- 把箱摆到 WalkArea2 **外**  
- Update 堆业务  

### 对照文档 / 代码

- `Assets/Doc/执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md`  
- `Assets/Doc/施工说明/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_施工说明.md`  
- `Assets/Doc/提示词/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构侦探提示词.md`（并行）  
- `Assets/Editor/Tool/Scene/KenMuNi1Tree2fHpMpBoxSetupEditor.cs`  
- `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageKenMuNi1HpMpBox.cs`  
- `Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Scene/VillageKenMuNi1Data.cs`  
- 场景：`Objects/Tree2fHpMpBox`、`MapLimit/VillageWalkArea2`、`ExitFrom_HomeSceneChief2f`  
- 检索：`Tree2fHpMpBox`、`[Tree2fBox]`、`tree2fHpMpBoxOpened`、`Setup KenMuNi1 巨树2楼`

### 用户可先手检（30 秒）

1. 打开 `Village_KenMuNi1` → Hierarchy 搜 **`Tree2fHpMpBox`**  
2. 有：选中 → **Frame Selected**（F）→ 看是否在枝干/WalkArea2 内；看 Inspector 是否 Missing Script  
3. 无：跑菜单 **`Tools / Scene / Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3`** 后再搜  
4. Play：走到箱旁（若卡死则先记「仅 Scene 可见性」结论，互动等卡住案）

---

## 验收/侦探 Prompt（复制给 Agent · 先跑）

```text
你是【验收员 + 架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码/场景/Prefab（本阶段只读）。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查提示词.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md
@Assets/Doc/施工说明/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_施工说明.md
@Assets/Doc/提示词/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构侦探提示词.md

## 产品
巨树 2 楼 VillageWalkArea2 内须有可互动宝箱 Tree2fHpMpBox：开箱 Hp×3+Mp×3 + GetHpBall→GetMpBall。
用户：Scene/Play 看不到箱，觉得道具没摆上（枝干截图为空）。

## 必读
@Assets/Editor/Tool/Scene/KenMuNi1Tree2fHpMpBoxSetupEditor.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageKenMuNi1HpMpBox.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Scene/VillageKenMuNi1Data.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
检索：Tree2fHpMpBox、VillageKenMuNi1HpMpBox、sceneObjs、OverlapPoint、[Tree2fBox]。

## 任务
1. 核实磁盘/场景是否已有 Tree2fHpMpBox（助手预扫称有，坐标约 -152,41.2；须你钉死）。
2. 按 H0～H7 证伪：看不见 vs 真没摆 vs 不能互动 vs 与卡住案混淆。
3. 核对：世界坐标、WalkArea2.OverlapPoint、Sorting 相对枝干、Active、sceneObjs、村脚本是否 Missing、存档已开态。
4. 与 0903 卡住案分条：何为「走不到」，何为「根本不可见」。
5. 推荐 V1/V2/V3；严禁改 WalkArea2 形状、挂西境箱脚本。
6. 更新 OPEN 宝箱场景验收状态建议。

## 报告
Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查报告.md
```

---

## 施工 Prompt（根因拍板后复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查报告.md
@Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查提示词.md
@Assets/Doc/施工说明/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_施工说明.md

## 目标
WalkArea2 内 Tree2fHpMpBox 玩家可见、可走到、可开箱（Hp×3+Mp×3+双 Tips）。不改 WalkArea2 形状。

## 约束
- 禁止改 VillageWalkArea2 点集/尺寸
- 禁止挂 WestRappRoadHpMpBox / 恢复 HomeScene2Box 当主逻辑
- 禁止新 Tips 系统；TipKey 仍 GetHpBall/GetMpBall
- 箱必须 OverlapPoint(WalkArea2)==true
- 代码/Setup 须详细注释；重要改动写原因
- 与 0903 卡住案解耦：除非报告写明同根因，否则不顺手大改 Town/W1

## 落盘
Assets/Doc/施工说明/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_施工说明.md
同步 OPEN_QUESTIONS.md

## 验收
- [ ] Hierarchy 有 Objects/Tree2fHpMpBox；Scene Frame 可见在枝干平台
- [ ] 世界坐标在 WalkArea2 内；相对 ExitFrom 可辨认（东侧或报告改定稿）
- [ ] Play：可交互开箱；+3/+3；GetHpBall→GetMpBall；同档不可再开
- [ ] WalkArea2 多边形未改；sceneObjs 含箱
- [ ] 无 Missing Script；Console 无 [Tree2fBox] Error
```

---

## 给开发者（一句话）

仓库里 **很可能已经摆了** `Objects/Tree2fHpMpBox`（约在 ExitFrom 东侧 `-152,41`）——先 Hierarchy 搜名字并 F 聚焦；若仍看不见再跑侦探 Prompt，**不要先改 WalkArea2，也不要和「卡死走不动」混成一个锅**。
