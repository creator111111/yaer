# Village · 门口进村长家改黑屏切场 — 架构溯源报告

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【架构侦探】只读定方案（**本阶段未改代码 / 场景 / Prefab**）  
**Unity**：2020.3.48f1  
**现象**：村长家门口 → `Village_Chief_House` 出现 **LoadingPanel**（蛋糕 Q 版 + 粉进度条）  
**产品改口（钉死 · 推翻 0831「进屋=读条」）**：日常进屋用系统 **BlackPanel**；**LoadingPanel 仅留给时间跳转**  
**不是**：取消自动进屋 / 取消续聊 / 改落点 / WalkArea  
**提示词**：`提示词/0902/Village_门口进村长家_改黑屏切场_架构侦探提示词.md`  

**上游将被改口**：
- `执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md`（L2=Loading）
- `执行文档/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构溯源报告.md`（依赖 Loading 盖景）

**关联**：`02_SYSTEM_SPEC` §5.2 · `技术文档/场景相关/场景切换.md` · 楼梯上楼 `ShowLoadingUI=false` 样板

---

## 沟通摘要

### ① 结论一句话

**H1+H2 双源读条：自动进屋走 `ChiefNearDoorStoryTrigger`→`LoadSceneWithLoadingPanel`；手动 `House_Chief` 亦 `ShowLoadingUI=1`。施工 F1+F2 改日常黑幕；续聊门闩保留，但须处理 H3——黑幕路径下 `OnEnterScene` 在淡出之后，建议同批用 `TryDeferBlackFadeForCover` 全黑内 Trigger（F1′），禁止开回 Loading。**

### ② 原因（通俗）

0831 故意把「对白结束进屋」做成蛋糕读条，还让手动门也勾了读条。  
产品现在说：普通进屋跟别的门一样黑一下就行，蛋糕读条留给「时间跳转」。  
另外：以前续聊靠读条还盖着时开播；改成黑幕后，现网 `OnEnterScene` 是**黑幕淡完才调**，若不微调，可能先闪一眼室内再出对话框。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 门口三人戏结束 → 进屋：**只见黑屏**，不见蛋糕/粉条 | |
| 2 | 落点仍对（`EnterFrom_Village` / Walk 内） | |
| 3 | 自动续聊仍播；**无明显露景漏缝** | |
| 4 | 手动按 E `House_Chief`：同样黑屏不读条；门闩续聊逻辑仍对 | |
| 5 | LeftDoor 出屋、楼梯上楼、出村长家送树屋：仍黑幕 | |
| 6 | 其它仍勾 `ShowLoadingUI` 的时间跳转门：读条未被误关 | |

### ④ 程序补充

见下文 §①～§⑨。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 自动进屋读条源 | **H1** · `ChiefNearDoorStoryTrigger.OnStoryFinished` → `LoadSceneWithLoadingPanel` |
| 手动门读条源 | **H2** · `Village_KenMuNi1` 实例 `House_Chief` Override **`ShowLoadingUI=1`** |
| 主修 | **F1** 改 `LoadScene(Village_Chief_House)`（默认 `blackFade:true`） |
| 必做对齐 | **F2** `House_Chief.ShowLoadingUI=false` |
| 续聊门闩 | **保留** `TryTriggerChiefContinueOnce`（门口已用 ∧ 续聊未用） |
| 遮罩时序 | **H3 成立风险** → 推荐同批 **F1′**：`ShouldPlayChiefContinue` 时 `TryDeferBlackFadeForCover` 全黑 Trigger，壳就绪再淡出 |
| API | **保留** `LoadSceneWithLoadingPanel`（勿删）；只停用门口路径 |
| 否 | F3 删 API；F4 缩短假读条冒充黑屏；为露景开回 Loading |

---

## ② 进 `Village_Chief_House` 且开 LoadingPanel 的调用点（任务 1）

| # | 调用点 | 磁盘核实 | 本期 |
|---|--------|----------|------|
| 1 | `ChiefNearDoorStoryTrigger.OnStoryFinished` → `LoadSceneWithLoadingPanel(Village_Chief_House)` | `ChiefNearDoorStoryTrigger.cs` L256–259 | **F1 改掉** |
| 2 | `SceneChangeDoor` 且 `ShowLoadingUI` → 同上助手 | `House_Chief` Override `ShowLoadingUI: 1`，`NextSceneName=Village_Chief_House` | **F2 改 false** |

`LoadSceneWithLoadingPanel` 全工程 C# 引用仅：

| 文件 | 角色 |
|------|------|
| `LoadSceneComponentGSM.cs` | API 定义（Open LoadingPanel → `LoadScene(..., blackFade:false)`） |
| `ChiefNearDoorStoryTrigger.cs` | 自动进屋（本期停用） |
| `SceneChangeDoor.cs` | 门勾选时转发（保留机制；门口门改 false） |

其它场景仍有 `ShowLoadingUI:1` 的门（例：`ForestScene` / `ForestEastScene` / `Village` / `Village_KenMuNi_night`；`Village_Chief_House` 内一扇 `NextSceneName: ForestEastScene`）——**非本期「门口进村长家」**，默认不动，作时间跳转/历史用途保留。

---

## ③ 现网 vs 目标时序（任务 2）

### 3.1 现网（0831 L2 + 0901 续聊）

```
门口三人戏结束
  → ChiefNearDoor.OnStoryFinished
  → LoadSceneWithLoadingPanel(Chief_House)
       → OpenUIForm(LoadingPanel)          ← 蛋糕读条（用户截图）
       → LoadScene(..., blackFade:false)   ← 故意不用黑幕
  → GSM Ready → OnBlackFadeEnd（立即）
  → OnEnterScene → TryTriggerChiefContinueOnce()
       // 注释：趁 LoadingPanel 仍盖住
```

### 3.2 目标（产品改口）

```
门口三人戏结束
  → ChiefNearDoor.OnStoryFinished
  → LoadScene(Village_Chief_House)         ← 默认 blackFade:true
       → BlackPanel FadeShow → 全黑
       → 卸场 / 加载 Chief_House
       → GSM Ready
            ├─【推荐 F1′】若应播续聊：TryDeferBlackFadeForCover
            │     → 仍全黑时 TriggerStory(继续对话)
            │     → 壳就绪 / 闸门 → CloseFormFade → OnEnterScene（可不再重复 Trigger）
            └─【仅 F1】hold → CloseFormFade → OnEnterScene → Trigger
                 ⚠ 淡出后才 Trigger → H3 露景风险
```

### 3.3 露景风险点（H3）

| 路径 | `OnEnterScene` 时机 | 续聊遮罩 |
|------|---------------------|----------|
| Loading（现网） | Ready 后立刻；Loading 仍盖 | 相对安全（靠读条皮） |
| Black 默认契约 | **`CloseFormFade` 完成之后** | ⚠ 室内可能先可见再出壳 |
| Black + Defer（F1′） | 淡出前已 Trigger；对齐进村开场 | ✅ 推荐 |

`stayAction`：在**旧场景**全黑 `onShowEnd` 时执行，**早于**新 GSM Ready，不能直接挂新场景 `TriggerStory`。H3 应用 **`TryDeferBlackFadeForCover`**（新场景 Ready、仍全黑），勿误用 stayAction 当续聊挂点。

---

## ④ 方案与最小改动清单（任务 3）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1** | `OnStoryFinished`：`LoadSceneWithLoadingPanel` → `LoadScene(SceneName.Village_Chief_House)`（或显式 `blackFade:true`） | ✅ 主修 |
| **F2** | `Village_KenMuNi1` · `House_Chief` · `ShowLoadingUI` **1→0** | ✅ 必做 |
| **F1′** | `Village_Chief_HouseSceneManager`：应播续聊时 override `TryDeferBlackFadeForCover`，全黑 Trigger，壳就绪再淡出；`OnEnterScene` 防重复 | ✅ **强烈建议同批**（治 H3） |
| F1″ | 仅改注释 + 接受短露景（0901 Q3 旧默认） | ⚠️ 仅当产品接受闪景 |
| F3 | 删除 `LoadSceneWithLoadingPanel` | ❌ |
| F4 | 缩短假读条 | ❌ |

### 施工最小清单

1. **`ChiefNearDoorStoryTrigger.cs`**
   - `LoadSceneWithLoadingPanel(...)` → `LoadScene(SceneName.Village_Chief_House)`
   - 更新类注释 / Header「对白结束→进屋」：黑幕而非 Loading；说明产品「读条仅时间跳转」
2. **`Village_KenMuNi1.unity`**
   - `House_Chief` Override：`ShowLoadingUI = 0`
3. **`Village_Chief_HouseSceneManager.cs`**
   - 注释：遮罩依赖 BlackPanel，不再写 LoadingPanel
   - **推荐 F1′**：`TryDeferBlackFadeForCover` + 续聊门闩；`OnEnterScene` 仅补「未在 defer 已播」路径
4. **保留** `LoadSceneWithLoadingPanel` API 与其它场景门勾选
5. **同步 OPEN**（推翻 0831 进屋=Loading；0901 遮罩依赖改口）
6. **禁止**改 EnterPos / WalkArea / 对话 Prefab / 楼梯与送树屋已定黑幕案

### stayAction 要不要动？

| 问 | 答 |
|----|-----|
| F1 是否必须传 stayAction？ | **否**；默认 `null` 即可 |
| 续聊能否挂 stayAction？ | **否**（旧场景时机）；用 F1′ defer |

---

## ⑤ 必须保留的 Loading 调用方（任务 4）

| 保留项 | 说明 |
|--------|------|
| `LoadSceneComponentGSM.LoadSceneWithLoadingPanel` | API 本身；时间跳转入口 |
| `SceneChangeDoor` + `ShowLoadingUI==true` 分支 | 机制保留；**仅**门口 `House_Chief` 改 false |
| 其它场景已勾读条的门 | Forest* / Village* / Chief_House→ForestEast 等 —— **本期不扫改**；产品若认定「非时间跳转」另案清 |

**时间跳转例外（产品语义）**：章末/地图长跳、明确「过了一段时间」的演出换场 → 可继续用 LoadingPanel。  
**日常进门/门口进屋** → BlackPanel。

---

## ⑥ OPEN 改口要点（任务 5）

见本报告落盘后 `OPEN_QUESTIONS.md` 同步：

| 旧决议（0831） | 新决议（0902） |
|----------------|----------------|
| 进屋主表现 = LoadingPanel（L2） | 进屋主表现 = **BlackPanel** |
| 手动门建议勾 ShowLoadingUI | **取消勾选**（与日常门一致） |
| 0901 续聊靠 Loading 盖景 | 续聊靠 **Black 全黑（F1′）** 或接受短闪（F1″） |

---

## ⑦ 假说证伪摘要

| ID | 假说 | 结果 |
|----|------|------|
| **H1** | 自动进屋唯一读条 = NearDoor → Loading 助手 | ✅ 成立（主因之一） |
| **H2** | `House_Chief` ShowLoadingUI=true | ✅ 成立（磁盘 Override=1） |
| **H3** | 改黑幕后续聊与淡出竞态露景 | ✅ **时序上成立**（默认契约淡出后才 OnEnterScene）；须 F1′ 或接受闪景 |

---

## ⑧ 回归与风险

| 风险 | 缓解 |
|------|------|
| 续聊露景 | F1′ defer；禁止开回 Loading |
| Setup/文档仍写「进屋=读条」 | 改注释 + OPEN；勿让后人按 0831 复原 |
| 误关全项目 Loading | 只改 NearDoor + House_Chief；勿删 API |
| 落点回归 | 不改 EnterPos；抽测从村进屋坐标 |

---

## ⑨ 代码锚点速查

| 主题 | 路径 |
|------|------|
| 自动进屋 | `ChiefNearDoorStoryTrigger.OnStoryFinished` |
| Loading 助手 | `LoadSceneComponentGSM.LoadSceneWithLoadingPanel` |
| 日常黑幕 | `LoadSceneComponentGSM.LoadScene(..., blackFade:true)` |
| 手动门 | `SceneChangeDoor` + 场景 `House_Chief` |
| 续聊 | `Village_Chief_HouseSceneManager.TryTriggerChiefContinueOnce` |
| 全黑遮罩样板 | `Village_KenMuNiSceneManager.TryDeferBlackFadeForCover` |

---

## ⑩ 给施工员的一句话

**F1+F2 去掉门口双读条；同批尽量做 F1′ 全黑内播续聊。API 留着给真·时间跳转，别删。**
