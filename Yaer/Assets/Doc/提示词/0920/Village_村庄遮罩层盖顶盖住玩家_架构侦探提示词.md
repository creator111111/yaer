# Cursor Agent Prompt · 村庄遮罩层：盖顶盖住玩家，不动原场景美术

> **角色**：先【架构侦探】只读核对挂点、图层、对齐；报告通过后再【施工员】最小挂载  
> **日期**：2026-09-20  
> **素材根目录**：`Assets/ArtRes/Scene/村庄遮罩/`  
> **产品期望（钉死）**：  
> 1. **不动**场景里已有合层 / 背景 / 道具美术  
> 2. 把对应**遮罩 Prefab**盖上去  
> 3. 遮罩在**最顶层**，必须**盖住玩家**（人走进窗光/树荫下要被压暗）  
> **不是**：改合层散图；改对话 Mask 小头像；改倒树 SpriteMask；挂到夜景场景；先改 `村长家`（无同款 Prefab 时另记）  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_村庄遮罩层盖顶盖住玩家_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「每个 Prefab 挂哪个场景、挂在 Hierarchy 哪、Sorting 用哪一层、Order 要比玩家高多少」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 村民家和村子里都还没加遮罩。新素材已经按名字做好 Prefab 了。  
> 不要动场景里原来的画。只把遮罩盖上去，盖到最上面，人要被盖住。

### 素材 ↔ 场景对照表（助手已对过合层引用；侦探须再 YAML 复核）

| 遮罩 Prefab | 主遮罩图（盖人用） | 目标场景 | 对齐锚点（现网合层） |
|-------------|-------------------|----------|----------------------|
| `村庄遮罩/村民家.prefab` | `村民家/图层 89.png` | `Village_HomeScene1` | `村民家1合层` |
| `村庄遮罩/村民家2.prefab` | `村民家2/图层 145.png` | `Village_HomeScene2` | `村民家2合层` |
| `村庄遮罩/村民家3.prefab` | `村民家3/图层 1.png` | `Village_HomeScene45` | `Map/Design/村民家3合层` |
| `村庄遮罩/村民家4.prefab` | `村民家4/图层 249.png` | `Village_HomeScene23` | `村民家4合层`（曾用名 HomeScene4） |
| `村庄遮罩/肯姆尼1.prefab` | `肯姆尼1/图层 90.png` | `Village_KenMuNi1` | `Map/Design/肯姆尼1合层` |
| `村庄遮罩/肯姆尼2.prefab` | `肯姆尼2/图层 106.png` | `Village_KenMuNi1` | `肯姆尼2合层` |
| `村庄遮罩/肯姆尼3.prefab` | `肯姆尼3/图层 4.png` | `Village_KenMuNi1` | `肯姆尼3合层` / 巨树段 |

**注意**：

- 村民家 **3→HomeScene45**，**4→HomeScene23**。不要按场景文件名数字硬套。  
- 肯姆尼 1/2/3 都在 **同一张** `Village_KenMuNi1`，是三段地图，不是三张场景。  
- `村庄遮罩/村长家.psd`：文件夹里**没有**同款遮罩 Prefab。本期默认**跳过** `Village_Chief_House`，写入 OPEN；不要硬挂 PSD。  
- **不要**挂到 `Village_KenMuNi_night`（夜景另有合层）。

### 挂法约束（产品）

| 项 | 要求 |
|----|------|
| 原场景美术 | **禁止改** Sprite / 合层结构 / 已有 Sorting |
| 新增 | 只新增遮罩 Prefab 实例（或空节点下挂 Prefab） |
| 层级 | 盖在玩家之上；建议对照现网 `TagManager`：玩家在 **Player** 层；前景遮罩可参考倒树「人前」用的 **Effect** 等（侦探钉死层名与 Order） |
| Prefab 内自带 Order | 现网遮罩 Prefab 子物体 Order 多在 **0～3**，**不够**盖住玩家 → 施工须在场景实例上抬高，或只抬「主遮罩」那一层 |

### 禁止

- 本阶段不改代码、不改场景、不改遮罩 Prefab 源（侦探只读）。  
- 不要把遮罩合并进现有合层 Prefab。  
- 不要改商店、夜景、龙宫。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0914/ForestEast_倒树换新合层保留图层_架构溯源报告.md
@Assets/ArtRes/Scene/村庄遮罩/
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/Village_HomeScene2.unity
@Assets/GameRes/Scenes/Village_HomeScene23.unity
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab、贴图。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_村庄遮罩层盖顶盖住玩家_架构溯源报告.md

---

## 背景（策划白话）

村庄遮罩是新素材。村民家和村里都还没挂。
要求：
1. 不动场景里已有的美术合层
2. 把遮罩层盖上去
3. 盖到最顶层，必须盖住玩家

素材在 Assets/ArtRes/Scene/村庄遮罩/。
上面「对照表」是提示词助手预扫，你必须以场景 YAML + Prefab 为准复核；错了就改正表。

---

## 必读 / 优先扫描

### A. 复核对照表

对每一个遮罩 Prefab：

1. 根物体名、子物体里哪一层是真正的「窗光/树荫」主遮罩（通常是 图层 xx，不是「背景」）
2. Prefab 默认 localPosition / Scale 是否按合层对齐做过
3. 目标场景里合层实例的路径与世界坐标；遮罩实例建议挂在哪（例如 Map 下新建 `VillageMask` 空节点，或与 Design 同级）——**不要**塞进合层 Prefab 源

输出一张「施工挂载表」：Prefab 路径 → 场景路径 → Hierarchy 父节点建议 → 对齐方式（跟合层同 XY？还是用 Prefab 自带坐标？）。

### B. 盖住玩家：图层钉死

读 `TagManager` / 玩家实体现网 SortingLayer + Order（至少读 Player Prefab 或场景实例一份）。

回答：

1. 玩家当前 SortingLayer 名与典型 Order  
2. 遮罩要盖住玩家，应改到哪一层、Order 下限建议多少  
3. Prefab 内多子物体（背景 / 护头 / 图层）哪些要抬 Order、哪些其实不该显示（若「背景」是占位全黑，是否应默认关掉以免整屏糊黑）  
4. 参考倒树「人前」层做法，但**不要**做成 SpriteMask 裁切，除非产品改口——本期是盖顶压暗/树荫，不是洞口裁切

### C. 村长家与夜景

- `村长家.psd` 有无可用 Prefab？无 → OPEN 记一条，本期跳过 Chief_House  
- 肯姆尼遮罩是否误挂到 night：明确写「只挂 KenMuNi1 白天」

### D. 推荐一种最小施工

只推一种，并满足：

1. 每个目标场景只新增遮罩实例，不改原合层 Sprite/结构  
2. 进 Play：人走进窗光/树荫，遮罩压在角色之上  
3. 不改移动、相机、对话  
4. 对齐偏差若超过半个窗格，写明是挪遮罩 Transform，还是只调 Order

另外两种各用一句话否决（例如「把遮罩 Sprite 塞进合层 Prefab」「整场景 Sorting 重排」）。

---

## 报告结构（固定）

① 结论一句话（挂哪些场景、图层怎么盖住人）  
② 原因 / 挂载表（大白话 + 全路径表）  
③ 用户需要做什么（逐场景进 Play，人走进窗下/树荫下看是否被盖）  
④ 给施工员的补充：每个场景拖哪个 Prefab、父节点、SortingLayer/Order、不要动什么；村长家是否跳过

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_村庄遮罩层盖顶盖住玩家_架构溯源报告.md
@Assets/ArtRes/Scene/村庄遮罩/
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/Village_HomeScene2.unity
@Assets/GameRes/Scenes/Village_HomeScene23.unity
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告挂载表施工。

硬规矩：
- 禁止修改场景里已有合层的 Sprite、子树结构、原有 Sorting
- 只新增遮罩 Prefab 实例（或报告指定的空父节点下挂实例）
- 遮罩必须盖住玩家（SortingLayer/Order 按报告）
- 不要挂夜景；村长家若报告写跳过就跳过
- 不要改移动、相机、对话脚本

对照提醒（勿写错场景）：
- 村民家3 → HomeScene45
- 村民家4 → HomeScene23
- 肯姆尼1/2/3 → 都在 Village_KenMuNi1

施工说明写入：
Assets/Doc/施工说明/0920/Village_村庄遮罩层盖顶盖住玩家_施工说明.md

完成后用大白话给验收清单：四个民居各走进窗下看一眼；村里三段树荫下各走一遍，确认人被压在遮罩下面。
```
