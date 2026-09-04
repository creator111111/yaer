# Village_Shop · Bake 只写 ShopPanel、场景 UI_Shop 名图仍 None — 架构溯源与修复执行说明

**文档版本**：v1（2026-07-21）  
**文档性质**：【架构侦探】只读溯源 + **修复施工指引**（本阶段先落文档；施工员按 §⑥ 改 Bake 目标后再烤场景）  
**触发**：`MainItemDatabase` 已挂齐 CostItem 三语 `shopNameSprite*`，并已跑 **Tools → Shop → Bake Shop Lists From MainItemDatabase**；在 **`Village_Shop` 场景**（非 Prefab 编辑模式）里选中列表行 `Name`，Inspector **Source Image 仍为 None**。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0713/Village_Shop_关FightingPanel与ShopPanel同黑幕节奏_执行说明.md`（**弃用** OpenUIForm(`ShopPanel`)；进店仍用场景 **合层 + `UI_Shop`**）
- `Assets/Doc/执行文档/0715/Shop_Bar_Name改商店名图_MainItem加shopNameSprite_架构溯源与施工执行说明.md`
- `Assets/Doc/执行文档/0721/MainItem_CostItem_ShopNameSprite三语配置_架构溯源与施工执行说明.md`
- 关联脚本：`ShopListBakeEditor.cs`、`ShopFormLogic.cs`、`ShopBarRowView.cs`
- 关联资源：`Village_Shop.unity`、`ShopPanel.prefab`、`Shop_Bar.prefab`、`MainItemDatabase.asset`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**不是 Database 没挂上，也不是你看错成 `Shop_Bar` 模板；是 Bake 菜单在存在 `ShopPanel.prefab` 时只烤 Prefab、跳过场景保存，而进店真正用的是场景里的 `UI_Shop`。场景行仍停在「TMP 写 `m_text`」的旧覆盖，Name 已改成 Image 后就只剩空 Sprite → Inspector 永远 None。**

**生活类比**：
- 你把新店招贴纸写进了「备用样板册」（`ShopPanel.prefab`）。
- 店里柜台上的价签（场景 `UI_Shop`）还是旧的「打印字」覆盖；字组件已经拆掉改成贴图板了，板子上却没贴图 → 空白。
- 刷新样板册不会自动换柜台上的价签。

---

## ② 玩家 / 策划会遇到什么现象

| # | 现象 | 原因归属 |
|---|------|----------|
| 1 | Database 三语槽已有图 | ✅ 数据侧 OK |
| 2 | Bake 弹窗成功 / Console 写「已写回 ShopPanel.prefab」 | ✅ 烤了**另一份** UI |
| 3 | `Village_Shop` → `UI_Shop` → `Shop_Bar_HpBall` → `Name` → Source Image = **None** | ❌ 场景未被本次 Bake 更新 |
| 4 | 同场景 Name 的 Prefab 覆盖仍是 `m_text`（如「生命之珠」） | ❌ 旧 TMP Bake 残留；Image 不认 `m_text` |
| 5 | `ShopPanel.prefab` 同行 Name 已是 `m_Sprite` → 生命球 guid | ✅ Prefab 烤对了，但进店不走它 |

---

## ③ 架构溯源（只读证据）

### 3.1 进店真正显示哪份 UI？

| 文档 / 工程决策 | 结论 |
|-----------------|------|
| 0713「关 FightingPanel + ShopPanel 同黑幕」**修订** | OpenUIForm(`ShopPanel`) **弃用**；继续 **场景合层 + `UI_Shop` 双轨** |
| `Village_Shop.unity` | 存在启用的 **`UI_Shop`** + `Bar_ListScroll_Buy/Sell` + 已实例化的 `Shop_Bar_*` |
| Play 路径 | `ShopFormLogic` 挂在场景 `UI_Shop` 上绑行；**不是**每次 OpenUIForm 拉 `ShopPanel` |

→ **验收名图必须以场景 `UI_Shop` 为准。**

### 3.2 Bake 实际写了哪里？

`ShopListBakeEditor.RunBake`（现行逻辑）：

```
若存在 Assets/GameRes/Prefabs/UI/ShopPanel.prefab
  → LoadPrefabContents(ShopPanel)
  → RunBakeOnRoot(..., skipSceneSave: true)   ★ 不写场景
  → SaveAsPrefabAsset → 只更新 ShopPanel.prefab
否则
  → 找场景 UI_Shop → 写场景并 SaveOpenScenes
```

Console 典型日志：`[ShopListBake] 目标：ShopPanel.prefab（OpenUIForm 正式面板）`  
→ 与 0713「已弃用 OpenUIForm ShopPanel」**脱节**。

### 3.3 场景 vs Prefab 磁盘对比（HpBall Name）

同一组件 fileID `8763737385084576367`（`Shop_Bar` 上 Name 的 Image）：

| 资源 | Name 覆盖字段 | 值 |
|------|---------------|-----|
| **`Village_Shop.unity`** `Shop_Bar_HpBall` | **`m_text`** | `"生命之珠"`（旧 TMP Bake） |
| **`ShopPanel.prefab`** `Shop_Bar_HpBall` | **`m_Sprite`** | guid `6aa38517…`（中文「生命球」）✅ |
| **`Shop_Bar.prefab` 模板** | `m_Sprite` | `{fileID: 0}`（模板故意空，正常） |

场景里 **没有** Name 的 `m_Sprite` 覆盖 → 继承模板空图 → Inspector **None**。  
`m_text` 覆盖对 Image **无效**（字段不属于 Image），看起来像「刷新了还是空」。

### 3.4 数据流（现状 → 目标）

```
【现状 · 错误】
MainItemDatabase.shopNameSprite*  ✅ 已配置
        ↓ Bake
ShopPanel.prefab 行 Name.m_Sprite  ✅ 有图（但进店不用）
Village_Shop.UI_Shop 行 Name       ❌ 仍 m_text / Sprite 空

【目标 · 修复后】
MainItemDatabase.shopNameSprite*
        ↓ Bake（主烤场景 UI_Shop；可选顺带烤 ShopPanel）
Village_Shop.UI_Shop → Shop_Bar_* → Name.Image.sprite  ✅ 中文预览
        ↓ Play
ShopFormLogic.RefreshAllShopNamesForLanguage → 按当前语言重贴
```

### 3.5 次要：Play 时是否「碰巧能好」？

`ShopFormLogic` 进店会 `RefreshAllShopNamesForLanguage()`，运行时**有机会**从 Database Resolve 出图并贴到场景行。  
但：

- **Editor 不 Play** 时场景仍 None → 策划验收失败；
- 若行未绑 `ShopBarRowView` / Resolve 失败，Play 也空白；
- 正确做法仍是 **场景 Bake 预览与 Play 同源数据**。

---

## ④ 范围冻结

| 项 | 约定 |
|----|------|
| **根因定性** | Bake **写错目标**（只写弃用路径的 `ShopPanel`），不是 Database / 美术路径问题 |
| **本修复要做** | Bake **必须以场景 `UI_Shop` 为必烤目标**；跑完后 `Village_Shop` 行 Name 有中文 `m_Sprite` |
| **可选** | 同时烤 `ShopPanel.prefab`（防日后又启用 OpenUIForm，避免双份漂移） |
| **不做** | 再改三语字段名；迁 `ShopName` 目录；恢复 OpenUIForm(`ShopPanel`) 为主路径（0713 已否决） |
| **不做** | 手改几十处场景 YAML `m_text`→`m_Sprite`（易漏；应用 Bake 清子物体重实例化） |

---

## ⑤ 根因一句话（给审查）

| ID | 根因 | 证据 |
|----|------|------|
| R1 | `ShopListBakeEditor` 有 Prefab 就 **只烤 ShopPanel + skipSceneSave** | `RunBake` 115～148 行 |
| R2 | 进店 UI = 场景 `UI_Shop` | 0713 修订文档 + 场景内实例 |
| R3 | 场景行 Name 仍是 TMP 时代 `m_text` 覆盖 | `Village_Shop.unity` 全行 `876373…` → `m_text` |
| R4 | Name 已是 Image，空 Sprite + 无效 `m_text` → None | `Shop_Bar.prefab` + 场景覆盖 |

---

## ⑥ 修复施工步骤（施工员）

### 方案 A · 改 Bake 目标（推荐 · 最小且对齐 0713）

**文件**：`Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/Editor/ShopListBakeEditor.cs`

**目标行为**：

1. **始终**打开 / 使用 `Village_Shop` 场景中的 `UI_Shop`，执行 `RunBakeOnRoot(..., skipSceneSave: false)`，并 `SaveOpenScenes`。  
2. **可选第二趟**：若存在 `ShopPanel.prefab`，再 `LoadPrefabContents` 烤一遍写回 Prefab（保持双份不漂）。  
3. 汇总对话框写清：`已更新场景 UI_Shop` /（若有）`已同步 ShopPanel.prefab`。  
4. 去掉或改正误导文案「OpenUIForm 正式面板」——改为「场景进店主路径 UI_Shop；ShopPanel 为可选镜像」。

**伪代码倾向**：

```csharp
// 1) 必烤场景（进店真源）
EnsureVillageShopSceneOpen();
var uiShopScene = Find UI_Shop in scene;
RunBakeOnRoot(uiShopScene, showDialog: false, skipSceneSave: false);

// 2) 可选镜像 Prefab（不替代场景）
if (ShopPanel.prefab exists) {
  var contents = LoadPrefabContents(...);
  try { RunBakeOnRoot(contents, ..., skipSceneSave: true); }
  finally { SaveAsPrefabAsset; UnloadPrefabContents; }
}

Report("场景 UI_Shop 已烤；ShopPanel 已同步（若存在）");
```

**复杂逻辑替代说明**：

| 方案 | 做法 | 为何不优先 |
|------|------|------------|
| **A. 场景必烤 + Prefab 可选同步（推荐）** | 对齐 0713 | — |
| B. 只烤场景、删除/忽略 ShopPanel | 更简单 | Prefab 长期腐烂，日后误用更惨 |
| C. 改回进店 OpenUIForm(ShopPanel) | 一次只维护 Prefab | **0713 已因错位弃用**，勿擅自改核心方向 |
| D. 手工在场景拖 8×3 张 Name 图 | 零代码 | 易漏、下次 Bake 若仍只烤 Prefab又漂 |

### 方案 B · 不改代码的临时验收（仅应急）

1. 临时把 `ShopPanel.prefab` **移出/改名**，使 Bake 走「找不到 Prefab → 烤场景」分支。  
2. 再跑一次 Bake → 场景应出现 `m_Sprite`。  
3. **事后必须恢复 Prefab**，并尽快合入方案 A（临时挪文件易忘、易进错误提交）。

> 不推荐作为正式流程。

### 施工后必做

```
① 合入方案 A 后：打开 Village_Shop
② Tools → Shop → Bake Shop Lists From MainItemDatabase
③ Console 确认「已更新场景 UI_Shop」（不应再只有 ShopPanel）
④ Hierarchy：UI_Shop → … → Shop_Bar_HpBall → Name
   Source Image = 生命球（非 None）
⑤ Ctrl+S 保存场景
⑥ Play 进店：名图可见；切语言可换图（Resolve）
```

**为何必须再 Bake 场景**：`BakeContent` 会 `ClearChildren` 再实例化行，才能清掉无效 `m_text` 覆盖并写入 `m_Sprite`。只改 Database / 只烤 Prefab **不够**。

---

## ⑦ 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| V1 | Bake 后 Console | 明确提到 **场景 UI_Shop** 已烤 |
| V2 | 场景 `Shop_Bar_HpBall/Name` | Source Image = 生命球；**不是** None |
| V3 | 文本搜索 `Village_Shop.unity` | Name 覆盖为 **`m_Sprite`**；不再对 Name 写 **`m_text`** |
| V4 | `ShopPanel.prefab`（若仍同步） | 与场景同行名图一致（可选） |
| V5 | Play 进店购买列表 | 名图显示；滚轮 / 价 / 数量正常 |
| V6 | 故意只改 Database 中文图再 Bake | 场景 Name 跟着变 |

建议日志前缀：`[ShopListBake]` / `[ShopNameSprite]`。

---

## ⑧ 提交说明模板（修复合入后填）

**改了哪些文件**：  
`ShopListBakeEditor.cs`  
（Bake 后）`Village_Shop.unity`  
（若同步）`ShopPanel.prefab`

**实现了什么**：Bake 以场景 `UI_Shop` 为进店真源再写名图；避免只更新弃用路径 `ShopPanel` 导致场景 Name 仍 None。

**如何验证**：按 §⑦；正规进店 Init → 村 → Door_Shop。

---

## ⑨ OPEN_QUESTIONS

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | `ShopPanel.prefab` 是否长期保留并每次双烤？ | **是**，作镜像；进店仍以场景为准 | 待确认 |
| Q2 | 是否删除 ShopPanel / 相关 Build 菜单以免再误导？ | **本期不删**；只改 Bake 文案与目标 | 待确认 |
| Q3 | Play 无 Bake 是否允许仅靠 Resolve 贴图？ | **允许作底线**；Editor 验收仍要求场景 Bake 有预览 | 待确认 |

有结论后可同步 `Assets/Doc/OPEN_QUESTIONS.md`。

---

## ⑩ 给程序看的补丁要点（极短）

1. **Bug**：`RunBake` 有 `ShopPanel` 就 `skipSceneSave`，场景 `UI_Shop` 不更新。  
2. **Fix**：场景 `UI_Shop` **必烤**；Prefab **可选第二趟**。  
3. **验收点**：`Village_Shop` → `Shop_Bar_HpBall/Name` → `m_Sprite` ≠ 0。  
4. **勿**再把进店主路径改回 OpenUIForm(`ShopPanel`)（0713 已否决）。

---

**文档路径**：`Assets/Doc/执行文档/0721/Village_Shop_Bake只写ShopPanel_场景Name仍None_架构溯源与修复执行说明.md`

| 版本 | 日期 | 说明 |
|------|------|------|
| v1 | 2026-07-21 | 架构侦探：定位 Bake 目标与场景 m_text 残留；给出场景必烤修复方案 |
| v1.1 | 2026-07-21 | **已施工**：`ShopListBakeEditor.RunBake` 改为场景必烤 + ShopPanel 可选镜像；场景 11 行 Name 覆盖 `m_text`→`m_Sprite` |
