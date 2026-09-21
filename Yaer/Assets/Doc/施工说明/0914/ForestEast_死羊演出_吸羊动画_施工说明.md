# ForestEast 死羊演出 · 吸羊动画 — 施工说明

**文档版本**：v1.1（2026-09-14）  
**文档性质**：【施工员】按侦探报告方案 **Key + Start 闸门**（用户反馈 Doc 方案未修好）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/ForestEast_死羊演出被改_吸羊动画没了_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**已修：走廊 Mgr2 改独立存档键；东郊只有本场吃羊剧情播过才关 Objects 吸羊循环，否则强制打开。**

### ② 原因（通俗）

`Part3/死羊` 一直是静图；两套循环在 `Objects` 下。v1.0 只写说明不改代码。真因是走廊 Mgr2 和东郊 Mgr1 **共用同一对存档键**，走廊打完再进东郊，`Start` 当成「东郊已开战」把循环关掉。本期拆键 + 按 `ForestEastSceneSlimeEatSheep` 是否已播做闸门。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | **未播过东郊吃羊**的档（含先打走廊再进东郊）走到 X≈175 | 两套循环可见、Animator 在转 |
| 2 | Hierarchy `Objects` | `1史莱姆吸动画1`、`2史莱姆吸动画3` Active |
| 3 | `Part3/死羊` | **仍无 Animator**（不要加） |
| 4 | **已播东郊吃羊/立坟**的档 | 无循环、尸体或坟 = 设计 |
| 5 | 新档走进东郊 Trigger | 对白后关循环、刷战斗怪 = 正常 |
| 6 | 走廊吃羊进度 | 仍按走廊自己的 Story2 键恢复，不误伤东郊 |

### ④ 程序补充

见下文。

---

## 改动清单

| 文件 | 改动 |
|------|------|
| `SlimeEatSheepStoryMgr2.cs` | 存档键改为 `SlimeEatSheepStory2_hasTriggerSlime` / `_hasCreateGrave`；缺键且走廊剧情已播才从旧共用键迁入 |
| `SlimeEatSheepStroy.cs` | `Start`：仅 `CheckStoryUsed(ForestEastSceneSlimeEatSheep)` 为真才坟/开战关动画；否则 `EnsureSlimeEatAniVisible` |
| `SimpleStoryTrigger.cs` | `VerdantCorridorSlimeEatSheep` → `Mgr2.InitBattleData` |
| `Part3/死羊`、吸羊 Prefab、场景贴图 | **未改** |

### 存档键（现网）

| 场景 | Trigger 键 | CreateGrave 键 |
|------|------------|----------------|
| 东郊 Mgr1 | `SlimeEatSheepStory_hasTriggerSlime` | `SlimeEatSheepStory_hasCreateGrave` |
| 走廊 Mgr2 | `SlimeEatSheepStory2_hasTriggerSlime` | `SlimeEatSheepStory2_hasCreateGrave` |

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 未播东郊能否看见循环 | **是**（闸门 + 强制 ON）；请 Play 验收 |
| Q2 已播东郊关动画 | **设计**，闸门后仍关 |
| Q3 走廊/东郊串档 | **已拆键 + 迁入带走廊剧情判定** |
| Q4 运镜手感 | **另开票** |
| Q5 改 Part3 静图 | **否** |

---

## 替代方案（未采用）

- **仅 Doc**：已证不够（用户反馈未修好）。  
- **给尸体加 Animator**：毁金标准分层，禁止。  
- **盲迁旧键到 Story2**：会把东郊进度当成走廊已开战；现改为「走廊剧情已播才迁」。  
- **回滚整份 ForestEastScene**：会丢掉倒树等无关改动。

---

## 剩余风险

- 极老档：走廊已打完但从未写入 `VerdantCorridorSlimeEatSheep` 计数 → 不会迁入，走廊可能要重触发一次（少见）。  
- 本机未 Play；请按清单 1、6 验收。
