# Village_KenMuNi1 地图边缘双机硬切 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab / Blend）  
> Unity：2020.3.48f1 / Cinemachine 2.6  
> 对照：0831 双 VirtualCamera。以现网 YAML / 代码为准。

---

## ① 结论一句话

**类型 C（又切机又顶围栏）。** 高台感应区的外缘和 `CameraArea` 贴灰底的边几乎重合，切机发生在镜头已经被围栏钉死的那一帧；0.4 秒 Blend 的起点和终点被钳到同一条边上，看起来像硬切。最小优化：感应区外缘往里收，并把 Street↔Part3 的 Blend 与冷却一起加长。不拆双机，不改 ScreenY，不改日常横移阻尼。

---

## ② 原因

大白话：高台和街道是两台相机，人走进绿框就换一台。绿框的左边、上边、下边画得和地图外缘一样齐。人贴着灰边走时，两台相机都被围栏按在同一条边上，中间那 0.4 秒几乎没有可移动的距离，所以换机像跳一下，而不是滑过去。

### A. 数字

**1. `CameraDepthFollowZone_Part3`（挂在 Map 下，Map 在原点）**

| 项 | 值 |
|----|----|
| 本地中心 | (-133, 21) |
| Box 尺寸 | 80 × 58，offset 0 |
| 世界中心 | **(-133, 21)** |
| 世界外缘 | **X -173～-93，Y -8～50** |

**2. `CameraArea`（Confiner 多边形，父节点 `Camera` 世界 (32.56, 0)，自身本地 (-32.82, 0)）**

多边形世界坐标（本地 + (-0.26, 0)），L 形：

| 顶点（世界） | 含义 |
|--------------|------|
| 左上 (-172.36, 50.11)、左下 (-172.43, -7.80) | **贴灰底的左边** |
| 高台内角 (-92.45, 50.15) → (-92.42, 8.00) | 高台与街道的台阶 |
| 街道底/顶 | Y ≈ -7.79～8.00（高约 **15.8**，刚好一屏，ortho 7.9×2） |
| 右缘 | X ≈ 66.07 |

高台这块围栏：X 约 **-172.4～-92.4**，Y 约 **-7.8～50.1**。  
和 Zone 比：左、上、下三条外缘相差不到 1 个单位；Zone 右缘 X=-93 也贴着台阶 X≈-92.4。

**3. 机名**

仍是 `VCam_Street`、`VCam_Part3`。Blend 资产按这两个名字匹配，**对得上，不是名字对不上才 Cut**。

**4. Brain**

`m_CustomBlends` guid = `KenMuNi1_StreetPart3_Blends`。双向 `m_Style: 1`（EaseInOut），`m_Time: 0.4`。默认 Blend 仍是 EaseInOut **2 秒**，这对有名字的切换不生效。

**5. 贴边那一帧**

玩家根若在 Zone 外缘（例如 X≈-173 或 Y≈50 / Y≈-8），滞回只有 0.35，判定盒几乎就在围栏边上。同一帧：人在 Zone 边上（会改 Priority），镜头视口也已经顶到 `CameraArea` 同一条边（Confiner `m_Damping: 0`，顶上去不滑）。**又切又顶。**

两台 ortho 都是 **7.9**，大小不是跳变来源。

### B. 为什么 0.4 秒在边缘像硬切

同一玩家位置，两台理想机位差主要在 **Y**：

| | Street | Part3 |
|--|--------|-------|
| ScreenY | 0.5（人在画面中间） | **0.88**（人偏上） |
| DeadZoneHeight | **1**（整屏死区，不跟 Y） | **0**（跟着 Y） |
| SoftZoneHeight | 1 | 0.351 |
| BiasY | 0 | 0.5 |
| ortho | 7.9 | 7.9 |

理想机位差：`(0.88 - 0.5) × 15.8 ≈ 6` 个世界单位（Part3 镜头更低，人更靠上）。高台里有垂直空间时，这 6 个单位用 0.4 秒 EaseInOut 还能看出是滑。

贴在灰边上时，Confiner 把两台都钳在同一条边上（街道走廊本身只有 15.8 高，镜头 Y 根本没有活动量；高台外缘同理）。Blend 的起点和终点世界坐标差被吃掉，接近 0。0.4 秒去插值一段「几乎为 0」的位移，肉眼就是硬切。加长时间只能缓解「还没贴死、仍有几单位差」的情况；**两台都被钉在同一像素上时，只加时间仍然像跳。**

所以切换边要离开围栏：在还有大约一屏活动空间时就换机，让那约 6 单位的 Y 差在围栏里面滑完，而不是在灰边上被钳没。

### 调用链（贴边）

```
玩家根坐标 LateUpdate
  VillageCameraDepthFollowZone
    内缩/外扩 0.35 的盒子
    进/出 → CameraComponent.SetKenMuNiPart3CameraMode（只改 Priority）
  Brain 按名字找 CustomBlends：VCam_Street ↔ VCam_Part3，0.4s EaseInOut
  两台 Framing 各自算出理想位（ScreenY 0.5 vs 0.88）
  各自 Confiner（Damping 0）钳进同一 CameraArea
  贴边时两条输出几乎重合 → Blend 看不见
```

冷却现网 **0.4s**，与 Blend 等长。加长 Blend 时冷却必须一起加长，否则 Blend 没完又翻一次，边缘来回抖。

---

## ③ 用户需要做什么

1. 走到截图那种贴灰底的外缘（高台左侧或顶/底边）。看跳的瞬间是整幅构图换了（切机），还是只是镜头撞上灰边弹一下（纯围栏）。按本报告应是两者叠在一起。  
2. 勾上 `CameraDepthFollowZone_Part3` 的 `logStateTransitions`，贴边时 Console 应出现 `part3Live=true/false`。有这句就是切了机。  
3. 施工后再走同一条边：应能看出一段过渡，而不是一帧跳完；高台里上下走，人仍偏画面上方（ScreenY 0.88 不变）；街上左右走，手感不要变硬。

---

## ④ 给施工员的补充

### 推荐方案（只此一种）

两处一起改，数值是起步值：

| 文件 | 字段 | 现网 | 改为 |
|------|------|------|------|
| `Assets/GameRes/Config/Camera/KenMuNi1_StreetPart3_Blends.asset` | 双向 `m_Time` | 0.4 | **0.8**（`m_Style` 保持 1 / EaseInOut） |
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` 上 `CameraDepthFollowZone_Part3` | `modeSwitchCooldownSeconds` | 0.4 | **0.8**（不要短于新 Blend） |
| 同上物体的 `BoxCollider2D` | 中心 / 尺寸 | 本地 (-133, 21)，80×58 | **外缘内收约 8**（约半屏高，盖住 ScreenY 那 6 单位）。起步：世界盒改为约 **X -164～-100，Y 0～42**（本地中心约 **(-132, 21)**，尺寸约 **64×42**）。右缘从台阶 -93 收到约 -100，避免在「走廊只有一屏高」的台阶上切。`hysteresisWorldUnits` **仍 0.35** |

内收后：人还没贴灰边就换机，两台在高台内部把约 6 单位的 Y 差用 0.8 秒滑完；贴到灰边时切机已经结束，围栏只负责不露出地图外。

### 不要改

| 不要改 | 原因 |
|--------|------|
| 拆掉 `VCam_Part3` / 改成全村一台 | 产品明确不要 |
| 每帧写 Framing Body / ScreenY | 0831 已否决，会和判定环打架 |
| `VCam_Street` 的 XDamping 0.7、SoftZoneWidth 0.25 | 日常横移不是本案主因 |
| Part3 的 ScreenY 0.88、DeadZoneHeight 0 | 高台纵深产品不变 |
| `CameraArea` 多边形 | 放大只会多露出灰底，不制造过渡 |
| `Village_Shop`、龙宫 `smoothTime` | 无关 |

### 否决方案

- 只挪 Zone、不改 Blend：台阶内侧仍可能用 0.4 秒滑完约 6 单位，边缘体感还是一闪。  
- 只把 `CameraArea` 放大一圈当唯一修复：灰底更宽，贴边时两台照样被钳在新边上，硬切还在。

### 会误伤的其它场景

按推荐方案：**无。** Blend 资产只被本村 Brain 引用；Zone 只在 `Village_KenMuNi1`。
