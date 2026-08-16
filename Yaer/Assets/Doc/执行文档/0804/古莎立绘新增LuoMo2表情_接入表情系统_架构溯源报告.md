# 古莎立绘新增 LuoMo2 表情 · 接入表情系统 — 架构溯源报告

**文档版本**：v1.0（2026-08-04）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 图集 / CSV / 台本**）  
**范围**：古莎（Gusha / GuSha）对话表情链路中，把新增的 **LuoMo2** 接到可调用状态；雅儿 Happy / Mask 接线仅作对照，**本期不改雅儿、不施工 Mask 启用**  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0804/古莎立绘新增LuoMo2表情_接入表情系统_架构侦探提示词.md`
- 强对照：`Assets/Doc/执行文档/0803/古莎立绘新增LuoMo表情_接入表情系统_架构溯源报告.md`
- 对照：`Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md` §4.1
- 代码 / Prefab / 图集 / ArtRes 静态阅读

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**仅凭 Prefab 上的 `Faces/LuoMo2` 还不能调用：`DialogueFaceType` 里没有 `LuoMo2`（也没有 `LuoMo`），SayEx/CSV 选不了。场景大立绘最小闭环 = 在枚举末尾追加 `LuoMo2`（键名已与节点一致，`UpdateFace` 零改逻辑）；是否列入为难体 `spcFaces`、是否补小头像需拍板。附带：`LuoMo` 轮仍未落地（脸在、枚举无）——本期默认只做 LuoMo2，是否同轮顺带补 `LuoMo` 记入开放问题。**

档位结论：**只加枚举**（大立绘可调） / 可选 **枚举+spcFaces** / 要小头像再 **+图集**。不是「零代码」。

---

## ② 原因（生活类比 + 技术锚点）

### 生活类比

舞台人偶衣柜里又挂了一张写着 `LuoMo2` 的脸（绑的是「落寞2」图）；但对讲机频道表（`DialogueFaceType`）还没有这个号——导演喊不出，灯光就切不过去。上一轮 `LuoMo` 也是同一坑，且频道至今没开。

### 与雅儿 Happy / 古莎 LuoMo 对照（勿抄错键）

| | 雅儿 Happy | 古莎 LuoMo（0803，对照） | 古莎 LuoMo2（本期） |
|--|-----------|--------------------------|---------------------|
| 枚举原先 | **已有** `Happy` | **无**（仍未落地） | **无**（主卡点） |
| 大立绘 Faces 键 | `Armor_NoHeadWear_{faceType}` | **`faceType.ToString()`** | **同左** → `"LuoMo2"` |
| Prefab 节点 | `Armor_NoHeadWear_Happy` | `LuoMo`（有） | `LuoMo2`（有） |
| 最小代码 | 可零改 C#（仅资源） | 至少改枚举 | **至少改枚举** |

### LuoMo2 调用链（台本 → 大立绘）

```
SayEx / CSV FaceType = LuoMo2
  → 须 DialogueFaceType.LuoMo2 存在（当前不存在 → 选不了 / CSV 校验失败）
  → StatementNodeEx.FaceType → SubtitlesRequestInfoEx
  → DialogueActorEx.RefreshAvatar
       ├─ DialogueAvatarLoader → Avatar_Gusha.GetSprite("LuoMo2")
       │     → 现无 Avatar/LuoMo2.png → 小头像隐藏（与大立绘不同源）
       └─ OnRefreshAvatarEvent
            → GuShaPainting（基类：UpdateFace(faceType.ToString())）
            → 键 "LuoMo2" → 激活 Faces/LuoMo2
            → 再按 spcFaces 切 clothes_normal / clothes_other
                 （现列表仅 Awkward/Cry/Daze/Sad，不含 LuoMo2）
```

### Prefab 核实（`GushaPainting.prefab`）

| 项 | LuoMo2 | LuoMo（对照状态行） |
|----|--------|---------------------|
| 路径 | `Assets/Prefabs/DialougeProtrait/GushaPainting.prefab` | 同 Prefab |
| Faces 节点 | **有**（RootOrder 14，列表底部） | **有**（RootOrder 13） |
| Image 绑图 | **有** → `…/古莎游戏中立绘/落寞2.png`（guid `5476de82…`） | **有** → `…/古莎游戏中立绘/落寞.png` |
| Rect | Pos≈`(-70.7, 524.3)`，SizeDelta≈`106.8×68.3`，Scale=`1.7` | **同参**（与 Awkward/Happy 等脸层一致） |
| 默认 Active | `1`（Awake 会按 defaultFace 关掉非默认脸） | 同 |
| 0803 Rect 备注 | 现网已与同级脸对齐；0803「≈205×110 vs Happy 全尺寸」口径对现 Prefab **已过时** | 同；验收仍建议肉眼看脸是否完整 |

### 枚举核实（`DialogueFaceType.cs`）

| 成员 | 现状 |
|------|------|
| `LuoMo2` | **无**（末项仍为 `Scared`） |
| `LuoMo` | **无** → **LuoMo 轮未落地**（仍缺枚举；节点可调链路未通） |

### 小头像

| 项 | 现状 |
|----|------|
| 图集 | `Assets/GameRes/Atlas/Avatar/Avatar_Gusha.spriteatlas`（路径由 `DialogueAvatarPathHelper.GetPath("Gusha")`） |
| Avatar 源目录 | `…/Gusha/Avatar/`：Angry…VerySurprised 等 **13** 张；**无 `LuoMo2.png`，亦无 `LuoMo.png`** |
| 缺图行为 | `GetSprite`→null → Portrait/历史 **隐藏** |
| 与大立绘 | **不同源**；大立绘有节点 ≠ 图集自动有 |

### Mask

`NormalDialogueNewPanel` 内嵌的 `GushaPainting` 实例会继承母体 `LuoMo2` 节点，但 Mask **未接线 FaceType**（见既有 Mask / 0803 报告）→ **本期与台本调用无关**。

### 文档

0601 §4.1 古莎表：**无** `LuoMo`、**无** `LuoMo2`（施工建议补表，非代码阻塞）。

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板（写入 OPEN · 2026-08-04 节）

1. **LuoMo2 中文含义 / 策划名**？与 LuoMo「落寞」是变体还是新表情？（源图 `落寞2.png` 暗示变体）  
2. **是否切为难体** `clothes_other`？（现 spcFaces 未含；**默认对齐 LuoMo：不切**，走 `clothes_normal`）  
3. **小头像是否本期必做**？（建议：大立绘先闭环；另补 `LuoMo2.png` + Pack）  
4. **Mask**：本期不做  
5. **Rect/绑图**：现与同级脸一致且已绑 `落寞2.png`；是否还需美术校正？（验收肉眼）  
6. **是否同轮顺带补** 仍缺失的 `DialogueFaceType.LuoMo`？（**默认不强制**；若补：先 `LuoMo` 再 `LuoMo2`，均追加末尾）

### 验收清单（枚举加上之后）

1. NodeCanvas 古莎 Actor 的 SayEx 下拉出现 **`LuoMo2`**。  
2. DialogDebug / 含古莎对话：选 LuoMo2 → Hierarchy `GushaPainting/Faces/LuoMo2` 亮，其它脸关。  
3. 看 `Clothes` vs `Clothes_other`：未进 spcFaces → 应亮 **正常衣**；若拍板进了 → 应亮 **为难衣**。  
4. 肉眼：落寞2 脸是否完整、对齐。  
5. 小头像（若未补图）：预期隐藏；补图后应显示。  
6. （若同轮补了 LuoMo）对 `LuoMo` 重复 1–5。

---

## ④ 给程序看的补充

### 4.1 缺口表

| 环节 | 现状（有/无/未知） | 期望 | 是否必须改代码 | 备注 |
|------|-------------------|------|----------------|------|
| `DialogueFaceType.LuoMo2` | **无** | SayEx/CSV 可选 | **是**（追加枚举末尾） | 本期主卡点 |
| `DialogueFaceType.LuoMo`（对照） | **无**（轮未落地） | 上一轮是否落地 | 否（非本期必做） | 可选顺带 |
| `GushaPainting` Faces/`LuoMo2` | **有**，已绑 `落寞2.png` | 键=`LuoMo2` | 否（逻辑） | Rect 与同级脸一致 |
| `GuShaPainting.spcFaces` | 未含 LuoMo2 | 是否为难体 | **待拍板** | 默认可对齐 LuoMo=否 |
| `Avatar_Gusha` + `LuoMo2.png` | **无** | 小头像同步 | 否（补资源+Pack） | 缺图隐藏 |
| 历史头像 | 同源 Loader | 同小头像 | 否 | |
| Mask 内 Gusha | 有实例、未接线 | 跟脸 | 另案 | 本期不做 |
| 0601 §4.1 手册 | 无 LuoMo2（亦无 LuoMo） | 补表 | 否（文档） | 施工建议 |

### 4.2 枚举追加与序列化

- **追加在 `Scared` 之后（末尾）**相对安全：Unity / `BBParameter` 常按底层 int 存；插到中间会错位旧节点表情。  
- 名称必须为 **`LuoMo2`**，与 Faces 子物体名、CSV 字符串一致（**禁止**改成中文节点名）。  
- 若拍板同轮补 `LuoMo`：**顺序建议** `…, Scared, LuoMo, LuoMo2`（先补缺失、再追加本期）；二者此前均不存在，追加不会改写旧成员 int。  
- **勿**只追加 `LuoMo2` 后再回头把 `LuoMo` 插到中间（会改变 `LuoMo2` 的 int）。若先只做 LuoMo2、以后再补 LuoMo：只能继续 **追加在 LuoMo2 之后**（顺序变成 `LuoMo2` 再 `LuoMo`，与命名序号不一致但序列化安全——故更建议同轮按 `LuoMo`→`LuoMo2` 一次加齐）。  
- `DialogueCsvParser`：`Enum.TryParse` 在枚举存在后即可通过；`DialogueFaceTypeCsvDefaults` 无需为 LuoMo2 改默认（古莎空列仍 Normal）。

### 4.3 施工员最小改动建议（只建议，不施工）

#### 方案 A — 仅场景大立绘（最小闭环）

| 步骤 | 文件 / 操作 | 改代码？ |
|------|-------------|----------|
| A1 | `DialogueFaceType.cs` 末尾追加 `LuoMo2` | **是** |
| A2 | （可选）验收时校正 `Faces/LuoMo2` 视觉；确认绑图 `落寞2.png` | Prefab/美术 |
| A3 | 台本 SayEx / CSV 填 `LuoMo2` 验收 | 否 |
| A4 | （可选）0601 §4.1 补一行（及若仍缺则顺带注明 LuoMo） | 文档 |

**不改**：`GuShaPainting.cs`（若拍板不切为难体）、Loader、雅儿、Mask。

#### 方案 A′ — 大立绘 + 为难体

在 A 基础上：

| 步骤 | 操作 |
|------|------|
| A′1 | `GuShaPainting.spcFaces` 增加 `DialogueFaceType.LuoMo2` |

#### 方案 B — 大立绘 + 字幕条/历史小头像

在 A（或 A′）上：

| 步骤 | 操作 |
|------|------|
| B1 | `…/Gusha/Avatar/LuoMo2.png`（名=枚举） |
| B2 | 重 Pack `Avatar_Gusha.spriteatlas` |

仍不必改 Loader。

#### 可选顺带 — LuoMo 枚举

| 步骤 | 操作 | 默认 |
|------|------|------|
| O1 | 同文件末尾先加 `LuoMo`、再加 `LuoMo2` | **不强制**；未拍板则只做 LuoMo2 |
| O2 | 小头像 `LuoMo.png` 仍另案（与 0803 OPEN 一致） | 否 |

#### spcFaces 两种分支（待拍板）

| 决议 | 改法 | 运行时 |
|------|------|--------|
| **不切为难**（默认建议，对齐 LuoMo） | 不动 `spcFaces` | `clothes_normal` 开 |
| **要切为难** | 列表加入 `LuoMo2` | `clothes_other` 开 |

### 4.4 相关文件清单

| 类别 | 路径 |
|------|------|
| 枚举（必改） | `Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs` |
| 古莎立绘脚本 | `…/Painting/GuShaPainting.cs` |
| 立绘基类 | `…/Painting/StoryFormPainting.cs` |
| Prefab | `Assets/Prefabs/DialougeProtrait/GushaPainting.prefab` |
| 当前绑图 | `…/Gusha/DialogueProtrait/古莎游戏中立绘/落寞2.png` |
| 小头像图集 | `Assets/GameRes/Atlas/Avatar/Avatar_Gusha.spriteatlas` |
| CSV 工具 | `DialogueCsvParser` / `DialogueFaceTypeCsvDefaults`（仅枚举齐即可） |
| 手册 | `…/0601/对话立绘表情与图片名称对照_执行说明.md` §4.1 |

### 4.5 开放问题（已追加 OPEN_QUESTIONS.md · 勿覆盖 0803 LuoMo 节）

| ID | 问题 | 施工默认建议 |
|----|------|--------------|
| Q1 | LuoMo2 中文含义？与 LuoMo「落寞」关系？ | 变体「落寞2」（对齐 `落寞2.png`） |
| Q2 | 是否纳入 `spcFaces` 切为难体？ | **否**（对齐 LuoMo / Happy/Smile） |
| Q3 | 小头像是否本期必做？ | **否**；先方案 A |
| Q4 | Mask 是否本期跟脸？ | **否** |
| Q5 | Rect/绑图是否还需校正？ | 现与同级脸一致；验收肉眼，必要时再改 Prefab |
| Q6 | 是否同轮顺带补 `DialogueFaceType.LuoMo`？ | **默认不强制**；若补则 `LuoMo`→`LuoMo2` 同末尾追加 |

---

## 施工员下一轮最小化文件清单（建议）

**仅大立绘（默认）**  
1. `DialogueFaceType.cs` — 末尾 `LuoMo2`（可选同轮 `LuoMo`）  

**大立绘 + 为难体**  
2. `GuShaPainting.cs` — `spcFaces`  

**大立绘 + 小头像**  
3. `…/Gusha/Avatar/LuoMo2.png` + Pack `Avatar_Gusha.spriteatlas`  

**文档（可选）**  
4. 0601 §4.1 补行  
