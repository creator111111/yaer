# MainItem · 商店六道具 ID 补全 — 架构溯源与施工执行说明

**文档性质**：架构侦探产出（只读溯源 + 施工指引；**本阶段先写文档，代码待施工**）  
**调查日期**：2026-07-05  
**触发**：填 `ShopCatalogConfig.asset` 时 `Item Id` 下拉缺少 6 种货目，目前只能误选 `Mp Ball` 等占位  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_货单配置Asset与Shop_Bar数据刷新_架构溯源与施工执行说明.md`（货单 `itemId` 对齐 `EMainItemName`）
- `Assets/Doc/执行文档/0629/商店系统_策划拆解_执行说明.md` §3.6（道具 ID / 图标 / 主表）
- 关联 Asset：`Assets/GameRes/Config/Shop/ShopCatalogConfig.asset`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**在道具主数据链上一次性补全 6 个 ID：`EMainItemName` 枚举 + `MainItemConfig.json` 新行 + `ArtRes/UI/Item/Icon` 图标并纳入 `MainItem_Icon` 图集；`name` 字段与枚举名、PNG 文件名三者必须相同，补完后回到 `ShopCatalogConfig` 为每条 buy/sell 货目选对 `Item Id`。**

---

## ①.1 范围冻结（2026-07-05）

| 项 | 约定 |
|----|------|
| **本任务新增道具（6）** | 小生命药、小体力药、大生命药、大体力药、碗装液体、鱼 |
| **与已有道具关系** | **不合并** `HpBall`（生命之珠）/ `MpBall`（体力之珠）；新 ID 为独立货目 |
| **ID 命名语言** | 英文 PascalCase，与现有 `InsectBeak`、`HpBall` 一致 |
| **主表 id 编号** | 接续现有 **9** → 新道具用 **10～15** |
| **Fish / BowlLiquid** | **itemType = 1（消耗品）** | 与 0629 商店购买消耗品一致 |
| **购买页货单** | **7 行** | 含 **单独一行小生命药**（`SmallHpPotion`），见 §4.4 |
| **6 张 Icon** | **策划后续提供** | ID-0～ID-4 **可先施工**；PNG + 图集（ID-2）待资源到位后补，不阻塞 enum / 主表 / 货单改绑 |
| **本任务不做** | 药水使用效果、战斗回复数值、购买扣金币（商店交易逻辑另任务） |
| **本任务不做（续）** | Excel 导表流水线（可选手动改 JSON；有 Excel 时再跑 `MainItemConfigExcelTool`） |

---

## ② 玩家在做什么 / 会遇到什么现象

| 现象 | 原因（生活类比） |
|------|------------------|
| Inspector 里 `Item Id` 下拉没有「小体力药」 | 价目本上的商品**还没在系统字典里登记编号** |
| 只能选 `Mp Ball`，名字却写「大生命药」 | 编号与商品对不上，像用**同一个条形码**贴不同价签 |
| 填了 Asset 但 Play 后图标空白 | 字典有了，但**货架照片**（Icon PNG / 图集）还没入库 |
| 买了道具背包里名字不对 | 只改了商店 Asset，**主表** `MainItemConfig` 没同步 |

**生活类比**：道具 ID 是**国标商品码**；`MainItemConfig` 是**工商登记册**；`ShopCatalogConfig` 是**本店价目表**。价目表上的每一行必须先有国标码，登记册和照片库也要有同一条记录。

---

## ③ 工程现状快照（架构侦探 · 只读）

### 3.1 当前 `EMainItemName`（仅 9 项）

**路径**：`Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs`

```
AiLinSword, XiaerPower, HpBall, MpBall, Map,
GushaNacklace, InsectBeak, TenWangFruit, SlimeCore
```

**缺失**：小/大生命药、小/大体力药、碗装液体、鱼 → **6 项未登记**。

### 3.2 当前 `MainItemConfig.json`（id 1～9）

**路径**：`Assets/GameRes/Config/MainItemConfig/MainItemConfig.json`

| id | name | cnName | itemType |
|----|------|--------|----------|
| 3 | HpBall | 生命之珠 | 1（消耗品） |
| 4 | MpBall | 体力之珠 | 1 |
| … | … | … | … |
| 9 | SlimeCore | 史莱姆核 | 2（素材） |

**itemType 约定**（`PlayerBagData` / 0629）：`0` 任务、`1` 消耗品、`2` 素材。

### 3.3 图标与图集（当前无新道具资源）

| 资源 | 现状 |
|------|------|
| `Assets/ArtRes/UI/Item/Icon/*.png` | 仅有既有 9 道具；**无** 6 个新 PNG |
| `MainItem_Icon.spriteatlas` | 打包名列表仅 9 项（HpBall、MpBall…） |
| `ShopMainItemIconResolver` | 按 `itemId.ToString() + ".png"` 兜底加载 |
| `ShopMainItemDisplayCache` | 兜底 cnName 字典**未含** 6 新道具 |

### 3.4 `ShopCatalogConfig` 填表现状（用户截图）

购买页目标 **7 行**（含 **单独一行小生命药**）；当前已填约 8 条，其中多条 `Item Id` 暂用 **`Mp Ball` 占位**：

| 购买页顺序（定稿） | displayName | 当前误用 Item Id | 应有 Item Id |
|--------------------|-------------|------------------|--------------|
| 1 | 生命之珠 / 生命球 | HpBall | `HpBall`（已有） |
| 2 | 小生命药 | （缺行或占位） | **`SmallHpPotion`** |
| 3 | 小体力药 | MpBall | **`SmallMpPotion`** |
| 4 | 大生命药 | MpBall | **`LargeHpPotion`** |
| 5 | 大体力药 | MpBall | **`LargeMpPotion`** |
| 6 | 碗装液体 | MpBall | **`BowlLiquid`** |
| 7 | 鱼 | MpBall | **`Fish`** |

> **小生命药**：购买列表 **必须有独立一行**，`itemId = SmallHpPotion`；与 `HpBall`（生命之珠）为不同货目。

### 3.5 数据链依赖（改一处不够）

```
EMainItemName          ← ShopCatalogConfig.itemId 下拉来源
       ↕ name 字符串一致
MainItemConfig.json    ← PlayerBagData / 背包 / 菜单中文名
       ↕ 同名
Icon/{Name}.png        ← ShopMainItemIconResolver、MainItem_Icon 图集
       ↕
ShopCatalogConfig      ← 价格仍只在此配置；displayName 可空（回退 cnName）
```

**硬规则**：`enum 名 == MainItemConfig.name == Icon 文件名（无扩展名）== 图集内 Sprite 名`。

---

## ④ 新增道具定稿表（施工必查）

### 4.1 推荐 ID 对照（2026-07-05 架构侦探建议）

| # | 中文名（策划） | **EMainItemName / name** | MainItemConfig.id | cnName | itemType | 说明 |
|---|----------------|--------------------------|-------------------|--------|----------|------|
| 1 | 小生命药 | **SmallHpPotion** | 10 | 小生命药 | **1** | 消耗品；小档 HP 药 |
| 2 | 小体力药 | **SmallMpPotion** | 11 | 小体力药 | **1** | 消耗品；小档 MP/体力药 |
| 3 | 大生命药 | **LargeHpPotion** | 12 | 大生命药 | **1** | 消耗品；大档 HP 药 |
| 4 | 大体力药 | **LargeMpPotion** | 13 | 大体力药 | **1** | 消耗品；大档 MP/体力药 |
| 5 | 碗装液体 | **BowlLiquid** | 14 | 碗装液体 | **1** | ✅ 消耗品（策划定稿） |
| 6 | 鱼 | **Fish** | 15 | 鱼 | **1** | ✅ 消耗品（策划定稿） |

**与旧道具区分**：

| 已有 | 新建 | 关系 |
|------|------|------|
| HpBall · 生命之珠 | Small/Large **HpPotion** | 同系不同档位，**不同 ID** |
| MpBall · 体力之珠 | Small/Large **MpPotion** | 同上 |

### 4.2 命名替代方案（二选一，施工前锁定）

| 方案 | 示例 | 优点 | 缺点 |
|------|------|------|------|
| **A · Hp/Mp + Potion（推荐）** | `SmallHpPotion` | 与 `HpBall`/`MpBall` 体系一致；见名知 HP/MP | 「药」与「珠」用词不同 |
| B · Life/Stamina + Potion | `SmallLifePotion` | 中文「生命/体力」直译 | 与现有 `Hp`/`Mp` 命名分裂 |

**本任务选用方案 A**；若策划坚持「药=Ball」，可改为 `SmallHpBall`，但需与「生命之珠」语义区分并在 cnName 写清。

### 4.3 JSON 单条模板（复制改 id/name/cnName）

```json
{
  "id": "10",
  "name": "SmallHpPotion",
  "cnName": "小生命药",
  "detail": "    （待策划补全文案）",
  "detail_en": "（待补）",
  "detail_jp": "（待补）",
  "itemType": 1
}
```

其余 5 条：`id` 11～15，`name` / `cnName` 按 §4.1 替换；**Fish / BowlLiquid 的 itemType 均为 1**；**不要**改动 id 1～9 已有行。

### 4.4 购买页 `buyItems` 定稿（7 行 · 2026-07-05）

补全 ID 后，`ShopCatalogConfig.buyItems` **Size = 7**，顺序与 `itemId` 如下（`displayName` 可留空，回退 cnName；**price 策划在 Inspector 自填**）：

| # | itemId | cnName（主表） | price 备注 |
|---|--------|----------------|------------|
| 1 | HpBall | 生命之珠 | 已有 **200** |
| 2 | **SmallHpPotion** | 小生命药 | **待填** |
| 3 | SmallMpPotion | 小体力药 | 已有 **20** |
| 4 | LargeHpPotion | 大生命药 | 已有 **50** |
| 5 | LargeMpPotion | 大体力药 | 已有 **50** |
| 6 | BowlLiquid | 碗装液体 | 已有 **500** |
| 7 | Fish | 鱼 | 已有 **500** |

若当前 Asset 为 8 条且缺「小生命药」行：补 ID 后 **增 1 行 SmallHpPotion**，并修正其余行的 `itemId`；总行数应为 **7**（去掉重复或错误占位行）。

---

## ⑤ 分阶段施工

### 阶段 ID-0 · 枚举补全

**文件**：`Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs`

```csharp
// 在 SlimeCore 后追加（保持逗号风格与文件一致）
SmallHpPotion,
SmallMpPotion,
LargeHpPotion,
LargeMpPotion,
BowlLiquid,
Fish,
```

**注意**：枚举**只追加在末尾**，勿插入中间，避免已有存档 / 序列化 ordinal 错位（若项目有按 int 存 enum 的存档需排查；当前背包用 `ToString()` 键，**追加末尾安全**）。

**验证 ID-0**：Unity 编译通过；`ShopCatalogConfig` Inspector 的 `Item Id` 下拉出现 6 个新项。

### 阶段 ID-1 · MainItemConfig.json 补 6 行

**文件**：`Assets/GameRes/Config/MainItemConfig/MainItemConfig.json`

| 做 | 不做 |
|----|------|
| 追加 id **10～15** 共 6 对象 | 不改已有 1～9 |
| `name` 与 enum **完全一致** | detail 可先写占位「待策划补」 |

**可选**：若使用 Excel 源表 → 改 `Assets/ExcelConfig/MainItemConfig/` 后跑菜单 `Editor/ExcelTool/MainItemConfig/GenerateJsonFile`。

**验证 ID-1**：JSON 合法；`PlayerBagData` 将来 `AddMainItem(EMainItemName.SmallHpPotion)` 能 `GetItemRow` 拿到 cnName。

### 阶段 ID-2 · 图标资源 + 图集（**可滞后 · 策划后续提供 6 张 Icon**）

| 步骤 | 说明 |
|------|------|
| 1 | 策划提供 6 张 PNG → `Assets/ArtRes/UI/Item/Icon/{Name}.png` |
| 2 | 文件名 **必须** = enum 名（如 `SmallHpPotion.png`） |
| 3 | 打开 `MainItem_Icon.spriteatlas` → 将 6 PNG **加入 Packables** |
| 4 | （可选）`MainItem_Detail.spriteatlas` 同步加 Detail 图 |

**施工顺序**：**ID-0 / ID-1 / ID-3 / ID-4 不依赖 Icon**，可先完成 enum、主表、货单改绑；ID-2 在 Icon 到位后执行。

**Icon 未到位时**：商店行 `icon` 留空 → Play 可能无图或 Console 提示缺 PNG；**Name / Price / itemId 仍应验收通过**。勿用 HpBall/MpBall 占位图代替正式资源提交。

**验证 ID-2**（Icon 到位后）：图集 Packed Sprites 含 6 新名；Play 商店 `icon` 留空时 `ShopMainItemIconResolver` 能加载 PNG。

### 阶段 ID-3 · 商店兜底表同步

**文件**：`ShopMainItemDisplayCache.cs`

在 `FallbackCnNames` 字典追加 6 项（与 cnName 一致），供 Village_Shop 无 GameManager 时显示中文名：

```csharp
{ EMainItemName.SmallHpPotion, "小生命药" },
{ EMainItemName.SmallMpPotion, "小体力药" },
{ EMainItemName.LargeHpPotion, "大生命药" },
{ EMainItemName.LargeMpPotion, "大体力药" },
{ EMainItemName.BowlLiquid, "碗装液体" },
{ EMainItemName.Fish, "鱼" },
```

**验证 ID-3**：`ShopCatalogConfig` 条目 `displayName` 留空 → Play 后 Name 列显示上表中文。

### 阶段 ID-4 · 回改 ShopCatalogConfig.asset

**文件**：`Assets/GameRes/Config/Shop/ShopCatalogConfig.asset`

| 做 | 说明 |
|----|------|
| 每条 buy/sell 的 **`itemId` 改选对** | 不再用 MpBall 占位 |
| **`displayName` 可留空** | 回退 cnName；已填且与 cnName 相同可保留或清空 |
| **`icon` 可留空** | 走 ID-2 图集兜底；或在 Inspector 手动拖 Sprite |
| **`price` 保持你已填数值** | 价格仍只在此 Asset，不进 MainItemConfig |

**购买页按 §4.4 对齐（7 行）**：

| # | itemId | price |
|---|--------|-------|
| 1 | HpBall | 200 |
| 2 | **SmallHpPotion** | 待填 |
| 3 | SmallMpPotion | 20 |
| 4 | LargeHpPotion | 50 |
| 5 | LargeMpPotion | 50 |
| 6 | BowlLiquid | 500 |
| 7 | Fish | 500 |

**操作要点**：若无第 2 行则 **新增**；修正原 MpBall 占位；`buyItems.Count == 7`。

**验证 ID-4**：Play Village_Shop → 购买列表 **7 行**；每行 Name/Price 与 Asset 一致；itemId 无重复 MpBall 占位。（Icon 待 ID-2 资源到位后再验）

### 阶段 ID-5 · 关联排查（可选但建议）

| 检查点 | 说明 |
|--------|------|
| `AA_TestPanel` 加道具列表 | 若用 `Enum.GetValues(EMainItemName)` 会自动含新 ID |
| 存档兼容 | 背包 key 为 string name → 新道具无旧存档问题 |
| `ItemEffectDataMgr` | 若需「使用药水」，**另任务**按 id 注册效果；本任务不阻塞商店展示 |

---

## ⑥ 技术要点与踩坑

### 6.1 三处 name 不一致的典型症状

| 症状 | 常见原因 |
|------|----------|
| 下拉有 ID，Play 名称为英文枚举 | MainItemConfig 缺行或 `name` 拼写差一个字母 |
| 图标不显示 | PNG 文件名与 enum 不一致；或图集未 Rebuild |
| 背包添加失败 / cnName 空 | JSON 未加载或 `GetItemRow` 找不到 name |

### 6.2 为何不能只在 ShopCatalogConfig 填 displayName

displayName 只解决**商店一行上的字**；`itemId` 仍负责：

- 购买 / 出售扣背包时 `TryRemoveMainItem(itemId)`
- 菜单 / 存档 / Debug 加道具
- 图标与主表文案统一

**因此必须补 enum + MainItemConfig，不能只改 Asset 上的中文名。**

### 6.3 Fish / BowlLiquid 的 itemType

**已定稿**：二者均为 **`itemType = 1`（消耗品）**，写入 MainItemConfig id 14 / 15。出现在 **buyItems** 购买列表，与 0629 商店消耗品约定一致。

### 6.4 枚举追加顺序

本项目背包用 **`itemName.ToString()`** 作字典键，**追加末尾 enum 值不影响已有存档**。若别处有序列化 enum 整型值，施工后需全局搜 `EMainItemName` 回归。

---

## ⑦ 验收清单

| # | 操作 | 期望 |
|---|------|------|
| ID-V1 | 打开 `ShopCatalogConfig` → Item Id 下拉 | 可见 **SmallHpPotion … Fish** 共 6 项 |
| ID-V2 | 检查 `MainItemConfig.json` | id 10～15 存在且 name 与 enum 一致 |
| ID-V3 | Icon 到位后检查 `ArtRes/UI/Item/Icon/` | 6 个同名 PNG；图集已打包（**可滞后**） |
| ID-V4 | `buyItems` **7 行**；6 个新 itemId 各不同、无 MpBall 占位 | 含 **SmallHpPotion** 独立一行 |
| ID-V5 | Play Village_Shop 购买页 | **7 行**中文名 + 价格正确（Icon 可待 ID-2） |
| ID-V6 | Console | 无 `GetItemRow` / 图标 Missing 报错 |
| ID-V7 | （可选）`AddMainItem(SmallHpPotion)` 测试 | 背包可显示「小生命药」 |

---

## ⑧ 待确认问题

| ID | 问题 | 影响 | 状态 |
|----|------|------|------|
| ~~Q1~~ | 6 道具英文 ID 命名 | — | ✅ §4.1（SmallHpPotion … Fish） |
| ~~Q2~~ | `Fish` / `BowlLiquid` 的 itemType | — | ✅ **均为 1（消耗品）** |
| ~~Q3~~ | 购买列表是否含小生命药 | — | ✅ **单独一行** `SmallHpPotion`；buyItems **7 行**（§4.4） |
| Q4 | 小生命药 **买价** | ShopCatalog | 📝 Inspector 自填；其余价格见 §4.4 |
| Q5 | detail / detail_en / detail_jp 全文案 | JSON | 📝 可先占位；菜单详情页再补 |
| ~~Q6~~ | 6 张 Icon 美术 | ID-2 | ✅ **策划后续提供**；ID-0～1/3～4 不阻塞 |

> 仍无结论的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

## ⑨ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| 道具枚举（**ID-0 改此**） | `Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs` |
| 道具主表（**ID-1 改此**） | `Assets/GameRes/Config/MainItemConfig/MainItemConfig.json` |
| Excel 导 JSON | `Assets/Editor/Tool/ExcelTool/MainItemConfigExcelTool.cs` |
| 列表图标图集（**ID-2 改此**） | `Assets/GameRes/Atlas/MainItem_Icon.spriteatlas` |
| Icon 目录 | `Assets/ArtRes/UI/Item/Icon/` |
| 商店货单 Asset（**ID-4 改此**） | `Assets/GameRes/Config/Shop/ShopCatalogConfig.asset` |
| 商店 cnName 兜底（**ID-3 改此**） | `ShopMainItemDisplayCache.cs` |
| 图标解析 | `ShopMainItemIconResolver.cs` |
| 背包 Add/Remove | `PlayerBagData.cs` |
| 货单驱动总文档 | `0704/Shop_货单配置Asset与Shop_Bar数据刷新_*` |

---

## ⑩ 文档变更记录

| 日期 | 说明 |
|------|------|
| 2026-07-05 | 初稿：填 ShopCatalogConfig 发现 ItemId 缺 6 项；定稿 enum/MainItemConfig/Icon 三链同步 + ID-0～ID-5 施工与验收 |
| 2026-07-05 | 策划对齐：Fish/BowlLiquid=itemType 1；购买页 7 行含 SmallHpPotion；Icon 后续提供、ID-2 可滞后 |
