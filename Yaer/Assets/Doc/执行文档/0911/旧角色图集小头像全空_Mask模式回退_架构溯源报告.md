# 旧角色图集小头像全空 — Mask 模式回退 — 架构溯源报告

**文档版本**：v1.0（2026-09-11）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 图集 / Git  
**Unity**：2020.3.48f1  
**现象**：父亲、莱、夏尔（小孩）、将军说话时，对话框左侧小头像全空  
**产品期望**：四人恢复 **旧图集** 小头像（跟 FaceType）；雅儿/古莎等 **Mask 角色保持 Mask**；**哥布林不要求**小头像  
**不是**：四人立刻做完整 Mask Painting；关全局 `useMaskAvatar`；改大立绘/台本；给哥布林补头像  
**并行**：与同日 NewGame 大立绘 / 开局 Mask 闪错脸案解耦  
**提示词**：`Assets/Doc/提示词/0911/旧角色图集小头像全空_Mask模式回退_架构侦探提示词.md`

---

## 沟通摘要

### ① 结论一句话

**根因 = H1：`useMaskAvatar=true` 后 `OnGetAvatar` 永久关掉旧 `actorPortrait`，而 Presenter 又不支持 King/Lai/Xiaer/LinEn → Mask 空 + 旧槽关 = 双路皆空。图集与 Loader 仍在；「小孩」=夏尔（Xiaer），不是哥布林。推荐 A′ 白名单四人混合回退。**

### ② 原因（通俗）

对话框左边现在优先走「Mask 立绘」新路。  
雅儿那些已经摆好 Mask 的角色：新路有图。  
父亲/莱/夏尔/将军从没做过 Mask，新路故意留空；同时旧图集那条槽被开关一刀切死，有图也不亮。  
所以不是图丢了，是 **两扇门都关了**。  
小孩指的是 **夏尔**；哥布林不用管（空着可通过）。

### ③ 用户自查清单

| # | 操作 | 预期（现网） |
|---|------|----------------|
| 1 | 播父亲（King）/ 莱 / **夏尔** / 将军（LinEn）句 | 左槽空；Mask 下各 Painting 全关 |
| 2 | Hierarchy：`actorPortrait`（或节点名 `Yaer`） | **Active=false**；Image.sprite **可能仍有图**（Loader 写了但不显示） |
| 3 | 打开历史列表同句 | 往往 **仍有** 图集头像（旁证 Loader 活着） |
| 4 | 播一句雅儿 | Mask 有脸；旧 Portrait 仍关（对照勿回归） |
| 5 | 哥布林句 | 空着 **不算失败** |

### ④ 程序补充

见下文 §1～§8。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H1 成立**：Mask 真源 + 未支持 Role + 强制关 Portrait → 双空 |
| **Loader** | **H2 排除倾向**：四人 `Avatar_*.spriteatlas` 仍在；`RefreshAvatar`→`GetAvatar` 仍跑；历史仍用图集 |
| **小孩** | **`Xiaer`（夏尔）**；Actor `_roleName: 3`；**不是** Goblin |
| **截图说法** | **H5**：用户说的「旧截图/图集」现网 = **SpriteAtlas + actorPortrait**，不是 0727 未落地 RT 截图 |
| **方案** | **A′ 白名单四人**（推荐）：仅 King/Lai/Xiaer/LinEn 在有 sprite 时亮旧槽；Mask 角色不变；哥布林保持空 |
| **禁止** | C 全局关 `useMaskAvatar`；未拍板做 B（四人 Mask 化） |

---

## 2. 证据链

### 2.1 角色钉名（产品 + Actor 核对）

| 用户口称 | `DialogueRoleName` | int | 复现 Prefab 例 | Actor `_name` | 图集 |
|----------|-------------------|-----|----------------|---------------|------|
| **父亲** | **King** | 2 | `NewGameStory.prefab`（`_roleName: 2`） | 「王」 | `Assets/GameRes/Atlas/Avatar/Avatar_King.spriteatlas` |
| **莱** | **Lai** | 4 | `ForestSceneLaiFirstDialogue.prefab`（`_roleName: 4`） | — | `Avatar_Lai.spriteatlas` |
| **小孩** | **Xiaer（夏尔）** | 3 | `HomeScene2Xiaer.prefab` 等（`_roleName: 3`，名「夏尔」） | 夏尔 | `Avatar_Xiaer.spriteatlas` |
| **将军** | **LinEn（琳恩）** | 5 | `ForestSceneLinEnStory.prefab`（`_roleName: 5`） | — | `Avatar_LinEn.spriteatlas` |

| 排除 | Role | 本期 |
|------|------|------|
| 哥布林兄/弟 | GoblinElder / GoblinYounger | **不要求**小头像；图集文件仍存在，但**勿列入验收必过** |

枚举：`DialogueRoleName`：`None, Yaer, King, Xiaer, Lai, LinEn, Gusha, GoblinElder, GoblinYounger, Amy, Aliy, Chief`  
图集对照：`执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md` §4.2～4.5  

路径：`DialogueAvatarPathHelper.GetPath(roleName)` → 非雅儿为 `Avatar_{Role}` 图集路径。

### 2.2 双路显示逻辑（同构用户现象）

```
句开始（有 DialogueActorEx）
  → actor.RefreshAvatar(FaceType)
       → DialogueAvatarLoader.GetAvatar(role, face)   // 图集仍加载
       → callback → DialogueTMPUGUI.OnGetAvatar
            → if useMaskAvatar: actorPortrait.SetActive(false)  // ★ 旧槽永关
                 （sprite 仍可写入 Image，但不显示）
  → OnGetNewStatement(role, face)
       → DialogueMaskAvatarPresenter.Apply
            → HideAllPaintings()
            → ResolvePainting(role)
                 Yaer/Gusha/Amy/Aliy → Painting
                 King/Lai/Xiaer/LinEn/Goblin… → null → return   // ★ Mask 空
```

**对照表**

| 句类型 | Mask | 旧 Portrait（现网） | 左槽体感 |
|--------|------|---------------------|----------|
| Yaer / Gusha / Amy / Aliy | Apply 亮对应 Painting | 强制关 | Mask 有脸 |
| Chief / 店专用口 | 专用 Apply | 强制关 | Mask 有脸 |
| **King / Lai / Xiaer / LinEn** | Resolve=null，保持空 | **强制关** | **双空** |
| Goblin* | Resolve=null | 强制关 | 双空（本期可接受） |
| 旁白 None | HideAll | 关 | 空（正确） |

### 2.3 关键代码锚点

| 点 | 路径 | 要点 |
|----|------|------|
| 开关 | `NormalDialogueNewPanel`：`useMaskAvatar: 1` | 全对话壳 Mask 真源 |
| 关旧槽 | `DialogueTMPUGUI.OnGetAvatar` ≈413–428 | Mask 模式下 **永不** `SetActive(true)`；注释写明供历史用图集 |
| 字段注释 | 同文件 ≈53–56 | Loader 仍跑；字幕以 Mask 为真源 |
| Apply | `DialogueMaskAvatarPresenter.Apply` ≈82–114 | 注释：**「未支持角色（King/Lai…）保持 Mask 空」** |
| Resolve | 同文件 ≈239–258 | switch 仅 Yaer/Gusha/Amy/Aliy；default null |
| Loader | `DialogueAvatarLoader.GetAvatar` | 按 Role 解析图集；四人路径仍有效 |
| 历史 | `HistoryDialogueBox` → `GetAvatar` | 不经 OnGetAvatar 关槽逻辑 → 历史可仍有图 |

### 2.4 文档债（与本期产品冲突）

| 文档 | 旧决议 | 本期产品 |
|------|--------|----------|
| 0803 Mask 接线报告 | 其它角色本期无实例 → **Mask 空**；旧 Portrait 可关死作字幕真源 | 四人要 **旧槽回亮** |
| OPEN 0804 Q2 | 「旧 Portrait **完全关闭**」✅ | **须修订**：改为 **混合**（Mask 角色关 Portrait；白名单旧角色可亮） |
| 0727「截图小头像」 | Prefab 嵌套截图方案另路 | 现网旧路仍是 **图集 Portrait**，非 RT 截图 |

→ 0803「Mask 空」是 **MVP 缺口**，不能当产品终态。

---

## 3. 假说表（H1～H6）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H1** | useMaskAvatar + 无 Mask Painting + 关 Portrait → 双空 | **✅ 主因** | OnGetAvatar / Apply 注释与代码同构现象 |
| **H2** | 图集丢了 / sprite 本 null | **排除倾向** | 四人 atlas 在磁盘；Loader 路径通；历史旁证；Play 可看 Portrait.sprite |
| **H3** | Actor 未绑 / Role=None | **排除（四人复现句）** | Prefab 上 `_roleName` 2/3/4/5 已钉 |
| **H4** | 仅个别 Prefab 坏 | **排除倾向** | 壳级 `useMaskAvatar`；任意未支持 Role 同构 |
| **H5** | 「截图」=0727 RT 方案 | **澄清** | 现网旧显示路 = SpriteAtlas+Portrait |
| **H6** | PrepareMask/HideAll 残留主因 | **非主因** | 无预亮句仍空；根因是每句 Apply null + 关 Portrait |

---

## 4. 方案裁定

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A · 凡 Resolve=null 且有 sprite 就亮旧槽** | 含 Goblin 也会亮（图集存在） | 可用，但产品说哥布林不要求；易「多亮」 |
| **A′ · 白名单四人** | 仅 **King / Lai / Xiaer / LinEn** + sprite≠null → 亮 Portrait；其它 null 保持空 | **✅ 推荐默认** |
| **B · 四人 Mask Painting** | 新建嵌套 + 扩 Resolve | 工作量大；用户要旧系统 → **非本期** |
| **C · useMaskAvatar=false** | 全员回旧 Portrait | **禁止终局**（毁雅儿 Mask） |
| **D · 改个别对话 Prefab** | 换壳 | 易漏；壳是全局 |

**互斥要点（施工须写清）**

1. Mask 支持角色（含 Chief/店）：继续关 Portrait，避免双影。  
2. 白名单旧角色：亮 Portrait；Mask 子树保持 HideAll 后的空。  
3. 切回 Mask 角色或旁白：关 Portrait。  
4. 挂点建议：以 `OnGetAvatar` 为主分支（已知 role 可从当前 actor / 缓存本句 Role）；或 Presenter.Apply 在 null 时发「请求旧槽」——**选一处真源，禁止两处抢 Active**。

---

## 5. 建议施工步骤（只建议不施工）

1. **方案 A′**：代码混合回退；**不改**全局 `useMaskAvatar=false`；**不改**图集名/台本。  
2. 白名单硬编码或静态集：`King, Lai, Xiaer, LinEn`。  
3. GoblinElder/Younger：**不**进白名单；空着通过。  
4. 其它未点名 Role（若有）默认不扩 → 记 OPEN。  
5. 回归：雅儿/古莎 Mask；店/村长旗不黑块；历史列表不回归。  
6. 施工说明：`Assets/Doc/施工说明/0911/旧角色图集小头像_Mask混合回退_施工说明.md`  
7. **禁止**未拍板做 B；**禁止**为哥布林开专项。

### 验收

| 必过 | 不过（勿当失败） |
|------|------------------|
| 父/莱/**夏尔**/将军：左槽有旧图集头像，脸跟 FaceType | 哥布林句空着 |
| 雅儿等 Mask 句：仍 Mask，无 Portrait 双影 | — |
| 历史列表头像正常 | — |

---

## 6. 剩余风险

| 风险 | 说明 |
|------|------|
| 双影 | Mask 句未关 Portrait → 左右叠影；切换时序要互斥 |
| 旁白 None | 须关 Portrait，防残留父亲脸 |
| 店/村长旗 | 已走专用口 + Invoke(None)；改 OnGetAvatar 时勿打断其关 Portrait |
| 异步回调 | RefreshAvatar 异步晚到；须校验「仍是本句 Role」再亮槽，防串脸 |
| OPEN 旧决议 | 0804「Portrait 完全关闭」与本期冲突，已记修订 |

---

## 7. OPEN_QUESTIONS

已写入新节（见 `OPEN_QUESTIONS.md`）：

| ID | 问题 | 默认 |
|----|------|------|
| Q1 | 通用 null 回退（A）还是仅四人白名单（A′）？ | **A′** |
| Q2 | 修订 0804「旧 Portrait 完全关闭」？ | **是** → 改为混合 |
| Q3 | 其它未点名 Role 是否顺带？ | **本期不扩** |
| Q4 | 哥布林 | **不要求**；不验收 |

---

## 8. 可复制给施工员（5～10 条）

> 根因已拍板：**H1**。小孩=**夏尔**。走 **A′**。

1. **目标**：King / Lai / **Xiaer** / LinEn 说话时左槽再亮旧图集头像；Mask 角色不变；哥布林不要求。  
2. **方案 A′**：`useMaskAvatar` 保持 true；白名单四人在 Loader sprite≠null 时 `actorPortrait.SetActive(true)` 并赋 sprite。  
3. Mask 已支持角色（Yaer/Gusha/Amy/Aliy/Chief/店）：逻辑不变，Portrait **保持关**。  
4. GoblinElder/Younger：**不要**进白名单；空着通过。  
5. 切换到 Mask 角色或旁白：关 Portrait，防双影/残留。  
6. **禁止** `useMaskAvatar=false`；**禁止**未拍板新建四人 Mask Painting；**禁止**改图集/台本（除非证 H2）。  
7. 挂点选一处真源（优先扩 `OnGetAvatar` 分支或 Presenter 协作），写清互斥。  
8. 施工说明：`Assets/Doc/施工说明/0911/旧角色图集小头像_Mask混合回退_施工说明.md`。  
9. 验收：父/莱/夏尔/将军有图；雅儿 Mask 无双影；**不要**因哥布林空而判失败。  
10. 异步回调防串脸：亮槽前确认仍是本句 Role。

---

## 附录：关键锚点

| 主题 | 路径 |
|------|------|
| 关旧槽 / 开关 | `.../DialogueTMPUGUI.cs`（`useMaskAvatar`、`OnGetAvatar`） |
| Mask Presenter | `.../DialogueMaskAvatarPresenter.cs` |
| Loader | `.../DialogueAvatarLoader.cs` |
| 路径 | `.../DialogueAvatarPathHelper.cs` |
| 图集 | `Assets/GameRes/Atlas/Avatar/Avatar_{King,Lai,Xiaer,LinEn}.spriteatlas` |
| 壳 Prefab | `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` |
| 0803 债 | `执行文档/8月/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md` |
| 表情对照 | `执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md` |
