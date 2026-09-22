# Village_HomeScene1 · 墙上画彩蛋 20% — 施工说明

**文档版本**：v1.0（2026-09-22）  
**文档性质**：【施工员】方案 A  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0922/Village_HomeScene1_墙上画彩蛋20_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**进 HomeScene1 看墙：只有正常画或彩蛋画之一；约两成出彩蛋，其余正常。**

### ② 原因（通俗）

墙上两幅图以前都开着会叠在一起。现在进门掷一次骰：20% 换彩蛋，80% 仍平常那幅。

### ③ 用户检查清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 进 HomeScene1 看墙 | 只亮一张，无叠影 |
| 2 | 反复进出 ≥20 次（或临时 chance=0.5） | 彩蛋会出现；多数正常 |
| 3 | 临时 `easterChance=1` | 必彩蛋 |
| 4 | 临时 `easterChance=0` | 必正常 |
| 5 | 画框 / 其它家具 | 不动 |
| 6 | 交回 `0.2` | 恢复产品概率 |
| 7 | 其它民居 | 墙上画逻辑无变化 |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `…/CommonEntity/RandomExclusiveChildActive.cs` | 新建：OnEnable 掷骰互斥 SetActive |
| `Village_HomeScene1.unity` | 「画」挂组件；绑彩蛋画/正常画；`easterChance=0.2`；彩蛋默认 Active=0 |

**未改**：SceneManager、画框、ArtRes Prefab、其它 HomeScene、存档。

---

## 为什么这样改

场景自包含、可复用；每次进场景重掷对齐经典彩蛋，本期不做 Archive。
