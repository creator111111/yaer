# Village_HomeScene1 — 进屋黑屏与「场景对象未注册」— 架构溯源报告

**文档性质**：架构侦探产出（只读溯源；**本阶段不改资产/代码**）  
**日期**：2026-08-20  
**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene1.unity`  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- 提示词：`Assets/Doc/提示词/0820/Village_HomeScene1_进屋黑屏与未注册_架构侦探提示词.md`
- 先例：`Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md` §6
- 配置前史：`Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md`（当时为 Sprite 空壳；**现网已半套施工**）
- 代码：`BaseSceneEntityLogic.cs`、`ComponentSystemMono.cs`、`ComponentSystem.cs`、`SceneEntityComponentGSM.cs`

**测试现象（开发者）**：进场景 Game **黑屏**；Console `NullReferenceException` @ `ComponentSystem.InitComponents` + 多条「场景对象未到场景管理器注册并初始化=>Xxx」。

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**黑屏因 Object 下多数实体的 `ComponentSystemMono.componentsList` 含 `None`（`fileID: 0`），同步进运行时后 `InitComponents` 对 null 调 `Init` 抛 NRE，打断 SceneManager 初始化；「未注册」是连带告警（饼干列表干净已成功，其余未跑完 `OnInit`），不是「没绑 GSM」主因。**

---

## ② 原因（生活类比）

| 角色 | 类比 |
|------|------|
| `componentsList` 里的 **None** | 总电闸接线排上插了一根**空插头** |
| `InitComponents` NRE | 合闸时摸到空插头 → **总闸跳掉** |
| Game 黑屏 | 整栋楼灯灭（相机/黑幕/玩家链路没跑完） |
| 「未注册=>面包/Npc1/…」 | 停电后巡楼：这些房间**没办完入住手续**（`isInit` 仍 false） |
| 饼干未出现在未注册列表 | 它是货架上**第一个**，且列表无空槽，入住成功 |

**不是**：CSV Speaker「1」导入中止（16:58，早于 17:34 进场景）；也不是「只忘了拖 sceneObjs」（`objRoot` 已指 Object，且已有 7 个 SceneEntity）。

---

## ③ 用户需要做什么（检查清单）

> **先清空槽，再进 Play。** 不要先怀疑「场景管理器没挂上」。

### 优先：清掉 6 处 `componentsList` 的 None

在 `Village_HomeScene1` → Hierarchy `Object` 下，选中物体 → `ComponentSystemMono` → **Components List**：

| 物体 | StoryPrefabName | 现网 componentsList | 动作 |
|------|-----------------|---------------------|------|
| **饼干** | `Village_Npc1_bingan` | 仅 Interactive（**无 None**） | 不必改列表；作对照样板 |
| **面包** | `Village_Npc1_mianbao` | Interactive + **None** | **删掉 None**（第一崩溃嫌疑） |
| **木桶** | `Village_Npc1_muzhiyuantong` | Interactive + **None** | 删 None |
| **米袋** | `Village_Npc1_huangmi` | Interactive + **None** | 删 None |
| **木箱** | `Village_Npc1_muxiang` | Interactive + **None** | 删 None |
| **土豆** | `Village_Npc1_tudou` | Interactive + **None** | 删 None |
| **Npc1** | `Village_Npc1` | Interactive + **None** | 删 None |

YAML 证据（节选）：

- 面包 L1941–1943：`9200000039` + `{fileID: 0}`
- Npc1 L1434–1436：`9200000009` + `{fileID: 0}`
- 饼干 L1688–1689：仅 `9200000024`（干净）

可选：在 Inspector 点组件系统的刷新（若有 `RefreshComponents`）会 `RemoveAll(null)`；**手删 None 即可**。

### 其次：确认绑定（现网已大体正确，勿当黑屏主因）

| 项 | 现网 |
|----|------|
| Manager | `Village_HomeScene1SceneManager`（勿换龙宫） |
| `objRoot` | `Object`（fileID `442106408`） |
| `sceneObjs` | 已挂 7 个 SceneEntity（非空）；运行时仍会按 objRoot 重扫 |
| Interactive / EntityControl / Body+Raycast | 7 物均已挂 |
| 物品 `requirePlayerOverlap` | 六物品为 `0`（远程）；Npc1 Body 为 `1`（近距）— **与黑屏无关** |

### 验收（对齐提示词）

| # | 操作 | 通过 |
|---|------|------|
| 1 | InitScene → 进 `Village_HomeScene1` | **不黑屏**；无 `InitComponents` NRE |
| 2 | Console | **无**「未注册=>Npc1/面包/…」 |
| 3 | 能看见室内与玩家 | 相机/光照正常 |
| 4 | （次要）Npc1 近距 / 物品远程对话 | 不黑后再验 |

**禁止**：改龙宫 `HomeScene1`；把本期绑进远程点击产品改需求；只清 `sceneObjs` 却留着 None。

---

## ④ 给程序看的补充

### 4.1 时间线与因果（对拍开发者 Console）

| 时间 | 日志 | 判定 |
|------|------|------|
| 16:58:37 | `[DialogueCsvGraphBuilder] Speaker「1」…导入已中止` | **无关**：编辑器导入工具；非进场景帧 |
| 17:34:15 | `NullReferenceException` → `ComponentSystem.InitComponents()` | **第一刀** |
| 17:34:15 | `未注册=>Npc1` / `面包` / `土豆` / `木箱` / `木桶` / `米袋` | **连带**（截图未列饼干 → 与 YAML 一致） |

**因果链：**

1. `SceneEntityComponentGSM.OnInit` → `objRoot.GetComponentsInChildren<SceneEntity>`  
2. Hierarchy 顺序：`饼干 → 面包 → 木桶 → 米袋 → 木箱 → 土豆 → Npc1`  
3. **饼干** `OnInit` 成功（`isInit=true`）  
4. **面包** `OnInit` → `ComponentSystemMono.OnInit` → `SyncComponentsToSystem` 把 **null** 同步进 `ComponentSystem` → `InitComponents` 对 null 调 `component.Init(this)` → **NRE**  
5. 异常向上打断 GSM / SceneManager 后续初始化 → **黑屏**  
6. 面包及之后实体未置 `isInit` → 下一帧 `Start` 刷「未注册」

> 生活类比对齐 HomeScene23 §6：那时是门缺 Interactive **硬抛**；这次是列表里 **空引用** 软炸到同一类「整场景 Init 中断」。

### 4.2 「未注册」机制（症状）

```text
BaseSceneEntityLogic.OnInit
  → InitComponentSystem() → componentSystem.OnInit()
  → isInit = true          // 仅成功跑完才置位

BaseSceneEntityLogic.Start
  → if (!isInit) Log.Error("场景对象未到场景管理器注册并初始化=>" + name)
```

谁应调用 `SceneEntity.OnInit`：`SceneEntityComponentGSM.OnInit` 对 `sceneObjs` 逐个调用（且运行时按 `objRoot` 重扫）。  
**现网绑定已通**；未注册 ≠ 漏挂 SceneEntity，而是 **OnInit 中途抛异常 / 后续未执行**。

### 4.3 NRE 代码路径（责任点）

`ComponentSystemMono.OnInit`：

1. `SyncComponentsToSystem()`：**不跳过 null**，`AddComponent(null)` 使用默认 priority 写入运行时列表  
2. `componentSystem.InitComponents()`：对每个元素 `component.Init(this)` → **null → NRE**

`RefreshComponents()` 虽有 `RemoveAll(item => item == null)`，但 **OnInit 不调用它**；空槽只存在于序列化 List。

**第一责任物体（现网）**：**面包**（Hierarchy 第二个、且带 None）。清完所有 6 处 None 后，即使顺序变化也不再炸。

门 / Map：`LeftDoor` 的 `SceneChangeDoor` 为 **禁用**且 `componentsList: []`；**不在** `objRoot=Object` 子树，不是本次 Object 初始化 NRE 源。勿与 HomeScene23「启用左门缺 Interactive」混为一谈（机制同类、物体不同）。

### 4.4 与前史报告对拍

| 报告时点 | Object 状态 |
|----------|-------------|
| 《Object 全量配置》侦探时 | Sprite 空壳、无 SceneEntity |
| **本次（现网 YAML）** | 已挂 SceneEntity + 三件套 + StoryPrefab + sceneObjs 七槽；**半套施工留下 6 个 None** |

远程 overlap 开关已配上，**不是**黑屏主因。

### 4.5 最小修复顺序（施工员）

1. **场景**：删掉上述 6 个物体 `componentsList` 的 None（保留已有 Interactive 引用）  
2. Play：确认无 NRE、无未注册、不黑屏  
3. （可选加固，OPEN）`SyncComponentsToSystem` / `InitComponents` 跳过 null 并打 Error——治标，**不能替代清资产**  
4. 不黑后再验对话 / 远程点击

### 4.6 OPEN

| ID | 问题 | 建议默认 | 状态 |
|----|------|----------|------|
| Q1 | 是否给 `InitComponents` / `SyncComponentsToSystem` 加 null 防护？ | **建议加**（防再半套施工黑屏）；仍须清现网 None | 待确认 |
| Q2 | None 从何而来（复制粘贴多一格 / Inspector 误加）？ | 施工规范：加完 Interactive 后目视 List 无空槽 | 待确认 |
| Q3 | 黑屏是 UI 黑幕未关还是相机未跟？ | 优先按「Init 中断」修；修好后若仍暗再查 Fade/Camera | 待验收 |

---

## ⑤ 验收回写（施工后填）

| # | 结果 |
|---|------|
| 不黑屏 / 无 InitComponents NRE | |
| 无未注册 Error | |
| 室内可见 | |
| Npc1 / 物品交互（次要） | |
