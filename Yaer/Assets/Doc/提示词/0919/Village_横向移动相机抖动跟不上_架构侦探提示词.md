# Cursor Agent Prompt · 村庄横向移动：摄像机跟不上、会抽一下

> **角色**：【架构侦探】只读对比村庄和其它场景的跟拍参数，定根因；报告通过后再施工  
> **日期**：2026-09-19  
> **现象（用户）**：在村庄里**左右走**（横向）时，摄像机发抖，感觉**跟不上人**，会**抽一下**  
> **产品期望（钉死）**：左右走时镜头跟人顺，不滞后再猛追，不抽搐。手感向「用户觉得正常的其它场景」靠，不要改成瞬切贴脸  
> **不是**：改上下纵深跟随规则；改开场运镜/对白锁相机；重做整套 Cinemachine；改角色移速来凑镜头  
> **报告落盘**：`Assets/Doc/执行文档/0919/Village_横向移动相机抖动跟不上_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「村庄哪套参数和正常场景差在哪、改哪一个文件」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 人在村里按左右走，镜头不是稳住跟着，而是慢半拍，然后抽一下追上来。  
> 先跟别的场景比，看人家为什么不抽，再决定村里改哪几个数。不要凭感觉把阻尼改成 0。

### 词不要混

| 词 | 这里指 | 不是 |
|----|--------|------|
| **横向** | 左右走，世界 **X** | 不是 W/S 纵深 |
| **跟不上、抽一下** | 镜头滞后，再突然拉近，看起来一顿一顿 | 不是切场景闪一下，也不是对白运镜抖 |
| **村庄** | 先钉能左右长距离走的村景，优先 `Village_KenMuNi1` | 不要先用室内小房间代表全村 |

### 现网相机（0822 之后又改过，数字必须重读场景）

0822 报告里 KenMuNi 街机大致是：Framing Transposer，`XDamping=0.7`，`DeadZoneWidth=0`，`YDamping=0`，`DeadZoneHeight=1`。  
后来改成**两台虚拟相机**：`VCam_Street` + `VCam_Part3`，进出高台只切 Priority，Body 写在 Inspector 里。代码里还有一份对照表 `KenMuNiStreetDefault` / `KenMuNiPart3DepthFollow`（`xDamping` 仍写 0.7），注释说**不再每帧写回 Body**。

所以：抽搐可能来自街机自己的横向阻尼，也可能来自两台相机切换/Blend，还可能来自人物移动和镜头更新不在同一拍。三件事都要查，不能只改一个阻尼交差。

### 嫌疑（排序用，可推翻）

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A** | 街机 `XDamping` / SoftZone / Lookahead 比正常场景更「橡皮筋」 | 把 KenMuNi 的 Street、Part3 和对照场景的 Framing Transposer 逐项列表 |
| **B** | 两台 VCam 的 Blend（文档曾写 Custom 约 0.4 秒）在横移时仍在混，或边界来回切 Priority | 读 Brain 的 Blend；读 `VillageCameraDepthFollowZone` 是否只切 Priority、会不会在直路上也切 |
| **C** | 人物 `Rigidbody2D` 在 FixedUpdate 动，镜头在 LateUpdate 跟，插值关掉就会一卡一卡 | 对比村庄玩家刚体 Interpolate，和手感正常的场景是否同一套 |
| **D** | `CameraComponent.smoothTime`（约 0.3）和 Cinemachine 阻尼**一起**在推镜头 | 确认日常横移走的是 Follow，还是每帧又手推了一次 |
| **E** | Confiner 贴边时把机位按回去，和跟随对着干 | 只在贴 CameraArea 边缘抽，还是路中间也抽。两种结论不同 |
| **F** | 像素对齐 / 正交尺寸取整，横移时镜头按像素跳 | 看 Brain 或相机上有没有 PixelPerfect、位置取整 |

### 对照场景（至少这三张，可再加一张用户觉得顺的）

| 场景 | 为什么拿来比 |
|------|----------------|
| `Village_KenMuNi1` | 村外长路，优先当复现场 |
| `Village_HomeScene1` | 同属村庄，0822 已对过室内 Framing，看横向是否本来就不同 |
| `ForestEastScene` | 村外横版，同一套 `CameraComponent` + Cinemachine 时，用它当「别的场景」 |

若 `Village.unity` / `Village_OutSide` / `Village_KenMuNi_night` 和 KenMuNi1 不是同一套相机，报告里写明「不复现 / 同一套 / 要另改」，不要用一个场景代表全部村庄。

### 禁止

- 本阶段不改代码、不改场景、不改 Prefab。  
- 不要把 0822「纵深要不要跟」重新做成方案。本次只解决**横向抽搐**。  
- 不要建议全局 `smoothTime=0` 或全项目阻尼改成 0。那是瞬切，不是治抽。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/8月/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/VillageCameraDepthFollowZone.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/ForestEastScene.unity

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0919/Village_横向移动相机抖动跟不上_架构溯源报告.md

---

## 背景（策划白话）

在村庄里左右移动时，摄像机会抖动，感觉跟不上人物，会抽一下。
其它场景没有这么明显。先对比别的场景的跟拍是怎么配的，再给出村里最小改法。

优先复现场景：`Village_KenMuNi1`（村外能走长距离的路）。
对照：`Village_HomeScene1`、`ForestEastScene`。
不要改上下跟随的产品规则，不要改开场对白锁相机。

---

## 必读 / 优先扫描

历史数字作废前必须在场景 YAML 里重读。0822 之后 KenMuNi 已改成双虚拟相机，街机参数以 Inspector / 场景为准，不以旧报告为准。

### A. 把三张场景的「日常横移跟拍」列成一张表

每张场景写出实际在跟玩家的那台 VirtualCamera（KenMuNi 要 Street 和 Part3 两行）：

- Body 类型（是不是 Framing Transposer）
- XDamping、YDamping
- DeadZoneWidth、SoftZoneWidth
- Lookahead Time / Smoothing（有则写，没有写无）
- Confiner 有没有、贴边时会不会把 X 按回去
- CinemachineBrain 更新时机（LateUpdate 还是 FixedUpdate / SmartUpdate）
- 有没有第二台 VCam、Blend 样式和时长
- `CameraComponent.smoothTime` 在横移过程中会不会再推一次位置

### B. 人物侧是不是和镜头不同拍

- 村庄横移改的是 `Rigidbody2D.velocity` 还是直接改 position
- 刚体 Interpolation 开没开
- 镜头跟的是玩家根节点，还是另一个滞后的点
- 对照场景若手感正常，人物更新方式有何不同

### C. 双相机会不会在直路上也抽

- `VillageCameraDepthFollowZone` 现在是不是只改 Priority、不写 Body
- 玩家沿路横走、人还在街区内时，会不会反复切 Street/Part3
- Blend 未播完时横移，画面会不会一顿

### D. 推荐一种最小改法

必须回答：

1. 主因是表里哪几项（可以叠加，但标主因）
2. 改场景里哪一台 VCam 的哪几个字段，还是改代码。写全路径
3. 为什么不把阻尼改成 0，为什么不动 Part3 的纵深参数
4. Home / Forest 里哪一项值得照抄，哪一项不能照抄（例如室内 Y 跟随不能抄到村街）

只推一种方案。另外两种各用一句话否决。

---

## 报告结构（固定）

① 结论一句话（抽搐主因 + 改场景还是改代码）  
② 原因（大白话 + 三场景参数对照表）  
③ 用户需要做什么（先在 KenMuNi 路中间左右走，再贴 CameraArea 边缘走一次，看是不是只有贴边才抽）  
④ 给施工员的补充：改哪些字段、不要改哪些、推荐方案和否决方案

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0919/Village_横向移动相机抖动跟不上_架构溯源报告.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/VillageCameraDepthFollowZone.cs

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告的推荐方案做最小改。不要重做 Cinemachine，不要改角色速度，不要改纵深跟随产品规则。

目标：
- 村庄左右走时镜头跟人顺，不慢半拍再抽一下
- 路中间和贴边界两种都要看报告：报告说只修一种，就不要两种都改
- Home、Forest 以及报告写明不要动的 VCam，保持原样

限制：
- 禁止在 Update 里新写跟拍逻辑；跟拍继续走现有 Cinemachine
- 不要把 smoothTime 或 XDamping 全局改成 0
- 若改双相机切换，注释说明为什么不会在直路上来回切
- 施工说明写入：`Assets/Doc/施工说明/0919/Village_横向移动相机抖动跟不上_施工说明.md`
- 没有新架构就不要新造技术文档

完成后用大白话给验收清单：村路中间按住左右走一段，再沿 CameraArea 边缘走一段。
```
