# ForestScene 门口 · M1 短黑幕掩护手推消闪 — 施工说明

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【施工员】在 M3 之上迭加报告方案 **M1**  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0912/ForestScene_保留相机移动_消闪与接话中断_架构溯源报告.md`  
**前置**：`施工说明/0912/ForestScene_保留相机移动_消闪并修复接话_施工说明.md`（M3 已保留移动+接话；验收仍闪）

---

## 沟通摘要

### ① 结论一句话

**林恩收束改为「短黑幕里平滑挪镜 → 揭幕再发门铃」：移动保留、滑动闪被挡住，雅儿仍应接话。**

### ② 原因（通俗）

镜头还是要慢慢挪回雅儿，但挪的过程露在画面上就会闪。  
先盖一层短黑幕，挪完再掀开，你看不见中间那一下乱晃。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 清 `homeDoorStoryComplete`，走完门口林恩链 | 收束时先短黑再亮；**无**可见大幅滑动闪 |
| 2 | 同上 | 亮屏后雅儿 **有** `YaerAfterLinEn` 接话 |
| 3 | Console 滤 `[CHAIN]` | 见 `M1 Open BlackPanel` → `SetFollow` → `手推到位 → CloseFormFade` → `FireNotify` |
| 4 | Inspector：`ForestSceneLinEnStory` | `useShortBlackCoverForCameraReturn=true`（默认开） |
| 5 | 若仍闪 | 查是否 Framing 绑 Follow 后二次跳（另案）；**禁止**再 `smoothTime=0` |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **M1** | `ForestSceneLinEnStory.cs` | `OnDialogueEnd` → FadeShow BlackPanel → 全黑后 `SetFollow`（保留 smoothTime）→ `onComplete` 揭幕 → 再 Ensure Notify |
| **字段** | 同 | `useShortBlackCoverForCameraReturn`（默认 true）；show/hide 默认 0.15 / 0.2s；安全兜底略调至 2.5s（从手推起算） |
| **保留** | 同 | M3 Notify 闸门 + B′ 只挡二次 snap |

### 未改

- Prefab 图 / Forest 场景 `smoothTime`
- 龙宫 Stairs
- 用瞬切消闪（禁止）

---

## 时序（M3+M1）

```
id40 OnDialogueEnd
  → Open BlackPanel FadeShow
  →（图继续）id41 Register CameraMoveEnd
  → 全黑 → SetFollow(smoothTime>0 手推，观众看不见)
  → onComplete → CloseFormFade
  → 揭幕完 → EnsureNotify（已 Register 则发）
  → id44 YaerAfterLinEn
```

**替代**：关 `useShortBlackCoverForCameraReturn` 回退纯 M3（移动可见、可能闪）。
