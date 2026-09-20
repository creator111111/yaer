# Cursor Agent Prompt · 苍翠走廊史莱姆吃羊：运镜抖/闪，改成慢慢丝滑

> **角色**：先【架构侦探】只读核实抖/闪断点；拍板后【施工员】最小化把运镜做丝滑  
> **日期**：2026-09-13  
> **现象（用户实测）**：`VerdantCorridor` → `SlimeEatSheepStoryTrigger` → `VerdantCorridorSlimeEatSheep` 演出里，**摄像机运动不是抖就是闪，一点也不丝滑**；产品要求 **慢慢来、好好做完这段运镜**  
> **产品期望（钉死）**：  
> - 去程推到看点、停顿、拉回玩家：全程 **平滑、可感知「慢慢移动」**，无明显顿挫/一帧跳变/露景闪  
> - 对白与镜头先后仍对齐东城郊金标准（先「那是……」再推镜；拉回后再后续台词）  
> - 黑幕掩护刷怪时 **不穿帮闪内容**  
> **不是**：取消运镜改瞬切；重做整套 Cinemachine；改东城郊图当实验不还原；改村庄相机；顺手大改其它走廊对白（除非同源且报告要求）  
> **对照**：`ForestEastSceneSlimeEatSheep`（时序金标准）；Forest 门口消闪经验见 `执行文档/0912/ForestScene_*屏闪*` / `保留相机移动_消闪*`（机制可参考，**场景不同勿照搬林恩链脚本**）  
> **报告落盘**：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_运镜抖闪不丝滑_架构溯源报告.md`  
> **与前案**：`提示词/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序错乱_*` 解决 **先后顺序**；本案解决 **运动质感（抖/闪/太急）**。时序已对齐仍抖 → 按本案修。

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 镜头要慢慢推过去、停一下、再慢慢拉回来，像正常运镜。  
> 现在又抖又闪，看着难受。别用瞬切糊弄，把这段推拉做好。

### 术语钉死

| 词 | 本案含义 |
|----|----------|
| **抖** | 运镜起止或中途出现短促往返、手推、跟拍目标切换造成的顿挫 |
| **闪** | 一帧或数帧错误机位/露景、黑幕未盖住时刷怪、Follow 瞬切露穿帮 |
| **丝滑** | DOMove（或等价）全程可见平滑位移；起止无二次手推；缓动自然；时长足够「慢慢」 |
| **慢慢来** | 去程/回程 Duration **明显长于现网 1s**（助手建议 1.8～2.5s 或按距离定速；侦探用 Play 体感钉死） |

### 现网图序（助手预扫 · 须再核 connections）

`VerdantCorridorSlimeEatSheep.prefab`（预扫已对齐时序）：

```
0 Setup+UI淡入
→ 8 Statement「那是……」
→ 1 CameraMove Pos1→Pos2  Duration=1
→ 2 Wait 1s
→ 3 CameraMove Pos2→Pos1  Duration=1
→ 4 CameraFollowPlayer（默认 forceSnap=false）
→ 9…13 后续台词
→ 5 BlackMask → 6 Action2("start") → 7 收 UI/黑幕
```

场景：`SlimeEatSheepStoryTrigger/CameraPos2` local x≈**20.22**（约 20 单位横推，1s 偏急）。

### 运镜实现链（助手预扫 · 抖闪主嫌疑）

```
CameraMoveTaskAction.Move():
  WaitUntil !IsLock
  new GO @ StartPos
  SetFollow(go)                    ★ 默认 forceSnapToTarget=true
                                   → smoothTime>0 时手推对齐临时 GO（可见顿挫/闪）
  SetLock(true)
  go.DOMove(EndPos, Duration)      ★ 未设 Ease（默认 OutQuad 两端易顿）
                                   ★ Duration 现网=1，距离~20 → 太急
  SetLock(false)
  SetFollow(EndPos, forceSnap:false)
  Destroy(go)                      ★ 同帧换目标+销毁，易一帧跳变
  EndAction()

下一节点 CameraFollowPlayer：
  再 SetFollow(player)             ★ 若 EndPos 已是玩家 → 多余切换再顿一下

收尾 BlackMask 0→1 + Action2 刷怪   ★ 黑未满时露景 =「闪」
```

| # | 嫌疑 | 体感 |
|---|------|------|
| **A** | Duration=1 对 ~20u 过快 | 不「慢慢」、像甩镜头 |
| **B** | DOMove 无 InOut 缓动 | 起停生硬 |
| **C** | 开推 `SetFollow(go)` 默认 forceSnap=true + smoothTime 手推 | 推之前先抖/闪一下 |
| **D** | 推完 Destroy(go) 与换 Follow 交接脏 | 到位瞬间跳一下 |
| **E** | 回程后再 FollowPlayer 重复 | 二次顿挫 |
| **F** | 黑幕与刷怪时序/透明度 | 闪一下怪/场景 |
| **G** | Cinemachine XDamping 与 DOMove 跟拍目标叠加 | 橡皮筋感（次要） |

### 侦探须回答

1. Play 复现：抖/闪发生在 **开推瞬间 / 推到中 / 到位瞬间 / 拉回 / FollowPlayer / 黑幕刷怪** 哪几段？  
2. 上表 A–G 哪些已证实？东城郊同 Duration 是否同样急（走廊是否更糟因距离）？  
3. 「慢慢」的推荐 Duration / Ease / 是否按距离算速度？  
4. 修复是否优先 **只改走廊 Prefab 参数**（Duration），还是必须改 `CameraMoveTaskAction`（Ease、forceSnap、Destroy 时序）？东城郊是否一并受益？  
5. **方案 ≥2**：  
   - **A**：Prefab 加长去程/回程 Duration +（若可）节点去掉多余 FollowPlayer  
   - **B**：改 `CameraMoveTaskAction`：InOutSine、开推 `forceSnap:false`、DOMove 结束后安全交接再 Destroy；可选等待 Follow 稳定  
   - **C**：禁止 — 用 `smoothTime=0` 瞬切消闪（产品已否决「不要运镜」）  
   - **D（可选）**：刷怪段短黑幕保证全黑再 Action2（对齐 Forest M1 思路，仅本图）  
   列利弊；推荐组合；写清不要动什么。

### 必读

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md`（镜头用完成回调衔接，禁止用乱 Delay 硬匹配节奏）  
3. `VerdantCorridorSlimeEatSheep.prefab` + 场景 `CameraPos2`  
4. `ForestEastSceneSlimeEatSheep.prefab`（对照）  
5. `CameraMoveTaskAction.cs` / `CameraFollowPlayerActionTask.cs` / `CameraComponent.SetFollow`  
6. （参考）`执行文档/0912/ForestScene_保留相机移动_消闪与接话中断_架构溯源报告.md`  
7. 本提示词  

### 禁止（侦探阶段）

- 禁止改代码 / Prefab / 场景 / Git  
- 禁止建议「取消运镜改瞬切」当终态  
- 禁止把走廊 `StoryPrefabName` 改成东城郊文件  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读。

### 输出

`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_运镜抖闪不丝滑_架构溯源报告.md`

结构：

1. **结论一句话**（抖/闪主因；是否「太急+交接脏」）  
2. **运镜调用链**（逐步标抖闪点）  
3. **证据表**  
4. **与时序前案关系**（顺序对仍不丝滑）  
5. **方案 ≥2** + 推荐 + Duration/Ease 建议值 + 回归（时序、Action2、SingleUse、东城郊）+ 验收  
6. 不清处记 `OPEN_QUESTIONS.md`

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：报告钉死抖闪点与推荐组合（预期：加长 Duration + Ease/力 Snap 交接 + 去多余 Follow + 黑幕盖刷怪）。  
> **目标**：走廊史莱姆吃羊去程/回程 **明显更慢更丝滑**；起止无明显抖闪；黑幕刷怪不穿帮；对白↔镜头顺序不回退。  
> **优先**：  
> 1）`VerdantCorridorSlimeEatSheep`：去程/回程 Duration 提到报告值；必要时删/跳过重复 `CameraFollowPlayer`；黑幕与 `start` 顺序保证全黑再刷怪  
> 2）若报告要求：改 `CameraMoveTaskAction`（Ease、开推 forceSnap、Destroy 交接）——改动须评估东城郊等同图，**禁止**破坏其它演出  
> **禁止**：整段改瞬切；全局 `smoothTime=0`；改 Action2→Action1；改东城郊时序金标准；Update 堆运镜；未评估就改全项目 CameraComponent 默认。  
> **文档**：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊_运镜丝滑_施工说明.md`  
> **验收**：  
> 1）Play ≥2 次：推镜「慢慢」可感知，无开推/到位明显跳变；  
> 2）拉回同样丝滑；Follow 后台词正常；  
> 3）黑幕期间刷怪不露闪；  
> 4）第一句仍在推镜前，后续台词仍在拉回后；  
> 5）抽测东城郊同触发器：无严重回归（若改了共用 CameraMoveTaskAction，两边都要过）。  

---

## 【验收员】Prompt（可选）

> Debug `[SlimeEatSheepCamSmooth]`：`CameraMoveTaskAction` 打 Start/End 名、Duration、forceSnap、DOMove 起止时间戳、Destroy 帧；肉眼或录屏标「抖/闪」时刻是否消失。  
> 输出：通过项 + 剩余风险（Damping、其它共用 CameraMove 的图）。
