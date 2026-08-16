# 雅儿小头像 · GoOut↔Dress 分类触发与盔冠机制 — 架构溯源报告

**文档版本**：v1.1（2026-08-07）  
**文档性质**：【架构侦探】溯源 + **产品拍板记录**（本期**不改代码**；机制已按方案 A 在 0806 落地）  
**范围**：对话框左侧雅儿「小头像」——原图集四套如何分类；Mask 如何对应；盔/冠如何细分  
**非目标**：不改开场分层；不扩表情；不重做 Prefab 摆位；**本期不改真源为镜像大立绘**  
**依据**：
- `Assets/Doc/提示词/0807/雅儿小头像_GoOut与Dress分类触发_盔冠机制_架构侦探提示词.md`
- `DialogueAvatarLoader` / `DialogueAvatarPathHelper` / `DialogueMaskAvatarPresenter`
- `GoOutStoryYaerPainting` / `YaerPainting` / `NormalDialogueNewPanel.prefab`
- 关联：0727 图集报告、0803 Mask 接线、0806 Dress 启用（已施工方案 A）

**Unity**：2020.3.48f1  

**产品拍板（2026-08-07）**：小头像服装真源 **继续跟存档**（方案 A）；四态分类与旧图集同一套逻辑（见 §3.1）。方案 C「镜像台上大立绘」**本期不做**。

---

## ① 结论一句话

**原来（图集世代）**：小头像跟存档 **Clothes × Headwear** 选四套 `Avatar_Yaer_*` 贴纸。  
**现网 Mask（已拍板维持）**：同一套逻辑——`Clothes==Dress` → `YaerPainting`；外出三态 → `GoOutStoryYaerPainting` + `Heads` 下 `ArmorCrown`/`ArmorHead` 显隐。真源仍是**存档**，不是场景大立绘。  
**体感注意**：大立绘由对话 Prefab 摆放，可与 Mask 不同源——属设计接受，不是缺切换机制。

---

## ② 原因（生活类比）

旧系统像**贴纸册**：按「衣服标签 + 头饰标签」抽一张贴到 Portrait。  
新系统像**窗后两个全身人偶**：只亮一个，再拨脸；盔/冠是外出人偶头上的零件。  
两个都挂在 `YaerAvatarRoot` 下≠「同时显示」——运行时 Presenter 互斥只亮一套。  
**已拍板**：遥控器继续读**衣柜存档**，不改成「台上演员穿什么就播什么」。

---

## ③ 用户需要做什么（拍板结果 + 验收）

### 3.1 已拍板（2026-08-07）

| ID | 决议 |
|----|------|
| **真源** | **继续跟存档** `PlayerClothesData`（Clothes + Headwear）；**不做**镜像台上大立绘 |
| **四态分类** | 与旧四套贴纸同一逻辑（下表定稿） |
| Q2～Q4 | 仍按现状：Dress 脸键 `Dress_Crown_*`；GoOut 戴冠只显隐零件不换 Face 前缀；字幕不要求四套 atlas 一一补齐 |

#### 产品定稿 · 四态对照表（旧贴纸 → Mask）

| # | 存档条件 | 旧图集 | Mask 亮谁 | Heads 细分 |
|---|----------|--------|-----------|------------|
| 1 | Clothes=`Dress`（头饰现网脸键含 Crown） | `Avatar_Yaer_Dress_Crown` | **`YaerPainting`** | （Dress 无人偶 Heads；脸键 `Dress_Crown_*`） |
| 2 | 非 Dress + Headwear=`NoHeadWear` | `Avatar_Yaer_Armor_NoHeadWear` | **`GoOutStoryYaerPainting`** | `ArmorCrown`/`ArmorHead` **都关** |
| 3 | 非 Dress + Headwear=`Crown` | `Avatar_Yaer_Armor_Crown` | **`GoOutStoryYaerPainting`** | **只开** `ArmorCrown` |
| 4 | 非 Dress + Headwear=`ArmorHead` | `Avatar_Yaer_Armor_ArmorHead` | **`GoOutStoryYaerPainting`** | **只开** `ArmorHead` |

> 实现锚点：`DialogueMaskAvatarPresenter`（套装）+ `GoOutStoryYaerPainting.SyncHeadwearFromArchive`（盔冠）。本期**无需再施工**（0806 已按方案 A 落地）；若验收四态有缺口再开施工员。

### 3.2 验收时看什么

1. Console：`[MaskAvatar] Yaer → GoOut|Dress face=…`  
2. Hierarchy：`YaerAvatarRoot` 下**仅一套** Active  
3. 存档 Clothes=Dress → `YaerPainting`；非 Dress → `GoOutStoryYaerPainting`  
4. GoOut 时按上表切 `Heads/ArmorCrown`、`Heads/ArmorHead`  
5. 大立绘可与 Mask 不同源（**已接受**；要对齐须另案拍板方案 C）

---

## ④ 给程序看的补充

### 4.1 两代调用链

```
【旧图集 · Portrait / 历史列表仍用】
Statement → DialogueTMPUGUI
  → Actor.RefreshAvatar(FaceType)
       → DialogueAvatarLoader.GetAvatar(role, face)
            → ResolveAvatarAtlasPath：
                 Yaer：PlayerClothesData Clothes + Headwear
                      → DialogueAvatarPathHelper
                         Assets/GameRes/Atlas/Avatar/Avatar_Yaer_{Clothes}_{HeadWear}.spriteatlas
                 其它角色：Avatar_{Role}.spriteatlas
            → atlas.GetSprite(faceType.ToString())
  → OnGetAvatar：useMaskAvatar=true 时 Portrait 关，sprite 可给历史；Mask 另路

【现网 Mask · 字幕窗】
同帧 OnGetNewStatement(role, face, text)
  → DialogueMaskAvatarPresenter.Apply
       → HideAll
       → ResolvePainting(Yaer)：
            yaerUseGoOutOnly? → GoOut
            else Clothes==Dress? → YaerPainting : GoOut
       → SetActive(true)；CanvasGroup alpha&lt;1 则拉回 1
       → 若 GoOut：SyncHeadwearFromArchive()
       → ResolveFaceKey → UpdateFace
  （PrepareMask 前奏：UIAlpha → presenter.Apply 同一套 Resolve）
```

### 4.2 旧图集决策表（原机制 · 钉死）

**真源**：存档 `BoneName.Clothes` + `BoneName.Headwear`（**不是**场景名 / 室内外）。  
**沙盒默认**（无 `PlayerDataComponentGM`）：`Dress` + `Crown`（与 `InitNewGameData` 一致）。  
**路径公式**：`Avatar_Yaer_{clothes}_{headWear}.spriteatlas`  
**表情**：图集内 Sprite 名 = `faceType.ToString()`（裸枚举，如 `Smile`）。

磁盘现有四套：

| Clothes | Headwear | 图集文件 | 旧小头像效果 |
|---------|----------|----------|--------------|
| Dress | Crown | `Avatar_Yaer_Dress_Crown` | 室内裙 + 冠 |
| Armor | NoHeadWear | `Avatar_Yaer_Armor_NoHeadWear` | 外出铠 / 无头饰 |
| Armor | Crown | `Avatar_Yaer_Armor_Crown` | 外出 + 王冠 |
| Armor | ArmorHead | `Avatar_Yaer_Armor_ArmorHead` | 外出 + 盔 |

| 维度 | 是否参与旧决策 |
|------|----------------|
| FaceType | 只选 Sprite，不选图集 |
| 场景室内外 / Prefab 大立绘 | **不参与** |
| `NoClothes` 等其它 Clothes | 会拼路径；无对应 atlas 则空头像 |

> 仓库**无** `Avatar_Yaer_Dress_NoHeadWear` 等；Dress 线旧图集实质只有 **Dress+Crown**。

### 4.3 Mask 现网决策表（复核 Prefab）

| 项 | 现网值 / 规则 |
|----|----------------|
| Prefab `yaerUseGoOutOnly` | **`0`（false）** ← 已复核，非 H1 |
| 代码默认 | `false` |
| Painting 引用 | 序列化全 0 → `Find("GoOutStoryYaerPainting")` / `Find("YaerPainting")` |
| Dress 条件 | `GetClothesName(Clothes) == "Dress"` |
| 非 Dress（含 Armor） | → GoOut |
| 无 PlayerData（DialogDebug） | 默认 **Dress**（对齐 Loader 沙盒） |
| Dress Face | `Dress_Crown_{Face}`；`Normal`→`Dress_Crown_Smile` |
| GoOut Face | 恒 `Armor_NoHeadWear_{Face}`；`Normal`→`…_Smile`（**不**随盔冠改前缀） |
| GoOut 头饰 | `SyncHeadwearFromArchive`：Crown→`armorCrown`；ArmorHead→`armorHead`；否则两者关 |
| 触发帧 | 每句 `OnGetNewStatement`；前奏 `PrepareMaskAvatarOnFadeIn`→`Apply` |
| 互斥 | `HideAll` 后只亮当前角色一套 |

```
IsYaerUsingGoOut():
  if yaerUseGoOutOnly → true
  else → !(Clothes == Dress)
```

### 4.4 盔 / 冠专节

| 能力 | 旧图集 | Mask GoOut | Mask Dress |
|------|--------|------------|------------|
| 王冠 | 换整本 atlas `…_Crown`（脸图已含冠） | **显隐** `ArmorCrown` 物体；脸键仍 `Armor_NoHeadWear_*` | 脸键**写死** `Dress_Crown_*`（冠画在脸上）；**无** Headwear 分支 |
| 头盔 | 换 atlas `…_ArmorHead` | **显隐** `ArmorHead` | **无** |
| 无头饰 | atlas `…_NoHeadWear` | 盔/冠都关 | 仍用 Crown 脸键（无「无冠 Dress」） |
| Face 是否随头饰变前缀 | **是**（换图集） | **否**（零件叠加） | **否**（永远 Crown 前缀） |

`armorNone` 字段在脚本上存在，现网 `SetArmorHeadwearActive` **只动** Head/Crown，不显式开关 `armorNone`（无头饰时靠两者都关表现）。

头饰数据读取：`gsm.GetArchiveData<PlayerClothesData>()`；衣服套装读取：`PlayerDataComponentGM.GetClothesData()`——底层同为 Archive 的 `PlayerClothesData`，一般同源。

### 4.5 大立绘 vs 小头像真源

| 层 | 谁决定穿哪套 | 现网 / 拍板 |
|----|--------------|-------------|
| 场景大立绘 | **对话 Prefab** 嵌哪套 Painting | 跟台本摆放，可不读存档 |
| Mask 小头像 | Presenter ← **存档 Clothes + Headwear** | **✅ 2026-08-07 维持存档** |
| 旧 Portrait | Loader ← 存档 Clothes×Headwear | 跟衣柜（更细四套 atlas） |

**「室内/室外」技术映射**：小头像 = **`Clothes==Dress?` + Headwear 细分**，**≠**场景名、**≠**大立绘 Prefab。  
冲突时 Mask 听存档——**产品已接受**（勿当 bug）。

### 4.6 假说 H1～H5 判定

| ID | 假说 | 判定 | 说明 |
|----|------|------|------|
| H1 | Prefab 仍 `yaerUseGoOutOnly=true` | **不成立** | 序列化 **`yaerUseGoOutOnly: 0`**（0806 已改） |
| H2 | 真源是存档 Clothes，不是室内场景/大立绘 | **成立且已拍板维持** | 大立绘可不同源 |
| H3 | 只切衣服套，盔冠不同步或 Dress 无无冠 | **部分成立** | GoOut 盔冠**会** Sync（四态 #2～#4）；Dress 无无冠态 |
| H4 | 旧四套 → Mask 两套 Painting + Heads | **成立**；产品定为**同一套逻辑的实现形态** | 不是「机制残了」，是人偶+零件 |
| H5 | 引用未绑 / Find 失败 / 逻辑空转 | **基本不成立** | Find 兜底；有 `[MaskAvatar]` 日志 |

### 4.7 旧→新对照总表

| 能力 | 旧图集 | Mask 现网（跟存档 · 已拍板） | 缺口 / 备注 |
|------|--------|------------------------------|-------------|
| Dress↔外出 | Clothes 选 atlas | Dress→`YaerPainting`，否则 GoOut | 已对齐 §3.1 |
| 王冠 / 盔 / 无头饰 | 换 atlas | GoOut：`Heads` 显隐 | 已对齐 §3.1 #2～#4 |
| FaceType | atlas Sprite 裸名 | 带前缀 Face 键 | 规则不同；可接受 |
| 与大立绘一致 | 存档一致时易齐 | **可不一致** | **已接受**；方案 C 另案 |
| 历史列表 | Loader 四套 | 仍 Loader | 新表情图集可不补 |

### 4.8 方案比选（拍板结果）

| 方案 | 摘要 | 状态 |
|------|------|------|
| **A 跟存档** | Clothes×Headwear → 四态 Mask | **✅ 2026-08-07 确认维持**（0806 已施工） |
| C 镜像大立绘 | 跟台上 Painting 类型 | **❌ 本期不做** |
| 加细 Dress 头饰 / GoOut 换脸前缀 / 恢复 Portrait | 见 OPEN Q2～Q4 | ⏸ 另案 |

### 4.9 开放问题

| ID | 问题 | 决议 / 建议 | 状态 |
|----|------|-------------|------|
| Q1 | 小头像服装真源：存档 vs 镜像大立绘？ | **跟存档（方案 A）** | ✅ 已决议 2026-08-07 |
| Q2 | Dress 是否长期只有 Crown 脸键？ | 现状是；无冠另案 | ⏸ 待确认 |
| Q3 | GoOut 戴冠：只显隐 vs 改 Face 前缀？ | **现状显隐**（对齐 §3.1） | ✅ 按四态定稿；改前缀另案 |
| Q4 | 旧四套 atlas 是否还需与 Mask 一一对应？ | **字幕不必**（人偶+Heads 已覆盖四态） | ✅ 倾向不必；历史按需补 |

---

## 验收速查（机制已落地 · 回归用）

| 步骤 | 期望 |
|------|------|
| 新档默认 Dress+Crown | Mask → Dress，`Dress_Crown_*`；日志 `Yaer → Dress` |
| 换装 Clothes=Armor + NoHeadWear | GoOut；盔冠都关 |
| Armor + Crown | GoOut；只亮 `ArmorCrown` |
| Armor + ArmorHead | GoOut；只亮 `ArmorHead` |
| 室内对话但存档仍 Armor | Mask **仍 GoOut**（跟存档，非 bug） |
| 历史列表 | 仍可走四套图集路径 |
