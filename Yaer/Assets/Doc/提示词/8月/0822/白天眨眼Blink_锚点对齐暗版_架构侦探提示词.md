# Cursor Agent Prompt · 白天眨眼 Blink 序列帧锚点对齐暗版（脚底）

> **角色**：先【架构侦探】核对画布与拷贝规则，确认后【施工员】只改白天 `Blink/` 目录的 `.png.meta`  
> **日期**：2026-08-22  
> **前置**：村民家室内 `Bink_DayLight` 动画替换已完成（见 `Assets/Doc/执行文档/0822/村民家室内_Bink_DayLight_架构溯源报告.md`）。  
> **现象**：新白天眨眼素材 `Assets/ArtRes/Animation/Yaer/Home/Blink/` 导入后锚点在图心（`alignment=0`，`pivot=0.5,0.5`），播起来人会跳；暗版眨眼（旧 `Bink` 用的参考帧）已经按脚底调过自定义锚点（`alignment=9`）。  
> **本阶段侦探**：只读、不改 meta / png / Clip。核对对应关系 + 画布是否同尺寸 + 写出逐帧拷贝/换算表。  
> **禁止**：改 png 像素、改 guid、改 AnimationClip、改 Animator、改暗版参考目录本身。

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（开发者原话）

> 动画成功替换完成了，接下来的需求是把这些素材的锚点调整一下，对比之前的暗版本素材。

「这些素材」= 新白天眨眼目录 `Assets/ArtRes/Animation/Yaer/Home/Blink/` 下 **9 张**（每套 3 帧：开→闭→开）。  
**本期只改这三套铠甲换装眨眼**；裙子 `Dress/Bink_DayLight` 若仍引用 `Dress/Idle/Bink/01~03`（暗版同图），**不在本期**。

### 参考 ↔ 目标对应表（按帧序 1→2→3）

| 头饰 | 暗版参考（只读，已有脚底锚点） | 白天目标（中心锚点，只改 meta） |
|------|-------------------------------|--------------------------------|
| 全无 None | `…/Armor/战斗服皇冠前摇/前摇无{1,2,3}.png` | `…/Blink/None{1,2,3}.png` |
| 皇冠 Crown | `…/Armor/战斗服皇冠前摇/皇冠前摇待机{1,2,3}.png` | `…/Blink/Crown{1,2,3}.png` |
| 护头 Armor | `…/Armor/战斗服皇冠前摇/护头前摇待机{1,2,3}.png` | `…/Blink/Armor.png`（帧1）、`Armor2.png`（帧2）、`Armor3.png`（帧3） |

**注意文件名**：护头白天帧 **不是** `Armor1.png`，而是 `Armor` / `Armor2` / `Armor3`。

暗版 Clip 引用关系（核实锚点来源是否正确）：

| 暗版 Clip | 引用的暗版帧图目录 |
|-----------|-------------------|
| `ArmorNoneBink.anim` | `战斗服皇冠前摇/前摇无1~3` |
| `ArmorCrownBink.anim` | `战斗服皇冠前摇/皇冠前摇待机1~3` |
| `ArmorBink.anim` | `战斗服皇冠前摇/护头前摇待机1~3` |
| `Bink.anim`（裙子） | `Dress/Idle/Bink/01~03`（本期不改） |

白天 Clip 引用（改 meta 后应自动生效，**不要改 anim**）：

- `ArmorNoneBink_DayLight.anim` → `Blink/None1~3`
- `ArmorCrownBink_DayLight.anim` → `Blink/Crown1~3`
- `ArmorBink_DayLight.anim` → `Blink/Armor / Armor2 / Armor3`

### 生活类比

旧暗版眨眼每张贴纸已经在脚底钉了图钉，人闭眼时脚不滑。新白天贴纸还把图钉钉在肚脐，一眨人就上下跳。要把白天每张图的图钉，钉到和暗版**同一帧、同一套铠甲**的脚的位置。

### 预扫（助手已读 meta，侦探须抽查像素尺寸）

**元数据**

- 白天 `Blink/` 9 张：全部 `alignment: 0`，`spritePivot: {x: 0.5, y: 0.5}`，`spritePixelsToUnits: 100`。  
- 暗版参考 9 张：全部 `alignment: 9`（Custom），`y` 约 `0.028~0.032`（贴脚底）。示例：
  - `前摇无1`：`{x: 0.5853096, y: 0.02873687}`
  - `皇冠前摇待机1`：`{x: 0.58544135, y: 0.028156212}`
  - `护头前摇待机1`：`{x: 0.56048924, y: 0.029769521}`

**画布宽高**

- 侦探须逐套逐帧量 `None1↔前摇无1`、`Crown2↔皇冠前摇待机2`、`Armor3↔护头前摇待机3` 等，填「同尺寸 / 同宽不同高 / 都不同」表。  
- 若**同尺寸** → 可 **逐帧原样拷贝** `alignment` + `spritePivot`。  
- 若**同宽不同高**（多出来的像素在头顶）→ 用 0818 公式（侦探确认后施工）：
  - `destPivot.x = srcPivot.x`
  - `destPivot.y = srcPivot.y * srcHeight / destHeight`
  - `alignment` 仍写 `9`

**spriteBorder**

- 裙子暗版 `Dress/Idle/Bink/01` 有 `spriteBorder`（`z:175`）；铠甲暗版前摇帧 **border 为 0**。  
- 白天 `Blink/` 当前 border 全 0 → **默认不要从裙子暗版拷 border**；侦探若发现护头/皇冠需 border 再写入 OPEN。

### 严禁的施工方向

1. 改暗版参考目录的 meta（`战斗服皇冠前摇/`、`Dress/Idle/Bink/`）。  
2. 改 `.anim` / Animator / `VillageHomeDayLightAnimApplier`。  
3. 改 png 像素或 guid。  
4. 把 `Armor.png` 误当成 `Armor1.png` 去对参考帧。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0822/村民家室内_Bink_DayLight_架构溯源报告.md
@Assets/ArtRes/Animation/Yaer/Home/Blink
@Assets/ArtRes/Animation/Yaer/Home/Armor/战斗服皇冠前摇
@Assets/ArtRes/Animation/Yaer/Home/Dress/Idle/Bink
@Assets/Animation/Object/Yaer/Home/None/ArmorNoneBink_DayLight.anim
@Assets/Animation/Object/Yaer/Home/Crown/ArmorCrownBink_DayLight.anim
@Assets/Animation/Object/Yaer/Home/Armor/ArmorBink_DayLight.anim
@Assets/Animation/Object/Yaer/Home/None/ArmorNoneBink.anim
@Assets/Animation/Object/Yaer/Home/Crown/ArmorCrownBink.anim
@Assets/Animation/Object/Yaer/Home/Armor/ArmorBink.anim
@Assets/Doc/执行文档/0818/白天待机走路_按战斗服锚点对齐_架构溯源报告.md

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止修改任何 png / meta / Clip / Animator。只读 + 写报告。

---

## 背景（策划白话）

村民家白天眨眼动画已接上，但新素材锚点在图心，播起来人会飘。暗版眨眼（旧 Bink 用的那套前摇帧）脚底锚点已经调好。要把 `Blink/` 下 9 张白天帧的锚点，按帧序对齐到对应暗版参考帧的脚。

---

## 侦探任务清单

1. **结论一句话**：能否按「同尺寸拷贝 pivot / 同宽不同高按距底像素换算」施工。
2. **核实对应表**（9 行）：白天文件 → 暗版参考文件 → 帧序（开/闭/开）→ Clip 是否已引用该 guid。
3. **画布抽查**：至少 `None1`、`Crown2`（闭眼帧）、`Armor3`；对照 Sprite 窗口里参考图 pivot 十字是否在脚。
4. **逐帧规则表**：目标文件 → 参考文件 → 宽×高是否相同 → 写入 `alignment=9` 以及 pivot 精确值（或公式）。
5. **与 Idle_DayLight 一致性**：白天眨眼锚点是否与同套 `*Idle_DayLight` 待机脚底大致一致（可选抽查 1 帧，避免眨完切 Idle 脚跳）。
6. **裙子范围**：确认 `Bink_DayLight.anim`（Dress）是否仍用 `Dress/Idle/Bink`，若是则写明「本期不改裙子」。
7. **禁止**：改参考目录、改 guid、改 Clip。
8. **开放问题**：仅技术例外（某帧宽也不同、脚不在底边、需拷 border）写入 `OPEN_QUESTIONS.md` 新节「白天 Blink 锚点对齐 · 2026-08-22」。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/白天眨眼Blink_锚点对齐暗版_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话  
② 原因（生活类比：新贴纸图钉在肚脐，要对齐暗版脚底）  
③ 用户需要做什么（认对应表 + 施工后进村民家眨眼看脚滑不滑）  
④ 给程序看的补充：9 行逐帧表、pivot 拷贝/公式、与 0818 Idle 锚点施工的一致性说明、禁止项

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告确认公式后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/白天眨眼Blink_锚点对齐暗版_架构溯源报告.md
@Assets/ArtRes/Animation/Yaer/Home/Blink

你现在是【施工员】。按溯源报告逐帧改 **Blink/** 目录下 9 张图的 `.png.meta`：
- 只改 `alignment`（0→9）和 `spritePivot`；
- 同尺寸：拷贝对应暗版参考帧 pivot；
- 同宽不同高：按报告公式（距底像素不变）；
- 保留 guid、spritePixelsToUnits、png 文件名；
- 不改 `战斗服皇冠前摇/`、`Dress/Idle/Bink/` 参考、不改 anim/controller、不拷 spriteBorder（除非报告明确要求）。

改完列出对照表（文件、旧 pivot、新 pivot、参考帧）。

验收：
- Unity Sprite 编辑器里，白天帧绿色锚点十字应落在脚底，与暗版同帧一致；
- 进村民家室内 → 站定触发眨眼 → 闭眼睁眼时脚不上下跳；
- 换 Crown / ArmorHead / None 各测一次；
- 眨完切 `Idle_DayLight` 时脚位不突然跳（若有偏差记入验收备注）。
```
