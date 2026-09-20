# SystemTipsPanel2 · Missing Script 导致无法安心改 Prefab — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改** Prefab / 代码 / 图集 / Git  
**Unity**：2020.3.48f1  
**目标 Prefab**：`Assets/GameRes/Prefabs/UI/SystemTipsPanel2.prefab`  
**现象**：根上 **Missing (Mono Script)**，黄叹号「The associated script can not be loaded…」；要改 BOSS 战前保存提示时不敢 Apply  
**产品期望**：  
1. 查清 Missing 是哪个脚本（GUID / 是否本就不该存在）  
2. 恢复 Prefab 可正常编辑、保存  
3. 运行时 BOSS 前 `SystemTipsPanel2` 仍可用（`OpenSystemTipsPanel2WaitSureActionTask` / Save）  
4. 顺带说明 `Img Tips Content = None` 与换 `SaveChar` 的关系  
**不是**：重做整套 SystemTips；改对话树；大改 `SystemTipsPanel`（v1）  
**提示词**：`Assets/Doc/提示词/0914/SystemTipsPanel2_MissingScript_架构侦探提示词.md`  
**OPEN**：`OPEN_QUESTIONS.md` 本节 Q1～Q4

---

## 沟通摘要

### ① 结论一句话

Missing 对应 GUID **`8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6`**：**全库无任何 `.meta`**，形态像手填占位，**从未进库的有效脚本**。推荐 **方案 A：删除该孤儿 MonoBehaviour YAML 块**（勿补脚本、勿写假 GUID）。另：**`imgTipsContent` 现为 None，且 Panel2 缺少 v1 的 `ImageContent` 节点**——与 Missing **无关**，但按现网 `UpdateInfo` **一开面板就会 NRE**；换 `SaveChar` 走图集，仍须有可绑的 Image。建议本期 **一并按 v1 补节点并绑槽**（OPEN Q1）。

### ② 原因（通俗）

根上多挂了一段「脚本引用」，指向一个仓库里从来不存在的 GUID，Unity 只能画黄叹号。真正干活的三个脚本（FormLogic / ComponentSystemUI / CursorChangeUI）都还在，BOSS 弹窗逻辑也不读这块。删掉黄槽就能安心改 Prefab。提示文案图不是拖在 Inspector 里的，是运行时从图集取 `SaveChar` 再赋给 `imgTipsContent`；槽空着或节点没了，换图也显示不出来，还会空引用。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网（未施工） |
|---|------|----------------|
| 1 | 打开 `SystemTipsPanel2` Prefab | 根上黄 Missing（或 YAML 残留） |
| 2 | 对照 `SystemTipsPanel` | 同套三脚本；**无**第四 MB；有 **`ImageContent`** |
| 3 | Panel2 Hierarchy | **无** `ImageContent`；有禁用全屏 `Image`（遮罩类，勿当内容图） |
| 4 | FormLogic → Img Tips Content | **None** |
| 5 | 拍板施工 | 删 Missing YAML；建议同期补绑 `imgTipsContent`（见方案） |
| 6 | 换 Save 文案图 | 改 `ArtRes/UI/Form/SystemTips/Char*/SaveChar.png` → Pack `tipsChar*` 图集；**不是**拖 Prefab 槽 |

### ④ 程序补充

见下文。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| Missing GUID | `8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6` |
| `.meta` 命中 | **0**（全 `Assets` 检索） |
| 其它资源引用 | **仅** `SystemTipsPanel2.prefab`（+ 本提示词/报告） |
| 序列化字段 | 仅 `buttonSure → ButtonSure`（与 FormLogic.`btnSure` **重复**） |
| 根 `m_Component` | 脚本侧仅 FormLogic / ComponentSystemUI / CursorChangeUI；**未列入** Missing 的 fileID（YAML **孤儿块**仍在文件末尾） |
| 与 Panel1 | Panel1 **无**该第四组件；有 `ImageContent` + `imgTipsContent` 已绑 |
| BOSS / ActionTask | **不依赖** Missing；只认 `SystemTipsFormLogic` + `proxy.onSure/onCancel` |
| 换 SaveChar | `SystemTipsFormProxy.UpdateTips(Save)` → `tipsChar*` 图集名 **`SaveChar`**；Prefab 槽是运行时接收 Image，**不是**美术源 |
| **推荐** | **A**：删孤儿 YAML；**同期**按 Panel1 补 `ImageContent` 并绑 `imgTipsContent`（或拍板 Q1「只删 Missing、绑槽另案」） |
| **禁止** | 新建假 GUID 脚本「消黄」；乱挂无关 MB；本期重做 UI / 改对话图 |

---

## 2. 证据

### 2.1 Missing 块（Panel2 根末尾）

```yaml
--- !u!114 &5728391048567291034
MonoBehaviour:
  m_GameObject: {fileID: 9139198441890940729}   # SystemTipsPanel2 根
  m_Script: {fileID: 11500000, guid: 8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6, type: 3}
  buttonSure: {fileID: 8121383294505878709}     # 同根 ButtonSure
```

- GUID 后缀 `…a1b2c3d4e5f6`：**连续占位形**，非典型 Unity 随机 GUID。  
- 全库 `guid: 8f4e2a1b…`：**无** `.cs.meta`。  
- `git log -S "8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6"`：未见「曾有真实脚本后删除」的独立提交痕迹；在已有提交 `2505bfb6`（bug修完推送）的 Prefab 快照里 **已是** 该 GUID + `imgTipsContent: {fileID: 0}`。  
- 嫌疑 **A（无效占位）** 成立；**B（删脚本忘改 Prefab）** 证据不足（无 meta、无类名、字段仅 buttonSure）；**C（编译失败）** 不适用（合法脚本 GUID 均有 meta 且可加载）。

### 2.2 根上仍有效的三个脚本

| 组件 | GUID | 脚本 |
|------|------|------|
| SystemTipsFormLogic | `47b2f842286746b19605853e8d0e7a1d` | `…/SystemTips/SystemTipsFormLogic.cs` |
| ComponentSystemUI | `05b7f39879134aed846421d81b8ad7b1` | `…/UI/Component/ComponentSystemUI.cs` |
| CursorChangeUI | `2578ec541db03504c856a946a0482fa9` | `…/Cursor/CursorChangeUI.cs` |

FormLogic 序列化（Panel2）：

| 字段 | Panel2 | Panel1 |
|------|--------|--------|
| imgAvatar | 有（ImageAvatar） | 有 |
| **imgTipsContent** | **`{fileID: 0}`** | **`ImageContent` 的 Image** |
| btnCancel / btnSure / btnConfirmExit | 有 | 有 |

### 2.3 Hierarchy：Panel2 缺 `ImageContent`

| 节点 | Panel1 | Panel2 |
|------|--------|--------|
| ImageTipsBG | 有（背景框） | 有；`m_Children: []` |
| **ImageContent** | **有**（RootOrder 1，绑 `imgTipsContent`） | **整节点不存在** |
| Image（全屏） | 有（常作暗底） | 有；**`m_IsActive: 0`**，1920×1080，半透明 — **不是** 提示文案槽 |

结论：`imgTipsContent = None` **不是**「忘拖一下」那么简单——**承接文案的 Image 物体本身在 Panel2 上缺失**。

### 2.4 运行时依赖（Save / BOSS）

```
OpenSystemTipsPanel2WaitSureActionTask
  → UIComponentGM.OpenUIForm(SystemTipsPanel2, userData=ESystemTipsType.Save)
  → SystemTipsFormLogic.OnOpen
       proxy.UpdateTips(Save)
         SystemTipsFormProxy：tipsChar / _en / _jp → GetSprite("SaveChar")
       onUpdateTips → UpdateInfo
         imgTipsContent.sprite = args.charSprite;   // ★ null → NRE
         imgTipsContent.SetNativeSize();
  → 等待 proxy.onSureEvent / onCancelEvent
```

- ActionTask / `DialoguePreBossSaveTipSettings` / Boss 文档 **均不读** Missing 组件。  
- 美术源路径：`Assets/ArtRes/UI/Form/SystemTips/Char/SaveChar.png`（及 `_en` / `_jp`）→ Pack `Assets/GameRes/Atlas/SystemTips/tipsChar*.spriteatlas`。  
- Prefab 上 `Img Tips Content`：**只负责显示** Proxy 传来的 Sprite；拖一张预览图进槽 **不会** 成为多语言权威源。

### 2.5 孤儿块与 `m_Component`

根 `m_Component` **未列出** `5728391048567291034`，但同文件仍保留完整 `!u!114` 块且 `m_GameObject` 指向根。这会造成：

- Inspector 仍可能出现 Missing（Unity 对孤儿 MB 的表现因版本/导入而异）；  
- 即使用户曾在列表里 Remove 过，**YAML 残留**也会让「改不动 / 不敢存」感持续。  

施工必须以 **删除整段 YAML** 为准，再确认 Inspector 无黄叹号。

---

## 3. 影响面

| 面 | 影响 |
|----|------|
| 编辑器 | Missing 黄叹号 → 不敢 Apply；删孤儿后与 Panel1 组件差对齐，可正常改 |
| 运行时弹窗逻辑 | Missing **无影响**（FormLogic 已绑按钮） |
| 运行时显示 Save 文案 | **`imgTipsContent` 空 + 无 ImageContent → UpdateInfo NRE / 无图**（比 Missing 更致命） |
| 换 SaveChar 素材 | 改 ArtRes + Pack 图集；**不**靠 Prefab 槽当源；但槽必须指向有效 Image |
| Panel1 | 本期不动；仅作对照模板（尤其 `ImageContent` 布局） |

---

## 4. 方案对比与推荐

### 方案 A（推荐）：删 Missing + 补 `ImageContent` 并绑槽

1. 打开 `SystemTipsPanel2.prefab`（或文本删 YAML）：删除 `&5728391048567291034` 整块；确认根 `m_Component` 无该 fileID。  
2. 对照 Panel1：在根下新增 **`ImageContent`**（RectTransform / CanvasRenderer / Image），布局参考 Panel1（约 AnchoredPos `(156.4, -33)`，Size `731×344`；以视觉对齐为准可微调）。  
3. FormLogic.`imgTipsContent` → 该 Image。  
4. **不要**把禁用全屏 `Image` 绑成 tips 内容。  
5. 保存 Prefab；Play：`WestRappRoadGoblinAndGusha` 弹窗 → 见 SaveChar → 确认/取消正常；Console 无 NRE。

**优点**：一次消黄 + 修显示/NRE。  
**缺点**：比「只删一行」多一步 UI 节点（仍属最小补齐，非重做）。

### 方案 B：只删 Missing，绑槽另开任务

仅删孤儿 YAML。编辑器可安心改；**运行时 Save 仍可能 NRE**，换图仍看不见。仅当产品确认「本期只消黄、弹窗另验」时用（默认 **不推荐**）。

### 方案 C：补「真实脚本」消黄

为假 GUID 新建 `.cs` 并改 meta GUID，或挂无关脚本。  
**否决**：无类名、无业务、字段与 FormLogic 重复；会制造第二套 button 引用。

### 方案 D：整份用 Panel1 覆盖 Panel2

**否决**：超出范围；可能冲掉 Panel2 已有差异（若有意保留的布局/按钮态）。

---

## 5. 施工步骤（给施工员，拍板后）

1. **删** Prefab 中 `guid: 8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6` 对应 `!u!114 &5728391048567291034` 整段。  
2. 按 OPEN **Q1** 默认：从 Panel1 **复制/重建 `ImageContent`**，绑到 `imgTipsContent`。  
3. 不动：FormLogic / ComponentSystemUI / CursorChangeUI；不动对话树；不改图集除非用户另开换图任务。  
4. 文档：`Assets/Doc/施工说明/0914/SystemTipsPanel2_MissingScript_施工说明.md`。

### 验收

| # | 项 | 期望 |
|---|----|------|
| 1 | Prefab Inspector | **无** Missing / 黄叹号 |
| 2 | 组件差 vs Panel1 | 同三脚本；无第四 MB |
| 3 | `imgTipsContent` | 非 None（若按 A） |
| 4 | Play BOSS 前保存提示 | 弹出、有文案图、确认/取消结束 Action |
| 5 | Console | 无 `UpdateInfo` NRE |

---

## 6. 替代方案说明（复杂逻辑）

| 路径 | 说明 |
|------|------|
| **权威（A）** | 删假 GUID 孤儿；补 Image 接收图集 Sprite |
| **仅消黄（B）** | 编辑器修好；运行时显示债留下 |
| **改 FormLogic 空判断** | 可防 NRE，但 **仍无文案图**；治标不治本，本期不推荐当主方案 |
| **换图** | ArtRes `SaveChar` + Pack atlas；与 Missing **正交** |

---

## 7. 参考路径

| 用途 | 路径 |
|------|------|
| Prefab | `Assets/GameRes/Prefabs/UI/SystemTipsPanel2.prefab` |
| 对照 | `Assets/GameRes/Prefabs/UI/SystemTipsPanel.prefab` |
| Logic / Proxy | `…/SystemTips/SystemTipsFormLogic.cs`、`SystemTipsFormProxy.cs` |
| ActionTask | `…/OpenSystemTipsPanel2WaitSureActionTask.cs` |
| Boss 说明 | `Assets/Doc/技术文档/Boss/BOSS战提示系统.md` |
| Save 源图 | `Assets/ArtRes/UI/Form/SystemTips/Char*/SaveChar.png` |
| 图集 | `Assets/GameRes/Atlas/SystemTips/tipsChar*.spriteatlas` |
