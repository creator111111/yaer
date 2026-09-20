# SystemTipsPanel2 · Missing Script — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】按侦探报告方案 **A**（删孤儿 YAML + 补 ImageContent 绑槽）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/SystemTipsPanel2_MissingScript_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**黄叹号对应的假脚本已从 Prefab 删掉；并按 Panel1 补了 `ImageContent`，BOSS 保存提示可以正常出图、不再空引用。**

### ② 原因（通俗）

根上多挂了一段指向仓库里不存在的 GUID，Unity 只能画黄叹号。真正干活的三个脚本还在。提示文案图要画在 `ImageContent` 上，Panel2 原先缺这个节点，一开面板就会空引用。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 打开 `SystemTipsPanel2` Prefab | **无** Missing / 黄叹号；可 Apply |
| 2 | 根组件 vs Panel1 | 仅 FormLogic / ComponentSystemUI / CursorChangeUI |
| 3 | Hierarchy | 有 `ImageContent`；不要把禁用全屏 `Image` 当文案槽 |
| 4 | FormLogic → Img Tips Content | 指向 `ImageContent`（非 None） |
| 5 | Play：BOSS 前保存提示 | 弹出、有 SaveChar 文案图、确认/取消正常 |
| 6 | Console | 无 `UpdateInfo` NRE |

换 Save 文案图仍走 `ArtRes/.../SaveChar.png` → Pack `tipsChar*`，**不是**拖 Prefab 槽。

### ④ 程序补充

见下文。

---

## 改动清单

只改 `Assets/GameRes/Prefabs/UI/SystemTipsPanel2.prefab`。

| 项 | 说明 |
|----|------|
| **删孤儿** | 整段 `!u!114 &5728391048567291034`（GUID `8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6`） |
| **补节点** | 根下 `ImageContent`（Rect / CanvasRenderer / Image），布局对齐 Panel1：`(156.4, -33)` / `731×344`，RootOrder=1 |
| **绑槽** | `imgTipsContent` → Image `208533821441395807` |

### 未改

- FormLogic / ComponentSystemUI / CursorChangeUI 脚本  
- `SystemTipsPanel`（v1）  
- 对话树 / ActionTask  
- `tipsChar*` 图集与 `SaveChar.png`  
- 禁用全屏 `Image`（遮罩，未当内容槽）

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 是否同期补 ImageContent | **是**（方案 A；不只消黄） |
| Q2 禁用全屏 Image 当内容槽 | **否** |
| Q3 新建假 GUID 脚本消黄 | **否** |
| Q4 用 Panel1 整份覆盖 Panel2 | **否** |

---

## 替代方案（未采用）

- **B** 只删 Missing：编辑器能存，运行时 Save 仍可能 NRE  
- **C** 补假脚本：无业务、与 FormLogic 重复  
- **D** Panel1 覆盖 Panel2：超出范围  

---

## 剩余风险

- ImageContent 布局按 Panel1 抄；若 Panel2 框位置有手调差异，Scene 可微移 AnchoredPos。  
- 占位 Sprite 与运行时 `SaveChar` 图集可能不同，以 Play 图集为准。  
- 本机未 Play BOSS 弹窗；请按清单验收。
