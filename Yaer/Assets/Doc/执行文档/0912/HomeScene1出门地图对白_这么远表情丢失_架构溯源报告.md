# HomeScene1 出门地图对白 ·「这么远!!!!」VerySurprised 对接核验 — 架构溯源报告

**文档版本**：v1.0（2026-09-12）  
**文档性质**：【架构侦探】只读对接核验；**未改**代码 / Prefab / 图集 / Git  
**Unity**：2020.3.48f1  
**前提（产品已确认）**：台本 `FaceType = VerySurprised`；原先 `GoOutStoryYaerPainting/Faces` 缺同名键 → 空白脸  
**用户已完成**：在 `GoOutStoryYaerPainting` → `Faces` 下新增 **`Armor_NoHeadWear_VerySurprised`**  
**本阶段目标**：核验 Resolve 键 ↔ 新节点名 ↔ Sprite 是否整条通；列影响面与 Play 验收；判定是否还需施工  
**提示词**：`Assets/Doc/提示词/0912/HomeScene1出门地图对白_这么远表情丢失_架构侦探提示词.md`  
**对照**：`Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md`

---

## 沟通摘要

### ① 结论一句话

**主路径对接已闭合**：`VerySurprised` → `Armor_NoHeadWear_VerySurprised` → Prefab Faces 同名节点 → Image 已绑 `震惊.png`（非空物体）；**小头像四套图集已含 `VerySurprised`，不必为本案再 Pack**；**施工员可跳过改代码**（可选仅回写 0601 对照表一行）。Play 验收「这么远!!!!」五官；若仍空脸再查 GetMap **拆包内嵌**旁路（见 §6）。

### ② 原因（通俗）

程序找的是「铠甲无头饰_非常惊讶」这张脸的物体名。你已经在 GoOut 立绘 Faces 里补了同名节点，并且挂了震惊图，所以共享 Prefab / 对话壳嵌套实例这条主线应已修好。  
不要和「震惊 ZhenJing」那套键搞混——台本用的是 `VerySurprised`，不是 `ZhenJing`。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 期望 |
|---|------|------|
| 1 | Play：HomeScene1 → 出门触发 → 进到 `HomeScene1GetMap` → 点到雅尔「这么远!!!!」 | 大立绘五官可见（惊愕），非空白脸 |
| 2 | Hierarchy：运行时选 `GoOutStoryYaerPainting/Faces/Armor_NoHeadWear_VerySurprised` | `active=true`；Image 有 Sprite |
| 3 | 同句看字幕条小头像（若开） | 有脸（图集已含 `VerySurprised`） |
| 4 | 抽测 ≥2 处其它雅尔 `VerySurprised`（如东郊树桥 / 史莱姆） | 同样有脸 |
| 5 | （可选）0601 对照表 GoOut Faces 列表补一行 `Armor_NoHeadWear_VerySurprised` | 文档与 Prefab 一致 |

### ④ 程序补充

见下文 §1～§7。

---

## 1. 对接表（台本 → Resolve → Prefab → Sprite）

| 环节 | 期望 | 磁盘实况 | 状态 |
|------|------|----------|------|
| 台词 | `HomeScene1GetMap`「这么远!!!!」、Actor=雅尔 | StatementNodeEx，`FaceType._value=8`，`_text=这么远!!!!`，`_actorName=雅尔` | ✅ |
| 枚举 | `DialogueFaceType.VerySurprised` | 枚举序：None=0…Sad=7，**VerySurprised=8**（与台本 value 一致） | ✅ |
| 与 ZhenJing 边界 | 勿混用 | `ZhenJing`/`ZhenJing2` 为末尾追加枚举；Resolve 键 `Armor_NoHeadWear_ZhenJing*`；**本案不改台本去顶替** | ✅ |
| Resolve 键 | `Armor_NoHeadWear_VerySurprised` | `GoOutStoryYaerPainting.ResolveGoOutFaceKey`：`$"Armor_NoHeadWear_{faceType}"`（Normal→Smile） | ✅ 逐字符一致 |
| Prefab 节点名 | 同名子物体 | `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` → `m_Name: Armor_NoHeadWear_VerySurprised` | ✅ |
| 默认显隐 | 默认关，仅当前脸开 | `m_IsActive: 0` | ✅ |
| Sprite | Image 非空 | `m_Sprite` → guid `731915572517fe848a200dcc19d1ca4f` = `ArtRes/.../表情/震惊.png` | ✅ 已绑图 |
| 与 ZhenJing 贴图 | — | `Armor_NoHeadWear_ZhenJing` **共用同一 guid**（同一张震惊图）——资源复用可接受，非空脸 | ℹ️ |
| Mask Presenter | 同 Resolve | `DialogueMaskAvatarPresenter.ResolveFaceKey` GoOut 分支调同一 `ResolveGoOutFaceKey` | ✅ |
| 对话壳嵌套 | 不丢新 Faces | `NormalDialogueNewPanel` / `HomeScene1GoOutStory` 为 PrefabInstance（guid `4c0e990…`）；**无**删掉新 Faces 的 Override | ✅ |
| 小头像图集 | `VerySurprised` 已在 atlas | `Avatar_Yaer_{Dress_Crown\|Armor_NoHeadWear\|Armor_ArmorHead\|Armor_Crown}.spriteatlas` 均含 `VerySurprised`；源图四套 `ArtRes/.../Yaer/Avatar/**/VerySurprised.png` 存在 | ✅ **无需为本案 Pack** |

**主路径裁定**：名字齐 + 图已绑 → **对接闭合**。

---

## 2. 调用链（出门地图一句）

```
GoOutStoryCollider → TriggerStory("HomeScene1GoOutStory")
  → …/HomeScene1GoOutStory.prefab
       （嵌套 PrefabInstance：GoOutStoryYaerPainting ← 用户补 Faces 的同一资产）
  → TriggerStory("HomeScene1GetMap")
  → …/HomeScene1GetMap.prefab
       Statement「这么远!!!!」 FaceType=8 (VerySurprised) Actor=雅尔
  → DialogueActorEx.OnRefreshAvatarEvent
       → GoOutStoryYaerPainting.UpdateFace(ResolveGoOutFaceKey(VerySurprised))
       → UpdateFace("Armor_NoHeadWear_VerySurprised")
       → Faces 下同名子物体 SetActive(true) + 已绑 Sprite
  →（并行）Mask/字幕条：DialogueMaskAvatarPresenter 同键切脸 / AvatarLoader 取图集 VerySurprised
  → GoOutMapStoryLogic + 地图 UI
```

---

## 3. 影响面清单（`FaceType._value=8` = VerySurprised）

扫描：`Assets/GameRes\Prefabs\Dialogue\*.prefab` 中 `"FaceType":{"_value":8}`。

| Prefab | 句数 | 说话人（磁盘） | 与本案 GoOut Faces 关系 |
|--------|------|----------------|-------------------------|
| **HomeScene1GetMap** | 1 | 雅尔「这么远!!!!」 | **本案主句**；走 GoOut Resolve |
| ForestEastSceneEnterTreeBridge | 3 | 雅尔 | 村外/铠甲线，受益于同一 GoOut 键 |
| ForestEastSceneSlimeEatSheep | 3 | 雅尔 | 同上 |
| ForestEastSceneViewBridgeFracture | 2 | 雅尔 | 同上 |
| VerdantCorridorAfterDestoryNest | 1 | 雅尔 | 同上（若用 GoOut 立绘） |
| VerdantCorridorBeforeDestoryNest | 1 | 雅尔 | 同上 |
| VerdantCorridorBeforeEnterHolyLand | 2 | 雅尔 | 同上 |
| VerdantCorridorNotDestoryNestForLongTime | 3 | 雅尔 | 同上 |
| WestRappRoadGoblinAndGusha | 7 | 雅尔 / 金发女孩 / 其它 | 雅尔句受益；非雅尔走各自 Painting |
| Village_ShopStart | 2 | **古莎** | 古莎 Faces 裸名 `VerySurprised`；**不依赖** GoOut Armor 键 |
| NewGameStory | 3 | 雅尔 | **Dress 线**：`Dress_Crown_VerySurprised`（NewGame 立绘已有节点）；**不是** GoOut Armor 键 |

补这一键后：**所有走 `GoOutStoryYaerPainting` + 雅尔 `VerySurprised` 的对话应一并恢复**；NewGame Dress / 古莎另轨，原本不靠该节点。

---

## 4. Play 验收清单

1. Init → 进到可触发出门剧情的 HomeScene1 状态（或 DialogDebug 直接播 `HomeScene1GetMap`）。  
2. 走完/跳到地图对白，点到雅尔 **「这么远!!!!」**。  
3. **大立绘**：五官可见，惊愕表情；Hierarchy 确认 `Armor_NoHeadWear_VerySurprised` 为当前唯一 Active 脸。  
4. **小头像**（若显示）：非空框。  
5. 对照：同流程前一句非 VerySurprised（如 Smile）仍正常，无叠脸。  
6. 抽测：`ForestEastSceneEnterTreeBridge` 或 `ForestEastSceneSlimeEatSheep` 中任意雅尔 VerySurprised 句。  
7. 失败分支：若 Prefab 路径已确认但仍空脸 → 查 §6 GetMap 拆包内嵌是否被激活。

---

## 5. 给施工员的一句话

**无需改代码。**  
可选：**仅文档回写**——在 `0601/对话立绘表情与图片名称对照_执行说明.md` 的 GoOut Faces / 雅儿列表补一行 `Armor_NoHeadWear_VerySurprised`（惊愕）；施工说明可写「用户已补资源，侦探核验通过」。  

**仍需（仅当 Play 失败）**：给 `HomeScene1GetMap` 内嵌拆包 `YaerPainting` 补 `Armor_NoHeadWear_VerySurprised`（或对齐脚本/键名）——见 OPEN Q2；**不要**把台本改成 `ZhenJing`。

---

## 6. 剩余风险 / OPEN

| ID | 风险 | 说明 | 建议 |
|----|------|------|------|
| R1 | GetMap **拆包内嵌**立绘键名不一致 | `HomeScene1GetMap` 内 `YaerPainting` 挂的是 **`GoOutStoryYaerPainting` 脚本**，但 Faces 子物体仍是裸名（`Smile`/`VerySurprised`…），**没有** `Armor_NoHeadWear_*`。若运行时真正亮的是这份拆包副本，`UpdateFace(Resolve…)` 会找不到键 → 仍空脸；且理论上 Smile 也会挂（与「只有 VerySurprised 坏」的产品描述不完全吻合 → **高度可疑为死旁路 / 未启用**，`YaerPainting.m_IsActive` 磁盘为 0） | Play 先验主路径；失败再打开该副本核对 Active |
| R2 | 拆包 `VerySurprised` Image guid `841320b8…` | 仓库内 **未找到** 对应 `.meta`（疑似断链）；即使用裸名激活也可能无图 | 仅旁路；主路径用 `震惊.png` guid `73191557…` 已通 |
| R3 | VerySurprised 与 ZhenJing 同图 | 美术是否接受「非常惊讶=震惊」同图 | 产品可选后续换独立贴图；**非空脸 blocker** |

已记入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

## 7. 证据锚点（路径速查）

| 主题 | 路径 |
|------|------|
| Resolve | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/GoOutStoryYaerPainting.cs` |
| 枚举 | `Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs` |
| Prefab（用户已补） | `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` |
| 台本 | `Assets/GameRes/Prefabs/Dialogue/HomeScene1GetMap.prefab` |
| Mask | `DialogueMaskAvatarPresenter.cs` → `ResolveGoOutFaceKey` |
| 贴图 | `Assets/ArtRes/UI/Story/DialogueForm/Yaer/Face/Dress/雅尔游戏中立绘/表情/震惊.png` |

---

## 8. 限制

- 未改代码 / Prefab / 图集；未删用户新 Faces；未提交 Git。  
- 未建议台本改 `ZhenJing` 顶替。  
- 未 Play；验收以 §4 为准。
