# Village_HomeScene45 — 面包/饼干 Item 预制体替换与 GSM 绑定 — 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读盘点；**本文件不施工**（不改场景 / Prefab / 代码）  
**Unity**：2020.3.48f1 / C#  
**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
**范围**：`Object` 下 **面包 / 饼干** 空壳 → `Item/面包.prefab`、`Item/饼干.prefab`；GSM `sceneObjs` 登记；**NPC45 本期不动**

关联：

- 提示词：`Assets/Doc/提示词/0822/Village_HomeScene45_面包饼干Item替换与GSM绑定_架构侦探提示词.md`
- 样板：`0820` HomeScene1 Object 全量配置、`0820` 远程点击物品交互
- 上案：`0822` NPC45 配置、`0821` 左门出屋

---

## ① 结论一句话

**屋里面包/饼干点不了，因为 `Object` 下仍是只有 `SpriteRenderer` 的空壳（无 `SceneEntity`、无碰撞、无对白），GSM `sceneObjs` 也只登记了 NPC45。施工：删掉两个空壳 → 在 `Object` 下实例化 `Item/面包.prefab` 与 `Item/饼干.prefab`（预制体已齐远程点击 + 对话名）→ `sceneObjs` 增至 3 条；`Village_HomeScene45SceneManager.cs` 不用改；合层里同名装饰面包/饼干须 **Disable SpriteRenderer** 防叠图。**

---

## ② 原因（生活类比）

`Object` 下摆着 **两张印刷画**（只有 Sprite，没门铃），`Map/Design/村民家3合层` 墙上还有 **同款装饰画**。工程里其实有带门铃和剧本的 **真物品预制体**（面包/饼干 Item），但还没换上去，住户登记表（`sceneObjs`）也没写这两个名字——所以远处点不到、也播不了对白。

「改场景管理器」**不是改 C#**，是把 GSM 的 `sceneObjs` 补上（运行时也会重扫，但 YAML 建议同步）。

---

## ③ 用户需要做什么

1. **认替换目标**：只动 **`Object/面包`、`Object/饼干`**；**不要**在 `Map/Design/村民家3合层` 下挂交互体。  
2. **认 Prefab 名**：实例化 `Assets/GameRes/Prefabs/Item/面包.prefab`、`饼干.prefab`；对白名已是 `Village_Npc1_mianbao` / `Village_Npc1_bingan`。  
3. **认美术去重**：替换后须 **关掉合层里装饰面包/饼干的 SpriteRenderer**（见 §4.6），否则会叠两层图。  
4. 施工后验收：进屋不黑屏 → **远处鼠标点**面包/饼干播对白 → NPC45 走近 E 仍正常 → 左门出屋仍正常。

### 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | `House_Npc45` 进屋 | 不黑屏；Console 无 NRE / 无「未注册=>面包」 |
| 2 | **远处**点 `Object` 下面包 | 播 `Village_Npc1_mianbao`，无需走近 |
| 3 | **远处**点饼干 | 播 `Village_Npc1_bingan` |
| 4 | 走近 NPC45 按 E | 仍播 `Village_Npc45` |
| 5 | 左/右门出屋 | 仍正常（0821） |
| 6 | 目视 | 桌上不叠两层面包/饼干 |

---

## ④ 给程序看的补充

### 4.1 Object vs Map 双层面包/饼干

| 位置 | 物体名 | 组件 | 交互？ | 施工 |
|------|--------|------|--------|------|
| **`Object/面包`** | 面包 | Transform + SpriteRenderer（Layer **0**） | **否**（空壳） | **删除** → 换 Item 预制体实例 |
| **`Object/饼干`** | 饼干 | Transform + SpriteRenderer（Layer **0**） | **否** | 同上 |
| **`Object/NPC45`** | NPC45 | 三件套齐，`StoryPrefabName=Village_Npc45` | 近距 E | **本期不动** |
| **`Map/Design/村民家3合层/面包`** | 面包 | 仅 SpriteRenderer（装饰） | **否** | **Disable Renderer**（场景 Override） |
| **`Map/Design/村民家3合层/饼干`** | 饼干 | 仅 SpriteRenderer | **否** | **Disable Renderer** |

合层来源：场景 PrefabInstance `村民家3合层.prefab`（guid `d1e960df…`），根节点约 `(-28.98, -5.39)`。装饰面包/饼干与 `Object` 空壳 **同位置叠放**（同 Sprite GUID：`d3c84f06` 面包、`55f1f493` 饼干），换 Item 后若不去重会 **三层图**（合层 + Item 自带 Sprite）。

**侦探裁定（美术去重）**：在 **场景实例** 上对合层子节点 `面包`、`饼干` 的 `SpriteRenderer.m_Enabled = 0`（PrefabInstance 属性覆盖）；**不改** `ArtRes/.../村民家3合层.prefab` 源文件。Item 预制体自带交互用 Sprite（面包 `6fd9ccfd`、饼干 `d2a6160b`），以 Item 为准。

### 4.2 现网空壳盘点（替换前坐标，须保留）

| 物体 | localPosition（Object 下） | Layer | 组件 | SceneEntity | 应在 sceneObjs |
|------|---------------------------|-------|------|-------------|----------------|
| **面包** | `(-22, 0.635, 7.5)` | 0 | Transform + SpriteRenderer | **无** | 替换后 **要** |
| **饼干** | `(-25.38, 0.425, 6.25)` | 0 | Transform + SpriteRenderer | **无** | 替换后 **要** |
| **NPC45** | `(-19.2, -0.02, 0)` | 21 | 三件套齐 | **有**（`751582444`） | **已登记** |

`Object` 子节点现网顺序：`NPC45` → `面包` → `饼干`。

### 4.3 Item 预制体对拍（施工直接拖，勿手搓）

| Prefab | guid | Layer | StoryPrefabName | requirePlayerOverlap | componentsList | 对话 Prefab |
|--------|------|-------|-----------------|----------------------|----------------|-------------|
| `Item/面包.prefab` | `5eae2847…` | **21** | `Village_Npc1_mianbao` | **0**（远程） | 1 条 Interactive，**无 None** | ✅ 磁盘存在 |
| `Item/饼干.prefab` | `a0b2f1ca…` | **21** | `Village_Npc1_bingan` | **0** | 同上 | ✅ 磁盘存在 |

结构（两套一致）：`SceneEntity` + `SimpleStoryTrigger` + `ComponentSystemMono` + `BaseEntityControll`（`canTouchWithPlayer=0`）+ `Components/Interactive` + `Clds/Body`（BoxCollider2D `1.5×1.2` + `RaycastListener`）。

**Prefab 默认位（HomeScene1 已用，仅供参考）**：

| Prefab | 默认 localPosition |
|--------|-------------------|
| 面包 | `(-21.805, 0.675, 7.692)` |
| 饼干 | `(-25.215, 0.435, 8.461)` |

**45 号屋施工优先对齐 §4.2 空壳坐标**（与合层装饰同位）；Z 可保留 Item 预制体默认（排序用，远程不依赖 overlap）。

### 4.4 StoryPrefabName 映射表

| 场景实例（Object 下） | Item 预制体 | SimpleStoryTrigger（预制体内已写好） | 对话 Prefab 路径 |
|----------------------|-------------|--------------------------------------|------------------|
| 面包 | `Item/面包.prefab` | `Village_Npc1_mianbao` | `GameRes/Prefabs/Dialogue/Village_Npc1_mianbao.prefab` |
| 饼干 | `Item/饼干.prefab` | `Village_Npc1_bingan` | `GameRes/Prefabs/Dialogue/Village_Npc1_bingan.prefab` |

**禁止改**预制体源里的 `StoryPrefabName`（除非验收加载失败）。

### 4.5 GSM / 场景管理器

| 检查项 | 现网 | 施工后 |
|--------|------|--------|
| `objRoot` | → `Object` ✅ | 不变 |
| `sceneObjs` 条数 | **1**（仅 NPC45 `751582444`） | **3**（+面包 +饼干 的 `SceneEntity`） |
| `Map.sceneEntityComponentGSM` | → `Entity/549694533` ✅ | 不变 |
| `Village_HomeScene45SceneManager.cs` | 已挂，无 per-item 逻辑 | **不改** |
| `Village_HomeScene45.asset` | `isFightingScene=0` ✅ | 不改 |

`SceneEntityComponentGSM.OnInit` 会 `GetComponentsInChildren<SceneEntity>(objRoot)` 重扫；**Play 不挡**，但 Editor 列表建议保存时刷新，避免误以为未登记。

### 4.6 推荐施工步骤（Unity Editor）

1. 打开 `Village_HomeScene45.unity`，**记录** §4.2 两空壳坐标。  
2. **删除** `Object/面包`、`Object/饼干`（仅 Transform+Sprite 空壳）。  
3. 拖入 **`Item/面包.prefab`、`Item/饼干.prefab`** 到 **`Object` 下**（与 HomeScene1 相同做法：PrefabInstance）。  
4. 实例 **localPosition** 设回 §4.2（或微调对齐合层桌面）。  
5. 选中 **`Map/Design/村民家3合层`** 实例 → 展开子物体 **`面包`、`饼干`** → **取消勾选 SpriteRenderer**（或 `m_Enabled=0` Override）。  
6. 选中 **`Entity`** → `SceneEntityComponentGSM`：确认 `sceneObjs` 含 **NPC45 + 面包 + 饼干** 三个 `SceneEntity`（保存场景触发 `OnValidate` 重扫）。  
7. 检查三个实例 `componentsList` **无 `None` 空槽**。  
8. **勿动** NPC45、LeftDoor/RightDoor、Item 预制体源文件。

### 4.7 与 NPC45 的差异（验收勿混）

| 项 | NPC45 | 面包 / 饼干 |
|----|-------|-------------|
| 交互 | 走近出 **E** | **远处鼠标点** |
| `requirePlayerOverlap` | **true** | **false**（预制体已设） |
| 根 Z | **0**（近距必须） | 可保留较高 Z（如 6~8，仅排序） |
| StoryPrefabName | `Village_Npc45` | `Village_Npc1_mianbao` / `_bingan` |

### 4.8 最小改动文件列表

| 文件 | 动作 |
|------|------|
| `Assets/GameRes/Scenes/Village_HomeScene45.unity` | 删 2 空壳；增 2× Item PrefabInstance；合层装饰 Disable Renderer；`sceneObjs` 增至 3 |
| `Item/面包.prefab`、`Item/饼干.prefab` | **不改**（除非对白加载失败） |
| `Village_HomeScene45SceneManager.cs` | **不改** |
| `ArtRes/.../村民家3合层.prefab` | **不改**（只场景实例 Override） |

### 4.9 严禁

- 只换 Sprite、不挂 Item 预制体  
- 把 Item 挂在 `Map/Design` 下（不在 `objRoot` 子树）  
- `componentsList` 留 `fileID: 0`  
- 误删 NPC45 或破坏 0821 出门链路  
- 改合层 **源 Prefab** 当去重（应只改场景实例）

### 4.10 仅技术开放问题

见 `OPEN_QUESTIONS.md` §「Village_HomeScene45 面包饼干 Item 替换 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0 侦探：空壳+GSM 缺登记；Item 预制体齐；合层须 Disable Renderer 去重 |
