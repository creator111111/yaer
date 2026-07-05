# CSV 导入工具 — FaceType 列 — 施工执行说明

**施工员交付（待实施）** | 日期：2026-06-01  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【施工员】最小化修改、先可运行
- `Assets/Doc/技术文档/CSV转NodeCanvas对话树导入工具_开发文档.md`（阶段 1 + 前奏已落地）
- `Assets/Doc/执行文档/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md` §3.4（`StatementNodeEx.FaceType`）
- 台本：`Assets/Doc/执行文档/0601/Village_村内雅古开场对白台本_执行说明.md`（单列 FaceType + CSV `Face` 列约定）
- 立绘对照：`Assets/Doc/执行文档/0601/对话立绘表情与图片名称对照_执行说明.md`

**Unity 版本**：2020.3.48f1  

---

## 1. 目标（一句话）

在 **不改动运行时对话播放链路** 的前提下，让 `Tools → Dialogue → Import CSV` 支持 CSV **第 7 列 `FaceType`**，导入时写入 `StatementNodeEx.FaceType`；**旧版 6 列 CSV 仍可导入**，空表情按说话人给安全默认值。

---

## 2. 背景

| 现状 | 问题 |
|------|------|
| CSV 表头 6 列：`ID, Type, Speaker, Text, Next, Extra` | 策划已在 Excel 增加 **FaceType**，手改每个 SayEx 成本高 |
| `CreateStatementNode` 写死 `DialogueFaceType.Normal` | 雅尔图集 **无 `Normal`**，导入后试播易空白脸 |
| 运行时 | 已支持「枚举名 = 图集子图名」，**每人 `Smile` 图不同**；无需按角色拆列 |

**本任务范围**：仅 `Assets/Editor/Tool/Dialogue/**` + 样例 CSV + 文档说明。  
**非范围**：改 `DialogueFaceType` 枚举、合并 Prefab 阶段 2、三语列、动作行 Type。

---

## 3. CSV 格式约定（与策划表对齐）

### 3.1 新表头（7 列）

```text
ID,Type,Speaker,Text,Next,Extra,FaceType
```

| 列序 | 列名 | 说明 |
|------|------|------|
| 0 | ID | 不变 |
| 1 | Type | `Dialogue` / `Choice` |
| 2 | Speaker | 策划简称，经 `DialogueSpeakerMapping` 映射 |
| 3 | Text | 不变 |
| 4 | Next | 不变 |
| 5 | Extra | Choice 选项；Dialogue 可空 |
| 6 | **FaceType** | **本句说话人**的表情，填 `DialogueFaceType` 枚举英文名，如 `Smile`、`Laugh` |

**兼容**：列数仍为 **6** 时，视为无 FaceType 列，行为见 §3.3。  
**列名**：解析按 **列位置**（第 7 字段），不读表头字符串；表头写 `FaceType` 或 `Face` 均可，施工文档与台本统一用 **`FaceType`**。

### 3.2 填写规则

| 行类型 | FaceType 列 |
|--------|-------------|
| `Dialogue` | 建议必填；空则走 §3.3 默认 |
| `Choice` | **忽略**（选项节点无 `StatementNodeEx.FaceType`）；可留空 |
| 非 CSV 动作句 | 不进本导入器（台本序号 9、17 等仍用 Action 节点手工做） |

### 3.3 空 FaceType 时的默认（必须实现）

| 映射后的 Actor 名 | 默认 `DialogueFaceType` | 原因 |
|-------------------|-------------------------|------|
| `雅尔` | **`Smile`** | `Avatar_Yaer_*` 无 `Normal` |
| `古莎` | **`Normal`** | `Avatar_Gusha` 有 `Normal` |
| `艾米` | **`Normal`** | `Avatar_Amy` 有 `Normal` |
| 其它 / 未映射 | **`Normal`** | 并 `Debug.LogWarning` 提示检查 |

> **兼容旧 CSV**：6 列文件全部走此默认，**不得**再全员 `Normal`（否则雅尔全空白）。

---

## 4. 兼容性约束（强制）

| 约束 | 要求 |
|------|------|
| **6 列 CSV** | 仍能 `TryParse` 成功，生成节点数与改前一致 |
| **前奏** | `DialoguePreludeOptions` 全 false 时，除 `FaceType` 赋值外，图结构不变 |
| **非法 FaceType** | `TryParse` **失败并中止**，`error` 写明 `ID` 与非法字符串 |
| **不改运行时** | 禁止改 `Assets/Scripts/Game/**` 下 `DialogueAvatarLoader`、`StatementNodeEx` 播放逻辑 |
| **枚举不改名** | 本阶段不重构 `DialogueFaceType`；CSV 填现有枚举名 |

---

## 5. 拟修改文件与职责

| 文件 | 操作 | 职责 |
|------|------|------|
| `DialogueRow.cs` | **改** | 增加 `public string faceType;`（原始字符串，解析后写入节点） |
| `DialogueCsvParser.cs` | **改** | 支持 ≥6 列；第 7 列读入；`Validate` 中校验 Dialogue 行枚举 |
| `DialogueFaceTypeCsvDefaults.cs` | **新建**（建议） | `TryParseFaceType(string, actorParameterName, out DialogueFaceType)` + 默认表 |
| `DialogueCsvGraphBuilder.cs` | **改** | `CreateStatementNode` 用解析结果赋 `node.FaceType.value` |
| `DialogueCsvImportWindow.cs` | **改** | HelpBox 补充 7 列说明 |
| `Assets/Dialog/村内第一段对话.csv` | **改**（可选） | 样例加 FaceType 列，便于回归 |
| `CSV转NodeCanvas对话树导入工具_开发文档.md` | **改**（可选） | §1.3 非目标删除 Face 列；补 7 列说明 |

**禁止**：为 FaceType 改 `DialogueCsvGraphBuilder.TryBuild` 签名（前奏重载保持不变）。

---

## 6. 代码施工步骤

### 6.1 `DialogueRow.cs`

```csharp
/// <summary>
/// CSV 第 7 列：DialogueFaceType 枚举名（如 Smile）。空串表示走说话人默认。
/// Choice 行可忽略。
/// </summary>
public string faceType;
```

更新类注释：表头 `ID, Type, Speaker, Text, Next, Extra, FaceType`。

---

### 6.2 `DialogueCsvParser.cs`

**常量**：

```csharp
private const int MinColumnCount = 6;
private const int FaceTypeColumnIndex = 6; // 第 7 列，0-based
```

**`TryParse` 循环内**（在 `fields` 解析后）：

```csharp
var faceRaw = fields.Count > FaceTypeColumnIndex ? fields[FaceTypeColumnIndex].Trim() : string.Empty;

rows.Add(new DialogueRow
{
    // ... 原有字段 ...
    faceType = faceRaw,
});
```

**列数判断**：`fields.Count < MinColumnCount` 时跳过行（与现逻辑一致，把 `ExpectedColumnCount = 6` 改为 `MinColumnCount`）。

**`Validate` 增补**（仅 `Dialogue` 行）：

```csharp
if (IsDialogueType(row.type) && !string.IsNullOrWhiteSpace(row.faceType))
{
    if (!Enum.TryParse<DialogueFaceType>(row.faceType, ignoreCase: true, out _))
    {
        error = $"ID {row.id} 的 FaceType 非法（「{row.faceType}」），须为 DialogueFaceType 枚举名。";
        return false;
    }
}
```

**不在 Parser 里写默认**：默认依赖 Actor 名，放在 `DialogueFaceTypeCsvDefaults`（建图时才有 `mapping`）。

---

### 6.3 `DialogueFaceTypeCsvDefaults.cs`（新建）

命名空间：`EditorC.Tool.Dialogue`。

```csharp
using System;
using Game.Static.Enum.Dialogue;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// CSV FaceType 列 → DialogueFaceType；空值时按 Actor 参数名给默认表情。
    /// </summary>
    public static class DialogueFaceTypeCsvDefaults
    {
        public static bool TryResolve(
            string faceTypeRaw,
            string actorParameterName,
            out DialogueFaceType faceType)
        {
            if (!string.IsNullOrWhiteSpace(faceTypeRaw))
            {
                if (Enum.TryParse(faceTypeRaw.Trim(), true, out faceType))
                    return true;

                faceType = DialogueFaceType.Normal;
                return false;
            }

            faceType = GetDefaultForActor(actorParameterName);
            return true;
        }

        private static DialogueFaceType GetDefaultForActor(string actorParameterName)
        {
            if (string.Equals(actorParameterName, "雅尔", StringComparison.Ordinal))
                return DialogueFaceType.Smile;

            return DialogueFaceType.Normal;
        }

        // 可选 v1.1：WarnIfFaceNotInAtlas(actorParameterName, faceType) 读 SpriteAtlas 校验，非 MVP
    }
}
```

---

### 6.4 `DialogueCsvGraphBuilder.cs` — `CreateStatementNode`

替换写死 `Normal` 的段落：

```csharp
if (!DialogueFaceTypeCsvDefaults.TryResolve(row.faceType, actorName, out var resolvedFace))
{
    Debug.LogError(
        $"[DialogueCsvGraphBuilder] ID {row.id} FaceType「{row.faceType}」无法解析为 DialogueFaceType。");
    return null;
}

if (node.FaceType == null)
    node.FaceType = new BBParameter<DialogueFaceType>();

node.FaceType.value = resolvedFace;
```

**顺序**：先 `TryResolve(row.speaker)` 得 `actorName`，再解析 FaceType。

---

### 6.5 `DialogueCsvImportWindow.cs`

HelpBox 增加一行，例如：

```text
CSV 支持 7 列：…, Extra, FaceType。FaceType 填枚举英文名（如 Smile）；
仅对白行有效。旧 6 列仍可用（雅尔默认 Smile，古莎默认 Normal）。
```

---

## 7. 样例 CSV（回归用）

`Assets/Dialog/村内第一段对话.csv` 可改为：

```csv
ID,Type,Speaker,Text,Next,Extra,FaceType
1,Dialogue,雅,好漂亮的村子。,2,,Laugh
2,Dialogue,古,雅尔一定是第一次来吧，一会带你逛一逛村子。,3,,Smile
3,Dialogue,雅,我挺好奇的。,,,Smile
```

进村长篇使用策划导出的 `Village_KenMuNiStart.csv`（路径自定），对照 `Village_村内雅古开场对白台本_执行说明.md` §1。

---

## 8. 验收步骤

1. **编译**：Unity 无报错。  
2. **旧 6 列**：用未加第 7 列的 CSV 导入 → 成功；打开 `.asset`，雅尔对白节点 `FaceType` 为 **Smile**（非 Normal）。  
3. **新 7 列**：用台本 §1 前 5 行导出 CSV → 节点 `FaceType` 与表一致（如 ID1=`Laugh`）。  
4. **非法值**：某行 `FaceType=NotAFace` → 导入失败，Console / 窗口 `lastError` 含 ID。  
5. **Choice 行**：FaceType 填 junk 也不应导致 Parse 失败（忽略即可）。  
6. **前奏**：勾选/不勾选前奏，节点 Face 与无前奏时一致。  
7. **DialogDebug**（可选）：合并 prefab 后试播，脸与表情一致（属阶段 2，非本任务阻塞）。

---

## 9. 施工自检清单

- [ ] `DialogueRow.faceType` 已加  
- [ ] 6 列 / 7 列 CSV 均可解析  
- [ ] `Validate` 校验非法枚举名  
- [ ] `CreateStatementNode` 不再写死 `Normal`  
- [ ] 空 FaceType：雅尔→`Smile`，古莎→`Normal`  
- [ ] 未改 `Assets/Scripts/Game/**` 运行时  
- [ ] `TryBuild` 四参 / 五参（含前奏）签名未破坏  
- [ ] §8 验收通过  

---

## 10. 可选后续（非 MVP）

| 项 | 说明 |
|----|------|
| Editor 校验「该 Actor 图集是否含此 Sprite」 | 读 `Avatar_*.spriteatlas` pack 名，Warning 不阻断 |
| CSV `Face` 列别名 | 表头映射，低优先级 |
| 导入窗口一键打开台本 md | 策划便利 |

---

## 11. 修订

| 日期 | 说明 |
|------|------|
| 2026-06-01 | 初版：FaceType 第 7 列、默认表情、改动文件与验收 |

**路径**：`Assets/Doc/执行文档/0601/CSV导入工具_FaceType列_施工执行说明.md`
