# Village_KenMuNi1 — `House_Tree` 交互播 `Village_TreeHouseLock` — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 接线拍板（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**产品挂点（用户 Hierarchy）**：`Objects` → **`House_Tree`**  
**对白**：`Assets/GameRes/Prefabs/Dialogue/Village_TreeHouseLock.prefab`  
**产品目标**：点树屋（门口）→ 物体交互套路播锁对白「锁上了打不开」；**不**进屋、**不**接商店 Chest C6

关联：`SimpleStoryTrigger` · `InteractiveComponent` · `RaycastListener.requirePlayerOverlap` · GSM `sceneObjs` · 同村 `StoneBrand` · Item 面包远程点 · 代办「树屋门口的交互对话」

---

## ① 结论一句话

**磁盘场景 `Objects` 下目前没有 `House_Tree`（用户 Hierarchy 若有，多半未保存或仅编辑器态）——施工第一步须在 `Objects` 新建/落盘该物体，再挂物体交互三件套。推荐方案 A：仿同村 `StoneBrand` / Item 面包——`SceneEntity` + `InteractiveComponent` + Body/`RaycastListener` + `SimpleStoryTrigger(Click, StoryPrefabName=Village_TreeHouseLock)`，`requirePlayerOverlap=0`（远程可点），`SingleUseInArchive=false`（可重复），Layer 21，登记 `sceneObjs`。对白 Prefab 根名/文案可加载，但图内**只有 1 句 Statement、无 UIAlpha**——对照 StoneBrand/Npc1 与 ShopChest 教训，**P0 须补 `NormalDialogueUIAlpha`**，否则易「播了但看不见框」。禁止做成 `SceneChangeDoor` / 商店 Special。**

---

## ② 原因（通俗）

村里点石头牌子、点面包，都是「门铃三件套 + 故事名」——不是换景门，也不是商店老板娘特殊线。  
树屋现在要么还没挂这个门铃物体（磁盘上根本没有 `House_Tree`），要么只有美术合层里的树、没有可点的交互体——所以点了没反应。

对话稿已经写好（雅儿说锁上了），但稿子缺「对话框淡入」那一灯，和最近点胸「没框」同类风险，接线时要一起补。

---

## ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Hierarchy 确认 `Objects/House_Tree` **已保存进场景** | 磁盘 YAML 能搜到 `m_Name: House_Tree` |
| 2 | 村内**远处**点击树屋交互区（按拍板远程） | 播 **Village_TreeHouseLock** |
| 3 | 画面 | 雅尔「**锁上了打不开**」；**对话框可见** |
| 4 | 结束后再点 | **可再播**（未勾只播一次） |
| 5 | Console | Trigger / 加载 `Village_TreeHouseLock`；无 Missing Prefab |
| 6 | 回归 | `Door_Shop` 仍进店；`House_Npc*` 仍进屋；树屋 DepthZone 门控不坏 |
| 7 | 边界 | **不** LoadScene 进树屋内部；**不**走商店 Chest C6 |

**施工前你先核对**：Unity 里若已有 `House_Tree`，先 **Ctrl+S 存盘**，再让施工扫磁盘；若存盘后仍无，按报告新建。

---

## ④ 给程序

### A. 定位 `House_Tree`（磁盘真源）

| 项 | 结果 |
|----|------|
| `Village_KenMuNi1.unity` 全文 `House_Tree` | ❌ **0 命中** |
| Prefab 资产名含 House_Tree | ❌ 无 |
| 用户截图 Hierarchy | 有 `Objects/House_Tree`（蓝立方 Prefab 实例图标） |

**磁盘 `Objects` 子节点（已核实）**

| 名 | 类型 |
|----|------|
| StoneBrand / FallDownTreeCollider / Enemy / PlayerJumpGuideTirgger | 场景 GO |
| House_Npc4 / House_Npc1 / House_Npc45 / House_NPC2 / House_Chief | Prefab 实例（门类 guid `bf2a028c…`） |
| Door_Shop | Prefab 实例 → **换景进店** |
| SetAutoMoveTrigger | Prefab 实例 |
| House4 (3)(4)(5)(6) | Prefab 实例 |
| **House_Tree** | ❌ **不在磁盘列表** |

**易混物体（勿当本期挂点）**

| 物体 | 用途 | 本期 |
|------|------|------|
| `Map/.../树屋` + `TreeDoor1/2` | DepthZone 上下楼门控 | ❌ 勿改成对白 |
| `House_Npc*` / `Door_Shop` | `SceneChangeDoor` 进屋/进店 | ❌ 勿接 TreeHouseLock |
| 合层树屋美术 | 纯表现 | 交互体可旁挂，**不要**破坏 DepthZone |

**推断**：编辑器有未保存实例，或计划新建尚未落盘。施工：**创建 `Objects/House_Tree`（空壳或专用交互 Prefab）→ 摆到树屋门口视觉位置 → 存盘 → 再挂三件套。**

### B. `Village_TreeHouseLock` Prefab

| 检查 | 结果 |
|------|------|
| 根名 | ✅ `Village_TreeHouseLock` |
| 可加载 | ✅ `DialoguePath.GetPath("Village_TreeHouseLock")` |
| Actor | ✅ Yaer（雅尔） |
| 文案 | ✅「锁上了打不开」 |
| FaceType | ✅ **12** |
| 句数 | **1** Statement；无 LoadScene |
| 壳层 | ❌ **无** Fighting / **无** `NormalDialogueUIAlpha` / 无立绘淡入 |
| 对照 | `ForestSceneStoneBrand`、`Village_Npc1` **均有** UIAlpha 前奏 |

**可播性**：故事名对齐即可 Trigger；**对话框可见性有风险**（Panel 打开后壳层 alpha 可能仍 0）。  
**P0**：补最小壳——至少 `NormalDialogueUIAlpha` EndAlpha=1（可抄 StoneBrand Dur≈0.7 或 Npc1 Dur=1）；短锁对白**不必**雅大立绘。

### C. 物体交互金样对照（同村 `StoneBrand` + Item `面包`）

```
点击
  → RaycastListener.OnClick
       （requirePlayerOverlap=true 时须玩家碰撞相交；false=远程）
  → InteractiveComponent.onClickInteractiveEvent
  → SimpleStoryTrigger (TriggerType.Click)
  → StoryComponentGSM.TriggerStory(StoryPrefabName)
  → 打开 NormalDialogueNewPanel + StartDialogue
```

| 组件 / 字段 | StoneBrand（KenMuNi1） | 面包 Item | House_Tree 目标 |
|-------------|------------------------|-----------|-----------------|
| Layer | 21 | 21 | **21** |
| SceneEntity | ✅ | ✅ | ✅ |
| InteractiveComponent | ✅ | ✅ | ✅ |
| Collider2D + RaycastListener | ✅（可根上） | ✅ | ✅（门口小盒即可） |
| `requirePlayerOverlap` | **1**（须靠近） | **0**（远程） | **0**（产品「像物体」→ 面包侧） |
| SimpleStoryTrigger | Click；`ForestSceneStoneBrand` | Click；物品名 | Click；**`Village_TreeHouseLock`** |
| `SingleUseInArchive` | 0 | 0 | **0**（可重复） |
| CursorChangeTrigger | View(2) | 无 | **P1 View**（开放 Chat） |
| sceneObjs | ✅ 已登记 | HomeScene 登记 | **须追加** |
| 当前 Active | StoneBrand=`0`（关） | — | 树屋应 **Active=1** |

GSM：`Entity.objRoot` → `Objects`(fileID `1948841490`)；`sceneObjs` 现 12 条，**无** House_Tree 槽。

### D. 接线方案拍板

| 方案 | 裁定 |
|------|------|
| **A · Objects/House_Tree 上补三件套** | ✅ **推荐** |
| B · 旁挂空壳子物体点区域 | 仅当根不宜改时；默认可 A |
| C · GSM C# 特判树屋 | ❌ |
| D · Enter 自动播 | ❌ |

**拍板字段**

| 项 | 值 |
|----|-----|
| `StoryPrefabName` | `Village_TreeHouseLock` |
| `triggerType` | `Click` |
| `requirePlayerOverlap` | **`false`（0）** |
| `SingleUseInArchive` | **`false`** |
| 光标 | P1：`CursorChangeTrigger` **View**（对齐 StoneBrand）；Chat 可选 |
| 点击区 | **门口小盒**（勿整棵树巨型 Collider） |
| 换景 | ❌ 不挂 `SceneChangeDoor` |

### E. 与 Door_Shop / 进屋 / Chest C6 边界

| 线 | 行为 | 本期 |
|----|------|------|
| House_Tree + TreeHouseLock | 点 → 锁对白 | ✅ |
| Door_Shop | LoadScene 商店 | 回归勿动 |
| House_Npc* | 进屋 | 回归勿动 |
| ShopChest C6 | 店内黑屏切树屋 | ❌ 不做、勿混 |

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 确认/新建并**保存** `Objects/House_Tree`；摆到树屋门口 | **P0** |
| 2 | 三件套：SceneEntity + Interactive + Collider + RaycastListener（overlap=0） | **P0** |
| 3 | SimpleStoryTrigger → `Village_TreeHouseLock`，Click，可重复 | **P0** |
| 4 | Layer 21；GSM `sceneObjs` 追加该 SceneEntity | **P0** |
| 5 | Prefab 补 `NormalDialogueUIAlpha`（防无框） | **P0** |
| 6 | 可选 Cursor View | P1 |
| 7 | 进树屋场景 / Chest C6 | ❌ |

**预期 diff**

- `Village_KenMuNi1.unity`（新建 House_Tree + 组件 + sceneObjs）  
- `Village_TreeHouseLock.prefab`（补 UIAlpha 壳）  
- **一般不改** `Village_KenMuNiSceneManager.cs` / 商店脚本  

### G. 验收清单

同 §③。

### H. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 锁对白是否永远可重复？钥匙任务后改对白/禁用？ | 本期 **可重复**；任务改口另开 | ✅ |
| Q2 | 点击区整棵树还是门口小盒？ | **门口小盒** | ✅ |
| Q3 | 悬停 View 还是 Chat？ | **View**（P1）；Chat 次选 | ✅ 倾向 |
| Q4 | Hierarchy 有、磁盘无 —— 未保存？ | 施工前先存盘再扫；仍无则新建 | ⏳ 用户确认 |
| Q5 | 落点世界坐标？ | 编辑器贴树屋门视觉；参考 TreeDoor/合层树屋区，勿写死传送 | ⏳ 关卡摆 |

（已追加 `OPEN_QUESTIONS.md`。）
