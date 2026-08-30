# 获得道具 Tips 横幅 —「艾琳之剑」溯源与老农复用 — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 复用接线拍板（**本阶段未改代码 / Prefab / 图集 / CSV**）  
**Unity**：2020.3.48f1  
**视觉样板**：全屏花边横幅「获得了艾琳之剑」（TipsPanel · Item）  
**样板触发**：龙宫卧室宝箱 `HomeScene2Box`  
**复用目标**：老农线（`Npc_Farmer` / `Village_老农打水任务*`）获得道具时弹出**同款**横幅  

---

## ① 结论一句话

**「获得了艾琳之剑」= 开箱剧情图里 `ExecuteFunction → HomeScene2Box.OnHomeScene2Box_GetSword`：先 `AddMainItem(AiLinSword)`，再 `TipsComponentGSM.OpenTipsForm("GetAiLinSword")`（默认 `ETipsType.Item`）。横幅字不是 TMP 拼出来的，是 TipsPanel 用图集 Sprite 名 `GetAiLinSword` 做 fill 动画 +「获得物品音效.mp3」。老农复用必须同样两步；`GetItemActionTask` 只入包不弹窗；现成 `AddTipsInfoActionTask` 会弹但默认 `Info`（无道具音效）。发什么道具 / Tip 图 / 发奖时机产品未定——未进图集则 `GetTipsSprite` 报错后 `OpenTipsForm` 静默不弹。**

---

## ② 原因（通俗）

入包和横幅是**两件事**：一个往背包塞东西，一个打开「获得了××」大横幅。  
横幅上的字**印在图片里**，不是代码填字——新道具要新图、新 Key，不能指望写个字符串就变出「获得了水桶」。  
项链线只塞了项链没弹这张花边，所以「有道具 ≠ 有横幅」。

---

## ③ 用户检查清单

### 剑样板（证伪链路）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 序章卧室未开箱档 → 点宝箱 | 播 `HomeScene2Box` 对白 |
| 2 | 到 GetSword 节点 | 背包有艾琳之剑 + **同款花边横幅** |
| 3 | 听音效 | Item 类型播「获得物品音效」 |

### 老农施工前须产品拍板（缺一不可）

| # | 问题 | 现状 |
|---|------|------|
| 1 | 发什么？道具 `EMainItemName` / 还是金币？ | ⏳ 开放；CSV「报酬」偏钱，未钉道具 ID |
| 2 | TipKey（如 `GetXxx`）与中/英/日 png？ | ⏳ 无图 → **不弹** |
| 3 | 何时发？接任务 / 交任务 / 某句对白后？ | ⏳ 开放 |
| 4 | 占位？暂用 `GetHpBall` 等旧图？ | 产品可接受才临时；视觉错字 |

### 施工后验收

| # | 操作 | 通过 |
|---|------|------|
| 1 | 触发老农发奖点 | 背包有对应物 + **TipsPanel 同款横幅** |
| 2 | TipKey 缺图 | Console **`未找到Tips图片：{key}`**，且不弹窗 |
| 3 | 连发两条 Tips | 队列依次 fill，不叠坏 |

---

## ④ 给程序

### A. 剑提示全链路（钉死）

```
玩家点宝箱 Interactive
  → HomeScene2Box.OpenBox
  → StoryComponentGSM.TriggerStory("HomeScene2Box")
  → 对话图 HomeScene2Box.prefab
       ExecuteFunction → OnHomeScene2Box_OpenBox   // 动画/存档已开
       …
       ExecuteFunction → OnHomeScene2Box_GetSword
            ├─ PlayerBagData.AddMainItem(EMainItemName.AiLinSword)
            └─ TipsComponentGSM.OpenTipsForm("GetAiLinSword")  // 默认 ETipsType.Item
                 ├─ TipsFormProxy.GetTipsSprite("GetAiLinSword")
                 │     ← tipsInfo / tipsInfo_en / tipsInfo_jp
                 │     ← ArtRes/.../TipInfoAtlas*/GetAiLinSword.png
                 └─ OpenUIForm("TipsPanel") → TipsFormLogic
                       Image.fillAmount 扫过 + soundSfxCpn「获得物品音效.mp3」
```

| 层 | 路径 | 作用 |
|----|------|------|
| 实体 | `HomeScene2Box.cs` | 开箱 TriggerStory；GetSword 入包+Tips |
| 剧情 | `GameRes/Prefabs/Dialogue/HomeScene2Box.prefab` | ExecuteFunction 调上述两方法 |
| 入口 | `TipsComponentGSM.OpenTipsForm` | 开 TipsPanel / 已开则 `AddTipsInfo` 入队 |
| UI | `TipsFormLogic` + `TipsPanel.prefab` | 花边底 + `imgChar` fill |
| 资源 | `ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas{,_en,_jp}/GetAiLinSword.png` | **文案在图里** |
| 图集 | `GameRes/Atlas/TipsPanel/tipsInfo*.spriteatlas` | Sprite 名 = TipKey |
| 音效 | `SoundResName: 获得物品音效.mp3`；仅 `ETipsType.Item` 播放 | Info/Boss 不走该音 |

### B. 「字从哪来」与无图行为

| 问题 | 真相 |
|------|------|
| 动态 TMP「获得了{0}」？ | **否**；整图 Sprite |
| Key 规则 | TipKey == png 文件名（无扩展名）== Atlas 内 Sprite 名；惯例 `Get` + 道具语义（`GetAiLinSword`） |
| 语言 | 按 `GameManager.language` 选 cn/en/jp 图集；未知语言走 en |
| `GetTipsSprite==null` | 先 `Debug.LogError("未找到Tips图片："+key)`（或 atlas 未就绪 Error）；`OpenTipsForm` **直接 return，不弹窗**（像坏了） |
| 动态改字 | 现网 **不支持**；新道具要新图（或产品接受错字占位） |

### C. 对照：谁弹 Tips、谁只入包

| 样板 | 入包 | Tips | TipKey / 类型 |
|------|------|------|----------------|
| 艾琳之剑 `GetSword` | ✅ | ✅ | `GetAiLinSword` · **Item** |
| 夏尔力量 / 出门球 | ✅ | ✅ | `GetXiaerPower` / `GetHpBall` / `GetMpBall` · Item |
| 西拉普路血蓝箱 | ✅ | ✅ | 双 `OpenTipsForm` 入队 |
| 地图 `GetMap` | （业务侧） | ✅ | Item |
| 项链 `VerdantCorridorGetNecklace` | ✅ `GetItemActionTask` | ❌ **无** Tips | — |
| `AddTipsInfoActionTask` | ❌ | ✅ | 任意 TipKey，但强制 **`ETipsType.Info`**（**无**获得物品音效） |
| SystemTips / 成就 | — | 另一套 | **不是**这张花边横幅 |

**易混钉死**

| 易混 | 真相 |
|------|------|
| 只挂 `GetItemActionTask` | 有道具、**无**截图横幅 |
| 只挂 `AddTipsInfoActionTask` | 有横幅图但 **Info**；与剑音效不一致 |
| 成就 / SystemTipsPanel | 另一 UI，勿当获得道具 Tips |

### D. 老农复用方案拍板

现网 `EMainItemName`：**无**水桶/老农专用项（仅有剑、球、地图、项链、商店六道具等）。  
CSV：`_接受` 提「报酬 / 打四桶水」；`_完成结算`「这是你的报酬」——**更像钱**，未钉道具名。

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A · 对话图双 Action** | 在对应 Prefab（建议 `_完成结算` 或产品指定句后）插：`GetItemActionTask`（或 AddMainItem 等价）+ **`OpenTipsForm(TipKey, Item)`** | ✅ **对白驱动首选**（少新建实体） |
| **B · C# 仿 HomeScene2Box** | Quest 接取/交付回调里双调 | ✅ 任务 FSM 成熟后更干净 |
| C · 只 GetItemActionTask | 无横幅 | ❌ |
| D · 新横幅 UI | — | ❌ |

**对话内 Tips 类型缺口（施工须选一）**

| 子方案 | 说明 | 倾向 |
|--------|------|------|
| **A1** | 新建 `OpenTipsFormActionTask(TipKey, ETipsType=Item)` + 已有 `GetItemActionTask` | ✅ 推荐（不破坏 Info 用法） |
| A2 | 扩展 `AddTipsInfoActionTask` 增加 `TipsType` 参数，默认 Info | ✅ 也可 |
| A3 | ExecuteFunction 挂到新/临时 Entity 方法仿 GetSword | ⚠️ 老农无专用 Logic 时啰嗦 |
| A4 | 仅 `AddTipsInfo`（Info） | ❌ 音效与剑不一致 |

**挂点建议（产品未定前默认）**

| 时机假说 | Prefab | 说明 |
|----------|--------|------|
| 交任务发报酬 | `Village_老农打水任务_完成结算`（待建 Prefab）句末 | 与 CSV「这是你的报酬」对齐 |
| 接任务发工具（桶） | `_接受` 末句后 | 须先有「水桶」道具定义 |
| 基础闲聊 | `Village_老农打水任务` | ❌ 不宜（本期仅唠嗑） |

### E. 资源门槛（P0，未齐勿宣「效果坏了」）

1. `EMainItemName` + MainItemDatabase 行（若发道具）  
2. TipKey（建议 `Get{ItemSemantic}`）  
3. 三语 png：`TipInfoAtlas` / `_en` / `_jp`，文件名=Key  
4. 打进 `tipsInfo*.spriteatlas` 并确保运行时能 `GetSprite`  
5. 接线：`AddMainItem` + `OpenTipsForm(key, Item)`  

金币若走 Quest 发奖：另查金币 API + 是否有「获得金币」Tip 图（现网 TipInfoAtlas 列表**无**明显 GetGold）。

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 0 | 产品定：道具/金币、Tip 文案图、发奖时机 | **P0 门禁** |
| 1 | 美术出三语 Tip 图并入图集（或书面接受占位旧 Key） | **P0** |
| 2 | 枚举/数据库补道具（若需要） | **P0** |
| 3 | A1：`OpenTipsFormActionTask`（Item 默认）或扩展 AddTipsInfo | **P0** |
| 4 | 目标对话 Prefab：GetItem + OpenTips 节点 | **P0** |
| 5 | 任务 FSM 成熟后可选迁到方案 B | P1 |
| 6 | 动态 TMP「获得了{0}」 | ❌ 另立项 |

**预期 diff（道具方案示例）**

- `ArtRes/.../TipInfoAtlas*/GetXxx.png` ×3 + atlas  
- （可选）`EMainItemName` + Database  
- （推荐）`OpenTipsFormActionTask.cs` 或改 `AddTipsInfoActionTask`  
- `Village_老农打水任务_完成结算.prefab`（或指定 Prefab）图节点  

**不改**：TipsPanel 视觉；SystemTips；剑样板逻辑（除非回归）。

### G. 验收清单

同 §③ 施工后表 + 剑样板回归。

### H. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 老农发 **道具还是金币**？道具则 `EMainItemName` 叫什么？ | 结算文案偏钱；待策划 | ⏳ |
| Q2 | TipKey / 三语图谁出？可否暂用旧图占位？ | 新图 P0；占位须产品书面接受 | ⏳ |
| Q3 | 发奖时机：接受 / 完成结算 / 其它？ | **默认完成结算句后** | ⏳ 待确认 |
| Q4 | 对话内 Tips 用 A1 新 Task 还是扩 AddTipsInfo？ | **A1** | ✅ 本报告 |
| Q5 | 是否做动态拼字？ | **本期否** | ✅ |

（已追加 `OPEN_QUESTIONS.md`。）
