# Cursor Agent Prompt · Village_Shop：把 `Village_ShopChest` 对话装到商人 Chest 热区

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **场景**：`Village_Shop.unity` · `商店界面合层` → `MerchantPainting` → `Trigger` → **`Chest`**  
> **对白资产（用户指定真源）**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab`  
> **产品目标（白话）**：玩家在商店 **点老板娘胸部** → 播 **`Village_ShopChest`** 这段对话（店/雅表情照常变）  
> **对照先例**：0829 点头已用方案 A 把常量改成 `Village_ShopHead`；点胸现网仍写旧名，应对齐同一套做法  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_Shop_Chest热区安装Village_ShopChest对话_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

> 现在就用 **`Village_ShopChest.prefab`** 接到商人 **Chest** 交互上。  
> 点胸应播这段图；不要再假设磁盘上一定有 `Village_ShopKeeper_ChestClick.prefab`（0828/编辑器菜单写的是这个旧名，**用户现网资产名已是 ShopChest**）。

| 项 | 期望 |
|----|------|
| 交互点 | Hierarchy：`…/MerchantPainting/Trigger/Chest` |
| 对白 Prefab | **`Village_ShopChest`**（路径见上） |
| Idle 可点 | 非对白中点 Chest → 启动对白 |
| 对白中 | 禁止叠第二段；藏买卖 UI（对齐 `TryTriggerShopkeeperSpecial`） |
| 表情 | 店句走 Face1～5 + BodyType + Registry/Mask；雅走 DialogueFaceType（以 Prefab/CSV 为准） |
| 本期范围 | **仅点胸店内段闭环（C1～C5 语义）**；C6+ 树屋黑屏转场 **不做** |

### 与现网已施工资产的关系（关键缺口假说）

| 层 | 预扫假说 | 侦探要答 |
|----|----------|----------|
| 热区 | 场景应有 `Trigger/Chest` + `ShopkeeperBodyHotspot`（`hotspotKind=Chest`）+ Collider2D | Chest 组件是否齐？与 Head 同父 Trigger？ |
| GSM | `TryTriggerShopkeeperSpecial` + Hide/Show UI + 热区开关（点头已通） | 点胸是否已走同一门？ |
| 常量名 | `ShopkeeperChestClickStoryName = "Village_ShopKeeper_ChestClick"` | **与用户 Prefab 名 `Village_ShopChest` 是否不一致？** |
| Hotspot 注释 | `ShopkeeperBodyHotspot` Chest 仍写旧名 `…_ChestClick` | 文档/注释过时？ |
| 编辑器菜单 | `ShopkeeperSpecialClickSetupEditor` → `Village_ShopKeeper_ChestClick.prefab` | 是否生成了空/错路径？与 `Village_ShopChest` 谁是真源？ |
| Prefab 磁盘 | **有** `Village_ShopChest.prefab`；预扫 **未见** `Village_ShopKeeper_ChestClick.prefab` | 命名撕裂 → TriggerStory 按旧名加载会失败？ |
| CSV / 图 | `Assets/Dialog/Village_商店点胸交互.csv` + Generated `Village_商店点胸交互.asset` | `Village_ShopChest` 是否已 Bind 该图？文案/表情对齐？ |
| 点头先例 | Head 已改常量 → `"Village_ShopHead"` | Chest **应对齐方案 A** |

**核心假说（须裁定）**：

```
点 Chest
  → ShopkeeperBodyHotspot (Kind=Chest)
  → GSM.TryTriggerShopkeeperSpecial("Village_ShopKeeper_ChestClick")
  → StoryComponentGSM.TriggerStory(name)
  → ResMgr.LoadAsset(DialoguePath.GetPath(name))
       ✗ 若 name≠Prefab 文件名 → 加载失败 / 无对白
       ✓ 用户资产 = Village_ShopChest.prefab → 故事名应等于 "Village_ShopChest"
```

生活类比：胸部门铃（Chest 热区）可能已装好，管家（GSM）仍按旧门牌喊「ChestClick 房间」，真正住着对白的房间门牌是 **`Village_ShopChest`**——侦探要钉死：改门牌常量（推荐）、改 Prefab 名、还是别名映射。

### 命名方案候选（侦探必拍板一个）

| 方案 | 做法 | 优点 | 风险 | 倾向 |
|------|------|------|------|------|
| **A · 改故事常量/调用名为 `Village_ShopChest`** | GSM 常量、Hotspot 注释、Editor Setup 路径一并改成 `Village_ShopChest` | **尊重用户指定 Prefab**；对齐点头方案 A | 旧文档/CSV 建议名残留 | **✅ 推荐** |
| **B · Prefab 改名为 `Village_ShopKeeper_ChestClick`** | 磁盘 Rename + 保持代码常量 | 对齐 0828 旧名 | 用户已定名 ShopChest；易丢 meta | 仅当 ShopChest 图残缺、旧名才是完整资产时 |
| **C · 别名映射** | Trigger 时旧名映射到 ShopChest | 兼容两名 | 魔法字符串易再漂 | ❌ |

**产品原话优先**：使用 `Village_ShopChest` → 默认 **A**。

### 须核对的 Prefab 完备性（`Village_ShopChest`）

对照 `Village_ShopHead` / `Village_ShopStart` 最小壳 + 0601 点胸台本：

| 检查项 | 期望 |
|--------|------|
| 根名 / DialogueTree 绑定 | 可被 `DialoguePath.GetPath("Village_ShopChest")` 加载 |
| Actor | 至少 **Merchant**；是否含 **Yaer**（预扫 Prefab 有 Yaer 子物体） |
| 店句 | `UseShopkeeperPortrait=true`；Face/Body 跟句 |
| 图内容 | 是否等于点胸 CSV C1～C5（店内段）；是否误含 C6+ 树屋节点 |
| 与 CSV | `Village_商店点胸交互.csv` 是否已 Import / Bind |
| 与 Head 互斥 | `HasRunningStory` 已挡；结束 Reset 合层脸 |

### 热区 / 点击（若未装齐）

| 检查 | 期望 |
|------|------|
| `Trigger/Chest` 存在 | Collider2D + `ShopkeeperBodyHotspot` Kind=Chest |
| Physics2DRaycaster | 相机上有（点头能点则多半已有） |
| 热区开关 | `SetShopkeeperHotspotsEnabled` 含 Chest |
| 悬停 Catch 光标 | **本期可选**；0829 明确 Head 有、Chest 曾排除——开放问题，默认不做除非用户要 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 点 Chest → 播 `Village_ShopChest` 闭环 | ❌ 重做首次进店 / Repeat / Yes/No |
| ✅ 命名/加载链路拍板 + 最小施工清单 | ❌ C6+ 黑屏转树屋 / `…_ChestClick_Treehouse` |
| ✅ Prefab 完备性 + 热区是否齐 | ❌ 扩 `DialogueFaceType` |
| ✅ Editor Setup 路径与常量同步 | ❌ 把点胸并进 ShopStart / ShopHead 同一棵图 |
| ✅ 对白结束 Idle Reset 脸（复用特殊对白结束） | ❌ 在 Update 轮询点击 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 未核实就断定「已接好」或「必须新建整套点胸系统」  
- 把点胸并进 `Village_ShopStart` / `Village_ShopHead`  
- 店句走 Smile/Angry 进 `DialogueFaceType`  
- 本期强做 C6+ 树屋转场  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_ShopChest.prefab` | **本期对白真源** |
| `Village_商店点胸交互.csv` + Generated `.asset` | 台本/表情对照 |
| `ShopkeeperBodyHotspot.cs` | Chest 点击入口 |
| `Village_ShopSceneManager.cs` | `ShopkeeperChestClickStoryName` / `TryTriggerShopkeeperSpecial` |
| `ShopkeeperSpecialClickSetupEditor.cs` | 旧 ChestClick 路径 |
| `Village_ShopHead.prefab` + 0829 Head 热区报告 | 命名方案 A 先例 |
| `0828/…Trigger特殊交互对话_架构溯源报告.md` | 点击方案 B、状态机 |
| `0601/Village_商店老板娘特殊交互_对白台本_执行说明.md` | C1～C5 / C6+ 边界 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md
@Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md
@Assets/Doc/执行文档/0829/Village_Shop_Head热区安装Village_ShopHead对话_架构溯源报告.md
@Assets/Doc/执行文档/0828/Village_Shop_商人默认Face1Normal与Body_YinXian_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/Dialog/Village_商店点胸交互.csv
@Assets/GameRes/DialogueTrees/Generated/Village_商店点胸交互.asset
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/ShopkeeperBodyHotspot.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Editor/Tool/Dialogue/ShopkeeperSpecialClickSetupEditor.cs
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写「Chest 热区 → Village_ShopChest」溯源报告。

---

## 背景（策划白话）

1. 点商人胸部要播 **`Village_ShopChest`** 这段对话。  
2. Prefab 已在工程里；热区/点击管线点头侧多半已通——点胸缺的是 **故事名与 Prefab 文件名对齐**（及热区/图是否齐）。  
3. 本阶段只摸清：Chest 热区在不在、常量是否仍指向不存在的旧名、Prefab 能否直接 Trigger、要否同步 Editor 菜单。

---

## 侦探任务清单

### A. 钉死 Prefab 真源与可播性
`Village_ShopChest`：根名、Actor、句数/文案、店 Portrait、是否 Bind 点胸 CSV 图、有无 C6+ 节点。  
确认磁盘 **无**（或有）`Village_ShopKeeper_ChestClick.prefab`。

### B. 钉死点击 → Trigger 链路
```
Chest Collider → ShopkeeperBodyHotspot → TryTriggerShopkeeperSpecial(常量) → TriggerStory → Load Prefab
```
标出现网常量字符串 vs 用户 Prefab 名；加载是否必失败。

### C. 命名方案拍板
推荐 **A**：`ShopkeeperChestClickStoryName = "Village_ShopChest"`；Hotspot 注释 + Editor Setup 路径同步。  
对齐点头 0829 方案 A；否决无必要的别名层。

### D. 热区完备性
场景 `Trigger/Chest`：Collider、Kind、Raycaster、热区开关是否含 Chest。  
缺则写入施工「补装热区」（可复用 Setup 菜单，但路径须先改）。

### E. 状态机与互斥
与 ShopStart / Repeat / Head / Yes/No / ESC：对白中藏 UI、关热区、结束 ResetDefault。  
点胸 **可重复触发**（不做存档只播一次），与点头一致。

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 常量改为 `"Village_ShopChest"` | **P0** |
| 2 | Hotspot / Editor 注释与 Prefab 路径同步 | **P0** |
| 3 | 若缺 Chest 热区：补装 Collider + Kind=Chest | **P0** |
| 4 | 验收点胸播对白；无 Missing Prefab | **P0** |
| 5 | Prefab 图/表情缺口（若 CSV 未 Bind） | P1 |
| 6 | Chest 悬停 Catch 光标 | P2（默认不做） |
| 7 | C6+ 树屋 | ❌ 不做 |

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Idle 点 Chest | 播 **Village_ShopChest**；藏 `UI_Shop` |
| 2 | 对白中再点头/胸 | **不叠**第二段 |
| 3 | 对白结束 | 回 Idle；UI 显；热区开；合层脸复位 |
| 4 | Console | `story=Village_ShopChest started=true`；无 Missing Prefab / 旧名 ChestClick |
| 5 | 再点一次 Chest | **可再播**（非只播一次） |
| 6 | 点 Head | 仍播 ShopHead（回归） |

### H. 开放问题
- Chest 要否挂 Catch 悬停光标（Head 已有）？  
- Prefab 若未 Bind 点胸 CSV，先 Bind 还是先改常量验收壳？  
- 旧名 `Village_ShopKeeper_ChestClick` 文档作废声明写哪？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_Shop_Chest热区安装Village_ShopChest对话_架构溯源报告.md`

MASTER 四段式：  
① 结论（命名方案 + 热区是否齐 + 最小挂点）  
② 原因（旧门牌 vs 新 Prefab 名；点头先例）  
③ 用户检查清单（怎么点胸验收）  
④ 给程序：链路表 + Prefab 表 + 最小 diff + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_Shop_Chest热区安装Village_ShopChest对话_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/ShopkeeperBodyHotspot.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Editor/Tool/Dialogue/ShopkeeperSpecialClickSetupEditor.cs
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【施工员】。按报告把 Village_ShopChest 接到商人 Chest 热区。

必须遵守：
- 故事名与 Prefab 文件名一致（推荐常量改为 Village_ShopChest）；对齐点头方案 A；
- 经 GSM TryTriggerShopkeeperSpecial，禁止 Hotspot 直开 TriggerStory；
- 本期不做 C6+ 树屋；不做存档只播一次；默认不做 Chest Catch 光标（除非报告要求）；
- 同步 Editor Setup 路径/注释，避免再生成旧名；
- 代码含详细注释；重要取舍写清原因。

提交说明：改了哪些常量/热区/路径、如何验收点胸、未做项。
```
