# Cursor Agent Prompt · 场景改名 Village_HomeScene4 → Village_HomeScene23（全引用盘点）

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-04  
> **范围**：将室内场景 **`Village_HomeScene4` 全量改名为 `Village_HomeScene23`**（场景文件、SceneName、SceneManager 类/文件、Config、Build、村门 NextSceneName、双侧 EnterPos、文档中的运行时相关引用）。  
> **状态**：**已施工完成**（见同目录溯源报告 v1.1）；下文保留侦探提示词原文供复盘。  
> **注意**：目标名是 **`Village_HomeScene23`**（用户指定），不是 HomeScene2；勿与现有 `Village_HomeScene2` / `Village_HomeScene3` 混淆。

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**（施工已另轮完成）。

---

## 提示词助手预梳理（侦探须全量复核并补漏）

### 产品目标
将曾用名 `Village_HomeScene4` 全量改为 `Village_HomeScene23`（运行时三位一体 + 文档按拍板）。

| 旧 | 新 |
|----|-----|
| `Village_HomeScene4` | **`Village_HomeScene23`** |

涉及「字符串场景名」的运行时契约必须与 **Unity 场景文件名（不含 .unity）** 一致，否则 `LoadScene` / `LastSceneName` / `EnterPosConfig.lastScene` 会断链。

### 静态已扫到的必改面（侦探用 rg 再扫一遍，补全）

| 类别 | 现路径 / 符号（施工后） | 说明 |
|------|----------------|----------|
| 场景文件 | `Assets/GameRes/Scenes/Village_HomeScene23.unity` (+.meta) | 保留 meta GUID |
| Build | `EditorBuildSettings.asset` | path 已同步 |
| 常量 | `SceneName.Village_HomeScene23` | 已替换旧常量 |
| Manager | `Village_HomeScene23SceneManager.cs` | 保留 meta GUID |
| Config | `…/SceneManagerConfig/Village_HomeScene23.asset` | 保留 meta GUID |
| 村门 | `House_Npc4.NextSceneName` | → `Village_HomeScene23` |
| 村回程 | `EnterPosConfig.lastScene` | → `Village_HomeScene23` |
| 室内 EnterPos | 本场景 `lastScene` 字符串 | 一般仍是 `Village_KenMuNi1`，**不必**因改名而改来源名 |
| 存档风险 | 旧档 `LastSceneName` / 任务/位置若存了旧场景字符串 | 写开放问题：是否兼容 |

**文档**：大量 `Doc/**/Village_HomeScene23_*` —— 侦探须区分：

- **运行时必改**：代码/场景/Config/Build  
- **文档可选改**：执行说明/提示词/技术说明——建议施工清单里单列「文档批量替换」，默认可同轮改文件名与正文，避免后人搜旧名

**勿误伤**：

- `Village_HomeScene2` / `Village_HomeScene3` / `HomeScene1` / `Village_House4`  
- 对话 Prefab `HomeScene1Npc4`（龙宫/通用 NPC 资源名，**≠** 场景名）  
- 物体名 `House_Npc4` / `Npc4`（门/NPC 名，**默认不改**，除非侦探发现硬编码场景名绑在物体名上）

### 已知关联文档（改名后引用路径会变）

- `Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md`
- `Assets/Doc/执行文档/0804/Village_HomeScene23_*`
- `Assets/Doc/提示词/0804/Village_HomeScene23_*`
- `Assets/Doc/执行文档/6月/0601/Village_HomeScene23_*`
- `OPEN_QUESTIONS.md` 中 HomeScene4 节

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Config、Build、文档文件名。只读扫描 + 写「改名影响面报告 + 施工顺序」。

---

## 背景（策划白话）

1. 要把室内场景 **`Village_HomeScene23` 改成 `Village_HomeScene23`**。  
2. 影响面大：场景文件、脚本常量、SceneManager、Config、Build、村里门的目标场景名、回村落点表等要一起对齐。  
3. 目标：列出**完整清单**（一处不漏）、**推荐改名顺序**、**风险（存档/误伤）**，供施工员按表执行。  
4. 本阶段**不要真的改名**。

---

## 必读 / 扫描方法

1. 全仓库搜索（至少）：`Village_HomeScene23`、`HomeScene4`、`HomeScene4SceneManager`、`VillageHomeScene23Debug`  
2. 核对：`SceneName.cs`、`EditorBuildSettings.asset`、`Village_KenMuNi1.unity`（NextSceneName + EnterPos）、本场景 `.unity` 内 Manager/config 引用  
3. 列出所有 `*HomeScene4*` 磁盘文件（Scenes / Scripts / Config / Doc）  
4. 对照 `Village_HomeScene2` 命名规范，确认新名 `Village_HomeScene23` 无冲突、无拼写歧义  

---

## 侦探任务清单

1. **完整影响面表**（必出）

   | ID | 类型 | 当前位置/符号 | 改后 | 是否阻塞运行时 | 备注 |
   |----|------|---------------|------|----------------|------|
   | … | 场景/代码/Config/Build/村门/EnterPos/文档/其它 | | | | |

2. **钉死运行时契约**  
   - `LoadScene` 参数、`nowSceneName`、`LastSceneName`、`EnterPosConfig.lastScene` 必须三位一体等于新文件名。  
   - `House_Npc4` / `Npc4` 物体名是否必须改（默认否）。

3. **推荐施工顺序**（防半改名）  
   例：先加常量与 Manager 类 → 改场景内引用 → 重命名 unity/asset 文件 → 改 Build → 改村门与 EnterPos → 全库 rg 验收零残留 → 文档。  
   （顺序可按你判断调整，但须写清「为何不能先改文件再改字符串」。）

4. **风险与开放问题**  
   - 旧存档停在 HomeScene4 / LastSceneName=旧名？  
   - Addressables / 硬编码 path 是否还有？  
   - meta GUID：Unity 内重命名 vs 外部改文件名（建议写清应用 Unity 或保持 meta guid）  
   - 文档是否同轮全改  

5. **验收清单**  
   - rg 无 `Village_HomeScene23` 运行时残留（文档策略按拍板）  
   - Build 含新场景  
   - 村 → House_Npc4 进 `Village_HomeScene23` → 出门回村落点正确  
   - Console 无「场景未找到」/ 错误 LastScene  

6. **禁止**：本阶段改名；把 `HomeScene1Npc4` 对话资源改名；误改 HomeScene2/3；删除场景另建导致丢关卡。

---

## 输出要求

写入：`Assets/Doc/执行文档/0804/Village_HomeScene23改名Village_HomeScene23_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（能否改、影响几大类、最大风险）  
② 原因（场景名=换场身份证）  
③ 用户需要做什么（拍板：文档是否同轮改、旧档兼容）  
④ 给程序看的补充：完整影响面表、施工顺序、误伤黑名单、验收 rg 关键字

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认文档范围与存档策略后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0804/Village_HomeScene23改名Village_HomeScene23_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告把 Village_HomeScene23 全量改名为 Village_HomeScene23。
必须保持 LoadScene / nowSceneName / EnterPosConfig.lastScene / 村门 NextSceneName 与场景文件名一致。
禁止误改 HomeScene2/3、HomeScene1Npc4 对话资源、House_Npc4 物体名（除非报告明确要求）。
优先用可保留 .meta GUID 的方式重命名。每次提交说明改动清单与 rg 验收结果。
```
