# Cursor Agent Prompt · 出商店回村：相机移动 + 闪屏（对照龙宫 Stairs）

> **角色**：【架构侦探】只读；对照序章下楼（龙宫 Stairs）已修案，查「出店回村」是否同一契约缺口  
> **日期**：2026-09-20  
> **触发**：从 `Village_Shop` 离开回村（ESC / 离开按钮 → `ExitShopToVillage` → `Village_KenMuNi1`）  
> **现象（用户）**：从商店出来有**摄像机移动 + 闪屏**，和序章下楼问题一样  
> **产品期望（钉死）**：黑幕揭开时镜头已在店门外落点机位；**无闪帧、无可见滑动/追镜头**（定格）  
> **不是**：改日常村里左右走跟拍手感（那是 0919 横向阻尼案）；改进店招呼对白；改回村落点表到别处；全局改所有场景 `smoothTime`  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_Shop_出店回村相机闪滑_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没对拍 Stairs 修复契约、没测清「回村场景当前 smoothTime / 默认 VCam ↔ EnterFrom_Shop 位移」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 出商店回村时，黑幕一开镜头会闪一下、还会挪一下，跟龙宫楼梯上下楼那时一样。  
> 先读当时怎么修的，再看出店是不是同一条缝；不要另发明一套镜头系统。

### 龙宫 Stairs 已定案（必读，当对照，勿当出店现网）

报告：`Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0912/HomeScene1_2_Stairs换场相机定格_施工说明.md`

| 项 | Stairs 结论 |
|----|-------------|
| **主因** | 进场 `SetFollow(forceSnap=true)` + 目标场景 `smoothTime≈0.3` → 手推 VCam（Follow 暂空）；黑幕只 hold **0.3s** 就 `CloseFormFade`，**手推未收束就露景** |
| **闪 vs 滑** | 同源；位移小偏「闪」，位移大（如下楼 Δx≈20）偏「可见滑动」 |
| **已排除** | 落点配错、剧情二次改相机（Stairs 路径） |
| **修复（方案 A）** | 目标场景 `CameraComponent.smoothTime = 0` → 进场当帧定格；**未**改公共换场代码 |
| **未采用** | 方案 B：黑幕等 `SetFollow.onComplete` 再揭幕（改公共契约，面太大） |

调用链摘要（出店应对齐核对）：

```
换场门 / ExitShop
  → LoadScene(..., blackFade)
  → 新场景 InitPlayer → SetPlayerPos(EnterPos)
  → CameraComponentGSM.SetFollow(player)  // forceSnap 默认 true
       · smoothTime > 0：多帧 SmoothDamp 手推  ← 露景窗口内 = 闪/滑
       · smoothTime ≈ 0：当帧定格
  → Ready 后 hold ≈0.3s → CloseFormFade  ← 错误机位窗口从这里打开
```

### 出店现网线索（预扫，须再证）

| 项 | 预扫 |
|----|------|
| 离店 API | `Village_ShopSceneManager.ExitShopToVillage` → `LoadScene(Village_KenMuNi1)`（ESC 与离开按钮同源） |
| 回村落点 | `EnterPosConfig` · `lastScene=Village_Shop` → `EnterFrom_Shop`（约 -29, -6.5） |
| 目标场景相机 | `Village_KenMuNi1.unity` 磁盘仍有 **`smoothTime: 0.3`**（Stairs 修的是龙宫 HS1/HS2，**没改村里**） |
| 村里双机 | KenMuNi 有 Street / Part3 双 VCam；日常跟拍是 Framing，**进场手推仍走 CameraComponent.smoothTime** |

预判（可证伪）：出店闪/滑 = **同一契约缺口**，目标场景换成 `Village_KenMuNi1`；Δ 取决于「场景默认 VCam 位 ↔ EnterFrom_Shop」有多大。

### 不要和这些案子混

| 案子 | 为何不是本案 |
|------|----------------|
| 0919 村里横向走抖 | 日常 XDamping，不是换场揭幕 |
| Forest 林恩收束 | 后来产品否决整场景 `smoothTime=0` 作运镜终态；本案产品要的是 **换场定格**，对齐 Stairs，不是运镜 |
| 进店 ShopStart/Repeat 防闪 Bar | 进店黑幕内对白；本案是 **出店回村揭幕** |

### 禁止

- 本阶段不改代码、不改场景、不改 Prefab。  
- 不要建议改全项目 `CameraComponent` 默认值。  
- 不要把日常村里走路跟拍改成瞬切来「顺便」消闪。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md
@Assets/Doc/施工说明/0912/HomeScene1_2_Stairs换场相机定格_施工说明.md
@Assets/Doc/执行文档/8月/0829/Village_Shop_ESC退出商店回村_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_Shop_出店回村相机闪滑_架构溯源报告.md

---

## 背景（策划白话）

从商店出来回村时，摄像机会移动并闪屏。
开发者说这和序章下楼（龙宫 HomeScene2 → HomeScene1 Stairs）是同一类问题。

龙宫案已修：进场 SetFollow 手推 + 黑幕揭太早；修法是目标楼层 CameraComponent.smoothTime=0，揭幕当帧定格。

这次查：出店回 Village_KenMuNi1 是不是同一条缝；若是，最小改法是否仍是「只改回村场景的 smoothTime」，还是村里双 VCam / 落点导致必须换方案。

产品期望：出店黑幕揭开时镜头已在店门外正确机位，无闪、无可见滑动。

---

## 必读 / 优先扫描

### A. 先对拍 Stairs 契约（证明同源或证伪）

对照 0912 报告，把出店链画成同一张表：

| 检查项 | Stairs（已知） | 出店回村（现网） |
|--------|----------------|------------------|
| 换场入口 | Stairs SceneChangeDoor | ExitShopToVillage / 离开按钮 |
| 目标场景 | HomeScene1 / 2 | Village_KenMuNi1 |
| blackFade / hold | true / ≈0.3s | ? |
| InitPlayer → SetFollow forceSnap | 是 | ? |
| 目标 CameraComponent.smoothTime | 已改为 0 | 磁盘预扫仍 0.3？须 YAML 复核 |
| 默认 VCam 世界坐标 | 有表 | 写出 Street（及若进场先亮的机） |
| 落点 | Stairs EnterPos | EnterFrom_Shop（复核坐标） |
| 默认机位 ↔ 落点 Δx/Δy | 下楼 Δx≈20 | 算出数字 |

结论必须写死：
- **同源**（同一 SetFollow 手推 + 早揭幕），或
- **不同源**（另有二次 SetFollow / Part3 切机 / 出店 stayAction 干扰）——若不同源，写清断点，不要硬套 Stairs。

### B. 出店路径有没有「额外」改相机

读 `ExitShopToVillage`、回村后 `OnEnterScene`、是否还有：
- 商店残留 CancelFollow / SetLock
- 进村剧情旗导致再 CancelFollow
- Part3 Zone 进场当帧切 Priority 造成二次跳

这些若存在，标为加重因素，不要和主因糊成一句。

### C. 推荐一种最小改法

优先对照 Stairs：

| 方案 | 做法 | 何时采用 |
|------|------|----------|
| **A（Stairs 同款）** | 仅 `Village_KenMuNi1` 的 `CameraComponent.smoothTime=0` | 同源且日常跟拍仍靠 CM Damping（Stairs 验收：走路阻尼仍正常） |
| B | 公共换场等 onComplete 再揭幕 | 仅当 A 不够且报告证明必须改契约 |
| C | 只挪默认 VCam 靠近 EnterFrom_Shop | 缩小滑幅但不消灭契约缝；一般否决为唯一修复 |

只推一种。必须回答：

1. 改 `Village_KenMuNi1` 的 smoothTime=0，会不会误伤村里日常横移、Part3 纵深（对照 Stairs「只影响 forceSnap 手推」的结论是否仍成立）
2. 要不要动 `Village_Shop` 场景相机（出店是卸店加载村，预判 **不用** 改店内 smoothTime）
3. 为何不把 Forest「禁止 smoothTime=0」套到本案（产品语不同：换场定格 vs 演出运镜）

### D. 验收矩阵（写入报告给用户）

| # | 操作 | 期望 |
|---|------|------|
| 1 | 村里进店 → ESC 或离开回村 | 揭幕无闪、无可见大滑 |
| 2 | 再进店再出 ≥2 次 | 同上 |
| 3 | 回村后只左右走 | 日常跟拍仍顺（若 A 后抖了，另记，勿 silently 接受） |
| 4 | （对照）龙宫 Stairs 上下 | 仍定格，不被本案改坏 |
| 5 | Console | 可见离店 LoadScene / 回村落点日志；无异常 |

---

## 报告结构（固定）

① 结论一句话（是否与 Stairs 同源 + 改哪个场景的哪个字段）  
② 原因（大白话 + 出店调用链 + 与 Stairs 对照表）  
③ 用户需要做什么（出店看闪/滑；Hierarchy 看 KenMuNi1 smoothTime）  
④ 给施工员的补充：改哪些文件、不要改哪些、推荐方案、否决方案

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_Shop_出店回村相机闪滑_架构溯源报告.md
@Assets/Doc/施工说明/0912/HomeScene1_2_Stairs换场相机定格_施工说明.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告推荐方案做最小改。对照龙宫 Stairs：优先场景序列化 smoothTime，不要改公共换场契约，除非报告点名必须改。

目标：
- 出商店回村，黑幕揭开时镜头定格在 EnterFrom_Shop 机位，无闪、无可见滑动
- 村里日常走路跟拍不被毁掉
- 龙宫 Stairs、进店对白管线保持原样

限制：
- 禁止全局改 CameraComponent 默认 smoothTime
- 禁止用 Update 野推相机
- 若方案是 KenMuNi1 smoothTime=0：注释/施工说明写明「只影响进场 forceSnap 手推，日常 Follow 仍走 Cinemachine」
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_Shop_出店回村相机闪滑_施工说明.md`

完成后用大白话给验收清单：进店→离开回村看揭幕；回村左右走看跟拍；抽测龙宫楼梯仍定格。
```
