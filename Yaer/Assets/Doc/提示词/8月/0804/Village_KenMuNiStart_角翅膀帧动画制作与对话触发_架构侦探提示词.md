# Cursor Agent Prompt · Village_KenMuNiStart：角/翅膀帧动画制作 + 对白触发方式选型

> **角色**：【架构侦探】只溯源、不改代码 / Prefab / CSV / 动画资源  
> **日期**：2026-08-04  
> **范围**：`Village_KenMuNiStart` 内 `Anim_Yaer` / `Anim_Gusha` 的**帧动画怎么做**，以及台本两句动作戏如何接到 NodeCanvas；重点回答「CSV 标动画再自动生成」vs「手插节点」哪种更贴现网。  
> **本阶段**：只读扫描 + 写溯源报告，**不施工、不做 AnimationClip、不改 Unity 资产**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者白话需求（已定意图，细节侦探可纠偏）

**第一步 · 做动画（资源层）**

1. 对话 Prefab `Village_KenMuNiStart` 下已挂：
   - `Anim_Yaer` → 子物体 `Y1`～`Y5`（各带 UI `Image` + 不同 Sprite，当前全 Active）
   - `Anim_Gusha` → 子物体 `G1`～`G5`（同上）
2. **动画还没做**；子物体就是帧素材。
3. 期望做成可播放的帧动画后，**运行时只保留一个子物体**（把多帧合进 Clip / 换图，而不是运行时靠五个子物体轮流 SetActive——除非侦探证明现网另有惯例更合适）。

**第二步 · 接到对白（逻辑层）**

台本 CSV：`Assets/Dialog/Village_村内雅古开场对白台本.csv`  
对应对话 Prefab：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`

| CSV ID | Speaker | Text（动作戏，目前仍是 Dialogue 行） | 期望动画 |
|--------|---------|--------------------------------------|----------|
| **9** | 古 | 古莎卷了卷角 | 播 `Anim_Gusha`（卷角） |
| **17** | 雅 | 雅尔呼扇呼扇头上的一对小翅膀。 | 播 `Anim_Yaer`（翅膀） |

开发者在问：**比较好的方式是什么？**

- **方案 A**：在 CSV 里标注动画类型 / 动画名，生成 NodeCanvas 时自动补 Action 节点  
- **方案 B**：对白仍走 CSV 导入；这两处**手动**在图里插「播动画」节点  

提示词助手预判（可证伪）：

| 点 | 预判 |
|----|------|
| CSV 导入现状 | `Type` 仅 `Dialogue` / `Choice`；`Extra` 主给 Choice；文档写明「旁白 Action 列」属**阶段 3 未做** |
| 两处触发 | 全项目仅此两句、且要绑 Prefab 内具体 `Anim_*` 引用 → **首版手插成本更低、风险更小** |
| CSV 扩展 | 若全村大量动作戏才值得扩展导入器；本期 2 处 → 扩展性价比低 |
| 动画本体 | UI Image 换 Sprite 的 AnimationClip（或 Animator）挂在保留的那一个子物体上，最贴现有层级 |

### 已有文档 / 资产（须对拍，防过期）

- `Assets/Doc/技术文档/CSV转NodeCanvas对话树导入工具_开发文档.md`（§1.3 非目标 / §4 CSV 格式 / 阶段 3）
- `Assets/Doc/执行文档/5月/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md`
- CSV：`Assets/Dialog/Village_村内雅古开场对白台本.csv`
- Prefab：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`（根下已有 `Anim_Yaer`、`Anim_Gusha`）
- 相关 ActionTask 线索：`AnimationEventRegisterTaskAction`（**等待**动画事件，不是播 Clip）；`CanvasGroupAlphaActionTask`；NodeCanvas 自带 / 项目自写「Play Animator / 换 Sprite」类任务——侦探须盘点有无现成「播动画」节点

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/CSV转NodeCanvas对话树导入工具_开发文档.md
@Assets/Doc/执行文档/5月/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md
@Assets/Dialog/Village_村内雅古开场对白台本.csv
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、AnimationClip、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 进村开场对话 Prefab 里加了 `Anim_Yaer`、`Anim_Gusha`，下面各有 5 帧 UI Image 素材，动画还没做。
2. 希望做成真正能播的帧动画，做完后**每个 Anim 容器 ideally 只留一个子物体**（多帧进 Clip）。
3. 两句台词要对上动画：
   - 「古莎卷了卷角」（CSV ID 9）→ 古莎角动画
   - 「雅尔呼扇呼扇头上的一对小翅膀。」（CSV ID 17）→ 雅尔翅膀动画
4. 要选型：**CSV 标类型自动生成节点** vs **手改 NodeCanvas 插播动画节点**，并说明理由与后续若动作戏变多时的升级路径。

---

## 必读 / 优先扫描线索

### A. Prefab 现状（资源层）
- `Village_KenMuNiStart` 层级：`Anim_Yaer/Y1..Y5`、`Anim_Gusha/G1..G5`
- 组件类型：确认是 UGUI `Image`（非 SpriteRenderer）；有无已有 `Animator` / `Animation` / 自定义帧切脚本
- 默认是否应隐藏（`Anim_*` 或单图初始 Inactive / alpha=0）——现网全 Active 叠在一起的风险
- 立绘 `Yaer` / `Gusha` 与 `Anim_*` 的空间关系：动画是盖在立绘上的局部特写，还是整身替换？

### B. 工程内「对话中播动画」有无先例
扫描 NodeCanvas 图 / ActionTask：
- 是否已有「Play Animator」「Play AnimationClip」「换 Image.sprite 序列」类任务
- `AnimationEventRegisterTaskAction` 的用法样例（等事件再往下走对白）——能否复用于「播完再下一句」
- 其它 Dialogue Prefab 是否在对白中间插过 ActionNode 播场景/UI 动画

### C. CSV → Graph 导入器能力边界（选型核心）
- `DialogueCsvParser` / `DialogueCsvGraphBuilder`：`Type` 支持哪些值；`Extra` / `FaceType` / 空列用途
- 文档「阶段 3」未做项：旁白 Action、动画列等——现状是否仍成立
- 若强行走方案 A，最小要改什么（新 Type？复用 Extra？新列？）以及**对象引用**（Anim_Yaer 在 Prefab 上，Generated `.asset` 如何绑到实例）——这是自动生成最难的一点，必须写清

### D. 图内现网拓扑（触发挂点）
- `Village_KenMuNiStart` 图：ID9 / ID17 对应的 `StatementNodeEx` 前后是谁
- 播动画应插在：Say **之前** / **同时** / **Say 之后等播完再 Continue**？三种时序的利弊（侦探推荐一种默认）
- 重导 CSV 是否会冲掉手插 ActionNode（导入工具是整图重建还是增量）——决定「手插」是否可维护

### E. 动画制作技术路径（只建议，不施工）
对比至少三种，标推荐：

| 方案 | 做法概要 | 是否符合「只留一子物体」 | 与 NodeCanvas 衔接 | 风险 |
|------|----------|-------------------------|--------------------|------|
| A UI Image + AnimationClip 换 sprite | 保留 Y1/G1，Clip 里 key sprite | 是 | Animator.Play / 原生任务 | |
| B 五子物体轮流 SetActive | 不合并 | 否（与用户期望冲突） | 自定义 Action | |
| C Animator Controller 多状态 | 可扩展多段动画 | 是 | 同 A | |

帧率、播完是否回到隐藏、是否循环：写入开放问题请用户拍板。

---

## 侦探任务清单

1. **钉死现网**：`Anim_Yaer` / `Anim_Gusha` 组件清单；图内 ID9/ID17 节点是否已有任何动画相关 Action；导入器能否生成「播动画」节点（答案预期：否）。

2. **第一步建议（做动画）**  
   - 推荐技术路径（一句话 + 对照表）  
   - 「只留一个子物体」的具体操作指引（给施工员：删谁、Clip 用谁的 Sprite、Animator 挂哪）  
   - **本阶段只写建议，禁止实际改 Prefab / 建 Clip**

3. **第二步选型（触发）——必须给明确推荐**

   | 方案 | 做法 | 适合场景 | 本期 2 句是否推荐 | 理由 |
   |------|------|----------|-------------------|------|
   | A CSV 标注 + 导入自动补节点 | | | | |
   | B 手插 ActionNode（对白仍 CSV） | | | | |
   | C 混合（CSV 仅记备注列，不驱动生成；图仍手插） | | | | |

   默认倾向（提示词助手）：**本期推荐 B 或 C**；A 留作「动作戏数量上来后的阶段 3」。侦探须用现网证据确认或推翻。

4. **推荐图内时序**（ASCII 即可）  
   例：Statement(ID8) → Action(Play Anim_Gusha) → Statement(ID9 字幕) → …  
   或：Statement(ID9) 与 Play 并行 / 播完再 EndAction 等。写清与「玩家点继续」的关系。

5. **施工员最小改动清单**（只建议分阶段）  
   - 阶段 1：做 Clip + 精简子物体 + Prefab 默认隐藏  
   - 阶段 2：手插两处播动画节点并验收  
   - 阶段 3（可选）：CSV 扩展 + 导入器——仅当开放问题拍板「要规模化」

6. **验收清单**  
   - 播到「古莎卷了卷角」出现卷角动画；播到翅膀句出现翅膀动画  
   - 其它句子不误播；对话结束 Anim 不残留挡立绘  
   - 重导 CSV 后手插节点是否还在（若会丢，写清规避：先合并再手插 / 禁止覆盖成品 Prefab 图）

7. **开放问题**追加 `OPEN_QUESTIONS.md`（新开「Village_KenMuNiStart · 角翅膀帧动画 · 2026-08-04」）：  
   - 动画播完是否自动进下一句，还是等玩家点继续？  
   - 帧率 / 是否循环 / 播完回隐藏还是停末帧？  
   - 本期是否坚持手插，还是强制上 CSV 自动生成？  
   - 动作戏 Text 是否改为旁白样式（不出口型）还是仍显示字幕？

8. **禁止**：改代码与资产；擅自扩展 CSV 列并改导入器；在 Update 里扫台词播动画；把五帧 SetActive 方案写成唯一解却不对比「单物体 Clip」。

---

## 输出要求

写入：`Assets/Doc/执行文档/0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（动画怎么做 + 本期触发选 A/B/C 哪个）  
② 原因（生活类比 + CSV 导入器能力边界）  
③ 用户需要做什么（拍板时序/循环 + 验收清单）  
④ 给程序看的补充：Prefab 现状表、方案对照表、推荐时序、分阶段最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认：① Clip 技术路径 ② 手插 vs CSV 自动 ③ 播完是否等点击 后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**：
1）把 Anim_Yaer / Anim_Gusha 做成可播放帧动画并尽量只留一个子物体；
2）在 Village_KenMuNiStart 图中于 CSV ID9 / ID17 对应位置接入播放（按报告推荐的手插或 CSV 方案执行）。
禁止在 Update 堆业务；优先复用现有 ActionTask / NodeCanvas 能力；不要顺手大改 CSV 导入器除非报告明确要求阶段 3。
每次提交说明：改了哪些文件、实现了什么、如何验证两句动作戏。
```
