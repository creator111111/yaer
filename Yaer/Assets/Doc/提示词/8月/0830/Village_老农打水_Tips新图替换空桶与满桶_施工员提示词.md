# Cursor Agent Prompt · 打水 Tips 新美术：空桶 / 装满水的桶 替换占位图

> **角色**：【施工员】为主（资源替换 + 图集打包验收）；必要时先【架构侦探】只读对拍 Key/挂点（可同会话）  
> **日期**：2026-08-30  
> **产品**：两张新「获得道具」横幅字图，分别装到 **获空桶**、**井上打水（满桶）**，替换现网血珠占位错字图  
> **用户交付图（聊天附件）**：  
> 1. 「**获得了空桶**」→ 接任务发空桶处  
> 2. 「**获得了装满水的桶**」→ 点井打水成功处  
> **现网 Key（勿轻易改代码）**：  
> - 获空桶：`GetEmptyWaterBucketx4`（接任务一次发 4，TipKey 仍用此名）  
> - 打水满桶：`GetFullWaterBucket`（`VillageWellLogic.FullBucketTipKey`）  
> **关联**：TipsPanel / `tipsInfo*.spriteatlas` / 艾琳之剑同款横幅管线  
> **报告/说明落盘**：`Assets/Doc/施工说明/0830/Village_老农打水_Tips新图替换空桶与满桶_施工说明.md`

把下面「施工」整段复制给 Cursor Agent（Agent Mode）执行。若需先核对挂点，可先跑文末「侦探核对」短段。

---

## 提示词助手预梳理（施工须核实）

### 安装对照表（钉死）

| 用户图文案 | 用途 | TipKey（Sprite 名） | 触发点（现网） |
|------------|------|---------------------|----------------|
| **获得了空桶** | 接任务拿到空桶时的横幅 | **`GetEmptyWaterBucketx4`** | 老农「帮」→ `OpenTipsForm("GetEmptyWaterBucketx4")`（对白图） |
| **获得了装满水的桶** | 井上打水成功时的横幅 | **`GetFullWaterBucket`** | `VillageWellLogic` → `OpenTipsForm(FullBucketTipKey)` |

**注意**：Key 名仍带 `x4`，但用户新图写的是「获得了空桶」（无「×4」字样）——**产品已用新图，以图为准**；**不要**为了对齐文件名去改用户文案，也**不要**为了图上无 ×4 就改 Key（改 Key 要动对话 Prefab + 三语图集，易漏）。

### 资源落盘路径（三语都要有文件）

源图目录（中文真源）：

```
Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas/
  GetEmptyWaterBucketx4.png   ← 换成「获得了空桶」
  GetFullWaterBucket.png      ← 换成「获得了装满水的桶」
```

同步（可暂用同一张中文图占位，或等译制）：

```
…/TipInfoAtlas_en/GetEmptyWaterBucketx4.png
…/TipInfoAtlas_en/GetFullWaterBucket.png
…/TipInfoAtlas_jp/GetEmptyWaterBucketx4.png
…/TipInfoAtlas_jp/GetFullWaterBucket.png
```

图集（须含同名 Sprite，替换后 **Pack**）：

```
Assets/GameRes/Atlas/TipsPanel/tipsInfo.spriteatlas
Assets/GameRes/Atlas/TipsPanel/tipsInfo_en.spriteatlas
Assets/GameRes/Atlas/TipsPanel/tipsInfo_jp.spriteatlas
```

（若工程还登记了 `Tips_Char.spriteatlas`，一并确认或按现网惯例同步。）

### 附件怎么进工程

1. 将用户附件两张 png **覆盖**上述 `TipInfoAtlas` 路径（保留文件名 = TipKey）。  
2. Import 设置对齐 `GetAiLinSword.png`（Sprite、透明、不压缩糊字）。  
3. 打开 Sprite Atlas → Pack；Play 验收。  
4. **默认不改** `VillageWellLogic` / 对话 Prefab TipKey（挂点已通，只换皮）。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 用新图替换两 Key 的占位 png（中+en+jp） | ❌ 改 TipsPanel 逻辑 / 新横幅系统 |
| ✅ Pack 图集；验收获空桶与打水两处 | ❌ 改道具 Icon（除非顺手有专用桶 Icon） |
| ✅ 写施工说明 | ❌ 改 Quest / 井换桶逻辑 / 存档 |

### 严禁

- 改 TipKey 字符串却漏改 Prefab/Atlas 导致不弹窗  
- 只换中文目录、忘 en/jp（切语言会缺图静默）  
- 文件名写成中文「获得了空桶.png」（必须 = Key）  

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/施工说明/0830/Village_老农打水_空满桶道具与井交互及接任务_施工说明.md
@Assets/Doc/执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Tips/TipsFormProxy.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageWellLogic.cs
@Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas/GetEmptyWaterBucketx4.png
@Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas/GetFullWaterBucket.png
@Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas_en/
@Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas_jp/
@Assets/GameRes/Atlas/TipsPanel/tipsInfo.spriteatlas
@Assets/GameRes/Atlas/TipsPanel/tipsInfo_en.spriteatlas
@Assets/GameRes/Atlas/TipsPanel/tipsInfo_jp.spriteatlas
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab

你现在是【施工员】。Unity 2020.3.48f1。
把用户提供的两张新 Tips 字图安装到「获得空桶」与「井上打水」触发点对应的图集资源上，替换血珠占位。

---

## 任务

1. **空桶图**「获得了空桶」→ 覆盖三语  
   `TipInfoAtlas{,_en,_jp}/GetEmptyWaterBucketx4.png`  
2. **满桶图**「获得了装满水的桶」→ 覆盖三语  
   `TipInfoAtlas{,_en,_jp}/GetFullWaterBucket.png`  
3. Import 设置对齐现有 GetAiLinSword / 旧占位 meta（Sprite、可读 alpha）。  
4. Pack `tipsInfo` / `tipsInfo_en` / `tipsInfo_jp`（及现网若引用的 Tips_Char）。  
5. **不要改** TipKey 与 Well/对话逻辑（除非发现 Key 与文件名不一致必须修）。  
6. 若用户附件路径在 Cursor workspaceStorage：复制进 ArtRes，勿只留在聊天缓存。  
7. 写出施工说明到：  
   `Assets/Doc/施工说明/0830/Village_老农打水_Tips新图替换空桶与满桶_施工说明.md`

---

## 验收

| # | 操作 | 通过 |
|---|------|------|
| 1 | 老农选「帮」发空桶 | 横幅字为 **获得了空桶**（非血珠/艾琳之剑） |
| 2 | 任务中点井成功 | 横幅字为 **获得了装满水的桶** |
| 3 | 切英文/日文（若暂用中文图） | 仍能弹出（不报「未找到Tips图片」） |
| 4 | Console | 无 Missing Sprite / 未找到Tips图片 |

---

## 提交说明须含

- 覆盖了哪些 png 路径  
- 是否 Pack 了哪些 atlas  
- TipKey 是否未改  
- 英/日是否暂用中文图  
- 未做项（专用 Icon、译制字图等）
```

---

## 可选：侦探核对（施工前 2 分钟，只读）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageWellLogic.cs
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab
@Assets/GameRes/Atlas/TipsPanel/tipsInfo.spriteatlas

只读确认：OpenTipsForm 的空桶 Key、井 FullBucketTipKey 是否仍为 GetEmptyWaterBucketx4 / GetFullWaterBucket；
图集是否已登记这两名。输出三行结论即可，然后按上文施工。
```
