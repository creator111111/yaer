# Cursor Agent Prompt · 父亲/莱/夏尔/将军 小头像全空（旧图集 Portrait 被 Mask 模式关掉）

> **角色**：【架构侦探】只读溯源；**禁止改代码 / Prefab / 图集 / Git 提交**  
> **日期**：2026-09-11  
> **现象（用户实测）**：  
> 1. **父亲、莱、夏尔（小孩）、将军** 说话时，对话框左侧小头像 **全部为空**  
> 2. 用户说明：这些角色用的是 **旧系统截图/图集小头像**（`DialogueAvatarLoader` + `actorPortrait`），不是 Mask 嵌套 Painting  
> 3. 「现在全部为空」——像是 Mask 接线后旧槽被永久关掉，又未给这些角色做 Mask 立绘  
> **产品期望（钉死）**：上述四人说话时左侧再次显示其 **旧图集小头像**（表情跟台本 FaceType）；雅儿/古莎/艾米等 **已上 Mask 的角色保持 Mask 真源**，不要整面板回退  
> **明确不在范围**：**哥布林**（`GoblinElder` / `GoblinYounger`）**不要求**小头像；本期不验收、不为其开旧槽专项  
> **不是**：给四人立刻做完整 Mask Painting Prefab（除非侦探证明必须）；不是改大立绘；不是改台本文案；不是关掉全员 `useMaskAvatar` 把雅儿也打回旧 Portrait；不是给哥布林补头像  
> **并行**：与同日 NewGame 大立绘 / 开局 Mask 闪错脸案解耦——本案是 **未进 Mask 白名单的指定旧角色 × 旧 Portrait 被关**  
> **报告落盘**：`Assets/Doc/执行文档/0911/旧角色图集小头像全空_Mask模式回退_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 雅儿那些新 Mask 头像先不管。  
> 父亲、莱、夏尔、将军以前对话框左边有图（旧图集/截图头像），现在说话时空的。  
> 「小孩」指的是 **夏尔**，不是哥布林。  
> 哥布林不需要小头像。  
> 要这四人恢复旧头像；已经上 Mask 的角色别被这次修坏。

### 角色名映射（产品已钉 · 侦探仍须在 Actor 上核对）

| 用户口称 | `DialogueRoleName`（已钉） | 旧图集（0601 文档） |
|----------|----------------------------|---------------------|
| **父亲** | **`King`**（序章国王/父） | `Avatar_King.spriteatlas` |
| **莱** | **`Lai`** | `Avatar_Lai.spriteatlas` |
| **小孩** | **`Xiaer`（夏尔）** — **不是**哥布林 | `Avatar_Xiaer.spriteatlas` |
| **将军** | **`LinEn`**（琳恩） | `Avatar_LinEn.spriteatlas` |

| 排除 | Role | 本期产品 |
|------|------|----------|
| 哥布林兄/弟 | `GoblinElder` / `GoblinYounger` | **不要求小头像**；空着可接受；勿写入验收必过 |

侦探须在复现对白 Prefab 上读 **DialogueActorEx.RoleName** 核对上表；若夏尔句绑成其它 Role，报告里标歧义，但施工范围仍以产品四人 + 排除哥布林为准。

### 现网机制（助手预扫 · 高度可疑）

```
句开始
  → actor.RefreshAvatar(FaceType) → DialogueAvatarLoader.GetAvatar（图集仍可能加载成功）
  → OnGetNewStatement(role, face) → DialogueMaskAvatarPresenter.Apply
       → HideAllPaintings()
       → ResolvePainting(role)  // Yaer/Gusha/Amy/Aliy/Chief/店 才有
       → King/Lai/Xiaer/LinEn… → painting==null → return   // Mask 空
  → OnGetAvatar(sprite)
       → 若 useMaskAvatar==true → actorPortrait.SetActive(false)  // 旧槽永不亮
       → （sprite 可能写进 Image 但不显示）
```

| 环节 | 预扫结果 | 若失败的体感 |
|------|----------|--------------|
| **面板开关** | `NormalDialogueNewPanel`：`useMaskAvatar: 1` | 全对话壳走 Mask 真源 |
| **旧槽行为** | `DialogueTMPUGUI.OnGetAvatar`：Mask 模式下 **强制关** `actorPortrait` | 图集有图也不显示 |
| **Presenter 注释** | `Apply`：**「未支持角色（King/Lai…）保持 Mask 空」** | 与用户现象同构；像是已知缺口被当成终态 |
| **ResolvePainting** | `switch` 仅 Yaer / Gusha / Amy / Aliy（+ Chief/店专用入口） | King/Lai/Xiaer/LinEn → null |
| **0803 文档** | Mask MVP：其它角色本期无实例 → Mask 空；旧 Portrait 决议保持关 | **产品债**：未支持角色既无 Mask 又无旧槽 |
| **历史列表** | 注释称 Loader 仍跑供历史；字幕条左槽仍空 | 「历史有、对话框无」可作旁证 |

### 假说表（须并列证伪）

| ID | 假说 | 证伪手段 |
|----|------|----------|
| **H1（首选）** | **`useMaskAvatar=true` + Presenter 不支持 King/Lai/Xiaer/LinEn + OnGetAvatar 关旧 Portrait** → 双路皆空 | 读开关、Apply/OnGetAvatar、ResolvePainting；Play 时 Portrait Active 与 Mask 子树 |
| **H2** | Loader 路径/图集丢了，sprite 本就 null（与 Mask 无关） | 断点/日志 `GetAvatar` 回调 sprite；图集是否仍在；历史页同句有无头像 |
| **H3** | Actor 未绑 `DialogueActorEx` / RoleName=None，走旁白清空 | 复现句 Hierarchy Actor 组件 |
| **H4** | 仅某几条 Prefab 坏，不是全局壳问题 | 多场景多对白对比；DialogDebug 换角色 |
| **H5** | 用户说的「截图小头像」其实是 0727 未接线的 Prefab 截图方案，与图集 Portrait 不是一路 | 对照 0727/0803：现网旧路是 **SpriteAtlas+Portrait**，不是 RT 截图 |
| **H6** | PrepareMask / HideAll 残留把不该关的关了 | 时序日志；非预亮句是否仍空 |

### 方案倾向（仅倾向，侦探可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · 混合回退（推荐方向）** | Mask 已支持角色仍走 Mask；对 **King/Lai/Xiaer/LinEn**（及可选：其它非哥布林、有图集且无 Mask 的 Role）在 Loader 有图时 **亮旧 `actorPortrait`** | **首选** |
| **A′ · 白名单四人** | 仅对产品点名的 King/Lai/Xiaer/LinEn 开旧槽；其它 `ResolvePainting==null`（含哥布林）保持空 | 若怕误亮哥布林，可走 A′；**产品已说哥布林不要求，空着即可** |
| **B · 为四人补 Mask Painting** | 新建/嵌套 Mask 立绘并扩 ResolvePainting | 工作量大；用户已说用旧系统，**非本期首选** |
| **C · 全局 `useMaskAvatar=false`** | 整壳回退旧 Portrait | **禁止作终局**（毁掉雅儿等 Mask） |
| **D · 只改个别对话 Prefab** | 换壳/关 Mask | 易漏；壳是全局 `NormalDialogueNewPanel` |

**产品钉死**：要 **A/A′ 混合**，不要 C；B 仅当产品改口要 Mask 化再开；**不要为哥布林补头像**。

### 侦探禁止事项

- 禁止改代码 / Prefab「先修再查」。  
- 禁止为修四人而关掉全局面板 `useMaskAvatar`。  
- 禁止借机重做雅儿 Mask / 大立绘 / 开局闪脸案。  
- 禁止把「小孩」写成哥布林或把哥布林列入验收必过。  
- 禁止把「0803 写 Mask 空」当成产品终态而不写修复建议。

---

## 【架构侦探】执行段（复制给 Agent）

你是【架构侦探】。只读溯源，**禁止改代码**。

### 目标

查清：**父亲（King）/ 莱（Lai）/ 夏尔（Xiaer）/ 将军（LinEn）** 小头像为何全空；钉死 RoleName 与旧图集链路是否仍加载成功、是否被 `useMaskAvatar` 挡住显示；给出 **混合显示** 的最小修复建议（Mask 角色不变，上述旧角色回退旧 Portrait；**哥布林不要求**）。

### 必查清单

1. **角色钉名**  
   - 找用户能复现的对白（如 `NewGameStory` 父/王句、莱句、**夏尔**句、将军/琳恩句）。  
   - 表：中文显示名 → `DialogueRoleName` → Actor 物体 → 图集路径。  
   - **确认「小孩」句 Role=`Xiaer`**；若误绑 Goblin，记入报告，但仍按产品修夏尔。

2. **双路显示逻辑**  
   - 精读：`DialogueTMPUGUI`（`useMaskAvatar`、`OnGetAvatar`、`OnGetNewStatement`）。  
   - 精读：`DialogueMaskAvatarPresenter.Apply` / `ResolvePainting` / `HideAllPaintings`。  
   - 画对照表：雅儿 Mask 句 vs King/Lai/Xiaer/LinEn 句（谁亮 Mask、谁亮 Portrait、谁双空）。

3. **Loader 是否还活着**  
   - 确认四人图集资源仍在；回调 sprite 非 null 时 Portrait 是否仍 Active=false。  
   - 历史对话框同角色是否有头像（旁证 Loader 正常）。

4. **文档债**  
   - 引用 0803「未支持角色 Mask 空」+ OPEN「旧 Portrait 保持关」——说明与本期产品冲突点。  
   - 澄清「截图小头像」在现网对应的是 **图集 Portrait** 还是未落地的 RT 截图。

5. **范围拍板建议**  
   - 施工默认：**King / Lai / Xiaer / LinEn** 必恢复。  
   - **GoblinElder / GoblinYounger：不要求**；通用回退若会误亮哥布林，应写清用白名单（A′）或接受哥布林仍空。  
   - 其它未点名 Role（若有）是否顺带回退：记 OPEN，默认不扩。

### 报告结构

1. 结论一句话（根因 + 是否 H1）  
2. 证据链（开关、方法、注释、Role 表；小孩=夏尔）  
3. 假说表 H1～H6  
4. 用户自查：说父亲/夏尔句时 Hierarchy 里 `actorPortrait` Active、sprite 有无、Mask 是否全关  
5. 建议施工（优先 A/A′；写清哥布林排除）  
6. 剩余风险（双影、历史、旁白 None、店/村长旗）  
7. 若需产品拍板：记 `OPEN_QUESTIONS.md`（通用回退 vs 仅四人白名单）  
8. 文末「可复制给施工员」5～10 条 bullet  

### 输出要求

- 中文；大白话；结论 → 原因 → 检查清单 →（可选）程序补充。  
- 报告：`Assets/Doc/执行文档/0911/旧角色图集小头像全空_Mask模式回退_架构溯源报告.md`  
- 可引用：  
  - `Assets/Doc/执行文档/8月/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`  
  - `Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`  
  - `Assets/Doc/执行文档/6月/0601/对话立绘表情与图片名称对照_执行说明.md`  
  - `Assets/Doc/OPEN_QUESTIONS.md`（0727/0803/0804 Mask 相关决议）

---

## 【施工员】Prompt（根因拍板后再用 · 预稿）

> 仅当侦探确认 **H1（Mask 真源下未支持角色双空）** 或等价后使用。

1. **优先方案 A / A′（混合）**  
   - Mask 已支持角色（Yaer/Gusha/Amy/Aliy/Chief/店等）：逻辑不变，`actorPortrait` 保持关。  
   - **必亮旧槽**：`King` / `Lai` / `Xiaer` / `LinEn`，且 Loader 返回了 sprite。  
   - **哥布林**：不要求；若实现是「凡 null 都亮旧槽」，须确认产品是否接受哥布林也被亮；**默认推荐白名单四人（A′）**，避免无需求角色被带上。  
   - 切换到 Mask 角色或旁白时：关旧 Portrait，避免双影/残留。  
2. 挂点建议（侦探可改）：`OnGetAvatar` 在 `useMaskAvatar` 下按「本句是否 Mask 驱动 / 是否旧角色白名单」分支——**选一处真源，写清互斥**。  
3. **禁止** `useMaskAvatar=false` 全局回退；**禁止**未拍板就为四人新建全套 Mask Painting；**禁止**把哥布林列入必做。  
4. **禁止**改图集资源名/台本，除非 H2 成立。  
5. 施工说明：`Assets/Doc/施工说明/0911/旧角色图集小头像_Mask混合回退_施工说明.md`  
6. 验收：  
   - 父亲（King）/ 莱 / **夏尔** / 将军（LinEn）句：左槽有旧图集头像，表情跟 FaceType  
   - **不验收**哥布林小头像（空着通过）  
   - 雅儿/古莎等 Mask 句：仍走 Mask，无旧 Portrait 双影  
   - 历史列表不回归；店/村长专用口不黑块  

---

## 【验收员】Prompt（施工后 · 预稿）

1. 日志建议：`[AvatarHybrid] role=… mask=… portraitActive=… sprite=…`  
2. 至少覆盖：父/王句、莱句、**夏尔句**、将军句 + 一句雅儿 Mask 对照。  
3. **不要**把哥布林句列为失败项。  
4. 输出验证结果 + 白名单外 Role 行为说明。

---

## 给用户的一句话

> 要修的是父亲、莱、夏尔、将军这四个旧图集头像；小孩=夏尔，哥布林不用管。根因仍是 Mask 开了之后旧槽被关、这四人又没 Mask 立绘。
