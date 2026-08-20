# Cursor Agent Prompt · Village_HomeScene1 · Npc1 走近无 E / 无法对话（对照 HomeScene23）

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **现象（开发者已测）**：`Village_HomeScene1` 的 **Npc1** 走近后 **没有交互 E 提示**，也无法对话。  
> **对照场景**：`Assets/GameRes/Scenes/Village_HomeScene23.unity` 里能正常对话的 NPC（如 `Npc1` / `NpcChair` / 其它已验收 NPC）。  
> **现场 Hierarchy（截图）**：`Village_HomeScene1 / Object / Npc1` 已有 `Components/Interactive`、`Clds` 结构（红箭头指向 Npc1）。  
> **本阶段**：只读扫描 + 写对照溯源报告，**不施工**  
> **前置**：若黑屏/未注册未修干净，须先说明「是否仍因 isInit 失败导致永远无 E」；再比组件差异。

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. HomeScene1 的 Npc1 靠近都没有 E，更谈不上对话。  
2. 和 `Village_HomeScene23` 里的 NPC **有啥区别**？  
3. 差在哪一项配置/组件，补上就能出 E？

### E 提示现网链路（预扫，须对拍）

```
玩家走近 → 玩家/NPC InteractiveCollider bounds 相交
  → PlayerLogic.checkCanAddKeyTipsInOtherEntity
  → GetFirstCanTouchEntiy（仅相交实体）
  → entityControll.canTouchWithPlayer == true
  → AddKeyTipsNode / 显示 E
按 E → OnInteractive → SimpleStoryTrigger → TriggerStory(Prefab)
```

任一环断 → **无 E**。无 E 通常还不是「对话 Prefab 坏了」优先项。

### 预扫嫌疑对照表（可证伪）

| 检查项 | HomeScene23 可对话 NPC（期望） | HomeScene1 Npc1（嫌疑） |
|--------|-------------------------------|-------------------------|
| 黑屏/未注册已修？ | 正常 isInit | 若仍有 `componentsList` None / 未 OnInit → **整实体死** |
| `SceneEntity` + 在 `objRoot=Object` | ✅ | 对拍 |
| `BaseEntityControll` | `entityType=NPC`，`canTouchWithPlayer=true`，引用齐 | 是否缺 / false / 引用断 |
| `InteractiveComponent.interactiveCollider` | 指 `Clds/Body` BoxCollider2D | 是否空、指错 |
| Body Collider | Is Trigger；尺寸罩人；**Z≈0** | Z 非 0 / 盒太小 / 不 Trigger |
| 玩家与 Body 能否 overlap | 样板能 | 站位/缩放/纵深导致永远不相交 |
| `SimpleStoryTrigger` | 有；StoryPrefabName 合法 | 缺？仍空？错名？ |
| StoryPrefabName | 如可用名 | 应为 `Village_Npc1`（磁盘有 Prefab） |
| Layer / isCanTouchWithOther | 场景允许交互 | GSM 是否关交互 |

生活类比：HomeScene23 的 NPC 是装好感应器的门铃；HomeScene1 的 Npc1 可能外壳像（有 Components/Clds），但感应器没接线、开关关着，或人根本没走进感应圈。

### 必读

- `Assets/Doc/执行文档/6月/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md` §3～5（三件套 + E 链路）
- `Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md`（None 槽 / isInit）
- `Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md`
- `Assets/Doc/执行文档/6月/0601/Village_HomeScene23_NPC对话配置_执行说明.md`
- `PlayerLogic.checkCanAddKeyTipsInOtherEntity` / `GetFirstCanTouchEntiy`
- `BaseEntityControll.AddKeyTipsNode`
- 场景对拍：  
  - `Village_HomeScene1.unity` → `Object/Npc1`  
  - `Village_HomeScene23.unity` → 至少一个**确认能出 E**的 NPC（优先同结构的 `Npc1` 或 `NpcChair`）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/6月/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md
@Assets/Doc/执行文档/6月/0601/Village_HomeScene23_NPC对话配置_执行说明.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
@Assets/Scripts/Game/GameRuntime/Entities/Base/BaseEntityControll.cs
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/Village_HomeScene23.unity
@Assets/GameRes/Prefabs/Dialogue/Village_Npc1.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改场景、Prefab、代码。只读扫描 + 写对照溯源报告。

---

## 背景

1. HomeScene1 的 Npc1：走近无 E、无法对话。
2. 开发者问：和 HomeScene23 的 NPC 有何区别？
3. Hierarchy 显示 Npc1 已有 Components/Interactive、Clds——须逐项对拍，勿凭「看起来有结构」结案。
4. 本期只查明无 E / 无对话的配置差异与最小补齐清单。

---

## 必查

### A. 先排除「实体根本没活」

- Play 进 HomeScene1：Console 是否仍有 Npc1「未注册」或 InitComponents NRE？  
- `isInit` 是否 true？若否：无 E 是黑屏案连带，报告写清「先修注册/None，再谈 E」。  
- 饼干等物品远程点击是否已能用？（对照：物品活、Npc1 死 → 更像 Npc1 近距/EntityControl 差异）

### B. 逐项对照表（必出）

选 HomeScene23 **一个能出 E 的 NPC** 为样板，与 HomeScene1 `Npc1` 并排填：

| 字段/组件 | HomeScene23 样板值 | HomeScene1 Npc1 | 是否一致 |
|-----------|-------------------|-----------------|----------|
| SceneEntity | | | |
| BaseEntityControll.canTouchWithPlayer | | | |
| entityType | | | |
| Interactive → interactiveCollider | | | |
| Body BoxCollider2D IsTrigger / size / 世界 Z | | | |
| RaycastListener + 是否在 raycastListeners | | | |
| requirePlayerOverlap（Npc 应 true） | | | |
| SimpleStoryTrigger 有无 | | | |
| StoryPrefabName | | | |
| ComponentSystemMono.componentsList 有无 None | | | |
| 在 objRoot 子树 / 被重扫进 sceneObjs | | | |

### C. 无 E 断在哪一环

用现网逻辑钉死第一条失败条件：

1. bounds 永不相交（盒太小/偏移/Z）  
2. GetFirstCanTouch 返回 null  
3. canTouchWithPlayer=false 或 entityControll 空  
4. KeyTips 预制加载失败  
5. isCanTouchWithOther=false  
6. 实体未 Init  

Play 下若只能静态读：写清「最可疑的前 3 项」与如何一眼验。

### D. 对话层（有 E 之后）

即使将来出了 E，对拍：

- StoryPrefabName 是否 `Village_Npc1`（磁盘存在）  
- TriggerType  
- 勿与龙宫 `HomeScene1Npc1` 路径混淆  

无 E 时对话问题标为次要。

### E. 最小修复建议（不施工）

按对照表列出「只改 HomeScene1 Npc1」的勾选清单；禁止改 HomeScene23 去迁就。

### F. 验收

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进 HomeScene1，走近 Npc1 | 出现 **E** |
| 2 | 按 E | 播 `Village_Npc1` |
| 3 | HomeScene23 原 NPC | 不回归 |
| 4 | Console | 无未注册/NRE |

---

## 侦探任务

1. **结论一句话**：无 E 因与 HomeScene23 NPC 差在某某（或仍未 Init）。  
2. **完整对照表**。  
3. **E 链路断点**。  
4. **用户检查清单**（Inspector 逐项）。  
5. OPEN：Body 尺寸标准；Z 必须 0 是否强制。  
6. **禁止**：改资产；把锅甩给「远程点击开关」却不查 canTouch/Collider；只说「缺对话 Prefab」却忽略无 E。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Village_HomeScene1_Npc1无E对照HomeScene23_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：门铃外壳有了，感应圈没接通）  
③ 用户检查清单  
④ 程序：对照表、E 断点、与黑屏案关系、修复顺序、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Npc1无E对照HomeScene23_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/Village_HomeScene23.unity

你现在是【施工员】。按对照报告修好 Village_HomeScene1 的 Npc1：走近须出现 E，按 E 能播 Village_Npc1。

必须：对齐 HomeScene23 可对话 NPC 三件套与 canTouch/Collider；不改坏 HomeScene23；若仍有 componentsList None / 未 Init 先按黑屏报告清掉；Npc1 保持近距（requirePlayerOverlap=true），不要改成远程物品。

提交说明：和 HomeScene23 差在哪几项、怎么改的、如何验收出 E 与对话。
```
