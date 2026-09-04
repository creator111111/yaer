# Cursor Agent Prompt · 巨树 2 楼 VillageWalkArea2 宝箱：生命球×3 + 体力球×3 + Tips

> **角色**：先【架构侦探】只读定方案，报告后再【施工员】  
> **日期**：2026-09-01  
> **场景**：`Village_KenMuNi1`  
> **摆放区**：用户已配的 **`VillageWalkArea2`**（巨树上方 2 楼可走区；**禁止改该多边形尺寸**）  
> **产品设定（钉死）**：  
> 1. 在 **VillageWalkArea2 内**放一个 **宝箱**  
> 2. 打开后获得：**生命球（HpBall）×3** + **体力球（MpBall）×3**  
> 3. **道具获得提示**与剑 / 空桶 / 针线包 **同款** Tips 横幅（`OpenTipsForm` · Item）  
> **样板**：`WestRappRoadHpMpBox`（双球入包 + 双 Tips 入队）；`HomeScene1Xiaer`（×3 + GetHpBall/GetMpBall）；`HomeScene2Box`（开箱存档）  
> **本阶段（侦探）**：只读；禁止改场景 / 代码 / WalkArea2 形状  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望）

```
玩家在 VillageWalkArea2（巨树 2 楼）
  → 靠近宝箱 → 点 E / 点击（对齐现网宝箱交互）
  → 开箱动画 + 存档「已开」
  → AddMainItem(HpBall, 3) + AddMainItem(MpBall, 3)
  → OpenTipsForm("GetHpBall") → OpenTipsForm("GetMpBall")  // 队列依次横幅
  → 同档不可再开
```

**禁止**：只入包不弹 Tips；只弹 Tips 不入包；改 WalkArea2 大小给宝箱腾地。

### 发奖与 Tips（已有能力 · 勿重发明）

| 步骤 | API | TipKey（图集已有） |
|------|-----|-------------------|
| 入包生命球×3 | `PlayerBagData.AddMainItem(EMainItemName.HpBall, 3)` | — |
| 入包体力球×3 | `AddMainItem(EMainItemName.MpBall, 3)` | — |
| 横幅生命 | `TipsComponentGSM.OpenTipsForm("GetHpBall")` | ✅ `TipInfoAtlas*/GetHpBall.png` |
| 横幅体力 | `OpenTipsForm("GetMpBall")` | ✅ `GetMpBall.png` |

**数量 vs 横幅**：现网 **HomeScene1Xiaer / WestRapp** 均为 **一次入包 N 个 + 弹一次对应 GetXxx 图**（图上未必写「×3」）。  
空桶曾用 `GetEmptyWaterBucketx4` 专图——本期 **倾向复用 GetHpBall/GetMpBall**，不必新做「获得了生命球×3」图，除非产品要字面 ×3（OPEN）。

**连发**：Tips 队列依次 fill（0830 报告已验收口径）；先 Hp 后 Mp（对齐 Xiaer/WestRapp）。

### 挂点方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **B1 · 复用/仿 `WestRappRoadHpMpBox`** | 新实体脚本或直接拷贝逻辑：`hpBallCount=3`、`mpBallCount=3`；场景放宝箱 Prefab；存档旗独立 | ✅ 最贴「双球+双 Tips+开箱」 |
| B2 · 对话图 GetItem×2 + OpenTipsFormActionTask×2 | 须 TriggerStory Prefab | ⚠️ 重；宝箱更宜 C# |
| B3 · 只 GetItemActionTask | 无横幅 | ❌ |
| B4 · 新 Tips UI | — | ❌ |

### 存档 / 单次

| 方案 | 做法 | 倾向 |
|------|------|------|
| **S1 · 场景 Archive 布尔** | 如 `KenMuNi1Data.tree2fHpMpBoxOpened`（对齐 `WestRappRoadData.hpMpBoxOpened` / `HomeScene2Data.boxOpened`） | ✅ |
| S2 · StoryTriggerCountData 键 | 无对白时略别扭 | 次选 |

读档：已开 → 动画 Open、不可交互。

### 场景摆放

| 项 | 要求 |
|----|------|
| 父级 | `Village_KenMuNi1` · 建议 `Objects` 下实体（进 `sceneObjs`） |
| 位置 | **在 VillageWalkArea2 多边形内**（脚点可站到）；对齐 `ExitFrom_HomeSceneChief2f` 附近平台 |
| WalkArea2 | **禁止改尺寸**；只挪宝箱 Transform |
| 交互 | `InteractiveComponent` + Collider；Layer/脚点规则对齐现网宝箱 |
| 美术 | 复用现网宝箱 Prefab/动画（侦探点名资源路径）；勿用商店 Chest 对白热区 |

### 与上游关系

| 依赖 | 说明 |
|------|------|
| 楼梯换场巨树 2 楼 | 玩家能到达 WalkArea2 才验得到箱 |
| Village2_5D | 2 楼须 WalkArea2 生效（0901 换场报告 W1） |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ WalkArea2 内宝箱：Hp×3 + Mp×3 + 双 Tips | ❌ 改 WalkArea2 多边形 |
| ✅ 同档单次开箱 | ❌ 商店点胸 `Village_ShopChest` 逻辑 |
| ✅ 复用 GetHpBall / GetMpBall 图集 | ❌ 新 Tips 系统 / 动态拼字 |
| ✅ 存档旗 + sceneObjs | ❌ 发金币当球 |

### 严禁

- 只 `AddMainItem` 不 `OpenTipsForm`  
- TipKey 用中文文件名  
- 把箱放在 WalkArea2 **外**导致站不到/夹回 1 楼  
- 改 WalkArea2「扩大好放箱」  

### 开放（侦探须答）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 交互：点 E / 点击 / 走进？ | 对齐 WestRapp/HomeScene2Box（点交互） |
| Q2 | Tips 要不要专用「×3」图？ | **否**；复用 GetHpBall/GetMpBall |
| Q3 | 脚本新建 vs 直接挂改参的 WestRapp 组件？ | **新建村用类名**（或通用 HpMpBox）避免西境存档耦合 |
| Q4 | 存档字段挂哪？ | `KenMuNi`/`Village` 场景 Data 扩布尔 |
| Q5 | 开箱要不要对白 Story？ | **倾向无对白直接发奖**（WestRapp `useStoryOnOpen=false` 路径） |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab。
禁止改 VillageWalkArea2 多边形尺寸。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
在 Village_KenMuNi1 的 VillageWalkArea2（巨树 2 楼）内放宝箱：
打开获得生命球×3、体力球×3，并弹出与剑/空桶同款 Tips 横幅（GetHpBall + GetMpBall）。
同档只开一次。

## 必读（Tips / 宝箱样板）
@Assets/Doc/执行文档/8月/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/WestRappRoad/WestRappRoadHpMpBox.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/HomeScene1/HomeScene1Xiaer.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/HomeScene2/HomeScene2Box.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/UIPanel/OpenTipsFormActionTask.cs
@Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas/GetHpBall.png
@Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas/GetMpBall.png

## 必读（摆放场景）
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
（VillageWalkArea2、ExitFrom_HomeSceneChief2f、sceneObjs、现有宝箱实体若有）
@Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md
（若已落盘；2 楼到达依赖）

检索：WestRappRoadHpMpBox、hpMpBoxOpened、GetHpBall、GetMpBall、
VillageWalkArea2、HomeScene2Box、AddMainItem。

## 侦探任务
1. 点名可复用的宝箱 Prefab/动画路径；确认 HpBall/MpBall 枚举与 Tips 图集 Key。
2. 裁定 B1 脚本方案 + 存档旗挂点；是否需要 Story。
3. 摆放：WalkArea2 内坐标建议（相对 ExitFrom_HomeSceneChief2f）；交互七件套。
4. 发奖顺序：入包×3×2 + 双 OpenTipsForm 队列；Save 时机。
5. 最小清单 + 验收 + OPEN；写明不改 WalkArea2 尺寸。

## 报告落盘
Assets/Doc/执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md

结构：①结论 ②样板对拍 ③脚本/存档 ④场景摆放 ⑤Tips ⑥最小清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md

## 目标
1. 在 VillageWalkArea2 内放置宝箱（坐标在区内；不改 WalkArea2 尺寸）。
2. 打开：HpBall×3 + MpBall×3 入包；OpenTipsForm("GetHpBall") 与 "GetMpBall"（Item）。
3. 同档单次；读档已开则不可再开。
4. 对齐 WestRappRoadHpMpBox / HomeScene2Box；禁止只入包无 Tips；禁止新 Tips UI。

## 落盘
Assets/Doc/施工说明/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_施工说明.md
同步 OPEN_QUESTIONS。

## 验收
- [ ] 站在 WalkArea2 内可与宝箱交互
- [ ] 开箱后背包生命球+3、体力球+3
- [ ] 依次出现 GetHpBall、GetMpBall 花边横幅 + 获得物品音效
- [ ] 同档再点不再发奖
- [ ] 读档已开：箱为打开态、不可交互
- [ ] VillageWalkArea2 多边形未改
- [ ] 剑/空桶/针线包 Tips 回归正常

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先跑侦探** → 定宝箱 Prefab、存档旗、摆点。  
2. 发奖几乎可直接抄 **`WestRappRoadHpMpBox`**（数量改成 3/3）。  
3. Tips 图 **已有** `GetHpBall` / `GetMpBall`，一般不用新做「×3」图。  
4. **不许改 `VillageWalkArea2` 大小**——只把箱子放进区里。
