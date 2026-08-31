# Cursor Agent Prompt · 修 bug：村长家门口初次对话 · 村长大立绘丢失（Sprite 空）

> **角色**：【施工员】最小化修复立绘显示  
> **日期**：2026-08-31  
> **现象**：`Village_村长家门口初次对话` 里 **村长立绘看不到**（Hierarchy 仍有 `村长/ChiefPainting/Face1|2|3`）  
> **助手预扫（须核实）**：  
> 1. 母体 `ChiefPainting.prefab` 三个 `Image.m_Sprite` 均为 **`{fileID: 0}`（空）**  
> 2. 美术夹 `精灵村长游戏中立绘/` **仅有** `组 2/闭眼/笑颜.png.meta`，**未见 png 真文件** → Setup `LoadSprite` 必失败  
> **对照**：`ChiefMaskPainting` YAML 仍写着上述 guid（若 png 已丢，Mask 也可能坏；以磁盘 png 为准）  
> **说明落盘**：`Assets/Doc/施工说明/0831/Village_村长家门口初次对话_村长大立绘丢失修复_施工说明.md`

把下面「施工」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（施工须核实）

### 根因假说

| ID | 假说 | 预扫 |
|----|------|------|
| **H1** | `ChiefPainting` **Image.Sprite 空** | ✅ Face1/2/3 均为 `m_Sprite: {fileID: 0}` |
| **H1b** | 美术 **png 真文件丢失**（只剩 meta） | ✅ 目录 Glob 仅三个 `.meta`，无 `.png` |
| H2 | Setup 空图仍 Save | 与 H1b 连锁 |
| H3 | 对话 Prefab 实例 Override 把 Sprite 清掉 | 次查门口 Prefab 内 ChiefPainting 实例 |
| H4 | CanvasGroup.alpha=0 且前奏未淡入村长一路 | Setup 默认 alpha=0；若仅雅/古淡入会「有节点无图」感——与空 Sprite 可并存 |
| H5 | Active 全关 / Face 叠法错误 | 次要；先修 Sprite |

**不是**：Hierarchy 缺 `ChiefPainting` 节点（用户截图节点在）。

### 正确贴图源（与 Mask 同源）

| 节点 | 文件 | Mask 已用 guid（可对拍） |
|------|------|--------------------------|
| Face1 | `组 2.png` | `ccb45a9fbac00a74aa47b87fff339497` |
| Face2 | `闭眼.png` | `b62f8898d59d41343bad099499ee6d69` |
| Face3 | `笑颜.png` | `e941e610bcd1f724c869806742279d04` |

目录：`Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘/`  
**P0**：先恢复三张 png（保留现有 `.meta` guid，勿换新 meta），来源：`git checkout` / 备份 / 从 `精灵村长游戏中立绘.psd` 导出同名。  
再跑 Setup 或把 Mask/SR 同源 Sprite 写回 `ChiefPainting`。

### 修复倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A0** | **恢复 png 真文件**（同路径、同 meta guid） | ✅ **必须先做** |
| **A** | 重跑 `Setup Chief Painting (UI Big Portrait)`；Console 无「未找到 Sprite」；同步门口 Prefab 实例 | ✅ png 恢复后 |
| B | 从仍可用的 Sprite 引用写回 ChiefPainting（Mask/Library） | 仅过渡；仍须补 png |
| C | 只改对话实例 Override | ❌ |

同时核前奏村长 CanvasGroup 淡入（H4）。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 恢复村长大立绘可见（Sprite + 必要 alpha/Active） | ❌ 重做 Face123 Import / 靠近 Trigger |
| ✅ 对齐 Mask 同源图 | ❌ 改雅/古立绘 |
| ✅ 短施工说明 | ❌ 无关 AB 大重构 |

### 严禁

- 空 Sprite 冒充已修复  
- 只调站位不绑图  
- 把 SR 版再嵌进 Dialogue 容器  

---

## 施工 Prompt（复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md

## Bug
村长家门口初次对话里，村长立绘丢失/看不见。
Hierarchy 仍有：Village_村长家门口初次对话 → … → 村长 → ChiefPainting → Face1/Face2/Face3。

## 预扫（须磁盘核实）
@Assets/Prefabs/DialougeProtrait/ChiefPainting.prefab
→ Face1/2/3 的 Image.m_Sprite 均为 fileID 0（空）。
对照正常：
@Assets/Prefabs/DialougeProtrait/ChiefMaskPainting.prefab
（三脸已绑组2/闭眼/笑颜）

美术目录：
@Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘/
Setup：
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/Painting/Editor/ChiefPaintingSetupEditor.cs
@Assets/Editor/Tool/Dialogue/VillageChiefDoorDialogueSetupEditor.cs
对话成品：
@Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab

## 任务
1. 确认 H1：ChiefPainting Sprite 空；确认 H1b：ArtFolder 是否只有 meta、无 png。
2. **先恢复** `组 2.png` / `闭眼.png` / `笑颜.png`（保留现 meta guid）；再 Import 为 Sprite。
3. 重跑 Setup Chief Painting 或脚本绑回三脸；保证 Image.sprite 非 null。
4. 同步门口对话 Prefab 内 ChiefPainting 实例。
5. 核前奏村长 CanvasGroup 淡入；Active 叠法 Face1 开、Face2/3 关。
6. 落盘：
   Assets/Doc/施工说明/0831/Village_村长家门口初次对话_村长大立绘丢失修复_施工说明.md
   （写明 png 是否曾缺失、从何处恢复）

## 验收
- [ ] ArtFolder 三张 png 真文件存在（不只是 meta）
- [ ] ChiefPainting 三脸 Sprite 非空
- [ ] 门口对话 Prefab 内村长立绘肉眼可见
- [ ] Play：村句大立绘出现；Face1/2/3 有图
- [ ] Mask 小头像、雅/古回归 OK

## 禁止
- 不恢复 png 只重跑空 Setup
- 删除 meta 换新 guid 导致 Mask/SR 全断（除非全库重绑）
- 删除 ChiefPainting 节点当修复

## 沟通风格
①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者（可先手验）

1. 打开文件夹 `Assets/Prefabs/DialougeProtrait/精灵村长游戏中立绘/`：是否只有 meta、**没有** png？  
2. 若缺图：从 git/备份/PSD 把三张 png 放回（**不要删现有 meta**）。  
3. 再跑 **`Tools / Dialogue / Setup Chief Painting (UI Big Portrait)`**，检查 Face1 Sprite 非空。  
4. 或把「施工 Prompt」交给 Agent。
