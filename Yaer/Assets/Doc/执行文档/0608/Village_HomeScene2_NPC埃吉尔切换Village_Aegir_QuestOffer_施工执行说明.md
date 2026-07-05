# Village_HomeScene2 — NPC_埃吉尔 切换 Village_Aegir_QuestOffer — 施工执行说明

**施工员交付（待实施）** | 日期：2026-06-08  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【施工员】最小化修改、先可运行
- 占位交互已交付：`Assets/Doc/执行文档/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md`
- 埃吉尔台本：`Assets/Doc/执行文档/0607/Village_HomeScene2_埃吉尔接任务对白台本_架构溯源与执行说明.md`
- 对话 Prefab 前置修复：`Assets/Doc/执行文档/0608/Village_Aegir_QuestOffer_CanvasGroup空引用_架构溯源与修复执行说明.md`
- NPC 三件套原理：`Assets/Doc/执行文档/0601/Village_HomeScene4_NPC对话配置_执行说明.md`

**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene2.unity`  
**目标对话 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestOffer.prefab`  
**Unity 版本**：2020.3.48f1  

---

## 1. 目标（一句话）

把 **`Village_HomeScene2` 屋内 `NPC_埃吉尔`** 的 **`Story Prefab Name`** 从占位 **`HomeScene1Npc1`** 改成 **`Village_Aegir_QuestOffer`**，使按 **E** 时播放埃吉尔接任务正式对白；**只改场景上一个字符串字段**，交互碰撞、`sceneObjs` 登记 **无需动**。

---

## 2. 背景与现状（静态阅读 2026-06-08）

### 2.1 场景侧

| 项目 | 当前状态 | 本任务 |
|------|----------|--------|
| Hierarchy | `SceneManager → Object → **NPC_埃吉尔**` 已存在 | 不改名、不挪结构 |
| `SimpleStoryTrigger.StoryPrefabName` | **`HomeScene1Npc1`**（龙宫占位） | → **`Village_Aegir_QuestOffer`** |
| `SceneEntityComponentGSM.sceneObjs` | 已含 `NPC_埃吉尔` 的 `SceneEntity` | **保持** |
| `isCanTouchWithOther` | `true` | **保持** |
| 进村换场 | `House_NPC2` → `Village_HomeScene2` | 验收路径不变 |

### 2.2 资源侧

| 资源 | 路径 | 状态 |
|------|------|------|
| 对话 Prefab | `Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestOffer.prefab` | ✅ 已存在 |
| CSV 源稿 | `Assets/Dialog/Village_HomeScene2_Aegir_QuestOffer.csv` | ✅ 17 行对白 + 选项分支 |
| 加载规则 | `DialoguePath.GetPath(name)` → `Assets/GameRes/Prefabs/Dialogue/{name}.prefab` | 文件名须 **完全一致** |

> **命名说明**：工程内实际 Prefab 名为 **`Village_Aegir_QuestOffer`**（非 `Village_HomeScene2_Aegir_QuestOffer`）。`StoryPrefabName` 填 **不含 `.prefab` 的文件名**。

### 2.3 前置条件（必须先过，否则 Play 会崩或播占位）

| # | 前置 | 文档 |
|---|------|------|
| P1 | Speaker 映射含 `埃吉尔 → 埃吉尔`、（完整 CSV）`— → 旁白` | `0608/CSV导入工具_埃吉尔Speaker映射缺失_…` |
| P2 | **`Village_Aegir_QuestOffer.prefab` 前奏已修**：删除未绑定的 `GushaPainting` 淡入，或改为 `AegirPainting` | `0608/Village_Aegir_QuestOffer_CanvasGroup空引用_…` |
| P3 | DialogDebug 或单独 Trigger 试播 **无** `CanvasGroupAlphaActionTask` NRE | 同上 §5 验收 |

**未完成 P2 就改场景**：按 E 后 Console 仍可能 `NullReferenceException`，对白一句不出。

---

## 3. 原理（只改一个字段为什么就够）

```
玩家按 E
  → InteractiveComponent → SimpleStoryTrigger
  → StoryComponentGSM.TriggerStory(StoryPrefabName)
  → 加载 Assets/GameRes/Prefabs/Dialogue/{StoryPrefabName}.prefab
  → DialogueTreeController 播放 NodeCanvas 图
```

`StoryPrefabName` 是唯一决定「播哪段剧情」的开关；碰撞盒、`sceneObjs`、按 E 链路在占位阶段已配好，**替换对话 = 只换货道里的商品名**。

---

## 4. Unity 施工步骤

### 4.1 确认对话 Prefab 可播（改场景前）

1. （推荐）打开 **`DialogDebug`** 场景，或 Project 中选中 `Village_Aegir_QuestOffer.prefab` 用现有试播手段。  
2. 触发播放，确认：  
   - Console **无** 加载失败、**无** `CanvasGroupAlphaActionTask` 空引用  
   - 首句「啊，是你呀。」能出字  
3. 若失败 → 先按 `0608/Village_Aegir_QuestOffer_CanvasGroup空引用_…` **方案 A** 修 Prefab，再回到本步骤。

### 4.2 修改场景 NPC（核心，约 1 分钟）

1. 打开 **`Assets/GameRes/Scenes/Village_HomeScene2.unity`**。  
2. Hierarchy：`SceneManager → Object → **NPC_埃吉尔**`。  
3. Inspector → **Simple Story Trigger**（或 `SimpleStoryTrigger`）：  

| 字段 | 改前 | 改后 |
|------|------|------|
| **Story Prefab Name** | `HomeScene1Npc1` | **`Village_Aegir_QuestOffer`** |

4. 其余字段 **保持与改前一致**：

| 字段 | 建议值 | 说明 |
|------|--------|------|
| **Trigger Type** | `Click` | 村庄 = 靠近按 **E** |
| **Single Use In Archive** | **不勾选** | 方便反复测全文 |
| **Stay Time To Trigger Story** | `0` | 非 Stay 模式 |

5. **Ctrl+S** 保存场景。

### 4.3 本任务明确不改的内容

| 项目 | 原因 |
|------|------|
| `sceneObjs` 列表 | `NPC_埃吉尔` 已登记 |
| Body 碰撞 / `InteractiveComponent` | 占位阶段已验收 |
| `Village_Aegir_QuestOffer.prefab` 内容 | 除非 P2 未过；场景切换 **不要求** 改 Prefab |
| C# 脚本 | 无必要 |
| `QuestAcceptAction` / 任务系统 | 选项「我会努力的」接任务属 **批次 B**，本任务仅换对白 |

---

## 5. 验收标准

**必须从 `InitScene` 启动**（不要单独 Open `Village_HomeScene2` 再 Play）。

| # | 操作 | 通过标准 |
|---|------|----------|
| Q1 | InitScene → 进村 → `House_NPC2` 按 E 进屋 | 正常进入 `Village_HomeScene2` |
| Q2 | 走到 **`NPC_埃吉尔`** | 出现 **E** 提示 |
| Q3 | 按 **E** | 对话 UI 弹出；**首句「啊，是你呀。」**（雅尔），非龙宫占位台词 |
| Q4 | 顺序播放 | 埃吉尔/旁白/雅尔台词与 CSV 一致（含 ID 7、9 旁白） |
| Q5 | 选项 | 末段出现 **「我还有事」「我会努力的」** 两选项（CSV ID 16） |
| Q6 | 选「我还有事」 | 对白正常结束，无 Console 报错 |
| Q7 | 选「我会努力的」 | 播放「我会努力的！」后结束（**任务接取可暂无**，属预期） |
| Q8 | Console | **无** `加载资源失败: .../Village_Aegir_QuestOffer.prefab` |
| Q9 | Console | **无** `NullReferenceException` / `CanvasGroupAlphaActionTask` |
| Q10 | 关对话后再按 E | 可再次触发全文（`SingleUseInArchive` 未勾选） |

### 5.1 故障排查

| 现象 | 优先检查 |
|------|----------|
| 仍是龙宫占位台词 | 场景是否保存；`StoryPrefabName` 是否拼写完全一致（区分大小写） |
| `加载资源失败: Village_Aegir_QuestOffer` | Prefab 是否在 `GameRes/Prefabs/Dialogue/` **根目录**（勿放子文件夹） |
| 按 E 无反应 | `sceneObjs` 是否仍含该 NPC；是否从 InitScene 进 |
| 一开场就 NRE | **未做 P2** → 修 Prefab 前奏 `GushaPainting` |
| 有 UI 无字幕 | Prefab Graph 是否 Bind；Actor 雅尔/埃吉尔是否绑定 |
| 埃吉尔无立绘只有字 | `DialogueRoleName.Aegir` / 立绘未入库，**首版可接受** |
| `sceneObjs` 第一项为 None | 场景里 `{fileID: 0}` 空槽位，建议 Editor 删掉空项，**不影响**已登记的埃吉尔 |

---

## 6. 改动范围与提交说明

| 类型 | 路径 | 改动 |
|------|------|------|
| 场景 | `Assets/GameRes/Scenes/Village_HomeScene2.unity` | `NPC_埃吉尔` → `StoryPrefabName = Village_Aegir_QuestOffer` |
| Prefab | `Village_Aegir_QuestOffer.prefab` | **仅当前置 P2 未做时**（前奏修复） |
| C# | — | **不改** |

**提交说明模板**：

```text
Village_HomeScene2：NPC_埃吉尔 对话切换为 Village_Aegir_QuestOffer

修改：Village_HomeScene2.unity（SimpleStoryTrigger.StoryPrefabName）
前置：Village_Aegir_QuestOffer.prefab 前奏 GushaPainting 已修（若本次一并提交）

验证：InitScene → House_NPC2 进屋 → 按 E 播埃吉尔接任务对白 + 双选项，Console 无加载/NRE
```

---

## 7. 后续（本任务不做）

| 项 | 说明 | 文档 |
|----|------|------|
| 选项「我会努力的」接 `Quest_002` | 需 `QuestAcceptAction` + 任务配置 | `0606/经典MMO击杀任务_…` |
| 埃吉尔立绘 `Avatar_Aegir` | Prefab Argir 侧 CanvasGroup + 枚举 | `0607/埃吉尔接任务对白台本_…` §3.4 |
| 首次/重复对话分支 | 存档旗标 + 第二 prefab | `0607/埃吉尔接任务对白台本_…` §7.2 |
| 统一 prefab 命名为 `Village_HomeScene2_Aegir_QuestOffer` | 可选重命名；若重命名须 **同步改** `StoryPrefabName` 与文档 | — |

---

## 8. 相关文档索引

| 主题 | 路径 |
|------|------|
| NPC 占位交互（已完成） | `0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md` |
| Prefab NRE 修复 | `0608/Village_Aegir_QuestOffer_CanvasGroup空引用_架构溯源与修复执行说明.md` |
| Speaker 映射 | `0608/CSV导入工具_埃吉尔Speaker映射缺失_架构溯源与执行说明.md` |
| 进村换场 | `0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md` |
| DialogDebug 试播 | `0525/DialogDebug对话测试场景_施工执行说明.md` |

---

## 9. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-06-08 | 初版：NPC_埃吉尔 从 HomeScene1Npc1 切换至 Village_Aegir_QuestOffer；含前置 P1～P3 与 Play 验收 |

**文档路径**：`Assets/Doc/执行文档/0608/Village_HomeScene2_NPC埃吉尔切换Village_Aegir_QuestOffer_施工执行说明.md`
