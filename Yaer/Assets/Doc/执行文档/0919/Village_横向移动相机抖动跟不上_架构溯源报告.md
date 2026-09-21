# Village · 横向移动相机抖动跟不上 · 架构溯源报告

> 日期：2026-09-19  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> 优先复现：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> 对照：`Village_HomeScene1`、`ForestEastScene`  
> 0822 单相机数字作废。下列 Framing 数字均从当前场景 YAML 重读。

---

## ① 结论一句话

**路中间的抽，主因是村里每物理帧把玩家坐标写死，刚体平滑被掐掉，镜头又用 0.7 秒阻尼去追这个一顿一顿的点，出了软区就硬拉一下。** 要改代码（`TownPlayerLocomotion` 写坐标的方式），不要改 `VCam_Part3` 的纵深，也不要把阻尼打成 0。

---

## ② 原因

### 大白话

左右走时，人是一格一格挪的，镜头还故意慢半拍去追，追太远就猛地拽回来。所以又像跟不上，又会抽一下。

森林和民居没有这么明显，不是因为它们的横向阻尼更「高级」。东郊街机参数和村街几乎一样。差别是：只有肯姆尼（和村长家）会在每个物理步里亲手改玩家位置，把「帧与帧之间的平滑」弄没了。民居、东郊让刚体自己插值，镜头追的是一条顺的线。

村外长路的中间**不会**在 Street / Part3 两台相机之间来回切。双相机只盖左边高台。贴 `CameraArea` 边、或人走到大约 x = -93 那条竖线时，才会额外被边界或切换拽一下。那是次因。

`CameraComponent.smoothTime = 0.3` 只在「重新把镜头对上人」那一次手推里用。人已经在路上左右走时，它不再推位置。

### 三场景日常横移跟拍（现网 YAML）

Brain 的 `m_UpdateMethod: 1` 是 LateUpdate（0=Fixed，1=Late，2=Smart）。三张场景都是 1。`CameraComponent.Init` 还会再强制写成 LateUpdate。

Lookahead Time / Smoothing：三台跟拍机都是 **无**（0 / 0）。

| 项 | KenMuNi `VCam_Street`（路中间就这台） | KenMuNi `VCam_Part3`（人进高台才 Live） | Home `Cinemachine` | ForestEast `Cinemachine` |
|----|----------------------------------------|------------------------------------------|--------------------|---------------------------|
| 路径 | `Village_KenMuNi1` → `Camera/VCam_Street` | 同场景 `Camera/VCam_Part3` | `Village_HomeScene1` → `Camera/Cinemachine` | `ForestEastScene` → `Camera/Cinemachine` |
| Body | Framing Transposer | Framing Transposer | Framing Transposer | Framing Transposer |
| XDamping | **0.7** | **0.7** | **1** | **0.7** |
| YDamping | **0** | **0** | **1** | **0** |
| DeadZoneWidth | **0** | **0** | **0** | **0** |
| SoftZoneWidth | **0.25** | **0.25** | **0.25** | **0.25** |
| DeadZoneHeight（不要当横移主因，只作对照） | 1（纵深不跟） | 0（高台跟纵深） | 0 | 1 |
| SoftZoneHeight | 1 | 0.351 | 0.8 | 2 |
| ScreenY | 0.5 | **0.88** | 0.5 | 0.5 |
| Lookahead | 无 | 无 | 无 | 无 |
| Confiner | 有，`CameraArea`，贴屏幕边，**Damping=0** | 同一块 `CameraArea`，Damping=0 | 有，Damping=0 | 有，Damping=0 |
| 贴边会不会把 X 按回去 | 会。低区竖高约 15.8，和镜头上下刚好卡满；左右尽头和 L 形拐角（世界 x≈-92）会硬钳 | 同盒，高台区更高，上下不卡满 | 室内盒子小，很快贴边后镜头基本钉住 | 有钳，但东郊盒子不跟村街一样「上下刚好一张屏」 |
| Brain | LateUpdate | 同一颗 Brain | LateUpdate | LateUpdate |
| 第二台 / Blend | 有 Part3。默认 Blend 是 2 秒缓入缓出；Street↔Part3 用 `KenMuNi1_StreetPart3_Blends`，**0.4 秒**缓入缓出 | 同上 | **无第二台**，CustomBlends 空 | **无第二台**，CustomBlends 空 |
| 起步 Priority | Street **10**，Part3 **0** | 人进区后 Part3 升到 **20** | 10 | 10 |
| smoothTime 横移中再推？ | **不会** | 不会 | 不会 | 不会 |
| 跟的点 | 玩家根节点 `transform`（`BaseGameSceneManager` 里 `SetFollow`） | 同一根，两台一起绑 | 同一根 | 同一根 |

Cinemachine 2.6 里：死区宽度是「阻尼追」的框，软区宽度是「出了就不再阻尼、直接拽回」的框。死区宽 0 等于人一偏，镜头就开始追；软区 0.25 等于人在画面上偏过大约一成二，镜头改为硬拉。村街和东郊这四项（X 阻尼 0.7、死区宽 0、软区宽 0.25）是同一套。所以**只改这几个数字，抄东郊等于没改。**

### 人物是不是和镜头不同拍

| 项 | 肯姆尼村外 | 民居 HomeScene1 / 东郊 |
|----|------------|------------------------|
| 横移怎么动 | `PlayerMoveComponent` 在 FixedUpdate 写 `Rigidbody2D.velocity.x` | 同样写速度 |
| 另外写位置？ | **会。** `TownPlayerLocomotion.WriteRootTransformWithAuthoritativeDepthY` 每个物理步把 `rb.position` 和 `transform.position` 都赋一遍；物理步结束后协程再写一次 | **不会。** `SceneName.IsVillageExplorationScene` 只有 `Village_KenMuNi1` 和村长家。这两张对照场景不进村庄 2.5D |
| 刚体 Interpolation | 玩家 Prefab `m_Interpolate: 1`（开了） | 同一 Prefab，开了，而且没人每步把位置写死，所以插值真的在工作 |
| 镜头跟谁 | 玩家根，没有第二个滞后点 | 一样 |

写死 `transform.position` 会把插值清掉。画面上人按物理步一顿一顿走（大约每秒 50 下）。镜头在 LateUpdate 用阻尼去追这些台阶：慢的时候像跟不上，误差超出软区就抽一下。

纵深规则不用动：权威 Y 仍由这段逻辑负责。要改的只是「纯左右走时不要连 X 一起写死」。

### 双相机在直路上会不会抽

`VillageCameraDepthFollowZone` 现网只改 Priority，注释和 `CameraComponent.SetKenMuNiPart3CameraMode` 都写了：有第二台时**不写 Body**。场景上那份 Profile 只是旧字段，双机路径不会拿去刷参数。

触发盒在 Map 下，中心本地 `(-133, 21)`，大小 `80×58`。Map 在原点，所以盒子大约是世界 x `-173～-93`、y `-8～50`，就是左边高台。村外长路在 x 大于约 -93 的右边，人在路中间时 `wantPart3` 一直是 false，不会反复切。

人贴着 x≈-93 那条线走时，滞回只有 0.35，冷却 0.4 秒。可能切一次。切换中两台 ScreenY 是 0.5 和 0.88，Blend 0.4 秒，画面会上下顿一下。这不是路中间的主因，本次也不改这套纵深切换。

开场锁相机不在这次范围里。

---

## ③ 用户需要做什么

1. 进 `Village_KenMuNi1`，站在**路中间**（离左右边界和左边高台都远），只按住左或右走一段。预期：现在就会抖、会抽。这能证明不是只有贴边才有。
2. 再贴着 `CameraArea` 左拐角（大约 x = -93）和地图左右尽头各走一次。预期：贴边会**额外**猛一下，那是 Confiner 阻尼为 0，和路中间不是同一件事。
3. 对照：`ForestEastScene` 平地左右跑，镜头应顺很多；`Village_HomeScene1` 房间小，镜头很快贴边，不要拿它的上下跟随去要求村街。

---

## ④ 给施工员的补充

### 推荐方案（只此一种）

改 `Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs` 里 `WriteRootTransformWithAuthoritativeDepthY`（以及 `PostPhysicsResyncDepthCoroutine` 里对它的再调用）：

- 纵深 Y 仍以 `_villageWorldY` 为准，产品规则不改。
- **Y 没变时不要写** `rb.position` / `transform.position`，让横移继续走速度 + 已开启的 Interpolation。
- Y 要变时用 `Rigidbody2D.MovePosition` 只改 Y，**不要再赋** `transform.position`（这一行会清掉插值）。
- 注释写明：为什么不能改成「阻尼打成 0」，为什么不能停掉权威 Y。

不要改这些：

- `VCam_Part3` 的 ScreenY、DeadZoneHeight、SoftZoneHeight、YDamping
- `VillageCameraDepthFollowZone` 的切机、滞回、开场锁相机
- `CameraComponent.smoothTime`
- 三张场景的 Confiner 形状（贴边另案）
- 不要把 `VCam_Street` 的 XDamping 改成 0

### 否决

| 方案 | 一句原因 |
|------|----------|
| 只把村街 XDamping 改成 0 | 人仍是一格一格的，阻尼为 0 会把每一格都照进镜头，更抽，不是更顺 |
| 把民居的 YDamping=1、DeadZoneHeight=0 抄到村街 | 主街上下也会拉镜头，和「村街纵深不跟、只高台跟」相反 |

### 对照里什么能抄、什么不能抄

- **不能抄民居的上下跟随**（YDamping、DeadZoneHeight）。室内要跟人的前后，村街产品是不跟。
- **不必抄东郊的 XDamping / 软区**。现网已经和村街一样，抄了等于没改。东郊顺，是因为没人每步写死坐标。
- **不要抄民居的 XDamping=1**。那是更拖，路会更像跟不上。
