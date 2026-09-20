# ForestEast · 树洞爬行嘎吱音效丢失 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / 音频 / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` 树洞内爬行（镜头上下晃时）  
**现象**：原来爬树洞晃动时有 **嘎吱嘎吱**，现在没了  
**产品期望**：洞内爬行晃动按原设计周期性播木头嘎吱；出爬行/出洞停晃合理；资源不丢、不改听感  
**不是**：重做音效；改 BGM；改 CameraAction 抖幅；改死羊/虫卵  
**权威文档**：`Assets/Doc/技术文档/演出相关/ForestEastScene音乐音效系统.md` §5 / FAQ「树桥嘎吱声不播」  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_树洞爬行嘎吱音效丢失_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q3

---

## 沟通摘要

### ① 结论一句话

**主因 A：`TreeBridgeLogic.PlayTreeBridgeMoveSfx` 里写死的资源名已编码损坏，对不上磁盘上的 `木头嘎吱嘎吱声 .mp3`（「声」与 `.mp3` 之间有空格）→ 加载失败 → 镜头仍抖、嘎吱没了。**  
推荐方案 **A**：把字面量改回精确真名，文件用 UTF-8 保存。`soundSfxCpn` 场景引用在；触发链完整。同文件 `AfterFallDown` 的「树掉进水里的声音」**一并乱码**，建议同票顺手修（OPEN Q2）。

### ② 原因（通俗）

爬的时候程序会按名字去找那一声「木头嘎吱」。音频文件还在硬盘上，但代码里那串中文名字坏成乱码了，等于喊错名字，系统找不到就静音。镜头晃还在，只是没声。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 进树洞蹲爬 | 镜头仍上下晃（抖在 → 不是 CameraAction 没进） |
| 2 | 施工后同路径 | **听得见**周期性嘎吱（不能只看 Log） |
| 3 | 停爬 / 出洞 | 晃停；无异常报错 |

### ④ 程序补充

见下文。施工文档：`施工说明/0914/ForestEast_树洞爬行嘎吱音效补全_施工说明.md`（拍板后写）。

---

## 1. 磁盘真源 vs 源码字面量

| 侧 | 内容 |
|----|------|
| **磁盘** | `Assets/GameRes/Audio/SFX/木头嘎吱嘎吱声 .mp3` |
| UTF-8 字节（核） | `E6 9C A8 E5 A4 B4 E5 98 8E E5 90 B1 E5 98 8E E5 90 B1 E5 A3 B0` + **`20`** + `2E 6D 70 33` → **「声」后有空格** |
| meta guid | `7f5d14c2bd7ed4243b88f95ecad7115a`（与提示词一致） |
| **源码现网** | `var moveSfxName = "ľͷ…" / 含 U+FFFD 替换符 … .mp3"`（文件编码损坏） |
| 源码 UTF-8 摘录 | 含 `EF BF BD`（replacement）+ 尾部仍有 `20 2E 6D 70 33`（空格+.mp3 痕迹） |

**裁定：字符串 ≠ 文件名 → FAQ 已写明的「须完全一致」失败。**

技术文档正确名：**`木头嘎吱嘎吱声 .mp3`（含空格）**。

---

## 2. 触发链（完整，未断）

```
playerIsInTreeBridge == true
  → ClimbMoveState：约 0.2s 后 CameraAction()
       ├─ MainCamera Y±0.3 循环抖
       └─ PlayTreeBridgeMoveSfx() → TreeBridgeLogic.PlayTreeBridgeMoveSfx()
            ├─ soundSfxCpn.ChangeSoundRes(错名)  ← 断点
            └─ PlaySound → SoundComponentGM 按名加载 SFX/{resName} 失败
  → 每个抖程 OnComplete 再播一次
  → 离开 ClimbMove：StopCameraAction()
```

| 项 | 现网 |
|----|------|
| `ClimbMoveState` 洞内调 `CameraAction` | ✅ |
| `ForestEastTreeBridgeStoryMgr.CameraAction` 调 `PlayTreeBridgeMoveSfx` | ✅ |
| 场景 `倒树.soundSfxCpn` | ✅ `{fileID: 602751119}`（未 Missing） |

次要嫌疑 B/C/D/E：**仅当改名后仍无声再查**。抖在声无 → 坐实 A，不是停抖施工主因。

---

## 3. AfterFallDown（同票可选）

| 项 | 值 |
|----|-----|
| 磁盘 | `Assets/GameRes/Audio/SFX/树掉进水里的声音.mp3`（**无**尾部空格） |
| 源码 | `AfterFallDown` 内字面量 **同样乱码** |
| 文档 §5 | 延迟约 3s 播该文件 |

**本案以嘎吱为主**；建议施工同票改回 `"树掉进水里的声音.mp3"`，避免倒树落水也静音。

---

## 4. 方案

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A（推荐）** | `moveSfxName = "木头嘎吱嘎吱声 .mp3"`（空格保留）；UTF-8 保存；注释指技术文档与空格陷阱；可选 `[TreeBridgeSfx]` | **默认** |
| B 改资源去空格 | 否决首选（全库/文档按现名） | 否 |
| C Inspector 序列化名 | 增强，非本期必须 | 否 |

**文件列表（仅 1）**：`Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/ForestEastScene/TreeBridgeLogic.cs`

**禁止**：换另一段随便音效；改音频内容；改 Camera 贴底/抖公式；无 Missing 勿动场景 sound 引用。

---

## 5. OPEN

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 嘎吱名是否改回含空格真名？ | **是** | 待施工 |
| Q2 | `AfterFallDown` 落水声是否同票修？ | **是**（已确认乱码） | 待施工 |
| Q3 | 改名后仍无声？ | 再查音量 / `soundSfxCpn` / 是否进 ClimbMove | 待命 |

---

## 6. 侦探声明

未改代码 / 音频 / Git。证据已够拍板方案 A；验收须 **听得见**。
