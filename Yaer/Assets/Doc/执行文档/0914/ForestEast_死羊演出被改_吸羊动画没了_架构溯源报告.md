# ForestEast 死羊演出被改 · 吸羊动画没了 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改** 场景 / Prefab / 对白图 / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene`（用户 Hierarchy 红箭头：`Map/Design/Interaction/Part3/死羊`）  
**现象**：觉得东郊死羊演出又被改了；检查时只看见静图尸体，史莱姆吸羊循环没了  
**产品期望**：走近羊尸仍能看见 **两套吸羊循环动画**；走进 Trigger 后对白+运镜仍是东郊金标准；打完才切坟。静图 `死羊` / `死羊_坟` / 石碑 **默认不许动**  
**不是**：走廊吃羊覆盖东郊；给静图加 Animator；顺手改倒树/洞口/地板  
**对照**：0913 走廊吃羊（共用 `CameraMoveTaskAction`）；0914 倒树换图 / 进洞贴底（都动过 `ForestEastScene.unity`）  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_死羊演出被改_吸羊动画没了_架构侦探提示词.md`  
**OPEN**：`OPEN_QUESTIONS.md` 本节 Q1～Q5  
**本会话限制**：未能在 Unity Play 对新档/旧档各截一帧；Play 验收留给拍板后

---

## 沟通摘要

### ① 结论一句话

用户点的 **`Part3/死羊` 从来就没有 Animator**，只是静图尸体；真正的吸羊循环在 **`Objects/1史莱姆吸动画1` 和 `2史莱姆吸动画3`**。现网这两套 Prefab **默认 Active、控制器完好**；工作区对 `ForestEastScene.unity` 的未提交 diff **没有改死羊坐标/Sprite/吸羊实例**。看不见循环 = 多半是 **看错节点**，或 **存档已 `hasTriggerSlime` / `hasCreateGrave`，`Start` 把动画关掉（设计如此，且全工程没有再 SetActive(true)）**。0914 倒树/贴底不是这套动画的根因。共用运镜质感变了是 **另一件事**，不要为「循环没了」去回滚 `CameraMoveTaskAction`。

### ② 原因（通俗）

Hierarchy 里「死羊」三个字的那个，从一开始就只是一张躺着的羊皮。两只史莱姆趴上去吸的，是旁边 Objects 里另外两个带 Animator 的 Prefab。进过吃羊对白或已经立坟的存档，一进东郊就会把那两个动画关掉，只剩静图或坟——这是写死的流程，不是 0914 把演出删了。镜头脚本 0913 为走廊改过平滑，顶多让推镜手感变，不会把循环物体关掉。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网（只读） |
|---|------|----------------|
| 1 | **新档**、未踩 Trigger，走到世界 X≈175 | **应**看见两套循环盖在尸体附近。本会话未 Play，须你本地验 |
| 2 | 不 Play，Hierarchy `Objects` | `1史莱姆吸动画1` / `2史莱姆吸动画3` 勾选；场景实例 **无** `m_IsActive:0` 覆盖 |
| 3 | 选中红箭头 `死羊` | **无 Animator**；**不要**当动画根去修 |
| 4 | 旧档已开战/已立坟再进东郊 | 无循环、只剩尸体或坟 = **设计** |
| 5 | 新档走进 Trigger | 金标准对白/推镜后黑幕刷怪，**此时关掉循环是正常的** |
| 6 | 先别再改 Part3 | git 未证明静图被误改 |

### ④ 程序补充

见下文。

---

## 1. 物体对照表

| 用户口头 | 现网路径 | 世界约 | Active（YAML） | 说明 |
|----------|----------|--------|----------------|------|
| 死羊（红箭头） | `Design/Interaction/Part3/死羊` `662229568` | Part3 (184.59,0) + (-9.09,-6.12) → **(175.50, -6.12)** | **1** | 仅 Sprite Order **5**，guid `ba2be86b…`。**无 Animator**。`deadSheepBody` |
| 死羊_坟 | 同级 | (-9.09, -5.81) 相对 | **0** | 立坟才开 |
| 石碑 | 同级 `石碑` | 相对 (7.56, 1.23) | **1** | 勿动 |
| 吸羊动画本体 | `Objects/1史莱姆吸动画1` | **(171.73, -5.84)** | Prefab `m_IsActive:1`，场景无 Inactive 覆盖 | Animator `SlimeLeft` `4722f93c…` Order **6** |
| 吸羊动画本体 | `Objects/2史莱姆吸动画3` | **(177.511, -4.786)** | 同上 | `SlimeRight` `d8548bab…` Order **6** |
| 走进触发 | `Objects/SlimeEatSheepStoryTrigger` | **(152.4, 0)** 盒 Offset.y=-2.82 Size (1, 9.16) | **1** | `ForestEastSceneSlimeEatSheep` Enter + **SingleUse** |
| 看尸对白 | `ViewDeadSheepStoryTrigger` | (175.72, 0) | **0** | 打完才开；勿和进场搞混 |
| 战斗怪 | `SlimeEatSheep_1/_2`（另两只在 slimeLogics） | Y≈轴上 | 场景覆盖 **`m_IsActive: 0`** | `start` 后 `TriggerSlime` 才开 |
| 父级图层 | `Interaction` SortingGroup **Default / -1**；`Near` **-2** | | | 吸羊在 Objects、**无 Group**、Order 6，相对 Group -1 应画在静图之上 |

`SlimeEatSheepStroy` 挂在 **Trigger** 上（不是挂在静图上）：`deadSheepBody` / `Grave` / `slimeEatAni_1/_2` / `ViewDeadSheep` / 四只 `slimeLogics`。

---

## 2. 调用链（进场循环如何出现 / 消失）

```
场景默认：死羊 Active=1；坟=0；两套吸羊 Prefab Active=1；战斗怪=0

SlimeEatSheepStroy.Start()
  关 ViewDeadSheepTrigger
  deadSheepBody.SetActive(true)     // 静图一直开
  关坟
  若存档 hasCreateGrave → ShowSheepGrave()
      关尸体、关两套动画、Destroy 怪、开看尸 Trigger
  else 若 hasTriggerSlime → TriggerSlime()
      开战斗怪、关两套动画
  else → 两套动画保持场景默认 ON  ★ 进场循环唯一来源

走进 Trigger → ForestEastSceneSlimeEatSheep
  金标准图：对白/运镜 → 黑幕 → SlimeEatSheepStoryAction("start")
      → OnSceneStoryTrigger(true) → BattleStoryStartOrEnd(true) → TriggerSlime()
      → 循环关掉（正常）

全工程检索：slimeEatAni_* 只有 SetActive(false)，没有 true。
关掉后本场景生命周期内不会再开。
```

存档键（东郊 Mgr **与走廊 Mgr2 字符串相同**）：

- `SlimeEatSheepStory_hasTriggerSlime`
- `SlimeEatSheepStory_hasCreateGrave`

`SimpleStoryTrigger.InitSomeEventState`：仅当对白名是 `ForestEastSceneSlimeEatSheep` 时 `InitBattleData`。未用过 → `enabled=true` 会把两键清 false；已用过 → 不清，Start 按档关动画。

东郊对白图 `start` 在黑幕节点之后（id 12），**不是一进图就开战**。F（图被改成进场就 start）现网 **不成立**。

---

## 3. git / 0914 场景 diff

| 文件 | 相对 HEAD / 历史 |
|------|-------------------|
| `Part3/死羊` 坐标、Sprite guid、Order、Active | 工作区 `ForestEastScene.unity` 大 diff（约 +534/-418）**过滤不到**这些字段 → **不是这次倒树/贴底误伤** |
| 两套吸羊 PrefabInstance 名/XY/`m_IsActive` | **无对应增删改**（diff 里出现实例 ID 只是邻近插入了倒树「外/光」YAML） |
| `1史莱姆吸动画1.prefab` / `2史莱姆吸动画3.prefab` | git status **未改**；Animator 引用存在 |
| `ForestEastSceneSlimeEatSheep.prefab` | status **未改** |
| `SlimeEatSheepStroy.cs` | status **未改** |
| `CameraMoveTaskAction.cs` | **工作区已改**（0913：`forceSnapToTarget: false` + InOutSine + 延后 Destroy） |

0914 施工说明只换倒树五层 GUID，明文未改死羊。倒树在地图右侧树桥，**盖不到** x≈175。

---

## 4. 复现矩阵（现网）

| 操作 | 期望 | 现网 |
|------|------|------|
| 新档走到 X≈175 | 两套循环 | YAML 支持；**Play 未在本会话做**（OPEN Q1） |
| Editor Hierarchy Objects | 两套勾选、Animator 在 | **是**（Prefab Active=1，无场景关） |
| 选中 `死羊` | 无 Animator | **是** |
| 旧档已触发/立坟 | 无循环 | **设计**（Start 关动画） |
| 新档进 Trigger | 先金标准再黑幕刷怪并关循环 | 图序：黑幕后 `start`；与 0913 金标准报告一致 |
| git 死羊/吸羊 | 若 0914 误改应有 hunk | **无** |

嫌疑裁定：

| 嫌疑 | 裁定 |
|------|------|
| **A 看错物体 + 存档已触发** | **主因（体感）** |
| **A′ 走廊 Mgr2 共用存档键** | **风险**（见 OPEN Q3）。若读档在 `InitBattleData(enabled=true)` 清键之后再次 Parse，可能把走廊进度写成东郊「已开战」 |
| **B 0914 场景误伤死羊/吸羊** | **否**（diff 无对应字段） |
| **C 共用运镜** | 可能改变 **推镜手感**；**不会** SetActive 掉循环。与「动画没了」分开 |
| **D 图层挡住** | 弱。吸羊 Order6、无 Group；Interaction Group -1。倒树在远处 |
| **E Prefab/Animator Missing** | **否**（controller 文件与 guid 在） |
| **F 一进场 start** | **否**（start 在黑幕后） |
| **G GroundCenter Y** | 洞内地板，羊在 x≈175 的 `GroundLeft_1` 一带。未证明错位到「看不见」 |

---

## 5. 方案对比与推荐

### 方案 Doc（推荐默认）：不改 Part3，用新档验收

1. **禁止**改 `死羊` / `死羊_坟` / 石碑。  
2. 新档走到 X≈175：Hierarchy 确认两套动画 Active，Game 能看见循环。  
3. 旧档：看存档两键；为 true 则说明「没了」是设计，写进验收说明即可。  
4. 运镜若觉得和记忆不同：另开票，用东郊图节点覆盖或 CameraMove 开关，**不要**为走廊再动东郊循环物体。

**这是最小修：零场景改动。**

### 方案 Key：拆存档键（仅当 Q1 新档也无循环且 Q3 坐实串档）

Mgr2 改为 `SlimeEatSheepStory2_hasTriggerSlime` 等独立键。  
**禁止**用走廊 Prefab/Action2 覆盖东郊。  
**禁止**给静图加 Animator。

### 方案 Cam：运镜开关（与循环无关，勿并进本票除非用户明确说镜头）

东郊图覆盖 forceSnap，或 CameraMove 加「东郊保持旧手感」开关。

### 方案 Rollback-B

仅当以后 diff **证明**死羊/吸羊被误改才回滚那几行。 **当前无证据，不做。**

---

## 6. 施工（仅拍板后）

若拍板 Doc：施工说明只写验收步骤 +「未改 Part3」。  
若拍板 Key：只改 Mgr2 键名并迁档说明。  
路径：`Assets/Doc/施工说明/0914/ForestEast_死羊演出_吸羊动画_施工说明.md`

### 验收

| # | 项 | 期望 |
|---|----|------|
| 1 | 新档 X≈175 | 两套吸羊循环可见 |
| 2 | 红箭头 `死羊` | 仍无 Animator、坐标/Sprite 与现网一致 |
| 3 | 进 Trigger | 金标准对白；黑幕后循环关、怪出 |
| 4 | 打完立坟 | 坟开、看尸 Trigger 开 |

---

## 7. 替代方案说明

| 路径 | 说明 |
|------|------|
| **权威 Doc** | 静图不是动画根；循环在 Objects；旧档关掉是设计 |
| **给尸体加 Animator** | 会毁金标准分层，**禁止** |
| **回滚整份 ForestEastScene** | 会丢掉倒树换图/贴底，且死羊本就没被那次 diff 改到 |
| **回滚 CameraMove** | 修的是推镜，不是循环消失 |

---

## 8. 参考路径

| 用途 | 路径 |
|------|------|
| 静图/坟 | `ForestEastScene` Part3 |
| 动画 Prefab | `Assets/Prefabs/SceneObject/1史莱姆吸动画1.prefab`、`2史莱姆吸动画3.prefab` |
| 逻辑 | `SlimeEatSheepStroy.cs` / `SlimeEatSheepStoryMgr.cs` / `SlimeEatSheepStoryAction.cs` |
| 走廊对照（勿覆盖） | `SlimeEatSheepStroy2` / `Mgr2`（**同存档键**） |
| 对白金标准 | `ForestEastSceneSlimeEatSheep.prefab` |
| 共用运镜 | `CameraMoveTaskAction.cs` |
