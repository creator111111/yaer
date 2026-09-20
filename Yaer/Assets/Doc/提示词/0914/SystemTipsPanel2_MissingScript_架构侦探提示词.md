# Cursor Agent Prompt · SystemTipsPanel2：Missing Script 导致无法改 Prefab

> **角色**：先【架构侦探】只读定位 Missing Script 身份与影响；拍板后【施工员】最小修复（恢复可编辑）  
> **日期**：2026-09-14  
> **目标 Prefab**：`Assets/GameRes/Prefabs/UI/SystemTipsPanel2.prefab`  
> **现象（用户 Inspector 截图）**：根上出现 **Missing (Mono Script)**，黄叹号「The associated script can not be loaded…」；用户要改 BOSS 战前保存提示素材/引用，但 Prefab **改不动 / 不敢存**  
> **产品期望（钉死）**：  
> 1. 查清 Missing 是**哪个脚本丢了**（GUID / 曾用类名 / 是否本就不该存在）  
> 2. **恢复 Prefab 可正常编辑、保存**（去掉坏引用或挂回正确脚本）  
> 3. `SystemTipsPanel2` 运行时弹窗仍可用（BOSS 战 `OpenSystemTipsPanel2WaitSureActionTask` / Save 类型）  
> 4. 顺带核实 `Img Tips Content` 现为 **None** 是否正常、是否影响换 `SaveChar` 素材（换图走图集，未必依赖该槽）  
> **不是**：重做整套 SystemTips UI；改对话图逻辑；顺手大改 `SystemTipsPanel`（v1）除非对照需要  
> **报告落盘**：`Assets/Doc/执行文档/0914/SystemTipsPanel2_MissingScript_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> BOSS 前提示用的 `SystemTipsPanel2` 预制体挂着一个丢了的脚本，Inspector 一片黄，没法安心改。  
> 先查这个丢的是什么、能不能删、还是要补回哪个脚本，修好后再换提示图。

### 现网预扫（须核实）

根物体组件顺序（与截图一致）：

| # | 组件 | Script GUID | 状态 |
|---|------|-------------|------|
| … | CanvasScaler / GraphicRaycaster | Unity 内置 | OK |
| | **SystemTipsFormLogic** | `47b2f842286746b19605853e8d0e7a1d` | OK（`.meta` 存在） |
| | **ComponentSystemUI** | `05b7f39879134aed846421d81b8ad7b1` | OK |
| | **CursorChangeUI** | `2578ec541db03504c856a946a0482fa9` | OK |
| | **??? Missing** | **`8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6`** | **全仓库仅 Panel2 引用；无任何 `.meta` 匹配** |

可疑点：

1. GUID `8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6` 呈 **连续占位形**（`…a1b2c3d4e5f6`），极像手填/错误生成，**不是**正常 Unity 随机 GUID。  
2. 对照 `SystemTipsPanel.prefab`（v1）：有 FormLogic + ComponentSystemUI + CursorChangeUI，**没有**该第四脚本。  
3. `imgTipsContent: {fileID: 0}`（Inspector 显示 None）——换 `SaveChar` 走 `SystemTipsFormProxy` 图集，侦探须说明是否必须绑 Image。

YAML 锚点（Panel2 根末尾）：约 `m_Script: … guid: 8f4e2a1b3c5d6478e9f0a1b2c3d4e5f6`（紧接 CursorChangeUI 之后）。

### 嫌疑优先级

| # | 嫌疑 | 体感 |
|---|------|------|
| **A（主）** | Prefab 多挂了一块 **无效/占位 GUID** 的 MonoBehaviour，脚本从未进库或已删 | 仅 Panel2 Missing；v1 无此槽 |
| **B** | 曾有专用脚本（如 Panel2 扩展）后删文件、GUID 未改 Prefab | 全库搜 GUID 无 meta → 文件已不在 |
| **C** | asmdef / 编译失败导致脚本未加载（显示 Missing） | 须先看 Console；但占位 GUID 更像 A |
| **D** | `imgTipsContent` None 与 Missing 无关，但是换图时的次要坑 | 换 SaveChar 不改此槽也能成 |

### 侦探须回答

1. Missing 的 GUID 在全工程 `.meta` 中是否存在？若否，能否从 git 历史找回曾对应的 `.cs`？  
2. 与 `SystemTipsPanel` 组件清单 diff：多出来的是否可 **安全删除** Missing 组件？  
3. 删除后 BOSS 提示 / 存档确认是否仍依赖该脚本？  
4. `imgTipsContent = None`：运行时 `UpdateTips` 是否 NRE？换素材路径是否仍为 `ArtRes/.../SaveChar.png` + tipsChar 图集？  
5. 推荐修复：**删 Missing 槽** vs **补回脚本并改 GUID**；给出施工步骤与验收。

### 必读

1. `Assets/GameRes/Prefabs/UI/SystemTipsPanel2.prefab`  
2. `Assets/GameRes/Prefabs/UI/SystemTipsPanel.prefab`（对照）  
3. `SystemTipsFormLogic.cs` / `SystemTipsFormProxy.cs`  
4. `OpenSystemTipsPanel2WaitSureActionTask.cs`、`Doc/技术文档/Boss/BOSS战提示系统.md`  
5. 用户 Inspector 截图  
6. 本提示词  

### 禁止（侦探阶段）

- 禁止改 Prefab / 代码 / Git  
- 禁止未核实就断定「必须重写 FormLogic」  
- 禁止为消黄叹号乱挂无关脚本  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读。

### 输出

`Assets/Doc/执行文档/0914/SystemTipsPanel2_MissingScript_架构溯源报告.md`

结构：

1. **结论一句话**（Missing 是什么 + 删还是补）  
2. **证据**（GUID 全库检索、与 Panel1 对照、序列化字段）  
3. **影响面**（编辑器改 Prefab、运行时弹窗、换 SaveChar）  
4. **方案 ≥2** + 推荐 + 验收  
5. 不清处记 `OPEN_QUESTIONS.md`  

回答风格：①结论 ②原因白话 ③检查清单 ④程序补充。

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：报告确认 Missing GUID 无对应脚本（或已定位应挂脚本）。  
> **目标**：`SystemTipsPanel2` 无 Missing Script，可正常打开/保存 Prefab；BOSS 战前 SystemTipsPanel2 弹窗正常。  
> **优先**：若确认为无效占位组件 → **删除该 Missing MonoBehaviour 块**（YAML 或 Inspector Remove Component）；保留 FormLogic / ComponentSystemUI / CursorChangeUI。  
> **若**报告要求补脚本：按报告 GUID/类名恢复，禁止再写假 GUID。  
> **顺带**（报告批准时）：补绑 `imgTipsContent` 到正确 Image，或注明「换 SaveChar 不必绑」。  
> **禁止**：重做 UI；改对话树；动 `tipsChar` 图集除非用户另开任务。  
> **文档**：`Assets/Doc/施工说明/0914/SystemTipsPanel2_MissingScript_施工说明.md`  
> **验收**：Inspector 无黄叹号；Apply Prefab 成功；Play 触发 BOSS 前保存提示可点确认；Console 无 FormLogic NRE。

---

## 【验收员】Prompt（可选）

> 打开 Prefab：组件列表无 Missing；对比 Panel1 组件差。  
> Play：`WestRappRoadGoblinAndGusha` 弹出 SystemTipsPanel2，确认/取消可用。  
> 输出：通过项 + 剩余风险（imgTipsContent 空、假 GUID 来源）。
