# Cursor Agent Prompt · Village_ShopChest：对齐 Head（光标 + 雅儿大立绘 + 对话框不出现 Bug）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **场景 / 交互**：`Village_Shop` · `Trigger/Chest` → 播 `Village_ShopChest`  
> **对照金样（必须逐项对齐）**：`Trigger/Head` → `Village_ShopHead`（已验收：Catch 光标、雅儿大立绘、对话框可见）  
> **产品目标（白话）**：点胸互动要和点头**一样好用**——  
> 1. 鼠标悬停胸部 → **光标变化**（同 Head 的 Catch）  
> 2. 对白中 → **雅儿大立绘出来**（先立绘后框，对齐 Head 时序）  
> 3. **对话框必须出现**（用户反馈：现在对话框都不会出现，疑似 Bug）  
> **前序**：0830 已改故事名常量 → `Village_ShopChest`（门牌对齐）；本期查 **表现/壳层缺口**，不是再改名  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_ShopChest_对齐Head光标立绘与对话框Bug_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户原话 → 三个必须直接回答

| # | 用户说 | 预扫假说（须证伪） |
|---|--------|-------------------|
| 1 | 要和头部一样，鼠标光标变化 | 场景 **Head** 已挂 `CursorChangeTrigger`（TargetState=Catch）；**Chest 只有** Collider + Hotspot，**无** CursorChangeTrigger → 悬停不变光标 |
| 2 | 雅儿的大立绘要出来 | `Village_ShopChest` 已嵌 `GoOutStoryYaerPainting` 且 BB 似已绑，但 **`m_Alpha` 多为 0**；点胸 Generated 图预扫 **无** `CanvasGroupAlpha` 淡入 → 立绘永远看不见（同 Head 修前态） |
| 3 | 对话框都不会出现，可能有 bug | 点胸图 `Village_商店点胸交互.asset` 预扫 **只有 StatementNode**，**没有** `FightingPanelVisible` + `NormalDialogueUIAlpha`；Head 图有 UIAlpha → 对话框壳可能一直 alpha=0 / 未打开 |

### 金样对照表（侦探必须填实）

| 能力 | Head（金样） | Chest（现网假说） | 缺口 |
|------|--------------|-------------------|------|
| 故事名 = Prefab | `Village_ShopHead` | `Village_ShopChest`（0830 已改） | 门牌 ✅；非本期主因 |
| 点击 → Special | Hotspot → `TryTriggerShopkeeperSpecial` | 同门 | ✅ |
| 悬停光标 | `CursorChangeTrigger` Catch | **无组件** | **P0** |
| 雅大立绘物体 | GoOut 嵌 + BB | 预扫有嵌+绑 | 查 alpha/淡入 |
| 立绘淡入时序 | Fighting → **CanvasGroupAlpha 立绘** → UIAlpha → 句 | 预扫图无立绘 Alpha | **P0** |
| 对话框淡入 | `NormalDialogueUIAlpha` EndAlpha=1 | 预扫图 **无** UIAlpha | **P0 · 对话框不出现主嫌** |
| 结束 Reset 脸 | Special onStoryEnd | 同门 | 核实 |

### 「对话框不出现」根因树（按优先级证伪）

| # | 假说 | 如何证伪 |
|---|------|----------|
| D1 | 点胸图缺 `NormalDialogueUIAlpha` / Fighting 前奏，Panel 不显 | 对比 Head Prefab 内嵌图序 vs `Village_商店点胸交互.asset` 节点列表 |
| D2 | TriggerStory 未真正 started（仍旧名 / Missing） | Console：`story=Village_ShopChest started=`；是否 Missing Prefab |
| D3 | Prefab 绑的是外部 `_graph`，壳层 Action 在 Prefab 本地图却为空 | 查 `_boundGraphSerialization` 是否空、谁在跑 |
| D4 | Hide UI / 层级挡住对话框 | Hierarchy 播时 `NormalDialogueNewPanel` active/alpha |
| D5 | Actor/语句立刻 End 无可见帧 | 句节点与 onStoryEnd 时序 |

**倾向**：D1 为主因（Import CSV 只生成 Statement，未抄 Head 壳层前奏）；侦探须用磁盘钉死，勿只猜。

### 雅儿大立绘（对齐 Head 0829 报告）

| 项 | 倾向 |
|----|------|
| 样板 | `Village_ShopHead`：**先立绘后对话框**（T1） |
| 做法 | 在 Fighting 之后、UIAlpha 之前串行 `CanvasGroupAlpha(GoOutStoryYaerPainting)` 0→1 |
| 禁止 | 把 Mask 小头相当大立绘；在 Prefab 再嵌老板娘 Painting |
| 店 | 仍用场景合层 + `UseShopkeeperPortrait` |

### 光标（对齐 Head Catch 施工）

| 项 | 倾向 |
|----|------|
| 方案 | 同物体挂 `CursorChangeTrigger`，**TargetState = Catch**（与 Head 一致，用户已定） |
| 开关 | `SetShopkeeperHotspotsEnabled` 已同步 disable CursorChangeTrigger——Chest 挂上后应自动进总开关（须核实代码是否遍历子物体组件） |
| 禁止 | 直接 `Cursor.SetCursor`；另起一套光标 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 查清对话框不出现根因 + 最小修法 | ❌ 再改故事名（已 ShopChest） |
| ✅ Chest 补 Catch 光标（对齐 Head） | ❌ C6 树屋 |
| ✅ 雅大立绘显隐 + 先立绘后框时序 | ❌ 重做点头线 |
| ✅ Prefab/Generated 图壳层补齐方案 | ❌ 改 CSV 剧情文案（除非缺句导致立刻结束） |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 宣称「门牌已对齐所以对话框一定会出现」而不对拍图节点  
- 只改 Chest 光标、不查对话框壳  
- 把点胸并进 ShopHead 同一棵图  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_ShopChest.prefab` | 点胸对白壳 / 立绘 |
| `Village_ShopHead.prefab` | 金样：光标无关；立绘+UIAlpha 时序 |
| `Village_商店点胸交互.asset` | 现网点胸图节点（对话框嫌疑） |
| `Village_Shop.unity` · Head / Chest | CursorChangeTrigger 有无 |
| `Village_ShopSceneManager` · Special / 热区开关 | 总开关是否带 Cursor |
| `0829/...Head悬停光标Catch_施工说明` | Catch 挂法 |
| `0829/...ShopHead_雅儿大立绘` + `先立绘后对话框时序` | 立绘/时序金样 |
| `0830/...Chest热区安装...施工说明` | 门牌已改；未做 Catch |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0830/Village_Shop_Chest热区安装Village_ShopChest对话_架构溯源报告.md
@Assets/Doc/施工说明/0830/Village_Shop_Chest热区安装Village_ShopChest_施工说明.md
@Assets/Doc/执行文档/0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md
@Assets/Doc/执行文档/0829/Village_ShopHead_先立绘后对话框时序_架构溯源报告.md
@Assets/Doc/执行文档/0829/Village_Shop_Head悬停光标Catch_施工说明.md
@Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/GameRes/DialogueTrees/Generated/Village_商店点胸交互.asset
@Assets/Dialog/Village_商店点胸交互.csv
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/ShopkeeperBodyHotspot.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C# / TextMeshPro / NodeCanvas。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 点胸要和点头一样：悬停变光标、雅儿大立绘出来、对话框正常出来说话。  
2. 用户实测：**对话框都不会出现**——优先当 Bug 查清。  
3. 故事名已改成 `Village_ShopChest`；本期重点是 **壳层演出 + 光标**，对齐 Head。

---

## 侦探任务清单

### A. 复现「对话框不出现」（P0）
- 画点胸 Trigger → Story → Panel 链路。  
- 对比 Head / Chest **图节点序**（Fighting / UIAlpha / 立绘 Alpha / Statement）。  
- 钉死主因（D1～D5）；给出最小修法：补哪些 Action、改 Prefab 本地图还是 Generated 图、是否要改 Import 流程防回潮。

### B. 雅儿大立绘（P0）
- 物体 / BB / alpha / 图内淡入是否齐。  
- 拍板时序：**对齐 Head T1（先立绘后框）**。  
- 列出 Prefab 最小补齐（可引用 Head 已施工节点参数）。

### C. 悬停光标（P0）
- 确认 Head 有、Chest 无 `CursorChangeTrigger`。  
- 拍板：Chest 同挂 Catch；核实 `SetShopkeeperHotspotsEnabled` 是否已遍历到 Chest 上新组件。  
- 对白中关热区后光标是否回 Normal。

### D. 回归边界
- 点头线不回退；Start/Repeat/Yes/No 不误伤。  
- C6 树屋仍不做。

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 修对话框壳：补 Fighting + NormalDialogueUIAlpha（及必要前奏） | **P0** |
| 2 | 补雅立绘 CanvasGroupAlpha；时序先立绘后框 | **P0** |
| 3 | Chest 挂 CursorChangeTrigger = Catch | **P0** |
| 4 | 验收：点胸有框、有立绘、悬停变手 | **P0** |
| 5 | Setup 菜单/文档注明点胸图须含壳层，防 CSV 重导冲掉 | P1 |
| 6 | MerchantPainting Prefab 源同步 Cursor（若场景-only） | P1 |

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 鼠标移入 Chest | 光标变 **Catch**（同 Head）；移出恢复 |
| 2 | Idle 点 Chest | **对话框出现**可读文案；藏 UI_Shop |
| 3 | 对白开始段 | **雅儿大立绘可见**（先于或按拍板序相对对话框） |
| 4 | 店句 / 雅句 | 合层脸 + 雅大立绘/Mask 跟句 |
| 5 | 结束 | Idle；UI 显；热区开；光标正常；脸复位 |
| 6 | 点 Head | 回归：光标/立绘/对话框仍正常 |
| 7 | Console | `Village_ShopChest started=true`；无 Missing；无立刻 onStoryEnd 无句 |

### G. 开放问题
- 壳层 Action 写在 Prefab `_boundGraph` 还是改 Generated asset？何者防 CSV 重导覆盖？  
- 点胸 UIAlpha 参数是否 1:1 抄 Head（Duration/Delay/PrepareMask）？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_ShopChest_对齐Head光标立绘与对话框Bug_架构溯源报告.md`

MASTER 四段式：  
① 结论（对话框主因一句话 + 光标/立绘缺口 + 对齐方案）  
② 原因（通俗：为什么点头有框点胸没有；立绘/光标差在哪）  
③ 用户检查清单（怎么验三件事）  
④ 给程序：Head↔Chest 对照表 + 图节点 diff + 最小施工 + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_ShopChest_对齐Head光标立绘与对话框Bug_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/GameRes/DialogueTrees/Generated/Village_商店点胸交互.asset
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs

你现在是【施工员】。按报告把点胸对齐点头：Catch 光标、雅儿大立绘、对话框可见。

必须遵守：
- 优先修「对话框不出现」根因（壳层 Fighting/UIAlpha 等），再补立绘淡入与 Chest CursorChangeTrigger=Catch；
- 时序对齐 Head：先雅儿大立绘后对话框；店仍走合层 Portrait；
- 禁止直接 Cursor.SetCursor；禁止改故事名回旧 ChestClick；不做 C6；
- 代码/Prefab 含详细注释或节点备注；重要取舍写清原因。

提交说明：改了图哪些节点、场景是否挂 Cursor、如何验收三件事、未做项。
```
