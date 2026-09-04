# Cursor Agent Prompt · Village_Shop：Head 悬停光标 → **Catch**（施工员）

> **角色**：【施工员】（架构报告已出，用户已选定样式）  
> **日期**：2026-08-29  
> **交互点**：`Village_Shop` · `商店界面合层` → `MerchantPainting` → `Trigger` → **`Head`**（用户 Hierarchy 已高亮）  
> **用户拍板**：悬停进入 Head → 光标变为 **`CursorState.Catch`**（张开的手；按住变握拳）  
> **架构真源**：`Assets/Doc/执行文档/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md`  
> **挂接方案**：报告 §D **方案 A** — 同物体挂现成 `CursorChangeTrigger`  
> **本期不做**：点胸同期挂光标；新贴图/第五态；改对白 Prefab 名；野 `Cursor.SetCursor`

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（施工须遵守）

### 产品需求（钉死）

| 项 | 值 |
|----|-----|
| 区域 | **仅 Head**（Chest 本期不挂） |
| 进 Head | 光标 → **`Catch`**（手张） |
| 在 Head 上按住左键 | 现网中枢会切 **手握**（Catch 自带，勿另写） |
| 出 Head / 热区关闭 | 出队恢复 Normal（或队列下一态） |
| 对白中 | 关热区时 **必须**让光标可靠 Exit，禁止卡在 Catch |

### 最小改动（对齐报告 §E）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `Head` 加 `CursorChangeTrigger`：`TargetState = Catch`，`Priority` 用报告建议（默认 **1**，与村内样例一致） | **P0** |
| 2 | `Village_ShopSceneManager.SetShopkeeperHotspotsEnabled`：**把 `CursorChangeTrigger` 一并 enable/disable**（或关热区时强制 Exit）——报告已点名现网缺口 | **P0** |
| 3 | 场景保存；可选同步 `MerchantPainting.prefab`（报告倾向场景必做，Prefab 同步 P1） | P0 / P1 |
| 4 | Chest | **不做** |

### 严禁

- 业务代码直接 `UnityEngine.Cursor.SetCursor`
- 新建第三套 Update 轮询光标
- 扩 `CursorState` / 换新贴图
- 改 `CursorComponentGM` 中枢逻辑（除非关热区不调 Exit 时必须的最小补丁，且写清原因）
- 把 Catch 挂到 Chest / 整棵 MerchantPainting

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorChangeTrigger.cs
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorComponentGM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/ShopkeeperBodyHotspot.cs

你现在是【施工员】。按 0829 光标溯源报告方案 A，把 Head 悬停光标接到 **Catch**。

## 用户已确认

- 选项：**2 · CursorState.Catch**（手张 / 按住手握）
- 点胸同期挂：**否**
- 第五种新图：**否**

## 必须完成

1. 在场景路径  
   `商店界面合层 / MerchantPainting / Trigger / Head`  
   上添加（若尚无）`CursorChangeTrigger`：
   - `TargetState = Catch`
   - `Priority = 1`（若报告另有建议数，以报告为准）
   - 复用 Head 已有 `Collider2D`（`[RequireComponent(typeof(Collider2D))]` 已满足）

2. 修改 `SetShopkeeperHotspotsEnabled`（或报告指定的等价开关），使禁用热区时：
   - `CursorChangeTrigger` 被禁用，或显式走 Exit，保证对白中 / 关 Collider 后光标不卡在 Catch。
   - 原因写进注释：只关 Collider 时 Trigger 的 Update 仍可能 OverlapPoint 判「仍在头上」。

3. 禁止：直接 SetCursor；改中枢队列算法；挂 Chest；改对白接线。

4. 代码含详细注释；重要取舍说明原因与替代方案（方案 B 仅当 A 无法验收时再提，本期不默认做）。

## 验收（你跑完后在回复里对照写结果）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Idle 鼠标移入 Head | 光标变 **手张（Catch）** |
| 2 | 在 Head 上按住左键 | 变 **手握**；松开回手张 |
| 3 | 移出 Head | 回普通光标 |
| 4 | 在 Head 上点开对白（若点头对白已接） | 对白中光标不卡死；结束后正常 |
| 5 | 对白中热区关闭 | 无「永远手」 |

## 提交说明

改了哪些文件、Head 上组件与序列化字段、热区开关如何带上 CursorChangeTrigger、未做项（Chest / Prefab 源是否同步）。
```

---

## （可选）验收员续跑

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【验收员】。验证 Head 悬停已为 Catch，且对白关热区后光标可恢复。
可加临时日志前缀 `[ShopHeadCursor]`；优先查组件是否挂上、TargetState、热区开关是否 disable 了 CursorChangeTrigger。
输出：通过项 / 失败项 / 剩余风险。
```
