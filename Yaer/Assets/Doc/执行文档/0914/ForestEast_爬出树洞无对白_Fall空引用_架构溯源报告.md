# ForestEast · 爬出/过树洞无对白 + TreeBridgeLogic.Fall 空引用 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / 场景 / Prefab / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` · 倒树 / `PassTreeBridgeStoryTrigger`  
**现象**：过树洞后 **无对白**；Console **`UnassignedReferenceException: AttachedGameObject has not been assigned`**，栈 **`TreeBridgeLogic.Fall()` → `TreeBridgeLogic.cs:119`**  
**产品期望**：倒下演出 + `ForestEastScenePassTreeBridge` 台词播完；Console 无该空引用；进/出洞原对白不回归坏  
**不是**：再改 `CanNotSomeActionArea` 当主修；删 Pass 对白；改死羊；回滚贴底；重做倒树合层  
**对照**：0914 洞口爬完卡死施工 ≠ 本案（本案栈已钉 `Fall`）  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_爬出树洞无对白_Fall空引用_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q4

---

## 沟通摘要

### ① 结论一句话

**主因 A：`TreeBridgeLogic.AttachedGameObject[4]` 是 Missing（`fileID: 0`）→ `Fall()` 对 null 调 `SetActive` 抛 Unassigned → Pass 对白链在倒下节点打断，后面「好险…」等台词根本跑不到。**  
遮罩物体 **还在** Hierarchy（现名 **`遮罩只影响人物`**，fileID **`1773642872123289953`**）；旧槽本绑 **`树洞新遮罩`/`399899081`**，合层换图过程中物体被换新 ID，列表槽位未重绑。推荐 **A（重绑遮罩）+ B（Fall/CheckFall 跳过 null）**。与洞口停爬施工 **无关**。

### ② 原因（通俗）

人已经碰到「过完树洞」那条对白触发器了，剧本刚喊倒树倒下，程序要顺手关掉一串挂件，其中有一格是空的，直接报错卡死——后面该说的话全停。把空格重新拖回遮罩，并让代码遇到空引用跳过，对白就能接着播。

### ③ 用户检查清单

| # | 操作 | 现网 / 期望 |
|---|------|-------------|
| 1 | 选中 `Objects/倒树` → `TreeBridgeLogic` → Attached | 第 **5** 项（下标 4）显示 **None/Missing** |
| 2 | Hierarchy 倒树子物体 | 仍有 **`遮罩只影响人物`**（勿当已删） |
| 3 | 新档走到倒树右侧约 **x=342** | 触发 Pass；Console 现网会炸 Fall |
| 4 | 施工后 | **无** Attached 空引用；台词「好险，差点就一起掉下去了」等播完 |

### ④ 程序补充

见下文。施工文档待拍板：`施工说明/0914/ForestEast_爬出树洞无对白_Fall空引用_施工说明.md`。

---

## 1. 「爬出」钉死 = Pass，不是出洞黑幕

| 用户口头 | 物体 | 坐标 / 类型 | 与本案 |
|----------|------|-------------|--------|
| **过完树洞、倒树倒下那句** | `PassTreeBridgeStoryTrigger` → Prefab **`ForestEastScenePassTreeBridge`** | 世界 **(342.14, 0)**；盒 Size≈(1.5×10.13) Offset.y≈−2.03；**`triggerType=1` Enter**；**`SingleUseInArchive=1`** | **主**（调用 `Fall`） |
| 左/右口黑幕送出洞外 | `AutoOutTreeBridge*` | 不调 Fall | 若只有出洞无词且 **无** Fall 报错 → 另票 |
| 洞口外侧进洞词 | `EnterTreeBridge*` | 0914 已施工 | **不是本 Console** |

倒树根 ≈ **(327.07, −7.67)**；Pass 在倒树 **右侧**。用户有 **Fall 报错** → 至少已进入 Pass 图的 `ExecuteFunction Fall` 节点。

---

## 2. Console 硬证据 ↔ 代码

```csharp
// TreeBridgeLogic.cs
public void Fall()
{
    animator.SetTrigger("Fall");
    foreach (GameObject go in AttachedGameObject)
    {
        go.SetActive(false); // L119：go == null → UnassignedReferenceException
    }
}
```

`CheckFall()` 在 `TreeBridgeFall==true` 时同样 `foreach` → `Destroy(go)`，**读档已倒下也会炸**（同缺守卫）。

---

## 3. AttachedGameObject 空槽（YAML 坐实）

现网 `ForestEastScene.unity` → 倒树 `TreeBridgeLogic`（`604494298`）：

| 下标 | fileID | 身份 |
|------|--------|------|
| 0 | `1365474294` | `TreeBridgeStory` |
| 1 | `2107338577` | `GroundCenter` |
| 2 | `1395263753` | `GroundUp` |
| 3 | `862690479` | `GroundDown` |
| **4** | **`{fileID: 0}`** | **Missing ← 炸点** |
| 5 | `1458951152` | `CanNotSomeActionArea` 根 |

### 第 4 槽原本是谁？

| 来源 | 内容 |
|------|------|
| git HEAD | `[4] = {fileID: 399899081}`，物体名 **`树洞新遮罩`**，父=`倒树`，pos≈(−29.47, 6.97) |
| 0914 倒树合层溯源 | 记为 Fall 要关的「遮罩只影响人物」角色（旧 ID `399899081`） |
| **现网工作区** | **`399899081` 整段已删**；同位置职责由新物体 **`遮罩只影响人物`** 承担：fileID **`1773642872123289953`**，仍是倒树子节点，Sprite 已是合层 `8596789c…` |
| 合层施工说明 | 声称「未改 AttachedGameObject」——**意图**是只换图；**结果**是旧遮罩对象被换新 ID 后列表仍指已删对象 → 序列化为 **0** |

**裁定**：不是「遮罩从场景消失」，而是 **列表未重绑新遮罩**。施工应 **把 `[4]` 指回 `1773642872123289953`**，不要只删空槽了事（否则倒下后遮罩可能仍盖人 → 嫌疑 C）。

---

## 4. 无对白 = Pass 链被 Fall 异常打断

`ForestEastScenePassTreeBridge` 图序（NodeCanvas，已核 connections 0→1→2→3→…）：

| 顺序 | 节点 | 内容 |
|------|------|------|
| 1 | Find | `TreeBridgeLogic` + Impulse |
| 2 | Wait | 1s |
| 3 | **ExecuteFunction `Fall`** + 取 `AnimationEventComponent` | **← 现网抛错断点** |
| 4 | 等动画事件 **`FallEnd`** | 到不了 |
| 5 | UI 淡入 | 到不了 |
| 6 | Statement | 「好险，差点就一起掉下去了。」等 | 到不了 |
| 7 | 写存档 `TreeBridgeFall=true` → `CheckFall()` | 到不了 |

因此体感「爬过树洞该说的话没有」= **链断在 Fall，不是 Trigger 没碰到**（有栈即已进 Pass）。  
`SingleUseInArchive=1` 只解释「再测无词」；**解释不了本次报错**。

---

## 5. 嫌疑裁定

| # | 嫌疑 | 裁定 | 证据 |
|---|------|------|------|
| **A Attached[4]=None** | **主因** | YAML `fileID:0` + Console 栈 L119 |
| **B Fall/CheckFall 无 null 守卫** | **同修** | 任意 Missing 再炸断对白 / 读档 Destroy |
| **C 遮罩未关穿帮** | **随 A 补绑消除** | 遮罩物体仍在；只删空槽不关遮罩 |
| **D SingleUse 已消耗** | 次要 | 解释复测；非本次报错 |
| **E 洞口停爬施工** | **无关** | 无 Fall 栈才考虑；本案有栈 |

---

## 6. 推荐方案（A+B）

| 方案 | 做法 | 文件 |
|------|------|------|
| **A（场景）** | Inspector：`AttachedGameObject[4]` ← 拖 **`倒树/遮罩只影响人物`**（`1773642872123289953`）；确认无 None | `ForestEastScene.unity` |
| **B（代码）** | `Fall` / `CheckFall`：`if (go == null) continue;` + 注释（Missing 会掐断 Pass 对白）；可选 `[TreeBridgeFall]` 打 null 跳过次数 | `TreeBridgeLogic.cs` |

**禁止**：改 Pass 文案；拿 `CanNotSomeActionArea` 当主修；整棵换倒树 Prefab；无因果动死羊/贴底。

**仅 B 不补绑**：对白能跑，倒下后遮罩可能仍显示（验收看）——不推荐作默认。

---

## 7. 与 0914 洞口施工关系

洞口票修的是进洞门 **`StopAutoCrawl` / 对白盒坐标**。本案错误栈是 **`Fall` + Attached 空槽**。二者 **无关**；禁止把洞口票当「修了个寂寞」的补丁目标。

---

## 8. OPEN

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | `[4]` 补绑新遮罩还是删空槽？ | **补绑** `1773642872123289953` | 待施工 |
| Q2 | 是否必须加 null 守卫？ | **是**（A+B） | 待施工 |
| Q3 | 合层是否误删旧 `树洞新遮罩` 未重绑？ | 工作区相对 HEAD：旧 ID 删、新遮罩在、Attached→0；施工说明意图未改列表 | ✅ 已坐实 |
| Q4 | 读档 `CheckFall` 是否一并护住？ | **是**（同 foreach） | 待施工 |

---

## 9. 侦探声明

- 未改代码 / 场景 / Git。  
- 用户 Console 截图 + 场景 YAML + Pass Prefab 图序已足以拍板施工；本机未再 Play。
