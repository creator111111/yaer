# Village_KenMuNi1 — `House4 (3)` 改名 `House_Shop` 影响面 — 架构溯源报告

**文档版本**：v1.0（2026-08-27）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改场景/代码**）  
**Unity**：2020.3.48f1  
**村场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**目标物体**：`Objects` → **`House4 (3)`** → 拟改名 **`House_Shop`**  

关联提示词：`Assets/Doc/提示词/0827/Village_KenMuNi1_House4(3)改名House_Shop_架构侦探提示词.md`  
对照样板：`0713/Village_KenMuNi1_Door_Shop换场Village_Shop纯UI_架构溯源与施工执行说明.md`  
换场通则：`Assets/Doc/技术文档/场景相关/场景切换.md`  
命名通则（钉死）：**Hierarchy 物体名 ≠ NextSceneName ≠ SceneName ≠ EnterPos.lastScene**

---

## ① 结论一句话

**可以只改 Hierarchy 名（PrefabInstance `m_Name` override），换场/GSM/`sceneObjs`/EnterPos 都不断——代码与资源无 `"House4 (3)"` 硬编码；最大风险不是技术断链，而是改成 `House_Shop` 后仍进 `Village_House4`，与旁边真·商店门 `Door_Shop→Village_Shop` 名实撞车，测试/策划易误点。另：`Village_House4.unity` 磁盘已缺失，冒烟按 E 可能本来就进不去（与改名无关）。**

---

## ② 原因（通俗）

门牌写在门上叫什么，和「按 E 去哪」是两回事：

| 概念 | 谁管 | 本物体现值 |
|------|------|------------|
| Hierarchy 名 | PrefabInstance `m_Name` | `House4 (3)` |
| 换场目标 | `SceneChangeDoor.NextSceneName` | **`Village_House4`** |
| 真·进店 | 同级 `Door_Shop` 的 `NextSceneName` | **`Village_Shop`** |
| GSM 登记 | `sceneObjs` 的 **fileID** | `2112093714`（不靠名字） |
| 离店回村 | `EnterPos` `lastScene: Village_Shop` → `EnterFrom_Shop` | 与本物体无关 |

改名字 = 只换门牌贴纸；不去改 `NextSceneName`，就不会变成进商店。  
旁边已经有一张真正的商店闸机（`Door_Shop`），再贴一张叫 `House_Shop` 但进民居的门，容易认错。

生活类比：把「4 号楼侧门」牌子改成「商店旁」，路还是通向 4 号楼；真正进商场仍要走旁边写着 `Door_Shop` 的那扇闸机。

---

## ③ 用户检查清单（改名前想清楚 / 改名后冒烟）

| # | 操作 | 通过标准 |
|---|------|----------|
| 1 | 产品确认意图 | **只改可读性**（仍进 House4）→ 可施工；若其实想进店 → **勿用本改名**，另开「接 `Village_Shop`」单 |
| 2 | Hierarchy | 见新名；场景内 **无第二个同名** |
| 3 | 靠近本物体按 E | 行为与改名前一致（目标仍应是 `Village_House4`；Console `[SceneChangeDoor] … next=Village_House4`） |
| 4 | 靠近 **`Door_Shop`** 按 E | 仍进 **`Village_Shop`**（纯 UI） |
| 5 | 商店离店 | 回村落 **`EnterFrom_Shop`**（约 x=-29.04） |
| 6 | Console | 无因 Find 旧名导致的 NullRef；无误改 `Door_Shop` / `EnterFrom_Shop` |
| 7 | （已知债）House4 场景 | 若按 E 报缺场景：属 **`Village_House4.unity` 磁盘缺失**，不是改名造成 |

---

## ④ 给程序：锁定目标 + 引用表 + 方案 + 施工

### A. 目标物体锁定（磁盘已保存）

| 项 | 值 |
|----|-----|
| Hierarchy 路径 | `Objects` / `House4 (3)`（父 Transform `fileID: 1948841490`，`m_Name: Objects`） |
| Prefab | `Assets/Prefabs/Stairs.prefab`（guid **`bf2a028c32ad14c4996c734e17e946b9`**） |
| PrefabInstance | **`&2112093712`** |
| stripped SceneEntity | **`&2112093714`**（进 `sceneObjs`） |
| stripped Transform | **`&2112093713`** |
| 约略坐标 | **(-29.02, 2.44, 0)**；`m_RootOrder: 8` |
| `NextSceneName` | **`Village_House4`**（override；改名**不会**动此字段） |
| 组件链 | Stairs：`SceneEntity` + `SceneChangeDoor` + `Interactive` + Collider；本实例 **Removed** Prefab 的 SpriteRenderer（`7435237835032165235`） |
| Active | PrefabInstance 存在且已序列化；按门常规为可用 |

**对照 · 真·商店门（勿混改）**

| 项 | `Door_Shop` |
|----|-------------|
| PrefabInstance | **`&1457861539`** |
| SceneEntity | **`&1457861541`**（亦在 `sceneObjs`） |
| `NextSceneName` | **`Village_Shop`** |
| 坐标 | **(-29.0394, 2.1124, 0)** — 与 `House4 (3)` **几乎重合**，交互区可能叠 |

**离店落点（勿动）**

| 项 | 值 |
|----|-----|
| GO | `EnterFrom_Shop`（`&5601461779999999001`） |
| EnterPos 行 | `lastScene: Village_Shop` → `pos: {fileID: 5601461779999999002}` |
| 坐标 | **(-29.04, -6.5, 0)** |

### B. 全仓「旧名」引用扫描

搜索：`"House4 (3)"` / `House4 (3)` / 路径片段。

| 位置 | 类型 | 改名后是否必改 | 风险 |
|------|------|----------------|------|
| `Village_KenMuNi1.unity` L19379 `propertyPath: m_Name` / `value: House4 (3)` | YAML 实例名 override | **是（施工本体）** | 低；只改此 value |
| `Assets/Scripts/**` | 代码 Find / 字符串 | **否**（零命中） | 无 |
| `Assets/Prefabs/**`、`*.anim` / Timeline / Animator | 资源路径 | **否**（零命中） | 无 |
| `Assets/Doc/提示词/0827/…` 等文档 | 文档叙述 | 可选后续同步 | 文档债，非运行时 |

**结论**：除场景里这一处 `m_Name` override 外，**无运行时硬编码依赖旧名**。

### C. 换场链路是否依赖物体名

| 环节 | 依赖物体名？ | 依赖什么 |
|------|--------------|----------|
| `SceneChangeDoor` 进门 | **否**（仅 Log 打印 `gameObject.name`） | 序列化 **`NextSceneName`** → `LoadScene` |
| `SceneEntityComponentGSM` | **否** | `sceneObjs` **fileID**（本门 `2112093714`）；`objRoot` 引用 |
| EnterPos 回程 | **否** | `lastScene` **场景字符串** + Transform **fileID** |
| Interactive 按 E | **否** | 组件事件订阅（`OnInit` 时挂） |
| `Village_Shop` GSM | **否**（相对本改名） | 进店只认 `Door_Shop` 的 `NextSceneName`；Find 的是商店内 `"商店界面合层"` |

**结论句**：改 `m_Name` 后，进屋 / 进店 / 离店落点链路 **不断**。

```
Objects/House4 (3)  [Stairs &2112093712]
  → SceneChangeDoor.NextSceneName = Village_House4
  → sceneObjs 含 fileID 2112093714
  → （与 Door_Shop→Village_Shop、EnterFrom_Shop 并行独立）
```

### D. 与 `Door_Shop` 名实冲突裁定

| 问题 | 裁定 |
|------|------|
| `House4 (3)` 当前进哪个场景？ | **`Village_House4`** |
| `Door_Shop` 当前进哪个场景？ | **`Village_Shop`**（0713 已拍板：纯 UI） |
| 改成 `House_Shop` 后会否误以为进商店？ | **会**（命名强暗示 + 坐标几乎贴在一起） |
| 场景内是否已有名为 `House_Shop` 的物体？ | **否**（磁盘无 `m_Name`/`value: House_Shop`） |
| 同族其它门 | `House4 (4)/(5)/(6)` 亦 `NextSceneName: Village_House4`；**本需求勿一并改名** |
| **推荐方案** | **B（优先）**；产品坚持「就叫 House_Shop」且知悉仍进 House4 时可选 **A** |

| 方案 | 说明 | 裁定 |
|------|------|------|
| **A** | `House4 (3)` → `House_Shop`；不动 NextSceneName / Door_Shop / EnterPos | **技术安全**；名实不符，需策划知情 |
| **B（推荐）** | 改中性名，如 `House4_NearShop` / `House_NearShop` | **技术同等安全**；避免与 `Door_Shop` 混淆 |
| **C** | 改名 + `NextSceneName=Village_Shop` | **超出本需求**；等于第二扇进店门 / 抢 `Door_Shop`，须另开施工单 |
| **D** | 不改名 | 功能零风险；Hierarchy 仍丑 |

### E. 若拍板可改 — 最小施工清单（给施工员，侦探不执行）

| # | 动作 | 是否必须 |
|---|------|----------|
| 1 | 仅改 PrefabInstance `&2112093712` 的 `propertyPath: m_Name` → 目标名（A=`House_Shop` / B=中性名） | **是** |
| 2 | 改任何 Find / 字符串引用 | **否**（当前无） |
| 3 | 改 `NextSceneName` | **默认否** |
| 4 | 改 `Door_Shop` / `EnterFrom_Shop` / `Village_Shop` GSM | **默认否** |
| 5 | 改其它 `House4 (*)` | **否** |
| 6 | 保存场景 + 按下表冒烟 | **是** |

YAML 锚点（施工时只动这一处 value）：

```yaml
# Village_KenMuNi1.unity · PrefabInstance &2112093712
- target: {fileID: 7435237835032165234, guid: bf2a028c32ad14c4996c734e17e946b9, type: 3}
  propertyPath: m_Name
  value: House4 (3)   # → House_Shop 或 House4_NearShop
```

### F. 验收清单（改名后冒烟）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Hierarchy 见新名，无重复名 | |
| 2 | 本物体：交互目标仍为 `Village_House4`（Log `next=Village_House4`） | |
| 3 | `Door_Shop`：仍进 `Village_Shop` | |
| 4 | 离店：仍落 `EnterFrom_Shop` | |
| 5 | Console：无缺组件 / Find 旧名 NullRef | |

### G. 改名安全七件套（侦探填完）

| # | 检查项 | 结果 |
|---|--------|------|
| 1 | 磁盘存在 `House4 (3)` | ✅ L19379 |
| 2 | `NextSceneName` 现值；改名不改此字段 | ✅ `Village_House4`；override 独立 |
| 3 | `sceneObjs` | ✅ fileID `2112093714`，不靠名字 |
| 4 | 全仓 `"House4 (3)"` | ✅ 仅场景 YAML + 本需求文档 |
| 5 | `GameObject.Find` / `transform.Find` 按此名 | ✅ Scripts 无命中 |
| 6 | Prefab 覆盖层 | ✅ 仅改实例 `m_Name` override，勿改 `Stairs.prefab` 默认名 |
| 7 | 新名冲突 | ✅ 场景内尚无 `House_Shop` |

---

## 开放问题

1. **产品意图**：是否其实想让该物体改指 `Village_Shop`（与 `Door_Shop` 合并/替换）？本报告默认 **否**；若是，须方案 **C 级**另开单（NextSceneName / 双门职责 / 是否删 `Door_Shop`）。  
2. **若仍指 `Village_House4`**：是否改用 **B 中性名**，避免 `House_Shop` 与 `Door_Shop` 双名混淆？**侦探推荐 B。**  
3. **`Village_House4.unity` 磁盘缺失**：Build Settings 仍登记 `Assets/GameRes/Scenes/Village_House4.unity`，但 `GameRes/Scenes` 目录下 **无此文件**（现有室内为 HomeScene1/2/23/45、Chief、Shop 等）。`House4 (3)/(4)/(5)/(6)` 四扇门 `NextSceneName` 均仍写 `Village_House4`。按 E 冒烟可能 Load 失败——**与本次改名解耦**，建议另记 `OPEN_QUESTIONS` / 另开「House4 场景去向」单（恢复场景 vs 改指 `Village_HomeScene23` 等）。  
4. **交互叠区**：`Door_Shop` 与 `House4 (3)` 坐标几乎重合，改名后更易点错；美术/Collider 是否需错开（非本改名必须项）。

---

## 本阶段严禁（已遵守）

- 未在编辑器/YAML 改名或改 `NextSceneName`  
- 未动 `Door_Shop` / `EnterFrom_Shop` / `Village_Shop` GSM  
- 未把其它 `House4 (*)` 一并改名  
- 未把本门悄悄改成进商店

---

*报告结束。施工员仅在产品拍板 A 或 B 后，按 §E 最小改 `m_Name`。*
)
