# 旧角色图集小头像 — Mask 混合回退 — 施工说明

**文档版本**：v1.0（2026-09-11）  
**文档性质**：【施工员】按侦探报告方案 **A′** 最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0911/旧角色图集小头像全空_Mask模式回退_架构溯源报告.md`  
**根因**：H1 — `useMaskAvatar` 永久关 `actorPortrait`，Presenter 又不支持 King/Lai/Xiaer/LinEn → 双空  
**小孩钉名**：**夏尔 Xiaer**（不是哥布林）

---

## 沟通摘要

### ① 结论一句话

**保持全局 Mask 开着；仅父亲/莱/夏尔/将军四人在有图集 sprite 时回亮旧 Portrait；雅儿等仍走 Mask；哥布林继续空。**

### ② 原因（通俗）

新路 Mask 只做了部分角色；旧图集槽又被一刀切关死，没做 Mask 的人就两边都没脸。  
不是图丢了，是两扇门都关了。现在给四人开回旧门，雅儿那扇新门照旧。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 父亲（King）/ 莱 / **夏尔** / 将军（LinEn）句 | 左槽有旧图集头像，脸跟 FaceType |
| 2 | Hierarchy `actorPortrait` | 四人句 Active=true 且有 sprite |
| 3 | 雅儿 / 古莎等 Mask 句 | 仍 Mask；Portrait **关**（无双影） |
| 4 | 店 / 村长旗 | 不黑块；Portrait 关 |
| 5 | 哥布林句 | 空着 **不算失败** |
| 6 | 历史列表 | 头像仍正常 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **A′** | `DialogueTMPUGUI.cs` | Mask 模式下白名单四人可亮旧 Portrait；异步防串脸 |

### 行为细则

| 句类型 | Portrait | Mask |
|--------|----------|------|
| King / Lai / Xiaer / LinEn + sprite≠null | **亮** | HideAll 后空（原样） |
| Yaer / Gusha / Amy / Aliy / Chief / 店 | **关**（Mask 句立刻关） | Apply 有脸 |
| Goblin* / 其它未点名 | **关** | 空 |
| 旁白 None | **关** | HideAll（店/村长旗跳过逻辑保留） |

### 互斥与防串

1. **真源只在 `OnGetAvatar`**：不改 Presenter.Resolve；避免两处抢 Active。  
2. `_subtitleAvatarRole`：开句写入；旁白/店/村长写 None；回调 Role 不一致则丢弃。  
3. 切到非白名单（Mask 角色）：开句**本帧** `SetActive(false)`，不等 Loader。

### 白名单（硬编码）

`King`, `Lai`, `Xiaer`, `LinEn` — **不含** GoblinElder/Younger。

---

## 未改（禁止项）

- 全局 `useMaskAvatar=false`（禁 C）
- 四人 Mask Painting / 扩 Resolve（禁 B）
- 图集名 / 台本 / Prefab 壳
- 哥布林专项补头像

---

## 文档债

0804「旧 Portrait 完全关闭」→ 修订为 **混合**：Mask 角色关；白名单旧角色可亮（OPEN 已决议）。

---

## 剩余风险

| 风险 | 说明 |
|------|------|
| 白名单句 Loader 失败 sprite=null | 仍空；先查图集/路径，勿扩 A |
| CSV/新 Role | 本期不扩；要亮再开 OPEN |
| 双影 | Mask 句已立刻关槽；若仍叠查时序 |

---

## 验收对照

- [ ] 父/莱/夏尔/将军有旧图集脸  
- [ ] 雅儿 Mask 无 Portrait 双影  
- [ ] 哥布林空不判失败  
- [ ] 历史列表正常  
