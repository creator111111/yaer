# Village_HomeScene23 进屋主角不出现 — 架构溯源报告

**文档版本**：v1.0（2026-08-04）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / 场景 / Build Settings**）  
**范围**：已从 `Village_KenMuNi1` 进入 **`Village_HomeScene23`**，但主角看不见、不能移动；对照 `Village_HomeScene2` / `Village_House4` 室内样板列缺项。  
**依据**：提示词 `0804/Village_HomeScene23_进屋主角不出现_架构侦探提示词.md`；`0530` House4 / `0606` HomeScene2 / `0601` HomeScene4 NPC 文档；场景与 Config 静态阅读  

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**这是室内屋；玩家大概率「已生成」但落点/相机/半成品门配置导致「看不见 + 不能正常玩」——不是 `canCreatePlayer=false`。主缺项：空的 `EnterPosConfig`（落到 `DefaultBorn` Y=-3.65，而样板进门用 `LeftBorn` Y=-1.3）、`SceneName`/`Build` 未登记本场景、`nowSceneName` 误报 `Village_House4`、出门链坏（左门空且组件禁用、右门 `ForestScene`）。按 HomeScene2 室内清单补齐即可恢复，不必重写换场总管线。**

三类归类：**已生成但看不见（落点/相机）** + **已生成但无法正常移动/出门（门与半成品）**；**不是**「配置禁止创建玩家」。

---

## ② 原因（生活类比）

钥匙开对了门进了屋（场景能加载），屋里该有的「门口地标贴纸」（`EnterPosConfig`）、户口本登记（`SceneName` + Build）、正确门牌（`nowSceneName`）、回村的门（左门）都还是半成品拷贝——人可能被搁在错误坐标、镜头盯错、或一碰右门飞去森林，体感就是「没人 / 不能动」。

### 这是室内场景吗？

**是。** `Village_HomeScene23.unity` = 肯姆尼村内某户室内（文档曾作商店/老板娘屋、NPC 对白样板）。  
村里入口：`House_Npc4` → `NextSceneName = Village_HomeScene23`（与多数 `House4*` → `Village_House4` **不是同一目标**）。

### 玩家有没有生成？

| 检查 | 结果 |
|------|------|
| `config` | 绑 **`HomeScene1.asset`** |
| `canCreatePlayer` | **1** → `InitPlayer` **会** `CreatePlayer` |
| `canMove` | **1** |
| `isFightingScene` | **0**（室内 Home 动画侧） |
| Manager | `Village_House4SceneManager` → `GetCurSceneTerrainType=Indoor` |

→ 根因**不是**「禁止创建」。验收时应在 Hierarchy 搜 Player；若有实体仍看不见 → 落点/相机；若完全没有 → 再查 CreatePlayer 运行时 Error（静态不像主因）。

### 为何「看不见 / 不能动」（高概率组合）

1. **`EnterPosConfig: []` 空** → 进门走 `DefaultBornPos` `(-24.12, -3.65)`；样板 HomeScene2 从村进门绑的是 **`LeftBorn`（同 X，Y=-1.3）**。Y 差约 2.35，易进地/错层 → 看不见或卡死。  
2. **双 Camera / 结构拷贝残留**（根级 Camera + SceneManager/Camera；Map 在 SceneManager 下）——跟错空相机时「没人」。  
3. **左门** `SceneChangeDoor` **Enabled=0** 且 `NextSceneName` 空；**右门** `NextSceneName=ForestScene` 且 `TriggerWhenMoveIn=1`——半成品，易「不能回村 / 误飞森林」。  
4. 身份漂移：`nowSceneName=Village_House4` ≠ 文件名 `Village_HomeScene23`；村 `EnterPos` 回村表只见 `lastScene: Village_House4`，与 HomeScene4 回村匹配易错。

### G1–G7 核实

| ID | 预梳理 | 核实 |
|----|--------|------|
| G1 | SceneName 无常量 | **证实**：有 `Village_House4` / `Village_HomeScene2`，**无** `Village_HomeScene23` |
| G2 | Build 未见 | **证实**：Build 有 HomeScene2、House4、Shop 等，**无** HomeScene4；Editor 靠路径偶发能进 |
| G3 | EnterPos 空 | **证实** |
| G4 | config=HomeScene1 | **证实**（guid→`HomeScene1.asset`）；canCreate/canMove 仍为 1，**不是**禁生成，但是错资产身份 |
| G5 | 左门 Forest | **修正**：左门 Next 空且组件禁用；**右门**才是 `ForestScene` |
| G6 | 双 Camera/Map | **证实**有双 Camera；Map 主份在 SceneManager 下 |
| G7 | nowSceneName 漂移 | **证实**：挂 `Village_House4SceneManager`，写死 `Village_House4` |

**另**：磁盘上 **无** `Village_House4.unity`，但 Build 仍登记该路径——多数 `House4*` 门指向的目标场景文件缺失；与 HomeScene4 问题并列，勿混为一谈。

---

## ③ 用户需要做什么

### 拍板（OPEN）

1. **本场景正式定位**：民居室内 / 废弃商店屋 / **门改指别处**？  
2. **修复策略 A**：把 `House_Npc4` 补成可玩的 HomeScene4（对齐 HomeScene2 清单）  
   **还是 B**：门改指其它已通室内（注意 `Village_House4.unity` 当前磁盘缺失）？  
3. Manager：新建 `Village_HomeScene23SceneManager`（推荐，仿 HomeScene2）还是改复用并改 `nowSceneName`？  
4. 与纯 UI `Village_Shop`：是否继续分离（0713 已定 Shop 独立）？

### 用户侧检查清单（进 Play 前可先看）

1. Hierarchy 进屋后有没有 **Player**？  
2. Player 世界坐标是否在 `(-24, -3.65)` 附近？改拖到 `LeftBorn` 高度是否立刻可见可走？  
3. 启用的 Camera 是否跟 Player？  
4. Console 有无 CreatePlayer / 换场 Error？

### 验收（施工后）

1. InitScene → 村 → `House_Npc4` → 主角出现在门口可走  
2. Hierarchy 有 Player；相机跟随  
3. 左门回 `Village_KenMuNi1` 落在门外  
4. 再进再出稳定；Console 无相关 Error  

---

## ④ 给程序看的补充

### 4.1 与样板 Diff（必出）

| 项 | Village_HomeScene2（通） | Village_House4（文档样板） | Village_HomeScene23（现状） |
|----|--------------------------|----------------------------|----------------------------|
| 场景文件 | 有 | **磁盘缺失**（Build 仍有条目） | 有 |
| Build Settings | 有 | 有（文件却无） | **无** |
| `SceneName` 常量 | 有 | 有 | **无** |
| 专用 SceneManager | `HomeScene2SceneManager`，`nowSceneName` 对齐 | `House4SceneManager`→`Village_House4` | **误挂 House4Manager**，名写成 House4 |
| Config 资产 | `Village_HomeScene2.asset` | `Village_House4.asset` | **`HomeScene1.asset`** |
| canCreatePlayer / canMove | 1 / 1 | 1 / 1 | 1 / 1（借 HomeScene1） |
| EnterPosConfig | `lastScene: Village_KenMuNi1` → LeftBorn | （样板应对齐门口） | **[] 空 → DefaultBorn** |
| 出门左门 | `Village_KenMuNi1` | 应对齐回村 | **空 + 组件禁用** |
| 右门 | 空/关 | — | **`ForestScene` + MoveIn** |
| Indoor 脚步 | IndoorType | IndoorType | IndoorType（靠误挂的 House4Manager） |
| 村门入口 | House_NPC2 | House4* | **House_Npc4 → HomeScene4** |

### 4.2 根因表

| 环节 | 现状 | 阻塞出现/移动？ | 必须改？ | 备注 |
|------|------|-----------------|----------|------|
| canCreatePlayer | 1（HomeScene1） | 否（应能生成） | 建议换对本场景 Config | 非主因 |
| EnterPosConfig | 空 | **是**（错 DefaultBorn） | **是** | 对齐 LeftBorn |
| DefaultBornPos | (-24.12,-3.65) | 与门口高度不一致时 **是** | 建议校正或勿作进门点 | |
| Camera 双份 | 根+SM 下各一 | 可能 | 建议清理/确认 Follow | |
| Build Settings | 无本场景 | 正式包 **是**；Editor 偶发能进 | **是** | |
| nowSceneName | Village_House4 | 回村落点/存档 **是** | **是** | |
| canMove | 1 | 否 | 否 | |
| 左门回村 | 空+禁用 | 出门 **是** | **是** | → KenMuNi1 |
| 右门 | ForestScene | 误触 **是** | **是** | 清空/禁用 |
| SceneName 常量 | 无 | 代码引用/一致性 | **是** | |
| Village_House4.unity | 磁盘无 | 其它 House4 门 | 另案 | 勿与本期混修除非拍板合并 |

### 4.3 调用链（进屋落点）

```
House_Npc4 → LoadScene("Village_HomeScene23")
  → Village_House4SceneManager.OnInit（nowSceneName=Village_House4 ← 错）
  → InitPlayer（canCreatePlayer=1）→ CreatePlayer
  → SetPlayerPos：EnterPos 空 → DefaultBorn (-24.12, -3.65)
  → Camera.SetFollow(player)
```

对照 HomeScene2：`EnterPos` 命中 `Village_KenMuNi1` → **LeftBorn**。

### 4.4 施工员最小改动建议（只建议）

**推荐：对齐 HomeScene2 交付清单（策略 A），新建专用 Manager。**

| 步骤 | 操作 |
|------|------|
| 1 | `SceneName.cs` 增 `Village_HomeScene23` |
| 2 | 新建 `Village_HomeScene23SceneManager`（抄 HomeScene2，`nowSceneName` 对齐）+ 可选 Config `Village_HomeScene23.asset` |
| 3 | 场景：换 Manager、换 config；`EnterPosConfig` 加 `Village_KenMuNi1` → LeftBorn |
| 4 | 左门：启用，`NextSceneName=Village_KenMuNi1`；右门去掉 Forest / 关 TriggerWhenMoveIn |
| 5 | Editor Build Settings **加入** `Village_HomeScene23.unity` |
| 6 | 村侧：确认回村 `EnterPos` 增加 `lastScene: Village_HomeScene23`（现仅见 House4） |
| 7 | （建议）理顺双 Camera，确认 Follow 用的是 GSM 相机 |

**不必**：重写 `LoadScene` / `InitPlayer` 总管线。  
**是否必须新建 Manager**：建议是（仿 HomeScene2）；若强行只改 `House4SceneManager.nowSceneName` 会破坏「真·House4」身份（且 House4 场景文件当前还缺）。

**策略 B（门改指）**：仅当产品废弃 HomeScene4；且目标场景文件必须真实存在——**当前不要改指缺失的 Village_House4.unity**。

### 4.5 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | 正式定位：民居 / 废弃改指 / 商店遗留？ | 先当可玩民居补齐；商店走 `Village_Shop` |
| Q2 | 新建 Manager 还是改 House4 复用？ | **新建 HomeScene4Manager** |
| Q3 | 与 Village_Shop 是否区分？ | **是**（0713 已定纯 UI 店） |

---

## 施工员下一轮最小化清单（建议）

1. SceneName + Build + 专用 Manager/Config  
2. EnterPos + 左右门  
3. 村回村落点表补 HomeScene4  
4. 验收门口可见可走、回村正确  
