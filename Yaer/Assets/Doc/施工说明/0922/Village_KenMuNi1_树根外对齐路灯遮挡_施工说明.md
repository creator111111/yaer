# Village_KenMuNi1 — 树根外对齐路灯遮挡 — 施工说明

**日期**：2026-09-22  
**角色**：施工员  
**依据**：`执行文档/0922/Village_KenMuNi1_树根外对齐路灯遮挡_架构溯源报告.md`  
**范围**：仅场景挂 `VillageSceneObjectDepthSort`；**不改** C#；**不改** Prefab 源；**不做** `树根内`。

---

## ① 结论一句话

`树根外` 已挂与 `精灵池路灯` 同款 DepthSort，绕根走会按前后在 Default↔SceneObject 换层。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Village_KenMuNi1.unity`（`树根外`） | Add `VillageSceneObjectDepthSort`（fileID `8828300003`） | 原先钉死 SceneObject/30，不会随前后换挡 |

**Inspector 初值（整表抄路灯）**

| 字段 | 值 |
|------|-----|
| 挂点 | `树根外` 根 GO `7070296266979326876` |
| `targetSpriteRenderers` | 本 SR `8775965915123942743` |
| `anchorOverride` | 本 Transform `4137765881751695108` |
| `invert` | 0 |
| Default / SceneObject Order | **9 / 3** |
| `preferTownLocomotionAuthoritativeY` | 1 |

**未改**：`VillageSceneObjectDepthSort.cs`；`肯姆尼3合层.prefab`；路灯 / 池中 / 围栏；`树根内`。

**调参顺序（实机不对）**：① invert → ② 锚点改根脚空物体 → ③ 抬 SceneObject Order（向原 30 靠拢，勿抄围栏 6/0）。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 站树根外「后方」 | 根 **挡住** 玩家 |
| 2 | 走到「前方」 | 玩家 **盖住** 根 |
| 3 | A/D、W/S、斜向绕根 | 无闪烁卡死 |
| 4 | 运行时树根外 SR | sortingLayer **Default ↔ SceneObject** |
| 5 | 再绕路灯 / 池中 | 金样仍正常 |
| 6 | Console | 无 DepthComponent 双开 Warning |

与 3 合层静图穿帮：优先抬 `sortingOrderWhenSceneObjectLayer`。

---

## ④ 给程序

- diff 仅场景组件序列化。  
- `树根内` / 合层 Prefab 同步另票。
