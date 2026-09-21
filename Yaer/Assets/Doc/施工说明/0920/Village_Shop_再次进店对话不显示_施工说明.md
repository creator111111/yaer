# Village_Shop · 再次进店对话不显示 — 施工说明

**文档版本**：v1.1（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_Shop_再次进店对话不显示_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**黑幕淡完之后，老板娘对话框才再淡出来。**

### ② 原因（通俗）

再进店程序有在说话，但以前对话框要么看不见（透明没拉回），要么和黑幕淡出叠在一起。现在先等黑屏完全褪掉，再短停一下，然后才把对话框淡进来。

### ③ 用户检查清单

同档：听完第一次开场（或确认已播过 Start），离店，再进店一次。

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 再进店看时序 | 先黑幕淡出露店景 → 再出「欢迎~」对话框（不要黑幕还在时框就露出来） |
| 2 | Console | 有 `TriggerStory Village_ShopRepeat`，随后有 `黑幕淡完，开闸允许对话框淡入` |
| 3 | 点完短招呼 | 商店 UI 正常出现 |
| 4 | 新档第一次进店 | ShopStart 仍正常（分层立绘时序不变） |
| 5 | 点头 / 点胸 / Yes·No | 仍正常 |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `Village_ShopRepeat.prefab` | v1.0：补对话框淡入；**v1.1**：淡入前加 `WaitShopStartBgRevealActionTask`（Hold 0.2s），连线 Wait→淡入→四句 |
| `Village_ShopSceneManager.cs` | Repeat Defer：`ResetForDeferredCover`；淡出前藏对话框；`onEndLoadingSceneEvent` 里 `SignalBgFullyVisible` |
| `ShopStartLayerRevealGate.cs` | 注释改为 Start/Repeat 共用亮屏闸门 |

**未改**：`DialogueTMPUGUI` / HideAll、ShopStart Prefab、Head / Chest / Yes / No。

---

## 为什么这样改（v1.1）

用户要的时序是「黑屏渐入渐出完成 → 再出对话框」。只补淡入不够：壳一就绪就关黑幕，图里立刻淡入，框会叠在黑幕上。

对齐首次进店：黑幕下 Trigger（防闪）→ 亮屏前 alpha=0 → CloseFormFade → 黑幕淡完开闸 → 图内 Wait → 对话框淡入 →「欢迎~」。

Hold 用 0.2（比 Start 的 0.4 短）：短招呼不需要分层立绘空拍那么长。

替代方案：黑幕淡完再 Trigger —— OnEnterScene 兜底可能抢先双开，未采用。
