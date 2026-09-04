# Village_ShopStart — 新建对话 NPC `Merchant`（老板娘 Actor 接线）— 架构溯源报告

**文档版本**：v1.0（2026-08-27）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改 Prefab/场景/代码**）  
**Unity**：2020.3.48f1  
**目标 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab`  
**关联 CSV**：`Assets/Dialog/Village_商店首次对话.csv`  
**关联生成图**：`Assets/GameRes/DialogueTrees/Generated/Village_商店首次对话.asset`  

关联提示词：`Assets/Doc/提示词/0827/Village_ShopStart_新建Merchant对话NPC_架构侦探提示词.md`  
Body/Face 链：`0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md`（v1.1）  
首次进店策划：`0629/商店系统_策划拆解_执行说明.md` §4  

---

## ① 结论一句话

**推荐方案 A：在 `Village_ShopStart` 下新建轻量 `Merchant` GO（`DialogueActorEx`，无 Painting 子节点），Inspector 把 Actor 参数「老板娘」拖到此 GO；Hierarchy 名 `Merchant` 与 Actor 名「老板娘」可以分离。店句视觉靠 `Village_Shop` 场景「商店界面合层」Toggle，位置在场景里调，不在对话 Prefab 里。**

---

## ② 原因（通俗）

### 2.1 店句 ≠ 雅/古 Painting 链

```
雅尔 / 古莎：
  Prefab 内 GoOutStoryYaerPainting / GushaPainting
  → RefreshAvatar + Mask 小头像

店（老板娘）：
  CSV Speaker「店」→ Actor「老板娘」
  → SayEx UseShopkeeperPortrait = true
  → SubtitlesRequestInfoEx → ShopkeeperFaceRegistry
  → Village_Shop「商店界面合层」Body/Face Toggle
  → 不走 StoryFormPainting
```

复制 Yaer 结构做 `MerchantPainting` 会与场景合层 **双份立绘**，且 **Body/Face CSV 分流失效**。

### 2.2 现网缺什么

磁盘已 **Import 对白图**（含大量店句 `UseShopkeeperPortrait=true`），但 Actor 参数 **「老板娘」未绑 GO**：

| Actor 参数 | 绑定 GO | 节点 `_actorName` |
|------------|---------|-------------------|
| 雅尔 | ✅ `Yaer`（`DialogueActorEx`） | 雅尔 |
| 古莎 | ✅ `Gusha`（`DialogueActorEx`） | 古莎 |
| **老板娘** | **❌ None** | 图中已写「老板娘」 |

未绑时 Play 风险：

- NodeCanvas 图内店句 SayEx **Actor 槽红/警告**
- 字幕条 **`actorName` 为空**（`DialogueTMPUGUI` 在店句分支仍读 `actor.name` 填名字）
- Body/Face **仍可切**（走 `UseShopkeeperPortrait` 分支，不依赖 `DialogueActorEx.RefreshAvatar`）——但 **缺字幕显示名**

### 2.3 命名三角（拍板）

| 层 | 值 | 是否必须一致 |
|----|-----|--------------|
| Hierarchy GO | **`Merchant`**（用户指定） | 与 Actor 名 **不必**同名 |
| NodeCanvas Actor 参数 | **`老板娘`** | 与 `ShopkeeperCsvDefaults.ShopkeeperActorName` **必须**一致 |
| CSV Speaker | **`店`** | 映射 `店→老板娘`（默认 SO **已有**） |
| 字幕显示 `_name` | **「老板娘」**（中文） | 策划可见名 |

**不要**把 `ShopkeeperActorName` 改成 `Merchant`，除非连带改 GraphBuilder / 已 Import 图 / 全引用。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 打开 `Village_ShopStart` Prefab | 见 `BG` / `Yaer` / `Gusha`；**无 Merchant** |
| 2 | Inspector → Dialogue Tree → Actor Parameters | **老板娘 = None** → 施工后拖 **`Merchant`** |
| 3 | 新建 `Merchant`：挂 `DialogueActorEx`，`_name` 填 **「老板娘」**；**不要**嵌 Painting | |
| 4 | `RectTransform` 可参考 Yaer/Gusha：**100×100 占位**即可（无立绘） | |
| 5 | **老板娘摆位**：改 **`Village_Shop.unity` → `商店界面合层` Transform**（世界坐标） | |
| 6 | **雅/古摆位**：改 Prefab 内 `GoOutStoryYaerPainting` / `GushaPainting` RectTransform | |
| 7 | **验收必须在 `Village_Shop` Play**（`ShopkeeperFaceRegistry` 仅场景注册）；DialogDebug 只能测雅/古字幕 | |
| 8 | 店句 Body/Face 随 CSV 切换；Console 无「未注册」Warning | |

---

## ④ 给程序

### A. Prefab 现网核实（磁盘 · 2026-08-27）

**Hierarchy**

```
Village_ShopStart（DialogueTreeController + Blackboard）
├── BG                    ← Image · 默认 Active=0
├── Yaer                  ← DialogueActorEx · _name=雅尔 · _roleName=Yaer(1)
│     └── GoOutStoryYaerPainting（嵌套 Prefab · CanvasGroup 淡入）
└── Gusha                 ← DialogueActorEx · _name=古莎 · _roleName=Gusha(6)
      └── GushaPainting（嵌套 Prefab · CanvasGroup 淡入）
```

**Blackboard 变量**：`GoOutStoryYaerPainting`、`GushaPainting`（前奏淡入用）—— **无老板娘变量**（正确，合层不需 CanvasGroup 淡入）。

**对白图**（Prefab 内嵌 JSON）：约 50 个 SayEx 节点；店句已设 `UseShopkeeperPortrait=true`，部分含 `ShopBody`/`ShopFace`（如末段 `ShopBody=Blush`）。雅/古句仍用 `DialogueFaceType`。

**CSV 源**：`Assets/Dialog/Village_商店首次对话.csv`（含可选 `BodyType` 列；多数店行 Body 空=继承）。

### B. 方案对比与推荐

| 方案 | 说明 | 裁定 |
|------|------|------|
| **A · 轻量 Actor 壳** | `Merchant` + `DialogueActorEx`，无子 Painting；绑 Actor「老板娘」 | **✅ 推荐** |
| B · 嵌 `商店界面合层` | 对话 Prefab 内再摆一份合层 | ❌ 与 Village_Shop 双份；Registry 绑哪份？0704/0713 双轨冻结 |
| C · 复制 Yaer + MerchantPainting | UGUI 大立绘 | ❌ 与 Toggle 链重复 |
| D · 仅 dummy 不建 GO | | ❌ NodeCanvas 要 Transform；字幕名空 |

### C. `Merchant` 最小结构（方案 A 施工规格）

| 项 | 规格 |
|----|------|
| GO 名 | **`Merchant`** |
| 父节点 | `Village_ShopStart` 根下（与 Yaer/Gusha 同级） |
| 组件 | **`DialogueActorEx`**（guid `4a0335bc9bbd00c4a9557f025c60444f`，与 Yaer/Gusha 同） |
| `_name` | **`老板娘`**（字幕条显示） |
| `_roleName` | **`None`（0）即可** — 店句不走 `RefreshAvatar`/`DialogueMaskAvatarPresenter`；本期 **不必**扩 `DialogueRoleName.Shopkeeper` |
| `_portrait` | 空（店句分支会关 `actorPortrait`） |
| 子节点 | **无** |
| RectTransform | 锚点居中；Size **100×100**（对齐 Yaer/Gusha 父节点占位）；位置随意（无可见 Sprite） |
| Blackboard | **不需要** Merchant 变量 |
| 前奏 Action | **不改** — 仍只淡入雅/古 Painting；老板娘合层在场景里 **常驻可见**（默认倾向 **否** 做淡入） |

### D. Actor / CSV / 运行时契约

| 检查项 | 现网 | 施工后 |
|--------|------|--------|
| `DialogueSpeakerMapping` `店→老板娘` | ✅ 默认 SO 已加 | 保持 |
| `ShopkeeperCsvDefaults.ShopkeeperActorName` | `"老板娘"` | **不改** |
| GraphBuilder 店句 | `UseShopkeeperPortrait=true` + ShopBody/ShopFace | 已 Import 进 Prefab |
| `DialogueTMPUGUI` 店句 | `ShopkeeperFaceRegistry.Instance.Apply` | 需 **Village_Shop** 场景 Play |
| 未绑 Actor 时字幕名 | **空** | 绑 Merchant 后显示「老板娘」 |
| 未绑 Actor 时 Body/Face | **仍能切**（若 Registry 有） | 绑 Actor 后体验完整 |

**运行时双轨（首次进店构图 · 0629）**

| 句 Speaker | 视觉轨 | 位置编辑 |
|------------|--------|----------|
| 雅 / 古 | Prefab Painting + Mask | `Village_ShopStart` RectTransform |
| **店** | 场景合层 Body/Face | **`Village_Shop` → `商店界面合层`** |

同屏时：店句 **不** 调 Mask 立绘（`OnGetNewStatement` 发 `RoleName.None`）；雅/古句 **不** 动合层 Toggle — **互不干扰**。

### E. 立绘位置编辑指南（给用户）

| 要调什么 | 改哪里 |
|----------|--------|
| **老板娘身体/脸在画面中的位置** | `Village_Shop.unity` → **`商店界面合层`** 根 Transform（及 Body/Face 子节点 local 偏移） |
| **雅/古大立绘在对话里的位置** | `Village_ShopStart.prefab` → `Yaer`/`Gusha` 下 Painting **RectTransform** |
| **对话框小头像** | `NormalDialogueNewPanel.prefab` → Mask 内各 Painting |
| **`Merchant` 节点本身** | 仅占位，**无可视内容**，一般不用调 |

**预览路径**

| 环境 | 能测什么 |
|------|----------|
| **DialogDebug** | 雅/古字幕 + 大立绘淡入；**店句 Body/Face 不可靠**（无 Registry） |
| **`Village_Shop` Play + 挂/触发 `Village_ShopStart`** | 店句字幕名 + Body/Face + 雅/古 **全链路** |

### F. 最小施工清单（给施工员 · 侦探不执行）

| # | 动作 | 必须？ |
|---|------|--------|
| 1 | `Village_ShopStart` 下新建 **`Merchant`** GO | ✅ |
| 2 | 挂 **`DialogueActorEx`**；`_name` = **「老板娘」**；`_roleName` = **None** | ✅ |
| 3 | Inspector **Actor 参数「老板娘」→ 拖 `Merchant` Transform** | ✅ |
| 4 | **不要**嵌 Painting / 合层 Prefab | ✅ |
| 5 | 保存 Prefab；打开 NodeCanvas 确认店句 SayEx **Actor 不红** | ✅ |
| 6 | **`Village_Shop` Play**：触发该对白；验店/雅/古各至少一句 | ✅ |
| 7 | 确认 `商店界面合层` 上 **`ShopkeeperFaceController`** 已注册 | ✅ |
| 8 | （可选）Re-Import CSV 回归 — 图已在 Prefab，**非必须**除非改表 | P2 |

**排除**：改 `ShopkeeperActorName` 为 `Merchant`；嵌合层；本期首次进店存档旗标 / 藏 UI / 黑屏（见开放问题）。

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Prefab：Actor **老板娘** ≠ None | |
| 2 | NodeCanvas：店句 SayEx 无 Actor 缺失红标 | |
| 3 | **Village_Shop Play**：店句字幕显示 **「老板娘」** | |
| 4 | 店句 Body/Face 随节点 ShopBody/ShopFace 切换（如 Face2、Blush） | |
| 5 | 雅/古句：Mask/大立绘正常；Merchant 不影响 | |
| 6 | Console：无 `ShopkeeperFaceController 未注册`（或仅 DialogDebug 可接受 Warning） | |

### H. 开放问题

| ID | 问题 | 建议 |
|----|------|------|
| Q1 | GO 必须叫 `Merchant` 还是也可 `老板娘`？ | **Merchant 可读性 OK**；Actor 参数仍 **`老板娘`** |
| Q2 | 本期是否接 **首次进店只播一次** 触发？ | **超出本单**；Prefab 已就绪，触发/存档另开 |
| Q3 | 店句要不要 CanvasGroup 淡入？ | **默认否** — 合层进店即见 |
| Q4 | 是否追加 `DialogueRoleName.Shopkeeper`？ | **本期不必**；`_roleName=None` + Actor 名即可 |
| Q5 | `Village_ShopStart` 由谁实例化？ | 磁盘 **未搜到** 运行时引用；首次进店 Trigger 待接（0629） |

---

## 附录 · 与 0827 Body/Face 报告衔接

| 0827 已施工（磁盘） | 本单关系 |
|---------------------|----------|
| `ShopkeeperFaceController` Toggle | 店句 **`Apply(ShopBody, ShopFace)`** 依赖场景 Registry |
| `StatementNodeEx.UseShopkeeperPortrait` | Prefab 内店句 **已写入** |
| CSV `BodyType` 可选列 | `Village_商店首次对话.csv` 已含列 |
| `DialogueSpeakerMapping` `店→老板娘` | Import / 图内 Actor 名 **已对齐** |
| **缺 Merchant GO** | **本单施工补齐** |

---

*报告结束。施工员按 §F 建 `Merchant` 并绑 Actor「老板娘」；全链路在 `Village_Shop` Play 冒烟。*
