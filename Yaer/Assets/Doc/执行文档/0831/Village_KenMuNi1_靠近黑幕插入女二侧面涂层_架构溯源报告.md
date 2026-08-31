# Village_KenMuNi1 — 靠近黑幕插入女二侧面涂层 — 架构溯源报告

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读溯源 + 分层/时序拍板（**本阶段未改场景 / 代码 / 导图**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**产品**：走近指定 NPC → 黑幕淡入淡出；**黑幕内**插入女二（古莎）**侧面全身场景涂层**；亮屏后 **玩家渲染在女二之下**；与 **老人** 对白前后错开  
**提示词**：`提示词/0831/Village_KenMuNi1_靠近黑幕插入女二侧面涂层_架构侦探提示词.md`  
**强关联**：`执行文档/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构溯源报告.md`（须 **合并同一黑幕**，禁止连闪两次）  
**非本期**：UI `GushaPainting` / Mask 正脸表情；把侧面图塞进对话 Canvas 冒充世界遮挡

---

## 沟通摘要

### ① 结论一句话

**拍板 C：挂在合层村长旁 `Npc_Chief` 同一条靠近黑幕链上——全黑时启用预置 `GushaSidePortrait`（世界 SR、钉 `SceneObject`），再播 `Village_村长家门口初次对话`；「老人」=村长（台本称奶奶），不是老农；女二侧面与 UI 正脸立绘职责分离，保证玩家压在侧面涂层下。**

### ② 原因（通俗）

走近村长家门口那段戏时，不只弹 UI 大立绘，还要在场里「变」出古莎侧身站着，像舞台布景挡住雅尔一点，再跟奶奶说话。  
黑一下是为了让侧面图无缝出现，不要亮着突然蹦出来；侧面图和对话框里的正脸古莎是两套东西。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 走近村长旁（不点 E） | **一次**黑幕；亮后场上有女二侧面 |
| 2 | 站位重叠区肉眼 | 女二图在玩家 **上面**（玩家在涂层下） |
| 3 | 对白 | `Village_村长家门口初次对话`；面向/错开村长可读 |
| 4 | 黑幕前 | 无裸切露馅；无二次连闪黑幕 |
| 5 | 同档再走近 | 不再触发（单次，跟靠近报告） |
| 6 | UI 正脸 / Mask 古莎 | 对话壳仍正常，不坏 |
| 7 | 点 `House_Chief` | 仍只进屋 |

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 身份方案 | **C**（见 §②） |
| 触发 | 与靠近报告 **合并**：`Objects/Npc_Chief` Enter + 同一 BlackPanel |
| Story | **`Village_村长家门口初次对话`** |
| 老人 | **村长**（Speaker「村」/ 台本「奶奶」）；**≠** `Npc_Farmer` |
| 女二侧面 | 世界 **`GushaSidePortrait`** SR；**S1** 预置默认关，黑幕内 `SetActive(true)` |
| Sorting | **F1 钉死 `SceneObject`**（验收硬条件）；勿靠 UI Canvas |
| 与 UI | 侧面 = 场景涂层；正脸 = `GushaPainting` / Mask —— **禁止硬绑同一套逻辑** |

---

## ② 触发与对白身份（A/B/C 拍板）

| 假说 | 含义 | 裁定 |
|------|------|------|
| A | 靠近村长，另绑「只跟村长」对白 | ❌ 现网无单独村长-only Prefab；与门口戏重复 |
| B | 靠近 `Npc_Farmer`，打水对白插侧面 | ❌ 证伪：Farmer Story=`Village_老农打水任务`；提示词挂点对齐村长线；台本「奶奶」≠老农 |
| **C** | 靠近村长；对白仍是 **门口三人戏**；侧面 = **场景氛围层** | ✅ |

**证据**

| 源 | 事实 |
|----|------|
| CSV `Village_村长家门口初次对话.csv` | 古称村「**奶奶**」；Speaker 村/古/雅 |
| 靠近报告 | `Npc_Chief` + 黑幕 + 同 Story 名 |
| 场景 | `Npc_Farmer` → 打水；**无** `Npc_Chief`（待施工）；合层 `村长` ≈世界 `(-157.7,-1.2,2.82)` 仅 SR |

**「老人」释义**：产品口「老人」= **精灵村长（奶奶）**，不是打水老农。

---

## ③ 黑幕 + 插层时序（与靠近报告合并）

**禁止**：「靠近村长黑幕」播完再黑一次插侧面。必须 **同一 ShowFade**。

```
玩家 Enter Npc_Chief（SingleUse 未用）
  → OpenUIForm BlackPanel FadeShow
  → onShowEnd（全黑）:
       ① 启用 GushaSidePortrait（SetActive + 校准 Sorting/位）
       ② TriggerStory("Village_村长家门口初次对话")
       订阅 onStoryTriggered
  → onStoryTriggered:
       极短 hold → HideFade
  → 亮屏可见：世界侧面涂层（压玩家）+ UI 三立绘壳
  → onStoryEnd:
       SingleUse 记档；侧面按 §⑧ Q1 处理（默认关）
```

序列图：

```
[Explore] --Enter--> [ShowFade] --全黑--> [Enable Side] + [TriggerStory]
                                              |
                                    onStoryTriggered
                                              |
                                         [HideFade] --> [Dialogue + 世界分层可读]
                                              |
                                          onStoryEnd --> [Hide Side? / 保留?]
```

| 项 | 拍板 |
|----|------|
| 插层时机 | **必须在全黑后、HideFade 前**（禁止亮屏后弹侧面） |
| 谁编排 | 扩 `ChiefNearDoorStoryTrigger`（或同挂组件）：黑幕回调里先 `EnableSide` 再 `TriggerStory` |
| Prefab 内 BlackMask | ❌ 勿叠第二层系统黑 |
| 无黑幕瞬切侧面 | ❌ |

---

## ④ Sorting / 世界位方案

### A. 分层基准（验收硬条件）

**玩家渲染在女二侧面之下** = 女二 SR 所在 SortingLayer 比玩家更「前」。

现网（`SortingLayerName` + 围栏金样）：

| 层 | 相对绘制 |
|----|----------|
| Default | 靠后 |
| Player | 中 |
| **SceneObject** | **更靠前**（可挡住玩家） |

玩家村庄排序：`TownPlayerLocomotion` / `DepthComponent` 改 **Default 上 sortingOrder**（按 Y），**不**跳到 SceneObject。

| 方案 | 做法 | 裁定 |
|------|------|------|
| **F1 钉 SceneObject** | 侧面 SR：`sortingLayerName=SceneObject`，`sortingOrder=0`（可调 0～10） | ✅ **本期**（必过「玩家在下」） |
| F2 DepthSort 动态 | 挂 `VillageSceneObjectDepthSort` 抄围栏 6/0 | ⏳ P1；对白中可能让玩家绕到「前」盖住女二，易不合格 |
| F3 只抬 Order 仍 Default | 与玩家抢 Order，易翻车 | ❌ |

**本期勿挂** `DepthComponent` 与 `VillageSceneObjectDepthSort` 双开（脚本已 Warning）。

### B. 物体方案 S1

| 项 | 拍板 |
|----|------|
| 名 | **`GushaSidePortrait`**（Objects 下，可与 `Npc_Chief` 同级或子物体） |
| 组件 | Transform + **SpriteRenderer**（无 Interactive；非触发源） |
| 默认 | **`SetActive(false)`** |
| 启用 | 黑幕 `onShowEnd` 内 `true` |
| 父节点 | **Objects，Z=0**（勿挂合层 Z≠0 装饰链） |
| 世界位建议 | 合层 `村长` 旁 ≈**(-156.5～-158.5, -1.5～-0.5, 0)**；略偏玩家走近走廊，与合层村长脚位错开；Scene 微调 |
| 纵深意图 | 世界 **Y 略低于** 玩家预期站位（DNF：Y 低更「前」），再靠 SceneObject 盖住 |
| 与合层 `村长` | 合层村长保留；侧面是 **古莎**，勿替换村长贴画 |

### C. 美术资源（磁盘）

| 检索 | 结果 |
|------|------|
| `*侧面*` | ❌ 工程内 **无** 女二侧面成品图 |
| 已有古莎 | UI `GushaPainting`、动画站立/坐、`村长家合层/古莎待机.png` 等——**皆非**用户所述蝶翼裙侧身全身 |
| 身份 | 女二 = **古莎**；侧面为 **新资源** |

**落盘建议（施工导图）**

| 项 | 建议 |
|----|------|
| 路径 | `Assets/ArtRes/Scene/Village/GushaSide/古莎_侧面全身.png`（或中文「女二侧面」子目录） |
| Import | Sprite (2D)；PPU 对齐村合层立绘（先跟同场景人物图；不对再调） |
| Pivot | 脚底中心（便于贴地） |
| 禁止 | 塞进 `Avatar_Gusha` 图集当 Mask；替换 `GushaPainting` 正脸 PSD |

---

## ⑤ 与 UI 立绘 / Mask 职责边界

| 层 | 资源 | 职责 | 本期 |
|----|------|------|------|
| **世界侧面涂层** | `GushaSidePortrait` + 新 PNG | 场内遮挡玩家、舞台纵深 | ✅ 新建 |
| **UI 大立绘** | `GushaPainting` / `GuShaPainting.cs` | 对话壳正脸表情 | 门口 Prefab 用；**不改逻辑冒充侧面** |
| **Mask 小头像** | `DialogueMaskAvatarPresenter` | 字幕旁小脸 | 不碰 |
| **合层装饰** | `村长` / `村长家门` | 场景贴画 | 保留；非触发、非侧面 |

对白播放时：**世界侧面**（氛围）与 **UI 正脸古莎**（台词表情）可同帧存在——前者管遮挡，后者管对白表现；勿用 UI 图去满足「玩家在涂层下」。

---

## ⑥ 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 导入女二侧面 PNG → Sprite | **P0** |
| 2 | 场景/Prefab：`GushaSidePortrait`（默认关，SceneObject，位在村长旁） | **P0** |
| 3 | 扩靠近黑幕触发：`onShowEnd` **先 EnableSide 再 TriggerStory**（合并一次黑幕） | **P0** |
| 4 | 依赖：`Npc_Chief` + `ChiefNearDoorStoryTrigger`（若未建，与靠近报告同轮） | **P0** |
| 5 | `onStoryEnd` 侧面去留（默认关，见 Q1） | P0 |
| 6 | 回归：UI 古莎 / Mask / `House_Chief` / 老农打水 | P0 |

**不改**：村庄 Y→Z；`VillageSceneObjectDepthSort.cs` 核心（除非 P1 动态遮挡）；晚宴表情；打水任务绑侧面。

---

## ⑦ 验收清单

同沟通摘要 §③；程序加测：

- [ ] 运行时 Inspector：侧面 SR `sortingLayerName == SceneObject`
- [ ] 玩家与侧面重叠时，玩家精灵被挡住（截图可归档）
- [ ] Console 无 Missing；无 DepthComponent+DepthSort 双开 Warning（侧面勿双挂）
- [ ] 仅 **一次** BlackPanel 周期

---

## ⑧ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 对白结束侧面去留？ | **onStoryEnd `SetActive(false)`**（不二次黑幕）；常驻/再黑摘掉为备选 | ⏳ 产品可改 |
| Q2 | 对白中玩家能否走动改遮挡？ | 对白中现网多 Pause；暂停则 F1 足够 | ✅ 跟现网 Pause |
| Q3 | 是否挂 DepthSort？ | **本期否**；验收过后再议 | ✅ |
| Q4 | 侧面 Scale / 精确 XY？ | Scene 美术微调；报告给区间即可 | ⏳ 施工 |
| Q5 | 与 UI 正脸古莎同场是否「双重古莎」违和？ | 舞台常见；若产品嫌多可对白中隐侧面（P1） | ⏳ |
| Q6 | A/B/C？ | **C** | ✅ 本报告 |

---

## ⑨ 程序补充（速查）

| API / 锚点 | 用途 |
|------------|------|
| `ChiefNearDoorStoryTrigger` + BlackPanel `onShowEnd` | 合并：EnableSide → TriggerStory → HideFade |
| `StoryComponentGSM.onStoryTriggered` / `onStoryEnd` | 亮屏 / 关侧面+单次档 |
| `SortingLayerName.SceneObject` | 钉死盖玩家 |
| `VillageSceneObjectDepthSort` | 围栏金样；**本期侧面不用** |
| `TownPlayerLocomotion` depth sort | 玩家仍在 Default Order；被 SceneObject 压 |
| 合层 `村长` 世界约 | `(-157.7, -1.2)`；交互 `Npc_Chief` Z=0 |
| Farmer | **无关**；Story=`Village_老农打水任务` |
| UI | `GuShaPainting` / `GushaPainting.prefab` — 边界见 §⑤ |

**与靠近报告合并一句**：靠近村长黑幕的产品时序升级为  
`ShowFade → 插女二侧面 → TriggerStory(门口初次) → HideFade`；施工勿拆成两次黑幕任务。
