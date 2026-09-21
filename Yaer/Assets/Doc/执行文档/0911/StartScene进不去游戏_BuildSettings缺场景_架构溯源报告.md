# StartScene 进不去游戏 — BuildSettings 缺场景 — 架构溯源报告

**文档版本**：v1.0（2026-09-11）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / 场景 / Prefab / Build Settings / Git  
**Unity**：2020.3.48f1  
**现象**：Init 播完后进不了主菜单；Console 红字加载失败 `Assets/GameRes/Scenes/StartScene.unity`  
**期望**：编辑器 Play 能从 Init → `StartScene`（主菜单）→ 再进局；不改玩法/对话/村庄  
**提示词**：`Assets/Doc/提示词/0911/StartScene进不去游戏_BuildSettings缺场景_架构侦探提示词.md`  
**并行**：与 0901～0904 村长家/巨树案解耦——本案仅启动链加载 StartScene 失败  

---

## 沟通摘要

### ① 结论一句话

**根因 = H1：本机 `EditorBuildSettings` 目前只登记了 InitScene，未登记 StartScene；编辑器资源模式下 `LoadSceneAsync` 直接失败。仓库 HEAD 仍有完整场景表（含 StartScene），是本地未提交地把列表缩成 Default（仅 Init）。**

### ② 原因（通俗）

游戏启动播完 Init 后，会去加载主菜单场景 `StartScene`。  
Unity 编辑器规定：用完整路径异步加载场景时，**必须先在 Build Settings 里勾选登记**。  
现在列表里几乎只有 Init，主菜单场景不在册，所以红字拦死，进不了游戏。  
场景文件本身还在磁盘上，不是丢了；也不是 AB 没打（本次日志明确走 editor resource files）。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 预期 |
|---|------|------|
| 1 | Unity：**File → Build Settings**，看 Scenes In Build | 现况大概率**只有** `InitScene`；**没有** `StartScene` |
| 2 | 日常开发：菜单 **`Game Framework / Scenes in Build Settings / All Scenes`** | 把 `Assets/GameRes/Scenes` 下场景（含 StartScene）写回列表 |
| 3 | **或**最小手搓：把 `Assets/GameRes/Scenes/StartScene.unity` 拖进 Build Settings 并勾选 enabled | 仅满足「能进主菜单」 |
| 4 | 从 **InitScene** 再 Play 一次 | Console **不再**出现该句 StartScene load 错误；能进主菜单 |
| 5 | 若曾点过 **Default Scenes** 准备出包：出包后务必再点 **All Scenes** 才能继续编辑器联调 | 交接文档已写清 |

### ④ 程序补充

见下文 §1～§7；施工请用文末可复制 bullet，**侦探本回合不施工**。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H1 成立**：`ProjectSettings/EditorBuildSettings.asset` 的 `m_Scenes` **仅** `InitScene`（enabled=1） |
| **次因 / 环境** | **H6 成立（本地）**：相对 `HEAD` 工作区 **删了 87 行**场景登记；`HEAD` 仍含 `StartScene.unity`（guid `30342bdc…`） |
| **资源模式** | **H4 排除**：`GameFramework.prefab` `m_EditorResourceMode: 1`；`BaseComponent` 打出「use editor resource files」与用户截图一致 |
| **路径 / 常量** | **H3 排除**：实参为 `Assets/GameRes/Scenes/StartScene.unity`，与 `SceneAssetPath` / 文件 / meta GUID 一致 |
| **场景损坏** | **H5 弱排除**：磁盘存在 `StartScene.unity` + `.meta`；错误文案是 Build Settings / AB，不是 missing script |
| **推荐修复** | **方案 A**：登记并启用 StartScene；**强烈建议**日常用菜单 **All Scenes**（方案 B 联调最小集），**禁止**改 `ProcedurePreload` / `EditorResourceComponent`（C/D） |
| **OPEN_QUESTIONS** | **不新增**：设计已钉死——出包用 Default（仅 Init），编辑器联调用 All Scenes（见交接文档） |

---

## 2. 证据链

### 2.1 报错调用链（与截图同构）

```
ProcedureLaunch
  → ProcedurePreload
      → Init 淡出结束
      → ChangeSceneComponentGM.LoadScene(sceneName = SceneName.StartScene)
          → SceneAssetPath.GetSceneAssetPath("StartScene")
             = "Assets/GameRes/Scenes/StartScene.unity"   ← 实际 sceneAssetName
          → SceneComponent.LoadScene(...)
              （EditorResourceMode 时 ResourceManager = EditorResourceHelper）
          → EditorResourceComponent.LoadScene
              → SceneManager.LoadSceneAsync(sceneAssetName, Additive)  ← ≈1153 行
          → Unity 红字：未进 Build Settings 或 AB 未加载
      → （失败则 ProcedureMenu 回调不执行 → 进不了主菜单）
```

| 环节 | 路径 / 行号 | 要点 |
|------|-------------|------|
| 切主菜单 | `ProcedurePreload.cs` ≈66–70 | `sceneName = SceneName.StartScene`；成功回调才 `ChangeState<ProcedureMenu>` |
| 切场入口 | `ChangeSceneComponentGM.cs` ≈75–89 | Init 上直接 `LoadScene`，不先卸 Init |
| 路径拼接 | `SceneAssetPath.cs` | `"Assets/GameRes/Scenes/" + sceneName + ".unity"` |
| 常量 | `SceneName.cs` | `StartScene = "StartScene"` |
| Editor 加载 | `EditorResourceComponent.cs` ≈1132–1153 | 校验 `Assets/`+`.unity` → `HasFile` → `LoadSceneAsync(完整路径)` |
| 模式日志 | `BaseComponent.cs` ≈204–207 | `During this run, Game Framework will use editor resource files…` |
| 模式开关 | `GameFramework.prefab` | `m_EditorResourceMode: 1` |
| GSM（未轮到） | `StartSceneManager.cs` | `nowSceneName = SceneName.StartScene`；场景都没加载成功则未必执行 |

### 2.2 为何 editor resource mode 会打出同一句 Unity 错误

编辑器资源模式**不会**从 AB 取场景，而是把完整 asset 路径交给 `UnityEngine.SceneManagement.SceneManager.LoadSceneAsync`。  
Unity 规则：该 API 加载的场景必须在 **Editor Build Settings** 中启用登记；否则抛出用户截图原文（「not been added to the build settings or the AssetBundle has not been loaded」）。  
GF 的 `EditorResourceComponent` 在 `HasFile` 通过后**不做** Build Settings 二次校验，故失败点落在 Unity 原生 API。

### 2.3 当前 Build Settings（磁盘现况）

`ProjectSettings/EditorBuildSettings.asset`（工作区）：

| enabled | path | guid |
|---------|------|------|
| 1 | `Assets/GameRes/Scenes/InitScene.unity` | `404756de211009f40b3aa90c9ae675ee` |

**无** `StartScene` 条目 → 与 H1 完全吻合。

### 2.4 场景资产存在性

| 项 | 值 |
|----|-----|
| 文件 | `Assets/GameRes/Scenes/StartScene.unity` **存在** |
| meta guid | `30342bdc2226cc840b1ac908f22f1120` |
| 与历史 Build 条目 | `HEAD` / `f6805e46` 等登记的 StartScene guid **一致** |
| InitScene guid | `404756de211009f40b3aa90c9ae675ee`（与现表一致） |

### 2.5 Git / 本地缩水（H6）

| 对比 | 内容 |
|------|------|
| `HEAD:Yaer/ProjectSettings/EditorBuildSettings.asset` | **完整表**，含 `StartScene.unity`（guid `30342bdc…`）及森林/村/民居等 |
| 工作区相对 HEAD | `git diff`：**0 增 / 87 删**（整表缩成仅 Init） |
| 提交状态 | **未 staged / 未 commit**（本地脏文件） |
| 最可能操作 | 菜单 **`Game Framework / Scenes in Build Settings / Default Scenes`**（读 `BuildSettings.xml`，Default 仅 Init）或等价手改 |
| `BuildSettings.xml` | `<DefaultScene Name="Assets/GameRes/Scenes/InitScene.unity"/>` 仅一条——与现况同构 |
| 文档侧 | `项目交接文档.md` 已写明：出包用 Default；**日常 Play 须 All Scenes**；并注明「当前已是 Default（仅 Init）」 |

→ **不是**仓库远端把 StartScene 永久删掉；是**本机联调状态停在出包 Default**，却用编辑器资源模式 Play。

### 2.6 Default / All Scenes 菜单行为（避免误修）

| 菜单 | 脚本 | 效果 |
|------|------|------|
| Default Scenes | `BuildSettings.cs` `DefaultScenes()` | 只写入 `BuildSettings.xml` 的 Default 列表 → **仅 Init** |
| All Scenes | `BuildSettings.cs` `AllScenes()` | Default ∪ `AssetDatabase.FindAssets("t:Scene", Assets/GameRes/Scenes)` → **含 StartScene 及全部可玩场景** |

---

## 3. 假说表（H1～H7）

| ID | 假说 | 裁定 | 证据摘要 |
|----|------|------|----------|
| **H1** | EditorBuildSettings 未登记 StartScene | **✅ 主因** | 现 `m_Scenes` 仅 Init；错误路径恰为 StartScene |
| **H2** | 登记了但 enabled=0 / GUID 错 | **排除** | 根本无该条目；且历史 GUID 与 meta 一致 |
| **H3** | sceneAssetName 路径格式不对 | **排除** | 拼接结果标准；若格式错会走 GF「invalid/not exist」回调文案，不是本句 Unity 红字 |
| **H4** | 误入 AB 模式且包未加载 | **排除** | 启动 Info 日志 + prefab EditorResourceMode=1；堆栈在 EditorResourceComponent |
| **H5** | StartScene 损坏 / 依赖丢 GUID | **弱排除（非主因）** | 文件在、GUID 稳；文案不匹配；若仍失败再双击开场景验收 |
| **H6** | 历史完整表被误改/未提交缩水 | **✅ 成立（环境）** | HEAD 完整；工作区 -87 行；Default 菜单可复现 |
| **H7** | 只加 StartScene 后下一关仍可能缺登记 | **✅ 风险保留** | 本案验收门槛只需进得了主菜单；进局还需 NewGame/村/森林等——日常应用 All Scenes |

---

## 4. 方案倾向（只建议不施工）

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A** | 将 `StartScene.unity` 加入 Build Settings（enabled），保留 Init | **首选最小修复**（对症截图） |
| **B** | 菜单 **All Scenes**，或按「编辑器联调最小集」分批补齐 | **日常强烈建议**；避免下一关同款红字 |
| **C** | 改 `ProcedurePreload` 绕过 / 改切场目标 | **禁止终局** |
| **D** | 改 `EditorResourceComponent` 降校验 | **禁止**框架临时修补 |

### 编辑器联调最小集（建议表 · 留给施工员评估）

| 优先级 | 场景 | 为何 |
|--------|------|------|
| P0（本案门槛） | `InitScene`、`StartScene` | 启动 → 主菜单 |
| P1（能进局） | `NewGameScene`（若走新游戏）、主推村 `Village_KenMuNi1` | 主菜单后再进可玩 |
| P2（常用联调） | `Village_Chief_House`、民居 Home*、`Village_Shop`、`ForestScene` / `ForestEastScene` 等按当日任务 | 按需，勿无依据一次盲加「文档全表」以外的无关项 |
| 推荐操作 | 直接菜单 **All Scenes** | 与交接文档一致；比手搓更不易漏 |

**打包注意**：出 Windows 包前再切回 **Default Scenes**（仅 Init + 已打好的 StreamingAssets AB）；打完若还要编辑器玩，再切回 All Scenes。

---

## 5. 建议施工步骤（最小改动 · 禁止项）

1. **优先**：Unity 菜单 `Game Framework / Scenes in Build Settings / All Scenes`（推荐），或仅手加 `Assets/GameRes/Scenes/StartScene.unity`（enabled）。  
2. 确认 `File → Build Settings` 中 StartScene 勾选；guid 应为 `30342bdc2226cc840b1ac908f22f1120`。  
3. 从 InitScene Play → 进主菜单；Console 无该句 StartScene 错误。  
4. 施工说明落盘：`Assets/Doc/施工说明/0911/StartScene进BuildSettings_施工说明.md`（写清 path/guid、是否 All Scenes）。  
5. **禁止**：改 `ProcedurePreload` / `EditorResourceComponent`；禁止无依据一次性手写重写全部目录；禁止把「仅加 StartScene」当成长期日常状态（进局仍会炸）。  
6. 若 All Scenes 后仍失败：转【验收员】查资源模式、实参字符串、场景能否双击打开——不要继续盲加。

---

## 6. 剩余风险

| 风险 | 说明 |
|------|------|
| 只加 StartScene | 主菜单能进；点新游戏/读档/进村时，下一场景若未登记会**同款红字** |
| 再次 Default Scenes | 出包后忘记 All Scenes → 本案复现 |
| 误提交 Default 表 | 若有人把「仅 Init」的 `EditorBuildSettings.asset` commit 上去，全员编辑器联调翻车；当前差异仍在工作区，提交前需 intentional |
| AB 真机路径 | 出包关 EditorResourceMode + StreamingAssets AB 后，Build Settings 可只留 Init；与本案编辑器问题正交 |

---

## 7. 可复制给施工员（5～10 条）

> 根因已拍板：**H1 + 本地 Default 缩表（H6）**。按下列施工，勿改业务流程。

1. **最小目标**：编辑器从 Init Play 能进入 `StartScene` / 主菜单；消除该句 StartScene load 红字。  
2. **推荐操作**：菜单 **`Game Framework / Scenes in Build Settings / All Scenes`**（保留 Init，自动补 StartScene 及 `GameRes/Scenes` 下其它场景）。  
3. **备选最小**：仅将 `Assets/GameRes/Scenes/StartScene.unity`（guid `30342bdc2226cc840b1ac908f22f1120`）加入 Build Settings 并 enabled；**勿删**现有 Init。  
4. **禁止**修改 `ProcedurePreload.cs`、`ChangeSceneComponentGM`、`EditorResourceComponent` 来绕过 Build Settings。  
5. **禁止**无报告依据地手搓「交接文档全部场景」以外的奇怪路径；优先用 All Scenes。  
6. 施工说明写入：`Assets/Doc/施工说明/0911/StartScene进BuildSettings_施工说明.md`（改了哪些 path/guid、是否 All、为何改）。  
7. 验收：Init → StartScene → 主菜单；Console 无本案红字。  
8. 告知用户：出包用 **Default Scenes**，日常联调必须再 **All Scenes**，否则会再炸。  
9. 若仅做了备选最小（只加 StartScene）：在施工说明「剩余风险」写明进局其它场景仍可能缺登记。  
10. 仍失败 → 转【验收员】，查 EditorResourceMode / 实参 / 场景能否打开，禁止继续盲加场景。

---

## 附录：关键锚点速查

| 主题 | 路径 |
|------|------|
| 预加载切 Start | `Assets/Scripts/Game/GameRuntime/Procedure/ProcedurePreload.cs` |
| 切场 | `Assets/Scripts/Game/GameMgr/Component/ChangeScene/ChangeSceneComponentGM.cs` |
| 路径 | `Assets/Scripts/Game/Static/Path/SceneAssetPath.cs` |
| 场景名 | `Assets/Scripts/Game/Static/Name/Res/SceneName.cs` |
| Editor 加载 | `Assets/Scripts/GameFramework/UnityRuntime/Resource/EditorResourceComponent.cs` |
| Default/All 菜单 | `Assets/Scripts/GameFramework/Editor/GF/Misc/BuildSettings.cs` |
| Default 配置 | `Assets/Scripts/GameFramework/Configs/BuildSettings.xml` |
| Build 列表现况 | `ProjectSettings/EditorBuildSettings.asset` |
| 交接约定 | `Assets/Doc/项目交接文档.md` §1.3 / §1.4 |
| 上下文 | `Assets/Project_context.md`、`Assets/Doc/02_SYSTEM_SPEC.md` |
