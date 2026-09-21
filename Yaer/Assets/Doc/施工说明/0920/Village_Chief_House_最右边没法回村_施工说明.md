# Village_Chief_House · 最右边没法回村 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_Chief_House_最右边没法回村_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**走到村长家最右边，会黑幕回村门前。**

### ② 原因（通俗）

右边那扇门以前按旧决议关掉了，目标还写着东郊。回村门装在左边。现在把右门打开，改成进肯姆尼村，和左门落同一处门前。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Hierarchy：`RightDoor` | 勾已开；`NextSceneName=Village_KenMuNi1`；`EnterPosKey=Village_Chief_House_Door`；`ShowLoadingUI` 关；`TriggerWhenMoveIn` 开 |
| 2 | 走到最右（约 x=26） | 走进即切场；Console 有 `[SceneChangeDoor]`；**不要**进东郊 |
| 3 | 落点 | 村侧 `ExitFrom_HomeSceneChief`（-156.5, -5.5） |
| 4 | 左边 `LeftDoor` 按 E | 仍可回村（本期保留双出口） |
| 5 | 上楼楼梯门 | 仍正常 |

### ④ 程序补充

见下文。

---

## 改动清单

只改 `Assets/GameRes/Scenes/Village_Chief_House.unity` 的 `RightDoor`：

| 字段 | 现网 | 改为 |
|------|------|------|
| `m_IsActive` | 0 | **1** |
| `NextSceneName` | ForestEastScene | **Village_KenMuNi1** |
| `EnterPosKey` | 空 | **Village_Chief_House_Door** |
| `ShowLoadingUI` | 1 | **0** |
| `TriggerWhenMoveIn` | 1 | **1**（保持） |

Interactive / componentsList 未拆。`LeftDoor`、村侧落点坐标、`VillageWalkArea`、上楼门未改。

---

## 为什么这样改

原样启用会进东郊。与左门共用 `Village_Chief_House_Door`，仍落门前，送树屋戏（G1）两边都会认——默认不拆键。左门先保留，避免验收当周只剩一扇门。

否决：只告诉玩家走左边；只开 Active 不改 Next。
