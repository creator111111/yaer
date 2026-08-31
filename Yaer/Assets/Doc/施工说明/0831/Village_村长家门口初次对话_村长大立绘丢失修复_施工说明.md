# Village_村长家门口初次对话 — 村长大立绘丢失修复 — 施工说明

**日期**：2026-08-31  
**依据**：`执行文档/0831/Village_村长家门口初次对话_村长大立绘丢失修复_施工执行说明.md`  
**性质**：H1 母体空 Sprite 已写回；Setup 加空图中止保护

---

## ① 结论

**H1 已修**：`ChiefPainting.prefab` Face1/2/3 已绑回与 Mask 同源 guid（组 2 / 闭眼 / 笑颜），SizeDelta 对齐。  
门口 Prefab 为 PrefabInstance、无 Sprite Override → **继承母体即有图**。  
Setup 现若缺 png **拒绝 Save**，避免再次空盘。

---

## ② 原因

缺图时曾跑 Setup，空 Sprite 落盘；png 后补但 Prefab 未重绑。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 打开 `ChiefPainting`：三脸 Sprite 非空 | |
| 2 | 门口 Prefab 内村长立绘可见 | |
| 3 | Play 村句：大立绘 + Face1/2/3 | |
| 4 | Mask / 雅 / 古 | 回归 |

不必强制再跑 Setup；若编辑器仍显示旧空图，Focus/重开 Prefab 或再跑 `Setup Chief Painting`。

---

## ④ 程序清单

| 路径 | 变更 |
|------|------|
| `ChiefPainting.prefab` | 三脸 `m_Sprite` + SizeDelta 写回 Mask 同源 guid |
| `ChiefPaintingSetupEditor.cs` | 三脸未齐则 Error 并 `return null`，禁止空图 Save |

**png**：磁盘已齐；**勿删 meta**。
