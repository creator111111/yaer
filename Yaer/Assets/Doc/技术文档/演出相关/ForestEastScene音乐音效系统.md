# ForestEastScene 音乐与音效系统说明

本文说明 **`ForestEastScene`（森林东郊 / 苍翠走廊一带）** 中与 **BGM、环境声、树桥事件音效** 相关的场景布置与代码入口，便于策划配置资源、程序排查「无声 / 叠音 / 切场景残留」等问题。

---

## 1. 全局播放管线（与本场景的关系）

所有通过 **`SoundToggleComponent`** 或 **`SoundComponentGM.PlaySound`** 播放的音频，资源路径由 **`Game.Static.Path.Sound.SoundPath`** 拼接：

| `SoundType` | 磁盘目录（相对 `Assets`） |
|-------------|---------------------------|
| **BGM** | `Assets/GameRes/Audio/BGM/` + 文件名 |
| **SFX** | `Assets/GameRes/Audio/SFX/` + 文件名 |

运行时由 **`SoundComponentGM`**（`GameManager` 的 GM 组件）调用底层 **`GameFramework` 的 `SoundComponent`** 播放；**BGM** 会先 **`StopBGM`** 再播新曲，避免叠两条 BGM。**SFX** 对「同名资源」有极短冷却（约 **0.1s**），同一 `resName` 在冷却内再次 `PlaySound` 会得到 **-1**（不播放）。因此环境声若共用同一条 `SoundToggle` 且资源名轮换不够快，可能出现偶发被挡；本场景鸟叫与虫鸣已拆到 **两个** `SoundToggleComponent`（见第 3 节与 `ForestEastSceneSfxNode` 实现）。

音量：`PlaySound` 内会乘 **`SettingsConfigData`** 的 **`allVolume`**，再分别乘 **`bgmVolume`** / **`soundVolume`**（SFX 侧还有额外系数，以代码为准）。

---

## 2. 场景内节点总览（`ForestEastScene.unity`）

在场景 **`Map`** 一类根节点下（与 `Design`、`BGM` 同级），与本场景音乐音效直接相关的物体主要有：

| 节点名 | 组件 | 作用 |
|--------|------|------|
| **BGM** | `SoundToggleComponent` | 进入场景并 **OnEnable** 时自动播放循环 BGM。 |
| **SceneSfxNode** | `ForestEastSceneSfxNode` + 子物体 **SFX_1** / **SFX_2** | 周期性随机 **鸟叫**、**虫鸣**（子物体上各挂一个 `SoundToggleComponent`，`isAutoPlay = false`，完全由脚本 `ChangeSoundRes` + `PlaySound` 驱动）。 |

**`SoundToggleComponent` 常用字段含义：**

- **`soundType`**：`BGM` 或 `SFX`（枚举 `SoundType`）。
- **`SoundResName`**：上述目录下的**文件名**（含扩展名，如 `.ogg` / `.mp3`）。
- **`Loop`**：是否循环（BGM 一般为 `true`）。
- **`isAutoPlay`**：为 `true` 时在 **`OnEnable`** 自动 `PlaySound`；为 `false` 则仅由代码调用 `PlaySound`。
- **`OnDisable`**：若曾成功申请到 **`soundID > 0`**，会 **`StopSound`**，离开场景或禁用物体时避免残留。

---

## 3. 背景音乐（BGM）

- **场景物体**：`BGM`。
- **当前配置（以场景为准）**：`soundType = BGM`，资源名 **`龙城郊，东郊苍翠走廊.ogg`**，`Loop = true`，`isAutoPlay = true`。
- **行为**：场景加载、该物体 **Enable** 后自动播放；切场景或禁用该物体时 **OnDisable** 内停止对应 **soundID**。

**编辑注意**：更换 BGM 时请在 Inspector 中改 **`Sound Res Name`**，并确认文件已放在 **`Assets/GameRes/Audio/BGM/`**；扩展名与磁盘文件名一致。

---

## 4. 环境音效（`ForestEastSceneSfxNode`）

- **脚本**：`Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/ForestEastScene/ForestEastSceneSfxNode.cs`
- **挂载物体**：`SceneSfxNode`。
- **逻辑概要**：
  - **`timeDistance_1`**（默认 15s）：每到间隔，随机 **`鸟叫1.mp3` ~ `鸟叫3.mp3`**，经 **`soundSfxCpn_1`** 播放。
  - **`timeDistance_2`**（默认 11s）：每到间隔，随机 **`昆虫1.mp3` ~ `昆虫5.mp3`**，经 **`soundSfxCpn_2`** 播放（与鸟叫分离，避免与 `SoundComponentGM` 的 SFX 同名冷却逻辑冲突）。

**资源目录**：上述文件需存在于 **`Assets/GameRes/Audio/SFX/`**。

**子物体 `SFX_1` / `SFX_2`**：Inspector 中 **`soundType = SFX`**，初始 **`SoundResName`** 可为占位（如「代码控制」），**`isAutoPlay = false`**，实际曲目由 `ForestEastSceneSfxNode` 在运行时 **`ChangeSoundRes`** 写入后再 **`PlaySound`**。

---

## 5. 树桥区域音效（`TreeBridgeLogic` + `ForestEastTreeBridgeStoryMgr`）

树桥实体脚本 **`TreeBridgeLogic`**（`TreeBridgeLogic.cs`）上除 **`SoundToggleComponent soundSfxCpn`** 外，还有 **`BaseSoundEntity waterSoundEntity`**（场景瀑布等**持续性**环境声，支持音量倍率调节）。

| 行为 | 说明 |
|------|------|
| **爬树洞时摄像机晃动** | `ForestEastTreeBridgeStoryMgr.CameraAction` 中调用 **`PlayTreeBridgeMoveSfx()`** → `TreeBridgeLogic.PlayTreeBridgeMoveSfx()`，播放 **`木头嘎吱嘎吱声 .mp3`**（注意文件名中带空格，须与资源完全一致）。 |
| **树倒下落水** | 动画事件 **`AfterFallDown`** 中延迟约 **3s** 播放 **`树掉进水里的声音.mp3`**。 |
| **进入 / 离开树洞** | `ForestEastTreeBridgeStoryMgr.OnEnterOrOutTreeBridge`：进入时 **`waterSoundEntity.ChangeVolumeByRate(0.5f)`**，离开时 **`ResetCurVolume()`**，用于树洞内压低瀑布等环境声。 |

以上 SFX 同样走 **`SoundToggleComponent`** → **`SoundComponentGM`**，文件放在 **`Assets/GameRes/Audio/SFX/`**。

---

## 6. 对话与其它剧情音效

大量 **`ForestEastScene*.prefab`** 对话与剧情由 **NodeCanvas** 等驱动，其中可包含 **播放音效 / 切换 BGM** 的 Action；与具体剧情节点绑定，**不在** `SceneSfxNode` 内统一列举。若某段剧情无声，请到对应 **Dialogue 预制体** 与 **`StoryComponentGSM`** 触发链路上排查。

---

## 7. 场景管理器与资源表（补充）

- **`ForestEastSceneManager`**：负责场景初始化、出口记录等，**不直接**播放 BGM；音乐仍依赖场景内 **`SoundToggleComponent`** 等。
- **`ForestEastSceneResConfig`**：当前为占位 ScriptableObject；**`GameSceneResManager` 内针对 `ForestEastScene` 的 `RegisterConfig` 为注释状态**，若项目后续启用「场景资源预加载表」，需与此处配置对齐后再在文档中补充条目。

---

## 8. 常见问题（FAQ）

**Q：进了 ForestEastScene 没有 BGM？**  
A：检查 **`BGM`** 物体是否激活、`SoundToggleComponent` 的 **`isAutoPlay`**、资源路径与 **`SoundResName`** 是否一致；确认 **`GameManager`** 与 **`SoundComponentGM`** 已初始化完成。

**Q：只有鸟叫没有虫鸣（或反之）？**  
A：确认 **`SceneSfxNode`** 上 **`soundSfxCpn_1` / `soundSfxCpn_2`** 均已赋值；虫鸣应走 **`soundSfxCpn_2`**（与鸟叫分通道）。检查 **`Assets/GameRes/Audio/SFX/`** 下是否存在对应 **`昆虫*.mp3` / `鸟叫*.mp3`**。

**Q：切场景后上一张地图 BGM 还在？**  
A：通常由新场景 BGM 的 **`PlaySound(BGM)`** 内 **`StopBGM`** 处理；若某场景未挂 BGM，需在场景流程序列中显式调用 **`SoundComponentGM.StopBGM`**。

**Q：树桥「嘎吱」声不播？**  
A：核对 **`木头嘎吱嘎吱声 .mp3`** 文件名是否与资源**完全一致**（含空格）；并确认 **`TreeBridgeLogic.soundSfxCpn`** 引用未丢。

---

## 9. 相关文件索引

| 类型 | 路径 |
|------|------|
| 场景 | `Assets/GameRes/Scenes/ForestEastScene.unity` |
| 环境音脚本 | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/ForestEastScene/ForestEastSceneSfxNode.cs` |
| 树桥逻辑 | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/ForestEastScene/TreeBridgeLogic.cs` |
| 树桥事件管理 | `Assets/Scripts/Game/GameMgr/Manager/Story/ForestEastTreeBridgeStoryMgr.cs` |
| 通用播放组件 | `Assets/Scripts/Game/GameRuntime/Audio/SoundToggleComponent.cs` |
| 声音 GM | `Assets/Scripts/Game/GameMgr/Component/SoundComponentGM.cs` |
| 路径枚举 | `Assets/Scripts/Game/Static/Path/SoundPath.cs` |

---

*文档与当前工程内 `ForestEastScene` 配置、`ForestEastSceneSfxNode` 实现一致；若场景或脚本字段变更，请同步更新本文第 2~5 节。*
