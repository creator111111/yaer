# ForestEast · 爬出树洞无对白 + Fall 空引用 — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】方案 **A（补绑遮罩）+ B（Fall/CheckFall null 守卫）**  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/ForestEast_爬出树洞无对白_Fall空引用_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**倒树倒下时挂件列表里空的那一格已绑回遮罩；代码遇到空引用会跳过，Pass 对白可以播完。**

### ② 原因（通俗）

过树洞的剧本一喊倒下，程序要关掉一串挂件，其中有一格是空的，直接报错卡死，后面「好险…」等台词全停。合层换图时旧遮罩对象换成了新的「遮罩只影响人物」，列表没重绑。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | `Objects/倒树` → TreeBridgeLogic → Attached | 第 5 项（下标 4）= **`遮罩只影响人物`**，无 None |
| 2 | 新档走到倒树右侧约 x=342 | 倒下演出；**无** `AttachedGameObject has not been assigned` |
| 3 | 对白 | 「好险，差点就一起掉下去了」等播完 |
| 4 | 倒下后画面 | 遮罩已关，不挡人 |
| 5 | 读档已倒下 | 无 CheckFall 空引用 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A 补绑** | `ForestEastScene.unity` `TreeBridgeLogic.AttachedGameObject[4]` | `{fileID: 0}` → `1773642872123289953`（遮罩只影响人物） |
| **B 守卫** | `TreeBridgeLogic.Fall` / `CheckFall` | `go == null` 则 `continue`；可选 `[TreeBridgeFall]` 警告 |

### 未改

- Pass 对白文案 / Prefab 图序  
- `CanNotSomeActionArea`  
- 死羊、贴底相机、倒树五层 Sprite（已合层）  

---

## OPEN 施工落地

| ID | 默认 | 状态 |
|----|------|------|
| Q1 补绑新遮罩 | 是 | ✅ |
| Q2 null 守卫 | 是 | ✅ |
| Q3 合层未重绑 | 已坐实 | ✅ |
| Q4 CheckFall 一并护 | 是 | ✅ |

---

## 剩余风险

- 若场景在编辑器里开着被本地覆盖，确认 Attached[4] 仍指向遮罩。  
- SingleUse 已消耗的档再走 Pass 无词是设计，不是本票。  
- 本机未 Play。
