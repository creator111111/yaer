# Cursor Agent Prompt · 巨树 2 楼：进层对白 + 开箱对白动画 + 生命/体力球 Tips

> **角色**：先【架构侦探】只读定方案；报告拍板后再【施工员】  
> **日期**：2026-09-03  
> **场景**：`Village_KenMuNi1` · 巨树 **`VillageWalkArea2`**（2 楼）  
> **产品（钉死 · 用户指定对话资源）**：  
> 1. **进入树屋二层** → 播对白 **`Village_玩家上树屋二层`**  
> 2. **开宝箱交互** → 播对白 **`Village_玩家上树屋二层触发宝箱`**（含开箱交互动画）→ 结束后弹**获得道具 Tips**：生命球×3、体力球×3（现网即 `GetHpBall` → `GetMpBall` 花边横幅；入包 `HpBall`/`MpBall` 各 3）  
> **对话资源（已有）**：  
> - Tree：`Assets/GameRes/DialogueTrees/Generated/Village_玩家上树屋二层.asset`  
> - Tree：`Assets/GameRes/DialogueTrees/Generated/Village_玩家上树屋二层触发宝箱.asset`  
> - CSV：`Assets/Dialog/Village_玩家上树屋二层.csv` / `Village_玩家上树屋二层触发宝箱.csv`  
> **实体底子（0901 已有）**：`Objects/Tree2fHpMpBox` + `VillageKenMuNi1HpMpBox`（现网 `useStoryOnOpen=0` 直发奖，**与本期「走对话」冲突，须改接**）  
> **样板**：进层单次戏 ≈ `Village_KenMuNiStart` / 门口初次对话挂法；开箱对白+Tips ≈ `HomeScene2Box`（ExecuteFunction）或针线包三连（`GetItem`+`OpenTipsFormActionTask`）  
> **不是**：改 WalkArea2 形状；不是挂西境箱；不是新 Tips UI；不是重写 0901 存档键名（仍用 `tree2fHpMpBoxOpened` 除非报告证明必须拆）  
> **并行**：0903 卡住走不动 / 宝箱看不见——**可同会话对照，但本期主交付是对话接线**；看不见箱则先保证实体在，再验开箱戏  
> **报告落盘**：`Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_架构溯源报告.md`

把下面「侦探」整段复制给 Agent。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 人第一次上到巨树 2 楼：雅自己嘀咕「好高 / 小心」。  
> 点宝箱：先对白「这里有箱子→打开看看」，播开箱动画；然后弹出和剑/空桶同款的「获得了生命球」「获得了体力球」大横幅，背包各 +3。

### CSV 现网对白（已生成 Tree · 助手预扫）

**`Village_玩家上树屋二层`**

| # | 雅 | Face |
|---|-----|------|
| 1 | 哇！好高啊！ | Surprised |
| 2 | 这里能看到远方，真漂亮。 | Smile |
| 3 | 。。。。。。 | Smile |
| 4 | 不能往下看。。。。。。要小心。。。 | ChiBie |

**`Village_玩家上树屋二层触发宝箱`**

| # | 雅 | Face |
|---|-----|------|
| 1 | 这里有一个箱子？ | Surprised |
| 2 | 嗯。。。。。。 | Smile |
| 3 | 打开看看吧~ | Laugh |

预扫：两份 Generated Tree 目前主要是 **UIAlpha 入场 + StatementNodeEx**；**未见** GetItem / OpenTips / 开箱 Animator 节点——施工须**补节点或 ExecuteFunction**，不能假设 CSV Import 已含发奖。

### 期望时序（钉死）

```
【A · 进 2 楼】
  落点 ExitFrom_HomeSceneChief2f / 进入 WalkArea2 语义
    → StoryComponentGSM.TriggerStory("Village_玩家上树屋二层")
    → 对白播完还控
    → 同档单次（StoryTriggerCount / SingleUseInArchive — 侦探拍板）

【B · 开箱】
  点 Tree2fHpMpBox
    → TriggerStory("Village_玩家上树屋二层触发宝箱")
    → 对白进行中/末：开箱动画（Open）+ 存档 tree2fHpMpBoxOpened
    → 入包 HpBall×3、MpBall×3
    → OpenTipsForm("GetHpBall") → OpenTipsForm("GetMpBall")  // 同款道具横幅，先 Hp 后 Mp
    → 同档不可再开
```

### 现网缺口（助手预扫）

| 项 | 现状 | 含义 |
|----|------|------|
| DialogueTree `.asset` | ✅ Generated 两份已有 | 有图可挂 |
| Dialogue **Prefab** | ❌ `GameRes/Prefabs/Dialogue/` 下 **无** 同名 Prefab | `TriggerStory` 通常要 Prefab 壳（对齐 `Village_出村长家送树屋` / `HomeScene2Box`） |
| 进层触发器 | ❌ 未见绑定这两 Story 的 Trigger | 须新建 Enter/落点后播 |
| `Tree2fHpMpBox` | ✅ 场景有；`useStoryOnOpen=0` 直 `OnOpenBox+OnGetHpMp` | **跳过对白**；本期须改走 Story |
| Tips 图集 | ✅ `GetHpBall` / `GetMpBall` 已有（0901/0830） | 不必新图；横幅不写死「×3」字样（与西境/卧室一致） |

### 进层对白 · 触发方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **E1 · 落点后 GSM 播** | `Village_KenMuNiSceneManager` 在楼梯路径 `LastScene==Village_Chief_House` 且 W1 绑完后 `TriggerStory(进层)`；`StoryTriggerCount` 单次 | ✅ 不依赖脚碰 Trigger；对齐「一上 2 楼就播」 |
| E2 · `SimpleStoryTrigger` Enter 罩 ExitFrom / WalkArea2 入口 | 场景可见；落点漂移可能漏播 | ⚠️ 次选；须保证落点在区内 |
| E3 · 与开箱合成一图 | 产品已拆两资源 | ❌ |

**禁止**：每回从 1 楼上 2 楼都重播（除非产品改口）；大门 `Village_Chief_House_Door` 路径误播进层戏。

### 开箱对白 + Tips · 方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **B1 · HomeScene2Box 同构** | `useStoryOnOpen=true`，`storyName=Village_玩家上树屋二层触发宝箱`；图内 `ExecuteFunction` → `OnOpenBox` / `OnGetHpMp`（箱上已有公开方法） | ✅ **最贴现网箱 API**；动画+存档+Tips 不丢 |
| B2 · 图内三连×2 | 对白后 `GetItem(HpBall,3)`→`OpenTipsForm(GetHpBall)`→`GetItem(MpBall,3)`→`OpenTipsForm(GetMpBall)`；另需节点/回调播 Open 动画与写存档 | ⚠️ 可；须防双发（C# 直发路径必须关） |
| B3 · 保持 `useStoryOnOpen=false` 只对白不发奖 | — | ❌ 与产品冲突 |
| B4 · 只改 CSV 期望 Import 自带发奖 | Generated 现无发奖节点 | ❌ |

**双发防护（必写）**：Story 路径开启后，**禁止** OpenBox 里再走 else 直 `OnGetHpMp`；已开档只播 Open 态、不可交互。

### Prefab 壳

| 项 | 要求 |
|----|------|
| 路径 | `Assets/GameRes/Prefabs/Dialogue/Village_玩家上树屋二层.prefab` 与 `…触发宝箱.prefab`（名须与 `TriggerStory` 字符串一致） |
| 内容 | 挂对应 DialogueTree；对齐现网对话壳（UIAlpha / Actor）；开箱图补 Action 节点 |
| 工具 | 若有 CSV→Prefab 流水线则复用；否则 Duplicate 相近村对话 Prefab 再换 Tree |

### 与 0901 纯 C# 发奖案关系

| 0901 | 本期 |
|------|------|
| 默认无 Story，直发奖 | **产品改口：必须走对话资源** |
| `OnOpenBox` / `OnGetHpMp` 保留 | **作 Story 回调或图内等价调用**，勿删后另写第三套 |
| 存档 `tree2fHpMpBoxOpened` | **继续用** |
| 禁止改 WalkArea2 | **不变** |

### 假说 / 风险（侦探须答）

| ID | 问题 |
|----|------|
| Q1 | 进层戏触发：E1 GSM 落点后 vs E2 Trigger？ |
| Q2 | 开箱：B1 ExecuteFunction vs B2 图内 GetItem+Tips？ |
| Q3 | Prefab 是否缺失？如何最小生成两壳？ |
| Q4 | 进层单次键名？（建议 `Village_玩家上树屋二层` 进 `StoryTriggerCountData`） |
| Q5 | 从村内爬楼到 2 楼（非村长家楼梯）是否也播进层戏？**须产品默认：仅首次进入 WalkArea2 / 或仅 Chief 楼梯路径——报告写清推荐** |
| Q6 | Tips 要不要专用「×3」图？助手倾向 **否**（复用 GetHpBall/GetMpBall，与 0901 一致） |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 进层对白接线 + 开箱对白/动画 + 双球入包 + 双 Tips | ❌ 改 WalkArea2；新 Tips 系统 |
| ✅ 单次进层、单次开箱 | ❌ 西境箱脚本；龙宫 HomeScene2 逻辑搬村 |
| ✅ Prefab 壳 + 图补节点 | ❌ 只改 CSV 不建 Prefab 就当完成 |
| ✅ 与卡住/看不见案边界说明 | ❌ 用对白掩盖「箱不在场景」 |

### 严禁

- `GetItem` 不弹 Tips；或只 Tips 不入包  
- TipKey 用中文文件名；新造 TipsPanel  
- `useStoryOnOpen=true` 同时 else 直发 → **双份球**  
- 改 WalkArea2 点集；挂 `WestRappRoadHpMpBox`  
- 非战斗演出不用 `StoryComponentGSM.TriggerStory`、脚本私锁 `isTalking`  

### 对照文档 / 代码

- `@Assets/GameRes/DialogueTrees/Generated/Village_玩家上树屋二层.asset`  
- `@Assets/GameRes/DialogueTrees/Generated/Village_玩家上树屋二层触发宝箱.asset`  
- `@Assets/Dialog/Village_玩家上树屋二层.csv`  
- `@Assets/Dialog/Village_玩家上树屋二层触发宝箱.csv`  
- `VillageKenMuNi1HpMpBox.cs`（`useStoryOnOpen` / `OnOpenBox` / `OnGetHpMp`）  
- `HomeScene2Box` + `Prefabs/Dialogue/HomeScene2Box.prefab`  
- `OpenTipsFormActionTask` / `GetItemActionTask`（0830/0901 针线包）  
- `Village_KenMuNiSceneManager`（W1 落点后挂点）  
- `SimpleStoryTrigger`  
- `执行文档/0901/…巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md`  
- `执行文档/8月/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md`  
- `提示词/0903/…宝箱看不见未摆放_验收排查提示词.md`（并行）

---

## 侦探 Prompt（复制给 Agent · 先跑）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码 / 场景 / Prefab / 对话图。只读 + 写溯源报告。
默认中文。沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_架构侦探提示词.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md
@Assets/Doc/执行文档/8月/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md

## 产品
1) 进树屋二层播 Village_玩家上树屋二层。
2) 开宝箱播 Village_玩家上树屋二层触发宝箱（含开箱动画），然后获得道具 Tips：生命球×3、体力球×3（GetHpBall→GetMpBall + 入包各 3）。
用户已指定这两份 Generated DialogueTree / CSV。

## 必读
@Assets/GameRes/DialogueTrees/Generated/Village_玩家上树屋二层.asset
@Assets/GameRes/DialogueTrees/Generated/Village_玩家上树屋二层触发宝箱.asset
@Assets/Dialog/Village_玩家上树屋二层.csv
@Assets/Dialog/Village_玩家上树屋二层触发宝箱.csv
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageKenMuNi1HpMpBox.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs
样板：HomeScene2Box 实体+对话 Prefab；OpenTipsFormActionTask；GetItemActionTask。
检索：useStoryOnOpen、TriggerStory、Tree2fHpMpBox、GetHpBall、Prefabs/Dialogue/Village_玩家上树屋。

## 任务
1. 核实两 Tree 现有节点：是否仅对白？缺开箱动画/发奖/Tips 哪些？
2. 核实 Dialogue Prefab 壳是否存在；TriggerStory 名与资源名对齐规则。
3. 拍板进层触发 E1/E2（含单次、是否仅 Chief 楼梯路径）。
4. 拍板开箱 B1/B2；写清防双发；存档仍 tree2fHpMpBoxOpened。
5. 列出最小施工清单（Prefab×2、箱序列化改 Story、图补节点、进层挂点）。
6. 与 0901 直发奖、0903 卡住/看不见案边界；更新 OPEN。
7. Q6 Tips×3 专图：默认否。

## 报告
Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_架构溯源报告.md
```

---

## 施工 Prompt（根因拍板后复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_架构溯源报告.md
@Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_架构侦探提示词.md

## 目标
- 首次进巨树 2 楼：播 Village_玩家上树屋二层
- 点 Tree2fHpMpBox：播 Village_玩家上树屋二层触发宝箱 + 开箱动画 → HpBall×3 + MpBall×3 + Tips GetHpBall→GetMpBall
- 同档进层单次、开箱单次；不改 WalkArea2

## 约束
- 非战斗演出必须 StoryComponentGSM.TriggerStory
- 禁止双发奖（Story 与 C# 直发只能留一条主路径）
- TipKey 仅 GetHpBall / GetMpBall；禁止中文 Key、禁止新 Tips UI
- 禁止改 VillageWalkArea2 形状；禁止挂西境箱脚本
- 代码/对话节点须详细注释；重要改动写原因；复杂逻辑注明替代方案
- Prefab 名与 TriggerStory 字符串一致

## 落盘
Assets/Doc/施工说明/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_施工说明.md
同步 OPEN_QUESTIONS.md

## 验收
- [ ] 首次上 2 楼播进层四句对白；同档再上不重播（按报告决议）
- [ ] 点箱播三句对白；箱 Open 动画；背包 +3/+3
- [ ] 依次出现 GetHpBall、GetMpBall 花边横幅 + 物品音效
- [ ] 已开箱不可再开；读档为打开态
- [ ] Prefabs/Dialogue 下两壳存在且可 TriggerStory
- [ ] WalkArea2 未改；无双份球；大门路径不误播进层戏（若报告要求）
```

---

## 给开发者（一句话）

两份对话 Tree/CSV **已经有了**，但缺 **Dialogue Prefab 壳 + 进层触发 + 开箱改走 Story（现网箱还在无对白直发奖）+ 图里补开箱动画/Tips**；丢侦探 Prompt 拍板 E1/B1 后施工，Tips 继续用现成 `GetHpBall`/`GetMpBall`，不必新做「×3」图。
