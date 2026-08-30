# Village_Shop — MerchantPainting Trigger（Head/Chest）特殊交互对话 + 表情 — 架构溯源报告

**文档版本**：v1.0（2026-08-28）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_Shop.unity` · `商店界面合层` → ` MerchantPainting`  
**台本真源**：`Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md`  
**表情真源**：`0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md` + `Village_商店首次对话.csv`  

关联提示词：`Assets/Doc/提示词/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构侦探提示词.md`  
关联报告：`0827/Village_ShopStart_新建Merchant` · `MerchantPainting_UI版` · `首次进店Village_ShopStart联调` · `0713/Village_Shop纯UI`

---

## ① 结论一句话

**推荐点击方案 B（Head/Chest 挂 `BoxCollider2D` + 主相机补 `Physics2DRaycaster` + 热区脚本 `IPointerClickHandler`）；两分支独立 Prefab/CSV（`Village_ShopKeeper_HeadClick` / `…_ChestClick`），店句表情复用现网 `Face1～5`+`BodyType`+`ShopkeeperFaceRegistry`/`MerchantMask` 双轨，禁止 Smile/Angry；胸部线本期只做店内 C1～C5，C6 树屋转场分期；磁盘上 Trigger/Head/Chest 尚未落盘，施工第一步须先保存热区节点。**

---

## ② 原因（通俗）

### 2.1 热区现状：Hierarchy 截图 ≠ 磁盘

| 项 | 提示词假说 / 截图 | **磁盘核实（2026-08-28）** |
|----|-------------------|---------------------------|
| `Trigger` / `Head` / `Chest` | 用户已挂 | **❌ 场景 / `MerchantPainting.prefab` / `商店界面合层.prefab` 均无此三节点** |
| ` MerchantPainting` 子树 | Body / Face / Trigger | **仅 `Body` + `Face`**（Transform 父节点） |
| 父链 | — | 世界空间 **SR**；合层根仅 `Transform`，**无 Canvas** |
| Layer | — | **0（Default）** |
| 可点组件 | 未必有 | **无** Collider / Image / Button |
| `EventSystem` | — | ✅ 有（StandaloneInputModule） |
| `UI_Shop` | — | Overlay Canvas + **GraphicRaycaster**；买卖 `Bar` 可点 |
| 主相机 | — | **无** `PhysicsRaycaster` / `Physics2DRaycaster` |
| 表情控制器 | — | 合层挂 `ShopkeeperFaceController` + DebugInput ✅ |

生活类比：海报（合层 SR）已经贴好，买卖柜台（`UI_Shop`）也能点；头/胸的「感应贴纸」在编辑器里可能画过，但**文件还没存上**——所以点了也不会有反应。

### 2.2 为何不能直接用提示词方案 A（合层挂透明 Image）

合层是 **SpriteRenderer 世界空间**，不在任何 `GraphicRaycaster` 树下。Head/Chest 上挂裸 `Image` **收不到** UGUI 点击。

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A · 合层下裸 Image** | Head/Chest 挂 Image | ❌ 无 Canvas，不可行 |
| **A′ · World Space Canvas 热区** | Trigger 下挂 WS Canvas + 透明 Image | 可行；多一层 Canvas，与 SR 缩放要对齐 |
| **B · Collider2D + Physics2DRaycaster（推荐）** | Head/Chest 挂 BoxCollider2D；相机补 Raycaster；脚本 `IPointerClickHandler` | **✅ 本期** — 与 SR 合层同坐标系，热区跟着立绘走 |
| **C · Overlay 分区 Button** | 挂在 `UI_Shop` 上盖头/胸 | 备选；相机锁死后可，但与合层缩放易漂 |
| **D · Keyboard Debug** | — | ❌ 仅验收辅助 |

**B 与 Overlay 抢点**：`UI_Shop` 的 `Bar` 偏柜台区；老板娘立绘在合层右侧。若某像素无 `RaycastTarget=1` 的 UI，事件可落到 Physics2D。施工验收时用 Scene 视图确认 Head/Chest 屏上区域**不被** `Bar_BG`/列表挡住；若挡住 → 改热区 Rect，或临时对白期仍走「整棵藏 `UI_Shop`」（见 §E）。

### 2.3 为何店句不能走旧 Smile/Angry

0601 台本填的是策划语义脸（`Smile`/`Angry`…）。0827 已拍板现网店句：

| 层 | 约定 |
|----|------|
| CSV Speaker `店` | Actor `老板娘` · `UseShopkeeperPortrait=true` |
| Face | **`Face1`～`Face5`** |
| Body | **`Normal` / `Red` / `YinXian`**（可选列） |
| 雅句 | 仍 `DialogueFaceType` |

把 Smile 塞进 `DialogueFaceType` 或扩枚举 = **污染雅/古链**；Import 也会在店行校验失败。必须按下方 **§D 迁移表** 填 CSV。

### 2.4 与首次进店隔离

| 需求 | Prefab | 本期 |
|------|--------|------|
| 首次进店 | `Village_ShopStart` · GSM 自动 Trigger | ❌ **勿改、勿并图** |
| 点头 | `Village_ShopKeeper_HeadClick` | ✅ |
| 点胸（店内） | `Village_ShopKeeper_ChestClick` | ✅ 仅 C1～C5 |
| 点胸 C6+ 树屋 | 另段 / 下期 | ❌ 分期 |

表情与 Actor 壳 **复制 ShopStart 最小壳**（`Yaer` + `Merchant`，胸部店内段 **无需古莎**），不要嵌 Painting 合层。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 打开 `Village_Shop` → `商店界面合层` → ` MerchantPainting` | 现网仅 Body/Face；**若编辑器里有 Trigger 未存，先 Ctrl+S 落盘** |
| 2 | 施工后：`Trigger/Head`、`Trigger/Chest` 有 **BoxCollider2D**（isTrigger）+ 点击脚本 | Gizmo 框对准头/胸 |
| 3 | Main Camera 有 **Physics2DRaycaster** | |
| 4 | Idle：点头 → 播点头对白；店脸 Face 随句变；雅脸随句变 | |
| 5 | Idle：点胸 → 播 C1～C5；**不**黑屏切树屋（本期） | |
| 6 | 对白中再点头/胸 | **不开**第二段 |
| 7 | 对白中 `UI_Shop` | **隐藏**（复用 ShopStart 的 Hide/Show） |
| 8 | 首次进店自动对白进行中点头胸 | 无干扰（热区 OFF 或 HasRunningStory 挡） |
| 9 | Console | 无 NRE / Missing Actor / Face 校验失败 / Registry 未注册 |
| 10 | Prefab 源：Trigger 是否写回 `MerchantPainting.prefab` / 合层 Prefab | 见开放问题 Q4；至少 **场景实例** 可验收 |

---

## ④ 给程序

### A. Trigger 现状表（任务清单 A · 钉死）

| 项 | 值 |
|----|-----|
| `Trigger` / `Head` / `Chest` 组件 | **磁盘无节点** → 施工新建后挂 **BoxCollider2D** + 点击脚本；**不要**裸 Image |
| Layer | 建议保持 **0**；或专用 Layer（须进相机 Culling + Raycaster mask） |
| Rect | Collider size/offset 对齐头、胸 Sprite 区域（世界单位） |
| 父链 | `商店界面合层` → ` MerchantPainting` → `Trigger` → Head/Chest；**世界空间 SR** |
| 点由谁管 | `EventSystem` +（UI）`UI_Shop.GraphicRaycaster` +（热区）**待加** `Physics2DRaycaster` |
| `UI_Shop` 盖热区？ | Bar 偏左/中；立绘偏右 — **默认不盖**；若盖则调 Collider 或藏 UI |
| 对白中谁挡谁 | **推荐整棵 `HideShopUiRoot()`**（与 ShopStart 同 API）→ 热区仅受脚本 `enabled` / Collider 开关控制 |

**现网合层树（磁盘）**

```
商店界面合层          ← ShopkeeperFaceController + DebugInput
├── 背景
└──  MerchantPainting ← 仅 Transform；子：Body、Face
      ├── Body / Normal|Red|YinXian   （SR）
      └── Face / Face1～5             （SR）
      └──（待建）Trigger
            ├── Head
            └── Chest
```

---

### B. 点击 → TriggerStory 架构选型（拍板）

#### B.1 推荐链路

```
Idle
  → 点 Head / Chest（IPointerClickHandler）
  → Village_ShopSceneManager.TryTriggerShopkeeperSpecial(storyName)
       if HasRunningStory → return false
       if 热区已禁用 → return
       HideShopUiRoot()
       onStoryEnd += ShowShopUiRoot（一次性）
       TriggerStory("Village_ShopKeeper_HeadClick" | "…_ChestClick")
  → 店句：Registry.Apply + MerchantMask.ApplyShopkeeperPortrait
  → 雅句：DialogueFaceType 原链
  → OnStoryEnd → ShowShopUiRoot → 热区再开
```

#### B.2 决策表

| 问题 | 拍板 |
|------|------|
| 点击脚本挂哪？ | **Head / Chest 各一**（或同一组件用 enum 区分）；`Trigger` 父级可选作总开关 `SetHotspotsEnabled` |
| Story / Prefab 名 | **沿用 0601**：`Village_ShopKeeper_HeadClick` / `Village_ShopKeeper_ChestClick` |
| 谁调 `TriggerStory`？ | **`Village_ShopSceneManager` 封装**（复用 Hide/Show、`HasRunningStory`）；热区 **不**直接调 StoryGSM |
| 互斥 API | 现成：`StoryComponentGSM.HasRunningStory` + `TriggerStory` 返回 false；再加热区 `enabled=false` 双保险 |
| 首次进店中热区 | **强制关**：`ShouldPlayShopStartStory` 为真或 `HasRunningStory` 时 `SetHotspotsEnabled(false)`；ShopStart 已藏 UI，再关 Collider 防漏点 |

#### B.3 脚本建议（最小）

| 类 | 职责 |
|----|------|
| `ShopkeeperBodyHotspot`（暂名） | 挂 Head/Chest；`OnPointerClick` → 调 GSM |
| `Village_ShopSceneManager` | `TryTriggerShopkeeperSpecial(string)`；`SetShopkeeperHotspotsEnabled(bool)`；订阅 `onStoryEnd` |

**禁止**：`Update` 里 `Input.mousePosition` 轮询切脸 / 判点击。

---

### C. 对白资产方案（两分支独立）

| 分支 | Prefab | CSV 建议名 | Actor | 表情列 | 范围 |
|------|--------|------------|-------|--------|------|
| 点头 §1 | `Village_ShopKeeper_HeadClick` | `Village_商店点头交互.csv` | 雅尔 + **Merchant（老板娘壳）** | 店=`Face1～5`+Body；雅=`DialogueFaceType` | H1～H11；**H3=ActionNode** |
| 点胸店内 | `Village_ShopKeeper_ChestClick` | `Village_商店点胸交互.csv` | 同上（无古莎） | 同上 | **仅 C1～C5** |
| 点胸树屋 | `…_ChestClick_Treehouse`（下期） | 另表 | 可能仅雅 | 雅 Face | C6～C8 |

**存放**：`Assets/GameRes/Prefabs/Dialogue/` 根目录（与 ShopStart 同级）。

**建图步骤（推荐）**

1. **复制** `Village_ShopStart.prefab` → 改名为 Head/Chest 两份。  
2. **删掉** Gusha（点头/点胸台本无古莎）；保留 Yaer + Merchant；Actor 参数只留需要的。  
3. **清空** 旧图 / 换绑新 Import 的 Generated Graph（或空图再 Import CSV）。  
4. **勿**把节点粘进 `Village_ShopStart` 同一棵图。  
5. H3 / C6：NodeCanvas **ActionNode**，禁止当 Say。本期 Chest 图在 C5 后 **自然结束**（不接 C6）。

**CSV 表头样板**（对齐首次进店）：

```csv
ID,Type,Speaker,Text,English,Next,Extra,FaceType,Voice,BodyType
```

---

### D. 表情迁移表（0601 → 现网 · 建议；待策划签字）

依据：`Village_商店首次对话.csv` 语义——Face1 平静、Face2 调侃/坏笑、Face3 贪财陶醉、Face4 责备/惊讶偏怒、Face5 更夸张（首次表几乎未用，留给高潮）。

#### D.1 点头 §1

| 台本 | Speaker | 旧 FaceType | 建议 ShopFace | 建议 BodyType | 雅 Face |
|------|---------|-------------|---------------|---------------|---------|
| H1 | 店 | Smile | **Face2** | Normal | — |
| H2 | 雅 | Daze | — | — | **Daze** |
| H3 | — | （动作） | — | — | ActionNode：旁白/演出，非 Say |
| H4 | 店 | Surprised | **Face4** | Normal | — |
| H5 | 雅 | VerySurprised | — | — | **VerySurprised** |
| H6 | 店 | Angry | **Face4** | Normal | — |
| H7 | 雅 | Unhappy | — | — | **Unhappy** |
| H8 | 雅 | Unhappy | — | — | **Unhappy** |
| H9 | 店 | Angry | **Face4** | Normal | — |
| H10 | 店 | Angry | **Face5** | **Red** | — |
| H11 | 雅 | Surprised | — | — | **Surprised** |

#### D.2 点胸 §1.2（本期仅店内）

| 台本 | Speaker | 旧 FaceType | 建议 ShopFace | 建议 BodyType | 雅 Face |
|------|---------|-------------|---------------|---------------|---------|
| C1 | 店 | Surprised | **Face4** | Normal | — |
| C2 | 店 | Laugh | **Face2** | Normal | — |
| C3 | 店 | Smile | **Face2** | **Red** | — |
| C4 | 雅 | Laugh | — | — | **Laugh** |
| C5 | 店 | Laugh | **Face2** | **Red** | — |
| C6 | — | 黑屏转树屋 | — | — | **下期** ActionNode |
| C7 | 雅 | Daze | — | — | 下期 |
| C8 | 雅 | VerySurprised | — | — | 下期 |

#### D.3 Mask 小表情

| 项 | 状态 |
|----|------|
| `MerchantMaskPainting` + `ApplyShopkeeperPortrait` | ✅ **已接**（0827 施工后） |
| 本期要求 | 店句 **必须** 合层 + Mask **双轨同步**（与 ShopStart 店句相同 `DialogueTMPUGUI` 分支） |
| 缺口 | 无新缺口；DialogDebug 无 Registry 时合层 Warning 可接受，Mask 仍应亮 |

---

### E. 与买卖 UI / 首次进店共存（状态机）

```
                    ┌─────────────────────────────────────┐
                    │ Idle                                │
                    │ · UI_Shop ON                        │
                    │ · 热区 ON                           │
                    │ · 可买卖 / 可点头胸                  │
                    └───────────┬─────────────────────────┘
           点 Head/Chest        │          首次进店 Trigger
                    ▼           │                    ▼
        ┌──────────────────┐    │    ┌──────────────────────────┐
        │ SpecialTalking   │    │    │ ShopStartTalking         │
        │ · 热区 OFF       │    │    │ · 热区 OFF               │
        │ · Hide UI_Shop   │    │    │ · Hide UI_Shop（现网）   │
        │ · 禁叠 Trigger   │    │    │ · HasRunningStory        │
        └────────┬─────────┘    │    └────────────┬─────────────┘
                 │ onStoryEnd   │                 │ onStoryEnd
                 ▼              │                 ▼（现网：结束黑幕后）
              Idle ◄────────────┴────────────── Idle
```

| 项 | 裁定 |
|----|------|
| 特殊对白期藏 UI？ | **是** — 调已有 `HideShopUiRoot` / `ShowShopUiRoot`，**不**重做首次进店旗标 |
| 特殊对白结束黑幕？ | **本期否**（直接 Show UI）；与 ShopStart 结束黑幕解耦 |
| 存档只播一次？ | **本期否** — 点头/点胸默认可重复（见开放问题 Q2） |
| 头胸互斥 | `HasRunningStory` 已足够；同一帧两 Collider 分区明确即可 |

---

### F. 胸部线分期裁定（必写）

| 范围 | 本期施工？ | 说明 |
|------|------------|------|
| **C1～C5** 店内对白 + 表情 | **✅ 是** | Prefab `Village_ShopKeeper_ChestClick` 播完回 Idle |
| **C6** 黑屏转树屋 | **❌ 下期** | 可复用 ShopStart `ShowShopBlackFade` / `LoadScene`；图内留 **TODO 注释挂钩**，勿空接 |
| **C7～C8** 树屋对白 | **❌ 下期** | 工程 **无**「雅尔树屋」专用 `SceneName`；候选民居场景未指定 |

**店内段结束行为（本期）**：C5 说完 → `OnStoryEnd` → `ShowShopUiRoot` → Idle。  
**不做**：占位全黑卡死、假切场。

---

### G. 最小施工清单（给施工员 · 本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | 场景热区 | 保存/新建 `Trigger/Head|Chest`；BoxCollider2D；点击脚本；相机 Physics2DRaycaster | **P0** |
| 2 | GSM | `TryTriggerShopkeeperSpecial` + 热区总开关；复用 Hide/Show UI；`HasRunningStory` 守卫 | **P0** |
| 3 | 对白资产 | 两份 Prefab + CSV Import；Merchant Actor 壳；H3=Action | **P0** |
| 4 | CSV 表情 | 按 §D 迁移表填 Face1～5 + BodyType；店行 UseShopkeeperPortrait | **P0** |
| 5 | Prefab 源同步 | Trigger 是否写回 `MerchantPainting.prefab`（见 Q4） | P1 |
| 6 | 胸部 C6+ | 仅注释挂钩 / 开放问题 | **P2 / 下期** |
| 7 | 技术说明短文 | `Doc/技术文档`（可选） | P2 |

**排除**：并进 `Village_ShopStart`；扩 `DialogueFaceType`；`Update` 轮询；重做首次进店存档 / DeferCover。

---

### H. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进店（非首次或首次结束后）点 Head | 播点头对白；店合层+Mask 随句变；雅脸变 |
| 2 | 点 Chest | 播 C1～C5；表情正确；**不**切树屋 |
| 3 | 对白中再点头/胸 | 不开第二段、无叠对话 |
| 4 | 买卖按钮 | 对白中 UI 隐藏；结束后可买卖 |
| 5 | 首次进店对白中点头胸 | 无干扰 |
| 6 | Console | 无 NRE / Missing Actor / Face 校验失败 |

---

### I. 开放问题（同步写入 `OPEN_QUESTIONS.md`）

| ID | 问题 | 侦探倾向 / 施工默认 |
|----|------|---------------------|
| Q1 | Smile/Angry→Face1～5 正式对照是否策划签字？ | **先按 §D 表施工**；策划可改 CSV 单元格，勿改枚举 |
| Q2 | 点头/点胸可否重复触发？ | **可重复**；不做 `StoryTriggerCountData`（与 ShopStart 区分） |
| Q3 | 胸部树屋场景名 / 落点？ | **未定**；工程无「树屋」SceneName；下期再开 |
| Q4 | Trigger 写场景实例还是同步 Prefab 源？ | **场景必做**；建议同步 `MerchantPainting.prefab`，合层美术 Prefab 可选 |
| Q5 | 若 UI_Shop 挡住热区？ | 调 Collider；或 Idle 也接受「热区在 Bar 外」；禁止全屏挡板 Raycast |

---

### J. 最小文件清单（预期 diff）

| 文件 | 动作 |
|------|------|
| `Village_Shop.unity` | Trigger 树 + Collider；相机 Physics2DRaycaster |
| `Village_ShopSceneManager.cs` | 特殊对白 Trigger + 热区开关 |
| 新建 `ShopkeeperBodyHotspot.cs`（名可调） | 点击 → GSM |
| `Village_ShopKeeper_HeadClick.prefab` + CSV | 新建 |
| `Village_ShopKeeper_ChestClick.prefab` + CSV | 新建（止于 C5） |
| （可选）`MerchantPainting.prefab` | 同步 Trigger |
| **不改** | `Village_ShopStart.prefab` 图结构；`DialogueFaceType` 枚举 |

---

### K. 与既有报告衔接

| 报告 | 本单依赖 |
|------|----------|
| BodyFace CSV / ShopkeeperFaceRegistry | 店句表情真源 |
| Merchant Actor 壳 | 复制到 Head/Chest Prefab |
| MerchantMaskPainting | 店句 Mask 已接，勿重做 |
| ShopStart 联调 / GSM Hide UI | **复用** Hide/Show + HasRunningStory |
| 0601 台本 | 文案真源；Face 列须迁移 |

---

**报告结束 · 待用户拍板后交【施工员】按 §G 最小清单执行。**
