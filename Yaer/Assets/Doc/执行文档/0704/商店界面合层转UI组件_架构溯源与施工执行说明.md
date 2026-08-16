# 商店界面合层 → UI 组件 — 架构溯源与施工执行说明

**文档性质**：架构侦探产出（只读溯源 + 施工指引；**本阶段不改代码**）  
**调查日期**：2026-07-04  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/02_SYSTEM_SPEC.md`
- `Assets/Doc/执行文档/0629/商店系统_策划拆解_执行说明.md`（阶段一～四逻辑与节点命名）
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`（商店 UI 测试场）
- 关联美术资源：`Assets/ArtRes/Scene/Village/商店界面合层.prefab` 及同目录 PNG

**Unity 版本**：2020.3.48f1  

---

> ## ⛔ 施工冻结（2026-07-13 修订 · 必读）
>
> **本文「合层 Sprite → 逐层搬进 `ShopPanel` UGUI」方案已弃用，禁止按下文 UI-0～UI-5 施工。**
>
> | 原因 | 说明 |
> |------|------|
> | **实机问题** | 按本文把合层搬进 Canvas / 收成正式 `ShopPanel` 后，**UI 位置错乱**（与世界坐标合层 + 已调好的 `UI_Shop` 对不齐） |
> | **现行做法** | 保持场景 **双轨**：世界空间 `商店界面合层`（相机对准）+ 场景内 `UI_Shop`（可点）；**不要**为进店/黑幕去 `OpenUIForm(ShopPanel)` |
> | **黑幕节奏** | 进店黑幕渐入渐出 **已解决**，与是否 ShopPanel 无关 |
> | **血条残留** | 另文处理：`0713/Village_Shop_进店关闭FightingPanel血条_执行说明.md`（仅 `CloseUIForm(FightingPanel)`） |
>
> 下文 §①～§⑩ **仅作历史溯源**（合层树、节点命名、与早期测试 Canvas 的对照），**不得当作当前排期任务书**。若将来重做「单面板」方案，须另开文档并先解决坐标/锚点基准，不可直接复活本文施工表。

---

## ① 结论（一句话）

> **〔历史结论 · 已冻结〕** `商店界面合层` 曾是「场景里摆着的 2D 精灵合层」；原文主张新建 `ShopPanel.prefab` 把合层图逐层搬进 Canvas。  
> **〔现行结论 · 2026-07-13〕** **不要**按该路径施工（会导致 UI 位置错乱）；继续 **合层 + `UI_Shop` 双轨**。详见文首冻结说明。

---

## ② 玩家在做什么 / 会遇到什么现象

| 现象 | 原因（生活类比） |
|------|------------------|
| 场景里能看到完整商店画面，但**点 Tab、点决定没反应** | 画面是「贴图展板」（SpriteRenderer），不是「带按钮的界面」（UGUI Button） |
| `Village_Shop` 里除了合层，还有**单独的 Canvas** | 程序在 Canvas 上搭了阶段一～四的测试控件（`Row_HpBall`、`BtnConfirm` 等），和合层**叠在一起、各管各的** |
| 合层里的「决定普通 / 决定选择 / 决定点击」只是**三张叠在一起的图** | 没有 Button 组件，也没有脚本监听鼠标 |
| 分辨率一变，合层和 Canvas **可能对不齐** | 合层用世界坐标 + SortingOrder；Canvas 用屏幕锚点，两套坐标系 |

**生活类比**：合层像把商店菜单**打印成海报**挂在场景里；UI 组件像把同一张菜单**装进带触控的平板**。外观可以一样，但结构必须重做。

---

## ③ 工程现状快照（架构侦探 · 只读）

### 3.1 美术合层 Prefab

| 项 | 现状 |
|----|------|
| **路径** | `Assets/ArtRes/Scene/Village/商店界面合层.prefab` |
| **根节点** | `商店界面合层` — 仅 `Transform`，无 Canvas |
| **渲染** | 全部子节点为 **`SpriteRenderer` + `Transform`**，Layer **0（Default）** |
| **深度** | 用 `SortingOrder` + 局部 **Z 偏移**（约 3～9）做前后关系，典型村庄场景合层做法 |
| **交互** | **无** Button / Image(Raycast) / EventTrigger |
| **源图目录** | `Assets/ArtRes/Scene/Village/商店界面合层/`（含 `组 7/组 6/` 下各状态图） |

### 3.2 合层 Hierarchy（完整树 · 来自 Prefab 扫描）

```
商店界面合层                          ← Transform 根，非 UI
├── 背景_2                            ← SpriteRenderer，远景/遮罩层
├── 背景                              ← SpriteRenderer，主背景 + 柜台
├── 组 7                              ← 商店「卷轴面板」区域
│   ├── 商店栏底                        ← 面板底图
│   ├── 组 6                            ← 面板内全部控件（仍全是 SpriteRenderer）
│   │   ├── 总价                        ← 静态装饰字「总价」
│   │   ├── 决定普通                      ← 决定按钮 · 常态
│   │   ├── 决定选择                      ← 决定按钮 · 悬停/选中
│   │   ├── 决定点击                      ← 决定按钮 · 按下
│   │   ├── 总价框                        ← 合计数字底框（需叠 TxtTotal 文本）
│   │   ├── 商品栏                        ← 列表行底图 ×1（策划有多行拷贝）
│   │   ├── 商品栏选择                    ← 行选中高亮
│   │   ├── 组 5 拷贝 … 组 5 拷贝 10    ← 共 10 张，列表占位行（美术命名）
│   │   ├── 购买组 / 购买组选择          ← Tab「购买」常态 / 高亮
│   │   └── 贩卖组 / 贩卖组选择          ← Tab「出售」常态 / 高亮
│   └── 商店栏边框                      ← 卷轴外框（SortingOrder 最高之一）
├── 正常体                            ← 老板娘身体立绘
└── 表情1                             ← 老板娘表情层（可切换）
```

> **与截图差异**：用户在 `Village_Shop` Hierarchy 里若只看到 5 个子节点，是因为 **组 7 未展开**；完整 Prefab 内 `组 6` 下共有 **17** 个子 Sprite。

### 3.3 测试场景 `Village_Shop.unity` 的双轨结构

| 轨道 | 物体 | 作用 | 问题 |
|------|------|------|------|
| **A · 美术轨** | Prefab 实例 `商店界面合层`（位置约 x:-8.95, y:-5.54） | 展示策划视觉稿 | 不可交互 |
| **B · 程序轨** | 根级 `Canvas` + `EventSystem` | 挂 `ShopFormLogic`，含 `Row_HpBall`、`TxtTotal`、`BtnConfirm` 等 | 控件是**临时方块**，未用合层美术 |

```
Village_Shop 场景（当前）
├── Main Camera
├── 商店界面合层          ← Prefab 实例（A 轨 · 只看）
├── Canvas                ← B 轨 · ShopFormLogic 在这里
│   ├── Button            ← 临时「购买」Tab（应对 btnBuy）
│   ├── Row_HpBall        ← 购买列表测试行
│   ├── TxtTotal
│   └── BtnConfirm
└── EventSystem
```

**结论**：阶段一～四的**逻辑已在 B 轨跑通**（见 `0629` 文档）；**视觉在 A 轨**。本任务目标是 **A + B 合并为一个正式 `ShopPanel` UI Prefab**。

### 3.4 已有程序资产

| 资产 | 路径 | 状态 |
|------|------|------|
| `ShopFormLogic` | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` | ✅ 阶段一～四逻辑；当前继承 **`MonoBehaviour`**，注释写明阶段五接 **`BaseUIFormLogic`** |
| `ShopBuyRowQuantityInput` | 同目录 | ✅ 行内 `TMP_InputField` 数量 |
| `ShopDebugLogger` | 同目录 | ✅ `[ShopDebug]` 假购买 Log |
| `ShopPanel.prefab` | `Assets/GameRes/Prefabs/UI/` | ❌ **尚未创建** |
| GF `OpenUIForm("ShopPanel")` | 全工程 | ❌ **尚未接入** |

### 3.5 工程 UI 标准（应对标的模板）

参考 `ItemShowPanel.prefab`、`MenuPanel.prefab`：

| 根节点必备组件 | 说明 |
|----------------|------|
| `RectTransform` | 全屏 stretch（anchor 0,0 → 1,1） |
| `Canvas` | 运行时由 `BaseUIFormLogic.Awake` 设为 **Screen Space - Camera** + UICamera |
| `CanvasScaler` | 推荐 **Scale With Screen Size**，参考分辨率 **1920×1080** |
| `GraphicRaycaster` | 接收点击 |
| `UIForm` | GameFramework 界面壳 |
| `ComponentSystemUI` | 工程 UI 组件系统 |
| **`ShopFormLogic`（未来改继承 `BaseUIFormLogic`）** | 业务逻辑 |
| Layer | **5（UI）** |

打开路径约定：`UIPrefabPath.GetUIPrefabPath("ShopPanel")` → `Assets/GameRes/Prefabs/UI/ShopPanel.prefab`

---

## ④ 目标架构（施工完成后应长什么样）

### 4.1 单一 Prefab，取代双轨

```
ShopPanel（Assets/GameRes/Prefabs/UI/ShopPanel.prefab）
├── [根组件] Canvas + CanvasScaler + GraphicRaycaster + UIForm + ShopFormLogic
├── BgLayer                         ← 原「背景_2」「背景」→ Image，纯装饰
├── ShopkeeperLayer                 ← 原「正常体」「表情1」→ Image；预留表情切换
└── ScrollPanel                     ← 原「组 7」区域
    ├── ImgFrame                    ← 商店栏边框 + 商店栏底（或合并为一层）
    ├── TabBar
    │   ├── BtnBuy                  ← 购买组 + 购买组选择 → Button SpriteSwap
    │   └── BtnSell                 ← 贩卖组 + 贩卖组选择
    ├── BuyPage
    │   └── ScrollView / Content
    │       ├── Row_HpBall          ← 对齐 0629 §3.5 命名
    │       └── Row_MpBall
    └── BottomBar
        ├── ImgTotalFrame           ← 总价框
        ├── TxtTotal                ← TMP/Text，程序刷新数字
        └── BtnConfirm              ← 决定普通/选择/点击 → Button
```

### 4.2 旧节点 → 新节点映射表（施工必查）

| 合层原节点（SpriteRenderer） | 新 UI 节点名 | 新组件 | 程序是否已有约定 |
|------------------------------|--------------|--------|------------------|
| 背景_2、背景 | `BgLayer/ImgBgBack`、`ImgBgMain` | Image | 否（纯装饰） |
| 正常体 | `ShopkeeperLayer/ImgBody` | Image | 否 |
| 表情1（及未来 表情2～5） | `ShopkeeperLayer/ImgFace` | Image | 否（后续表情脚本） |
| 商店栏边框 + 商店栏底 | `ScrollPanel/ImgFrame` | Image | 否 |
| 购买组 / 购买组选择 | **`BtnBuy`** | Button + Image | ✅ `ShopFormLogic.btnBuy` |
| 贩卖组 / 贩卖组选择 | **`BtnSell`** | Button + Image | 📝 0629 阶段六 |
| 商品栏 + 组5拷贝×N | **`Row_*` 行容器** | 见 §4.3 | ✅ `Row_HpBall` / `Row_MpBall` |
| 商品栏选择 | 行内 `ImgSelected` | Image，SetActive | 📝 选中态 |
| 总价框 | `BottomBar/ImgTotalFrame` | Image | 否 |
| 总价（静态字图） | 可保留或改 TMP 文案 | Image 或 Text | 否 |
| —（程序新建） | **`TxtTotal`** | Text / TMP | ✅ 阶段三合计 |
| 决定普通/选择/点击 | **`BtnConfirm`** | Button (SpriteSwap 三态) | ✅ 阶段四 |
| —（程序新建） | 行内 **`TxtStock`** | TMP_InputField | ✅ 阶段二 |
| —（程序新建） | 行内 **`TxtName` `TxtPrice` `Image`(图标)** | Text + Image | ✅ 阶段一 |

### 4.3 列表行 `Row_HpBall` 推荐结构（与现有脚本对齐）

现有 `ShopFormLogic` 通过节点名 **`FindDeepChild`** 查找，**名称必须一致**：

```
Row_HpBall
├── Image              ← 道具图标（ShopFormLogic 克隆 MpBall 时也 Find("Image")）
├── TxtName            ← 「生命之珠」
├── TxtPrice           ← 「200」（Awake 时脚本会写 HpBallUnitPrice）
└── TxtStock           ← TMP_InputField + ShopBuyRowQuantityInput
```

> **重要**：`0629` 文档写 `ImgIcon`，但 **`ShopFormLogic` 实际 Find 的是 `"Image"`**（见 `ApplyMpBallRowPresentation`）。施工以 **脚本为准**，节点名用 **`Image`**。

### 4.4 决定按钮三态 → Unity Button

| 合层 Sprite | Button 配置 |
|-------------|-------------|
| 决定普通 | **Target Graphic** + Transition = **Sprite Swap** → Normal |
| 决定选择 | Highlighted / Selected |
| 决定点击 | Pressed |

Tab 按钮（购买组/贩卖组）同理。

**替代方案说明**（二选一，施工前定一种）：

1. **Sprite Swap（推荐）**：一个 Button，三态图拖进 Inspector；最少节点，与 `MenuPanel` 习惯一致。  
2. **多 Image 叠层 + 脚本显隐**：保留合层三张图结构，用 `ShopFormLogic` 或小型 `ShopButtonHover` 监听 `PointerEnter/Exit`；节点多，但改图不改 Button 配置。

---

## ⑤ 分阶段施工（最小可运行优先）

与 `0629` §9 对齐；**本任务只覆盖「UI 壳子 + 对齐阶段一～四」**，不重复写交易/存档逻辑。

### 阶段 UI-0 · 新建 ShopPanel 壳（不动合层 Prefab）

| 做 | 不做 |
|----|------|
| 复制 `ItemShowPanel.prefab` → 另存为 `ShopPanel.prefab` | 不改 `商店界面合层.prefab`（保留美术参考） |
| 清空子节点，保留根上 Canvas / UIForm / ComponentSystemUI | 不接 GF 打开（下阶段） |
| 挂 `ShopFormLogic`（或先空壳） | 不删 `Village_Shop` 旧 Canvas（对照用） |

**验证**：Prefab 能拖进空场景，Game 视图全屏可见 Canvas。

### 阶段 UI-1 · 搬背景 + 老板娘

1. 从 `ArtRes/.../商店界面合层/背景.png`、`背景_2.png`、`正常体.png`、`表情1.png` 拖为 **Sprite (2D and UI)**。  
2. 在 `ShopPanel` 下建 `BgLayer`、`ShopkeeperLayer`，各子节点 **Add Component → UI → Image**。  
3. 按合层相对位置 **手调 RectTransform**（建议以 1920×1080 画布中心为基准，截图对照 Game 视图）。  
4. **Raycast Target**：背景 Image **取消勾选**（避免挡点击）；立绘层默认不挡，除非要做点头/点胸交互（见 §8 Q3）。

**验证**：Play `Village_Shop`，目视与合层实例**大致重合**（允许 1～2 像素级偏差，后续再修）。

### 阶段 UI-2 · 搬卷轴面板 + Tab + 底部栏

1. 建 `ScrollPanel`，搬入：商店栏边框、商店栏底、总价框、决定三态、购买/贩卖 Tab 图。  
2. Tab → **`BtnBuy` / `BtnSell`** Button；决定三态 → **`BtnConfirm`** Button。  
3. 新建 **`TxtTotal`**（Text 或 TMP），叠在总价框上；初始 `200` 或与脚本默认一致。  
4. 将 **`ShopFormLogic`** 拖到 `ShopPanel` 根，Inspector 绑定 `btnBuy`、`btnConfirm`、`txtTotal`。

**验证**：Play → 点购买 Tab → Console 无报错；改数量 → `TxtTotal` 仍随 `ShopFormLogic` 刷新（需完成 UI-3 才有行输入）。

### 阶段 UI-3 · 购买列表行（对接阶段一～四）

1. 在 `ScrollView/Content` 下手工搭 **`Row_HpBall`**（结构 §4.3）。  
2. 图标拖 `Assets/ArtRes/UI/Item/Icon/HpBall.png`；`Row_MpBall` 可交给脚本 **`autoCreateMpBallRow`** 克隆。  
3. `TxtStock` 按 `0629` §3.7 配 **TMP_InputField**。  
4. 绑定 `rowHpBall`；`btnBuy` 指向 `BtnBuy`。

**验证**：跑通 `0629` 阶段一～四验收表 P1-1～P4-4（在 ShopPanel 上，而非旧 Canvas）。

### 阶段 UI-4 · 替换测试场景 + 接 GF（程序施工）

| 步骤 | 说明 |
|------|------|
| 1 | `ShopFormLogic` 改继承 **`BaseUIFormLogic`**，`OnInit` 里做原 `Awake` 绑定（或保留 Awake 仅绑引用） |
| 2 | `ShopPanel` 根补全 **UIForm** 配置（对照 `MenuPanel`） |
| 3 | Resource Editor / AB 注册 `ShopPanel.prefab` |
| 4 | 场景内 **`OpenUIForm(UIPrefabPath.GetUIPrefabPath("ShopPanel"), ...)`** 测试入口（可先写在 `Village_Shop` 临时脚本或 AA_TestPanel） |
| 5 | 删除或禁用 `Village_Shop` 中旧 Canvas + 场景内 **`商店界面合层` 实例** |

**验证**：从 GF 打开商店 UI → 功能与 UI-3 一致；Esc / CloseForm 行为符合 `BaseUIFormLogic` 约定。

### 阶段 UI-5 · 打磨（可后置）

- `ScrollRect` + 滚动条（`0629` §3.4）  
- 行选中高亮（`商品栏选择`）  
- 老板娘表情切换（`表情2～5.png`）  
- 出售页、`ShopConfig.json`（`0629` 阶段五～六）

---

## ⑥ 坐标与分辨率迁移要点

合层使用 **世界单位**（Transform.localPosition，如 x:4.86, y:5.03, z:6.06）；UGUI 使用 **锚点 + anchoredPosition**。

| 建议 | 原因 |
|------|------|
| 新建 **1920×1080** 参考分辨率 | 与 `ItemShowPanel` 一致，便于复用 CanvasScaler |
| 以「组 7」外框为 **ScrollPanel 的 RectTransform**，子控件相对父节点布局 | 避免整屏绝对坐标难维护 |
| 列表行用 **Vertical Layout Group** 或固定 `Spacing` | 替代「组 5 拷贝 2～10」十张手动 Y 坐标 |
| Z 深度 **全部弃用** | UGUI 只靠 **Hierarchy 顺序 + Sibling Index** 决定前后 |

**替代方案**：若美术坚持像素级还原 PSD，可在 Photoshop / 合层里量相对坐标，写入表格后再填 RectTransform；不要从 SpriteRenderer 自动换算（Unity 无可靠一键工具）。

---

## ⑦ 验收清单（UI 组件化 · 本任务范围）

| # | 操作 | 期望 |
|---|------|------|
| U-1 | 打开 `ShopPanel.prefab` | 根为 Canvas；Layer 全为 UI(5)；**无** SpriteRenderer |
| U-2 | Play `Village_Shop`（或 GF 打开 ShopPanel） | 画面与旧合层**视觉可辨认**为同一套商店 UI |
| U-3 | 点击「购买」Tab | `Row_HpBall` / `Row_MpBall` 显示 |
| U-4 | 改 HpBall 数量 | `TxtTotal` 实时 ×200 |
| U-5 | 点「决定」 | Console：`[ShopDebug] 成功购买生命球，扣除金币 {n}` |
| U-6 | 场景中 **无** 独立 `商店界面合层` 实例 + 无重复 Canvas | 仅一个 ShopPanel 来源 |
| U-7 | 改 Game 窗口分辨率 | 面板整体缩放，不出现「按钮能点但图偏了半个屏」 |

---

## ⑧ 待确认问题（施工前对齐）

| ID | 问题 | 影响 |
|----|------|------|
| Q1 | `ShopPanel` 默认 **1920×1080** 还是跟合层 PSD 原始尺寸？ | CanvasScaler 与布局工时 |
| Q2 | 「组 5 拷贝 2～10」是 **10 行固定列表** 还是 **Scroll 动态行**？ | UI-3 用 Layout 还是 10 个预置 Row |
| Q3 | 老板娘 **点头/点胸** 交互（`0601` 老板娘特殊交互）是否画在同一 ShopPanel 上？ | 立绘层是否要透明 Button 热区 |
| Q4 | UI-4 何时改 **`BaseUIFormLogic`**？是否与 `0629` 阶段五（真扣金币）同一 PR？ | 程序排期 |
| Q5 | 正式进店场景用 **`Village_HomeScene4`** 还是继续 **`Village_Shop` 测试场**？ | 场景接入顺序（`0629` 阶段七） |

> 无结论时写入 `Assets/Doc/OPEN_QUESTIONS.md`，勿擅自改打开方式或存档逻辑。

---

## ⑨ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| 美术合层 Prefab | `Assets/ArtRes/Scene/Village/商店界面合层.prefab` |
| 合层 PNG 源 | `Assets/ArtRes/Scene/Village/商店界面合层/` |
| 测试场景 | `Assets/GameRes/Scenes/Village_Shop.unity` |
| 商店逻辑（阶段一～四） | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` |
| 行内数量 | `ShopBuyRowQuantityInput.cs` |
| UI 基类 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Base/BaseUIFormLogic.cs` |
| UI Prefab 路径约定 | `Assets/Scripts/Game/Static/Path/UIPrefabPath.cs` |
| 参考 UI Prefab | `Assets/GameRes/Prefabs/UI/ItemShowPanel.prefab` |
| 策划阶段与节点命名 | `Assets/Doc/执行文档/0629/商店系统_策划拆解_执行说明.md` |
| **目标产出 Prefab** | `Assets/GameRes/Prefabs/UI/ShopPanel.prefab`（待建） |

---

## ⑩ 文档变更记录

| 日期 | 说明 |
|------|------|
| 2026-07-04 | 初稿：扫描 `商店界面合层.prefab` + `Village_Shop` 双轨现状；给出 ShopPanel UGUI 迁移映射与 UI-0～UI-5 施工阶段 |
| 2026-07-13 | **施工冻结**：合层搬 ShopPanel 实机会 UI 位置错乱，禁止按本文排期；现行保持双轨；血条/黑幕见 0713 专文 |
