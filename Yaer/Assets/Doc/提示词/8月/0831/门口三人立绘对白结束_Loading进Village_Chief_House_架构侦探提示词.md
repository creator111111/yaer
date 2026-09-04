# Cursor Agent Prompt · 树屋外/村长门口三人立绘对白结束 → Loading 进度条进 Village_Chief_House

> **角色**：先【架构侦探】只读定方案，报告后再【施工员】  
> **日期**：2026-08-31  
> **产品设定（钉死）**：  
> 1. **屋外对话**使用 **三人立绘**（雅 + 古 + 村长）播完  
> 2. 对话 **完成后自动** 跳转 **`Village_Chief_House`**（进屋）  
> 3. 进屋转场用本游戏 **进度条画面**（`LoadingPanel`），**不要**只用黑幕当这次进屋主表现  
> **台本/Prefab 倾向**：`Village_村长家门口初次对话`（三人 + Face123；与近期 Npc_Chief / 女二侧面同一条戏）  
> **用户口「树屋外」**：现网 `House_Tree` 走的是锁门对白 `Village_TreeHouseLock`、**无** `Village_Chief_House` 场景；目标场景是 **村长家室内**——侦探须拍板「树屋外」= 合层村长门口戏，还是误指树屋交互（报告写清，勿 silently 改成进树屋场景）  
> **本阶段（侦探）**：只读  
> **报告落盘**：`Assets/Doc/执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望）

```
屋外（KenMuNi1 · 村长旁）
  → 三人立绘对白 Village_村长家门口初次对话 播完
  → 【自动】开 LoadingPanel（假读条 / 进度条转场）
  → LoadScene("Village_Chief_House")，blackFade=false（与门 ShowLoadingUI 同路）
  → 室内 EnterPos：lastScene=Village_KenMuNi1 落点
  → 玩家已在村长家，无需再点 House_Chief（或门仍保留作二次进入）
```

**规范钉死**（`02_SYSTEM_SPEC` / `技术文档/场景相关/场景切换.md`）：

- 对话结束 **不会**自动换场 → 须显式 `LoadScene` / `LoadSceneTaskAction` / 门逻辑复用  
- **Loading 进度条** = `LoadingPanel`；门上 `ShowLoadingUI=true` 时：`OpenUIForm(LoadingPanel)` → `LoadScene(..., blackFade:false)`  
- 默认门黑幕与 Loading **一般不同时主控一次切场**

### 与现网资产对照

| 项 | 现状 |
|----|------|
| 三人立绘对白 | `Village_村长家门口初次对话`（CSV + Setup 菜单；施工中/已有） |
| 靠近触发 | `Npc_Chief` + `ChiefNearDoorStoryTrigger`（黑幕播对白） |
| `House_Chief` | `SceneChangeDoor` → `Village_Chief_House`（点 E 进屋；默认多黑幕） |
| `Village_Chief_House` EnterPos | 已有 `lastScene: Village_KenMuNi1` |
| `House_Tree` | `Village_TreeHouseLock` 锁门对白，**不是**进 Chief_House |
| Loading | `SceneChangeDoor.ShowLoadingUI` / `加载地图的加载条功能.md` |

### 「树屋外」裁定（OPEN · 必答）

| 假说 | 含义 | 倾向 |
|------|------|------|
| **A** | 用户口语树屋 = 村长家门口高台戏；对白=`村长家门口初次对话` → Chief_House | ✅ 接续 0831 全线 |
| B | 真·树屋 `House_Tree` 外对白后再进 Chief_House | ⚠️ 产品跨点；须另台本 |
| C | 树屋外进「树屋场景」 | ❌ 用户已写死 `Village_Chief_House` |

### 对白结束后自动进屋方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **L1 · 对话图末 `LoadSceneTaskAction`** | Prefab 图最后 Action：开 Loading + LoadScene | 须确认 Action 是否支持 Loading；否则扩 |
| **L2 · `onStoryEnd` 在 Trigger/GSM** | `ChiefNearDoorStoryTrigger` 或 GSM 订 onStoryEnd → 复用门的 Loading 开法 | ✅ 与「靠近播对白」同组件好收束 |
| L3 · 对白完只解锁门，仍要点 E | ❌ 产品要自动 |
| L4 · 对白完黑幕进门 | ❌ 产品要 **进度条** |

抽取建议：把门上「ShowLoadingUI → Open LoadingPanel → LoadScene(name, blackFade:false)」收成可复用 API（`LoadSceneComponentGSM` 或静态小助手），门与对白结束共用，避免两套假读条。

### 三人立绘（验收）

| 角色 | 大立绘 |
|------|--------|
| 雅 | GoOut / 村线 |
| 古 | GushaPainting（UI） |
| 村 | ChiefPainting Face1～3 |

女二 **场景侧面涂层**（若已施工）与 UI 三人立绘并存；进室内后侧面层随村场景卸载即可。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 三人立绘屋外对白播完 → 自动进 `Village_Chief_House` | ❌ 改成进不存在的树屋 SceneName |
| ✅ 转场主表现 = **LoadingPanel 进度条** | ❌ 这次进屋主用 BlackPanel（可保留靠近播对白那次黑幕） |
| ✅ EnterPos / LastScene 验收 | ❌ 重做室内家具玩法 |
| ✅ 与 `House_Chief` 关系写清（保留/弱化） | ❌ 拆掉所有进屋门导致无法再进 |

### 严禁

- 对白结束无显式 LoadScene（假自动）  
- Loading 与 blackFade 双开抢表现  
- 落点错成 DefaultBorn / 室外坐标  
- 把 `House_Tree` 锁门对白误接成进 Chief_House  

### 开放

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 「树屋外」= 村长门口戏？ | **A** |
| Q2 | 自动进屋后 `House_Chief` 是否仍可点？ | **保留**二次进入 |
| Q3 | Load 挂图末 vs onStoryEnd？ | **L2** 或 L1+Loading 能力补齐 |
| Q4 | Loading 假读条时长？ | 对齐现网 LoadingPanel 默认 |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
1. 屋外用三人立绘播完对话（倾向 Village_村长家门口初次对话）。
2. 对白结束后自动进入 Village_Chief_House。
3. 进屋转场使用游戏 LoadingPanel 进度条（ShowLoadingUI 同路），非本次主用黑幕。
4. 澄清用户「树屋外」与 House_Tree / 村长门口 / Chief_House 的对应关系。

## 必读
@Assets/Doc/02_SYSTEM_SPEC.md（§5.2 换场）
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/技术文档/场景相关/加载地图的加载条功能.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/LoadSceneComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/ChiefNearDoorStoryTrigger.cs
@Assets/Doc/执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md
@Assets/Doc/施工说明/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_施工说明.md
@Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构溯源报告.md
@Assets/Doc/执行文档/0830/Village_KenMuNi1_House_Tree交互Village_TreeHouseLock_架构溯源报告.md
@Assets/GameRes/Scenes/Village_Chief_House.unity（EnterPosConfig）
@Assets/Dialog/Village_村长家门口初次对话.csv

检索：LoadSceneTaskAction、ShowLoadingUI、LoadingPanel、onStoryEnd、House_Chief、Village_Chief_House。

## 侦探任务
1. 拍板屋外对白 Prefab 名与「树屋外」地理含义（A/B/C）。
2. 画出：对白结束 → LoadingPanel → LoadScene(Chief_House) → EnterPos 全链；对比现网 House_Chief 门。
3. 推荐 L1/L2；若 LoadSceneTaskAction 无 Loading，给出最小扩法或复用门逻辑。
4. 三人立绘验收依赖（Prefab 是否已 Setup）。
5. 最小清单 + 验收 + OPEN（含门是否保留）。

## 报告落盘
Assets/Doc/执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md

结构：①结论 ②树屋外裁定 ③时序与 Loading ④挂点方案 ⑤EnterPos ⑥最小清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md

## 目标
1. 三人立绘屋外对白（报告定名）播完后自动 LoadScene("Village_Chief_House")。
2. 转场表现走 LoadingPanel 进度条（对齐 SceneChangeDoor.ShowLoadingUI：先开 Loading，LoadScene blackFade=false）。
3. 室内 lastScene=Village_KenMuNi1 落点正确。
4. 与靠近黑幕播对白、女二侧面按报告共存；避免「对白黑幕 + 进屋再黑幕」叠成双黑（进屋应用进度条）。

## 落盘
Assets/Doc/施工说明/0831/门口三人立绘对白结束_Loading进Village_Chief_House_施工说明.md
同步 OPEN_QUESTIONS。

## 验收
- [ ] 屋外三人立绘对白可完整播完
- [ ] 结束后自动出现 Loading 进度条并进入 Village_Chief_House
- [ ] 本次进屋主表现是进度条，不是纯黑幕
- [ ] 落点在室内正确位置（非卡墙/室外）
- [ ] House_Chief 按报告：仍可进或行为符合决议
- [ ] House_Tree 锁门对白未被误改成进村长家

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. 先跑「侦探 Prompt」——确认「树屋外」是否就是 **村长家门口三人戏**。  
2. 再跑「施工 Prompt」。  
3. 进度条 = 现成 **`LoadingPanel`**（与门勾 ShowLoadingUI 同路），不是新做一套 UI。
