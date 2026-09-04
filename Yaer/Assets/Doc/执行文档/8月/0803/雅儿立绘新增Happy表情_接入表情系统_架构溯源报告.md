# 雅儿立绘新增 Happy 表情 · 接入表情系统 — 架构溯源报告

**文档版本**：v1.0（2026-08-03）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 图集 / CSV / 台本**）  
**范围**：雅儿（Yaer）对话表情链路中，把新增的 **Happy** 接到可调用状态；古莎仅作「Happy 已有先例」对照，**本期不改古莎**  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0803/雅儿立绘新增Happy表情_接入表情系统_架构侦探提示词.md`
- 对照：`Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md`
- 对照：`Assets/Doc/执行文档/6月/0614/雅儿对话立绘素材替换_执行说明.md`
- 对照：`Assets/Doc/执行文档/7月/0727/对话系统_主角对话框表情显示_架构溯源报告.md`
- 技术说明：`Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`
- 代码 / Prefab / 图集 / ArtRes 静态阅读

**Unity 版本**：2020.3.48f1  

---

## ① 结论一句话

**村线 GoOut 大立绘：台本选 `Happy` 已可「零改 C#」切到 `Armor_NoHeadWear_Happy`（键名与 `ResolveGoOutFaceKey` 对齐，Awake 字典会收录）；但节点上绑的是连衣裙「开心」图、尺寸也远小于其它脸，验收时要先看脸对不对。小头像四套图集仍无 `Happy` → 字幕条/历史会藏头像；连衣裙线 `YaerPainting` / `NewGame` 无 `Dress_Crown_Happy`；Mask 内嵌立绘仍未接 FaceType，本期不算闭环。**

生活类比：舞台人偶（大立绘）衣柜里多挂了一张「开心」脸，台词喊 Happy 就能换上——但挂上去的可能是裙子版缩略图；手机头像（小头像）图册里还没有这张；窗里探出来的人偶（Mask）还没接到对讲机。

---

## ② 原因（生活类比 + 技术锚点）

### 2.1 必须先分清的三条链路（勿混谈）

| 你在游戏里看到的 | 本报告称呼 | Happy 现状 | 是否跟大立绘节点自动同步 |
|------------------|------------|------------|--------------------------|
| 屏幕半身大图 | **大立绘 Painting** | GoOut 已有 `Armor_NoHeadWear_Happy` 节点 | —（本条就是大立绘） |
| 字幕条左侧小图 | **小头像 Avatar**（`Portrait` / 图集） | 四套 `Avatar_Yaer_*` **无** `Happy` 子图 | **否** |
| Mask 窗内嵌立绘 | **Mask 内嵌 Painting** | Prefab 已嵌 GoOut 实例（会继承母体 Happy 节点），但 **未接 FaceType** | **否**（接线属下一轮） |

> **假结论警告**：大立绘 Prefab 加了 Happy 节点 ≠ 小头像自动有 Happy ≠ Mask 头像会跟台本变脸。

### 2.2 Happy 调用链（台本 → Actor 事件 → UpdateFace 键）

```
SayEx / CSV FaceType = Happy
  → StatementNodeEx.FaceType（DialogueFaceType.Happy，枚举已存在）
  → SubtitlesRequestInfoEx.FaceType
  → DialogueTMPUGUI → DialogueActorEx.RefreshAvatar(Happy, …)
       ├─ DialogueAvatarLoader.GetAvatar
       │    → atlas.GetSprite("Happy")
       │    → 雅儿四套图集均无 → sprite=null → Portrait / 历史头像隐藏
       └─ OnRefreshAvatarEvent(role, Happy, sprite)
            → GoOutStoryYaerPainting.RegisterRefreshAvatarEvent
            → UpdateFace(ResolveGoOutFaceKey(Happy))
            → 键 = "Armor_NoHeadWear_Happy"
            → StoryFormPainting.facesDic 激活同名子物体
```

| 阶段 | 谁 | 做什么 | 技术锚点 |
|------|-----|--------|----------|
| 1 枚举 | `DialogueFaceType` | `Happy` **已存在**（在 `Daze` 与 `CloseEyes` 之间） | `Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs` |
| 2 台本入口 | `StatementNodeEx` / CSV | 可选 `Happy`；CSV `Enum.TryParse` 可通过 | `DialogueCsvParser` 对白行校验；`DialogueFaceTypeCsvDefaults` 仅管空列默认 |
| 3 Actor 广播 | `DialogueActorEx.RefreshAvatar` | 先挂事件再 Loader 取图 | 大立绘**只吃** `faceType`，忽略事件里的 Sprite |
| 4 GoOut 键 | `ResolveGoOutFaceKey` | `Normal`→Smile；其它 → `Armor_NoHeadWear_{faceType}` | **Happy → `Armor_NoHeadWear_Happy`**，无需改代码 |
| 5 切脸 | `StoryFormPainting.UpdateFace` | Awake 时按 **子物体 name** 建字典；有键则激活 | 缺键 → 全部脸关掉（人消失） |
| 6 小头像 | `DialogueAvatarLoader` | `GetSprite(faceType.ToString())` | 缺图 → null → `OnGetAvatar` / `HistoryDialogueBox` **隐藏** Image |

### 2.3 GoOut 大立绘：钉死「零代码能否工作」

| 检查项 | 结果 |
|--------|------|
| Prefab 路径 | `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` |
| `Faces` 下是否有 `Armor_NoHeadWear_Happy` | **有**（与其它 9 张并列，共 10 张脸） |
| Image 是否绑图 | **有**（`m_Sprite` 非空） |
| 绑的是哪张图 | guid `e6798917815114d449866f63fd8a131e` → `ArtRes/.../Yaer/Face/Dress/.../开心.png`（**连衣裙开心**，非 ArmorNone 源） |
| 节点尺寸 vs 其它脸 | Happy：`SizeDelta ≈ 166×134`、Pos `(-21.7, 525.3)`；Smile 等：`≈ 1078×1497`、Pos `(0,0)` → **布局明显异常，验收必看** |
| `ResolveGoOutFaceKey(Happy)` | `"Armor_NoHeadWear_Happy"` → **与节点名一致** |
| Awake 字典 | 按 `child.name` 收录，**会收入 Happy**，无需改脚本 |
| `defaultFace` | 指向 `Armor_NoHeadWear_Smile`；Awake 会关掉非默认脸（含 Prefab 里 Active=true 的 Happy） |
| 是否必须改 C# | **否**（仅村线 GoOut 大立绘键路径） |
| ArmorNone Face 源目录是否有「开心」 | **无**（`Face/ArmorNone/` 仍为旧 9 表情拷贝命名）；Dress / Painting 目录有 `开心.png` |

**结论（任务 1）**：  
逻辑层 **零改 C# 即可切换**；卡点不在事件/键名/字典，而在 **资源正确性**（铠甲开心原图？尺寸对齐？）。若台本已选 Happy 但「整人消失」，才回头查场景里 Actor 是否挂在该 Painting 或其父节点上（`StoryFormPainting.Start` 只找 self / parent 的 `DialogueActorEx`）。

### 2.4 连衣裙 / 新游戏线大立绘（非本期必做，但须写清）

| Prefab / 脚本 | 期望 Happy 键 | 现状 |
|---------------|---------------|------|
| `YaerPainting`（空子类 → 基类 `faceType.ToString()`） | 脚本找裸名 `Happy`；Prefab 实际是 `Dress_Crown_*` | **无** `Dress_Crown_Happy`；且脚本键与 Prefab 命名长期不一致（0727 OPEN Q7） |
| `NewGameYaerPainting` | `Dress_Crown_Happy` | 脚本会拼该键；Prefab 侧未见 `Dress_Crown_Happy` |
| `GuShaPainting`（对照，不改） | 裸枚举 `Happy` | 古莎已有 Happy 资源与用法 |

仅加 GoOut 节点 **不会**覆盖新游戏线 / 连衣裙立绘。

### 2.5 小头像（必须并列表）

| 项 | 现状 |
|----|------|
| 路径公式 | `Avatar_Yaer_{衣服}_{头饰}.spriteatlas` |
| 四套图集 | `Dress_Crown` / `Armor_NoHeadWear` / `Armor_Crown` / `Armor_ArmorHead` |
| Avatar PNG | 四目录均仅 9 张：`Surprised/Daze/ChiBie/Smug/Sad/Laugh/Smile/Unhappy/VerySurprised` — **无 `Happy.png`** |
| 取键 | `faceType.ToString()` → `"Happy"` |
| 缺图行为 | `GetSprite` → null → `actorPortrait` / `imgAvatar` **SetActive(false)**，字幕继续 |
| 历史头像 | 同源 Loader，同样缺图隐藏 |
| 是否必须改代码 | **否**（补 PNG + Pack 图集即可）；改代码帮不上忙 |

### 2.6 Mask 内嵌立绘（是否本期范围）

| 项 | 说明 |
|----|------|
| Prefab | `NormalDialogueNewPanel` → `Bottom/Mask/YaerAvatarRoot/GoOutStoryYaerPainting`（母体实例） |
| 技术说明结论 | Prefab 摆位已落地；**变脸 / 换装接线属下一轮**；旧 `Portrait` Active=false |
| 与本次 Happy | 母体加了 Happy 后，Mask 内实例**资源上**会带上该脸；但运行时 **不会**因 SayEx 自动切脸 |
| 本期建议 | **另开轮**；不要写成「对话调用 Happy 已覆盖 Mask」 |

### 2.7 文档手册缺口（只记施工建议）

| 手册 | 缺口 |
|------|------|
| 0601 表情对照 | 雅儿常用表写「每套 9 表情、不含 Happy」；Happy 备注偏古莎/夏尔 → 需补「雅儿 GoOut 大立绘已有 / 小头像待补」 |
| 0614 换图手册 | Faces 列表仍为 9 张，无 Happy → 需补节点与换图步骤 |

---

## ③ 用户需要做什么（检查清单）

### A. 只验村线 GoOut 大立绘 Happy（最小闭环）

1. 打开使用 `GoOutStoryYaerPainting` 的对话（DialogDebug 或村线雅儿说话）。  
2. NodeCanvas `SayEx`（或 CSV FaceType）选 **`Happy`**。  
3. Hierarchy 展开该立绘 → `Faces`：应只亮 **`Armor_NoHeadWear_Happy`**，其它脸关掉。  
4. Game 视图肉眼确认：开心脸是否完整、是否铠甲风格；若只有一小块脸或风格像裙子 → 按 §2.3 换图/对齐 SizeDelta（**属资源修正，不是再写 C#**）。  
5. 再切回 `Smile`，确认能正常回来。

### B. 小头像（预期当前失败）

1. 同句 Happy：看 `Bottom/Portrait`（即便 Active 被藏，逻辑上 Loader 仍会拿 null）。  
2. 打开历史：对应行头像应隐藏（与现网缺图行为一致）。  
3. 若要小头像也 Happy：四套 `Avatar/.../Happy.png` + 重 Pack 四套图集后再测。

### C. 勿误判的项

- 不要用「Mask 窗里的人」判断本期 Happy 是否接通（未接线）。  
- 不要改 Faces 节点名为中文。  
- 连衣裙线另测：当前选 Happy 预期切不到脸（缺节点 + `YaerPainting` 键名问题）。

---

## ④ 给程序看的补充

### 4.1 缺口表（任务 2 · 必出）

| 环节 | 现状 | 期望 | 是否必须改代码 | 备注 |
|------|------|------|----------------|------|
| `DialogueFaceType.Happy` | **有** | 可在 SayEx/CSV 选用 | 否 | 序号已定，勿挪位以免旧序列化脏数据 |
| GoOut Faces 节点 | **有** `Armor_NoHeadWear_Happy`，已绑图 | 键名对齐 + 铠甲开心原图 + 尺寸对齐其它脸 | 否（资源/Prefab 美术） | 当前绑 Dress `开心.png`；SizeDelta 异常 |
| `YaerPainting` Faces | **无** `Dress_Crown_Happy`；脚本用裸枚举名 | 若全线可调：补节点并理顺键名 | 视范围；改键属接线轮 | 与 0727 Q7 同源 |
| `NewGameYaerPainting` | 脚本会拼 `Dress_Crown_Happy`；资源无 | 补 `Dress_Crown_Happy` | 否（资源） | 仅新游戏线需要时 |
| 四套 Avatar PNG + 图集 | **无** Happy | 子图名 `Happy` | 否 | 缺图 → 头像隐藏 |
| 历史头像 | 同源 Loader | 同小头像 | 否 | 补图即同步 |
| Mask 内嵌立绘 | 有实例、**未接 FaceType** | 跟台本变脸 | **是（另轮接线）** | 本期范围外 |
| 0601 / 0614 手册 | 仍写 9 表情 | 补 Happy 行 | 否（文档） | 施工建议 |

### 4.2 施工员最小改动建议清单（只建议，本阶段不施工）

#### 方案 A — 仅保证村线大立绘 Happy（最小）

| 步骤 | 文件 / 操作 | 改代码？ |
|------|-------------|----------|
| A1 | 确认 / 替换 `GoOutStoryYaerPainting` → `Faces/Armor_NoHeadWear_Happy` 的 Source Image 为**铠甲无头饰开心**全尺寸立绘（对齐其它脸 ≈1078×1497、Pos 归零或与同套一致） | 否 |
| A2 | 台本 / DialogDebug 用 `FaceType=Happy` 验收 | 否 |
| A3 | （可选）更新 0614 Faces 列表 + 0601 备注 | 否 |

**不改**：`GoOutStoryYaerPainting.cs`、`StoryFormPainting.cs`、枚举、CSV 工具、古莎。

#### 方案 B — 大立绘 + 字幕条小头像同步

在 A 基础上追加：

| 步骤 | 文件 / 操作 | 改代码？ |
|------|-------------|----------|
| B1 | 于 `ArtRes/.../Yaer/Avatar/{Dress\|ArmorNone\|ArmorCrown\|Armor}/` 各放 `Happy.png`（名=枚举） | 否 |
| B2 | 重 Pack 四套 `Avatar_Yaer_*.spriteatlas` | 否 |
| B3 | 验收：Portrait（若重新启用）与历史行显示 Happy；缺一套则对应服装空白 | 否 |

仍 **不必改** `DialogueAvatarLoader`（契约已是 `GetSprite("Happy")`）。

#### 方案 C — 明确本期不做（写入开放问题）

- `Dress_Crown_Happy` / 新游戏线全覆盖  
- Mask 头像 FaceType 接线（0727 下一轮）  
- 为可读性把节点改成中文名  
- 改古莎  

### 4.3 相关文件清单

| 类别 | 路径 |
|------|------|
| 枚举 | `Assets/Scripts/Game/Static/Enum/Role/DialogueFaceType.cs` |
| GoOut 立绘脚本 | `.../Painting/GoOutStoryYaerPainting.cs` |
| 立绘基类 | `.../Painting/StoryFormPainting.cs` |
| GoOut Prefab | `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` |
| 连衣裙 Prefab | `Assets/Prefabs/DialougeProtrait/YaerPainting.prefab` |
| Actor / 字幕 | `DialogueActorEx.cs` / `DialogueTMPUGUI.cs` / `StatementNodeEx.cs` |
| 小头像 | `DialogueAvatarLoader.cs` / `DialogueAvatarPathHelper.cs` |
| 对话 UI | `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` |
| Happy 当前绑图 | `Assets/ArtRes/UI/Story/DialogueForm/Yaer/Face/Dress/雅尔游戏中立绘/表情/开心.png` |

### 4.4 开放问题（已追加 `OPEN_QUESTIONS.md`）

见下方摘要；详细表以 OPEN 文件为准。

| ID | 问题 | 施工默认建议 |
|----|------|--------------|
| Q1 | 连衣裙 / NewGame 是否本期必做 `Dress_Crown_Happy`？ | **否**，另开轮 |
| Q2 | 小头像四套是否必须同步上 Happy？ | 仅大立绘验收可先不做；要「对话头像也开心」则做方案 B |
| Q3 | Mask 内嵌立绘是否本期跟脸？ | **否**，属 0727 接线轮 |
| Q4 | GoOut Happy 是否接受暂用 Dress `开心.png`？ | **不建议长期**；应换铠甲开心并校正 Rect |
| Q5 | `YaerPainting` 裸枚举键 vs `Dress_Crown_*` 不一致是否顺带修？ | **否**（禁止借题发挥）；另案 |

---

## ⑤ 与「施工员续跑」的衔接

侦探结论已足够开施工员，但建议 **先定范围**：

1. **只验收村线大立绘** → 方案 A（几乎纯 Prefab/美术）。  
2. **还要小头像** → 方案 A + B。  
3. Mask / 连衣裙 → 明确写进 OPEN，**不要塞进本轮 Happy 最小闭环**。

施工员 Prompt 模板见：`Assets/Doc/提示词/0803/雅儿立绘新增Happy表情_接入表情系统_架构侦探提示词.md` 文末「施工员续跑」段（报告通过后再贴）。
