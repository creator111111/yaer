# Cursor Agent Prompt · ForestEast 死羊演出被改：史莱姆吸羊动画没了

> **角色**：先【架构侦探】只读查清「谁动了死羊 / 吸羊动画为何看不见」；**未拍板禁止施工**  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene`（用户 Hierarchy 红箭头：`Map/Design` 下 `Interaction/Part3` → **`死羊`**，同级 `石碑` / `死羊_坟`）  
> **现象（用户实测）**：  
> 1. **这里的死羊演出又被改了**  
> 2. 产品曾明确：**不要改这里**（东郊死羊/吸羊金标准，走廊吃羊只能对照、不能拿东郊当实验田）  
> 3. **刚刚检查：史莱姆吸羊的动画演出都没了**（只剩静图尸体，或镜头演出不对）  
> **产品期望（钉死）**：东郊走近羊尸处，**应仍能看见两只史莱姆在吸羊的循环动画**；走进 `SlimeEatSheepStoryTrigger` 后对白+运镜仍是东郊金标准；打完才切坟。静图 `死羊` / `死羊_坟` / 石碑 **恢复原样，不再被顺手改**  
> **不是**：用走廊 `VerdantCorridorSlimeEatSheep` 覆盖东郊图；重做吸羊 Prefab；把静图 `死羊` 当成「动画根」去加 Animator；顺手改倒树/洞口/地板  
> **近期必对照（勿当唯一真相）**：  
> - 0913 走廊吃羊：对白/时序/运镜丝滑 —— 改过 **共用** `CameraMoveTaskAction` / `CameraFollowPlayerActionTask`；走廊 Prefab；**明文要求勿改东郊图当实验**  
> - 0914 倒树换合层、进洞相机贴底、用户可能调过 `GroundCenter` Y —— 都动过 `ForestEastScene.unity`  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_死羊演出被改_吸羊动画没了_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末（**仅拍板后**）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 东郊那只死羊、史莱姆趴上去吸的动画，说了别动。现在动画没了，Hierarchy 里点开只看见静图「死羊」。查是谁改了场景/共用镜头代码/存档把动画关了，还是看错了物体。**先别再改 Part3。**

### 术语钉死（极易看错）

| 用户口头 | 现网物体 | 说明 |
|----------|----------|------|
| **死羊（Hierarchy 红箭头）** | `Map/Design/Interaction/Part3/死羊` | **静图尸体** Sprite，fileID `662229568`。**没有 Animator**。`SlimeEatSheepStroy.deadSheepBody` |
| **死羊_坟** | 同级 `死羊_坟` | 默认 **Inactive**。打完/存档已立坟才开 |
| **吸羊动画（演出本体）** | `Objects/1史莱姆吸动画1`、`Objects/2史莱姆吸动画3` | Prefab + Animator（`SlimeLeft`/`SlimeRight`）。`slimeEatAni_1` / `_2` |
| **走进触发器** | `Objects/SlimeEatSheepStoryTrigger` | 对白 `ForestEastSceneSlimeEatSheep`；`triggerType=Enter(1)` |
| **看尸对白** | `ViewDeadSheepStoryTrigger` | 默认 **Inactive**；打完才激活。勿和吸羊进场搞混 |

**用户点的是静图，不是动画 Prefab。** 动画不在 `Part3` 下。侦探报告必须写清：用户看到的「演出没了」是 **动画物体被关/被挡/被改**，还是 **本来就只该看 Objects 下那两套循环**、静图从未播过动画。

### 现网坐标与引用（YAML 预扫）

| 物体 | 约坐标 | Active | 其它 |
|------|--------|--------|------|
| `Part3` | (184.59, 0) | 1 | 父：`Design/Interaction`（**与 Near 同级**，不是 Near 子物体；Hierarchy 缩进易误读） |
| `死羊` | 相对 Part3 **(-9.09, -6.12)** → 世界约 **(175.50, -6.12)** | **1** | SR Order **5**；Sprite guid `ba2be86b8a63e96419048bab9552e1d9` |
| `死羊_坟` | 相对 **(-9.09, -5.81)** | **0** | |
| `1史莱姆吸动画1` | **(171.73, -5.84)**，父 `Objects` | Prefab 默认 **1**；场景实例 **无 m_IsActive 覆盖** | Order **6**；ctrl `SlimeLeft` guid `4722f93c…` |
| `2史莱姆吸动画3` | **(177.511, -4.786)** | 同上 | Order **6**；ctrl `SlimeRight` |
| `SlimeEatSheepStoryTrigger` | **(152.4, 0)**，盒 Offset.y=**-2.82** Size **(1, 9.16)** | 1 | `StoryPrefabName: ForestEastSceneSlimeEatSheep`；`SingleUseInArchive: 1` |
| `SlimeEatSheep_1/_2` 战斗怪 | Y 约 **-6.61 / -6.61** 一带 | 场景覆盖 **0** | 对白 `start` 后才刷 |

`SlimeEatSheepStroy` 序列化：

- `deadSheepBody` → `死羊`  
- `deadSheepGrave` → `死羊_坟`  
- `slimeEatAni_1/_2` → 上述两套吸羊 Prefab 实例  
- `deadSheepStoryTrigger` → `ViewDeadSheepStoryTrigger`  
- `slimeLogics` 四只史莱姆  

`Interaction` 上有 **SortingGroup Order = -1**；`Near` SortingGroup Order = **-2**。吸羊动画在 `Objects` 下、**无 SortingGroup**、Order 6。侦探须用 Scene/Game 判定是否被近景合层挡住。

### C# 关键行为（预扫 · 须对照源码）

`SlimeEatSheepStroy.Start()`：

1. 关 `ViewDeadSheepStoryTrigger`  
2. **`deadSheepBody.SetActive(true)`**（静图尸体一直开）  
3. 关坟  
4. 若存档 **已立坟** → `ShowSheepGrave()`：**关尸体、关两套吸羊动画**  
5. 否则若存档 **已触发史莱姆** → `TriggerSlime()`：**关两套吸羊动画**，开战斗怪  

`TriggerSlime` / `ShowSheepGrave` **只会 `slimeEatAni_*.SetActive(false)`**。全工程 **没有任何 `SetActive(true)` 再打开吸羊动画**。  
→ 进场循环全靠场景里这两物体 **默认 Active=1**。一旦被存档分支关掉、或实例被改成 Inactive、或 Animator/图丢了，**用户就只剩静图「死羊」**，体感就是「动画没了、死羊演出被改了」。

`SlimeEatSheepStoryMgr`：`start` → 开战；`createGrave` → 立坟。存档键 `SlimeEatSheepStory_hasTriggerSlime` / `_hasCreateGrave`。

`SimpleStoryTrigger.InitSomeEventState`：仅当对白名是 `ForestEastSceneSlimeEatSheep` 时 `InitBattleData`。侦探须核 **enabled / 存档** 会不会一进场景就走 `TriggerSlime`。

### 「又被改了」嫌疑优先级

| # | 嫌疑 | 为何像 | 查法 |
|---|------|--------|------|
| **A（主 · 看错物体 + 存档已触发）** | 用户盯着 `Part3/死羊`；Play 用的档已经吃羊/立坟，`Start` 把吸羊动画关掉 | 静图本来就在；动画在 Objects | 新档 vs 旧档；Play 时 Hierarchy 看 `1史莱姆吸动画1` Active |
| **B（场景误伤）** | 0914 改 `ForestEastScene.unity`（倒树 GUID、相机、地板）时连带改了 `死羊` 坐标/Sprite、或吸羊 Prefab 实例 Transform/Active | 用户说「这里又改了」 | `git diff` / 历史：`死羊`、两套吸羊 PrefabInstance、`ForestEastSceneSlimeEatSheep.prefab` |
| **C（共用运镜误伤东郊「演出」）** | 0913 为走廊改了 **`CameraMoveTaskAction`（forceSnap/Ease/Destroy 时机）** 和 Follow 默认不 forceSnap | 用户把「镜头演出」和「吸羊动画」混说 | 新档走触发器：循环动画在不在 vs 推镜质感变了 |
| **D（图层挡住）** | 倒树/近景 Sorting 或 `Interaction` SortingGroup 把 Order6 的吸羊动画盖住 | 物体 Active 但 Game 看不见 | Scene 孤立选中两套动画；比 Near/Interaction Group |
| **E（Prefab/Animator 坏）** | `1史莱姆吸动画1.prefab` / controller / clip Missing | 只显示第一帧或粉图 | Inspector Animator、Console Missing |
| **F（Trigger 一进场就 `start`）** | 对白图被改成一进就 `SlimeEatSheepStoryAction("start")`，黑幕刷怪并把动画关掉 | 0913 禁止改东郊图，但要 **diff 东郊 Prefab** | 对白图 connections vs 0913 报告里的金标准序 |
| **G（Y 被地板拖偏）** | `GroundCenter` 降了，人/相机相对羊；动画还在原 Y，看起来「没了」或错位 | 用户刚要调爬行高度 | 世界 Y：尸体 -6.12 vs 动画 -5.84/-4.79 vs 人脚 |

### 复现矩阵（侦探须填现网）

| 操作 | 期望 | 现网 |
|------|------|------|
| **新档**、尚未走进 Trigger，走到世界 X≈175 | 看见 **两套循环吸羊动画**盖在尸体附近，不是只有一张死羊静图 | |
| Editor 不 Play，Hierarchy `Objects` | `1史莱姆吸动画1`、`2史莱姆吸动画3` 勾选、Animator 在播预览 | |
| 选中用户红箭头 `死羊` | **无 Animator**；勿当作动画根去「修」 | |
| 旧档（已触发/已立坟）再进东郊 | 无吸羊循环；尸体或坟 —— **这是设计**，须与「bug 没了」分开写 | |
| 新档走进 Trigger | 先对白/推镜（金标准），黑幕后刷怪；此时动画被关掉 **正常** | |
| `git log/diff` 上表文件 | 列出 0913/0914 是否真改了死羊/吸羊实例/东郊对话图 | |

### 侦探须回答

1. 「吸羊动画没了」精确指：进场循环没了、对白运镜没了、还是打完后的静图/坟？用新档/旧档各拍一张。  
2. `1史莱姆吸动画1` / `2史莱姆吸动画3` 在 **Play 当帧** 是 Active 还是被 `TriggerSlime`/`ShowSheepGrave` 关掉？存档两键值。  
3. `Part3/死羊` 相对 git 是否真被改（坐标/Sprite/Order/Active）？**若没改，写明用户看的是静图，动画从未挂在这节点上。**  
4. 0913 共用 `CameraMoveTaskAction` 是否让东郊「演出」变了？与「循环动画消失」是否两件事？  
5. 0914 `ForestEastScene.unity` 倒树/相机 diff 是否误包含死羊/吸羊？  
6. 方案 ≥2 + **推荐最小修**。硬约束：  
   - **默认禁止改 `Part3/死羊`、`死羊_坟`、石碑**（除非 diff 证明它们被误改，只回滚误改）  
   - **禁止**用走廊 Prefab/Action2/Mgr2 覆盖东郊  
   - **禁止**给静图尸体加 Animator 冒充吸羊  
   - 若只是旧档已触发：施工是 **文档+验收说明**，不是改场景  
   - 若共用运镜伤了东郊：优先 **东郊图上的节点覆盖** 或 **CameraMove 加开关**，不要为走廊再改坏东郊循环动画

### 必读

1. `SlimeEatSheepStroy.cs` / `SlimeEatSheepStoryMgr.cs` / `SlimeEatSheepStoryAction.cs`  
2. `SimpleStoryTrigger.cs`（`ForestEastSceneSlimeEatSheep` → `InitBattleData`）  
3. 场景：`ForestEastScene.unity` 上表物体  
4. Prefab：`Assets/Prefabs/SceneObject/1史莱姆吸动画1.prefab`、`2史莱姆吸动画3.prefab`  
5. 对白：`ForestEastSceneSlimeEatSheep.prefab`（金标准，0913 报告有图序）  
6. 共用：`CameraMoveTaskAction.cs`、`CameraFollowPlayerActionTask.cs` + 施工说明 `0913/VerdantCorridor_史莱姆吃羊_运镜丝滑_*`、`拉回锁定消闪_*`  
7. 0914：倒树换合层 / 进洞贴底 施工说明（确认 `ForestEastScene.unity` 改动范围）  
8. 用户 Hierarchy 截图 + 本提示词  

### 报告必须包含

- 一张「物体对照表」：静图 / 两套动画 / Trigger / 坟 / 对白 Prefab  
- git：上述文件相对近期是否变更（有则贴 hunk 摘要）  
- 新档 Play：动画 Active 与否、Console 有无 Missing  
- 根因一句话 + 方案表（回滚误改 / 只恢复动画 Active / 运镜开关 / 仅说明存档）  
- OPEN_QUESTIONS：若无法区分「用户旧档」与「场景坏了」

---

## 施工员（仅侦探报告拍板后；本阶段不要跑）

> 按报告最小文件列表修。  
> **禁止**改 `Part3/死羊` 除非报告写明那是误改回滚。  
> **禁止**改走廊吃羊逻辑来「顺便修」东郊。  
> 施工说明：`Assets/Doc/施工说明/0914/ForestEast_死羊演出_吸羊动画_施工说明.md`  
> 验收：新档走到 X≈175 必须看见吸羊循环；走进 Trigger 对白仍金标准；`死羊` 静图与石碑与拍板前一致（或已回滚误改）。
