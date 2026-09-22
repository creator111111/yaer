# Village_KenMuNi1 — 精灵池路灯对齐池中遮挡 — 施工说明

**日期**：2026-09-22  
**角色**：施工员  
**依据**：`执行文档/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构溯源报告.md`  
**范围**：仅场景挂 `VillageSceneObjectDepthSort`；**不改** C#；**不改** Prefab 源；**不挂** `路灯蘑菇`；**不做** `精灵池上`。

---

## ① 结论一句话

`精灵池路灯` 已挂与 `精灵池中` 同款 DepthSort，绕灯走会按前后在 Default↔SceneObject 换层。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Village_KenMuNi1.unity`（`精灵池路灯`） | Add `VillageSceneObjectDepthSort`（fileID `8828300002`） | 原先钉死 SceneObject，不会随前后换挡 |

**Inspector 初值（整表抄池中）**

| 字段 | 值 |
|------|-----|
| 挂点 | `精灵池路灯` 根 GO `1222846987` |
| `targetSpriteRenderers` | 本 SR `1222846989`（仅 1 片） |
| `anchorOverride` | 本 Transform `1222846988` |
| `invert` | 0 |
| Default / SceneObject Order | **6 / 0** |
| `preferTownLocomotionAuthoritativeY` | 1 |
| `updateEveryNthFrame` | 1 |
| `debugLogOnLayerChange` | 0 |

**未改**：`VillageSceneObjectDepthSort.cs`；`肯姆尼2合层.prefab`；`精灵池中` / 青石围栏；`精灵池上`；`路灯蘑菇`。

**调参顺序（实机不对）**：① invert → ② 锚点改灯脚空物体 → ③ 微调 Order。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 站路灯「后方」（玩家 Y 更大） | 灯 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 灯 |
| 3 | A/D、W/S、斜向绕灯 | 无闪烁卡死 |
| 4 | 运行时路灯 SR | sortingLayer **Default ↔ SceneObject** |
| 5 | 再绕 `精灵池中`、青石围栏 | 金样仍正常 |
| 6 | Console | 无 DepthComponent 双开 Warning |

前后整段反了：勾 `invert`。切换偏：灯脚空锚点。

---

## ④ 给程序

- diff 仅场景组件序列化。  
- `精灵池上` / `路灯蘑菇` / 合层 Prefab 同步另票。
