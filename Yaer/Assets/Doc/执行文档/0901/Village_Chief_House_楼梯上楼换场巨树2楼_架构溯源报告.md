# Village_Chief_House — 楼梯上楼换场巨树 2 楼 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探】→ 施工已落地（见 `施工说明/0901/Village_Chief_House_楼梯上楼换场巨树2楼_施工说明.md`；楼梯门须跑 Setup 菜单）  
**Unity**：2020.3.48f1  
**产品**：`Village_Chief_House` 走楼梯上楼 → `LoadScene(Village_KenMuNi1)` 巨树 **2 楼**；落点 **`ExitFrom_HomeSceneChief2f`**；可走区 **`VillageWalkArea2`**  
**提示词**：`提示词/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构侦探提示词.md`  
**依赖**：`执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md`（室内先能走上楼）  
**样板**：`场景切换.md` / `Stairs.prefab` / `Village_HomeScene1` 出门（`TriggerWhenMoveIn: 1`） / 现网 `LeftDoor`

---

## 沟通摘要

### ① 结论一句话

**村侧落点与 EnterPos 已配对 2 楼；缺的是室内楼梯顶换场门，以及落点后把生效 WalkArea 切到 `VillageWalkArea2`（现网只认 `VillageWalkArea`，会被拉回 1 楼高度）。1 楼 `LeftDoor` 与楼梯共用 `lastScene=Village_Chief_House` 会抢同一落点——推荐 E3′：保留现网 2f EnterPos 给楼梯，给 `LeftDoor` 加最小 `enterPosKey` 覆盖另落 1 楼门前。**

### ② 原因（通俗）

回村时程序只看「上一场景叫什么」来选出生点，不看你是从大门还是楼梯出去的；现在这一行已经指到巨树 2 楼。  
村里走路又只认一块叫 `VillageWalkArea` 的地板；2 楼那块 `VillageWalkArea2` 摆好了但程序没用上，人一落地会被吸回下面。  
室内还没有楼梯顶的换场门，合层「楼梯」只是画。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 室内走上楼 → 触发区 → 进 `Village_KenMuNi1` | |
| 2 | 落点在 `ExitFrom_HomeSceneChief2f` 附近（巨树 2 楼，约 Y≈41） | |
| 3 | 落点后 **不被拉回 1 楼高度**；在 `VillageWalkArea2` 内可村式移动 | |
| 4 | `VillageWalkArea2` 多边形点集/尺寸与施工前一致（未改） | |
| 5 | 1 楼 `LeftDoor` 出门落点符合决议（E3′：门前 1 楼，非 2 楼） | |
| 6 | Console 无 LoadScene / EnterPos / 空引用 Error | |
| 7 | 回村后仍为 `Village2_5D`；其它 Home 仍不开 Town | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 落点 | **保持** KenMuNi1 `EnterPos`：`lastScene: Village_Chief_House` → Transform **`ExitFrom_HomeSceneChief2f`**（fileID **`880002002`**，已核实） |
| 室内门 | **新建**楼梯顶 `SceneChangeDoor`（旁挂合层「楼梯」美术；**勿**把 SR 当唯一体） |
| 七件套 | `NextSceneName=Village_KenMuNi1`；**`TriggerWhenMoveIn=true`（Q1）**；**`ShowLoadingUI=false`（黑幕，对齐 LeftDoor / Q4）**；进 **`sceneObjs`** |
| EnterPos 冲突 | **E3′**（见 §④）：保留 2f 给默认 `Village_Chief_House`；`LeftDoor` 用 **`enterPosKey` 覆盖** → 新键 + 新建 1f `ExitFrom_HomeSceneChief` |
| WalkArea2 | **W1**：落 2 楼后生效多边形切到 `VillageWalkArea2`；**禁止**改其点集/尺寸（否 W2/W3） |
| 回程 | **本期不做**（OPEN Q5）；村 2 楼→再进 Chief 仍走现网门/`EnterFrom_Village` |
| 依赖 | 室内划区 **A1** 须先能走上楼；否则门摆了验不了 |

---

## ② 已配锚点（磁盘核实）

| 物体 | 场景 / 路径 | 磁盘 | 备注 |
|------|-------------|------|------|
| **`ExitFrom_HomeSceneChief2f`** | KenMuNi1 / MapLimit | ✅ Active；Transform fileID **`880002002`**；local ≈ **`(-159.34, 41.66, 0)`** | 产品落点；优先保留 |
| **`VillageWalkArea2`** | KenMuNi1 / MapLimit | ✅ Active；PolygonCollider2D；local ≈ **`(-139, 37.5, 0)`** | **形状锁定** |
| **`VillageWalkArea`** | 同父 | ✅ local ≈ **`(0, -5.91, 0)`** | 现网唯一绑定名 → 1 楼高度 |
| KenMuNi1 **`EnterPosConfig`** | `lastScene: Village_Chief_House` | ✅ `pos: {fileID: 880002002}` = 上表 2f | **勿误绑回 1 楼**（楼梯路径） |
| **`ExitFrom_HomeSceneChief`（1f）** | — | ❌ **不存在** | E3′ 施工须新建（门前落点） |
| Chief **`LeftDoor`** | → `Village_KenMuNi1` | ✅ `TriggerWhenMoveIn=0`，`ShowLoadingUI=0` | 与楼梯同目标场景 → 冲突 |
| Chief **楼梯换场门** | — | ❌ **无**（场景无 Stairs/上楼门；仅合层 SR「楼梯」） | 本期主缺 |
| Chief `sceneObjs` | Map 注册表 | ⚠️ 现网列表 **未含** `LeftDoor` 的 SceneEntity（另有 `leftDoor` 字段引用）；**新楼梯门必须进 `sceneObjs`** | 对齐七件套 |

---

## ③ 换场链（期望 vs 现网）

### 期望时序

```
Village_Chief_House
  → 沿室内 WalkArea 走上楼触发区
  → SceneChangeDoor.EnterDoor
  → LastSceneName = Village_Chief_House（默认，无 override）
  → LoadScene(Village_KenMuNi1, blackFade: true)   // ShowLoadingUI=false
  → KenMuNi1 GSM.SetPlayerPos：EnterPos 命中 → ExitFrom_HomeSceneChief2f
  → Town 模式 + 生效 WalkArea = VillageWalkArea2
  → 巨树 2 楼可走
```

### 现网已通 / 未通

| 环节 | 状态 |
|------|------|
| EnterPos → 2f Transform | ✅ 已配 |
| `ChangeSceneComponentGM` 记 `lastSceneName = nowSceneName`（卸当前前） | ✅ 标准链；**无**门级 suffix 先例 |
| `BaseGameSceneManager.SetPlayerPos` 按 `lastScene` 精确字符串匹配 | ✅ |
| 室内楼梯顶门 | ❌ 缺失 |
| 绑定 `VillageWalkArea2` | ❌ `TryBindVillageWalkPolygonFromActiveScene` **只找** `"VillageWalkArea"`；`villageWalkAreaOverride` 仅私有 SerializeField，无运行时 Setter |
| 落 2f 仍套 1f WalkArea | ⚠️ **必现**：ClosestPoint 拉向 `VillageWalkArea`（Y≈-5.91 一带） |

### 楼梯门七件套（施工填表）

| # | 项 | 拍板 |
|---|----|------|
| 1 | GO Active；挂可交互层级（建议 Map / Design 旁楼梯顶，**旁挂**合层「楼梯」） | 须 |
| 2 | `SceneChangeDoor` Enabled + `InteractiveComponent` + Collider | 须 |
| 3 | `NextSceneName` = **`Village_KenMuNi1`** | 钉死 |
| 4 | `TriggerWhenMoveIn` | **`true`**（走进即切；Home 出门有先例） |
| 5 | 进 Chief `sceneObjs` | 须（勿只靠特殊字段） |
| 6 | 表现 | **`ShowLoadingUI=false`** → `LoadScene(..., blackFade:true)` 黑幕；对齐 LeftDoor |
| 7 | 合层「楼梯」 | **仅美术锚点**；触发体独立 |

样板：可复制 `Assets/Prefabs/Stairs.prefab` 改 `NextSceneName` / Trigger；勿改合层 SR 为门。

---

## ④ EnterPos 冲突裁定（E1 / E2 / E3）

### 事实

- `EnterPos` **一 `lastScene` 字符串 → 一个 Transform**。  
- 凡从 `Village_Chief_House` 卸场，`LastSceneName` 恒为 **`Village_Chief_House`**（`ChangeSceneComponentGM`；`LoadSceneArgs` **无** override 字段）。  
- 现网该键已指 **2f**。  
- 故 **`LeftDoor`（1 楼按 E）与楼梯门（2 楼走进）会落到同一点**，除非拆键或改产品。

### 方案对比

| 方案 | 做法 | 判定 |
|------|------|------|
| **E1** | 凡从 Chief 回村都落 2 楼；1 楼门改别处或禁用 | ❌ 破坏「大门回村门前」直觉；须产品书面接受才可 |
| **E2** | 伪场景名硬改 LastScene / 多套侵入 | ⚠️ 重；且无现成 API |
| **E3′（推荐）** | **保留** `Village_Chief_House`→**2f**（楼梯零改 EnterPos）；`SceneChangeDoor` + `LoadSceneArgs` 增可选 **`enterPosKey`**（空=真实场景名）；`ChangeSceneComponentGM` 记 lastScene 时优先用该键；**`LeftDoor`** 填键如 `Village_Chief_House_Door`；KenMuNi1 **新增** EnterPos 行 → 新建 **`ExitFrom_HomeSceneChief`**（1f 门前，靠近户外 `House_Chief`） | ✅ 最小 API；对齐产品已摆的 2f |

**否**：把现网 EnterPos 改回 1f 却不给楼梯 override——楼梯会落到门前，违背本期产品。

**施工顺序建议**：先落楼梯门 + W1（可临时接受 LeftDoor 也落 2f 做联调）→ 再补 E3′ 1f 键与 `ExitFrom_HomeSceneChief`。

---

## ⑤ WalkArea2 生效（W1）

### 根因

```776:803:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
        private void TryBindVillageWalkPolygonFromActiveScene()
        {
            // ...
            Transform named = FindNamedTransformInLoadedScene(scene, "VillageWalkArea");
            // ...
            _villageWalkPolygonFromScene = poly;
        }
```

- 仅 KenMuNi1 + 名 **`VillageWalkArea`**。  
- `ResolveEffectiveWalkPolygon`：Inspector `villageWalkAreaOverride` 优先，否则上式。  
- **无**按名切 `VillageWalkArea2`、无公开 `SetOverride`。

落点 Y≈41 在 2 楼，若仍套 1 楼多边形 → **ClosestPoint 校正把人拉回低处**（产品「传送成功却站不住」）。

### 裁定 **W1**（不改 WalkArea2 形状）

| 步骤 | 做法 |
|------|------|
| API | `TownPlayerLocomotion` 增 **`SetVillageWalkAreaOverride(PolygonCollider2D)`**（可 null 清回场景绑定） |
| 进 2 楼 | 在 KenMuNi1 **SetPlayerPos 命中 `ExitFrom_HomeSceneChief2f` 之后**（或 `lastScene==Village_Chief_House` 且未走 Door 键）：查找 **`VillageWalkArea2`** → `SetVillageWalkAreaOverride`；再 `FlushAuthoritativeVillageTransformAfterSceneDepthInject`（若现网有） |
| 回 1 楼平台 | 可选：地面 Zone / 与 1f WalkArea Overlap 时 `SetOverride(null)` 并重新 `TryBind` → `VillageWalkArea`（同场景下楼不换场时需要；否则 2 楼约束套在地面会夹错） |
| **禁止** | 合并点集（W2）；关掉 ClosestPoint（W3）；改 WalkArea2 Scale/点「扩大可走区」 |

**替代方案（不推荐本期）**：改 `TryBind` 为「多多边形列表 + 并集」——注释已写明首版仅单 Polygon；改动面大于 Override 切换。

---

## ⑥ 最小施工清单

1. **依赖确认**：室内 A1（白名单 `Village_Chief_House` + 窄 WalkArea 含楼梯条带）可玩到楼上。  
2. Chief：楼梯顶新建门（Stairs 样板）→ `NextSceneName=Village_KenMuNi1`，`TriggerWhenMoveIn=1`，`ShowLoadingUI=0`，Collider + Interactive，**登记 `sceneObjs`**。  
3. **W1**：`SetVillageWalkAreaOverride` + KenMuNi1 进场后绑 `VillageWalkArea2`（条件见 §⑤）；**不改** WalkArea2 几何。  
4. **E3′**：`LoadSceneArgs.enterPosKey`（或等价）+ GM 记 lastScene 优先键；`LeftDoor` 填 1f 键；新建 `ExitFrom_HomeSceneChief` + EnterPos 行。  
5. 回归：其它 Home 出门、村内 1 楼 WalkArea、续聊/换古莎/Loading 进屋。  
6. 文档：施工说明 + 同步 OPEN。

---

## ⑦ 验收

- [ ] 室内走上楼触发 → 进入 `Village_KenMuNi1`  
- [ ] 落点 ≈ `ExitFrom_HomeSceneChief2f`（巨树 2 楼）  
- [ ] 落点后 Y 不被拉回 1 楼；`VillageWalkArea2` 内可 A/D+W/S  
- [ ] `VillageWalkArea2` 点集/尺寸与施工前一致  
- [ ] `LeftDoor` 出部落 1 楼门前（E3′ 完成后）  
- [ ] Console 无相关 Error  
- [ ] 回村仍 `Village2_5D`  

---

## ⑧ OPEN

| ID | 问题 | 决议 / 默认 | 状态 |
|----|------|-------------|------|
| Q1 | 上楼 `TriggerWhenMoveIn`？ | **true** | ✅ 侦探拍板 |
| Q2 | 1 楼门 vs 2 楼落点？ | **E3′**（保留 2f EnterPos；LeftDoor `enterPosKey`） | ✅ 侦探拍板；待施工 |
| Q3 | WalkArea2 如何生效？ | **W1** Override 切换；不改形状 | ✅ |
| Q4 | 黑幕还是 Loading？ | **黑幕**（`ShowLoadingUI=false`，对齐 LeftDoor） | ✅ |
| Q5 | 村 2 楼是否「下楼回村长家」对开？ | **本期不做** | ⏳ 另案 |
| Q6 | 同场景 2f→1f 是否要 Zone 切回 `VillageWalkArea`？ | **建议做**（否则下树仍套 WalkArea2） | ⏳ 施工可最小：仅进 2f 绑 2；下树另触发 |
| Q7 | 室内划区 A1 是否已施工可玩？ | 依赖前案；未完成则本期联调阻塞 | ⏳ |

---

## ⑨ 程序补充

### 关键锚点

| 符号 | 路径 |
|------|------|
| EnterPos 匹配 | `BaseGameSceneManager.SetPlayerPos` |
| LastScene 写入 | `ChangeSceneComponentGM.LoadScene`（卸当前回调里 `lastSceneName = nowSceneName`） |
| 门换场 | `SceneChangeDoor.EnterDoor` → `LoadScene` / `LoadSceneWithLoadingPanel` |
| Walk 绑定 | `TownPlayerLocomotion.TryBindVillageWalkPolygonFromActiveScene` |
| 室内依赖 | 0901 划区报告 · A1 白名单 + 窄 `VillageWalkArea` |

### 硬禁止（施工）

- 改 `VillageWalkArea2` 多边形尺寸 / 点集 / 用 Scale 扩区  
- 只配 `NextSceneName` 不核实 EnterPos / 不接 W1  
- 合层「楼梯」SR 无 Collider、不进 `sceneObjs` 当唯一门  
- 默认同开其它 Home 的 Town / WalkArea2 逻辑  

### 与「不是」对齐

| 不是 | 说明 |
|------|------|
| 进树屋室内 Scene | 无该 `SceneName`；目标是 KenMuNi1 2 楼平台 |
| 只挪出生点不配门 | 室内触发缺失则玩法不通 |
| 合并两大 WalkArea | 用户禁止动 WalkArea2 大小 |
