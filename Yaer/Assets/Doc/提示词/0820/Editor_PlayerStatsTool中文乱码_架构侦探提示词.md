# Cursor Agent Prompt · Editor 中文乱码：Tools 菜单 + Player Stats Tool 窗口

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **现象 1（Tools 菜单）**：`Tools` 下「增加日期」显示正常；其下一项为 **整段 `` 替换符乱码**（开发者红线标出）。  
> **现象 2（工具窗口）**：打开后标题栏旁/内容区出现 **`δ╬╘PlayerLogic`** 一类乱码，下方按钮 **「Find PlayerLogic」** 英文正常。  
> **窗口名**：`Player Stats Tool`  
> **本阶段**：只读 + 写溯源报告，**不施工**（可给最小修复建议）

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. Tools 菜单里那一项为什么是乱码？上面「增加日期」为什么正常？  
2. Player Stats Tool 窗口里「δ╬╘PlayerLogic」又是为什么？和菜单乱码是不是同一原因？

### 现场证据（截图）

| 位置 | 看到的 | 预扫含义 |
|------|--------|----------|
| `Tools` 菜单 | `增加日期` 正常；下一项全是 `` | 菜单路径字符串源码已坏，或编码读错 |
| Player Stats Tool | `δ╬╘PlayerLogic` + `Find PlayerLogic` 按钮正常 | 中文 Label 坏了；英文硬编码还好 |
| 同 Editor 其它中文 | `Tools/Dialogue/...`、`增加日期` 正常 | **不是**整机 Unity 不会显示中文 |

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| 罪魁文件 | `Assets/Editor/Tool/PlayerStatsEditorWindow.cs` |
| 菜单项 | `[MenuItem("Tools/……")]` 源码里中文**已经是乱码字节**（如 `Tools/״̬���Թ���`）→ Unity 菜单只能画出替换符 |
| 窗口 Label | `GUILayout.Label("δҵPlayerLogic")` 一类，本意是文档写的 **「未找到PlayerLogic」**；UTF-8/GBK 被错误另存后变成 `δ╬╘PlayerLogic` |
| 对照正常文件 | `Assets/Editor/Tool/AddDateMenuItem.cs`：`[MenuItem("Tools/增加日期")]` 中文完好 |
| 文档真相 | `Assets/Doc/技术文档/Player/PlayerStatsTool编辑指南.md`：菜单应为 **`Tools → 人物状态调试工具`**；找不到玩家时应显示 **「未找到PlayerLogic」** |
| 根因类型 | **源文件编码损坏 / 错误编码保存**（经典：UTF-8 当 GBK 存，或反之），**不是**运行时字体缺字、也不是任务系统问题 |
| 其它中文串 | 同文件内「无敌开关」「修复服装」「受到10点伤害」「血量」「体力」等注释/按钮预扫一并损坏 |

### 必读 / 扫描

- `Assets/Editor/Tool/PlayerStatsEditorWindow.cs`（全文中文字符串与 MenuItem）
- `Assets/Editor/Tool/AddDateMenuItem.cs`（正常对照）
- `Assets/Doc/技术文档/Player/PlayerStatsTool编辑指南.md`（正确中文文案清单）
- 编码证据：文件 BOM（有无 EF BB BF）、用十六进制或 Python 看 MenuItem 行原始字节；对比「增加日期」行
- 可选：同目录其它 Editor 脚本是否也有乱码（扩大面，但主因钉在 PlayerStats）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Editor/Tool/PlayerStatsEditorWindow.cs
@Assets/Editor/Tool/AddDateMenuItem.cs
@Assets/Doc/技术文档/Player/PlayerStatsTool编辑指南.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码。只读扫描 + 写溯源报告。

---

## 背景

1. 开发者在 Tools 菜单看到一项纯乱码（红线标出），「增加日期」却正常。
2. 打开 Player Stats Tool 后，窗口出现「δ╬╘PlayerLogic」乱码；Find PlayerLogic 按钮正常。
3. 要查明：为什么乱码、是不是同一个文件、正确中文应是什么、怎么修（只建议不施工）。

---

## 必查线索

### A. 菜单乱码 ↔ 哪个 MenuItem

- 搜所有 `[MenuItem("Tools/`  
- 找出路径已是乱码的那条；确认是否就是 `PlayerStatsEditorWindow.Open`
- 对照 `AddDateMenuItem` 为何正常（同为 Tools 下中文）

### B. 窗口乱码 ↔ 哪句 GUI

- `OnGUI` 里所有中文 `GUILayout.Label` / `Button` / `ToggleLeft`
- 对拍截图 `δ╬╘PlayerLogic` 是否对应「未找到PlayerLogic」的错误解码
- 窗口标题 `"Player Stats Tool"` 为何正常（英文）

### C. 编码根因（钉死一种主因）

用证据回答，勿空猜：

| 检查 | 方法 |
|------|------|
| 源文件是否已是乱码 | 直接读 .cs，中文是否已不可读 |
| UTF-8 vs GBK | 原始字节；尝试用错误编码解释能否还原成「人物状态调试工具」「未找到」 |
| BOM | 文件头 3 字节 |
| 是否仅本文件 | 同目录 Editor 抽样 |
| 是否 Unity 字体 | 若源码已坏，则不是字体问题 |

### D. 正确文案清单（来自技术文档 + 合理还原）

列出修复时应恢复的字符串表，例如：

| 位置 | 损坏现状（现网） | 应恢复为 |
|------|------------------|----------|
| MenuItem | Tools/???? | Tools/人物状态调试工具 |
| 未找到 Label | δ╬╘PlayerLogic | 未找到PlayerLogic |
| … | … | … |

### E. 最小修复建议（不施工）

- 用 UTF-8（建议带 BOM 或项目统一无 BOM，与同目录正常文件一致）重写中文字符串  
- 或暂时改成英文 MenuItem 避险（次选）  
- 不要「全局改 Unity 语言/字体」当主方案，除非证据证明源码是好的

---

## 侦探任务

1. **结论一句话**：乱码因为 `PlayerStatsEditorWindow.cs` 源码中文编码损坏（或按证据改写）。  
2. **菜单项与窗口是不是同一文件**。  
3. **为何「增加日期」正常**。  
4. **编码证据**（BOM/字节/能否还原）。  
5. **应恢复字符串表** + 最小修复建议。  
6. OPEN：项目 Editor 脚本编码规范（UTF-8？）；是否还要扫其它已损坏文件。  
7. **禁止**：改文件；把问题归到任务/对话系统。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Editor_PlayerStatsTool中文乱码_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：菜单招牌油漆被洗坏了，不是顾客眼镜花了）  
③ 用户需要做什么（哪个文件、应改成什么字）  
④ 给程序：MenuItem 行、Label 行、编码证据、修复步骤、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Editor_PlayerStatsTool中文乱码_架构溯源报告.md
@Assets/Editor/Tool/PlayerStatsEditorWindow.cs
@Assets/Doc/技术文档/Player/PlayerStatsTool编辑指南.md

你现在是【施工员】。按报告修复 PlayerStatsEditorWindow.cs 中文乱码：Tools 菜单与窗口 Label/按钮恢复可读中文（或报告裁定的英文兜底）。

必须：文件保存编码与同目录正常 Editor 脚本一致（如 AddDateMenuItem）；不改工具逻辑；不改其它插件菜单。

提交说明：恢复了哪些字符串、编码怎么存、Tools 菜单与窗口如何验收。
```
