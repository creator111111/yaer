# Village_Shop · Head 悬停光标 Catch — 施工说明（0829）

**角色**：施工员  
**日期**：2026-08-29  
**依据**：`执行文档/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md` §D 方案 A  
**用户拍板**：选项 **2 · `CursorState.Catch`**（手张 / 按住手握）；点胸不同期挂；不要第五种新图

---

## ① 结论一句话

**已在场景 `Head` 挂上 `CursorChangeTrigger`（Catch / Priority=1），并让 `SetShopkeeperHotspotsEnabled` 同步开关该组件，对白关热区时会走 Trigger.OnDisable → Exit，避免光标卡在「永远手」。**

---

## ② 改了什么

| 文件 | 改动 |
|------|------|
| `Assets/GameRes/Scenes/Village_Shop.unity` | `Trigger/Head` 增加 `CursorChangeTrigger`：`TargetState=1`（Catch），`Priority=1`；复用已有 `BoxCollider2D` |
| `Village_ShopSceneManager.cs` | `SetShopkeeperHotspotsEnabled` 增加对子树 `CursorChangeTrigger` 的 enable/disable；注释写明「只关 Collider 会卡光标」 |

**未改**：`CursorComponentGM` 中枢；Chest；`MerchantPainting.prefab`（场景实例已够验收，Prefab 同步仍为 P1）；对白 Prefab / 故事名。

---

## ③ Head 组件现状（施工后）

路径：`商店界面合层` → ` MerchantPainting` → `Trigger` → `Head`

| 组件 | 序列化 |
|------|--------|
| `BoxCollider2D` | isTrigger，size≈(2.2, 2) |
| `ShopkeeperBodyHotspot` | `hotspotKind=Head` |
| **`CursorChangeTrigger`（新增）** | **`TargetState = Catch (1)`** · **`Priority = 1`** |

Chest：**无** `CursorChangeTrigger`（本期排除）。

---

## ④ 热区开关如何带上光标

```
SetShopkeeperHotspotsEnabled(enabled)
  → Collider2D.enabled = enabled
  → ShopkeeperBodyHotspot.enabled = enabled
  → CursorChangeTrigger.enabled = enabled   // 0829 新增
       disable 时 → CursorChangeTrigger.OnDisable → OnPointerExit → 出队
```

原因：只关 Collider 时 Trigger 的 Update 仍可能 `OverlapPoint` 判「仍在头上」。  
替代方案（未做）：方案 B 在 Hotspot 上写 IPointerEnter/Exit。

---

## ⑤ 验收清单（请你在 Unity Play 对照）

| # | 操作 | 通过标准 | Agent 侧 |
|---|------|----------|----------|
| 1 | Idle 移入 Head | 光标变 **手张（Catch）** | 场景已挂 Catch；需 Play 目视 |
| 2 | Head 上按住左键 | **手握**；松开回手张 | 中枢既有逻辑，未改 |
| 3 | 移出 Head | 回普通光标 | 依赖 Trigger Exit |
| 4 | Head 点开对白 | 对白中不卡死；结束正常 | 关热区会 disable Trigger |
| 5 | 对白中热区关闭 | 无「永远手」 | 同 #4 |

> 本环境未跑 Unity Play；磁盘 YAML/代码已按清单落地。请本地进店验 1～5。

Console 可滤：`[ShopSpecial] SetShopkeeperHotspotsEnabled` 应出现 `cursorTriggers=`（Head 挂上后至少为 1）。

---

## ⑥ 未做项

| 项 | 状态 |
|----|------|
| Chest 同挂 Catch | ❌ 用户明确否 |
| 写回 `MerchantPainting.prefab` | ❌ P1；现仅场景实例 |
| 扩枚举 / 新贴图 | ❌ |
| 改点头对白故事名 | ❌ 另单 |

---

## ⑦ OPEN_QUESTIONS 更新建议

| ID | 更新 |
|----|------|
| 悬停光标 Q1 | ✅ 用户选定 **Catch** |
| Q3 点胸 | ✅ 本期否 |
| Q4 关热区 Exit | ✅ 已施工（同步 disable Trigger） |
