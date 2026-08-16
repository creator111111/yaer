# Shop · Shop_Bar.Name 改 TMP（描边 + 投影）— 架构溯源与施工执行说明

**文档版本**：v1（2026-07-13）  
**文档性质**：【架构侦探】产出 + 施工指引（**本阶段只写文档，不改代码 / 不改 Prefab**）  
**触发**：策划要把 `Shop_Bar` 预制体里的 **`Name`（道具名）** 从 Legacy `Text` 换成 **TextMeshPro（TMP）**，并加上 **描边 + 投影**，对齐参考图观感。

**视觉目标（策划给定）**：

| 项 | 值 | 说明 |
|----|-----|------|
| 主体字色（Face） | `#323346` | RGB ≈ (50, 51, 70) |
| 描边 | **1px**，色号 `#eef0ff` | RGB ≈ (238, 240, 255) |
| 投影 | 有（参考图可见软阴影） | **色号 / 偏移未给死** → 见 §⑩ OPEN |

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_Editor烘焙双列表_MainItemDatabase一键刷行_施工执行说明.md`（Name 文案由 Bake 写入）
- `Assets/Doc/执行文档/0706/Shop_Bar数字图片化_Price_Number_Total_架构溯源与施工执行说明.md`（明确 **Name 不动**；本任务专做 Name）
- 关联预制体：`Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab`
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`（Bake 后的行实例）
- 工程内 TMP 字体先例：`Assets/ArtRes/Front/Alibaba-PuHuiTi-*.asset`（含 Outline / Underlay 的 MB、Underlay 变体）

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**只改 `Shop_Bar` 的 `Name` 节点：去掉 Legacy Text → 换成 `TextMeshProUGUI`；描边和投影不要靠代码，要靠「字体材质预设（Material Preset）」；字色写在 TMP 组件上 `#323346`，描边 `#eef0ff` 约 1 屏像素；改完预制体后重跑一次 Bake，让买/卖列表行全部换新 Name。程序侧几乎不用动——Bake 和行视图已经会写 TMP。**

**生活类比**：现在价签名是「普通打印黑体」；目标是「带白边、带一点阴影的招牌字」。字还是那几个字（Bake 照旧刷 `displayName`），换的是「油墨和描边模具」（TMP + 材质预设）。

---

## ② 玩家在做什么 / 会遇到什么现象

| # | 策划期望 | 当前工程 | 生活类比 |
|---|----------|----------|----------|
| 1 | 道具名是 TMP，可描边/投影 | `Name` = Legacy `Text`（阿里巴巴普惠体 Medium，字号 25，居中，占位「生命球」） | 价签用不带描边的打印机 |
| 2 | 字色 `#323346` | Legacy 默认深灰约 `#323232` | 墨色略偏黑 |
| 3 | 白描边约 1px `#eef0ff` | Legacy Text **没有**真正的矢量描边（最多靠 Outline 组件糊一层） | 没有描边模具 |
| 4 | 有投影 | 无 | 字贴在板上，没有立体感 |
| 5 | Bake / Play 后名称仍正确 | Bake 已写 `Name` 文案；换组件后只要还叫 `Name` 且挂 TMP 即可 | 换招牌模具，货名还从档案柜贴 |

---

## ③ 架构溯源

### 3.1 Prefab 现状（`Shop_Bar`）

```
Shop_Bar
├── Shadow
├── Icon
├── Name          ← 本任务唯一目标
├── Price         ← 已图片化，不动
└── Number        ← 已图片化 + 隐形 TMP 输入，不动
```

| 节点 | 组件（当前） | 关键字段 |
|------|--------------|----------|
| `Name` | `RectTransform` + `CanvasRenderer` + **Legacy `Text`** | Font=`Alibaba-PuHuiTi-Medium.ttf`，Size=25，Alignment=MiddleCenter，Text=`生命球`，Color≈深灰 |
| 脚本 GUID | `5f7201a12d95ffc409449d95f23cf332` = `UnityEngine.UI.Text` | — |

### 3.2 谁在写 Name 文案

```
MainItemDatabase.entry.displayName
        ↓
ShopListBakeEditor.BakeContent
        → SetText(row, "Name", entry.displayName)
              └─ 已同时写 Legacy Text 与 TextMeshProUGUI（兼容双组件）
        ↓
（可选）ShopBarRowView.Bind / ApplyName
        → SetLabelText(nameText, nameTextTmp, displayName)
              └─ 优先 TMP，再写 Legacy
```

**侦探结论**：

- **`ShopListBakeEditor.SetText` / `ShopBarRowView` 已兼容 TMP**，本任务 **不以改 C# 为主**。
- 真正缺口在 **Prefab 资源**：`Name` 还没挂 `TextMeshProUGUI`，也没有「描边+投影」材质。
- 场景里 Buy/Sell 列表行是 Bake 出来的 `Shop_Bar_*` 实例；**改源 Prefab 后必须再 Bake 一次**（或确认实例已 Apply），否则旧行可能仍是 Legacy。

### 3.3 TMP「描边 / 投影」在工程里怎么做才对

Unity TMP **不能**指望只改 Vertex Color 就出描边；描边 / 投影靠 **SDF 材质关键字**：

| 效果 | TMP 面板名 | 材质关键字 | 工程先例 |
|------|------------|------------|----------|
| 描边 | Outline | `OUTLINE_ON` | `LiberationSans SDF - Outline.mat`；`Alibaba-PuHuiTi-*-MB` 上 `_OutlineWidth` 已开过 |
| 投影 | Underlay（底层衬底） | `UNDERLAY_ON` | `Alibaba-PuHuiTi-Regular SDF Underlay 1.asset`；MB 系列有 Offset |

**推荐路径（定稿）**：从中文字体 SDF **新建一份 Material Preset**，只给商店 Name 用；**禁止直接改** `Alibaba-PuHuiTi-Medium Atlas Material` 默认材质（会污染全项目共用该字体的 UI）。

**字体资产选型（定稿倾向）**：

| 候选 | 路径 | 理由 |
|------|------|------|
| **首选** | `Assets/ArtRes/Front/Alibaba-PuHuiTi-Medium SDF.asset` | 与当前 Legacy Name 同源（Medium），中文齐全 |
| 备选 | `Alibaba-PuHuiTi-Regular SDF.asset` | 字重更细；仅当美术觉得 Medium 太粗时换 |

### 3.4 「1 像素描边」在 TMP 里不是填 `1`

TMP 的 **Outline Width** 是 **0～1 相对值**（相对字号 / SDF Gradient），**不是**屏幕像素整数。

施工时用 **目测对齐 1px** 的流程：

1. Font Size 先对齐旧版 **25**（或美术指定字号）。
2. Outline Width 从 **0.05～0.12** 试起（Medium SDF 的 GradientScale 与 MB 不同，以 Game 视图为准）。
3. Canvas Scaler 若为 Scale With Screen Size，在 **目标分辨率** 下看描边是否约 1 屏像素。
4. 勾选 TMP 的 **Extra Padding**，必要时略加大 `Name` 的 Rect，避免描边被裁切。

**替代方案（不推荐作主路径）**：

| 方案 | 做法 | 为何不优先 |
|------|------|------------|
| A. Material Preset（推荐） | 一份 mat，Outline + Underlay 配好，拖给 Name | — |
| B. 运行时 `fontMaterial` 实例改 Outline | 代码里 `SetOutline` / 改实例材质 | 商店 Name 是静态 Bake UI，无必要；易泄漏材质实例 |
| C. 叠一个半透明 TMP 子节点当影 | 两个 TMP 对齐全 | 双份网格、对不齐、维护差 |
| D. 继续用 Legacy + Outline/Shadow 组件 | 不换 TMP | **违背本任务**；中文描边质量差 |

---

## ④ 范围冻结

| 项 | 约定 |
|----|------|
| **改什么** | 仅 `Shop_Bar.prefab` → `Name`；可选新建 1 个 Material Preset `.mat` |
| **不改什么** | `Price` / `Number` / Icon / Bake 过滤规则 / `ShopFormLogic` 交易逻辑 |
| **文案来源** | 仍是 `MainItemDatabase.displayName`，Bake 写入 |
| **字色** | Face `#323346`（写在 TMP 组件 **Vertex / Face Color**） |
| **描边** | Outline Color `#eef0ff`，视觉约 **1px** |
| **投影** | Underlay 开启；具体色/偏移见 §⑩，施工时先用推荐初值再目测 |
| **代码** | 默认 **零改动**；仅当 Bake 后实例未吃到 TMP 时，再在 Bake 里加「Ensure Name 为 TMP」兜底（§⑦ 可选） |
| **本阶段不做** | 全局 UI 字体统一、动态改描边、多语言字号自适应 |

---

## ⑤ 目标外观 / Inspector 定稿值

### 5.1 颜色换算（给程序 / 美术填 Inspector）

| 用途 | Hex | RGBA 0～1（约） |
|------|-----|-----------------|
| Face（主体） | `#323346` | (0.196, 0.200, 0.275, 1) |
| Outline | `#eef0ff` | (0.933, 0.941, 1.000, 1) |
| Underlay（推荐初值） | `#000000` Alpha **0.35～0.55** | 偏软阴影；不对齐参考图再调 |

### 5.2 Material Preset 建议参数（初值，验收可微调）

建议资源路径（新建）：

`Assets/ArtRes/Front/Materials/ShopBar_Name_PuHuiTiMedium_OutlineUnderlay.mat`  
（目录不存在则建 `Materials`；命名可微调，但须 **独立文件**，勿覆盖字体默认 mat）

| 参数 | 推荐初值 | 说明 |
|------|----------|------|
| Shader | `TextMeshPro/Distance Field`（或工程 Medium 同款 Mobile/SDF） | 与字体资产一致即可 |
| Face Dilate | 0 | 勿为了「更粗」乱加，先保字号 |
| Outline | On | Color=`#eef0ff`，Width≈**0.08**（再目测到 1px） |
| Outline Softness | 0 | 硬边更接近「一像素描边」 |
| Underlay | On | Color 黑 + Alpha 0.45；OffsetX≈**0.5～1**；OffsetY≈**-0.5～-1**；Dilate=0；Softness≈**0.1～0.25** |
| Keywords | `OUTLINE_ON` + `UNDERLAY_ON` | 缺一则对应效果不出 |

### 5.3 `Name` 节点 TMP 组件建议

| 字段 | 值 |
|------|-----|
| Font Asset | `Alibaba-PuHuiTi-Medium SDF` |
| Material Preset | 上一节新建的 mat |
| Font Size | **25**（对齐旧 Legacy） |
| Alignment | Middle Center |
| Color / Face | `#323346` |
| Raycast Target | **关**（名称不需点；避免挡 Number 点击） |
| Extra Padding | **On** |
| Overflow | Overflow 或 Truncate（与旧版 Wrap 策略一致；四字名居中即可） |
| 占位 Text | 可留「生命球」或任意中文，Bake 会覆盖 |

---

## ⑥ 策划 / 美术施工步骤（主路径 · 零代码）

```
① Project 选中 Alibaba-PuHuiTi-Medium SDF
        ↓
② 右键 Create → TextMeshPro → Material Preset
   （或从字体 Inspector 的 Material Preset 创建副本）
   存到 ArtRes/Front/Materials/…ShopBar_Name_….mat
        ↓
③ 打开该 mat：勾 Outline + Underlay，按 §5.2 填色与初值
        ↓
④ 打开 Shop_Bar.prefab（Prefab Mode）
   选中 Name → Remove Component「Text」
   → Add Component「TextMeshPro - Text (UI)」
        ↓
⑤ 赋 Font Asset + Material Preset；Face `#323346`；字号 25；居中；关 Raycast
        ↓
⑥ Prefab 保存 → 打开 Village_Shop
   菜单：Tools → Shop → Bake Shop Lists From MainItemDatabase
        ↓
⑦ 不 Play：展开 Buy/Sell Content，目检每行 Name
   字色 / 白描边 / 投影 / 文案是否与 Database 一致
        ↓
⑧ Ctrl+S 存场景 → Play 切 Tab，确认名称仍在、无 Missing Script
```

**为何必须再 Bake**：列表行是场景里 Instantiate 的实例；只改源 Prefab 而不 Bake，旧实例可能仍挂 Legacy，或 Override 残留。

---

## ⑦ 程序侧（仅兜底 · 默认不做）

| 优先级 | 改动 | 何时需要 |
|--------|------|----------|
| P0 | **无** | Prefab + Bake 正常 |
| P1 | Bake 时若 `Name` 仍无 `TextMeshProUGUI`，打 **Error** 并跳过写文案 | 防止静默写到已删除的 Legacy |
| P2 | Editor 工具一键「Ensure Shop_Bar Name = TMP + 指定 mat」 | 多预制体复用时再做；本任务一个 Prefab 手改即可 |

`ShopBarRowView` 已有 `nameTextTmp` 查找逻辑，**不必**为本次功能改运行时。

---

## ⑧ 任务拆分（给施工员）

| ID | 内容 | 负责人 | 验收 |
|----|------|--------|------|
| **NM-0** | 新建 Material Preset（Outline+Underlay） | 美术/施工 | Project 可见独立 `.mat`，关键字双开 |
| **NM-1** | `Shop_Bar.Name`：Legacy → TMP，绑字体+材质，字色 `#323346` | 施工 | Prefab Mode 可见 TMP，无 Legacy Text |
| **NM-2** | 目测调 Outline≈1px、投影接近参考图 | 美术 | Game 视图对齐参考图 |
| **NM-3** | 重跑 Bake，检查 Buy+Sell 全部行 Name | 施工 | 未 Play 即见新样式文案 |
| **NM-4**（可选） | Bake 缺 TMP 时 Error 日志 | 程序 | 故意弄坏 Prefab 应报错 |

---

## ⑨ 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| V1 | Prefab `Name` Inspector | 仅有 `TextMeshProUGUI`，无 `Text` |
| V2 | Face Color | `#323346` |
| V3 | Material | 独立 Preset；Outline `#eef0ff`；有投影 |
| V4 | 描边观感 | 目标分辨率下约 **1px**，不过粗糊边 |
| V5 | Bake 后 Buy 行（如「生命球」） | TMP 样式 + 正确 displayName |
| V6 | Bake 后 Sell 行 | 同上 |
| V7 | Play → 买/卖 Tab | 名称不丢、不粉字、不挡 Number 点击 |
| V8 | 其它界面 TMP | 未被改坏（证明没动字体默认 Atlas Material） |

---

## ⑩ OPEN_QUESTIONS（设计未写死 · 勿擅自定死全局规范）

| ID | 问题 | 建议默认 | 谁拍板 |
|----|------|----------|--------|
| OQ-NM-1 | 投影色号、透明度、偏移未给 | 黑 Alpha 0.45；Offset (0.8, -0.8)；Softness 0.15 | 美术对照参考图 |
| OQ-NM-2 | 字重 Medium vs Regular | **Medium**（对齐旧 Legacy） | 策划/美术 |
| OQ-NM-3 | 字号是否保持 25 | **先 25**；描边后若显挤再微调 Size/Rect | 美术 |
| OQ-NM-4 | 是否把同一材质复用到其它商店文案 | 本任务 **仅 Name**；其它另开任务 | 策划 |

（若需登记到全局 `Docs/OPEN_QUESTIONS.md`，施工前由制作人决定是否同步；本文件已自包含。）

---

## ⑪ 风险与注意

1. **改默认字体材质 = 全项目中毒**：必须用 **Material Preset 副本**。  
2. **描边被裁切**：开 Extra Padding；Rect 高度约 34 若裁切则略加宽/高。  
3. **场景实例不同步**：改 Prefab 后忘记 Bake → 列表仍旧样式。  
4. **Raycast**：Name 若开 Raycast，可能挡住同行 Number 点击。  
5. **中文 SDF 体积大**：只引用已有 Medium SDF，**不要**为此再烘一份巨型字体。

---

## ⑫ 给程序看的文件清单（速查）

| 文件 | 角色 |
|------|------|
| `Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab` | **主改**：`Name` Legacy → TMP |
| `Assets/ArtRes/Front/Alibaba-PuHuiTi-Medium SDF.asset` | Font Asset（只读引用） |
| `Assets/ArtRes/Front/Materials/ShopBar_Name_… .mat`（新建） | Outline + Underlay Preset |
| `Assets/GameRes/Scenes/Village_Shop.unity` | Bake 后行实例样式落地 |
| `ShopListBakeEditor.cs` | 已兼容 TMP；默认不动 |
| `ShopBarRowView.cs` | 已兼容 TMP；默认不动 |

---

## ⑬ 提交说明模板（施工完成后用）

```
改了哪些：Shop_Bar.Name → TMP；新增 ShopBar_Name Outline+Underlay 材质；重 Bake Village_Shop。
实现了什么：道具名 #323346 + 约 1px #eef0ff 描边 + Underlay 投影。
如何验证：未 Play 看 Buy/Sell Name；Play 切 Tab；确认其它 UI 字体材质未被污染。
```

---

**侦探签收**：逻辑链已清（Bake / RowView 已通 TMP）；本任务本质是 **Prefab + Material Preset 美术施工**，不是业务逻辑重构。施工员按 §⑥～§⑧ 最小化落地即可。
