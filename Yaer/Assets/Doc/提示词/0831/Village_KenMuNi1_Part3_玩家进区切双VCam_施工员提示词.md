# Cursor Agent Prompt · KenMuNi1 Part3：判定改回「玩家进区」切 VCam_Part3

> **角色**：【施工员】最小化改判定；必要时先只读核对 Priority / Follow（勿写长侦探报告）  
> **日期**：2026-08-31  
> **场景**：`Village_KenMuNi1` · `Map/CameraDepthFollowZone_Part3`  
> **产品再改口（钉死）**：  
> - **保留**双 VirtualCamera（`VCam_Street` + `VCam_Part3`）+ Brain Priority / Blend  
> - **废弃**「街道路白框完全 ⊆ 绿框」（A1）作进区条件——实测难切上 Part3  
> - **改为**：玩家进入该区域（**玩家位置**在 Zone 内）就切到 `VCam_Part3`；离开则回 Street  
> **本阶段**：改 Zone 判定为主；禁止再开单机 `ApplyFraming` 主路径  
> **说明落盘**：`Assets/Doc/施工说明/0831/Village_KenMuNi1_Part3_玩家进区切双VCam_施工说明.md`

把下面「施工」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（施工须核实）

### 改口对照

| | 现网（双机 + A1） | 本需求 |
|--|------------------|--------|
| 切机手段 | Priority → Part3 Live | **不变** |
| Body | 两台 Inspector 写死 | **不变** |
| 进区条件 | Street 正交框 **完全 ⊆** Zone | **玩家在 Zone 内** |
| 出区条件 | 框离开（外扩滞回） | **玩家离开 Zone**（可带小滞回） |

**为何改**：A1 过严——人已在高台，镜头白框常有一边在绿盒外 → `wantPart3` 长期 false，体感「切不到 VCam_Part3」。玩家位置判定简单、可预期，且**不**复活「改 Body→白框变→狂切」反馈环（因为不再运行时改 Framing）。

### 判定实现倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **P1 · bounds.Contains(玩家根坐标)** | LateUpdate / CameraUpdated 读 `PlayerLogic.transform.position` | ✅ 简单；与读档补检友好 |
| P2 · Trigger + `PlayerFoot` | OnTriggerEnter/Exit | ✅ 亦可；注意 Layer/刚体 |
| P3 · 继续 A1 白框全进 | — | ❌ 产品已否 |

滞回：进出各差 **0.2～0.5** 世界单位（或内缩/外扩盒）即可；**可去掉**专门为「改 ScreenY」准备的长冷却，或保留 ≤ Blend 时长（0.4s）防 Priority 连翻。

### 不要动（除非坏了才修）

| 保留 | 说明 |
|------|------|
| `CameraComponent` 双写 Follow / Confiner / Size / CancelFollow | 双机契约 |
| `SetKenMuNiPart3CameraMode` → Priority | 切机入口 |
| Part3 Body（ScreenY=0.88 等）Inspector | 不改回 Apply |
| Zone 几何右缘 ≈ -93 | 避免右街误切；若盒不对只微调场景，不改判定哲学 |
| `KenMuNi1_StreetPart3_Blends` | 名依赖 `VCam_Street` / `VCam_Part3` |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ Zone 主条件改为玩家进/出 | ❌ 删掉第二台 VCam |
| ✅ 小滞回；日志可保留 | ❌ 恢复每帧 ApplyFraming / Reassert Body |
| ✅ 短施工说明 + 更新 OPEN 改口 | ❌ 重做 CameraArea / 开场锁相机大改 |

### 严禁

- 用「白框完全进入」继续当主条件  
- 进区又改同一台 Framing 参数  
- `SetFollow` 只绑 Street、漏 Part3  
- Zone 再扩到盖住右街主道  

---

## 施工 Prompt（复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / Cinemachine / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
Village_KenMuNi1 Part3：
保留 VCam_Street + VCam_Part3 与 Priority/Blend。
进/出条件从「街道路白框完全 ⊆ Zone」改为「玩家进入/离开该区域」（用玩家位置判断）。
人进绿区就应能切到 VCam_Part3；人出区回 Street。

## 必读
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/VillageCameraDepthFollowZone.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponentGSM.cs
@Assets/Doc/施工说明/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_施工说明.md
@Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md（Part3 双 VCam / 白框判定相关段）

## 施工步骤
1. 改 VillageCameraDepthFollowZone：主条件改为玩家在 Zone 内（Contains 玩家 transform 或 PlayerFoot Trigger，二选一写清原因）。
2. 删除或旁路 IsStreetFrustumFullyInsideZone 作为主路径；注释标明产品改口。
3. 保留 TrySetPart3CameraMode → SetKenMuNiPart3CameraMode（Priority）；禁止恢复 ApplyFraming 主路径。
4. 进出加小滞回；冷却若保留，对齐 Blend，勿过长导致「进了区半天不切」。
5. 落盘：
   Assets/Doc/施工说明/0831/Village_KenMuNi1_Part3_玩家进区切双VCam_施工说明.md
   并建议更新 OPEN_QUESTIONS：判定改回玩家进区。

## 验收
- [ ] 玩家走进 CameraDepthFollowZone_Part3 → Live 切到 VCam_Part3（ScreenY=0.88 手感）
- [ ] 玩家走出 Zone → 回 VCam_Street
- [ ] 边界来回多次：能反复切，无明显狂切、无「再也不切」
- [ ] 右街主道（约 x>-93）不被误切
- [ ] Play 后两台 Follow 仍为 Player；开场锁相机契约不坏
- [ ] 不再依赖「白框完全进绿框」才能切机

## 禁止
- 删双机改回单机改 Body
- 大范围重构 CameraComponent
- 无说明扩大 Zone 盖右街

## 沟通风格
① 结论一句话 ② 原因 ③ 用户检查清单 ④（可选）程序补充
```

---

## 给开发者

1. 新开 Agent Mode，复制上文「施工 Prompt」整段发送。  
2. Play：走进左翼高台绿区 → 应 Blend 到 Part3；走出 → 回 Street。  
3. 若仍不切：先查 `virtualCameraPart3` 引用、Priority、两台 Follow，再查 Zone 盒是否 ind 到角色。
