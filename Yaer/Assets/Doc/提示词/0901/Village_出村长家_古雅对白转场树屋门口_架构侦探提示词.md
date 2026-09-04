# Cursor Agent Prompt · 出村长家 → 古雅对白 → 转场树屋门口续聊道别

> **角色**：先【架构侦探】只读定方案，报告后再【施工员】  
> **日期**：2026-09-01  
> **产品设定（钉死 · 台本）**：  
> **从村长家出来**后开始对话（古/雅），对白中段 **主角与屏幕转场到雅尔树屋门口**，再续古莎道别与晚饭约定。  
> **台本全文见下文「产品台本」**  
> **地理**：村场景 `Village_KenMuNi1`；树屋门口锚点倾向 **`House_Tree`**（现绑 `Village_TreeHouseLock`，约 `(9.23,-7.5)`）  
> **不是**：进树屋室内 Scene（现网无）；不是巨树 2 楼 `VillageWalkArea2` 宝箱线；不是晚宴台本整段替换  
> **本阶段（侦探）**：只读；禁止改场景 / 代码 / Prefab  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_出村长家_古雅对白转场树屋门口_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 产品台本（钉死 · 勿擅自改文案）

### 段 A · 出村长家后（尚在村长家门外一带）

| 说 | 文 |
|----|-----|
| 古 | 雅尔，雅尔真觉得自己不是那个女神吗？ |
| 雅 | 是啊，我感觉自己只是碰巧穿了一身白衣服，我的能力不行，年纪也小，出行目的也不是拯救什么，你们一定是认错了。 |
| 雅 | 也许将来会有一个更吻合的。 |
| 古 | 。。。。。。 |
| 雅 | 我不想让你们失望，可我真没有能力承担拯救沧桑这么重的责任。 |
| 古 | 没关系，现在的日子也挺好的。 |
| 古 | 只是周遭有点危险，小心一些也没什么。 |

### 转场（产品硬要求）

> **主角人物和屏幕转场到雅尔树屋门口**

### 段 B · 树屋门口

| 说 | 文 |
|----|-----|
| 古 | 那，就送你到这里了，我先回去了。 |
| 古 | 同样时间来村长家吃饭吧。 |
| 雅 | 好。 |

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望）

```
玩家从 Village_Chief_House 出门 → Village_KenMuNi1
  →（落点：村长家门前一带 —— 须与「楼梯上 2 楼」落点拆开，见 OPEN）
  → 自动 TriggerStory（段 A：女神质疑）
  → 段 A 末
  →【转场】黑幕/Loading：玩家根位 + 镜头 → 雅尔树屋门口（House_Tree 旁）
  → 段 B：道别 + 晚饭约定
  → onStoryEnd 还控；存档单次
```

**禁止**理解成：出屋后无对白自己走到树屋再点古莎；或段 A/B 拆成玩家可中途乱跑。

### 「出村长家」出口裁定（必答）

| 假说 | 含义 | 倾向 |
|------|------|------|
| **O1 · 1 楼门 LeftDoor / House_Chief 对称出门** | 落在村长家门前 → 播本对白 → 再转场树屋 | ✅ 台本「出来」+「送到树屋」地理通 |
| O2 · 楼梯上巨树 2 楼 | 落 `ExitFrom_HomeSceneChief2f` | ❌ 与「树屋门口」距离/语义不符；2 楼另有宝箱线 |
| O3 · 续聊结束直接室内黑幕送出 | 不经门 | ⚠️ 可作备选；产品写「出来」偏 O1 |

**与 0901 楼梯换场冲突**：若 EnterPos `Village_Chief_House` 已绑 **2 楼**，则 1 楼门也会落 2 楼——侦探必须在报告里 **拆开 1 楼出门落点**（新建 `ExitFrom_HomeSceneChief` 门前）与 2 楼楼梯落点，否则本戏无法在「门前」开场。

### 转场方案倾向（同场景内）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **T1 · 对白中 Action：黑幕 → SetPlayerPos(树屋门口) → 镜头对齐 → 亮幕 → 续 Statement** | 不换 Scene；符合「屏幕转场」 | ✅ |
| T2 · 段 A 结束 LoadScene 回村再落树屋 | 已在村内则多余；且 lastScene 乱 | ❌ 若已在 KenMuNi1 |
| T3 · 段 A/B 两个 Story，GSM 黑幕串 | 可；多一次 Trigger | 次选 |

须点名：现网是否已有 **对话内传送玩家** Action；无则最小新建（对齐 BlackPanel ShowFade 回调内写坐标）。  
落点建议：`House_Tree` 旁可走点（WalkArea 内），勿塞进锁门 Collider 里卡死。

### 对话资源

| 项 | 倾向 |
|----|------|
| Story / Prefab / CSV 名 | **`Village_出村长家送树屋`**（或报告定名，三者一致） |
| Speaker | **古 / 雅**（无村长句） |
| 立绘 | 倾向雅+古双人大立绘（对齐 KenMuNiStart / 门口戏减村长）；Mask 跟现网 |
| 单次 | `StoryTriggerCountData` / SingleUse |

CSV 须新建（台本上表）；Import → Generated → Prefab Setup。

### 触发挂点倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **G1 · KenMuNi1 GSM OnEnterScene** | `lastScene==Village_Chief_House` ∧ 来自 **1 楼门**（非 2 楼）∧ 本档未播 → TriggerStory | ✅ 对齐进村开场 / 进屋续聊 |
| G2 · LeftDoor 出屋 callBack | 门切换后立刻 Trigger | ⚠️ 与 EnterPos/Ready 竞态 |
| G3 · 门前 Trigger 体积 | 出屋落地走进区再播 | 可防误触；多一步 |

**2 楼楼梯回来** 不得误播本戏（用落点分区 / 门类型旗 / 不同 lastScene 伪名——报告拍板）。

### 与现网解耦

| 系统 | 关系 |
|------|------|
| `House_Tree` / `Village_TreeHouseLock` | 转场后仍可点树屋播锁对白；本戏 **不**改成进树屋 |
| 门口初次 / 室内续聊 / 针线包 | 上游已播完；本戏是 **出屋后下一环** |
| 巨树 2 楼宝箱 | 正交；勿绑在本 Trigger |
| 古莎场景侧面 / 动画合层 | 出屋是否显示场景古莎——OPEN |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 出村长家（1 楼）→ 自动段 A → 转场树屋门口 → 段 B | ❌ 进树屋室内 Scene |
| ✅ 新建 CSV/Prefab/单次旗 | ❌ 改晚宴整本台本 |
| ✅ 拆清 1 楼门前落点 vs 2 楼楼梯落点 | ❌ 改 WalkArea2 尺寸 |
| ✅ 转场遮罩防露脚穿帮 | ❌ Update 堆业务 |

### 严禁

- 段 A 播完还控让玩家自己走去树屋再点人  
- 2 楼下来也播「出村长家送树屋」  
- 转场无遮罩硬闪坐标  
- Story 名与 Prefab 名不一致导致加载失败  

### 开放（侦探须答）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 出门落点 O1？ | **是**；须补门前 ExitFrom |
| Q2 | 转场 T1 黑幕？ | **BlackPanel**（同场景）；非 Loading 换场 |
| Q3 | 一段 Prefab 还是 A/B 两段？ | **一段图 + 中段传送 Action** |
| Q4 | 树屋落点精确坐标？ | `House_Tree` 旁 Walk 内；Scene 微调 |
| Q5 | 场景古莎要不要跟着到树屋？ | 倾向转场后可关/不强制；报告写 |
| Q6 | 「同样时间来吃饭」是否接晚宴旗？ | 本期只对白；晚宴触发另案 |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
从村长家出来后自动播古/雅对白（段 A）；
对白中「主角人物和屏幕」转场到雅尔树屋门口；
再播道别+晚饭约定（段 B）。台本以提示词正文为准，勿改文案。

## 必读
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构溯源报告.md
@Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md
@Assets/Doc/施工说明/0830/Village_KenMuNi1_House_Tree交互Village_TreeHouseLock_施工说明.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Scenes/Village_Chief_House.unity
（LeftDoor、EnterPos、House_Tree、ExitFrom_HomeSceneChief2f）

检索：House_Tree、TreeHouseLock、EnterPos、Village_Chief_House、SetPlayerPos、
ShowFade、TriggerStory、StoryTriggerCountData、LoadSceneTaskAction。

## 侦探任务
1. 拍板「出村长家」= O1 门前；与 2 楼楼梯 EnterPos 如何拆分。
2. 设计：出屋 → 自动播段 A → 中段黑幕传送至 House_Tree 旁 → 段 B；挂点 G1/T1。
3. 对话资源命名、双人立绘、CSV 结构、单次存档。
4. 转场 Action 是否现成；镜头/玩家坐标/WalkArea 夹紧风险。
5. 与 House_Tree 锁对白、2 楼宝箱、室内续聊的解耦。
6. 最小清单 + 验收 + OPEN。

## 报告落盘
Assets/Doc/执行文档/0901/Village_出村长家_古雅对白转场树屋门口_架构溯源报告.md

结构：①结论 ②出门落点裁定 ③全时序 ④转场方案 ⑤对话资源
⑥与 2 楼/树屋锁解耦 ⑦最小清单 ⑧验收 ⑨OPEN ⑩程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_出村长家_古雅对白转场树屋门口_架构溯源报告.md

## 目标
1. 从村长家 1 楼出门回村后，自动播段 A 台本（文案以报告/提示词为准，勿改字）。
2. 段 A 后黑幕转场：玩家+屏幕到雅尔树屋门口（House_Tree 旁），再播段 B。
3. 同档单次；拆清与巨树 2 楼楼梯落点，避免 2 楼回来误播。
4. 新建 CSV/Generated/Prefab；禁止进树屋室内 Scene；禁止改 WalkArea2 尺寸。

## 落盘
Assets/Doc/施工说明/0901/Village_出村长家_古雅对白转场树屋门口_施工说明.md
同步 OPEN_QUESTIONS；CSV 建议 Dialog/ 下与 Story 同名。

## 验收
- [ ] 1 楼出门 → 自动段 A（女神质疑台本正确）
- [ ] 中段转场有遮罩；亮后人在树屋门口一带
- [ ] 段 B 道别+晚饭句完整；还控
- [ ] 同档再出村长家（1 楼）不重播
- [ ] 楼梯上 2 楼回来不误播本戏
- [ ] House_Tree 仍可点出锁对白
- [ ] Console 无加载对白 Prefab 失败

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先跑侦探**——尤其拍板：出门落在 **村长家门前**（不要和巨树 2 楼落点抢 EnterPos）。  
2. 结构：**一段对白 + 中间黑幕传送到 `House_Tree`**，不是进树屋场景。  
3. 台本已钉在提示词里，施工勿改中文台词。
