# DialogDebug 对话测试场景 — 施工执行说明（修订）

**施工员交付** | 日期：2026-05-25（**解耦沙盒**，取代初版 GF 方案）  
**依据**：`DialogDebug对话测试场景_架构溯源与执行说明.md`（2026-05-25 修订）

---

## 1. 方案说明

| 项 | 内容 |
|----|------|
| 目标 | Open `DialogDebug.unity` → Play → Inspector **拖 prefab** 即测 |
| 路径 | `Assets/GameRes/Scenes/DialogDebug.unity`（无需 `SceneName` / Resource Editor 日常配置） |
| 核心 | `DialogDebugPlayground` + `DialogDebugRuntimeBootstrap` + 场景内 `DialogueTMPUGUI` |

**初版 GF 方案**（`DialogDebugSceneManager`、`StoryComponentGSM`、Enter 菜单）已废弃，见架构文档附录 A。

---

## 2. 改了哪些文件

| 文件 | 说明 |
|------|------|
| `DialogDebugPlayground.cs` | **新建**：拖 prefab → Instantiate → `StartDialogue()` |
| `DialogDebugRuntimeBootstrap.cs` | **新建**：最小 `GameManager`（仅 language） |
| `DialogDebugSceneSetupMenu.cs` | **重写**：搭沙盒层级，清理旧 SceneManager |
| `DialogDebugSceneMenu.cs` | **废弃**：仅提示改用 Open Scene + Play |
| `DialogDebugSceneManager.cs` | 标记 `[Obsolete]`，不再 Setup 使用 |
| `DialogDebugStoryTester.cs` | 标记 `[Obsolete]`，由 Playground 替代 |

**未改**：`StoryComponentGSM`、`NormalDialogueFormNewLogic`、`DialogueTMPUGUI` 核心逻辑。

---

## 3. 如何验证

1. 编译通过后：**Tools → Dialogue → Setup DialogDebug Scene**（一次性）
2. Open `DialogDebug.unity` → Inspector 确认 `dialoguePrefab` 已拖 `Village_KenMuNiStar_Test`（Setup 会默认赋值）
3. **直接 Play**（不要从 InitScene 启动）
4. 字幕三句、立绘正常；播完按 **T** 重播
5. 更换 Inspector 中 `dialoguePrefab` 为另一 prefab，再 Play / 按 T，无需改代码
6. Console 无 `StoryComponentGSM 未就绪`、无 GF 场景路径错误

---

## 4. 给程序的补充

- 含 `LoadSceneTaskAction` 的图在沙盒可能异常，纯对白/选项图最稳
- `DialogDebug.asset` / `SceneName.DialogDebug` 可保留但沙盒不依赖
- Toolbar（存读档等）在 Setup 时已隐藏，避免点击找 `UIComponentGM`
