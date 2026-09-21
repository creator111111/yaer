# ForestEast 倒树遮罩只影响主角 — 施工说明

**文档版本**：v1.2（2026-09-14）  
**文档性质**：【施工员】**产品改口**：要的是环境阴影压暗，不是裁切不可见  
**Unity**：2020.3.48f1  
**权威机制**：`Assets/Doc/技术文档/EnvironmentShadowCamera与LayerEnvironmentShadow用法.md`

---

## 沟通摘要

### ① 结论一句话

**已撤回 SpriteMask 裁切；遮罩改挂 `EnvironmentShadow` 层，只压暗带环境阴影材质的人/虫/卵，其它图不受影响、也不会被裁没。**

### ② 原因（通俗）

你要的是「阴影罩在人、虫、卵身上变暗」，不是把身体裁成看不见。工程里本来就有 EnvironmentShadow 相机：阴影图只进专用层，角色材质按屏幕采样压暗。先前误用了 SpriteMask，才会变成看不见。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 进倒树洞口区域 | 人/虫/卵 **仍可见**，在阴影区会 **变暗** |
| 2 | 同屏树干/装饰/其它贴图 | **不被**这张阴影图盖住染色 |
| 3 | Hierarchy 遮罩物体 | Layer = **EnvironmentShadow**；有 SpriteRenderer；**无** SpriteMask |
| 4 | Pass 倒下 | Attached 仍关遮罩 → 压暗消失 |
| 5 | 「外」靠近淡出 | 仍在 |

### ④ 程序补充

见下文。

---

## 改动清单（v1.2）

| 项 | 处理 |
|----|------|
| `遮罩只影响人物` | **Layer → 12 EnvironmentShadow**；SR 重新启用；**删除 SpriteMask** |
| Player / WoodWorm MaskInteraction | **全部恢复 None（0）** |
| WormEggType1/2/3 | Animation 改用 `Sprites_PlayerWithEnvironmentShadow` + 挂 `CharacterEnvironmentShadow` |
| WoodWorm / Player | 原本已有环境阴影材质/组件，仅撤回 Outside |
| 主相机 | 本就不渲染 Layer12（无需改） |
| Fall Attached 绑定 | **未改** |

### 数据流（现网正确语义）

```
遮罩 Sprite (Layer EnvironmentShadow)
  → EnvironmentShadowCamera → RT
  → CharacterEnvironmentShadow 写入 _ShadowTex
  → Shader 用 alpha 压暗角色 RGB（人仍可见）
```

未挂该材质的贴图 → 不受影响。

---

## 相对 v1.0/v1.1

| 版本 | 错误/状态 |
|------|-----------|
| v1.0 | SpriteMask + Outside → 裁成不可见（产品否） |
| v1.1 | 卵虫也 Outside → 仍是裁切 |
| **v1.2** | 改走 EnvironmentShadow 压暗 |

---

## 剩余风险

- `_ShadowScale` 过强/过弱可在材质上微调。  
- 本机未 Play；须肉眼确认「变暗」而非「消失」。
