# Cursor Agent Prompt · 修 bug：ChiefPainting Face2/Face3 贴脸偏离

> **角色**：【施工员】最小化把大立绘 Face2/Face3 对齐回 Mask / SR 源（先对表再改）  
> **日期**：2026-09-01  
> **现象**：`ChiefPainting` 下 **Face2 / Face3 完全偏离**（用户 Hierarchy 红箭头指 Face2）  
> **对齐参考（用户钉死）**：  
> 1. `Assets/Prefabs/DialougeProtrait/ChiefMaskPainting.prefab`（UI 小窗正确叠法）  
> 2. `Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘.prefab`（SR 源：组2 / 闭眼 / 笑颜）  
> **修复目标**：`Assets/Prefabs/DialougeProtrait/ChiefPainting.prefab`（及嵌它的门口/继续对话实例若被 Override）  
> **说明落盘**：`Assets/Doc/施工说明/0901/ChiefPainting_Face2Face3贴脸偏离修复_施工说明.md`

把下面「施工」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（施工须核实，勿当唯一真相）

### 叠法语义（钉死）

| 节点 | 图 | 角色 |
|------|-----|------|
| **Face1** | `组 2.png` | **底图全身**（常开） |
| **Face2** | `闭眼.png` | 贴脸层（与 Face3 互斥） |
| **Face3** | `笑颜.png` | 贴脸层 |

脚本：`ChiefMaskPainting.Apply`（大立绘 Prefab 复用同一脚本）。

### 磁盘对照表（助手预扫 · 须证伪）

| 节点 | **ChiefMaskPainting**（应对齐） | **ChiefPainting**（现网） | SR `精灵村长游戏中立绘` |
|------|----------------------------------|---------------------------|-------------------------|
| Face1 Pos | `(0, 0)` | `(0, 0)` | `组 2` local `(5.77, 13.16)` |
| Face1 Size | **`1128 × 2625`** | **`880 × 2048`** ⚠️ | 底图 |
| Face2 Pos | **`(373, 1016)`** | `(373, 1016)` 数同 | `闭眼` `(9.5, 23.32)` |
| Face2 Size | `202 × 76` | `202 × 76` | — |
| Face3 Pos | **`(364, 936.5)`** | `(364, 936.5)` 数同 | `笑颜` `(9.41, 22.525)` |
| Face3 Size | `216 × 209` | `216 × 209` | — |
| 根 Size | `1128 × 2625` | `1128 × 2625` | — |

**预扫推论（H1）**：Face2/3 的 AnchoredPosition **数字已抄 Mask**，但 Face1 **SizeDelta 比 Mask 小一圈**（880×2048 vs 1128×2625）。  
底图变小、贴脸坐标仍按「满框底图」算 → 肉眼就是 **贴脸飞出/错位 =「完全偏离」**。  
勿只改 Face2/3 数字而忽略 Face1 框。

### SR → UI 公式（两 Setup 同源）

```
PPU = 100
BodyLocal = (5.77, 13.160001)   // 组 2
Face2 UI = (闭眼 - Body) * PPU = (373, 1016)
Face3 UI = (笑颜 - Body) * PPU = (364, 936.5)
```

参考：`ChiefMaskPaintingSetupEditor` / `ChiefPaintingSetupEditor` 常量须一致。

### 根因假说

| ID | 假说 | 倾向 |
|----|------|------|
| **H1** | Face1 SizeDelta 未对齐 Mask 满框，导致 Face2/3 相对底图错位 | ✅ 主因 |
| H2 | Face2/3 Pos 被手改/Override 弄坏 | 对表 Mask；数同则次要 |
| H3 | Setup `CreateImageLeaf` 用 `sprite.rect.size` 写 Face1，未强制根框尺寸 | ✅ 回潮源；Mask 现网 Face1 已是 1128×2625（可能手调过） |
| H4 | Pivot/Anchor 不一致 | 预扫均为中心；次查 |
| H5 | 对话 Prefab 实例 Override 了 Face 子节点 | 门口/继续若有子 Override 一并清/同步 |

### 修复倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A** | `ChiefPainting` Face1/2/3 的 Pos/Size/Pivot/Anchor **逐项抄齐** `ChiefMaskPainting`；再用 SR 公式复核 Face2/3 | ✅ |
| **A+** | 改 `ChiefPaintingSetupEditor`：Face1 `sizeDelta` **强制 = 根框 1128×2625**（或与 Mask 一致），勿只信 `sprite.rect.size`；防菜单重跑回潮 | ✅ |
| B | 只手挪 Face2/3 猜坐标 | ❌ 易与 Mask/SR 再漂 |
| C | 改 SR 源 Transform | ❌ 用户要参考它对齐，不是改源 |

同步：嵌 `ChiefPainting` 的 `Village_村长家门口初次对话` / `Village_村长家继续对话`——若子节点有 Rect Override，以母体对齐结果为准清掉错误 Override。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ Face1/2/3 相对叠齐（对 Mask + SR） | ❌ 改表情映射 / CSV FaceType |
| ✅ Setup 防回潮 | ❌ 改雅/古立绘 |
| ✅ 短施工说明 | ❌ 重做 Scale 站位大改（可与 0.65 并存，勿混为一谈） |

### 严禁

- 只调 Face2/3、不核对 Face1 Size  
- 改坏 Sprite guid / 清空贴图  
- 把 Mask 小窗根 Scale(0.18) 抄进大立绘根 Scale  
- 改 SR Prefab 世界坐标当「修 UI」  

### 开放

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | Face1 强制 1128×2625 是否拉伸？ | 对齐 Mask；`preserveAspect` 保持与 Mask 一致 |
| Q2 | 是否同步重跑 Mask Setup？ | **否**（Mask 是参考真理）；只修大立绘 |
| Q3 | 肉眼仍差 1～2px？ | 以 Mask 为准微调，写入说明 |

---

## 施工 Prompt（复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md

## Bug
村长大立绘 ChiefPainting 的 Face2 / Face3 贴脸完全偏离（用户 Hierarchy 红箭头 Face2）。
须参考 Mask 与 SR 源对齐，改回正确叠脸。

## 必读 / 参考（钉死）
@Assets/Prefabs/DialougeProtrait/ChiefMaskPainting.prefab
@Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘.prefab
@Assets/Prefabs/DialougeProtrait/ChiefPainting.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/Editor/ChiefPaintingSetupEditor.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/Editor/ChiefMaskPaintingSetupEditor.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/ChiefMaskPainting.cs

关联对话实例（若有 Face 子 Override 一并处理）：
@Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab

## 核实（改前写入施工说明）
1. 三表对照：Mask vs ChiefPainting vs SR（组2/闭眼/笑颜）的 Pos、SizeDelta、Pivot、Anchor、Active。
2. 确认预扫 H1：ChiefPainting.Face1 Size 是否为 880×2048，而 Mask.Face1 为 1128×2625。
3. Setup 公式 (FaceLocal-BodyLocal)*100 是否与 Mask Face2/3 一致。
4. 对话 Prefab 是否 Override 了 Face 子 Rect。

## 修复目标
1. ChiefPainting 的 Face1/Face2/Face3：Pos / SizeDelta / Pivot / Anchor **对齐 ChiefMaskPainting**
   （Face2/3 同时用 SR 公式复核）。
2. 优先修正 Face1 满框尺寸，再保证 Face2=闭眼、Face3=笑颜贴在脸上，切换无飞脸。
3. 修改 ChiefPaintingSetupEditor，避免再跑菜单把 Face1 写回错误 Size。
4. 禁止改 Mask 当主修；禁止改 SR 世界坐标；禁止动雅/古。
5. Sprite 仍绑：组2 / 闭眼 / 笑颜（与 Mask 同源 guid）。

## 落盘
Assets/Doc/施工说明/0901/ChiefPainting_Face2Face3贴脸偏离修复_施工说明.md
结构：①结论 ②根因对照表 ③改了什么 ④验收 ⑤程序补充
同步 OPEN_QUESTIONS（若有）。

## 验收
- [ ] Prefab 模式：只开 Face1 → 底图正常
- [ ] 开 Face2（关 Face3）：闭眼贴在脸上，不飞到肩膀/虚空
- [ ] 开 Face3（关 Face2）：笑颜贴脸正确
- [ ] Face1 Size 与 Mask 一致（1128×2625 倾向）
- [ ] Face2/3 Pos/Size 与 Mask 一致（或说明经 SR 微调的差值）
- [ ] Play 门口/继续对话：村长句切 Face2/Face3 不偏离
- [ ] 重跑 Setup Chief Painting 后贴脸仍正确（若做了 A+）

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. 直接跑「施工 Prompt」。  
2. 参考真理是 **`ChiefMaskPainting`**；SR Prefab 用来核对闭眼/笑颜相对「组 2」的偏移。  
3. 重点先看 **Face1 是否比 Mask 小**：小了的话 Face2/Face3 坐标再「正确」也会看起来完全歪。
