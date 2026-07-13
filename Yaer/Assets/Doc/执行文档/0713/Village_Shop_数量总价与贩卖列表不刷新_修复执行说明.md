# Village_Shop · 改数量不刷总价 / 点贩卖不切列表 — 修复执行说明

**性质**：架构侦探 + 修复施工指引  
**日期**：2026-07-13  
**现象**：从 **InitScene → 村里 → Door_Shop** 正常进店后：
1. 输入数量 → **Total2 总价数字不刷新**
2. 点「贩卖」→ **出售列表不切换 / 不刷新**

**范围外**：黑幕节奏（已解决）；0704 合层整页搬 ShopPanel（已弃用）。

---

## 1. 结论（一句话）

**接线被挪进了 GF 的 `OnInit`，但正常进店仍用场景里常驻的 `UI_Shop`，从来不会走 `OpenUIForm`，所以 `OnInit` 根本不跑——数量监听和贩卖按钮都没绑上。**

生活类比：把「门铃线」接到了「只有正式前台才通电」的插座上，可你进的是侧门常驻柜台，线永远通不上电。

---

## 2. 现象 ↔ 代码

| 玩家操作 | 期望 | 实际（正规进店） | 缺的接线 |
|----------|------|------------------|----------|
| 改 Number 数量 | `RefreshTotal2()` 刷合计图 | 行内 Digit 可能变，**Total2 不动** | `OnQuantityValueChanged += RefreshTotal2` |
| 点 SELL / 贩卖 | `SwitchToSellTab()` 显出售 Scroll | **列表不切** | `btnSell.onClick → SwitchToSellTab` |

两处接线目前都只在 `ShopFormLogic.OnInit` 里做。

---

## 3. 根因溯源

### 3.1 生命周期错位（主因）

```
【旧 · 能用】ShopFormLogic : MonoBehaviour
  Awake() → Resolve / Collect / WireAll / WireSell … ✅ 场景 UI_Shop 一加载就绑好

【现 · 坏了】ShopFormLogic : BaseUIFormLogic
  OnInit() → 同上接线   ← 只有 UIComponentGM.OpenUIForm 才会调
  Awake()  → 只补 canvas / UICamera，不接线
```

| 进店方式 | `UI_Shop` 在场景常驻 | 是否 `OpenUIForm(ShopPanel)` | 是否调用 `OnInit` | 接线 |
|----------|----------------------|------------------------------|-------------------|------|
| 正规流程 Door_Shop | ✅ Active | ❌ **工程内无任何 Open ShopPanel 调用** | ❌ | **全断** |
| 若将来只 Open Prefab | 应禁用场景 UI_Shop | ✅ | ✅ | 正常 |

静态核对（2026-07-13）：

- `Village_Shop.unity` → `UI_Shop` **`m_IsActive: 1`**，挂着 `ShopFormLogic`
- 全工程 **没有** `OpenUIForm(...ShopPanel...)` 的运行时调用（仅有 Bake Editor / 注释）
- `Village_ShopSceneManager` **只**关 FightingPanel + 相机，**不**开商店 Form

→ 正规进店 = **场景 MonoBehaviour 路径**，却依赖 **Form OnInit** → 必现双 Bug。

### 3.2 为何像「只有正规流程才坏」

| 环境 | 说明 |
|------|------|
| 改继承 **之前** 直接 Play 场景 | `Awake` 接线，数量/贩卖正常 |
| 改成 `OnInit` 后正规进店 | 不跑 `OnInit`，双 Bug |
| 若有人误以为已走 ShopPanel OpenUIForm | Prefab 路径其实未接入，仍踩场景 `UI_Shop` |

### 3.3 次要风险（修主因时一并避开）

| 风险 | 说明 |
|------|------|
| 场景 `UI_Shop` + 再 Open `ShopPanel` 双开 | 两套逻辑抢点击；本期 **不要**为修 Bug 强行 Open Prefab |
| `ShopBuyRowQuantityInput` OnEnable/OnDisable | 切 Tab 会卸 TMP 监听，但 C# 事件 `OnQuantityValueChanged` 仍在；主因仍是根本没 Wire 到 `RefreshTotal2` |

---

## 4. 修复方案（推荐）

### FIX-1 · 场景路径也能接线（主修 · 必做）

把「商店运行时绑定」抽成可幂等方法，**两条入口都调**：

| 入口 | 何时 |
|------|------|
| `OnInit` | 若以后真走 `OpenUIForm(ShopPanel)` |
| `Awake` 末尾或 `Start` | **场景常驻 `UI_Shop`（现行正规进店）** |

示意（施工员按现有方法名落地，加详细注释）：

```csharp
private bool _shopRuntimeBound;

/// <summary>
/// 解析引用 + 收集行 + 绑数量刷新 / Tab / 确认 / 离店。
/// 必须幂等：OnInit（GF）与 Awake/Start（场景 UI_Shop）都可能调用。
/// 原因：正规进店不走 OpenUIForm，仅 OnInit 接线会导致 Total2/贩卖全失效。
/// 替代方案：强制 OpenUIForm(ShopPanel) 并禁用场景 UI_Shop —— 与「保持双轨、弃用 0704 搬合层」冲突，本期不采用。
/// </summary>
private void EnsureShopRuntimeBound()
{
    if (_shopRuntimeBound)
    {
        return;
    }

    ResolveShopReferences();
    EnsureDualScrollShell();
    ApplyScrollInteractionFixes();
    CollectBuyRowViews();
    CollectSellRowViews();
    ResolveTotal2DigitsReference();
    WireAllRowQuantityRefresh();
    WireBuyTabButton();
    WireSellTabButton();
    ResolveConfirmButtonReference();
    WireConfirmButton();
    ResolveExitButtonReference();
    WireExitButton();

    _shopRuntimeBound = true;
}

protected override void Awake()
{
    // …现有 canvas / componentSystemUI 兜底 + base.Awake()…
    EnsureShopRuntimeBound();
}

protected internal override void OnInit(object userData)
{
    base.OnInit(userData);
    EnsureShopRuntimeBound();
}

protected internal override void OnOpen(object userData)
{
    base.OnOpen(userData);
    EnsureShopRuntimeBound(); // 防御：池化复开
    SwitchToBuyTab();
    AllowOpenMenu(true);
}
```

**注意**：

- `Wire*` 内已有 `RemoveListener` 再 `Add`，可防双绑；`WireAllRowQuantityRefresh` 开头已 `Unwire` —— 保持。
- `OnDestroy` 继续 Unwire，避免泄漏。

### FIX-2 · 验收日志（建议）

`EnsureShopRuntimeBound` 成功时打一条：

`[ShopFormLogic] runtime bound buyRows=N sellRows=M wiredInputs=K sellBtn=(ok|null)`

正规进店 Console 必须能看到，且 `wiredInputs>0`、`sellBtn=ok`。

### FIX-3 · 不要做的

| 做法 | 原因 |
|------|------|
| 为修 Bug 再推 0704 合层进 ShopPanel | 已证实位置错乱；已冻结 |
| 只改 Prefab、不改场景 `UI_Shop` 脚本 | 正规进店仍跑场景实例 |
| 改基类 `BaseUIFormLogic` 全局 Awake 调业务 | 污染所有 Form |

---

## 5. 验收表

| ID | 操作 | 通过 |
|----|------|------|
| R1 | InitScene → Door_Shop 进店 | Console 有 `runtime bound`，`wiredInputs>0` |
| R2 | 购买 Tab 改某一行数量 | **Total2** 图片合计实时变 |
| R3 | 点贩卖 / SELL | 露出 **出售列表**（Buy Scroll 关、Sell Scroll 开） |
| R4 | 出售行改数量 | Total2 按卖价合计变（若出售行有输入） |
| R5 | 再点购买 | 回到购买列表；合计逻辑正确 |
| R6 | 离店 / ESC 菜单 | 不回归；与本修无关功能抽测 |

---

## 6. 改动文件

| 文件 | 动作 |
|------|------|
| `ShopFormLogic.cs` | 抽 `EnsureShopRuntimeBound`；`Awake`/`OnInit`/`OnOpen` 调用；幂等标志 |

场景 / Prefab **可不改**（现行双轨保持）。

---

## 7. 与前序文档

| 文档 | 关系 |
|------|------|
| `0704/商店界面合层转UI组件_…` | **冻结**；本修 **不**复活 ShopPanel 开店 |
| `0713/…关FightingPanel与ShopPanel同黑幕节奏_…` v0.2 | 黑幕已解决；OpenUIForm 方案已作废 |
| 本修 | 承认现行是 **场景 `UI_Shop`**，把接线从「只信 OnInit」改回「场景也能绑」 |

---

## 8. 版本记录

| 版本 | 日期 | 说明 |
|------|------|------|
| 0.1 | 2026-07-13 | 定位：OnInit 接线 vs 场景 UI_Shop 无 OpenUIForm；FIX-1 幂等 EnsureShopRuntimeBound |

**文档路径**：`Assets/Doc/执行文档/0713/Village_Shop_数量总价与贩卖列表不刷新_修复执行说明.md`
