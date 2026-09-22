# Cursor Agent Prompt · 针线包：非消耗品、只应有一个（角标「3」不对）

> **角色**：先【架构侦探】只读；报告拍板后再【施工员】  
> **日期**：2026-09-22  
> **产品原话（钉死）**：**针线包不是消耗品，就一个**  
> **现象（用户截图）**：背包格里针线包右下角显示 **`3`**（像可堆叠消耗品）  
> **真源入口**：`EMainItemName.SewingKit` / `MainItemDatabase` / `PlayerBagData` / 续聊 `GetItem`  
> **不是**：改 Tips「获得了针线包」图；改续聊文案；改商店六药水堆叠上限；做新背包 UI  

把下面「侦探」整段交给 Agent。没分清「类型错了 / 重复发放叠成 3 / UI 一律显示数量」之前，不要施工。

---

## 提示词助手预梳理（须证伪）

### 产品口径

| 项 | 期望 |
|----|------|
| 性质 | **非消耗品**（不能当 CostItem 使用/快捷栏消耗） |
| 数量 | **全档唯一 1 个**；不应出现 2、3… |
| 展示 | 角标不应像消耗品那样显示 **`3`**（倾向：重要道具不显示堆叠数，或恒为 1 且可隐藏「1」——侦探对拍剑/地图金样） |

生活类比：针线包是村长送的**一件**家当，不是血瓶可以囤三个。现在格子角上写着 3，像买了三瓶药。

### 现网预扫（假说）

| 点 | 预扫 | 嫌疑 |
|----|------|------|
| `MainItemDatabase.asset` · SewingKit | `itemType: 0` = **TaskItem** | 类型可能已对；问题更像**叠数**或 **UI 一律画 num** |
| `MainItemConfig.json` · SewingKit | `itemType: 2` = MaterialItem | 与 Database **不一致**（v2 以 Database 为准，JSON 可能误导） |
| `GetItemActionTask` | 直接 `AddMainItem`，**无**「已有则跳过」 | 续聊重复触发 / 调试多播 → 叠到 3 |
| `MenuFormMainItemBtn.UpdateInfo` | `num.text = $"{item.num}"` **无条件** | TaskItem=1 也可能显示「1」；=3 必显「3」 |
| 0901 定案 | 入库 TaskItem、买卖 -1、GetItem×1 | 产品「非消耗、就一个」与当时一致；**现网角标 3 是兑现失败** |

金样对照：`AiLinSword` / `Map` / `GushaNacklace` 等 TaskItem 在背包是否显示角标、重复 GetItem 会否叠加——侦探必须 YAML/Play 对拍。

### 否决

| 方案 | 原因 |
|------|------|
| 只改 Icon 图去掉「3」 | 「3」是 TMP 数量，不是画在 Icon 上 |
| 把 MaxStackPerItem 改成 1（全局） | 误伤药水/球；应针对唯一道具 |
| 删掉 GetItem 节点 | 会丢首次获得；应防重复或钳 1 |
| 当消耗品做「用掉」逻辑 | 产品明确不是消耗品 |

---

## 侦探（复制给 Agent）

```
你是【架构侦探】。只读分析，不改代码、不改资源、不提 MR。

任务：查清背包针线包显示「3」、像消耗品堆叠的根因；给出最小修法，满足「非消耗品、全档只有一个」。

### 产品钉死

1. 针线包不是消耗品。  
2. 就一个（持有量必须为 1；不能叠到 3）。  
3. 截图角标「3」必须消失（修完后不应再出现大于 1；UI 是否隐藏「1」对拍剑/地图）。

### 必读

@Assets/Doc/执行文档/0901/Village_村长家继续对话_中途获得针线包Tips_架构溯源报告.md
@Assets/Doc/施工说明/0901/Village_村长家继续对话_中途获得针线包Tips_施工说明.md
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Player/PlayerBagData.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/GetItemActionTask.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/MainItemPage/MenuFormMainItemBtn.cs
@Assets/GameRes/Config/MainItem/MainItemDatabase.asset
@Assets/GameRes/Config/MainItemConfig/MainItemConfig.json
@Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs
@Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab

### A. 类型：是不是被当成消耗品？

填表：

| 源 | SewingKit itemType | 枚举含义 |
|----|--------------------|----------|
| MainItemDatabase | ？ | TaskItem=0 / CostItem=1 / MaterialItem=2 |
| MainItemConfig.json | 预扫 2 | ？ |
| 运行时入包后 MenuFormMainItemInfo.itemType | ？ | Refresh 后谁覆盖谁 |
| IsCanUse(SewingKit) | ？ | 应为 false |

裁定：类型是否已满足「非消耗」；若 JSON 仍 2，是否会造成某路径当素材/可卖。

### B. 数量：为什么是 3？

1. `AddMainItem` 是否无条件 `num += count`？  
2. 续聊 Prefab 里 `GetItem(SewingKit)` 出现几次？Story 已用旗能否挡重发？调试跳过门闩是否仍可叠？  
3. 是否有其它系统再发 SewingKit？  
4. 对拍 AiLinSword：重复 GetItem 会否叠数？

### C. UI：角标规则

1. `MenuFormMainItemBtn` 是否对 TaskItem 也画数字？  
2. 剑/地图现网：有 1 时显不显角标？  
3. 产品「就一个」最小是：  
   - **U1** 持有钳到 1（角标最多 1）；和/或  
   - **U2** TaskItem（或唯一道具列表）隐藏数量 TMP  

推荐组合写清。

### D. 方案对比

| 方案 | 做法 | 何时选 |
|------|------|--------|
| **A** | Database/运行时保证 TaskItem；JSON 同步 0 | 类型仍错或双源不一致 |
| **B** | `AddMainItem`/`GetItem`：SewingKit（或 TaskItem 唯一件）已有则不再加；或 `SetMainItemCount(..., 1)` | 叠到 3 的主因 |
| **C** | 读档/Refresh 时把 SewingKit 钳为 1（修旧档） | 已有错误存档 |
| **D** | TaskItem 隐藏角标（或 num≤1 隐藏） | 展示层；须对拍剑 |
| **E** | 全局 MaxStack=1 | **否决** |

必答：最小集合是 B+C，还是还要 D？会不会误伤空桶×4（EmptyWaterBucket 是 TaskItem 但靠数量表达）——**空桶必须排除在「唯一钳 1」之外**。

### E. 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 新档走续聊首次获得 | 背包针线包 **1**；非消耗（不能当药喝） |
| 2 | 角标 | 无「3」；按报告：无数字或仅符合金样 |
| 3 | 同档再触发 GetItem（若能） | 仍为 1，不叠 |
| 4 | 旧档已有 3 | 读档或进包后变为 1（若方案含 C） |
| 5 | 空桶仍可用数量 1～4 | **不被**唯一钳误伤 |
| 6 | 血药/球仍可堆叠消耗 | 不误伤 |
| 7 | Tips「获得了针线包」 | 仍正常 |

### F. 报告结构

① 结论一句话（类型 / 叠数 / UI 各怎么修）  
② 为何会出现「3」  
③ 方案对比（含空桶排除）  
④ 要改文件路径级  
⑤ 验收表  
⑥ 风险与回滚  

写入：`Assets/Doc/执行文档/0922/SewingKit_非消耗唯一一件_架构溯源报告.md`
```

---

## 施工员（侦探闭环后再复制）

```
@Assets/Doc/执行文档/0922/SewingKit_非消耗唯一一件_架构溯源报告.md
@Assets/Doc/施工说明/0901/Village_村长家继续对话_中途获得针线包Tips_施工说明.md

你是【施工员】。按报告让针线包成为非消耗、全档唯一一件，去掉错误角标「3」。

约束：
- 禁止全局 MaxStackPerItem=1
- 禁止误伤 EmptyWaterBucket / FullWaterBucket 的数量玩法
- 禁止误伤 HpBall/MpBall/商店药水堆叠与消耗
- 保持 GetSewingKit Tips 与续聊三连节点可用
- 写入：`Assets/Doc/施工说明/0922/SewingKit_非消耗唯一一件_施工说明.md`
```

---

## 使用顺序

1. 复制「侦探」→ 先钉死「3」是重复发放还是类型/UI  
2. 确认空桶数量玩法不被误伤  
3. 复制「施工员」→ 新档+旧档（有 3 的）各验一遍
