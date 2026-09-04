# Editor Player Stats Tool — 中文乱码 — 架构溯源报告

**文档性质**：架构侦探产出（只读溯源；**本阶段不改代码**）  
**日期**：2026-08-20  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- 提示词：`Assets/Doc/提示词/0820/Editor_PlayerStatsTool中文乱码_架构侦探提示词.md`
- 罪魁：`Assets/Editor/Tool/PlayerStatsEditorWindow.cs`
- 正常对照：`Assets/Editor/Tool/AddDateMenuItem.cs`
- 正确文案：`Assets/Doc/技术文档/Player/PlayerStatsTool编辑指南.md`

**现象**：

| 位置 | 看到的 |
|------|--------|
| `Tools` 菜单 | 「增加日期」正常；下一项整段 `` 替换符乱码 |
| Player Stats Tool 窗口 | `δ╬╘PlayerLogic` 一类乱码；`Find PlayerLogic` 英文正常 |

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**菜单乱码和窗口「δ╬╘PlayerLogic」是同一文件 `PlayerStatsEditorWindow.cs`：源码里中文字符串已被错误编码保存（含大量 UTF-8 替换符 `U+FFFD` + 残留 GBK 碎片），不是 Unity 字体/语言问题；「增加日期」正常是因为 `AddDateMenuItem.cs` 仍是完好 UTF-8。**

---

## ② 原因（生活类比）

菜单招牌上的漆被洗花了（源文件字节坏了），不是顾客眼镜花了（不是编辑器不会显示中文）。  
同街区另一块招牌「增加日期」漆还在，所以看起来正常。  
英文按钮「Find PlayerLogic」像用英文喷的漆，没被洗坏。

---

## ③ 用户需要做什么

| # | 动作 |
|---|------|
| 1 | 只修 **`Assets/Editor/Tool/PlayerStatsEditorWindow.cs`**（本批不必动其它插件菜单） |
| 2 | 按下方「应恢复字符串表」把中文改回正确字 |
| 3 | **用与 `AddDateMenuItem.cs` 相同方式保存**：UTF-8、**无 BOM**（两文件现网皆无 BOM） |
| 4 | 重启/刷新 Unity 后看 `Tools → 人物状态调试工具`；窗口无玩家时应显示 **「未找到PlayerLogic」** |
| 5 | **不要**去改 Unity 语言、全局字体当主方案 |

次选（若中文环境仍不稳）：MenuItem 暂时改成英文如 `Tools/Player Stats Tool`（窗口标题已是英文）。

---

## ④ 给程序看的补充

### 4.1 菜单项 ↔ 哪个 MenuItem

工程内 `[MenuItem("Tools/` 抽样：

| 文件 | 路径 | 中文 |
|------|------|------|
| `AddDateMenuItem.cs` | `Tools/增加日期` | ✅ 完好 |
| **`PlayerStatsEditorWindow.cs`** | `Tools/????…`（源码已坏） | ❌ **即菜单乱码项** → `Open()` |
| Dialogue 工具 | `Tools/Dialogue/...` | 英文，正常 |

**同一文件**同时造成：Tools 菜单乱码 + 窗口中文 Label 乱码。

### 4.2 窗口乱码 ↔ 哪句 GUI

| 现网（损坏） | 应对 |
|--------------|------|
| `GUILayout.Label("…PlayerLogic")`（字节已坏，界面似 `δ╬╘PlayerLogic`） | 文档：**未找到PlayerLogic** |
| `GUILayout.Button("Find PlayerLogic")` | 英文，正常 |
| 窗口标题 `"Player Stats Tool"` | 英文，正常 |
| `ToggleLeft` / 其它中文 Button、Label | 同文件一并损坏（见字符串表） |

### 4.3 编码证据（钉死）

| 检查 | `PlayerStatsEditorWindow.cs` | `AddDateMenuItem.cs` |
|------|------------------------------|----------------------|
| BOM | **无** | **无** |
| `Tools/` 后字节 | 大量 **`EF BF BD`（U+FFFD）** + 残留如 `D7 B4 CC AC`（GBK「状态」碎片） | 正规 UTF-8：`E5 A2 9E…`（增加日期） |
| 用 UTF-8 读 MenuItem | `Tools/?????????????` | `Tools/增加日期` |
| `Assets/Editor/Tool/*.cs` 含 U+FFFD | **仅本文件** | — |

**根因类型**：**源文件编码损坏 / 错误另存**（UTF-8 ↔ GBK 误转后二次保存，不可逆替换符已写入磁盘）。  
**不是**：运行时缺字、任务/对话系统、整机不会显示中文（旁证：同菜单「增加日期」、Dialogue 英文项均正常）。

「未找到」完好 GBK 应为 `CE B4 D5 D2 B5 BD`；现网 Label 附近为 `CE B4 EF BF BD D2 B5 EF BF BD` + `PlayerLogic`——中间已被 `U+FFFD` 打断，界面再按错误代码页显示成 `δ╬╘…` 一类，与截图一致。

### 4.4 应恢复字符串表

依据 `PlayerStatsTool编辑指南.md` + 控件语义：

| 位置 | 损坏现状（现网可读形态） | 应恢复为 |
|------|--------------------------|----------|
| `[MenuItem(...)]` | `Tools/` + 乱码/替换符 | **`Tools/人物状态调试工具`** |
| 未找到 Label | `δ╬╘PlayerLogic` / `δҵPlayerLogic` 等 | **`未找到PlayerLogic`** |
| 无敌 Toggle | 乱码 | **`无敌开关`**（与 `EditorInvincible` 注释语义一致；文档未单列字面，按控件用途） |
| 修服装 Button | 乱码 | **`修复服装`** |
| 受伤 Button | 乱码 | **`受到10点伤害`** |
| 血量 Label | 乱码 | **`血量：`**（或文档口语「血量」；现网意图带冒号） |
| 体力 Label | 乱码 | **`体力：`** |
| XML 注释 / `//` 注释中文 | 乱码 | 按语义重写（可选，不影响菜单；建议一并修以免再误导） |

**勿改**：`"Player Stats Tool"`、`"Find PlayerLogic"`、逻辑代码。

### 4.5 最小修复建议（不施工）

1. 在支持 UTF-8 的编辑器中打开该 `.cs`，按上表替换全部损坏中文。  
2. **另存为 UTF-8 无 BOM**（与 `AddDateMenuItem.cs` 一致）。  
3. Unity 刷新后验收：  
   - Tools 出现可读 **「人物状态调试工具」**  
   - 无玩家时 Label 为 **「未找到PlayerLogic」**  
   - 有玩家时：无敌开关 / 修复服装 / 受到10点伤害 / 血量： / 体力：可读  
4. 次选：MenuItem 改英文避险（仅当 UTF-8 中文仍异常时）。  
5. **禁止**把「改 Unity 语言/字体」当主方案。

### 4.6 开放问题（已记入 OPEN）

| ID | 问题 | 建议 |
|----|------|------|
| Q1 | Editor 脚本统一 UTF-8（有/无 BOM）？ | **与现网正常文件一致：UTF-8 无 BOM** |
| Q2 | 是否还要扫其它已损坏 Editor 文件？ | `Tool/` 下仅本文件含 FFFD；可再扫 `Assets/Editor` 全树作卫生检查（另批） |

---

## 5. 相关路径

| 资源 | 路径 |
|------|------|
| 损坏源码 | `Assets/Editor/Tool/PlayerStatsEditorWindow.cs` |
| 正常对照 | `Assets/Editor/Tool/AddDateMenuItem.cs` |
| 文案指南 | `Assets/Doc/技术文档/Player/PlayerStatsTool编辑指南.md` |

---

## 6. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-08-20 | 初版：菜单+窗口同文件编码损坏；字节证据；恢复字符串表 |

**文档路径**：`Assets/Doc/执行文档/0820/Editor_PlayerStatsTool中文乱码_架构溯源报告.md`
