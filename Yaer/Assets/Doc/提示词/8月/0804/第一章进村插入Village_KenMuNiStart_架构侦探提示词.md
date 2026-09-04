# Cursor Agent Prompt · 第一章地图进村后插入 Village_KenMuNiStart 对话

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-04  
> **范围**：第一章流程 —— 世界地图点选肯姆尼 / 精灵村 → 黑幕换场进入 `Village_KenMuNi1` 之后，**自动播放** `Village_KenMuNiStart`；不扩其它村内对话、不改对话台本文案。  
> **本阶段**：只摸清挂点、时序与一次性条件，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品目标（开发者白话 · 已定）

按**第一章正常流程**：

1. 序章结束 → 地图 → 点选肯姆尼关卡  
2. **黑屏渐入渐出**（与现有换场黑幕同一套）  
3. 进入村庄场景后，**首先播放**对话 Prefab **`Village_KenMuNiStart`**  
4. 对话内容资源已有：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`（台本即村内雅古开场「好漂亮的村子。」那一段）

### 现网漂移（侦探必须对齐「文档说已做」vs「代码实际」）

| 来源 | 说法 | 线索 |
|------|------|------|
| `MapFormLogic.OnSelectJingLingVillage` | 现只 `LoadScene(Village_KenMuNi1, blackFade:true)` | 注释写明：**替代方案「黑幕后 TriggerStory(Village_KenMuNiStart)」本期不做** |
| `0721/序章结束_恢复地图选肯姆尼_…` | 定稿：点关卡 → **黑幕进村**；**不要**改成「只播对话不换场」 | 当时未要求进村后自动播 Start |
| `技术文档/演出相关/MapPanel精灵城入口与黑幕对话_开发文档.md` | 状态写「黑幕→亮屏后 TriggerStory 已落地」 | **可能与现网 MapFormLogic 脱节**——侦探以代码为准修订 |
| `Village_KenMuNi1.unity` | 出现名为 `Village_KenMuNiStart` 的对象覆写 | 可能是场景内嵌对话壳/触发器，**未必**等于「进村自动播」 |
| `Village_KenMuNiSceneManager` | `OnEnterScene` 管镜头锁 / `homeDoorStoryComplete`，**未见** TriggerStory Start | 进村后开场对话缺口的高概率位置之一 |

### 期望玩家时序（侦探可微调措辞，但勿改成「不进村只播对话」）

```
地图点 ButtonJingLingVillage
  → 黑幕淡入（现有 LoadScene blackFade）
  → 加载 Village_KenMuNi1、关 MapPanel
  → 黑幕淡出 / 场景 Ready
  → 【插入】TriggerStory("Village_KenMuNiStart")
  → NormalDialogueNewPanel + Dialogue/Village_KenMuNiStart.prefab
  → 对白结束 → 还控给玩家（镜头/战斗面板规则与现村逻辑对齐）
```

**禁止**回退成：点地图只播对话、不进 `Village_KenMuNi1`（与 0721 定稿冲突，除非侦探证明产品已改且用户确认——本任务用户已说「进入村庄后」播对话）。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/演出相关/MapPanel精灵城入口与黑幕对话_开发文档.md
@Assets/Doc/执行文档/7月/0721/序章结束_恢复地图选肯姆尼_架构溯源与施工执行说明.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 第一章：点地图进村时已经有黑屏渐入渐出，进村后要**马上播** `Village_KenMuNiStart`。
2. Prefab 已在：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`。
3. 现网点关卡只会换场进 `Village_KenMuNi1`，**没有**自动拉这段开场对话（或拉了但坏了——侦探须证实）。
4. 目标：找到**最小插入点**，让「黑幕换场完成 → 首播 Village_KenMuNiStart」成为第一章主链的一环；并写清只播一次 / 重进村是否再播。

---

## 必读 / 优先扫描线索

### A. 地图进村主链（现状）
- `MapFormLogic.OnSelectJingLingVillage` / `LoadSceneComponentGSM.LoadScene(..., blackFade:true)`
- stayAction 关 MapPanel 时机；黑幕 Show/Hide 与 `onGameSceneManagerReady` / `OnEnterScene` 先后
- 注释中的「TriggerStory 替代方案」历史意图 vs 本期产品需求

### B. 进村后谁该 TriggerStory
候选挂点（比较优劣，推荐一个）：
1. `Village_KenMuNiSceneManager.OnEnterScene`（或 OnInit，注意黑幕是否仍全黑——对照 `Village_ShopSceneManager` 注释）
2. `LoadScene` 的 ready / 黑幕淡出完成回调（在 Map 或 GSM 侧订阅一次）
3. 场景内 `SimpleStoryTrigger`（Enter/自动）绑定 `Village_KenMuNiStart`
4. 恢复/改造旧 MapPanel「亮屏后 TriggerStory」——但须仍先/同时进村，不能只播对话

对照样例：`NewGameSceneManager.OnEnterScene` → `TriggerStory("NewGameStory")`

### C. Story 管线钉死
- `StoryComponentGSM.TriggerStory("Village_KenMuNiStart")`
- `DialoguePath` → `Assets/GameRes/Prefabs/Dialogue/{name}.prefab`
- 壳：`NormalDialogueNewPanel`；Prefab 内前奏（藏战斗面板 / 立绘淡入 / UI 淡入）是否与「刚黑幕淡出」叠两次黑/淡

### D. 场景内已有 Village_KenMuNiStart 引用
- `Village_KenMuNi1.unity` 中名为 `Village_KenMuNiStart` 的实例：是对话 Prefab 嵌套、触发器、还是摆设？
- 是否已有触发但被存档旗 / Active / SingleUse 挡住？

### E. 一次性与存档
- 是否已有字段可复用（如 `ForestSceneData.homeDoorStoryComplete` 或其它）标记「开场对白已播」？
- 新档首次进村必播；读档再进村、从 Shop/村外门再进村：**默认建议只播一次**，写入开放问题请用户拍板
- `SimpleStoryTrigger.SingleUseInArchive` 机制是否够用

### F. 与现有村逻辑协调
- `homeDoorStoryComplete == false` 时锁镜头、藏战斗图：开场对话期间/之后是否冲突
- 玩家可否在对话中走动；对话结束谁解锁
- BGM（`homeDoorStoryComplete` 才开）与开场对白的关系

---

## 侦探任务清单

1. **钉死现网**：点地图进村后，`Village_KenMuNiStart` 会不会自动播？若不会，卡在哪一步？若会，为何开发者体感「没插上」？

2. **画出第一章进村时序**（现网 vs 期望插入点）  
   Map 点击 → 黑幕 → LoadScene → SceneManager Ready → OnEnterScene → （？）TriggerStory

3. **推荐最小插入方案表**

   | 方案 | 挂点文件 | 优点 | 风险 | 是否推荐 |
   |------|----------|------|------|----------|
   | A SceneManager OnEnterScene | | | | |
   | B 黑幕淡出回调 | | | | |
   | C 场景 Trigger | | | | |
   | D 改 Map 不换场只播对话 | | 与「进村后」冲突 | | **默认不推荐** |

4. **一次性条件**：建议用哪面存档旗；重进村行为写开放问题。

5. **施工员最小改动清单**（只建议）：改哪些脚本/场景、是否动 Prefab、如何验证。

6. **验收清单**  
   - 新档：序章 → 地图点肯姆尼 → 黑幕 → 进村 → **自动出** Village_KenMuNiStart（首句可见）  
   - 对话结束可操作（或符合锁镜头设计）  
   - 再进村是否不再播（按拍板）  
   - Console：可有 `[MapSelect]` + TriggerStory 成功日志建议  
   - DialogDebug 拖 Prefab 仍可单测（勿破坏）

7. **开放问题**追加 `OPEN_QUESTIONS.md`（新开「第一章进村 · Village_KenMuNiStart · 2026-08-04」）：  
   - 仅首次进村播？  
   - 挂在 SceneManager 还是场景 Trigger？  
   - Prefab 前奏淡入与换场黑幕是否叠？  
   - 与 `homeDoorStoryComplete` 是否共用一旗？

8. **禁止**：改对话台本；删除换场只播对话；扩成全村对话表大改；在 Update 轮询播剧情。

---

## 输出要求

写入：`Assets/Doc/执行文档/0804/第一章进村插入Village_KenMuNiStart_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（现网缺哪一环、推荐挂在哪）  
② 原因（生活类比 + 现网/文档漂移说明）  
③ 用户需要做什么（拍板一次性 + 验收清单）  
④ 给程序看的补充：时序图、方案表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认挂点（SceneManager vs Trigger vs 黑幕回调）与「只播一次」后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0804/第一章进村插入Village_KenMuNiStart_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使第一章地图点选进村并黑幕换场后，自动播放 Village_KenMuNiStart。
必须保留 LoadScene(Village_KenMuNi1)；禁止改成「点地图只播对话不进村」。
禁止在 Update 堆剧情触发；优先组件解耦。
每次提交说明：改了哪些文件、实现了什么、如何验证（新档进村首句自动出）。
```
