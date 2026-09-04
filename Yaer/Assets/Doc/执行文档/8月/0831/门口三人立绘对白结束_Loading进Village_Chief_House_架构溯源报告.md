# 门口三人立绘对白结束 → Loading 进 Village_Chief_House — 架构溯源报告

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读定方案（**本阶段未改代码 / 场景 / Prefab**）  
**Unity**：2020.3.48f1  
**产品**：屋外 **三人立绘**对白播完 → **自动** `LoadScene("Village_Chief_House")`；转场主表现 = **`LoadingPanel` 进度条**（非本次主用黑幕）  
**台本/Prefab**：`Village_村长家门口初次对话`  
**提示词**：`提示词/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构侦探提示词.md`  
**关联**：靠近村长黑幕 · 女二侧面涂层 · `House_Chief` · `House_Tree`/`Village_TreeHouseLock` · `02_SYSTEM_SPEC` §5.2 · `技术文档/场景相关/加载地图的加载条功能.md`

---

## 沟通摘要

### ① 结论一句话

**「树屋外」拍板 A＝村长家门口三人戏（非 `House_Tree`）；对白结束后用 L2 在 `ChiefNearDoorStoryTrigger` 订 `onStoryEnd`，复用门同路「Open LoadingPanel → LoadScene(Chief_House, blackFade:false)」自动进屋；`House_Chief` 保留二次进入；现网 `LoadSceneTaskAction` 无 Loading，勿裸调默认黑幕。**

### ② 原因（通俗）

播完「快进屋」后，游戏应自己打开进度条把人送进村长家，不用再点门。  
用户说的「树屋外」其实是门口那段三人戏，不是点树屋锁门对白；进的是村长室内，不是树屋。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 屋外三人立绘对白完整播完 | 雅+古+村可见 |
| 2 | 结束后 | **自动**出 Loading 进度条 → 进 `Village_Chief_House` |
| 3 | 本次进屋主表现 | **进度条**，不是纯黑幕 |
| 4 | 落点 | 室内正确（非卡墙/室外） |
| 5 | `House_Chief` | 仍可点进（二次进入） |
| 6 | `House_Tree` | 仍只播 `Village_TreeHouseLock`，**不**进村长家 |

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 地理「树屋外」 | **A**（村长门口戏） |
| Story | **`Village_村长家门口初次对话`** |
| 自动进屋 | **L2**：`ChiefNearDoorStoryTrigger` / `onStoryEnd` → Loading 切场 |
| Loading API | 对齐 `SceneChangeDoor`：`OpenUIForm(LoadingPanel)` → `LoadScene(name, null, **false**)` |
| 门 | **`House_Chief` 保留** |
| 树屋 | **勿改** `House_Tree` / `Village_TreeHouseLock` |

---

## ② 「树屋外」裁定（A/B/C）

| 假说 | 含义 | 裁定 |
|------|------|------|
| **A** | 口语树屋 ≈ 村长家门口高台戏；对白=`村长家门口初次对话` → `Village_Chief_House` | ✅ |
| B | 真·`House_Tree` 外对白后再进 Chief_House | ❌ 跨点；Tree 现绑锁门对白 |
| C | 树屋外进「树屋场景」 | ❌ 用户已钉 `Village_Chief_House`；现网无树屋室内 Scene 作目标 |

**证伪 B/C**

| 物体 | 世界约 | 行为 |
|------|--------|------|
| **`Npc_Chief`** | `(-157.5, -1.2, 0)` | `ChiefNearDoorStoryTrigger`；Story=`Village_村长家门口初次对话`；Enter；SingleUse |
| **`House_Chief`** | `(-158.3, 1.9, 0)` | `SceneChangeDoor` → `Village_Chief_House`（点 E） |
| **`House_Tree`** | `(28.32, 5.45, 0)` | `Village_TreeHouseLock`「锁上了」；**不**换场进 Chief |

---

## ③ 时序与 Loading

### A. 产品全链（期望）

```
KenMuNi1 · Npc_Chief 靠近
  →（既有）BlackPanel 插层/播三人立绘对白
  → 对白播完（含「快进屋」）
  → 【本需求】OpenUIForm(LoadingPanel)
  → callBack: LoadScene("Village_Chief_House", null, blackFade:false)
  → Chief_House GSM：lastScene=Village_KenMuNi1 → EnterFrom_Village
  → 玩家已在室内
```

**表现分工**

| 阶段 | 表现 | 说明 |
|------|------|------|
| 靠近播对白 | 系统 **BlackPanel** | 已有；遮插入/露立绘 |
| 对白→进屋 | **LoadingPanel** | 本期主表现；**禁止**再主控 BlackPanel 切场 |
| 手动点门 | 现状默认黑幕（`ShowLoadingUI` 未勾） | 可另勾 Loading 与自动进屋对齐（Q5） |

### B. 现网门 Loading 样板（须复用）

`SceneChangeDoor`（`ShowLoadingUI==true`）：

```
OpenUIForm(LoadingPanel, Top, callBack:
  LoadSceneComponentGSM.LoadScene(NextSceneName, null, false))
```

`ShowLoadingUI==false`（含当前 `House_Chief` 源 Prefab 未序列化该字段 → 默认 **false**）：

```
LoadScene(NextSceneName, null, true)  // 黑幕
```

文档契约：`02_SYSTEM_SPEC` —— 对话结束 **不会**自动换场；须显式 `LoadScene`。Loading 与 blackFade **一般不同时主控一次切场**。

### C. `LoadSceneTaskAction` 缺口

| 项 | 现状 |
|----|------|
| 路径 | `LoadSceneTaskAction.cs` |
| 行为 | 直接 `LoadScene(SceneName)` → **默认 blackFade=true** |
| Loading | **无** |

故 **L1 裸挂图末 Action** → 会走出黑幕进屋，**违背**「进度条主表现」。若坚持 L1，须扩 Action：`useLoadingUI` / `blackFade` 参数，或内部调同一助手。

---

## ④ 挂点方案（L1 / L2）

| 方案 | 做法 | 裁定 |
|------|------|------|
| L1 图末 `LoadSceneTaskAction` | Prefab 最后 Action | ⚠️ 现网无 Loading；扩 Action 后可作备选 |
| **L2 onStoryEnd** | `ChiefNearDoorStoryTrigger` 在剧情成功启动后，结束时开 Loading+Load | ✅ **推荐** |
| L3 只解锁门仍点 E | — | ❌ 产品要自动 |
| L4 对白完黑幕进门 | — | ❌ 产品要进度条 |

### L2 最小实现要点

1. **抽取**可复用助手（倾向放 `LoadSceneComponentGSM` 或小静态工具，门与 Trigger 共用）：  
   `LoadSceneWithLoadingPanel(string sceneName)`  
   = 现门 `ShowLoadingUI` 分支同逻辑。  
2. `ChiefNearDoorStoryTrigger`：在 `TryStartBoundStory()` 成功后，额外订阅 `onStoryEnd`（或把基类 `OnStoryFinished` 改 `protected virtual` 后 `override` 里 `base` + Load）。  
3. 回调内：**仅当**本次 Story 名为门口初次对话时进屋（防误伤）。  
4. 调用 `LoadSceneWithLoadingPanel(SceneName.Village_Chief_House)`。  
5. **勿**在进屋路径再 `Open BlackPanel`。

**注意**：基类 `OnStoryFinished` 当前为 `protected void`（非 virtual）——施工二选一：改 virtual 覆写，或并行第二订阅（须在 `OnDestroy` 解绑）。

**与单次存档**：`SingleUseInArchive=1` 已挂 `Npc_Chief`；播完记档后自动进屋；回村后再走近不重播（门仍可进）。

---

## ⑤ EnterPos / LastScene

| 项 | 磁盘核实 |
|----|----------|
| 室内 GSM | `Village_Chief_House` · `EnterPosConfig` |
| 条目 | `lastScene: **Village_KenMuNi1**` → pos `EnterFrom_Village` |
| 落点 Transform | local **`(-4.23, -3.65, 0)`**（父为场景根级 Objects 链，Z=0） |
| 常量 | `SceneName.Village_Chief_House` |

`ChangeSceneComponentGM` 维护 `LastSceneName`：从 KenMuNi1 `LoadScene` 进房后，室内应按上表取 `EnterFrom_Village`。

**验收**：落点非 DefaultBorn、非卡墙、非仍在村坐标。

回村：`KenMuNi1` 已有 `lastScene: Village_Chief_House` 落点（既有），本期不改。

---

## ⑥ 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 抽取 `LoadSceneWithLoadingPanel`；`SceneChangeDoor` 可选改为调用（去重） | **P0** |
| 2 | `ChiefNearDoorStoryTrigger`：`onStoryEnd` → Loading 进 `Village_Chief_House` | **P0** |
| 3 | 确认门口 Prefab 已 Setup 可播完（菜单依赖） | **P0** |
| 4 | `House_Chief` **保留**；勿删门 | — |
| 5 | **不改** `House_Tree` / TreeHouseLock | — |
| 6 | 回归：手动点门仍进屋；女二侧面随村卸载 | P0 |

**依赖**：三人立绘 Prefab（Setup 菜单）；靠近黑幕触发已挂 `Npc_Chief`。

**不改**：室内家具玩法；树屋 SceneName；用 BlackPanel 当本次进屋主表现。

---

## ⑦ 验收清单

同沟通摘要 §③；另：

- [ ] Console：`[SceneLoad] scene=Village_Chief_House blackFade=False`（或等价）
- [ ] 靠近播对白那次黑幕与进屋 Loading **不叠成「双黑主控」**
- [ ] 同档播过后自动进屋只发生一次（跟对白单次）；之后靠门进

---

## ⑧ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 「树屋外」= 村长门口戏？ | **A** | ✅ |
| Q2 | 自动进屋后 `House_Chief` 是否仍可点？ | **保留** | ✅ |
| Q3 | Load 挂图末 vs onStoryEnd？ | **L2** | ✅ |
| Q4 | Loading 假读条时长？ | 对齐现网 LoadingPanel 默认 | ✅ |
| Q5 | 手动 `House_Chief` 是否也改 ShowLoadingUI？ | **建议勾**与自动进屋表现一致 | ⏳ |
| Q6 | 对白 Prefab 未 Setup 时是否先合 Load？ | 可先合代码；全链路等 Prefab | ⏳ |

---

## ⑨ 程序补充（速查）

| API / 锚点 | 用途 |
|------------|------|
| `SceneChangeDoor` + `ShowLoadingUI` | Loading 开法金样 |
| `LoadSceneComponentGSM.LoadScene(name, stay, blackFade)` | `blackFade:false` 配 Loading |
| `UIPrefabPath` → `LoadingPanel` | 进度条 UI |
| `LoadSceneTaskAction` | **无 Loading**；默认黑幕——勿裸用 |
| `ChiefNearDoorStoryTrigger` | L2 挂点；已有黑幕播对白 |
| `Npc_Chief` | `(-157.5,-1.2,0)`；Story 门口初次；SingleUse；Enter |
| `House_Chief` | `(-158.3,1.9,0)` → Chief_House；**保留** |
| `House_Tree` | TreeHouseLock；**禁止**误接进屋 |
| `EnterFrom_Village` | Chief_House · lastScene KenMuNi1 · `(-4.23,-3.65,0)` |
| `SceneName.Village_Chief_House` | 常量 |

**一句话合并上游**：靠近黑幕只服务「插层+播三人戏」；**进屋**另开 Loading，二者职责分离、顺序衔接。
