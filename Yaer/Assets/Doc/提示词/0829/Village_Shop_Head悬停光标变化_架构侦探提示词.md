# Cursor Agent Prompt · Village_Shop：Head 热区悬停光标变化 — 现网光标体系溯源

> **角色**：先【架构侦探】只读溯源；**本阶段不施工、不替用户选定光标样式**  
> **日期**：2026-08-29  
> **场景 / 交互点**：`Village_Shop` · `商店界面合层` → `MerchantPainting` → `Trigger` → **`Head`**  
> **产品目标（白话）**：玩家鼠标 **进入 Head 区域** 时，光标要 **换成另一种样子**；离开后恢复  
> **侦探额外职责**：查清项目 **现有光标变化怎么实现**、**一共有几种可切换样式**，在报告里做成 **「用户选项菜单」**，等用户点名后再开施工  
> **关联**：点头对白 `Village_ShopHead` / Head 热区安装（可并行，但本期只查光标，不改对白名）  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / 贴图  
> **报告落盘**：`Assets/Doc/执行文档/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

> 鼠标 **进入** Head → 光标变化；**离开** Head → 光标恢复。  
> **选哪种样子由用户看完报告后再定**；侦探只列清单 + 推荐挂接方式，**禁止擅自拍板「用 Chat」之类并写进施工必做**。

| 项 | 期望 |
|----|------|
| 触发区域 | `Trigger/Head`（与现有点击热区同一碰撞体优先） |
| 时机 | **悬停进入/离开**（不是点击才变） |
| 样式 | **待用户从现网已有样式中选择**（或声明要新贴图） |
| 对白中 | 热区若关闭，光标是否应强制回 Normal？（侦探给建议，写入开放问题） |
| 本期 | 只溯源 + 选项表；**不施工** |

### 现网光标体系假说（2026-08-29 预扫 · 须证伪）

工程里已有整套 GM 光标组件（勿另起 `Cursor.SetCursor` 野路子）：

| 层 | 路径 / 类型 | 预扫含义 |
|----|-------------|----------|
| 中枢 | `CursorComponentGM` | 队列 + 优先级；`UnityEngine.Cursor.SetCursor` |
| 状态枚举 | `CursorState` | **`Normal` / `Catch` / `View` / `Chat`**（至少这 4 种） |
| 世界空间入口 | `CursorChangeTrigger` | 要求 `Collider2D`；`Update` 里 `OverlapPoint` 进/出 |
| UI 入口 | `CursorChangeUI` | 挂在 `BaseUIFormLogic`；OnEnable/OnDisable 切状态 |
| 参数 | `CursorChangeArgs` | `TargetState` + `Guid` + `Priority` |
| 贴图配置 | `InitScene` 上 `CursorComponentGM` 序列化 | Normal / Catch 张合 / View 眨眼 / Chat 气泡序列帧 |
| 美术目录 | `Assets/ArtRes/Cursor/` | `光标.png`、`手张`/`手握`、`眼…`、`对话气泡1～4` 等 |

**白话**：游戏已经有「普通 / 抓取手 / 观察眼 / 对话气泡」四套光标；Head 悬停应 **复用这套**，不要再写一套 SetCursor。

### Head 现状与挂接假说（须裁定）

`Head` 已有（0828 施工 / 场景 YAML 假说）：

- `BoxCollider2D` + `ShopkeeperBodyHotspot`（`IPointerClickHandler`）
- 主相机 `Physics2DRaycaster`（点击用 EventSystem）

悬停光标候选挂法：

| 方案 | 做法 | 优点 | 风险 | 助手倾向 |
|------|------|------|------|----------|
| **A · 同物体加 `CursorChangeTrigger`** | Head 再挂 Trigger，TargetState=用户所选 | 零新脚本；与村内 NPC/可互动物一致 | Trigger 用 **Update+OverlapPoint**，与 Pointer 射线两套进/出；对白关 Collider 时能否正确 Exit？ | **优先核实样例后推荐** |
| **B · 扩展 `ShopkeeperBodyHotspot`** | 加 `IPointerEnter/Exit` → 调 `CursorComponentGM` | 与现有点击同一条 EventSystem 链 | 改共享组件；须处理 Priority/Guid | 若 A 在纯 UI 店场景不可靠则用 B |
| **C · 新建细组件** | `ShopkeeperHotspotCursor` 只管光标 | 解耦 | 多一类文件 | A/B 不够时 |
| **D · 直接 `Cursor.SetCursor`** | ❌ | — | 绕过队列/优先级，易卡死光标 | **禁止** |

侦探必须：**找 1～2 个现网 `CursorChangeTrigger` 使用样例**（哪个场景、TargetState 填什么），再裁定 Head 用 A 还是 B。

### 「用户选项菜单」要求（报告必出 · 本阶段核心交付）

用表列出 **每一种现网可切换光标**，方便用户回复「选 X」：

| 选项 ID | CursorState | 视觉（贴图/动画白话） | 现网典型用途（样例物体） | 适合点头？ |
|---------|-------------|----------------------|--------------------------|------------|
| 1 | Normal | … | 默认 | 基线，非悬停目标 |
| 2 | Catch | 手张/手握… | ? | ? |
| 3 | View | 眼睛闪… | ? | ? |
| 4 | Chat | 对话气泡序列帧… | ? | ? |
| … | （若还有扩展/未绑贴图） | | | |

并写清：

1. **InitScene 上实际绑了哪些贴图**（guid→文件名），避免「枚举有、图空」。  
2. 若某状态 **无贴图 / 动画坏了**，标 ❌ 不可选。  
3. **推荐倾向**可写一句（例如点头对白语义更像 Chat），但必须标注「**仅建议，等用户确认**」。  
4. 若用户要第五种样子：说明是「新贴图 + 扩枚举」还是「复用某态换皮」，并记入开放问题（本期不施工）。

### 与点击 / 对白共存（必查）

```
Idle：进 Head → 光标变（用户所选）；出 Head → 回 Normal（或队列下一优先级）
对白中：热区 Collider / Hotspot 关闭时 → 必须能 Exit，光标不能卡在 Chat/View
点 Head 开对白瞬间：若仍在 Collider 内，进/出时序会否抖一下？
UI_Shop / 全屏 UI：CursorChangeUI 高 Priority 是否盖过 Head？
```

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 溯源光标中枢 + 枚举 + 两种入口组件 | ❌ 施工挂组件 / 改枚举 |
| ✅ 列出可选样式菜单（含贴图对照） | ❌ **替用户选定**最终样式 |
| ✅ 裁定 Head 挂接方案 A/B/C | ❌ 改点头对白名 / Village_ShopHead 接线（另单） |
| ✅ 最小施工清单（等用户选样式后） | ❌ 新建整套光标系统 |
| ✅ Chest 是否同样挂：只写「可复用，本期可选」 | ❌ 强制做 Chest |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / 贴图  
- 报告写「已选定 Chat，施工照做」而未等用户回复  
- 绕过 `CursorComponentGM` 直接 SetCursor 的「临时方案」当推荐主路径  
- 在 `ShopkeeperBodyHotspot.Update` 里轮询切光标（若选 B，应用 PointerEnter/Exit 或明确为何不能）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md
@Assets/Doc/提示词/0829/Village_Shop_Head热区安装Village_ShopHead对话_架构侦探提示词.md
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorComponentGM.cs
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorChangeTrigger.cs
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorChangeUI.cs
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorChangeArgs.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/ShopkeeperBodyHotspot.cs
@Assets/GameRes/Scenes/InitScene.unity
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/ArtRes/Cursor/

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、贴图。只读扫描 + 写「Head 悬停光标」溯源报告。

---

## 背景（策划白话）

1. 商店里老板娘 **Head** 热区：鼠标 **移上去** 要换光标，移开恢复。  
2. 项目里好像已有光标系统；先查 **怎么实现、有几种样子**。  
3. 报告里把可选样子列成菜单，**等用户选中后再施工**；本阶段不要改工程。

---

## 侦探任务清单

### A. 钉死现网光标架构（中枢）

| 项 | 填 |
|----|-----|
| 谁真正 `Cursor.SetCursor`？ | |
| 状态如何排队 / 比优先级？ | |
| 进/出 API | `OnEnterChangeTrigger` / `OnExitChangeTrigger`？ |
| 按下鼠标是否改外观？ | Catch Hold 等 |
| 动画态 | View / Chat 协程如何停净？ |

画一条白话链路：

```
区域进 → CursorChangeXxx → CursorComponentGM 入队
     → 按 Priority 取当前 CursorState → SetCursor(对应贴图)
区域出 → 出队 → 刷新为下一态或 Normal
```

### B. 钉死「有几种可变化」（用户选项菜单 · 必出）

1. 枚举 `CursorState` 全量列出。  
2. 对照 `InitScene` 里 `CursorComponentGM` 序列化字段 ↔ `ArtRes/Cursor` 文件名。  
3. 每种给：**静态图 or 动画**、**按下是否变形**、**现网至少 1 个使用样例**（搜 `CursorChangeTrigger` / `CursorChangeUI` 的 TargetState）。  
4. 输出选项表（选项 ID + 状态名 + 视觉描述 + 样例 + 是否建议用于「可对话热区」）。  
5. **明确写**：最终样式 = 用户回复选项 ID；侦探推荐仅作括号备注。

### C. 钉死两种入口组件差异

| 组件 | 适用空间 | 进/出判定 | 默认 Priority | Head 能否直接用 |
|------|----------|-----------|---------------|-----------------|
| `CursorChangeTrigger` | | OverlapPoint？ | | |
| `CursorChangeUI` | | OnEnable？ | | |

Village_Shop 是纯 UI 场景 + 合层世界空间 SR：哪种与 Head（Collider2D + Physics2DRaycaster）更匹配？

### D. Head 挂接方案拍板（架构，不含样式）

在预梳理 A/B/C 中选推荐方案，并回答：

1. 是否与 `ShopkeeperBodyHotspot` 共挂同一 Head？  
2. 对白中 `SetShopkeeperHotspotsEnabled(false)` / 关 Collider 时，光标能否可靠恢复？  
3. 是否需要 `IPointerEnterHandler`（EventSystem）替代 Update OverlapPoint？原因？  
4. Priority 建议填多少（相对 UI `CursorChangeUI` 的 100）？  
5. Chest 是否预留同款（本期不做也要写一句复用方式）？

### E. 最小施工清单（样式栏留空 · 等用户选）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | Head 挂接光标（按 §D 方案） | TargetState = **【用户选定】** | P0（用户选定后） |
| 2 | 对白中 Exit / 禁热区时光标恢复冒烟 | | P0 |
| 3 | （可选）Chest 同款 | | P2 |
| 4 | 若要新样式 | 新贴图 + 否扩枚举 | 仅当用户选「新样式」 |

**排除**：重做光标中枢；绕过 GM 直接 SetCursor；本期改对白 Prefab 名。

### F. 验收清单（施工后用；本阶段只设计）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Idle 鼠标移入 Head | 光标变为用户所选样式 |
| 2 | 移出 Head | 恢复 Normal（或正确下层状态） |
| 3 | 在 Head 上点开对白 | 对白中光标不卡死；结束后正常 |
| 4 | 对白中移出/热区关闭 | 无「永远气泡/永远手」 |
| 5 | 打开高优先级 UI 再关 | 光标队列不乱 |

### G. 开放问题

- 用户选哪一档 CursorState？（**等回复**）  
- 是否要全新光标图（第五种）？  
- 点胸是否同期挂同一光标？  
- 纯 UI 店场景用 OverlapPoint 是否依赖 MainCamera Tag？

---

## 输出要求

写入：`Assets/Doc/执行文档/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（现网几套样式 + 推荐挂接方案 A/B/C + **样式待用户选**）  
② 原因（通俗：谁管光标、Trigger vs UI、为何不要野 SetCursor）  
③ **用户检查 / 选择清单**（必须含「选项 1/2/3/4… 回复选哪个」）  
④ 给程序：架构表 + 样例物体 + 最小 diff（TargetState 留占位）+ 开放问题

口头汇报同样用 MASTER 四段式；**选项菜单要让非程序也能看懂每种光标长什么样**。
```

---

## 用户选定样式后 · 施工员续跑（侦探报告 + 用户选项确认后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/Scripts/Game/GameMgr/Component/Cursor/
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/ShopkeeperBodyHotspot.cs

你现在是【施工员】。用户已选定光标样式：【在此填写选项 ID / CursorState】。
只按报告 §D 挂接方案 + 用户选定的 TargetState，实现 Head 悬停换光标。

必须遵守：
- 复用 CursorComponentGM 队列，禁止业务处直接 Cursor.SetCursor；
- 对白关热区时必须能恢复光标；
- 不改光标中枢逻辑除非报告写明必要的最小补丁；
- 禁止 Update 堆新业务（若用现成 CursorChangeTrigger 的 Update 则复用，不新写第三套轮询）；
- 代码含详细注释；重要取舍写清原因。

提交说明：挂在哪个物体、TargetState/Priority、如何验收进/出/对白中。
```
