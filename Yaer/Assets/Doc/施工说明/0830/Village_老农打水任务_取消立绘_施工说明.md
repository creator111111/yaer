# Village_老农打水任务 — 取消立绘 — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_老农打水任务_取消立绘_架构溯源报告.md`  
**范围**：仅改本对白 Prefab；不改交互 / Story 名 / Choice / Import 器。

---

## ① 结论一句话

已按方案 A 拆掉 `GoOutStoryYaerPainting` 与 BB 绑定；对白仍 UIAlpha→句，只出对话框与雅尔/老人名字，无大立绘。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Village_老农打水任务.prefab` | 删除 Yaer 下 `GoOutStoryYaerPainting` PrefabInstance 及 stripped RectTransform/CanvasGroup | 源立绘默认 alpha=1，上场即可见；产品不要大立绘 |
| 同上 | Yaer `m_Children` 清空；BB `_variables`/`_objectReferences`/`_serializedVariables` 清空 | 对齐 TreeHouseLock；防空引用 |
| 同上 | `_boundGraphSource._comments` 注明「本对白无立绘」 | 防误嵌立绘回潮 |
| 同上 | **保留** UIAlpha（PrepareMask 关）+ 雅尔/老人 Actor + 15 句 | 对话框与名字栏必须在 |
| `…老农基础对话交互_施工说明.md` | 改口「产品不要立绘」 | 产品取消老人立绘 P1 |
| `OPEN_QUESTIONS` | 取消立绘节标已施工；Q4 关闭 | 关单 |

**未改**：CSV、场景、`Npc_Farmer`、Import 器、Generated.asset、全局 Mask。

**Mask 口径**：本期不关 Panel 默认小头像（对照 TreeHouseLock）；坚持纯字幕无 Mask → 另开 P1。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 点 `Npc_Farmer` | **对话框出现**，文案正常 |
| 2 | 全程 | **无**雅儿/老人 **大立绘** |
| 3 | 名字栏 | 雅尔 / 老人 |
| 4 | 结束 | 回村正常；可再谈 |
| 5 | Console | 无 Missing Painting / BB 空引用 |

---

## ④ 给程序 · 防回潮

- 再 Import 本 CSV：可勾「对话框 UI 淡入」；**勿**勾「立绘 CanvasGroup 淡入」、勿指定立绘参考 Prefab。  
- 勿从 ShopStart/ShopHead 拷 Yaer 立绘子树。  
- 若又嵌立绘：再跑方案 A（删实例 + 清 BB）。  
- 下期 `_接受/_拒绝` Prefab 建壳时抄本线无立绘结构。
