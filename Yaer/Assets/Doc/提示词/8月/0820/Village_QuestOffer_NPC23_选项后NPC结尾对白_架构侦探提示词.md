# Cursor Agent Prompt · Village_QuestOffer_NPC23：选项后补两句 NPC 结尾对白（产品已改）

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **性质**：对 `0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md` 的 **结尾拓扑补丁**（不是重做整张任务卡）  
> **产品已拍板（开发者流程图）**：  
> - 玩家选 **「我有些忙」** → NPC 说 **「没关系我一会自己去吧」** → 结束（不接任务）  
> - 玩家选 **「好呀」** → NPC 说 **「太感谢了」** → 再走收尾 / 接任务  
> **作废旧建议**：上一份报告「接受后雅尔复读『好呀！』」**不再采用**；拒分支也**不再**点完选项直接 END。  
> **本阶段**：只读 + 写补丁报告，**不施工**  
> **范围**：只改/补 `Village_QuestOffer_NPC23` Graph 选项后的两句 Statement + 出边。不改埃吉尔 Prefab，不做采集运行时。

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

选项对话后面要补结尾：  
1. 我说「我有些忙」之后，NPC 要回 **「没关系我一会自己去吧」**。  
2. 我说「好呀」之后，NPC 要回 **「太感谢了」**。  
请查明现网 Graph 缺哪两句、挂在谁身上、接到哪、会不会挡住后面的 QuestAccept。

### 产品流程图（开发者截图，文案以本表为准）

```
MultipleChoice
  ├─ 我有些忙。 → NPC：「没关系我一会自己去吧」 → 收尾 END（不 Accept）
  └─ 好呀。     → NPC：「太感谢了」             → 收尾（批次 B：此句之后才 QuestAccept）
```

| 玩家选项（按钮） | 随后说话人（预扫） | 随后台词（已定） |
|------------------|--------------------|------------------|
| 我有些忙 / 我有些忙。 | NPC3（妈妈，请托人） | 没关系我一会自己去吧 |
| 好呀 / 好呀。 | NPC3 | 太感谢了 |

标点：流程图选项带句号「我有些忙。」「好呀。」；上一份报告按钮无句号。侦探对拍 Prefab 现网按钮文案，**台词以开发者本条为准**；按钮是否带句号记 OPEN（不要擅自改已定「我有些忙 / 好呀」除非现网已带句号）。

### 与旧报告的差异（必须写进新报告）

| 项 | 0820 选项报告（旧） | 本期（新） |
|----|---------------------|------------|
| 拒出边 | MC → 直接收尾 | MC → **NPC「没关系我一会自己去吧」** → 收尾 |
| 接出边 | MC → **雅尔「好呀！」** → 收尾 | MC → **NPC「太感谢了」** → 收尾（Accept 仍在这句之后） |
| OPEN Q1 雅尔复读 | 建议要 | **作废** |

### 预扫结论方向（可证伪）

- 仍是手改 Graph 加 **StatementNode**，不必新 Prefab、不必改 CSV（除非侦探认为要同步台本）。  
- 两句 Actor 预扫 **NPC3**（妈妈），不是孩子 NPC2、也不是雅尔；须用 Prefab 请托句 Actor 对拍。  
- 埃吉尔样板：接受后是**玩家**复读；本期是**NPC 道谢/放手**，机制相同（Choice 出边 → Statement），只换说话人和文案。  
- `QuestAcceptAction` 必须挂在 **「太感谢了」播完之后**，不要插在「好呀」按钮和「太感谢了」之间（避免没谢完就签收）。  
- 拒分支仍 **禁止** Accept。

### 必读

- `Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md`（旧拓扑 §4.3，须标明作废点）
- `Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab`（现网有没有 MC、出边接到哪）
- 埃吉尔样板：`Village_Aegir_QuestOffer.prefab`（Choice → Statement 的挂法）
- `Assets/Doc/OPEN_QUESTIONS.md` 中「NPC23 接任务对话选项」Q1

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestOffer.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写补丁报告。

---

## 背景

1. NPC23 接任务双选项文案已定：拒「我有些忙」、接「好呀」。
2. 新需求：两条选项后面都要再跟一句 NPC 对白（开发者已画流程图）。
3. 旧报告建议「好呀」后雅尔说「好呀！」——产品已否决，改为妈妈「太感谢了」；拒绝后也不再静默结束。
4. 本期只查明结尾两句怎么补进 Graph。任务 JSON / 扣藤蔓果另案。

---

## 必读线索

### A. 现网 Graph 对拍

打开 `Village_QuestOffer_NPC23`：

- 有没有 MultipleChoice？选项文案是否已是「我有些忙 / 好呀」（有无句号）？
- 拒/接出边现在接到哪（直接 Success、空 Action、雅尔句、还是还没有 MC）？
- 末请托句仍是不是 NPC3「我会给你报酬的。」

### B. 目标拓扑（按产品改写，须与 Prefab 对拍后定稿）

```
#12 我会给你报酬的。
  → MultipleChoice
       0 我有些忙 → Statement NPC3「没关系我一会自己去吧」 → 收尾（不 Accept）
       1 好呀     → Statement NPC3「太感谢了」 →（可选批次 B）QuestAcceptAction → 收尾
```

侦探须钉死：

- 两句 Statement 的 Actor（预扫 NPC3）
- FaceType：空 / Normal / 沿用妈妈上一句？
- 收尾叶子与埃吉尔 FightingPanel 是否仍建议补
- Accept 只允许出现在「太感谢了」之后

### C. 不要误伤

- 不改 `Village_Aegir_QuestOffer`
- 不把孩子 NPC2 误绑成道谢句（除非现网请托人不是 NPC3）
- 不把本补丁扩成采集任务卡施工
- CSV 默认可不改；若要同步台本，只建议追加行，不强制

---

## 侦探任务

1. **结论一句话**：选项后各补一句 NPC 台词；旧「雅尔复读好呀」作废。
2. **现网 vs 目标对照表**（缺哪两个节点）。
3. **推荐拓扑**（含 Actor、出边、Accept 插入点）。
4. **验收**：点「我有些忙」必听到「没关系我一会自己去吧」且无 Accept；点「好呀」必听到「太感谢了」。
5. **OPEN**：按钮文案要不要句号；FaceType；「太感谢了」后要不要感叹号。
6. **禁止**：改资产；恢复雅尔复读方案当主方案。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_选项后NPC结尾对白_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：点菜单后店员还要回一句，不能点完就关灯）  
③ 用户检查清单（Graph 加哪两句、接到哪）  
④ 给程序：作废旧 §4.3 点、节点锚点、Accept 仍在谢完之后

口头汇报用 MASTER 四段式。
```

---

## 施工员续跑（补丁报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_选项后NPC结尾对白_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab

你现在是【施工员】。只补 Village_QuestOffer_NPC23 选项后的两句结尾，按补丁报告拓扑手改 Graph。

必须：
- 「我有些忙」→ NPC「没关系我一会自己去吧」→ 结束，不接任务
- 「好呀」→ NPC「太感谢了」→ 再收尾（本批若报告未要求则先不挂 QuestAccept）
- 不要雅尔复读「好呀！」；不改埃吉尔 Prefab；不改任务 JSON

提交说明：两句 Actor 是谁、接在 MC 哪条出边、如何 Play 验收。
```
