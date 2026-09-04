# Village_KenMuNiStart · 角/翅膀帧动画 + CSV Type 触发 — 架构溯源与执行说明

**文档版本**：v1.1（2026-08-04）  
**文档性质**：【架构侦探】结论 + **【施工已落地代码/资源】**  
**依据**：提示词 `0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构侦探提示词.md`；开发者决议：**先做动画 → NodeCanvas 可调用 → CSV `Type` 控制类型**  
**Unity**：2020.3.48f1  

> **施工结果（2026-08-04）**  
> - Clip：`Assets/GameRes/Animation/Dialogue/Anim_Gusha_Horn` / `Anim_Yaer_Wing`（+ Controller，状态名 `Play`，10fps 不循环）  
> - Task：`PlayUiAnimatorActionTask`（Category Animation）  
> - CSV：ID9/17 → `Type=Anim`，Extra=`Anim_Gusha`/`Anim_Yaer`  
> - 导入器：`DialogueCsvParser`/`GraphBuilder` 支持 Anim → Play → Statement  
> - Prefab：`Anim_*` 已默认 Inactive；**请跑** `Tools/Dialogue/Setup KenMuNiStart Horn Wing Anim` 挂 Animator/BB/删多余帧  
> - 再 Import CSV 生成 Generated，**手工合并**进 `Village_KenMuNiStart` 成品图（勿覆盖前奏）

---

## ① 结论一句话

**你的三步顺序是对的，也是现网该走的正道。**  
动画本体用 **UI Image + AnimationClip 换 Sprite（只留一子物体）**；再做一个 **「播 Animator」ActionTask**（挂 Blackboard 引用）；最后扩展 CSV **`Type=Anim`（或 `Action`）+ `Extra=动画键`**，导入器自动生成 Action 节点。  
**但现状还不能「改完 Type 立刻生效」**：导入器目前只认 `Dialogue` / `Choice`，且阶段 1 只生成独立 `.asset`、不会自动合并进成品 Prefab 图——所以必须按下面阶段施工，不能跳过「可调用」直接只改 CSV。

---

## ② 原因（生活类比）

台本是菜单（CSV），厨房是 Prefab 上的 `Anim_*`，服务员是 NodeCanvas Action。  
菜单上写「卷角」「翅膀」之前，厨房得先有菜（Clip），服务员得会端盘（Play 任务 + Blackboard 绑盘）。  
只改菜单 Type、厨房没菜 / 服务员不会端 → 顾客还是吃不到。

### 现网钉死（只读核实）

| 项 | 现状 |
|----|------|
| Prefab | `Village_KenMuNiStart` 根下已有 `Anim_Yaer`（Y1～Y5）、`Anim_Gusha`（G1～G5） |
| 组件 | 各帧为 UGUI **Image** + Sprite；容器仅 Transform；**无** Animator / AnimationClip |
| 默认 Active | `Anim_*` 与全部子帧 **均为 Active=1** → 五帧叠在一起，入场前应隐藏 |
| 位置 | 左右分挂（Yaer 偏左、Gusha 偏右），盖在立绘区域上的局部特写，非整身替换 |
| CSV ID9 / 17 | 仍是 `Type=Dialogue` 动作说明句（「古莎卷了卷角」「雅尔呼扇…翅膀」） |
| 图内 | 尚无播这两处动画的 ActionNode |
| 导入器 `Type` | **仅** `Dialogue` / `Choice`（`DialogueCsvParser.TryParseNodeType`） |
| `Extra` | **仅 Choice** 用（选项文案）；Dialogue 行 Extra 空着 |
| 阶段 3 文档 | 「旁白 Action 列」**仍未做** |
| 现成播 Clip 任务 | **无**项目自写 PlayAnimator；仅有 **`AnimationEventRegisterTaskAction`（等事件，不播）**、UI Alpha 类 |
| CSV 导入产物 | 生成 `Generated/*.asset`；**不**自动写回 `Village_KenMuNiStart.prefab`（合并属阶段 2，待做） |

### 方案对照（触发层）

| 方案 | 做法 | 适合 | 与本期决议 |
|------|------|------|------------|
| A CSV `Type` + 导入自动补节点 | 扩展解析/建图；`Extra`=动画键；BB 名解析绑 Animator | 动作戏会增多、要可维护 | **目标方案（开发者已选）** |
| B 仅手插 ActionNode | 对白仍 CSV；两处手动插播 | 只 2 句、赶验收 | 可作阶段 2 **临时验收**，不取代 A |
| C CSV 备注列不驱动生成 | 给人看的备注 | 文档用 | 不必单独做 |

提示词助手曾倾向「本期只手插」——对「只验收两句」成本更低；**你选 A 作为正式路径合理**，只要接受：要多改导入器 + 约定动画键，且 Prefab 合并仍要一步人工或等阶段 2 工具。

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板（OPEN）

| ID | 问题 | 执行默认建议 |
|----|------|----------------|
| Q1 | 播完是否自动进下一句？ | **否**：动画与字幕同句；**等玩家点继续**再往下（动作戏当一句 Dialogue 体验） |
| Q2 | 帧率 / 循环 / 播完状态？ | **约 8～12 fps、不循环、播完隐藏** `Anim_*`（不挡立绘） |
| Q3 | CSV 正式 Type 名？ | **`Anim`**（清晰）；`Extra` 填 `Anim_Gusha` / `Anim_Yaer` |
| Q4 | ID9/17 是否仍显示字幕 Text？ | **是**：`Type=Anim` 时 **先/并行播动画，再出 Statement 字幕**（或同节点链：Play → Say） |
| Q5 | 本期是否坚持走 CSV Type（方案 A）？ | **是**（开发者决议）；阶段 2 允许手插先验动画 |

### 验收清单

1. 播到「古莎卷了卷角」出现卷角动画；翅膀句出现翅膀动画  
2. 其它句子不误播；对话结束 `Anim_*` 不残留挡立绘  
3. 改 CSV `Type`/`Extra` 后重导 Generated 图，对应行出现 Play Action（导入器扩展验收）  
4. 成品 Prefab 实机：进村开场两句动作戏可见  
5. 重导时勿直接覆盖成品 Prefab 图导致前奏/立绘引用丢失（见 §4.5）

---

## ④ 给程序看的补充（执行说明书）

### 4.1 推荐总流程（对齐开发者三步）

```
阶段 1  资源：做帧动画 Clip，精简为单子物体，默认隐藏
    ↓
阶段 2  运行时可调：Play Animator ActionTask + Prefab Blackboard 绑 Anim_* 
         （可选手插两处先验收）
    ↓
阶段 3  CSV：Type=Anim + Extra=动画键 → 导入器自动生成 Action 节点
         → 再合并进 Village_KenMuNiStart 成品图
```

**不要**跳过阶段 1/2 只改 CSV——现网导入器会报 `Type 非法`。

---

### 4.2 阶段 1 · 做动画（资源）

**推荐路径：A — UI Image + AnimationClip 换 `Image.sprite`**

| 方案 | 做法 | 只留一子物体 | 与 NC 衔接 | 风险 |
|------|------|--------------|------------|------|
| **A（推荐）** | 保留 Y1/G1，Clip 关键帧 Sprite；挂 Animator | 是 | Animator.Play | 需手工做 Clip |
| B | 五物体轮流 SetActive | 否（违背期望） | 自定义 Action | 层级脏、难维护 |
| C | Animator Controller 多状态 | 是 | 同 A | 本期仅一段动画，Controller 可极简单状态 |

**施工员操作指引（只建议）：**

1. 在 Project 建 Clip，如 `Anim_Gusha_Horn`、`Anim_Yaer_Wing`（路径建议 `GameRes/Animation/Dialogue/` 或 Prefab 旁）  
2. 以 **G1 / Y1** 为唯一显示 Image：把 G2～G5、Y2～Y5 的 Sprite **录进 Clip 曲线**（`Image.m_Sprite`）  
3. Clip 播完可加事件或靠长度 EndAction；**默认不循环**  
4. 删掉多余子物体（或 Inactive 备份后删），容器 `Anim_Gusha` / `Anim_Yaer` 上挂 **Animator** + 极简 Controller（默认态 Idle 空 / 入口态 Play）  
5. Prefab 默认：`Anim_*` **Inactive** 或 alpha=0；播放时由 Action 打开再播  

---

### 4.3 阶段 2 · NodeCanvas 可调用

现网 **没有**「播 AnimationClip」ActionTask；`等待Animation事件` 不够。

**最小新建（建议）：**

| 项 | 说明 |
|----|------|
| 类名示例 | `PlayUiAnimatorActionTask`（Category Animation） |
| 参数 | `BBParameter<Animator> animator`；可选 `stateName` / `trigger`；`bool waitUntilFinish`；播前 `SetActive(true)`，结束后按策略隐藏 |
| Prefab BB | 增加变量如 `Anim_Gusha`、`Anim_Yaer` → 拖容器上 Animator（对齐立绘 `GushaPainting` 用 `_name` 字符串绑定的现网习惯） |
| 图内时序（推荐默认） | 见下 ASCII |

**推荐图内时序（与 Q1 默认一致）：**

```
… → Statement(ID8 对白)
  → Action(Play Anim_Gusha)          // 显示并播放；waitUntilFinish=true（或 false 与字幕并行）
  → Statement(ID9 「古莎卷了卷角」)   // 玩家点继续
  → Statement(ID10 …)
…
  → Action(Play Anim_Yaer)
  → Statement(ID17 翅膀句)
  → …
```

- **替代**：`Type=Anim` 一行同时生成「Play + Say」两节点（导入器一次吐出链）——阶段 3 推荐此结构，台本仍保留 Text 作字幕。  
- **不推荐**：只播动画无字幕（策划句会丢）；或在 Update 扫台词字符串匹配（脆、难维护）。

阶段 2 可先 **手插** 上述两处 Action，验证 Clip/Task/BB，再开阶段 3。

---

### 4.4 阶段 3 · CSV `Type` 控制（正式目标）

#### 台本约定（建议）

```csv
ID,Type,Speaker,Text,English,Next,Extra,FaceType,...
9,Anim,古,古莎卷了卷角,,10,Anim_Gusha,Happy
17,Anim,雅,雅尔呼扇呼扇头上的一对小翅膀。,,18,Anim_Yaer,Laugh
```

| 列 | `Type=Anim` 时含义 |
|----|-------------------|
| Type | `Anim`（新枚举；导入器须扩展） |
| Extra | **动画键** = Prefab/BB 名：`Anim_Gusha` / `Anim_Yaer`（与物体名一致，便于解析） |
| Text | 仍作字幕（生成 StatementNodeEx） |
| FaceType | 可选；跟说话人表情，与现网 Dialogue 行相同 |
| Speaker | 字幕归属立绘说话人 |

> **为何不只改 Type、不用 Extra？**  
> Type 只回答「这是什么节点类」；**播哪一段**必须另有键。复用 `Extra` 最贴现网列结构（Choice 已占用 Extra；Anim 行与 Choice 互斥，无冲突）。

#### 导入器最小改动面

| 文件 | 改什么 |
|------|--------|
| `DialogueCsvParser` | `TryParseNodeType` 增加 `Anim`；校验 Anim 行 `Extra` 非空 |
| `DialogueRow` / 文档表头说明 | 注明 Anim 时 Extra=动画键 |
| `DialogueCsvGraphBuilder` | `Anim` → 生成 `ActionNode(PlayUiAnimator)` → 再连 `StatementNodeEx`（Text）；BB `_name`=Extra |
| 技术文档阶段 3 | 勾选「旁白/动作 → Action」已部分落地 |

#### 对象引用难点（必须写清）

生成的是 **Graph 资产**，Animator 活在 **Prefab 实例**上。现网立绘淡入做法：Action 里只写 Blackboard **变量名字符串**，运行时由 Controller 所在 Prefab 的 BB 解析。  
→ 导入器 **不要**试图序列化场景引用；只写 `animator._name = Extra`（如 `Anim_Gusha`）。  
→ 成品 Prefab 必须 **预先**建好同名 BB 变量并拖好引用（阶段 2 完成）。否则 Generated 图看着有节点，进 Prefab 也播不出来。

#### 与 Prefab 合并

- 当前：`Tools/Dialogue/Import CSV` → `GameRes/DialogueTrees/Generated/*.asset`  
- 成品图在 `Village_KenMuNiStart.prefab` 的 `_boundGraphSerialization`  
- **重导会整图重建 Generated，不会智能保留手插**；合并进 Prefab 仍靠人工拷贝节点或等「阶段 2 合并工具」  
- **规避**：动画键与 Play Task 稳定后，再批量导；合并后 **不要**用「覆盖成品 Prefab」当日常迭代，除非有合并流程

---

### 4.5 分阶段最小改动清单（施工勾选）

| 阶段 | 做什么 | 不做什么 |
|------|--------|----------|
| **1** | Clip + 单 Image + Animator；删多余帧物体；默认隐藏 | 不改 CSV / 导入器 |
| **2** | `PlayUiAnimatorActionTask`；BB 绑定；图内两处可选手插验收 | 不在 Update 扫台词；不改龙宫无关对话 |
| **3** | CSV `Type=Anim` + Extra；Parser/Builder；文档；导 Generated 再合并 Prefab | 不把五帧 SetActive 当正式方案 |

---

### 4.6 开放问题

已追加 `OPEN_QUESTIONS.md`「Village_KenMuNiStart · 角翅膀帧动画 · 2026-08-04」。

---

## 附录 · Prefab 现状速查

| 节点 | 子物体 | 组件 | Active |
|------|--------|------|--------|
| `Anim_Yaer` | Y1～Y5 | 各 Image+Sprite；容器无 Animator | 全 1 |
| `Anim_Gusha` | G1～G5 | 同上 | 全 1 |

CSV：`Assets/Dialog/Village_村内雅古开场对白台本.csv`  
Prefab：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`  
导入工具文档：`Assets/Doc/技术文档/CSV转NodeCanvas对话树导入工具_开发文档.md`
