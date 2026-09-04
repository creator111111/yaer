# Cursor Agent Prompt · Village_Shop：把 `Village_ShopHead` 对话装到商人 Head 热区

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-29  
> **场景**：`Village_Shop.unity` · `商店界面合层` → `MerchantPainting` → `Trigger` → **`Head`**（用户 Hierarchy 已高亮）  
> **对白资产（用户指定真源）**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab`  
> **产品目标（白话）**：玩家在商店 **点老板娘头部** → 播 **`Village_ShopHead`** 这段对话（店/雅表情照常变）  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0829/Village_Shop_Head热区安装Village_ShopHead对话_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

> 现在就用 **`Village_ShopHead.prefab`** 接到商人 **Head** 交互上。  
> 点头应播这段图；不要再假设磁盘上一定有 `Village_ShopKeeper_HeadClick.prefab`（0828 报告/编辑器菜单写的是这个旧名，**用户现网资产名已是 ShopHead**）。

| 项 | 期望 |
|----|------|
| 交互点 | Hierarchy：`…/MerchantPainting/Trigger/Head` |
| 对白 Prefab | **`Village_ShopHead`**（路径见上） |
| Idle 可点 | 非对白中点 Head → 启动对白 |
| 对白中 | 禁止叠第二段；藏买卖 UI（对齐现网特殊交互） |
| 表情 | 店句走 Face1～5 + BodyType + Registry/Mask；雅走 DialogueFaceType |
| 本期范围 | **仅点头线闭环**；点胸 / Chest Prefab **可只挂钩、不扩范围** |

### 与 0828 已施工资产的关系（关键缺口假说）

0828 已落地（侦探须用磁盘证伪，勿当未做）：

| 层 | 预扫假说 | 侦探要答 |
|----|----------|----------|
| 热区 | 场景已有 `Trigger/Head` + `ShopkeeperBodyHotspot`（`hotspotKind=Head`）+ Collider2D | Head 组件是否齐？Physics2DRaycaster 是否在？ |
| GSM | `TryTriggerShopkeeperSpecial` + Hide/Show UI + 热区开关 | 点头是否已走这条？结束是否 Reset 脸（可引用 0828 Face 复位报告，不重做） |
| 常量名 | `ShopkeeperHeadClickStoryName = "Village_ShopKeeper_HeadClick"` | **与用户 Prefab 名 `Village_ShopHead` 是否不一致？** |
| 编辑器菜单 | `ShopkeeperSpecialClickSetupEditor` 仍写 `…_HeadClick.prefab` | 是否生成了空/错路径？与 `Village_ShopHead` 谁是真源？ |
| Prefab 磁盘 | **有** `Village_ShopHead.prefab`；**未见** `Village_ShopKeeper_HeadClick.prefab` | 命名撕裂 → TriggerStory 按名加载会失败？ |
| CSV | 有 `Assets/Dialog/Village_商店点头交互.csv`（H1～H11 · Face/Body 已迁移样） | `Village_ShopHead` 图是否已 Import 该 CSV？文案/表情是否对齐？ |

**核心假说（须裁定）**：

```
点 Head
  → ShopkeeperBodyHotspot
  → GSM.TryTriggerShopkeeperSpecial("Village_ShopKeeper_HeadClick")
  → StoryComponentGSM.TriggerStory(name)
  → ResMgr.LoadAsset(DialoguePath.GetPath(name))
       ✗ 若 name≠Prefab 文件名 → 加载失败 / 无对白
       ✓ 用户资产 = Village_ShopHead.prefab → 故事名应等于 "Village_ShopHead"
```

生活类比：门铃按钮（Head 热区）已经装好，线路也接到「管家」（GSM），但管家按旧门牌去喊「HeadClick 房间」，而真正住着对白的房间门牌是 **`Village_ShopHead`**——本期侦探要钉死：改门牌常量、改 Prefab 改名、还是做别名映射。

### 命名方案候选（侦探必拍板一个）

| 方案 | 做法 | 优点 | 风险 | 倾向（助手预判，可推翻） |
|------|------|------|------|--------------------------|
| **A · 改故事常量/调用名为 `Village_ShopHead`** | GSM 常量、Hotspot 注释、Editor Setup 路径一并改成 `Village_ShopHead` | **尊重用户指定 Prefab**；少动对白图 | 文档/0828 旧名残留；Editor 菜单路径要同步 | **✅ 推荐倾向** |
| **B · Prefab 改名为 `Village_ShopKeeper_HeadClick`** | 磁盘 Rename + 保持代码常量 | 对齐 0828 报告/台本建议名 | 用户已定名 ShopHead；改名易丢 meta/引用 | 仅当 ShopHead 图残缺、HeadClick 才是完整资产时 |
| **C · 别名映射** | Trigger 时把 HeadClick 映射到 ShopHead 路径 | 兼容两名 | 多一层魔法字符串，易再漂 | ❌ 除非有多处硬编码旧名且不能动 |

**产品原话优先**：使用 `Village_ShopHead` → 默认倾向 **A**；若侦探发现 ShopHead 图不可用而别处另有完整 HeadClick，再改口并写清。

### 须核对的 Prefab 完备性（`Village_ShopHead`）

对照 `Village_ShopStart` 最小壳 + 0828 点头报告 §C/§D：

| 检查项 | 期望 |
|--------|------|
| 根名 / DialogueTree 绑定 | 可被 `DialoguePath.GetPath("Village_ShopHead")` 加载 |
| Actor | 至少 **雅尔（Yaer）+ Merchant（老板娘壳）**；点头线一般 **无古莎** |
| 店句 | `UseShopkeeperPortrait=true`；Face/Body 跟句 |
| H3 旁白/动作 | ActionNode（勿当老板娘/雅 Say） |
| 图内容 | 是否等于点头 CSV H1～H11；是否仍是 ShopStart 旧图未清？ |
| 与 CSV | `Village_商店点头交互.csv` 是否已 Import 进该 Prefab / Generated Graph |

### 本期边界（钉死）

| 做 | 不做 |
|----|------|
| ✅ 点 Head → 播 `Village_ShopHead` 闭环 | ❌ 重做首次进店 `Village_ShopStart` |
| ✅ 命名/加载链路拍板 + 最小施工清单 | ❌ 扩 `DialogueFaceType` / Smile 旧键 |
| ✅ 表情是否跟句（合层+Mask）核对 | ❌ 本期强制做完点胸 / 树屋 C6+ |
| ✅ 对白结束 Idle 是否 Reset 脸（可引用 0828 复位报告） | ❌ 在 Update 轮询点击 |
| ✅ Editor Setup 路径与常量是否要改 | ❌ 并进 ShopStart 同一棵图 |

### 须对拍的现成资产 / 报告

| 资产 | 用途 |
|------|------|
| `Village_ShopHead.prefab` | **本期对白真源** |
| `Village_商店点头交互.csv` | 台本/表情列对照 |
| `ShopkeeperBodyHotspot.cs` | Head 点击入口 |
| `Village_ShopSceneManager.cs` | `ShopkeeperHeadClickStoryName` / `TryTriggerShopkeeperSpecial` |
| `StoryComponentGSM.TriggerStory` + `DialoguePath` | 名字如何解析成 Prefab 路径 |
| `ShopkeeperSpecialClickSetupEditor.cs` | 旧 HeadClick 路径是否误导 |
| `0828/…Trigger特殊交互对话_架构溯源报告.md` | 点击方案 B、状态机、迁移表 |
| `0828/…商人默认Face1Normal与Body_YinXian_架构溯源报告.md` | 结束是否 ResetDefault |
| `0601/Village_商店老板娘特殊交互_对白台本_执行说明.md` | 文案语义真源 |
| `Village_ShopStart.prefab` | Actor 壳对照样板 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 未核实就断定「已接好」或「必须新建整套点头系统」  
- 把点头并进 `Village_ShopStart`  
- 店句走 Smile/Angry 进 `DialogueFaceType`  
- 本期范围扩到 Chest 完整施工（除非发现 Head 接线必须顺带改共享常量文档说明）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md
@Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md
@Assets/Doc/执行文档/0828/Village_Shop_商人默认Face1Normal与Body_YinXian_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_ShopStart_新建Merchant对话NPC_架构溯源报告.md
@Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Dialog/Village_商店点头交互.csv
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/ShopkeeperBodyHotspot.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/StoryComponentGSM.cs
@Assets/Editor/Tool/Dialogue/ShopkeeperSpecialClickSetupEditor.cs
@Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、台本。只读扫描 + 写「Head 安装 Village_ShopHead」溯源报告。

---

## 背景（策划白话）

1. 用户 Hierarchy 里商人立绘下已有 **`Trigger/Head`**（点头热区）。  
2. 对白要用现成 Prefab：**`Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab`**。  
3. 0828 曾规划故事名 `Village_ShopKeeper_HeadClick`，与现网 Prefab 名可能不一致。  
4. 本阶段 **不出施工**，只摸清：点 Head 现在触发了什么名字、为何播不出 / 会播错、怎样最小改动能让 `Village_ShopHead` 真正播起来，以及 Prefab/CSV/表情是否已齐。

---

## 侦探任务清单

### A. 钉死 Head 热区现状（场景 YAML）

| 项 | 填 |
|----|-----|
| `Trigger/Head` 组件 | Collider2D？`ShopkeeperBodyHotspot`？`hotspotKind`？ |
| Main Camera | 有无 `Physics2DRaycaster`？ |
| 点击后实际传入的 story 名 | 常量值？ |
| Idle / 对白中 / 首次进店中 | 热区是否可点、是否被 GSM 关掉？ |

### B. 钉死「故事名 ↔ Prefab 路径」链路（P0）

1. `StoryComponentGSM.TriggerStory` → `DialoguePath.GetPath` 规则：是否 **严格等于 Prefab 文件名（无扩展名）**？  
2. 现网常量 `ShopkeeperHeadClickStoryName` 的值是什么？  
3. 磁盘是否存在 `Village_ShopKeeper_HeadClick.prefab`？是否只存在 `Village_ShopHead.prefab`？  
4. **拍板命名方案 A / B / C**（见预梳理表），写清推荐理由与预期 diff 文件。  
5. Editor Setup 菜单若仍写旧路径：施工时是否必须同步改？

### C. 钉死 `Village_ShopHead.prefab` 内容是否「可装即用」

| 检查 | 结论 |
|------|------|
| Actor：Yaer / Merchant 是否齐？多余 Gusha？ | |
| 图节点：是否点头 H1～H11？还是空壳/ShopStart 残留？ | |
| 店句 `UseShopkeeperPortrait` + ShopFace/ShopBody | |
| H3 是否 ActionNode | |
| 与 `Village_商店点头交互.csv` 是否一致（文案+Face+Body） | |
| Mask / 合层表情：跟句是否已走现网桥（无需新桥？） | |

若 Prefab **残缺**：施工清单写「补 Import / 补 Actor / 清旧图」，**不要**建议重新发明点击架构。  
若 Prefab **已齐**：施工应是「改名对齐 + 验收」，改动量钉死。

### D. 运行时闭环（状态机核对，可引用 0828）

```
Idle（UI_Shop ON，热区 ON）
  → 点 Head → TryTriggerShopkeeperSpecial(最终故事名)
       → Hide UI、热区 OFF、HasRunningStory
  → 播 Village_ShopHead（店脸/雅脸跟句）
  → onStoryEnd → Show UI、热区 ON、（？）ResetDefault
```

回答：

- 现网特殊交互结束回调是否已接？缺的是否只是故事名？  
- 对白结束是否应 `ShopkeeperFaceRegistry.ResetDefault()`（对齐 0828 默认脸报告；若已施工注明「已有则勿重复」）？

### E. 最小施工清单（给施工员，本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | 故事名 ↔ `Village_ShopHead` 对齐（按拍板方案） | | **P0** |
| 2 | Prefab/CSV/Actor/H3 缺口（若有） | | **P0** |
| 3 | Editor Setup 路径/常量同步 | | P1 |
| 4 | 结束 ResetDefault（若仍缺） | | P1（可引用既有清单） |
| 5 | 文档/注释旧名清理 | | P2 |
| 6 | 点胸线 | | **本期排除**（仅注明共享常量勿误伤） |

**排除**：新建第二套热区；并进 ShopStart；扩 DialogueFaceType；重做首次进店。

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进店 Idle 点 Head | 成功 `TriggerStory` **Village_ShopHead**；Console 有 started=true |
| 2 | 对白过程 | 店合层+Mask 随句变；雅脸变；UI_Shop 隐藏 |
| 3 | 对白中再点 Head/Chest | 不开第二段 |
| 4 | 对白结束 | UI 恢复；可买卖；Idle 脸身符合产品默认（Face1+Normal，若已拍板复位） |
| 5 | Console | 无 Missing Prefab / Missing Actor / Face 校验失败 / NRE |
| 6 | （对照）勿要求存在 `Village_ShopKeeper_HeadClick`（若方案 A） | |

### G. 开放问题（写入报告；不清则追加 OPEN_QUESTIONS）

- 对外故事名最终以 `Village_ShopHead` 还是旧 `…_HeadClick` 为准？（产品已偏前者）  
- `Village_ShopHead` 图若与 CSV 不一致，以 Prefab 图为准还是以 CSV 再 Import 覆盖？  
- 点胸常量/Prefab 是否仍用 `…_ChestClick`（本期不施工，但改共享工具时勿误改）？  
- Trigger 是否需写回 `MerchantPainting.prefab`（场景已有则 P2）？

---

## 输出要求

写入：`Assets/Doc/执行文档/0829/Village_Shop_Head热区安装Village_ShopHead对话_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（命名方案 A/B/C + Prefab 是否可装即用 + 最小缺口是什么）  
② 原因（通俗：门牌名 vs 房间名、热区是否已好、图是否齐）  
③ 用户检查清单（Hierarchy / Play 点头 / Console 故事名）  
④ 给程序：加载链路表 + Prefab 完备表 + 最小文件 diff + 开放问题

口头汇报同样用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_Shop_Head热区安装Village_ShopHead对话_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/ShopkeeperBodyHotspot.cs

你现在是【施工员】。只按报告把「点 Head → 播 Village_ShopHead」最小闭环接上。

必须遵守：
- 故事名与 Prefab 文件名按报告拍板方案对齐；
- 不重做热区点击架构（已有 ShopkeeperBodyHotspot / 方案 B 则复用）；
- 禁止并进 Village_ShopStart；禁止扩 DialogueFaceType；
- 点胸仅当报告要求「改共享常量时勿误伤」则保持不动；
- 禁止 Update 堆业务；代码含详细注释；重要取舍写清原因。

提交说明：改了哪些文件、点头如何验收、未做项（若有）。
```
