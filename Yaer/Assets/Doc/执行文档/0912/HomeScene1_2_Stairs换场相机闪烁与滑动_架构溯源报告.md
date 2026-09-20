# HomeScene1 ↔ HomeScene2 · Stairs 换场相机闪烁 / 左右滑动 — 架构溯源报告

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**场景（钉死）**：龙宫室内 `HomeScene1` / `HomeScene2`（**不是** `Village_HomeScene*`）  
**触发物**：`Object/Stairs`（两侧均有）  
**现象**：1→2 揭幕「闪一下」；2→1 镜头「从左往右滑」  
**产品期望**：黑幕揭开时镜头已在正确机位；**无闪帧、无可见滑动**  
**提示词**：`Assets/Doc/提示词/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构侦探提示词.md`  
**规范对照**：`02_SYSTEM_SPEC.md` §3；`技术文档/场景相关/场景切换.md`

---

## 沟通摘要

### ① 结论一句话

**闪与滑同源**：进场 `SetFollow(forceSnap=true)` 在 `smoothTime≈0.3` 下手推 VCam（Follow 暂空），黑幕只 hold **0.3s** 就开始淡出，**对齐未完成就露景**；双向观感不同，是因为两侧 **场景默认 VCam 位 ↔ Stairs 落点** 的位移差差了一个数量级（2→1 约 20 单位横移 → 明显左→右滑；1→2 几乎只差 Y → 更像闪一下）。

### ② 原因（通俗）

楼梯换场走标准黑幕门。新场景一就绪，相机不是「直接卡在玩家身上」，而是从场景里 **摆好的旧机位** 用平滑手推去追玩家；黑幕又揭得偏早。  
下楼（2→1）旧机位在地图很左边、人落在楼梯中部 → 你看见镜头从左挪到右。  
上楼（1→2）旧机位 X 已经贴着落点、主要差竖直 → 挪得短，更像「闪一下再定」。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 期望 |
|---|------|------|
| 1 | Hierarchy 选 `HomeScene1/Camera` 上的 `CameraComponent` | 未见序列化 `smoothTime` 时运行时仍为脚本默认 **0.3**（与代码字段一致） |
| 2 | 选 `HomeScene2/Camera` → `CameraComponent` | 磁盘已写 **`smoothTime: 0.3`** |
| 3 | 看两侧 `Cinemachine` VCam 世界坐标 vs Stairs 落点 | HS1 VCam≈**(-19.51,0)** vs 落点≈**(0.17,-3.65)**；HS2 VCam≈**(12.72,0)** vs 落点≈**(12.59,-3.21)** |
| 4 | Stairs 按 E 换场，过滤 Console `SceneChangeDoor` / `SceneLoad` | `blackFade=true`；`next=HomeScene1|2`；非 Loading |
| 5 | 同场景只走路（不换场） | 日常 Follow 阻尼 **不是**本案「揭幕瞬间的大滑」 |
| 6 | （施工前自测）临时把两侧 `smoothTime` 改 **0** 再双向 Stairs | 若闪/滑同时消失 → 坐实本报告；**测完请改回，勿当正式施工** |

### ④ 程序补充

见下文 §1～§7。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **嫌疑 A + B 合并成立**：`CameraComponent.SetFollow` 在 `forceSnapToTarget && smoothTime>0` 走 `_isSmoothingSnap` 手推；`LoadSceneComponentGSM` Ready 后仅 `mapTransitionBlackHoldSeconds`（默认 **0.3**）再 `CloseFormFade`，**不等** `SetFollow` 的 `onComplete` |
| **闪（1→2）** | 同源手推未收束 + 位移以 **Y≈3.2** 为主（X 几乎贴齐）→ 露景时像短促跳变/闪帧；HS2 Framing **`DeadZoneHeight=1` / `YDamping=0`** 可能加重绑 Follow 后的二次校正（高度可疑） |
| **滑（2→1）** | 同源手推；HS1 默认 VCam **x=-19.51** → 落点 **x≈0.17**，Δx≈**19.7** → 黑幕淡出窗口内可见 **左→右** SmoothDamp |
| **是否同源** | **是**（同一契约缺口）；表现差来自 **默认机位↔落点距离不对称**，不是两套无关 bug |
| **落点** | 两侧 `EnterPosConfig` **命中**（`lastScene` 配对正确）；落点合理，**不是**「落错点再追楼梯」主因 |
| **剧情二次改相机** | **已排除**（Stairs 路径 LastScene 为对侧楼层；`HomeScene1FirstEnter` / `ChangeClothesSceneExit` 条件不满足） |
| **推荐修复** | **方案 A**：两侧龙宫 `CameraComponent.smoothTime = 0`（进场当帧 `ApplyFollowWithCinemachineStateAligned`）→ 揭幕即定格；改动面最小、符合「定格」产品语 |

---

## 2. 调用链（错误机位可能露出的帧窗口）

```
Stairs (SceneChangeDoor)
  NextSceneName = HomeScene2 | HomeScene1
  ShowLoadingUI = false
  TriggerWhenMoveIn = false
  → EnterDoor → LoadSceneComponentGSM.LoadScene(name, null, blackFade:true)
        │
        ▼
BlackPanel FadeShow ──全黑──► onShowEnd
  → 旧 GSM OnExit/OnShutDown
  → ChangeSceneComponentGM.LoadScene（LastSceneName = 卸场前 Now）
  → 加载 Assets/GameRes/Scenes/{HomeScene1|2}.unity
        │
        ▼
新 BaseGameSceneManager.Awake → OnInit → InitPlayer
  → CreatePlayer 回调：
       SetPlayerPos（EnterPosConfig 按 LastSceneName）
       CameraComponentGSM.SetFollow(player)   // forceSnapToTarget 默认 true
            │
            ├─ smoothTime > 0：
            │     Follow=null；_isSmoothingSnap=true
            │     LateUpdate 多帧 SmoothDamp(vcam → player.xy + followSnapOffset)
            │     ★ 此时画面若已揭开 = 可见滑动 / 未到位闪帧
            │
            └─ smoothTime ≈ 0：
                  当帧 ApplyFollowWithCinemachineStateAligned → 定格
        │
        ▼
首帧 Update：initAsyncCounter Done → OnGameSceneManagerReady
  → hold mapTransitionBlackHoldSeconds（默认 0.3s）
  → CloseFormFade（黑幕淡出）★★ 错误机位窗口从这里打开 ★★
  → OnBlackFadeEnd → OnEnterScene
```

**窗口定义**：从 `CloseFormFade` 开始淡出，到 `_isSmoothingSnap==false` 且（可选）CM Framing 稳定为止。  
现网：**hold 0.3s ≪ 手推收束时间**（`smoothTime=0.3` 的 SmoothDamp 实际常需更久才到阈值；另有 `maxHandSnapRealSeconds=2.5` 兜底）。

---

## 3. 证据表

| # | 证据 | 状态 | 说明 |
|---|------|------|------|
| E1 | `SceneChangeDoor.EnterDoor` → `LoadScene(..., true)` | **已证实** | Stairs：`ShowLoadingUI=0`；走黑幕非 Loading |
| E2 | HS1 Stairs：`NextSceneName: HomeScene2`；在 `sceneObjs` | **已证实** | 场景内嵌实例；`fileID: 7564793752969083927` 在 `sceneObjs` |
| E3 | HS2 Stairs：Prefab `Assets/Prefabs/Stairs.prefab`，`NextSceneName: HomeScene1`；在 `sceneObjs` | **已证实** | 实例修改仅 Transform；目标名来自 Prefab |
| E4 | `InitPlayer`：`SetPlayerPos` 后立刻 `SetFollow(player)`（无 onComplete） | **已证实** | `BaseGameSceneManager.cs` |
| E5 | `SetFollow`：`smoothTime>0` → 手推 + 清 Follow | **已证实** | `CameraComponent.cs` |
| E6 | HS2 `CameraComponent.smoothTime: 0.3` | **已证实** | 场景 YAML |
| E7 | HS1 `CameraComponent` YAML **未序列化** `smoothTime` | **已证实** | 仅 `cinemachineBrain`/`virtualCamera`；运行时取脚本默认 **`smoothTime = 0.3f`** |
| E8 | 黑幕 Ready → hold → `CloseFormFade`，**不订阅** SetFollow onComplete | **已证实** | `LoadSceneComponentGSM.cs` |
| E9 | EnterPos 配对 | **已证实** | HS1：`lastScene: HomeScene2` → Stairs/`Pos`；HS2：`lastScene: HomeScene1` → Prefab `Pos` |
| E10 | 默认 VCam ↔ 落点距离（见 §4） | **已证实（磁盘算）** | 解释双向观感差 |
| E11 | Framing 不对称（HS1 X/Y Damping=1 / DeadZoneH=0；HS2 X=0.2 Y=0 / DeadZoneH=1） | **高度可疑** | 加重 1→2「闪」的二次校正；非滑动主因 |
| E12 | `OnEnterScene` 剧情改相机 | **已排除** | Stairs LastScene≠`NewGameScene`/`SelectClothesScene` |
| E13 | 落点配错导致「从错误落点再追」 | **已排除为主因** | 落点即楼梯口 Pos；滑动起点是 **场景默认 VCam**，不是错误出生点 |
| E14 | Play 实测帧日志（`[HomeStairsCamDebug]`） | **未测（侦探无 Play）** | 施工/验收补；磁盘链已足够拍板方案 |

### 3.1 Stairs / 换场字段（磁盘）

| 场景 | 组件形态 | NextSceneName | ShowLoadingUI | TriggerWhenMoveIn | sceneObjs |
|------|----------|---------------|---------------|-------------------|-----------|
| HomeScene1 | 场景内嵌 `SceneChangeDoor` | `HomeScene2` | 0 | 0 | 含 Stairs `SceneEntity` |
| HomeScene2 | Prefab 实例（guid `bf2a028c…`） | Prefab 默认 `HomeScene1` | 0 | 0 | 含 stripped `SceneEntity` |

### 3.2 相机关键字段（磁盘）

| 字段 | HomeScene1 | HomeScene2 |
|------|------------|-------------|
| `CameraComponent.smoothTime` | 未写入 YAML → **默认 0.3** | **0.3** |
| VCam OrthoSize | 5.4 | 5.4 |
| VCam 默认 LocalPos | **(-19.51, 0, -10)** | **(12.72, 0, -10)** |
| Framing `m_XDamping` | **1** | **0.2** |
| Framing `m_YDamping` | **1** | **0** |
| Framing `m_DeadZoneHeight` | **0** | **1** |
| `mapTransitionBlackHoldSeconds` | 组件默认 **0.3**（场景未见覆盖） | 同左 |

### 3.3 落点世界坐标（由 Transform 相加，误差可忽略）

| 方向 | LastScene | 落点 Transform | 估算世界坐标 | 默认 VCam (x,y) | Δ 约 |
|------|-----------|----------------|--------------|-----------------|------|
| 2→1 | HomeScene2 | HS1 `Stairs/Pos`：Stairs(0.169,1.938)+Pos(0,-5.588) | **(0.169, -3.65)** | **(-19.51, 0)** | Δx≈**+19.7**，Δy≈-3.7 |
| 1→2 | HomeScene1 | HS2 Object(-5.996,1.797)+Stairs(19.396,-0.003)+Pos(-0.81,-5) | **≈(12.59, -3.21)** | **(12.72, 0)** | Δx≈**-0.13**，Δy≈**-3.2** |

---

## 4. 双向差异（为何一闪一滑）

| | 1 → 2（上楼 · 偏闪） | 2 → 1（下楼 · 偏左右滑） |
|--|----------------------|---------------------------|
| 目标场景 | HomeScene2 | HomeScene1 |
| 手推起点 | VCam≈(12.72, 0) | VCam≈(-19.51, 0) |
| 手推终点 | 玩家≈(12.59, -3.21) | 玩家≈(0.17, -3.65) |
| 可见运动 | 几乎无横向；主要竖直短距 → **闪/跳** | 大幅 **左→右** 横移 → **滑动** |
| Framing 加重 | DeadZoneH=1 / YDamp=0 → 绑 Follow 后 Y 校正易「再跳一下」 | Damping 高，手推结束后仍可能软跟，但主观已是「大滑」 |
| 黑幕 | 同契约：hold 0.3 后淡出，**均不等**手推结束 | 同左 |

**一句话**：同一条「未对齐就揭幕」的缝，乘上「机位差」放大系数 → 一边闪、一边滑。

---

## 5. 修复方案对比（只方案，不施工）

### 方案 A — 龙宫两侧 `smoothTime = 0`（**推荐**）

| 项 | 内容 |
|----|------|
| 做法 | `HomeScene1` / `HomeScene2` 的 `CameraComponent.smoothTime` 显式设为 **0**（HS1 补序列化字段） |
| 效果 | 进场 `SetFollow` 走当帧 `ApplyFollowWithCinemachineStateAligned`；揭幕时已在落点机位 |
| 改动量 | **仅两场景序列化**；零代码 |
| 风险 | 进这两场景的 **所有** 换场（含森林门、换装回二楼）都会瞬切跟拍——对「进门定格」通常是加分；若某条剧情依赖进场平滑追镜，需单独验收 |
| 是否符合 SPEC §3 | **符合**：仍走 `SetFollow`；不用固定 Wait；瞬切是 API 已支持分支 |
| 验收 | 双向 Stairs ≥2 次：无闪、无左→右滑；同场景走路跟拍仍正常；Forest→HS1、SelectClothes→HS2 抽测无异常 |

### 方案 B — 黑幕淡出延后到 `SetFollow` onComplete

| 项 | 内容 |
|----|------|
| 做法 | `InitPlayer` 把 `CloseBlack` 挂到 `SetFollow(..., onComplete)`；或 `TryDeferBlackFadeForCover` 扩展到「等相机对齐」 |
| 效果 | 保留 smoothTime>0 时的平滑能力，但揭幕必在对齐后 |
| 改动量 | **代码**（LoadScene / InitPlayer / 可能 GSM 虚方法）；影响面大于本案 |
| 风险 | 手推超时/目标移动导致揭幕变慢；须与现有 `mapTransitionBlackHold`、村开场 Defer 共存 |
| SPEC | **符合**（用 onComplete，禁死 Wait 硬匹配） |
| 适用 | 想全项目统一「对齐再揭幕」时再上；本案过重 |

### 方案 C — 只挪场景默认 VCam 靠近 Stairs 落点

| 项 | 内容 |
|----|------|
| 做法 | 把 HS1 VCam 从 x=-19.51 挪到 ≈0.17；HS2 已接近，可微调 Y |
| 效果 | 缩小手推距离，滑变短；**不消灭**「Follow 清空 + 多帧手推」契约缝 |
| 风险 | 仍可能短闪；编辑器里 VCam 预览位变化；治标 |
| 建议 | **不单独采用**；可作 A 的辅助美观，非必须 |

### 方案 D — 仅 Stairs 特殊瞬切

| 项 | 内容 |
|----|------|
| 做法 | 门/GSM 识别 Stairs 路径临时 `smoothTime=0` 或 `forceSnap` 特判 |
| 风险 | 分支多、易漏；与「保持架构简单」冲突 |
| 建议 | **否** |

**推荐**：**方案 A**。产品语是「直接定格」；龙宫室内进场不需要 0.3s 手推秀。若验收发现某条剧情进场需要平滑，再对那条 Story 显式 `SetFollow(..., forceSnap:false)` 或临时改 smoothTime，而不是让默认进场永远手推。

**可选加强（A 之后仍闪再做）**：把 HS2 Framing 的 `m_DeadZoneHeight` 向 HS1 靠拢（0）并给一点 `YDamping`，减少绑 Follow 后的 Y 跳变——属配置微调，另记验收项。

---

## 6. OPEN_QUESTIONS

已写入 `Assets/Doc/OPEN_QUESTIONS.md`（本节摘要）：

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 是否采用方案 A（两侧 smoothTime=0）？ | **是** | 待产品/开发确认 |
| Q2 | Forest→HS1、换装→HS2 是否允许同样瞬切？ | **允许**（与定格一致） | 待确认 |
| Q3 | HS2 Framing DeadZoneH=1 是否本期一并改？ | **否**；A 后仍闪再开 | 待验收决定 |
| Q4 | 是否上方案 B 做全项目「对齐再揭幕」？ | **本期否** | 待确认 |

---

## 7. 复现矩阵（磁盘结论；Play 栏留给验收）

| 方向 | 操作 | 闪烁？ | 左右滑动？ | Console / 磁盘要点 |
|------|------|--------|------------|-------------------|
| 1→2 | HS1 Stairs → HS2 | **预期有（短）** | 预期弱/无横滑 | LastScene=HomeScene1；Δ 主 Y |
| 2→1 | HS2 Stairs → HS1 | 可能伴随 | **预期强左→右** | LastScene=HomeScene2；Δx≈20 |
| 对照 | 同场景走路 | — | 日常阻尼 ≠ 揭幕大滑 | 无 BlackPanel 契约 |
| 对照 | HS2 `Door`→SelectClothes | 非本案 | — | 另一 EnterPos；可抽测 A 回归 |

**最短 Play 路径**：Init/读档进龙宫可走 → Stairs 上楼 → 立刻 Stairs 下楼；过滤 `SceneChangeDoor`、`SceneLoad`。

---

## 8. 限制与非范围

- 未改代码 / Prefab / 场景 / Git。  
- 未扩大到村庄纵深相机、村长家 Stairs、战斗镜头。  
- 未在 Editor Play 打帧日志；根因以调用链 + 场景序列化距离差为准，验收用 `[HomeStairsCamDebug]` 可二次坐实。
