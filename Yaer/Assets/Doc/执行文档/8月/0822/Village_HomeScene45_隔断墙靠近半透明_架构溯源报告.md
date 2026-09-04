# Village_HomeScene45 — 隔断墙靠近半透明 — 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读盘点；**本文件不施工**（不改场景 / 代码 / Prefab）  
**Unity**：2020.3.48f1 / C#  
**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
**目标物体**：`Object/隔断墙`（非 `Map/Design/村民家3合层` 内装饰）

关联：

- 提示词：`Assets/Doc/提示词/0822/Village_HomeScene45_隔断墙靠近半透明_架构侦探提示词.md`
- 可参考：`ActivateChildOnPlayerFootTrigger.cs`（PlayerFoot 检测范式）
- 否决对照：`VillageSceneObjectDepthSort.cs`（只切 Sorting Layer，不改 alpha）

---

## ① 结论一句话

**`Object/隔断墙` 现网只有 Transform + SpriteRenderer（无碰撞、alpha=1），缺 Trigger 与半透明逻辑；工程内无现成「靠近改 Sprite alpha」脚本。施工：新建 `SpriteFadeOnPlayerFootTrigger`（对齐 `ActivateChildOnPlayerFootTrigger` 的 PlayerFoot 识别）+ 子物体 `ProximityTrigger` 挂 BoxCollider2D Trigger；不改 GSM / SceneManager；现网合层实例无同名隔断墙（不叠图），源 Prefab 仍有装饰须防未来合并复现。**

---

## ② 原因（生活类比）

隔断墙是「毛玻璃屏风」——只挡视线、不挡脚（SortingOrder 挡前后，不靠碰撞）。现在屏风没有感应区，也没有变透的程序：玩家走近时 alpha 一直是 1。需要像门口脚垫传感器一样，在墙前划一块 **Trigger**，脚（`PlayerFoot`）踩进去就把 `SpriteRenderer.color.a` 降下来，出去再恢复。

这与村庄 Y 轴遮挡（`VillageSceneObjectDepthSort`）不是一回事：后者切 Sorting Layer 决定谁挡谁，**不会变半透明**。

---

## ③ 用户需要做什么

1. **认目标**：只给 **`Object/隔断墙`** 加感应与 fade；**不要**在 `Map/Design/村民家3合层` 上绑逻辑。  
2. **认行为**：墙**不挡人**（无实心 Collider）；走近变透、离开恢复。  
3. **认参数**：施工默认 `nearAlpha=0.4`、`fadeDuration=0.2s`（策划可后调）。  
4. 验收时反复进出 Trigger，并确认远程点面包/饼干、走近 NPC45 不受影响。

### 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 控制玩家穿过墙所在区域 | **不被** Collider 挡住 |
| 2 | 走进 Trigger | 墙明显变半透明 |
| 3 | 走出 Trigger | alpha 恢复 **1** |
| 4 | 反复进出 5 次 | 不卡半透明、不报错 |
| 5 | 远处点面包/饼干、走近 NPC45 | 交互正常 |
| 6 | Console | 无 NullRef / 无重复 Fade 异常 |

---

## ④ 给程序看的补充

### 4.1 目标物体确认（A）

| 物体 | 路径 | 组件 | Sprite | SortingOrder | 与本期关系 |
|------|------|------|--------|--------------|------------|
| **交互目标** | `Object/隔断墙` | Transform + SpriteRenderer | `隔断墙.png`（guid `988687fa…`）4.53×13.06 | **5** | **本期施工** |
| 合层装饰（源 Prefab） | `ArtRes/.../村民家3合层.prefab` 内 `隔断墙` | 仅 SpriteRenderer | 同 guid | 5 | **源文件有；现网场景实例无此子节点** |
| 合层实例（45 场景） | `Map/Design/村民家3合层` | 背景/灶台/面包/饼干/图层等 | — | — | **YAML 无 `隔断墙` 子物体** ✅ 当前不叠图 |

**世界/本地坐标（`Object/隔断墙`）**：`localPosition (-9.485, 1.11, 3.75)`；Z 仅用于前后排序。

**Renderer 数量**：**单 SpriteRenderer**（无子节点）。

**美术去重裁定**：现网 45 场景**无需**关合层 Renderer（合层里已无隔断墙）。若日后重新 Apply 合层 Prefab 带出同名子节点，按面包/饼干先例 **场景实例 Disable 合层 `隔断墙` SpriteRenderer**，只保留 `Object/隔断墙` 受 fade 脚本驱动。

### 4.2 玩家脚底契约（B）

| 项 | `Player.prefab` 现网 |
|----|----------------------|
| 物体名 | **`PlayerFoot`** |
| Layer | **3**（`LayerName.PlayerFoot`） |
| Collider | **CapsuleCollider2D**，`IsTrigger: 0`，Size ≈ `(1, 2)` |
| 玩家根 | 有 **Rigidbody2D**（Dynamic，`UseFullKinematicContacts: 1`） |
| Tag | `Untagged`（**不用 Tag 检测**） |

**检测约定**：与 `ActivateChildOnPlayerFootTrigger` 一致，用 **`other.gameObject.name == "PlayerFoot"`**（或序列化字段默认 `"PlayerFoot"`）。该参考脚本同时实现 **Trigger + Collision** 四套回调，兼容脚底非 Trigger 配置。

**隔断墙侧**：**不需** Rigidbody2D；静态 Trigger + 玩家刚体即可收到 `OnTriggerEnter2D` / `OnCollisionEnter2D`。

### 4.3 现网缺口表（C）

| 检查项 | 现网 | 应有 |
|--------|------|------|
| 实心 Collider（挡人） | **无** ✅ | **无** |
| Trigger Collider | **无** ❌ | **有**（BoxCollider2D IsTrigger） |
| 半透明脚本 | **无** ❌ | **有**（改 SpriteRenderer alpha） |
| SceneEntity / GSM | **无** ✅ | **不需要**（纯视觉） |
| `VillageSceneObjectDepthSort` | **无** ✅ | **不挂**（见 §4.5） |

### 4.4 方案对比与裁定（D）

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A（推荐）** | 新建 `SpriteFadeOnPlayerFootTrigger` + 子物体 `ProximityTrigger` | ✅ **主推** |
| B | 场景内联脚本 | ❌ 不可复用 |
| C | `VillageSceneObjectDepthSort` | ❌ **不满足半透明** |
| D | 专用 Shader/Material | ❌ 本期过度 |
| 扩展 `ActivateChildOnPlayerFootTrigger` | 加 alpha 分支 | ❌ 职责混杂（现网只做 SetActive） |

**为何不扩展 `ActivateChildOnPlayerFootTrigger`**：该类语义是「脚底进/出 → **SetActive** 子物体」；本期是「改 **SpriteRenderer.color.a** + 可选 Lerp」。新建类更清晰，且可序列化 `SpriteRenderer[]`、`normalAlpha`、`nearAlpha`、`fadeDuration`。

**Trigger 挂载（D1 推荐）**：

```
Object/隔断墙
├── SpriteRenderer          （保持；受 fade 驱动）
└── ProximityTrigger        （新建子物体）
    ├── BoxCollider2D       IsTrigger=true；尺寸略大于墙图
    └── SpriteFadeOnPlayerFootTrigger
        └── targetRenderers → 拖父节点 SpriteRenderer
```

脚本挂在 **子物体**（与 Collider 同 GO），`OnTriggerEnter2D` 才能稳定收到回调；父节点只负责显示。

**替代子方案 D2**（Collider 与 Sprite 同 GO）：可行但 Trigger 与美术 bounds 绑死，不利于略放大感应区；**不推荐**。

### 4.5 与遮挡系统关系（E）

| 项 | 裁定 |
|----|------|
| 是否挂 `VillageSceneObjectDepthSort` | **否**。室内 `Village_HomeScene45SceneManager.GetCurSceneTerrainType()` 为 **IndoorType**，玩家 Locomotion 非 `Village2_5D`，该脚本本就不会跑；且它只切 **Sorting Layer**，不做 alpha。 |
| 半透明时 Sorting | **保持不变**（`SortingOrder=5`、Default Layer）；fade 脚本 **只写 `color.a`**，不改 `sortingLayerName` / `sortingOrder`。 |
| 与 `DepthComponent` | 隔断墙未挂；**无需** |

### 4.6 推荐脚本行为（施工参考）

**路径**：`Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SpriteFadeOnPlayerFootTrigger.cs`

**核心逻辑（须中文注释）**：

1. `OnTriggerEnter2D` / `OnCollisionEnter2D`：若 `other.gameObject.name == playerFootObjectName` → `_overlapCount++`；从 0→1 时开始向 `nearAlpha` Lerp。  
2. `OnTriggerExit2D` / `OnCollisionExit2D`：匹配 PlayerFoot → `_overlapCount--`；归零时 Lerp 回 `normalAlpha`。  
3. 用 **引用计数** 防止多 Collider 抖动导致闪 alpha。  
4. `OnDisable`：停止协程/Tween，**强制恢复** `normalAlpha`。  
5. **禁止** `Update` 里 `Find("Player")`。  
6. Lerp：协程 `Mathf.Lerp` 或 `SpriteRenderer.DOFade`（工程有 DOTween；`OnDisable` 须 `DOKill`）。

**参考**：`ActivateChildOnPlayerFootTrigger.cs`（PlayerFoot 过滤 + 双 Trigger/Collision 回调）；**勿抄** `TreeBridgeLogic` 的 Interactive 链（本期无交互）。

### 4.7 开放参数表（施工默认）

| 参数 | 建议默认 | 说明 |
|------|----------|------|
| `normalAlpha` | `1` | 远离 / 初始 |
| `nearAlpha` | **`0.4`** | 靠近半透明（策划区间 0.35～0.5） |
| `fadeDuration` | **`0.2`** s | `0`=瞬切；>0 协程/DOTween |
| `playerFootObjectName` | `"PlayerFoot"` | 与 Prefab 一致 |
| Trigger Size（BoxCollider2D） | **约 `5.5 × 14`**（墙图 4.53×13.06 略放大） | Scene Gizmos 验收；可向玩家站立侧略外扩 |
| Trigger Offset | 居中或略朝玩家侧 | Play 微调 |

### 4.8 Hierarchy 施工步骤（Unity Editor）

1. 新建脚本 `SpriteFadeOnPlayerFootTrigger.cs`（含 `.meta`），放 `CommonEntity`。  
2. 在 `Object/隔断墙` 下新建子物体 **`ProximityTrigger`**（Layer 保持 Default 0 即可）。  
3. 子物体加 **BoxCollider2D**：`Is Trigger = true`；按 §4.7 调 Size/Offset。  
4. 子物体挂 **SpriteFadeOnPlayerFootTrigger**：`targetRenderers` 拖 **父节点** `隔断墙` 的 SpriteRenderer；填 alpha / duration。  
5. **确认父节点无 Collider**（保持可穿过）。  
6. 保存场景；Play 验收 §4.3 表。  
7. **不改** `Village_HomeScene45SceneManager.cs`、GSM、Item 门 NPC。

### 4.9 最小改动文件列表

| 文件 | 动作 |
|------|------|
| `.../CommonEntity/SpriteFadeOnPlayerFootTrigger.cs` | **新建** |
| `Assets/GameRes/Scenes/Village_HomeScene45.unity` | `Object/隔断墙` 增子物体 + 组件 |
| `村民家3合层.prefab` | **本期不改**（场景已无叠图） |
| `Village_HomeScene45SceneManager.cs` | **不改** |

### 4.10 严禁

- 给隔断墙加 **非 Trigger** Collider 挡人  
- Trigger 挂在合层 Prefab 源上  
- `Update` 每帧 Find Player  
- 登记 `SceneEntity` / GSM  
- 同物体挂 `VillageSceneObjectDepthSort` 与 fade 脚本抢写渲染状态  
- 用 `SetActive` 隐藏整墙代替半透明（会闪灭 NPC/后面物体）

### 4.11 开放问题

见 `OPEN_QUESTIONS.md` §「Village_HomeScene45 隔断墙半透明 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0 侦探：Object/隔断墙 纯 Sprite；无现成 fade 脚本；方案 A 新脚本 + 子 Trigger；合层实例无叠图 |
