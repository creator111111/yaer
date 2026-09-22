# ForestEast ↔ VerdantCorridor · 换场相机闪滑 — 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 场景：`ForestEastScene` ↔ `VerdantCorridor`  
> 现象：东郊进苍翠走廊（及反向）揭幕可见摄像机平移 + 闪屏  
> 对照：0912 龙宫 Stairs；0920 出店回村（方案 A：目标场景 `smoothTime=0`）  
> 提示词：`Assets/Doc/提示词/0922/ForestEast_VerdantCorridor_换场相机闪滑_架构侦探提示词.md`

---

## ① 结论一句话

**与 Stairs / 出店同源**：进场 `InitPlayer → SetFollow(forceSnap)` 在目标场景 `CameraComponent.smoothTime=0.3` 下手推 VCam，黑幕 hold≈0.3s 就 `CloseFormFade`，手推未收束即露景。最小修法仍是 **方案 A**：先把 **`VerdantCorridor`** 的 `smoothTime` 改为 **0**（覆盖主诉东郊→走廊）；反向若要一并定格，再把 **`ForestEastScene`** 的 `smoothTime` 改为 **0**（回东郊 Δx≈129，滑幅远大于进走廊）。

---

## ② 原因

大白话：跨场景门换场后，镜头先停在目标场景磁盘上的默认机位，人却刷在门边 EnterPos。黑幕只多留约 0.3 秒就开始淡出，镜头还在用 `smoothTime=0.3` 的 SmoothDamp 往人身上挪——位移小偏「闪」，位移大偏「可见平移」。龙宫下楼、出店回村是同一条缝；东郊/走廊 Stairs/出店修完后磁盘仍是 0.3，属于漏修同款。

### 调用链（东郊 → 走廊）

```
ForestEastScene / VerdantCorridorDoor (SceneChangeDoor)
  NextSceneName = VerdantCorridor
  TriggerWhenMoveIn = 0；ShowLoadingUI = 0；EnterPosKey 空
  FirstEnterStoryName = ForestEastSceneFirstEnterVerdantCorridor（仅首次）
  → EnterDoor → LoadSceneComponentGSM.LoadScene(VerdantCorridor, blackFade:true)
        │
        ▼
BlackPanel FadeShow ──全黑──► onShowEnd
  → 卸东郊 → Load VerdantCorridor
        │
        ▼
BaseGameSceneManager.InitPlayer
  → SetPlayerPos（LastScene=ForestEastScene → LeftBorn）
  → CameraComponentGSM.SetFollow(player)   // forceSnap 默认 true
       smoothTime=0.3 → Follow=null；_isSmoothingSnap 手推
  → Ready 后 hold mapTransitionBlackHoldSeconds(0.3s) → CloseFormFade
       （不等 SetFollow onComplete；本路径无 TryDeferBlackFadeForCover）
  → 揭幕时手推仍在跑 → 闪 + 可见滑
```

反向走廊→东郊：活跃 `LeftDoor`（`m_IsActive: 1`）→ `ForestEastScene`，同一 `LoadScene` / `InitPlayer` / `SetFollow` 契约；目标变成东郊 `smoothTime=0.3`。

### A. 与 Stairs / 出店对照表（YAML 现网）

| 检查项 | Stairs（已知） | 出店回村（已知） | 东郊→走廊（现网） | 走廊→东郊（现网） |
|--------|----------------|------------------|-------------------|-------------------|
| 换场入口 | Stairs Door | ExitShopToVillage | **`VerdantCorridorDoor`** | **`LeftDoor`**（活跃份；另有 Inactive 副本） |
| InitPlayer → SetFollow(forceSnap) | 是 | 是 | **是**（`BaseGameSceneManager` 337 行） | **是** |
| 黑幕 hold → CloseFormFade | ≈0.3 | ≈0.3 | **≈0.3**（组件默认；场景未见覆盖） | **同左** |
| 目标 `CameraComponent.smoothTime` | 已 0 | KenMuNi1 已 0 | **磁盘仍 `0.3`** | **磁盘仍 `0.3`** |
| 默认 VCam 世界 XY | HS1≈(-19.5,0) 等 | VCam_Street≈(32.56,0) | **`Cinemachine` 父 Camera(0,0)+本地(0,0,-10) → XY (0,0)** | **同结构 → XY (0,0)** |
| EnterPos | Stairs 落点 | EnterFrom_Shop≈(-29,-6.5) | **`LeftBorn` (2.5, -6.61)**；`lastScene: ForestEastScene` | **门下 `Pos` (129.2, -6.61)**；`lastScene: VerdantCorridor` |
| 默认机位 ↔ 落点 | 下楼 Δx≈20 | Δx≈62 | **Δx≈2.5，Δy≈-6.61**（偏闪 + 小滑） | **Δx≈129.2，Δy≈-6.61**（大滑） |
| 进场剧情二次改相机 | Stairs 无 | 无 | **无**：`ForestEastSceneFirstEnterVerdantCorridor` 仅 UIAlpha + 一句对白，**无** SetFollow / DOMove | 无本向 FirstEnter |

**裁定：同源**（同一 SetFollow 手推 + 早揭幕）。不是落点配错，不是首次进走廊剧情推镜，不是树洞 / TreeBridge 内推镜，不是双 VCam（两侧 `virtualCameraPart3: {fileID: 0}`）。

### B. 双向是否同修

| # | 问题 | 裁定 |
|---|------|------|
| 1 | 只改 `VerdantCorridor.smoothTime=0` 是否覆盖主诉？ | **是**（主诉是东郊→走廊，目标场景是走廊） |
| 2 | 反向是否同症状？ | **是**（同一契约）；且 Δx≈129，**比进走廊更易看成大平移** |
| 3 | 只改走廊、不改东郊的验收缺口 | 走廊回东郊揭幕仍可能闪/大滑；须产品明确是否接受 |
| 4 | Forest「禁止整场景 smoothTime=0」能否套死东郊？ | **不能直接套死。** 旧否决对象是 **`ForestScene` 门口林恩「保留可见运镜」**（无黑幕收束 / 演出推镜终态），不是本案 **跨场景黑幕下定格**。`ForestEastScene` 树洞推镜 / 史莱姆演出走 `ChangeCamera` / DOMove 等路径，**不读** 进场 `forceSnap` 用的这个 `smoothTime`。若产品同意「回东郊也定格」，改东郊场景字段与 Stairs/出店同口径；若仍担心林恩类语感，应单独 Play 验收门口运镜，而不是把旧否决当证据否决本案方案 A |

### 强制排除复核

| 勿当主因 | 现网证据 |
|----------|----------|
| 树洞 / TreeBridge 推镜 | 本案入口是 `VerdantCorridorDoor` / `LeftDoor` 跨场景门，不是洞内 `ChangeCamera` |
| 只改门 Trigger / EnterPosKey | 落点表已配对；空 `EnterPosKey` 走 `lastScene` 表，坐标合理 |
| 拉长黑幕作主修 | Stairs/出店已否决；优先场景 `smoothTime` |
| 首次进走廊剧情 | Prefab 图仅 Alpha + StatementNode；不加重相机 |

---

## ③ 用户需要做什么

1. 东郊右门进苍翠走廊（非首次）：揭幕是否闪、是否小幅滑（预期现网有）。  
2. 同路径首次（清过 FirstEnter 标记后）：揭幕后对白正常；**闪/滑仍应在揭幕瞬间出现**（剧情不二次推镜）。  
3. （对照）走廊左门回东郊：预期大滑更明显（Δx≈129）。  
4. Hierarchy：`VerdantCorridor` → `Camera` → `CameraComponent` → 现网 **`smoothTime = 0.3`**（施工后主诉路径应为 **0**）。  
5. 若产品批准双向：`ForestEastScene` → `Camera` → 同字段现网 **0.3** → 施工后 **0**。  
6. 走廊内只左右走：日常跟拍应仍靠 CM `XDamping=0.7` / SoftZone（验收项 3）。

---

## ④ 方案对比与推荐

| 方案 | 做法 | 何时选 |
|------|------|--------|
| **A（同款，推荐）** | 目标场景 `CameraComponent.smoothTime=0` | 同源且日常跟拍仍靠 CM Damping — **本案成立** |
| **B** | 拉长黑幕 hold 等手推完 | 仅当不能动该场景 smoothTime（本案无此证据） |
| **C** | 改公共 SetFollow / 换场契约 | 最后手段；Stairs/出店未走 |

**推荐施工默认**

1. **必做**：`VerdantCorridor.unity` → `Camera` → `smoothTime: 0.3 → 0`（覆盖主诉）。  
2. **建议同票或紧随**：`ForestEastScene.unity` → 同字段 → **0**（盖反向大滑；见 OPEN Q1）。  
3. **不改**：`LoadSceneComponentGSM` / `CameraComponent.cs` 默认值；门 Trigger / EnterPos；树洞 / 史莱姆演出 Prefab；HS1/HS2 / KenMuNi1（已定格对照）。

| 必答 | 答案 |
|------|------|
| 改走廊（和/或东郊）smoothTime=0，会不会误伤日常跟拍、树洞/演出？ | **不会按主因误伤。** `smoothTime` 只参与 `forceSnap` 手推；日常 Follow 是 FramingTransposer XDamping/SoftZone。树洞/史莱姆走独立运镜，不依赖本字段收束换场 |
| 首次进走廊是否要求可见推镜？ | **否**（现网 Prefab 无相机节点）。方案 A 与首次对白不冲突；剧情若以后要 DOMove，仍可 `forceSnap=false` / 直接 DOMove，与进场定格拆分 |
| 为何不把 Forest「禁止 smoothTime=0」套死本案？ | 产品语不同：旧否决是 **ForestScene 门口可见运镜**；本案是 **黑幕揭开已到位**，对齐 Stairs/出店，不是演出推镜终态 |

### 否决

- **方案 B**：契约面大、所有 blackFade 换场回归贵；A 已够。  
- **只挪默认 VCam 靠近 LeftBorn**：进走廊可缩小滑幅，但消灭不了「hold 0.3 vs 手推未完」；回东郊默认机位也不该硬钉在 x=129。

---

## ⑤ 要改哪些文件（路径级）

| 全路径 | 做什么 |
|--------|--------|
| `Assets/GameRes/Scenes/VerdantCorridor.unity` | **必改** `Camera` 上 `CameraComponent.smoothTime`：**0.3 → 0** |
| `Assets/GameRes/Scenes/ForestEastScene.unity` | **建议改** 同字段 **0.3 → 0**（待产品确认 OPEN Q1；盖反向） |
| `LoadSceneComponentGSM` / `CameraComponent.cs` / 门 YAML / EnterPos / FirstEnter Prefab | **不改** |
| HomeScene1/2、Village_KenMuNi1 | **不改**（对照已定格） |

施工说明建议路径（侦探阶段不写场景）：  
`Assets/Doc/施工说明/0922/ForestEast_VerdantCorridor_换场相机定格_施工说明.md`  
（可对照 `施工说明/0912/HomeScene1_2_Stairs换场相机定格_施工说明.md`、`施工说明/0920/Village_Shop_出店回村相机闪滑_施工说明.md`）

---

## ⑥ 施工后怎么验证

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 东郊进苍翠走廊（非首次 / 首次各一次） | 揭幕无平移、无闪屏；首次对白仍正常 |
| 2 | Hierarchy：目标场景 Camera → CameraComponent | `smoothTime = 0`（按上表改过的场景） |
| 3 | 走廊内左右走 | 跟拍手感正常（CM 阻尼） |
| 4 | （对照）龙宫 Stairs；出店回村 | 仍定格 |
| 5 | （若改了东郊）走廊回东郊 | 揭幕定格；另点验：东郊日常横移 / 树洞进洞相机仍按原演出，**不**因本字段变「瞬切运镜」 |

---

## ⑦ 风险与回滚

| 风险 | 说明 | 回滚 |
|------|------|------|
| 只改走廊 | 主诉消失；回东郊仍可能大滑 | 补改东郊，或接受缺口 |
| 改东郊 smoothTime=0 | 所有进 `ForestEastScene` 的 blackFade 换场进场都会当帧定格（与 Stairs 允许「其它入口也瞬切」同口径） | 字段改回 `0.3` |
| 误伤演出 | 低：史莱姆/树洞不读进场手推字段；若 Play 发现某条东郊内链依赖「无黑幕 forceSnap 手推可见」，再单开票（勿全局拉 hold） | 按单路径回滚或特判 |

**OPEN**：见 `Assets/Doc/OPEN_QUESTIONS.md` 本节 Q1（是否同票改东郊）。
