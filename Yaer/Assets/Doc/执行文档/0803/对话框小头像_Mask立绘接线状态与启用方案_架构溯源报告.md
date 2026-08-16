# 对话框小头像 · Mask 立绘接线状态与启用方案 — 架构溯源报告

**文档版本**：v1.0（2026-08-03）  
**文档性质**：【架构侦探】只读溯源 + **可交给施工员的接线方案**（**禁止改代码 / Prefab / 图集**）  
**范围**：`NormalDialogueNewPanel` 的 Mask + `YaerAvatarRoot` 立绘是否已启用；「四套图集」含义；如何最小侵入让 Mask 立绘跟台本 `FaceType` 换脸  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0803/对话框小头像_Mask立绘接线状态与四套图集含义_架构侦探提示词.md`
- `Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`
- `Assets/Doc/执行文档/7月/0727/对话系统_主角对话框表情显示_架构溯源报告.md`
- `Assets/Doc/执行文档/0803/雅儿立绘新增Happy表情_接入表情系统_架构溯源报告.md`
- Prefab / 脚本静态阅读

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**现状是「视觉靠 Mask 壳、逻辑仍走旧图集」：五个 Painting 已嵌进 `YaerAvatarRoot`，但没有任何代码驱动显隐/变脸；运行时仍 `DialogueAvatarLoader` → `actorPortrait`（节点现名 `Yaer`，Active 默认关，但有 Sprite 时会被 `OnGetAvatar` 重新打开）。推荐新建薄组件挂在 `YaerAvatarRoot`，订阅 `DialogueTMPUGUI.OnGetNewStatement`，按角色互斥显隐 + 调各 Painting 的 `UpdateFace`；旧图集可保留给历史，字幕头像以 Mask 为真源。四套图集是雅儿小头像 PNG 图册，和 Mask 立绘不是一回事。**

---

## ② 原因（生活类比 + 技术锚点）

### 生活类比

- **Mask 窗**：字幕条左侧挖好的相框。  
- **YaerAvatarRoot 里的五个 Painting**：框后面摆好的人偶，还没接遥控器。  
- **四套图集**：另一本「微信头像贴纸册」，专给旧 `Yaer` Image / 历史列表用。  
- 现在：人偶在框后站着（有的默认还亮着古莎），遥控器仍在往旧贴纸框贴图——贴纸框默认关着，但一贴成功又会被代码打开。

### Q1：是不是只放了 Prefab、代码未绑定？

**是。** 证据：

| 证据 | 内容 |
|------|------|
| 0727 Prefab 阶段文档 | 明确「本阶段不改 C#」「变脸接线另开轮」 |
| `StoryFormPainting.Start` | 只找 **自身或父节点** 的 `DialogueActorEx` |
| Mask 层级 | `YaerAvatarRoot` 无 Actor；Painting 父级是 Root，**不是**场景说话 Actor |
| 全项目搜索 | 无任何脚本引用 `YaerAvatarRoot` / Mask 头像 Presenter |
| Prefab 默认 Active | GoOut/Yaer/Amy/Aliy = **0**；仅 **Gusha = 1**（与技术说明「GoOut 默认可显示」已漂移） |

→ Mask 内 Painting **订不到** 说话人的 `OnRefreshAvatarEvent`，不会跟台本换脸。

### Q2：当前运行时真正刷脸走哪条？

```
SayEx FaceType
  → DialogueActorEx.RefreshAvatar
  → DialogueAvatarLoader.GetAvatar（读 Avatar_*.spriteatlas）
  → DialogueTMPUGUI.OnGetAvatar
  → actorPortrait.sprite = …；sprite≠null 则 SetActive(true)
```

旁路：`OnGetNewStatement` → 历史只记枚举，打开历史再走 Loader。  
场景大立绘（若挂在说话 Actor 上）另订 `OnRefreshAvatarEvent`——与 UI Mask **不是同一实例**。

### Q3：「四套图集」是什么？是不是 YaerAvatarRoot 下那些 Painting？

**不是。** 四套图集 = 雅儿小头像 SpriteAtlas（按衣服+头饰分册）：

| # | 全路径 | 存档条件（衣服 / 头饰） |
|---|--------|-------------------------|
| 1 | `Assets/GameRes/Atlas/Avatar/Avatar_Yaer_Dress_Crown.spriteatlas` | Dress + Crown |
| 2 | `Assets/GameRes/Atlas/Avatar/Avatar_Yaer_Armor_NoHeadWear.spriteatlas` | Armor + NoHeadWear |
| 3 | `Assets/GameRes/Atlas/Avatar/Avatar_Yaer_Armor_Crown.spriteatlas` | Armor + Crown |
| 4 | `Assets/GameRes/Atlas/Avatar/Avatar_Yaer_Armor_ArmorHead.spriteatlas` | Armor + ArmorHead |

源 PNG：`Assets/ArtRes/UI/Story/DialogueForm/Yaer/Avatar/{Dress\|ArmorNone\|ArmorCrown\|Armor}/`  
取图键：`faceType.ToString()`（如 `Smile`、`Happy`）。

| | 四套图集 | Mask 下 Painting |
|--|----------|------------------|
| 是什么 | 静态小头像贴纸 | 可切脸的立绘 Prefab 实例 |
| 谁用 | `DialogueAvatarLoader` → `actorPortrait` / 历史 | **尚无人用（待接线）** |
| GoOut 加 Happy | **不等于** 图集有 Happy | 母体有 `Armor_NoHeadWear_Happy` 则 Mask 接线后可亮 |

---

## ③ 用户需要做什么（拍板 / 验收清单）

报告通过后请拍板（详见 OPEN）：

1. **是否接受推荐方案**：`DialogueMaskAvatarPresenter` 挂 `YaerAvatarRoot` + 订阅 `OnGetNewStatement`？  
2. **雅儿服装本期范围**：先固定只开 GoOut，还是 Dress↔GoOut 跟存档？  
3. **历史头像**：本期仍走图集（建议），还是同期改 Mask？  
4. **图集 Happy**：Mask 真源后可缓补（建议缓）；历史要 Happy 再补。  
5. **拍板后**贴提示词文末【施工员】段开工。

验收（施工完成后）：DialogDebug 雅儿 `Happy` → Mask 内 `Armor_NoHeadWear_Happy` 亮；换古莎 → 只显 `GushaPainting`；旧 `Yaer` Image 保持关。

---

## ④ 给程序看的补充

### Part 1 — 双链路与 Prefab 现状

#### 1.1 当前显示源判定

| 选项 | 是否 |
|------|------|
| A 仅 Mask 立绘逻辑驱动 | **否**（无驱动代码） |
| B 仅旧 Portrait 图集 | **逻辑是**；节点现名 **`Yaer`**（非文档里的 `Portrait`），默认 Active=0 |
| C 视觉 A / 逻辑 B | **接近**：Mask 结构可见；逻辑仍 Loader；且 `OnGetAvatar` 在有 Sprite 时会把 `Yaer` **重新 Active=true**，可能与 Mask 叠影 |

**磁盘现状摘要**（`NormalDialogueNewPanel.prefab`）：

| 节点 | 状态 |
|------|------|
| `Bottom/Mask`（282×282，Image+Mask，Show Mask Graphic=关） | Active=1 |
| `Mask/YaerAvatarRoot` | Active=1；下挂 5 个 Prefab Instance |
| GoOut / YaerPainting / Amy / Aliy | 实例 `m_IsActive` **override=0** |
| GushaPainting | **override=1**（默认亮古莎——接线前换说话人不会关） |
| `Bottom/Yaer`（旧头像 Image，`actorPortrait`→`11400000`） | Active=0；有成功 Sprite 时会被代码打开 |

#### 1.2 双链路对照表

| | 旧小头像（现网逻辑） | Mask 立绘（已摆未接） | 场景大立绘（对照） |
|--|---------------------|----------------------|-------------------|
| 载体 | `Bottom/Yaer` Image | `YaerAvatarRoot` 下 Painting 实例 | 场景/对话树上的 Painting |
| 数据源 | `Avatar_*.spriteatlas` | Prefab `Faces` 子物体 | 同左，另一实例 |
| 驱动 | `RefreshAvatar` → Loader → `OnGetAvatar` | **无** | `OnRefreshAvatarEvent`（Actor 在 self/parent） |
| Happy | 雅儿图集无 → 藏头像 | GoOut 母体有节点，接线后可用 | 同 GoOut 母体规则 |

#### 1.3 为何 Mask Painting「订不到」Actor

```34:44:Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/StoryFormPainting.cs
        private void Start()
        {
            var dialogueActor = GetComponent<DialogueActorEx>();
            if (dialogueActor == null)
            {
                dialogueActor = transform.parent.GetComponent<DialogueActorEx>();
            }
            if (dialogueActor != null)
            {
                RegisterRefreshAvatarEvent(dialogueActor);
            }
```

UI 壳路径：`…/YaerAvatarRoot/GoOutStoryYaerPainting` → parent 无 Actor → **不订阅** → 静默摆位。

---

### Part 2 — 施工方案（给施工员）

#### 2.1 推荐方案一句话

**在 `YaerAvatarRoot` 挂新建薄组件 `DialogueMaskAvatarPresenter`，订阅同面板 `DialogueTMPUGUI.OnGetNewStatement(role, faceType, text)`：按角色互斥显隐五个 Painting，再对当前实例调用已有 `StoryFormPainting.UpdateFace(键)`；同时改 `OnGetAvatar`：Mask 启用时不再激活旧 `Yaer` Image（Loader 可继续跑给历史用）。**

**为什么最小侵入**

| 候选 | 评价 |
|------|------|
| 1. 只改 `DialogueTMPUGUI` 旁路 | 可行但 UI 脚本变肥；Presenter 更清晰 |
| **2. Presenter 挂 Root 订 OnGetNewStatement** | **推荐**：事件已有 Role+FaceType；历史已用同事件；不改 Actor 契约；Painting 不用假挂 Actor |
| 3. 改 `DialogueAvatarLoader` | **不适合**：Loader 只产 Sprite，不懂 Prefab 显隐；硬塞破坏历史/其它入口 |
| 4. UI 壳补 `DialogueActorEx` 转发事件 | 多角色说话时一个 Actor 代表不了；易与场景 Actor 混淆 |

**备选（退路）**：若不便订 `OnGetNewStatement`，在 `DialogueTMPUGUI.Internal_OnSubtitlesRequestInfo` 于 `RefreshAvatar` 前后直接调用 Presenter 的 `Apply(role, faceType)`（多一行耦合）。

#### 2.2 分步施工清单（可勾选）

**脚本**

- [ ] **新建** `DialogueMaskAvatarPresenter.cs`（建议目录：`Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/` 或 `…/Story/Base/Control/`）
  - 序列化引用：五个 `StoryFormPainting`（或 GameObject + GetComponent）
  - 可选：`DialogueTMPUGUI` 引用；空则 `GetComponentInParent`
  - `OnEnable` 订 `OnGetNewStatement`，`OnDisable` 退订（禁止 Update 轮询）
  - `Apply(DialogueRoleName role, DialogueFaceType face)`：
    1. 全关五个实例  
    2. 按角色开一个（见下表）  
    3. `painting.UpdateFace(ResolveFaceKey(role, face))`  
  - 旁白 / `None`：全关 Mask 内容（或关整个 Mask）
- [ ] **小改** `DialogueTMPUGUI.OnGetAvatar`：若 Presenter 存在且启用，**不要** `actorPortrait.SetActive(true)`（保持旧框关死）；或加 `[SerializeField] bool useMaskAvatar` 默认 true  
- [ ] **可选小改** `GoOutStoryYaerPainting`：把 `ResolveGoOutFaceKey` 提成 `public static`，供 Presenter 复用（避免复制字符串规则）  
- [ ] **不改** `DialogueAvatarLoader`（历史继续用）  
- [ ] **本期不改** `YaerPainting` 键名大修（若开 Dress：由 Presenter **直接传** `Dress_Crown_{face}` 进 `UpdateFace`，绕过基类裸枚举订阅）

**Prefab**

- [ ] `NormalDialogueNewPanel`：`YaerAvatarRoot` 加 Presenter，拖好五个引用 + TMPUGUI  
- [ ] 确认旧 `Yaer`（actorPortrait）保持默认关；接线后逻辑也不再打开  
- [ ] 初始：五 Painting 可全关，或保留；Presenter 首句会纠正（建议 Prefab 默认全关，避免残古莎）

**角色显隐规则**

| `DialogueRoleName` | 显示的实例 | Face 键 |
|--------------------|------------|---------|
| `Yaer` | 见服装规则 | GoOut：`Armor_NoHeadWear_{face}`（Normal→Smile）；Dress：`Dress_Crown_{face}` |
| `Gusha` | `GushaPainting` | 裸枚举名（`Happy` 等）；走 `GuShaPainting.UpdateFace` 的服装层逻辑 |
| `Amy` | `AmyPainting` | 裸枚举（现有 `Normal`/`Scared` 等） |
| `Aliy` | `AliyPainting` | 裸枚举（`Normal`/`Scared`/`CloseEyes`） |
| 其它（King/Lai…） | 本期无实例 → Mask 空 | Out；勿假用雅儿 |

**雅儿服装规则（本期建议）**

| 方案 | 规则 | 推荐 |
|------|------|------|
| **MVP** | 雅儿一律 `GoOutStoryYaerPainting`；头饰仍靠 GoOut 现有 `SetDefaultPainting`（ArmorHead/Crown 开关） | **推荐先做**（村线主路径） |
| 完整 | `Clothes==Dress` → `YaerPainting`；否则 GoOut；并切头饰 | 第二小步；需有 `Dress_Crown_*` 脸（含 Happy 则另补） |

**旧 Portrait / Loader**

| 项 | 建议 |
|----|------|
| Loader | **保留**（历史 `HistoryDialogueBox` 仍依赖） |
| 旧 `Yaer` Image | **藏死**：不删引用，避免空引用；OnGetAvatar 不再激活 |
| 双轨过渡 | 仅历史走图集；字幕头像 Mask 为唯一真源 |

#### 2.3 本期范围边界

| In | Out |
|----|-----|
| 字幕条 Mask 跟 `FaceType` 换脸 | 不必补四套图集 Happy（Mask 真源后字幕不依赖） |
| 五角色互斥显隐（已摆的五个） | 历史列表改 Mask（OPEN：另案） |
| 雅儿 MVP：固定 GoOut + FaceType | 一次性重写 DialogueForm |
| Happy：GoOut 已有节点即可验 | 改 Faces 中文名；King/Lai 等未摆角色 |
| 弱化旧 `Yaer` 图集赋值 | 修 `YaerPainting` 裸键 vs Dress_Crown 架构债（可另案；Dress 路径用 Presenter 传键规避） |

**取舍理由**：开发者目标是「已摆 Prefab 成为运行时真源」；继续维护小头像矩阵与 Mask 双轨会永久分叉。历史仍用图集可避免本轮改 History UI。

#### 2.4 验收清单

1. DialogDebug / 村线：雅儿 SayEx=`Smile` → Mask 内 GoOut，亮 `Armor_NoHeadWear_Smile`。  
2. 同角 `Happy` → 亮 `Armor_NoHeadWear_Happy`（资源尺寸问题见 0803 Happy 报告，属美术，不挡接线逻辑）。  
3. 切古莎 `Happy` → 仅 `GushaPainting`，脸为 `Happy`。  
4. 切 Amy / Aliy → 对应实例，其它关。  
5. 旁白无 Actor → Mask 内容空/关；不 NRE。  
6. Hierarchy：`Bottom/Yaer` 全程不出现（不被 OnGetAvatar 打开）。  
7. 打开历史：仍能出图集头像（或旧行为）；不要求 Mask。

#### 2.5 风险与开放问题

| 风险 | 说明 |
|------|------|
| Prefab 默认只亮 Gusha | 接线前/Presenter 未跑时误显；施工应默认全关 + 首句驱动 |
| `YaerPainting` 键不一致 | Presenter 必须传 `Dress_Crown_*`，不能指望基类订阅 |
| GoOut Happy 尺寸/Dress 图 | 逻辑可亮，观感可能错（0803 已记） |
| `OnGetAvatar` 再开旧框 | 必须改，否则双影 |
| 未摆角色说话 | Mask 空，可接受；勿回退乱显上一角色（Presenter 应全关） |
| 场景大立绘与 Mask | 两套实例；场景 Actor 上 Painting 仍走原事件，互不替代 |

开放问题已追加 `OPEN_QUESTIONS.md`（本节 Mask 接线 · 0803）。

#### 2.6 预估改动文件列表

| 路径 | 动作 |
|------|------|
| `…/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs`（新建，路径可微调） | 新建 |
| `…/NodeCanvasExtend/DialogueTMPUGUI.cs` | 小改 OnGetAvatar / 可选开关 |
| `…/Painting/GoOutStoryYaerPainting.cs` | 可选：公开 ResolveGoOutFaceKey |
| `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` | 挂 Presenter、拖引用、默认 Active |
| `Assets/Doc/OPEN_QUESTIONS.md` | 已记拍板项（侦探阶段） |
| **不改** | `DialogueAvatarLoader`、四套图集、历史 Prefab、古莎/Amy 母体 Faces 命名 |

#### 2.7 Presenter 伪代码（施工对齐用）

```csharp
// 详细注释由施工员落地；此处仅钉契约
void OnStatement(DialogueRoleName role, DialogueFaceType face, string text)
{
    HideAllPaintings();
    var painting = ResolvePainting(role); // Yaer→GoOut或Dress；Gusha/Amy/Aliy
    if (painting == null) return;         // 未支持角色：Mask 空
    painting.gameObject.SetActive(true);
    painting.UpdateFace(ResolveFaceKey(role, face));
}
```

禁止：`Update` 里扫对话树；禁止改 Faces 中文名；禁止重写整棵 DialogueForm。

---

### Part 3 — 与 Happy / 图集关系（一句话）

Mask 接线完成后，雅儿字幕头像 Happy **只依赖** GoOut Prefab 节点，**不依赖** 四套图集是否有 `Happy.png`；图集 Happy 仅影响历史/旧框回退。
