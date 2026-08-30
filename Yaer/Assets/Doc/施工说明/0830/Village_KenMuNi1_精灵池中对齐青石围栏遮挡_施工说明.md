# Village_KenMuNi1 — 精灵池中对齐青石围栏遮挡 — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md`  
**范围**：仅场景挂 `VillageSceneObjectDepthSort`；**不改** C#；**不改** `Collider (1)`；**不做** `精灵池上`。

---

## ① 结论一句话

`精灵池中` 已挂与青石围栏同款 DepthSort，会按玩家世界 Y 在 Default↔SceneObject 间换层。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Village_KenMuNi1.unity`（`精灵池中`） | Add `VillageSceneObjectDepthSort` | 原先钉死 SceneObject，不会随前后换挡 |

**Inspector 初值（抄围栏）**

| 字段 | 值 |
|------|-----|
| `targetSpriteRenderers` | 本物体 SR `8162121874168452420` |
| `anchorOverride` | 本 Transform `244632720003562929` |
| `invert` | 0 |
| Default / SceneObject Order | **6 / 0** |
| `preferTownLocomotionAuthoritativeY` | 1 |

**未改**：`VillageSceneObjectDepthSort.cs`；子物体 `Collider (1)` 物理；`精灵池上`；`青石围栏`。

**调参顺序（实机不对）**：① invert → ② 锚点改 `Collider (1)` → ③ 微调 Order。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 站池「后方」（玩家 Y 更大） | 池子 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 池子 |
| 3 | A/D、W/S、斜向 | 无闪烁卡死 |
| 4 | 运行时 SR | sortingLayer **Default ↔ SceneObject** |
| 5 | 青石围栏 | 仍正常 |
| 6 | Console | 无 DepthComponent 双开警告 |

前后整段反了：勾 `invert`。

---

## ④ 给程序

- diff 仅场景组件序列化。  
- `精灵池上` 穿帮再同挂（P1）。
