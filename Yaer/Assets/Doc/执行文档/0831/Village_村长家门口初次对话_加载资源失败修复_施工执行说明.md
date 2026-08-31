# Village_村长家门口初次对话 — 加载资源失败修复 — 施工执行说明

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【执行说明】根因核实 + 修复步骤（**本阶段未代点 Unity 菜单、未改运行时 C#**）  
**Unity**：2020.3.48f1  
**Console**：`加载资源失败:Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab`  
**触发链**：`Npc_Chief` → `ChiefNearDoorStoryTrigger` → `TriggerStory("Village_村长家门口初次对话")` → `StoryComponentGSM` → `DialoguePath.GetPath` → `ResMgr.LoadAsset` → **失败**  
**提示词**：`提示词/0831/Village_村长家门口初次对话_加载资源失败修复_施工员提示词.md`  
**配套施工说明**：`施工说明/0831/Village_村长家门口初次对话_加载资源失败修复_施工说明.md`  
**关联**：`施工说明/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_施工说明.md`（已要求跑 Setup，成品尚未落盘）

---

## 沟通摘要

### ① 结论一句话

**根因 H1：目标 Prefab 从未生成；不是 ResMgr/名写错。在 Unity 执行 `Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab` 即可；壳 `Village_KenMuNiStart.prefab` 磁盘存在，Setup 按路径拷贝不受场景 Missing guid 阻断。**

### ② 原因（通俗）

靠近村长时游戏去加载门口对白预制体，但工程里根本没有这个文件——只有 CSV 和一键生成菜单。  
场景里「KenMuNiStart Missing」是另一件事：场景记着旧 guid，和磁盘壳文件 guid 对不上，会红，但**不挡**菜单按路径拷贝壳。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Unity 菜单：`Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab` | Console 有 `[ChiefDoorSetup] Prefab 已写入…` |
| 2 | Project 可见 | `Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab` |
| 3 | Generated | `Assets/GameRes/DialogueTrees/Generated/` 下有门口对应对白树 `.asset` |
| 4 | Play 靠近 `Npc_Chief` | **不再**报该路径「加载资源失败」 |
| 5 | 对白 | 能打开（立绘/脸小问题可另案） |
| 6 | （可选）场景 KenMuNiStart Missing | 见 §④ H4；不挡本 bug 修复 |

### ④ 程序补充

见下文 §①～§⑥。

---

## ① 根因裁定

| ID | 假说 | 磁盘核实 | 裁定 |
|----|------|----------|------|
| **H1** | 成品 Prefab **从未落盘** | `GameRes/Prefabs/Dialogue/` **无** `Village_村长家门口初次对话.prefab`；仅有 CSV + Setup 脚本 | ✅ **主因** |
| H2 | 有 Prefab 但未进 AB/Resource | 文件都不存在 → 不适用 | ❌ 本期否 |
| H3 | Story 名不一致 | 常量/`Npc_Chief`/`DialoguePath` 均为 `Village_村长家门口初次对话` → 路径正确 | ❌ |
| H4 | Setup 因壳失败 | 壳文件**在**；场景 PrefabInstance guid **断链**（次要） | ⚠️ 不阻断路径 Setup；场景红另修 |

**调用链（已对齐代码）**

```
TriggerStory("Village_村长家门口初次对话")
  → DialoguePath.GetPath → Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab
  → ResComponentGM.LoadAsset
  → 资源不存在 → Log.Error("加载资源失败:{0}", assetname)
```

---

## ② 壳与场景 Missing（H4 说明）

| 项 | 值 |
|----|-----|
| 壳磁盘路径 | `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab` ✅ 存在 |
| 壳 `.meta` guid | **`aace8b8bade72b749af5feadb70df3b7`** |
| 场景引用 guid | **`397659d35618d91408c384a96ea6660f`**（`Village_KenMuNi1` / `_night` PrefabInstance） |
| 工程内其它 `.meta` 含旧 guid | ❌ **无** → 场景指向已失踪的旧资源 ID |

| 影响 | 结论 |
|------|------|
| Setup `LoadAssetAtPath(壳路径)` / `CopyAsset` | ✅ **可用**（按路径，不靠场景实例） |
| 场景内嵌 KenMuNiStart 实例 | ❌ Missing Prefab（开场戏若靠该实例会坏） |
| 本 bug（门口 Load） | **先跑 Setup 即治**；场景 guid 重绑为 **P1** |

**P1 修场景 Missing（可选）**：在 Unity 把场景里 Missing 的 `Village_KenMuNiStart` 实例删掉，重新拖入当前壳 Prefab 并保存；或把 YAML 中 `397659d3…` 全局替换为 `aace8b8b…`（须核对 fileID 是否仍匹配，优先编辑器重挂）。

---

## ③ 修复步骤（施工）

### 主路径（必做）

1. 打开 Unity 2020.3.48f1，工程加载完成。  
2. 菜单：**`Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab`**  
   - 脚本：`VillageChiefDoorDialogueSetupEditor.SetupFromMenu`  
   - 顺序：UI `ChiefPainting` → CopyAsset 壳 → 嵌三立绘/BB → CSV Import → Save Prefab  
3. 确认落盘：  
   - `Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab`  
   - `Assets/GameRes/DialogueTrees/Generated/` 下对应 Generated 树  
4. Play → 靠近 `Npc_Chief` → 不应再「加载资源失败」。

### 若菜单报错

| Console | 处理 |
|---------|------|
| `ChiefPainting UI 化失败` | 查 `ChiefPaintingSetupEditor`；修 UI 大立绘后再跑 |
| `壳 Prefab 不存在` | 确认壳路径文件未被删/移 |
| `CopyAsset 失败` | 查只读/路径占用；勿改 Trigger 名绕过 |
| CSV / Face3 非法 | 回归 Face123 施工（C1 应已合入） |

### 明确不做

- ❌ 改 `ResMgr` / 全项目 AB 当主修  
- ❌ 删 `Npc_Chief` 装修好  
- ❌ 手工空 Prefab 无 DialogueTree  
- ❌ 改 Story 名绕过路径  

---

## ④ 名与路径对照（H3 已对齐）

| 源 | 值 |
|----|-----|
| `ChiefNearDoorStoryTrigger.DoorStoryPrefabName` | `Village_村长家门口初次对话` |
| 场景 `Npc_Chief.StoryPrefabName` | 同上 |
| `DialoguePath.GetPath` | `Assets/GameRes/Prefabs/Dialogue/{name}.prefab` |
| Setup `TargetPrefabPath` | 同上完整路径 |

---

## ⑤ 验收

- [ ] Project 中可见 `Village_村长家门口初次对话.prefab`  
- [ ] Play 靠近 `Npc_Chief`：无该路径「加载资源失败」  
- [ ] `TriggerStory` 能打开对白壳（立绘/脸可另案）  
- [ ] Setup 无 Error；H4 场景 Missing 已说明或已重挂  

---

## ⑥ OPEN / 残留

| ID | 问题 | 状态 |
|----|------|------|
| Q1 | 根因？ | ✅ **H1** |
| Q2 | 场景 KenMuNiStart guid 断链？ | ⏳ P1（不挡 Setup） |
| Q3 | Editor Play 新建 Prefab 是否要 Refresh Resource？ | 跑完 Setup 后 `Refresh` 已在菜单末；若仍失败再查 H2 |
| Q4 | Agent 能否代点菜单？ | ❌ 须用户/本机 Unity 执行 |

---

## ⑦ 程序速查

| 路径 | 用途 |
|------|------|
| `Editor/.../VillageChiefDoorDialogueSetupEditor.cs` | 一键生成成品 Prefab |
| `DialoguePath.GetPath` | 加载路径规则 |
| `StoryComponentGSM.TriggerStory` | `ResMgr.LoadAsset` 入口 |
| `ResComponentGM` ~57 | `加载资源失败` 日志 |
| `Village_KenMuNiStart.prefab` | Setup 壳（guid `aace8b8b…`） |
| `Dialog/Village_村长家门口初次对话.csv` | Import 台本 |
