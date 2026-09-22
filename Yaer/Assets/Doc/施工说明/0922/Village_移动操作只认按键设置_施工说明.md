# Village · 移动操作只认按键设置（键族单通道）— 施工说明

**文档版本**：v1.0（2026-09-22）  
**文档性质**：【施工员】方案 A  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0922/Village_移动操作只认按键设置_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**村里左右只认设置里的那一对键，前后由这对键的「键族」推导；另一套键完全不动。村外和村长家室内同一套。**

### ② 原因（通俗）

设置只绑了左/右，但村里还用 Unity 轴和硬编码 WASD∪方向键，所以两套都能走。现在横移只认表里的 Left/Right；设成 A/D 时只用 W/S 前后，设成 ←→ 时只用 ↑↓ 前后。

### ③ 用户检查清单

| # | 设置 Left/Right | 操作 | 期望 |
|---|-----------------|------|------|
| 1 | A / D | 只按 A/D | 左右移 |
| 2 | A / D | 只按 W/S | 上下/纵深 |
| 3 | A / D | 只按 ←→↑↓ | **完全不动** |
| 4 | ← / → | 只按 ←→ | 左右移 |
| 5 | ← / → | 只按 ↑↓ | 上下/纵深 |
| 6 | ← / → | 只按 WASD | **完全不动** |
| 7 | A/D 改成 ←→ 后立刻测 | 再按 WASD | 失效；方向键生效 |
| 8 | 回归 | Forest 跳击、进屋转身 | 不坏 |
| 9 | 村外 KenMuNi1 + 室内 Chief | 上表 1～6 相同 | 同一读键 |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `PlayerInputComponent.cs` | `VillageMoveKeyFamily`；Intent/Sign/Enter 收紧为键族单通道；`RebuildKeyBindingsFromSettings` / `RebuildKeyBindingsOnAllPlayers` |
| `TownPlayerLocomotion.cs` | 纵深 / Home Walk 意图改走推导，去掉裸 `GetAxisRaw` |
| `SettingFormLogic.KeyBindingHelper` | 改键 / 重置后触发 Rebuild |

**未改**：`ControlInputType` Forward/Back；设置 UI 前后槽；场景 YAML；战斗 Default 队列读键；探索禁蹲（S 仍可作 WASD 族后退）。

---

## 键族规则

| 族 | Left/Right | 横移 | 纵深 |
|----|------------|------|------|
| Wasd | A / D | 仅表内 Left/Right | W 上、S 下 |
| ArrowKeys | ← / → | 仅表内 Left/Right | ↑ 上、↓ 下 |
| Custom | 其它 | 仅表内 Left/Right | **0** |

---

## 为什么这样改

公共推导保证室内外一致；关掉 Axis 与双族硬编码才能验收「另一套完全不动」。改键 Rebuild 保证验收项 7。
