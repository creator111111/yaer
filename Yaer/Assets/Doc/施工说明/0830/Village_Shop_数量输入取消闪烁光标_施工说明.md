# Village_Shop — 数量输入取消闪烁光标 — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_Shop_数量输入取消闪烁光标_架构溯源报告.md`  
**范围**：方案 A — Helper 关死 caret + Bind 已有引用也 Apply；**未**改 Prefab YAML（P1）、未动鼠标 Catch / 对话。

---

## ① 结论一句话

聚焦数量框时不再画闪烁竖线；键盘输入与 DigitStrip / Total2 逻辑不变。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `ShopQuantityInputHelper.cs` | `ApplyInvisibleInputTextStyle` 公开；补 `caretWidth=0`、`caretBlinkRate=0`、selection alpha=0 | 仅颜色透明挡不住网格闪烁 |
| `ShopBuyRowQuantityInput.cs` | `BindQuantityInput` 已有 `quantityInput` 也 Apply | 堵 Prefab 预绑早退漏刷 |

**未做（P1）**：ShopPanel / Village_Shop 逐行序列化同步；Play 时 Ensure/Apply 覆盖即可。

**替代（未采用）**：手改 11 行 YAML；拆 InputField 改加减钮。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进店 → 点数量框聚焦 | **无**闪烁竖线 |
| 2 | 键盘输入 1～99 | DigitStrip / Total2 正常；可点「决定」 |
| 3 | 买 / 卖 Tab 各一行 | 都不闪 |
| 4 | 失焦再聚焦 | 仍不闪 |
| 5 | 回归 | 能点到框、能输入、数字仍是图片非 TMP 字 |

---

## ④ 给程序

- 主挂点仅 Helper；运行时覆盖磁盘旧 width/blink。
- 本期无关：Head Catch 鼠标、ShopYes/No 对话。
