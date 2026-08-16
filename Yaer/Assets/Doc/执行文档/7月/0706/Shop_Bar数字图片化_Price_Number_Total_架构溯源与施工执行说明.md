# Shop · Bar 数字图片化（Price / Number / Total）— 架构溯源与施工执行说明

**文档版本**：v3（2026-07-06 · 间距二次收紧 + Total 五位容量）  
**文档性质**：架构侦探产出 + 施工指引（**IMG-0～5 已施工**；本版记录 Play/Bake 验收问题与修正口径）  
**触发**：美术已在 `Assets/ArtRes/UI/Text/` 提供 **0～9** 数字图片素材；策划要求 `Bar_ListScroll_Buy` / `Bar_ListScroll_Sell` 各行 **Price**、**Number** 与底部 **Total** 合计数字，全部改为图片显示，不再用系统字体 Text/TMP。

**v2 测试反馈（2026-07-06）**：
1. **Price 出现前导零**：买价 200 显示成 `0200`、卖价 20 显示成 `020`——应显示 **`200` / `20`**，禁止固定位宽补零。
2. **数字间距偏大**：Price / Number / Total 的 `DigitStrip.spacing` 初值 **4px** 观感过散；Number 列两位数字尤其明显，需整体收紧。

**v3 测试反馈（2026-07-06）**：
1. **Number 间距仍偏大**：v2 定稿 `spacing=0` 实测两位数量（如 `12`）仍显散，需再收紧（**允许负 spacing 叠字**）。
2. **Total2 合计间距偏大**：v2 定稿 `spacing=1` 仍偏散；且合计须 **最多容纳 5 位整数**（如 `99999`），在 `Total2` 底框（约 **144×45**）内完整显示、不裁切。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_Editor烘焙双列表_MainItemDatabase一键刷行_施工执行说明.md`（Price 烘焙时机 · EB 方案）
- `Assets/Doc/执行文档/0706/Shop_Total2双Tab全行合计与出售数量输入_架构溯源与施工执行说明.md`（Total2 合计区 · Number 输入能力）
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`
- 关联预制体：`Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab`
- 关联素材：`Assets/ArtRes/UI/Text/0.png` … `9.png`（另有 Day/Month/Year/P/Z，**本任务只用 0～9**）
- 可复用先例：`MenuCalendarDayNumDisplay.cs`（菜单日历 DayNum 十位+个位图片）

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**新增通用组件 `UiSpriteNumberDisplay`（0～9 图片横排），Price 在 Bake 时一次性刷图；Number 保留隐形 `TMP_InputField` 负责键盘输入，输入变化时同步刷图片；Total 合计改调同一组件显示整数——三者共用素材；数字按自然位数显示（200 非 0200）；间距 v3：**Number 用负 spacing 叠紧（约 -1px）**，**Total2 更紧（约 0～-1px）且 `poolCapacity=5` 容纳五位总价**。

---

## ② 玩家在做什么 / 会遇到什么现象

| # | 策划期望 | 当前工程（施工前） | 生活类比 |
|---|----------|-------------------|----------|
| 1 | 每行 **Price** 显示手写风数字图，不是黑体字 | Bake / `ShopBarRowView` 写 `Price.text = "200"` | 价签是打印体，和店招风格不搭 |
| 2 | 购买行 **Number** 能点、能输，**每敲一个数字就换一张图** | `TMP_InputField` 显示 TMP 字体；无图片层 | 收银台显示屏是液晶屏，不是木质数字牌 |
| 3 | 出售行 **Number** 同样可输、同样图片显示 | 出售行占位 Legacy Text `"1"`（0706 任务将改输入） | 同上 |
| 4 | 底部 **Total / Total2** 合计也是图片数字 | `ShopFormLogic.SetTotalText` 写 `TxtTotal` Legacy Text | 结账总额也是打印体 |
| 5 | 多位数（如 `200`、`600`）数字之间间距顺眼 | v2 后 Price/Total **1px**、Number **0px** 仍偏散 | 价签数字像「字距拉太开」 |
| 6 | 价格按**实际位数**显示，不补前导零 | 实测出现 `0200`、`020`（IMG-R1 已修） | 价签像电子表，200 元却写成 0200 |
| 7 | **Total2** 最多 **5 位**总价能完整显示在底框内 | 五位时可能挤/裁切；spacing 仍偏大 | 大额合计数字伸出底框 |

**生活类比**：现在商店像「Excel 表格」；目标是「木质价签 + 拨盘计数器」——数字是贴上去的图，但拨盘（键盘输入）还得能用。

---

## ③ 架构溯源

### 3.1 数字素材

| 路径 | 内容 | 导入设置（已存在） |
|------|------|-------------------|
| `Assets/ArtRes/UI/Text/0.png` … `9.png` | 单字符 Sprite | `Texture Type = Sprite (2D and UI)`，`spriteMode = Single` |
| 同目录 `Day.png` / `Month.png` / `Year.png` / `P.png` / `Z.png` | 非数字装饰字 | **本任务不用** |

`MenuPanel.prefab` 的 `DayNum` 已引用同一套 0～9 GUID（与 `0.png.meta` 的 `0101366b…` 一致），证明素材可直接复用。

### 3.2 列表行：Price / Number 现状

#### 3.2.1 `Shop_Bar.prefab` 节点快照

| 节点 | 组件（施工前） | 程序写入方 |
|------|----------------|------------|
| `Price` | Legacy `Text`，占位 `"200"` | `ShopListBakeEditor.SetText(..., "Price", price.ToString())`；`ShopBarRowView.ApplyPrice` |
| `Number` | Legacy `Text`，占位 `"200"` | 购买行：Bake 时 `ShopQuantityInputHelper.EnsureTmpIntegerInputField` 替换为 TMP InputField；出售行：`SetSellQuantityPlaceholder("1")` |
| `Name` / `Icon` | Text / Image | **本任务不动** |

#### 3.2.2 Price 数据流（静态 · 适合 Bake 刷图）

```
MainItemDatabase.entry.buyPrice / sellPrice
        ↓
ShopListBakeEditor.BakeContent
        → SetText(row, "Price", price.ToString())     // 当前：写 Text
        → ShopBarRowView.EditorSetBakedData(..., price, ...)
        ↓
运行时 ShopBarRowView.Awake：Price = bakedPrice（只读，不再改 UI）
```

**结论**：Price **运行时不变**，只需在 **Bake 时** 把 Text 换成图片即可；重跑 Bake 即刷新全表单价显示。

#### 3.2.3 Number 数据流（动态 · 核心难点）

```
玩家点击 Number 列
        ↓
TMP_InputField（ShopBuyRowQuantityInput 挂载）
        → onValueChanged / onEndEdit
        → OnQuantityValueChanged 事件
        → ShopFormLogic.RefreshTotal（0706：Σ 全行合计）
```

**当前缺口**：输入框的 **可见层是 TMP 字体**；没有任何组件把 `"12"` 映射为 `1.png` + `2.png`。

**定稿方案（透明输入 + 图片叠层）**：

```
Number（RectTransform 容器）
├── DigitStrip          ← 新增；挂 UiSpriteNumberDisplay；玩家「看到」的数字
├── RaycastImage        ← 近乎透明；接收点击（沿用 ShopQuantityInputHelper）
└── Text Area           ← TMP_InputField 子层级
        └── Text        ← 字体颜色 Alpha≈0、Caret 可隐藏；仅承载 IME/键盘逻辑
```

- **为什么保留 InputField**：工程已在 `ShopQuantityInputHelper` 完成整数校验、`characterLimit=2`、`QuantityForTotal` 等逻辑；删掉重写键盘/粘贴/失焦成本过高。
- **为什么加 DigitStrip**：满足「输入一个数字显示一张图」；`onValueChanged` 每变一字就 `digitDisplay.SetDigitString(text)`。
- **替代方案（不推荐）**：纯图片 + 自写点击区与按键监听——移动端、粘贴、退格边界多，易出垃圾代码。

### 3.3 底部合计：Total / Total2 现状

| 节点 | 场景现状 | 程序 |
|------|----------|------|
| `TxtTotal` | Legacy Text，`Enabled=0`，占位 `111` | `ShopFormLogic.SetTotalText(string)` |
| `Total2` | 仅 Image 底框，**无子节点** | 0706 任务拟改绑合计文案到此 |

**本任务约定**：合计数字显示统一用 **`UiSpriteNumberDisplay`**；挂载位置以 **0706 定稿的 Total2 子节点** 为准（建议节点名 `Total2_Digits`）。`SetTotalText` 升级为 `SetTotalNumber(int)` 或内部调 `UiSpriteNumberDisplay.SetNumber(total)`。

### 3.4 工程内可复用先例

`MenuCalendarDayNumDisplay`（`Assets/Scripts/Game/GameRuntime/UI/Component/MenuCalendarDayNumDisplay.cs`）：

- 固定 **十位 `tens` + 个位 `ones`** 两个 `Image` 子节点；
- `digitSprites[0..9]` + `SetDay(int)` 刷图；
- `hideTensWhenSingleDigit` 隐藏前导十位。

**与商店的差异**：

| 维度 | DayNum | Shop Price | Shop Number | Shop Total |
|------|--------|------------|-------------|------------|
| 位数 | 固定 2（1～31） | 1～3（如 5、200） | 1～2（`MaxQuantityDigits`） | 1～4+（Σ 全行，可能上千） |
| 变化时机 | 存档日期变 | Bake 时一次 | **每次按键** | 数量变时 |
| 对齐 | 居中 | 行内居中（与现 Price 一致） | **右对齐**（与现 InputField 一致） | 底栏居中或右对齐（与 Total2 美术） |

**结论**：不宜直接复用 `MenuCalendarDayNumDisplay`（API 绑死 Day 1～31）；应抽 **通用** `UiSpriteNumberDisplay`，DayNum 将来可迁（**非本任务**）。

---

## ④ 范围冻结

| 项 | 约定 |
|----|------|
| **替换范围** | `Shop_Bar` 的 `Price`、`Number`；场景 `UI_Shop` 底部合计数字（`Total2_Digits` 或等价节点） |
| **不替换** | `Name` 文案、道具 `Icon`、Tab 按钮文字、Total1 装饰底图 |
| **素材** | 仅 `ArtRes/UI/Text/0～9.png` |
| **Price 刷新** | Editor Bake；改价后 **再点 Bake** |
| **Number 刷新** | 运行时 `onValueChanged` + `onEndEdit` 同步图片 |
| **Total 刷新** | 沿用 `ShopFormLogic` 合计事件链（0706 全行 Σ） |
| **位数上限** | Price：按**实际位数**显示（1～4 位常见）；Number：`2`（`MaxQuantityDigits`）；**Total：`poolCapacity=5`**（最大 **5 位整数**，如 `99999`；仅池上限，显示仍无前导零） |
| **前导零（定稿 · 必守）** | **Price / Total / Number 失焦后**：严格按自然整数显示，**禁止** `PadLeft`、固定位宽补零。例：200→`[2][0][0]`（3 张图），**不是** `0200`（4 张）；20→`[2][0]`，**不是** `020`。仅 Number **输入过程中**允许单字符 `"0"`；空串合计按 0。 |
| **`poolCapacity` 语义** | 仅子节点池**预分配上限**，**不等于**固定位数；可见位数 = `value.ToString()` 或输入串实际长度，多余池位必须 `SetActive(false)`。 |
| **间距（v3 定稿）** | 见 §5.2；**Number 允许负值**；Total 与 Price **分常量**；Bake / `EnsureOn` / Prefab 须三处一致 |
| **本阶段不做** | 小数点、千分位、负号图片；Name/货币单位「G」图片化 |

---

## ⑤ 目标方案：`UiSpriteNumberDisplay`

### 5.1 组件职责

| 职责 | 说明 |
|------|------|
| 持有 `digitSprites[10]` | Inspector 拖入或 Editor 菜单从 `ArtRes/UI/Text` 一键填充 |
| 维护数字条子节点 | `HorizontalLayoutGroup` + 动态 `Image` 子物体池（按位数伸缩） |
| `SetNumber(int value)` | 整数 → **`value.ToString()`**（无前导零）→ 刷图；**禁止** `PadLeft` / `ToString("D4")` 等固定位宽 |
| `SetDigitString(string digits)` | 直接按字符刷图（**Number 输入中**用，保留 `"0"`、`""` 空串清空）；Price/Total **不走**补零路径 |
| 对齐 | `childAlignment`：Price 用 MiddleCenter；Number 用 MiddleRight |

### 5.2 推荐 Hierarchy（DigitStrip 模板）

```
DigitStrip                         ← UiSpriteNumberDisplay + HorizontalLayoutGroup
├── Digit_0                          ← Image（池化；SetNativeSize 或统一高度）
├── Digit_1
└── …（运行时按位数 Enable/Disable 或动态增减）
```

**间距调试参数（策划可调 · v3 二次收紧）**：

| 参数 | v1 | v2 | **v3 定稿** | 调哪里 | 备注 |
|------|----|----|-------------|--------|------|
| Price `spacing` | 4 | 1 | **0～1**（定稿 **0**） | `Price/DigitStrip` | 三位价 `200` |
| **Number `spacing`** | 4 | 0 | **-2～-1**（定稿 **-1**） | `Number/DigitStrip` | Unity `HorizontalLayoutGroup.spacing` **可为负**，用于两位叠紧 |
| **Total `spacing`** | 4 | 1 | **-1～0**（定稿 **0** 或 **-1**） | `Total2/Total2_Digits` | 五位合计须塞进底框；宜 **≤ Number 绝对值** |
| Total `poolCapacity` | — | 5 | **5（必守）** | `Total2_Digits` · `UiSpriteNumberDisplay` | 与 `ShopFormLogic.EnsureOn(..., capacity: 5)` 一致 |
| Total 底框 | — | 144×45 | **144×45**（现状）；五位仍裁切则略加宽或再减 spacing | `Total2` RectTransform | 优先调 spacing，再动底框 |
| 单字尺寸 | `SetNativeSize` | 同左 | 保持 `SetNativeSize` | Digit Image | 五位裁切时**勿**先压扁，先减 spacing |
| Number 列宽 | ≈47 | 同左 | 不变 | Prefab `Number` | spacing 为负后一般够用 |

**程序常量（v3 建议拆分）**：

| 常量 | v2 | **v3 定稿** | 用于 |
|------|-----|-------------|------|
| `ShopPriceSpacing` | 共用 `ShopPriceTotalSpacing=1` | **0f** | Price 行 |
| `ShopNumberSpacing` | `0f` | **-1f** | Number 行 |
| `ShopTotalSpacing` | 共用 `1f` | **0f** 或 **-1f** | **仅** Total2_Digits |
| `ShopTotalPoolCapacity` | 隐式 5 | **5** | Total2 Bake + `EnsureOn` |

> **为何 Number 用负 spacing**：0～9 图片字形两侧有透明留白，`spacing=0` 视觉仍像有空隙；**-1～-2** 让相邻 digit 略重叠，更接近旧版 TMP 紧凑感。若 `-1` 仍散，试 **-2**；若笔画粘连再回退 **-0.5**（Unity 支持 float）。

**程序默认传入（须与上表对齐）**：`UiSpriteNumberDisplay` 字段默认、各 `EnsureOn(..., stripSpacing)`、Bake 内 `SetSpacing`；改后 **保存 Prefab → 重跑 Bake → Play 复验 IMG-V10 / V11**。

### 5.3 三处接入方式

#### A · Price（静态）

| 步骤 | 操作 |
|------|------|
| Prefab | `Price` 下删/禁用 Legacy `Text`；加子节点 `DigitStrip` + `UiSpriteNumberDisplay` |
| Bake | `ShopListBakeEditor`：`SetSpriteNumber(row, "Price", price)` 替代 `SetText` |
| 运行时 | `ShopBarRowView.ApplyPrice` 改为调 `UiSpriteNumberDisplay`（兼容 Editor 未 Bake 场景） |

#### B · Number（动态输入）

| 步骤 | 操作 |
|------|------|
| Prefab | `Number` 下加 `DigitStrip`；保留 InputField 体系 |
| `ShopQuantityInputHelper` | 建 InputField 后：TMP `textComponent.color.a = 0`；`caretColor` 可选半透明或隐藏 |
| 新桥接 | `ShopBuyRowQuantityInput`（或 `ShopQuantityDigitBridge`）Awake 缓存 `UiSpriteNumberDisplay`；`onValueChanged` → `SetDigitString`；`onEndEdit` 校验后再刷一次 |
| Bake | `EnsureShopBuyRowQuantityInput` 后调用 `RefreshDigitDisplay(DefaultQuantity)` |
| 出售行 | 与购买行同一套（0706 已要求 Sell 也挂输入组件） |

#### C · Total（动态合计）

| 步骤 | 操作 |
|------|------|
| 场景 | `Total2` 下新建 `Total2_Digits`，挂 `UiSpriteNumberDisplay` |
| 位数 | **`poolCapacity = 5`**；`EnsureOn(..., capacity: 5)`；显示 `99999` 时为 **5 张图**（无前导零） |
| 间距 | **`ShopTotalSpacing`**（v3：**0 或 -1**），**独立于 Price**；五位须落在 `Total2` 底框（约 144×45）内 |
| 对齐 | `childAlignment = MiddleCenter`（与底框居中）；若五位贴边可改 `MiddleRight` + 略扩底框 |
| `ShopFormLogic` | `ResolveTotal2DigitsReference` → `SetTotal2Number(int)`；`total2Digits` Inspector 绑定优先，少运行时 `EnsureOn` |
| 初值 | 打开商店显示 `0`（与 0706 默认数量 0 一致） |

### 5.4 素材绑定方式（二选一）

| 方案 | 做法 | 推荐 |
|------|------|------|
| **A · 各 Display  Inspector 拖 10 张图** | 与 DayNum 一致；Bake 工具可提供「填充默认 DigitSprites」 | Prefab / 场景一次性配置 |
| **B · ScriptableObject `UiDigitSpriteSet`** | 全工程共用一份 0～9 引用；Display 只拖 SO | 多面板共用时更省事 |

**本任务推荐 A**（与现有 DayNum 一致，改动面最小）；若 Shop 行数多、不想每行手拖，由 **Bake 工具** 自动 `AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArtRes/UI/Text/{i}.png")` 写入 Display。

---

## ⑥ 目标 Hierarchy（施工完成后）

### 6.1 `Shop_Bar.prefab` 单行

```
Shop_Bar
├── Icon
├── Name
├── Price
│   └── DigitStrip              ← UiSpriteNumberDisplay；Bake 写入如 200 → [2][0][0]（3 位，无前导零）
└── Number
    ├── DigitStrip              ← 随输入刷新；右对齐
    ├── Image                   ← 透明点击底（raycastTarget）
    └── Text Area               ← TMP_InputField；文字不可见
        ├── Placeholder
        └── Text
```

### 6.2 场景 `UI_Shop` 底部

```
…
├── Total1                      ← 装饰；不动
├── Total2                      ← 底图 Image；约 144×45（v3：须容纳 5 位总价）
│   └── Total2_Digits           ← UiSpriteNumberDisplay；poolCapacity=5；spacing 0～-1
└── TxtTotal                    ← Legacy Text 保持 Disable 或删除绑定；程序不再写
```

### 6.3 Bake 后 Editor 验收（不 Play）

```
Bar_ListScroll_Buy/Viewport/Content/Shop_Bar_HpBall/Price/DigitStrip   → 可见买价图片
Bar_ListScroll_Sell/.../Shop_Bar_InsectBeak/Price/DigitStrip           → 可见卖价图片（如 5）
```

Number 列 Editor 下可显示默认 `0` 的图片；**输入交互仅 Play 验收**。

---

## ⑦ 施工阶段（IMG-0 ～ IMG-5）

| 阶段 | 内容 | 主要文件 | 验证 |
|------|------|----------|------|
| **IMG-0** | 新建 `UiSpriteNumberDisplay.cs` + 子节点池逻辑 + `SetNumber` / `SetDigitString` | `Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs` | 单独挂到测试空物体，`SetNumber(200)` 显示三张图 |
| **IMG-1** | 改 `Shop_Bar.prefab`：Price/Number 下加 DigitStrip；移除/禁用 Legacy Text | `Shop_Bar.prefab` | Prefab 模式目检结构 |
| **IMG-2** | `ShopListBakeEditor`：`SetSpriteNumber`；Bake 自动加载 0～9 路径；Buy+Sell 行 Price 刷图 | `ShopListBakeEditor.cs` | **不 Play**，Bake 后各行 Price 为图片 |
| **IMG-3** | `ShopQuantityInputHelper` 隐形 TMP；`ShopBuyRowQuantityInput` 绑 DigitStrip 同步 | `ShopQuantityInputHelper.cs`、`ShopBuyRowQuantityInput.cs` | Play：点 Number 输入 `12`，见两张图；退格变 `1` |
| **IMG-4** | `ShopFormLogic` + 场景 Total2_Digits；合计改 `SetTotalNumber` | `ShopFormLogic.cs`、`Village_Shop.unity` | Play：改数量，Total2 图片合计跟着变 |
| **IMG-5** | `ShopBarRowView.ApplyPrice` / 出售占位改图片；全量 Bake；间距策划微调 | `ShopBarRowView.cs` | EB-V 同类 + 本文件 §⑧ |
| **IMG-R1** | **修正前导零**：`SetNumber` / Bake 刷价确保 `value.ToString()`，可见位数=实际位数；排查 Prefab 池位未 Disable | `UiSpriteNumberDisplay.cs`、`ShopListBakeEditor.cs` | IMG-V9 |
| **IMG-R2** | **收紧间距 v2**：Price/Total **1**、Number **0** | 同上 | IMG-V10 |
| **IMG-R3** | **间距 v3 + Total 五位**：`ShopNumberSpacing=-1`；拆分 `ShopTotalSpacing`；Total `poolCapacity=5` 验收 `99999` | `UiSpriteNumberDisplay.cs`、`Shop_Bar.prefab`、`ShopListBakeEditor.cs`、`ShopQuantityInputHelper.cs`、`ShopFormLogic.cs`、`Village_Shop.unity` | IMG-V10、**IMG-V11** |

**与 0706 任务顺序建议**：

1. 若 0706（Total2 全行合计、默认 0、Sell 输入）**未做**：可先 IMG-0～3，IMG-4 与 0706 合并施工。  
2. 若 0706 **已做**：IMG-4 只替换 Total 的**显示层**，不改合计公式。

---

## ⑧ 验收清单

| ID | 操作 | 期望 |
|----|------|------|
| IMG-V1 | 跑 `Bake Shop Lists From MainItemDatabase`，**不 Play** | Buy/Sell 各行 **Price** 为图片数字，与 Database 一致 |
| IMG-V2 | 不 Play，看 HpBall Price | 买价 **200** 显示为 **三张**图（`2`+`0`+`0`），**不是** `0200` 四张；非 Text 字体 |
| IMG-V3 | Play → 购买 Tab → 点 Number 输入 `2` | 立刻显示 `2.png`；再输入 `0` 显示 `2`+`0` |
| IMG-V4 | 退格 / 全删 | 图片位数随删改；空串显示空白（或全 Disable） |
| IMG-V5 | 切 SELL Tab，出售行输入数量 | 同样有图片数字（0706 前置） |
| IMG-V6 | 改任意行数量 | **Total2** 合计图片更新（数值与 0706 公式一致） |
| IMG-V7 | 调 DigitStrip `spacing` | 多位价 `200` 三字间距可在 Inspector 微调，无需改代码 |
| IMG-V8 | Console | 无 NullReference；无重复 Instantiate 列表 Log |
| **IMG-V9** | Bake 后看 HpBall Price=**200**、某卖价 **20** 的行 | 分别显示 **3 张**、**2 张**数字图；**不得**出现 `0200` / `020` 式前导零 |
| **IMG-V10** | 对比 Price / Number / Total2 数字条 | v3：Number **≤-1px** 观感紧凑；Total **≤0px**；Price **≤1px** |
| **IMG-V11** | Play 调试或临时把合计改为 **99999**（或各行 qty 叠满） | Total2 显示 **5 张**数字图，**不超出** `Total2` 底框、无裁切 |
| **IMG-V12** | Number 输入 `12` | 两位数字间距明显小于 v2（`spacing=0`） |

---

## ⑧-b 测试反馈修正要点（IMG-R1 / IMG-R2）

### R1 · 前导零（Price / Total）

| 现象 | 根因（排查顺序） | 修正 |
|------|------------------|------|
| 200 显示为 `0200` | ① `SetNumber` 用了 `PadLeft` / `D4` 固定位宽；② `poolCapacity` 子节点未 Disable，占位 `0` 仍可见；③ Bake 未刷价，Prefab 占位 Digit 全为 0 | 统一 `SetDigitString(value.ToString())`；循环刷图时 `i >= digits.Length` 必须 `SetActive(false)` |
| 20 显示为 `020` | 同上，常见于 **3 位池 + 只应 2 位** | 验收以**可见 Image 个数**为准，不以 `poolCapacity` 为准 |

**禁止写法示例**（施工员勿用）：

```csharp
// BAD：固定位宽 → 200 变 "0200"
value.ToString("D4");
value.ToString().PadLeft(4, '0');
```

**正确写法**：

```csharp
// GOOD：自然位数
display.SetNumber(price);          // 内部 value.ToString()
display.SetDigitString(qtyText);   // Number 输入过程
```

### R2 · 间距收紧

| 位置 | 当前（v1） | v2 目标 | 修改点 |
|------|------------|---------|--------|
| `UiSpriteNumberDisplay.spacing` 字段默认 | `4f` | Price/Total **`1f`**，Number **`0f`** | 组件默认值 |
| `EnsureOn(..., stripSpacing)` | 各处 `4f` | Price/Total **`1f`**，Number **`0f`** | Bake、`ShopQuantityInputHelper`、`ShopFormLogic` |
| `Shop_Bar.prefab` | Price/Number DigitStrip `m_Spacing: 4` | **1 / 0** | Prefab 目调后保存 |
| `Village_Shop` Total2_Digits | 可能仍为 4 | **1** | 场景或 Bake 校正 |

改完后：**保存 Prefab → 重跑 Bake → Play 复验 IMG-V9 / V10 / V11**。

### R3 · 间距 v3（Number 负值 + Total 五位）

| 位置 | v2 | **v3 定稿** | 修改点 |
|------|-----|-------------|--------|
| `ShopNumberSpacing` | `0f` | **`-1f`**（可试 `-2f`） | `UiSpriteNumberDisplay` 常量 + `ShopQuantityInputHelper.EnsureOn` + Bake Number 行 |
| `ShopTotalSpacing`（新建） | 与 Price 共用 `1f` | **`0f` 或 `-1f`** | **仅** `Total2_Digits`、`ShopFormLogic`、`ShopListBakeEditor.EnsureTotal2Digits` |
| `ShopPriceSpacing` | `1f` | **`0f`** | Price 行 Bake / Prefab |
| Total `poolCapacity` | `5` | **`5`（确认未改小）** | 场景 `Total2_Digits`、Bake、`EnsureOn(..., capacity: 5)` |
| `Shop_Bar.prefab` Number DigitStrip | `m_Spacing: 0` | **`-1`** | Prefab 保存 |
| `Village_Shop` Total2_Digits | `spacing: 1` | **`0` 或 `-1`** | 场景或 Bake |

**五位总价验收**：临时 `SetTotal2Number(99999)` 或策划填满多行数量，目检 **5 位不裁切**；若裁切顺序：① Total spacing 再减 1 → ② `Total2` 宽 **144→160**（最后手段）。

---

## ⑨ 踩坑与约束

### 9.1 间距「多试几次」（v3）

- **程序只暴露 `spacing` 与 `SetNativeSize` 开关**；v3 起 **Price / Number / Total 三分常量**，禁止 Total 与 Price 共用一个偏大的值。
- **Number 优先用负 spacing**（-1 起试）；Total 五位场景同样可用 0～-1。
- Price 三位数与 Total 五位数 **poolCapacity 不同**（Price 约 4、Total **5**），但 **spacing 口径可不同**——Total 更紧。
- 若五位仍超出 `Total2` 底框，**先减 spacing，再加宽底框**，不要压扁 Sprite。

### 9.1-b 前导零与 `poolCapacity` 勿混淆

- **`poolCapacity` 只限制池大小**，不表示固定位数；**Total 池=5** 表示最多同时亮 5 张图（如 `99999`），200 元仍只亮 3 张。
- 若 Inspector 里看到 4～5 个 `Digit_*` 子节点但只应显示 3 位，属正常池化；**未使用的子节点必须隐藏**，否则会叠出前导 `0`。
- Price / Total 只调 `SetNumber(int)`；**不要**为「对齐」而补零。

### 9.2 InputField 与图片抢视觉

- TMP `Text` 必须 **Alpha≈0**，否则会出现「字体与图片叠影」。
- `caretWidth` 可设 0 或 caret 同色透明；玩家靠数字变化感知输入即可（或保留微弱光标，策划可选）。

### 9.3 Bake 只刷 Price，不刷 Number

- Number 默认值由运行时 `ResetToDefault` + Digit 同步负责；Bake 时 **不要** 把 Number 写成静态 Text。
- 出售行 Number 禁止再 `SetSellQuantityPlaceholder("1")` 写 Legacy Text（0706 已删此路径）。

### 9.4 与 `MenuCalendarDayNumDisplay` 的关系

- **不强行合并**两个类；先新建通用的 `UiSpriteNumberDisplay`。
- DayNum 迁移属技术债，**本任务不验收**。

### 9.5 性能

- 单行最多 2～4 个 Image，全表 &lt; 20 行；**禁止在 Update 刷图**，只在 `onValueChanged` / 合计刷新 / Bake 时调用。

---

## ⑩ 改动文件清单（施工员用）

| 文件 | 改动摘要 |
|------|----------|
| **新建** `UiSpriteNumberDisplay.cs` | 通用 0～9 图片数字条 |
| `Shop_Bar.prefab` | Price/Number 下 DigitStrip；禁用 Legacy Text |
| `ShopListBakeEditor.cs` | `SetSpriteNumber`；Bake Price |
| `ShopBarRowView.cs` | `ApplyPrice`、出售数量占位 → 图片 |
| `ShopQuantityInputHelper.cs` | 隐形 TMP 配置 |
| `ShopBuyRowQuantityInput.cs` | 输入事件 → DigitStrip 同步 |
| `ShopFormLogic.cs` | Total → `UiSpriteNumberDisplay` |
| `Village_Shop.unity` | Total2 下 `Total2_Digits` |
| **IMG-R1/R2/R3** | `UiSpriteNumberDisplay.cs`：前导零 + spacing 常量拆分；Prefab / Bake / 场景同步 |

---

## ⑪ 文档关系

| 文档 | 关系 |
|------|------|
| `Shop_Editor烘焙双列表_…md` | Price **烘焙时机**沿用；`SetText(Price)` 改为 `SetSpriteNumber` |
| `Shop_Total2双Tab全行合计_…md` | 合计**公式与事件**沿用；仅 **显示层** 字体→图片 |
| `MenuCalendarDayNumDisplay` | 素材与思路先例；API **不直接复用** |

---

## ⑫ 修订记录

| 日期 | 说明 |
|------|------|
| 2026-07-06 | 首版：架构溯源；定稿「透明 InputField + UiSpriteNumberDisplay」；IMG-0～5 + IMG-V1～V8 |
| 2026-07-06 | **v2**：Play 验收反馈——禁止 Price/Total 前导零（200 非 0200）；spacing 初值由 4px 收紧为 Price/Total **1px**、Number **0px**；增 IMG-R1/R2、IMG-V9/V10、§⑧-b |
| 2026-07-06 | **v3**：Number/Total 间距仍偏大——Number **-1px**（可负）、Total **0～-1px** 且 **poolCapacity=5** 容纳五位总价；拆分 `ShopTotalSpacing`；增 IMG-R3、IMG-V11/V12、§⑧-b R3 |
