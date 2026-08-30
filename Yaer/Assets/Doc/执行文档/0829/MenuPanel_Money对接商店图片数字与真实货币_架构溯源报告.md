# MenuPanel·Money — 对接商店图片数字逻辑与真实货币 — 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码 / Prefab / 场景**）  
**Unity**：2020.3.48f1  
**目标**：`MenuPanel` · `ButtonMoney` / `Money` / `Money (1)` → 显示 `PlayerGoldData.gold`

关联提示词：`Assets/Doc/提示词/0829/MenuPanel_Money对接商店图片数字与真实货币_架构侦探提示词.md`  
商店数字真源：`UiSpriteNumberDisplay.cs` · `0706/Shop_Bar数字图片化_…`  
货币门面：`QuestManager.GetPlayerGoldData` / `PlayerGoldData.gold`

---

## ① 结论一句话

**商店价签/合计已统一走 `UiSpriteNumberDisplay`（`ArtRes/UI/Text/0～9`、自然位数、`SetNumber`）；Menu 的 Money 现网只是两张裸 Image（`Money`=静态 `0.png` 占位、`Money (1)`=另一图标），无 DigitStrip、无读金。推荐方案 A：在 `ButtonMoney` 下挂与 Total2 同款的 `DigitStrip`+`UiSpriteNumberDisplay`，`MenuFormLogic.OnOpen` 调 `QuestManager.GetPlayerGoldData().gold` → `SetNumber`；禁止日历双位组件、禁止 TMP 充数。**

---

## ② 原因（通俗）

商店已经有一盒「0～9 木牌数字贴片」；菜单 Money 现在只贴了一张死的「0」图，不算会变的数字条。  
日历那天数是「十位+个位」固定两格，撑不住几百几千金币——必须复用商店那套可变位数组件，再接钱包存档。

---

## ③ 用户检查清单

| # | 操作 | 通过（施工后） |
|---|------|----------------|
| 1 | 存档 gold=0 → ESC 开菜单 | Money 区图片显示 **`0`**（无前导零） |
| 2 | gold=123 / 99999（容量内） | 显示 **`123` / `99999`**，素材风格与商店价签一致 |
| 3 | 商店买完扣款 → 回村再 ESC | 菜单金币 = 扣后余额 |
| 4 | 外观 | 图片数字，非系统字；可选保留硬币图标 |
| 5 | Console | 无 Digit 池 / 栈溢出旧坑；无 NRE |

---

## ④ 给程序

---

### §1 商店数字逻辑说明书（第一步 · 必读）

#### 1.1 组件与素材

| 项 | 内容 |
|----|------|
| 类 | `Game.GameRuntime.UI.Component.UiSpriteNumberDisplay` |
| 路径 | `Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs` |
| 子节点名 | **`DigitStrip`**（`DigitStripNodeName`）；位图子节点 `Digit_0`… |
| 素材 | `Assets/ArtRes/UI/Text/0.png`～`9.png`（`DigitSpriteFolderPath`） |
| 对外 API | `SetNumber(int)`（自然位数，`ToString()`，**禁止 PadLeft**）；`SetDigitString`；`EnsureOn` / `FindUnder`；Total2：`ApplyShopTotalLayout` |
| 布局 | `HorizontalLayoutGroup` + 序列化 `spacing` / `digitAlignment` / `poolCapacity` / `fitWithinParentWidth` |

#### 1.2 商店定稿常量

| 常量 | 值 | 用途 |
|------|-----|------|
| `ShopPriceSpacing` | **-12** | 行 Price |
| `ShopNumberSpacing` | **-12** | 行 Number |
| `ShopTotalSpacing` | **-12** | Total2 Bake 初值 |
| `ShopTotalPoolCapacity` | **6** | Total2 最多 6 位 |
| `ShopTotalFitPadding` | **4** | Total2 窄框 fit |

Price/Number 默认 `poolCapacity=5`；Total2 运行时 `ApplyShopTotalLayout` → capacity=6 + fit。

#### 1.3 谁调用、何时刷

| 槽位 | 谁 | 何时 | 调用 |
|------|----|------|------|
| **Price** | Bake `EnsureOn` + `ShopBarRowView` | 刷行数据 | `SetNumber(price)` |
| **Number** | `ShopQuantityInputHelper` / 行输入 | 键入/默认数量 | `SetNumber` / `SetDigitString` |
| **Total2** | `ShopFormLogic.RefreshTotal2` → `SetTotal2Number` | Tab 切换、数量变、买完清零 | `total2Digits.SetNumber(total)` |

解析链：`Total2/Total2_Digits` → `FindUnder(Total2)` → `EnsureOn(..., ShopTotalSpacing, capacity=6)`；无图则回退旧 Text（兼容，**新 UI 勿走回退**）。

#### 1.4 禁止事项（商店已踩坑）

| 禁止 | 原因 |
|------|------|
| `PadLeft` / 固定 D4 | 会出现 `0200` 假前导零（0706） |
| Update 轮询刷数字 | 架构禁止堆业务 |
| `HideAllDigits` ↔ `EnsureInitialized` 互调 | 栈溢出闪退（0706 CRASH-1） |
| 重复 `Digit_*` 残留不剪 | 假前导零；靠 `PruneDuplicateDigitChildren` |

#### 1.5 与 `MenuCalendarDayNumDisplay` 差异

| | `UiSpriteNumberDisplay` | `MenuCalendarDayNumDisplay` |
|--|-------------------------|----------------------------|
| 位数 | **可变** 1～N（池上限） | **固定** 十位+个位 |
| 用途 | 价/量/合计/（拟）金币 | 日历日 1～31 |
| 布局 | HLG 横排池 | 两个 Image 引用 |
| 菜单金币 | ✅ 应用 | ❌ 不够用 |

最小伪代码：

```csharp
display.SetNumber(priceOrTotalOrGold); // 自然位数 → Digit_* Image
```

---

### §2 MenuPanel Money 现状（第二步）

路径（Prefab）：

```
MenuPanel
  └── … → ButtonMoney          // SizeDelta ≈ (208, 50)
        ├── Money              // 28×28，裸 Image，Sprite = **0.png**（guid 0101366b…）
        └── Money (1)          // 28×28，裸 Image，Sprite = **Z.png**（guid d6adcc02…，同目录字母图，非 DigitStrip）
```

| 项 | 磁盘结论 |
|----|----------|
| `UiSpriteNumberDisplay` / `DigitStrip` | ❌ **无** |
| `Money` 的 `0.png` | **静态占位一位「0」**，≠ 已接可变数字条 |
| `Money (1)` 的 `Z.png` | 字母占位/装饰；**不是**商店 Digit 逻辑 |
| `MenuCalendarDayNumDisplay` | 在 **`DayNum`**，**不在** Money |
| `MenuFormLogic` | `OnOpen` 只 `dayNumDisplay.RefreshFromArchive()`；**无 Gold/Money** |
| 底框 | `ButtonMoney` ≈ **208×50** → 够放图标 + 最多 6 位（对齐 Total2） |

---

### §3 对接方案拍板（第三步）

| 方案 | 裁定 |
|------|------|
| **A · Money 区挂 `UiSpriteNumberDisplay`** | **✅ 推荐** |
| B · 日历双位 | ❌ |
| C · 新写 MenuMoneyDisplay | ❌ 无必要 |
| D · 静态图不读档 | ❌ |

**A 细则**

1. **挂点（推荐）**  
   - 在 `ButtonMoney` 下新建 **`Money_Digits`**（或扩宽 `Money`）挂 `DigitStrip` + `UiSpriteNumberDisplay`。  
   - 现网 `Money`（死 `0.png`）与 `Money (1)`（`Z.png`）**施工时须二选一**：隐藏/删除占位，或换成真正币标后再并排数字条——**禁止**留着静态 `0.png` 与 DigitStrip 叠出双「0」。  
   - 若美术另有硬币 Icon：可替换 `Money (1)` 的 Sprite，数字走 DigitStrip。  

2. **布局参数**  
   - spacing：复用 **`ShopTotalSpacing` (-12)**（可加注释别名「MenuMoney」；不必强行新常量）。  
   - `poolCapacity`：**6**（与 Total2）。  
   - alignment：`MiddleRight` 或 `MiddleCenter`（相对币标排版手调）。  
   - `fitWithinParentWidth`：若数字容器偏窄则 **true**（学 Total2）。  

3. **读金**  
   ```csharp
   var goldData = QuestManager.Instance 或 现网取 QuestManager 的方式
       .GetPlayerGoldData();
   int gold = goldData != null ? goldData.gold : 0; // null → 显示 0，不隐藏、不 NRE
   moneyDigits.SetNumber(gold);
   ```  
   - 门面与商店购买一致：`GetPlayerGoldData()`（场景 Archive 优先，无 GSM 则 `ArchiveComponentGM.GetData`）。  
   - **本期只读**；不调 `TrySpendPlayerGold`。  

4. **刷新挂点**  
   - **P0**：`MenuFormLogic.OnOpen`（紧挨日历 `RefreshFromArchive`）→ `RefreshMoneyFromArchive()`。  
   - 可选：`OnReveal` 再刷一次（从设置返回菜单时）。  
   - Village_Shop ESC 已改口离店 → 店内通常不开菜单；**OnOpen 足够**看到买完回村后的余额。  

5. **中枢**  
   - 默认 **不改** `UiSpriteNumberDisplay` 源码；仅 Prefab 挂载 + MenuFormLogic 几行刷新。  

---

### §4 最小施工清单（本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | `MenuPanel.prefab` | Money 区挂 DigitStrip + `UiSpriteNumberDisplay`，绑 0～9；调 Rect；处理与 `0.png`/币标叠层 | **P0** |
| 2 | `MenuFormLogic.cs` | OnOpen（可选 OnReveal）读 gold → `SetNumber`；序列化或 `FindUnder` 引用 | **P0** |
| 3 | 验收 | gold=0/123/99999；买完再开菜单 | **P0** |
| 4 | spacing/capacity 手调 | | P1 |
| 5 | 与商店购买/货币检测文档交叉备注 | | P2 |

**排除**：菜单扣款；重做商店 Bake；日历组件硬撑多位；独立 `displayGold` 字段。

**预期 diff**

- `Assets/GameRes/Prefabs/UI/MenuPanel.prefab`  
- `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/MenuFormLogic.cs`  

---

### §5 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 本报告含完整「商店数字逻辑」§1 | ✅ |
| 2 | 菜单图片数字 = 存档 gold，无前导零 | |
| 3 | 与商店同套 0～9 | |
| 4 | goldData null → 显示 0，无 NRE | |
| 5 | 无 Digit 栈溢出复现 | |

---

### §6 开放问题

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | `Money` / `Money (1)` 如何处理？ | **新建 DigitStrip 显示金**；隐藏或替换静态 `0.png`/`Z.png` 占位；币标另补可选 | 待确认 |
| Q2 | 最大位数？ | **6**（对齐 Total2） | 待确认 |
| Q3 | 店内 ESC 已离店，刷新是否只靠 OnOpen？ | **是**；可选 OnReveal | ✅ 建议 |
| Q4 | 是否新建 `MenuMoneySpacing` 常量？ | **否**，复用 `ShopTotalSpacing` | 待确认 |

（已追加 `OPEN_QUESTIONS.md`。）

---

## 附录 · 关键锚点

| 主题 | 路径 |
|------|------|
| 数字组件 | `UiSpriteNumberDisplay.cs` |
| 商店合计 | `ShopFormLogic.ResolveTotal2DigitsReference` / `SetTotal2Number` |
| 行价 | `ShopBarRowView` Price `SetNumber` |
| 菜单 | `MenuPanel.prefab` · `MenuFormLogic.OnOpen` |
| 金币 | `PlayerGoldData.gold` · `QuestManager.GetPlayerGoldData` |
| 0706 文档 | `执行文档/7月/0706/Shop_Bar数字图片化_…` |
