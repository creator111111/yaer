# Cursor Agent Prompt · ForestEast 倒树「遮罩只影响人物」：只遮挡主角

> **角色**：先【架构侦探】钉死现网「假遮罩」机制与产品语义；拍板后【施工员】改成真·只影响主角  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene` → `Objects` → **`倒树`** → **`遮罩只影响人物`**（用户 Hierarchy 红箭头）  
> **产品期望（钉死）**：这张遮罩图 **只对主角做遮挡/裁切**；虫卵、木虫、史莱姆、场景装饰、特效等 **一律不受影响**  
> **不是**：删掉遮罩交差；整棵倒树换 Prefab；改树桥进出玩法；顺手大改全图 Sorting；改死羊  
> **对照**：0914 倒树合层报告已写明——现网名字叫遮罩，**实际不是 `SpriteMask`**，只是 Effect 层普通 Sprite **盖在所有人上面**  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_倒树遮罩只影响主角_架构溯源报告.md`

把下面整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实）

### 产品白话

> 倒树上那张「遮罩只影响人物」，现在像一张大贴纸盖住整段洞口，虫和道具也会被盖。我只要它挡住/裁主角，别的东西别被这张图影响。

### 现网是什么（硬事实）

| 项 | 现网 |
|----|------|
| 物体 | `倒树/遮罩只影响人物`，fileID ≈ `1773642872123289953` |
| 组件 | **只有** Transform + **SpriteRenderer**（无 `SpriteMask`） |
| Sprite | 合层 `遮罩只影响人物.png` guid `8596789c…` |
| Sorting | **Effect / Order 0**（在 Player、多数 Monster 之上） |
| `MaskInteraction` | **0 = None** |
| 材质 | Sprites-Default |

语义（0914 合层溯源原文）：名字靠 **画在 Effect 盖住角色** 冒充「只影响人物」，**并没有**按角色做遮罩交互。  
→ Effect 层会盖住 **排序在它下面的一切**（主角 + 洞内卵/虫/特效等）→ 用户要改的正是这个。

`TreeBridgeLogic.AttachedGameObject` 已绑此物体（Fall 时关掉）——改遮罩实现时 **勿弄丢该引用**。

### 产品语义二选一（侦探看图 + Play 裁定，写入报告）

| 语义 | 观感 | 技术倾向 |
|------|------|----------|
| **S1 · 洞形裁主角** | 主角只在树洞轮廓内显示（像钻进洞里），洞外身体被裁掉；卵/虫完整显示 | **`SpriteMask` + 主角 `Visible Inside Mask`**；卵/虫保持 `None` |
| **S2 · 贴纸只盖主角** | 遮罩贴图只叠在主角身上，不盖旁边的虫 | 需 **Stencil / 自定义**（Unity 默认 SpriteMask **不能**「遮罩图只画在角色轮廓上」）；或接受 S1 |

名字「只影响人物」更贴近 **S1**。若美术图是半透明盖层而非洞形 alpha，侦探须写清：S1 是否仍合适，或要 S2。

### 方案对比

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A（推荐 · 真 SpriteMask）** | 在 `遮罩只影响人物` 上加 **`SpriteMask`**（可用同一 Sprite）；**关掉或极透明**原「盖全场」的 SpriteRenderer 染色（避免双画）；主角相关 `SpriteRenderer.maskInteraction = VisibleInsideMask`（或报告裁定 Outside）；怪物/场景保持 **None** | 对齐物体名；卵虫不被盖 | 须列全主角 SR（身体/武器/影子）；换装/昼夜若另有 SR 要纳入；Mask 范围/自定义范围要 Play 调 |
| **B · 只改 Sorting** | 把遮罩挪到仅高于 Player、低于 Monster 的层/Order | 改动小 | Monster1/2 **低于** Player，仍会被盖；**不能**真正「只主角」 |
| **C · Stencil 着色器** | 遮罩图只在主角模板内绘制 | 可做 S2 | 工程量最大；本期不优先 |

**否决 B 作主方案**（层序天生盖不住「只主角」）。  
**默认推荐 A + 语义 S1**；若 Play 证明必须 S2，记 OPEN 再开 C。

### 主角 MaskInteraction 范围（须列清单）

至少扫描：

- `Assets/GameRes/Prefabs/Entity/Player/Player.prefab` 全部 `SpriteRenderer`
- 运行时换装/昼夜是否另挂 SR（有则同设，或统一入口设）
- **不要**改：WormEgg / WoodWorm / Slime / 场景装饰的 MaskInteraction

可选：小工具/一次性 Editor 或进洞时只对当前 Player 设 Interaction、出洞还原——仅当全局改 Prefab 影响其它场景时再用。

### 与倒树其它层关系

| 层 | 是否动 |
|----|--------|
| 内 / 缝 / 光 / 外 | **默认不动**（外仍负责靠近淡出） |
| 遮罩只影响人物 | 本案主改 |
| CollisonMap / SFX / Components | 不动 |
| Fall → Attached 绑遮罩 | **保持绑定**；Fall 后遮罩仍应关掉 |

### 侦探须回答

1. 现网遮罩盖到了哪些非主角（截图：卵/虫被盖）？  
2. 产品语义定 **S1 还是 S2**？依据贴图 alpha。  
3. 推荐 A 的具体：SpriteMask 用哪张图；主角用 Inside 还是 Outside；原 SpriteRenderer 是禁用、Alpha0，还是改成 Mask 专用。  
4. 主角 SR 完整列表；是否影响村内/其它场景。  
5. 方案 ≥2；否决 B 为主因写清。  

### 必读

1. Hierarchy：`倒树/遮罩只影响人物` Inspector（SR / Sorting / MaskInteraction）  
2. `执行文档/0914/ForestEast_倒树换新合层保留图层_架构溯源报告.md` §2.1（假遮罩说明）  
3. `Player.prefab` SpriteRenderer  
4. Unity `SpriteMask` + `SpriteRenderer.maskInteraction`  
5. `TreeBridgeLogic.AttachedGameObject`（勿丢绑）  
6. 用户红箭头截图 + 本提示词  

### 报告必须包含

- 一句话：现网是 Effect 盖全场，不是真遮罩  
- S1/S2 裁定  
- 方案 A 操作清单 + 主角 SR 表  
- 验收：主角被洞形裁/挡；同屏卵虫不被该图挡住  

---

## 【施工员】（拍板后）

> **前置**：报告已定 S1/S2 与 Inside/Outside。  
> **目标**：`遮罩只影响人物` 只作用于主角。  
> **优先**：方案 A（SpriteMask + 主角 MaskInteraction）；最小改倒树其它层。  
> **禁止**：B 当唯一手段；删遮罩；改树桥传送/Fall 逻辑；动死羊。  
> **文档**：`Assets/Doc/施工说明/0914/ForestEast_倒树遮罩只影响主角_施工说明.md`  
> **验收**：  
> 1. 人进倒树区域：遮罩效果只作用在主角  
> 2. 同屏虫卵/木虫/特效不被该遮罩盖住或裁掉  
> 3. 外层靠近淡出仍在；Pass 倒下后遮罩随 Attached 关掉  
> 4. 出树洞/其它场景主角显示正常（若改了 Prefab）  
> Debug：可选 `[TreeMask]` 打印玩家 MaskInteraction 与 SpriteMask.isActive。
