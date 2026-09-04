# Village_Shop — 关货币旁路默认 — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_Shop_没钱仍能购买与成败对白验收_架构溯源报告.md`  
**范围**：仅改 `bypassGoldCheckForBagJoint` 正式默认为关；**未**改 Yes/No 接线、GSM、扣款 API。

---

## ① 结论一句话

三处旁路已同步为 **false/0**：正规进店点「决定」会真实扣款；钱不够不入包并播 `Village_ShopNo`。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `ShopFormLogic.cs` | 字段默认 `true` → `false`；注释写清正式默认关 / 手开验不出 No | 脚本新建组件不再默开旁路 |
| `ShopPanel.prefab` | `bypassGoldCheckForBagJoint: 0` | Prefab 打开路径一致 |
| `Village_Shop.unity`（UI_Shop） | 同上 `0` | **正规进店主路径**；漏改则仍假成功 |

Unity 序列化：**场景/Prefab 实例值优先于脚本字段初始值**，故必须三处一起改。

**未改**：`Village_ShopSceneManager`、Yes/No Prefab、`PlayerGoldData`、成败对白接线（报告已齐）。

**替代方案（未做 · 报告 Q1）**：旁路仅 Editor / 开发菜单；本期保留 SerializeField，联调可手开。

---

## ③ 验收清单（Unity Play）

| # | 前置 | 操作 | 通过 |
|---|------|------|------|
| 1 | 旁路关（磁盘已关，勿依赖手关）；金币 **&lt;** Total2 | 点「决定」 | **不入包**；金币不变；播 **ShopNo**；Console：`LogInsufficientGold` + `[ShopSpecial] … Village_ShopNo` |
| 2 | 旁路关；金币 **≥** 合计 | 点「决定」 | 扣款入包；播 **ShopYes** |
| 3 | Inspector **故意勾上**旁路；金币再少 | 点「决定」 | 仍入包 + Yes（开关仍可用）；**勿**当失败已验 |
| 4 | Console | — | 成败故事名 = `Village_ShopYes` / `Village_ShopNo`；**不是** `Village_ShopStart` |

**把金币刷到不够**：`Tools / Debug / Player Gold Tool…` → **「减少 Spend」** 减到 &lt; 合计后再点决定。

---

## ④ 给程序

- diff 仅默认值 + 注释；业务分支未动。
- 旁路仍 true 时设计如此会假成功；验收失败对白必须确认三处为 0。
