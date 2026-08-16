# Cursor Agent Prompt · Village_HomeScene23 进门后主角不出现 / 不能移动

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-04  
> **范围**：玩家已能从 `Village_KenMuNi1` 进到 **`Village_HomeScene23`**，但主角看不见、不能移动；判断遮挡 vs 未生成 vs 落点/输入门控，并对照已通室内样板列出缺项。  
> **本阶段**：只溯源、不改代码 / 场景

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 现象

- 已成功进入场景：`Village_HomeScene23`
- 主角**不出现**或看不见；**无法在里面移动**
- 不确定：被挡住 / 没生成 / 生成在屏外 / 有人但输入被锁

### 这是不是室内场景？

**是。** `Village_HomeScene23.unity` 是肯姆尼村内某户室内（文档里曾作商店/老板娘屋、NPC 对白样板）。  
村里入口：`House_Npc4` 门的 `NextSceneName = Village_HomeScene23`（与多数 `House4*` → `Village_House4` **不是同一场景**）。

### 高度可疑缺口（静态已扫到，侦探必须逐项证实/证伪）

| ID | 线索 | 为何可能导致「没人 / 不能动」 |
|----|------|------------------------------|
| G1 | `SceneName.cs` **无** `Village_HomeScene23` 常量 | 命名漂移；`nowSceneName` 现挂 `Village_House4SceneManager` → 报 `Village_House4` |
| G2 | **Build Settings 未见** `Village_HomeScene23.unity`（有 `Village_House4`、`Village_HomeScene2`） | 编辑器偶发能进、真机/正式 Load 不稳定；须核对 `ChangeSceneComponentGM` 实际加载方式 |
| G3 | 场景 `EnterPosConfig: []` **空** | 进门后走 `DefaultBornPos`；若默认点在墙后/屏外 →「没人」观感 |
| G4 | SceneManager `config` 指向 **`HomeScene1.asset`**（非 `Village_House4.asset`） | 室内脚步/战斗面板/canCreatePlayer 是否与预期一致须核实 |
| G5 | 左门 `NextSceneName: ForestScene`（场景内已见） | 出门错链；非「人没了」主因，但说明场景仍是半成品拷贝 |
| G6 | Hierarchy 根级与 `SceneManager` 下 **双 Camera / 双 Map** | 可能错相机、遮挡、或跟错空相机 |
| G7 | 文档已知：`Village_House4SceneManager.nowSceneName = Village_House4` ≠ 文件名 `Village_HomeScene23` | 回村落点 `EnterPosConfig` 用 `LastSceneName` 匹配时会错 |

### 对照「应该有什么」（已通样板）

| 项 | `Village_House4` / `Village_HomeScene2` 样板 | `Village_HomeScene23`（待核实） |
|----|-----------------------------------------------|-------------------------------|
| 专用/正确 SceneManager | 有，`nowSceneName`=场景名 | 挂 House4 管理器但场景名不同？ |
| Config `canCreatePlayer` | 1 | 现绑 HomeScene1？ |
| `EnterPosConfig` | `lastScene: Village_KenMuNi1` + 门口 Transform | **空** |
| Build 登记 | 有 | **疑似无** |
| Map / DefaultBorn / 左右门 | 左门回 `Village_KenMuNi1` | 左门疑似 `ForestScene` |
| 室内：`GetCurSceneTerrainType=Indoor`、Home 动画 | 有 | ？ |

对照文档：

- `Assets/Doc/技术文档/场景相关/场景切换.md`
- `Assets/Doc/执行文档/5月/0530/Village_House4场景管理器_施工执行说明.md`
- `Assets/Doc/执行文档/6月/0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md`
- `Assets/Doc/执行文档/6月/0601/Village_HomeScene23_NPC对话配置_执行说明.md`
- `Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md`（曾写 HomeScene4 与 House4 管理器名不一致）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/5月/0530/Village_House4场景管理器_施工执行说明.md
@Assets/Doc/执行文档/6月/0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md
@Assets/Doc/执行文档/6月/0601/Village_HomeScene23_NPC对话配置_执行说明.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、Build Settings。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 从村里进了 **`Village_HomeScene23`**，这是室内屋。
2. 进去后**看不到主角**，也**不能移动**——不知是被挡住、没生成、还是生成了但不能控。
3. Hierarchy 可见：`Object` / `Map` / `SceneManager`（含 Camera、MapControl、Entity、Map）/ 根上还有 Camera、Map——结构可疑。
4. 目标：钉死根因（未创建 / 落点屏外 / 相机错 / 输入锁 / canCreatePlayer=false 等），对照 House4/HomeScene2 列出**缺什么**，给最小修复建议。

---

## 必读 / 优先扫描线索

### A. 玩家到底有没有生成
- `BaseGameSceneManager.InitPlayer` → `config.canCreatePlayer` → `PlayerHandlerComponentGSM.CreatePlayer`
- 本场景 `config` 实际资产（预梳理指 HomeScene1.asset）的 `canCreatePlayer`
- 运行时 Hierarchy 是否出现 Player（Entity 下或 DontDestroy）；Console 有无 CreatePlayer / NRE
- `OpenFightingPanel` / 禁用移动 API 是否在进室内后把人锁死

### B. 落点与「看不见」
- `EnterPosConfig` 为空时走 `MapControl.DefaultBornPos`：坐标是否在可玩区、是否被 Map 前景遮挡、Z/排序
- 相机：根 Camera vs SceneManager/Camera 谁 Active；`CameraComponentGSM.SetFollow` 跟了谁
- Cinemachine / 正交范围是否盯着空处

### C. 「不能移动」
- `config.canMove`、室内 `TownPlayerLocomotion` / Home 动画切换是否失败
- `DisablePlayerMove`、对话未结束、黑幕未关、`Camera.SetLock`
- 碰撞：出生点卡墙/落在触发器死循环

### D. 场景身份是否半成品
- `SceneName` 有无 `Village_HomeScene23`
- Editor Build Settings 是否包含该场景
- `Village_House4SceneManager.nowSceneName` 与文件名不一致的影响（尤其回村 `LastSceneName`）
- 左/右门 `NextSceneName`；`House_Npc4` 进门链是否唯一指向本场景

### E. 与样板 diff 清单（必出表）
对照 `Village_House4` 与 `Village_HomeScene2`，列出 HomeScene4 缺的每一项（Build、EnterPos、Config、SceneName、门回村、专用 Manager 等）。

---

## 侦探任务清单

1. **结论必须说清三类之一（可组合）**  
   - 未生成玩家  
   - 已生成但看不见（屏外/遮挡/错相机）  
   - 已生成可见但不能移动（输入/配置/卡碰撞）

2. **回答：这是室内场景吗？缺什么才能正常进屋游玩？**（检查清单给用户）

3. **根因表**

   | 环节 | 现状 | 是否阻塞主角出现/移动 | 是否必须改 | 备注 |
   |------|------|----------------------|------------|------|
   | canCreatePlayer | | | | |
   | EnterPosConfig | | | | |
   | DefaultBornPos | | | | |
   | Camera 双份 | | | | |
   | Build Settings | | | | |
   | nowSceneName 漂移 | | | | |
   | canMove / 输入锁 | | | | |
   | 门回村 NextSceneName | | | | |

4. **施工员最小改动建议**（只建议）  
   - 优先对齐 HomeScene2 室内交付清单，勿重写换场管线。  
   - 明确：补 `EnterPosConfig` + Build + SceneName + 纠正 config/门，是否足以恢复；是否必须新建 `Village_HomeScene23SceneManager`。

5. **验收清单**  
   - InitScene → 村 → `House_Npc4` 进门 → 主角出现在门口可走  
   - Hierarchy 有 Player；相机跟随  
   - 左门回 `Village_KenMuNi1` 落在门外  
   - Console 无 CreatePlayer/换场 Error

6. **开放问题**追加 OPEN（「Village_HomeScene23 进屋无主角 · 2026-08-04」）：  
   - 本场景正式定位：民居 / 商店屋 / 废弃改指 `Village_House4`？  
   - 是否新建专用 Manager 还是复用并改 `nowSceneName`？  
   - 与 `Village_Shop` 纯 UI 店是否仍要区分？

7. **禁止**：改资产；把「改名门」当根因（除非证实 Find 依赖）；扩成全村所有 House 大重构。

---

## 输出要求

写入：`Assets/Doc/执行文档/0804/Village_HomeScene23_进屋主角不出现_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（未生成 / 看不见 / 不能动 + 主缺项）  
② 原因（生活类比）  
③ 用户需要做什么（检查清单 + 拍板场景定位）  
④ 给程序看的补充：与样板 diff、根因表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认「补齐室内清单」还是「门改指 Village_House4」后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0804/Village_HomeScene23_进屋主角不出现_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使从村里进入 Village_HomeScene23 后主角可见且可移动。
优先对齐 Village_HomeScene2 / Village_House4 室内交付项；禁止重写换场总管线。
每次提交说明：改了哪些文件、缺项补了什么、如何验证进屋可见可走。
```
