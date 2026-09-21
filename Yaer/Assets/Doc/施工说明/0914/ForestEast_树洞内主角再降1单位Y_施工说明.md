# ForestEast 树洞内 · 主角再降 1 单位 Y — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】按侦探报告方案 **A**（进洞 oldY−1 / 出洞 oldY+1）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/ForestEast_树洞内主角再降1单位Y_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**进树洞后脚会比刚才再低 1 格；出洞加回去。再拖洞里那三块地板不会让人变矮。**

### ② 原因（通俗）

进洞黑幕结束时，程序只把人挪到洞内落点的左右位置，高度故意沿用洞外站着的高度。角色又没有重力，不会掉到地板上。所以拖 `GroundCenter` / `GroundUp` / `GroundDown` 人不会跟着走。现在只在传送那一行：进去减 1，出来加 1。镜头贴底没动。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 左口进洞 | 人明显矮一档；Console `[TreeBridgePlayerY] enter=True` 且 after ≈ before − 1 |
| 2 | 洞内蹲走 | 不卡死、不穿顶 |
| 3 | 左口出洞 | `enter=False` after ≈ 进洞前洞外脚高 |
| 4 | 右口进出各一次 | 同上 |
| 5 | 进洞镜头 | 仍贴 `CameraTreeInArea` 底边 |
| 6 | Hierarchy 红框三块地 | **不必再拖**；改了也不该是本票手段 |

若头顶空、穿帮或对白盒碰不到：记现象，**不要先抬地板 / 改相机边界**。

### ④ 程序补充

见下文。

---

## 改动清单

只改 `ForestEastTreeEnterTrigger.ChangePlayerPos` 赋值。

```
进洞：afterY = oldPos.y - InTreePlayerYOffset   // 默认 1
出洞：afterY = oldPos.y + InTreePlayerYOffset
pos = (InPos/OutPos.x, afterY)
```

常量 `InTreePlayerYOffset = 1f`，试探只改这一处。

### 未改

- `GroundCenter` / `GroundUp` / `GroundDown` / `GroundLeft_1`  
- `TreeBridgeInPos*` / `OutPos*` 场景点  
- `CameraTreeInArea` / 贴底公式  
- 倒树 / 死羊 / 洞口爬行案  

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 再拖三块地板 | **否** |
| Q2 抄 InPos 完整 XY（方案 B） | **否** |
| Q3 人降后改贴底/边界 | **否**（穿帮另开相机案） |
| Q4 出洞抄 OutPos.y=-6.3 | **否**（用 +offset 恢复洞外原档） |

---

## 替代方案（未采用）

- **B** 抄落点完整 XY：要改场景四点；OutPos=-6.3 与洞外 ≈-6.6 不一致  
- **C** 再降地板：无重力吸附，已证无效  

---

## 剩余风险

- 胶囊降 1 后更可能顶到 `GroundUp`（层 13）；挤出再 OPEN，勿先抬地板。  
- Enter 对白盒底约 -7.55，一般仍能碰到；碰不到记 OPEN，勿瞎抬 Trigger。  
- 人低了构图可能变空，贴底另案。  
- 本机未 Play；请按清单看 Console `[TreeBridgePlayerY]`。
