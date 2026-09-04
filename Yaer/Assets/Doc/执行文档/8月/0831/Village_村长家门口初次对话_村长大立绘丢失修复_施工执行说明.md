# Village_村长家门口初次对话 — 村长大立绘丢失修复 — 施工执行说明

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【执行说明】根因核实 + 修复步骤（**本阶段未改 Prefab YAML / 未代点 Unity 菜单**）  
**Unity**：2020.3.48f1  
**现象**：门口初次对话里 **村长立绘看不见**；Hierarchy 仍有 `村长/ChiefPainting/Face1|2|3`  
**提示词**：`提示词/0831/Village_村长家门口初次对话_村长大立绘丢失修复_施工员提示词.md`  
**配套施工说明**：`施工说明/0831/Village_村长家门口初次对话_村长大立绘丢失修复_施工说明.md`  
**关联**：Mask Face123 施工 · 门口 Prefab Setup · 加载失败修复（Prefab 已落盘）

---

## 沟通摘要

### ① 结论一句话

**根因 H1：母体 `ChiefPainting.prefab` 三脸 `Image.m_Sprite` 全空；曾因 H1b（仅剩 `.meta`、无 png）导致 Setup 绑图失败。现磁盘三张 png 已在且 guid 与 Mask 一致，须重跑 `Setup Chief Painting (UI Big Portrait)` 写回 Sprite；门口 Prefab 嵌套实例无 Sprite Override，会随母体恢复。**

### ② 原因（通俗）

节点还在，但三张脸都没挂图，所以画面上空空。  
多半是缺图时跑过 Setup，空着就存盘了；图后来补回来了，Prefab 却还没重新绑。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | `精灵村长游戏中立绘/` 下有 **三张 png 真文件**（不只 meta） | 组 2 / 闭眼 / 笑颜 |
| 2 | Unity Focus 后无 Texture 粉红丢失 | |
| 3 | 菜单：`Tools / Dialogue / Setup Chief Painting (UI Big Portrait)` | Console **无**「未找到 Sprite」 |
| 4 | 打开 `ChiefPainting`：Face1/2/3 的 Image.Sprite **非空** | |
| 5 | 门口对话 Prefab 内村长立绘肉眼可见 | |
| 6 | Play 村句 | 大立绘出现；Face1/2/3 有图 |
| 7 | Mask 小头像、雅/古 | 回归 OK |

### ④ 程序补充

见下文。

---

## ① 根因裁定（磁盘核实 2026-08-31）

| ID | 假说 | 核实 | 裁定 |
|----|------|------|------|
| **H1** | `ChiefPainting` Image.Sprite 空 | Face1/2/3 均为 `m_Sprite: {fileID: 0}` | ✅ **当前直接原因** |
| **H1b** | 美术 png 曾丢失（只剩 meta） | 提示词预扫时 Glob 仅 meta；**现**目录有三张 png（约 3.1MB / 26KB / 47KB）且 meta guid 未换 | ✅ **历史成因**；A0 若仍缺图须先补 |
| H2 | Setup 空图仍 Save | `LoadSprite` null 时 Warning 仍 `CreateImageLeaf`；与 H1b 连锁 | ✅ 解释为何空着落盘 |
| H3 | 门口实例 Override 清 Sprite | PrefabInstance `guid:6d66031c…` 仅改 Name/Transform/`m_Alpha=1`，**无** Sprite Override | ❌ 非独立根因（继承母体空图） |
| H4 | CanvasGroup alpha=0 | 门口实例已 Override **`m_Alpha=1`**；图前奏含 `ChiefPainting` 淡入到 1 | ❌ 非主因 |
| H5 | Active 错 | Face1 Active=1；Face2/3 Active=0（叠法正确） | ❌ |

**不是**：Hierarchy 缺节点。

---

## ② 贴图源与 guid 对照（须保留 meta）

目录：`Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘/`

| 节点 | 文件 | guid（与 Mask / SR 同源） |
|------|------|--------------------------|
| Face1 | `组 2.png` | `ccb45a9fbac00a74aa47b87fff339497` |
| Face2 | `闭眼.png` | `b62f8898d59d41343bad099499ee6d69` |
| Face3 | `笑颜.png` | `e941e610bcd1f724c869806742279d04` |

| 资产 | Sprite 状态 |
|------|-------------|
| `ChiefMaskPainting.prefab` | ✅ 已绑上述三 guid |
| `精灵村长游戏中立绘.prefab`（SR） | ✅ 已绑 |
| `ChiefPainting.prefab`（UI） | ❌ 全空 |
| `Village_村长家门口初次对话.prefab` 内嵌 | 继承母体 → 亦空 |

旁证：空绑时 `m_SizeDelta` 多为 **100×100**（无 sprite.rect）；重绑后 Setup 会按图尺寸改 SizeDelta。

---

## ③ 修复步骤（施工）

### A0 · 若 png 仍缺失（先做）

1. 恢复 `组 2.png` / `闭眼.png` / `笑颜.png` 到上表目录。  
2. **保留现有 `.meta`（勿删勿换 guid）**，否则 Mask/SR 全断。  
3. 来源：`git` / 备份 / 从同目录 `精灵村长游戏中立绘.psd` 导出同名层。  
4. Unity `Assets → Refresh`，确认可作 Sprite。

**当前磁盘**：三张 png **已存在** → A0 可标完成；仍须做 A。

### A · 重绑 UI 大立绘（必做）

1. Unity：`Tools / Dialogue / Setup Chief Painting (UI Big Portrait)`  
   - 脚本：`ChiefPaintingSetupEditor.CreateOrUpdatePrefab`  
2. Console：**不得**出现 `[ChiefPaintingSetup] 未找到 Sprite：…`  
3. 核母体：三脸 `m_Sprite` 指向上表 guid；Face1 开、Face2/3 关。  

### B · 同步门口对话（通常自动）

- 门口为 **PrefabInstance** 引用母体 `6d66031c…`，无 Sprite Override → 母体修好后实例即有图。  
- 若编辑器仍显示旧空图：重开 Prefab / Apply；仅当仍空再跑  
  `Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab`（会重建嵌套，注意覆盖站位）。

### 明确不做

- ❌ 不恢复 png 只空跑 Setup  
- ❌ 删 meta 换新 guid  
- ❌ 只改站位不绑图；删 `ChiefPainting` 节点  
- ❌ 把 SR 版再嵌 Dialogue 容器  
- ❌ 改雅/古立绘、重做 Face123 Import / Trigger  

---

## ④ 前奏 / Alpha（H4 说明）

| 项 | 状态 |
|----|------|
| 图内三路淡入 | 含 `ChiefPainting` → EndAlpha=1 |
| 实例 Override | `m_Alpha = 1` |
| 结论 | 修好 Sprite 后应可见；若仍无，再查运行时 `Apply`/Active |

---

## ⑤ 验收

- [ ] ArtFolder 三张 png 真文件存在  
- [ ] `ChiefPainting` 三脸 Sprite 非空（YAML/`fileID: 21300000, guid:…`）  
- [ ] 门口 Prefab 内村长立绘肉眼可见  
- [ ] Play：村句大立绘 + Face1/2/3 有图  
- [ ] Mask、雅/古回归 OK  

---

## ⑥ OPEN

| ID | 问题 | 状态 |
|----|------|------|
| Q1 | 根因？ | ✅ **H1**（空 Sprite）；**H1b** 为历史成因，png 现已回 |
| Q2 | 是否须重跑门口 Setup？ | 默认否；母体修好即继承 | ⏳ 验收定 |
| Q3 | png 从何处恢复？ | 施工说明填写（本机已有文件则写「磁盘已补齐」） | ⏳ |

---

## ⑦ 程序速查

| 路径 | 用途 |
|------|------|
| `ChiefPaintingSetupEditor.cs` | UI 大立绘 Setup；`LoadSprite(ArtFolder+文件名)` |
| `ChiefPainting.prefab` | 空 Sprite 母体（待重绑） |
| `ChiefMaskPainting.prefab` | 同源 guid 金样 |
| `Village_村长家门口初次对话.prefab` | PrefabInstance → ChiefPainting |
| `精灵村长游戏中立绘/*.png` | 贴图真源 |
