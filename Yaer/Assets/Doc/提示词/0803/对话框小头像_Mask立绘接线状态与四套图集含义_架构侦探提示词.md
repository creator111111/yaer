# Cursor Agent Prompt · 对话框小头像：Mask立绘接线核实 + 真正启用施工方案

> **角色**：【架构侦探】只溯源、不改代码；**必须产出可交给施工员的接线方案**  
> **日期**：2026-08-03  
> **开发者目标**：让 `NormalDialogueNewPanel` 里已摆好的 Mask + `YaerAvatarRoot` 立绘 Prefab **真正跟台本 FaceType 换脸**（不再只是摆位壳）  
> **次要澄清**：「四套图集」是什么、与 Mask 立绘是否一回事  
> **本阶段**：只读扫描 + 写报告 + **写清施工方案**；禁止改代码 / Prefab / 图集

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV。只读扫描 + 写「溯源报告 + 施工方案」。
报告通过后，施工员应能按方案直接改，无需再猜挂点。

---

## 背景（策划白话 + 截图已对齐）

1. Hierarchy 可见：
   `NormalDialogueNewPanel` → `Root` → `Bottom` → `Mask` → **`YaerAvatarRoot`**
   其下已有：
   - `GoOutStoryYaerPainting`
   - `YaerPainting`
   - `AmyPainting`
   - `AliyPainting`
   - `GushaPainting`
2. **本期主目标（开发者已拍板方向）**：
   让上述 Mask 立绘预制体**真正被对话框使用**——说话时按 `FaceType` 切脸、按角色/服装显隐对应 Painting 实例；旧图集小头像链路可保留回退，但玩家看到的应是 Mask 立绘。
3. 必须同时答清的澄清题：
   - **Q1**：之前是不是只放了 Prefab、代码未绑定？
   - **Q2**：当前运行时真正刷脸走哪条？
   - **Q3**：「四套图集」到底是哪四个文件？是不是 YaerAvatarRoot 下那些 Painting？（用全路径钉死）
4. 对照文档（以代码为准）：
   - `Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`
   - `Assets/Doc/执行文档/0727/NormalDialogueNewPanel_遮罩立绘搭对话头像_Prefab修改执行说明.md`
   - `Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md`
   - `Assets/Doc/执行文档/0803/雅儿立绘新增Happy表情_接入表情系统_架构溯源报告.md`（若有）
   - `Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md`
   - Prefab 技术说明「下一轮施工建议」§7（可作方案起点，须按现网代码修订）

---

## 必读 / 优先扫描线索

### A. UI Prefab 现状
- `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab`
- `YaerAvatarRoot` 五实例默认 Active；`Portrait` Active；`DialogueTMPUGUI.actorPortrait` 指向

### B. 现网赋值链（为何 Prefab「没用上」）
- `DialogueTMPUGUI` → `OnGetAvatar` / `actorPortrait`
- `DialogueActorEx.RefreshAvatar` + `OnRefreshAvatarEvent`
- `DialogueAvatarLoader` / `DialogueAvatarPathHelper`
- UI 壳内 Painting：`StoryFormPainting.Start` 找 `DialogueActorEx` 的路径——订得到还是订不到？

### C.「四套图集」钉死（独立小节，大白话）
核实是否即：
- `Avatar_Yaer_Dress_Crown.spriteatlas`
- `Avatar_Yaer_Armor_NoHeadWear.spriteatlas`
- `Avatar_Yaer_Armor_Crown.spriteatlas`
- `Avatar_Yaer_Armor_ArmorHead.spriteatlas`
路径、`Assets/GameRes/Atlas/Avatar/`；说明与 Mask 立绘**不是同一套**；为何 GoOut 加 `Happy` 不等于图集有 Happy。

### D. 接线方案所需接口面（为施工方案服务）
- 对话请求字幕时，哪个组件最适合拿到「当前说话人 + FaceType +（雅儿）服装」？
- 最小侵入挂点候选（须比较优劣，推荐一个）：
  1. 在 `DialogueTMPUGUI` 旁路：有 FaceType 时驱动 `YaerAvatarRoot`，弱化/跳过对雅儿的 Portrait 赋值
  2. 新建薄组件挂在 `YaerAvatarRoot`（如 DialogueMaskAvatarPresenter），订阅 Actor / 字幕事件
  3. 改 `DialogueAvatarLoader`（通常不适合驱动 Prefab 显隐，写清原因）
  4. 给 UI 壳补 `DialogueActorEx` 或转发 `OnRefreshAvatarEvent` 给 Mask 内 Painting
- 角色切换：说话人换到古莎/艾米时，如何互斥显隐五个 Painting？
- 雅儿换装：GoOut vs YaerPainting、头饰 `ArmorHead`/`ArmorCrown` 是否本期必做？
- `Happy`：Mask 接线后，GoOut 上已有 `Armor_NoHeadWear_Happy` 是否即可；小头像图集是否可不再依赖？

---

## 侦探任务清单

### Part 1 — 现状核实（必答）
1. 当前显示源：A Mask立绘 / B 旧Portrait图集 / C 视觉A逻辑B  
2. 是否「只摆 Prefab 未绑代码」——证据  
3. 「四套图集」全路径解释 + 与 YaerAvatarRoot 对照表  
4. 双链路表（旧 Portrait vs Mask 立绘 vs 场景大立绘对照）

### Part 2 — 施工方案（本期核心交付，仍不改代码）
必须输出一节 **「施工方案（给施工员）」**，至少包含：

1. **推荐方案一句话**（选哪个挂点、为什么最小侵入）  
2. **备选方案**一行（若推荐挂点不可行时的退路）  
3. **分步施工清单**（按顺序，可勾选）：
   - 改哪些脚本（新建 or 改现有；类名建议）
   - 改哪些 Prefab 引用/序列化字段
   - 旧 `Portrait` / Loader：隐藏？跳过雅儿赋值？还是双轨过渡？
   - 角色显隐规则（Yaer/Gusha/Amy/Aliy）
   - 雅儿服装显隐规则（本期做全量还是先固定 GoOut）
   - FaceType → `UpdateFace` / 键名规则（GoOut 的 `Armor_NoHeadWear_*`、古莎裸枚举名等）
4. **本期范围边界**（明确 In / Out）  
   - In：例如「字幕条 Mask 头像跟 FaceType」  
   - Out：例如「不必补四套图集 Happy」「不改历史头像」或相反——侦探按架构建议并写清取舍理由  
5. **验收清单**（DialogDebug / 哪条对话、选 Happy 应亮哪节点、换说话人应显隐谁）  
6. **风险与开放问题**（写入报告；设计不清追加 `OPEN_QUESTIONS.md`）  
7. **预估改动文件列表**（路径级，方便施工员开干）

原则：
- 优先保持现有 FaceType 契约与 Painting `UpdateFace` 机制；
- 禁止建议把 Faces 节点改成中文名；
- 禁止建议一次性重写整个 DialogueForm；
- 禁止在方案里要求 Update 堆业务逻辑；
- 方案必须让「已摆好的五个 Painting Prefab」成为运行时真源，而不是再维护一套静态小头像矩阵（除非侦探论证双轨更稳并写明过渡期）。

---

## 输出要求

写入：`Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`

报告结构：

① 结论一句话（现状 + 推荐怎么启用）  
② 原因（生活类比 + 锚点）  
③ 用户需要做什么（验收/拍板清单：是否接受「先接线、图集 Happy 可缓」等）  
④ 给程序看的补充：
   - Part1 双链路与四套图集说明
   - **Part2 完整施工方案**（见上）
   - 文件清单、开放问题

禁止修改任何资产；禁止把「嵌了 Prefab」写成「已启用」；禁止只列选项不给推荐方案。

完成后用 MASTER 四段式口头汇报；详细以报告为准。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。严格按报告「施工方案（给施工员）」做最小化修改，
让 NormalDialogueNewPanel 的 Mask + YaerAvatarRoot 立绘真正跟台本 FaceType 换脸。
优先复用 StoryFormPainting / 各角色 Painting 现有 UpdateFace；禁止改 Faces 节点为中文；禁止 Update 堆业务。
每次提交说明：改了哪些文件、实现了什么、如何验证。
```
