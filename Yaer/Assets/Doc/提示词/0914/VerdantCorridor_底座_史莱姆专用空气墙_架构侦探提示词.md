# Cursor Agent Prompt · VerdantCorridor 底座：史莱姆专用硬空气墙（通用组件）

> **角色**：先【架构侦探】只读拍板方案；拍板后【施工员】落地通用组件 + 场景挂载  
> **日期**：2026-09-14  
> **场景 / 挂点**：`VerdantCorridor` → `Map` → `Design` → `Near` → `Part5` → **`底座`**（用户 Hierarchy 红箭头）  
> **产品期望（钉死）**：  
> 1. 在 **底座** 位置加一道 **空气墙**（隐形硬挡）  
> 2. **只对史莱姆生效**：玩家 / 其他怪 / 掉落物 / 场景交互 **不挡**  
> 3. **硬性**：史莱姆 **无法穿过**，含 **玩家击退 / 击飞 / 冲撞位移**，不能被速度或 `MovePosition` 穿模过去  
> 4. 做成 **通用组件**：别处（别的场景/别的障碍物）可复用，不写死「底座」名字  
> **不是**：挡玩家；挡木虫/天琬/羊等非史莱姆；改史莱姆 AI/数值；改全局 Physics2D 矩阵当首选；复用 `MapLimit` 当地图边空气墙误挡全员  
> **对照 / 勿混**：  
> - `MapLimit` + Editor「生成空气墙」= **地图边界** Polygon，对谁碰取决于 Layer，**不是**「只挡史莱姆」方案  
> - 0723/0913：`GroundCld`→OnlyMapObj + Trigger 是「怪不挡人」，与本案「墙挡怪」方向相反  
> - `VillageWalkObstacleCollisionBootstrap` = 村庄障碍矩阵策略，**勿**拿来硬套战斗走廊  
> **报告落盘**：`Assets/Doc/执行文档/0914/VerdantCorridor_底座_史莱姆专用空气墙_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 苍翠走廊「底座」那儿要加一道看不见的墙，只拦史莱姆。  
> 史莱姆自己爬过去不行，被玩家打飞/击退撞上去也穿不过去，要硬挡。  
> 玩家和其他怪随便过。组件要写干净，以后别的地方也能挂。

### 为何不能「随便挂个 Collider + OnlyMapObj」

| 点 | 现网线索（须核实） |
|----|-------------------|
| 史莱姆站地图靠 | `BaseMonster`：`groundCld.layer = onlyMapObjLayer(7)` |
| 木虫/天琬等 | 同样有 `GroundCld` → OnlyMapObj |
| 若墙层只与 OnlyMapObj 硬碰 | **所有**带实心/参与碰撞的 OnlyMapObj 都会被挡 → **不是「只挡史莱姆」** |
| 史莱姆位移 | `MoveComponent` 走 `Rigidbody2D.MovePosition` / `AddForce`；击退可能走同一套 → 薄墙 + Discrete 可能 **穿模**，硬挡要确认 Collision Detection / 厚度 / 或脚本权威夹紧 |
| 史莱姆 GroundCld | 0913 后多为 **Trigger**（不挡人）→ **Trigger 不产生硬解算**；若只靠墙↔GroundCld 硬碰，可能 **根本挡不住**（须查实际 isTrigger + 根物体 Layer） |

→ 侦探必须先回答：**史莱姆身上哪几个 Collider / 哪个 Layer / 是否 Trigger / 哪个 Rigidbody，才能被「硬墙」挡住？** 再选方案。

### 术语钉死

| 词 | 本案含义 |
|----|----------|
| **空气墙** | 无美术可见体（可留 Gizmo），有碰撞或等价硬约束的阻挡体 |
| **只对史莱姆** | 判定对象为 `Slime` / `ISlime`（或报告确认的史莱姆实体身份），**不含**其它 Monster |
| **硬性** | 主动移动、被击退/击飞、冲量位移后，史莱姆中心/脚 **不得越过墙的禁侧**；不允许「闪一下穿过去」 |
| **通用组件** | 挂任意场景物体即可；尺寸/朝向用 Collider 或序列化框配置；不依赖 `底座` 节点名；可多实例 |

### 现网预扫（须核实）

```
挂点：
  VerdantCorridor/Map/Design/Near/Part5/底座
  （当前 Hierarchy 在 InitScene 下展开 VerdantCorridor 子树；正式 Play 场景以 Build/AB 加载的 VerdantCorridor 为准）

已有「空气墙」名：
  MapLimit（EdgeCollider2D + PolygonCollider2D，MapAirWallIndex）
  Editor MapEditorWindow「生成空气墙」→ 填 Polygon 路径
  → 用途=地图边，非史莱姆筛选

史莱姆碰撞分层（BaseMonster.OnInit 倾向）：
  root.layer     → Monster1/2/3（随 GroundType）
  bodyCld.layer  → atkCheckLayer(19)
  groundCld.layer→ OnlyMapObj(7)；Slime 侧多已 isTrigger=true

位移：
  MoveComponent.MovePosition(rg.MovePosition)
  击退/击飞路径须顺着 Wound / Damage / 状态机追一遍（侦探补全）

对照组件风格：
  VillageWalkObstacleCollisionBootstrap（矩阵策略样例，勿照搬村庄）
  LayerName.cs（常量层名习惯）
```

### 嫌疑 / 方案优先级（侦探排，施工勿自选）

| # | 方向 | 直觉利弊 |
|---|------|----------|
| **A** | **专用 Layer + 矩阵只碰史莱姆相关层** | 干净；但史莱姆 GroundCld=Trigger 时可能仍不硬碰；且 Monster 层可能含非史莱姆 → 易误伤 |
| **B（常更贴「只挡史莱姆」）** | **墙 Collider 默认与万物 IgnoreCollision，运行时仅对史莱姆的非 Trigger 盒 / 指定盒 Un-ignore**；或反过来：只对 `ISlime` 的 Collider 建立碰撞对 | 筛选准；要处理生成/死亡/对象池生命周期 |
| **C** | **Trigger 检测 + 每帧/Fixed 权威夹紧**（穿透分离 / 禁止越过平面）挂在通用组件上，身份用 `GetComponentInParent<ISlime>` | 不依赖矩阵；击退穿模也好控；须写清与 `MovePosition` 的时序（FixedUpdate 顺序） |
| **D** | **A/B 硬碰 + C 夹紧双保险** | 最硬；代码稍多；推荐作「击退必过」底线时采用 |
| **E（否决作首选）** | 改全局 Physics2D 矩阵大面积开关 | 影响面大；0723 已多次否决「矩阵当首选」 |
| **F（否决）** | 复用 `MapLimit` 直接挡 | 会挡非史莱姆或挡玩家，语义不符 |

### 复现 / 验收矩阵（侦探须填「现网」；施工后验收勾）

| 操作 | 期望 |
|------|------|
| 史莱姆从墙一侧走向底座方向 | 贴墙停住，不穿 |
| 玩家把史莱姆朝墙方向击退 / 击飞 | **不穿墙**；可沿墙滑或贴墙停 |
| 玩家走向 / 穿过底座空气墙位置 | **无阻挡** |
| 同场景木虫（若有）穿过该墙位置 | **无阻挡** |
| 史莱姆死亡 / 尸体 / 掉落 | 不因墙产生异常（卡尸体、飞掉等）；报告裁定是否仍挡尸体 |
| 组件挂到另一空物体试一次 | 行为一致（通用性） |

### 侦探须回答

1. 史莱姆 **哪套 Collider + Rigidbody** 能参与「硬挡」？GroundCld 已 Trigger 后，墙↔GroundCld 是否无效？  
2. 击退 / 击飞最终写的是 `velocity` / `MovePosition` / 直接改 `transform`？会不会 Discrete 穿薄墙？  
3. 「只对史莱姆」推荐 **B / C / D** 哪条？为何不选 A/E/F？  
4. 组件 API 草案：类名、命名空间、序列化字段（尺寸、单向/双向、Gizmo色）、生命周期（谁注册 IgnoreCollision）。  
5. 场景挂载：子物体挂在 `底座` 下 vs 与 `底座` 同级；Collider 尺寸如何对齐美术底座（侦探给建议，不强制改美术）。  
6. 开放问题：死后尸体是否仍挡、是否挡睡眠史莱姆、多只史莱姆同时顶墙等 → 不清则写 `OPEN_QUESTIONS.md`。

### 必读

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md`  
3. `BaseMonster.cs`（layer / groundCld / bodyCld）  
4. `Slime.cs`（GroundCld Trigger 注释与实现）  
5. `MoveComponent.cs`（MovePosition / AddForce）  
6. 击退相关：玩家攻击 → 史莱姆 Wound / Damage 状态（侦探定位具体文件）  
7. `MapLimit.cs`、`MapEditorWindow.GenerateAirWall`（划清边界，勿混用）  
8. `LayerName.cs`、`ProjectSettings/TagManager.asset`（层名清单）  
9. 本提示词 + 用户 Hierarchy 截图（`Part5/底座`）

### 禁止（侦探阶段）

- 禁止改代码 / Prefab / 场景 / Git  
- 禁止首推改全局 Physics2D 矩阵  
- 禁止把方案写成「只改底座名字的临时 if」  
- 禁止空气墙挡玩家或其他非史莱姆怪（除非 OPEN 另批）

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读。

### 输出

`Assets/Doc/执行文档/0914/VerdantCorridor_底座_史莱姆专用空气墙_架构溯源报告.md`

结构：

1. **结论一句话**（推荐方案字母 + 能否硬挡住击退）  
2. **调用链**（史莱姆移动 / 击退 → Collider/Layer → 为何现网能穿底座）  
3. **证据表**（史莱姆各盒 Layer/Trigger；Move/击退写位置方式；MapLimit 差异）  
4. **影响面**（走廊其它怪、玩家、尸体、对象池）  
5. **方案 ≥2** + **推荐** + 组件职责边界 + 场景挂载步骤草案 + 验收清单  
6. 不清处记 `Assets/Doc/OPEN_QUESTIONS.md`

回答风格对齐主提示词：①结论 ②原因白话 ③检查清单 ④程序补充。

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：报告已推荐方案（预期偏 **B/C/D** 之一）并确认击退不会穿模策略。  
> **目标**：  
> 1）新增 **通用**「仅史莱姆硬空气墙」组件（详细注释；说明替代方案）；  
> 2）在 `VerdantCorridor` 的 `Part5/底座` 处挂好实例（子物体命名清晰，如 `SlimeAirWall`），Collider/尺寸对齐底座阻挡意图；  
> 3）玩家与其它怪可穿；史莱姆移动+击退均不能穿。  
> **质量**：  
> - 不写死场景名/「底座」字符串逻辑；  
> - 不在 `Update` 堆无关业务；固定步长逻辑放 `FixedUpdate`（若用夹紧）；  
> - 生成/销毁史莱姆要正确注册/注销碰撞对（若用 IgnoreCollision）；  
> - 编辑器 Gizmo 画出墙体，方便摆放；  
> - 常量层名走 `LayerName` 或局部清晰常量，忌魔法数字。  
> **禁止**：改全局矩阵当唯一手段；挡玩家；改史莱姆伤害/AI；顺手大改 `MapLimit`；一次性重写目录。  
> **文档**：`Assets/Doc/施工说明/0914/VerdantCorridor_底座_史莱姆专用空气墙_施工说明.md`  
> **验收**：按上文复现矩阵逐条过；至少 1 次「组件挂到临时空物体」证明通用。

---

## 【验收员】Prompt（可选）

> Debug 前缀建议 `[SlimeAirWall]`：Awake 打 Layer/IsTrigger/尺寸；史莱姆靠近时打是否 IgnoreCollision、接触点、夹紧前后位置；击退帧打位移 delta 是否越过墙平面。  
> 输出：通过项 + 剩余风险（薄墙穿模、尸体、对象池漏注册、OnlyMapObj 误伤）。
