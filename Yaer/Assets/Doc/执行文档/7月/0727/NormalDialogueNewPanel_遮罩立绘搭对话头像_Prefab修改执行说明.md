# NormalDialogueNewPanel · 遮罩 + 立绘 Prefab 搭新对话头像 — Prefab 修改执行说明

**文档版本**：v1.0（2026-07-27）  
**文档性质**：【施工指引】只改 **UI Prefab 摆位**（本阶段**不改 C#**；运行时仍可能继续给旧 `Portrait` 赋 Sprite）  
**目标**：在字幕条左侧用 **Mask + 嵌套雅儿立绘 Prefab** 做出「裁切后的对话头像」观感，方便你亲自调位置/缩放  
**前置**：你已在 `Bottom` 下加了空节点 `Mask`、`YaerAvatarRoot`（见 Hierarchy 截图）  
**关联**：
- 溯源：`Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md`
- 立绘对照：`Assets/Doc/执行文档/6月/0614/雅儿对话立绘素材替换_执行说明.md`
- OPEN：`Assets/Doc/OPEN_QUESTIONS.md`（对话头像 Q1～Q8）

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**打开 `NormalDialogueNewPanel`，把层级改成 `Bottom → Mask → YaerAvatarRoot →（嵌立绘）`，Mask 裁窗、Root 里塞铠甲/裙子两套立绘；旧 `Portrait` 先关掉当对照，本阶段只摆 UI，不接线。**

生活类比：字幕条左侧挖一个圆/方窗（Mask），窗后面放全身立绘人偶，挪人偶让脸对准窗口。

---

## ② 先认清你现在的状态

### 2.1 要改的预制体（唯一）

| 项 | 路径 |
|----|------|
| 对话 UI 壳 | `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` |
| 字幕条 | Hierarchy：`NormalDialogueNewPanel → Root → Bottom` |

### 2.2 你已加好的节点（以磁盘为准）

| 节点 | 当前情况 | 问题 |
|------|----------|------|
| `Bottom/Mask` | 仅有 RectTransform，约 100×100，居中；**无 Image / 无 Mask 组件**；**无子物体** | 还裁不了图 |
| `Bottom/YaerAvatarRoot` | 仅有 RectTransform，约 100×100；**与 Mask 平级**；空 | 不在 Mask 下则遮罩无效 |
| `Bottom/Portrait` | 仍在；`DialogueTMPUGUI.actorPortrait` 仍指向它 | 运行时旧小头像还会刷这里 |

### 2.3 立绘母预制体（嵌进 Root 用）

| 形态 | 嵌哪个 | 路径 |
|------|--------|------|
| ArmorNone / ArmorCrown / Armor（铠甲三态） | **`GoOutStoryYaerPainting`** | `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` |
| Dress（连衣裙） | **`YaerPainting`** | `Assets/Prefabs/DialougeProtrait/YaerPainting.prefab` |

> 立绘本体很大（脸图约 **1078×1497**），塞进约 282 的头像窗必须 **缩小 + 平移**，靠 Mask 只露脸。

### 2.4 本阶段非目标（避免一次做完）

- 不改 `DialogueAvatarLoader` / `DialogueTMPUGUI` 代码  
- 不要求历史记录 `ImageAvatar` 同步（可后做）  
- 不在本 Prefab 里删 `Portrait`（先禁用，方便回退）  
- 不指望嵌进去的立绘 **自动跟对话变脸**（立绘脚本要找 `DialogueActorEx`；UI 壳上通常没有。变脸接线另开施工轮）

---

## ③ 目标层级（改完应长这样）

```
Bottom
├── Mask                    ← 裁剪窗口（尺寸 ≈ 旧 Portrait 282×282）
│   └── YaerAvatarRoot      ← 头像内容根（必须在 Mask 子级）
│       ├── GoOutStoryYaerPainting   ← Prefab 实例（铠甲三态；默认可显示）
│       └── YaerPainting             ← Prefab 实例（Dress；默认可先 SetActive=false）
├── Portrait                ← 旧 Image；本阶段勾掉 Active，作对照/回退
├── ImgForward
├── NameBG
└── …（其它按钮不变）
```

**关键规则**：`YaerAvatarRoot` **必须是 `Mask` 的子物体**。你现在两者都挂在 `Bottom` 下平级 —— **第一步先拖层级**。

---

## ④ 操作步骤（按顺序在 Unity 里做）

### 步骤 0 — 打开 Prefab

1. Project 定位：`Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab`  
2. 双击进入 Prefab 编辑模式  
3. Hierarchy 展开：`Root → Bottom`  
4. 建议：Scene 视图开 **2D**，选中 `Bottom`，方便对齐字幕条左侧

### 步骤 1 — 纠正父子关系

1. 把 **`YaerAvatarRoot` 拖到 `Mask` 下面**（成为子物体）  
2. 确认顺序大致为：`Mask` 在 `Portrait` **前面或同侧左侧**（同级 Sibling Index 可后调；先保证 Mask 盖住头像区域）  
3. **不要**把立绘直接挂在 `Bottom` 下绕过 Mask

### 步骤 2 — 配置 `Mask`（裁剪窗）

选中 `Mask`，按下面补组件（缺什么加什么）：

| 组件 | 设置 |
|------|------|
| **RectTransform** | 对齐旧 `Portrait`：Anchor **左中** `(0, 0.5)`；Pivot `(0, 0.5)`；Pos ≈ `(15, 0)`；**Width/Height = 282**（可先抄 Portrait，再微调） |
| **Image** | 必须有（Mask 依赖 Graphic）。圆形头像：拖一张**白色圆形** Sprite；方形：用 Unity 内置 `UISprite` / 白方块即可。颜色可白、Alpha=1 |
| **Mask** | `Add Component → UI → Mask`。勾选 **Show Mask Graphic** 可先开着调位置；定稿后可关掉以免多一块白底 |
| **Raycast Target** | 建议 **关**（头像区不挡点字幕） |

> **Rect Mask 2D** 也可（矩形裁剪、无圆角）。要圆形头像请用 **Mask + 圆形 Sprite**，不要用 RectMask2D。

### 步骤 3 — 配置 `YaerAvatarRoot`

选中 `YaerAvatarRoot`：

| 项 | 建议 |
|----|------|
| RectTransform | Stretch 铺满 Mask，或居中 282×282；Pos `(0,0)` |
| 组件 | 本阶段 **只需 RectTransform**（不必挂脚本） |
| 作用 | 以后换装只显隐子物体；缩放/位移可打在 Root 或各立绘实例上 |

### 步骤 4 — 嵌铠甲立绘（三态共用）

1. Project 找到 `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab`  
2. **拖进** `YaerAvatarRoot`（Unpack 与否：建议 **保持 Prefab Instance**，方便母体改图后同步；若拖动时乱套再 Unpack Completely）  
3. 选中实例，先粗调：

| 项 | 起步建议（再靠眼调） |
|----|----------------------|
| Scale | 约 **0.25～0.35**（立绘原图很大，必须缩小） |
| Anchored Position | 先 `(0, 0)`，再 **上下左右挪**，让**脸**落在 Mask 窗正中 |
| CanvasGroup（若有） | Alpha=1；Interactable/BlocksRaycasts 可关 |

4. 在 Prefab 内展开 `Faces`，临时只开一张脸试构图（如 `Armor_NoHeadWear_Smile`），其它关掉。  
5. 试三态头饰（仍在同一 Prefab）：
   - ArmorNone：`armorHead` / `armorCrown` 都关  
   - ArmorCrown：只开 `armorCrown`  
   - Armor：只开 `armorHead`  
   （对应脚本字段；Hierarchy 里常见名 `ArmorHead` / `ArmorCrown`）

### 步骤 5 — 嵌 Dress 立绘（另一套）

1. 拖 `Assets/Prefabs/DialougeProtrait/YaerPainting.prefab` 到 **同一** `YaerAvatarRoot`  
2. Scale / Position **尽量与 GoOut 实例对齐**（两套脸构图可能略有偏差，分开记一版数值）  
3. 本阶段建议：默认 **只显示一套**  
   - 调铠甲时：`GoOutStoryYaerPainting` Active=On，`YaerPainting` Active=Off  
   - 调裙子时：反过来  
4. `YaerPainting` 的 `Faces` 子物体名为 `Dress_Crown_*`；试构图时手动开 `Dress_Crown_Smile`

### 步骤 6 — 旧 `Portrait` 怎么处理

| 做法 | 说明 |
|------|------|
| **推荐** | 取消勾选 `Portrait` 的 Active（隐藏） |
| 保留引用 | **不要**清空 `DialogueTMPUGUI.actorPortrait`（本阶段不改代码；清空可能报空引用） |
| 对照 | 需要对比旧图集头像时，临时再打开 `Portrait` |

### 步骤 7 — 保存

1. Prefab 窗口点 **Save**（或 Ctrl+S）  
2. 退出 Prefab 编辑模式  

---

## ⑤ 调位置小技巧（少踩坑）

1. **先定 Mask 窗，再挪人偶**：窗尺寸锁死后，只动立绘 Scale/Pos。  
2. **只露脸**：立绘是半身，Mask 小；宁可 Scale 大一点再下移，把胸口裁掉。  
3. **ImgForward / NameBG**：若头像被前景挡住，检查 Sibling 顺序；头像一般在文字后面、前景框前面（按你美术要求调）。  
4. **不要改母立绘里的大图尺寸来迁就头像**：只在 `NormalDialogueNewPanel` 实例上 Scale，避免影响全屏大立绘对话。  
5. **DialogDebug 预览**：可 Open `DialogDebug` 场景 Play，看字幕条左侧；本阶段变脸不会自动跟台本，属预期。

---

## ⑥ 验收清单（Prefab 阶段）

- [ ] Hierarchy 为 `Bottom/Mask/YaerAvatarRoot/...`（Root **在** Mask 下）  
- [ ] `Mask` 有 Image + Mask，窗口约在原 Portrait 位置（≈282）  
- [ ] `YaerAvatarRoot` 下有 `GoOutStoryYaerPainting` 实例  
- [ ] （可选）同级有 `YaerPainting` 实例，且与铠甲互斥显示  
- [ ] Mask 内能看到裁切后的脸，窗外无大片漏出  
- [ ] 手动切换 `ArmorHead` / `ArmorCrown` 后，窗内头饰变化正确  
- [ ] 手动开 `Dress_Crown_Smile` 时裙子脸构图可接受  
- [ ] 旧 `Portrait` 已隐藏；`actorPortrait` 引用仍在  
- [ ] Prefab 已 Save  

---

## ⑦ 调完 UI 之后（下一轮施工，本文不执行）

Prefab 观感 OK 后再开【施工员】，建议顺序：

1. 按服装显隐 `GoOutStoryYaerPainting` / `YaerPainting`，并调 GoOut 头饰开关  
2. 把 FaceType 切脸接到 `YaerAvatarRoot` 内实例（或新建薄封装组件）  
3. 决定是否停用 `DialogueAvatarLoader` 对雅儿的图集赋值、是否藏死 `Portrait`  
4. 历史 `ImageAvatar` 是否同步（OPEN Q2）  

挂点优先级仍建议：**逻辑进 Loader / 专用 Avatar 控制器，UI 只保留 Mask 结构**（见溯源报告 §4.1）。

---

## ⑧ 相关路径速查

| 用途 | 路径 |
|------|------|
| 改这个 | `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` |
| 铠甲立绘 | `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` |
| 裙子立绘 | `Assets/Prefabs/DialougeProtrait/YaerPainting.prefab` |
| 旧小头像节点 | `Bottom/Portrait`（`DialogueTMPUGUI.actorPortrait`） |
| 溯源 | `Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md` |

---

## ⑨ 开放问题（摆 Prefab 时可顺手拍板）

| ID | 问题 | 建议默认 |
|----|------|----------|
| P1 | 头像窗圆形还是方形？ | 跟现网 `Portrait` 视觉；有圆框就用圆形 Mask Sprite |
| P2 | Dress 与铠甲是否长期共存于 Root 下互斥？ | **是**（省运行时 Instantiate） |
| P3 | 是否 Unpack 立绘实例？ | **尽量不 Unpack**；仅当需要改死局部子物体时再 Unpack |
| P4 | 历史列表是否同期改 Mask？ | 本期 **否**；先定字幕条 |

（可回写到 `OPEN_QUESTIONS.md` 对话头像一节。）
