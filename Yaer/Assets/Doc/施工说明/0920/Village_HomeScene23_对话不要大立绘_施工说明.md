# Village_HomeScene23 · 对话不要大立绘 — 施工说明

**文档版本**：v1.2（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_HomeScene23_对话不要大立绘_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**椅子孩子三场任务对话不再先出雅尔大半身，直接进对话框。**

### ② 原因（通俗）

那张大半身嵌在这三张对话图里，开场会把它淡出来。淡入节点删掉之后，不能再把整张物体关掉：对话框左下角小头像是同一套立绘，关根节点会一起没。所以物体留着，只把左侧这份的透明度压成 0，不再淡入。

### ③ 用户检查清单

进 `Village_HomeScene23`，坐椅子触发三场，不要重导 CSV。

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 第一天接任务 | 不要先出雅尔大半身。第一句仍是「妈妈，有外人！」。对话框左下小头像还在 |
| 2 | 交任务、谢过 | 同样不要大半身，字和选项还在 |
| 3 | 其它村庄对话（开场、商店、门口） | 该有的大立绘不要被这次关掉 |

### ④ 程序补充

见下文。

---

## 改动清单

只改这三张 Prefab，母体、面板、CSV、场景、触发脚本都没动：

- `Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab`
- `Assets/GameRes/Prefabs/Dialogue/Village_QuestThanks_NPC23.prefab`
- `Assets/GameRes/Prefabs/Dialogue/Village_QuestTurnIn_NPC23.prefab`

每张两处：

1. 删掉开场 `CanvasGroupAlphaActionTask`（目标 `GoOutStoryYaerPainting`，EndAlpha=1）。上一节点改接到对话框淡入。
2. 嵌套实例不关物体。只把这份实例的 `CanvasGroup.m_Alpha` 写成 0。关根节点会把对话框里同名小头像一起关掉，所以改成只压透明度。

没改：`GoOutStoryYaerPainting` 母体、`NormalDialogueNewPanel`、`HomeScene1Npc1`、`Npc23QuestStoryTrigger.cs`、`Village_NPC23椅子孩子第一天对话.csv`、`Village_HomeScene23.unity`。

---

## 为什么这样改

NodeCanvas 开场是节点数组的第一项，不是「没有入边的那个」。

- 接任务图：立绘淡入原来排在数组最前，所以会先播。藏战斗面板（原节点 18）在数组末尾，连接存在但跑不到。现在把它放到数组第一项，接到对话框淡入（原节点 1），立绘节点删掉。
- 交任务、谢过：第一项本来就是藏战斗面板，立绘淡入夹在中间（0→1→2）。删掉中间节点，改成 0→2。

替代做法是只改母体默认不激活，或重导 CSV 把淡入清掉。母体会连累其它引用；重导 CSV 会把这次删掉的淡入写回去。所以只动这三张图上的实例和节点。

---

## v1.2 雅尔这句小头像是空的

截图那句是雅尔「啊啊。。。。我只是随便转转。。。。」，表情写的是 `Awkward`。外出立绘没有这张脸，尴尬脸的物体名是 `Armor_NoHeadWear_GanGa`。对不上时会把脸全关掉，左下角 Mask 就是空的。

`GoOutStoryYaerPainting.ResolveGoOutFaceKey`：雅儿 GoOut 的 `Awkward` 改去 `GanGa`。古莎仍用 `Awkward`，枚举没动。室内 Dress 没有这张脸，回 `Dress_Crown_Smile`，避免小窗全空。

孩子、妈妈的 Actor 角色是空的，没有小头像图，那几句格子仍然是空的。这不是这次关掉的。
