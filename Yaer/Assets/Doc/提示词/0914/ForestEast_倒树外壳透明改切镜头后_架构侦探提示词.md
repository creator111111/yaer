# Cursor Agent Prompt · ForestEast 倒树：「外」透明改为切镜头后再淡

> **角色**：先【架构侦探】只读核实淡出绑定与进/出洞相机写入点；拍板后【施工员】最小改  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene` 倒树 / 树洞（`TreeBridge` / `OuterSprite` 子物体「外」）  
> **现象（用户期望）**：现在靠近倒树就会把外壳「外」淡透明；希望改成 **切完洞内镜头之后** 再改透明  
> **产品期望（钉死）**：  
> 1. **靠近**倒树（Interactive 进入范围）时，外壳 **保持不透明**，不再提前 Fade  
> 2. **进洞切镜头完成后**（`ChangeCamera(true)` 切 Confiner + Size + 贴底等写完）再 `OuterSpriteFade(0)`  
> 3. **出洞切回洞外镜头**（`ChangeCamera(false)`）后恢复 `OuterSpriteFade(1)`，外壳回到不透明  
> 4. 读档已在洞内：加载走 `ChangeCamera(true)` 时也应处于透明（与进洞一致），避免读档后外壳挡洞内  
> **不是**：改爬行 / Pass Fall / 遮罩 / 嘎吱音效 / 相机 Size / Confiner 形状；靠近淡出与进洞淡出叠两次；只进洞淡、出洞不还原  
> **对照（提示词助手预梳理，侦探须核实）**：  
> - 现网：`TreeBridgeLogic.OnInit` 订阅 `InteractiveComponent.onEnterInteractiveEvent → OuterSpriteFade(0)`、`onExit → Fade(1)`  
> - 进洞切镜：`ForestEastTreeEnterTrigger.ChangePlayerPos` 黑幕内 → `ForestEastTreeBridgeStoryMgr.ChangeCamera(true, …)`  
> - 出洞：同路径 `ChangeCamera(false)`；读档洞内：`BaseGameSceneManager` 也会 `ChangeCamera(true)`  
> - `OuterSpriteFade` 现为 **private**，`OuterSpriteFadeTime` 默认约 1s（场景序列化）  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_倒树外壳透明改切镜头后_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 别一走近树壳就变透明。  
> 要等进洞黑幕里镜头切到树洞内之后，再把外面那层「外」淡掉；出来切回外面镜头再淡回来。

### 「切镜头」定义（本案钉死）

| 算「切镜头」 | 不算 |
|--------------|------|
| `ChangeCamera(true/false)`：换 Confiner / OrthoSize（及现网贴底） | 靠近 Interactive 触发器 |
| | 洞内爬行 `CameraAction` 抖动（那是抖，不是切盒） |
| | Pass Fall 倒下动画 |

若侦探发现用户口头「切镜头」另有含义，写入 OPEN 并 **停工问人**，勿擅自绑到 `CameraAction`。

### 推荐方案（侦探排优先级）

| # | 方向 | 利弊 |
|---|------|------|
| **A（推荐）** | 去掉 Interactive Enter/Exit 对 `OuterSpriteFade` 的订阅；`OuterSpriteFade` 改为 **public**（或等价公开 API）；在 `ChangeCamera` **末尾**按 `isEnterTree` 调 `Fade(0)` / `Fade(1)` | 写入点唯一；进/出/读档只要走 ChangeCamera 就一致 |
| **B** | 淡出放在 `CloseFormFade` 黑幕完全关掉之后 | 更晚、更「看见洞内再透」；读档无黑幕路径须另补一次 |
| **C（否决）** | 保留靠近淡出，再在 ChangeCamera 淡一次 | 叠两次，违背「靠近不透」 |
| **D（慎用）** | 仅改 EnterTrigger，不改 ChangeCamera | 读档 `ChangeCamera(true)` 可能漏淡；出洞还原易漏 |

### 复现 / 验收矩阵

| 操作 | 期望 |
|------|------|
| 洞外走近倒树（不进洞） | 「外」**不**变透明 |
| 洞外走开 | 「外」仍不透明（无残留半透明） |
| 左/右入口进洞（黑幕 → 切镜） | **切镜完成后**「外」开始淡到透明（可用现网 `OuterSpriteFadeTime`） |
| 洞内左右爬 | 保持透明，不闪回不透明 |
| 左/右出口出洞 | 切回洞外镜头后「外」淡回不透明 |
| 读档已在树洞 | 加载后「外」为透明（或等价不可见挡洞内） |
| Pass Fall / 倒下后 | 不因本案改动额外坏外壳逻辑（侦探注明 Fall 后是否仍依赖 Outer） |

### 侦探须回答

1. Interactive Enter/Exit 是否 **仅** 用于外壳淡出？去掉订阅有无其它副作用？  
2. `ChangeCamera` 是否覆盖：交互进洞、交互出洞、读档已在洞内？有无旁路进洞不走 ChangeCamera？  
3. `OuterSprite` 空引用时 Fade 是否需 null 防护（对照 Pass Fall Attached 教训）？  
4. 推荐 **A 或 A+B**；写出施工改动文件清单与出洞/读档验收句。  
5. 若已 Fall（`CheckFall()`）跳过 OnInit 订阅：本案改完后 Fall 存档态外壳表现是否仍正确？

### 必读

1. `Assets/Project_context.md`、`Assets/Doc/02_SYSTEM_SPEC.md`  
2. `TreeBridgeLogic`：`OnInit` Interactive 订阅、`OuterSpriteFade`  
3. `ForestEastTreeBridgeStoryMgr.ChangeCamera`  
4. `ForestEastTreeEnterTrigger.ChangePlayerPos`（黑幕内调用顺序）  
5. `BaseGameSceneManager` 读档 `isInTreeBridge` → `ChangeCamera`  
6. 本提示词  

### 禁止（侦探阶段）

- 禁止改代码 / 场景资产 / Git  
- 禁止把淡出绑到 `CameraAction` 或 Pass 对白，除非用户另行确认  

### 侦探输出

1. 溯源报告 → 落盘路径见文首  
2. 结论：推荐方案字母 + 写入点（类.方法）  
3. 给施工员的最小改清单（3～8 条）  

---

## 【施工员】提示词（侦探拍板后整段交给 Agent）

> **角色**：【施工员】  
> **前置**：已读 `Assets/Doc/执行文档/0914/ForestEast_倒树外壳透明改切镜头后_架构溯源报告.md`（或本提示词方案 **A** 已确认）  
> **目标**：靠近不透；`ChangeCamera` 完成后进洞淡出、出洞淡入；读档洞内一致  
> **施工说明落盘**：`Assets/Doc/施工说明/0914/ForestEast_倒树外壳透明改切镜头后_施工说明.md`

### 最小改（默认按方案 A）

1. `TreeBridgeLogic.OnInit`：**删除或注释** Interactive `onEnter` / `onExit` 对 `OuterSpriteFade` 的订阅。  
2. 将 `OuterSpriteFade` 改为 **public**（或新增 public 包装，命名与项目风格一致）；对 `OuterSprite == null` **安全跳过**（打一条 Warn 即可，勿抛死）。  
3. `ForestEastTreeBridgeStoryMgr.ChangeCamera`：**在现有切 Confiner / Size / 贴底逻辑全部执行完之后**，若 `storyLogic != null`：  
   - `isEnterTree == true` → `storyLogic.OuterSpriteFade(0)`  
   - `isEnterTree == false` → `storyLogic.OuterSpriteFade(1)`  
4. **禁止**：改 Interactive 触发范围、改 `OuterSpriteFadeTime` 默认值（除非侦探要求）、改相机公式、改 Pass Fall。  
5. 关键注释写清：**为何从 Interactive 挪到 ChangeCamera**（靠近不透、切镜后再透、出洞/读档同源）。  
6. 写施工说明：改了哪些文件、验收矩阵勾选结果、已知风险（如黑幕未关完就开始淡是否可接受——方案 A 可接受；若产品要「见洞内再透」改 B）。

### 禁止（施工）

- 大重构、改无关故事触发器  
- 只改进洞不改出洞  
- `git commit` / `push`（除非用户另嘱）  

### 完工自检（勾选）

- [ ] 走近不进洞：外壳不透明  
- [ ] 进洞切镜后：开始淡透明  
- [ ] 出洞：淡回不透明  
- [ ] 读档在洞内：外壳透明  
- [ ] 施工说明已落盘  

---

## 用户怎么用

1. Agent 先跑文首到「侦探输出」→ 出溯源报告。  
2. 确认方案 A（或 A+B）后，再跑「施工员」整段。  
3. 进 ForestEast 按验收矩阵打一遍进/出/读档。  
