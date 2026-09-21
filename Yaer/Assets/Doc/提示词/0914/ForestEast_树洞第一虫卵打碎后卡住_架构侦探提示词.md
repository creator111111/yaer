# Cursor Agent Prompt · ForestEast 树洞：打碎第一个虫卵后卡死无法前进

> **角色**：先【架构侦探】只读钉死卡死层；拍板后【施工员】最小修  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene` 树洞内（用户 Scene 截图：倒树横截面、左侧洞外、人蹲在**已碎红卵**前、头顶仍有 **E**、地上绿盒、右前方平台上有幼虫）  
> **现象（用户实测）**：**第一个虫卵打碎之后**卡在原地动不了，**无法前进**  
> **产品期望（钉死）**：打碎第一颗挡路卵后，应能继续往洞内爬/蹲走；E 若是「看碎卵旁白」可点一次，**点不点都不该锁死位移**；后续卵/木虫链保持可用  
> **不是**：改死羊/吸羊；改 `CameraTreeInArea`；把整条树桥重做；把 E 旁白整段删掉交差；再拖 `GroundCenter/Up/Down` 碰运气  
> **对照（勿当唯一真相）**：  
> - 0723 树洞卵卡住：活卵靠 `GroundCld` 挡路；死后须关盒；E 是裂缝旁白不是开路键  
> - 0914 洞口爬完卡死：`CanNotSomeActionArea`（SquatUp，盒宽约 **80**）`StartAutoCrawl` 未 `Stop`  
> - 用户刚要试「洞内人 Y 再低 1」；地板三块已手动挪过  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_树洞第一虫卵打碎后卡住_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 进树洞，打烂面前第一颗卵，人就钉在碎壳前面走不动。截图上 E 还在、地上有绿框。查是碎壳还在挡、爬行锁没解开、弹出的观察对白把人锁住，还是地板/身高把人卡住。

### 截图对齐（须进 Scene 再核）

| 画面 | 现网嫌疑物体 |
|------|----------------|
| 人蹲在碎红卵前 | `WormEggType3` = `TreeBridgeLogic.spcWormEgg`，约 **(276.26, -6.36)** |
| 碎卵后仍出 **E** | `ViewBrokenEggStoryTrigger`（默认 Inactive；卵死后 `TreeBridgeLogic.Update` 才打开）。Click 对白 `ForestEastSceneViewBrokenEgg`。盒约 (276.1, 0)、Size **(3.45 × 4.5)**、Offset.y=**-5.8** |
| 地上绿矩形 | 优先核：① 刚激活的 `ViewBrokenEgg` Trigger；② 卵 `GroundCld`（Prefab Size≈6.61×4.99、Scale 0.6、非 Trigger）；③ 洞内整段 `CanNotSomeActionArea` |
| 右前方平台幼虫 | 须分清：场景原有 `storyWoodWormLogicList` **还是** 卵 `woodWormObj` 在 `MonsterDeadEndEvent` 里被 `SetActive(true)` |

**第一颗卵 = Type3 / `spcWormEgg`**（从左口 x≈264 往右遇到的第一颗）。后面才是 Type2≈292、Type1≈317。不要和洞口外卡死、过桥对白搞混。

### 设计意图（预扫）

树洞通关：强制蹲爬 → **蹲击打碎挡路卵** → 继续往右。第一颗碎后允许看一眼旁白（E），然后应能爬过碎壳。  
`WormEggLogic.MonsterRealRemove` **故意不销毁**碎壳；开路靠关碰撞，不是删物体。

### 打碎后立刻发生的事（YAML/C# 预扫）

1. `WormEggLogic.OnDead` → `base.OnDead`（Body/Foot 改 Trigger；**显式 `groundCld.enabled=false`**，0723 为挡路加的）→ `SetActiveAll(false)`（**CldController.nodes 通常不含 GroundCld**）→ `WormEggBreakState`  
2. 破卵动画结束 → `MonsterDeadEndEvent` → **`woodWormObj.SetActive(true)`** + `OnBounceFromWormEgg`  
3. `TreeBridgeLogic.Update`：`spcWormEgg.IsDead && !eggStoryTrigger.activeSelf` → **打开 `ViewBrokenEggStoryTrigger`** → 头顶 E  
4. 人仍在 `CanNotSomeActionArea` 里（约 x **259～339**），`isEnableSquatUp=false`；自动爬 `Stop` 只在离区 / 3s 未进 ClimbMove / 进洞门调用

`ViewBrokenEgg`：`triggerType=Click(0)`，`SingleUseInArchive=0`，`canTouchWithPlayer=1`，`requirePlayerOverlap=1`。图里有台词节点，**不是空壳**；但 Condition 用 `TriggerCount==0` 分支，侦探须核点 E 后会不会停在 Condition、对白 UI 锁操作不结束。

### 「打碎后卡死」嫌疑优先级

| # | 嫌疑 | 为何像截图 | 查法 |
|---|------|------------|------|
| **A（主 · 碎壳 `GroundCld` 仍实心）** | 死后 `groundCld` 没关、关错实例、或动画后又被打开。盒几乎塞满爬行高度 | 人贴碎卵走不过；绿盒在卵上 | Play 卡死帧：`WormEggType3/GroundCld` **enabled / isTrigger / Layer**；脚是否顶着它 |
| **B（主 · 自动爬锁）** | 盒宽 80，洞内永远不 Exit；蹲击打卵会离开 `ClimbMove`；`isCrawling` 仍 true 时 `DisablePlayerMove`/`AutoMove` 可能留着；3s 超时只在 **从没进过 ClimbMove** 时才 Stop | 原地完全没响应，不只是「被墙挡住」 | Console `[CanNotSomeActionArea] Start` 有无成对 `Stop`；`DisablePlayerMove`、`AutoMoveState`、`IsClimbMove` |
| **C** | 碎卵瞬间打开观察 Trigger，人叠在盒上出 E。点 E 对白 `HasRunningStory` 锁住；或不点也因 Interactive/Pause 吞输入 | E 还在（设计上会出，**不能单凭 E 定罪**） | 有无对话框；`HasRunningStory`；不按 E 能否走 |
| **D** | `woodWormObj` 弹到身前，`PlayerBodyCollider` **对木虫仍 `StopMove`**（只跳过 WormEgg） | 右前方有虫 | 卡死帧木虫 Active、是否贴人、`StopMove` 是否每帧 |
| **E** | 用户降过地板 / 准备降人 Y：脚与 `GroundUp`（顶板 Y≈-7.35）或 Center 夹死；打卵只是走到夹点 | 人看起来贴地 | 卡死帧 player.y vs GroundUp 底/Center 顶 |
| **F** | 蹲击后卡在 `SquatAtk`/`SquatStay`：`isEnableSquatUp=false` 不能站；`HasMoveInput` 进不了 Climb；或 `canInStateSetPos=false` | 攻击后定住 | 状态机 Sign |
| **G** | 0723 已关 `groundCld` 但 **Layer=OnlyMapObj 的其它盒**（平台、碎壳子碰撞）仍挡 PlayerFoot | 绿盒不是 GroundCld | 列出与脚相交的全部 Collider2D |

### 复现矩阵（侦探须填）

| 操作 | 期望 | 现网 |
|------|------|------|
| 新档进洞，**不按 E**，蹲击第一卵，打完按方向 | 能越过碎壳继续爬 | |
| 同上，打完先按 E 听完旁白再走 | 旁白结束解锁，仍能前进 | |
| 卡死帧 Hierarchy | `GroundCld.enabled`；`ViewBrokenEgg` Active；`CanNotSomeActionArea.isCrawling` | |
| Console | `StartAutoCrawl`/`StopAutoCrawl`；有无对白/Pause | |
| 左口进 / 若从右口进第一颗是否仍是 Type3 | 写清 | |

### 侦探须回答

1. 「动不了」是 **位移锁**（Disable/AutoMove/Pause/对白）还是 **物理挡**（GroundCld/顶板/木虫）还是两者？证据。  
2. E 是 `ViewBrokenEgg` 还是卵自己的 Interactive？不按 E 是否同样卡？  
3. `groundCld.enabled` 在 OnDead 当下与卡死帧是否仍为 false？  
4. 与 0914 洞口自动爬、用户改地板/人 Y 是否有关？无关写明。  
5. 方案 ≥2 + 推荐最小修。硬约束：  
   - 活卵仍须挡路（不要拆存活 `GroundCld` 设计）  
   - 不要删 `ViewBrokenEgg` 旁白；最多修「激活时机 / 对白结束解锁 / 别锁移动」  
   - 不要用走廊/死羊逻辑盖树洞  
   - 不要把 `CanNotSomeActionArea` 缩到洞外却弄坏洞内必须爬  
   - 禁止无因果回滚进洞贴底

### 必读

1. `WormEggLogic.cs` / `BaseMonster.OnDead` / `WormEggType3.prefab`（`GroundCld`、`woodWormObj`）  
2. `TreeBridgeLogic.Update`（`spcWormEgg` → `eggStoryTrigger`）  
3. `ViewBrokenEggStoryTrigger` + `ForestEastSceneViewBrokenEgg.prefab` connections  
4. `CanNotSomeActionArea.cs`（Start/Stop/3s/`blockAutoCrawl`）  
5. `PlayerBodyCollider.OnCollisionMonster`（跳过卵、不跳过木虫）  
6. `SquatAtkState` / `SquatStayState`（洞内 `isEnableSquatUp=false`）  
7. 执行文档：`7月/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_*`；`0914/ForestEast_洞口爬完对白不触发卡死_*`  
8. 用户 Scene 截图 + 本提示词  

### 报告必须包含

- 卡死帧：人坐标、相交 Collider 列表、`DisablePlayerMove`、`AutoMove`、`IsClimb*`、`HasRunningStory`、`GroundCld.enabled`  
- 第一卵身份确认（Type3 / fileID `1296467376`）  
- 根因一句话 + 方案表  
- OPEN：若无法区分「没按方向只看见 E」与真锁死

---

## 【施工员】（仅拍板后）

> 按报告最小文件列表修。活卵继续挡路；碎后必须让开。  
> 若根因是 GroundCld：只保证 **死后保持关闭**（含动画结束/孵虫后）。  
> 若根因是自动爬：碎卵后或蹲击结束须 `StopAutoCrawl` / 清 `DisablePlayerMove`，且仍禁止在洞内站起。  
> 若根因是观察对白：保证 Click 结束解锁；**不要**改成一碎卵就强行播并卡住。  
> **禁止**：删 E 旁白交差；改死羊；改相机边界；为修卡把三块地板再乱降。  
> 文档：`Assets/Doc/施工说明/0914/ForestEast_树洞第一虫卵打碎后卡住_施工说明.md`  
> 验收：新档左口进洞 → 打碎 Type3 → **不按 E 也能越过碎壳**；按 E 旁白能走完且事后能爬；第二颗卵仍可打；出洞正常。  
> Debug：`[WormEggBreak]` 死瞬间 GroundCld；`[CanNotSomeActionArea]` Start/Stop；卡死帧打锁与相交盒。
