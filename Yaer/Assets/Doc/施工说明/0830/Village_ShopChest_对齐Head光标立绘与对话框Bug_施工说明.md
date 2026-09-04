# Village_ShopChest — 对齐 Head（光标 + 立绘 + 对话框）— 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_ShopChest_对齐Head光标立绘与对话框Bug_架构溯源报告.md`  
**范围**：R1 壳层写入 Prefab bound + Chest Catch；**未**改故事名 / C6 / Head 节点序。

---

## ① 结论一句话

点胸已对齐 Head：悬停 Catch、雅立绘先淡入、对话框再出现（Fighting → CGAlpha → UIAlpha → 句）。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Village_ShopChest.prefab` | bound 图含 Fighting→立绘→UIAlpha→C1～C5；`_graph=0` | D1 主因：仅 Statement 无壳 → 框不出现 |
| `Village_商店点胸交互.asset` | 同步壳层（旁路校对） | 与 bound 一致 |
| `Village_Shop.unity` | `Trigger/Chest` 挂 `CursorChangeTrigger` Catch/1 | 悬停不变手 |
| `ShopkeeperSpecialClickSetupEditor.cs` | Chest `fadeYaerPortrait=true`；Rebuild Chest 菜单；EnsureHotspot 挂 Catch；Fighting 随立绘开 | 防 Setup 回潮 |
| `DialoguePreludeBuilder.cs` | UIAlpha Delay=0.5 + PrepareMask 对齐 Head | 后续 Rebuild 参数一致 |

**未改**：GSM 常量 / Hotspot 点击逻辑；Head 图；C6 树屋；`MerchantPainting` Prefab 源（P1）。

**替代（未采用）**：只手改 Generated；双图本地壳。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 移入 Chest | 光标 **Catch**；移出恢复 |
| 2 | Idle 点 Chest | **对话框出现**；藏 UI_Shop |
| 3 | 对白开始 | **雅大立绘可见**且先于框 |
| 4 | 店/雅句 | 表情跟句 |
| 5 | 结束 | Idle；UI；热区；光标；脸复位 |
| 6 | 点 Head | 回归正常 |
| 7 | Console | `Village_ShopChest started=true` |

若以后 CSV 重导丢壳：菜单  
`Tools / Dialogue / Rebuild Shopkeeper Chest Prefab Only (Village_ShopChest)`。

---

## ④ 给程序

- 纯 CSV 重导会丢框——须带 Prelude（`fadeYaerPortrait=true`）写回 Prefab bound。  
- 前序「Chest Catch 本期否」本单改口已做。
