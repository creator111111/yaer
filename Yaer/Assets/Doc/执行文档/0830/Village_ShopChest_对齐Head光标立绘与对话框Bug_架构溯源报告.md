# Village_ShopChest — 对齐 Head（光标 + 雅儿大立绘 + 对话框不出现 Bug）— 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 施工拍板（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1 · NodeCanvas  
**交互**：`Trigger/Chest` → `Village_ShopChest`  
**金样**：`Trigger/Head` → `Village_ShopHead`（Catch / 雅大立绘 / 对话框已验收）  
**前序**：0830 门牌已改为 `Village_ShopChest`（见施工说明）；本期查 **壳层演出 + 光标**，不再改名

关联：`Village_商店点胸交互.asset` · `DialoguePreludeBuilder` · `ShopkeeperSpecialClickSetupEditor` · 0829 Head 立绘时序 / Catch 施工

---

## ① 结论一句话

**「对话框不出现」主因 D1：点胸图（Generated）只有 5 个 Statement，没有 `NormalDialogueUIAlpha`（也无 Fighting / 立绘 Alpha）——对白虽能 Trigger，对话框壳一直不淡入。雅大立绘物体/BB 已齐但 `m_Alpha=0` 且图内无淡入 → 永隐。Chest 场景无 `CursorChangeTrigger` → 悬停不变 Catch。拍板：对齐 Head——补壳层序 `Fighting → CanvasGroupAlpha(GoOut) → UIAlpha → 句`（或跑 Rebuild Chest 且 `fadeYaerPortrait=true`）；场景 Chest 挂 Catch；Setup 勿再对 Chest 关立绘淡入。**

---

## ② 原因（通俗）

点头像一场完整演出：先藏好战斗条、雅儿立牌淡进来、对话框再淡出来，然后说话。  
点胸像只把台词本塞进场——**开幕灯光和字幕条都没接**，所以玩家觉得「对话框都不会出现」；立牌默认透明又没人拉亮，雅儿大立绘也不见；胸上没贴「换手形」贴纸，悬停光标也不变。

门牌已经改对了（能找到房间），缺的是**房间里的灯和幕布**。

---

## ③ 用户检查清单（施工后 · 三件事）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 鼠标移入 Chest | 光标变 **Catch**（同 Head）；移出恢复 |
| 2 | Idle 点 Chest | **对话框出现**可读文案；藏 `UI_Shop` |
| 3 | 对白开始段 | **雅儿大立绘可见**，且 **先于**对话框（T1） |
| 4 | 店句 / 雅句 | 合层脸 + 雅大立绘/Mask 跟句 |
| 5 | 结束 | Idle；UI 显；热区开；光标正常；脸复位 |
| 6 | 点 Head | 回归：光标/立绘/对话框仍正常 |
| 7 | Console | `Village_ShopChest started=true`；无 Missing；无「立刻 onStoryEnd 无句」 |

---

## ④ 给程序

### A. 「对话框不出现」根因树

| # | 假说 | 结果 | 证据 |
|---|------|------|------|
| **D1** | 点胸图缺 `NormalDialogueUIAlpha` / Fighting 前奏 | ✅ **主因** | Generated：`StatementNodeEx×5`，**ActionNode×0**；无 UIAlpha / Fighting / CGAlpha |
| D2 | TriggerStory 未 started（旧名） | ❌ 非本期主因 | 常量已是 `Village_ShopChest`；门牌施工已落地 |
| D3 | Prefab `_boundGraph` 空、只跑外部 `_graph` | ⚠️ **加重** | Prefab `_boundGraphSerialization` 空；`_graph` → Generated（纯 Statement）→ 跑的就是无壳图 |
| D4 | Hide UI 挡住对话框 | 次要/否 | Special 藏的是 `UI_Shop`；对话框靠 UIAlpha 开壳，与 Head 同门 |
| D5 | 句立刻 End 无可见帧 | ❌ | 5 句文案齐；缺的是可见壳，不是空图 |

**最小修法（对话框）**：在进首句前插入 **`NormalDialogueUIAlpha`（EndAlpha=1）**；金样对齐再加 Fighting + 雅 CGAlpha（见 §B）。

### B. Head ↔ Chest 金样对照表（磁盘 2026-08-30）

| 能力 | Head（金样） | Chest（现网） | 缺口 |
|------|--------------|---------------|------|
| 故事名 = Prefab | `Village_ShopHead` | `Village_ShopChest` | ✅ 门牌已齐 |
| 点击 → Special | Hotspot → Special | 同门 | ✅ |
| 悬停光标 | `CursorChangeTrigger` Catch（TargetState=1） | **仅** Collider+Hotspot，**无** Cursor | **P0** |
| 雅大立绘物体 | GoOut 嵌 + BB 绑 | ✅ 同构嵌+绑 | 物体齐 |
| 立绘初始 alpha | `m_Alpha=0` | `m_Alpha=0` | 靠淡入 |
| 立绘淡入 | ✅ `CanvasGroupAlpha(GoOut)` id=13 | ❌ 图内无 | **P0** |
| 对话框淡入 | ✅ `NormalDialogueUIAlpha` EndA=1 Dur=1 Delay=0.5 PrepareMask=true | ❌ 无 | **P0 · 框不出现** |
| Fighting 前奏 | ✅ `FightingPanelVisible` id=0 | ❌ 无 | 建议抄齐 |
| 图节点序 | `0→13→1→2…` = Fighting → **CGAlpha** → **UIAlpha** → 句 | 仅句 0→1→2→3→4 | **P0** |
| 图存放 | Prefab **bound** 内嵌 | Prefab bound **空** + 外部 Generated | 施工应写入 bound（同 Head） |
| 结束 Reset | Special onStoryEnd | 同门 | ✅ |
| Setup 建图 | `fadeYaerPortrait: **true**` | SetupAll 仍 `fadeYaerPortrait: **false**` | **回潮风险** |

**Head 现网序（已施工 T1）**

```
0 FightingPanelVisible
  → 13 CanvasGroupAlpha(GoOutStoryYaerPainting) 0→1 Dur=1
    → 1 NormalDialogueUIAlpha EndA=1 Dur=1 Delay=0.5 PrepareMask=true
      → Statement…
```

**Chest 现网序**

```
Statement×5（C1～C5）  // 无任何壳层 Action
```

### C. 雅儿大立绘拍板

| 项 | 决议 |
|----|------|
| 时序 | **T1 对齐 Head**：Fighting 后、UIAlpha **前**，串行 `CanvasGroupAlpha(GoOutStoryYaerPainting)` |
| 物体 / BB | **已齐**，勿再嵌老板娘 Painting；勿把 Mask 当大立绘 |
| 初始 alpha | 保持 0，靠节点拉到 1 |
| 参数 | **1:1 抄 Head**（立绘 Dur=1；UIAlpha Dur=1 Delay=0.5 PrepareMask=true；MaskAvatarRole/Face 同 Head 除非产品另定） |

### D. 悬停光标拍板

| 项 | 决议 |
|----|------|
| 方案 | Chest **同挂** `CursorChangeTrigger`，`TargetState = Catch (1)`，`Priority = 1`（对齐 Head 0829） |
| 总开关 | `SetShopkeeperHotspotsEnabled` **已** `GetComponentsInChildren<CursorChangeTrigger>` → 挂上即进开关；对白关热区 → OnDisable → Exit，避免卡手 |
| 禁止 | `Cursor.SetCursor`；新造第五种光标图 |
| Setup | `EnsureHotspot` **现不挂** Cursor → 施工改场景 YAML 和/或 Setup 给 Head/Chest 都 Ensure Cursor（防只手改场景） |
| 相对前序 | 0830 Chest 安装报告「Catch 本期否」→ **本单改口为 P0 要做** |

### E. 壳层写在哪（防 CSV 回潮）

| 方案 | 裁定 |
|------|------|
| **R1 · Rebuild Chest：`fadeYaerPortrait=true` + SetBoundGraphReference** | ✅ **推荐**：与 Head `RebuildHeadPrefabOnly` 同管线；Prelude = 立绘 CGAlpha → UIAlpha；Prefab 嵌入 bound |
| R2 · 只手改 Generated.asset 插节点 | ⚠️ 可救急；CSV/旧 Setup 重导易冲掉 |
| R3 · Prefab 本地壳 + 外部只挂句 | ❌ 双图难维 |

**Setup 必改**

| 项 | 现网 | 应改 |
|----|------|------|
| `SetupAll` → Chest `fadeYaerPortrait` | `false` | **`true`**（否则 Rebuild 仍无大立绘） |
| 菜单 | 仅 Head Rebuild | **增** `Rebuild Shopkeeper Chest Prefab Only (Village_ShopChest)` |
| Fighting | Prelude 默认 `HideFightingPanelOnStart=false` 不插 Fighting | Head 金样有 Fighting：Rebuild 后 **手补** 或扩展 Prelude；最小验收以 **UIAlpha+CGAlpha** 为准，Fighting 对齐金样作 P0 补齐 |

说明：现盘点胸 Generated **连 UIAlpha 都没有**，说明不是「只关了立绘淡入」，而是 **从未用带 `FadeDialogueUI` 的 Prelude 写进当前运行图**（或 bound 被清空只留了纯 Statement asset）。应用 R1 整图重建，勿只插一个节点却忘写回 Prefab bound。

### F. 回归边界

| 项 | 要求 |
|----|------|
| Head | 不改节点序 / 不关 Catch |
| Start / Repeat / Yes / No | 不动 |
| C6 树屋 | ❌ 仍不做 |
| 故事名 | 保持 `Village_ShopChest` |

### G. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 修对话框壳：保证运行图含 `NormalDialogueUIAlpha`（推荐 R1 Rebuild） | **P0** |
| 2 | 补雅 `CanvasGroupAlpha`；序 = Fighting(?) → 立绘 → UIAlpha → 句（对齐 Head T1） | **P0** |
| 3 | `SetupAll` Chest `fadeYaerPortrait=true`；加 Rebuild Chest Only | **P0** |
| 4 | 场景 `Trigger/Chest` 挂 `CursorChangeTrigger` Catch；可选 Setup Ensure | **P0** |
| 5 | 验收三件事 + Head 回归 | **P0** |
| 6 | 文档：点胸须含壳层；纯 CSV 重导会丢框 | P1 |
| 7 | `MerchantPainting` Prefab 源同步 Cursor | P1 |
| 8 | C6 / 改 CSV 文案 / 改故事名 | ❌ |

**预期 diff**

- `Village_ShopChest.prefab`（bound 图含壳层；或经 Rebuild）  
- `Village_商店点胸交互.asset`（与 bound 同步）  
- `Village_Shop.unity`（Chest + CursorChangeTrigger）  
- `ShopkeeperSpecialClickSetupEditor.cs`（fade 旗标 + Rebuild Chest 菜单；可选 Ensure Cursor）  
- **一般不改** GSM 常量 / Hotspot 点击逻辑  

### H. 验收清单

同 §③。

### I. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 壳层写 Prefab bound 还是只改 Generated？ | **R1：Rebuild 写入 Prefab bound**（Generated 旁路校对） | ✅ |
| Q2 | UIAlpha 是否 1:1 抄 Head（Dur/Delay/PrepareMask）？ | **是** | ✅ |
| Q3 | Fighting 节点是否必须？ | **建议有**（对齐 Head 金样）；缺则至少 UIAlpha 可先出框 | ✅ 倾向有 |
| Q4 | 前序「Chest Catch 本期否」？ | **本单改口：要做** | ✅ |
| Q5 | EnsureHotspot 是否默认挂 Catch？ | **是**（Head+Chest），避免场景-only | ✅ 倾向 |

（已追加 `OPEN_QUESTIONS.md`。）
