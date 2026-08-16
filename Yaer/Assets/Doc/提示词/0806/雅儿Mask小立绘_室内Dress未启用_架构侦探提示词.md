# Cursor Agent Prompt · 雅儿对话框小立绘服装未跟大立绘（室内 Dress 未启用，仍走 GoOut）

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-06  
> **范围**：对话框左侧 **Mask 小立绘** 启用/服装选择错误——大立绘已是**室内连衣裙（Dress）**，小头像仍是**外出/白裙（GoOut）**，服装对不上。不扩新表情枚举、不改台本文案。  
> **本阶段**：只摸清「谁决定启用 GoOut vs YaerPainting」与 Face 键，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 复现（已对齐截图 · 2026-08-06）

| 项 | 状态 |
|----|------|
| 现象 | 开头对话：场景**大立绘**是雅儿**深色连衣裙（室内 Dress）**；对话框左 **Mask 小头像**仍是**白色高领外出装（GoOut）** |
| 用户表述 | 「没有室内的小立绘头像」「服装没有对上号」 |
| Hierarchy 证据 | `Bottom/Mask/YaerAvatarRoot` 下**同时存在**：`GoOutStoryYaerPainting`（有 Clothes/Heads/Faces）与 **`YaerPainting`**（红箭头；Clothes/Faces；用户指室内套） |
| Faces 键线索 | 室内套 Faces 可见 `Dress_Crown_Unhappy/Daze/Smug/Smile/…` 命名 |
| 与旧 bug 区分 | **不是**「小头像全黑」（0806 分层误伤）；**不是**「同服装错 FaceType」（0804 Laugh→Smile）。本期是 **启用了错误那一套 Painting（服装线）** |

> 侦探须钉死复现对话名（用户口语「开头」可能是 `Village_KenMuNiStart` / NewGame / 进屋线等）。以**大立绘已是 Dress、小头像仍是 GoOut** 为准，勿被场景名带偏。

### 高度可疑根因（优先验证，可证伪）

`DialogueMaskAvatarPresenter` 现网：

```csharp
[SerializeField] private bool yaerUseGoOutOnly = true; // MVP：雅儿一律 GoOut

// ResolvePainting(Yaer)：yaerUseGoOutOnly → 只返回 goOutYaerPainting
// ResolveFaceKey：GoOut → Armor_NoHeadWear_{face}；完整 Dress 才拼 Dress_Crown_*
```

| 历史决议 | 内容 | 与本期冲突 |
|----------|------|------------|
| OPEN 0803 Q2 | **MVP 固定 GoOut**；Dress↔存档 **第二小步** | 产品现要求小头像跟室内大立绘服装 → **第二小步到期** |
| OPEN 0727 Q3 | 小头像服装建议跟 `PlayerClothesData` | 接线时未做完整切换 |
| Prefab | `YaerAvatarRoot` 下已嵌 `YaerPainting`（Dress） | 资产有了，**逻辑未启用** |

**生活类比**：衣柜里挂了两套衣服（外出白裙 / 室内连衣裙），大演员已换上连衣裙上台；话筒旁小显示器被遥控器写死「永远播外出装频道」，所以对不上号。

### 替代假说（须并列排查）

1. `dressYaerPainting` 引用未绑上 / `Find("YaerPainting")` 失败 → 即使关 GoOutOnly 也会回退 GoOut。  
2. 大立绘不是 Mask 同源：场景 Actor 用另一套 `YaerPainting`，Mask 仍 GoOut——根因仍是 Presenter 策略。  
3. `yaerUseGoOutOnly=false` 时现逻辑 **只回 dress、不读存档**（代码：非 GoOutOnly 直接 `dressYaerPainting`）——须确认是否应按 `PlayerClothesData` 在 GoOut↔Dress 间切换，而非永远 Dress。  
4. Dress Face 键：`Dress_Crown_{face}`；`Normal`→`Dress_Crown_Smile`；Prefab 缺某脸 → 启用对了服装但表情空/错（次要，先钉服装）。  
5. 旧 `Portrait` Image 仍亮，叠在 Mask 上造成错觉（低概率；截图红框像 Mask 窗内 GoOut）。

### 对照文档

- `0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`（Q2 MVP GoOut）  
- `技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`  
- `0804/对话框小表情_首句未跟FaceType_…`（表情竞态；**本期主因是服装线，勿只修 Face**）  
- `0806/分层后小头像不显示_…`（黑窗；已修白名单；**勿回退广扫**）  
- `OPEN_QUESTIONS`：0727 Q3 服装跟谁；0803 Q2 Dress 第二小步

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/Doc/执行文档/0804/对话框小表情_首句未跟FaceType_架构溯源报告.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_分层后小头像不显示_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/GoOutStoryYaerPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/YaerPainting.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 雅儿对话框**小表情立绘**启用仍有问题：开头对话里大立绘已是**室内连衣裙**，小头像却还是**外出白裙**，服装对不上。
2. Hierarchy 里 `YaerAvatarRoot` 下明明有室内用的 **`YaerPainting`**，但运行时没启用对的那套。
3. 目标：摸清为何一直走 `GoOutStoryYaerPainting`；给出启用室内 Dress 小立绘、并与大立绘/存档服装对齐的**最小方案**（第二小步落地）。
4. 不要把本期当成「表情 FaceType 错了」或「小头像又黑了」——主诉是**服装套装选错**。

---

## 必读 / 优先扫描线索

### A. 钉死运行时亮的是哪套
- 复现对话打开时 Hierarchy：`GoOutStoryYaerPainting.active` vs `YaerPainting.active`  
- 大立绘（场景 DialogueActor 下）用的是哪套 Prefab / 衣服枚举  
- 截图红框小头像视觉：白裙外出 = GoOut 特征是否坐实

### B. Presenter 服装策略（本期主线）
- `yaerUseGoOutOnly` Prefab 序列化值（默认 true）  
- `ResolvePainting` / `ResolveFaceKey` 在 true/false 下的分支  
- false 时是否**只开 Dress**、还是应按 `PlayerClothesData`（或等价）在 GoOut↔Dress 间切换——对照 Loader / 大立绘如何选衣服  
- `dressYaerPainting` 绑定是否成功（子物体名 `YaerPainting`）

### C. 存档/场景服装真源
- `PlayerClothesData`（或项目内 Clothes 枚举）在「开头对话」时是否已是 Dress  
- 场景大立绘切换衣服的入口（谁在对话前把大立绘切成 Dress）  
- 小头像是否应**跟随同一真源**（推荐），还是按对话 Prefab 写死

### D. Dress Face 键与 Prefab 结构
- `YaerPainting` Faces：`Dress_Crown_*`；缺 `Heads` 是否影响切脸  
- Presenter 拼键 `Dress_Crown_{faceType}` / Normal→Smile 是否覆盖台本脸  
- 启用 Dress 后表情是否仍可能空（次要验收项）

### E. 范围冻结
- **保留**：Mask Presenter 架构；Prepare 白名单；分层/渐入成果  
- **可改建议**：关掉或条件化 `yaerUseGoOutOnly`；按存档选 Painting + Face 键；必要时补 Dress 缺脸  
- **禁止**：删掉 GoOut 套（村线外出仍要用）；重做四套图集小头像；改台本文案；名字广扫 CanvasGroup

---

## 侦探任务清单

1. **结论一句话**：小头像服装错的根因（优先钉 `yaerUseGoOutOnly` MVP）+ 推荐切换规则（跟存档 / 跟大立绘 / 永久 Dress）。

2. **对照表**

   | 层 | 现网用哪套 | 应由谁决定 | 现网谁在决定 |
   |----|------------|------------|--------------|
   | 场景大立绘 | | | |
   | Mask 小立绘 | | | |
   | Face 键 | | | |

3. **方案比选表**（至少 3 档，推荐 1 个）

   | 方案 | 做法摘要 | 村线 GoOut | 室内 Dress | 改动面 | 风险 | 推荐？ |
   |------|----------|------------|------------|--------|------|--------|
   | A | `yaerUseGoOutOnly=false` 且按 `PlayerClothesData` 选 GoOut/Dress + 对应 Face 键 | | | | | |
   | B | 仅 false → 永远 Dress（简单但外出线会错） | | | | | |
   | C | 跟场景当前大立绘类型镜像切换 | | | | | |
   | D | 按对话 Prefab/章节配置写死 | | | | | |

4. **与历史 OPEN 关系**  
   - 标注 0803 Q2「第二小步」到期；0727 Q3 服装跟存档是否采纳。  
   - 明确：**不是**再开 0804 FaceType 竞态主修（可顺带验收表情，非主因）。

5. **施工员最小改动清单**（只建议）  
   - 优先改 `DialogueMaskAvatarPresenter` 解析逻辑 + Prefab 开关默认值；验证 `YaerPainting` 引用。  
   - 验收日志建议：`[MaskAvatar] Yaer → GoOut|Dress face=…`

6. **验收清单**  
   - 室内/Dress 开头对话：小头像 = **连衣裙套**（`YaerPainting`），与大立绘服装一致。  
   - 外出/GoOut 对话（如村线开场若仍是外出装）：小头像仍走 **GoOut**，不回归。  
   - 表情跟台本 FaceType（至少 Smile/常见脸）；Mask 不黑。  
   - DialogDebug 两套衣服各测一句。

7. **开放问题**追加 OPEN（「雅儿 Mask 小立绘 Dress 启用 · 2026-08-06」）：  
   - 服装真源：存档 `PlayerClothesData` vs 镜像大立绘？  
   - 皇冠/头饰是否跟 Clothes 子状态？  
   - `yaerUseGoOutOnly` 字段删除还是改为「强制覆盖」调试开关？

8. **禁止**：改台本；拆掉 GoOut 实例；Update 轮询换装；为修服装重写整棵对话 Painting 系统。

---

## 输出要求

写入：`Assets/Doc/执行文档/0806/雅儿Mask小立绘_室内Dress未启用_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（根因 + 推荐方案）  
② 原因（生活类比：大演员换装、小显示器频道写死）  
③ 用户需要做什么（拍板服装真源 A/B/C + 验收）  
④ 给程序看的补充：对照表、方案表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认「跟存档 Clothes」还是「镜像大立绘」后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0806/雅儿Mask小立绘_室内Dress未启用_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使雅儿 Mask 小立绘在室内 Dress 对话中启用 YaerPainting（Dress_Crown_*），与大立绘服装一致；外出线仍保 GoOut。
禁止拆掉 GoOut；禁止回退 Prepare 白名单。禁止在 Update 堆补丁。
每次提交说明：改了哪些文件、服装如何选择、如何验证室内/外出两套。
```
