# StartScene 进 Build Settings — 施工说明

**文档版本**：v1.0（2026-09-11）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0911/StartScene进不去游戏_BuildSettings缺场景_架构溯源报告.md`  
**方案**：报告方案 **B**（等同菜单 **All Scenes**）

---

## 沟通摘要

### ① 结论一句话

**已把本机 `EditorBuildSettings` 从「仅 Init」恢复为仓库 HEAD 完整场景表（含 `StartScene`），未改任何启动/切场业务代码。**

### ② 原因（通俗）

日常联调曾切到出包用的 **Default Scenes**（只留 Init），编辑器资源模式下加载 `StartScene` 必须先在 Build Settings 登记，所以 Init 播完后红字拦死。场景文件没丢，只是列表被缩水。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Unity：`File → Build Settings` | 列表含 `InitScene` + `StartScene`（及其它 GameRes/Scenes） |
| 2 | 确认 StartScene | path=`Assets/GameRes/Scenes/StartScene.unity`，guid=`30342bdc2226cc840b1ac908f22f1120`，enabled |
| 3 | 从 **InitScene** Play | Console **无**「StartScene.unity has not been added to the build settings…」 |
| 4 | Init 播完后 | 能进主菜单（StartScene / ProcedureMenu） |
| 5 | 若仍失败 | 转【验收员】：查 EditorResourceMode、实参路径、场景能否双击打开——勿再盲加 |

### ④ 程序补充

见下文改动清单与注意。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **Build Settings 恢复** | `Yaer/ProjectSettings/EditorBuildSettings.asset` | `git checkout HEAD --` 恢复完整 `m_Scenes`（等同 All Scenes） |

**未改（报告禁止项）**：

- `ProcedurePreload.cs`
- `ChangeSceneComponentGM.cs`
- `EditorResourceComponent.cs`
- `BuildSettings.xml` / `BuildSettings.cs` 菜单逻辑
- 任何玩法、对话、村庄脚本

---

## 恢复后关键条目（抽检）

| enabled | path | guid |
|---------|------|------|
| 1 | `Assets/GameRes/Scenes/InitScene.unity` | `404756de211009f40b3aa90c9ae675ee` |
| 1 | `Assets/GameRes/Scenes/StartScene.unity` | `30342bdc2226cc840b1ac908f22f1120` |

另含：`NewGameScene`、`Village_KenMuNi1`、`Village_Chief_House`、森林/民居/商店等（与 HEAD 一致）。

---

## 为何用 All（HEAD）而不是「只加 StartScene」

| 选项 | 说明 |
|------|------|
| 只加 StartScene | 能过本案门槛，但点新游戏/进村仍可能同款红字（报告 H7） |
| **All / HEAD 全表** | 与交接文档「日常联调用 All Scenes」一致；一次消除联调缺登记 |

---

## 打包 / 联调切换（务必告知）

| 时机 | 菜单 |
|------|------|
| 出 Windows 包前 | `Game Framework / Scenes in Build Settings / **Default Scenes**`（仅 Init + AB） |
| 打完还要编辑器玩 | 立刻再点 **`All Scenes`**，否则本案复现 |

---

## 剩余风险

| 风险 | 说明 |
|------|------|
| 再点 Default 后忘记 All | Init→Start 再次失败 |
| 误提交「仅 Init」表 | 当前已对齐 HEAD；勿把缩水表 commit 上去 |
| 真机 AB 路径 | 出包关 EditorResourceMode + StreamingAssets 后，Build 可只留 Init；与本案编辑器问题正交 |

---

## 验收对照（报告 §7）

- [ ] Init Play → StartScene / 主菜单  
- [ ] Console 无本案 StartScene load 红字  
- [ ] 未改 ProcedurePreload / EditorResourceComponent  
