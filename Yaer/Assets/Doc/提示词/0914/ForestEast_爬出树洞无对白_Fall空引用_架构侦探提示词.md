# Cursor Agent Prompt · ForestEast 爬出/过树洞无对白 + TreeBridgeLogic.Fall 空引用（复验）

> **角色**：先【架构侦探】用 Console 钉死因果；拍板后【施工员】最小修（空槽 + Fall 空安全 + 对白链续跑）  
> **日期**：2026-09-14  
> **严重度**：恶性 — 用户验收「修了个寂寞」；洞口票施工后 **爬出/过洞仍无对白且 Console 报错**  
> **现象（用户实测 + Console 截图）**：  
> 1. 爬出树洞之后 **仍不触发对话**  
> 2. Console：**`UnassignedReferenceException: The variable AttachedGameObject of TreeBridgeLogic has not been assigned`**  
> 3. 栈：`TreeBridgeLogic.Fall()` → **`TreeBridgeLogic.cs:119`**（`go.SetActive(false)`）  
> **产品期望（钉死）**：过树洞倒下演出 + **后续对白完整播完**（`ForestEastScenePassTreeBridge`）；Console **无**该空引用；出洞/进洞原对白不回归坏  
> **不是**：再改 `CanNotSomeActionArea` 当主修；删 Pass 对白；改死羊；回滚贴底相机；重做倒树合层  
> **对照**：0914 洞口爬完卡死施工（进洞门停爬）≠ 本案；本案错误栈已指向 **`Fall` + AttachedGameObject**  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_爬出树洞无对白_Fall空引用_架构溯源报告.md`

把下面整段交给 Cursor Agent。

---

## 提示词助手预梳理（侦探须核实，但空槽已是硬证据）

### 产品白话

> 好不容易爬过树洞了，该说话的时候没词，Console 还报倒树 Fall 空引用。上次修洞口等于白修。对着报错把 Missing 引用补上或跳过，让倒下后的对白跑完。

### Console 硬证据（用户截图，勿再猜）

```
UnassignedReferenceException:
The variable AttachedGameObject of TreeBridgeLogic has not been assigned.
You probably need to assign the AttachedGameObject variable ...
TreeBridgeLogic.Fall ()  (at .../TreeBridgeLogic.cs:119)
```

现网 `Fall()`：

```csharp
public void Fall()
{
    animator.SetTrigger("Fall");
    foreach (GameObject go in AttachedGameObject)
    {
        go.SetActive(false); // ← 第 119 行；go == null 必炸
    }
}
```

### 场景 YAML 已坐实空槽（助手预扫）

`ForestEastScene` → 倒树 `TreeBridgeLogic.AttachedGameObject`：

| 下标 | fileID | 现网身份（预扫） |
|------|--------|------------------|
| 0 | `1365474294` | `TreeBridgeStory` |
| 1 | `2107338577` | `GroundCenter` |
| 2 | `1395263753` | `GroundUp` |
| 3 | `862690479` | `GroundDown` |
| **4** | **`{fileID: 0}`** | **Missing / None ← 炸点** |
| 5 | `1458951152` | `CanNotSomeActionArea` 根 |

0914 倒树合层溯源曾写：列表含 **遮罩只影响人物**（旧记 fileID `399899081`）。现网第 4 槽已是 **0**——引用丢了（Missing）。合层施工说明声称「未改 AttachedGameObject」，但 **盘上就是空槽**；侦探用 Inspector 确认 Missing，并查 git 谁弄丢。

`CheckFall()` 里同样 `foreach` 会 `Destroy(go)`，读档已倒下时也会炸。

### 对白为何「没有」——与 Fall 的因果

`PassTreeBridgeStoryTrigger`（约 **x=342.14**，Enter，`SingleUseInArchive=1`）→ Prefab **`ForestEastScenePassTreeBridge`**：

图序预扫（须再核 connections）：

1. Find `TreeBridgeLogic` / Impulse  
2. Wait 1s  
3. **`ExecuteFunction TreeBridgeLogic.Fall`** + 取 `AnimationEventComponent`  
4. 等动画事件 **`FallEnd`**  
5. UI 淡入 → 台词（「好险，差点就一起掉下去了」等）  
6. 写存档 `TreeBridgeFall=true` → `CheckFall()`

→ **Fall 一抛异常，NodeCanvas 链在倒下节点打断**：后面台词 / `FallEnd` 等待 / 存档写入都可能停。用户体感 = **爬过树洞该说的话没有 + 报错**。  
这与「洞口外侧进洞对白」是 **另一条 Trigger**；本案以 Console 为准，主战场是 **Pass + Fall**。

### 「爬出树洞」钉死（侦探须用坐标确认）

| 用户口头 | 可能物体 | 与本案关系 |
|----------|----------|------------|
| 过完树洞、倒树倒下那句 | `PassTreeBridgeStoryTrigger` → `ForestEastScenePassTreeBridge` | **主**（调用 Fall） |
| 左/右口黑幕送出洞外 | `AutoOutTreeBridge*` | 不调 Fall；若只有出洞无词且无 Fall 报错 → 另票 |
| 洞口外侧进洞词 | `EnterTreeBridge*` | 0914 已施工；**不是本 Console** |

用户本次 **有 Fall 报错** → 至少走到了 Pass 图的 Fall 节点。

### 嫌疑优先级（本案）

| # | 嫌疑 | 裁定倾向 |
|---|------|----------|
| **A（主）** | `AttachedGameObject[4]=None` → `Fall` NRE/Unassigned → Pass 对白链断 | **已有 YAML+栈** |
| **B** | `Fall` 无 null 守卫；列表任意 Missing 都会再炸 | 与 A 同修 |
| **C** | 空槽本应是「遮罩只影响人物」；Fall 后遮罩未关导致穿帮 | 补绑或确认可删槽 |
| **D** | Pass `SingleUse` 已消耗，再测无对白 | 解释「再进无词」；**解释不了本次报错** |
| **E** | 洞口停爬施工导致出洞锁/盒 | 无 Fall 栈则弱；有栈则次要 |

### 方案（推荐 A+B）

| 方案 | 做法 | 说明 |
|------|------|------|
| **A（场景）** | Inspector 删掉 Attached 里 **None**；若遮罩仍在 Hierarchy，**重新拖回**第 4 槽（或合层后的「遮罩只影响人物」） | 治本：Fall 不再踩空 |
| **B（代码）** | `Fall` / `CheckFall`：`if (go == null) continue;` + 短注释 | 防再丢引用再炸断对白 |
| **C** | 只加 null 不补遮罩 | 对白能跑；倒下后遮罩可能仍显示（验收看） |

硬约束：

- **禁止**再拿洞口 `CanNotSomeActionArea` 当本票主修（除非取证证明出洞仍锁且无 Fall 报错）  
- **禁止**删 `ForestEastScenePassTreeBridge` 台词交差  
- **禁止**为修空引用整棵换倒树 Prefab  
- 活卵/进洞贴底无因果勿动  

### 侦探须回答（可短）

1. 用户「爬出」是否 = 碰到 `PassTreeBridge`（x≈342）？人坐标。  
2. Inspector `AttachedGameObject` 第 4 槽是否 Missing？原本是否遮罩？  
3. Fall 抛错后 NodeCanvas 停在哪一节点（Fall / FallEnd / Statement）？  
4. 推荐 A+B；补绑还是删空槽？  
5. 与 0914 洞口施工关系：无关写明。

### 必读

1. `TreeBridgeLogic.cs` `Fall` / `CheckFall`  
2. 场景倒树 Inspector `AttachedGameObject`  
3. `ForestEastScenePassTreeBridge.prefab`（Fall → FallEnd → 台词）  
4. `PassTreeBridgeStoryTrigger`  
5. `执行文档/0914/ForestEast_倒树换新合层保留图层_架构溯源报告.md`（Attached 含遮罩记载）  
6. 用户 Console 截图 + 本提示词  

### 报告必须包含

- 空槽下标与应绑物体  
- Pass 图调用 Fall 的节点证据  
- 「无对白」= 链被异常打断（不是 Trigger 没碰到，除非坐标证明没进 Pass）  
- 方案 A+B 文件列表  

---

## 【施工员】（拍板后立刻可跑；证据已够）

> **目标**：`Fall()` 不再抛 UnassignedReferenceException；Pass 对白台词播完；存档 `TreeBridgeFall` 仍按原图写入。  
> **写入**：  
> 1. `ForestEastScene.unity`：清掉 `AttachedGameObject` 的 `fileID: 0`；遮罩还在则重新赋值  
> 2. `TreeBridgeLogic.Fall` / `CheckFall`：foreach 跳过 null，注释说明为何（Missing 会掐断 Pass 对白）  
> **禁止**：改 Pass 文案；改洞口停爬当主修；动死羊。  
> **文档**：`Assets/Doc/施工说明/0914/ForestEast_爬出树洞无对白_Fall空引用_施工说明.md`  
> **验收**：  
> 1. 新档穿过树洞踩 `PassTreeBridge` → **无** Attached 空引用  
> 2. 倒下动画 + **「好险…」等台词**完整  
> 3. 存档倒下后重进：`CheckFall` 不炸  
> 4. 抽测左口进洞对白仍在  
> Debug：`[TreeBridgeFall]` 打印 Attached 数量与 null 跳过次数。

---

## 【验收员】

> 复现必看 Console：**0** 条 `AttachedGameObject has not been assigned`。  
> 若仍无词但无报错：再查人是否碰到 Pass 盒 / SingleUse；**另开票**，勿再改 Fall。
