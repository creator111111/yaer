# Cursor Agent Prompt · LoadGamePanel 列表遮罩失效（上下漏字）

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-06  
> **范围**：`LoadGamePanel` 读档列表滚动区 **裁剪/遮罩失效**——Viewport 外上下仍能看见存档标题文字。不扩读档业务、不改存档数据。  
> **本阶段**：只摸清 Mask / RectMask2D / Viewport 几何与 Prefab 配置，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 复现（已对齐截图 · 2026-08-06）

| 项 | 状态 |
|----|------|
| 界面 | `LoadGamePanel(Clone)` 读档列表 |
| 分辨率 | Game 视图 **4K UHD (3840×2160)**（截图标注） |
| 上沿 | 装饰分隔线**上方**仍可见上一档文字（如「序章：拉普路西」「游戏时长：…」）——红框标出 |
| 下沿 | 装饰分隔线**下方**仍可见下一档文字（如「存档4 / 存档5」「序章：拉普路西」等）——红框标出 |
| 期望 | 超出列表可视框（Viewport）的 Content 行应被裁掉，只露框内 |

→ 用户体感：**「遮罩失效了」**；上下都漏，不是单边 padding 问题。

### Prefab 静态线索（侦探必须在 Hierarchy/序列化里复核，勿当唯一真相）

`Assets/GameRes/Prefabs/UI/LoadGamePanel.prefab` 现网扫到：

| 节点 | 线索 |
|------|------|
| `Scroll View` | 标准 `ScrollRect`；`m_Viewport` → Viewport；`m_Content` → Content；纵向滚动 |
| **`Viewport`** | 同时挂了 **`Mask`**（`Show Mask Graphic=0`）**和** **`RectMask2D`**（`Softness ≈ (0, 100)`） |
| Viewport Rect | `AnchoredPosition.y ≈ -51.7`，`SizeDelta.y ≈ -185.9`（相对父级内缩） |
| `Content` | `GridLayoutGroup`（Cell≈1560×250）；`ContentSizeFitter`；锚点/坐标有 `AnchoredPosition.x ≈ -960` 等异常感——须核是否运行时被改 |
| 同构对照 | `SaveGamePanel.prefab` Viewport **同样** Mask+RectMask2D 且 Softness y=100 —— **侦探须对比存档页是否也漏字** |

### 高度可疑根因（优先验证，可证伪）

1. **`RectMask2D.Softness.y = 100` 过大**：软边把裁切区「糊」出硬框外，上下漏半透明/仍可读文字；看起来像遮罩坏了。Shop 文档建议软边约 24～40，此处 100 明显偏大。  
2. **同一 Viewport 上 Mask + RectMask2D 叠用**：部分机型/分辨率下 stencil 与矩形裁剪互相干扰或一方失效；应比选「只留其一」。  
3. **Viewport 几何与装饰框不一致**：装饰线在视觉上是「列表边」，实际裁剪矩形更大/错位，字仍在 Viewport 内 → 用户以为漏出。  
4. **4K / CanvasScaler**：参考分辨率与 Mask 精灵、软边像素尺度放大后体感更严重（须对照 1080p 是否也漏）。  
5. **行 Prefab 逃逸裁剪**：`ButtonArchive` 若挂独立 Canvas / `overrideSorting` / `maskable=false` / 特殊材质，会绕过 Mask（脚本侧未见，须查 Prefab）。

### 非本期（勿扩）

- 读档/删档逻辑、`LoadGameFormProxy`、Procedure 读档流程  
- 存档标题文案、三语  
- 商店 Scroll Softness 大改造（仅可作**对照样例**）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/GameRes/Prefabs/UI/LoadGamePanel.prefab
@Assets/GameRes/Prefabs/UI/SaveGamePanel.prefab
@Assets/GameRes/Prefabs/UI/Control/ButtonArchive.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Archive/LoadGamePanel/LoadGameFormLogic.cs
@Assets/Doc/执行文档/7月/0713/Shop_BuySell列表滚轮边界虚化过渡_架构溯源与施工执行说明.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 打开读档界面 `LoadGamePanel`，滚动列表时**上下两边框外的字还会露出来**（截图上下都有红框）。
2. 像是列表遮罩/裁剪坏了：框外的存档行标题、时长不该看得见。
3. 目标：钉死是 Softness、双 Mask 叠用、Viewport 尺寸，还是行 Prefab 逃逸；给出**最小 Prefab/组件修复**建议，不重写读档逻辑。

---

## 必读 / 优先扫描线索

### A. 钉死裁剪责任组件
- Viewport 上：`Mask` 是否 Enabled、Image/Sprite 是否有效（Mask 依赖 Graphic）  
- `RectMask2D`：Enabled、Padding、**Softness** 实测值  
- 是否应只保留一种裁剪（Mask **或** RectMask2D）  
- 运行时 Clone 上组件是否被脚本关掉（`LoadGameFormLogic` 现网未见改 Mask——仍须全链路搜）

### B. Viewport 几何 vs 视觉「列表框」
- Scroll View / Viewport / 装饰分隔线的 Rect 关系（谁在框内谁在框外）  
- 漏出的字：是在 **Viewport 矩形外**（真失效），还是仍在 Viewport 内、只是越过装饰线（几何错位）  
- 画一张简图：父级 → Scroll View → Viewport → Content → ButtonArchive

### C. Softness / 分辨率
- Softness (0,100) 在 1080p vs 4K 下泄漏宽度  
- 对照 Shop `RectMask2D` Softness 经验值（约 24～40）  
- Softness 归零后是否硬裁正常（脑内/静态推演 + 建议验收步骤）

### D. Content / 行 Prefab
- `UpdateArchiveButton`：`SetParent(contentTransform)`、scale=1；有无改父级到 Viewport 外  
- `ButtonArchive.prefab`：有无 Canvas、`Graphic.maskable`、自定义材质、TMP 特殊 shader  
- Content 的 GridLayout / SizeFitter / 初始 AnchoredPosition 是否导致异常布局（次要）

### E. 对照 SaveGamePanel
- 存档页是否同样双组件 + Softness 100？打开是否也漏字？  
- 若两边一起坏 → 公共 Prefab 模板问题；若仅 Load → 查 Load 独有差异

### F. 范围冻结
- **可改建议**：Viewport 裁剪组件配置、Softness、去掉冗余 Mask/RectMask2D 之一、微调 Viewport 高度对齐装饰框  
- **不改**：读档业务、存档槽数量算法、无关 UI 面板（除非证实同模板批量坏）

---

## 侦探任务清单

1. **结论一句话**：遮罩失效的根因（Softness / 双裁剪 / Viewport 错位 / 行逃逸 四选一或组合）。

2. **现网结构图**  
   `LoadGamePanel → Scroll View → Viewport(组件列表) → Content → ButtonArchive×N`

3. **证据表**

   | 检查项 | Prefab/运行时事实 | 是否足以解释上下漏字 |
   |--------|-------------------|----------------------|
   | Mask | | |
   | RectMask2D Softness | | |
   | Viewport Rect vs 装饰线 | | |
   | ButtonArchive 逃逸 | | |
   | SaveGame 对照 | | |

4. **方案比选表**（至少 3 档，推荐 1 个）

   | 方案 | 做法摘要 | 改动面 | 风险 | 推荐？ |
   |------|----------|--------|------|--------|
   | A | Softness 改为 (0,0) 或小值（如 0～24），先恢复硬裁 | | | |
   | B | 只保留 RectMask2D（或只保留 Mask），去掉另一个 | | | |
   | C | 校正 Viewport Size/Pos 对齐装饰可视框 | | | |
   | D | 修 ButtonArchive maskable/Canvas 逃逸 | | | |

5. **施工员最小改动清单**（只建议）  
   - 优先只动 `LoadGamePanel.prefab`（及若对照确认则同步 `SaveGamePanel`）  
   - 避免改 `LoadGameFormLogic` 业务，除非证实代码关遮罩

6. **验收清单**  
   - 打开 Load：滚到顶/底，装饰线外**无**可读存档文字（上沿、下沿）  
   - 框内列表正常滚动、点选、双击读档、删除仍可用  
   - 1080p 与 4K（或常用分辨率）各验一次  
   - 若拍板同步：SaveGamePanel 同样不漏字

7. **开放问题**追加 OPEN（「LoadGamePanel 列表遮罩 · 2026-08-06」）：  
   - Softness 要硬切还是保留轻微软边？  
   - SaveGame 是否一并修？  
   - Mask 与 RectMask2D 最终只留哪个？

8. **禁止**：重写 ScrollRect 列表系统；在 Update 里手动藏字；为修裁剪改读档存档逻辑。

---

## 输出要求

写入：`Assets/Doc/执行文档/0806/LoadGamePanel_列表遮罩失效_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（根因 + 推荐方案）  
② 原因（生活类比：窗帘短了 / 窗帘纱太透）  
③ 用户需要做什么（拍板 Softness/是否同步 Save + 验收）  
④ 给程序看的补充：结构图、证据表、方案表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认 Softness / 单裁剪组件 / Viewport 几何哪条后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0806/LoadGamePanel_列表遮罩失效_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，修复 LoadGamePanel 列表上下漏字（遮罩/裁剪失效）。
优先改 Prefab 裁剪配置；不改读档业务逻辑。若报告认定 SaveGame 同源，可一并修。
禁止在 Update 堆补丁。每次提交说明：改了哪些文件、裁剪如何恢复、如何验证。
```
