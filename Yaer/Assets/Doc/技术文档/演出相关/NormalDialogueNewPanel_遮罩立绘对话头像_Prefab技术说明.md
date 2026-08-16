# NormalDialogueNewPanel · 遮罩 + 立绘对话头像 — Prefab 技术说明

> 文档日期：2026-07-27（Prefab 摆位）  
> **现网补充（2026-08-04）**：C# 已接线（`DialogueMaskAvatarPresenter` + `useMaskAvatar`）。运行时与枚举/大立绘总览见：  
> **`演出相关/对话立绘与表情系统_技术说明.md`**（以该文为准；下文「本阶段不改 C#」仅描述 0727 当时范围）。  
> 状态：**Prefab 摆位已落地** — 雅儿两套 + Amy/Aliy/Gusha 的 Pos/Scale/SizeDelta **各自定稿**（互不覆盖）。  
> 范围（本文）：`NormalDialogueNewPanel` 字幕条左侧头像区的 **UI Prefab 结构**。  
> 关联：
>
> - 系统总览：`Assets/Doc/技术文档/演出相关/对话立绘与表情系统_技术说明.md`
> - 执行说明：`Assets/Doc/执行文档/0727/NormalDialogueNewPanel_遮罩立绘搭对话头像_Prefab修改执行说明.md`
> - 架构溯源：`Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md`
> - 立绘对照：`Assets/Doc/执行文档/6月/0614/雅儿对话立绘素材替换_执行说明.md`
> - OPEN：`Assets/Doc/OPEN_QUESTIONS.md`（对话头像 Q1～Q8）

**Unity**：2020.3.48f1  

---

## 一、背景与定位

### 1.1 要解决什么问题

现网字幕条左侧「小头像」走 `Avatar_Yaer_*` 图集 → `Bottom/Portrait`（`Image.sprite`）。目标方向是改为：**用 Mask 裁切嵌在 UI 内的雅儿立绘 Prefab**，做出对话头像观感，并为后续 FaceType / 换装接线留挂点。

本阶段只完成 **Prefab 结构与摆位底座**，方便美术/策划在编辑器里对脸、对窗。

### 1.2 设计原则（一句话）

**UI 壳只保留「窗（Mask）+ 内容根（YaerAvatarRoot）+ 两套立绘实例」；变脸与换装逻辑下一轮再接，不进本 Prefab 脚本。**

生活类比：先在字幕条左侧挖好圆/方窗，窗后放全身立绘人偶；人偶先摆好，以后再接「说哪句换哪张脸」。

### 1.3 非目标（本阶段明确不做）

| 不做 | 说明 |
|------|------|
| 改 `DialogueAvatarLoader` / `DialogueTMPUGUI` | 运行时仍可能给旧 `Portrait` 赋 Sprite |
| 历史记录 `ImageAvatar` 同步 | OPEN Q2，另案 |
| 删除 `Portrait` 节点 | 仅 **Active=false**，保留回退与 `actorPortrait` 引用 |
| 立绘自动跟台本变脸 | UI 壳上通常无 `DialogueActorEx`；接线属下一轮 |
| **各立绘共用同一 Pos/Scale** | 构图不同，**各自定稿**；禁止批量对齐覆盖 |

---

## 二、资产与挂点

| 用途 | 路径 / 节点 |
|------|-------------|
| 对话 UI 壳（本期唯一改动目标） | `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` |
| 字幕条 | `Root → Bottom` |
| 裁剪窗 | `Bottom/Mask` |
| 头像内容根 | `Bottom/Mask/YaerAvatarRoot` |
| 铠甲三态立绘 | `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` |
| 连衣裙立绘 | `Assets/Prefabs/DialougeProtrait/YaerPainting.prefab` |
| 旧小头像（已隐藏） | `Bottom/Portrait` ← `DialogueTMPUGUI.actorPortrait` |

### 2.1 目标 Hierarchy

```
Bottom
├── Mask                         ← Image + Mask；裁剪窗口 ≈ 旧 Portrait 282×282
│   └── YaerAvatarRoot           ← 必须在 Mask 子级；Stretch 铺满 Mask
│       ├── GoOutStoryYaerPainting   ← Prefab Instance（铠甲三态；默认可显示）
│       ├── YaerPainting             ← Prefab Instance（Dress；默认 Active=false）
│       ├── AmyPainting              ← Prefab Instance（默认 Active=false）
│       ├── AliyPainting             ← Prefab Instance（默认 Active=false）
│       └── GushaPainting            ← Prefab Instance（默认 Active=false）
├── Portrait                     ← 旧 Image；Active=false；引用保留
├── ImgForward / NameBG / …
└── …
```

**硬规则**：`YaerAvatarRoot` 必须是 `Mask` 的子物体；立绘不得挂在 `Bottom` 下绕过 Mask。

---

## 三、已落地结构说明

### 3.1 `Mask`（裁剪窗）

| 组件 | 约定 |
|------|------|
| **RectTransform** | 对齐旧 `Portrait`：Anchor 左中 `(0, 0.5)`；Pivot `(0, 0.5)`；Pos `(15, 0)`；Size **282×282** |
| **Image** | Mask 依赖 Graphic；当前用内置 **UISprite**（方形白底）；Raycast Target **关** |
| **Mask** | `Show Mask Graphic` **关**（只裁不画，避免窗内白底） |

> **圆形 vs 方形**：OPEN / Prefab 开放题 P1。现网旧 `Portrait` 为方形显示区，故本期默认方形。若要圆形头像，将 Mask 的 Sprite 换成白色圆形图，**不要**用 `RectMask2D` 做圆。

### 3.2 `YaerAvatarRoot`（内容根）

| 项 | 约定 |
|----|------|
| RectTransform | Stretch 铺满 Mask（`AnchorMin/Max=(0,0)/(1,1)`，`SizeDelta=(0,0)`） |
| 组件 | 本阶段仅 RectTransform，不挂业务脚本 |
| 职责 | 以后换装只显隐子物体；缩放/位移打在 Root 或各立绘实例上 |

### 3.3 嵌套立绘实例

| 实例 | 母 Prefab | 默认显隐 | 说明 |
|------|-----------|----------|------|
| `GoOutStoryYaerPainting` | `…/GoOutStoryYaerPainting.prefab` | **显示** | ArmorNone / ArmorCrown / Armor 三态共用；头饰靠子物体 `ArmorHead` / `ArmorCrown` |
| `YaerPainting` | `…/YaerPainting.prefab` | **隐藏** | Dress；`Faces` 下为 `Dress_Crown_*` |
| `AmyPainting` | `…/AmyPainting.prefab` | **隐藏** | 摆位见 §4.1 |
| `AliyPainting` | `…/AliyPainting.prefab` | **隐藏** | 摆位见 §4.1 |
| `GushaPainting` | `…/GushaPainting.prefab` | **隐藏** | 摆位见 §4.1 |

- **保持 Prefab Instance，尽量不 Unpack**（母体改图可同步；仅局部改死子物体时再 Unpack）。
- CanvasGroup：`Interactable` / `BlocksRaycasts` 建议关，避免头像区挡点字幕。

### 3.4 旧 `Portrait`

| 项 | 约定 |
|----|------|
| Active | **false**（隐藏） |
| `actorPortrait` | **不清空**（本阶段不改代码，清空易空引用） |
| 用途 | 对照旧图集头像、紧急回退 |

---

## 四、摆位定稿（各角色各自属性，互不覆盖）

> **原则**：`NormalDialogueNewPanel` 内每个立绘实例的 **Pos / Scale / SizeDelta 各自定稿**；禁止用某一角色数值批量覆盖其它角色。

### 4.1 已定稿数值表（磁盘 Prefab，2026-07-27）

| 节点 | Pos (Anchored) | Scale | SizeDelta (W×H) | 状态 |
|------|----------------|-------|-----------------|------|
| `GoOutStoryYaerPainting` | `(-13.8, -90)` | `0.65` | `853.9938 × 949.0917` | ✅ |
| `YaerPainting` | `(46.2, -250.7)` | `0.65` | `1046.2583 × 1951.0939` | ✅ |
| `AmyPainting` | `(136.1, -264.2)` | `0.8` | `831 × 1029` | ✅ |
| `AliyPainting` | `(-83.6, -269.7)` | `0.8` | `831 × 1029` | ✅ |
| `GushaPainting` | `(43, -391)` | `0.7` | `723 × 1554` | ✅ |

> 立绘原图很大（脸图约 **1078×1497**），必须缩小后才能进约 282 的头像窗；各角色构图不同，故 Scale/Pos **本来就不会相同**。

### 4.2 调位原则（少踩坑）

1. **先锁 Mask 窗，再挪人偶**：窗尺寸对齐 Portrait 后，只动该立绘实例的 Scale/Pos。  
2. **只露脸**：半身立绘进小窗；宁可 Scale 略大再下移，裁掉胸口。  
3. **不要改母立绘大图尺寸**迁就头像：只在 `NormalDialogueNewPanel` 内实例上调，避免影响全屏大立绘对话。  
4. **Sibling**：若被 `ImgForward` / `NameBG` 挡住，调同级顺序；头像一般在文字后、前景框前。  
5. **变脸**：本阶段手动开 `Faces` 子物体试构图即可；自动跟台本属下一轮。  
6. **改某一角色时只改它自己**，不要「对齐」到其它角色的 Pos/Scale。

---

## 五、与现网小头像链路的关系

现网仍走（本阶段未切断）：

```
FaceType → DialogueActorEx.RefreshAvatar
        → DialogueAvatarLoader.GetAvatar（Avatar_Yaer_* 图集）
        → DialogueTMPUGUI.OnGetAvatar
        → actorPortrait（Bottom/Portrait）
```

| 现状 | 影响 |
|------|------|
| `Portrait` Active=false | 玩家看不到旧图集头像 |
| `actorPortrait` 引用仍在 | Loader 仍可能赋值，一般不炸 |
| Mask 内立绘 | **纯展示/摆位**；未接 FaceType / 换装 |

> 生活类比：旧微信头像框还在后台收图，只是关了显示；新框是舞台人偶探进圆窗，接线还没通。

---

## 六、验收清单（Prefab 阶段）

- [x] Hierarchy：`Bottom/Mask/YaerAvatarRoot/...`（Root **在** Mask 下）  
- [x] `Mask` 有 Image + Mask；窗口对齐旧 Portrait（≈282）；**Show Mask Graphic=关**  
- [x] `YaerAvatarRoot` 下有 GoOut / YaerPainting / Amy / Aliy / Gusha 实例（保持 Prefab Instance）  
- [x] **各角色 Pos / Scale / SizeDelta 分套定稿**（见 §4.1；互不覆盖）  
- [x] 旧 `Portrait` 已隐藏；`actorPortrait` 引用仍在  
- [x] Amy / Aliy / Gusha 眼检定稿  

---

## 七、下一轮施工建议（本文不执行）

建议顺序（与溯源报告挂点优先级一致：**逻辑进 Loader / 专用 Avatar 控制器，UI 只留 Mask 结构**）：

1. 按存档/台本服装显隐 `GoOutStoryYaerPainting` / `YaerPainting`，并切换 GoOut 头饰（`ArmorHead` / `ArmorCrown`）  
2. 把 `FaceType` 切脸接到 `YaerAvatarRoot` 内实例（或薄封装组件）  
3. 决定是否停用 Loader 对雅儿的图集赋值、是否藏死 `Portrait`  
4. 历史 `ImageAvatar` 是否同步（OPEN Q2）  
5. 立绘母体尺寸统一后，再考虑是否沉淀「分套 Pos 表」或统一标准  

---

## 八、相关路径速查

| 用途 | 路径 |
|------|------|
| 本期改动 | `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` |
| 铠甲立绘母体 | `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` |
| 裙子立绘母体 | `Assets/Prefabs/DialougeProtrait/YaerPainting.prefab` |
| 旧小头像 | `Bottom/Portrait`（`actorPortrait`） |
| 沙盒预览 | `Assets/GameRes/Scenes/DialogDebug.unity`（技术说明见同目录 DialogDebug 文档） |

---

## 九、开放问题（Prefab 阶段拍板）

| ID | 问题 | 本期默认 | 状态 |
|----|------|----------|------|
| P1 | 头像窗圆形还是方形？ | **方形**（UISprite + Mask；跟旧 Portrait 显示区） | 可改圆形 Sprite |
| P2 | Dress 与铠甲是否长期共存于 Root 下互斥？ | **是**（省运行时 Instantiate） | 已按此摆 |
| P3 | 是否 Unpack 立绘实例？ | **否**（保持 Prefab Instance） | 已按此摆 |
| P4 | 历史列表是否同期改 Mask？ | **否** | 另案 |
| P5 | 各立绘 Pos/Scale 能否统一？ | **否**；各自定稿见表 §4.1（GoOut/Yaer/Amy/Aliy/Gusha 均已确认） | ✅ 已确认 |
