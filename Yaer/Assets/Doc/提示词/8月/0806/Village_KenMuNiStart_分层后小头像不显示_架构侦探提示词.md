# Cursor Agent Prompt · Village_KenMuNiStart 分层显现后对话框小头像变黑/不显示

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-06  
> **范围**：分层显现节奏**已正确**；回归 bug——字幕条左侧 **Mask 小头像**不显示（黑窗）。不扩台本、不重做分层产品节奏。  
> **本阶段**：只摸清「谁把小头像弄没了」，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 复现（已对齐截图 · 2026-08-06）

| 项 | 状态 |
|----|------|
| 流程 | 第一章进村自动 `Village_KenMuNiStart` |
| 分层节奏 | **正常**（BG → 对话框 → 大立绘） |
| 场景大立绘 | **正常**（雅儿 + 古莎可见） |
| 对话框正文 | **正常**（首句「好漂亮的村子。」） |
| 字幕条左侧小头像 | **异常**：Mask 窗内全黑空框，无脸 |

→ 不是「整段对话挂了」，是**对话框左侧头像槽**单独挂了。

### 高度可疑根因（优先验证，可证伪）

分层施工把 0804 的 `Snap…Opaque` 改成了 `PrepareVillageStartLayeredReveal()`：按**名字模糊匹配**把对话壳下一批 `CanvasGroup.alpha` 打成 **0**：

```csharp
// Village_KenMuNiSceneManager.PrepareVillageStartLayeredReveal
// 名含 Painting / GoOut / Gusha / Bottom / subtitles / Subtitle → alpha = 0
```

小头像路径（0803 定稿）：

```
NormalDialogueNewPanel/Bottom/Mask/YaerAvatarRoot/GoOutStoryYaerPainting（等）
```

| 误伤点 | 为何像 |
|--------|--------|
| 名含 **`GoOut` / `Painting` / `Gusha`** | Mask 内嵌的也是同名 Painting Prefab，会被一并 alpha=0 |
| 名含 **`Bottom`** | 整条字幕条父级先藏；框淡回后，**子级 Painting 的 CanvasGroup 未必跟着回 1** |
| Prefab 前奏只 Fade **场景大立绘** BB | 不会把 `YaerAvatarRoot` 下 Mask 立绘 alpha 拉回 |
| Presenter `SetActive(true)` + `UpdateFace` | Active 了但父/自身 **CanvasGroup 仍 0** → Mask 窗仍黑 |

**生活类比**：舞台分层时把「话筒旁小显示器」和「台上大演员」当成同名道具一起关灯；大演员后来有专人开灯，小显示器没人开，就一直黑屏。

### 替代假说（须并列排查，勿只认一条）

1. Presenter 首句未 `Apply` / `OnGetNewStatement` 订阅晚于首句（历史 0804 首句 FaceType 竞态同类）。  
2. `useMaskAvatar` 被关掉，旧 `Portrait` Image 也空，Mask 窗裸露黑底。  
3. Prefab 分层改序后，某 Action 误关 `Mask` / `YaerAvatarRoot` Active。  
4. Mask 内 Painting 被 `HideAll` 后未再开（角色解析失败 / None）。  
5. 仅进村旁路复现；DialogDebug 拖同 Prefab 仍正常 → 更坐实 SceneManager Prepare 误伤。

### 对照文档 / 上一轮

- `0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md`（方案 A + 废除 Snap → Prepare）  
- `0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`  
- `技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`  
- `0804/对话框小表情_首句未跟FaceType_…`（小头像链路；本期是**不显示**不是错脸）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md
@Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 开场分层节奏已经对了（先 BG、再对话框、再大立绘）。
2. 但对话框左边的**小头像又不显示了**——黑框空窗（截图：首句「好漂亮的村子。」，大立绘在、字在、头像槽黑）。
3. 高度怀疑是分层准备代码按名字把 Mask 里的 Painting 也 alpha 清零了，后面没人拉回来。
4. 目标：钉死根因 + 最小修复建议；**保住分层节奏与零漏缝**，只修小头像回归。

---

## 必读 / 优先扫描线索

### A. 钉死「黑窗」是哪块 UI（运行时路径）
- Hierarchy 期望：`…/Bottom/Mask/YaerAvatarRoot/GoOutStoryYaerPainting`（或当前说话人对应 Painting）
- 对比旧 `Bottom/Portrait`（actorPortrait）是否仍参与
- `DialogueTMPUGUI.useMaskAvatar`（或等价）现网值
- 黑的是：Mask Graphic、YaerAvatarRoot 空、Painting Active=false、还是 CanvasGroup.alpha=0？

### B. Prepare 误伤清单（本期主线）
读 `PrepareVillageStartLayeredReveal`：
- `GetComponentsInChildren<CanvasGroup>(true)` 扫到的**每一个**命中节点列成表  
- 明确标出 Mask 下哪些被写成 alpha=0  
- Prefab 前奏 / `NormalDialogueUIAlpha` 恢复时，**会不会**把这些节点写回 1？  
- 场景大立绘 BB 与 Mask 内同名 Painting：是否同一次名字匹配误伤

### C. Presenter 时序
- `DialogueMaskAvatarPresenter.Awake` HideAll → 首句 `OnGetNewStatement` → `Apply`  
- 分层亮屏后首句是否仍触发 Apply  
- Apply 只 `SetActive`，**不改 CanvasGroup** → 若 Prepare 已把 alpha 打 0，Apply 无法自愈

### D. 对照实验线索（报告里写清如何验）
| 实验 | 若结果 | 含义 |
|------|--------|------|
| DialogDebug 拖 Village_KenMuNiStart，不走进村旁路 | 小头像正常 | 坐实 SceneManager Prepare / 进村专用 |
| 进村复现时 Hierarchy 看 Mask/GoOut 的 alpha | =0 | 坐实误伤未恢复 |
| 临时跳过 Prepare 名字匹配中的 Painting（只读脑内推演） | 应恢复 | 施工方向 |

### E. 范围冻结
- **保留**：分层三拍、BG 盖景零漏缝、只播一次、台本  
- **可改建议**：Prepare 匹配范围收窄（排除 `Mask`/`YaerAvatarRoot` 子树）；或 Fade 回框时顺带恢复 Mask 内 Painting alpha；或 Presenter.Apply 后强制自身 CanvasGroup=1  
- **禁止**：为修头像关掉分层；重写整棵对话头像系统；改其它无关对话 Prefab（除非证实公共壳被污染）

---

## 侦探任务清单

1. **结论一句话**：小头像黑窗的根因（优先钉 Prepare 误伤 vs Presenter/开关）。

2. **现网时序图**（分层拍2 对话框出现 → 首句 → 小头像应亮未亮）  
   标出 Prepare、框 FadeIn、Presenter.Apply、大立绘 FadeIn。

3. **误伤节点表**（必出）

   | 节点路径 | 被 Prepare 命中？ | 谁应负责恢复 alpha/Active | 现网是否恢复 |
   |----------|-------------------|---------------------------|--------------|
   | Bottom（字幕条） | | | |
   | Mask/YaerAvatarRoot/GoOut… | | | |
   | Prefab 场景大立绘 GoOut… | | | |

4. **方案比选表**（至少 3 档，推荐 1 个）

   | 方案 | 做法摘要 | 保分层 | 改动面 | 风险 | 推荐？ |
   |------|----------|--------|--------|------|--------|
   | A | Prepare 排除 `Mask`/`YaerAvatarRoot` 子树；大立绘仍按 BB/明确路径藏 | | | | |
   | B | 名字匹配白名单：只动 Prefab 场景立绘 + subtitlesCanvasGroup，禁止扫整棵 Panel | | | | |
   | C | Presenter.Apply / 框 Fade 完成时强制 Mask 内当前 Painting CanvasGroup=1 | | | | |
   | D | 其它 | | | | |

5. **施工员最小改动清单**（只建议）  
   - 优先改 `Village_KenMuNiSceneManager.Prepare…` 匹配范围；避免动 Mask 接线主链。  
   - 验收日志建议：`[VillageStart][Mask]` 打印命中节点名（可随后删）。

6. **验收清单**  
   - 新档进村：分层节奏不变；首句起 Mask 小头像**可见**且跟说话人/脸（至少雅儿 Laugh 或台本脸）。  
   - 大立绘仍正常；无裸村景。  
   - DialogDebug 同 Prefab 不回归。  
   - 后续句换古莎等，小头像仍切换（若本段有换人）。

7. **开放问题**追加 OPEN（「分层显现 · Mask 小头像回归 · 2026-08-06」）：  
   - Prepare 用排除子树还是白名单路径？  
   - 是否顺手给 Presenter 加「Activate 时 alpha=1」防再误伤？

8. **禁止**：改台本；取消分层；在 Update 轮询补头像；为修 bug 重开 0803 Mask 大改造。

---

## 输出要求

写入：`Assets/Doc/执行文档/0806/Village_KenMuNiStart_分层后小头像不显示_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（根因 + 推荐方案）  
② 原因（生活类比：大演员开灯、小显示器没开）  
③ 用户需要做什么（拍板 A/B/C + 验收）  
④ 给程序看的补充：误伤表、时序图、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认「Prepare 收窄」还是「Presenter 自愈」后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_分层后小头像不显示_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，恢复 Village_KenMuNiStart 开场对话框 Mask 小头像显示。
必须保留分层显现节奏与零漏缝；禁止重写 Mask 整套接线。
禁止在 Update 堆补丁。每次提交说明：改了哪些文件、如何避免再误伤 Mask、如何验证。
```
