# Shop · Editor 烘焙双列表（MainItemDatabase 一键刷行）— 施工执行说明

**文档版本**：v1（2026-07-05）  
**文档性质**：架构定稿 + 施工指引（**取代「运行时 Awake 刷新列表」方案**）  
**触发**：现有 `Tools/Shop` 菜单与策划设想不符——跑完工具后 Content **仍是空的**，必须 **Play** 才能看到行与 Icon；且 `ShopFormLogic` 在 **Awake/Start 反复 Refresh**，Editor 里无法验收。

**策划设想（本任务唯一目标）**：

> 点 **一次** 菜单 → 场景里直接出现 **`Bar_ListScroll_Buy`** 和 **`Bar_ListScroll_Sell`** → 两个 `Content` 下 **Shop_Bar 行已按 MainItemDatabase 刷好 Icon / Name / Price** → **不用进 Play** 验收，**运行时也不再 Instantiate / Refresh 列表**。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_货单瘦身_MainItemDatabase驱动Shop_Bar刷新_架构溯源与施工执行说明.md`（**数据过滤规则保留；刷新时机改为 Editor 烘焙**）
- `Assets/Doc/执行文档/0704/Shop_货单瘦身…md` §⑪ Play 问题（Icon / 双 Scroll / 滚轮 — 并入本工具一步做完）
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`
- 关联资源：`MainItemDatabase.asset`、`Shop_Bar.prefab`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**合并现有分散 Tools 为单一菜单 `Tools → Shop → Bake Shop Lists From MainItemDatabase`：Editor 内创建/校正 Buy+Sell 双 Scroll 壳 → 读 `MainItemDatabase` 过滤 → 在两侧 `Content` 下 `InstantiatePrefab` 并 **烘焙** Icon/Name/Price 到场景节点 → 保存场景；`ShopFormLogic` 运行时 **只** 扫已有子节点做 Tab/合计/交易，**删除** Awake/Start 的 `RefreshBuyList` / `RefreshSellList`。**

---

## ①.1 与旧方案对比（为何现有 Tools 不符合设想）

| 维度 | 旧方案（当前工程） | 新方案（本任务） |
|------|-------------------|------------------|
| 菜单 | `Setup Bar List Scroll` + `Setup Database Driven Lists` **两个**，职责重叠 | **一个** Bake 菜单全流程 |
| 跑完工具后 Content | **空**（文案写「运行时生成」） | **已有 N/M 行 Shop_Bar，数据已写入** |
| 验收方式 | 必须 **Play** | **Scene / Game 视图（未 Play）** 即可见 Icon 与文案 |
| 运行时 | `Awake` + `Start` 调 `RefreshBuyList/SellList`，Instantiate + Bind | **零刷新**；只 `CollectRowViews` + Tab 切换 |
| Icon | 依赖 Play 时 Provider / 图集异步 | Editor 同步读 Database `entry.icon` + `AssetDatabase` 兜底，**写进 Image.sprite** |
| Database 变更 | Play 时可能才变 | 策划 **再点一次 Bake** 重刷场景 |

**生活类比**：旧方案是「货架空着，开店时才按档案柜现挂价签」；新方案是「关店装修时就把价签全部贴好，开门只切换买/卖两排货架的灯」。

---

## ①.2 范围冻结

| 项 | 约定 |
|----|------|
| **数据来源** | `MainItemDatabase.asset`（唯一） |
| **购买行** | `itemType == CostItem` 且 `buyPrice >= 0` |
| **出售行** | `itemType == MaterialItem` 且 `sellPrice >= 0` |
| **烘焙内容** | 每行：`Icon.sprite`、`Name.text`、`Price.text`、行根 `ShopBarRowView` 序列化 `itemId` / `price` / `isBuyRow` |
| **Scroll 壳** | Buy + Sell 同时生成；Viewport **无 Image**；`scrollSensitivity = 30`（沿用 `ShopScrollShellHelper`） |
| **Sell 默认** | `Bar_ListScroll_Sell` **SetActive(false)**；Buy 显示 |
| **本阶段不做** | Play 时动态增删行、背包过滤、Database 改完自动 Bake（可后续加 `OnValidate` 钩子） |
| **废弃** | 独立菜单「只清空 Content 等运行时刷」的逻辑；`ShopFormLogic` 运行时 Instantiate 路径 |

---

## ② 策划 / 美术工作流（施工后）

```
① 改 MainItemDatabase（类型、买价/卖价、拖 Icon、改 displayName、调 entries 顺序）
        ↓
② Unity 菜单：Tools → Shop → Bake Shop Lists From MainItemDatabase
        ↓
③ 不 Play，在 Hierarchy 展开：
   Bar_ListScroll_Buy/Viewport/Content   → 应见 Shop_Bar_HpBall 等购买行
   Bar_ListScroll_Sell/Viewport/Content → 应见 Shop_Bar_InsectBeak 等出售行
        ↓
④ 目检 Icon / 名称 / 单价正确 → Ctrl+S 保存场景（工具也会 MarkDirty + Save）
        ↓
⑤ Play 仅验证 Tab 切换、数量输入、合计（列表内容应与 Editor 一致，无「闪一下才出现」）
```

**改 Database 后忘记 Bake** → 场景仍是旧行；Console 可在 Bake 时打 Summary Log，**不做**运行时自动同步。

---

## ③ 目标 Hierarchy（Bake 完成后 · Editor 内应看到）

```
UI_Shop
└── Bar
    ├── Bar_BG
    ├── Bar_ListScroll_Buy          ← Active
    │   └── Viewport                ← 仅 RectMask2D，无 Image
    │       └── Content             ← VerticalLayoutGroup
    │           ├── Shop_Bar_HpBall
    │           ├── Shop_Bar_SmallHpPotion
    │           └── …（7 行，2026-07-05 数据）
    └── Bar_ListScroll_Sell         ← Inactive
        └── Viewport
            └── Content
                ├── Shop_Bar_InsectBeak
                ├── Shop_Bar_TenWangFruit
                └── Shop_Bar_SlimeCore   （3 行）
```

每行 `Shop_Bar_*` 在 **未 Play** 时 Inspector 应可见：

| 节点 | 已烘焙字段 |
|------|------------|
| `Icon` | Image.sprite = Database icon |
| `Name` | Text = displayName |
| `Price` | Text = buyPrice 或 sellPrice |
| 根 `ShopBarRowView` | serialized itemId、price、isBuyRow |

---

## ④ 一键菜单设计

### 4.1 菜单路径（定稿）

| 项 | 值 |
|----|-----|
| **菜单** | `Tools / Shop / Bake Shop Lists From MainItemDatabase` |
| **脚本（新建）** | `ShopListBakeEditor.cs`（`Editor/` 下） |
| **Batchmode** | `ShopListBakeEditor.ExecuteBatchBake()` |

### 4.2 执行流水线（EB-All · 单方法顺序）

```
1. 打开/确认 Village_Shop 场景，Find UI_Shop/Bar
2. EnsureScrollShell(Buy)  → 无则 CreateScrollView，有则校正
3. EnsureScrollShell(Sell) → 无则 Duplicate Buy，命名 Bar_ListScroll_Sell
4. ShopScrollShellHelper.ApplyInteractionFixes(Buy/Sell)
5. Load MainItemDatabase.asset
6. BakeContent(Buy.Content,  filter: CostItem & buyPrice>=0,  isBuyRow: true)
7. BakeContent(Sell.Content, filter: MaterialItem & sellPrice>=0, isBuyRow: false)
8. 绑定 ShopFormLogic：buyContent/sellContent/barListScrollBuy/barListScrollSell/btnSell
9. Sell SetActive(false)，Buy SetActive(true)
10. LayoutRebuilder.ForceRebuildLayoutImmediate(两侧 Content)
11. MarkSceneDirty + SaveOpenScenes
12. Dialog / Log：Buy=N Sell=M，未解析 Icon 的 itemId 列表
```

### 4.3 BakeContent 核心（Editor 专用 · 伪代码）

```csharp
static void BakeContent(Transform content, IEnumerable<MainItemDefEntry> entries, bool isBuyRow)
{
    ClearChildren(content);
    foreach (var entry in entries) // 顺序 = Database entries 数组顺序
    {
        var row = PrefabUtility.InstantiatePrefab(shopBarPrefab, content) as GameObject;
        row.name = $"Shop_Bar_{entry.itemId}";

        // ① 直接写 UI（Scene 里立刻可见，不依赖 Play）
        SetImageSprite(row, "Icon", ResolveIconEditor(entry));
        SetText(row, "Name", entry.displayName);
        SetText(row, "Price", (isBuyRow ? entry.buyPrice : entry.sellPrice).ToString());

        // ② 组件序列化（运行时只读，不再 Bind）
        var view = row.GetComponent<ShopBarRowView>() ?? row.AddComponent<ShopBarRowView>();
        view.EditorSetBakedData(entry.itemId, isBuyRow ? entry.buyPrice : entry.sellPrice, isBuyRow);

        if (isBuyRow)
            EnsureShopBuyRowQuantityInput(row);

        ResetRowRectForLayout(row);
    }
}
```

### 4.4 Editor Icon 解析（与运行时 Provider 对齐）

**优先级**（仅 Editor Bake 用，同步、不走 GameManager）：

1. `entry.icon`（Database 已拖 Sprite）  
2. `AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/ArtRes/UI/Item/Icon/{entry.itemId}.png")`  
3. 从 `MainItem_Icon.spriteatlas` 取 `GetSprite(entry.itemId.ToString())`  
4. null → Warning Log，该行 Icon 留空

**重要**：中文文件名 Icon（如「红烧鱼」）**必须**在 Database **icon 槽拖好**，否则 Bake 时第 2/3 步找不到。

---

## ⑤ 程序改造清单

### 5.1 新建 `ShopListBakeEditor.cs`

| 职责 | 说明 |
|------|------|
| 合并 | 吸收 `ShopBarListScrollSetupEditor` 的 Scroll 创建 + `ShopCatalogSetupEditor` 的双 Scroll |
| 新增 | `BakeContent` + `ResolveIconEditor` |
| 删除旧菜单入口 | 旧菜单改 `[Obsolete]` 并 **Dialog 跳转** 到新菜单，或内部 `forward` 调用 Bake |

### 5.2 改 `ShopBarRowView.cs`

| 改动 | 原因 |
|------|------|
| 增加 `[SerializeField] EMainItemName bakedItemId` | 运行时识别行，无需再 Bind |
| 增加 `[SerializeField] int bakedPrice`、`[SerializeField] bool bakedIsBuyRow` | 合计 / 交易读价 |
| 增加 `EditorSetBakedData(...)`（`#if UNITY_EDITOR`） | Bake 工具写序列化字段 |
| `Awake`：`ItemId = bakedItemId; Price = bakedPrice;` | **不**调 Provider |
| 保留 `Bind(MainItemDef)` | 仅供 Editor 工具或将来动态商店；**ShopFormLogic 不再调用** |

**替代方案**：不用序列化字段，纯靠 Editor 改 Text/Sprite — 运行时 `GetBuyQuantity` 无法匹配行。**不推荐**。

### 5.3 改 `ShopFormLogic.cs`（运行时瘦身）

| 删除 / 停用 | 保留 |
|-------------|------|
| `Awake/Start` 中 `RefreshBuyList()`、`RefreshSellList()` | `SwitchToBuyTab` / `SwitchToSellTab` |
| `OnDefinitionsRebuilt` → Refresh | `CollectBuyRowViews()`：遍历 `buyContent` 子节点 `GetComponent<ShopBarRowView>()` |
| 运行时 `InstantiateShopBarRow` | `GetBuyQuantity`、`GetCurrentHpBallBuyTotal`、`OnConfirmClick` |
| `shopBarPrefab` 运行时加载（可保留 SerializeField 仅 Editor 参考，或移除） | `ResolveShopReferences`、`ApplyScrollInteractionFixes`（Play 时壳修正可保留） |

**Awake 新流程**：

```
ResolveShopReferences()
EnsureDualScrollShell()          // 缺 Sell 时 Log 提示「请 Bake」，不自动 Instantiate 行
ApplyScrollInteractionFixes()
CollectBuyRowViews()             // 从场景已有行收集
CacheHpBallQuantityInput()
Wire 按钮 / 合计
// 无 RefreshBuyList / RefreshSellList
```

### 5.4 废弃旧菜单（施工时二选一）

| 旧菜单 | 处理 |
|--------|------|
| `Tools/Shop/Setup Bar List Scroll (SC-0~SC-4)` | 删除或 Obsolete → 调用 `ShopListBakeEditor` |
| `Tools/Shop/Setup Database Driven Lists` | 同上 |

**对外只宣传一个入口**：`Bake Shop Lists From MainItemDatabase`。

---

## ⑥ 施工阶段（EB-0 ～ EB-5）

| 阶段 | 内容 | 验证 |
|------|------|------|
| **EB-0** | 新建 `ShopListBakeEditor`，实现 Scroll 壳 Ensure + `ShopScrollShellHelper` | 跑菜单后 Hierarchy 有 Buy/Sell 双 Scroll |
| **EB-1** | 实现 `BakeContent` + `ResolveIconEditor` | **不 Play**，Content 下有行且 Name/Price 正确 |
| **EB-2** | `ShopBarRowView.EditorSetBakedData` + 运行时读 baked 字段 | Play 后 Tab/合计正常，**无 Instantiate Log** |
| **EB-3** | `ShopFormLogic` 删 Refresh，改 `CollectBuyRowViews` | Play 列表与 Editor 一致，无首帧闪空 |
| **EB-4** | 废弃旧 Tools、更新 Dialog 文案 | 仅一个 Bake 菜单 |
| **EB-5** | Database 6 道具补 icon；全量 Bake Village_Shop | Editor 目检 7+3 行 Icon |

---

## ⑦ 验收清单（Editor 优先 · 无需 Play）

| ID | 操作 | 期望 |
|----|------|------|
| EB-V1 | 空 Bar 场景跑 Bake | 生成 `Bar_ListScroll_Buy` + `_Sell` |
| EB-V2 | **不 Play**，展开 Buy Content | **7** 行，`Name/Price` 与 Database 一致 |
| EB-V3 | **不 Play**，展开 Sell Content | **3** 行，单价 **5** |
| EB-V4 | **不 Play**，点选 `Shop_Bar_HpBall/Icon` | Image.sprite **非 None**（Database 已绑 icon） |
| EB-V5 | 改 Database 某 buyPrice → 再 Bake | 对应行 Price **Scene 中立即变**，未 Play |
| EB-V6 | Play → 购买 Tab | 行数/文案与 EB-V2 **一致**，无延迟出现 |
| EB-V7 | Play → SELL Tab | 出售 Scroll 显示，**3** 行 |
| EB-V8 | Console | **无** `[ShopFormLogic] RefreshBuyList 跳过`；**无** Awake Instantiate 相关 Log |

---

## ⑧ 踩坑与约束

### 8.1 为何必须 Editor 写 Text/Sprite，不能只挂 Prefab

Prefab 实例若不在 Bake 时写入 **场景 override**，Editor 里看到的仍是 Prefab 默认值（如 HpBall 占位图），策划无法「关店验收」。

### 8.2 Bake 与 Prefab 的关系

- 行结构仍以 `Shop_Bar.prefab` 为模板 **InstantiatePrefab**  
- 数据是 **场景 override**，不是改 Prefab 本身  
- 改 Prefab 布局后 → **再 Bake** 重建行

### 8.3 过滤规则与行数

与货单瘦身文档一致；2026-07-05：**Buy=7（无 MpBall），Sell=3**。Database 变更后行数变 → 再 Bake。

### 8.4 Play 时仍调 `ApplyScrollInteractionFixes`

Scroll 壳参数（Sensitivity 等）允许 Play 时兜底；**列表内容**不允许 Play 时重建。

### 8.5 将来动态商店（非本任务）

若某 NPC 店货单 ≠ 全表过滤，再开 **「Bake to ShopProfile.asset + 按 Profile 烘焙」**；本任务只做 **KenMuNi 通用店 = Database 全表过滤**。

---

## ⑨ 文档关系

| 文档 | 关系 |
|------|------|
| `Shop_货单瘦身_MainItemDatabase驱动Shop_Bar刷新_…md` | **过滤规则沿用**；运行时 Refresh **作废**，以本文为准 |
| `Shop_货单瘦身…md` §⑪ | 滚轮/Viewport/Icon/Sell 壳问题 **并入 Bake 一步** |
| `Shop_Bar列表滚动_…md` | Scroll 尺寸/Layout 参数 **被 Bake 工具内联** |
| `MainItem_道具固有属性表_…md` v2 | Database 仍为唯一数据源 |

---

## ⑩ 修订记录

| 日期 | 说明 |
|------|------|
| 2026-07-05 | 首版：Editor 烘焙双列表一键工具；废弃运行时 Awake Refresh；EB-0～EB-5 + EB-V1～V8 |
