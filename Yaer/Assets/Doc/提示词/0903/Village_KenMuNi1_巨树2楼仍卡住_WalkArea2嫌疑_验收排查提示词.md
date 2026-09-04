# Cursor Agent Prompt · 上树屋 2 楼仍动不了：是不是 VillageWalkArea2 坏了？（验收复测）

> **角色**【验收员】为主；必要时短证【架构侦探】补残余假说——**禁止一上来改 WalkArea2 多边形**  
> **日期**：2026-09-03  
> **用户原话**：「上来树屋 2 楼之后还是动不了，难道是 VillageWalkArea2 有问题吗？」  
> **场景**：`Village_Chief_House` 楼梯 → `Village_KenMuNi1` 巨树 2 楼 / `VillageWalkArea2`  
> **已有结论（0903 侦探 · 必须先读，勿当没查过）**：  
> **主因不是 WalkArea2 形状坏了**，而是 **纵深标尺缺失（DepthGap）**：Prefab `depthYMaxWorld=8` 与 2 楼 Y≈41 + WalkArea2 ClosestPoint **每帧撕扯** → 焊死/落不稳 ExitFrom。  
> **已施工（磁盘声称）**：F_D1 场景 `VillageDepthY_Min=-20` / `Max=46`；F_D2+F_Order 楼梯路径先抬 Max、绑 WalkArea2、再 Teleport ExitFrom。  
> **本期问题**：用户 **修后仍（或仍感觉）动不了**，并怀疑 WalkArea2 —— 要 **验收施工是否真生效**，并 **明确回答「是不是 WalkArea2 的锅」**。  
> **不是**：改 `VillageWalkArea2` 点集/尺寸当主修；不是重开 0901 楼梯案；不是宝箱/进层对白另案  
> **报告落盘**：`Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查报告.md`

把下面整段复制给 Agent（Agent Mode）。

---

## 提示词助手预梳理（验收须用 Play 证据打脸，勿只读旧报告）

### 直接回答用户（预结论 · 须复测确认）

| 问题 | 0903 侦探已裁定 | 本期验收要证明 |
|------|-----------------|----------------|
| **是 VillageWalkArea2 多边形坏了吗？** | **基本否**。ExitFrom 相对 WalkArea2 **PIP=True**；形状锁定；改形状是 **严禁主修** | 复测：形仍合理；卡死时 Override 指向谁；`depthYMax` 是否仍为 8 |
| **那为什么感觉「区里走不动」？** | WalkArea2 **被绑上了**，但与 **maxY=8** 撕扯，体感像地板区坏了 | 修后：Max≥45 且不再 `CLAMP_AT_YMAX` 刷屏时，应可走 |
| **磁盘施工了为何还卡？** | 可能：本机场景未同步标尺；进场未打 `[Village2f]`；Max 未注入；障碍/输入锁残余 | 按下方检查清单逐项过 |

### 生活类比（给用户）

绿线框（WalkArea2）=「脚必须踩在这块地板上」。  
另有一把尺子规定「人最高只能站到 Y=8」。  
2 楼地板在 Y≈40：地板往上吸、尺子往下压 → 人焊在中间。  
**看起来像绿线框有问题，其实是尺子太矮。** 施工是把尺子抬到 46，不是去改绿线框形状。

### 磁盘预扫（须再证）

| 项 | 预扫 |
|----|------|
| `VillageDepthY_Min` | KenMuNi1 有，Y=**-20** |
| `VillageDepthY_Max` | KenMuNi1 有，Y=**46** |
| `SetPlayerPos` 楼梯键 | 有 F_D2 抬 Max + Override + Teleport |
| `VillageWalkArea2` | 仍在 `(-139,37.5)`；**禁止当主因去改点集** |

### 验收假说（修后仍卡 · 并列）

| ID | 假说 | 证伪 |
|----|------|------|
| **A1** | **本机未带上 F_D1 标尺**（Hierarchy 无 DepthY / 打开旧场景） | 搜 `VillageDepthY_Max`，Y 是否 46 |
| **A2** | **运行时 Max 仍是 8**：Inject 失败 / 顺序错 / Prefab 覆盖 | Play 读 `DebugDepthYMaxWorld`；Console 无 `depthYMax→` |
| **A3** | **W1 未绑**：仍夹 1 楼 WalkArea | 无 `[Village2f] 已 SetVillageWalkAreaOverride` |
| **A4** | **标尺够了仍卡**：障碍焊死（0819）/ 输入锁 / 黑幕未关 / 对白锁 | Foot Overlap；`isTalking`；黑幕 |
| **A5** | **WalkArea2 形状真有问题**（凹坑/漏点导致贴边焊死） | 仅当 A1～A3 已否且 PIP/贴边日志钉死；**仍优先微挪落点/障碍，最后才谈点集** |
| **A6** | 用户未走楼梯键（读档脏坐标 / 传送） | LastScene / EnterPos 路径 |

### 方案（按验收结果）

| 结果 | 动作 |
|------|------|
| A1 | 跑 `Tools / Scene / Setup KenMuNi1 巨树纵深标尺 DepthY`；保存场景 |
| A2/A3 | 查 `Village_KenMuNiSceneManager.SetPlayerPos` 是否执行；补日志；禁改 WalkArea2 |
| A4 | 走 0819/输入锁最小修 |
| A5 | 报告单开；**默认仍不改形状**，先证伪 |
| 一切通过仍卡 | 新假说进 OPEN；加 `[Village2fMove]` 帧日志（权威 Y、Clamp、Override 名、vx） |

### 严禁

- **先改 `VillageWalkArea2` 点集/尺寸「试试能不能走」**  
- 关 ClosestPoint / 撤村探索白名单  
- 用 1 楼 `VillageWalkArea` 扩罩 2 楼代替 Override  
- 与宝箱对白、进层戏混修  

### 对照（必读）

- `执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md`  
- `施工说明/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_施工说明.md`  
- `Village_KenMuNiSceneManager.cs`（`[Village2f]`）  
- `TownPlayerLocomotion`（`depthYMaxWorld` / `CLAMP_AT_YMAX` / Override）  
- Hierarchy：`VillageDepthY_*`、`VillageWalkArea2`、`ExitFrom_HomeSceneChief2f`

---

## 验收 Prompt（复制给 Agent）

```text
你是【验收员】。Unity 2020.3.48f1 / C#。
默认只读核实施工是否生效；仅当证据证明残余 bug 且报告批准后才可最小改代码。
禁止改 VillageWalkArea2 多边形点集/尺寸。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查提示词.md
@Assets/Doc/执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md
@Assets/Doc/施工说明/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_施工说明.md
@Assets/Doc/OPEN_QUESTIONS.md

## 用户问题（必须正面回答）
上来树屋 2 楼还是动不了——是不是 VillageWalkArea2 有问题？

## 必读代码 / 场景
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
场景：VillageDepthY_Min/Max、VillageWalkArea2、ExitFrom_HomeSceneChief2f
检索：[Village2f]、depthYMax、CLAMP_AT_YMAX、SetVillageWalkAreaOverride、DebugDepthYMaxWorld

## 任务
1. 用一句话回答：WalkArea2 形状是不是主因（对齐侦探 DepthGap；用本次 Play/磁盘证据）。
2. 按 A1～A6 验收：标尺在不在、运行时 Max 是否≥45、W1 日志、脚是否稳在 ExitFrom、区内能否走。
3. 若仍卡：钉残余主因（注入失败 / 障碍 / 输入锁 / 其它），给出最小下一修；默认仍禁止改 WalkArea2 形状。
4. 列出用户 30 秒自检步骤（Hierarchy 搜 DepthY_Max、看 Console [Village2f]）。
5. 更新 OPEN：卡住案验收状态。

## 报告
Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查报告.md
```

---

## 若验收确认「标尺未进本机 / 未注入」——补施工段（可选）

```text
你是【施工员】。仅当验收报告写明 A1/A2 成立时执行。
最小化：确保 KenMuNi1 有 VillageDepthY_Max≈46；楼梯 SetPlayerPos 打出 depthYMax→ 与 Override 日志；脚稳 ExitFrom 可走。
禁止改 VillageWalkArea2 点集。落盘施工说明附录或新 0903 补丁说明。
```

---

## 给开发者（一句话）

**多半不是 WalkArea2 画错**，而是「能站多高」的尺子曾经只有 8、和 2 楼绿线框打架；磁盘已抬到 46——你先 Hierarchy 搜 **`VillageDepthY_Max`**，Play 看 Console 有没有 **`[Village2f] depthYMax→`** 和 **`已 SetVillageWalkAreaOverride`**；若 Max 仍是 8 或没有这两条日志，把上面验收 Prompt 丢给 Agent，**不要先去改绿线框形状**。
