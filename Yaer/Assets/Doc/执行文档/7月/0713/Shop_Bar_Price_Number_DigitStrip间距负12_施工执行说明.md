# Shop · Price/Number DigitStrip 间距统一 -12 — 架构溯源与施工执行说明

**文档版本**：v1（2026-07-13）  
**文档性质**：架构定稿 + 已施工记录  
**触发**：策划目测 `Shop_Bar` 内 **Price / Number** 的 `DigitStrip` 字距仍偏散；确认 **`spacing = -12`** 观感合适，并要求 **每次 Tools Bake 重刷列表** 后仍保持 `-12`，不被旧常量盖回。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【施工员】
- `Assets/Doc/执行文档/0706/Shop_Bar数字图片化_Price_Number_Total_架构溯源与施工执行说明.md`（间距 v3：Number=-1、Price=0；本任务 **覆盖** 该定稿）
- 关联：`UiSpriteNumberDisplay.cs`、`ShopListBakeEditor.cs`、`ShopQuantityInputHelper.cs`、`Shop_Bar.prefab`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**把商店行 Price / Number 的官方字距常量都改成 `-12`，并同步 `Shop_Bar` 预制体；Bake / 数量输入 Ensure 路径本来就读这两个常量，之后每次 Tools 刷新都会写出 `-12`，无需再改 Bake 逻辑。**

**生活类比**：以前价签和拨盘数字「字距模具」分别是 0 和 -1；现在统一换成 -12 这把更紧的模具，工厂（Bake）每次出货都用同一把尺。

---

## ② 为何只改常量就能保证 Tools 刷新

Bake / Ensure 已强制调用：

| 调用点 | 使用的常量 |
|--------|------------|
| `ShopListBakeEditor` → Price DigitStrip | `UiSpriteNumberDisplay.ShopPriceSpacing` |
| `ShopListBakeEditor` → Number DigitStrip | `UiSpriteNumberDisplay.ShopNumberSpacing` |
| `ShopQuantityInputHelper` → Number DigitStrip | `ShopNumberSpacing` |

因此：

- **只改 Prefab Inspector** → 下次 Bake 会被常量盖回（旧坑）。
- **改常量** → 每次 `Tools → Shop → Bake Shop Lists From MainItemDatabase` 自动 `SetSpacing(-12)`。

`ShopTotalSpacing` 已是 `-12`，本任务 **不改 Total2**（已对齐观感）。

---

## ③ 范围冻结

| 项 | 约定 |
|----|------|
| **Price spacing** | `0` → **`-12`** |
| **Number spacing** | `-1` → **`-12`** |
| **Total2 spacing** | 保持 `ShopTotalSpacing = -12`（不动） |
| **Bake 代码结构** | 不改调用链，只吃新常量 |
| **不做** | 改数字图素材、压扁 Digit 尺寸、改 poolCapacity |

---

## ④ 改动清单

| 文件 | 改什么 |
|------|--------|
| `Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs` | `ShopPriceSpacing = -12f`；`ShopNumberSpacing = -12f`；注释同步 |
| `Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab` | `Price/DigitStrip` 与 `Number/DigitStrip` 的 `spacing` + HLG `m_Spacing` → `-12` |
| 本执行文档 | 定稿与验收 |

**替代方案（未采用）**：Bake 里写死 `-12` 魔法数——与 Total / Ensure 多处重复，易再次漂移；常量单点源更干净。

---

## ⑤ 施工后工作流

```
① 代码常量已为 -12（本任务）
        ↓
②（可选）打开 Shop_Bar 目检 DigitStrip Spacing = -12
        ↓
③ Tools → Shop → Bake Shop Lists From MainItemDatabase
        ↓
④ 未 Play：看 Buy/Sell 任一行 Price（如 200）与 Number（如 12）字距
        ↓
⑤ Play：改 Number 输入，字距仍为 -12（Ensure 路径读同一常量）
```

---

## ⑥ 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| V1 | 读 `UiSpriteNumberDisplay` 常量 | Price/Number 均为 `-12f` |
| V2 | Prefab `Price/DigitStrip`、`Number/DigitStrip` | `spacing` 与 HLG Spacing = `-12` |
| V3 | 再跑一次 Bake | 场景行 DigitStrip 仍为 `-12`（不被盖回 0/-1） |
| V4 | Play 输入 Number `12` | 两位图间距与 Prefab 一致、偏紧不散 |

---

## ⑦ 提交说明模板

```
改了哪些：ShopPriceSpacing/ShopNumberSpacing → -12；Shop_Bar Price/Number DigitStrip 同步。
实现了什么：价签与数量图字距统一为 -12，Tools Bake 重刷后仍保持该间距。
如何验证：Bake 后目检多位价/数量；Play 输入 Number 确认字距未回弹。
```
