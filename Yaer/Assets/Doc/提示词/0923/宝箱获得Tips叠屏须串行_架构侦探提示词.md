# Cursor Agent Prompt · 宝箱多道具获得 Tips 叠在一起：须一个一个出

> **角色**：【架构侦探】只读；查开箱连发获得横幅叠屏  
> **日期**：2026-09-23  
> **现象（用户 + 截图）**：`Assets/Prefabs/Box.prefab` 开箱后**获得提示叠一起了**；产品要 **一个一个出**  
> **锚点场景（预扫）**：`Village_KenMuNi1` 巨树 2 楼 `Tree2fHpMpBox`（`VillageKenMuNi1HpMpBox` + Box Prefab；Hp×3+Mp×3 → `GetHpBall`/`GetMpBall`）；西境同款亦连续双 OpenTips  
> **不是**：改发奖数量/存档旗；改 WalkArea2；改 Tips 图集美术文案；改对话内 `OpenTipsFormActionTask` 业务键名（可共用排队修法）  

把下面「侦探」整段交给 Agent。没证伪「同帧二次 OpenUIForm 双开 TipsPanel」vs「单面板队列失效」之前，不要施工。

---

## 提示词助手预梳理（须证伪）

### 产品钉死

| 项 | 要求 |
|----|------|
| 多枚 Tips | **串行**：上一张播完（或按现网 wait）再出下一张，**禁止同屏叠两张** |
| 音效 | 每条 Item Tips 仍可各自播一次（对齐现网） |
| 范围 | 至少修 Box 开箱双 Tips；若根因在 `TipsComponentGSM`，顺带覆盖西境箱 / 夏尔等连发 |

### 现网可疑（预扫，勿当终裁）

```
VillageKenMuNi1HpMpBox.OnGetHpMp / WestRappRoadHpMpBox
  → tips.OpenTipsForm("GetHpBall");
  → tips.OpenTipsForm("GetMpBall");   // 同帧连调

TipsComponentGSM.OpenTipsForm:
  if (tipsFormLogic != null && isActiveAndEnabled)
      AddTipsInfo(info);              // 入队
  else
      OpenUIForm(TipsPanel, callBack → tipsFormLogic = ...);
```

假说：

1. **主假说（竞态）**：第一次 `OpenUIForm` 的 `callBack` **尚未**赋 `tipsFormLogic`，第二次仍走 `else` → **开两个 TipsPanel** → 同屏叠  
2. `TipsFormLogic` 有 `tipsQueue` + `ShowCharCoroutine`，设计本可排队；`AddTipsInfo` 未走到则队列无用  
3. 单面板入队了但 `waitSecond`/动画导致体感叠（次查；截图更像双实例）  
4. Box Prefab 本身无 Tips 逻辑；问题在挂载脚本 + Tips GSM，勿只改 Prefab 美术  

0901 报告曾写「Tips 队列 fill」——**须用 Play 证伪现网是否真入队**。

### 强制排除

| 勿当主因 | 理由 |
|----------|------|
| 图集两张 Sprite 同图 | 叠的是 UI 实例，不是图内容 |
| 只改开箱只发一个 Tips | 产品要两个都出，只是串行 |
| 改 Box.controller 开箱动画 | 与横幅叠无关 |

---

## 侦探（复制给 Agent）

```
你是【架构侦探】。只读分析，不改代码、不改场景、不提 MR。

任务：查清 Box 开箱多 Tips「叠一起」的根因；给出最小修，满足「一个一个出」。

### 必读

@Assets/Prefabs/Box.prefab
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageKenMuNi1HpMpBox.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/WestRappRoad/WestRappRoadHpMpBox.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Tips/TipsFormLogic.cs
@Assets/Doc/执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md

Grep：`OpenTipsForm` 连续调用方（Xiaer、对话 Action、老农等）。

### A. 复现矩阵（必须填）

| 操作 | Hierarchy TipsPanel 个数 | 画面是否叠 | Console |
|------|--------------------------|------------|---------|
| 村 2 楼 Box 开箱（Hp+Mp Tips） | ？ | ？ | ？ |
| 西境 HpMp Box（若可达） | ？ | ？ | ？ |
| 对话里间隔较远的两次 OpenTips | ？ | ？ | 对照：非同帧 |
| 临时只调一次 OpenTipsForm | 1 | 不叠 | 基线 |

记录：第二次调用时 `tipsFormLogic` 是否仍 null。

### B. 对拍

1. `OpenTipsForm`×2 同帧是否双开 Form？`callBack` 赋引用时机？  
2. `AddTipsInfo` → `UpdateInfo` enqueue + coroutine 是否本可串行？为何没走到？  
3. `OnClose` `tipsQueue.Clear()` 是否在双开时互相踩？  
4. 最小修挂点：`TipsComponentGSM`（待开队列 / 合并 Open）vs 仅箱脚本 Delay？  
5. 影响面：所有 `OpenTipsForm` 连发调用方列表。

### C. 方案对比

| 方案 | 做法 | 何时选 |
|------|------|--------|
| **A** | GSM：若 Panel 正在 Open/回调未到，把后续 key **先入待开队列**，首屏 Open 回调里一次性 `AddTipsInfo` | **主推**（治根） |
| **B** | 同步/阻塞等到 `tipsFormLogic` 再 Add（慎：卡帧） | 备 |
| **C** | 仅箱脚本 `WaitForSeconds` 再第二次 Open | 治标；其它连发仍叠 |
| **D** | 合并成一张「获得 Hp+Mp」图 | **否决**（产品要各出） |

必答：主因一句话；推荐方案；文件路径；验收「开箱两横幅严格先后、同屏只有一张」。

### D. 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 村 Tree2f Box 开箱 | GetHpBall 播完（或间隔）再 GetMpBall；**不叠** |
| 2 | Hierarchy | 同时最多 **一个** TipsPanel（或单实例轮换） |
| 3 | 背包 | Hp+3、Mp+3 仍正确 |
| 4 | 西境同款箱（抽测） | 同样串行 |
| 5 | 单条 Tips（剑/针线包） | 不回潮 |
| 6 | 对话连续 OpenTipsFormActionTask（若有近距离双发） | 写清是否同修覆盖 |

### E. 报告

① 结论  
② 复现（双 Form vs 队列坏）  
③ 调用链  
④ 方案  
⑤ 路径  
⑥ 验收  
⑦ 风险  

写入：`Assets/Doc/执行文档/0923/宝箱获得Tips叠屏须串行_架构溯源报告.md`
```

---

## 施工员（侦探闭环后再复制）

```
@Assets/Doc/执行文档/0923/宝箱获得Tips叠屏须串行_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Tips/TipsFormLogic.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageKenMuNi1HpMpBox.cs

你是【施工员】。按报告最小修：开箱等多 Tips 一个一个出，禁止叠屏。

约束：
- 优先修 Tips 公共入队/防双开（方案 A 类）；勿只给村箱加 Delay 当唯一交付（除非报告证伪仅此一处）
- 保留 GetHpBall → GetMpBall 两条横幅与入包数量
- 勿改 Box.prefab 美术/Animator 当主修
- 写入：`Assets/Doc/施工说明/0923/宝箱获得Tips叠屏须串行_施工说明.md`
```

---

## 使用顺序

1. 复制「侦探」→ Play 开箱看 TipsPanel 是否开了两个  
2. 确认竞态后用方案 A  
3. 复制「施工员」→ 村箱 + 西境抽测
