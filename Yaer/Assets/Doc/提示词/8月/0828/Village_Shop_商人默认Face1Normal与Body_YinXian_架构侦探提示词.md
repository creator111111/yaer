# Cursor Agent Prompt · Village_Shop：商人默认 Face1 + Body Normal，并确认/补齐 Body·YinXian

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-28  
> **场景 / 载体**：`Village_Shop` · `商店界面合层` → `MerchantPainting`（大立绘）· `MerchantMaskPainting`（对话框小表情）  
> **产品目标（白话）**：  
> 1. 商人**默认表情**固定为 **`Face1`**  
> 2. 商人**默认身体**固定为 **`Normal`**  
> 3. Body **必须具备并可切换**状态 **`YinXian`**（阴险身；与 Normal / Red 并列）  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0828/Village_Shop_商人默认Face1Normal与Body_YinXian_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 项 | 产品设定 | 备注 |
|----|----------|------|
| 默认脸 | **`Face1`** | 进店 Idle、对白未指定脸、对白/特殊交互结束后回默认，均应落到 Face1 |
| 默认身 | **`Normal`** | 同上；CSV `BodyType` 空列也应等价 Normal |
| Body 状态集 | **`Normal` / `Red` / `YinXian`** | 用户强调 **新增/具备 YinXian**；侦探须核对是否「代码有、Prefab 缺」或「有节点但切不过去」 |

**本期不做**：点头/点胸新台本、首次进店演出重做、扩 `DialogueFaceType`、改雅/古表情。

### 磁盘预扫（2026-08-28 · 假说，须证伪）

| 层 | 预扫现状 | 侦探任务 |
|----|----------|----------|
| 枚举 | `ShopkeeperFaceType.Face1=0`；`ShopkeeperBodyType.Normal=0`，`Sinister`→GO名 **`YinXian`**，另有 `Blush`→`Red` | 是否与产品语义一致；CSV 写 `YinXian` 还是 `Sinister`？ |
| API | `ShopkeeperFaceController.ResetDefault()` = **Normal + Face1**；已 `RegisterBodyNode(…, "YinXian")` | `Awake`/`Start`/对白结束是否真调用？会否被首句盖掉？ |
| Mask | `MerchantMaskPainting` 同构 `ResetDefault` + YinXian 注册 | 与合层是否双轨一致 |
| Prefab | `MerchantPainting.prefab` / `MerchantMaskPainting.prefab` 树内**似已有** `Body/YinXian`、`Face/Face1` | 默认 **Active** 是否只有 Normal+Face1？YinXian 是否 inactive 待命？ |
| 场景 | `Village_Shop.unity` 合层下亦见 `YinXian` 名 | 场景 override 是否把默认脸/身改成了别的？ |
| CSV | 店行 `BodyType` 允许 `Normal/Red/YinXian`；空=？ | GraphBuilder 空列是否落到 Normal+Face1 |

> **推论**：若代码与 Prefab「看起来已有」，本期很可能是 **默认 Active / 进店复位 / 对白结束回默认 / Mask 与合层不一致 / YinXian 切失败** 类缺口，而不是从零加枚举。侦探必须用表证伪，禁止想当然写「已完成无需施工」。

### 须对齐的「默认」时机（必查）

```
T0 进 Village_Shop（含二进宫）     → 应为 Normal + Face1
T1 首次进店 / 点头胸 对白进行中   → 跟 CSV Body/Face（可 YinXian）
T2 对白结束回 Idle               → 应回 Normal + Face1？（产品倾向：是；侦探确认现网有无 Reset）
T3 Debug 热键切身后              → ResetDefault / 再进店是否恢复
```

开放裁定：对白结束是否**强制** `ResetDefault`，还是「停在最后一句的脸」？  
**产品本次原话只钉 Idle 默认 = Face1+Normal**；结束是否回默认写入报告建议，设计不清则记 `OPEN_QUESTIONS`。

### YinXian 三载体核对（必出表）

| 载体 | 路径 | 有 YinXian GO？ | 默认 Active？ | SetBody(Sinister)/CSV YinXian 能亮？ |
|------|------|-----------------|---------------|--------------------------------------|
| 场景合层大立绘 | `Village_Shop` → `MerchantPainting/Body` | | Normal+Face1？ | |
| SR 参考 Prefab | `MerchantPainting.prefab` | | | |
| UI Mask | `MerchantMaskPainting.prefab` | | | |

缺任一处 → 施工清单写「补节点 / 补 Sprite / 补注册」，勿只改枚举。

### 与 Red / 枚举命名（勿混）

| Hierarchy GO | 枚举 | CSV `BodyType` |
|--------------|------|----------------|
| `Normal` | `ShopkeeperBodyType.Normal` | `Normal`（或空） |
| `Red` | `Blush` | `Red` |
| `YinXian` | `Sinister` | **`YinXian`**（对外名） |

禁止把 YinXian 塞进 `DialogueFaceType`；禁止用换 Sprite 替代子物体 Toggle（现网已拍板 Toggle）。

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 重做点头胸 / 首次进店全流程  
- 未核对三载体就断定「YinXian 要新建枚举」或「已全部就绪」  
- 在 `Update` 里轮询默认脸  

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md
@Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店第二波遗留_架构溯源报告.md
@Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceRegistry.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperBodyType.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceType.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/MerchantMaskPainting.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceDebugInput.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvParser.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs
@Assets/Editor/Tool/Dialogue/ShopkeeperCsvDefaults.cs
@Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab
@Assets/Prefabs/DialougeProtrait/MerchantMaskPainting.prefab
@Assets/ArtRes/Scene/Village/商店界面合层.prefab
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写溯源报告（执行文档）。

---

## 背景（策划白话）

商人（老板娘）立绘要满足：

1. **默认表情 = Face1**  
2. **默认 Body = Normal**  
3. **Body 要有 YinXian 状态**（可被台本/调试切到阴险身）

请摸清：现网代码/Prefab/场景是否已对齐；缺口在默认 Active、进店/结束复位、还是 YinXian 节点/注册/CSV；给出最小施工面。

---

## 侦探任务清单

### A. 默认态真源表（必出）

| 检查点 | 期望 | 现网 | 差距 |
|--------|------|------|------|
| 枚举默认值 Face1 / Normal | ✅ | | |
| `ResetDefault()` 实现 | Normal+Face1 | | |
| 场景合层进 Play 首帧 Active | 仅 Normal+Face1 | | |
| MerchantPainting Prefab 序列化 Active | 同上 | | |
| MerchantMaskPainting 默认 Active | 同上 | | |
| CSV 空 Body/Face 导入落点 | Normal+Face1 | | |

### B. YinXian 完备性（必出）

1. 三载体是否都有 `Body/YinXian` 子物体与正确 Sprite/Image？  
2. `ShopkeeperFaceController` / `MerchantMaskPainting` 注册表是否都能 `Apply(Sinister, …)`？  
3. Debug 输入或 CSV 样例是否已覆盖 YinXian？缺则施工要补哪？  
4. 若用户说「新增」：是 **Hierarchy 缺节点**、**Mask 缺**、**合层 Prefab 与场景不同步**，还是 **仅文档/台本未用过**？

### C. 生命周期：何时回到默认？

画清现网调用链：

```
进店 / 开合层
  → ? ResetDefault / 仅靠 Prefab Active
对白句 Apply(body, face)
对白结束 / 特殊交互结束
  → ? ResetDefault
关店离场
  → ?
```

给出推荐（最小改动）：

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A** | Prefab/场景序列化保证默认；对白结束显式 `ResetDefault` | Idle 观感稳定 | 可能闪一下最后一句脸 |
| **B** | 对白结束保持末句脸，仅进店/首次 Awake 默认 | 少一次切 | 二进宫可能仍是阴险身 |
| **C** | 仅 Editor 校正，运行时不 Reset | ❌ 易漂 | — |

**产品原话优先保证 Idle 默认**；推荐倾向写清并标开放问题。

### D. 与现网对白/点头胸的关系（只挂钩）

- 店句 CSV `BodyType=YinXian` 是否已能驱动合层+Mask？  
- 点头/点胸 CSV 若需阴险身，是否只需改表、无需改架构？  
- **禁止**本期改特殊交互触发逻辑，除非默认复位必须挂在结束回调上（写清挂点文件）。

### E. 最小施工清单（本阶段不执行）

| # | 文件/物体 | 动作 | 优先级 |
|---|-----------|------|--------|
| | 场景/Prefab 默认 Active 校正为 Normal+Face1 | | P0 |
| | 缺则补 `Body/YinXian`（合层/Mask/参考 Prefab 同步） | | P0 |
| | 确认 Register + CSV 校验含 YinXian | | P0 |
| | 进店或对白结束 `ResetDefault`（按方案 A/B） | | P1 |
| | 短技术说明 / 对照表更新 | | P2 |

**明确排除**：新对话剧情、改 Face1～5 语义、扩 DialogueFaceType。

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进 `Village_Shop` Idle | 大立绘 = Normal + Face1 |
| 2 | Debug 或临时句切到 YinXian + FaceX | 合层身变阴险；脸正确；Mask（若显示）一致 |
| 3 | 切回 / 结束对白后（按报告方案） | Idle 回到 Normal + Face1 |
| 4 | CSV `BodyType=YinXian` 店句 | Import 成功且运行时切换成功 |
| 5 | Console | 无「未找到子 GO YinXian」类 Warning |

### G. 开放问题

- 对白结束是否强制回默认，还是保留末句表情？  
- `商店界面合层.prefab` 与场景实例是否要双写默认 Active？  
- 阴险身是否必须搭配某一固定脸（如 Face3），还是 Body/Face 正交自由组合？

---

## 输出要求

写入：`Assets/Doc/执行文档/0828/Village_Shop_商人默认Face1Normal与Body_YinXian_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（默认是否已对齐；YinXian 是缺资产还是缺复位；推荐方案 A/B）  
② 原因（通俗：代码枚举 ≠ 场景默认 Active；三载体哪里漏）  
③ 用户检查清单（Hierarchy 默认勾选、进店看一眼、切 YinXian 再回 Idle）  
④ 给程序：真源表 + 生命周期 + 最小文件清单 + 开放问题

口头汇报同样用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0828/Village_Shop_商人默认Face1Normal与Body_YinXian_架构溯源报告.md
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab
@Assets/Prefabs/DialougeProtrait/MerchantMaskPainting.prefab

你现在是【施工员】。只按报告实现「默认 Face1 + Normal」与「Body·YinXian 可切换」最小闭环。

必须遵守：
- 默认脸/身以报告为准（Face1 + Normal）；
- YinXian 走现网 Toggle + CSV BodyType，禁止扩 DialogueFaceType、禁止 Update 轮询；
- 合层大立绘与 MerchantMask 若报告要求双写则双写；
- 对白结束是否 Reset 严格按报告方案，不擅自改点头胸/首次进店剧情；
- 代码含详细注释；重要取舍写清原因。

提交说明：改了哪些文件、Idle 默认如何验、YinXian 如何验、未做项（若有）。
```
