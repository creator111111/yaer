# Village_HomeScene23：右门回村 + Npc4 对话组件 — 架构溯源报告

**文档版本**：v1.0（2026-08-04）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 场景 / CSV**）  
**范围**：① **右门**与村里双向进出 `Village_KenMuNi1`；② **`Object/Npc4`** 按 Npc1 仿照绑齐对话组件（为后续对话铺路，不写死台本）。  
**关联**：进屋无主角见 `0804/Village_HomeScene23_进屋主角不出现_*`（**已施工**：专用 Manager / Build / 左门回村等）——本期是在其上**改右门主链 + 补 Npc4**，勿整屋重做。  

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**右门现网未闭环：`RightDoor` 的 `SceneChangeDoor` 禁用且 `NextSceneName` 空；进屋落点仍绑 `LeftBorn`（左门方案残留）。村里 `House_Npc4`↔回程 `EnterPos` 已有。Npc4 只是 `Object` 下的空壳（Transform+Sprite），缺 EntityControl / Interactive / Body / SimpleStoryTrigger；磁盘已有可占位的 `Dialogue/HomeScene1Npc4.prefab`。**

---

## ② 原因（生活类比）

屋里现在开的是**左边消防通道**（左门已指向回村）；产品要改走**右边正门**，但右门锁着、门牌空白。Npc4 只摆了纸板人像，没有感应器和按钮——要对话得先装上和 Npc1 一样的「售货机」零件。合层 `Design` 里的画不是交互对象，别往画上拧螺丝。

### 前置依赖（进屋报告 · 已落地部分）

| 项 | 现网 | 本期是否还挡路 |
|----|------|----------------|
| `Village_HomeScene23SceneManager` + `nowSceneName` | 已有、对齐文件名 | 否 |
| Build / `SceneName` 常量 | 已有 | 否 |
| 村门 `House_Npc4` → HomeScene4 | 已有 | 否 |
| 村回程 `lastScene: Village_HomeScene23` | 已有（门外≈62.4,-6.1） | 否（坐标可验是否贴 House_Npc4） |
| 室内 `EnterPos` | **有，但指向 LeftBorn** | **是**——右门方案须改为 RightBorn |
| 玩家可生成 / 可玩 | 已按左门方案修过 | 测右门/Npc4 前确认仍可进 |

→ 本期**不必**重做 Manager/Build；**必须**把门+落点切到右门，并补 Npc4 组件。

---

## ③ 用户需要做什么

### 拍板（OPEN）

1. **Npc4 的 `StoryPrefabName`**：暂用现成 **`HomeScene1Npc4`**（根目录已有），还是新建 `Village_HomeScene23_Npc4`？  
2. **左门**：永久禁用 / 保持备用出口？（默认：**禁用**，免双门）  
3. **进屋落点**：确认改为 **`RightBorn`**？  
4. RightDoor 触发盒世界坐标偏中（见下）——是否需美术/策划确认门口位置后再改 Transform？

### 验收清单

**右门**  
1. 村 → `House_Npc4` 进屋 → 落在 **RightBorn** 附近可走  
2. 走 **RightDoor** 回村 → 落在 House_Npc4 门外  
3. 左门不触发换场（若拍板禁用）  
4. Console 可有 `[SceneChangeDoor]` / 无空 NextSceneName 误触  

**Npc4**  
1. 靠近出 E；按 E → `TriggerStory`（有 Prefab 则出对话；占位 Prefab 至少不 NRE）  
2. 不点到 `Design` 合层装饰  
3. DialogDebug 可拖对应 Prefab 单测  

---

## ④ 给程序看的补充

### 4.1 右门闭环表

| 环节 | 现状 | 期望（右门主链） | 是否必须改 |
|------|------|------------------|------------|
| 村 `House_Npc4.NextSceneName` | `Village_HomeScene23` | 保持 | 否 |
| 村 `EnterPos` `lastScene=Village_HomeScene23` | 有 → 门外 Transform | 保持；验收贴门 | 否（可选校正坐标） |
| 室内 `RightDoor` `SceneChangeDoor` | **m_Enabled=0**，`NextSceneName` **空** | **启用**，`Village_KenMuNi1` | **是** |
| `TriggerWhenMoveIn` | 0 | 与左门/样板一致即可（现 0=需交互或走到触发逻辑以组件为准） | 按样板 |
| 室内 `LeftDoor` | **已启用**，`NextSceneName=Village_KenMuNi1` | **建议禁用**（或清空 Next） | **建议是** |
| 室内 `EnterPosConfig` | `KenMuNi1` → **LeftBorn** `(-24.12,-1.3)` | → **RightBorn** `(-3.62,-1.3)` | **是** |
| RightDoor 世界位置 | MapRight(28.8)+local(-29.53)≈**-0.7**，偏中 | 应在右侧门口可踩 | **建议验收/校正** |
| `nowSceneName` | `Village_HomeScene23` | 已对齐，回程 LastScene 可匹配 | 否 |

**现网能否「村→右门进屋→右门回村」？**  
不能。缺：右门启用+目标场景；进屋落点仍左；左门反而通。

### 4.2 Npc4 vs Npc1 Diff（必出）

| 组件 / 配置 | Npc1（仿照源） | Npc4（现状） | 是否必须 |
|-------------|---------------|--------------|----------|
| 父节点 | `Object`（=`objRoot`） | `Object` | 已对 |
| Active | **0（场景里关着）** | 1 | Npc4 保持开；复制时别抄 Active=0 |
| SpriteRenderer | 有 | **仅有此+Transform** | 外观可留 |
| SceneEntity（guid e394…） | 有 | **无** | **是** |
| EntityControl `entityType=NPC`(3) | 有，`canTouch=1`，keyTipsY=3.5 | **无** | **是** |
| ComponentSystem + InteractiveComponent | 有 | **无** | **是** |
| Body 触发碰撞 | 有子物体 | **无** | **是** |
| SimpleStoryTrigger | 有，`HomeScene1Npc1`，Click | **无** | **是** |
| 对话 Prefab 磁盘 | `Dialogue/HomeScene1Npc1.prefab` | 可用 **`HomeScene1Npc4.prefab`（已存在）** | 绑名即可；或新建村专用名 |
| 合层 Design | — | Design 下为美术，**无**同名交互 npc | **勿绑合层** |

**结论**：Npc4 = **空壳**，不是「只差 StoryPrefabName」；须整套仿 Npc1（或复制 Npc1 改名改图改 Story 名）。  
**禁止**挂 `HomeScene1Xiaer`。

`HomeScene1Npc4.prefab` 内容为龙宫风 NPC4 短对白（「刚才不要闹了就好了…」）——可作接线验收占位；正式村台词另开 CSV/Prefab（开放问题）。

### 4.3 施工清单（只建议 · 最小化）

#### A. 右门闭环（场景操作）

1. 选中 `Map/MapRight/RightDoor`：  
   - 启用 `SceneChangeDoor`  
   - `NextSceneName = Village_KenMuNi1`  
2. `Map/MapLeft/LeftDoor`：禁用 `SceneChangeDoor`（或清空 Next）——与产品「只走右门」一致。  
3. SceneManager `EnterPosConfig`：将 `Village_KenMuNi1` 的 `pos` 从 LeftBorn 改为 **RightBorn**（`rightBornTsf`）。  
4. Play：从村进屋应落在 RightBorn；从右门回村应落在村 EnterPos。  
5. 若踩不到门：检查 RightDoor 碰撞盒世界位置，对齐右侧门口（必要时改 localPos）。

#### B. Npc4 对话组件（仿 Npc1）

1. 在 `Object` 下**复制 `Npc1`**（含完整组件树），改名为工作用体，或把组件迁到现有 `Npc4`。  
2. 设 Active=1；外观用现 Npc4 的 Sprite（或保留复制体图再换）。  
3. `SimpleStoryTrigger.StoryPrefabName` = **`HomeScene1Npc4`**（占位）或拍板后的新名。  
4. `triggerType=Click`；`entityType=NPC`；Body Trigger 罩住角色。  
5. **不要**改 `Design/...` 合层。若合层挡点击：降合层排序 / 关合层 Collider（若有）。  
6. DialogDebug 拖 Prefab 冒烟；再进屋按 E。

**不改**：台本正文（除非占位不够验）；换场总管线；NpcXiaer 龙宫脚本（本期可不碰）。

### 4.4 对话资源约定

```
TriggerStory(StoryPrefabName)
  → Assets/GameRes/Prefabs/Dialogue/{StoryPrefabName}.prefab   // 根目录
```

| 候选名 | 磁盘 | 说明 |
|--------|------|------|
| `HomeScene1Npc4` | **有** | 接线验收最快 |
| `Village_HomeScene23_Npc4` | 无 | 正式村内容时再建 |

### 4.5 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | Npc4 StoryPrefab 正式名？ | 先 `HomeScene1Npc4` 占位；正式另开 |
| Q2 | 左门永久禁用？ | **是** |
| Q3 | nowSceneName 纠正是否本轮？ | **已完成**，不必再做 |
| Q4 | 进屋无主角是否本轮前置？ | **已施工**；本轮只切右门+Npc4 |
| Q5 | RightDoor 位置是否要先校正？ | 验收踩门；踩不到再改 Transform |

---

## 施工员下一轮最小化清单（建议）

1. 右门启用 + Next=KenMuNi1；左门关；EnterPos→RightBorn  
2. Npc4 按 Npc1 绑齐组件 + StoryPrefabName 占位  
3. 双向换场 + 按 E 触发验收  
