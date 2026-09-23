# 宝箱获得 Tips 叠屏须串行 · 架构溯源报告

> 日期：2026-09-23  
> 角色：架构侦探（**只读**，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 现象：`Box.prefab` 开箱后获得横幅叠在一起；产品要 **一个一个出**  
> 锚点：`Village_KenMuNi1` · `Tree2fHpMpBox`（`VillageKenMuNi1HpMpBox`：GetHpBall → GetMpBall）；西境 / 夏尔同构连发  
> 提示词：`Assets/Doc/提示词/0923/宝箱获得Tips叠屏须串行_架构侦探提示词.md`

---

## ① 结论一句话

**主因是 `TipsComponentGSM.OpenTipsForm` 开屏竞态，不是队列坏、不是图集叠图。**  
同帧连调两次时：第一次走 `OpenUIForm(TipsPanel)`，`callBack` 要等 `OpenUIFormSuccess` **异步**才赋 `tipsFormLogic`；第二次仍见 `tipsFormLogic == null` → **再开第二个 TipsPanel** → 同屏两张横幅。  
`TipsFormLogic.tipsQueue` + `ShowCharCoroutine` **本可串行**，但第二次从未走到 `AddTipsInfo`，队列用不上。

0901 写「入队依次 fill」是**理想路径**（Panel 已开时）；冷启动同帧双开时不成立。推荐 **方案 A**：GSM 在「正在 Open / 回调未到」期间把后续 key 入**待开队列**，首屏回调里一次性 `AddTipsInfo`。覆盖村箱 / 西境箱 / `HomeScene1Xiaer` 及一切连发 `OpenTipsForm`。

---

## ② 复现矩阵（代码证伪；Play 勾 Hierarchy）

| 操作 | Hierarchy TipsPanel 个数 | 画面是否叠 | 第二次调用时 `tipsFormLogic` | 备注 |
|------|--------------------------|------------|------------------------------|------|
| 村 2 楼 Box 开箱（Hp+Mp） | **推演：2** | **叠** | **仍 null**（同帧） | 主路径 |
| 西境 HpMp Box | **2** | 叠 | 同左 | 同构连调 |
| 夏尔 `OnHomeScene1GoOutXiaerEnd` Mp→Hp | **2** | 叠 | 同左 | 同构 |
| 对话里间隔较远的两次 OpenTips | **1**（第二次走 Add） | 不叠 | 已非 null 且 enabled | 对照 |
| 临时只调一次 OpenTipsForm | **1** | 不叠 | — | 基线 |

**证伪单面板队列失效为主因**：队列在 `TipsFormLogic` 内；双 Form 时各有各的队列，第一张只播 Hp、第二张只播 Mp，**同屏并存**＝截图形态。  
Play 验收：开箱瞬间 Hierarchy 是否同时出现两个 `TipsPanel`。

---

## ③ 调用链

### 开箱连发

```
VillageKenMuNi1HpMpBox.OnGetHpMp / WestRappRoadHpMpBox.On…GetHpMp
  → AddMainItem(Hp) + AddMainItem(Mp)     // 入包 OK，勿改数量
  → tips.OpenTipsForm("GetHpBall");       // 同帧①
  → tips.OpenTipsForm("GetMpBall");       // 同帧②
```

```84:95:Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageKenMuNi1HpMpBox.cs
        public void OnGetHpMp()
        {
            // ...
            tips.OpenTipsForm("GetHpBall");
            tips.OpenTipsForm("GetMpBall");
        }
```

### GSM 分支（竞态点）

```26:49:Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs
        public void OpenTipsForm(string info, ETipsType tipsType = ETipsType.Item)
        {
            // ...
            if (tipsFormLogic != null && tipsFormLogic.isActiveAndEnabled)
            {
                tipsFormLogic.AddTipsInfo(info);   // 理想：入队串行
            }
            else
            {
                uiComponentGM.OpenUIForm(..., callBack = formLogic => tipsFormLogic = ...);
                // callBack 在 OpenUIFormSuccess **之后**才跑 → 同帧第二次仍进 else
            }
        }
```

`UIComponentGM.OpenUIForm`：先 `uiComponent.OpenUIForm` 拿 SerialId，再登记 `callBackActions`；成功事件里才 `Invoke` —— **非同步返回 Logic**。

### 单面板本可串行（未走到）

```
AddTipsInfo → UpdateInfo → tipsQueue.Enqueue
  → 若 coroutine==null → StartCoroutine(ShowCharCoroutine)
ShowCharCoroutine：dequeue → fill → WaitForSeconds(waitSecond) → 下一条 → Hide
OnClose：tipsQueue.Clear()   // 双开时各清各的；互踩次要
```

### 影响面（`OpenTipsForm` 连发 / 单发）

| 调用方 | 模式 | 叠风险 |
|--------|------|--------|
| `VillageKenMuNi1HpMpBox` | 同帧 Hp→Mp | **高** |
| `WestRappRoadHpMpBox` | 同帧 Hp→Mp | **高** |
| `HomeScene1Xiaer.OnHomeScene1GoOutXiaerEnd` | 同帧 Mp→Hp | **高** |
| `HomeScene2Box` / 井 / 剑 / Boss / GoOutMap | 单条 | 低 |
| `OpenTipsFormActionTask` / `AddTipsInfoActionTask` | 对白节点；近距离双发同帧则同病 | 方案 A 一并治 |

`Box.prefab` 无 Tips 逻辑；勿改 Animator/美术当主修。

---

## ④ 方案对比

| 方案 | 做法 | 何时选 | 裁决 |
|------|------|--------|------|
| **A** | GSM：`_opening` / 待开队列；Open 中后续 key 先入队；首屏 `callBack` 赋 Logic 后对余下 `AddTipsInfo` | 治根；覆盖所有连发 | **主推** |
| **B** | 同步等到 Logic 再 Add | 易卡帧 / 难接异步 Open | 备 |
| **C** | 仅箱脚本 `WaitForSeconds` 再第二次 | 治标；夏尔/对白仍叠 | **否决为唯一交付** |
| **D** | 合并一张「Hp+Mp」图 | 产品要各出 | **否决** |

### 必答

1. **主因**：同帧二次 `OpenUIForm(TipsPanel)`，非队列坏。  
2. **推荐**：**A**（公共 GSM）。  
3. **最小文件**：`TipsComponentGSM.cs`（主）；一般不动箱脚本发奖顺序；`TipsFormLogic` 仅当需在回调后补 Add 时配合，现有队列可复用。  
4. **验收**：开箱两横幅严格先后；同屏最多一张 TipsPanel；Hp+3/Mp+3 不变。

**A 实现要点（施工）**

- 缺图仍静默 return（现网）。  
- 状态：`idle` / `opening` / `ready`。  
- `opening` 时第二次：`pending.Enqueue((info, type))`，**禁止**再 `OpenUIForm`。  
- `callBack`：`tipsFormLogic = …`；`OnOpen` 已用首条 `userData.info`；再对 pending 逐条 `AddTipsInfo`（或把首条也统一走队列，避免双路径）。  
- Panel `OnClose` 后：`tipsFormLogic` 失效时靠 `isActiveAndEnabled` 已走重开；建议关闭时清空 pending、可置 null 防脏引用（可选加固）。

---

## ⑤ 要改文件（路径级；本阶段未改）

| 全路径 | 预期 |
|--------|------|
| `…/TipsComponentGSM.cs` | 待开队列 + 防双开（A） |
| `…/TipsFormLogic.cs` | 一般不动；若加固 Close 通知 GSM 清空引用可小改 |
| 箱脚本 / Box.prefab / 图集 | **不改为主修**；发奖顺序与数量保留 |

施工说明：`Assets/Doc/施工说明/0923/宝箱获得Tips叠屏须串行_施工说明.md`

---

## ⑥ 验收表

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 村 Tree2f Box 开箱 | GetHpBall 播完（或 `waitSecond` 间隔）再 GetMpBall；**不叠** |
| 2 | Hierarchy | 同时最多 **一个** TipsPanel |
| 3 | 背包 | Hp+3、Mp+3 仍正确 |
| 4 | 西境同款箱（抽测） | 同样串行 |
| 5 | 单条 Tips（剑 / 针线包） | 不回潮 |
| 6 | 夏尔结束发球（抽测） | 不叠 |
| 7 | 对白近距离双 `OpenTipsFormActionTask` | 方案 A 应覆盖；写进回归 |

---

## ⑦ 风险与回滚

| 风险 | 处置 |
|------|------|
| pending 未在失败 Open 时清空 | Failure 回调清队列 / `_opening=false` |
| 首条既在 userData 又 Add 一次 | 回调只 Add pending，勿重复首条 |
| `OnClose` Clear 与连发竞态 | 单实例后风险下降；Close 后新开走正常路径 |
| 只 Delay 村箱 | 其它连发仍叠 → 禁止当唯一修 |
| 改发奖数量 / WalkArea2 | **禁止** |

**回滚**：还原 `TipsComponentGSM`；箱与 Prefab 无改则无资源回滚。
