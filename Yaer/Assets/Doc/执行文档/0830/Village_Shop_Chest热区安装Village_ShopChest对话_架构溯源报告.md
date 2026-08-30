# Village_Shop — Chest 热区安装 `Village_ShopChest` 对话 — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 接线拍板（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**场景**：`Village_Shop.unity` · `商店界面合层` → `MerchantPainting` → `Trigger` → **`Chest`**  
**对白真源（用户指定）**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab`  
**台本 / 图**：`Assets/Dialog/Village_商店点胸交互.csv` · Generated `Village_商店点胸交互.asset`  
**对照先例**：0829 点头方案 A（常量 → `Village_ShopHead`）

关联：`ShopkeeperBodyHotspot` · `ShopkeeperChestClickStoryName` · `TryTriggerShopkeeperSpecial` · `ShopkeeperSpecialClickSetupEditor` · 0828 Trigger 特殊交互 · 0601 点胸台本

---

## ① 结论一句话

**热区 / 点击管线 / Hide-Show / 结束 Reset 已齐（与点头同门）；唯一 P0 缺口是门牌：常量仍为不存在的 `Village_ShopKeeper_ChestClick`，磁盘真源是 `Village_ShopChest`——点胸必加载失败。拍板方案 A（对齐 Head）：`ShopkeeperChestClickStoryName = "Village_ShopChest"`，并同步 Hotspot 注释与 Editor Setup 路径。Prefab 已 Bind 点胸 Generated 图（C1～C5、无 C6）；本期不做树屋、默认不做 Chest Catch 光标。**

---

## ② 原因（通俗）

### 2.1 门铃装好了，管家喊错门牌（同点头）

胸上的感应区（`Trigger/Chest`）和管家流程（`TryTriggerShopkeeperSpecial`）都已装好；管家仍按旧门牌喊 **`Village_ShopKeeper_ChestClick` 房间**，真正住着对白的房间门牌是 **`Village_ShopChest`**。

| 层 | 磁盘核实（2026-08-30） | 白话 |
|----|------------------------|------|
| 常量 `ShopkeeperChestClickStoryName` | `"Village_ShopKeeper_ChestClick"` | 旧门牌 |
| Prefab 文件 | ✅ 仅有 `Village_ShopChest.prefab` | 新房间 |
| `…_ChestClick.prefab` | ❌ **不存在**（全库 0 命中） | 旧房间空号 |
| `DialoguePath.GetPath(name)` | `Assets/GameRes/Prefabs/Dialogue/{name}.prefab` | **名字必须 = 文件名** |

Idle 点 Chest → Console 多半 `story=Village_ShopKeeper_ChestClick started=…`，资源路径指向空号 → **无对白 / Missing Prefab**。不是「还没做点胸系统」。

### 2.2 点头先例已证明改常量即可

0829 Head：旧名 `…_HeadClick` → 方案 A 改成 `Village_ShopHead` 后加载通。  
Chest **同一撕裂形态** → **同一解法 A**；不要 Rename Prefab 回旧名，不要加别名映射。

### 2.3 Prefab / 图：店内段已齐，可直接 Trigger

`Village_ShopChest` 根名对齐；子物体 **Yaer + Merchant**；对话图走外部 `_graph` → `Village_商店点胸交互.asset`（guid `57909249…` 对得上），文案/表情与点胸 CSV **C1～C5** 一致，**无** LoadScene / 树屋 Action（C6+ 未进图）。改常量后即可播店内闭环。

---

## ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Idle 点 Chest | 播 **Village_ShopChest**；藏 `UI_Shop`；店/雅表情跟句变 |
| 2 | 对白中再点头/胸 | **不叠**第二段 |
| 3 | 对白结束 | 回 Idle；UI 显；热区开；合层脸复位（Normal+Face1） |
| 4 | Console | `story=Village_ShopChest started=true`；**无** Missing Prefab / 旧名 ChestClick |
| 5 | 再点一次 Chest | **可再播**（非只播一次） |
| 6 | 点 Head | 仍播 ShopHead（回归） |
| 7 | （边界）播完停在店内 | **不**黑屏切树屋（C6 本期不做） |

---

## ④ 给程序

### A. Prefab / 图完备性（`Village_ShopChest`）

| 检查 | 结果 |
|------|------|
| 根 `m_Name` | ✅ `Village_ShopChest` |
| 绑定方式 | `_boundGraphSerialization` 空；`_graph` → Generated `Village_商店点胸交互.asset` ✅ |
| Actor GO | ✅ **Yaer** + **Merchant**；Yaer 下嵌 `GoOutStoryYaerPainting`（alpha0）+ BB 已绑 |
| 句数 | **5** Statement（= CSV 行 1～5 = 0601 **C1～C5**） |
| Action / C6 | ❌ **0** ActionNode、无 LoadScene → 树屋未进图 ✅ 符合本期 |
| 店句 Portrait | ✅ `UseShopkeeperPortrait=true`（店句） |
| 表情（Generated） | C1 ShopFace=3（Face4）Normal；C2 Face=1（Face2）；C3 Face=1 Body=1（Red）；C4 雅 FaceType=6（Laugh）；C5 Face=1 Body=1（Red）——对齐 **CSV**（非 0601 旧 Smile/Surprised 字面） |
| 旧名 Prefab | ❌ 无 `Village_ShopKeeper_ChestClick.prefab` |

**可播性**：改故事名对齐后即可 `TriggerStory("Village_ShopChest")`；一般**不必**为加载再 Rebuild 图（图已 Bind）。若 Editor 误跑旧路径生成空 ChestClick，以 **ShopChest** 为准、删错文件。

### B. 点击 → Trigger 链路（现网）

```
Trigger/Chest
  BoxCollider2D (isTrigger, size 3×2.5, local 13.5/6.2)
  ShopkeeperBodyHotspot (hotspotKind=1 Chest)
    → OnPointerClick
      → TryTriggerShopkeeperSpecial(ShopkeeperChestClickStoryName)
           现网 = "Village_ShopKeeper_ChestClick"  ❌ 空号
           应改 = "Village_ShopChest"              ✅
      → StoryGSM.TriggerStory → DialoguePath.GetPath(name)
```

| 项 | 状态 |
|----|------|
| `Trigger/Chest` 存在 | ✅ 与 Head 同父 `Trigger` |
| Collider + Kind=Chest | ✅ |
| Main Camera `Physics2DRaycaster` | ✅（guid `56666c5a…`） |
| `SetShopkeeperHotspotsEnabled` 含 Chest | ✅（children 全开全关） |
| Hide UI / HasRunningStory / 结束 ResetDefault | ✅ 走 Special 同管线（与 Head/Yes/No） |
| Head 悬停 Catch | ✅ 仅 Head 挂 `CursorChangeTrigger`；**Chest 无**（本期默认不补） |

### C. 命名方案拍板

| 方案 | 裁定 | 理由 |
|------|------|------|
| **A · 常量改为 `Village_ShopChest`** | ✅ **拍板** | 用户指定 Prefab；对齐 0829 Head；少动资源 |
| B · Prefab 改名回 ChestClick | ❌ | 用户已定名；易丢 meta |
| C · 别名映射 | ❌ | 魔法字符串易再漂 |

**须同步**

| 文件 | 改什么 |
|------|--------|
| `Village_ShopSceneManager.cs` | `ShopkeeperChestClickStoryName = "Village_ShopChest"` + 注释（仿 Head 0829） |
| `ShopkeeperBodyHotspot.cs` | Chest 枚举注释旧名 → ShopChest |
| `ShopkeeperSpecialClickSetupEditor.cs` | `ChestPrefabPath` → `…/Village_ShopChest.prefab`；去掉「点胸线本期不动」过时注释 |

0601「建议 prefab 名 ChestClick」→ **作废**（产品真源 ShopChest）。

### D. 状态机与互斥

| 场景 | 行为 |
|------|------|
| Idle 点 Chest | Special → ShopChest；可重复（**不** CheckStoryUsed） |
| 对白中 | 热区 OFF + HasRunningStory → 再点头/胸忽略 |
| ShopStart / Repeat 窗口 | 现网 Special 已拒或 HasRunningStory 挡 |
| Yes/No | 同门 Special；故事名不同，勿混 |
| ESC | 对白中忽略离店（现网） |
| C6+ 树屋 | ❌ 本期不做；图内亦无节点 |

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 常量 → `"Village_ShopChest"` | **P0** |
| 2 | Hotspot / Editor 注释与 Prefab 路径同步 | **P0** |
| 3 | 热区补装 | ❌ **已齐**，一般不动场景 |
| 4 | 验收点胸播对白；Console 故事名=ShopChest | **P0** |
| 5 | Prefab 再 Import CSV | P1（现网已对齐，仅图坏时） |
| 6 | Chest 悬停 Catch 光标 | **P2 默认不做** |
| 7 | C6+ 树屋 | ❌ |

**预期 diff**

- `Village_ShopSceneManager.cs`  
- `ShopkeeperBodyHotspot.cs`  
- `ShopkeeperSpecialClickSetupEditor.cs`  
- **一般不改** Prefab / 场景 / CSV  

### F. 验收清单

同 §③。

### G. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | Chest 要否 Catch 悬停光标（Head 已有）？ | **本期否**（P2） | ✅ |
| Q2 | Prefab 未 Bind 时先 Bind 还是先改常量？ | 现网**已 Bind** → **先改常量验收** | ✅ |
| Q3 | 旧名 ChestClick 文档作废写哪？ | **本报告 + OPEN_QUESTIONS**；0601 建议名过时 | ✅ |
| Q4 | Setup 菜单是否加「仅 Rebuild Chest」？ | 可选 P1；改路径后全量 Setup 亦可 | ⏳ |

（已追加 `OPEN_QUESTIONS.md`。）
