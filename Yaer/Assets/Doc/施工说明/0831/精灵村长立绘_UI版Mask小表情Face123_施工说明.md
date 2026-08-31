# 精灵村长立绘 — UI 版 Mask 小表情 Face123 — 施工说明

**日期**：2026-08-31  
**角色**：施工员  
**溯源**：`Assets/Doc/执行文档/0831/精灵村长立绘_UI版Mask小表情Face123_架构溯源报告.md`

---

## ① 结论一句话

新建 `ChiefMaskPainting`（Face1 底 + Face2/3 互斥贴脸），扩 `DialogueRoleName.Chief`，晚宴 Leader→Chief；Presenter F2 映射 Smile/CloseEyes。

---

## ② 改了什么

| 文件 | 变更 |
|------|------|
| `ChiefMaskPainting.cs` + `ChiefFaceType` | Apply 叠法；无 Start Reset |
| `ChiefMaskPainting.prefab` | UI Image 绑组2/闭眼/笑颜 |
| `ChiefMaskPaintingSetupEditor.cs` | 菜单可重建嵌套（备用） |
| `RoleName.cs` | 末尾追加 `Chief=11` |
| `DialogueMaskAvatarPresenter.cs` | HideAll + case Chief + MapToChiefFace |
| `NormalDialogueNewPanel.prefab` | 嵌 ChiefMaskPainting，默认关 |
| `Village_Leader…GuShaAmyAliy.prefab` | Leader `_roleName=11`（Chief） |

**未改**：SR `精灵村长游戏中立绘`；全局 `DialogueFaceType`；晚宴 CSV；商人。

**F2 默认映射**：Normal/空/Sad/Laugh→Face1；CloseEyes→Face2；Smile→Face3。

---

## ③ 验收

- [ ] 村长句 Mask 可见立绘  
- [ ] Face1 默认；Face2 闭眼；Face3 笑颜  
- [ ] Face2/3 都关仍有 Face1；不双贴脸  
- [ ] 雅/古句关村长；商人回归 OK  
- [ ] Leader 不再 None 空窗  

摆位可在 Panel 上微调 AnchoredPosition / Scale。若贴脸偏移不对：跑 `Tools/Dialogue/Setup Chief Mask Painting` 重建，或手调 Face2/3 坐标。

---

## ④ 开放

| ID | 状态 |
|----|------|
| Q1 叠法肉眼 | ⏳ 验收可改互斥三选一 |
| Q3/Q4 Smile/Sad 映射 | ⏳ 产品确认 |
