# Village_HomeScene1 — Object 全量配置与 GSM 绑定 — 架构溯源报告

**文档性质**：架构侦探产出（场景配置盘点；**本阶段不改资产**）  
**日期**：2026-08-20  
**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene1.unity`  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- 提示词：`Assets/Doc/提示词/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构侦探提示词.md`
- 远程点击：`Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md`（**方案 A 已进工程**：`RaycastListener.requirePlayerOverlap`）
- 三件套样板：`Assets/Doc/执行文档/6月/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md`
- `SceneEntityComponentGSM.cs`、`Village_HomeScene1SceneManager.cs`

**产品分类**：

| 类型 | 物体 | 交互规则 |
|------|------|----------|
| 正常 NPC | `Npc1` | 近距点击 / E（`requirePlayerOverlap=true`） |
| 互动物品 | 饼干/面包/土豆/木箱/木桶/米袋 | **远程点击**（`requirePlayerOverlap=false`） |

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**Object 下 7 个物体目前都只是「Sprite 空壳」，没有 SceneEntity / 交互三件套 / StoryTrigger；GSM 的 `objRoot` 已指向 Object，但 `sceneObjs` 只有脏空槽、重扫也扫不到实体——须按 NpcChair 样板给每个物体补全组件并挂对 `Village_Npc1*` Prefab，物品关 overlap、Npc1 保持近距。**

---

## ② 原因（生活类比）

Object 像货架上摆好的按钮外观；场景管理器像总电源接线板。  
现在按钮只有塑料壳、没电线（无 SceneEntity / 无点击盒 / 无对白名），接线板 `objRoot` 插对了架子，但插座列表是空的（`sceneObjs` 一条 `None`）——按了没电。

---

## ③ 用户需要做什么

### 绑定 GSM（定义）

「和场景管理器绑定」=：

1. `SceneEntityComponentGSM.objRoot` → 含交互体的根（现网已是 **`Object`**）  
2. 每个可交互物体挂 **`SceneEntity`**，且在 `objRoot` 子树内  
3. 运行时 `OnInit` **按 objRoot 重扫** `GetComponentsInChildren<SceneEntity>` 填 `sceneObjs`（编辑器列表可脏，但**必须以子树有 SceneEntity 为准**）  
4. 场景主逻辑为 **`Village_HomeScene1SceneManager`**（现网已挂，勿换成龙宫 `HomeScene1Manager`）

只拖进 Hierarchy **不够**。

### 每个物体勾选（施工目标）

| 物体 | SceneEntity | Interactive+Body+RaycastListener | SimpleStoryTrigger | StoryPrefabName | requirePlayerOverlap |
|------|-------------|----------------------------------|--------------------|-----------------|----------------------|
| Npc1 | 要 | 要 | 要 | `Village_Npc1` | **true（近距）** |
| 饼干 | 要 | 要 | 要 | `Village_Npc1_bingan` | **false（远程）** |
| 面包 | 要 | 要 | 要 | `Village_Npc1_mianbao` | false |
| 土豆 | 要 | 要 | 要 | `Village_Npc1_tudou` | false |
| 木箱 | 要 | 要 | 要 | `Village_Npc1_muxiang` | false |
| 木桶 | 要 | 要 | 要 | `Village_Npc1_muzhiyuantong` | false |
| 米袋 | 要 | 要 | 要 | `Village_Npc1_huangmi` | false |

仿 `Village_HomeScene23/NpcChair`：`Components/InteractiveComponent`、`Clds/Body`（Collider2D + RaycastListener）、根上 `SimpleStoryTrigger` + `SceneEntity`。

### 最小施工顺序

1. ~~远程开关~~ → **已落地**（可配物品 `requirePlayerOverlap=0`）  
2. **逐物体补组件**（可复制 NpcChair 结构剥任务逻辑）  
3. 填 `StoryPrefabName`（与磁盘 Prefab **逐字一致**）  
4. 清 `sceneObjs` 脏 `None`；进 Play 确认重扫到 7 个 SceneEntity  
5. 验收：近距 Npc1；远处点六物品  

**不要**：把 `Map/Design/村民家1合层` 当交互体；改龙宫 `HomeScene1`；把 Npc1 改成远程。

---

## ④ 给程序看的补充

### 4.1 GSM 绑定现网对拍

| 检查项 | 应是 | 现网 |
|--------|------|------|
| 场景 Manager | `Village_HomeScene1SceneManager` | ✅ 已挂（guid `c1d2e3f4…`） |
| `SceneEntityComponentGSM.objRoot` | `Object` Transform | ✅ `fileID: 442106408`（即 Object） |
| `sceneObjs` | 全部交互 SceneEntity | ❌ **仅** `- {fileID: 0}`（脏空槽） |
| Object 下 SceneEntity | 每交互体一个 | ❌ **零** → 运行时重扫仍为空列表 |
| 缺 SceneEntity 后果 | — | 实体永不 `OnInit`；点击/E 链找不到可交互组件 |

```28:33:Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/SceneEntityComponentGSM.cs
            // 运行时按 objRoot 重扫：避免场景 YAML 漏挂 sceneObjs 导致 NPC Start 报「未注册」且永不 OnInit。
            if (objRoot != null)
            {
                sceneObjs = objRoot.GetComponentsInChildren<SceneEntity>(true).ToList();
            }
```

### 4.2 Object 全量盘点表（必出）

Object 子物体 **恰好 7 个**（与开发者名单一致，无遗漏其它交互名）：

| 物体名 | SceneEntity | Interactive 三件套 | RaycastListener | SimpleStoryTrigger | StoryPrefabName | 远程忽略距离 | 结论 |
|--------|-------------|--------------------|-----------------|--------------------|-----------------|--------------|------|
| **饼干** | ❌ | ❌ 仅 Transform+SpriteRenderer | ❌ | ❌ | — | — | **全缺**；空壳美术节点 |
| **面包** | ❌ | ❌ 同上 | ❌ | ❌ | — | — | **全缺** |
| **木桶** | ❌ | ❌ | ❌ | ❌ | — | — | **全缺** |
| **米袋** | ❌ | ❌ | ❌ | ❌ | — | — | **全缺** |
| **木箱** | ❌ | ❌ | ❌ | ❌ | — | — | **全缺** |
| **土豆** | ❌ | ❌ | ❌ | ❌ | — | — | **全缺** |
| **Npc1** | ❌ | ❌ 同上（无 Children） | ❌ | ❌ | — | — | **全缺**（连近距对话都未配） |

现网 **无任何** `StoryPrefabName` / `requirePlayerOverlap` 序列化在这些物体上。

### 4.3 StoryPrefabName 最终映射（磁盘已存在）

| Hierarchy | StoryPrefabName（须一致） | 磁盘 Prefab |
|-----------|---------------------------|-------------|
| Npc1 | `Village_Npc1` | ✅ `…/Dialogue/Village_Npc1.prefab` |
| 饼干 | `Village_Npc1_bingan` | ✅ |
| 面包 | `Village_Npc1_mianbao` | ✅ |
| 土豆 | `Village_Npc1_tudou` | ✅ |
| 木箱 | `Village_Npc1_muxiang` | ✅ |
| 木桶 | `Village_Npc1_muzhiyuantong` | ✅ |
| 米袋 | `Village_Npc1_huangmi` | ✅ |

（对应旧 CSV 木箱/黄米/土豆/面包/饼干/圆桶语义；拼音 Prefab 名以工程为准。）

### 4.4 Npc1 vs 物品差异

| 项 | Npc1 | 六件物品 |
|----|------|----------|
| `requirePlayerOverlap` | **true**（默认） | **false** |
| Prefab | `Village_Npc1` | `Village_Npc1_*` |
| 走近 / E | 要（E 仍走 overlap） | 产品以远程点击为主；E 提示 OPEN |
| Actor | 通常 NPC1+雅尔 | 多半仅雅尔自语（对拍各 Prefab） |
| 远程开关依赖 | — | **已落地**；配场景即可用，不再卡「开关未施工」 |

### 4.5 与美术层边界

点击打的是 **带 Collider2D + RaycastListener** 的物体。  
`Map/Design/村民家1合层` 合层贴图 **≠** 交互实体；须在 Object 物体上设对准可点区域的碰撞，避免只点到合层、或合层挡住（Layer/排序注意）。

### 4.6 验收（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | InitScene → 进 `Village_HomeScene1` | 无 SceneEntity 未注册 / Interactive 初始化异常；`sceneObjs` 重扫 ≥7 |
| 2 | 走近 Npc1 点或按 E | 播 `Village_Npc1` |
| 3 | 门口远处点六物品 | 各播对应 Prefab；**无需走近** |
| 4 | 远处点 Npc1 | **不播** |
| 5 | Console | 无 Dialogue 加载失败 |

### 4.7 开放问题（已记入 OPEN）

| ID | 问题 | 施工默认 |
|----|------|----------|
| Q1 | 物品要不要 E 提示？ | **可不挂**；以远程点击为主 |
| Q2 | Collider 尺寸？ | 对齐精灵可点区，略大于美术热点即可 |
| Q3 | Object 是否还有未列子物体？ | 现网 **仅上述 7 个** |
| Q4 | Npc1 是否复用 HomeScene23 完整 Entity 结构？ | **建议仿 NpcChair 三件套**，勿只加一个脚本 |

---

## 5. 相关路径

| 资源 | 路径 |
|------|------|
| 场景 | `Assets/GameRes/Scenes/Village_HomeScene1.unity` |
| Manager | `Village_HomeScene1SceneManager.cs` |
| 对话 Prefab | `Assets/GameRes/Prefabs/Dialogue/Village_Npc1*.prefab` |
| 远程开关 | `RaycastListener.requirePlayerOverlap` |

---

## 6. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-08-20 | 初版：Object 七空壳；objRoot 对、sceneObjs 脏空；Prefab 映射齐；远程开关已可用 |

**文档路径**：`Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md`
