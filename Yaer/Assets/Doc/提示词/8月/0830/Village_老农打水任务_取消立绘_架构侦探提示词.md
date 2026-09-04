# Cursor Agent Prompt · `Village_老农打水任务`：对白不需要立绘（改 Prefab）

> **角色**：先【架构侦探】只读确认缺口，拍板后【施工员】改 Prefab（可同会话续跑）  
> **日期**：2026-08-30  
> **目标 Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab`  
> **产品改口（钉死）**：**老农打水任务对白不需要立绘**——只要对话框（名字+字幕）能正常播；不要大立绘、不要为立绘做淡入/绑 BB  
> **对照（无立绘短对白）**：`Village_TreeHouseLock.prefab`（仅 Yaer Actor + 句，无 GoOut 立牌）  
> **关联**：0830 老农基础对话已通；施工说明曾写「老人立绘 P1」——**产品现明确不做**  
> **本阶段侦探**：只读；禁止改代码 / 场景 / CSV（施工阶段只动对白 Prefab / 必要时 Generated，**默认不改** Import 器全局行为）  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_老农打水任务_取消立绘_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> `Village_老农打水任务` **不要立绘**。  
> 玩家跟老农说话：出对话框、轮流说话即可；屏幕上**不要**再出雅儿/老人大立绘（或为立绘准备的空壳动画）。

| 要 | 不要 |
|----|------|
| ✅ 对话框淡入（`NormalDialogueUIAlpha` 可保留） | ❌ `GoOutStoryYaerPainting` 大立绘（嵌物体 / BB / CanvasGroupAlpha 淡入） |
| ✅ Actor 雅尔 / 老人（说话人名字） | ❌ 老人 Mask / Painting 资源补齐 |
| ✅ 文案与 CSV 一致可播 | ❌ 先立绘后框的 ShopHead 时序 |
| ✅ 小头像 Mask（若 Panel 默认会出）→ **侦探裁定**：产品说「立绘」通常=大立绘；Mask 小头像 **倾向也关/不 Prepare**，与「纯字幕框」一致时写入拍板 | ❌ 为无立绘去拆交互 / 改 Story 名 |

### 现网 Prefab 预扫（须证伪）

| 层 | 预扫 | 与「无立绘」关系 |
|----|------|------------------|
| 根名 | `Village_老农打水任务` | 保持 |
| 图序 | 仅 `NormalDialogueUIAlpha` → 各 Statement（预扫 **无** Fighting / **无** 立绘 Alpha 节点） | UI 框可留 |
| Yaer 子树 | 有子物体；BB 绑 **`GoOutStoryYaerPainting`**；override `m_Alpha` | **应拆除或禁用** |
| Elder | Actor「老人」 | 保留名字 Actor，**不挂**立绘 |
| PrepareMaskAvatarOnFadeIn | UIAlpha 上预扫为空/关 | 确认勿打开；若运行时仍出 Mask，查 Panel 默认 |

生活类比：这句对白是田边唠嗑字幕条，不是剧场开幕竖立牌——把立牌拆掉，字幕条留下。

### 修改方案倾向（侦探拍板）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · Prefab 去立绘壳** | 删 Yaer 下 `GoOutStoryYaerPainting` 实例；清 BB 变量或解绑；不增加 CanvasGroupAlpha；保留 UIAlpha+Statement | **✅ 推荐** |
| B · 立绘留着但 alpha 永 0 | 仍占 Prefab、易被误开淡入 | ❌ |
| C · 改 CSV Import 全局「永不嵌立绘」 | 影响其它对话 | ❌ 本期只改本 Prefab |
| D · 拆掉全部 Actor 只留旁白 | 名字栏会空/错 | ❌ |

**再 Import CSV 防回潮**：施工说明写清——重导时 **勿**勾「嵌雅立绘 / 参考 ShopStart」类选项；若工具默认嵌立绘，报告写「重导后须再跑去立绘」或开放「Import 本 CSV 跳过立绘」P1。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 确认并去掉本 Prefab 大立绘相关 | ❌ 接任务 Choice |
| ✅ 验收：播对白无大立绘、有对话框 | ❌ 改 Npc_Farmer 交互 |
| ✅ 文档改口：老人立绘 P1 **取消** | ❌ 改全局 Mask 系统 |

### 严禁（侦探阶段）

- 改代码 / 场景 / CSV / 其它 Prefab  
- 为去立绘拆掉对话框 UIAlpha 导致「无框」回潮  
- 把 TreeHouseLock 整份覆盖把台本冲掉  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_老农打水任务.prefab` | 改造目标 |
| `Village_TreeHouseLock.prefab` | 无立绘对照 |
| `Village_ShopHead.prefab` | 反例（有立绘时序，勿抄） |
| `0830/...老农基础对话交互_施工说明.md` | 改口「未做老人立绘」→「产品不要立绘」 |
| `NormalDialogueUIAlphaAnimationTaskAction` | PrepareMask 开关 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/施工说明/0830/Village_KenMuNi1_老农基础对话交互_施工说明.md
@Assets/Doc/执行文档/0830/Village_KenMuNi1_老农基础对话交互_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_TreeHouseLock.prefab
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写「老农打水任务取消立绘」溯源报告。

---

## 背景（策划白话）

1. `Village_老农打水任务` 对白 **不需要立绘**。  
2. Prefab 里可能还嵌着雅儿 GoOut 立绘 / BB——要摸清删什么、留什么（对话框必须还在）。  
3. 本阶段只出最小改 Prefab 清单与验收；拍板后再施工。

---

## 侦探任务清单

### A. 盘点本 Prefab 立绘相关
表：Yaer/Elder 子物体、BB `GoOutStoryYaerPainting`、图内 Alpha/PrepareMask、运行时会不会出大立绘/Mask。

### B. 对照 TreeHouseLock
无立绘短对白最小结构；本 Prefab 多了哪些立绘件。

### C. 方案拍板
推荐 A：去立绘实例+解绑 BB；保留 UIAlpha+Statement+双 Actor 名。  
Mask 小头像：关/开写死一句产品口径。

### D. 防回潮
再 Import 本 CSV 时如何避免又嵌立绘。

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 移除/禁用 GoOut 雅大立绘嵌套与 BB 绑定 | **P0** |
| 2 | 确认无立绘淡入节点；UIAlpha 保留且 PrepareMask 关 | **P0** |
| 3 | Play：有框无大立绘 | **P0** |
| 4 | 施工说明改口「产品不要立绘」 | P1 |
| 5 | Import 工具加「跳过立绘」 | P2（可选） |

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 点 Npc_Farmer 播本对白 | **对话框出现**，文案正常 |
| 2 | 全程 | **无**雅儿/老人大立绘出现（按拍板亦无 Mask） |
| 3 | 名字栏 | 仍显示雅尔 / 老人（或现网约定） |
| 4 | 结束 | 回村正常；可再谈 |
| 5 | Console | 无 Missing Painting / BB 空引用刷屏 |

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_老农打水任务_取消立绘_架构溯源报告.md`

MASTER 四段式：  
① 结论（去哪些立绘件、留对话框）  
② 原因（产品不要立绘；现网多嵌了什么）  
③ 用户检查清单  
④ 给程序：Prefab diff 清单 + 防回潮 + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_老农打水任务_取消立绘_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_TreeHouseLock.prefab

你现在是【施工员】。按报告修改 Village_老农打水任务：对白不需要立绘。

必须遵守：
- 去掉大立绘嵌套与 BB/淡入；保留对话框 UIAlpha 与雅尔/老人说话人名；
- 不要拆交互、不要改 Story 名、不要做接任务；
- 不要用 ShopHead 那套先立绘后框；
- Prefab/注释写清「本对白无立绘」；重要取舍写原因。

提交说明：删了哪些物体/BB、Mask 是否关闭、如何验收、再 Import 注意点。
```
