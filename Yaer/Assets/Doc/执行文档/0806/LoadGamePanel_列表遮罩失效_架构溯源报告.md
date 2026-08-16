# LoadGamePanel 列表遮罩失效（上下漏字）— 架构溯源报告

**文档版本**：v1.0（2026-08-06）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 场景 / CSV / 台本**）  
**范围**：`LoadGamePanel` 读档列表滚动区裁剪/遮罩失效——Viewport 外上下仍能看见存档标题文字。不扩读档业务、不改存档数据。  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0806/LoadGamePanel_列表遮罩失效_架构侦探提示词.md`
- Prefab：`LoadGamePanel` / `SaveGamePanel` / `ButtonArchive`
- 对照：`0713/Shop_BuySell列表滚轮边界虚化过渡_…`（RectMask2D Softness 经验值）

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**根因高度坐实为 Viewport 上 `RectMask2D.Softness = (0, 100)` 过大：上下各约 100px 软边把框外文字糊成仍可读，体感像「遮罩坏了」；同节点还叠了 `Mask`（冗余）。推荐方案 A：先把 Softness 改为 `(0,0)` 或 Shop 级 `(0, 24～32)` 恢复硬/轻软裁；并建议同步 `SaveGamePanel`（配置完全同源）。行 Prefab 逃逸与业务脚本关遮罩可排除。**

---

## ② 原因（生活类比）

### 生活类比

列表可视框像一扇窗户，窗帘本该把窗外的字挡住。现在用的是**上下各拖出很长一段薄纱（Softness Y=100）**：窗外的上一档/下一档标题隔着纱还能念出来——不是窗户没装，是纱帘太透、拖得太长。Shop 列表用的是短纱（约 24～40），读档页却用了 100。

### 复现对齐

| 项 | 状态 |
|----|------|
| 上沿 | 装饰线上方仍见上一档标题/时长 |
| 下沿 | 装饰线下方仍见下一档标题 |
| 分辨率 | 截图为 4K；软边像素感在高分下更显眼（须 1080p 对照） |
| 期望 | Content 超出 Viewport 硬框应裁掉 |

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板

1. **Softness**：要**硬切 `(0,0)`**，还是保留轻微软边 **`(0, 24～32)`**（对齐 Shop）？  
   - 默认建议：先 `(0,0)` 验收「完全不漏」；若要虚化再收到 24～32。  
2. **SaveGamePanel**：是否一并改？（建议：**是**——Viewport 同为 Mask+RectMask2D、`Softness y=100`）  
3. **双裁剪**：最终只留 `RectMask2D`（去 Mask），还是 Softness 修好后双组件都留？（建议：主修 Softness；有余力去 Mask、只留 RectMask2D，对齐 Shop）

### 验收清单

1. 打开 Load：滚到顶/底，装饰线外**无**可读存档文字（上、下）。  
2. 框内滚动、点选、双击读档、删除仍可用。  
3. 1080p 与 4K（或常用分辨率）各验一次。  
4. 若拍板同步：SaveGamePanel 同样不漏字。

---

## ④ 给程序看的补充

### 4.1 现网结构图

```
LoadGamePanel
 └─ … / Scroll View          ← ScrollRect（纵向；Viewport+Content 已绑）
      ├─ Viewport              ← Image(UIMask 精灵) + Mask(ShowMaskGraphic=0)
      │                        + RectMask2D(Padding=0, Softness=(0,100))  ★
      │     └─ Content         ← GridLayoutGroup(Cell≈1560×250) + ContentSizeFitter
      │           └─ ButtonArchive × N   ← SetParent(contentTransform)；maskable=1
      └─ Scrollbar Vertical
```

**几何**：Viewport 相对 Scroll View 拉伸锚点，`AnchoredPosition.y≈-51.7`，`SizeDelta.y≈-185.9`（上下内缩）。装饰线在视觉上是「列表边」；漏字若在 Viewport 矩形外 → 裁剪/软边问题；若仅越过装饰线仍在 Viewport 内 → 几何错位（次要假说，见证据表）。

### 4.2 证据表

| 检查项 | Prefab/运行时事实 | 是否足以解释上下漏字 |
|--------|-------------------|----------------------|
| **Mask** | Viewport 上 Enabled；`Show Mask Graphic=0`；Image 有内置 UIMask Sprite (`10917`) | 单独不像「全失效」；与 RectMask2D **叠用**属冗余/干扰风险 |
| **RectMask2D Softness** | **`m_Softness: {x: 0, y: 100}`**；Padding=0 | **足以**：上下对称泄漏；Shop 文档建议纵向约 **24～40**，此处 100 明显过大 |
| Viewport Rect vs 装饰线 | 有内缩；装饰线非 Viewport 子节点名（面板装饰在外层） | **可能叠加**：若装饰线比 Viewport 更「收」，字仍在 Viewport 内但越线——须 Play 用 Gizmo 对一下；**不解释 Softness 本身** |
| ButtonArchive 逃逸 | 无独立 Canvas；各 Graphic **`m_Maskable: 1`**；材质默认 | **否** |
| 脚本关遮罩 | `LoadGameFormLogic` 仅 `SetParent(contentTransform)` + scale=1；**无** Mask/RectMask/Softness 操作 | **否** |
| SaveGame 对照 | Viewport **同样** Mask + RectMask2D + **Softness y=100**；Rect 同≈`-51.7` / `-185.9` | **同源** → 存档页大概率同样漏；属公共 Prefab 模板问题 |

### 4.3 Softness / 分辨率（对照 Shop）

| 来源 | Softness | 用途 |
|------|----------|------|
| Shop Buy/Sell Viewport（0713 定稿） | 目标约 **Y=32**（可调 24～48），X=0 | 边界**轻**虚化，仍要裁住 |
| Load / Save Viewport（现网） | **Y=100** | 软边过长 → 框外仍可读 |

推演：Softness 归零 → 硬裁，装饰线外不应再读到字。若归零后仍漏 → 再查 Viewport 与装饰线几何 / 双 Mask。

### 4.4 方案比选表

| 方案 | 做法摘要 | 改动面 | 风险 | 推荐？ |
|------|----------|--------|------|--------|
| **A** | Softness 改为 `(0,0)` 或 `(0, 24～32)`，先恢复裁切观感 | 仅 Prefab 字段 | 几乎无；硬切可能略「刀口」 | **推荐（先做）** |
| B | 只保留 RectMask2D（去掉 Mask+可不依赖 Image），或只留 Mask | Prefab 组件 | 去 Mask 后确认 Image 是否可关；双去风险 | Softness 修好后的清理 |
| C | 校正 Viewport Size/Pos 对齐装饰可视框 | Prefab Rect | 易牵布局；仅当 A 后仍「越线」 | 次选 |
| D | 修 ButtonArchive maskable/Canvas | 行 Prefab | 现网无逃逸证据 | 不推荐 |

### 4.5 施工员最小改动清单（只建议）

| 优先级 | 文件 | 做什么 |
|--------|------|--------|
| 1 | `Assets/GameRes/Prefabs/UI/LoadGamePanel.prefab` → `Viewport`/`RectMask2D` | Softness **Y: 100 → 0**（或 24～32，按拍板） |
| 2 | `SaveGamePanel.prefab` 同节点（若拍板同步） | 同样改 Softness |
| 3 | （可选）去掉 Viewport 上冗余 `Mask`，只留 `RectMask2D`（对齐 Shop 思路） | 须 Play 验一次裁切 |
| 不改 | `LoadGameFormLogic` 读档/删档；存档槽算法；ButtonArchive 业务 | |

**禁止**：重写 ScrollRect；Update 里手动藏字；为修裁剪改读档存档逻辑。

### 4.6 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | Softness 硬切 `(0,0)` 还是轻软边 `(0, 24～32)`？ | 先 **(0,0)** 验收不漏；要虚化再 24～32 |
| Q2 | SaveGamePanel 是否一并修？ | **是**（同源） |
| Q3 | Mask 与 RectMask2D 最终只留哪个？ | 主修 Softness 后；建议最终 **只留 RectMask2D** |

---

## 施工员下一轮最小化清单（建议 · 待拍板后开）

1. Load（+建议 Save）Viewport：`RectMask2D.Softness` 按拍板改小/归零。  
2. 按 §③ 验收顶底不漏字、读写删仍可用。  
3. 若仍漏：再用 Gizmo 对 Viewport vs 装饰线（方案 C），勿先动业务脚本。  
