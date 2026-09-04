# CSV 导入工具 — Speaker 映射扩展 — 施工执行说明

**施工员交付（待实施）** | 日期：2026-06-01  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【施工员】最小化修改、先可运行
- `Assets/Doc/技术文档/CSV转NodeCanvas对话树导入工具_开发文档.md`
- `Assets/Doc/执行文档/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md` §3.8、§5.3
- 台本：`Assets/Doc/执行文档/0601/Village_村长家晚宴对白台本_执行说明.md`（Speaker 含 `艾米` / `艾莉` / `村`）
- 样例 CSV：`Assets/Dialog/Village_村长家晚宴对白台本.csv`

**Unity 版本**：2020.3.48f1  

---

## 1. 目标（一句话）

在 **不改动运行时对话播放链路** 的前提下，扩展 CSV 导入器的 **Speaker 简称 → NodeCanvas Actor 参数名** 映射，使 `Village_村长家晚宴` 等台本能直接导入；新增 **艾米→艾米、艾莉→艾莉、村→村长**，并保留现有 **雅→雅尔、古→古莎**。

---

## 2. 背景与痛点

| 现状 | 问题 |
|------|------|
| `DialogueSpeakerMapping.CreateDefaultInstance()` 仅含 **雅→雅尔、古→古莎** 两条 | 导入 `Village_村长家晚宴对白台本.csv` 时，含 `艾米` / `艾莉` / `村` 的行会 **Console 报错并中止** |
| 窗口未拖入 SO 时走内置默认 | 策划/程序误以为「Speaker 列写中文名即可」，实际未映射 |
| 成品 Prefab（如 `Village_LeaderGuShaAmyAliy`）图内 Actor 名为 **「艾米」「艾莉」** | CSV 简称与图内名一致时，映射应为 **恒等或短称→全名** |
| 村长晚宴台本 Speaker 用单字 **「村」** | 图内 Actor 建议统一为 **「村长」**（与 `Village_Leader` 占位及后续立绘命名一致） |

**本任务范围**：`Assets/Editor/Tool/Dialogue/**` + 可选新建 **DialogueSpeakerMapping .asset** + 文档/HelpBox 文案。  
**非范围**：新增 `DialogueRoleName` 枚举、村长立绘图集、Prefab 合并阶段 2、运行时 `DialogueAvatarLoader`。

---

## 3. 映射表（本任务必须实现）

| CSV `Speaker` 列（策划简称） | 图内 `actorParameters.name` | 说明 |
|------------------------------|-----------------------------|------|
| `雅` | `雅尔` | **已有**，保持不变 |
| `古` | `古莎` | **已有**，保持不变 |
| **`艾米`** | **`艾米`** | **新增**；与 `Village_LeaderGuShaAmyAliy` 等 Prefab Actor 名一致 |
| **`艾莉`** | **`艾莉`** | **新增**；同上 |
| **`村`** | **`村长`** | **新增**；台本单字简称 → 图内完整称呼 |

**匹配规则**（已实现，勿改）：`TryResolve` 对 `csvSpeaker` / 入参 **Trim 后完全相等**（区分大小写；中文无大小写问题）。

**未命中行为**（已实现，勿改）：`DialogueCsvGraphBuilder.TrySetupActorParameters` 收集错误并 **中止导入**，Console 输出  
`Speaker「xxx」（ID n）未在映射表中找到，导入已中止。`

---

## 4. 兼容性约束（强制）

| 约束 | 要求 |
|------|------|
| **旧 CSV** | 仅含 `雅` / `古` 的 `Village_村内雅古开场` 等文件，导入结果与改前 **节点数、Actor 名一致** |
| **API** | 不删改 `DialogueSpeakerMapping.TryResolve` 签名；不破坏 `DialogueCsvGraphBuilder.TryBuild` 各重载 |
| **运行时** | **禁止**修改 `Assets/Scripts/Game/**`（村长立绘缺资源属已知问题，另立项） |
| **自定义 SO** | 用户拖入的 `DialogueSpeakerMapping` 资产 **优先于** 内置默认；本任务同时更新内置默认与推荐 SO 内容 |

---

## 5. 拟修改 / 新建文件

| 文件 | 操作 | 职责 |
|------|------|------|
| `Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs` | **修改** | `CreateDefaultInstance()` 增加 §3 三条映射；更新类注释中的默认说明 |
| `Assets/Editor/Tool/Dialogue/DialogueFaceTypeCsvDefaults.cs` | **修改（建议）** | 空 `FaceType` 默认表增加 `艾莉` → `Normal`（与 `Avatar_Aliy` 一致）；`村长` → `Normal` 并 `LogWarning`（图集未就绪） |
| `Assets/Editor/Tool/Dialogue/DialogueCsvImportWindow.cs` | **修改（文案）** | HelpBox 中「内置默认」改为五条映射摘要 |
| `Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset` | **新建（推荐）** | 项目内常驻 SO，窗口可默认引用或策划手动拖入 |
| `Assets/Doc/技术文档/CSV转NodeCanvas对话树导入工具_开发文档.md` | **修改（实施后）** | 补充 §3 完整映射表 |

**禁止修改**：`DialogueCsvParser.cs`（除非发现 Speaker 列解析 bug）、`StoryComponentGSM`、任意运行时对话 UI。

---

## 6. 代码变更规格

### 6.1 `DialogueSpeakerMapping.CreateDefaultInstance()`

在现有 `entries` 列表末尾 **追加**（顺序建议：雅、古、艾米、艾莉、村）：

```csharp
new Entry { csvSpeaker = "艾米", actorParameterName = "艾米" },
new Entry { csvSpeaker = "艾莉", actorParameterName = "艾莉" },
new Entry { csvSpeaker = "村", actorParameterName = "村长" },
```

**注释要求**（用户规则）：在 `CreateDefaultInstance` 方法上方或条目块旁注明：

- 为何 `村` 映射为 `村长`（台本简称 vs 图内 Actor 命名）
- 未命中仍中止导入，避免生成 NodeCanvas 红色未定义 Actor

**替代方案（不推荐本任务采用）**：

- **方案 B**：仅新建 `.asset`，不改 `CreateDefaultInstance` → 未拖 SO 时晚宴 CSV 仍会失败，不符合「先可运行」。
- **方案 C**：CSV 直接写 `村长` 而不映射 `村` → 与策划台本 §2 Speaker 列约定不一致。

### 6.2 `DialogueFaceTypeCsvDefaults.GetDefaultForActor`（建议一并做）

空 `FaceType` 列时的默认表情扩展：

| 映射后 Actor 名 | 默认 `DialogueFaceType` | 原因 |
|-----------------|-------------------------|------|
| `艾莉` | `Normal` | `Avatar_Aliy` 有 `Normal` |
| `村长` | `Normal` | 占位；立绘未接入前至少不因空列解析失败 |

`GetDefaultForActor` 中对 `村长` 可保留现有 `LogWarning` 文案，提示检查映射与图集。

**注意**：晚宴 CSV 已填 `FaceType` 列时，**不依赖**本默认表；仅 6 列旧 CSV 或空列行受益。

### 6.3 推荐 SO 资产（可选但建议）

| 项 | 值 |
|----|-----|
| 路径 | `Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping_Default.asset` |
| 创建方式 | Project 右键 → **Create → Yaer/Dialogue/Speaker Mapping**，按 §3 填 5 行 |
| 用途 | 团队统一引用；Import CSV 窗口 ObjectField 拖此资产 |

内置 `CreateDefaultInstance()` 与 SO 内容应 **保持一致**，避免「拖 SO / 不拖 SO」两套映射分叉。

### 6.4 窗口 HelpBox 文案（`DialogueCsvImportWindow`）

将：

```text
未指定映射时将使用内置默认（雅→雅尔、古→古莎）。
```

改为：

```text
未指定映射时将使用内置默认：雅→雅尔、古→古莎、艾米→艾米、艾莉→艾莉、村→村长。
建议在项目中创建 DialogueSpeakerMapping 资产统一管理（可与内置默认内容一致）。
```

---

## 7. 与 Prefab / 运行时对齐说明

导入生成的 `.asset` 中 `actorParameters` 应出现：

```text
雅尔, 古莎, 艾米, 艾莉, 村长
```

（实际去重子集取决于 CSV 出现的 Speaker。）

合并进 Prefab（阶段 2）时：

| 图内 Actor 名 | 建议绑定 |
|---------------|----------|
| `艾米` | `DialogueActorEx` → `DialogueRoleName.Amy` |
| `艾莉` | `DialogueRoleName.Aliy` |
| `村长` | **待补**枚举/立绘；当前可保留 Actor 仅播字幕，或暂绑 `DialogueRoleName.None` |

**本任务不要求**完成村长立绘或 Prefab 合并，仅保证 **CSV 能生成图且 Actor 名正确**。

---

## 8. 验证步骤（施工员自检）

### 8.1 编译

- [ ] Unity 2020.3.48f1 无 Editor 脚本报错

### 8.2 回归：旧 CSV

- [ ] 打开 `Tools → Dialogue → Import CSV`
- [ ] **不拖** Speaker 映射 SO
- [ ] 选择 `Assets/Dialog/Village_村内雅古开场对白台本.csv`（或等价仅雅/古文件）
- [ ] 生成 `.asset` 成功；Console **无** Speaker 未映射错误
- [ ] 打开 `.asset` → NodeCanvas 图：节点 `_actorName` 仅为 **雅尔 / 古莎**

### 8.3 新 CSV：晚宴台本

- [ ] 选择 `Assets/Dialog/Village_村长家晚宴对白台本.csv`
- [ ] 生成成功；Console **无** `Speaker「村」未在映射表中找到`
- [ ] 图内 `actorParameters` 含 **艾米、艾莉、村长**（及雅尔、古莎若 CSV 有）
- [ ] 抽查节点：
  - ID 含 `Speaker=村` 的行 → `_actorName` 为 **`村长`**
  - `Speaker=艾米` → **`艾米`**
  - `Speaker=艾莉` → **`艾莉`**

### 8.4 负例

- [ ] 临时在 CSV 加一行 `Speaker=测试未映射`，导入应 **失败** 且 Console 有明确 ID

### 8.5 自定义 SO

- [ ] 创建 SO 仅含部分映射 → 拖入窗口 → 验证 **SO 覆盖** 内置默认（缺项仍中止）

---

## 9. 提交说明模板

```text
扩展 CSV 导入 Speaker 映射：艾米/艾莉/村（→村长），更新内置默认与 FaceType 空列默认。

修改：DialogueSpeakerMapping.cs、DialogueFaceTypeCsvDefaults.cs、DialogueCsvImportWindow.cs
新建：DialogueSpeakerMapping_Default.asset（若做）
文档：CSV导入工具_Speaker映射扩展_施工执行说明.md

验证：Village_村内雅古开场 + Village_村长家晚宴 CSV 均可 Import CSV 生成 .asset
```

---

## 10. 修订

| 日期 | 说明 |
|------|------|
| 2026-06-01 | 初版：晚宴台本驱动，新增 艾米/艾莉/村 三条映射及验收清单 |

**路径**：`Assets/Doc/执行文档/0601/CSV导入工具_Speaker映射扩展_施工执行说明.md`
