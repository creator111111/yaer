# Cursor Agent Prompt · HomeScene1 出门地图对白：「这么远!!!!」· VerySurprised 对接核验

> **角色**：【架构侦探】只读核验对接；**禁止改代码 / Prefab / 图集 / Git 提交**（用户已手工补 Faces）  
> **日期**：2026-09-12（修订：用户已补 `Armor_NoHeadWear_VerySurprised`）  
> **已确认根因（产品侧）**：台本 `FaceType = VerySurprised`，原先 `GoOutStoryYaerPainting/Faces` **缺同名键** → 空白脸  
> **用户已完成**：在 `GoOutStoryYaerPainting` → `Faces` 下新增 **`Armor_NoHeadWear_VerySurprised`**（Hierarchy 截图红箭头已标）  
> **本阶段目标（钉死）**：  
> 1. **对接核验**：`HomeScene1GetMap`「这么远!!!!」→ `VerySurprised` → `ResolveGoOutFaceKey` → **新 Faces 子物体** 是否整条通  
> 2. 检查新节点是否只改了名字、还是 **Image/Sprite 引用也绑好**（有物体但图空仍会空脸）  
> 3. 列出同 `VerySurprised` 影响面；说明是否还要补小头像图集 / 其它 Painting Prefab  
> 4. 给出 **Play 验收清单**；若无需再改代码，明确写「施工员可跳过 / 仅文档回写」  
> **不是**：再猜「缺哪个名」；改枚举顺序；批量改台本 FaceType；修 0911 父亲旧头像案  
> **报告落盘**：`Assets/Doc/执行文档/0912/HomeScene1出门地图对白_这么远表情丢失_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话（当前状态）

> 就是缺了一个 **`VerySurprised`**。  
> 用户已经在 GoOut 立绘 Faces 里补了 **`Armor_NoHeadWear_VerySurprised`**。  
> 侦探不要再当「还没补」来查；要核对：**程序找的名字 ↔ 新物体名 ↔ 贴图引用**是否对齐，以及后面用同一表情的对话是否一并修好。

### 对接契约（已拍板，侦探只验证）

```
台词「这么远!!!!」
  ∈ HomeScene1GetMap.prefab（StatementNodeEx，$id≈9，Actor=雅尔）
  FaceType._value = 8 → DialogueFaceType.VerySurprised

运行时（GoOut / Mask 立绘）：
  GoOutStoryYaerPainting.ResolveGoOutFaceKey(VerySurprised)
    → "Armor_NoHeadWear_VerySurprised"
    → Faces 下同名子物体 SetActive + Image

用户已补：
  Prefab: Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab
  Hierarchy: GoOutStoryYaerPainting / Faces / Armor_NoHeadWear_VerySurprised  ← ★ 已存在
```

| 环节 | 期望 | 侦探核验点 |
|------|------|------------|
| 对话文件 | `HomeScene1GetMap.prefab` | 复核台词 + FaceType=8 |
| 枚举 | `VerySurprised` | `DialogueFaceType.cs` 勿与 `ZhenJing` 混淆 |
| Resolve 键 | `Armor_NoHeadWear_VerySurprised` | 与 `ResolveGoOutFaceKey` 字符串完全一致 |
| Faces 子物体 | **已由用户新增** | YAML/`m_Name` 是否真是该字符串（无空格/错拼） |
| 贴图 | Image.sprite / 子图非空 | **有空物体无图仍空脸** —— 必查 |
| 默认显隐 | 仅当前表情 Active | 新节点默认是否误开导致叠脸 |

### 触发链（背景，仍须一句话写进报告）

```
GoOutStoryCollider → TriggerStory("HomeScene1GoOutStory")
  → …/HomeScene1GoOutStory.prefab
  → TriggerStory("HomeScene1GetMap")
  → …/HomeScene1GetMap.prefab   ← 「这么远!!!!」
  → GoOutMapStoryLogic + 地图 UI
```

### 与 `ZhenJing` 的边界（避免误改）

| 枚举 | Resolve 键 | 含义 |
|------|------------|------|
| `VerySurprised` | `Armor_NoHeadWear_VerySurprised` | **本案台本用的**；用户刚补 |
| `ZhenJing` / `ZhenJing2` | `Armor_NoHeadWear_ZhenJing*` | 另一套「震惊」键；**不要**改台本去顶替本案，除非用户另行要求 |

### 侦探须回答的核心问题（对接版）

1. **链路是否闭合**：`VerySurprised` → Resolve 键 → 用户新 Faces 节点，名字是否 **逐字符一致**？  
2. **资源是否可用**：新节点上的 **Sprite/Image 是否已赋值**？未赋值则报告写「名字齐了但图空，仍须绑图」。  
3. **显示槽**：出门地图句空白脸走的是 Mask/`GoOutStoryYaerPainting`，还是旧小头像图集？补 Faces 后是否 **足够** 修截图像；小头像 `Avatar_Yaer_*.spriteatlas` 的 `VerySurprised` 是否已有、是否需 Pack。  
4. **影响面**：`GameRes/Prefabs/Dialogue` 中雅儿 `FaceType=VerySurprised`（value=8）的 Prefab/句数清单 —— 补这一键后哪些对话应一并恢复。  
5. **还需施工吗**：  
   - 若名字+贴图都齐、无代码缺口 → **结论：无需施工员改代码；建议 Play 验收 +（可选）回写 0601 对照表一行**  
   - 若缺绑图 / 缺其它 Prefab 副本 / 缺图集 Pack → 列出 **最小剩余清单**（仍不改代码，留给用户或下一施工 Prompt）

### 必读 / 扫描范围

**必读**

1. `Assets/Project_context.md`  
2. `Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md`  
3. `DialogueFaceType.cs`  
4. `GoOutStoryYaerPainting.cs`（`ResolveGoOutFaceKey`）  
5. 本提示词（以「用户已补 Faces」为准）

**扫描（优先对接核验）**

- `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab`：确认存在 `m_Name: Armor_NoHeadWear_VerySurprised`，并读其 Image/Sprite 引用  
- 若场景/对话壳里嵌了该 Prefab 实例：查 Override 是否把新 Faces 弄丢或关掉  
- `HomeScene1GetMap.prefab`：复核「这么远!!!!」FaceType  
- 全 Dialogue Prefab：`VerySurprised` / FaceType value=8 命中表  
- （对照）`ArtRes/.../Yaer/Avatar/**/VerySurprised.png` 与图集是否已含同名 Sprite  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity 2020.3.48f1 / C#。只读。

**前提**：用户已在 `GoOutStoryYaerPainting/Faces` 增加 `Armor_NoHeadWear_VerySurprised`。你的任务是 **核验对接是否完成**，不是再找「缺哪个英文名」。

### 输出

写到：`Assets/Doc/执行文档/0912/HomeScene1出门地图对白_这么远表情丢失_架构溯源报告.md`

固定结构：

1. **结论一句话**（对接是否闭合；是否还需绑图/Pack/其它动作）  
2. **对接表**：台本 FaceType → Resolve 键 → Prefab 节点 → Sprite 状态（✅/❌）  
3. **调用链**（出门地图一句走通即可）  
4. **影响面清单**（同 `VerySurprised` 对话）  
5. **Play 验收清单**（逐步：进 GetMap → 点到「这么远!!!!」→ 看脸）  
6. **剩余风险 / OPEN**（若有）写入 `Assets/Doc/OPEN_QUESTIONS.md`  
7. **给施工员的一句话**：`无需改代码` 或 `仅文档回写` 或 `仍需：……`（列具体项）

### 限制

- 不改代码 / Prefab / 图集；不提交 Git；**不要删用户新加的 Faces**  
- 不要建议把台本改成 `ZhenJing` 来「代替」已补的 `VerySurprised`（除非发现 Resolve 写死了别的键且证据确凿）  
- 默认中文；大白话 + 精确路径/名字  

---

## 【施工员】Prompt（仅当侦探结论写「仍需…」或「仅文档回写」时再用）

> **若侦探结论 = 对接已闭合、贴图已绑**：  
> - **跳过改代码**。可选：在 `0601/对话立绘表情与图片名称对照_执行说明.md` 的 GoOut Faces 列表补一行 `Armor_NoHeadWear_VerySurprised`；施工说明写「用户已补资源，侦探核验通过」。  
> **若侦探结论 = 名字有、图未绑 / 实例 Override 丢节点**：  
> - 只做最小补绑：保证 `Armor_NoHeadWear_VerySurprised` 的 Image 指向正确 Sprite；勿改枚举、勿改台本。  
> **文档**：`Assets/Doc/施工说明/0912/HomeScene1_VerySurprised表情补名_施工说明.md`  
> **验收**：`HomeScene1GetMap`「这么远!!!!」五官可见；另抽 ≥2 处 `VerySurprised` 对话正常。  
