# Village_村长家门口初次对话 — 三人大立绘 + Face1～3 Import — 架构溯源报告

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读溯源 + 落地方案（**本阶段未改代码 / Prefab / CSV**）  
**Unity**：2020.3.48f1  
**台本**：`Assets/Dialog/Village_村长家门口初次对话.csv`（村句已写 **Face1/2/3**）  
**目标 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab`（**尚未落盘**）  
**用户卡点**：Import `ID 2 FaceType「Face3」非法，须为 DialogueFaceType`  
**关联**：`执行文档/0831/精灵村长立绘_UI版Mask小表情Face123_架构溯源报告.md` · `施工说明/0831/…Face123_施工说明.md` · 商人店行分流样板  
**提示词**：`提示词/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构侦探提示词.md`

---

## ① 结论一句话

**Import 红字因只有「店」行允许 Face1～5，「村→村长」仍走 `DialogueFaceType`；须做 C1 村长分流（对齐店：BB `UseChiefPortrait`+`ChiefFace`），并把仍为 SR 的 `ChiefPainting` UI 化后与雅/古一起挂进门口对话 Prefab，前奏三路淡入；Mask 已有 `ChiefMaskPainting`，门口须直吃 Face1～3 而非晚宴 F2 映射。**

---

## ② 原因（通俗）

导入工具顶栏写着 Face1～5，但代码里**只有老板娘那一行**认这些字；村长行仍按雅/古表情名校验，所以 `Face3` 直接报红。  
门口对白要三个人站一起说话，还要把村长三脸接到大立绘和小窗——小窗 Mask 刚做好，**大立绘 Prefab 磁盘上还是 SpriteRenderer，不能直接塞对话容器**；门口对话 Prefab 本身也还没生成。

---

## ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | CSV Import 无「Face3 非法」；Generated 出盘 | |
| 2 | 成品 Prefab 内 **雅 + 古 + 村** 三张大立绘同场（可淡入） | |
| 3 | 村句 Face1/2/3：大立绘与 Mask 一致 | |
| 4 | 雅/古句仍走 DialogueFaceType，不坏 | |
| 5 | 商人店句 Face1～5 回归 OK | |
| 6 | Play 走到「快进屋」无 Missing / 空窗 | |

---

## ④ 给程序

### A. Import 红字调用链（已磁盘证实）

```
CSV ID2: Speaker=村, FaceType=Face3
  → DialogueSpeakerMapping: 村 → Actor「村长」
  → DialogueCsvParser 校验 FaceType:
       if IsShopkeeperRow(店/老板娘) → 允许 Face1～5
       else → Enum.TryParse<DialogueFaceType>(「Face3」) → 失败
  → 红字：须为 DialogueFaceType 枚举名
```

| 证据 | 路径 |
|------|------|
| 仅店行分流 | `DialogueCsvParser.cs` ~151–171 |
| 店判定 | `ShopkeeperCsvDefaults.IsShopkeeperRow`（Actor「老板娘」/ Speaker「店」） |
| Import 窗文案 | 「店填 Face1～Face5」——**未写村长**，易误解 |
| 村长空列默认 | `DialogueFaceTypeCsvDefaults` 仍 Warning「立绘未就绪」→ Normal |

**门口 CSV 村句已大量 Face1/2/3**（非 Smile/CloseEyes）→ 与晚宴 F2 映射路径**不一致**；修 Import 必须认 Face1～3，不能只改回 Smile。

---

### B. Face1～3 方案（拍板 C1）

| 方案 | 做法 | 裁定 |
|------|------|------|
| **C1 · 村长行分流（对齐店）** | Parser 认「村长」Face1～3→`ChiefFaceType`；GraphBuilder 写 `UseChiefPortrait`+`ChiefFace`；TMP 双轨驱大立绘+Mask | ✅ **推荐**（CSV 已写 Face1～3） |
| C2 · CSV 改回 Smile/CloseEyes | 少改 Parser；与现门口 CSV 冲突 | ❌ 本期否 |
| C3 · Face1～3 进全局 `DialogueFaceType` | 污染雅/古 | ❌（0831 已否） |

#### B1. C1 最小接线（仿店）

| 层 | 店（已有） | 村长（本期加） |
|----|------------|----------------|
| Parser 分流 | `ShopkeeperCsvDefaults` | 新建 `ChiefCsvDefaults`（Actor「村长」/ Speaker「村」；Face1～3） |
| 节点 BB | `UseShopkeeperPortrait` + `ShopBody/Face` | **`UseChiefPortrait` + `ChiefFace`**（`StatementNodeEx`） |
| 字幕 Info | `SubtitlesRequestInfoEx` 店字段 | 增 `UseChiefPortrait` + `ChiefFace` |
| GraphBuilder | 店行写 BB | 村长行写 BB；`FaceType` 可置 None |
| TMP | Registry + `ApplyShopkeeperPortrait` + Invoke(None) | 大立绘 Apply + `ApplyChiefPortrait(ChiefFace)` + Invoke(None)+**chiefMaskActive 旗**（防 HideAll） |

**晚宴兼容**：仍可 `RoleName.Chief` + Smile/CloseEyes → 现网 `MapToChiefFace`（F2）。  
**门口**：走 `UseChiefPortrait` **直通** Face1～3，不经 `DialogueFaceType`。

---

### C. 与 Mask 0831 关系

| 已有 | 门口缺口 |
|------|----------|
| `ChiefMaskPainting` + `ChiefFaceType` + `ApplyChiefPortrait` | Import 过不了 → 图建不出 |
| `MapToChiefFace(DialogueFaceType)`（晚宴 Smile→Face3…） | 门口 CSV **已是 Face1～3**，F2 **吃不到** |
| `DialogueRoleName.Chief`；晚宴 Leader→Chief | 门口 Prefab **未建**；Actor 须 RoleName=Chief |
| `ChiefPainting.prefab` 磁盘有 | **仍是 SR**（Layer0 + SpriteRenderer +「组 2」）❌ 不能塞 DialogueSceneContainer |

---

### D. 三人大立绘架构

| 角色 | CSV | 大立绘 | Mask |
|------|-----|--------|------|
| 雅 | 雅 | `GoOutStoryYaerPainting`（村线铠甲） | 现网 Presenter |
| 古 | 古 | `GushaPainting` | 现网 |
| 村 | 村 | **UI `ChiefPainting`**（须重做/转 UI） | `ChiefMaskPainting` |

#### D1. 对照 `Village_KenMuNiStart`

| 项 | KenMuNiStart | 门口初次对话 |
|----|--------------|--------------|
| 大立绘 | **雅 + 古** 两台 + BB CanvasGroup 并行淡入 | 须 **第三路村长** |
| 参考 Import | 用户可选 KenMuNiStart 作壳 | **壳可借，不能只生成双人图** |
| PrepareMask | 曾误伤小头像（0806） | 淡入白名单 **勿广扫** Mask |

结构倾向：

```
Village_村长家门口初次对话
  DialogueSceneContainer（或等价）
    GoOutStoryYaerPainting   (CanvasGroup BB)
    GushaPainting            (CanvasGroup BB)
    ChiefPainting            (CanvasGroup BB) ← UI Face1底+Face2/3贴脸
  Actors: 雅尔 / 古莎 / 村长（RoleName.Chief）
  前奏: 三路 CanvasGroupAlpha 并行（Q4 倾向是）
```

#### D2. `ChiefPainting` 脚本

| 选项 | 说明 | 倾向 |
|------|------|------|
| 大立绘挂同一套 `ChiefMaskPainting.Apply` | Prefab 分离、脚本复用 | ✅ 最快 |
| 抽 `ChiefPortraitController` 共用 | 更干净 | P1 |
| SR 直嵌 | — | ❌ |

**叠法**：与 Mask 一致——Face1 常亮 + Face2/3 互斥贴脸；`组 2`→改名 Face1。

#### D3. 大立绘运行时谁 Apply？

店：`ShopkeeperFaceRegistry` 场景合层。  
门口立绘在 **对话 Prefab 内**：倾向 TMP 在 `UseChiefPortrait` 分支  
`GetComponentInChildren<ChiefMaskPainting>(true)`（或专用组件）对 **Dialogue 根下大立绘** Apply；Mask 仍走 Presenter。  
替代：对话级 mini-Registry——可后做。

---

### E. Prefab / 场景流水线

| # | 步骤 | 说明 |
|---|------|------|
| 1 | C1 Import 修复 | Parser + GraphBuilder + StatementNodeEx + Info + TMP |
| 2 | UI 化 `ChiefPainting` | Image/Rect/CanvasGroup/Layer5；Face1/2/3；挂 Apply 脚本 |
| 3 | Import → Generated → 成品 Prefab | 嵌三立绘；三路淡入；Actor RoleName |
| 4 | 场景 Trigger | 见 Q1（现网 **无** `Village_村长家门口*` Prefab；`House_Chief` 仅为进屋门 NextScene） |
| 5 | 验收 | §③ |

**触发（OPEN Q1）**：`Village_KenMuNi1` 检索未见门口初次对话 `StoryPrefabName`；`House_Chief` 是换场进屋。倾向在 **村长家门前** 新建 `SimpleStoryTrigger`（存档单次）→ `Village_村长家门口初次对话`；精确定位点施工时对 Hierarchy。

---

### F. 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `ChiefCsvDefaults` + Parser 村长行 Face1～3 | **P0** |
| 2 | `StatementNodeEx` / `SubtitlesRequestInfoEx`：UseChiefPortrait + ChiefFace | **P0** |
| 3 | GraphBuilder 村长行写 BB | **P0** |
| 4 | TMP：UseChiefPortrait → 大立绘 Apply + `ApplyChiefPortrait` + None 旗 | **P0** |
| 5 | UI 化 `ChiefPainting.prefab`（禁 SR） | **P0** |
| 6 | Import 门口 CSV → 成品 Prefab 三立绘+三路淡入 | **P0** |
| 7 | 场景 TriggerStory（门前） | **P0/P1** |
| 8 | Import 窗文案：「村填 Face1～3」 | P1 |
| 9 | 更新 `DialogueFaceTypeCsvDefaults` 村长 Warning 文案 | P2 |
| 10 | Face1 进全局枚举 / 改门口 CSV 回 Smile | ❌ |

**勿动**：商人 Body×Face；晚宴全文 CSV（Import 修复可共用）；KenMuNiStart 分层黑幕强绑。

---

### G. 验收清单（施工员）

- [ ] Import 无 Face3 非法；Generated + 成品 Prefab 存在  
- [ ] Prefab 同时可见/可淡入三张大立绘  
- [ ] 村句 Face1/2/3 大+Mask 一致  
- [ ] 雅/古 DialogueFaceType 不坏；店 Face1～5 OK  
- [ ] Prepare/淡入不误伤 Mask 小头像  
- [ ] Play 至「快进屋」无空窗 / Missing  

---

### H. 开放问题

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 门口对白触发场景/物体？ | KenMuNi1 门前新建 Trigger；非进屋门本身 | ⏳ |
| Q2 | 三立绘左右站位？ | 产品/美术；施工先占位 | ⏳ |
| Q3 | 大立绘脚本复用 Mask 还是独立？ | **复用 Apply 组件**；Prefab 分离 | ⏳ |
| Q4 | 前奏是否淡入三立绘？ | **是**（对齐用户勾选） | ⏳ |
| Q5 | 进城后是否还播门口对白？ | 存档单次；与进屋门解耦 | ⏳ |

---

### I. 程序速查

| 项 | 现状 |
|----|------|
| 门口 CSV | ✅ 有；村句 Face1/2/3 |
| 门口 Dialogue Prefab | ❌ 无 |
| `ChiefMaskPainting` | ✅ UI Mask |
| `ChiefPainting` | ❌ 仍 SR（组2/Face2/Face3） |
| `DialogueRoleName.Chief` | ✅ |
| Import 村长 Face1～3 | ❌ 仅店分流 |
| `MapToChiefFace` | ✅ 晚宴用；门口直写 Face 不适用作主路径 |
