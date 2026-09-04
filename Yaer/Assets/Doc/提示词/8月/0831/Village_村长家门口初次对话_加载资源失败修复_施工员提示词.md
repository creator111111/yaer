# Cursor Agent Prompt · 修 bug：加载资源失败 Village_村长家门口初次对话.prefab

> **角色**：【施工员】为主；必要时短【验收员】核对磁盘与 AB  
> **日期**：2026-08-31  
> **Console 证据**：`加载资源失败:Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab`  
> **触发链**：`Npc_Chief` / `ChiefNearDoorStoryTrigger` → `TriggerStory("Village_村长家门口初次对话")` → `StoryComponentGSM` → `ResMgr.LoadAsset` → **失败**  
> **助手预判（须核实）**：磁盘 **尚无** 该 Prefab（仅有 CSV + Setup 菜单代码）；施工说明要求先跑  
> `Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab`  
> **同屏次要**：Scene 打开 `Village_KenMuNi1` 报 `Village_KenMuNiStart` Missing Prefab guid——Setup **以 KenMuNiStart 为壳拷贝**，壳坏则菜单也会失败，须一并核  
> **说明落盘**：`Assets/Doc/施工说明/0831/Village_村长家门口初次对话_加载资源失败修复_施工说明.md`

把下面「施工」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（施工须核实）

### 根因假说（按概率）

| ID | 假说 | 预扫 |
|----|------|------|
| **H1** | 成品 Prefab **从未落盘** | ✅ Glob：`GameRes/Prefabs/Dialogue/` 下 **无** `Village_村长家门口初次对话.prefab`；仅 `Dialog/…csv` |
| H2 | Prefab 有，但未进 AB / Resource 清单 | 次查：Editor Play 若走 GF Resource，新建后可能要 Refresh/Rebuild |
| H3 | 名不一致（StoryPrefabName 多空格/错字） | 代码常量 = `Village_村长家门口初次对话`；路径须完全一致 |
| H4 | Setup 菜单跑过但失败（壳缺失） | Console 另有 `Village_KenMuNiStart` Missing guid `397659d3…` |

### 已有修复入口（勿重复造轮）

| 工具 | 路径 |
|------|------|
| Setup 菜单 | `Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab` |
| 脚本 | `Assets/Editor/Tool/Dialogue/VillageChiefDoorDialogueSetupEditor.cs` |
| 壳 | `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`（磁盘有；场景实例可能断链） |
| CSV | `Assets/Dialog/Village_村长家门口初次对话.csv` |
| 施工说明 | `施工说明/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_施工说明.md` |

### 修复步骤倾向

1. 确认目标路径文件是否存在（Project 窗口 / 磁盘）。  
2. 若无：跑 Setup 菜单；确认生成 Prefab + Generated `.asset`。  
3. 若菜单失败：先修 `Village_KenMuNiStart` 壳（场景 Missing 与 Asset 是否同 guid）。  
4. 若文件已有仍 Load 失败：查 Resource/AB 注册与 `DialoguePath.GetPath`。  
5. Play：靠近 `Npc_Chief` 不再报「加载资源失败」；对白能起。  

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 让门口对白 Prefab 可被 Load | ❌ 改掉 Trigger 名绕过（除非产品改名） |
| ✅ 修 Setup 阻塞（壳 Missing） | ❌ 重做三人立绘/Face123 大功能（已有则复用） |
| ✅ 短施工说明写清根因+步骤 | ❌ 无视 H1 去改 ResMgr |

### 严禁

- 删 `Npc_Chief` 触发当「修好」  
- 手工空 Prefab 无 DialogueTree 应付加载  
- 不跑菜单却改 Resource 瞎注册  

---

## 施工 Prompt（复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md

## Bug
Console：加载资源失败:Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab
靠近村长触发 ChiefNearDoorStoryTrigger → TriggerStory 加载该路径失败。

## 必读
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/ChiefNearDoorStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/StoryComponentGSM.cs
@Assets/Scripts/Game/GameMgr/Component/ResComponentGM.cs
@Assets/Scripts/Game/Static/Path/DialogueFilePath.cs
@Assets/Editor/Tool/Dialogue/VillageChiefDoorDialogueSetupEditor.cs
@Assets/Doc/施工说明/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_施工说明.md
@Assets/Dialog/Village_村长家门口初次对话.csv
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab

## 任务
1. 核实磁盘是否存在目标 Prefab；不存在则按 Setup 菜单逻辑生成（可在 Editor 脚本里调用与菜单相同的 Setup，或写清用户须点的菜单 + 你侧保证脚本可成功）。
2. 若 Setup 依赖壳 Village_KenMuNiStart：核 guid、场景 Missing Prefab 是否阻断；修好壳或改用可用壳，保证 CopyAsset 成功。
3. 生成后确认：
   - Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab 存在
   - Generated DialogueTree 存在
   - 路径与 DialoguePath.GetPath / Story 名一致
4. 若 Editor 下仍 Load 失败：查 GF Resource 是否需登记/打 AB；给最小修复。
5. 落盘：
   Assets/Doc/施工说明/0831/Village_村长家门口初次对话_加载资源失败修复_施工说明.md
   写清：根因（H?）、做了什么、用户若还需点菜单写进检查清单。

## 验收
- [ ] Project 中可见 Village_村长家门口初次对话.prefab
- [ ] Play 靠近 Npc_Chief：不再出现该路径「加载资源失败」
- [ ] TriggerStory 能打开对白（允许后续立绘/脸小问题另案，但资源必须加载成功）
- [ ] Setup/壳相关 Error 清掉或已说明残留风险

## 禁止
- 去掉 Trigger 假装无 bug
- 大范围改 ResMgr / 全项目 AB 流程（除非证实 H2 且最小必要）

## 沟通风格
①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者（立刻可做）

1. 新开 Agent，复制「施工 Prompt」。  
2. 若 Agent 无法代点菜单：你在 Unity 先执行  
   **`Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab`**  
   再让 Agent 核磁盘与 Play。  
3. 若菜单报壳不存在：先修 `Village_KenMuNiStart.prefab` 与场景里 Missing 实例。
