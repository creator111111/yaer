# Village_Shop — Head 热区安装 `Village_ShopHead` 对话 — 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_Shop.unity` · `商店界面合层` → ` MerchantPainting` → `Trigger` → **`Head`**  
**对白真源（产品指定）**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab`  
**台本文案真源**：`0601/Village_商店老板娘特殊交互_对白台本_执行说明.md` · CSV 迁移样：`Assets/Dialog/Village_商店点头交互.csv`

关联提示词：`Assets/Doc/提示词/0829/Village_Shop_Head热区安装Village_ShopHead对话_架构侦探提示词.md`  
关联报告：`0828/…Trigger特殊交互对话` · `0828/…商人默认Face1Normal` · `0828/…施工说明` · `0827/Village_ShopStart`

---

## ① 结论一句话

**拍板命名方案 A：把故事常量改成 `Village_ShopHead`（与 Prefab 文件名对齐）；热区 / GSM Hide-Show / 结束 `ResetDefault` 已齐，点击架构不必重做；但现网 Prefab 图内容对齐的是旧 CSV「头_对白台本」而非 0601/`Village_商店点头交互.csv`——施工 P0 = 改常量 + 用点头 CSV 重 Import（修 H1 文案、H3→ActionNode、雅脸与 Face/Body），并同步 Editor 菜单路径；不要新建第二套热区，也不要并进 `Village_ShopStart`。**

---

## ② 原因（通俗）

### 2.1 门铃装好了，管家喊错门牌

生活类比：头上的感应贴纸（`Trigger/Head`）和管家流程（`TryTriggerShopkeeperSpecial`）都装好了；管家仍按旧门牌去喊 **`Village_ShopKeeper_HeadClick` 房间**，而真正住着对白的房间门牌是 **`Village_ShopHead`**。

| 层 | 磁盘核实（2026-08-29） | 白话 |
|----|------------------------|------|
| 常量 `ShopkeeperHeadClickStoryName` | `"Village_ShopKeeper_HeadClick"` | 旧门牌 |
| Prefab 文件 | ✅ 仅有 `Village_ShopHead.prefab` | 新房间 |
| `…_HeadClick.prefab` | ❌ **不存在** | 旧房间空号 |
| `DialoguePath.GetPath(name)` | `Assets/GameRes/Prefabs/Dialogue/{name}.prefab` | **名字必须 = 文件名（无扩展名）** |

因此：Idle 点 Head → 日志会出 `story=Village_ShopKeeper_HeadClick started=true`，但资源加载路径指向**不存在的 Prefab** → 对白起不来 / `OnStoryPrefabLoad` 空引用告警。

### 2.2 热区与状态机：0828 假说「未落盘」已过时

0828 溯源时 Trigger 尚未存盘；**现网场景已齐**（与施工说明一致）：

| 项 | 结论 |
|----|------|
| `Trigger/Head` | ✅ `BoxCollider2D`（isTrigger，size≈2.2×2）+ `ShopkeeperBodyHotspot`（`hotspotKind=0` Head） |
| `Trigger/Chest` | ✅ 同构（`hotspotKind=1`）；**本期不扩施工** |
| Main Camera | ✅ 已挂 `Physics2DRaycaster`（guid `56666c5a…` = uGUI Physics2DRaycaster） |
| 点击入口 | ✅ `OnPointerClick` → `TryTriggerShopkeeperSpecial(storyName)` |
| 对白中互斥 | ✅ `HasRunningStory` + 热区 OFF + Hide `UI_Shop` |
| 结束回调 | ✅ `OnShopkeeperSpecialStoryEnd` → **`ResetShopkeeperPortraitDefault()`** + Show UI + 热区 ON（0828 Face 复位 **已施工，勿重复造**） |
| `MerchantPainting.prefab` | ❌ 仍无 Trigger（仅场景实例有）→ 开放问题 Q4，P2 |

### 2.3 `Village_ShopHead`：壳可装，图未对齐产品点头台本

| 检查 | 结论 |
|------|------|
| 根名 / 可被 `GetPath("Village_ShopHead")` 加载 | ✅ `m_Name: Village_ShopHead` |
| Actor：Yaer + Merchant | ✅；**无 Gusha GO**；**无 Narrator GO** |
| 店句 `UseShopkeeperPortrait` | ✅ 店 Statement 已勾 |
| 图内容 vs `Village_商店点头交互.csv` | ❌ **不一致**（见 §C） |
| 图内容 vs 旧 `Village_商店老板娘特殊交互_头_对白台本.csv` | ✅ **高度吻合**（含旧 H1、H9/H10 YinXian、H3 当雅 Say） |
| H3 是否 ActionNode | ❌ 现为雅尔 `StatementNodeEx`（旁白动作句误当角色说） |
| Mask / 合层桥 | ✅ 无需新桥；跟句走现网 `ApplyShopkeeperPortrait` 双轨 |

**可装即用？**  
- **加载壳**：改名对齐后即可加载。  
- **产品点头线（0601 H1～H11 + 迁移表情）**：**不可直接当终稿**——须用 `Village_商店点头交互.csv` 再 Import（或等价手改图）。

### 2.4 命名方案裁定

| 方案 | 裁定 | 理由 |
|------|------|------|
| **A · 常量/调用改 `Village_ShopHead`** | **✅ 拍板** | 产品指定 Prefab；磁盘真源已是 ShopHead；少动图资源引用 |
| B · Prefab 改名回 HeadClick | ❌ | 用户已定名；Editor 旧路径才是错的 |
| C · 别名映射 | ❌ | 多一层魔法字符串，易再漂 |

Editor `ShopkeeperSpecialClickSetupEditor` 仍写 `Village_ShopKeeper_HeadClick.prefab`——施工须同步改成 `Village_ShopHead`，避免再生成空/错路径覆盖真源。

---

## ③ 用户检查清单

| # | 操作 | 通过标准 |
|---|------|----------|
| 1 | Hierarchy：`商店界面合层` → ` MerchantPainting` → `Trigger` → `Head` | 有 Collider2D + `ShopkeeperBodyHotspot`（Head） |
| 2 | Main Camera | 有 `Physics2DRaycaster` |
| 3 | 施工前 Play：Idle 点 Head | Console：`story=Village_ShopKeeper_HeadClick`；**对白通常失败**（缺 Prefab） |
| 4 | 施工后 Play：Idle 点 Head | `[ShopHotspot] … story=Village_ShopHead started=true` + `[ShopSpecial] TriggerStory Village_ShopHead` |
| 5 | 对白过程 | 首句应为「**身为公主怎么会出门不带钱呢？**」；店合层+Mask 随句变；雅脸变；`UI_Shop` 隐藏 |
| 6 | H3 | 桌面放钱为**旁白/动作**，不是雅尔嘴说 |
| 7 | 对白中再点 Head/Chest | 不开第二段 |
| 8 | 对白结束 | UI 恢复；Idle 脸身 = Face1 + Normal（已有 ResetDefault） |
| 9 | Console | 无 Missing Prefab / Missing Actor / Face 校验失败 / NRE |
| 10 | （方案 A） | **不要求**存在 `Village_ShopKeeper_HeadClick.prefab` |

---

## ④ 给程序

### A. Head 热区现状表

| 项 | 值 |
|----|-----|
| 路径 | `商店界面合层` / ` MerchantPainting` / `Trigger` / `Head` |
| Collider | `BoxCollider2D` · isTrigger=1 · size=(2.2, 2) · localPos≈(13.23, 8.4) |
| 脚本 | `ShopkeeperBodyHotspot` · `hotspotKind: 0`（Head） |
| 点击后 story 名（现网） | `Village_ShopSceneManager.ShopkeeperHeadClickStoryName` = **`Village_ShopKeeper_HeadClick`** |
| Physics2DRaycaster | ✅ 挂在 Main Camera（`fileID` 与场景 `794234387` 同 GO） |
| Idle | 非首次进店且无 RunningStory → 热区 ON |
| 对白中 / 首次进店 | 热区 OFF + Hide UI；`TryTrigger` 拒绝叠开 |

### B. 故事名 ↔ Prefab 加载链路（P0）

```
点 Head
  → ShopkeeperBodyHotspot.OnPointerClick
  → storyName = ShopkeeperHeadClickStoryName   // 现网旧名
  → Village_ShopSceneManager.TryTriggerShopkeeperSpecial(storyName)
       → HideShopUiRoot + SetShopkeeperHotspotsEnabled(false)
       → StoryComponentGSM.TriggerStory(storyName)
            → ResMgr.LoadAsset(DialoguePath.GetPath(storyName))
                 = Assets/GameRes/Prefabs/Dialogue/{storyName}.prefab
```

| 检查 | 结论 |
|------|------|
| `GetPath` 是否严格 = 文件名？ | **是** |
| 现网常量 | `Village_ShopKeeper_HeadClick` |
| 磁盘 Prefab | 仅 `Village_ShopHead.prefab` |
| `TriggerStory` 返回值 | 启动加载即 `true`；**不代表 Prefab 已找到**（缺资源时 `OnStoryPrefabLoad` 收 null） |

**方案 A 预期 diff（最小）**

| 文件 | 改动 |
|------|------|
| `Village_ShopSceneManager.cs` | `ShopkeeperHeadClickStoryName = "Village_ShopHead"` |
| `ShopkeeperBodyHotspot.cs` | 注释里旧名 → `Village_ShopHead` |
| `ShopkeeperSpecialClickSetupEditor.cs` | `HeadPrefabPath` → `…/Village_ShopHead.prefab`；注释同步 |
| `Village_ShopHead.prefab` | **用 `Village_商店点头交互.csv` 重 Import**（见 §C；勿再指向 HeadClick 路径） |
| （可选）Generated | `Assets/GameRes/DialogueTrees/Generated/Village_商店点头交互.asset` 旁路落盘 |

**Chest 共享常量**：`ShopkeeperChestClickStoryName` / `…_ChestClick.prefab` **本期不动**；改 Editor 时只改 Head 路径，勿误伤 Chest。

### C. `Village_ShopHead.prefab` 完备表

| 检查 | 现网 | 期望（产品） | 缺口 |
|------|------|--------------|------|
| 根名 | `Village_ShopHead` | 同左 | 无 |
| Yaer / Merchant | ✅ | ✅ | 无 |
| Gusha GO | 无 | 无 | 无（BB 残留 GushaPainting 变量无害） |
| Narrator | ❌ 无 | H3 旁白需要 | **补 Actor 或 Import 时 EnsureNarrator** |
| 首句文案 | 「说起来，看你的穿着…没带钱」 | 「**身为公主怎么会出门不带钱呢？**」（0601 改稿 / 点头 CSV） | **P0 错文案** |
| H3 | 雅尔 Statement（旧台本把动作当雅说） | **ActionNode / 旁白** | **P0** |
| 店 Face/Body | 接近旧头 CSV（H9/H10 **YinXian**） | 点头 CSV：H1 Face2；H10 **Face5+Red** 等 | **P0 以点头 CSV 为准重 Import** |
| 雅 Face | 旧键 Smug/ChiBie/Angry 的 int | Daze / VerySurprised / Unhappy / Surprised | Import 后自动对齐 |
| 前奏 | FightingPanelVisible + 对话框 Alpha 淡入 | 特殊线可保留淡入；**无** ShopStart 黑幕闸门 | 可接受 |
| 与 `Village_商店点头交互.csv` | ❌ 未对齐 | ✅ | 施工 Import |
| 与旧 `…头_对白台本.csv` | ✅ 对齐 | 仅考古 | 勿再当真源 |

**文案/表情真源优先级（施工默认）**

1. **0601 台本**（H1 改稿、H3 动作、分行）  
2. **`Village_商店点头交互.csv`**（已按 0828 §D 迁移 Face1～5 + Body）  
3. Prefab 现图 / 旧「头_对白台本.csv」→ **覆盖丢弃**（除非策划书面改回）

若 Prefab 与 CSV 冲突：**以 CSV 再 Import 覆盖 Prefab 图**（开放问题 Q2 默认如此）。

### D. 运行时闭环（状态机 · 引用 0828，已核实）

```
Idle（UI_Shop ON，热区 ON）
  → 点 Head → TryTriggerShopkeeperSpecial("Village_ShopHead")  // 施工后
       → Hide UI、热区 OFF、HasRunningStory
  → 播 Village_ShopHead（店脸/雅脸跟句）
  → onStoryEnd → ResetDefault + Show UI + 热区 ON
```

| 问 | 答 |
|----|-----|
| 结束回调是否已接？ | **已接**；缺的主因是故事名，不是状态机 |
| 是否还要写 ResetDefault？ | **否**（`OnShopkeeperSpecialStoryEnd` 已调）；勿重复施工 |
| 首次进店窗口点头？ | `ShouldPlayShopStartStory()` / 热区 OFF 挡 |

### E. 最小施工清单（本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | 故事名 | `ShopkeeperHeadClickStoryName` → `"Village_ShopHead"`；Hotspot 注释同步 | **P0** |
| 2 | Prefab 图 | 对 `Village_ShopHead`：Ensure Narrator → Import `Village_商店点头交互.csv`（清旧图；H3 Action；表情跟 CSV） | **P0** |
| 3 | Editor Setup | `HeadPrefabPath` / 日志文案改 ShopHead；**Chest 路径勿动** | **P1** |
| 4 | ResetDefault | 已有 → **跳过** | — |
| 5 | 文档旧名 | 0828 施工说明/注释中 HeadClick → 注明已更名 ShopHead | P2 |
| 6 | 点胸线 | **本期排除** | — |
| 7 | Trigger 写回 Prefab | 见 Q4；非点头闭环阻塞 | P2 |

**排除**：新建第二套热区；并进 ShopStart；扩 `DialogueFaceType`；重做首次进店；方案 C 别名。

**推荐施工顺序**

1. 改常量（立刻能加载到现 Prefab，哪怕图仍旧——便于先验点击链路）。  
2. 改 Editor 路径后跑一次「仅 Import 进 ShopHead」或手跑 CSV→图（验收文案/表情）。  
3. Play 按 §F 验收。

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进店 Idle 点 Head | `TriggerStory Village_ShopHead`；`started=true`；对白真正开始 |
| 2 | 听/看首句 | 「身为公主怎么会出门不带钱呢？」；店 Face2+Normal |
| 3 | H3 | 旁白/动作，非雅说 |
| 4 | 对白过程 | 合层+Mask 随店句；雅脸随句；UI_Shop 隐藏 |
| 5 | 对白中再点 | 不开第二段 |
| 6 | 结束 | UI 恢复；可买卖；Idle Face1+Normal |
| 7 | Console | 无 Missing Prefab / Actor / Face 校验失败 / NRE |
| 8 | 方案 A | 不要求 `Village_ShopKeeper_HeadClick` 文件存在 |

### G. 开放问题

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 对外故事名最终以谁为准？ | **`Village_ShopHead`（方案 A）** | ✅ 本报告拍板 |
| Q2 | Prefab 图与 CSV 不一致时谁覆盖谁？ | **以 `Village_商店点头交互.csv` Import 覆盖 Prefab** | 待确认（默认如此） |
| Q3 | 点胸是否仍用 `Village_ShopKeeper_ChestClick`？ | **是；本期不施工、改共享工具勿误伤** | ✅ 保持 |
| Q4 | Trigger 是否写回 `MerchantPainting.prefab`？ | 场景已有即可验收；写回为 P2 | 待确认（沿用 0828 Q4） |
| Q5 | 旧 `…头_对白台本.csv` 是否归档/标注废弃？ | 建议标注「勿再 Import」，免与点头 CSV 双真源 | 待确认 |

（已追加至 `Assets/Doc/OPEN_QUESTIONS.md`。）

---

## 附录 · 关键代码锚点

| 主题 | 路径 |
|------|------|
| 热区点击 | `ShopkeeperBodyHotspot.cs` |
| 特殊对白 GSM | `Village_ShopSceneManager.TryTriggerShopkeeperSpecial` |
| 结束复位 | `OnShopkeeperSpecialStoryEnd` → `ShopkeeperFaceRegistry.ResetDefault` |
| 加载路径 | `DialoguePath.GetPath` · `StoryComponentGSM.TriggerStory` |
| Editor | `ShopkeeperSpecialClickSetupEditor.cs` |
| 点头 CSV | `Assets/Dialog/Village_商店点头交互.csv` |
| 旧头 CSV（考古） | `Assets/Dialog/Village_商店老板娘特殊交互_头_对白台本.csv` |
