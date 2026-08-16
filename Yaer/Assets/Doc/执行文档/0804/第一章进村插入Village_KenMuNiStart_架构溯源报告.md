# 第一章进村插入 Village_KenMuNiStart — 架构溯源报告

**文档版本**：v1.0（2026-08-04）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 场景 / CSV / 台本**）  
**范围**：第一章 —— 地图点选肯姆尼 → 黑幕进 `Village_KenMuNi1` 之后，**自动播放** `Village_KenMuNiStart`；不扩其它村内对话、不改台本文案。  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0804/第一章进村插入Village_KenMuNiStart_架构侦探提示词.md`
- 对照：`0721/序章结束_恢复地图选肯姆尼_…`；`技术文档/演出相关/MapPanel精灵城入口与黑幕对话_开发文档.md`（**文档已漂移**）
- 代码 / 场景 / Dialogue Prefab 静态阅读

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**现网点地图只会 `LoadScene(Village_KenMuNi1, blackFade:true)`，整条链上没有 `TriggerStory("Village_KenMuNiStart")`——开场对话缺的就是「进村 Ready / 黑幕淡出之后」这一环。推荐挂在 `Village_KenMuNiSceneManager.OnEnterScene`（对齐 NewGame 样例；该回调在现网挂在黑幕淡出结束），并用 `StoryTriggerCountData` 做「同档只播一次」。禁止改回「点地图只播对话不进村」。**

场景里那个名叫 `Village_KenMuNiStart` 的嵌套对象 **不是**自动播触发器（且源 Prefab guid 与 `Dialogue/Village_KenMuNiStart.prefab` 不一致，疑似孤儿引用）。

---

## ② 原因（生活类比 + 漂移说明）

### 生活类比

地图点「肯姆尼」像坐火车到站：黑幕是隧道，进村是下车。现在火车到站、门开了，但站长没喊「欢迎来到村子」那段开场白——台词本（Dialogue Prefab）在抽屉里放着，没人按播放键。旧说明书还写着「在地图上就播对白」，和现在「先到村再播」两套说法打架，以代码为准。

### 文档 vs 现网（必须修订认知）

| 来源 | 说法 | 代码实际 |
|------|------|----------|
| `MapPanel…开发文档.md`（2026-04） | 「黑幕→亮屏后 TriggerStory 已落地」 | **未落地**；Map 只换场 |
| `MapFormLogic` 注释 | 替代方案 TriggerStory「本期不做」 | 与 0721「点关卡进村」一致；**本期要做的是进村后再 Trigger**，不是替代换场 |
| `0721` 定稿 | 点关卡 → 黑幕进 `Village_KenMuNi1` | **已实现**；当时未要求进村后自动 Start |
| 场景 `Village_KenMuNiStart` 名 | 像已接线 | 仅 Canvas 下 PrefabInstance 改名；**无** `StoryPrefabName=Village_KenMuNiStart` 的 Trigger |

### 现网时序（缺环）

```
ButtonJingLingVillage
  → MapFormLogic.OnSelectJingLingVillage
  → LoadScene(Village_KenMuNi1, stayAction=关 MapPanel, blackFade:true)
  → 黑幕淡入 → stayAction → 卸载旧景 → 加载村景
  → onGameSceneManagerReady → 全黑再 hold≈0.3s → 黑幕淡出
  → onEndLoadingSceneEvent → BaseGameSceneManager.OnEnterScene
       → Village_KenMuNiSceneManager.OnEnterScene
            → 锁镜头 / homeDoor 旗（若未完成）
            → 【缺口】无 TriggerStory("Village_KenMuNiStart")
```

### 期望时序（插入点）

```
…同上直到 OnEnterScene …
  → 【插入】若本档尚未播过 → TriggerStory("Village_KenMuNiStart")
  → NormalDialogueNewPanel + Dialogue/Village_KenMuNiStart.prefab
  → 对白结束 → 还控（镜头仍可按 homeDoorStoryComplete==false 保持锁，与现村设计对齐）
```

### Story 管线（已齐，只需调用）

| 层 | 锚点 |
|----|------|
| API | `StoryComponentGSM.TriggerStory("Village_KenMuNiStart")` |
| 路径 | `DialoguePath` → `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`（**存在**） |
| 壳 | `NormalDialogueNewPanel` |
| Prefab 前奏 | 藏战斗立绘意图 + 立绘 CanvasGroup 淡入 + UI Alpha 淡入（**软淡入**，非再开一层全屏黑幕） |

### 场景内引用核实

| 项 | 结果 |
|----|------|
| `SimpleStoryTrigger` 绑定 Start？ | **无**。场景内仅见 `StoryPrefabName: ForestSceneStoneBrand`（疑似森林拷贝残留） |
| 名为 `Village_KenMuNiStart` 的对象 | Canvas（Scale 常为 0）下 PrefabInstance **改名**；源 guid `397659d3…` **在工程 meta 中未找到**；Dialogue Prefab guid 实为 `aace8b8b…` → **不是**自动播链路 |
| 会被进村自动播吗？ | **不会** |

### 与 `homeDoorStoryComplete` 协调

| 行为（`homeDoor==false`） | 与开场对白 |
|---------------------------|------------|
| 锁镜头、CancelFollow | 对白期间玩家难走远 —— **可接受**；对白本身会控输入/UI |
| 藏战斗立绘 | TriggerStory 也会关战斗立绘 —— 一致 |
| BGM 要等 homeDoor 完成才开 | 开场对白时可能无村 BGM —— **待拍板**是否接受；**勿**把 Start 播完直接写成 `homeDoorStoryComplete=true`（那是森林门口另一套剧情旗） |

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板（OPEN）

1. **仅首次进村播？**（默认：**是**；Shop/村外再进不再播）  
2. **挂点**：SceneManager `OnEnterScene` vs 场景 Trigger？（默认：**SceneManager**）  
3. Prefab 前奏软淡入与换场黑幕是否叠感？（默认：**可接受**；已在亮屏后播）  
4. 与 `homeDoorStoryComplete` **是否共用一旗？**（默认：**否**；用 `StoryTriggerCountData` / 独立键）

### 验收清单

1. 新档：序章 → 地图点肯姆尼 → 黑幕 → 进村 → **自动出** Village_KenMuNiStart（首句「好漂亮的村子。」）  
2. 对话结束可操作（或符合锁镜头设计）  
3. 再进村（Shop 回村 / 重进）按拍板：**默认不再播**  
4. Console：可保留 `[MapSelect]`；建议加一行 Trigger 成功日志（如 `[VillageStart]`）  
5. DialogDebug 拖 Prefab 单测仍可用（勿绑死只能从地图进）

**不必**因本任务重导 CSV / 改台本。

---

## ④ 给程序看的补充

### 4.1 方案表

| 方案 | 挂点文件 | 优点 | 风险 | 是否推荐 |
|------|----------|------|------|----------|
| **A SceneManager OnEnterScene** | `Village_KenMuNiSceneManager.cs` | 与 NewGame 样例一致；现网 OnEnterScene **在黑幕淡出后**（`onEndLoadingSceneEvent`）；改动面最小 | 须防重复进入（Shop 回村）；`OnEnterScene` 可能被多事件订阅——靠 `HasRunningStory` + 存档旗 | **推荐** |
| B 黑幕淡出回调 | Map / LoadScene 订阅 | 时机精确 | Map 已关；跨场景订阅易漏退订；与「村逻辑」分离 | 次选 |
| C 场景 SimpleStoryTrigger | `Village_KenMuNi1.unity` | 策划可视；`SingleUseInArchive` 现成 | Enter 依赖碰撞/站位，难保证「进村首先」；场景脏（StoneBrand 残留） | 不优先 |
| D 改 Map 不换场只播对话 | `MapFormLogic` | — | **与 0721 / 本期产品冲突** | **禁止** |

### 4.2 一次性条件建议

| 选项 | 说明 | 建议 |
|------|------|------|
| `StoryTriggerCountData.CheckStoryUsed("Village_KenMuNiStart")` | TriggerStory 结束会 `OnStoryTriggered`；与 `SimpleStoryTrigger.SingleUseInArchive` 同源 | **推荐**（无需新存档字段） |
| 新字段 `villageStartStoryComplete` | 语义清晰 | 可，但多改 Archive |
| 复用 `homeDoorStoryComplete` | 省字段 | **不推荐**（语义是森林门口剧情，村里还锁镜头/BGM） |

伪代码（施工参考，本阶段不落地）：

```csharp
// Village_KenMuNiSceneManager.OnEnterScene 末尾
var counts = GetArchiveData<StoryTriggerCountData>();
if (!counts.CheckStoryUsed("Village_KenMuNiStart"))
{
    GetModule<StoryComponentGSM>().TriggerStory("Village_KenMuNiStart");
}
```

### 4.3 黑幕 / 前奏叠化

- 换场黑幕：`LoadScene` 路径，**淡出完成后**才 `OnEnterScene`。  
- Prefab 内：`FightingPanelVisible` + 立绘 Alpha + `NormalDialogueUIAlpha` —— **UI/立绘淡入**，不是第二层全屏 BlackPanel。  
- 风险：亮屏后立刻再淡入对话框，体感「又闪一下」——通常可接受；若嫌叠，可后续再压 Prefab 前奏 Duration（**非本期必做**）。

### 4.4 施工员最小改动清单（只建议）

| 步骤 | 文件 | 改什么 |
|------|------|--------|
| 1 | `Village_KenMuNiSceneManager.cs` | `OnEnterScene` 内按存档旗 `TriggerStory("Village_KenMuNiStart")` + 日志 |
| 2 | （可选）清理场景孤儿 `Village_KenMuNiStart` 嵌套 / StoneBrand 误拷贝 | **非必须**；勿当成接线手段 |
| 3 | 不改 | `MapFormLogic` 换场主链、Dialogue Prefab 台本、CSV |

**验证**：新档进村首句自动出；Shop 回村不再播；DialogDebug 仍可拖 Prefab。

### 4.5 相关文件

| 类别 | 路径 |
|------|------|
| 地图进村（已齐） | `MapFormLogic.OnSelectJingLingVillage` |
| 换场黑幕 | `LoadSceneComponentGSM.LoadScene` |
| **推荐挂点** | `Village_KenMuNiSceneManager.OnEnterScene` |
| Story API | `StoryComponentGSM.TriggerStory` |
| 内容 Prefab | `GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab` |
| 一次性 | `StoryTriggerCountData` |
| 对照样例 | `NewGameSceneManager.OnEnterScene` → `NewGameStory` |
| 过时文档 | `MapPanel精灵城入口与黑幕对话_开发文档.md`（应标「仅历史；现网=换场进村，Start 待插」） |

### 4.6 开放问题（已追加 OPEN）

见 OPEN「第一章进村 · Village_KenMuNiStart · 2026-08-04」。

---

## 施工员下一轮最小化清单（建议）

1. `Village_KenMuNiSceneManager.OnEnterScene` + `StoryTriggerCountData` 门闩 + `TriggerStory("Village_KenMuNiStart")`  
2. 保留 Map `LoadScene`；**禁止**改成只播对话  
3. 按 §③ 验收  
