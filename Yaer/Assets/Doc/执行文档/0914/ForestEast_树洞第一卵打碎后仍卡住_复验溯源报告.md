# ForestEast 树洞 · 第一卵打碎后仍卡住 — 复验溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读复验；**未改**代码 / 场景 / Prefab / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` 树洞内 · Type3/`spcWormEgg`≈(276.26, -6.36)  
**现象**：v1.1 施工后用户验收 **仍无法越过第一碎壳**（恶性残留）  
**产品期望**：不按 E 也能越过碎壳；第二卵仍挡；出洞脚高恢复  
**不是**：再写一遍「关 GroundCld / 孵虫 Behind」交差；删 E；改死羊；回滚贴底；再拖三块地板  
**前案**：`执行文档/0914/ForestEast_树洞第一虫卵打碎后卡住_架构溯源报告.md` + `施工说明/0914/…施工说明.md`（v1.1，**未过验收**）  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_树洞第一卵打碎后仍卡住_复验_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q5  
**本会话取证限制**：本机 **无 Unity Editor / 未能 Play**；下表 A～G 以 **源码+场景 YAML 静态** 填；**H（offset=0 对比）未跑**。按提示词判定树，H 缺失则 **不能标「已 Play 钉死」**；但仍须选出唯一施工默认主因，避免再猜着叠补丁。

---

## 沟通摘要

### ① 结论一句话

**主因裁定：R1 — 洞内人仍嵌在 `GroundUp`（Layer13 实心盒）里，像顶死墙。**  
v1.1 已在仓库落地（Behind / GroundCld 双关 / 4s skip / offset 1→0.35），**不应再当新修复重做**；0.35 仍让 Body 底切入 `GroundUp` 顶约 **0.7** 单位。施工默认：**先把 `InTreePlayerYOffset` 改 `0`**；若仍夹，**只抬/改洞内段 `GroundUp` 几何**做净空（见 OPEN）。正式验收仍须用户补 Console + H。

### ② 原因（通俗）

上一版修的是「碎壳挡路、小虫贴身刹车、自动爬没停」。这些修了你还是过不去，更像人被洞顶那块实心板 **楔住**——脚降下去之后身体插进顶板，方向键怎么按都像坏了。先别再动卵和虫子脚本；先把人抬回不嵌顶，或把顶板挪开一点空档。

### ③ 用户检查清单（补 Play 证据）

| # | 操作 | 通过 / 判定 |
|---|------|-------------|
| 1 | 新档左口进洞，蹲击 Type3，**不按 E**，Pause | 看能否动 |
| 2 | Console 搜 `[WormEggBreak]` | **有** → 排除 R4；**无** → 先编译进包（R4） |
| 3 | 看 `[TreeBridgePlayerY] after=` | 现网期望约 **-6.95**（洞外≈-6.6 − 0.35） |
| 4 | Hierarchy：`GroundUp` 与 Player `Body` 是否相交 | 相交 → 坐实 R1 |
| 5 | **临时** `InTreePlayerYOffset=0` 再进洞砸卵 | **能过 → R1 结案**；仍卡再查前方虫（R3） |

### ④ 程序补充

见下文。施工文档待拍板后写：`施工说明/0914/ForestEast_树洞第一卵打碎后仍卡住_复验施工说明.md`。

---

## 1. v1.1 是否进包（R4）

| 项 | 仓库现状（2026-09-14 静态） | 用户机 |
|----|------------------------------|--------|
| `EnsureGroundCldDisabled` / `SetActive(false)` | ✅ `WormEggLogic.cs` | 须 Console 有 `GroundCld off+inactive` |
| `PlaceHatchedWormBehindPlayer` | ✅ 同侧身后，非 Away | 须 `hatch worm BEHIND` |
| `UnlockCrawlAfterEggBreak` → `StopAutoCrawlForPlayer` | ✅ | 须 `StopAutoCrawl after break` |
| `skipPlayerBodyStopMove` 4s + Body 跳过 | ✅ `WoodWormLogic` / `PlayerBodyCollider` | — |
| `InTreePlayerYOffset` | ✅ **0.35f**（注释已写 offset=1 嵌 GroundUp） | 须 `[TreeBridgePlayerY]` |

**裁定**：源码侧 **不像 R4**；若用户 Console **完全没有** `[WormEggBreak]`，才改判 R4，禁止再贴 v1.1 逻辑当新补丁。

---

## 2. 强制取证表（本会话填写）

| 证据 | 看什么 | 本会话结果 |
|------|--------|------------|
| **A** | `[WormEggBreak]` GroundCld / BEHIND / StopAutoCrawl | **未 Play**。源码三处均会打日志 → 用户机有日志则排除 R4 |
| **B** | `[TreeBridgePlayerY] after=` 脚 Y | **未 Play**。静态期望：洞外≈-6.6 → 洞内 **≈-6.95**（offset=0.35） |
| **C** | `GroundUp` ∩ Player Body/Foot | **静态计算：相交（见 §3）**。Play Contact 待补 |
| **D** | Type3/`GroundCld` activeSelf+enabled | **未 Play**。代码 OnDead + MonsterDeadEndEvent 双关；回归才再查 A |
| **E** | 孵虫世界 X vs 玩家 X | **未 Play**。代码同侧身后；左口期望虫 X &lt; 卵 X |
| **F** | 右前方平台虫 / `storyWoodWormLogicList` | 列表两条 **无** skip 字段（仅孵虫弹窗有）。**未 Play** 是否 Stay 刹 |
| **G** | cantMove / Disable / AutoMove / Climb / canInStateSetPos / Story | **未 Play**。`UnlockCrawlAfterEggBreak` 会 `StopAutoCrawl`+`canInStateSetPos=true`；R5 仅当卡死帧仍真锁 |
| **H** | 临时 offset=**0** 再砸卵 | **未测（本机无 Editor）** → 判定树未走完最后一叉 |

### 判定树应用（本会话）

```
R4：仓库有日志代码 → 默认否；用户无日志则改 R4
H：未测
→ 在「v1.1 已处理碎壳/身后虫/自动爬停」且用户仍「完全不能前进」下，
  唯一与「完全物理卡死」最贴且几何已坐实的是 R1。
R3：保留为 H 失败后的次主因（前方故事虫无 skip）。
R2：脚 Y 低于爬区盒底，但 Body 高胶囊仍可能盖住 Trigger → 次要加重，不作主修。
R5：代码路径已 Unlock；无卡死帧真锁证据 → 降级。
```

---

## 3. R1 几何钉死（静态）

### `GroundUp`

| 项 | 值 |
|----|-----|
| 路径 | `Map/GroundColliders` → `GroundUp` |
| Layer | **13 GroundUp** |
| 中心 | **(300.47, -7.35)** |
| Size | **(56.1, 2)** 实心 `isTrigger=0` |
| Y 占用 | **-8.35 ～ -6.35**（覆盖第一卵 X≈276 所在洞内段） |

### Player `Body`（Prefab，根 Scale=1）

| 项 | 值 |
|----|-----|
| Capsule | Offset **(−0.2, 2.56)**，Size **(2, 5.3)**，`isTrigger=0`，Layer **11 Player** |
| 相对根 | 底 ≈ **playerY − 0.09**，顶 ≈ **playerY + 5.21** |

### 嵌深（洞外脚按 ≈−6.6）

| `InTreePlayerYOffset` | 洞内脚 Y | Body 底 | 与 GroundUp 顶(−6.35) | 嵌深 |
|----------------------|----------|---------|------------------------|------|
| **1**（已废弃） | ≈−7.6 | ≈−7.69 | 切入 | ≈**1.34** |
| **0.35**（现网 v1.1） | ≈−6.95 | ≈−7.04 | 切入 | ≈**0.69** |
| **0**（复验默认） | ≈−6.6 | ≈−6.69 | 仍切入 | ≈**0.34** |

结论：

1. 现网 **0.35 仍嵌** → 足以解释「修卵/虫后仍完全不能挪」。  
2. **仅改 0 可能不够**（仍 ~0.34）→ OPEN 须准备 **抬/改 `GroundUp`** 第二刀。  
3. `GroundCenter` 顶≈−7.6；人无重力，脚高于地板顶是设计，**禁止再降地板顶人**。

### R2（加重，非主）

`CanNotSomeActionArea` 世界 Y ≈ **−6.18～−3.10**（中心 301.32/−5.07，Size.y≈3.08）。脚在 −6.95 时 **脚点低于盒底**；因 Body 很高，Trigger 仍可能 Stay。不作本期主修，除非 Play 证明已 Exit 且无法手爬。

### R3（次主，H 失败再用）

`storyWoodWormLogicList` 两条 **没有** `skipPlayerBodyStopMove`。孵虫 4s skip **挡不住** 右前方平台虫。`PlayerBodyCollider` 在 `IsClimbMove` 且虫在前进侧时仍 `StopMove`。截图右前方幼虫 → H 后仍卡时优先打 F。

### 已降级（勿再主修）

仅关 GroundCld、仅 Behind、仅 4s skip —— **v1.1 已做**；无日志证明没跑时禁止重复粘贴。

---

## 4. 推荐方案（唯一主因 R1）

| 步 | 动作 | 文件 | 禁止 |
|----|------|------|------|
| **1（默认）** | `InTreePlayerYOffset = 0f`；注释写清：0.35 仍嵌 GroundUp；出洞仍 `+offset` | 仅 `ForestEastTreeEnterTrigger.cs` | 改回 1；再降 GroundCenter |
| **2（若 H/验收仍卡）** | **只动树洞段 `GroundUp`**：缩小 `m_Size.y` 或平移 Y，使在目标脚高下 **Body∩GroundUp 嵌深≤0**（数值见 OPEN） | 仅 `ForestEastScene.unity` 的 `GroundUp` | 动倒树贴底相机；全局 Physics 矩阵；删 E |
| **保留 v1.1** | Behind / GroundCld 双关 / UnlockCrawl / 4s skip **全部保留** | — | 再叠一套重复卵逻辑 |

**文件列表 ≤3**：

1. `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/ForestEastTreeEnterTrigger.cs`  
2. （仅步 2）`Assets/GameRes/Scenes/ForestEastScene.unity` → `GroundUp`  
3. （可选 Debug，不增逻辑）沿用已有 `[WormEggBreak]` / `[TreeBridgePlayerY]`

**相对 v1.1 差什么**：v1.1 把 offset 收到 0.35 仍嵌；复验主修是 **脚高归零 + 必要时顶板净空**，不是再改卵。

---

## 5. 验收（施工后 / 用户本地）

1. Console **必有** `[WormEggBreak]`（否则先 R4）。  
2. 新档左口 → 碎第一卵 → 不按 E → **越过碎壳**。  
3. 第二卵仍挡。  
4. 出洞脚高恢复（`after` 回到进洞前档）。  
5. 若步 1 不够：卡死帧打 `GroundUp` 相交；再步 2，记录最终中心/Size。

**本报告不标「已修好」。**

---

## 6. OPEN（净空目标）

| ID | 问题 | 目标 / 默认 | 状态 |
|----|------|-------------|------|
| Q1 | 主因是否 R1？ | 静态嵌深已坐实；**须用户补 H**：offset=0 能过则结案 | 待 Play |
| Q2 | offset=0 后嵌深≈0.34 是否仍卡？ | 仍卡 → 执行方案步 2 | 待验 |
| Q3 | `GroundUp` 净空目标值 | 目标脚高 **Y_p≈−6.6** 时 Body 底≈**−6.69**；须 **GroundUp 顶 ≤ −6.69 − ε（建议 ε≥0.05）** 或整盒移出 Body 占用带；记录改后中心/Size | 待定数值 |
| Q4 | 是否 R3？ | 仅当 offset=0（或抬顶后）仍卡且前方虫 Stay 刹 | 待命 |
| Q5 | 爬区盒是否下扩（R2）？ | 本期默认否；Body 仍盖 Trigger | 降级 |

---

## 7. 侦探声明

- 未改任何代码 / 场景 / Git。  
- 未 Play；主因 R1 为 **几何 + 残余现象** 裁定，**H 为验收必补项**。  
- 禁止在未填 A/H 的情况下再开「再猜一个嫌疑」的施工票。
