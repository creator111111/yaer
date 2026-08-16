# 对话框小表情 · 首句未跟 FaceType — 架构溯源报告

**文档版本**：v1.0（2026-08-04）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 图集 / CSV / 台本**）  
**范围**：字幕条 / Mask **小表情**首句不跟台本 `FaceType`；场景**大立绘**正常。不扩新表情、不改 CSV 内容。  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0804/对话框小表情_首句未跟FaceType_架构侦探提示词.md`
- 对照：`0803` Mask 接线报告；`NormalDialogueNewPanel` Prefab 技术说明
- 复现台本：`Assets/Dialog/Village_村内雅古开场对白台本.csv` 第 1 行 `FaceType=Laugh`

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**用户看到的「小表情」是 Mask 内 `GoOutStoryYaerPainting`（`useMaskAvatar=1`，旧 Portrait 已关），不是图集 Image。首句 Smile 的直接写入点是：Presenter 首次 `SetActive(true)` + `UpdateFace(Laugh)` 之后，同一帧稍后跑的 `Start` → `SetDefaultPainting()` **无条件**再 `UpdateFace("Armor_NoHeadWear_Smile")`，把 Laugh 盖掉。场景大立绘一直 Active，`Start` 早跑完，只吃 Actor 事件切 Laugh，故两边不一致。CSV/资产首句已是 Laugh（`_value:6`），**不必重导 CSV。**

---

## ② 原因（生活类比 + 时序）

### 生活类比

相框里的人偶（Mask 立绘）对话前被藏在柜子里（`HideAll` → Active=false）。第一句导演说「笑出声（Laugh）」——灯光师刚把 Laugh 脸贴上，人偶第一次出场时却执行了入场仪式「默认微笑（Smile）」，把刚贴好的脸撕掉换成 Smile。台上那尊一直站着的大立绘（场景实例）早就做过入场仪式，只会听对讲机换脸，所以它对、框里的错。

### 钉死两件事

| 问题 | 结论 |
|------|------|
| 小表情是谁？ | **Mask** `YaerAvatarRoot/GoOutStoryYaerPainting`（Presenter 驱动）。旧 `actorPortrait` 在 `useMaskAvatar=1` 时强制 `SetActive(false)`，不是截图真源。 |
| Smile 谁写的？ | **`GoOutStoryYaerPainting.SetDefaultPainting`**（由基类 `StoryFormPainting.Start` 调用），不是 CSV 默认、不是 Loader 默认 Smile、不是 FaceType 没传到 Presenter。 |

### 第一句时序图（脚本锚点）

```
SayEx FaceType=Laugh（CSV 已写；Generated 首项 FaceType._value=6=Laugh）
  → DialogueTMPUGUI.Internal_OnSubtitlesRequestInfo
       ├─ actor.RefreshAvatar(Laugh, …)     // 场景大立绘：订 OnRefreshAvatarEvent → UpdateFace(Laugh)
       │     （Loader 回调后才 Invoke 事件；场景实例 Start 早已跑完）
       └─ OnGetNewStatement(Yaer, Laugh, text)   // 同步，紧挨着 RefreshAvatar 调用之后
            → DialogueMaskAvatarPresenter.Apply
                 HideAll()
                 goOut.SetActive(true)           // ★ 首次激活
                   →（同帧）Awake（若尚未）建 facesDic；defaultFace 多为 Smile
                 goOut.UpdateFace("Armor_NoHeadWear_Laugh")   // Presenter 已设对
                 ……同帧稍后 / 首帧 Update 前……
                 StoryFormPainting.Start()
                   → SetDefaultPainting()
                   → UpdateFace("Armor_NoHeadWear_Smile")     // ★ 盖掉 Laugh
```

**大立绘分叉**：场景 `GoOutStoryYaerPainting` 对话前一直 Active → `Start`/`SetDefaultPainting` 开场前已执行完毕 → 首句只靠 Actor 事件切 Laugh，**不会再被 Start 盖一次**。

**为何多半「仅首句 / 该角色首次出场」**：`Start` 每个实例只跑一次。雅儿第二句再说话时 GoOut 已 Started，Presenter `UpdateFace` 会留住。古莎 `GuShaPainting` **未覆盖** `SetDefaultPainting`（空实现）→ 古莎首次出场一般**不会**被强制 Smile 盖掉（仅 Awake 按 defaultFace 初始化，随后被 Presenter `UpdateFace` 覆盖且 Start 不再改脸）。

### 替代假说排查结果

| 假说 | 结论 |
|------|------|
| 旧 Portrait + Loader 首句默认 Smile | **否**（`useMaskAvatar=1` 旧框保持关） |
| `OnGetNewStatement` 首句未发 / 订阅过晚 | **否**（与 `RefreshAvatar` 同协程同步 Invoke；Presenter `OnEnable` 订阅） |
| CSV/资产未写入 Laugh | **否**（CSV=`Laugh`；Generated `_value:6`=枚举 `Laugh`） |
| Prefab 默认亮 Smile 且 Presenter 未调 UpdateFace | **否**（Apply 会调；问题是之后被 SetDefault 盖掉） |
| `yaerUseGoOutOnly` 走错 Painting | **否**（Prefab=`1`，走 GoOut；键 `Armor_NoHeadWear_Laugh` 正确，非 Normal 回退） |

### 复现边界（验收时注意）

| 条件 | 预期 |
|------|------|
| 面板/GoOut **新实例**首次雅儿句（村内开场） | 小表情 Smile、大立绘 Laugh |
| 同一 UI 实例内第二句起 / 雅儿再次说话 | 小表情一般已跟 FaceType（Start 不再跑） |
| **销毁重建** Panel 后再开同一树 | 首句可再复现 |
| DialogDebug 若整页新建 | 可同现 |

→ 用户说「第一句话」有问题，与「仅该角色 Mask 实例首次激活」高度吻合，**不是**「Mask 永远默认 Smile」。

---

## ③ 用户需要做什么

### 不必做

- **不要**因本 bug 重导 CSV（首句已是 Laugh）。
- **不要**重写大立绘链路。

### 拍板（OPEN）

1. Mask 壳上的 GoOut：是否在无 Actor 时跳过强制 Smile？（推荐）  
2. 旧 Portrait：是否维持完全关闭？（现状已关，建议保持）  
3. 是否所有覆盖 `SetDefaultPainting` 强制脸的角色都要同样防竞态？（现网主要是 GoOut 雅儿）

### 验收清单（施工后）

1. 村内开场第一句「好漂亮的村子。」：小表情与大立绘均为 **Laugh**。  
2. 第二句古莎、再切回雅儿：小表情跟当前句 FaceType。  
3. **关掉对话 UI 再开**（或重进场景）再验首句（新实例）。  
4. DialogDebug 若可复现则加一条首句非 Smile 的雅儿句。

---

## ④ 给程序看的补充

### 4.1 根因表

| 环节 | 现状 | 是否首句特有 | 是否必须改代码 | 备注 |
|------|------|--------------|----------------|------|
| CSV FaceType=Laugh | **有** | — | 否 | 不必重导 |
| Generated 首节点 | `_value:6`=Laugh | — | 否 | 资产已写入 |
| OnGetNewStatement 参数 | 传 `Laugh` | 否 | 否 | 同步正确 |
| Presenter.Apply | SetActive+UpdateFace(Laugh) | 首次激活敏感 | 可选加固 | 逻辑本身对 |
| **SetActive→Start→SetDefaultPainting** | **强制 Smile 覆盖** | **是（每实例首次）** | **是** | **主根因** |
| UpdateFace(Laugh) | 曾设对，被盖 | 是 | — | |
| 旧 Portrait Loader | useMaskAvatar 下关闭 | — | 否 | 非真源 |
| 场景大立绘 | Actor 事件切脸，Start 已过 | 对照正常 | 否 | 勿改 |

### 4.2 关键代码锚点

| 文件 | 要点 |
|------|------|
| `DialogueTMPUGUI.cs` ~218–221 | `RefreshAvatar` 后同步 `OnGetNewStatement(role, info.FaceType, text)`；`useMaskAvatar` 关旧框 |
| `DialogueMaskAvatarPresenter.cs` Apply | `SetActive(true)` 后立刻 `UpdateFace`；Awake `HideAll` 保证 Mask 内 GoOut 默认 Inactive |
| `StoryFormPainting.cs` Start | 订 Actor（Mask 下通常 null）后 **总是** `SetDefaultPainting()` |
| `GoOutStoryYaerPainting.cs` SetDefaultPainting | **无条件** `UpdateFace("Armor_NoHeadWear_Smile")` |
| `NormalDialogueNewPanel.prefab` | `useMaskAvatar: 1`；Presenter `yaerUseGoOutOnly: 1`；嵌套 GoOut `m_IsActive: 0` |

### 4.3 施工员最小改动建议（只建议，不施工）

**推荐方案 A（根因处改，最小且稳）**  
在 `GoOutStoryYaerPainting.SetDefaultPainting`：若本实例**没有**可订的 `DialogueActorEx`（Mask 壳，与基类 Start 查找规则一致：自身或 parent），则：
- **仍可**处理 `armorHead` / `armorCrown`（若需要）；
- **不要**调用 `UpdateFace("Armor_NoHeadWear_Smile")`，把表情交给 Presenter。

场景大立绘有 Actor → 行为不变（开场仍默认 Smile，再听事件换脸）。

**备选方案 B（Presenter 加固）**  
`Apply` 在 `SetActive(true)` 后延迟到 `EndOfFrame` / 下一帧再 `UpdateFace`（或 Start 后再 Apply）。能盖过 SetDefault，但多一帧闪白/闪 Smile 风险，且每个强制默认脸的角色都要记得延后。

**备选方案 C**  
基类 `Start`：无 Actor 时不调 `SetDefaultPainting`。影响面大于仅 GoOut，须确认其它 Painting 依赖。

**禁止**：改 Faces 中文名；在 Update 扫对话树；重写整棵 DialogueForm；把「CSV 写错」当修复。

### 4.4 相关文件（施工预期）

| 优先级 | 路径 |
|--------|------|
| 推荐改 | `…/Painting/GoOutStoryYaerPainting.cs` |
| 可选加固 | `…/DialogueMaskAvatarPresenter.cs` |
| 勿动（本 bug） | CSV、Generated asset、场景大立绘 Actor 订阅、旧 Portrait 图集 |

### 4.5 开放问题（已追加 OPEN_QUESTIONS.md）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | Mask 实例是否应禁用/弱化强制 Smile 的 `SetDefaultPainting`？ | **是**（无 Actor 时跳过 UpdateFace Smile） |
| Q2 | 旧 Portrait 是否完全关闭？ | **保持** useMaskAvatar 下关闭 |
| Q3 | 是否所有角色首句都有同类竞态？ | 现网主风险是 **GoOut 雅儿**；古莎等空 SetDefault 通常无此盖写 |

---

## 施工员下一轮最小化清单（建议）

1. `GoOutStoryYaerPainting.SetDefaultPainting`：无 Actor → 不强制 Smile。  
2. （可选）Presenter 首帧后再 Apply 作双保险。  
3. 按 §③ 验收村内开场第一句 Laugh。  
