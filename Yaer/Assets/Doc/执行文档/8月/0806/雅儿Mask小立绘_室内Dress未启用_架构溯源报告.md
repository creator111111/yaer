# 雅儿 Mask 小立绘 · 室内 Dress 未启用 — 架构溯源报告

**文档版本**：v1.0（2026-08-06）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 场景 / 图集 / CSV / 台本**）  
**范围**：对话框左侧 **Mask 小立绘** 服装套装选错——大立绘已是**室内 Dress**，小头像仍是 **GoOut**。不扩新表情枚举、不改台本。  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0806/雅儿Mask小立绘_室内Dress未启用_架构侦探提示词.md`
- `0803/对话框小头像_Mask立绘接线状态与启用方案_…`（Q2 MVP GoOut / 第二小步）
- `技术文档/…/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`
- 现网：`DialogueMaskAvatarPresenter`、`NormalDialogueNewPanel.prefab`、`DialogueAvatarLoader`、`PlayerClothesData`

**Unity**：2020.3.48f1  

**与旧 bug 区分**：不是「小头像全黑」（0806 Prepare）；不是「同套装错 FaceType」（0804）。本期是 **启用了错误那一套 Painting（服装线）**。

---

## ① 结论一句话

**根因是 Mask Presenter 的 MVP 开关 `yaerUseGoOutOnly=1`（Prefab 已序列化）把雅儿小立绘写死为 `GoOutStoryYaerPainting`，从未启用已嵌好的 `YaerPainting`（Dress）。推荐方案 A：关掉写死，按存档 `PlayerClothesData`（衣服骨）在 GoOut↔Dress 间切换并配对应 Face 键——对齐 Loader / OPEN 0727 Q3，并兑现 0803 Q2「第二小步」。**

---

## ② 原因（生活类比）

### 生活类比

衣柜里挂了两套：外出白裙（GoOut）和室内连衣裙（Dress）。大演员已换上连衣裙上台；话筒旁小显示器的遥控器写死「永远播外出频道」，所以对不上号。遥控器开关就是 `yaerUseGoOutOnly`。

### 复现对齐

| 项 | 事实 |
|----|------|
| 大立绘 | 对话 Prefab 场景侧嵌 **Dress 套**（如 `YaerPainting` / 室内线）；视觉为深色连衣裙 |
| Mask 小头像 | 运行时亮 **`GoOutStoryYaerPainting`**（白裙外出） |
| Hierarchy | `YaerAvatarRoot` 下 **两套都在**：GoOut + `YaerPainting`（Dress_Crown_* Faces） |
| Prefab 开关 | `NormalDialogueNewPanel` → Presenter **`yaerUseGoOutOnly: 1`** |
| 引用 | `goOutYaerPainting`/`dressYaerPainting` 序列化为 0 → 靠 `Find("YaerPainting")` 自动绑；**Dress 实例找得到，只是策略不用** |

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板

1. **服装真源**：  
   - **A** 跟存档 `PlayerClothesData`（推荐，对齐历史图集 Loader）  
   - **C** 镜像当前场景大立绘类型（更贴「台上穿什么」；跨对话写死衣服时更稳）  
   - **B** 永远 Dress（**不推荐**：村线外出会错）  
2. **皇冠/头饰**：GoOut 侧是否继续跟存档 Headwear（现 `GoOutStoryYaerPainting.SetDefaultPainting` 已做）？Dress 是否长期只用 `Dress_Crown_*`？  
3. **`yaerUseGoOutOnly` 字段**：删除，还是改成「调试强制 GoOut」默认 false？

### 验收清单

1. 室内/Dress 对话：小头像 = **`YaerPainting` 连衣裙**，与大立绘服装一致。  
2. 外出/GoOut 对话（如村线 `Village_KenMuNiStart` 若仍是外出装）：小头像仍 **GoOut**，不回归。  
3. 表情跟台本 FaceType（至少 Smile/常见脸）；Mask 不黑。  
4. DialogDebug：Dress / GoOut（或改存档衣服）各测一句。

---

## ④ 给程序看的补充

### 4.1 对照表

| 层 | 现网用哪套 | 应由谁决定 | 现网谁在决定 |
|----|------------|------------|--------------|
| 场景大立绘 | 各对话 Prefab 嵌哪套（例：室内 `YaerPainting`；村开场/出门 `GoOutStoryYaerPainting`） | 台本/Prefab 摆哪套（可与存档一致） | **对话 Prefab 实例** |
| Mask 小立绘 | **恒 GoOut**（`yaerUseGoOutOnly`） | 与大立绘/存档服装一致 | **Presenter MVP 写死** |
| Face 键 | GoOut：`Armor_NoHeadWear_{face}`；Dress 键逻辑已写但未走 | 随所选 Painting | Presenter 在 GoOutOnly 分支 |

### 4.2 Presenter 现网分支（钉死）

```csharp
// Prefab: yaerUseGoOutOnly = 1
ResolvePainting(Yaer):
  if (yaerUseGoOutOnly) return goOutYaerPainting;           // ← 现网唯一路径
  return dressYaerPainting ?? goOutYaerPainting;            // false 时：永远 Dress，仍不读存档

ResolveFaceKey(Yaer):
  GoOutOnly → Armor_NoHeadWear_*
  else → Dress_Crown_*（Normal→Dress_Crown_Smile）
```

**假说排查**：

| 假说 | 结论 |
|------|------|
| `dressYaerPainting` Find 失败 | **否为主因**：子物体名 `YaerPainting` 存在；即使引用为 0 也会 Find |
| 大立绘与 Mask 不同源 | **是常态**（两套实例）；根因仍是 Presenter 不切 Dress |
| `yaerUseGoOutOnly=false` 只开 Dress | **代码如此** → 单独关开关会修室内、**弄坏外出**；必须做成 A 条件切换 |
| 旧 Portrait 叠影 | **低**：`useMaskAvatar=1`；截图为 Mask 窗内 GoOut 特征 |
| 0804 FaceType 竞态 | **非主因**；本期是服装线 |

### 4.3 存档真源（方案 A 可复用）

- `PlayerClothesData.GetClothesName(BoneName.Clothes)` → `ClothesName.Clothes.Dress` / `Armor` / …  
- 旧图集：`DialogueAvatarLoader` 已按 **衣服+头饰** 选 Atlas（0727 Q3 建议同源）。  
- GoOut 大立绘头饰：`GoOutStoryYaerPainting.SetDefaultPainting` 已读 `Headwear`（Crown/ArmorHead）。  
- Mask 切到 GoOut 时：Activate 后应仍走该头饰逻辑（场景 Actor 判定已跳过 Smile 覆盖）；若 Start 已跑过，可能需 Presenter 在切 GoOut 时补一次头饰同步（施工验收点）。

### 4.4 Dress Face / Prefab

| 项 | 事实 |
|----|------|
| 键 | Presenter 已备 `Dress_Crown_{faceType}`；`Normal`→`Dress_Crown_Smile` |
| `YaerPainting` Faces（母体抽样） | 有 Smile/Laugh/Sad/Daze/Smug/Unhappy/Surprised/VerySurprised 等；**未必有**全部新枚举脸 |
| Heads | Dress 套可能无独立 Heads；切脸靠 Faces 键即可 |
| 风险 | 启用 Dress 后某新表情缺节点 → 空脸（次要；先钉服装） |

### 4.5 方案比选表

| 方案 | 做法摘要 | 村线 GoOut | 室内 Dress | 改动面 | 风险 | 推荐？ |
|------|----------|------------|------------|--------|------|--------|
| **A** | `yaerUseGoOutOnly` 默认 false/作废；按 `PlayerClothesData` Clothes 选 GoOut vs Dress + 对应 Face 键 | **保**（存档 Armor/外出） | **保**（存档 Dress） | Presenter + Prefab 开关 | 对话 Prefab 写死衣服与存档不一致时仍可能偏（少见） | **推荐** |
| B | 仅 false → 永远 Dress | **坏** | 好 | 一行 | 村开场小头像变连衣裙 | ❌ |
| C | 镜像场景当前大立绘类型（扫 Actor 下 Painting 类型 / 名） | 跟台上 | 跟台上 | Presenter 稍复杂 | 须定义「当前大立绘」；旁白无雅儿时 | 次选（台上≠存档时更准） |
| D | 按对话 Prefab/章节配置写死 | 可配 | 可配 | 配置膨胀 | 易与存档漂移 | 不推荐作主方案 |

### 4.6 与历史 OPEN

| 条目 | 关系 |
|------|------|
| **0803 Q2**「MVP 固定 GoOut；Dress 第二小步」 | **第二小步到期**；本报告落地后应标 ⛔ 被取代 / ✅ 已拍板施工 |
| **0727 Q3**「小头像服装跟 `PlayerClothesData`」 | 方案 A **采纳**该建议 |
| **0804** 首句 FaceType | **不是**本期主修；启用 Dress 后顺带验表情即可 |
| **0806** Prepare 白名单 | **保留**；勿为换装恢复名字广扫 |

### 4.7 施工员最小改动清单（只建议）

| 优先级 | 文件 | 做什么 |
|--------|------|--------|
| 1 | `DialogueMaskAvatarPresenter.cs` | 雅儿：读 `PlayerClothesData`（Clothes==Dress → dress+`Dress_Crown_*`；否则 GoOut+`Armor_NoHeadWear_*`）；`yaerUseGoOutOnly` 仅作调试强制 GoOut |
| 2 | `NormalDialogueNewPanel.prefab` | `yaerUseGoOutOnly` → **0**（或保留字段默认 false） |
| 3 | （可选）切 GoOut 时补头饰同步；`[MaskAvatar] Yaer → GoOut\|Dress face=…` 日志 | |
| 不改 | 拆掉 GoOut 实例；台本；Prepare 白名单；四套图集大改造 | |

**禁止**：Update 轮询换装；重写整棵 Painting 系统；为修服装回退 Mask 黑窗修复。

### 4.8 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | 服装真源：存档 vs 镜像大立绘？ | **存档 `PlayerClothesData`（方案 A）** |
| Q2 | 皇冠/头饰是否跟 Clothes 子状态？ | GoOut 跟 Headwear；Dress 暂 `Dress_Crown_*` |
| Q3 | `yaerUseGoOutOnly` 删除还是调试强制？ | **保留为调试强制 GoOut，默认 false** |

---

## 施工员下一轮最小化清单（建议 · 待拍板后开）

1. Presenter 按存档切换 GoOut/Dress + Face 键；Prefab 关闭写死 GoOut。  
2. 室内 Dress / 村线 GoOut 双验收；表情与 Mask 不黑。  
3. 更新 OPEN：0803 Q2 第二小步结案。  

**✅ 已施工（2026-08-06）**：方案 A 落地——`DialogueMaskAvatarPresenter` 读 `PlayerClothesData`；`yaerUseGoOutOnly` 默认/Prefab=false；`GoOutStoryYaerPainting.SyncHeadwearFromArchive`；OPEN 0803 Q2 / 0806 Q1–Q3 结案。验收见上文清单。
