# Shop · Bar.Name 改「商店名图」+ MainItem 增 `shopNameSprite` — 架构溯源与施工执行说明

**文档版本**：v1.1（2026-07-15 · 补充中/英/日三语名图）  
**文档性质**【架构侦探】**：只读溯源 + 施工指引（**本阶段只写文档，不改代码 / Prefab / Asset**）  
**触发**：商店货架上显示的 **道具名（`Shop_Bar.Name`）** 要从「打字出来的字」全部换成「美术画好的一整张名牌图」；且名图要有 **中文 / 英文 / 日文** 三套，跟现有详情文案 `detail` / `detailEn` / `detailJp`、以及工程语言后缀（中文无后缀、英 `_en`、日 `_jp`）对齐；数据必须从 **`MainItemDatabase`（道具总档案）** 出发，不要在商店货单里再抄一遍。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/MainItem_道具固有属性表_架构溯源与施工执行说明.md`（`icon` / `displayName` 唯一源范式）
- `Assets/Doc/执行文档/0706/Shop_Bar数字图片化_Price_Number_Total_架构溯源与施工执行说明.md`（价签已图片化；**本次做 Name**）
- `Assets/Doc/执行文档/0713/Shop_Bar_Name改TMP描边投影_架构溯源与施工执行说明.md`（Name 已换成 TMP；**本任务将再换成 Image，视为对商店 Name 展示的下一形态**）
- 关联脚本：`MainItemDefEntry.cs` / `MainItemDef.cs` / `MainItemDefProvider.cs` / `ShopBarRowView.cs` / `ShopListBakeEditor.cs`
- 关联资源：`MainItemDatabase.asset`、`Shop_Bar.prefab`、`Village_Shop.unity`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**在 `MainItemDefEntry` 增加三语商店名图：`shopNameSprite`（中）/ `shopNameSpriteEn`（英）/ `shopNameSpriteJp`（日），经 Provider 按当前游戏语言解析后供给商店；`Shop_Bar.Name` 从 TMP 改为 `Image`；Bake / Bind / 切语言时贴对应 Sprite，不再写 `displayName`。`displayName` 文字仍留给背包等；货品 `icon` 与店招名图仍是两条槽。**

**生活类比**：
- 现在货架价签上的「生命球」三个字，是打印机现打的（TMP）。
- 目标改成：每种货在档案柜里准备 **三张店招贴纸**（中/英/日）；上架时按顾客选的语言撕对应那一张。
- 背包里仍可读档案上的**印刷品名**（`displayName`），店里只认贴纸。

---

## ② 玩家在做什么 / 会遇到什么现象

| # | 策划期望 | 当前工程 | 生活类比 |
|---|----------|----------|----------|
| 1 | 买/卖列表每行道具名是**名牌图**，不是字体 | `Name` = `TextMeshProUGUI`，Bake 写 `entry.displayName` | 价签名是打印字 |
| 2 | 名图绑在**道具档案**上，改一处买/卖两边一起对 | Database 只有 `icon` + `displayName` 字符串，**没有「店招名图」槽** | 档案柜只有商品照片和印刷名，没有店招贴纸 |
| 3 | **中 / 英 / 日** 各有一套名图，切语言货架跟着换 | 详情已有 `detail` / `detailEn` / `detailJp`；商店 Name **无**三语文图 | 店招只备了中文纸，英文顾客来了仍贴中文 |
| 4 | 与价签 Price（数字图）观感一致 | Price 已 `UiSpriteNumberDisplay`；Name 仍是字 | 价钱是木质数字牌，名字却是黑白打印 |
| 5 | 缺图时能看出来；某语缺图有明确回退 | 无解析链路；缺则占位 TMP 或旧 Bake 字 | 忘贴贴纸没人提醒；缺英文时也不知贴哪张 |

---

## ③ 架构溯源（只读）

### 3.1 数据层：MainItem 固有属性现状

| 字段（`MainItemDefEntry`） | 用途 | 商店是否已用 |
|----------------------------|------|--------------|
| `itemId` | 唯一键 | ✅ Bake / Row 身份 |
| `icon` | 列表/背包小图标（**无分语言**，商品图共用） | ✅ 写到 `Icon` |
| `displayName` | 中文**字符串**名 | ✅ **写到 `Name`（TMP）** ← 本任务要换掉这条展示路径 |
| `buyPrice` / `sellPrice` | 固有价 | ✅ 写到 Price 数字图 |
| `itemType` | 上架过滤 | ✅ |
| `detail` / `detailEn` / `detailJp` | 详情**文字**三语（中/英/日） | 别处用；**名图应对齐同一三语范式** |

**工程语言后缀先例**（`LanguageType.GetLanaguageResTag`）：

| 语言 | `LanguageEnumType` | 资源后缀 tag |
|------|-------------------|--------------|
| 中文 | `Chinese` | `""`（无后缀） |
| 英文 | `English` | `"_en"` |
| 日文 | `Japanese` | `"_jp"` |

其它 UI（如章末标题、设置页、Tips 按钮）已按「当前语言 + tag 找图，缺则回退英文」工作；商店名图应走同一习惯。

**缺口**：没有「商店专用整图名」字段，更没有中/英/日三槽。不能拿 `icon` 冒充名图（Icon 是货品图且不分语言；Name 是文字牌，美术尺寸与构图都不同）。

### 3.2 展示层：`Shop_Bar.Name` 现状（2026-07-15 快照）

```
Shop_Bar
├── Shadow
├── Icon          ← Image（Database.icon）
├── Name          ← TextMeshProUGUI（displayName 字符串）  ★ 本任务目标
├── Price         ← 数字图片（已完工）
└── Number        ← 数字图片 + 隐形输入（已完工）
```

| 项 | 值 |
|----|-----|
| Prefab | `Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab` |
| 节点 | `Name` |
| 组件 | **TMP**（`TextMeshProUGUI`），字号 25，居中，Raycast 关 |
| Rect 约 | 宽 **195** × 高 **34**（锚点中心，位置约 x=-67.7） |
| 写入口（Bake） | `ShopListBakeEditor` → `SetText(row, "Name", entry.displayName)` |
| 写入口（动态） | `ShopBarRowView.Bind` → `ApplyName(def.DisplayName)` → 写 TMP/Legacy |

**侦探结论**：商店 Name **唯一业务文案源**已是 MainItem；要换图，正确切口是 **Database 加三语 Sprite 槽 + Bake/Row 按当前语言写 Image**，而不是在 `ShopCatalog` 或 Prefab 上再抄一套名字。

### 3.3 数据流（现状 → 目标）

```
【现状】
MainItemDatabase.entry.displayName (string · 仅中文印刷名)
        ↓
ShopListBakeEditor.SetText(..., "Name", displayName)
ShopBarRowView.ApplyName(displayName)
        ↓
Shop_Bar.Name  →  TextMeshProUGUI.text

【目标】
MainItemDatabase.entry
  ├── shopNameSprite     （中文店招图）
  ├── shopNameSpriteEn   （英文店招图）
  └── shopNameSpriteJp   （日文店招图）
        ↓
Provider.ResolveShopNameSprite(itemId, curLanguage)
  // 按 LanguageEnumType 取槽；缺图回退见 §5.3
        ↓
ShopListBakeEditor.SetImageSprite(..., "Name", resolved)   // Editor 预览默认中文槽
ShopBarRowView.ApplyShopName(resolved)                     // Play：当前语言；切语言重刷
        ↓
Shop_Bar.Name  →  Image.sprite（节点名仍叫 "Name"，组件从 TMP 换 Image）
```

### 3.4 与既有文档 / 多语言惯例的关系

| 文档 / 惯例 | 本任务关系 |
|-------------|------------|
| 0704 MainItem 固有属性表 | **继承其范式**：再加 Sprite 槽；三语命名对齐 `detail` / `detailEn` / `detailJp` |
| 0706 Price 数字图片化 | **并列**：价用 0～9 拼图（数字无语种）；名用**整张店招图 × 三语** |
| 0713 Name 改 TMP 描边投影 | **商店 Name 展示被本任务取代**；TMP 描边仅作过渡 |
| 背包对接 Database | **不动**：背包继续用 `icon` + `displayName` 文字（背包文字多语言另案） |
| `LanguageType.GetLanaguageResTag` | 文件名兜底后缀：`""` / `_en` / `_jp`；缺图回退习惯对齐 Tips「缺语种用英文」 |

---

## ④ 范围冻结

| 项 | 本阶段约定 |
|----|------------|
| **新增字段（三语）** | **`shopNameSprite`**（中）/ **`shopNameSpriteEn`**（英）/ **`shopNameSpriteJp`**（日） |
| **唯一维护处** | `MainItemDatabase.asset` 每条 Entry 拖三张（上架道具尽量三语齐；见缺图回退） |
| **商店 Name 节点** | 保留子物体名 **`Name`**；组件改为 **`UnityEngine.UI.Image`**；去掉 TMP / Legacy Text |
| **运行时选图** | `ResolveShopNameSprite(itemId, GameManager 当前语言)` |
| **Bake** | Editor 预览默认贴 **中文槽**（`shopNameSprite`）；**不再** `SetText(Name, displayName)`；切语言验收走 Play，不必为每种语言各 Bake 一次场景 |
| **`displayName` 字符串** | **保留**；供背包、菜单、Debug；商店货架**不展示**该字符串 |
| **本阶段要做** | 三语名图数据槽 + 解析 + 商店 Name 贴图；Play 中切语言应能换名图（见 SN-7 / §5.4） |
| **本阶段不做** | 背包名也改图、用单字 Sprite 拼多语言名、改交易/滚动/价格逻辑 |
| **本阶段不做（续）** | 运行时图集异步兜底强依赖（Editor PNG 兜底见 §5.2） |

---

## ⑤ 目标数据与解析规则

### 5.1 字段定义（给程序）

**`MainItemDefEntry` 追加**（建议插在 `icon` 与 `displayName` 之间；Inspector 顺序：**图标 → 店招中/英/日 → 印刷名**）：

```csharp
[Tooltip("商店货架道具名图 · 中文（整图）。对齐 LanguageEnumType.Chinese / 资源后缀空")]
public Sprite shopNameSprite;

[Tooltip("商店货架道具名图 · 英文（整图）。对齐 LanguageEnumType.English / 后缀 _en")]
public Sprite shopNameSpriteEn;

[Tooltip("商店货架道具名图 · 日文（整图）。对齐 LanguageEnumType.Japanese / 后缀 _jp")]
public Sprite shopNameSpriteJp;

[Tooltip("中文展示名（原 MainItemConfig.cnName）；背包/菜单/日志用；商店货架改读三语 shopNameSprite*")]
public string displayName;
```

命名说明：中文槽不加 `Cn` 后缀，与现有 `detail`（中）/ `detailEn` / `detailJp` **一致**，避免一半叫 `shopNameSpriteCn`、一半叫 `detail`。

**`MainItemDef` 追加只读**（三语都暴露，解析在 Provider）：

```csharp
public Sprite ShopNameSprite { get; }      // 中
public Sprite ShopNameSpriteEn { get; }    // 英
public Sprite ShopNameSpriteJp { get; }    // 日
```

### 5.2 解析优先级（定稿倾向）

```
ResolveShopNameSprite(itemId, language)：

1. 按语言取 Entry 槽：
     Chinese  → entry.shopNameSprite
     English  → entry.shopNameSpriteEn
     Japanese → entry.shopNameSpriteJp
2. 若该语槽为空 → Editor 兜底 PNG：
     Assets/ArtRes/UI/Item/ShopName/{itemId}{resTag}.png
     // resTag = LanguageType.GetLanaguageResTag(language)
     // 例：HpBall.png / HpBall_en.png / HpBall_jp.png
3. 若仍空 → 语言回退链（与工程 Tips 习惯对齐）：
     当前语 → English 槽（及 _en.png）→ Chinese 槽（及无后缀.png）→ null
4. null → Warning + Name.Image 空（见 §5.3）
```

| 建议资源目录 | 说明 |
|--------------|------|
| **`Assets/ArtRes/UI/Item/ShopName/`** | 与 `Icon/` 并列；**新建目录**；三语图同目录 |
| 文件名（自动兜底） | `{itemId}.png` / `{itemId}_en.png` / `{itemId}_jp.png` |
| Database 手拖 | **允许**任意文件名（含中文美术名）；手拖后不依赖磁盘文件名 |
| 导入 | `Texture Type = Sprite (2D and UI)`，`Sprite Mode = Single` |
| 画幅建议 | 适配 Name 槽约 **195×34**；三语图建议同高，免切语言时跳动 |

**示例（`HpBall`）**：

| 语言 | Database 槽 | 兜底文件名 |
|------|-------------|------------|
| 中文 | `shopNameSprite` | `HpBall.png` |
| 英文 | `shopNameSpriteEn` | `HpBall_en.png` |
| 日文 | `shopNameSpriteJp` | `HpBall_jp.png` |

**替代方案（不推荐作主路径）**：

| 方案 | 做法 | 为何不优先 |
|------|------|------------|
| A. Database 拖三语 `shopNameSprite*`（推荐） | 与 `icon` + `detail*` 一致 | — |
| B. 仅一张中文图 + 运行时 TMP 翻英/日 | 省美术 | **违背「名字全是图片」**；英日又变回字 |
| C. 单一图集 key + 语言 tag 自动取，不拖三槽 | 像章末 `titleTips`+tag | 可作日后优化；本期仍要 Inspector 三槽可验收 |
| D. ShopCatalog 分语言挂图 | 改货单 | **违反** MainItem 唯一源 |

### 5.3 缺图行为（定稿倾向）

| 情况 | 行为 |
|------|------|
| 当前语图空，但英/中有图 | 按 §5.2 回退链贴图；可打一次 Warning 记「用了回退语」 |
| 三语皆空（含 PNG 兜底） | Name.Image.sprite=null，`enabled=false`；Bake summary / Play Log 列出 itemId；**禁止**回退写 `displayName` 字 |
| 图在但尺寸不对 | 以 Rect 为准目测；不在本任务做自动缩放算法（见 §⑩） |

### 5.4 Bake 预览 vs Play 切语言（重要）

| 时机 | 贴哪张 |
|------|--------|
| Editor **Bake** | 固定用 **中文槽**（场景里一眼能验收「有图」；不必为英/日各 Bake 一份场景） |
| Play **进店 / Bind** | `ResolveShopNameSprite(itemId, 当前游戏语言)` |
| Play **设置里改语言** | 商店若仍开着：遍历可见 `ShopBarRowView` 再 `ApplyShopName`（幂等）；不要在 `Update` 里轮询 |

**复杂逻辑替代说明**：若坚持「场景 Bake 死贴死一种语言、Play 永不重刷」——切语言后货架会错语，**不可接受**；故 Bake 只负责「有图 + 中文预览」，真正语种以 Play Resolve 为准。

---

## ⑥ 目标代码改动清单（施工员按序）

> 原则：最小化；禁止在 `Update` 里刷名图；商店仍以 **Editor Bake** 放行资源，**Play Resolve** 负责语种。

| 步骤 | 文件 | 做什么 |
|------|------|--------|
| SN-1 | `MainItemDefEntry.cs` | 加 `shopNameSprite` / `shopNameSpriteEn` / `shopNameSpriteJp` + 注释/Tooltip |
| SN-2 | `MainItemDef.cs` | 加三语只读属性 + 构造参数 |
| SN-3 | `MainItemDefProvider.cs` | `RebuildCache` 传入三语；**`ResolveShopNameSprite(itemId, language)`**（§5.2） |
| SN-4 | `MainItemDatabase.cs`（若有校验） | 上架道具缺**中文**名图 Warning；英/日缺可降级 Tip（勿 Error 挡进 Play） |
| SN-5 | `Shop_Bar.prefab` | `Name`：移除 TMP → 加 `Image`；Raycast 关；Color 白 |
| SN-6 | `ShopListBakeEditor.cs` | `SetText(Name,…)` → `SetImageSprite(Name, Resolve…Chinese)`；缺中图记 summary；缺 Image 组件 Error |
| SN-7 | `ShopBarRowView.cs` | `nameText*` → `Image nameImage`；`ApplyShopName(Sprite)`；`Bind` / 可选 `RefreshShopNameForLanguage()` |
| SN-8 | `ShopFormLogic.cs`（小改） | 进店 Ensure 绑定后按当前语言刷一遍 Name；若已有设置「改语言刷新 UI」钩子，挂上重刷 Name（无钩子则进店时刷一次即可，店内切语言验收见 §⑩ Q6） |
| SN-9 | `MainItemDatabase.asset` | 上架道具拖齐三语名图（至少中文必填；英日尽量齐） |
| SN-10 | 美术资源 | `ShopName/{itemId}.png` + `_en` + `_jp`；或仅手拖、不依赖文件名 |
| SN-11 | 场景 | Bake → 存 `Village_Shop` |

**可不改**：交易/合计/Tab 核心；`UiSpriteNumberDisplay`；背包脚本。

**复杂逻辑替代说明（给审查）**：禁止「Bake 仍写字再叠图」双轨；禁止「一张图靠代码热替换文字」冒充三语。

---

## ⑦ Prefab / Bake 施工步骤（人机可读）

```
① 美术：每个上架道具准备三张店招图（中/英/日）
   放入 ArtRes/UI/Item/ShopName/
   建议名：{itemId}.png / {itemId}_en.png / {itemId}_jp.png
        ↓
② 程序 SN-1～3 合入后，打开 MainItemDatabase.asset
   每条上架道具拖齐 shopNameSprite / En / Jp
        ↓
③ 打开 Shop_Bar.prefab → Name
   Remove TextMeshProUGUI → Add Image
   Preserve Aspect 默认 On
        ↓
④ 程序改 Bake + Row +（可选）Form 切语言重刷（SN-6～8）
        ↓
⑤ Village_Shop → Tools → Shop → Bake … From MainItemDatabase
   （预览应为中文名图）
        ↓
⑥ 不 Play：Buy/Sell 每行 Name 是图不是字
        ↓
⑦ Ctrl+S → Play 进店
   设置切 中/英/日，确认货架名图切换（或至少重新进店后语言正确）
```

**为何必须再 Bake**：列表行是场景实例；只改源 Prefab / Database 不 Bake，旧行仍可能挂 TMP。

---

## ⑧ 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | Inspector 打开 Database | 上架道具可见 **三个** 名图槽，均可拖 |
| 2 | Bake 后 Buy/Sell | Name 为**中文**名图，无 TMP 字 |
| 3 | 故意清空某条中文槽再 Bake | summary 标缺；Name 空且不回退成字 |
| 4 | Play：语言=中文进店 | 中文名图 |
| 5 | Play：语言=英文（缺英图时应回退英链→中） | 英文名图或文档约定的回退图；**不是**中文字符串 |
| 6 | Play：语言=日文 | 日文名图或回退链 |
| 7 | 滚轮 / Tab / Price Number | 不受影响 |
| 8 | 背包同道具 | 仍见文字名 / Icon（只动商店 Name） |

建议日志前缀：`[ShopNameSprite]`。

---

## ⑨ 提交说明模板（施工完成后填）

**改了哪些文件**：  
（例）`MainItemDefEntry.cs` / `MainItemDef.cs` / `MainItemDefProvider.cs` / `ShopBarRowView.cs` / `ShopListBakeEditor.cs` /（可选）`ShopFormLogic.cs` / `Shop_Bar.prefab` / `MainItemDatabase.asset` / `Village_Shop.unity` / `ShopName/*.{png}`（含 `_en` `_jp`）

**实现了什么**：商店货架道具名改为 Database 三语 `shopNameSprite*` 整图；按当前语言解析显示。

**如何验证**：按 §⑧；正规进店 Init → 村 → Door_Shop；设置里切换中/英/日。

---

## ⑩ OPEN_QUESTIONS（未拍板 · 先记此处，勿擅自定核心方向）

| ID | 问题 | 施工默认（文档建议） | 状态 |
|----|------|----------------------|------|
| Q1 | 名图 Image：`Preserve Aspect` 开还是拉满 Rect？ | **On**，居中 | 待确认 |
| Q2 | 是否要做运行时图集兜底（类似 Icon 的 MainItem_Icon）？ | **本期不做**；Database 拖图 + Editor PNG 兜底够用 | 待确认 |
| Q3 | 任务道具 / 未上架道具是否也要填三语名图？ | **否**，仅买/卖上架需要 | 待确认 |
| Q4 | 0713 TMP 名材质 / 专用 mat 是否删除？ | **可留资源**；商店 Name 不再引用即可 | 待确认 |
| Q5 | 手拖时是否允许中文文件名？ | **允许手拖**；自动兜底仍用 `{itemId}{_en|_jp}.png` | 待确认 |
| Q6 | 店内打开设置立刻改语言，是否要求**不关店**即换名图？ | **尽量做** Form 钩子重刷；若设置关商店再开，至少「重进店正确」为底线 | 待确认 |
| Q7 | 某语缺图回退：英→中 还是 英→中→空？ | 文档 §5.2：**当前 → 英 → 中 → null** | 待确认 |

有结论后可同步更新 `Assets/Doc/OPEN_QUESTIONS.md` 或在本表改「已决议」。

---

## ⑪ 给程序看的补丁要点（极短）

1. **新固有字段（三语）**：`shopNameSprite` / `shopNameSpriteEn` / `shopNameSpriteJp` → Def 只读同源。  
2. **解析**：`ResolveShopNameSprite(itemId, language)` + `_en`/`_jp` 文件兜底 + 缺图回退英→中。  
3. **Bake**：只贴中文槽预览；`SetImageSprite(Name, …)` 替代 `SetText`。  
4. **Row / Form**：`Image` + `ApplyShopName`；Play 按当前语言；切语言能重刷。  
5. **Prefab**：`Name` 只留 Image。  
6. **`displayName` 不删**；商店货架不读它。

---

**文档路径**：`Assets/Doc/执行文档/0715/Shop_Bar_Name改商店名图_MainItem加shopNameSprite_架构溯源与施工执行说明.md`

| 版本 | 日期 | 说明 |
|------|------|------|
| 0.1 | 2026-07-15 | 架构侦探首版：只文档、不施工 |
| 0.2 / v1.1 | 2026-07-15 | 补充中/英/日三语名图字段、解析、Bake vs Play、验收 |
