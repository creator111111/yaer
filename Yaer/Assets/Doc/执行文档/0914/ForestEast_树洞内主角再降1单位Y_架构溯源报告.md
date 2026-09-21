# ForestEast 树洞内 · 主角 Y 再低 1 单位 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / 地板 / 相机 / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` → `Map/GroundColliders`（用户红框：`GroundCenter` / `GroundUp` / `GroundDown`）  
**现象**：改这三块地板，进洞后主角 Y **仍像进洞前那档**  
**产品期望**：洞**内**脚 Y 比**现在进洞后**再低 **1** 世界单位；出洞恢复进洞前洞外高度；**不**整图变矮  
**不是**：再拖三块地板碰运气；改 `CameraTreeInArea` 冒充人矮；改死羊/倒树；重写树桥  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_树洞内主角再降1单位Y_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q4  
**施工默认**：方案 **A**（本报告**不推翻**）

---

## 沟通摘要

### ① 结论一句话

根因是进洞传送 **只抄目标 X、Y 原样保留进洞前脚高**（`ChangePlayerPos`），不是地板没挪够。Player **`gravityScale=0`**，不会落到 `GroundCenter` 顶。推荐 **方案 A**：进洞 `oldY - 1`，出洞 `oldY + 1`，常量 `InTreePlayerYOffset = 1f`；**禁止**再以拖红框三块交差。相机贴底先不动。

### ② 原因（通俗）

进树洞时黑幕一结束，程序只把人挪到洞内左右落点的 **横坐标**，高度故意沿用洞外站着的高度。所以你怎么拖洞里那三块地，人都不会跟着掉下去——本来就没写「落到地板上」。镜头贴底是另一套，改的是相机不是脚。要对齐，就在传送那一行把进洞时的脚主动减 1，出来再加回去。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网 |
|---|------|------|
| 1 | 再拖 `GroundCenter/Up/Down` | **无效**（Y 被代码锁） |
| 2 | 拍板后只改 `ChangePlayerPos` 赋值 | 进 −1 / 出 +1 |
| 3 | 左/右口各进一次 | Console `[TreeBridgePlayerY]` before/after |
| 4 | 出洞 | 脚 Y 回到洞外原档 |
| 5 | 镜头 | 仍贴底；穿帮另开相机案 |

### ④ 程序补充

见下文。

---

## 1. 根因（程序向）

写入点：`ForestEastTreeEnterTrigger.ChangePlayerPos`，黑幕 `onShowEnd`：

```csharp
var oldPos = playerLogic.gameObject.transform.position;
playerLogic.gameObject.transform.position =
    new Vector2(targetObj.transform.position.x, oldPos.y);
```

| 项 | 现网 |
|----|------|
| `targetObj` 进洞 | `TreeBridgeInPosLeft/Right`，Y 都是 **-6.3**（**未使用**） |
| `targetObj` 出洞 | `TreeBridgeOutPosLeft/Right`，Y 也是 **-6.3**（**未使用**） |
| 传送后 player.y | **== 进洞前 oldPos.y**（代码保证；本会话未 Play，逻辑自证） |
| 用户体感 | 「还是进洞前的 Y」= **符合现网设计** |

---

## 2. 为何改三块地板不动人

| 物体 | Layer | Transform Y | Size | 顶面约 | 角色站立？ |
|------|-------|-------------|------|--------|------------|
| `GroundCenter` | **14** GroundCenter | **-8.6** | (56.1, 2) | **-7.6** | 洞内地板掩体；**不吸人** |
| `GroundUp` | **13** GroundUp | **-7.35** | (56.1, 2) | -6.35～-8.35 | 洞内上层/顶，**不是** Layer14 站立面 |
| `GroundDown` | **15** GroundDown | **-7.85** | (56.1, 2) | 同量级下层 | 同上 |
| 洞外 `GroundLeft_1` | 14 | **-7.6** | (289.7, 2) | **-6.6** | 洞外脚高同档来源 |

- Player Prefab：`Rigidbody2D` **Dynamic**，**`m_GravityScale: 0`** → 无「落到 Center 顶」这一步。  
- 落地检测 `CapsuleGroundChecker` 只回答「脚下有没有地面」，**不把人吸到盒顶**；跳跃 Y 由动画/脚本写，不是物理重力。  
- 改 Up/Down **不同 Layer**，更不会带动 Center 上的脚。  
→ **方案 C（再降地板）彻底排除。**

---

## 3. 与镜头的关系

0914 进洞：`ChangeCamera(true)` → Size=5 + `SnapLiveOrthoYToConfinerFloor`（相机中心约 **-2.9**）。  
那是 **VCam**，不是角色 Transform。本案 **只动人的 Y**；贴底先保持。人降 1 后若头顶空/穿帮 → OPEN，**另开相机案**，不改 `CameraTreeInArea`。

---

## 4. 方案对比

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A（推荐）** | 进：`(target.x, oldY - InTreePlayerYOffset)`；出：`(target.x, oldY + InTreePlayerYOffset)`；`InTreePlayerYOffset = 1f` + 注释「试探可改」 | **默认施工**。相对 **进洞前/出洞前的 oldY**，符合「比现在再低 1」 |
| **B** | 进洞抄 InPos 完整 XY，InPos.y 改成 **-7.3**；出洞抄 OutPos 完整 XY（-6.3） | 可用，但要改场景四点；且 OutPos=-6.3 **未必等于**洞外真实脚高（常 ≈-6.6） |
| **C** | 再降 Ground* | **禁止** |

### 方案 A 精确公式

```
进洞 isEnterTree==true:
  afterY = oldPos.y - InTreePlayerYOffset   // 默认 1
  pos = (InPos.x, afterY)

出洞 isEnterTree==false:
  afterY = oldPos.y + InTreePlayerYOffset // 对称加回
  pos = (OutPos.x, afterY)
```

日志建议：`[TreeBridgePlayerY] enter={0} before={1} after={2} target=({3},{4})`

出洞用 `+ offset` 而不是抄 `OutPos.y`：产品要的是 **恢复进洞前洞外高度**；洞外约 -6.6，OutPos.y 写死 -6.3，抄完整 OutPos 会偏。

写入范围：**仅** `ChangePlayerPos` 赋值那一行（进/出同一处用 `isEnterTree` 分支）。常量 + 详细注释（地板为何无效、为何对称加减、替代 B）。

可选：改用 `playerLogic.SetPos` 与别处一致（现网已是裸写 Transform，跟现网一致即可；Rb 同步若有穿帮再 OPEN）。

---

## 5. 风险与验收

| 风险 | 说明 | 处理 |
|------|------|------|
| 胶囊 vs `GroundUp` | 人降 1 后 Body 更大概率顶到层 13 盒 | Play 抽测；挤出再 OPEN，**勿先抬地板** |
| Enter 对白盒 | 盒底约 -7.55；脚再低仍靠高交互盒，一般仍相交 | 碰不到记 OPEN，勿瞎抬 Trigger |
| 相机贴底 | 人低了构图可能变 | 另案；本期不改贴底 |
| 洞口爬行卡死 | 另案，**不绑**本偏移 |

### 验收矩阵

| # | 操作 | 期望 |
|---|------|------|
| 1 | 左口进洞 | afterY ≈ beforeY − 1；人明显矮一档 |
| 2 | 洞内蹲走 | 不卡死、不穿顶 |
| 3 | 左口出洞 | afterY ≈ 洞外进洞前 |
| 4 | 右口进出 | 同上 |
| 5 | 进洞镜头 | 仍贴底 |

---

## 6. 替代方案说明

| 路径 | 说明 |
|------|------|
| **权威 A** | 传送时相对 oldY ±1，最小、可试探改常量 |
| **B 场景点** | 可调 InPos，但 OutPos=-6.3 与洞外 -6.6 不一致 |
| **C 地板** | 无重力吸附，已证无效 |

---

## 7. 参考路径

| 用途 | 路径 |
|------|------|
| 传送 | `ForestEastTreeEnterTrigger.ChangePlayerPos` |
| 相机（对照） | `ForestEastTreeBridgeStoryMgr.ChangeCamera` |
| 贴底施工 | `Assets/Doc/施工说明/0914/ForestEast_倒树进洞相机贴边界底边_施工说明.md` |
| 落点 | `TreeBridgeInPos*` / `OutPos*` |
| 地板 | `GroundColliders/GroundCenter|Up|Down`、`GroundLeft_1` |
