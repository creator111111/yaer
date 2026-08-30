# Village_Shop — Head 热区悬停光标变化 — 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读溯源 + **用户选项菜单**（**本阶段未改代码 / Prefab / 场景 / 贴图；未替用户选定样式**）  
**Unity**：2020.3.48f1  
**交互点**：`Village_Shop` · `商店界面合层` → ` MerchantPainting` → `Trigger` → **`Head`**

关联提示词：`Assets/Doc/提示词/0829/Village_Shop_Head悬停光标变化_架构侦探提示词.md`  
关联：`0829/…Head热区安装Village_ShopHead`（对白名另单；本期只查光标）

---

## ① 结论一句话

**现网已有完整光标中枢 `CursorComponentGM`（队列+优先级），可切换样式正好 4 种：Normal / Catch / View / Chat，InitScene 四套贴图均已绑定、均可选；Head 推荐挂接方案 A——在同一 `Head` 上再挂现成 `CursorChangeTrigger`（与村内 NPC/石碑一致），TargetState 留空等你选；施工时须把 `CursorChangeTrigger` 一并纳入热区开关（否则对白关 Collider 时光标可能卡住）。样式请你回复选项 ID，侦探不代选。**

---

## ② 原因（通俗）

### 2.1 谁管光标？

游戏不用各处乱调 `Cursor.SetCursor`。统一走：

```
区域进 → CursorChangeTrigger / CursorChangeUI
      → CursorComponentGM.OnEnterChangeTrigger（入队 + Guid + Priority）
      → 按 Priority 排序，取队首 CursorState
      → SetCursor(对应贴图) / 开 View·Chat 协程动画
区域出 → OnExitChangeTrigger(Guid) 出队 → 刷新成下一态或 Normal
```

野路子直接 `Cursor.SetCursor`：**禁止**——会绕过队列，UI 开/关后容易「永远气泡 / 永远手」。

### 2.2 两种入口差在哪？

| 组件 | 适用 | 进/出怎么判 | 默认 Priority | Head 能否直接用 |
|------|------|-------------|---------------|-----------------|
| **`CursorChangeTrigger`** | 世界空间 + `Collider2D` | `Update` 里 `Camera.main` → `OverlapPoint` | **1** | ✅ 与 Head 已有 Collider 同构 |
| **`CursorChangeUI`** | 挂 `BaseUIFormLogic` 的 UI 表单 | `OnEnable` / `OnDisable` | **100** | ❌ Head 不是 UI Form |

商店是「纯 UI 买卖 + 世界空间合层 SR」：Head 热区是 **世界 Collider2D**，和村里 NPC 一样，应走 **Trigger**，不是 UI。

### 2.3 为何推荐方案 A（同挂 `CursorChangeTrigger`）

| 方案 | 裁定 | 理由 |
|------|------|------|
| **A · Head 加 `CursorChangeTrigger`** | **✅ 推荐（架构）** | 零新脚本；样例遍布 `Village_HomeScene23` NpcChair、`Village_KenMuNi1` StoneBrand、`Slime`/`Box`；商店已有 `MainCamera` Tag，`OverlapPoint` 可用 |
| B · 扩 `ShopkeeperBodyHotspot` 加 PointerEnter/Exit | 备选 | 与点击同 EventSystem 链；但要改共享组件 + 自管 Guid；仅当 A 验收失败再用 |
| C · 新建细组件 | 不优先 | 多一类文件，收益低 |
| D · 直接 SetCursor | ❌ 禁止 | 破坏队列 |

**对白关热区风险（A 必须补的一点）**  
现网 `SetShopkeeperHotspotsEnabled` 只关 **Collider2D + ShopkeeperBodyHotspot**，**不会**关 `CursorChangeTrigger`。Trigger 的 `OnDisable` 才保证 Exit；只关 Collider 时 Update 仍跑，`OverlapPoint` 可能仍判「在头上」→ **对白中光标卡在 Chat/View**。  
施工清单 P0：**把 `CursorChangeTrigger` 列入同开关**（或关热区时强制 `OnPointerExit`）。不要另起第三套 Update 轮询。

### 2.4 按下鼠标会变吗？

| 状态 | 按下左键 |
|------|----------|
| Normal / View / Chat | 外观不变（View/Chat 继续播自己的帧动画） |
| **Catch** | 按下 → `手握`；松开 → `手张` |

---

## ③ 用户检查 / 选择清单（请回复选项 ID）

### 3.1 先确认现网「长什么样」（Play 或看图）

| 选项 ID | 状态名 | 你看到的样子（白话） | 贴图（InitScene 已绑） | 现网样例 | 适合点头热区？ |
|---------|--------|----------------------|------------------------|----------|----------------|
| **1** | `Normal` | 普通箭头光标 | `光标.png` | 默认；多数 UI `CursorChangeUI` 强制回此态 | 基线，**不是**悬停目标 |
| **2** | `Catch` | 张开的手；**按住变握拳** | `手张.png` / `手握.png` | `Slime` Prefab；`Box` Prefab | 偏「抓取/可捡」，点头语义弱 |
| **3** | `View` | 眼睛；约 10s 睁眼再极短「眨一下」 | `眼 副本 29.png` / `眼 副本 31.png` | `Village_KenMuNi1` · `StoneBrand`（观察碑） | 偏「仔细看」，可用 |
| **4** | `Chat` | 对话气泡 1→2→3→4 循环闪（约 0.3s/帧） | `对话气泡1～4.png` | `Village_HomeScene23` · `NpcChair/Body` 等可对话 NPC | **语义最贴「点头开对白」**（仅建议） |

> **最终样式 = 你回复的选项 ID。**  
> 侦探倾向备注：若只论「点头会说话」，更像 **选项 4 Chat**；若想表达「打量老板娘脸」，可选 **3 View**。**等你确认后再施工，不写入必做样式。**

### 3.2 施工前你可自测（可选）

| # | 操作 | 期望 |
|---|------|------|
| 1 | 村里把鼠标移到可对话 NPC（如民居椅子） | 应变气泡光标（Chat） |
| 2 | 移到史莱姆/宝箱类 | 应变手（Catch） |
| 3 | 商店 Idle 移到 Head（现网） | **还不会变**（场景尚无 CursorChangeTrigger） |

### 3.3 请直接回复一句

```
选光标：选项 X（Normal/Catch/View/Chat）
点胸是否同期挂：是 / 否（默认否）
要不要第五种新图：否 / 要（说明样子）
```

---

## ④ 给程序

### A. 中枢架构表

| 项 | 值 |
|----|-----|
| 唯一 `SetCursor` 出口 | `CursorComponentGM.SetCursor` → `UnityEngine.Cursor.SetCursor` |
| 入队 | `OnEnterChangeTrigger(CursorChangeArgs)` |
| 出队 | `OnExitChangeTrigger(Guid)` |
| 当前态 | 队列按 Priority **降序** Sort 后取 `[0].TargetState`；空队列 = Normal |
| 枚举全量 | `Normal=0, Catch=1, View=2, Chat=3`（**无第五态**） |
| 动画 | View / Chat：`StartCoroutine`；切态前 `StopCoroutine` |
| Init 入口 | `OnEnter` 入队一条 Normal Priority=0 |

### B. InitScene 贴图对照（枚举有、图也有 → 均可选）

挂载：`InitScene` → `Cursor` GO → `CursorComponentGM`

| 字段 | guid → 文件 | 可选？ |
|------|-------------|--------|
| `Normal` | `b18a0d22…` → `光标.png` | ✅ |
| `CatchCursorNormal` | `2119785c…` → `手张.png` | ✅ |
| `CatchCursorHold` | `ace78d70…` → `手握.png` | ✅ |
| `ViewCursorNormal` | `d17076c8…` → `眼 副本 29.png` | ✅ |
| `ViewCursorHold` | `1c1b7d76…` → `眼 副本 31.png` | ✅ |
| `ChatCursors[0..3]` | `614d…`/`4a4b…`/`cea5…`/`dde3…` → `对话气泡1～4.png` | ✅ |
| 间隔 | Chat 0.3s；View 睁 10s / 眨 0.1s | — |

无「枚举有、图空」不可选项。

### C. 入口组件 + Head 挂接拍板

| 问 | 答 |
|----|-----|
| 与 `ShopkeeperBodyHotspot` 共挂同一 Head？ | **是**（Collider 共用） |
| 对白关 Collider 时能否可靠恢复？ | **现网不能只靠关 Collider**；须同步 disable `CursorChangeTrigger`（其 `OnDisable` 会 Exit） |
| 是否必须改用 `IPointerEnter`？ | **否**（优先 A）；商店 `Camera.main` 已 Tag；OverlapPoint 与村内一致 |
| Priority 建议 | **1**（对齐村内 Trigger；低于 UI 的 100） |
| 对白 UI 盖过 Head？ | `NormalDialogueNewPanel`：`CursorChangeUI` TargetState=Normal Priority=100 → 对白中强制普通光标，合理 |
| Chest | 同挂 Trigger、同一 TargetState 即可；**本期可选 P2** |

**点击开对白瞬间**：鼠标仍在 Head 内 → Trigger 若未 Exit，队列里仍有 Chat/View；对话 Panel Enable 后 Normal@100 盖过；结束后 Panel Disable 出队，若热区已重新 Enable 且鼠标仍在头上 → 应再次 Enter。验收看是否抖一下（开放问题）。

### D. 最小施工清单（TargetState = 用户选定后填）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | `Head` | 加 `CursorChangeTrigger`；`TargetState = 【用户选定】`；`Priority = 1` | P0（选定后） |
| 2 | `Village_ShopSceneManager.SetShopkeeperHotspotsEnabled` | **同步 enable/disable 子树内 `CursorChangeTrigger`**（保证对白 Exit） | **P0** |
| 3 | 验收 | 进/出 Head、点开对白、对白中、关 UI | P0 |
| 4 | （可选）Chest | 同款组件同 TargetState | P2 |
| 5 | 若用户要第五种 | 新贴图 + 扩 `CursorState` + InitScene 绑定 | 仅当用户明确要求 |

**排除**：重做中枢；业务处直接 SetCursor；本期改 `Village_ShopHead` 故事名；替用户写死 Chat。

**预期 diff（样式占位）**

```
Village_Shop.unity · Head
  + CursorChangeTrigger
      TargetState: <USER>   // Catch|View|Chat（勿用 Normal 当悬停目标）
      Priority: 1

Village_ShopSceneManager.SetShopkeeperHotspotsEnabled
  + 遍历 CursorChangeTrigger，enabled = enabled
    // 原因：仅关 Collider 不会触发 Trigger.OnDisable，光标可能卡态
```

### E. 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Idle 移入 Head | 光标 = 用户所选 |
| 2 | 移出 Head | 回 Normal（或队列下层） |
| 3 | Head 上点开对白 | 对白中不卡死；通常被对话 UI 拉回 Normal |
| 4 | 对白中 / 热区关闭 | 无「永远气泡/永远手」 |
| 5 | 开高优先级 UI 再关 | 队列不乱 |
| 6 | 对白结束鼠标仍在 Head | 应再次变为所选态（允许极短闪一下） |

### F. 开放问题

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 用户选哪一档 CursorState？ | **等回复选项 2/3/4**（勿代选） | ⏳ 等用户 |
| Q2 | 是否要第五种新光标图？ | **否**；复用四态 | 待确认 |
| Q3 | 点胸是否同期挂同一光标？ | **否**（P2 可复用） | 待确认 |
| Q4 | 对白关热区是否强制 Exit？ | **是**（开关 Trigger 组件） | ✅ 本报告建议拍板 |
| Q5 | OverlapPoint 是否依赖 MainCamera Tag？ | 商店已设 Tag；勿摘 Tag | ✅ 现网满足 |

（已追加 `OPEN_QUESTIONS.md`。）

---

## 附录 · 样例速查

| CursorState | 样例物体 | 路径提示 |
|-------------|---------|----------|
| Chat (3) | `NpcChair` → `Body` | `Village_HomeScene23.unity` |
| View (2) | `StoneBrand` | `Village_KenMuNi1.unity` |
| Catch (1) | `Slime` / `Box` | Prefab |
| Normal (0) | `NormalDialogueNewPanel` 等 UI | Priority 100 |

代码锚点：`Assets/Scripts/Game/GameMgr/Component/Cursor/`  
贴图：`Assets/ArtRes/Cursor/`
