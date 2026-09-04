# Village_HomeScene4 → Village_HomeScene23 改名影响面 — 架构溯源报告

**文档版本**：v1.1（2026-08-04）  
**文档性质**：【架构侦探】盘点 + **【施工已完成】**  
**目标**：曾用名 `Village_HomeScene4` **全量**改为 **`Village_HomeScene23`**（≠ HomeScene2/3）  
**依据**：提示词 `0804/Village_HomeScene4改名Village_HomeScene23_架构侦探提示词.md`  

**Unity**：2020.3.48f1  

> **施工结果（2026-08-04）**：运行时三位一体已对齐为 `Village_HomeScene23`；文档按 OPEN 默认建议同轮改名。旧存档 `LastSceneName=Village_HomeScene4` **不兼容**（可接受）。下文保留施工前影响面，供审计。

---

## ① 结论一句话

**可以改，且必须「字符串三位一体」一次对齐：场景文件名 = `SceneName` 常量 = `nowSceneName` / 村门 `NextSceneName` / 回程 `EnterPos.lastScene` / Build path。运行时必改面小（约 5～6 个资产 + 2 个脚本），文档面大；最大风险是旧存档里残留 `LastSceneName=Village_HomeScene4` 回村对不上表。新名 `Village_HomeScene23` 与现有 `HomeScene2`/`HomeScene3` 无冲突。**

---

## ② 原因（生活类比）

场景名是换场「身份证」：门牌（NextSceneName）、户口本（SceneName/nowSceneName）、回村路标（EnterPos.lastScene）、派出所登记（Build）必须同一张。只改文件名不改门牌，人进不去；只改门牌不改文件，派出所说没这地方。

`SceneAssetPath.GetSceneAssetPath(name)` → `Assets/GameRes/Scenes/{name}.unity`，**无别名表**。

---

## ③ 拍板（已按默认施工）

| ID | 决议 |
|----|------|
| Q1 文档同轮改 | **是** |
| Q2 旧档兼容 | **不兼容可接受**（未双写 EnterPos） |
| Q3 新名 | **`Village_HomeScene23`** |

### 验收

1. rg 运行时路径无 `Village_HomeScene4`（Scripts/GameRes/ProjectSettings）  
2. Build 含 `Village_HomeScene23.unity`  
3. 村 `House_Npc4` → 进 `Village_HomeScene23` → 出门回村落点正确  
4. Console 无「场景未找到」；日志 `[VillageHomeScene23Debug]`  
5. **误伤检查**：`HomeScene1Npc4`、`House_Npc4`、`Village_HomeScene2` 仍在  

---

## ④ 运行时契约（现网）

```
LoadScene("Village_HomeScene23")
  == 文件 Village_HomeScene23.unity
  == SceneName.Village_HomeScene23
  == Village_HomeScene23SceneManager.nowSceneName
  == House_Npc4.NextSceneName
  == 村 EnterPos.lastScene（回程）
  == EditorBuildSettings path
```

`House_Npc4` / `Npc4` / `HomeScene1Npc4` **未改**。

### meta GUID（施工保留）

| 资产 | GUID |
|------|------|
| 场景 .unity.meta | `6fe5ae5c3282b3e4581d45ff25b782b3` |
| Manager .cs.meta | `a1b2c3d4e5f6478091a2b3c4d5e6f708` |
| Config .asset.meta | `b8c9d0e1f2a3445566778899aabbccdd` |

---

## ⑤ 误伤黑名单（应仍在）

| 保留 | 原因 |
|------|------|
| `HomeScene1Npc4` | 对话 Prefab |
| `House_Npc4`、`Npc4` | 门/NPC 物体名 |
| `Village_HomeScene2`、`Village_HomeScene3.unity` | 其它户 |
| `Village_House4`、`HomeScene1`/`HomeScene2` | 其它场景 |
