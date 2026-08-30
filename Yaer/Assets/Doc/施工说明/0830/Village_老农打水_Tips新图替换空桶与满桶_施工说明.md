# Village_老农打水 — Tips 新图替换空桶/满桶 — 施工说明

**日期**：2026-08-31  
**角色**：施工员  
**依据**：`执行文档/0830/Village_老农打水_Tips新图替换空桶与满桶_架构溯源报告.md`  
**范围**：仅覆盖 TipKey png 像素；**未改** Key / Prefab / 井逻辑 / Quest。

---

## ① 结论一句话

三语 `GetEmptyWaterBucketx4` / `GetFullWaterBucket` 已用「获得了空桶 / 获得了装满水的桶」新图覆盖；meta guid 未动。进 Unity 后 Pack 图集即可验收。

---

## ② 改了什么 & 原因

| 动作 | 说明 | 原因 |
|------|------|------|
| 覆盖 `TipInfoAtlas{,_en,_jp}/GetEmptyWaterBucketx4.png` | 源：中文「获得了空桶.png」（23493B） | 运行时按 TipKey 取图；中文文件名不会被用到 |
| 覆盖同目录 `GetFullWaterBucket.png` | 源：中文「获得了装满水的桶.png」（37945B） | 井打水 Tips |
| **保留** 各文件 `.meta` guid | 覆盖前后一致 | 避免图集丢引用 |
| 英日暂用中文图 | 报告 Q1 | 译制另开 |
| **未改** TipKey / Prefab / `VillageWellLogic` | 报告钉死 | 改 Key 会漏挂点 |

**未做（可选）**：把中文文件名源图移出 `TipInfoAtlas/`（Q2）——源图仍留在目录作备份；若 Pack 后多出无用 Sprite，可手动移走。

体积校验：覆盖后 ≠ `GetHpBall`（35017B）→ 已非血珠占位。

---

## ③ 验收清单（Unity）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 选中 tipsInfo / _en / _jp → **Pack**（或等自动） | 无报错 |
| 2 | 老农选「帮」 | 横幅字 **获得了空桶**（非血珠） |
| 3 | 任务中点井成功 | **获得了装满水的桶** |
| 4 | Console | 无「未找到Tips图片」 |

---

## ④ 给程序

- TipKey 仍为 `GetEmptyWaterBucketx4`（图上无「×4」以美术为准）。  
- 英日正式译制字图另开；本期共用中文像素。
