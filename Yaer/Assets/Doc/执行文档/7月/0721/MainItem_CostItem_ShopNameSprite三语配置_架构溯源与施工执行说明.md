# MainItem · CostItem 三语 `shopNameSprite` 配置 — 架构溯源与施工执行说明

**文档版本**：v1（2026-07-21）  
**文档性质**：【架构侦探】只读溯源 + **数据配置**施工指引（**本阶段只写文档；不改代码 / Prefab；施工员按 §⑥ 拖 Sprite**）  
**触发**：`MainItemDatabase` 里消耗品（`Cost Item`）的 **Shop Name Sprite / En / Jp** 仍为 `None`；美术已把中/英/日名图放到 `Assets/ArtRes/UI/ShopName/{Chinese|English|Japanese}/`，需把图挂进档案柜，商店货架才能按语言出店招图。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0715/Shop_Bar_Name改商店名图_MainItem加shopNameSprite_架构溯源与施工执行说明.md`（字段 / Resolve / Bake 已定稿）
- `Assets/Doc/执行文档/0704/MainItem_商店六道具ID补全_架构溯源与施工执行说明.md`（CostItem 枚举与购买行）
- 关联脚本：`MainItemDefEntry.cs` / `MainItemDefProvider.cs` / `MainItemDatabase.cs`
- 关联资源：`MainItemDatabase.asset`、`Assets/ArtRes/UI/ShopName/`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**代码侧三语槽与解析已就绪；本任务只需打开 `MainItemDatabase.asset`，给全部 8 个 CostItem（买价 ≥0）分别拖上中/英/日三张店招图。图在 `ArtRes/UI/ShopName` 的语言子目录里，文件名是中文美术名（不是 enum 名）。拖完后重 Bake 商店列表验收。**

**生活类比**：档案柜抽屉标签（字段）早就钉好了，贴纸（PNG）也印好了，只是还没往抽屉里贴。贴好之后，货架才会按顾客语言撕对应那一张。

---

## ② 玩家在做什么 / 会遇到什么现象

| # | 策划期望 | 当前工程（2026-07-21 快照） | 生活类比 |
|---|----------|------------------------------|----------|
| 1 | 商店购买列表每行是店招**图** | `shopNameSprite*` 全空；Inspector 显示 `None (Sprite)` | 价签板空着 |
| 2 | 中 / 英 / 日各一张 | 美术已分三语目录放齐；Database **未引用** | 贴纸印好了没贴到货架档案 |
| 3 | 切语言名图跟着换 | `ResolveShopNameSprite` 已写好，但槽空 → 回退链也空 → Name 空白 | 翻译员来了也没贴纸可撕 |
| 4 | 只动消耗品上架货 | CostItem 共 8 条且 `buyPrice>=0` 都会进购买页 | 只给「能买的药/鱼」贴店招 |

---

## ③ 架构溯源（只读）

### 3.1 代码已就绪（无需本任务再写逻辑）

| 层 | 现状 | 说明 |
|----|------|------|
| `MainItemDefEntry` | 已有 `shopNameSprite` / `En` / `Jp` | 插在 `icon` 与 `displayName` 之间 |
| `MainItemDefProvider.ResolveShopNameSprite` | 已按语言取槽 + 缺图回退（当前→英→中） | 日志前缀 `[ShopNameSprite]` |
| `MainItemDatabase.OnValidate` | 上架道具缺中文名图打 Warning | 口径：`CostItem && buyPrice>=0` 或 `MaterialItem && sellPrice>=0` |
| 商店 Bake / Row | 0715 施工后应读 Image，不再写 `displayName` 字 | 本任务只补数据；若 Prefab 仍是 TMP，属 0715 未合完，另案 |

**侦探结论**：本任务是 **配置闭环**，不是再开字段。

### 3.2 资源实际落点（与 0715 文档路径不一致）

| 项 | 0715 / Provider 兜底约定 | **工程现状（以磁盘为准）** |
|----|---------------------------|---------------------------|
| 根目录 | `Assets/ArtRes/UI/Item/ShopName/` | **`Assets/ArtRes/UI/ShopName/`**（无 `Item` 这一层） |
| 组织方式 | 同目录、`{itemId}.png` / `_en` / `_jp` | **三语子文件夹** + **中文文件名** 三份同名 |
| 子文件夹 | — | `Chinese/` · `English/` · `Japanese/` |
| Provider Editor 兜底 | `Item/ShopName/HpBall.png` 等 | **当前对不上**，兜底基本无效 |
| Import JSON 工具 | 同上路径自动 Load | **当前挂不上图** |

| 语言槽（Database） | 资源目录 |
|--------------------|----------|
| `shopNameSprite` | `Assets/ArtRes/UI/ShopName/Chinese/` |
| `shopNameSpriteEn` | `Assets/ArtRes/UI/ShopName/English/` |
| `shopNameSpriteJp` | `Assets/ArtRes/UI/ShopName/Japanese/` |

**复杂逻辑替代说明**：
- **推荐（本期）**：Inspector **手拖**（或按对照表批量改 `.asset` YAML）。不依赖文件名 = enum。
- **备选（下期施工员）**：改 `ShopNameFolderPath` + Import 路径，或加 Editor 菜单「按对照表一键挂 CostItem 名图」。本期文档不强制改代码。

### 3.3 Database 现状

路径：`Assets/GameRes/Config/MainItem/MainItemDatabase.asset`  
`entries` 共 **15** 条；其中 **CostItem（`itemType: 1`）且 `buyPrice >= 0`** 共 **8** 条。  
YAML 中尚无 `shopNameSprite*` 序列化字段 → 运行时三槽均为 **null**（与 Inspector `None` 一致）。

### 3.4 数据流（配置后）

```
ArtRes/UI/ShopName/{Chinese|English|Japanese}/某某.png
        ↓ 手拖（本任务）
MainItemDatabase.entry.shopNameSprite / En / Jp
        ↓
Provider.ResolveShopNameSprite(itemId, 当前语言)
        ↓
Shop Bake（预览贴中文槽） / ShopBarRowView.Bind（Play 按语言）
        ↓
Shop_Bar.Name → Image.sprite
```

---

## ④ 范围冻结

| 项 | 约定 |
|----|------|
| **要做** | 8 个 CostItem × 三语名图全部挂到 `MainItemDatabase` |
| **唯一维护处** | `MainItemDatabase.asset`（不要在 ShopCatalog / Prefab 再抄一套） |
| **资源只读** | 本期不改 PNG 文件名、不搬目录（除非策划另批「对齐 Item/ShopName」） |
| **不做** | 改 Resolve 逻辑、改 Prefab、改交易价、背包名改图 |
| **可选后续** | MaterialItem（虫喙 / 藤蔓果 / 史莱姆核）同目录已有图，出售列表也可同样拖三语（见 §5.2） |

---

## ⑤ 对照表（施工唯一依据）

> **注意**：`displayName`（印刷名）≠ 美术 PNG 名。例如印刷名「生命之珠」，图文件叫「生命球」。  
> **秘药 / 灵药**：按 Icon 惯例，**秘药 = HP（生命）**，**灵药 = MP（体力）**。若与美术意图不符 → §⑩ Q1。

### 5.1 CostItem（本任务必做 · 8 条）

| Element（约） | `itemId`（枚举） | `displayName` | 买价 | 中/英/日 **同文件名** | Chinese 路径示例 |
|---------------|------------------|---------------|------|----------------------|------------------|
| 2 | `HpBall` | 生命之珠 | 200 | **生命球.png** | `ShopName/Chinese/生命球.png` |
| 3 | `MpBall` | 体力之珠 | 200 | **体力球.png** | `ShopName/Chinese/体力球.png` |
| 9 | `SmallHpPotion` | 小生命药 | 20 | **小精灵秘药.png** | `…/Chinese/小精灵秘药.png` |
| 10 | `SmallMpPotion` | 小体力药 | 20 | **小精灵灵药.png** | `…/Chinese/小精灵灵药.png` |
| 11 | `LargeHpPotion` | 大生命药 | 50 | **大精灵秘药.png** | `…/Chinese/大精灵秘药.png` |
| 12 | `LargeMpPotion` | 大体力药 | 50 | **大精灵灵药.png** | `…/Chinese/大精灵灵药.png` |
| 13 | `BowlLiquid` | 碗装液体 | 500 | **迷之饮品.png** | `…/Chinese/迷之饮品.png` |
| 14 | `Fish` | 鱼 | 500 | **红烧鱼.png** | `…/Chinese/红烧鱼.png` |

每条 Entry 拖法：

| Database 槽 | 从哪个文件夹拖「同文件名」 |
|-------------|---------------------------|
| Shop Name Sprite | `ShopName/Chinese/` |
| Shop Name Sprite En | `ShopName/English/` |
| Shop Name Sprite Jp | `ShopName/Japanese/` |

### 5.2 MaterialItem（可选 · 出售列表）

同目录已有图，**本任务不强制**；若要做出售货架名图，可一并拖：

| `itemId` | `displayName` | 文件名 |
|----------|---------------|--------|
| `InsectBeak` | 虫喙 | 虫喙.png |
| `TenWangFruit` | 藤蔓果 | 藤蔓果.png |
| `SlimeCore` | 史莱姆核 | 史莱姆核.png |

### 5.3 资源 GUID 速查（改 YAML / 脚本挂载时用）

| 文件名 | Chinese guid | English guid | Japanese guid |
|--------|--------------|--------------|---------------|
| 生命球.png | `6aa385173f9f6c247b7d9b9c350fdadf` | `d376073e76d34b44abcbc9a8c8e6ab6e` | `7d1cd86f492bd5840a6ffc637f9d6226` |
| 体力球.png | `b26cefa7ab1b8944da9c55addad0a1dc` | `a0e329d5217a4b74ab59721dad5c81f8` | `eb69df8d27f7d414dbff0e9c96158988` |
| 小精灵秘药.png | `bac075385993eb34e9f8fccbcecbdfdf` | `636cf239153e97646adb243e9c0d4af4` | `50bf8fac2699b3045a4da646d7efc719` |
| 小精灵灵药.png | `4368e0d93c2a99946b950ecc01a0df56` | `0440d8163963cad4fa4c36da68e96aee` | `6005897fee517194395b365b171394f6` |
| 大精灵秘药.png | `b709736138a6b3f4b9acc609edb1ac92` | `f2ef837eb8e8a3543a0ed3ab31fe7c76` | `9c0dcc8bcac7e32418516a00fbdefd42` |
| 大精灵灵药.png | `1498efe245d9f5047afe56363095effc` | `8c31a71b939f8f2448fc6adb4c83a68b` | `774f859f8f132b84e89c45d2bd08945c` |
| 迷之饮品.png | `9294440abbd65b345ab569feeaf31c63` | `cb01e999e7372964cba6b55200ca5375` | `b605bf410112e3d47bf31a6c492129ff` |
| 红烧鱼.png | `bd78e7c46e01f5f4a814158e7785a7a6` | `231ed8e2c44ca954da34e8b87990b07f` | `1e2c23083a510aa4d8be02949bb904aa` |

YAML 引用格式（Sprite）：

```yaml
shopNameSprite: {fileID: 21300000, guid: <ChineseGuid>, type: 3}
shopNameSpriteEn: {fileID: 21300000, guid: <EnglishGuid>, type: 3}
shopNameSpriteJp: {fileID: 21300000, guid: <JapaneseGuid>, type: 3}
```

插在该条的 `icon:` 与 `displayName:` 之间（与 `MainItemDefEntry` 字段顺序一致）。

---

## ⑥ 施工步骤（施工员）

### 方案 A · Inspector 手拖（推荐，零代码）

```
① Project 定位：
   Assets/GameRes/Config/MainItem/MainItemDatabase.asset
        ↓
② 展开 Entries，找到 Item Type = Cost Item 且 Buy Price ≥ 0 的 8 条
        ↓
③ 按 §5.1：每条拖齐三个槽
   Shop Name Sprite     ← Chinese/同名.png
   Shop Name Sprite En  ← English/同名.png
   Shop Name Sprite Jp  ← Japanese/同名.png
        ↓
④ Ctrl+S 保存 Asset
        ↓
⑤（若商店已切 Image Name）Village_Shop
   → Tools → Shop → Bake … From MainItemDatabase
        ↓
⑥ 验收 §⑧
```

**为何不要跑「Import Database From JSON」**：Import 会清 entries 重灌，且仍读旧路径 `ArtRes/UI/Item/ShopName/{enum}.png`，**挂不上现有图，还有覆盖手填价格/文案风险**。

### 方案 B · 直接改 Asset YAML（熟练者）

按 §5.3 给 8 条 CostItem 写入三语 guid；保存后回 Unity 让它 Refresh。改错 guid 会导致粉紫丢失引用，优先方案 A。

### 方案 C · 日后 Editor 一键挂（本期不做）

菜单设想：`Tools/MainItem/Bind ShopName Sprites For CostItems`，内部用 §5.1 字典 `itemId → 中文文件名`，从三语目录 `LoadAssetAtPath`。适合道具再增多时；**本期手拖即可**。

---

## ⑦ 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 打开 Database → 任选 CostItem（如 Hp Ball） | 三语槽均有预览图，非 `None` |
| 2 | 对照 §5.1 扫 8 条 | 无漏拖；秘药/灵药未串（小生命↔秘药，小体力↔灵药） |
| 3 | Console 无持续 `[ShopNameSprite] 上架道具缺中文店招名图` | OnValidate 通过 |
| 4 | Bake 后 Buy 列表 | Name 为中文店招图（非 TMP 字） |
| 5 | Play：语言中/英/日进店（或重进店） | 名图随语言切换；**不是** `displayName` 字符串 |
| 6 | 故意清空某条英槽 | 应回退英链→中图；Console 可有回退 Warning |
| 7 | 背包同道具 | 仍用 Icon + 文字名（本任务未动） |

建议日志前缀：`[ShopNameSprite]`。

---

## ⑧ 提交说明模板（配置完成后填）

**改了哪些文件**：  
`Assets/GameRes/Config/MainItem/MainItemDatabase.asset`  
（若 Bake）`Assets/GameRes/Scenes/Village_Shop.unity`

**实现了什么**：8 个 CostItem 挂齐中/英/日商店名图，数据源仍是 MainItem 唯一档案。

**如何验证**：按 §⑦；正规进店 Init → 村 → Door_Shop；设置切换语言。

---

## ⑨ OPEN_QUESTIONS（未拍板 · 先记此处）

| ID | 问题 | 施工默认（文档建议） | 状态 |
|----|------|----------------------|------|
| Q1 | 「小/大精灵**秘药**」是否确定对应 HP、「**灵药**」对应 MP？ | **是**（对齐 Icon「精灵秘药 / 精灵灵药」）；若美术反了，只改对照表不改代码 | 待确认 |
| Q2 | 资源目录是否迁回 `ArtRes/UI/Item/ShopName/` + `{itemId}_en` 命名以启用 Provider 兜底？ | **本期不迁**；手拖即可。迁目录另开施工单并改 Provider/Import | 待确认 |
| Q3 | MaterialItem 三语名图是否同 PR 挂上？ | **否**，可选后续；出售页需要时再拖 §5.2 | 待确认 |
| Q4 | `displayName`（生命之珠）与 PNG 名（生命球）是否统一文案？ | **不强制**；商店读图，背包读字 | 待确认 |

有结论后可同步 `Assets/Doc/OPEN_QUESTIONS.md`。

---

## ⑩ 给程序看的补丁要点（极短）

1. **本期零代码**：只填 `MainItemDatabase` 三语 Sprite。  
2. **真路径**：`Assets/ArtRes/UI/ShopName/{Chinese|English|Japanese}/中文名.png`。  
3. **对照**：§5.1；秘药=HP，灵药=MP。  
4. **禁止**：为挂图重跑 JSON Import。  
5. **Provider 旧兜底路径失效**：依赖 Database 手拖，直到另案改路径。

---

**文档路径**：`Assets/Doc/执行文档/0721/MainItem_CostItem_ShopNameSprite三语配置_架构溯源与施工执行说明.md`

| 版本 | 日期 | 说明 |
|------|------|------|
| v1 | 2026-07-21 | 架构侦探：CostItem 三语名图配置对照表 + 手拖施工；不改代码 |
