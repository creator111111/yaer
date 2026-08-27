# Village_Shop — 老板娘表情变化系统 — 架构溯源报告

**文档版本**：v1.0（2026-08-27）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码/Prefab/场景**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_Shop.unity`（纯 UI · 进店 `Door_Shop`）  
**美术源**：`Assets/ArtRes/Scene/Village/商店界面合层/`  

关联提示词：`Assets/Doc/提示词/0827/Village_Shop_老板娘表情变化系统_架构侦探提示词.md`  
进店真源：`0713/Village_KenMuNi1_Door_Shop换场Village_Shop纯UI_…`  
双轨冻结：`0704/商店界面合层转UI组件_…`（§施工冻结 · **勿**复活整页搬进 ShopPanel）  
下期演出：`0629/商店系统_策划拆解_执行说明.md` §4（**本期不做**）  
台本参考：`0601/Village_商店老板娘特殊交互_对白台本_执行说明.md`（FaceType 语义 · 图集待建）

---

## ① 结论一句话

**推荐方案 B（商店专用切脸器）：本期在进店真源「世界空间 `商店界面合层`」的 `正常体` / `表情1` 两个 `SpriteRenderer` 上挂 `ShopkeeperFaceController`，用 `SetFace` / `SetBody` 换 Sprite；可独立 Debug 验收 5 表情，不依赖对话。勿走雅儿 `StoryFormPainting` / `DialogueFaceType` 链。`ShopPanel.prefab` 虽有 `ShopkeeperLayer` 但 Play 不可见，仅 P2 镜像同步。**

---

## ② 原因（通俗）

### 2.1 商店脸 ≠ 雅儿对话立绘

```
通用对话（雅/古莎）：
  SayEx / CSV FaceType
    → DialogueFaceType
    → StoryFormPainting.UpdateFace(Faces 子物体名)
    → DialogueMaskAvatarPresenter

商店老板娘（现网）：
  场景里「海报合层」SpriteRenderer：正常体 + 表情1
  UI_Shop 只管买卖 Bar，不管老板娘脸
  ≠ GoOutStoryYaerPainting 那条链
```

硬塞进 `DialogueFaceType` 会把 5 张独立 PNG 与雅儿图集键名搅在一起；0601 台本里的 `Smile`/`Angry` 是**语义名**，不等于 `表情1.png` 文件名。

### 2.2 玩家 Play 时脸画在哪（侦探钉死）

| 载体 | Play 是否可见 | 老板娘层 | 备注 |
|------|---------------|----------|------|
| **`商店界面合层`**（场景根 GO，世界 Sprite） | **✅ 是** | `正常体` SR + `表情1` SR | GSM 相机对准此合层；**本期主改点** |
| **`UI_Shop`**（场景 Canvas + `ShopFormLogic`） | ✅ 交互层 | **无** ShopkeeperLayer | 0713 进店真源；仅 Bar/Tab/决定 |
| **`ShopPanel.prefab`** | ❌ 否 | 有 `ShopkeeperLayer/ImgBody/ImgFace` | OpenUIForm 已弃用；0704 镜像预备 |

**0713 双轨（仍有效）**：

```
Village_Shop
├── 商店界面合层          ← A 轨 · 看（老板娘 + 背景）
└── UI_Shop               ← B 轨 · 点（买卖 UI）
```

场景内 `商店界面合层` 为**精简实例**（约 3 子节点：背景 + 正常体 + 表情1），非完整 Prefab 树（无 `组 7` 货架 Sprite）。

### 2.3 美术磁盘 vs Prefab 挂载

| 资源 | 磁盘 PNG | 合层 Prefab | 场景实例 | ShopPanel UGUI |
|------|----------|-------------|----------|----------------|
| 表情1 | ✅ | ✅ `表情1` SR | ✅ SR 已绑 | ✅ ImgFace 默认 |
| 表情2～5 | ✅ | ❌ 未挂 | ❌ | ❌ 仅 ImgFace 单槽 |
| 正常体 | ✅ | ✅ | ✅ | ✅ ImgBody |
| 脸红体 / 阴险体 | ✅ | ❌ | ❌ | ❌ |
| 背景 / 组7 UI 图 | ✅ | 部分 | 背景有 | BgLayer 有 |

**换脸实现**：本期对 **`表情1` 的 SpriteRenderer** 做 `sprite = 表情N`（单槽换图），**不必**在 Hierarchy 挂 5 个子物体（与 `StoryFormPainting` 多子物体 Active 不同）。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 确认本期范围 | **只做表情 API + Debug**；不做首次进店 / 存档 / 藏 UI / 黑屏 |
| 2 | 经 `Door_Shop` 进 `Village_Shop` | 看见老板娘（合层 `正常体`+`表情1`）+ 可点买卖 Bar |
| 3 | Debug 切 `Face1→Face5` | 脸图变，身体/背景/Bar 不乱 |
| 4 | （若施工含换身）切 `Normal/Blush/Sinister` | 身体变，脸层仍叠在上面 |
| 5 | 买卖 Tab / 决定 / ESC 菜单 | 不被表情层挡点击 |
| 6 | 策划补表 | `表情1～5` 与台本 `Smile/Angry/…` 的对照（见开放问题） |

---

## ④ 给程序

### A. 「脸画在哪」锁定表

| 项 | 值 |
|----|-----|
| 进店 Play 老板娘来源 | **`商店界面合层`**（`Village_ShopSceneManager` Find 同名根） |
| 身体 GO / 组件 | `商店界面合层/正常体` · `SpriteRenderer` · 默认 `正常体.png`（guid `384a369d…`） |
| 脸 GO / 组件 | `商店界面合层/表情1` · `SpriteRenderer` · 默认 `表情1.png`（guid `e618f45a…`） · SortingOrder **22**（高于身体 21） |
| `ImgBody` / `ImgFace`（UGUI） | 仅在 **`ShopPanel.prefab`**；**当前 Play 不可见** |
| `表情2～5` / 脸红体 / 阴险体 | **磁盘有、Hierarchy 未挂**；施工时 **Resources/SerializeField 引用 PNG**，运行时换 Sprite |
| 合层 Prefab vs UI Prefab | **双份存在**；本期 **只改场景合层 + 脚本**；ShopPanel **P2 可选镜像**（Bake 工具已有「场景必烤、Prefab 镜像」惯例，见 `ShopListBakeEditor`） |

### B. 5 表情 + 身体变体语义表

> 策划尚未给 `表情1～5` 正式英名；下表为**施工占位**，台本 FaceType 对照待策划补（开放问题 §G）。

| ID | 源图 | 建议枚举 | 类型 | 默认 | 备注 |
|----|------|----------|------|------|------|
| 1 | `表情1.png` | `ShopkeeperFaceType.Face1` | 换脸 | **✅ 默认脸** | 与现网默认一致 |
| 2 | `表情2.png` | `Face2` | 换脸 | | |
| 3 | `表情3.png` | `Face3` | 换脸 | | |
| 4 | `表情4.png` | `Face4` | 换脸 | | |
| 5 | `表情5.png` | `Face5` | 换脸 | | |
| — | `正常体.png` | `ShopkeeperBodyType.Normal` | 换身 | **✅ 默认身** | 换 `正常体` SR |
| — | `脸红体.png` | `Blush` | 换身 | | 全身变体，脸 SR 仍独立 |
| — | `阴险体.png` | `Sinister` | 换身 | | 同上 |

**0601 台本 FaceType（老板娘句）**：`Smile` `Laugh` `Surprised` `Angry` 等 — **语义层**，下期用 **Mapper** 映到 `ShopkeeperFaceType`，**本期不扩** `DialogueFaceType`。

### C. 架构选型（侦探拍板）

| 方案 | 裁定 |
|------|------|
| **A · 复用对话 Painting** | ❌ 本期拒绝：工程大、商店合层与对话 Mask 双份、0704 已冻结整页迁 UI |
| **B · 商店专用切脸器** | **✅ 推荐**：贴合现网 Sprite 合层 + 0704 预留 `ImgFace` 概念 |
| **C · 手改 Sprite** | ❌ 拒绝 |

| 问题 | 裁定 |
|------|------|
| 新增 `DialogueRoleName` 老板娘？ | **本期不必**；下期首次进店对白再加 `Shopkeeper`（或 `Dian`） |
| 扩展 `DialogueFaceType`？ | **本期不扩**；独立 `ShopkeeperFaceType` + 下期 `ShopkeeperFaceMapper` |
| 切脸实现 | **`SpriteRenderer.sprite` / `Image.sprite` 单槽换图**（非 Faces 多子物体 Active） |
| 谁持有引用 | **独立组件 `ShopkeeperFaceController`** 挂 `商店界面合层` 根；**不**塞进 `ShopFormLogic.Update` |
| GSM 是否驱动 | **否**；`Village_ShopSceneManager` 最多 `Find` 一次做验收 Log，不绑业务 |

**建议 API（本期 + 下期共用）**：

```csharp
// 文件建议：Assets/Scripts/.../Shop/ShopkeeperFaceController.cs

public enum ShopkeeperFaceType { Face1, Face2, Face3, Face4, Face5 }
public enum ShopkeeperBodyType { Normal, Blush, Sinister }

public class ShopkeeperFaceController : MonoBehaviour
{
    public void SetFace(ShopkeeperFaceType face);   // 换脸 SR / 可选同步 UGUI
    public void SetBody(ShopkeeperBodyType body);   // P1：换身；API 先定，体可后做
    public void ResetDefault();                     // Face1 + Normal
    public ShopkeeperFaceType CurrentFace { get; }
    public ShopkeeperBodyType CurrentBody { get; }
}
```

**Debug 验收（P0）**：`ShopkeeperFaceDebugInput` 或 Editor 菜单 · Play 下按 `1～5` 切脸、`F1～F3` 切身（仅 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`）；**禁止**在 `Update` 写业务，仅读 Input 调 `SetFace`。

### D. 下期桥接设计（只设计不实现）

```
首次进店 NodeCanvas / NormalDialogueFormNewLogic
  SayEx(店, Smile) 或 CSV Face 列
    → DialogueTMPUGUI.OnGetNewStatement(role, faceType, text)
        →【下期】ShopkeeperDialogueFaceBridge.OnGetNewStatement
              if role == Shopkeeper（新 enum）
                face = ShopkeeperFaceMapper.FromDialogueFace(faceType)
                ShopkeeperFaceController.SetFace(face)
                （可选）SetBody(...)
  对白结束 → 黑幕 → 显示 UI_Shop   ← 下期施工，本期不做
```

**本期留钩**：

- `ShopkeeperFaceController` 对外静态查找或 `Village_Shop` 单例注册（轻量 `ShopkeeperFaceRegistry.Instance`），下期桥接 **只订阅事件 + 调 API**。  
- **左侧女主立绘**仍走现有 `DialogueMaskAvatarPresenter` + 雅儿 Painting；与老板娘合层 **并存**（0629 §4.3 构图：左二女 + 右老板娘）。

**禁止本期**：存档旗标、`TriggerStory` 首次进店、藏 `UI_Shop`、黑屏时序。

### E. 最小施工清单（给施工员，侦探不执行）

| # | 文件 / 物体 | 动作 | 优先级 |
|---|-------------|------|--------|
| 1 | 新建 `ShopkeeperFaceType.cs` / `ShopkeeperBodyType.cs` | 枚举 + 注释 | P0 |
| 2 | 新建 `ShopkeeperFaceController.cs` | 序列化 `SpriteRenderer`（脸/身）+ 5 脸 3 身 Sprite 引用；`SetFace`/`SetBody`/`ResetDefault` | P0 |
| 3 | `Village_Shop.unity` · `商店界面合层` | 挂 Controller；拖 SR 与 Sprite | P0 |
| 4 | 新建 `ShopkeeperFaceDebugInput.cs`（或 Editor 菜单） | Play 下 1～5 / F1～F3 验收 | P0 |
| 5 | `ShopPanel.prefab` · `ShopkeeperLayer` | **可选 P2**：同 Controller 或 `ShopkeeperFaceController` 增 `Image` 同步槽，防日后 OpenUIForm 漂移 | P2 |
| 6 | `商店界面合层.prefab` | **不必**为本期改完整树；若要从其它场景复用，再同步挂组件 | P3 |
| 7 | 技术文档 | `Doc/技术文档/` 增「商店老板娘表情对照表」（策划填语义名后） | P2 |

**明确排除**：`DialogueRoleName` / CSV / 首次进店 Prefab / `PlayerArchive` 旗标 / 改 `DialogueFaceType` / 改 `ShopFormLogic` 买卖逻辑。

### F. 验收清单（表情系统单独可验）

| # | 操作 | 通过 |
|---|------|------|
| 1 | InitScene → 村 → `Door_Shop` → `Village_Shop` | 默认脸 `Face1` + 身 `Normal` |
| 2 | Debug：`Face1→Face5` 循环 | `表情1` SR 换图正确，SortingOrder 仍在身体上 |
| 3 | Debug：`Normal→Blush→Sinister`（若 P1 已做） | 身体 SR 换图，脸 SR 不受影响 |
| 4 | 点购买 Tab / 改数量 / 决定 | Bar 可点；脸层无 Raycast（Sprite 默认无 UI 阻挡） |
| 5 | Console | 无 Missing Sprite / NRE；可选 `[ShopkeeperFace] SetFace=Face3` Log |

### G. 开放问题

| ID | 问题 | 建议 |
|----|------|------|
| Q1 | `表情1～5` 与台本 `Smile/Angry/Laugh/…` 的一一对照？ | 策划填表后再做 `ShopkeeperFaceMapper`；本期用 `Face1～5` |
| Q2 | `脸红体`/`阴险体` 是否本期必做？ | API **先定** `SetBody`；实现可 **P1**（仅脸也能验收） |
| Q3 | 首次进店时左侧雅/古莎是否仍走通用 Painting？ | **是**（0629 构图）；与右侧合层老板娘 **并存** |
| Q4 | 老板娘要不要对话框小头像？ | **本期默认否**（合层立绘已占右侧） |
| Q5 | 长期是否把合层迁回 `ShopPanel` UGUI？ | 0704 冻结；表情 API 设计为 **SR + Image 双后端可同步**，便于日后迁 |
| Q6 | `DialogueRoleName` 叫 `Shopkeeper` 还是 `Dian`（店）？ | 0601 CSV 说话人「店」；下期定 enum 名 |

---

## 附录 · 现网代码/资源索引

| 资产 | 路径 | 与表情关系 |
|------|------|------------|
| 进店逻辑 | `ShopFormLogic.cs` | 无 ImgFace；**勿**在本类堆切脸 |
| 场景 GSM | `Village_ShopSceneManager.cs` | Find `商店界面合层` 对准相机 |
| 对话切脸样板 | `StoryFormPainting.cs` / `DialogueMaskAvatarPresenter.cs` | **勿复用**于老板娘 |
| 角色枚举 | `RoleName.cs` · `DialogueFaceType.cs` | 无老板娘 |
| 合层 Prefab | `ArtRes/Scene/Village/商店界面合层.prefab` | 仅 `正常体`+`表情1` |
| UI 镜像 | `GameRes/Prefabs/UI/ShopPanel.prefab` | `ShopkeeperLayer/ImgBody/ImgFace`，raycastTarget=0 |
| Bake 惯例 | `ShopListBakeEditor.cs` | 场景 `UI_Shop` 真源 · Prefab 镜像 |

---

## 本阶段严禁（已遵守）

- 未改代码 / Prefab / 场景 / CSV  
- 未把本期扩成首次进店全流程  
- 未把老板娘 5 表情塞进雅儿 `DialogueFaceType`  
- 未假定「只改 ShopPanel 即可 Play 验收」

---

*报告结束。施工员按 §E 实现最小闭环；下期对白只加 §D 桥接，不推翻 API。*
