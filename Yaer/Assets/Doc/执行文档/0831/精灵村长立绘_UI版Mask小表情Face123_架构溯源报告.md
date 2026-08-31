# 精灵村长立绘 — UI 版 Mask 小表情 Face1/2/3 — 架构溯源报告

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读溯源 + 方案拍板（**本阶段未改 Prefab / 代码 / CSV**）  
**Unity**：2020.3.48f1  
**源 Prefab（SR）**：`Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘.prefab`  
**对照样板**：`MerchantMaskPainting` + `DialogueMaskAvatarPresenter.ApplyShopkeeperPortrait`  
**挂点**：`NormalDialogueNewPanel` → `Bottom/Mask/YaerAvatarRoot`  
**产品表情（钉死）**：仅 **Face1 / Face2 / Face3**；Face2、Face3 都不开 → **Face1**（禁止空白）  
**提示词**：`提示词/0831/精灵村长立绘_UI版Mask小表情Face123_架构侦探提示词.md`

---

## ① 结论一句话

**须新建 UI 版 `ChiefMaskPainting`（Face1←「组 2」底 + Face2/Face3 互斥贴脸），挂 Mask；扩 `DialogueRoleName.Chief` 并修正晚宴 Actor「Leader」现为 `RoleName=None` 的缺口；CSV 的 Smile/CloseEyes 等用映射表落到 Face1～3（F2），勿把 Face1 塞进全局 `DialogueFaceType`。**

---

## ② 原因（通俗）

村长现网立绘是场景用的 **SpriteRenderer 海报**，对话框小窗要的是 **UI Image**——不能直接塞进 Mask。  
晚宴里「村长」Actor 叫 Leader，但角色枚举是 **None**，小表情系统不认，窗一直空。  
美术只有三张脸（底图 / 闭眼 / 笑颜），台本却写 Smile、Normal——要先定「旧名→三脸」对照表再接线。

---

## ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村长句 Mask 可见立绘（非空白） | |
| 2 | 默认 / Face1 → 仅默认脸（底图） | |
| 3 | Face2 → 开闭眼层；Face3 → 开笑颜层 | |
| 4 | Face2/3 都关 → 仍显示 Face1 | |
| 5 | 切到雅/古句 → 村长 Mask 关闭 | |
| 6 | DialogDebug / 晚宴对白可测三脸 | |
| 7 | 商人小表情回归不坏 | |

---

## ④ 给程序

### A. SR ≠ UI（钉死）

| 项 | `精灵村长游戏中立绘`（源） | Mask 目标（对齐商人） |
|----|---------------------------|----------------------|
| 根 | **Transform only** | **RectTransform** + CanvasRenderer + **CanvasGroup** + 脚本 |
| Layer | **0** | **5（UI）** |
| 叶子 | **SpriteRenderer** | **Image** |
| 脚本 | 无 | 新建 `ChiefMaskPainting` |
| 子节点 | `组 2` / `Face2` / `Face3` | 改名 **`Face1`** / Face2 / Face3 |

**禁止**把 SR Prefab 直接嵌 `YaerAvatarRoot`。保留 SR 作参考或日后场景大立绘。

---

### B. 源树与叠法裁定

| 子物体 | 资源文件 | SortingOrder | 本地偏移（约） |
|--------|----------|--------------|----------------|
| **组 2**（→Face1） | `组 2.png` | **0** | 根下底图 |
| **Face2** | `闭眼.png` | **1** | ≈(9.5, 23.3) 头区 |
| **Face3** | `笑颜.png` | **2** | ≈(9.4, 22.5) 头区 |

**叠法拍板（施工默认）**：**Face1 底图常亮** + **Face2 / Face3 互斥贴脸**（二者不同时开）。

| 意图 | Active |
|------|--------|
| Face1 / 默认 | Face1 **开**；Face2、Face3 **关** |
| Face2 | Face1 **开**；Face2 **开**；Face3 **关** |
| Face3 | Face1 **开**；Face3 **开**；Face2 **关** |

**否决**：默认三张完整立绘只亮一张（与头区偏移 + SortingOrder 分层 + 文件名「闭眼/笑颜」不符）。  
**施工验收**：Scene 肉眼确认贴脸是否盖住底图五官；若美术实为整换图，再改互斥三选一（记 OPEN）。

现网 Prefab 三脸全 `Active=1` → UI 化后默认态须校正为 **仅 Face1 开**（Face2/3 关）。

---

### C. Mask 接线方案（推荐方案 A）

对齐商人：**专用 UI Prefab + 专用脚本**，不继承 `StoryFormPainting`，不把 Face1 塞进雅式 `DialogueFaceType`。

| 层 | 命名（拍板） | 说明 |
|----|--------------|------|
| UI Prefab | **`ChiefMaskPainting.prefab`** | 与 `MerchantMaskPainting` 英文一致；`Assets/Prefabs/DialougeProtrait/` |
| 脚本 | **`ChiefMaskPainting.cs`** | `Apply(ChiefFaceType face)`；**无 Start 自动 Reset** |
| 局部枚举 | `ChiefFaceType { Face1, Face2, Face3 }` | **勿**写入 `DialogueFaceType` |
| 挂点 | `YaerAvatarRoot/ChiefMaskPainting` | 默认 **Active=false** |
| Presenter | `case Chief` → `ApplyChiefPortrait` | HideAll 须关 Chief |
| 场景大立绘 | **本期不做** | 无 Registry；只做 Mask |

#### C1. Role 缺口（已核实 · 必修）

| 位置 | 现网 |
|------|------|
| `DialogueRoleName` | **无** Chief / 村长 |
| Import | Speaker `村` → Actor 参数名 **`村长`** |
| 晚宴 Prefab `Village_Leader…` | Actor GO **`Leader`**，`_roleName: **0** = None` |

```
村长句 → actor.RoleName == None
  → OnGetNewStatement(None, …)
  → Presenter HideAll / return
  → Mask 空 ★
```

**拍板**：`DialogueRoleName` **末尾追加** `Chief`（禁止插中间打乱 int）+ 晚宴/村长图 Actor 的 `_roleName` 设为 **Chief**。

```
Apply(Chief, faceType)
  → HideAll（含 Merchant / 雅 / 古…）
  → ChiefMaskPainting.SetActive(true)
  → Apply(MapToChiefFace(faceType))
```

**对比商人**：店句走 `UseShopkeeperPortrait` + Invoke(None) 特殊旗；村长走 **正常 Role 分支**（更干净），无需 Shop 式 None 旗。

#### C2. 方案对照

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A · UI Prefab + ChiefMaskPainting + RoleName.Chief** | 上表 | ✅ **推荐** |
| B · StoryFormPainting + Faces/Face1 | 可行；易与 Laugh 键混 | ⚠️ 次选 |
| C · SR 直嵌 Mask | — | ❌ |
| D · DialogueFaceType 加 Face1 | 污染雅/古 | ❌ |

---

### D. CSV / FaceType 映射（开放 · 勿擅自猜完）

晚宴 `Village_村长家晚宴对白台本.csv` 村长句 FaceType 统计（约）：

| FaceType | 约次数 | 资源假说 |
|----------|--------|----------|
| **Normal** | ~20 | → **Face1** |
| **CloseEyes** | ~8 | → **Face2**（闭眼.png） |
| **Smile** | ~8 | → **Face3**（笑颜.png） |
| Sad | ~4 | 待产品 |
| Laugh | ~1 | 待产品 |
| 空 | ~1 | → Face1（Import 默认 Normal） |

| 策略 | 说明 | 倾向 |
|------|------|------|
| **F2 · 运行时映射表** | 保留 Smile 等；`MapToChiefFace` | ✅ **本期推荐**（少改晚宴 CSV） |
| F1 · CSV 改 Face1/2/3 | 与产品键一致；须批量改行 | 新台本可用 |
| F3 · 新列 ChiefFace | 仿 ShopFace | ❌ 偏重 |

**待产品确认表（施工默认草稿 · 未拍板勿当终裁）**：

| 旧 DialogueFaceType | → ChiefFace | 依据 |
|---------------------|-------------|------|
| Normal / None / 空 | **Face1** | 默认底 |
| CloseEyes | **Face2** | 闭眼.png |
| Smile | **Face3** | 笑颜.png |
| Sad / Laugh / 其它 | **Face1**（占位） | ⚠ 须产品确认 |

**严禁**：无对照表就批量改晚宴 CSV 语义；把 Face1 加入全局 `DialogueFaceType`。

---

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 新建 `ChiefMaskPainting.prefab`：UI 化；`组 2`→**Face1**；Image 绑原 Sprite | **P0** |
| 2 | 新建 `ChiefMaskPainting.cs` + `ChiefFaceType`；Apply 叠法 §B；无 Start Reset | **P0** |
| 3 | `DialogueRoleName` 末尾加 **Chief**；晚宴 Actor Leader `_roleName=Chief` | **P0** |
| 4 | Presenter：引用 + HideAll + `case Chief` + F2 映射 | **P0** |
| 5 | 嵌 `NormalDialogueNewPanel/YaerAvatarRoot`，默认关；摆位独立定稿 | **P0** |
| 6 | 产品确认 Sad/Laugh 映射；可选新台本改 F1 | P1 |
| 7 | 场景大立绘合层 / Registry | ❌ 本期不做 |
| 8 | 商人 Body/Face、雅/古枚举插入 | ❌ |

**预期 diff**

- 新：`ChiefMaskPainting.cs`、`ChiefMaskPainting.prefab`  
- 改：`RoleName.cs`、`DialogueMaskAvatarPresenter.cs`、`NormalDialogueNewPanel.prefab`、晚宴 Dialogue Prefab Actor  
- 保留：`精灵村长游戏中立绘.prefab`（SR）

---

### F. 验收清单（给施工员）

- [ ] Mask 窗村长句可见立绘（非空白）  
- [ ] Face1 默认；Face2=闭眼；Face3=笑颜  
- [ ] Face2/3 都关 → Face1；禁止双贴脸同开  
- [ ] 雅/古句关村长 Mask；商人回归 OK  
- [ ] Leader.RoleName=Chief；不再 None 空窗  
- [ ] 无 Start Reset 盖首句  

---

### G. 开放问题

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | 叠法：底+贴脸 vs 三张互斥？ | **底+贴脸** | ⏳ 肉眼验收可改 |
| Q2 | CSV 策略 F1 / F2 / F3？ | **F2 映射** | ⏳ |
| Q3 | Smile→Face3、CloseEyes→Face2？ | **是**（按文件名） | ⏳ 产品确认 |
| Q4 | Sad / Laugh →？ | 暂 **Face1** | ⏳ |
| Q5 | Prefab 名？ | **ChiefMaskPainting** | ⏳ |
| Q6 | 本期场景大立绘？ | **否，只 Mask** | ✅ 默认 |
| Q7 | Actor 显示名 vs GO「Leader」？ | 保持；只改 RoleName | ✅ |

---

### H. 与商人样板对照（速查）

| | 商人 | 村长（本期） |
|--|------|--------------|
| 驱动 | `ApplyShopkeeperPortrait` + Invoke(None) | `Apply(Chief, faceType)` → Map → `Apply(face)` |
| 结构 | Body×3 × Face×5 | **仅 Face×3（底+2 贴脸）** |
| Role 枚举 | 不扩（店句专用旗） | **须扩 Chief**（现网 Leader=None） |
| Start Reset | 禁止 | 同左 |
