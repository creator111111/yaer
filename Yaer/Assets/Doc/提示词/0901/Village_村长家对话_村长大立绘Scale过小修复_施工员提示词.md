# Cursor Agent Prompt · 修 bug：村长家门口/继续对话 · 村长大立绘 Scale 过小

> **角色**：【施工员】最小化把村长立绘尺寸改回与雅/古同级观感（先核实再改）  
> **日期**：2026-09-01  
> **现象（用户截图）**：三人门口对白里，雅/古大立绘正常，**村长缩成背景小人**；Hierarchy 选中 `村长/ChiefPainting`  
> **用户 Inspector（现态）**：`Scale (0.32, 0.32, 0.32)` · Size `(1128, 2625)` · Pos `(420, -120)` · CanvasGroup Alpha 可为 0（编辑态）  
> **触发口述**：「修改了对话之后」变小（疑重跑 Setup / 生成继续对话 / 重建 ChiefPainting）  
> **目标 Prefab**：至少  
> - `Village_村长家门口初次对话`  
> - `Village_村长家继续对话`（若同嵌 ChiefPainting）  
> **说明落盘**：`Assets/Doc/施工说明/0901/Village_村长家对话_村长大立绘Scale过小修复_施工说明.md`

把下面「施工」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（施工须核实，勿当唯一真相）

### 现象对照

| 截图 | 说明 |
|------|------|
| Game | 雅左、古中为大立绘；靠门一侧村长 **极小** |
| Hierarchy | `Canvas → Village_村长家门口初次对话 → 村长 → ChiefPainting`（选中） |
| Inspector | Scale **0.32**（与母体一致） |

**不是**：Sprite 又空了（0831 丢失修）；不是缺节点。本期是 **尺寸**。

### 根因假说（磁盘预扫 · 须证伪）

| ID | 假说 | 预扫 |
|----|------|------|
| **H1** | 母体 `ChiefPainting.prefab` 默认 Scale=**0.32**（`ChiefPaintingSetupEditor` 写死） | ✅ 母体 YAML `0.32`；Setup `localScale = new Vector3(0.32f…)` |
| **H2** | 门口对话曾用 PrefabInstance **Override Scale=0.65** 对齐三人构图；改对话后 Override **丢失 / fileID 断链** → 回落到母体 0.32 | ✅ 门口磁盘仍可见 Override `0.65`；用户 Inspector 却是 `0.32` → **现编状态或 Override 失效** 必查 |
| **H3** | `Village_村长家继续对话` Setup **只 Nudge X=420，从未写 Scale** → 续聊必为 0.32 | ✅ Continue Setup 仅 `TrySetAnchoredX`；续聊 YAML **无** `m_LocalScale` Override |
| H4 | 父节点 `村长` Actor 被缩放过小 | 次查；预扫倾向否 |
| H5 | SizeDelta 被改坏（非 Scale） | 现 Size 仍 1128×2625，与母体同 → 次要 |

**「改回来」的目标尺寸（钉死倾向）**

| 项 | 值 | 来源 |
|----|-----|------|
| **对话内 ChiefPainting Scale** | **`0.65`** | 门口 Prefab 曾定稿的 Override；与雅线大立绘常用 0.65 同量级 |
| Pos | 保持 `(420, -120)` 或仅 X=420（Setup Nudge） | 勿为放大乱挪脚 |
| SizeDelta | 保持 `1128 × 2625` | 勿靠改 Size 冒充放大 |
| 雅 / 古 | **禁止**为「对齐」去改他们的 Scale | 立绘各自定稿 |

视觉验收：村长与雅/古同为「半身～全身级」大立绘，**不再**像场景里的小 NPC。

### 修复倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A** | 门口 + 继续两对话 Prefab：实例 `ChiefPainting.localScale = (0.65,0.65,0.65)` 并保存 Override | ✅ **必做** |
| **A+** | Setup（Door + Continue）`NudgePortraitLayout` 增加写 Scale=0.65，防再跑菜单回潮 | ✅ 推荐 |
| B | 只改母体 `ChiefPainting` 默认 0.32→0.65 | ⚠️ 可能影响其它引用；若仅对话用可接受，报告写清 |
| C | 只改 SizeDelta 不改 Scale | ❌ |
| D | 改 Canvas Scaler / 分辨率 | ❌ |

若门口磁盘 Override 仍是 0.65 但 Play 仍小：查 **Override 的 target fileID** 是否仍指向当前 RectTransform（重建母体后易断链）→ 删旧 Override 重写或 Apply 新修改。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 村长对话大立绘 Scale 恢复 **0.65**（门口+继续） | ❌ 重做 Face/Sprite/Import |
| ✅ Setup 防回潮（写 Scale） | ❌ 改雅/古立绘尺寸「强行统一」 |
| ✅ 短施工说明 | ❌ 无关换场 / 针线包 / Tips |

### 严禁

- 只改母体却不验两对话实例  
- 把 0.32 当「正确」留下（用户已否）  
- 用父节点乱 Scale 叠乘  
- 为修尺寸清空 Sprite / 拆嵌套  

### 开放

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 母体默认是否一并改为 0.65？ | **先实例+Setup**；母体可随 A+ 或保留 0.32 仅对话 Override |
| Q2 | Y 是否微调？ | **先只 Scale**；脚切再单调 Pos.y |
| Q3 | 晚宴等其它嵌 Chief 的图？ | 检索一并列出；本期优先门口+继续 |

---

## 施工 Prompt（复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md

## Bug
修改对话相关内容后，村长家门口（及继续）对话里村长大立绘过小（用户截图像背景小人）。
Hierarchy：Village_村长家门口初次对话 → 村长 → ChiefPainting。
现 Inspector：Scale 0.32；用户要求改回与三人对话匹配的大小。

## 必读
@Assets/Prefabs/DialougeProtrait/ChiefPainting.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/Editor/ChiefPaintingSetupEditor.cs
@Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab
@Assets/Editor/Tool/Dialogue/VillageChiefDoorDialogueSetupEditor.cs
@Assets/Editor/Tool/Dialogue/VillageChiefContinueDialogueSetupEditor.cs
@Assets/Doc/施工说明/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_施工说明.md

## 核实（改前写进施工说明）
1. 母体 Scale 是否 0.32；Setup 是否写死 0.32。
2. 门口 PrefabInstance 是否曾有 / 仍有 Scale=0.65 Override；fileID 是否断链。
3. 继续对话实例当前 Scale；Setup 是否只改了 Anchored X。
4. 父节点「村长」Scale 是否为 1。

## 修复目标（钉死）
- 对话内 ChiefPainting：**localScale = (0.65, 0.65, 0.65)**
- SizeDelta / 三脸 Sprite 保持正确；Pos 默认保持 (420,-120) 量级
- 门口 + 继续两 Prefab 都要到
- Door/Continue Setup 的 NudgePortraitLayout：补写 Scale=0.65，避免菜单重跑回 0.32
- 禁止改雅/古立绘 Scale 来「凑齐」

## 落盘
Assets/Doc/施工说明/0901/Village_村长家对话_村长大立绘Scale过小修复_施工说明.md
结构：①结论 ②根因（H1/H2/H3）③改了什么 ④验收 ⑤程序补充
若设计不清记 OPEN_QUESTIONS。

## 验收
- [ ] Prefab 模式 / Play：村长与雅/古同为正常大立绘体量，不再像小豆人
- [ ] ChiefPainting Scale 显示 0.65（门口 + 继续）
- [ ] Face1～3 Sprite 仍在；前奏淡入仍三路
- [ ] 重跑 Door/Continue Setup 后 Scale 仍为 0.65（若做了 A+）
- [ ] 雅/古尺寸未被动过

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. 直接跑上面「施工 Prompt」即可（根因已较清楚：母体 **0.32**，对话应 **0.65**）。  
2. 你 Inspector 里的 **0.32** 就是母体默认；改对话/Setup 后容易丢掉门口曾经的 **0.65** Override。  
3. 改完 Play 看三人并排：村长不应再缩在门边当小景。
