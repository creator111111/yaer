# Village_HomeScene45 — 新增 NPC45 配置与 GSM 绑定 — 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读盘点；**本文件不施工**（不改场景 / Prefab / 代码 / CSV）  
**Unity**：2020.3.48f1 / C#  
**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
**现象**：Hierarchy 已加 `NPC45`（开发者口述），磁盘场景 **尚未保存**；需配交互三件套 + GSM 登记 + 绑对话 Prefab

关联：

- 提示词：`Assets/Doc/提示词/0822/Village_HomeScene45_NPC45配置与GSM绑定_架构侦探提示词.md`
- 样板：`0820` HomeScene1 Npc1 / Object 全量配置；`0821` HomeScene45 左门出屋
- 对话 CSV：`Assets/Dialog/Village_NPC45_对话交互.csv`
- GSM：`SceneEntityComponentGSM.cs`（运行时 `objRoot` 重扫）

---

## ① 结论一句话

**屋里还没有能对话的 NPC45：磁盘 YAML 只有 `Object/Npc1`，没有 `NPC45`；对话 Prefab 已 Import 为 `Village_Npc45`（注意大小写），Speaker 4/5 映射已齐。施工只需 Duplicate `Npc1` 补三件套、`StoryPrefabName=Village_Npc45`、根 Z=0；`Village_HomeScene45SceneManager.cs` 不用改，GSM 运行时重扫会登记，保存场景时 `sceneObjs` 建议同步。**

---

## ② 原因（生活类比）

场景管理器像「住户登记表」——`Entity` 上的 `SceneEntityComponentGSM` 已经指好 `objRoot=Object`，但表里 **只登记了 Npc1**。你在 Hierarchy 里加了 `NPC45`，若没保存或没装门铃（`SceneEntity` + `InteractiveComponent` + `SimpleStoryTrigger`），走近就不会出 E，也播不了对白。

对话台本 CSV 和 Prefab **已经写好**，缺的是场景里把 NPC45 **登记进表并接线**。

---

## ③ 用户需要做什么

1. **先 Ctrl+S 保存场景**（磁盘目前无 `NPC45`，施工员无法改空壳）。  
2. **认 StoryPrefabName**：磁盘 Prefab 名是 **`Village_Npc45`**（`Npc` 小写 pc），**不是** `Village_NPC45` 或 `HomeScene1Npc*`。  
3. 施工后验收：进村 → 走近 NPC45 出 E → 按 E 播对白 → 左门出屋仍正常。  
4. **本期可不动 Npc1**（其 `StoryPrefabName` 仍是龙宫残留 `HomeScene1Npc1`，另案）。

### 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 `House_Npc45` 进屋 | 不黑屏 |
| 2 | 走近 `Object/NPC45` | 出现 **E** |
| 3 | 按 E | 播 CSV 六句（Speaker 4 / 5 / 雅） |
| 4 | Console | 无「未注册」/ Dialogue 加载失败 |
| 5 | `LeftDoor` 出屋回村 | 仍正常（0821 勿回归） |

---

## ④ 给程序看的补充

### 4.1 开发者是否已保存场景

| 检查 | 结果 |
|------|------|
| 磁盘 `Village_HomeScene45.unity` 搜 `NPC45` / `Npc45` | **无匹配** |
| `Object` 子物体（Transform children） | **仅 `Npc1`**（fileID `1099577284`） |

**结论：须先 Unity 内 Ctrl+S 保存场景，再让施工员改 YAML；否则施工只能从零在磁盘创建 NPC45。**

### 4.2 NPC45 现网盘点表

| 检查项 | 期望 | 现网（磁盘） |
|--------|------|--------------|
| 在 `Object` 下 | 是 | **否**（无 NPC45） |
| 有 `SceneEntity` | 是 | **否** |
| 有 `SimpleStoryTrigger` | 是 | **否** |
| 有 `InteractiveComponent` + `Clds/Body` | 是 | **否** |
| 根 Z = 0 | 是 | — |
| `StoryPrefabName` | `Village_Npc45` | — |
| Layer | 与 Npc1 一致（**21**） | — |

### 4.3 样板：`Object/Npc1`（施工照抄）

```
Object/Npc1  (Layer 21, Position Z=0)
├── SpriteRenderer
├── SceneEntity
├── SimpleStoryTrigger  ← StoryPrefabName 现网误写 HomeScene1Npc1（本期不动）
├── ComponentSystemMono → componentsList: [InteractiveComponent]
├── BaseEntityControll  ← canTouchWithPlayer=1, entityType=3
├── Components/
│   └── InteractiveComponent
│       ├── sceneEntity → Npc1.SceneEntity
│       ├── entityControll → BaseEntityControll
│       ├── interactiveCollider → Clds/Body BoxCollider2D
│       └── raycastListeners → Body.RaycastListener
└── Clds/
    └── Body
        ├── BoxCollider2D  IsTrigger=1, Size≈(1.24, 5.56)
        └── RaycastListener  requirePlayerOverlap=1
```

**村庄硬约定**（0820 Npc1 无 E 报告）：根 **Z=0**；`requirePlayerOverlap=true`；**勿**勾远程忽略距离。

### 4.4 场景管理器 / GSM 绑定

| 对象 | 要不要改 | 现网 |
|------|----------|------|
| `Village_HomeScene45SceneManager.cs` | **否** | 已挂 SceneManager（guid `a1b2c3d4…`）；`nowSceneName=Village_HomeScene45`、`KenMuNi`、室内脚步齐 |
| `Village_HomeScene45.asset` | **否** | `isFightingScene:0`、`canCreatePlayer:1` ✓ |
| `Map.sceneEntityComponentGSM` | **否**（引用链通） | → `Entity` 上 `{fileID: 549694533}` ✓ |
| `Entity.SceneEntityComponentGSM.objRoot` | **否** | → `Object`（`442106408`）✓ |
| `Entity.sceneObjs` YAML 列表 | **施工后追加 NPC45** | 现网 **仅 Npc1**（`1099577288`） |

`SceneEntityComponentGSM.OnInit` **运行时会 `GetComponentsInChildren<SceneEntity>` 重扫**（注释写明防 YAML 漏登记）。因此：

- **最低要求**：NPC45 在 `Object` 下且挂 `SceneEntity` → Play 能 Init；  
- **建议**：保存场景时让 `OnValidate` 刷新 `sceneObjs`，Editor 里一眼可见登记齐全。

**不需改 C# 场景管理器**：无 per-NPC 逻辑。

### 4.5 对话资源链

| 环节 | 状态 | 说明 |
|------|------|------|
| CSV | ✅ | `Assets/Dialog/Village_NPC45_对话交互.csv`（6 句；Speaker `4`/`5`/`雅`） |
| Import Generated | ✅ | `Assets/GameRes/DialogueTrees/Generated/Village_NPC45_对话交互.asset` |
| 运行时 Prefab | ✅ | **`Assets/GameRes/Prefabs/Dialogue/Village_Npc45.prefab`** |
| Speaker `4`→`NPC4`、`5`→`NPC5` | ✅ | `DialogueSpeakerMapping_Default.asset` 已登记 |
| Speaker `雅`→`雅尔` | ✅ | 映射表 `csvSpeaker: 雅` |
| 图内 Actor 绑定 | ✅ | `derivedData.actorParameters`：`NPC4` / `NPC5` / `雅尔` |
| 图内节点 `_actorName` | ✅ | 语句节点写 `NPC4`、`NPC5`、`雅尔` |

**StoryPrefabName 映射表（侦探裁定）**：

| 场景物体 | CSV | 磁盘 Prefab 名 | `SimpleStoryTrigger.StoryPrefabName` |
|----------|-----|----------------|--------------------------------------|
| **NPC45** | `Village_NPC45_对话交互.csv` | **`Village_Npc45`** | **`Village_Npc45`**（逐字一致） |

加载逻辑按 **Prefab 文件名**（无路径、无扩展名），大小写须与 `GameRes/Prefabs/Dialogue/Village_Npc45.prefab` 一致。提示词里的 `Village_NPC45` 与磁盘 **`Village_Npc45`** 不同，**以磁盘为准**。

**次要风险（不挡 E / 播对白）**：Prefab 子节点 `npc4` / `npc5` 上 `DialogueActor._name` 仍显示 `NPC2`（复制残留）；图参数键已是 `NPC4`/`NPC5`。若字幕名字显示不对，再改 Prefab 内 `_name`（本期可选）。

### 4.6 暗版 Clip 引用核实（Npc1 旁注）

| 物体 | 现网 `StoryPrefabName` | 村民家应有 |
|------|------------------------|------------|
| Npc1 | `HomeScene1Npc1`（龙宫残留） | 本期 **不动**；若要对白应另案改 `Village_Npc1` |

### 4.7 0821 出屋链路（施工勿碰）

| 门 | `SceneChangeDoor` | `NextSceneName` | 状态 |
|----|-------------------|-----------------|------|
| **LeftDoor** | **Enabled** | `Village_KenMuNi1` | ✅ 主出口（0821 已施工） |
| **RightDoor** | **Disabled** | `Village_KenMuNi1` | ✅ 保持 Disable |

RightDoor 下有一组 **孤儿 `InteractiveComponent`**（`1105019192`，`raycastListeners: []`），与 NPC45 无关；本期可不管，勿误删导致门组件缺失。

### 4.8 推荐施工方案（最小改动）

1. Unity 打开 `Village_HomeScene45` → **Ctrl+S**（若 Hierarchy 已有 NPC45）。  
2. **Duplicate `Object/Npc1`** → 改名 **`NPC45`** → 调整 XY 站位 / 换 `SpriteRenderer` 立绘。  
3. 根 Transform **Z = 0**；Layer **21**。  
4. `SimpleStoryTrigger.StoryPrefabName` → **`Village_Npc45`**。  
5. 确认三件套连线完整（照抄 Npc1：`InteractiveComponent` 指 Body、`raycastListeners` 含 Body Listener）。  
6. 保存场景 → `Entity.sceneObjs` 应出现第二条（或依赖运行时重扫）。  
7. **不改** `Village_HomeScene45SceneManager.cs`、Config、龙宫资源。

**若无 Hierarchy 未保存对象**：直接在 `Object` 下 Duplicate `Npc1` 即可，不必等开发者先拖空壳。

### 4.9 最小改动文件列表（只建议）

| 文件 | 动作 |
|------|------|
| `Assets/GameRes/Scenes/Village_HomeScene45.unity` | 增/改 `Object/NPC45` 全结构；`Entity.sceneObjs` 追加；保存 |
| `Village_HomeScene45SceneManager.cs` | **不改** |
| `Village_HomeScene45.asset` | **不改** |
| `Village_Npc45.prefab` | **已存在**；仅当字幕 Actor 显示名错误时再改 `_name` |
| `Village_NPC45_对话交互.csv` | **不改**（已 Import） |

### 4.10 仅技术开放问题

见 `OPEN_QUESTIONS.md` §「Village_HomeScene45 NPC45 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0 侦探：磁盘无 NPC45；Prefab=Village_Npc45；GSM 不必改 C#；最小 Duplicate Npc1 方案 |
