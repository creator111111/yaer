# Cursor Agent Prompt · KenMuNi1 Part3：双 VirtualCamera 切换落地（替代单机改 Body）

> **角色**：【架构侦探】只读溯源 + 落地改法拍板（本阶段禁止改代码 / 场景）  
> **日期**：2026-08-31  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **产品背景**：单 VCam 运行时 `ApplyFramingTransposerProfile` 切换 Body（ScreenY=0.88 等）**不稳定**——改参挪机位 →「白框 ⊆ 绿框」下一帧翻面 → 狂切 / 冷却后卡死（见 0831 施工返修）。  
> **产品改口（钉死）**：改用 **两台不同 Body 的 VirtualCamera**，由 Brain **切换 / Blend**；**不再**靠改同一台 Framing 参数当主方案。  
> **手感目标不变**：Part3 机 = 用户实测表（ScreenY=0.88、DeadH=0、Bias 0.5/0.5…）；街道路 = 现网默认（DeadH=1、ScreenY=0.5…）。  
> **报告落盘**：`Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 为何单机改 Body 不稳（已证实 · 须写进报告「改口理由」）

```
判定用「当前输出白框 ⊆ Zone」
  → Apply 改 ScreenY / DeadZone
  → 机位立刻/阻尼挪动
  → 下一帧白框几何变了 → 判定翻转
  → 狂切；或标志与真实 Framing 脱节后「再也不切」
```

冷却 / 滞回 / 每帧重申 = **压症状**。产品决定改 **方案 C：双 VCam**。

### 目标架构倾向

| 机 | 职责 | Body |
|----|------|------|
| **VCam_Street**（现有 Virtual Camera 可改名保留） | 右街 / 默认 Live | 街道路 Framing（DeadH=1, ScreenY=0.5…） |
| **VCam_Part3**（新建） | 第三部分高台 Live | 用户表（ScreenY=**0.88**, DeadH=0, YDamp=0, SoftH=0.351, Bias 0.5/0.5…） |

切换手段倾向：

| 手段 | 做法 | 倾向 |
|------|------|------|
| **Priority** | Part3 进区时 Priority 高于 Street；出区降回 | ✅ CM 常规 |
| enabled | 关一台开一台 | ⚠️ 不如 Priority+Blend 平滑 |
| 仍 Apply Profile | 单机改参 | ❌ 本期主方案废弃 |

Brain Default Blend：建议 **EaseInOut 0.3～0.6s**（侦探对拍现网 Brain 设置后定）。

### 判定条件（须拍板 · 打断反馈环）

「白框完全进绿框」若仍用 **当前 Live 输出** 算框，切到 Part3 后白框又变 → **双机也会抖**。

| 方案 | 判定输入 | 倾向 |
|------|----------|------|
| **A1** | 用 **街道路 VCam**（或固定 Street）算出的正交世界 AABB ⊆ Zone | ✅ 推荐：切机不影响判定源 |
| A2 | **玩家**在 Zone / 世界 x≤-93（辅助） | 可作滞回辅条件；产品曾否「仅玩家」为主 |
| A3 | 仍用 Brain.OutputCamera 白框 | ⚠️ 须极强调滞回；易复发 |
| A4 | 仅 Zone Trigger（玩家脚） | ❌ 与「镜头框」产品口不完全一致，仅备选 |

报告必须 **明确推荐 A? **，并写「进/出」两套滞回（避免 Blend 期间 Priority 抖）。

### 现网耦合（侦探必须扫全调用）

`CameraComponent` / `CameraComponentGSM` **当前只认一台** `virtualCamera`：

| API | 风险 |
|-----|------|
| `SetFollow` / `CancelFollow` | 只绑一台 → Part3 机 Follow 为空 |
| `ChangeVirtualCameraShowSize` / Reset | 只改一台 OrthographicSize |
| `ChangeCameraBoundingArea` | Confiner 只打一台 |
| 开场 `SetLock` + `CancelFollow` | 村剧情锁相机可能漏第二台 |
| Zone `SetKenMuNiPart3CameraMode` → Apply Profile | **应改为切 Priority / 激活机**，停用主路径 Apply |

倾向施工契约：

```
SetFollow(player) → Street + Part3 都 Follow=player
Confiner → 两台都绑同一 CameraArea（或共享 Extension）
Zone → 只调 Priority（或启用），不改 Framing 字段
```

### Part3 Body 表（固化到 VCam_Part3 Inspector，勿再运行时刷）

| 字段 | 值 |
|------|-----|
| Screen X / Y | 0.5 / **0.88** |
| Dead Zone W/H/D | 0 / 0 / 0 |
| X / Y Damping | 0.7 / **0** |
| Soft Zone W/H | 0.25 / **0.351** |
| Bias X / Y | **0.5 / 0.5** |
| Camera Distance | 10 |
| Target Movement Only | 是 |

街道路保持现网磁盘默认（DeadH=1, ScreenY=0.5, SoftH≈1…）。

### 与旧文档关系

| 文档 | 关系 |
|------|------|
| 0822 报告方案 C「第二套 VCam」曾 ❌ | **本需求改口启用**；写清「因单机改参反馈环」 |
| 0831 白框判定报告 / 施工 / 返修 | **判定思想可保留**，**写参手段废弃**；冷却重申可删或降级 |
| OPEN_QUESTIONS Part3 条 | 报告末建议新增「双 VCam」条目并改状态 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 双 VCam + Brain 切换落地改法（清单级） | ❌ 本期就改代码（侦探只读） |
| ✅ `CameraComponent` 双机 Follow/Confiner/Size 契约 | ❌ 全村每区都上双机（仅 Part3） |
| ✅ Zone 改为切机；停用主路径 Apply | ❌ 手推机位替代 CM |
| ✅ Blend / Priority / 判定源防反馈环 | ❌ 重做 CameraArea 多边形当跟拍 |

### 严禁

- 继续以「每帧 ApplyFramingTransposerProfile」为 Part3 主方案  
- 只建第二台却 `SetFollow` 仍只绑 Street  
- 用 Live 输出白框判定又切 Live（不解决反馈环）  
- 破坏村开场 `CancelFollow` / `SetLock`  

### 开放问题（报告写入 OPEN 建议）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 判定用 A1 街道路算框还是 A2 玩家区？ | **A1 为主**；A2 可选辅 |
| Q2 | Blend 时长？ | 0.4s EaseInOut 起步 |
| Q3 | 旧 `ApplyFramingTransposerProfile` / `SetKenMuNiPart3CameraMode`？ | 保留 API 但 Part3 Zone **不走**；或内部改成切 Priority |
| Q4 | 第二台是否复制 ImpulseListener / Confiner？ | **要**，与 Street 对齐 |
| Q5 | 场景里旧单机 Profile 序列化字段？ | 可留作文档；运行时以两台 Inspector 为准 |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / Cinemachine / C#。禁止修改任何代码、Prefab、场景、配置。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
Village_KenMuNi1 第三部分相机：
单 VCam 运行时改 Framing Body 不稳定（改参→白框变→判定翻转）。
改为：两台 VirtualCamera（Street Body + Part3 Body），Brain 切换/Blend。
Part3 Body 数值保持用户表（ScreenY=0.88 等）。
Zone 只负责切换哪台 Live，不再主路径 Apply Profile。

## 必读（现网与改口史）
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/VillageCameraDepthFollowZone.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Doc/执行文档/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构溯源报告.md
@Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_架构溯源报告.md
@Assets/Doc/施工说明/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_施工说明.md
@Assets/Doc/OPEN_QUESTIONS.md（KenMuNi1 Part3 相关段）
@Assets/GameRes/Scenes/Village_KenMuNi1.unity（Camera / Virtual Camera / CameraArea / CameraDepthFollowZone_Part3）

全文检索：SetFollow、CancelFollow、ChangeCameraBoundingArea、ChangeVirtualCameraShowSize、
SetKenMuNiPart3CameraMode、ApplyFramingTransposerProfile、CinemachineConfiner、Priority。

## 侦探任务
1. 用链路图说明单机改 Body 反馈环（对照施工返修）。
2. 设计双 VCam 落地图：命名、父子层级、Priority 默认值、Confiner/Follow/Lens 如何共享。
3. 拍板切换判定（必须打断反馈环）：推荐 A1/A2/… 及进出错滞回。
4. 列出 CameraComponent / GSM / Zone / 开场锁相机 的最小改动清单（哪些 API 双写、哪些废弃主路径）。
5. Brain Blend 建议；进出验收表；风险（漏 Follow、漏 Confiner、Blend 中判定抖）。
6. 开放问题写入报告，并给出 OPEN_QUESTIONS 更新建议（改口：启用方案 C）。

## 报告落盘
Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构溯源报告.md

建议结构：
① 结论一句话
② 为何弃单机改参（反馈环）
③ 双 VCam 目标架构
④ 判定与滞回（防反馈）
⑤ CameraComponent 双机契约
⑥ Zone 改切 Priority 的伪代码级说明
⑦ 最小改动文件表
⑧ 验收清单
⑨ OPEN
⑩ 给程序补充

沟通风格：
① 结论一句话
② 原因（通俗）
③ 用户检查清单
④（可选）给程序看的补充
```

---

## 给开发者

1. 新开 Agent，复制上文「侦探 Prompt」整段执行。  
2. 报告拍板后（尤其 Q1 判定源、Q2 Blend）再开施工员。  
3. 施工前可在 Scene 手动复制一台 VCam 调好 Part3 Body 做手感预览，但 **以侦探清单为准再正式接线**。
