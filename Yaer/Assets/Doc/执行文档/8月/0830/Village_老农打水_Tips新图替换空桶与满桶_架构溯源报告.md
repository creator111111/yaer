# Village_老农打水 — Tips 新图替换空桶/满桶 — 架构溯源与施工执行说明

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读对拍 + **施工执行清单**（本阶段**未改**代码 / Prefab / 未覆盖 TipKey 文件；新图已以中文文件名落盘）  
**Unity**：2020.3.48f1  
**产品**：用新横幅字图替换血珠占位，挂到获空桶 / 井打水两处  
**正式施工落盘（覆盖 png 后）**：`Assets/Doc/施工说明/0830/Village_老农打水_Tips新图替换空桶与满桶_施工说明.md`

关联：`获得道具Tips横幅_艾琳之剑…` · `Village_老农打水_空满桶…施工说明` · TipsPanel / `tipsInfo*.spriteatlas`

---

## ① 结论一句话

**TipKey 与挂点已通、勿改：`GetEmptyWaterBucketx4`（帮→发空）、`GetFullWaterBucket`（井成功）。图集三语已登记这两名。现网 Key 路径下仍是血珠占位（与 `GetHpBall` 同哈希）。用户新图已在 `TipInfoAtlas/` 但文件名为中文「获得了空桶 / 获得了装满水的桶」——运行时按 Key 取 Sprite，中文名图不会被用到。施工只需把新图内容覆盖到三语 `GetEmptyWaterBucketx4.png` / `GetFullWaterBucket.png`，再 Pack 图集；不要改 Key、不要把正式文件改成中文名。**

---

## ② 原因（通俗）

游戏认的是图集里的**英文代号**（TipKey），不是文件夹里随便起的中文名。  
新美术已经进工程了，但挂在「获得了空桶.png」这种名字上，代码仍去取 `GetEmptyWaterBucketx4`——取到的还是旧的血珠错字图。  
把新图**拷进正确文件名**（三语各一份）再打包图集，横幅字就对了。

---

## ③ 用户检查清单

### 侦探已核实（现网）

| # | 项 | 结果 |
|---|----|------|
| 1 | 对话 Prefab TipKey | **`GetEmptyWaterBucketx4`**（`Village_老农打水任务` · OpenTipsForm） |
| 2 | 井 `FullBucketTipKey` | **`GetFullWaterBucket`**（`VillageWellLogic`） |
| 3 | `tipsInfo{,_en,_jp}` 登记 | ✅ 两名均在 `m_PackedSpriteNamesToIndex` |
| 4 | Key 路径 png vs `GetHpBall` | **同哈希** → 仍是血珠占位 |
| 5 | 新美术 | ✅ 已有 `获得了空桶.png`、`获得了装满水的桶.png`（文案正确） |
| 6 | 中文名是否进图集 Key | ❌ 图集按文件夹 pack，Sprite 名=文件名；中文名 ≠ TipKey → **现网播不出新字** |

### 施工后验收

| # | 操作 | 通过 |
|---|------|------|
| 1 | 老农选「帮」 | 横幅 **获得了空桶**（非血珠/剑） |
| 2 | 任务中点井成功 | 横幅 **获得了装满水的桶** |
| 3 | 切英/日（若暂用中文图） | 仍弹出；无「未找到Tips图片」 |
| 4 | Console | 无 Missing Sprite |

---

## ④ 给程序

### A. 安装对照表（钉死 · 勿改 Key）

| 用户图文案 | TipKey（Sprite 名） | 触发点 |
|------------|---------------------|--------|
| **获得了空桶** | **`GetEmptyWaterBucketx4`** | 帮 → `OpenTipsForm("GetEmptyWaterBucketx4")` |
| **获得了装满水的桶** | **`GetFullWaterBucket`** | `VillageWellLogic` → `OpenTipsForm(FullBucketTipKey)` |

- Key 仍带 `x4`，图上无「×4」——**以新图为准**；**禁止**为对齐文案改 Key（会漏 Prefab/三语图集）。  
- **禁止**把运行时文件改名为中文「获得了空桶.png」。

### B. 磁盘现状（2026-08-31 扫盘）

| 路径 | 状态 |
|------|------|
| `TipInfoAtlas/GetEmptyWaterBucketx4.png` | 占位＝GetHpBall（35017B） |
| `TipInfoAtlas/GetFullWaterBucket.png` | 占位＝GetHpBall |
| `TipInfoAtlas/获得了空桶.png` | **新图真源**（≈23KB，文案正确） |
| `TipInfoAtlas/获得了装满水的桶.png` | **新图真源**（≈38KB，文案正确） |
| `TipInfoAtlas_en/GetEmpty…` / `GetFull…` | 体积异常大（≈1.7MB）——施工时用中文新图**覆盖**，勿保留错位大图 |
| `TipInfoAtlas_jp/…` | 旧占位量级；施工覆盖 |

Import meta：Key 文件已有 Sprite 设置（对齐剑/旧占位）；覆盖像素后 **保留** 现有 `.meta` guid，避免图集丢引用。

### C. 最小施工步骤（执行时按此做）

1. **覆盖中文目录**（保留文件名与 meta guid）  
   - 将 `获得了空桶.png` **内容**写入 `GetEmptyWaterBucketx4.png`  
   - 将 `获得了装满水的桶.png` **内容**写入 `GetFullWaterBucket.png`  
2. **同步 en / jp**（本期可暂用同一中文图）  
   - `TipInfoAtlas_en/`、`TipInfoAtlas_jp/` 下同名两文件一并覆盖  
3. Unity：确认 TextureType=Sprite、透明可读；对齐 `GetAiLinSword`  
4. **Pack** `tipsInfo` / `tipsInfo_en` / `tipsInfo_jp`（文件夹已是 packable，一般无需改 atlas YAML）  
5. **不要改** `VillageWellLogic`、对话 Prefab TipKey、Quest  
6. 中文文件名源图：可留作备份，或移出 TipInfoAtlas 以免图集多打进无用 Sprite（可选清理）  
7. 写出 `施工说明/0830/Village_老农打水_Tips新图替换空桶与满桶_施工说明.md`

### D. 不做

| 不做 |
|------|
| 改 TipsPanel / 新横幅系统 |
| 改道具 Icon（Icon 目录另有「空桶/水桶」图，非本期 Tips） |
| 改 Quest / 井逻辑 / 存档 |
| 英日正式译制字图（可另开） |

### E. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 英/日是否长期共用中文图？ | **暂用中文**；译制另开 | ✅ |
| Q2 | 中文文件名源图是否移出 Atlas 目录？ | 施工后建议移走或删，防多余 Sprite | ⏳ |
| Q3 | en 下 1.7MB 异常图从何来？ | 覆盖即消；无需深究 | ✅ |

（已追加 `OPEN_QUESTIONS.md`。）
