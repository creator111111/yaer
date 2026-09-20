# Cursor Agent Prompt · ForestScene 门口演出：保留相机移动 + 消闪 + 修好主角继续说话

> **角色**：先【架构侦探】核对回归根因（只读）；拍板后【施工员】修复  
> **日期**：2026-09-12（产品纠偏 · 相对同日 A′/B′ 施工）  
> **场景**：`ForestScene` 门口林恩链  
> **产品纠偏（钉死 · 高于旧报告期望）**：  
> 1. **摄像机要移动过去**（保留原来的平滑移动手感）—— **不要**再做成当帧瞬切/定格  
> 2. 旧问题只是移动过程 **会闪一下** —— 要消闪，但 **不能**以取消移动为代价  
> 3. **相机移动结束后，主角要接着说话**（接上 `ForestSceneYaerAfterLinEnStory`）  
> 4. **现网 Bug**：移动完了（或本该移动完）**主角不会继续说话** —— 必须修好这条接续  
> **不是**：把龙宫 Stairs「smoothTime=0 定格」搬到 Forest；改 ForestEast；改台词立绘  
> **前置施工（须视为嫌疑，可回滚）**：`施工说明/0912/ForestScene_演出结束相机定格去二次snap_施工说明.md`（A′ 临时 `smoothTime=0` 瞬切 + B′ `OnCameraMoveEnd` 幂等）  
> **报告落盘**：`Assets/Doc/执行文档/0912/ForestScene_保留相机移动_消闪与接话中断_架构溯源报告.md`

把下面整段交给 Cursor Agent。若侦探结论已清晰，可直接进文末【施工员】。

---

## 提示词助手预梳理（侦探须核实）

### 产品白话

> 戏播完镜头 **慢慢挪回雅儿** 是对的，以前只是挪的时候 **闪一下**。  
> 最近修完之后：要么移动没了，要么挪完了 **雅儿下一句不说了**。  
> 目标：**移动还在 + 尽量不闪 + 挪完一定接上主角继续对话**。

### 正确链路（期望）

```
ForestSceneLinEnStory 图末
  id40  OnDialogueEnd()
          → SetLock(false)
          → SetFollow(玩家, onComplete→OnCameraMoveEnd, forceSnap)  ← ★ 应有可见移动（smoothTime>0）
  id41  等待 AnimationEvent「CameraMoveEnd」   ← ★ 必须先进入等待，再收到事件
  id42  （可选）再调 OnCameraMoveEnd
  id43  清理
  id44  TriggerStory("ForestSceneYaerAfterLinEnStory")  ← ★ 主角接着说话
```

### 高度可疑：A′ 瞬切导致「事件发早 → 图卡死」

| 步骤 | A′ 施工后可能发生的事 | 体感 |
|------|----------------------|------|
| `OnDialogueEnd` 里临时 `smoothTime=0` | `SetFollow` **同步**调用 `onComplete` → **立刻** `OnCameraMoveEnd` → `TryNotify(CameraMoveEnd)` | |
| 此时图还在 id40，**尚未进入 id41 等待** | 事件发出时 **无人 Register**（脚本自己也有 Warning 文案） | |
| 图随后进入 id41 傻等 | **永远等不到** 已错过的 `CameraMoveEnd` | **主角不再说话** |
| B′ 幂等 | 若之后兜底/图再调 `OnCameraMoveEnd`，直接 SKIP，**不会再发事件** | 接话彻底断 |

> 生活类比：门铃在你走到门口前就按过了；你站门口一直等门铃，不会再响。

**对照**：旧版 `smoothTime=0.3` 手推多帧 → `onComplete` 较晚 → 图通常已到 id41 → 能接 `YaerAfterLinEn`；副作用是 **可见闪/滑**。

### 产品约束 vs 旧推荐方案

| 旧报告/施工 | 产品现态 |
|-------------|----------|
| 推荐 A′ 瞬切消闪 | **否决作为终态** —— 必须保留移动 |
| 推荐 B′ 幂等挡二次 snap | **可保留思路**，但不得阻断「必须发出且图能收到的 CameraMoveEnd」 |
| 龙宫 Stairs `smoothTime=0` | **禁止**套用到本门口收束 |

### 消闪（在保留移动前提下）可选方向（侦探排序，施工择一）

| 方案 | 做法 | 利弊 |
|------|------|------|
| **M1 黑幕掩护移动** | 收束前淡黑 → 手推/跟拍 → `onComplete` 揭幕并发 `CameraMoveEnd` | 符合「有移动但不露错位」；改动中等 |
| **M2 延迟发事件** | 保持平滑移动；**保证 id41 已 Register 后再** `TryNotify`；或改图：id40 只启动移动、等事件与 onComplete 对齐 | 专治接话中断；闪可能仍在 |
| **M3 回滚 A′ + 只修事件时序** | 去掉临时 `smoothTime=0`，恢复移动；另加「等待已注册再通知」或图删 id42 防双闪 | 最小纠偏；闪可能回到旧水平，再迭 M1 |
| **M4 降阻尼/缩短手推** | 略降 `smoothTime` 或 Framing XDamping，缩短可见窗 | 弱化闪，不消根；接话仍靠时序 |

**禁止**：再次以「整段收束 smoothTime=0 同步 onComplete」作为唯一修复且不处理事件时序。

### 侦探须回答

1. Play 复现：Console 是否出现「未注册 CameraMoveEnd」/ `OnCameraMoveEnd ENTER` 早于 id41？`YaerAfterLinEn` 是否从未 Trigger？  
2. 接话中断是否由 **A′ 同步 Notify 过早** 和/或 **B′ SKIP 不再发事件** 导致？（已证实/排除）  
3. 在 **保留移动** 前提下，推荐 **M1/M2/M3** 哪条最小闭环？如何同时减轻闪？  
4. id42 二次 `OnCameraMoveEnd`：保留幂等还是改 Prefab 删除？如何避免挡掉唯一一次有效 Notify？

### 必读

1. `ForestSceneLinEnStory.cs`（现网含 A′/B′）  
2. `ForestSceneLinEnStory.prefab` 图序 id40～id44  
3. `执行文档/0912/ForestScene_演出结束相机位置与屏闪_架构溯源报告.md`  
4. `施工说明/0912/ForestScene_演出结束相机定格去二次snap_施工说明.md`  
5. `CameraComponent.SetFollow`（`smoothTime>0` 异步 onComplete vs `=0` 同步）  
6. `02_SYSTEM_SPEC.md` §3（跟拍用 SetFollow + onComplete，禁死 Wait 硬匹配）

---

## 【架构侦探】任务（可与施工合并时仍先写短报告）

只读。输出：`Assets/Doc/执行文档/0912/ForestScene_保留相机移动_消闪与接话中断_架构溯源报告.md`

结构：

1. **结论一句话**（接话为何断；闪与移动如何兼得）  
2. **时序图**：id40 / SetFollow / onComplete / Register 等待 / Notify / id44  
3. **证据**（Log 文案、代码行、A′/B′ 因果）  
4. **方案对比**（须含「保留移动」约束）+ 推荐  
5. OPEN 如有记入 `OPEN_QUESTIONS.md`

---

## 【施工员】任务（侦探拍板后执行；可与上段同会话）

> **目标**：  
> 1. 门口林恩结束后镜头 **有移动** 回到玩家  
> 2. 尽量 **无穿帮闪烁**（可用黑幕或时序，禁止再靠取消移动）  
> 3. 移动结束后 **必定** `TriggerStory(ForestSceneYaerAfterLinEnStory)`，主角继续说话  

> **优先最小改动**：  
> - **回滚或改写 A′**：去掉「收束临时 smoothTime=0 同步瞬切」作为默认路径  
> - **修好 CameraMoveEnd 时序**：确保图进入「等待」之后才发事件（或等价：onComplete 仅在等待已注册时 Notify；未注册则延迟到注册/下一帧安全点再发）  
> - **B′ 幂等**：若保留，须保证 **有效的那一次** 仍发出 `CameraMoveEnd`；不得出现「唯一一次 Notify 发生在等待前，之后全部 SKIP」  
> - 消闪优先 **短黑幕掩护手推** 或 **缩短可见手推但不切零**；不要整场景 `smoothTime=0`  

> **禁止**：  
> - 再引入「无黑幕 + smoothTime=0 同步 onComplete」而不处理事件时序  
> - 在 Update 堆业务；重写整套 Cinemachine；改龙宫/村庄相机  

> **文档**：`Assets/Doc/施工说明/0912/ForestScene_保留相机移动_消闪并修复接话_施工说明.md`  
> 并在 `OPEN_QUESTIONS.md` 门口相机条下补一行：产品纠偏为「保留移动」。  

> **验收清单**：  
> | # | 操作 | 通过 |  
> |---|------|------|  
> | 1 | 新档/清 `homeDoorStoryComplete`，走完门口至林恩对白结束 | 镜头 **可见移向** 玩家（不是瞬切） |  
> | 2 | 同上 | 移动过程闪烁可接受或已用黑幕遮住（无长时间错机位裸露） |  
> | 3 | 同上 | **主角继续说话**（`ForestSceneYaerAfterLinEnStory` 出现） |  
> | 4 | Console 滤 `[CHAIN]` | 有 `OnCameraMoveEnd ENTER`；等待侧能收到事件；**无**永久卡在等 CameraMoveEnd |  
> | 5 | 抽测 Forest 日常走路 | 跟拍手感正常 |  
