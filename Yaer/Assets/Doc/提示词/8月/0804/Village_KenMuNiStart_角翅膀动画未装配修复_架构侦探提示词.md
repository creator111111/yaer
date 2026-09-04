# Cursor Agent Prompt · Village_KenMuNiStart：角/翅膀动画未播完（表格已通 · 资源装配修复）

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-04  
> **范围**：CSV `Type=Anim` / `播放UI Animator` **表格与节点链路已验收通过**；本期只修 **动画本体未制作完成 / Prefab 未装配导致播不出来**。不扩导入器、不改台本文案。  
> **本阶段**：只摸清缺口与最小修复面，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者现况（已测）

| 层 | 状态 |
|----|------|
| CSV 表格 | **已完成**：ID9/17 为 `Type=Anim`，Extra=`Anim_Gusha` / `Anim_Yaer` |
| 导入 / 节点 | **已完成**：图内已有 `播放UI Animator`（State=`Play`，Wait Until Finish、Hide When Finished 已勾） |
| 动画表现 | **未完成**：Hierarchy 仍是 `Y1～Y5` / `G1～G5` 五帧素材叠着；播动画节点 **Animator 字段红叹号** |

### 复现线索（对齐截图）

1. Prefab：`Village_KenMuNiStart`  
   - `Anim_Yaer` 下仍有 **Y1～Y5**  
   - `Anim_Gusha` 下仍有 **G1～G5**（未精简成单子物体）  
2. NodeCanvas：Action「播放 UI Animator: Anim_Gusha / Play」  
   - Animator 绑定显示红叹号  
   - Fallback Object Name = `Anim_Gusha`  
   - 右侧 Blackboard **只有** `GoOutStoryYaerPainting`、`GushaPainting`，**没有** `Anim_Gusha` / `Anim_Yaer`  
3. 文档声称已落盘的资源 / 工具（须对拍是否「磁盘有、Prefab 没挂上」）：  
   - Clip/Controller：`Assets/GameRes/Animation/Dialogue/Anim_Gusha_Horn`、`Anim_Yaer_Wing`  
   - Task：`PlayUiAnimatorActionTask`  
   - 装配菜单：`Tools/Dialogue/Setup KenMuNiStart Horn Wing Anim`（`KenMuNiStartAnimSetup.cs`）  
   - 溯源说明：`0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构溯源与执行说明.md`（写「请跑 Setup」）

### 高度可疑根因（优先验证，可证伪）

**主假说：阶段 1～2 的 Prefab 装配没落地（或被覆盖回退）**  
代码/CSV/Generated 节点齐了，但 `Anim_*` 上仍无可用 Animator + BB 变量 + 单帧子物体 → `PlayUiAnimator` 解析失败（红叹号 / Log「未找到 Animator」）→ 玩家看不到卷角/翅膀动画。

并列排查（勿只认一条）：

1. **未跑 Setup 菜单**：五子物体仍在；容器无 `Animator`；BB 无 `Anim_Gusha`/`Anim_Yaer`。  
2. **跑过 Setup 但成品 Prefab 被旧实例覆盖**：场景里嵌的对话壳 / 未 Apply Prefab。  
3. **Clip 曲线 path 与绑定哈希不一致**：`.anim` 曲线 `path: G1/Y1`，但 `m_ClipBindingConstant.genericBindings.path` 可能为 `0` → 即使挂上 Animator 也可能**不换图**。  
4. **Controller ↔ Clip GUID 错绑** 或状态名不是 `Play`。  
5. **节点只绑了 BB 名、无 fallback 生效路径**：BB 缺失时 fallback Find 也因无 Animator 组件失败。  
6. **Anim_* 默认 Active + 五帧叠显**，即使用别的方式「播了」也看不清正确帧。

### 明确不在本期范围

- 不再讨论「CSV 标不标 Type / 要不要自动生成节点」（开发者已测表格完成）  
- 不改导入器列格式、不重写 `PlayUiAnimatorActionTask` 业务（除非侦探证实 Task 本身有 bug）  
- 不改台本中文文案

### 对照文档 / 资产

- `Assets/Doc/执行文档/0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构溯源与执行说明.md`
- `Assets/Doc/提示词/0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构侦探提示词.md`
- `Assets/Editor/Tool/Dialogue/KenMuNiStartAnimSetup.cs`
- `Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/PlayUiAnimatorActionTask.cs`
- `Assets/GameRes/Animation/Dialogue/Anim_Gusha_Horn.anim` / `.controller`
- `Assets/GameRes/Animation/Dialogue/Anim_Yaer_Wing.anim` / `.controller`
- `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`
- CSV：`Assets/Dialog/Village_村内雅古开场对白台本.csv`（仅核对 Anim 行仍在，勿改）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构溯源与执行说明.md
@Assets/Editor/Tool/Dialogue/KenMuNiStartAnimSetup.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/PlayUiAnimatorActionTask.cs
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab
@Assets/GameRes/Animation/Dialogue/
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、AnimationClip、CSV、台本。只读扫描 + 写溯源/修复报告。

---

## 背景（策划白话）

1. 表格功能（CSV `Type=Anim` → 生成「播放UI Animator」→ 再出字幕）已经测通。  
2. 但角/翅膀**动画本身没做完**：Hierarchy 还是五帧素材；节点上 Animator 红叹号；实机看不到正确帧动画。  
3. 目标：钉死「缺哪一环」，给出**最小修复清单**（优先复用已有 Setup 菜单 / 已有 Clip，能不重做就不重做）。

---

## 必读 / 优先扫描线索

### A. Prefab 装配缺口（主战场）
对 `Village_KenMuNiStart.prefab` 逐项打勾：

| 检查项 | 期望 | 现网 |
|--------|------|------|
| `Anim_Gusha` / `Anim_Yaer` 子物体数 | 仅 G1 / Y1 | ? |
| 容器上 `Animator` + Controller | Horn / Wing | ? |
| 容器默认 Active | false（入场隐藏） | ? |
| Blackboard 变量 `Anim_Gusha`/`Anim_Yaer` | 指向上述 Animator | ?（截图疑似无） |
| 图内 Play 节点 `animator._name` | 与 Extra/BB 同名 | ? |

对照 `KenMuNiStartAnimSetup`：是否等于「菜单该做的事但 Prefab 还没做」。

### B. 动画资源是否真的可播
- `.anim`：PPtr 曲线是否绑 `Image.m_Sprite`；`path` 是 `G1`/`Y1` 还是空  
- `m_ClipBindingConstant` 的 path 哈希是否与子物体名一致（YAML 手写常见 path=0 漂移）  
- `.controller`：默认态是否叫 **`Play`**；Motion GUID 是否指向对应 `.anim`  
- 时长 / 不循环是否与 Task 的 `waitUntilFinish` 匹配

### C. PlayUiAnimator 失败路径
- Resolve 顺序：BB.value → fallback 名 → agent 下 Find → GetComponent\<Animator\>  
- 红叹号：是 Editor 未解析 BB 变量，还是运行时也会 null  
- Console 是否已有 `[PlayUiAnimator] 未找到 Animator`（若有，记为铁证）

### D. 与「表格已通」划界
- 确认 CSV ID9/17、导入器、成品图节点链**不必再改**（或仅需「装配后把 BB 勾上」这一下）  
- 若节点 Animator 字段只写了名字、BB 补上即可变绿——写入施工步骤，勿重导整表除非必要

### E. 场景实例 vs Prefab 源
- `Village_KenMuNi1` 里若嵌了对话壳：修 Prefab 源后实例是否同步；避免「源修好、场景覆写仍旧五帧」

---

## 侦探任务清单

1. **结论一句话**：动画播不出是因为「资源没挂上 / Clip 绑错 / Task bug」中的哪一类（可并列，标主因）。

2. **缺口对照表**（必填）

   | 项 | 磁盘/代码 | Prefab 现网 | 是否阻塞播放 |
   |----|-----------|-------------|--------------|
   | Clip + Controller | | | |
   | Setup 菜单脚本 | | 是否已执行痕迹 | |
   | 单子物体 | | | |
   | Animator 组件 | | | |
   | BB Anim_* | | | |
   | Play 节点绑定 | | 红叹号原因 | |
   | Clip path 绑定 | | | |

3. **推荐修复顺序**（最小路径，只建议）  
   例：跑 Setup → 核对 BB → 修 Clip Binding（若需要）→ DialogDebug 单测两句 → 实机进村。  
   写清：**哪些一步菜单能搞定，哪些必须进 Animation 窗口重绑。**

4. **验收清单**  
   - Hierarchy：`Anim_*` 下仅一子物体；有 Animator；默认隐藏  
   - BB：`Anim_Gusha`/`Anim_Yaer` 有值；节点 Animator 无红叹号  
   - 播到「古莎卷了卷角」见卷角换帧；翅膀句见翅膀换帧  
   - 播完按 Hide When Finished 不挡立绘  
   - 其它对白不误播

5. **开放问题**追加 OPEN（「KenMuNiStart 角翅膀动画装配修复 · 2026-08-04」）：  
   - Setup 是否已有人跑过又被 Prefab 回滚？  
   - Clip Binding path=0 是否实机已证实不换图？  
   - 场景嵌套实例要不要同步检查？

6. **禁止**：改 CSV Type 约定；重写导入器；用五帧 SetActive 替代已有 Clip 方案（除非证明 Clip 路线彻底不可用）；在 Update 扫台词播动画。

---

## 输出要求

写入：`Assets/Doc/执行文档/0804/Village_KenMuNiStart_角翅膀动画未装配修复_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（主因 + 最小修法）  
② 原因（生活类比：菜单好了厨房没开火）  
③ 用户需要做什么（跑哪菜单 / 验哪几项）  
④ 给程序看的补充：缺口表、Clip/BB/节点核对、施工勾选清单、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认主因是「未跑 Setup」还是「Clip Binding」或其它后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0804/Village_KenMuNiStart_角翅膀动画未装配修复_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使 Village_KenMuNiStart 的 Anim_Gusha / Anim_Yaer 真正可播（单子物体 + Animator + BB + 节点无红叹号），且两句 Anim 对白能看见换帧。
优先复用 Tools/Dialogue/Setup KenMuNiStart Horn Wing Anim 与现有 Clip；禁止无证据重写 PlayUiAnimator / 导入器；禁止回到五帧 SetActive 方案除非报告明确要求。
每次提交说明：改了哪些文件、实现了什么、如何验证卷角/翅膀两句。
```
