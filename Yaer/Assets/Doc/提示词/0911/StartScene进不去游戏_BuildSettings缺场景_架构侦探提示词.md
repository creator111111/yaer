# Cursor Agent Prompt · StartScene 加载失败导致进不去游戏（Editor Resource Mode）

> **角色**：【架构侦探】只读溯源；**禁止改代码 / 场景 / Prefab / Build Settings / Git 提交**  
> **日期**：2026-09-11  
> **现象（用户 + Console 截图）**：  
> 1. 从 Init 流程走完后无法进入游戏 / 主菜单  
> 2. Console 红字：`Scene 'Assets/GameRes/Scenes/StartScene.unity' couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded.`  
> 3. 同次运行日志写明：`During this run, Game Framework will use editor resource files`（**编辑器资源模式**）  
> 4. Unity `2020.3.48f1`；堆栈经 `EditorResourceComponent.LoadScene` → `SceneManager.LoadSceneAsync`  
> **产品期望（钉死）**：编辑器 Play 能从 Init 正常进到 `StartScene`（主菜单），再可继续进局；不要求本次改玩法/对话/村庄逻辑  
> **不是**：修村庄卡死、对话立绘、NodeCanvas 演出；不是改 `ProcedurePreload` 业务分支；不是一次性把全场景清单重写进 Build Settings「盲加」  
> **并行**：与 0901～0904 村长家/巨树案解耦——本案是 **启动链加载 StartScene 失败**  
> **报告落盘**：`Assets/Doc/执行文档/0911/StartScene进不去游戏_BuildSettings缺场景_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 开游戏播完启动界面后进不去主菜单。  
> 红字说加载不了 `StartScene`，原因写成「没进 Build Settings，或者 AssetBundle 没加载」。  
> 本次是编辑器里直接 Play，日志说用的是 editor resource files。

### 报错锚点（截图已钉死）

| 项 | 值 |
|----|-----|
| 时间戳 | `[08:59:00]` |
| 场景路径 | `Assets/GameRes/Scenes/StartScene.unity` |
| 错误原文关键词 | `has not been added to the build settings or the AssetBundle has not been loaded` |
| 资源模式 | Editor resource files（非已加载 AB 包路径） |
| 堆栈关键帧 | `EditorResourceComponent.LoadScene`（约 `EditorResourceComponent.cs:1153`）→ `UnityEngine.SceneManagement.SceneManager.LoadSceneAsync` |
| 上游（日志可见） | `GameFramework.Scene.SceneManager` → `SceneComponent.LoadScene` |

### 现网机制（助手预扫 · 高度可疑）

| 环节 | 预扫结果 | 若失败的体感 |
|------|----------|--------------|
| **场景文件是否存在** | `Assets/GameRes/Scenes/StartScene.unity` **磁盘存在**（另有 `.meta`） | 一般不是「文件丢了」 |
| **Build Settings 场景列表** | `ProjectSettings/EditorBuildSettings.asset` 的 `m_Scenes` **目前仅含** `Assets/GameRes/Scenes/InitScene.unity`（enabled） | Editor 下 `LoadSceneAsync(完整路径或场景名)` 会报与截图同款错 |
| **谁触发加载** | `ProcedurePreload` 预加载+Init 展示结束后：`ChangeSceneComponentGM.LoadScene(sceneName = SceneName.StartScene)` 再 `ChangeState<ProcedureMenu>` | Init 能开、Start 起不来 → 卡在「进不了主菜单」 |
| **场景名常量** | `SceneName.StartScene = "StartScene"`；交接文档写主菜单场景即该路径 | 路径/常量名与文件名一致的概率高 |
| **场景管理器** | 存在 `StartSceneManager`，`nowSceneName = SceneName.StartScene` | 本案优先卡在「场景都没加载成功」，GSM 未必轮到 |
| **文档侧预期** | `Project_context` / 交接文档：`ProcedureLaunch` → `ProcedurePreload` → 切 StartScene → `ProcedureMenu` | 与报错发生点同构 |

### 假说表（须并列证伪，按优先级写进报告）

| ID | 假说 | 证伪手段 |
|----|------|----------|
| **H1（首选）** | **EditorBuildSettings 未登记 `StartScene`**（目前列表几乎只有 InitScene），编辑器资源模式下 `LoadSceneAsync` 直接失败 | 读 `EditorBuildSettings.asset`；对照 Console 路径；Unity File→Build Settings 列表是否缺该项 |
| **H2** | 登记了但 **enabled=0** / GUID 指向错误或丢失的 meta | 查 `m_Scenes` 条目 `enabled`、path、guid 是否与 `StartScene.unity.meta` 一致 |
| **H3** | 加载时传入的 **asset 路径格式**与 EditorResourceComponent 期望不一致（多/少 `Assets/`、扩展名、大小写） | 从 `ChangeSceneComponentGM.LoadScene` → SceneComponent → EditorResourceComponent 跟一次实际 `sceneAssetName` 字符串 |
| **H4** | 误入 **AssetBundle 资源模式**却未加载含 StartScene 的包（截图日志偏不支持，但仍须一眼排除） | 确认 ResourceComponent / 启动日志是否仍是 editor resource files；AB 清单有无 StartScene |
| **H5** | `StartScene.unity` 损坏 / 依赖丢 GUID，导致「看似存在但 Load 失败」（文案通常不同，作次要） | 能否在 Editor 双击打开场景；Console 是否另有 YAML/missing script |
| **H6** | 历史曾有完整 Build Settings，被误改/未提交导致本机只剩 InitScene | `git log -p -- ProjectSettings/EditorBuildSettings.asset`；交接文档场景清单 vs 当前 m_Scenes |
| **H7** | 仅缺 StartScene 不够：后续进局还缺其它场景，但**本案验收门槛只要求先能进 StartScene** | 报告中单列「最小可玩 Build 列表建议」，勿在本案一次性盲加全表除非证据要求 |

### 方案倾向（仅倾向，侦探可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A** | 将 `Assets/GameRes/Scenes/StartScene.unity` **加入 Editor Build Settings（enabled）** 并保证 GUID 正确 | **首选**（对症截图原文 + 预扫仅 Init） |
| **B** | 按交接文档「常用场景清单」审计 Build Settings，补齐编辑器联调最小集（Init/Start/NewGame/…），写入施工说明避免下次再炸 | 强烈建议在 A 之后评估，**分批**，不要一次重写全部目录 |
| **C** | 改 `ProcedurePreload` 改切别的场景 / 硬编码绕过 | **禁止作终局**；与架构不符 |
| **D** | 改 GameFramework `EditorResourceComponent` 降低校验 | **禁止**；框架层临时修补 |

### 侦探禁止事项

- 禁止修改任何 `.cs` / Prefab / 场景 / `EditorBuildSettings.asset`「先修绿再查」。
- 禁止擅自 `git add` / `commit` / `push`。
- 禁止借机重写整份 Build Settings 或全项目场景依赖。

---

## 【架构侦探】执行段（复制给 Agent）

你是【架构侦探】。只读溯源，**禁止改代码**。

### 目标

查清：为何 Play 时加载 `Assets/GameRes/Scenes/StartScene.unity` 失败并阻断进入主菜单；给出可证伪根因与**最小修复建议**（优先环境/Build Settings，而非改业务流程）。

### 必查清单

1. **报错调用链**  
   - 从截图堆栈核对：`ProcedurePreload`（或其它入口）→ `ChangeSceneComponentGM.LoadScene` → GameFramework `SceneComponent` / `SceneManager` → `EditorResourceComponent.LoadScene`（约 1153 行）→ `UnityEngine.SceneManagement.SceneManager.LoadSceneAsync`。  
   - 写清：**实际传入的 `sceneAssetName` 字符串**是什么。

2. **场景资产是否存在**  
   - 确认 `Assets/GameRes/Scenes/StartScene.unity` + `.meta`（GUID）。  
   - 与 `SceneName.StartScene`、交接文档主菜单路径是否一致。

3. **Build Settings（本案关键）**  
   - 完整列出 `ProjectSettings/EditorBuildSettings.asset` 的 `m_Scenes`（path / enabled / guid）。  
   - 对照：是否缺少 `StartScene`；若有历史提交曾包含，注明何时被删/缩水。  
   - 说明：在 **editor resource mode** 下，为何「未进 Build Settings」会打出与用户截图**同一句** Unity 错误。

4. **资源模式排除**  
   - 用启动日志证明本次是 editor resource files，还是 AB；排除 H4 或保留为「须再验」。

5. **最小修复建议（只建议不施工）**  
   - 若 H1 成立：应把哪些场景 **最先** 加进 Build Settings 才能「进得了主菜单」。  
   - 可选：引用 `Assets/Doc/项目交接文档.md` 场景清单，给出「编辑器联调最小集」建议表（仍留待施工员执行）。  
   - 若设计不清（例如：团队是否故意只留 Init、靠 AB 跑）：记入 `Assets/Doc/OPEN_QUESTIONS.md`，**不要擅自改核心设计**。

### 报告结构（写入落盘路径）

1. 结论一句话（根因 + 是否 H1）  
2. 证据链（路径、Build Settings 条目、调用链、行号）  
3. 假说表（H1～H7：成立 / 排除 / 待确认）  
4. 用户侧「现在就能自查」清单（大白话：File → Build Settings 看有没有 StartScene）  
5. 建议施工步骤（最小改动；禁止项）  
6. 剩余风险（只加 StartScene 后，下一关切其它场景是否同样缺登记）  
7. 文末附「可复制给施工员」的 5～10 条 bullet（仍不直接改）

### 输出要求

- 中文；大白话优先；结构对齐仓库沟通习惯：结论 → 原因 → 检查清单 →（可选）程序补充。  
- 报告落盘：`Assets/Doc/执行文档/0911/StartScene进不去游戏_BuildSettings缺场景_架构溯源报告.md`  
- 技术细节可引用：`Assets/Project_context.md`、`Assets/Doc/02_SYSTEM_SPEC.md`、`Assets/Doc/项目交接文档.md`、`Assets/Doc/技术文档/InitPanel(Clone)启用逻辑.md`。

---

## 【施工员】Prompt（根因拍板后再用 · 预稿）

> 仅当侦探确认 **H1（Build Settings 缺 StartScene）** 或等价结论后使用。

1. **最小改动**：在 `EditorBuildSettings` 中启用并加入 `Assets/GameRes/Scenes/StartScene.unity`（保留现有 `InitScene`；不要删无关项除非报告要求）。  
2. 按侦探报告「编辑器联调最小集」**评估**是否同批补齐后续必切场景；**禁止**无依据地一次加入交接文档全部场景。  
3. **禁止**修改 `ProcedurePreload` / GameFramework `EditorResourceComponent` 来绕过 Build Settings。  
4. 施工说明落盘：`Assets/Doc/施工说明/0911/StartScene进BuildSettings_施工说明.md`（写清改了哪些 path/guid、为何改）。  
5. 验收：编辑器从 Init Play → 能进入 StartScene/主菜单；Console 不再出现该句 StartScene load 错误。  
6. 若仍失败：转【验收员】查资源模式、路径字符串、场景损坏，而不是继续盲加场景。

---

## 【验收员】Prompt（施工后或侦探存疑时 · 预稿）

1. 可加前缀日志如 `[StartSceneLoadDebug]`（仅若需要），确认 `LoadScene` 实参与 Resource 模式。  
2. 优先核对：Build Settings 列表、场景 GUID、Play 入口是否从 InitScene。  
3. 输出：验证结果 + 剩余风险（下一场景是否同样未登记）。

---

## 给用户的一句话（可先自查）

> 先打开 **File → Build Settings**，看列表里有没有 `Assets/GameRes/Scenes/StartScene.unity`；预扫显示现在很可能 **只有 InitScene**，所以编辑器加载主菜单场景会直接红字失败。
