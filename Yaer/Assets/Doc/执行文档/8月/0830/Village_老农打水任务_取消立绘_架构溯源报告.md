# Village_老农打水任务 — 对白取消立绘 — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读确认缺口 + Prefab 最小改清单（**本阶段未改 Prefab / 代码 / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**目标**：`Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab`  
**产品改口（钉死）**：**本对白不需要立绘**——只要对话框（名字+字幕）；不要大立绘、不要为立绘淡入/绑 BB；老人立绘 P1 **取消**  
**无立绘对照**：`Village_TreeHouseLock.prefab`  
**反例（勿抄）**：`Village_ShopHead` / `Village_ShopStart`（Fighting→立绘 CGAlpha→UIAlpha）

关联：`施工说明/0830/Village_KenMuNi1_老农基础对话交互_施工说明.md`（「未做老人立绘」→ 改口「产品不要立绘」）

---

## ① 结论一句话

**图序已是「仅 UIAlpha → Statement」，没有立绘淡入节点；但 Yaer 下仍嵌着 `GoOutStoryYaerPainting` 实例且 BB 绑了同名 CanvasGroup，且本 Prefab 未像 ShopStart 把 `m_Alpha` 覆写为 0——源 Prefab 默认 alpha=1，实机很可能一开对白就出雅大立绘。拍板方案 A：删该嵌套实例 + 清空 BB `GoOutStoryYaerPainting`；保留 UIAlpha、双 Actor（雅尔/老人）、全部 Statement。`PrepareMaskAvatarOnFadeIn` 已关，保持关。Mask 小头像走 Panel 默认（与 TreeHouseLock 同路径），本期不改全局 Mask；施工说明改口「产品明确不要立绘」。**

---

## ② 原因（通俗）

田边唠嗑只要字幕条，不需要剧场大立牌。  
施工基础对话时可能从带立绘的壳拷过 Yaer 子树，**立牌还在**，只是图里没写「淡入」——但立牌源资源默认全透明度为 1，等于一上场就亮着。  
产品现明确说不要立绘，就把立牌拆掉，字幕条留下。

---

## ③ 用户检查清单（施工后验收）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 点 `Npc_Farmer` 播本对白 | **对话框出现**，文案正常 |
| 2 | 全程 | **无**雅儿/老人 **大立绘** |
| 3 | 名字栏 | 仍显示 雅尔 / 老人 |
| 4 | 结束 | 回村正常；可再谈 |
| 5 | Console | 无 Missing Painting / BB 空引用刷屏 |
| 6 | （对照）`TreeHouseLock` | Mask 小头像表现与本线 **一致即可**（本期不单独关全局 Mask） |

无关：接任务 Choice；改 `Npc_Farmer`；改 Import 器全局。

---

## ④ 给程序

### A. 现网 Prefab 盘点（已证伪）

| 层 | 磁盘真源 | 与「无立绘」 |
|----|----------|-------------|
| 根名 | `Village_老农打水任务` | 保持 |
| 图序 | **仅** `NormalDialogueUIAlpha`（id0）→ Statement id1～15 | ✅ 无 Fighting；**无** `CanvasGroupAlpha` |
| UIAlpha · PrepareMask | `PrepareMaskAvatarOnFadeIn` 空/`false`；Role/Face 空 | ✅ 保持关；**勿打开** |
| Yaer | DialogueActor「雅尔」；子物体嵌 PrefabInstance → **`GoOutStoryYaerPainting`**（guid `4c0e9909…` = `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab`） | ❌ **应拆除** |
| BB | `_serializedBlackboard` / `_serializedVariables` 含 **`GoOutStoryYaerPainting` → CanvasGroup**（ref `8129792323373277873`） | ❌ **应清空** |
| override `m_Alpha` | **无**（对比 ShopStart 明确 `m_Alpha=0`） | 源默认 **alpha=1** → 大立绘易直接可见 |
| Elder | Actor「老人」；`_portrait: {fileID: 0}`；**无** Painting 子物体 | ✅ 只留名字 Actor |
| Fighting / 立绘 Alpha 节点 | 无 | ✅ 勿新增 |

### B. 对照 TreeHouseLock（目标态）

| 项 | TreeHouseLock | 老农打水任务（现网） | 施工后 |
|----|---------------|---------------------|--------|
| UIAlpha → 句 | ✅ | ✅ | 保持 |
| Yaer Actor | ✅ 无子物体 | ✅ 有 GoOut 嵌套 | **对齐：无嵌套** |
| BB 立绘变量 | `_variables:{}` | 有 GoOut… | **清空** |
| 第二 Actor | 无 | Elder「老人」 | **保留**（要名字栏） |
| PrepareMask | 关 | 关 | 关 |

**勿**整份覆盖 TreeHouseLock（会冲台本与双 Actor）。

### C. 方案拍板

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A · Prefab 去立绘壳** | 删 Yaer/`GoOutStoryYaerPainting` 实例；清 BB 变量与 objectReferences；不增 CGAlpha；保留 UIAlpha+Statement+雅尔/老人 | ✅ **推荐** |
| B · 立绘留着 alpha 永 0 | 仍占 Prefab、易被误开淡入 | ❌ |
| C · 改 CSV Import「永不嵌立绘」 | 影响其它对话 | ❌ 本期只改本 Prefab |
| D · 拆掉全部 Actor | 名字栏空/错 | ❌ |

**Mask 小头像产品口径（写死）**

- 「立绘」主指 **GoOut 大立绘** → **必须去掉**。  
- **不**勾 `PrepareMaskAvatarOnFadeIn`（现网已关）。  
- Statement → Panel `useMaskAvatar` 若仍出 **小头像**，与 TreeHouseLock **同路径**；本期 **不改** 全局 Mask / Panel。若验收坚持「纯字幕条也无 Mask」→ 另开 P1，不绑在本次 Prefab diff。

### D. 防回潮（再 Import）

| 项 | 注意 |
|----|------|
| Import 窗口 | 可勾 **「对话框 UI 淡入」**；**勿**勾 **「立绘 CanvasGroup 淡入」**；勿指定立绘参考 Prefab |
| 合并进 Prefab | 勿从 ShopStart/ShopHead 拷 Yaer 立绘子树 |
| 若工具/人工又嵌立绘 | 重导后 **再跑一遍本报告方案 A** |
| Import「本 CSV 跳过立绘」开关 | **P2 可选**，本期不做 |
| 兄弟 Generated | 磁盘有 `…_接受/_拒绝/…` 等 **.asset**，**无**对应 Dialogue Prefab；本期只动主 Prefab |

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 删除 PrefabInstance `GoOutStoryYaerPainting`（及 stripped RectTransform/CanvasGroup） | **P0** |
| 2 | Yaer `m_Children` 置空；BB `_serializedBlackboard`→`{}`；清 `_objectReferences` / `_serializedVariables` | **P0** |
| 3 | 确认图内无 CGAlpha；UIAlpha 保留且 PrepareMask 关 | **P0** |
| 4 | Play：有框无大立绘；名字正常 | **P0** |
| 5 | 施工说明 / OPEN_QUESTIONS：老人立绘 P1 → **产品不要立绘（取消）** | P1 |
| 6 | Import 工具「跳过立绘」 | P2 |

**预期 diff 文件**

- 必改：`Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab`  
- 文档：`施工说明/0830/…老农基础对话…` 改口；`OPEN_QUESTIONS` Q4 关单  
- **不改**：CSV、场景、`Npc_Farmer`、Import 器、Generated（除非施工员选择同步注释）

### F. Prefab 结构目标（示意）

```
Village_老农打水任务
├─ Yaer          ← DialogueActor「雅尔」；无子物体
├─ Elder         ← DialogueActor「老人」；无立绘
├─ DialogueTreeController
│    图: UIAlpha(PrepareMask=false) → Statement×15
└─ Blackboard    ← _variables 空
```

### G. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | Mask 小头像是否也必须关？ | **本期否**（对照 TreeHouseLock）；坚持无 Mask → P1 | ✅ 本报告 |
| Q2 | 下期 `_接受/_拒绝` Prefab 是否一并无立绘？ | **是**（建 Prefab 时抄本线无立绘壳） | ⏳ 下期 |
| Q3 | 老人大立绘是否还会做？ | **产品取消** | ✅ 关闭 |

（已追加 `OPEN_QUESTIONS.md`。）
