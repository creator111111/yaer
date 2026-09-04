# Cursor Agent Prompt · 甲方 CS0103：`VillageChiefDialoguePortraitLayout` 不存在（仅甲方机）

> **角色**：【架构侦探】只读溯源；**禁止改代码 / 场景 / Prefab / Git 提交**  
> **日期**：2026-09-04  
> **现象（用户 + 甲方 Console 截图）**：  
> 1. `Assets\Editor\Tool\Dialogue\VillageChiefDoorDialogueSetupEditor.cs(230,13): error CS0103: The name 'VillageChiefDialoguePortraitLayout' does not exist in the current context`  
> 2. Unity 提示：`All compiler errors have to be fixed before you can enter playmode!` → **无法进 Play**  
> 3. **仅甲方电脑报错**；开发者本机**无此错、可编译**  
> **产品期望（钉死）**：甲乙双方同仓库同提交后，Editor 脚本编译通过，可进 Play；不要求甲方本机有未提交本地文件  
> **不是**：改村长家门口/续聊立绘坐标业务；不是修运行时对话逻辑；不是改 NodeCanvas 图；不是「临时删 Nudge 调用糊编译」当终局方案（可作风险兜底假说，须标清）  
> **并行**：与 0901～0903 村长家/巨树 WalkArea 案解耦——本案是 **Editor 编译缺类型**，不是场景卡死  
> **报告落盘**：`Assets/Doc/执行文档/0904/甲方CS0103_VillageChiefDialoguePortraitLayout不存在_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 甲方一开工程就红字编译失败，进不了游戏。  
> 报错说找不到叫 `VillageChiefDialoguePortraitLayout` 的东西。  
> 我这边电脑好好的，只有甲方那边炸——很像「我这边有文件、仓库/甲方没有」。

### 报错锚点（截图已钉死）

| 项 | 值 |
|----|-----|
| 文件 | `Assets/Editor/Tool/Dialogue/VillageChiefDoorDialogueSetupEditor.cs` |
| 行/列 | **230,13** |
| 码 | **CS0103** |
| 符号 | `VillageChiefDialoguePortraitLayout` |
| 调用形态（本机预扫） | `NudgePortraitLayout` → `VillageChiefDialoguePortraitLayout.ApplyToDialogueRoot(root)` |
| 命名空间（本机预扫） | 调用方与定义方均为 `EditorC.Tool.Dialogue`（同 ns，一般不缺 `using`） |

### 现网机制（助手预扫 · 高度可疑）

| 环节 | 磁盘 / Git 预扫 | 若失败的体感 |
|------|-----------------|--------------|
| **调用方已进库** | `VillageChiefDoorDialogueSetupEditor.cs` **已被 git 跟踪**（近提交含「村长家对话反馈修改」等） | 甲方 pull 后有第 230 行引用 |
| **定义方未进库** | `VillageChiefDialoguePortraitLayout.cs` + `.meta` 在开发者机上为 **`??` Untracked** | 甲方磁盘**无此文件** → CS0103；开发者本机有本地文件 → **不报错**（与「仅甲方有」高度同构） |
| 同目录其它未跟踪 | `VillageChiefContinueDialogueSetupEditor.cs`、`…SewingKit…`、`…PaintingScaleFix…`、`…LeaveChiefEscort…` 等亦 `??` | 甲方若再跑相关菜单可能连环缺类型；本案以 Console 已报的 CS0103 为主 |
| asmdef | `Assets/Editor` 下未见 asmdef（预扫） | 优先级低于「文件未进库」；仍须侦探确认甲方是否另有程序集拆分 |
| 条件编译 | 两文件均 `#if UNITY_EDITOR` | 双方都是 Editor，一般不是「仅甲方」主因 |

### 假说表（须并列证伪，按优先级写进报告）

| ID | 假说 | 证伪手段 |
|----|------|----------|
| **H1（首选）** | **定义文件从未 commit/push**：`VillageChiefDialoguePortraitLayout.cs`（及 `.meta`）只在开发者本地，远程/甲方无 | 开发者机：`git status` / `git ls-files` / `git check-ignore`；远程：该路径是否存在；甲方工程是否缺同路径文件 |
| **H2** | 文件在远程但 **甲方未 pull / 不同分支 / 浅克隆漏文件** | 对比双方 `git rev-parse HEAD`、分支名、`git log -1 -- path`；甲方是否有文件但 GUID 丢 |
| **H3** | 文件被 **.gitignore / LFS / sparse-checkout** 排除，仅本机能看见 | `git check-ignore -v`；`.gitignore` 是否匹配 `Editor/Tool/Dialogue/*Portrait*` |
| **H4** | 甲方有文件但 **.meta GUID 冲突 / 导入失败** 导致类型未进编译 | 甲方 Console 是否还有其它脚本错误；Library 损坏；Reimport 后是否仍 CS0103 |
| **H5** | **命名空间 / 类名拼写 / 可见性** 不一致（甲方旧副本被手改） | 双方打开定义文件比对 `namespace`、`public static class` 名 |
| **H6** | **程序集边界**：定义在 Runtime、调用在 Editor 或缺引用（本仓库预扫弱） | 搜 `.asmdef`；Assembly Definition 引用图 |
| **H7** | 调用已合入，但定义写在**另一未跟踪脚本**里被误删/改名 | 全库搜 `class VillageChiefDialoguePortraitLayout`、`ApplyToDialogueRoot` |

### 方案倾向（仅倾向，侦探可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A** | 将 `VillageChiefDialoguePortraitLayout.cs` + `.meta` **纳入版本库并推送**；甲方 pull 后清 Library 可选 | **首选**（对症「仅甲方缺类型」） |
| **B** | 审计同批 `??` 的村长家 Dialogue Setup 脚本，一并入库，避免下一枪 CS0103 | 强烈建议与 A 同批评估 |
| **C** | 临时从 SetupEditor 去掉 Nudge 调用 / 内联常量 | **不推荐作终局**（会再漂移 0901 已踩坑）；仅紧急 unblock 可记 OPEN_QUESTIONS |
| **D** | 让甲方手动拷贝开发者该文件 | 可救急，**不能替代**入库 |

### 侦探禁止事项

- 禁止修改任何 `.cs` / Prefab / 场景「先修绿再查」。
- 禁止擅自 `git add` / `commit` / `push`（入库属施工员，须报告拍板）。
- 禁止把「我本机有文件」当成已交付远程。

---

## 【架构侦探】执行段（复制给 Agent）

你是【架构侦探】。只读溯源，**禁止改代码**。

### 目标

查清：为何 **甲方** 报 `CS0103: VillageChiefDialoguePortraitLayout does not exist`，而 **开发者本机没有**；给出可证伪根因与最小修复建议（谁入库、拷哪些文件）。

### 必查清单

1. **调用点**  
   - 打开 `Assets/Editor/Tool/Dialogue/VillageChiefDoorDialogueSetupEditor.cs` 约 **230** 行：`NudgePortraitLayout` 如何引用该类型。  
   - 该文件的 `git log` / 哪次提交引入了 `VillageChiefDialoguePortraitLayout`。

2. **定义点**  
   - 全库搜索：`VillageChiefDialoguePortraitLayout`、`ApplyToDialogueRoot`。  
   - 若存在 `Assets/Editor/Tool/Dialogue/VillageChiefDialoguePortraitLayout.cs`：读 namespace、类修饰符、`#if`。  
   - **`git ls-files` / `git status`**：该 `.cs` 与 `.meta` 是 tracked 还是 **Untracked**？是否被 ignore？

3. **「仅甲方」对照**  
   - 用表格写清：开发者磁盘有无、Git 跟踪与否、远程理应有无、甲方缺什么时必现 CS0103。  
   - 列出同目录其它 **Untracked** 且可能被已入库脚本引用的文件（预防连环炸）。

4. **次要假说快速证伪**  
   - asmdef、命名空间、gitignore、双方 HEAD 是否一致（能读到的本地证据写进报告；甲方机无法直连则写「须甲方确认」清单）。

5. **与 Play 的关系**  
   - 确认：这是 Editor 编译失败阻断 Play，还是运行时缺资源（一句话钉死）。

### 报告结构（写入落盘路径）

1. 结论一句话（根因 + 是否 H1）  
2. 证据链（路径、git 状态、提交、行号）  
3. 假说表（H1～H7：成立 / 排除 / 待甲方确认）  
4. 甲方侧「打开工程前」核对清单（大白话）  
5. 建议施工步骤（最小：入库哪些文件；可选：同批其它 `??`）  
6. 剩余风险（未入库的其它 Setup 脚本）  
7. 若设计不清：记入 `Assets/Doc/OPEN_QUESTIONS.md`（例如：未跟踪 Editor 工具是否允许长期本地-only）

### 输出要求

- 中文；大白话优先。  
- 报告落盘：`Assets/Doc/执行文档/0904/甲方CS0103_VillageChiefDialoguePortraitLayout不存在_架构溯源报告.md`  
- 文末附「可复制给施工员」的 5～10 条 bullet（仍不直接改代码）。

---

## 【施工员】Prompt（根因拍板后再用 · 预稿）

> 仅当侦探确认 **H1（定义文件未进 Git）** 或等价结论后使用。

1. 将 `VillageChiefDialoguePortraitLayout.cs` + `.meta` 加入版本库（按团队规范 commit；**本会话若用户未要求提交则只改工作区并写施工说明**）。  
2. 评估是否同批纳入其它已引用、仍 Untracked 的村长家 Dialogue Setup 脚本，避免甲方下一枪再红。  
3. **禁止**为过编译而删除 `NudgePortraitLayout` 或把立绘常量复制回两处 Setup（与 0901 定稿同源要求冲突），除非 OPEN_QUESTIONS 另有决议。  
4. 施工说明落盘：`Assets/Doc/施工说明/0904/甲方CS0103_VillageChiefDialoguePortraitLayout入库_施工说明.md`。  
5. 验收：干净 clone / 模拟无该本地文件的目录下，打开工程无 CS0103；开发者机回归仍绿。

---

## 给用户的一句话（可转甲方）

> 先别清 Library 瞎忙：很大概率是立绘布局脚本只在我电脑上、**还没推进仓库**；推进去你 pull 一下应能过编译。
