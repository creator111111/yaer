# Cursor Agent Prompt · 东郊 ↔ 苍翠走廊：换场相机平移 + 闪屏（对照 Stairs / 出店回村）

> **角色**：【架构侦探】只读；对照龙宫 Stairs（0912）与出店回村（0920）已修案，查「ForestEastScene ↔ VerdantCorridor」是否同一契约缺口  
> **日期**：2026-09-22  
> **症状**：今天（22 号）仍在；从龙城东郊进苍翠走廊（或反向）切换场景时，**摄像机可见平移 + 闪屏**  
> **场景**：`ForestEastScene` ↔ `VerdantCorridor`  
> **不是**：改东郊树洞内推镜 / 史莱姆演出相机；改日常横移阻尼手感；全局改所有场景 `smoothTime`；改 Part3 双机；改进场剧情文案  

把下面「侦探」整段交给 Cursor Agent。没对拍 Stairs/出店契约、没测清**两侧目标场景**当前 `smoothTime` / 默认 VCam ↔ EnterPos 位移之前，不要施工。

---

## 侦探（复制给 Agent）

```
你是【架构侦探】。只读分析，不改代码、不改场景、不提 MR。

任务：查清「龙城东郊 → 苍翠走廊」（及反向若同症状）换场时相机平移 + 闪屏的根因，并给出最小修复推荐。开发者认为这和序章 Stairs / 出店回村是同一类问题。

### 龙宫 Stairs + 出店回村已定案（必读，当对照，勿当东郊/走廊现网）

Stairs 报告：`Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md`  
Stairs 施工：`Assets/Doc/施工说明/0912/HomeScene1_2_Stairs换场相机定格_施工说明.md`  
出店报告：`Assets/Doc/执行文档/0920/Village_Shop_出店回村相机闪滑_架构溯源报告.md`  
出店施工：`Assets/Doc/施工说明/0920/Village_Shop_出店回村相机闪滑_施工说明.md`

| 项 | 已定案结论 |
|----|------------|
| **主因** | 进场 `SetFollow(forceSnap=true)` + 目标场景 `CameraComponent.smoothTime≈0.3` → 手推 VCam（Follow 暂空）；黑幕只 hold **≈0.3s** 就 `CloseFormFade`，**手推未收束就露景** |
| **闪 vs 滑** | 同源；位移小偏「闪」，位移大偏「可见平移」 |
| **已排除（对照案）** | 落点配错、剧情二次改相机（Stairs / 出店路径） |
| **修复（方案 A）** | **只改目标场景** `CameraComponent.smoothTime = 0` → 进场当帧定格；**未**改公共换场代码 |
| **日常跟拍** | 仍靠 Cinemachine FramingTransposer XDamping/SoftZone，**不读** 这个 `smoothTime` |

契约示意（两案相同）：

```
LoadScene → InitPlayer → SetFollow(forceSnap)
  · smoothTime > 0：多帧 SmoothDamp 手推  ← 露景窗口内 = 闪/滑
  · smoothTime ≈ 0：当帧定格
黑幕 hold≈0.3s → CloseFormFade（揭幕）
```

### 本案预扫（须 YAML/代码复核，不可当终裁）

| 项 | 预扫线索 |
|----|----------|
| 东郊出门 | `ForestEastScene` → `VerdantCorridorDoor`：`NextSceneName: VerdantCorridor`；`FirstEnterStoryName: ForestEastSceneFirstEnterVerdantCorridor`；`TriggerWhenMoveIn: 0` |
| 走廊回东郊 | `VerdantCorridor` → `LeftDoor`（可能多份）：`NextSceneName: ForestEastScene` |
| 东郊相机 | `ForestEastScene.unity` 磁盘仍有 **`smoothTime: 0.3`**（Stairs/出店**没改过东郊**） |
| 走廊相机 | `VerdantCorridor.unity` 磁盘仍有 **`smoothTime: 0.3`**；`EnterPosConfig` 含 `lastScene: ForestEastScene` |
| 方向 | 产品主诉是「东郊 → 苍翠走廊」；反向若同症状一并裁定是否同一修法 |

### 强制排除（勿当主因，除非证据压倒）

| 勿当主因 | 理由 |
|----------|------|
| 树洞 / TreeBridge 推镜 | 那是东郊**内**演出相机；本案是**跨场景门**换场 |
| Forest「禁止整场景 smoothTime=0」旧否决 | 那是**门口林恩可见运镜**产品语；本案产品要的是 **换场黑幕下定格**，对齐 Stairs/出店，不是演出推镜 |
| 全局改换场 hold / CloseFormFade | Stairs/出店已否决「拉长黑幕」作主修；优先场景 `smoothTime` |
| 只改门 Trigger / EnterPosKey 空串 | 先证明落点与手推同源，再谈门配置 |

---

@Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md
@Assets/Doc/施工说明/0912/HomeScene1_2_Stairs换场相机定格_施工说明.md
@Assets/Doc/执行文档/0920/Village_Shop_出店回村相机闪滑_架构溯源报告.md
@Assets/Doc/施工说明/0920/Village_Shop_出店回村相机闪滑_施工说明.md

请用 Grep / Read / Glob 只读查清（路径以仓库为准）：

1. `SceneChangeDoor` / LoadScene / InitPlayer / `SetFollow` / `forceSnap` / `smoothTime` / `CloseFormFade` / 黑幕 hold
2. `ForestEastScene.unity`：`VerdantCorridorDoor`、`CameraComponent.smoothTime`、从走廊回来的 `EnterPosConfig`
3. `VerdantCorridor.unity`：回东郊的 Door、`CameraComponent.smoothTime`、`EnterPosConfig` 中 `lastScene: ForestEastScene` 的坐标
4. 首次进走廊剧情 `ForestEastSceneFirstEnterVerdantCorridor`（或同名 Story）：是否在揭幕前后二次 `SetFollow` / DOMove 相机（加重或另因）
5. 若有东郊↔走廊旧相机文档，只作背景，以现网 YAML 为准

---

## 现象（用户原话）

今天是 22 号，龙城东郊到苍翠走廊切换场景的地方还是会有摄像机平移和闪屏的问题。

对照：龙宫 Stairs、出店回村已用「目标场景 smoothTime=0」定格修好；东郊/走廊磁盘预扫仍是 0.3，很像漏修的同款缝。

---

## 必做分析

### A. 先对拍 Stairs / 出店契约（证明同源或证伪）

填表现网（YAML 数字为准）：

| 检查项 | Stairs（已知） | 出店回村（已知） | 东郊→走廊（现网） | 走廊→东郊（若测） |
|--------|----------------|------------------|-------------------|-------------------|
| 换场入口 | Stairs Door | ExitShopToVillage | VerdantCorridorDoor？ | LeftDoor？ |
| InitPlayer → SetFollow(forceSnap) | 是 | 是 | ？ | ？ |
| 黑幕 hold → CloseFormFade | ≈0.3 | ≈0.3 | ？ | ？ |
| 目标 CameraComponent.smoothTime | 已 0 | KenMuNi1 已 0 | 磁盘预扫 0.3？ | 磁盘预扫 0.3？ |
| 默认机位 ↔ EnterPos 位移 | 下楼 Δx≈20 | 出店 Δ 更大 | 量出来 | 量出来 |
| 进场剧情二次改相机 | Stairs 路径无 | 无 | FirstEnterVerdantCorridor？ | ？ |

裁定：

- **同源**（同一 SetFollow 手推 + 早揭幕），或
- **不同源**（首次进走廊剧情推镜 / 双 VCam / 门 stayAction）——若不同源，写清断点，不要硬套 Stairs。

### B. 双向是否同修

产品主诉是东郊→走廊。请明确：

1. 只修 `VerdantCorridor` 的 `smoothTime` 是否覆盖主诉  
2. 反向走廊→东郊是否同症状；若是，是否也要 `ForestEastScene.smoothTime=0`  
3. 若只改走廊、不改东郊：写清「回东郊仍可能闪滑」的验收缺口，供产品取舍（Forest 整场景=0 与林恩运镜的旧冲突要单独一句话说明，不要假装不存在）

### C. 方案对比（必须有）

优先对照 Stairs/出店：

| 方案 | 做法 | 何时选 |
|------|------|--------|
| **A（同款）** | 目标场景 `CameraComponent.smoothTime=0` | 同源且日常跟拍仍靠 CM Damping |
| **B** | 拉长黑幕 hold 等手推完 | 仅当不能动该场景 smoothTime（须证据） |
| **C** | 改公共 SetFollow / 换场契约 | 最后手段；Stairs/出店未走这条 |

必答：

1. 改 `VerdantCorridor`（和/或 `ForestEastScene`）的 smoothTime=0，会不会误伤该场景日常跟拍、树洞/演出相机（对照「smoothTime 只影响 forceSnap 手推」是否仍成立）
2. 首次进走廊剧情是否要求**可见推镜**——若要求，方案 A 是否冲突，要不要「仅换场定格、剧情仍可 DOMove」的拆分说明
3. 为何不把 Forest「禁止 smoothTime=0」旧否决直接套死本案（产品语：换场定格 vs 门口运镜）

### D. 验收清单（写入报告）

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 东郊进苍翠走廊（非首次 / 首次各一次） | 揭幕无平移、无闪屏 |
| 2 | Hierarchy：目标场景 Camera → CameraComponent | `smoothTime = 0`（按报告指定改哪些场景） |
| 3 | 走廊内左右走 | 跟拍手感正常（CM 阻尼） |
| 4 | （对照）龙宫 Stairs；出店回村 | 仍定格 |
| 5 | （若改了东郊）走廊回东郊 | 揭幕定格；并注明对林恩门口运镜的影响结论 |

### E. 报告结构（必须按此写）

① 结论一句话（是否与 Stairs/出店同源 + 改哪个场景的哪个字段）  
② 原因（大白话 + 调用链 + 对照表 + 位移量级）  
③ 用户需要做什么（进门看闪/滑；Hierarchy 看 smoothTime）  
④ 方案对比与推荐  
⑤ 要改哪些文件（路径级）  
⑥ 施工后怎么验证（上表）  
⑦ 风险与回滚  

写入：`Assets/Doc/执行文档/0922/ForestEast_VerdantCorridor_换场相机闪滑_架构溯源报告.md`  
（可另附施工说明草稿路径建议，但侦探阶段不改场景。）
```

---

## 施工员（侦探闭环后再复制；未闭环勿用）

```
@Assets/Doc/执行文档/0922/ForestEast_VerdantCorridor_换场相机闪滑_架构溯源报告.md
@Assets/Doc/施工说明/0912/HomeScene1_2_Stairs换场相机定格_施工说明.md
@Assets/Doc/施工说明/0920/Village_Shop_出店回村相机闪滑_施工说明.md

你是【施工员】。只按溯源报告推荐方案做最小改。对照 Stairs/出店：优先场景序列化 smoothTime，不要改公共换场契约，除非报告点名必须改。

约束：
- 龙宫 Stairs、出店回村、东郊树洞推镜保持原样（除非报告点名必须动东郊 smoothTime，并写清与林恩运镜的取舍）
- 禁止全局改 CameraComponent 默认 smoothTime
- 禁止「先拉长黑幕」当主修（除非报告否决方案 A）
- 若方案是目标场景 smoothTime=0：注释/施工说明写明「只影响进场 forceSnap 手推，日常 Follow 仍走 Cinemachine」
- 写入施工说明：`Assets/Doc/施工说明/0922/ForestEast_VerdantCorridor_换场相机定格_施工说明.md`
```

---

## 使用顺序

1. 复制「侦探」→ Agent → 出 `执行文档/0922/...架构溯源报告.md`  
2. 你确认方案（尤其：**只改走廊**还是**双向都改**；Forest smoothTime=0 与林恩运镜）  
3. 再复制「施工员」→ 改场景 → 按验收表打进门
