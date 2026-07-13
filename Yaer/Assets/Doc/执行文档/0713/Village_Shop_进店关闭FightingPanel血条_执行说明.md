# Village_Shop · 进店关闭 FightingPanel（血条）— 执行说明

**性质**：架构侦探 + 施工指引（**只解决血条不隐藏**）  
**日期**：2026-07-13  
**范围外**（本文不管）：
- 黑幕渐入渐出（**已解决**，勿再改）
- 0704 合层 → ShopPanel（**弃用**，会导致 UI 位置错乱）

**依据**：
- `BaseGameSceneManager.InitPlayer` / `OpenFightingPanel`
- `Village_Shop` Config：`canCreatePlayer=0`、`isFightingScene=0`
- 修订说明：`0713/Village_Shop_关FightingPanel与ShopPanel同黑幕节奏_执行说明.md` v0.2

---

## 1. 结论

**进店后血条还在，是因为商店是纯 UI、不生成玩家，基类关血条的逻辑根本没跑到；在 `Village_ShopSceneManager.OnEnterScene` 里显式 `CloseUIForm(FightingPanel)` 即可。**

生活类比：上一站挂的价签，这间店进门时没人负责摘；自己进门时摘掉就行。

---

## 2. 根因

| 步骤 | 有玩家的室内 / 战斗场 | `Village_Shop` |
|------|----------------------|----------------|
| `canCreatePlayer` | true | **false** |
| `InitPlayer` | 会跑 | **不跑** |
| 末尾 `OpenFightingPanel()` | 会跑 | **不跑** |
| `isFightingScene=false` 时 | 发现已开则 **Close** FightingPanel | — |
| 上一场景留下的血条 | 被关掉 | **一直挂着** |

血条面板路径：`UIPrefabPath.GetUIPrefabPath("FightingPanel")`（GF UI，跨场景常驻直到被关）。

---

## 3. 施工（最小改动）

**文件**：`Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs`  
**位置**：`OnEnterScene`，建议在 `base.OnEnterScene()` 之后尽早执行。

```csharp
// 纯 UI 不跑 InitPlayer，基类不会自动关 FightingPanel；进店显式关掉血条 HUD。
// 原因：上一场景（如村里）打开的 FightingPanel 会跨场景残留。
// 替代方案：改 BaseGameSceneManager「canCreatePlayer==false 也调 OpenFightingPanel」——影响面大，本任务不采用。
var fightingPath = UIPrefabPath.GetUIPrefabPath("FightingPanel");
var ui = GameManager.GetGMComponent<UIComponentGM>();
if (ui.GetUIForm(fightingPath) != null)
{
    ui.CloseUIForm(fightingPath);
    Debug.Log("[VillageShopDebug] CloseUIForm FightingPanel");
}
```

需补 using（若尚未有）：

- `Game.GameMgr.Component.UI`
- `Game.Static.Path`（`UIPrefabPath`）

**不要**：

- 为关血条去建 ShopPanel / 改 0704  
- 改基类给所有无玩家场景统一关（除非后续多场景复用再立项）

---

## 4. 验收

| # | 操作 | 通过 |
|---|------|------|
| 1 | InitScene → 村里（可见血条）→ Door_Shop 进店 | **无**血条 / FightingPanel |
| 2 | 进店后商店合层 + `UI_Shop` | 仍正常（本改不动商店 UI） |
| 3 | 离店回村 | 若村里是战斗/需血条场景，按原逻辑再开 FightingPanel（与本改无关，抽测即可） |
| 4 | Console | 可选：出现 `[VillageShopDebug] CloseUIForm FightingPanel` |

---

## 5. 改动清单

| 文件 | 动作 |
|------|------|
| `Village_ShopSceneManager.cs` | `OnEnterScene` 增加显式 Close FightingPanel |

---

## 6. 版本记录

| 版本 | 日期 | 说明 |
|------|------|------|
| 0.1 | 2026-07-13 | 从「关血条+ShopPanel」合订本拆出；仅血条隐藏 |

**文档路径**：`Assets/Doc/执行文档/0713/Village_Shop_进店关闭FightingPanel血条_执行说明.md`
