# Village_Shop — 老板娘 Body×Face 子物体切换与 CSV 列设计 — 架构溯源报告

**文档版本**：v1.1（2026-08-27 · 第二轮修订 · BodyType **可选列**）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码/CSV**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_Shop.unity` → `商店界面合层`  
**用户拍板**：
- **`BodyType` 为可选列**（同 `English`/`Voice`；**表头无此列 = 旧表零改动**；有列且单元格空 = 继承上一句 / 默认 `Normal`）
- **不用** Dialogue 行 `Extra` 存身体；**不采用** `FaceType` 后缀（如 `Face2_Red`）
- 换脸改为 **Body/Face 子 GO 互斥 `SetActive`**

关联提示词：`Assets/Doc/提示词/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构侦探提示词.md`  
上轮报告：`0827/Village_Shop_老板娘表情变化系统_架构溯源报告.md`（v1 · SR 单槽换图，**已被用户 Hierarchy 改版 supersede**）  
台本参考：`0601/Village_商店老板娘特殊交互_对白台本_执行说明.md`（Speaker **`店`**）

---

## ① 结论一句话

**拍板：`BodyType` 是可选列（不强制改旧 CSV）；仅商店/老板娘新表需要时可加表头，旧表无此列照常 Import。`Extra` 仍专供 Choice/Anim；`FaceType` 按 Speaker 分流——`店` 填 `Face1～5`，雅/古仍填 `Laugh/Cry`。运行时 Toggle 切 `Body`/`Face` 子物体，扩展 Subtitles 链路驱动；拒绝 `FaceType` 后缀混写 Body。**

---

## ② 原因（通俗）

### 2.1 为何不用 `Extra` 存 Body

| 行类型 | `Extra` 现网职责 | 若 Dialogue 也写 `Red` |
|--------|------------------|------------------------|
| **Dialogue** | **导入忽略**，不落盘 | 须改 Parser + GraphBuilder + 运行时；与 Choice/Anim **同列不同义** |
| **Choice** | 选项文案 `\|` 分隔 | 策划填表易混 |
| **Anim** | 动画键 `Anim_*` | 同上 |

生活类比：`Extra` 已是 Choice 的「选项栏」和 Anim 的「动画键栏」；Dialogue 的身体变体应单独开 **`BodyType` 列**，与 **`FaceType` 并列**——但和 `English`/`Voice` 一样，**有表才用，没有就跳过**，不必给全项目 CSV 批量加列。

### 2.2 为何不采用 `FaceType` 后缀（如 `Face2_Red`）

| 问题 | 说明 |
|------|------|
| 两维挤一列 | 脸与身独立组合（3×5），后缀难读、难校验 |
| 只改身不改脸 | 须写成 `Face4_Red`，策划须记得当前脸 |
| 与雅/古混用 | `Laugh` 无后缀、`Face2_Red` 有后缀，Import 分流更脆 |
| **裁定** | **否决**；Body 仍用独立 **`BodyType` 可选列**表达 |

### 2.3 旧 CSV 零迁移（可选列口径）

| 表类型 | 要不要改 | 行为 |
|--------|----------|------|
| 现有雅/古/NPC 表（无 `店`、无 BodyType 列） | **不改** | Import 与现网一致 |
| 新/商店老板娘表 | **可选**加 `BodyType` 列 | 仅变身行填 `Red`/`YinXian`；多数行留空=继承 |
| 全项目批量加 BodyType 列 | **禁止要求** | 迁移成本过高且无必要 |

### 2.4 为何 `FaceType=Face1` 现网 Import 必炸

`DialogueCsvParser.Validate` L138–144：Dialogue/Anim 行非空 `FaceType` 必须 `Enum.TryParse<DialogueFaceType>` —— **`Face1`/`Face2` 不在枚举内**，校验直接失败。

### 2.5 与 v1 报告 diff

| 项 | v1 报告（0827 上午） | 用户现网 + 本轮拍板 |
|----|---------------------|---------------------|
| Hierarchy | `正常体` + `表情1` 两个 SR | **`Body/Normal|Red|YinXian` + `Face/Face1～5`** |
| 切换方式 | `sprite` 单槽换图 | **子 GO 互斥 `SetActive`** |
| 已有代码 | 设计 `ShopkeeperFaceController`（SR） | **已施工 SR 版**，绑旧 SR 引用，**与 Toggle 树冲突** |
| CSV | 未接 | 用户试填 `Extra=Red` + `FaceType=Face1` → **Extra 无效 + Face 校验失败** |

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | **旧 CSV 不用改**；仅新商店表**可选**加 `BodyType` 列 | |
| 2 | **店** 行：`FaceType=Face1`～`Face5`；需变身时才填 `BodyType=Red` 等；**Extra 留空** | |
| 3 | **雅/古** 行：与现网相同；无 BodyType 列或列内留空 | |
| 4 | Import 前：`DialogueSpeakerMapping` 增加 **`店` → `老板娘`** | |
| 5 | 场景默认 **仅 `Body/Normal` + `Face/Face1` Active** | |
| 6 | 旧表 Import 回归 + 新商店表冒烟 | 旧表无报错；店句可切 Body+Face |
| 7 | 本期仍 **不做** 首次进店存档 / 藏 UI / 黑屏 | |

---

## ④ 给程序

### A. 现网 Hierarchy 核实（磁盘已保存）

**路径**：`Village_Shop.unity` → `商店界面合层`（根 `fileID: 3771588061535772191`）

```
商店界面合层                    ← ShopkeeperFaceController + ShopkeeperFaceDebugInput（v1 SR 模式）
├── 背景                        ← SpriteRenderer · SortingOrder 1
├── Body                        ← 空 Transform 父节点
│     ├── Normal                ← SR · 正常体.png · Active ✅
│     ├── Red                   ← SR · 脸红体.png · Active ❌
│     └── YinXian               ← SR · 阴险体.png · Active ❌
└── Face                        ← 空 Transform 父节点
      ├── Face1                 ← SR · 表情1.png · Active ❌（应为默认）
      ├── Face2                 ← SR · 表情2.png · Active ⚠️ 误开
      ├── Face3～Face5          ← SR · Active ❌
```

| 项 | 裁定 |
|----|------|
| 子 GO 名 | 与用户一致：`Normal`/`Red`/`YinXian`、`Face1`～`Face5` |
| 组件 | 各子节点 **SpriteRenderer**（无 SortingGroup）；脸 SR SortingOrder **22**，身 **21** |
| v1 控制器绑定 | 仍绑 `bodyRenderer→Normal`、`faceRenderer→Face1` 的 **SR 换图**；**未**走 Toggle |
| Setup Editor | `ShopkeeperFaceSetupEditor` 仍 `Find("正常体")`/`Find("表情1")` —— **已失效**，须改 `Body`/`Face` |

### B. CSV 列设计裁定（用户拍板 · BodyType **可选**）

#### B.1 Extra vs BodyType vs FaceType 后缀

| 维度 | A · Extra 存 Body | **B · BodyType 可选列（拍板）** | C · FaceType 后缀 `Face2_Red` |
|------|---------------------|--------------------------------|--------------------------------|
| 策划可读性 | 差 | **好** | 差 |
| 与 Choice/Anim Extra 冲突 | **高** | **无** | 无 |
| **旧 CSV 迁移** | 须改语义 | **零改动**（无列即跳过） | 零列但语法混 |
| Parser 改动 | 大 | **+1 可选列**（`BodyTypeIndex=-1` 时当无列） | 自定义 split |
| **推荐** | ❌ | **✅ 拍板** | ❌ 否决 |

**可选列实现要点**（对齐 `English`/`Voice`）：

```
表头无 BodyType     → BodyTypeIndex = -1 → 全表 bodyType="" → 店行默认 Normal / 继承
表头有 BodyType     → 读列；单元格空 → 继承上一句 Body
非店 Speaker 有值   → Warning 或 Import 拒（Body 仅店用）
```

#### B.2 FaceType：F1 按 Speaker 分流（拍板，不新建 ShopFace 列）

| 维度 | **F1 · Speaker 分流 FaceType** | F2 · 新建 ShopFace 列 | F3 · 扩 DialogueFaceType |
|------|-------------------------------|------------------------|---------------------------|
| 策划表宽 | 不变 | +1 列 | 不变 |
| 雅/古兼容 | **完全兼容** | 完全兼容 | **污染枚举** |
| 改动面 | Validate + GraphBuilder + 运行时 | 更大 | 否决 |
| **推荐** | **✅** | 备选 | ❌ |

#### B.3 策划填表示例

**（1）旧表 · 无需改动** — 与现网一致，无 BodyType 列：

```csv
ID,Type,Speaker,Text,English,Next,Extra,FaceType,Voice
1,Dialogue,雅,……,……,2,,Smile,
```

**（2）新商店表 · 可选加 BodyType 列** — 仅变身行填写：

```csv
ID,Type,Speaker,Text,Next,Extra,FaceType,BodyType
1,Dialogue,店,欢迎光临。,2,,Face1,
2,Dialogue,店,哼，你有钱吗？,3,,Face2,Red
3,Dialogue,雅,钱的话我还是有带的。。。,4,,Daze,
4,Dialogue,古,这……这是哪门子的钱？！,END,,Cry,
5,Choice,店,要买什么？,6|END,生命珠|离开,,
```

| 列 | 店 | 雅/古/NPC |
|----|-----|-----------|
| **BodyType** | 表**无列**→全程 `Normal`；**有列**时 `Red`/`YinXian` 或**空=继承**（首句空→`Normal`） | 无列或**必须空** |
| **FaceType** | `Face1`～`Face5`（空=继承；首句空→`Face1`） | `Laugh`/`Cry`/…（不变） |
| **Extra** | **空** | **空**（Choice/Anim 除外） |

**BodyType 合法值（CSV 对外 = Hierarchy GO 名）**：

| CSV / GO | 内部枚举 `ShopkeeperBodyType` |
|----------|-------------------------------|
| `Normal` | `Normal` |
| `Red` | `Blush` |
| `YinXian` | `Sinister` |

### C. FaceType 导入冲突（已验证）

1. **`Face1`/`Face2` 现网 Import**：**必失败** —— `DialogueCsvParser.Validate` L140 `Enum.TryParse<DialogueFaceType>` 不通过。  
2. **分流校验规则（施工）**：

```csharp
// 伪代码 · Speaker 映射后 actorName == "老板娘"（或 csvSpeaker == "店"）
if (IsShopkeeperSpeaker(row, mapping)) {
    // FaceType → ShopkeeperFaceType；空 → 继承/默认 Face1
    // BodyType → ShopkeeperBodyType；空 → 继承/默认 Normal
    // 禁止 Laugh/Angry 等 DialogueFaceType 字符串（防误用）
} else {
    // FaceType → DialogueFaceType（现逻辑）
    // BodyType 必须空
}
```

3. **数据存哪**（最小 diff 推荐）：

| 层 | 改动 |
|----|------|
| `DialogueRow` | +`bodyType` 字符串 |
| `DialogueCsvColumnMap` | +`BodyTypeIndex`（`FindColumnIndex("BodyType")`；**-1 = 表无此列**，`MinRequiredFieldCount` **不**把 BodyType 算进必需列） |
| `StatementNodeEx` | +`BBParameter<string> ShopBodyRaw` + `ShopFaceRaw`（或强类型 enum BB） |
| `SubtitlesRequestInfoEx` | +`ShopkeeperBodyType Body` + `ShopkeeperFaceType Face` + `bool UseShopkeeperPortrait` |
| `StatementNodeEx.OnExecute` | 店句：填 `UseShopkeeperPortrait=true` + Body/Face；`FaceType` BB 可置 `None` |

**不采用**：把店 Body 塞进 `DialogueFaceType`；**不采用**：Dialogue 行复用 `Extra`。

### D. Toggle 控制器设计（替代 v1 SR）

#### D.1 API（保留对外名，改内部实现）

```csharp
public void SetBody(ShopkeeperBodyType body);   // Body 下互斥 Active
public void SetFace(ShopkeeperFaceType face);   // Face 下互斥 Active
public void Apply(ShopkeeperBodyType body, ShopkeeperFaceType face);
public void ResetDefault();                     // Normal + Face1
```

- **内部**：Awake 缓存 `Body`、`Face` Transform；`SetBody` 关 `Normal/Red/YinXian` 再开目标；`SetFace` 同理。  
- **SortingOrder**：已在各 SR 上配好（21/22），Toggle **不必**再调。  
- **删除/废弃**：v1 的 `bodyRenderer`/`faceRenderer` 单槽换图、`BindFaceSprites` 序列（或仅 Editor 烘焙用）。

#### D.2 空列继承（拍板）

| 列空 | 行为 |
|------|------|
| `BodyType` 空 | **保持上一句 Body**；对话开始前 `ResetDefault()` → `Normal` |
| `FaceType` 空（店） | **保持上一句 Face**；对话开始前 → `Face1` |

#### D.3 v1 → Toggle 迁移清单

| # | 项 |
|---|-----|
| 1 | `ShopkeeperFaceController` 改 Toggle；常量改为 `BodyChildName="Body"`, `FaceChildName="Face"` |
| 2 | `ShopkeeperFaceSetupEditor` 改 Find `Body`/`Face`；Setup 后调用 `ResetDefault()` 校正 Active |
| 3 | `ShopkeeperFaceDebugInput` 仍调 `SetFace`/`SetBody`（API 不变） |
| 4 | 场景 YAML：校正默认 Active（**仅 Normal + Face1**） |
| 5 | `Start()` 里 `ResetDefault()` 避免 Editor 误开 Face2 |

### E. 对白桥接链路

```
CSV Import
  → StatementNodeEx（ShopBodyRaw / ShopFaceRaw 或 enum BB）
Play SayEx
  → SubtitlesRequestInfoEx（+ UseShopkeeperPortrait / Body / Face）
  → DialogueTMPUGUI.Internal_OnSubtitlesRequestInfo
       if (info.UseShopkeeperPortrait)
         ShopkeeperFaceRegistry.Instance?.Apply(body, face)
       else if (actor != null)
         actor.RefreshAvatar(info.FaceType, …)   // 雅/古原链
         OnGetNewStatement → DialogueMaskAvatarPresenter
```

| 项 | 现网 | 施工 |
|----|------|------|
| `DialogueSpeakerMapping` 默认 | **无 `店`** | 增 **`店` → `老板娘`** |
| `DialogueRoleName` | **无 Shopkeeper** | 下期可加；Bridge 可先判 **Actor 参数名 == `老板娘`** |
| 订阅点 | 无 | **`DialogueTMPUGUI` 内 Subtitles 分支**（信息最全）；或独立 `ShopkeeperDialogueBridge` 订 `OnGetNewStatement`（**需扩展事件签名**，不推荐单独订旧事件） |
| 同屏双轨 | 首次进店：左雅/古 Mask + 右合层老板娘 | **店句只切合层**；雅/古句只切 Mask —— **互不 SetActive 对方** |
| 竞态 | `ShopkeeperFaceController.Start` → `ResetDefault` | 对白开始前若需指定脸，**对白树首句 CSV 显式写 Body/Face**；Mask `SetDefaultPainting` **不影响**合层 |

### F. 最小施工清单（给施工员）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | CSV 规范 | **`BodyType` 可选列**；旧表不改；新商店表见 §B.3 | P0 |
| 2 | `DialogueRow` + `DialogueCsvColumnMap` | 有列才读 BodyType；`BodyTypeIndex=-1` 时 `bodyType=""` | P0 |
| 3 | `DialogueCsvParser.Validate` | Speaker 分流 Face；**仅当表含 BodyType 列**时校验非店行 Body 须空 | P0 |
| 3b | 回归 | **随机抽 2～3 张旧 CSV Import**（无 BodyType 列）须与改前一致 | P0 |
| 4 | `DialogueCsvGraphBuilder` + 新 `ShopkeeperCsvDefaults` | 建图写 `StatementNodeEx` 店 BB；继承上一句可在 GraphBuilder 第二遍扫 | P0 |
| 5 | `StatementNodeEx` + `SubtitlesRequestInfoEx` | 携带店 Body/Face | P0 |
| 6 | `DialogueTMPUGUI` | 店句驱动 `ShopkeeperFaceRegistry` | P0 |
| 7 | `ShopkeeperFaceController` | **Toggle 重构** | P0 |
| 8 | `DialogueSpeakerMapping` | **`店`→`老板娘`** | P0 |
| 9 | `ShopkeeperFaceSetupEditor` | 对齐 Body/Face 树 | P1 |
| 10 | 用户 CSV Import 冒烟 | 修复 Face1 校验 | P0 |

**排除**：首次进店存档旗标、藏 `UI_Shop`、黑屏时序。

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Import **旧 CSV**（无 BodyType 列，如 `Village_村内雅古开场`） | 与改前一致，无新报错 |
| 2 | Import **新商店样例**（§B.3） | `Face1` 校验通过；有 `Red` 行落盘 |
| 3 | Play 对白 | 店句 Red+Face2；雅 Daze 走 Mask |
| 4 | 店句仅改 Face、BodyType 空 | 身保持（继承） |
| 5 | 无 BodyType 列的店表 | 身始终 Normal |
| 6 | Debug 1～5 / F1～F3 | Toggle 3×5 可调 |
| 7 | Console | 无 Missing GO；Body/Face 各 **恰好 1** 个 Active |

### H. 开放问题

| ID | 问题 | 建议 |
|----|------|------|
| Q1 | CSV 用 `Red` 还是 `Blush`？ | **对外 `Red`/`YinXian`**（与 GO 一致）；代码 enum 仍 `Blush`/`Sinister` |
| Q2 | 店行 `FaceType=Laugh` 是否 Import 拒？ | **建议拒** + 报错提示改 `Face1～5` |
| Q3 | 用户 CSV 文件路径 / Speaker SO | 施工时确认 Import 窗口映射含 **`店`** |
| Q4 | 禁止组合（YinXian+Face1）？ | 策划未禁则 **全组合合法** |
| Q5 | `DialogueRoleName.Shopkeeper` 何时加？ | **桥接可先靠 Actor 名**；枚举下期加 |
| Q6 | 是否强制全项目加 BodyType 列？ | **否（v1.1 拍板）**；仅新商店表可选加 |
| Q7 | FaceType 后缀方案？ | **否决**；见 §2.2 |

---

## 附录 · 现网导入器契约（2026-08-27 磁盘）

| 列 | Dialogue | Choice | Anim | 旧表 | 施工后 |
|----|----------|--------|------|------|--------|
| Extra | **忽略** | 选项文案 | 动画键 | 不变 | 不变 |
| FaceType | → `DialogueFaceType` | 忽略 | 同左 | 不变 | **店**→`ShopkeeperFaceType` 分流 |
| **BodyType** | **不存在** | — | — | **无列·跳过** | **可选列**；无列=Normal/继承 |

**旧表样例表头**（`Assets/Dialog/` 多数文件）：`ID,Type,Speaker,Text,English,Next,Extra,FaceType,Voice` — **施工不得要求补 BodyType**。

相关文件：

- `Assets/Editor/Tool/Dialogue/DialogueCsvParser.cs`  
- `Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs`  
- `Assets/Editor/Tool/Dialogue/DialogueRow.cs`  
- `Assets/Editor/Tool/Dialogue/DialogueSpeakerMapping.cs`  
- `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs`（**v1 SR · 待重构**）

---

*报告结束（v1.1）。施工顺序：Toggle 控制器 → CSV **可选** BodyType + Face 分流 → Subtitles 扩展 → **旧表回归 + 新商店表** Import 冒烟。*
