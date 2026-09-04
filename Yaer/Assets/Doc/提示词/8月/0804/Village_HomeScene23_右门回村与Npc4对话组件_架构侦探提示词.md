# Cursor Agent Prompt · Village_HomeScene23：右门回村 + Npc4 对话组件接线

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-04  
> **范围**：`Village_HomeScene23` 室内 —— ① **右门**进出回 `Village_KenMuNi1`；② 给 **`Npc4`** 仿照其它 NPC 绑齐对话相关组件，为后续「和 NPC 对话」铺路。  
> **本阶段**：只摸清缺项与仿照清单，**不施工**  
> **关联**：进屋无主角见 `0804/Village_HomeScene23_进屋主角不出现_*`（可并列提及，但本期焦点是右门 + Npc4）

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品目标（开发者已拍板）

1. **本场景走右门进出**，回到村庄 `Village_KenMuNi1`（不是左门主链；与 HomeScene2「HouseDoor/左门」样板可能不同，以本场景右门为准）。  
2. **`Npc4`** 要能像其它可对话 NPC 一样交互；后续功能是对话，本期侦探先钉死「要绑哪些组件 / 缺哪块资源」。  
3. Hierarchy（截图）：`Object/Npc4`（红箭头目标）；同层有 `Npc1`、`NpcXiaer` 可仿；`Map/MapRight/RightDoor`；落点有 `RightBorn` / `LeftBorn` / `DefaultBornPos`；`Map/Design/村民家4合层/npc` 多为**合层美术占位**，勿与 `Object/Npc4` 混为一谈。

### 已知对照

| 主题 | 文档 / 样板 |
|------|-------------|
| NPC 三件套 | `0601/Village_HomeScene23_NPC对话配置_执行说明.md`（`SimpleStoryTrigger` + 交互碰撞 + `Dialogue/{名}.prefab`） |
| 推荐仿谁 | **`Npc1`**（通用 `SimpleStoryTrigger`）；**慎仿** `NpcXiaer`（`HomeScene1Xiaer` 龙宫专用，易路径报错） |
| 换场通则 | `技术文档/场景相关/场景切换.md`；门 = `SceneChangeDoor.NextSceneName` + 双侧 `EnterPosConfig` |
| 村里入口 | `House_Npc4` → `NextSceneName = Village_HomeScene23`（KenMuNi1） |
| 右门回村落点 | 村里须有 `lastScene` 匹配本场景逻辑名 → 门外 Transform；室内进门落点建议 **`RightBorn`**（右门侧） |

### 预扫风险（须证实）

- 室内 `RightDoor`：`NextSceneName` 是否已是 `Village_KenMuNi1`，还是空 / `ForestScene` / 其它残留。  
- `LeftDoor` 是否应保持禁用，避免双门打架。  
- `EnterPosConfig` 本场景曾为空；右门方案下应配 `Village_KenMuNi1` → `RightBorn`（或等价门口点）。  
- 村里回程：`EnterPosConfig` 是否已有本场景名（注意 `nowSceneName` 可能仍报 `Village_House4` 与文件名 `Village_HomeScene23` 漂移）。  
- `Npc4`：是否已有 `BaseEntityControll` / `InteractiveComponent` / Body 碰撞 / `SimpleStoryTrigger`；还是只有空壳。  
- 对话 Prefab：`StoryPrefabName` 对应文件是否已在 `Assets/GameRes/Prefabs/Dialogue/` **根目录**（不要只放子文件夹）。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/6月/0601/Village_HomeScene23_NPC对话配置_执行说明.md
@Assets/Doc/执行文档/6月/0608/Village_HomeScene2_HouseDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/6月/0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md
@Assets/Doc/执行文档/0804/Village_HomeScene23_进屋主角不出现_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. `Village_HomeScene23` 是室内屋；**进出村庄走右门**（`Map/MapRight/RightDoor`）。  
2. 要给 **`Object/Npc4`** 仿照其它 NPC，把**对话相关组件**绑齐，下一步才能做对话内容。  
3. 合层里的 `Design/.../npc` 是画，真正交互对象是 **`Object/Npc4`**（侦探须对比两边，防绑错物体）。  
4. 本期不写死具体台词；但要写清：对话 Prefab 命名约定、缺资源时施工员先补什么。

---

## 必读 / 优先扫描线索

### A. 右门进出回村（主链）
- `Village_HomeScene23` → `Map/MapRight/RightDoor`：`SceneChangeDoor`、`NextSceneName`、`TriggerWhenMoveIn`、是否 Active、是否在 `sceneObjs`
- `MapLeft/LeftDoor` 现状：建议是否禁用以免双出口
- 村里 `House_Npc4`：`NextSceneName`、门外落点空物体名
- 双侧 `EnterPosConfig`：
  - 室内：`lastScene=Village_KenMuNi1` → 建议 `RightBorn`
  - 村里：`lastScene`=本场景逻辑名 → `House_Npc4` 门外
- `nowSceneName` / `SceneName` 常量 / Build Settings：与回程 `LastSceneName` 匹配是否断裂（House4 vs HomeScene4 漂移）

### B. Npc4 对话组件（仿 Npc1）
对照 `Object/Npc1`（及可选 HomeScene2 埃吉尔）逐项 diff `Object/Npc4`：

| 组件 / 配置 | Npc1 现状 | Npc4 现状 | 是否必须 |
|-------------|-----------|-----------|----------|
| SceneEntity / 登记 sceneObjs | | | |
| BaseEntityControll（entityType=NPC） | | | |
| InteractiveComponent + Body 触发碰撞 | | | |
| SimpleStoryTrigger + StoryPrefabName | | | |
| keyTipsPosX/Y | | | |
| 对话 Prefab 磁盘文件 | | | |

- **禁止**建议把 Npc4 改成挂 `HomeScene1Xiaer`（龙宫专用，0601 文档已踩坑）。  
- 明确：美术合层 `npc` 要不要藏/是否与 Npc4 重叠挡点击。

### C. 对话资源约定
- `TriggerStory(StoryPrefabName)` → `Assets/GameRes/Prefabs/Dialogue/{名}.prefab`  
- 若尚无 Npc4 专用 Prefab：施工建议「先复制 Npc1 用的 Prefab 改名占位」还是「等策划 CSV」——写入开放问题。  
- DialogDebug 可否单测该 Prefab。

### D. 与「进屋无主角」关系
- 右门 / EnterPos / 生成玩家若仍缺，对话测不了：报告里用一节「前置依赖」引用 0804 进屋报告，列出必须先修的 1～2 项，勿把本期扩成全场景大修。

---

## 侦探任务清单

1. **钉死右门闭环**  
   - 现网能否：村 → 右门进屋 → 右门回村？缺哪一环？  
   - 落点是否应用 `RightBorn`？

2. **钉死 Npc4 缺口表**（必出，相对 Npc1）  
   - 只差绑组件 / 还差 Prefab / 还差 sceneObjs / 绑错合层 npc？

3. **仿照施工清单**（只建议，分「门」与「Npc4」两块）  
   - Unity 操作步骤级（照 0601 §4 风格），最小化；能复用 Stairs/现成门配置则写明。

4. **验收清单**  
   - 右门：双向换场 + 落点正确 + Console `[SceneChangeDoor]`  
   - Npc4：靠近出 E、按 E 能 `TriggerStory`（有 Prefab 则出对话；无则至少不 NRE 且日志点明缺资源）  
   - 不误触合层装饰 npc

5. **开放问题**追加 OPEN（「HomeScene4 右门+Npc4 · 2026-08-04」）：  
   - Npc4 对话 Prefab 正式名？  
   - 左门是否永久禁用？  
   - `nowSceneName` 纠正为 `Village_HomeScene23` 是否与右门回程同轮必做？  
   - 进屋无主角是否本轮前置？

6. **禁止**：改资产；给 Npc4 挂龙宫 `HomeScene1Xiaer`；把出门改成只走左门（与产品「右门」冲突，除非侦探证明 RightDoor 不可用并征得确认）；扩写整段台本内容。

---

## 输出要求

写入：`Assets/Doc/执行文档/0804/Village_HomeScene23_右门回村与Npc4对话组件_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（右门缺什么 + Npc4 缺什么）  
② 原因（生活类比）  
③ 用户需要做什么（拍板 Prefab 名 / 左门是否关 + 验收）  
④ 给程序看的补充：右门闭环表、Npc4 vs Npc1 diff、施工清单、前置依赖、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认右门 NextSceneName、回程 EnterPos、Npc4 的 StoryPrefabName 后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0804/Village_HomeScene23_右门回村与Npc4对话组件_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**：
1）Village_HomeScene23 右门与村里 House_Npc4 双向进出可验收；
2）Object/Npc4 按 Npc1 仿照绑齐对话组件（SimpleStoryTrigger，禁止 HomeScene1Xiaer）。
若对话 Prefab 尚未交付，可先占位并在说明中写清；不要阻塞门闭环。
禁止在 Update 堆逻辑。每次提交说明改动与验证步骤。
```
