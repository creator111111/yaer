# Cursor Agent Prompt · 对话框小表情首句未跟台本 FaceType（大立绘对、小头像 Smile）

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-04  
> **范围**：字幕条 / Mask **小表情**（对话框左侧头像）首句不跟 `FaceType`；场景**大立绘**正常。不扩成新表情枚举、不改 CSV 台本内容（除非证实资产未写入）。  
> **本阶段**：只摸清根因与改动面，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 复现（已对齐截图 + CSV）

| 项 | 值 |
|----|-----|
| 场景/对白 | 村内开场「好漂亮的村子。」（CSV 第 1 行） |
| 台本 | `Assets/Dialog/Village_村内雅古开场对白台本.csv` → `FaceType=Laugh` |
| 大立绘（场景） | **Laugh**（正确） |
| 小表情（对话框左） | **Smile**（错误，与大立绘对不上） |
| 后续句 | 用户描述为「第一句话」有问题；侦探须确认第 2 句起小表情是否已正常，缩小到「仅首句」还是「Mask 永远默认 Smile」 |

### 高度可疑根因（优先验证，可证伪）

现网 Mask 路径大致是：

`DialogueTMPUGUI` → `OnGetNewStatement` → `DialogueMaskAvatarPresenter.Apply`  
→ `painting.SetActive(true)` → `painting.UpdateFace(ResolveFaceKey(..., Laugh))`

而 `GoOutStoryYaerPainting.SetDefaultPainting()` **无条件** `UpdateFace("Armor_NoHeadWear_Smile")`。

**竞态假说**：Mask 内 `GoOutStoryYaerPainting` 对话前被 Presenter `HideAll` 关掉；首句 `SetActive(true)` 后，Unity 会在稍后跑首次 `Start` → `SetDefaultPainting` → **把刚设好的 Laugh 盖回 Smile**。  
场景大立绘一直 Active，`Start` 早在开场前跑完，首句 Actor 事件切 Laugh 后不再被默认逻辑覆盖 → 两边不一致。

替代假说（须并列排查，勿只认一条）：

1. 玩家看到的「小表情」其实是旧 `Portrait` Image + `DialogueAvatarLoader`（图集），首句未刷 / 默认 Smile，Mask 另路正常但被盖住或未显。  
2. `OnGetNewStatement` 首句未发、订阅过晚（OnEnable 晚于第一句）。  
3. CSV/生成资产首句 FaceType 未进 `StatementNodeEx`（导入漏列），大立绘却另有来源——须核对 Generated `.asset` 第 1 节点。  
4. Prefab 默认亮着 Smile 脸，Presenter 未对首句调 `UpdateFace`。

### 对照文档

- `Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`
- `Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`
- `Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md`
- `Assets/Dialog/Village_村内雅古开场对白台本.csv`（行 1：`Laugh`）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 对话**第一句**时，对话框里的**小表情**不用台本设定的脸，和大立绘对不上。
2. 截图证据：第一句「好漂亮的村子。」——大立绘是 **Laugh**，小表情是 **Smile**。
3. CSV 该行 FaceType 已写 **`Laugh`**（不是空列默认 Smile）。
4. 目标：摸清小表情首句为何掉成 Smile，给出**最小修复**施工建议；大立绘链路已正常则不要顺手重写大立绘。

---

## 必读 / 优先扫描线索

### A. 钉死「小表情」到底是哪块 UI
- `NormalDialogueNewPanel`：`Mask/YaerAvatarRoot` + `DialogueMaskAvatarPresenter` vs 旧 `Portrait`/`actorPortrait` Image
- `DialogueTMPUGUI`：`useMaskAvatar`（或等价开关）、`OnGetAvatar`、`OnGetNewStatement` 调用顺序
- 截图里带 gizmo 选中的是否 Mask 内 GoOut，还是旧 Image——写死路径

### B. 首句事件时序（本期主线）
- `DialogueTMPUGUI` 处理首句 `SubtitlesRequestInfoEx`：何时 `RefreshAvatar`、何时 `OnGetNewStatement?.Invoke(role, faceType, text)`
- `DialogueMaskAvatarPresenter`：Awake `HideAll` → OnEnable 订阅 → `Apply` → `SetActive(true)` → `UpdateFace`
- `StoryFormPainting` / `GoOutStoryYaerPainting`：`Start` / `SetDefaultPainting` / `UpdateFace` 相对 `SetActive` 的顺序
- **重点验证竞态假说**：首次激活是否在 `UpdateFace(Laugh)` 之后又执行 `SetDefaultPainting`→Smile
- 对比场景大立绘实例：为何不受影响（一直 Active？订的是 Actor 事件？）

### C. FaceType 是否真传到小表情
- 首句 `info.FaceType` 是否为 `Laugh`（NodeCanvas 节点、Debug 日志点建议）
- `ResolveFaceKey` / `GoOutStoryYaerPainting.ResolveGoOutFaceKey`：Laugh → `Armor_NoHeadWear_Laugh` 是否正确（非 Normal 回退 Smile）
- `DialogueFaceTypeCsvDefaults`：空列才 Smile；本行有 Laugh，不应走默认——但若资产未写入仍会 Smile

### D. Prefab / 默认脸
- Mask 内 `GoOutStoryYaerPainting` Faces 默认 Active 哪张；母 Prefab 是否 Smile 默认亮
- Presenter `yaerUseGoOutOnly` 是否导致走错 Painting

### E. 复现边界
- 仅第一句？跳到第二句再回来？重开对话？DialogDebug 是否同现？
- 换说话人首句（古莎 Normal/Smile）是否同样被默认脸盖掉？

---

## 侦探任务清单

1. **结论必须二选一钉死（或组合）**  
   - 小表情控件身份：Mask 立绘 / 旧图集 Portrait / 两者叠着。  
   - 首句 Smile 的直接写入点：`SetDefaultPainting` / Loader 默认 / Prefab 默认 Active / FaceType 未传入 / 其它。

2. **画出「第一句」时序图**（大白话 + 脚本锚点）  
   - 从 SayEx/字幕请求 → TMP → Presenter → Painting.Start/UpdateFace  
   - 标出大立绘分叉点（为何它对、小的错）。

3. **缺口 / 根因表**

   | 环节 | 现状 | 是否首句特有 | 是否必须改代码 | 备注 |
   |------|------|--------------|----------------|------|
   | CSV FaceType=Laugh | | | | |
   | StatementNodeEx 首节点 | | | | |
   | OnGetNewStatement 参数 | | | | |
   | Presenter.Apply | | | | |
   | SetActive→Start→SetDefaultPainting | | | | 竞态假说 |
   | UpdateFace(Laugh) | | | | |
   | 旧 Portrait Loader | | | | |
   | 场景大立绘 | | 对照 | | 正常参考 |

4. **施工员最小改动建议**（只建议，不施工）  
   - 优先：避免「首次激活默认 Smile 覆盖台本脸」（例如默认脸仅在无语句时用；或 Presenter 在 Start 之后再 Apply；或 SetDefault 不在 Mask 实例上强制 Smile；或首句 Apply 延后一帧——写清推荐与风险）。  
   - 禁止：改 Faces 中文名；在 Update 扫对话树；重写整棵 DialogueForm。  
   - 若证实是资产未带 FaceType：写清是否需重新 Import CSV（工具用法点到为止）。

5. **验收清单**  
   - 村内开场第一句：小表情与大立绘均为 **Laugh**。  
   - 第二句古莎、再切回雅儿：小表情跟当前句 FaceType。  
   - 重开同一 DialogueTree 再验首句。  
   - DialogDebug 若可复现则加一条。

6. **开放问题**追加 `Assets/Doc/OPEN_QUESTIONS.md`（新开一节「对话框小表情首句 · 2026-08-04」）：  
   - Mask 实例是否应禁用/弱化 `SetDefaultPainting`？  
   - 旧 Portrait 是否完全关闭？  
   - 是否所有角色首句都有同类竞态？

7. **禁止**：改资产；把「CSV 写错」当结论若文件已是 Laugh；扩大到新表情资源补齐。

---

## 输出要求

写入：`Assets/Doc/执行文档/0804/对话框小表情_首句未跟FaceType_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（根因一句话 + 小表情是 Mask 还是 Portrait）  
② 原因（生活类比 + 时序/脚本锚点）  
③ 用户需要做什么（验收清单；是否要重导 CSV）  
④ 给程序看的补充：时序图、根因表、最小改动建议、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认根因（尤其是 Start/SetDefaultPainting 竞态 vs Portrait 双源）后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0804/对话框小表情_首句未跟FaceType_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使对话第一句小表情与台本 FaceType、大立绘一致。
优先修时序/默认脸覆盖；禁止改 Faces 节点名为中文；禁止在 Update 堆业务。
每次提交说明：改了哪些文件、实现了什么、如何验证（村内开场第一句 Laugh）。
```
