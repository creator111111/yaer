# Village_KenMuNi1 — 巨树 2 楼 WalkArea2 宝箱 Hp/Mp×3 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探 → 施工已落地】脚本/存档/Setup 菜单已就绪；**场景箱体须开 Unity 跑 Setup**；**禁止改 `VillageWalkArea2` 多边形尺寸/点集**  
**Unity**：2020.3.48f1  
**场景**：`Village_KenMuNi1`  
**摆放区**：`MapLimit / VillageWalkArea2`（巨树 2 楼；形状锁定）  
**产品**：区内宝箱 → **生命球×3 + 体力球×3** + **`GetHpBall` / `GetMpBall`** 同款 Tips 横幅；同档单次  
**提示词**：`提示词/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构侦探提示词.md`  
**上游依赖**：楼梯换场巨树 2 楼（能到 WalkArea2）+ W1（WalkArea2 生效，否则站不住/夹回 1 楼）  
**样板**：`WestRappRoadHpMpBox`（双球+双 Tips+开箱无对白）· `HomeScene1Xiaer`（×3）· `HomeScene2Box`（开箱存档/动画）

---

## 沟通摘要

### ① 结论一句话

**仿 WestRapp：在 WalkArea2 内实例化 `Assets/Prefabs/Box.prefab`，挂新建村用脚本（默认 hp/mp=3、无 Story），存档布尔挂独立 `VillageKenMuNi1Data.tree2fHpMpBoxOpened`；开箱 `AddMainItem`×2 + `OpenTipsForm(GetHpBall/GetMpBall)` 入队；禁止改 WalkArea2 形状，禁止挂西境组件以免读错存档。**

### ② 原因（通俗）

西境箱子已经会「开箱 → 球进包 → 两张获得横幅依次弹出」，数量改成 3 就能用。  
横幅字印在图集 `GetHpBall` / `GetMpBall` 里，不用新做「×3」图。  
箱子必须摆在 2 楼可走多边形里，否则人站不到或被 1 楼地板吸走；可走区大小已定死，只挪箱子。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 站在 WalkArea2 内可对宝箱点 E / 点击交互 | |
| 2 | 开箱后背包生命球 +3、体力球 +3 | |
| 3 | 依次出现 GetHpBall、GetMpBall 花边横幅 + 获得物品音效 | |
| 4 | 同档再点不再发奖；箱为打开态 | |
| 5 | 读档已开：Open 动画、不可交互 | |
| 6 | `VillageWalkArea2` 多边形未改 | |
| 7 | 剑 / 空桶 / 针线包 Tips 回归正常 | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 方案 | **B1**：仿 `WestRappRoadHpMpBox`；**新建**村用类（勿直接挂西境脚本） |
| 数量 | `hpBallCount=3`，`mpBallCount=3`（Serialize 默认可改） |
| Tips | `OpenTipsForm("GetHpBall")` → `OpenTipsForm("GetMpBall")`（默认 Item；**先 Hp 后 Mp**，对齐 WestRapp） |
| ×3 专图 | **否**（Q2）；复用现图集 Key |
| 对白 | **`useStoryOnOpen=false`**（Q5）；无 TriggerStory |
| 交互 | **`onClickInteractiveEvent`**（点 E / 点击；对齐 West/Home2；非走进即开） |
| 存档 | **S1**：新建 `VillageKenMuNi1Data.tree2fHpMpBoxOpened`（勿用 `WestRappRoadData` / `HomeScene2Data`） |
| Prefab | **`Assets/Prefabs/Box.prefab`**（guid `3d242310…`）+ Animator `Assets/Animation/Object/HomeScene2/Box.controller`（Bool `Open`） |
| 摆放 | `Objects` 下；坐标 **WalkArea2 内**、略偏开 `ExitFrom_HomeSceneChief2f`；进 **`sceneObjs`** |
| WalkArea2 | **禁止**改尺寸/点集 |

---

## ② 样板对拍

### WestRappRoadHpMpBox（主抄）

| 步骤 | 现网 |
|------|------|
| 交互 | `InteractiveComponent.onClickInteractiveEvent` → `OpenBox` |
| 已开读档 | `WestRappRoadData.hpMpBoxOpened` → `animator.SetBool("Open",true)` + `canTouchWithPlayer=false` |
| 无 Story 路径 | `On…_OpenBox()`（旗=true + 动画 + SFX）→ `On…_GetHpMp()` |
| 入包 | `AddMainItem(HpBall, hpBallCount)` + `AddMainItem(MpBall, mpBallCount)`（场景默认 **2/2**） |
| Tips | 连续两次 `OpenTipsForm`；队列 fill（Tips 已开则 `AddTipsInfo`） |
| 场景挂法 | **PrefabInstance** `Box.prefab` + 附加 `WestRappRoadHpMpBox`；`useStoryOnOpen: 0` |

### HomeScene1Xiaer（数量×3）

- `AddMainItem(MpBall,3)` + `GetMpBall`，再 `HpBall,3` + `GetHpBall`（顺序与 West **相反**）。  
- 本期横幅顺序 **跟 West：先 Hp 后 Mp**（提示词钉死）。

### HomeScene2Box（开箱态）

- 存档 `boxOpened` + Animator `Open`；发奖走 Story Prefab。  
- 本期 **不走 Story**（避免缺 Prefab 锁死，West 已踩过）。

### Tips 图集（已核实路径）

| TipKey | 资源 |
|--------|------|
| `GetHpBall` | `ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas/GetHpBall.png` |
| `GetMpBall` | `…/GetMpBall.png` |

枚举：`EMainItemName.HpBall` / `MpBall`（`EMainItemName.cs`）。

**禁止**：只 `AddMainItem`；TipKey 用中文文件名；新 Tips UI；商店 `Village_ShopChest`。

---

## ③ 脚本 / 存档

### 脚本（Q3）

| 方案 | 判定 |
|------|------|
| 直接挂 `WestRappRoadHpMpBox` | ❌ 读 `WestRappRoadData`；与村档耦合，Debug 还会误比 `HomeScene2Data` |
| **新建** `VillageKenMuNi1HpMpBox`（或 `VillageHpMpBox`） | ✅ 逻辑拷贝 West；默认 count=3；`useStoryOnOpen=false` |
| B2/B3 对话图 | ❌ 重 / 无横幅 |

建议路径：`Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/VillageKenMuNi1HpMpBox.cs`  
公开方法可保留 `OnOpenBox` / `OnGetHpMp` 对称 West（即使无 Story 也便于调试）。

### 存档（Q4 · S1）

KenMuNi1 GSM 现用 **`ForestSceneData`**（门口剧情等）。**不宜**把巨树宝箱旗塞进森林语义类（可扩展但脏）。

| 推荐 | 说明 |
|------|------|
| **新建** `VillageKenMuNi1Data : BaseArchiveData` | 字段 `tree2fHpMpBoxOpened`；键如 `VillageKenMuNi1Data_tree2fHpMpBoxOpened` |
| 读写 | `SceneManager.GetArchiveData<VillageKenMuNi1Data>()`（`ArchiveComponentGM.GetData<T>` **按需 new**，无需改注册表） |
| 否 | 复用 `hpMpBoxOpened` / `boxOpened`（跨场景串档） |

读档：已开 → Open 动画 + 关交互（同 West）。  
写档时机：开箱时内存置 `true`（对齐 West；随存档管线序列化）。对话线才强制 `SavePlayerBag`——**C# 宝箱不强制另调 Save**，除非产品要求开箱即时落盘（OPEN 可升）。

---

## ④ 场景摆放

### 现网缺口

| 项 | 磁盘 |
|----|------|
| KenMuNi1 内宝箱实体 | ❌ 无 Box/Chest/宝箱名命中 |
| `VillageWalkArea2` | ✅ `(-139, 37.5, 0)` + PolygonCollider2D；**勿改** |
| `ExitFrom_HomeSceneChief2f` | ✅ `(-159.34, 41.66, 0)`（2 楼落点参考） |
| `Objects` | ✅ Transform fileID `1948841490`；`sceneObjs` 已有多实体 |

### 摆放建议

| 项 | 拍板 |
|----|------|
| 父级 | **`Objects`**（与现网实体一致） |
| Prefab | 实例化 **`Assets/Prefabs/Box.prefab`**，替换/附加村脚本（勿保留西境脚本） |
| 建议坐标 | 约 **`(-152, 41.2, 0)`**（落点东侧，避免与出生重叠）；施工用 OverlapPoint 确认在 **WalkArea2 内**后可微移 |
| 备选 | `(-165, 40.8, 0)`（西侧平台，仍须在多边形内） |
| Layer / Clds | 跟 Box Prefab（Click Trigger + Body）；勿改成商店 Chest |
| `sceneObjs` | **必须登记** SceneEntity 引用 |
| WalkArea2 | **只挪箱**；禁止改多边形「腾地」 |

### 交互七件套

| # | 项 |
|---|----|
| 1 | GO Active |
| 2 | `SceneEntity` + `ComponentSystem` + 村用 Box 逻辑 Enabled |
| 3 | `InteractiveComponent` + Click Collider |
| 4 | Animator + `Box.controller`（Bool `Open`） |
| 5 | `SoundToggleComponent`（开箱音，对齐 Prefab SFX） |
| 6 | 进 `sceneObjs` |
| 7 | 脚点可达：玩家在 WalkArea2 内可触（依赖上游 W1） |

---

## ⑤ Tips / 发奖顺序

```
OpenBox（未开）
  → opened=true；关 canTouch
  → 存档 tree2fHpMpBoxOpened=true；Animator Open；SFX
  → AddMainItem(HpBall, 3)
  → AddMainItem(MpBall, 3)
  → OpenTipsForm("GetHpBall")   // Item
  → OpenTipsForm("GetMpBall")   // 入队依次 fill
```

| 规则 | 说明 |
|------|------|
| 入包 ∧ Tips | **缺一不可** |
| 数量 vs 图 | 一次入包 3 + **弹一次**对应图（图上可不写 ×3） |
| 队列 | `TipsComponentGSM`：已开 TipsPanel 则 `AddTipsInfo`（0830/0830 验收口径） |
| 缺图 | `GetTipsSprite==null` → **静默不弹**（须保证图集 Key 在运行时 Atlas） |

---

## ⑥ 最小施工清单

1. 新建 `VillageKenMuNi1Data`（`tree2fHpMpBoxOpened` Parse/Serialize）。  
2. 新建 `VillageKenMuNi1HpMpBox`（拷贝 West；默认 3/3；读本 Data；去掉西境/Home 误用 Debug 或改成本档）。  
3. KenMuNi1 `Objects` 下实例化 `Box.prefab`，挂村脚本，坐标落在 WalkArea2 内（建议近 ExitFrom 但错开）。  
4. 登记 `sceneObjs`。  
5. **不改** WalkArea2 几何。  
6. 回归：西境 HpMp 箱、卧室剑箱、针线包 Tips。  
7. 施工说明 + OPEN。

---

## ⑦ 验收

- [ ] WalkArea2 内可交互开箱  
- [ ] 背包 HpBall+3、MpBall+3  
- [ ] 依次 GetHpBall → GetMpBall 横幅 + 物品音效  
- [ ] 同档不可再开；读档保持打开态  
- [ ] WalkArea2 点集/尺寸未变  
- [ ] 剑/空桶/针线包 Tips 正常  
- [ ] Console 无空引用 / Tips 未找到图（若图集丢 Key）  

---

## ⑧ OPEN

| ID | 问题 | 决议 / 默认 | 状态 |
|----|------|-------------|------|
| Q1 | 交互方式？ | **点交互**（`onClick`；对齐 West/Home2） | ✅ |
| Q2 | 专用 ×3 Tips 图？ | **否**；复用 GetHpBall/GetMpBall | ✅ |
| Q3 | 脚本？ | **新建村用类**；勿挂 WestRapp 组件 | ✅ |
| Q4 | 存档挂哪？ | **`VillageKenMuNi1Data.tree2fHpMpBoxOpened`** | ✅ |
| Q5 | 开箱对白？ | **无**（`useStoryOnOpen=false`） | ✅ |
| Q6 | 开箱是否强制即时 SavePlayerBag？ | **否**（对齐 West 内存脏写）；产品要强存另开 | ⏳ |
| Q7 | 上游楼梯换场 + W1 是否已可玩？ | 依赖；未通则箱验不了 | ⏳ |

---

## ⑨ 程序补充

### 关键锚点

| 符号 | 路径 |
|------|------|
| 双球+双 Tips | `WestRappRoadHpMpBox.OnWestRappRoadHpMpBox_GetHpMp` |
| ×3 样板 | `HomeScene1Xiaer.OnHomeScene1GoOutXiaerEnd` |
| Tips 入口 | `TipsComponentGSM.OpenTipsForm` |
| 宝箱 Prefab | `Assets/Prefabs/Box.prefab` |
| 动画 | `Assets/Animation/Object/HomeScene2/Box.controller` |
| 西境实例参考 | `WestRappRoad.unity` PrefabInstance + `hpBallCount/mpBallCount` |
| 村 GSM 存档 | `Village_KenMuNiSceneManager` → 现 `ForestSceneData`（宝箱另用新 Data） |

### 硬禁止

- 改 `VillageWalkArea2` 尺寸/点集  
- 只入包不 Tips / 只 Tips 不入包  
- TipKey 中文文件名  
- 箱在 WalkArea2 **外**  
- 挂 `WestRappRoadHpMpBox` 读西境档  
- 发金币冒充球；新 Tips 系统  

### 与上游

| 依赖 | 不满足时 |
|------|----------|
| 楼梯 → KenMuNi1 2 楼落点 | 玩家到不了箱 |
| W1 WalkArea2 生效 | 落 2 楼被 1 楼 WalkArea 拉回 / 踩不到箱 |
