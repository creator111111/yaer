# Cursor Agent Prompt · ForestEast 树洞内：主角 Y 再低 1 单位（试探）

> **角色**：【架构侦探】用一页钉死「为何改三块地板人不动」；默认按方案 **A** 进洞只降人、出洞加回来。拍板后【施工员】最小改  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene` → `Map/GroundColliders`（用户红框：`GroundCenter` / `GroundUp` / `GroundDown`）  
> **现象（用户实测）**：改这三块 **对主角位置没什么影响**，进洞后 Y **还是进洞前那档**  
> **产品期望（钉死）**：树洞**里面**主角再 **低 1 个世界单位** 试试（相对**现在进洞后的脚 Y**，不是相对地板盒中心）；出洞后洞外 Y **回到进洞前**，不能整图变矮  
> **不是**：再靠拖这三块地板碰运气；改 `CameraTreeInArea` 冒充人低了；改死羊/吸羊；重写树桥  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_树洞内主角再降1单位Y_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工默认方案 A，除非报告推翻。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 那三块地面我挪了，人还在进洞前的高度。不要再跟地板较劲。进树洞以后把人往下挪 1 格看看对不对齐；出来还要回到外面原来的高度。

### 为何改 `GroundCenter` / `GroundUp` / `GroundDown` 几乎不动人

进洞传送在 `ForestEastTreeEnterTrigger.ChangePlayerPos`（黑幕 `onShowEnd`）：

```csharp
var oldPos = playerLogic.gameObject.transform.position;
playerLogic.gameObject.transform.position = new Vector2(targetObj.transform.position.x, oldPos.y);
```

- 目标点是 `TreeBridgeInPosLeft/Right`（Y 现网 **-6.3**）或出洞 `TreeBridgeOutPos*`（Y 也是 **-6.3**）  
- **只抄目标 X，Y 原样保留 = 进洞前的脚高**  
- 所以用户感觉「还是进洞前的 Y」——**这是代码写死的，不是地板没挪够**

三块盒现网（YAML 预扫，侦探进编辑器再核）：

| 物体 | Layer | Transform Y | 盒高 | 顶面约 |
|------|-------|-------------|------|--------|
| `GroundCenter` | 14 | **-8.6** | 2 | **-7.6**（洞内地板；更早报告曾为中心 -7.6 / 顶 -6.6，现网已降过） |
| `GroundUp` | 13 | **-7.35** | 2 | 多半是洞内**上层/顶**，不是站立面 |
| `GroundDown` | 15 | **-7.85** | 2 | 多半是洞内**下层** |
| 洞外 `GroundLeft_1` | 14 | **-7.6** | （须核盒高） | 顶约 **-6.6** → 与进洞前脚高同档 |

本工程东郊/村 **Gravity 常为 0、Y 当纵深或由层碰撞卡住**。没有「落到地板顶」这一步时，**拖地板不会吸人**。侦探须用一帧日志确认进洞后有没有向下的落地/Raycast。

`GroundUp`/`GroundDown` 与 Center **不同 Layer**：改它们更不会带动 Layer14 的站立脚。

### 进洞后「人看起来没变矮」还可能叠了镜头

0914 已做 **相机贴 `CameraTreeInArea` 底边**（相机中心约 -2.9，Size=5）。那是 **镜头**，不是角色 Transform。本案只动**人的 Y**；镜头先保持贴底。若人降 1 后头顶空/穿帮，记 OPEN，**另开相机案**，不要顺手改边界盒。

### 现网传送点

| 点 | XY（约） |
|----|----------|
| `TreeBridgeInPosLeft` | (264.33, **-6.3**) |
| `TreeBridgeInPosRight` | (335.51, **-6.3**) |
| `TreeBridgeOutPosLeft` | (252.94, **-6.3**) |
| `TreeBridgeOutPosRight` | (342.15, **-6.3**) |

### 方案（施工默认 A，侦探可推翻）

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A（推荐 · 试 1 单位）** | `ChangePlayerPos`：`isEnterTree` 时 `(target.x, oldPos.y - 1f)`；出洞 `(target.x, oldPos.y + 1f)`（或出洞抄 `OutPos.y` 现网 -6.3）。常量 `InTreePlayerYOffset = 1f` + 注释「试探可改」 | 最小；解释得通用户体感；出洞对称 | 蹲爬胶囊与 `GroundUp` 顶可能挤；对白盒 Offset 要抽测 |
| **B** | 进洞抄 InPos **完整 XY**；把两个 InPos 的 Y 改成 **-7.3**（-6.3-1）；出洞抄 OutPos 完整 XY（仍 -6.3） | 落点可在 Scene 里调，不靠魔法数 | 改场景点；须左右口都改 |
| **C** | 再降 `GroundCenter` 并指望落地 | 已证明无效（Y 被 `oldPos.y` 锁住） | **禁止当主方案** |

硬约束：

- **禁止**只改红框三块地板交差  
- **禁止**改 `CameraTreeInArea` / 贴底公式（除非报告证明人降 1 后镜头必穿帮，另案）  
- **禁止**改死羊、倒树图、走廊吃羊  
- 出洞必须恢复洞外高度；左右口都要试  
- 洞口爬行 `CanNotSomeActionArea` / 进洞卡死另案，不要绑进本次偏移

### 侦探须回答（可短）

1. Play 打日志：进洞前 `player.y`、传送后 `player.y`、`target.x/y`、`isEnterTree`。是否 **后 Y == 前 Y**？  
2. 进洞后有无重力/落地把人吸到 `GroundCenter` 顶？没有则 C 彻底排除。  
3. 推荐 A 还是 B？`InTreePlayerYOffset=1` 相对 **oldY** 还是相对 **InPos.y**？（默认相对 **进洞前 oldY**，符合「比现在再低 1」）  
4. 人降 1 后：胶囊 vs `GroundUp`、Enter 对白盒、相机贴底是否立刻坏？坏则 OPEN，先落地 A 再开子任务。

### 必读

1. `ForestEastTreeEnterTrigger.cs` → `ChangePlayerPos` / `GetTargetGameObj`  
2. `ForestEastTreeBridgeStoryMgr.ChangeCamera`（只对照，默认不改）  
3. 场景：In/Out Pos；`GroundCenter/Up/Down`；`GroundLeft_1`  
4. 施工说明：`0914/ForestEast_倒树进洞相机贴边界底边_施工说明.md`  
5. 用户红框截图 + 本提示词  

### 报告必须包含

- 一句话根因：只抄 X、保留进洞前 Y  
- 证据：进洞前后 player.y  
- 方案 A 的精确公式与出洞恢复  
- 验收矩阵  

---

## 【施工员】（侦探不反对 A 即可跑）

> **目标**：洞内主角 Transform.y 比进洞前 **低 1**；出洞恢复。  
> **写入点**：仅 `ForestEastTreeEnterTrigger.ChangePlayerPos` 里赋值那一行（进/出分支）。常量 + 详细注释（为何地板无效、为何出洞要加回、替代方案 B）。  
> **禁止**：改三块 Ground* 当主修复；改相机边界；改 In/Out Pos（除非改走 B 且报告点名）。  
> **文档**：`Assets/Doc/施工说明/0914/ForestEast_树洞内主角再降1单位Y_施工说明.md`  
> **验收**：  
> 1. 左口进洞：人明显低于进洞前约 1 单位（Console 打 before/after y）  
> 2. 洞内可蹲走，不卡死、不穿顶  
> 3. 出洞：脚 Y 回到洞外原档  
> 4. 右口进出各 1 次  
> 5. 进洞镜头仍贴底；对白盒仍能碰到则记通过，碰不到记 OPEN 勿瞎抬 Trigger  
> Debug 前缀：`[TreeBridgePlayerY]`
