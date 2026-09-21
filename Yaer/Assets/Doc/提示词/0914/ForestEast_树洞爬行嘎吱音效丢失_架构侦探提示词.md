# Cursor Agent Prompt · ForestEast 树洞爬行晃动：嘎吱音效丢失补全

> **角色**：先【架构侦探】一页核实触发链 + 文件名是否对得上；默认【施工员】把乱码资源名改回真文件名（证据已够）  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene` 树洞内爬行（镜头上下晃时）  
> **现象（用户）**：原来爬树洞晃动时有 **嘎吱嘎吱** 音效，现在没了  
> **产品期望（钉死）**：洞内进入爬行晃动后，按原设计周期性播放木头嘎吱声；出爬行/出洞停止合理；资源文件不丢、不改听感  
> **不是**：重做音效；改 BGM；改 CameraAction 抖幅当主修；改死羊/虫卵  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_树洞爬行嘎吱音效丢失_架构溯源报告.md`（可短）  
> **权威文档**：`Assets/Doc/技术文档/演出相关/ForestEastScene音乐音效系统.md` §5 / FAQ「树桥嘎吱声不播」

把下面整段交给 Cursor Agent。

---

## 提示词助手预梳理（施工前快速核，根因几乎已钉死）

### 产品白话

> 在树洞里爬的时候镜头一晃，以前会嘎吱嘎吱响，现在没声了。别重做音频，把原来那条播放链路和文件名对上就行。

### 音效位置（磁盘）

| 项 | 路径 / 名 |
|----|-----------|
| **真源文件** | `Assets/GameRes/Audio/SFX/木头嘎吱嘎吱声 .mp3` |
| **注意** | 文件名在「声」与 `.mp3` 之间有一个 **空格**：`声 .mp3` |
| UTF-8（助手核） | `木头嘎吱嘎吱声` + `0x20` + `.mp3` |
| meta guid | `7f5d14c2bd7ed4243b88f95ecad7115a` |

### 触发方法（调用链）

```
洞内 playerIsInTreeBridge == true
  → 玩家进入 ClimbMoveState（蹲爬移动）
  → 约 0.2s 后首次：ForestEastTreeBridgeStoryMgr.CameraAction()
       ├─ 抖 MainCamera（Y±0.3 循环）
       └─ PlayTreeBridgeMoveSfx()
            └─ TreeBridgeLogic.PlayTreeBridgeMoveSfx()
                 ├─ soundSfxCpn.ChangeSoundRes(moveSfxName)
                 └─ soundSfxCpn.PlaySound()
                      └─ SoundComponentGM.PlaySound(SFX, resName, …)
                           └─ 按名加载 Assets/GameRes/Audio/SFX/{resName}
  → 每个抖程 OnComplete 再 PlayTreeBridgeMoveSfx() 一次
  → 离开 ClimbMove / 出洞：StopCameraAction()（停抖；不负责关错名）
```

| 脚本 | 职责 |
|------|------|
| `ClimbMoveState.cs` | 洞内爬行启动/停止 `CameraAction` |
| `ForestEastTreeBridgeStoryMgr.cs` | `CameraAction` / `PlayTreeBridgeMoveSfx` / `StopCameraAction` |
| `TreeBridgeLogic.cs` | **写死** `moveSfxName` + `soundSfxCpn` 播放 |
| 场景 `倒树` | `TreeBridgeLogic.soundSfxCpn` → fileID `602751119`（须核未丢） |

### 根因（助手预扫 · 须打开文件确认）

`TreeBridgeLogic.PlayTreeBridgeMoveSfx` **现网字面量已乱码**：

```csharp
var moveSfxName = "ľͷ…֨… .mp3";  // 源文件编码损坏，不是「木头嘎吱嘎吱声 .mp3」
```

技术文档写明正确名是 **`木头嘎吱嘎吱声 .mp3`（含空格）**；FAQ：**文件名须与资源完全一致，否则不播**。  
磁盘上 UTF-8 文件名正确；**C# 字符串因编码损坏对不上** → `ChangeSoundRes` 换成错误名 → `PlaySound` 加载失败 → **晃还在、嘎吱没了**（或静默失败）。

同文件 `AfterFallDown` 里「树掉进水里的声音」也可能乱码——**本案以嘎吱为主**；若侦探确认一并乱码，可同票最小修（OPEN）。

### 次要嫌疑（仅当改名后仍无声再查）

| # | 嫌疑 | 查法 |
|---|------|------|
| B | `soundSfxCpn` Missing | Inspector 倒树上引用 |
| C | `CameraAction` 根本没进（不在洞内 / 未 ClimbMove） | 有无镜头抖；`playerIsInTreeBridge` |
| D | 0914 `StopCameraAction` 贴底导致立刻停抖 | 抖是否还在；在则不是主因 |
| E | 音量/静音/SFX 通道关 | SoundComponentGM / 用户设置 |

### 方案（推荐 A）

| 方案 | 做法 |
|------|------|
| **A（推荐）** | 把 `moveSfxName` 改回 **精确** `"木头嘎吱嘎吱声 .mp3"`（含空格）；文件 **UTF-8 with BOM 或 UTF-8** 保存，防再坏；加注释指向技术文档与空格陷阱 |
| **B** | 改资源文件名为无空格 ASCII，再改代码——**否决首选**（全库引用/文档都按现名） |
| **C** | 序列化到 Inspector 字符串代替硬编码——可作增强，非本期必须 |

硬约束：

- **禁止**换另一段随便音效交差  
- **禁止**为修音效改 Camera 贴底/抖公式  
- 验收必须 **听得见**，不能只看 Log 路径拼对  

### 侦探须回答（可极短）

1. 源码字符串 vs 磁盘文件名是否一致？（贴两侧字面）  
2. `soundSfxCpn` 是否赋值？  
3. Play 爬行时镜头是否仍抖？（抖在声无 → 坐实 A）  
4. `AfterFallDown` 是否同乱码？本期是否顺手修？  

### 必读

1. `TreeBridgeLogic.PlayTreeBridgeMoveSfx`  
2. `ForestEastTreeBridgeStoryMgr.CameraAction`  
3. `ClimbMoveState`（洞内调用）  
4. `Assets/GameRes/Audio/SFX/木头嘎吱嘎吱声 .mp3`  
5. `ForestEastScene音乐音效系统.md` §5 / FAQ  
6. 本提示词  

---

## 【施工员】（侦探不反对即可跑）

> **目标**：洞内爬行晃动时恢复嘎吱循环声。  
> **写入**：仅 `TreeBridgeLogic.cs` —— `moveSfxName = "木头嘎吱嘎吱声 .mp3"`（空格保留）；注释说明编码/空格；可选同修 `AfterFallDown` 的 `树掉进水里的声音.mp3`（若确认乱码）。  
> **禁止**：改音频文件内容；改 CameraAction 曲线；改场景 sound 引用除非 Missing。  
> **文档**：`Assets/Doc/施工说明/0914/ForestEast_树洞爬行嘎吱音效补全_施工说明.md`  
> **验收**：  
> 1. 进树洞蹲爬 → 镜头晃 **且** 周期性嘎吱  
> 2. 停爬 / 出洞 → 晃停；无异常报错  
> 3. Console 可打 `[TreeBridgeSfx] play name=…` 确认名与磁盘一致  
> Debug 前缀：`[TreeBridgeSfx]`
