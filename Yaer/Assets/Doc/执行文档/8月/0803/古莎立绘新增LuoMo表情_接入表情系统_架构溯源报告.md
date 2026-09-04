# 古莎立绘新增 LuoMo 表情 · 接入表情系统 — 架构溯源报告

**文档版本**：v1.0（2026-08-03）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 图集 / CSV / 台本**）  
**范围**：古莎（Gusha / GuSha）对话表情链路中，把新增的 **LuoMo** 接到可调用状态；雅儿 Happy / Mask 接线仅作对照，**本期不改雅儿、不施工 Mask 启用**  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0803/古莎立绘新增LuoMo表情_接入表情系统_架构侦探提示词.md`
- 对照：`Assets/Doc/执行文档/0803/雅儿立绘新增Happy表情_接入表情系统_架构溯源报告.md`
- 对照：`Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md` §4.1
- 代码 / Prefab / 图集 / ArtRes 静态阅读

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**仅凭 Prefab 上的 `Faces/LuoMo` 还不能调用：枚举 `DialogueFaceType` 里没有 `LuoMo`，SayEx/CSV 选不了。场景大立绘最小闭环 = 在枚举末尾追加 `LuoMo`（键名已与节点一致，`UpdateFace` 零改逻辑）；是否把 LuoMo 列入为难体 `spcFaces`、是否补小头像 `LuoMo.png` 需拍板。另：LuoMo 节点当前 SizeDelta≈205×110（其它脸如 Happy≈573×1517），绑的是 `DialogueProtrait/落寞.png`，验收要先看脸是否完整。**

档位结论：**只加枚举**（大立绘可调） / 可选 **枚举+spcFaces** / 要小头像再 **+图集**。不是「零代码」。

---

## ② 原因（生活类比 + 技术锚点）

### 生活类比

舞台人偶（大立绘）衣柜里已经挂好了写着 `LuoMo` 的脸；但对讲机下拉菜单（`DialogueFaceType`）还没有这个频道——导演喊不出「落寞」，灯光师也就切不过去。雅儿的 Happy 是「频道早就有、只差挂脸」；古莎 LuoMo 是「脸挂了、频道还没开」。

### 与雅儿 Happy 对照一行（勿抄错键）

| | 雅儿 Happy | 古莎 LuoMo |
|--|-----------|------------|
| 枚举原先 | **已有** `Happy` | **无** `LuoMo`（卡点） |
| 大立绘 Faces 键 | `Armor_NoHeadWear_{faceType}` | **`faceType.ToString()`**（裸名，如 `LuoMo`） |
| Prefab 节点 | `Armor_NoHeadWear_Happy` | `LuoMo`（已核实存在） |
| 最小代码 | 可零改 C#（仅资源） | **至少改枚举** |

### 调用链（台本 → 大立绘）

```
SayEx / CSV FaceType = LuoMo
  → 须 DialogueFaceType.LuoMo 存在（当前不存在 → 选不了 / CSV 校验失败）
  → StatementNodeEx → SubtitlesRequestInfoEx
  → DialogueActorEx.RefreshAvatar
       ├─ Loader → Avatar_Gusha.GetSprite("LuoMo")  → 现无图 → 小头像隐藏
       └─ OnRefreshAvatarEvent
            → GuShaPainting（基类订阅：UpdateFace(faceType.ToString())）
            → 键 "LuoMo" → 激活 Faces/LuoMo
            → 再按 spcFaces 切 clothes_normal / clothes_other
```

### Prefab 核实（`GushaPainting.prefab`）

| 项 | 结果 |
|----|------|
| 路径 | `Assets/Prefabs/DialougeProtrait/GushaPainting.prefab` |
| `Faces/LuoMo` | **有**（与 Angry/Smile/Happy 等同级） |
| Image 绑图 | **有** → `Assets/ArtRes/UI/Story/DialogueForm/Gusha/DialogueProtrait/落寞.png` |
| Rect | Pos≈`(-68.6, 524.4)`，SizeDelta≈`205×110`；对照 Happy：`0,0` / `573×1517` → **布局异常风险** |
| Prefab 默认 Active | LuoMo=`1`（Awake 会按 defaultFace 关掉非默认脸） |
| `clothes_normal` / `clothes_other` | 已序列化；spcFaces = Awkward/Cry/Daze/Sad（**不含 LuoMo**） |

### 小头像

| 项 | 现状 |
|----|------|
| 图集 | `Assets/GameRes/Atlas/Avatar/Avatar_Gusha.spriteatlas` |
| Avatar 源目录 | `…/Gusha/Avatar/`：13 张（含 Happy），**无 `LuoMo.png`** |
| 缺图行为 | `GetSprite`→null → Portrait/历史 **隐藏** |
| 与大立绘 | **不同源**；大立绘有节点 ≠ 图集自动有 |

### Mask

`NormalDialogueNewPanel` 内嵌的 `GushaPainting` 实例会继承母体 `LuoMo` 节点，但 Mask **未接线 FaceType**（见 0803 Mask 报告）→ **本期与台本调用无关**。

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板（写入 OPEN）

1. **LuoMo 中文含义**是否就是「落寞」？（节点拼音 / 源图文件名已暗示）  
2. **是否切为难体** `clothes_other`？（现 spcFaces 未含；未说明则默认**不切**，与 Smile/Happy 同走 `clothes_normal`）  
3. **小头像是否本期必做**？（建议：大立绘先闭环；小头像另补 `LuoMo.png` + Pack）  
4. **Mask**：本期不做（已与其它 OPEN 对齐）  
5. **LuoMo 节点尺寸**：是否按 Happy 对齐全尺寸 Rect？（建议做，属 Prefab/美术，可与加枚举同轮或紧随）

### 验收清单（枚举加上之后）

1. NodeCanvas 古莎 Actor 的 SayEx 下拉出现 **`LuoMo`**。  
2. DialogDebug / 含古莎对话：选 LuoMo → Hierarchy `GushaPainting/Faces/LuoMo` 亮，其它脸关。  
3. 看 `Clothes` vs `Clothes_other`：若未进 spcFaces → 应亮 **正常衣**；若拍板进了 → 应亮 **为难衣**。  
4. 肉眼：落寞脸是否完整（当前小 Rect 风险）。  
5. 小头像（若未补图）：预期隐藏；补图后应显示。

---

## ④ 给程序看的补充

### 4.1 缺口表

| 环节 | 现状 | 期望 | 是否必须改代码 | 备注 |
|------|------|------|----------------|------|
| `DialogueFaceType.LuoMo` | **无** | SayEx/CSV 可选 | **是**（追加枚举末尾） | 本期主卡点 |
| `GushaPainting` Faces 节点 | **有**，已绑 `落寞.png` | 键=`LuoMo` | 否（逻辑）；建议校正 Rect | SizeDelta 异常 |
| `GuShaPainting.spcFaces` | 未含 LuoMo | 是否为难体 | **待拍板** | 两种改法见下 |
| `Avatar_Gusha` + `LuoMo.png` | **无** | 小头像同步 | 否（补资源+Pack） | 缺图隐藏 |
| 历史头像 | 同源 Loader | 同小头像 | 否 | |
| Mask 内 Gusha | 有实例、未接线 | 跟脸 | 另案 | 本期不做 |
| 0601 §4.1 手册 | 无 LuoMo | 补表 | 否（文档） | 施工建议 |

### 4.2 枚举追加与序列化

- **追加在 `Scared` 之后（末尾）**相对安全：Unity/`BBParameter` 常按底层 int 存；插到中间会错位旧节点表情。  
- 名称必须为 **`LuoMo`**，与 Faces 子物体名、CSV 字符串一致（禁止改成中文节点名）。  
- `DialogueCsvParser`：`Enum.TryParse` 在枚举存在后即可通过；`DialogueFaceTypeCsvDefaults` 无需为 LuoMo 改默认（空列仍 Normal）。

### 4.3 施工员最小改动建议（只建议，不施工）

#### 方案 A — 仅场景大立绘（最小闭环）

| 步骤 | 文件 / 操作 | 改代码？ |
|------|-------------|----------|
| A1 | `DialogueFaceType.cs` 末尾追加 `LuoMo` | **是** |
| A2 | （建议）校正 `Faces/LuoMo` Rect 对齐其它脸；确认绑图正确 | Prefab/美术 |
| A3 | 台本 SayEx / CSV 填 `LuoMo` 验收 | 否 |
| A4 | （可选）0601 §4.1 / 全表补一行「落寞」 | 文档 |

**不改**：`GuShaPainting.cs`（若拍板不切为难体）、Loader、雅儿、Mask。

#### 方案 A′ — 大立绘 + 为难体

在 A 基础上：

| 步骤 | 操作 |
|------|------|
| A′1 | `GuShaPainting.spcFaces` 增加 `DialogueFaceType.LuoMo` |

#### 方案 B — 大立绘 + 字幕条/历史小头像

在 A（或 A′）上：

| 步骤 | 操作 |
|------|------|
| B1 | `…/Gusha/Avatar/LuoMo.png`（名=枚举） |
| B2 | 重 Pack `Avatar_Gusha.spriteatlas` |

仍不必改 Loader。

#### spcFaces 两种分支（待拍板）

| 决议 | 改法 | 运行时 |
|------|------|--------|
| **不切为难**（默认建议） | 不动 `spcFaces` | `clothes_normal` 开 |
| **要切为难** | 列表加入 `LuoMo` | `clothes_other` 开 |

### 4.4 相关文件清单

| 类别 | 路径 |
|------|------|
| 枚举（必改） | `Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs` |
| 古莎立绘脚本 | `…/Painting/GuShaPainting.cs` |
| 立绘基类 | `…/Painting/StoryFormPainting.cs` |
| Prefab | `Assets/Prefabs/DialougeProtrait/GushaPainting.prefab` |
| 当前绑图 | `…/Gusha/DialogueProtrait/落寞.png` |
| 小头像图集 | `Assets/GameRes/Atlas/Avatar/Avatar_Gusha.spriteatlas` |
| CSV 工具 | `DialogueCsvParser` / `DialogueFaceTypeCsvDefaults`（仅枚举齐即可） |

### 4.5 开放问题（已追加 OPEN_QUESTIONS.md）

| ID | 问题 | 施工默认建议 |
|----|------|--------------|
| Q1 | LuoMo 中文是否为「落寞」？ | 是（对齐源图文件名） |
| Q2 | 是否纳入 `spcFaces` 切为难体？ | **否**（与 Happy/Smile 同正常衣） |
| Q3 | 小头像是否本期必做？ | **否**；先方案 A |
| Q4 | Mask 是否本期跟脸？ | **否** |
| Q5 | LuoMo 小 Rect 是否必须对齐全尺寸？ | **是**（建议与加枚举同轮验收） |
