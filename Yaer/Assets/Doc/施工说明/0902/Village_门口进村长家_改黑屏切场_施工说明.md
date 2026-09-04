# Village · 门口进村长家改黑屏切场 — 施工说明

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【施工员】按侦探报告 F1+F2+F1′ 落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0902/Village_门口进村长家_改黑屏切场_架构溯源报告.md`  
**产品**：日常进屋用系统 **BlackPanel**；**LoadingPanel 仅时间跳转**（推翻 0831 进屋=读条）

---

## 沟通摘要

### ① 结论一句话

**自动进屋与手动 `House_Chief` 都改日常黑幕；续聊在换场全黑内 Trigger（F1′），Loading API 保留给时间跳转。**

### ② 原因（通俗）

以前对白结束进屋故意开蛋糕读条，手动门也勾了读条。  
产品改口：普通进屋跟别的门一样黑一下即可。  
改黑幕后若等淡出完再开续聊，可能先闪一眼室内——所以改成全黑里就把续聊开起来。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 门口三人戏结束 → 进屋：**只见黑屏**，不见蛋糕/粉条 | |
| 2 | 落点仍对（`EnterFrom_Village` / Walk 内） | |
| 3 | 自动续聊仍播；**无明显露景漏缝** | |
| 4 | 手动 E `House_Chief`：黑屏不读条；续聊门闩仍对 | |
| 5 | LeftDoor 出屋、楼梯上楼、送树屋：仍黑幕 | |
| 6 | 其它仍勾 `ShowLoadingUI` 的时间跳转门：读条未被误关 | |

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `ChiefNearDoorStoryTrigger.cs` | `LoadSceneWithLoadingPanel` → `LoadScene(Village_Chief_House)`；注释改口 |
| 2 | `Village_KenMuNi1.unity` | `House_Chief` Override `ShowLoadingUI` **1→0** |
| 3 | `Village_Chief_HouseSceneManager.cs` | **F1′** `TryDeferBlackFadeForCover` 全黑 Trigger 续聊；`OnEnterScene` 兜底防双开 |

**未改**：`LoadSceneWithLoadingPanel` API；其它场景已勾读条的门；EnterPos / WalkArea；对话 Prefab；楼梯与送树屋。

---

## ② 时序（落地后）

```
门口三人戏结束
  → ChiefNearDoor.OnStoryFinished
  → LoadScene(Village_Chief_House)          ← 日常黑幕
       → BlackPanel 全黑 → 卸场 / 加载
       → GSM Ready
            → TryDeferBlackFadeForCover
                 → 全黑 TriggerStory(继续对话)
                 → 壳就绪 → CloseFormFade → OnEnterScene（HasRunning 则跳过）
```

手动 `House_Chief`：`ShowLoadingUI=false` → 同一 `LoadScene` 黑幕路径；门闩仍按「门口已用 ∧ 续聊未用」。

---

## ③ 方案说明

| 方案 | 本期 |
|------|------|
| F1 自动进屋改黑幕 | ✅ |
| F2 手动门关读条 | ✅ |
| F1′ 全黑内续聊 | ✅ |
| F3 删 Loading API | ❌ 禁止 |
| F4 缩短假读条冒充黑屏 | ❌ |

---

## ④ OPEN

| ID | 项 | 状态 |
|----|----|------|
| 0831 进屋=Loading | 已改口 BlackPanel | ✅ 施工 |
| 0901 续聊靠 Loading 盖景 | 改 F1′ Black defer | ✅ 施工 |
| Q7 其它场景读条门清扫 | 本期不扫 | ⏳ 另案 |
