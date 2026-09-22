# Village_HomeScene1 · 墙上画彩蛋 20% · 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（**只读**，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 场景：`Village_HomeScene1`  
> 产品钉死：彩蛋画 **20%** / 正常画 **80%**；**互斥**只亮一张；仅本场景这组「画」  
> 提示词：`Assets/Doc/提示词/0922/Village_HomeScene1_墙上画彩蛋20_架构侦探提示词.md`

---

## ① 结论一句话

**挂点钉死为场景内 `Map / Design / 村民家1合层 / 画`；现网「彩蛋画」「正常画」均为 Active=1 → 进门会双显叠图。**  
推荐 **方案 A**：在「画」根挂小组件（如 `RandomExclusiveChildActive`），引用两子物体 + `easterChance=0.2`，**OnEnable 每次进场景重掷**一次互斥显隐；缺引用则 Error 并只开正常画。不写存档；不改「画框」（同层兄弟）；不改其它民居；**勿**只改 `ArtRes/.../村民家1合层.prefab`（旧资产无彩蛋对，且场景未引用该 guid）。

---

## ② Hierarchy / 现网双显风险

### 路径（场景 YAML 已核实）

```
Map
 └─ Design
     └─ 村民家1合层
         ├─ 画                    ← Transform only；Active=1
         │   ├─ 彩蛋画            ← Transform + SpriteRenderer；Active=1
         │   └─ 正常画            ← Transform + SpriteRenderer；Active=1
         └─ 画框                  ← 同级兄弟；Transform + SpriteRenderer；Active=1
```

| 节点 | 组件 | `m_IsActive` |
|------|------|--------------|
| 画 | Transform | **1** |
| 彩蛋画 | Transform + SpriteRenderer | **1** |
| 正常画 | Transform + SpriteRenderer | **1** |
| 画框 | Transform + SpriteRenderer | **1**（独立，勿误关） |

**双显风险：成立。** 两张画同时 Active → 叠图；施工必须互斥，不能靠 SortingOrder 盖。

### 资产注意

| 项 | 事实 |
|----|------|
| 场景内合层 | 已展开在 `Village_HomeScene1.unity`（含彩蛋画/正常画） |
| `Assets/ArtRes/Scene/Village/Prefab/村民家1合层.prefab` | 仅有「画1」等旧节点；**无**彩蛋/正常对；guid **不在**本场景引用列表 |
| 施工真源 | **改场景内「画」节点**（挂脚本 + 绑引用）；不要当「改 Prefab 就能驱动场景」 |

### 场景管理器

`Village_HomeScene1SceneManager.OnEnterScene` 现仅设 Place / 打日志，**无**画相关逻辑。可挂方案 B，但推荐场景自包含方案 A。

### 同类先例

未扫到村屋「双 Sprite 概率互斥」金样；有 `RandomGrassWave` / `RandomStartBlink` 等无关随机。**新建小脚本即可**，勿塞进 Archives / Achievement（`FindOneSecret` 等本期不做）。

---

## ③ 方案对比与推荐

| 方案 | 做法 | 裁决 |
|------|------|------|
| **A** | 「画」根挂组件：两引用 + chance；OnEnable 掷一次 | **推荐**（可复用、场景自包含） |
| **B** | `Village_HomeScene1SceneManager.OnEnterScene` Find 名字开关 | 可用但名字耦合；次选 |
| **C** | 动画 / Timeline 随机 | **否决**（过重） |

**拍板：A + 每次进场景（OnEnable）重掷；不写 Archive。**

### 逻辑契约

```
roll = Random.value          // 或等价 Range；Inspector: easterChance=0.2f
if roll < easterChance:
  彩蛋画 Active=true；正常画 Active=false
else:
  彩蛋画 Active=false；正常画 Active=true
```

- 禁止两张同时 true。  
- 缺引用 → `Debug.LogError` + 回退「只开正常画」（能开则开）。  
- 禁止 Update 每帧 Random。  
- 禁止两张都开靠 Order 盖。

---

## ④ Inspector 字段表

| 字段 | 类型 | 默认 | 说明 |
|------|------|------|------|
| `easterEgg` | `GameObject` / Transform | 绑「彩蛋画」 | 必填 |
| `normalArt` | `GameObject` / Transform | 绑「正常画」 | 必填 |
| `easterChance` | `float` `[0,1]` | **0.2** | 彩蛋概率；调 0/1/0.5 便于验收 |
| （可选）`rollOnEnable` | `bool` | **true** | 关则仅手动调；默认开 |

生命周期：优先 **OnEnable**（进场景 / 对象被激活时掷一次）。同场景不重载则不重掷；整场景重进再掷（对齐 Q1/Q2 默认）。

---

## ⑤ 要改文件（路径级；本阶段未改）

| 全路径 | 改动 |
|--------|------|
| 新建 `…/SceneEntities/…/RandomExclusiveChildActive.cs`（路径可放 `CommonEntity`） | 掷骰 + 互斥 SetActive；中文注释 |
| `Assets/GameRes/Scenes/Village_HomeScene1.unity` | 「画」挂组件并绑引用；建议进 Play 前把两子默认 Active 交给脚本（或保持双 1，靠首次 OnEnable 纠正） |
| `Village_HomeScene1SceneManager.cs` | **不改**（方案 A） |
| `ArtRes/.../村民家1合层.prefab` | **不改** |
| 其它 HomeScene* / 画框 / 观察光标 | **不改** |

施工说明：`Assets/Doc/施工说明/0922/Village_HomeScene1_墙上画彩蛋20_施工说明.md`

---

## ⑥ 验收表

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 进 HomeScene1 看墙 | **只有**正常画或彩蛋画之一（无叠影） |
| 2 | 反复进出 ≥20 次（或临时 chance=0.5） | 彩蛋会出现；多数为正常画 |
| 3 | 临时 `easterChance=1` | 必出彩蛋画 |
| 4 | 临时 `easterChance=0` | 必出正常画 |
| 5 | 画框 / 其它家具 | 不受影响 |
| 6 | 交回 `0.2` | 恢复产品概率 |
| 7 | HomeScene2/23/45 | 墙上画逻辑**无**变化 |

---

## ⑦ 开放问题

| ID | 问题 | 侦探默认 | 状态 |
|----|------|----------|------|
| Q1 | 每次进场景重掷，还是整档只掷一次写存档？ | **每次进场景重掷**；不做存档字段 | ✅ 默认拍板（产品可改） |
| Q2 | 同一次进屋中途存读档？ | 随 Q1；场景重载则再掷 | ✅ |
| Q3 | `Random.value < 0.2` vs `Range(0,100)<20`？ | 等价；Inspector 用 `easterChance=0.2` | ✅ |

若产品事后要「本档永远同一张画」→ 另票 Archive，本期不做。
