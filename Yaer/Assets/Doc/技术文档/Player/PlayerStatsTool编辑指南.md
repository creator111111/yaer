# Player Stats Tool（人物状态调试工具）编辑指南

面向策划、QA 与客户端：在 **Unity 编辑器运行游戏（Play Mode）** 时，快速查找玩家实体、改血改体力、触发常用调试操作。工具窗口标题为 **「Player Stats Tool」**，菜单入口为中文。

---

## 1. 工具定位

| 项目 | 说明 |
|------|------|
| **类型** | `EditorWindow`，仅编辑器存在，不会打进玩家包体。 |
| **源码** | `Assets/Editor/Tool/PlayerStatsEditorWindow.cs` |
| **依赖** | 场景/运行流程中已存在 `PlayerLogic`，且能挂载 `HealthComponent`、`StaminaComponent`。 |

本工具用于 **调试与验收**，不替代正式存档或关卡内的数值配置流程。

---

## 2. 如何打开

1. 在 Unity 顶部菜单选择 **`Tools` → `人物状态调试工具`**。  
2. 弹出独立窗口 **「Player Stats Tool」**。  
3. **建议先进入 Play Mode 再打开**（见下文「查找玩家」逻辑）。

---

## 3. 使用前提

### 3.1 必须：场景中存在 `PlayerLogic`

- 工具通过 **`PlayerLogic`** 访问 `HealthComponent`（血量）与 `StaminaComponent`（体力）。  
- 若当前 Hierarchy / 运行实例中 **没有** `PlayerLogic`，窗口会显示 **「未找到PlayerLogic」**，此时只有 **「Find PlayerLogic」** 按钮可用。

### 3.2 推荐：在 Play Mode 下使用

查找顺序（与代码一致）：

1. **若在播放中**：优先通过 `GameManager.GetGMComponent<EntityComponentGM>()` 取实体管理器，再 **`GetEntityLogic<PlayerLogic>()`**。  
2. **若仍为空**：回退为 **`Object.FindObjectOfType<PlayerLogic>(true)`**（包含未激活对象）。

因此：**正式跑关时**一般应能通过 `EntityComponentGM` 找到玩家；**未播放**或 **GM 尚未初始化** 时，可能只能依赖场景里是否散落 `PlayerLogic`（多数流程下不可靠）。

### 3.3 再次查找

场景切换、玩家销毁重建、或刚进 Play 时尚未生成玩家时，点击 **「Find PlayerLogic」** 可重新解析引用，并 **把滑条同步为当前 HP / 体力数值**。

---

## 4. 界面功能说明

找到 `playerLogic` 后，窗口提供以下控件（顺序与 `OnGUI` 绘制一致）。

### 4.1 「Find PlayerLogic」

- **作用**：重新查找 `PlayerLogic`，并用当前 **`HealthComponent.hp`**、**`StaminaComponent.Stamina`** 刷新内部滑条变量 `HPValue` / `StaminaValue`。  
- **何时用**：换人、重载场景、滑条与界面不同步时。

### 4.2 「修复服装」

- **作用**：调用 `PlayerLogic.FixClothes()`。  
- **效果概要**：清除「衣服破损」相关运行时状态（如 `playerCommonData.ClothesBroken = false`）、同步动画与战斗 UI 上的破损表现。  
- **适用**：衣服破损逻辑卡死、需要立刻恢复为完好状态做对比测试时。

### 4.3 「受到10点伤害」

- **作用**：调用 **`PlayerLogic.TakeDamage(10)`**（不是直接改 `HealthComponent` 字段）。  
- **与滑条改血的区别**：`TakeDamage` 会走玩家侧逻辑，例如 **受伤音效**、**`OnTakeDamage` 委托**、**战斗 UI `PlayerUnderAttack`** 等；更接近真实受伤。  
- **点击后**：工具会把 **`HPValue`** 设为当前 `HealthComponent.hp`，便于滑条与真实血量一致。

**实现细节提示（IMGUI）**：该按钮与下方滑条区域在代码里为 **`if / else`** 关系——**按下按钮的那一帧**不会执行滑条绘制与 `SetData`；下一帧起恢复滑条。一般不影响使用，若需「点伤害的同时立刻拖滑条」，可在一帧后再拖。

### 4.4 血量滑条

- **范围**：`0` ~ 当前 **`HealthComponent.maxHp`**。  
- **行为**：每帧（在未点「受到10点伤害」的分支里）调用 **`HealthComponent.SetData(HPValue, MaxHP)`**，即同时写入 **当前血量** 与 **最大血量（保持原 Max 不变）**。  
- **事件**：`SetData` 在 **hp 数值变化** 时会触发 **`onHpChange`**；与 `TakeDamage` 路径触发的逻辑可能不完全相同，做「纯数值对齐 UI」类测试时注意区分。

### 4.5 体力滑条

- **范围**：`0` ~ 当前 **`StaminaComponent.MaxStamina`**。  
- **行为**：每帧调用 **`StaminaComponent.SetData(StaminaValue, MaxStamina)`**。  
- **注意**：当前 `StaminaComponent.SetData` **不会** 调用 **`OnStaminaChanged`**。依赖体力变化事件刷新 UI 时，可能出现 **条上数值变了但界面未跟新** 的情况；若需完全一致，应在代码侧为 `SetData` 补事件或在本工具中额外通知（属代码改进项，见第 6 节）。

---

## 5. 与正式战斗受伤的关系（速查）

| 操作 | 入口 | 典型用途 |
|------|------|----------|
| 扣 10 点血（带表现） | 「受到10点伤害」 | 测受伤音效、UI 受击、委托链。 |
| 任意血量 | 血量滑条 + `SetData` | 快速对齐到指定血量、测死亡边界（配合 `hp<=0` 逻辑时注意是否还需走 `onHpIsZero` 等）。 |
| 战斗结算伤害 | 游戏内攻击 / `DamageData` | 走 `HasHurt` → `BattleComponent` 等完整管线，**本工具不替代**。 |

若测试 **死亡、复活、存档读档**，仍以关卡与存档系统为准；本工具仅改组件上的数值与部分玩家 API。

---

## 6. 维护与扩展建议（给开发）

以下为可选优化，**非使用工具的必要条件**：

1. **体力 `SetData` 与 `OnStaminaChanged`**：若希望编辑器调条与 UI 完全一致，可在 `StaminaComponent.SetData` 内在值变化时 `OnStaminaChanged?.Invoke(Stamina)`，或工具里在 `SetData` 后手动调 UI 刷新接口。  
2. **`GUILayout.Button` 与滑条的 `if/else`**：可拆成独立 `if (GUILayout.Button(...)) { ... }` 与后续滑条逻辑，避免「点按钮的那一帧不写 SetData」的细微差异。  
3. **非 Play 模式**：当前设计以运行中调试为主；若需在 Edit Mode 调 SerializedObject，需另写编辑器数据通路。  
4. **多玩家 / 本地双人**：`FindObjectOfType` 只取一个实例；若项目存在多 `PlayerLogic`，应改为列表选择或指定引用。

---

## 7. 相关代码索引

| 内容 | 路径 |
|------|------|
| 工具窗口 | `Assets/Editor/Tool/PlayerStatsEditorWindow.cs` |
| 玩家逻辑（受伤、修服装） | `Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs` |
| 血量组件 | `Assets/Scripts/Game/GameRuntime/Entities/Component/Health/HealthComponent.cs` |
| 体力组件 | `Assets/Scripts/Game/GameRuntime/Entities/Component/Stamina/StaminaComponent.cs` |

---

## 8. 常见问题（FAQ）

**Q：窗口一直显示「未找到PlayerLogic」？**  
A：确认已进入 **Play Mode**，且当前流程已创建玩家；点击 **「Find PlayerLogic」** 重试。若项目启动顺序很晚，可晚几秒再点。

**Q：体力条在游戏 UI 上没变？**  
A：见 **4.5**：直接 `SetData` 可能不触发 `OnStaminaChanged`，以实际代码为准。

**Q：改血量后角色没死 / UI 不对？**  
A：`SetData` 与 `TakeDamage` / `AddHp` 等路径触发的副作用不同。特别地，**仅把滑条拖到 0** 时走的是 `HealthComponent.SetData`，当前实现里 **不会** 调用 **`onHpIsZero`**（与 `TakeDamage` 扣到 0 的行为不一致）。需要测死亡流程时请用游戏内伤害或扩展工具单独触发死亡逻辑。

---

*文档版本与 `PlayerStatsEditorWindow` 实现同步；若改动了工具脚本，请同步更新本节与第 4 节描述。*
