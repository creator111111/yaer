# Cursor Agent Prompt · 村长家门口对话：框出现时不应有默认小头像

> **角色**：先【架构侦探】只读定根因，报告通过后再【施工员】最小修复  
> **日期**：2026-09-02  
> **对白**：`Village_村长家门口初次对话`（村街靠近村长黑幕后播的门口三人戏）  
> **现象（用户截图）**：对话框**已出现**、**正文仍空**的第一帧，左侧已露出雅儿**默认/闭眼**小头像（「多了个表情」）  
> **产品期望（钉死）**：  
> 1. **对话框刚出现时**：左侧小头像区 **空**（无小头像 / 不亮任何脸）  
> 2. **按现网逻辑真正开始显示第一句话时**：再出现该句对应的小头像（表情跟台本）  
> **不是**：改大立绘时序；不是改台本 CSV 文案；不是关掉 Mask 体系改回旧 Portrait  
> **报告落盘**：`Assets/Doc/执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md`  
> **施工落盘**（报告通过后）：`Assets/Doc/施工说明/0902/Village_村长家门口初次对话_框出时空头像_施工说明.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）。施工 Prompt 见文末，根因拍板后再用。

---

## 提示词助手预梳理（须再证，勿当唯一真相）

### 产品时序（期望）

```
黑幕 / 壳就绪
  → 对话框出现（可渐入）…… 此时：有框、无字、【无小头像】
  → 首句 OnSubtitlesRequest / OnGetNewStatement …… 此时：出字 + 出该句小头像
  → 后续句按现网逻辑换脸
```

| 阶段 | 框 | 正文/名字 | 小头像（Mask 或 Portrait） |
|------|----|-----------|---------------------------|
| 框刚出 / 淡入中 | ✅ 可见或渐入 | ❌ 空（现网已有清字） | ❌ **须空**（本期要修） |
| 首句真正显示 | ✅ | ✅ 第一句 | ✅ 跟首句 FaceType |
| 用户截图现状 | ✅ | ❌ 空 | ⚠ **已亮默认脸** |

### 与「进村开场」对照（勿照搬）

| 对白 | 框出时是否预亮 Mask | 产品 |
|------|---------------------|------|
| `Village_KenMuNiStart` | 文档写 **`PrepareMaskAvatarOnFadeIn`**（雅儿/Laugh）与框一起淡出 | 开场分层要「框+头像」同拍 |
| **`Village_村长家门口初次对话`** | **本期产品不要**框出就带头像 | 空框 → 首句再出头像 |

侦探须在门口 Prefab 上**单独读** `NormalDialogueUIAlphaAnimationTaskAction` 的 BB：`PrepareMaskAvatarOnFadeIn` / `MaskAvatarRole` / `MaskAvatarFace`，勿用 KenMuNiStart 配置当门口真相。

### 现网相关链路（助手预扫）

```
NormalDialogueUIAlphaAnimationTaskAction（框 FadeIn）
  → ClearSubtitleTextsForEmptyFrame()     // 已清字，故截图「有框无字」符合
  → 若 PrepareMaskAvatarOnFadeIn=true
       → DialogueMaskAvatarPresenter.Apply(role, face)  // ⚠ 会提前亮 Mask
  → DOFade 框透明度

首句真正来时：
DialogueTMPUGUI → OnGetNewStatement → Presenter.Apply(role, faceType)
```

| 组件 | 路径 | 嫌疑 |
|------|------|------|
| 框淡入任务 | `NormalDialogueUIAlphaAnimationTaskAction.cs` | **H1**：门口图勾了预亮 Mask |
| Mask 呈现 | `DialogueMaskAvatarPresenter.cs` | Apply / HideAll；None 跳过 HideAll 旗 |
| 雅儿 GoOut 默认脸 | `GoOutStoryYaerPainting.SetDefaultPainting` | **H2**：首次 SetActive 后默认 Smile/闭眼盖脸 |
| 旧 Portrait | `DialogueTMPUGUI.actorPortrait` + Loader | **H3**：useMaskAvatar 下仍闪一下旧图 |
| Prefab 默认 Active 脸 | `NormalDialogueNewPanel` / 门口壳内 Mask 子树 | **H4**：Prefab 里某张脸默认亮着 |

### 关键假说（本轮优先）

| ID | 假说 | 预扫 | 证伪方式 |
|----|------|------|----------|
| **H1** | 门口 Prefab 框 FadeIn **勾了 `PrepareMaskAvatarOnFadeIn`**，用默认 Role/Face 提前 `Apply` → 空字+有头像 | 进村开场故意预亮；门口若误开则正中现象 | 读门口 Prefab BB；对比 KenMuNiStart |
| **H2** | 未预亮，但框 Active 时 Mask 内 Painting 首次 `Start`→`SetDefaultPainting` 亮默认脸，首句前无人 `HideAll` | 0804 首句 Smile 竞态同类 | 日志：FadeIn 前后 Mask 子物体 Active / face |
| **H3** | 旧 `Portrait` Image 未彻底关，残留默认 sprite | 技术文档称 Portrait Active=false | Hierarchy 截图左槽是 Mask 还是 Portrait |
| **H4** | Prefab 序列化默认亮着某 Face 子物体 | 母体/实例 | 离线看 Prefab 默认 Active 脸 |
| **H5** | 首句事件早到又被清字盖掉，只剩头像 | 清字只清 TMP，不 Hide Mask | 时序：首句 Invoke vs ClearText vs FadeIn |

### 方案倾向（施工默认，侦探可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1 · 门口关预亮** | 门口 Prefab：`PrepareMaskAvatarOnFadeIn=false`；保持 FadeIn 清字 | ✅ 若 H1 成立，零代码/最小改 Prefab |
| **F2 · 框出强制 HideAll** | FadeIn 清字时同步 `Presenter.HideAll`（或等价），首句 `Apply` 再亮 | ✅ 治 H2/H4；可做成「仅门口」或「未勾预亮则一律 Hide」 |
| **F3 · 预亮改为空角色** | 仍勾预亮但 Role=None 且 Presenter 保证空槽 | ⚠️ 须读清 None 是否跳过 HideAll（店/村长旗） |
| F4 · 改首句逻辑延后出框 | 推翻现网「先框后句」 | ❌ 产品只要空头像，不要重做分层 |
| F5 · 关 useMaskAvatar / 拆 Mask | 回退体系 | ❌ |

**推荐**：先证 H1 → 成立则 **F1**；若不预亮仍露脸 → **F2**（框出 HideAll，首句再 Apply）。**禁止**为修门口把 KenMuNiStart「框+头像同拍」弄坏。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 钉死左槽控件身份（Mask vs Portrait） | ❌ 改 CSV 台词 / FaceType 表内容（除非资产未写入） |
| ✅ 钉死框出→首句时序与谁写了默认脸 | ❌ 改场景大立绘摆位 / Face123 Import |
| ✅ 最小修：空框无头像 → 首句出头像 | ❌ 全局关掉 `PrepareMaskAvatarOnFadeIn` 而不回归进村开场 |
| ✅ 回归门口三人戏首句表情正确 | ❌ 顺手改商店头像 / 续聊壳 |

### 严禁

- 用「首句一开始就写死默认字」掩盖空框需求  
- 为修空头像破坏 `Village_KenMuNiStart` 预亮 Mask 分层  
- 大范围重写 `DialogueTMPUGUI` / Presenter  
- 不经验证就把责任推给「美术默认图」而不查 FadeIn/Apply 时序  

### 对照文档（必挂）

- `Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`
- `Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`（预亮对照，勿照搬产品）
- `Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`（若仍在）
- `Assets/Doc/提示词/0804/对话框小表情_首句未跟FaceType_架构侦探提示词.md`（竞态参考）
- `Assets/Doc/执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md`

---

## 侦探 Prompt（复制给 Agent · 先跑）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。
默认中文。沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/提示词/0902/Village_村长家门口初次对话_框出时空头像_架构侦探提示词.md
@Assets/Doc/OPEN_QUESTIONS.md

## 现象（产品）
村长家门口对话 `Village_村长家门口初次对话`：
对话框出现的第一帧（正文仍空）左侧已有雅儿默认/闭眼小头像。
期望：框出时小头像区为空；按现网逻辑显示第一句话时再出小头像。

## 必读脚本 / 资源
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs
（ClearSubtitleTextsForEmptyFrame、PrepareMaskAvatarOnFadeIn、PrepareMaskAvatarForFadeIn）
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
（Apply、HideAll、None 跳过 HideAll）
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
（useMaskAvatar、OnGetAvatar、OnGetNewStatement）
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/GoOutStoryYaerPainting.cs
（SetDefaultPainting）
@Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab
（框 Fade 节点 BB：PrepareMaskAvatar*）
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab
（Mask / Portrait 默认 Active）
对照：@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab 的预亮配置（仅对照，勿改产品）

检索：PrepareMaskAvatarOnFadeIn、HideAllPaintings、SetDefaultPainting、useMaskAvatar、OnGetNewStatement、ClearSubtitleTexts。

## 任务
1. 钉死截图左槽是 Mask 立绘还是旧 Portrait（或双影）。
2. 画出门口对白：壳打开 → 框 FadeIn → 清字 →（是否预亮）→ 首句出字/出头像 时序图；标出「默认脸」写入点。
3. 按 H1～H5 证伪；明确门口 Prefab 是否勾选 PrepareMaskAvatarOnFadeIn 及 Role/Face。
4. 对比 KenMuNiStart：说明为何进村可预亮、门口不能照搬。
5. 推荐 F1/F2/F3；写最小修复清单与回归范围（必须含 KenMuNiStart 框+头像同拍不被破坏）。
6. 不清的设计记入 OPEN_QUESTIONS.md（勿擅自改核心设计）。

## 报告
Assets/Doc/执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md
```

---

## 施工 Prompt（根因拍板后复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md
@Assets/Doc/提示词/0902/Village_村长家门口初次对话_框出时空头像_架构侦探提示词.md

## 目标
Village_村长家门口初次对话：对话框出现且尚未显示第一句话时，左侧小头像必须为空；
第一句话按现网逻辑出现时，再显示对应小头像（表情跟台本）。

## 默认施工方向（若报告未改口）
1. 若 H1：门口 Prefab 关闭 PrepareMaskAvatarOnFadeIn（F1）；确认 FadeIn 仍清字。
2. 若不预亮仍露脸：FadeIn 清字路径同步 HideAll Mask（F2）；首句 OnGetNewStatement → Apply 再亮。
3. 代码含详细注释；重要修改写原因；复杂逻辑注明替代方案（F1 vs F2 vs F3）。
4. 同步 OPEN_QUESTIONS.md。

## 约束
- 禁止破坏 Village_KenMuNiStart 的 PrepareMaskAvatarOnFadeIn「框+头像同拍」
- 禁止关 useMaskAvatar / 拆 Mask / 回退旧 Portrait 当主修
- 禁止改门口大立绘摆位、CSV 文案、进村长家 Loading 链
- 禁止全局无差别关掉所有对话的预亮而不做进村回归

## 落盘
Assets/Doc/施工说明/0902/Village_村长家门口初次对话_框出时空头像_施工说明.md

## 验收
- [ ] 门口对白：框出现瞬间（无字）左槽无小头像
- [ ] 第一句话出现时左槽出现对应该句的头像/表情
- [ ] 后续句换脸正常；三人戏大立绘不受影响
- [ ] Village_KenMuNiStart：框出仍按原设计可预亮/同拍（不被本次改坏）
- [ ] 商店等其它 Mask 对话无回归（若动到 Presenter/FadeIn 公共逻辑）
```

---

## 给开发者（一句话）

框已经会**清空文字**，但门口很可能在淡入时**顺带预亮了 Mask 默认脸**（或 Painting 默认脸先亮了）。要的是：**空框 → 首句再出头像**；先跑侦探 Prompt 钉死是 Prefab 预亮还是默认脸竞态，再按文末施工。
