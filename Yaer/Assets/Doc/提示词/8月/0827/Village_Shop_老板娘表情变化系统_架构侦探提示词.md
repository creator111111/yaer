# Cursor Agent Prompt · Village_Shop：老板娘表情变化系统（先表情、后首次进店对白）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-27  
> **场景 / UI**：`Village_Shop`（纯 UI）· 进店门 `Door_Shop`  
> **美术源**：`Assets/ArtRes/Scene/Village/商店界面合层/`（用户截图：`表情1`～`表情5` 已就绪）  
> **本阶段范围**：**只做老板娘表情（+ 如需的身体变体）切换系统**；**不做**「首次进店整段对白触发 / 存档只播一次 / 对白期藏商店 UI / 黑屏后再出柜」  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / 台本

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话 · 分两期）

| 期 | 内容 | 本期？ |
|----|------|--------|
| **① 表情系统** | 对话进行时，老板娘脸（及必要时身体）能按台本切到对应图；现有 **5 种表情** | **✅ 本期** |
| ② 首次进店演出 | 玩家**第一次**进店 → 播与老板娘的特殊对白 → 表情跟句变 → 结束后再进买卖 UI | ❌ **下期**（本报告只预留挂钩点） |

用户原话对齐：**「现在先做这个表情变化系统」**。

### 美术资源盘点（磁盘 · 2026-08-27）

路径：`Assets/ArtRes/Scene/Village/商店界面合层/`

| 资源 | 文件 | 预扫用途假说 |
|------|------|--------------|
| 表情脸 ×5 | `表情1.png` … `表情5.png` | **本期主对象**：按对话切脸 |
| 身体 · 常态 | `正常体.png` | 默认身体 |
| 身体 · 变体 | `脸红体.png`、`阴险体.png` | 可能是「换身」而非换脸；侦探裁定：本期是否纳入表情系统 API |
| 背景 | `背景.png`、`背景_2.png` | 与表情无关，勿动 |
| UI 控件图 | `组 7/组 6/*` | 买卖 UI，勿当表情 |

> 用户截图选中 `表情1`；同目录另有 `表情2`～`5`、`正常体`、`脸红体`、`阴险体`。

### 现网载体对照（预扫）

| 载体 | 现状（预扫） | 备注 |
|------|--------------|------|
| 美术合层 Prefab | `Assets/ArtRes/Scene/Village/商店界面合层.prefab` | 树内仅见 **`正常体` + `表情1`**；`表情2～5` / `脸红体` / `阴险体` **多半未挂进 Hierarchy** |
| 正式 UI Prefab | `Assets/GameRes/Prefabs/UI/ShopPanel.prefab` | 已有 `ShopkeeperLayer` / `ImgBody` / `ImgFace`（0704 预留「后续表情脚本」） |
| 进店真源场景 | `Village_Shop.unity` → 主路径多为 **`UI_Shop`**（0713 后进店不走 OpenUIForm） | 侦探须钉死：**表情切在 `UI_Shop` 还是 `ShopPanel`，或两者都要** |
| Shop 脚本 | `ShopFormLogic` 等 | **无** ImgFace / SetFace 相关 API（预扫） |
| 对话角色枚举 | `DialogueRoleName` | **无老板娘**（0601 台本已写明缺口） |
| 通用表情枚举 | `DialogueFaceType` | 雅/古莎等用；老板娘 5 表情 **是否复用枚举名** 待裁定 |

### 与「雅儿/古莎立绘表情」的边界（钉死）

```
通用对话立绘：
  SayEx / CSV FaceType
    → DialogueFaceType
    → StoryFormPainting.UpdateFace(Faces子物体名)
    →（可选）Mask / 小头像图集

商店老板娘（现网）：
  场景/UI 上的 ImgBody + ImgFace（或合层 Sprite）
  ≠ GoOutStoryYaerPainting 那条链
```

侦探必须在报告里**二选一或给混合方案**，并写清理由：

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A · 复用对话 Painting** | 新增老板娘 `*Painting` + `DialogueRoleName` + Faces 子物体，台本 `FaceType` 驱动 | 与首次进店对白天然一体 | 工程大；商店 UI 立绘与对话立绘可能双份 |
| **B · 商店专用切脸器（推荐倾向）** | `ShopkeeperFaceController`（名待定）挂 `ShopkeeperLayer`，对外 `SetFace(id/enum)`；**先**可被 Debug/按钮测，**后**再订对话事件 | 贴合现有 `ImgFace`；本期可最小闭环 | 若下期对白走 Actor 事件，需再做一层桥接 |
| **C · 只换 Sprite 不建系统** | 手改 Image.sprite | ❌ 拒绝（临时修补） | — |

### 下期挂钩（本期只写清接口，不实现）

首次进店（0629 / 0601 文档）预期：

```
首次进店标记 → 藏买卖 UI → 播对白
  →【调用本期表情 API】老板娘随句变脸
  → 对白结束 → 黑屏 → 出商店 UI
```

本期报告须留下：**表情 API 签名建议** + **谁在下期订阅 `OnGetNewStatement` / SayEx**，但**禁止**本期做存档旗标与整段演出。

### 勿混需求

| 勿当成一期 | 文档 |
|------------|------|
| 点头/点胸特殊交互对白 | `0601/Village_商店老板娘特殊交互_对白台本_执行说明.md` |
| 首次进店完整演出 + 存档只播一次 | `0629/商店系统_策划拆解_执行说明.md` §4 |
| 合层整页迁 UI（已部分完成） | `0704/商店界面合层转UI组件_…` |
| 雅儿/古莎 `DialogueFaceType` 扩枚举 | 0803/0804 表情提示词 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 把本期做成「首次进店全流程」施工清单却不做表情最小闭环  
- 强行把老板娘 5 表情塞进雅儿图集键名  
- 在 `Update` 里轮询切脸  
- 未分清 `UI_Shop`（进店真源）与 `ShopPanel.prefab`（镜像）就断定「改一处即可」

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/7月/0704/商店界面合层转UI组件_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/7月/0713/Village_KenMuNi1_Door_Shop换场Village_Shop纯UI_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md
@Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md
@Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md
@Assets/Doc/提示词/0803/雅儿立绘新增Happy表情_接入表情系统_架构侦探提示词.md
@Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs
@Assets/Scripts/Game/Static/Enum/Role/RoleName.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/StoryFormPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/ArtRes/Scene/Village/商店界面合层.prefab
@Assets/ArtRes/Scene/Village/商店界面合层/
@Assets/GameRes/Prefabs/UI/ShopPanel.prefab
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图、台本。只读扫描 + 写「老板娘表情系统」溯源报告。

---

## 背景（策划白话）

1. 后续要做：玩家**第一次进商店**时，和**老板娘**播一段特殊对话，对话过程中老板娘表情要变。  
2. 美术已给 **5 种表情**（`表情1`～`表情5`），另有身体图 `正常体` / `脸红体` / `阴险体`。  
3. **本期只先做「表情变化系统」**：能按指令切到 5 表情之一（及裁定是否含换身）；先可 Debug 验收，再留给下期对白驱动。  
4. 本阶段**不要施工**，只出方案与最小改动面。

---

## 侦探任务清单

### A. 钉死「脸画在哪」

| 项 | 填 |
|----|-----|
| 进店 Play 时玩家看见的老板娘，来自哪个 GO？ | `UI_Shop` / `ShopPanel` 实例 / 合层 Prefab / 其它 |
| `ImgBody` / `ImgFace` 是否已绑默认 Sprite？ | |
| `表情2～5`、`脸红体`、`阴险体` 是否已进 Prefab Hierarchy？ | |
| 合层 Prefab 与 UI Prefab 是否双份、是否需双写？ | |

### B. 5 表情 + 身体变体语义表（必出）

向用户/策划可暂用占位名，但报告须给**建议枚举/ID**：

| ID | 源图 | 建议内部名（英文） | 是换脸还是换身 | 默认？ |
|----|------|-------------------|---------------|--------|
| 1 | 表情1 | | Face | |
| 2 | 表情2 | | Face | |
| 3 | 表情3 | | Face | |
| 4 | 表情4 | | Face | |
| 5 | 表情5 | | Face | |
| ? | 正常体 / 脸红体 / 阴险体 | | Body？ | |

开放问题：策划是否为 表情1～5 提供正式英文 FaceType（Smile/Angry…）？若无，本期可用 `ShopFace1`…`ShopFace5`。

### C. 架构选型（必填推荐）

在 **A / B / C**（见预梳理）中拍板推荐方案，并回答：

1. 是否新增 `DialogueRoleName`（老板娘）？**本期是否必须？**（倾向：表情系统可先不绑 Role，下期对白再加）  
2. 是否扩展 `DialogueFaceType`？还是独立 `ShopkeeperFaceType` / int？  
3. 切脸实现：换 `Image.sprite` 列表 vs `StoryFormPainting` 多子物体 Active？  
4. API 建议（示例，可改）：`SetFace(...)` / `SetBody(...)` / `ResetDefault()`  
5. 谁持有引用：挂在 `ShopFormLogic`？独立组件？`Village_ShopSceneManager`？

### D. 与对话系统的「下期桥接」设计（只设计不实现）

画清：

```
SayEx(老板娘, FaceX) 或 OnGetNewStatement
  → ??? 桥接 ???
  → 本期 SetFace
```

要求：本期表情系统**即使没有对话也能单测**；下期只加订阅，不推翻 API。

### E. 最小施工清单（给施工员，本阶段不执行）

| # | 文件/物体 | 动作 | 优先级 |
|---|-----------|------|--------|
| | | 挂组件 / 拖 5 张脸 Sprite / Debug 热键或临时按钮 | P0 |
| | | 是否同步改 `UI_Shop` 场景实例与 `ShopPanel.prefab` | |
| | | 文档：表情对照表 | P2 |

**明确排除**：首次进店旗标、对白 Prefab/CSV、藏 UI、黑屏。

### F. 验收清单（表情系统单独可验）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进 `Village_Shop`（经 Door_Shop 或直接开场景） | 看见老板娘默认脸/身 |
| 2 | 调 Debug：切 表情1→5 | ImgFace（或等价层）图正确切换，无花屏/错位 |
| 3 | （若含换身）切 正常体/脸红体/阴险体 | 身体层对，脸层仍在 |
| 4 | 买卖 UI 仍可点 | 表情系统不挡 Raycast、不打断 ShopFormLogic |
| 5 | Console | 无 Missing Sprite / NRE |

### G. 开放问题（写入报告；必要时追加 OPEN_QUESTIONS）

- 表情1～5 的正式语义名与台本 FaceType 对照？  
- `脸红体`/`阴险体` 是否本期 API 必做？  
- 首次进店对白里左侧女主立绘是否仍走通用 Painting（与老板娘商店层并存）？  
- 老板娘是否需要对话框**小头像**（本期默认否）？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/Village_Shop_老板娘表情变化系统_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（推荐方案 A/B/C + 本期能否独立验收）  
② 原因（通俗：商店脸 ≠ 雅儿 Faces 链；现挂在哪）  
③ 用户检查清单  
④ 给程序：语义表 + API + 最小文件清单 + 下期桥接点 + 开放问题

口头汇报同样用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘表情变化系统_架构溯源报告.md
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Prefabs/UI/ShopPanel.prefab
@Assets/ArtRes/Scene/Village/商店界面合层/

你现在是【施工员】。只按报告实现「老板娘表情变化系统」最小闭环。

必须遵守：
- 不做首次进店旗标 / 整段对白 / 藏商店 UI / 黑屏；
- 优先报告推荐的专用切脸方案；禁止 Update 堆业务；
- 进店真源（UI_Shop）与 ShopPanel 镜像若报告要求双写则双写，否则只改真源并注明；
- 提供可验收的 Debug 切脸方式（临时按钮或菜单，验收后可留）；
- 代码含详细注释；重要取舍在提交说明写清原因。

提交说明：改了哪些文件、5 表情如何验收、下期对白如何接线（一两句）。
```
