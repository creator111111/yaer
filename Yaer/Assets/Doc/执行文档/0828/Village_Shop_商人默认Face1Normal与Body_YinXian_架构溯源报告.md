# Village_Shop — 商人默认 Face1 + Body Normal，并确认 Body·YinXian — 架构溯源报告

**文档版本**：v1.0（2026-08-28）  
**文档性质**：【架构侦探】只读溯源 + 施工指引（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**载体**：`Village_Shop` · `商店界面合层` → ` MerchantPainting`（大立绘）· `MerchantMaskPainting`（Mask 小表情）  

关联提示词：`Assets/Doc/提示词/0828/Village_Shop_商人默认Face1Normal与Body_YinXian_架构侦探提示词.md`  
关联：`0827/BodyFace CSV` · `0827/MerchantMaskPainting` · `0828/Trigger 特殊交互`

---

## ① 结论一句话

**默认 Face1+Normal 与 Body·YinXian 在枚举 / 注册 / 三载体 Prefab·场景上已齐；真正缺口是「对白/特殊交互结束后没有 `ResetDefault`」——首次进店末句停在 Face2+Red、点头末店句 Face5+Red、点胸末句 Face2+Red，Idle 会带着非默认身脸。推荐方案 A：在 GSM 对白结束（及进店兜底）显式 `ShopkeeperFaceRegistry.ResetDefault()`；YinXian 无需新建节点，验收用 Debug F3 或 CSV 试填即可。**

---

## ② 原因（通俗）

### 2.1 「看起来已有」≠ Idle 永远正确

| 层 | 现状 | 白话 |
|----|------|------|
| 代码 | `ResetDefault()` = Normal+Face1；YinXian 已 `RegisterBodyNode(Sinister,"YinXian")` | 工具箱齐了 |
| Prefab / 场景序列化 | 默认勾选仅 Normal+Face1；YinXian inactive 待命 | 进店**首帧**通常对 |
| 运行时 | **Awake 不调 Reset**；**onStoryEnd 不调 Reset** | 对白改完脸/身就**停在那儿** |
| 台本 | 全店 CSV **从未写过** `BodyType=YinXian`（只用过 Red） | 阴险身「有货没上场」 |

生活类比：衣柜里 Normal / Red / YinXian 三套衣服都挂好了，进店默认穿 Normal+Face1；但聊完天（尤其首次进店末段红脸）老板娘**不换衣服**就站着卖货——所以要在「对话结束」补一句「换回默认装」。

### 2.2 用户说「新增 YinXian」的真含义（证伪）

| 假说 | 磁盘结论 |
|------|----------|
| Hierarchy 缺节点 | ❌ 场景 / `MerchantPainting` / `MerchantMaskPainting` **均有** `Body/YinXian` + Sprite `阴险体.png`（guid `6406c966…`） |
| Mask 缺 | ❌ Mask Prefab 同构，Sprite 已绑 |
| 枚举 / Register 缺 | ❌ `Sinister` ↔ GO `YinXian` ↔ CSV `YinXian`；Debug **F3** 已切 Sinister |
| 仅文档/台本未用过 | ✅ **最接近**；另加 **结束不复位** 体验缺口 |

`ArtRes/.../商店界面合层.prefab` 仍是旧美术树（`正常体`/`表情1` 中文名），**不是**运行 Toggle 真源；运行真源 = 场景合层下 ` MerchantPainting`。勿把旧合层 Prefab 当「缺 YinXian」依据。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Hierarchy：`Body` 下 Normal✅ / Red❌ / YinXian❌；`Face` 仅 Face1✅ | 进店前序列化默认 |
| 2 | Play 进 `Village_Shop`（二进宫、未触发对白） | 大立绘 = **Normal + Face1** |
| 3 | Dev：按 **F3**（阴险身）再 **F1**（Normal）；数字键 1～5 切脸 | 合层切换无 Warning |
| 4 | 播完首次进店 / 点头 / 点胸后看 Idle | 施工后应回 **Normal + Face1**（现网会停在末句红脸） |
| 5 | （可选）任店 CSV 一行填 `BodyType=YinXian` → Import → Play | 合层+Mask 同亮阴险身 |
| 6 | Console | 无「未找到子 GO YinXian」 |

---

## ④ 给程序

### A. 默认态真源表

| 检查点 | 期望 | 现网 | 差距 |
|--------|------|------|------|
| 枚举 `Face1=0` / `Normal=0` | ✅ | ✅ | 无 |
| `ResetDefault()` | Normal+Face1 | ✅ 合层 + Mask 均有 | **无人在运行时调用**（除 Editor Setup） |
| `Awake`/`Start` | 可选复位 | 仅 Cache+Register；**无 Start Reset** | 依赖序列化 Active |
| 场景合层 Active | 仅 Normal+Face1 | ✅ Normal=1 Face1=1；其余 0 | 无 |
| `MerchantPainting.prefab` | 同上 | ✅ | 无 |
| `MerchantMaskPainting.prefab` | 同上（根可 inactive） | ✅ Body/Face 默认正确 | 无 |
| CSV 空 Body/Face | Normal+Face1 起算 | GraphBuilder 累计器初值 ✅；空列继承 | 无 |
| CSV 写 `YinXian` | → `Sinister` | `ShopkeeperCsvDefaults.TryParseBody` ✅ | 无生产行使用 |
| 对白结束 Idle | 产品倾向默认 | **停在末句**（ShopStart 末 Face2+Red 等） | **P0 缺口** |

**末句残留（现网 CSV）**

| 对白 | 最后店句身/脸 | Idle 若不 Reset |
|------|---------------|-----------------|
| `Village_ShopStart` | Face2 + **Red** | 红脸调侃身 |
| `Village_ShopKeeper_HeadClick` | Face5 + **Red**（H10） | 怒红 |
| `Village_ShopKeeper_ChestClick` | Face2 + **Red**（C5） | 红脸 |

---

### B. YinXian 完备性（三载体）

| 载体 | 路径 | YinXian GO | 默认 Active | Sprite/Image | `Apply(Sinister)` |
|------|------|------------|-------------|--------------|-------------------|
| 场景大立绘 | `Village_Shop` → ` MerchantPainting/Body/YinXian` | ✅ | ❌（待命） | SR ✅ `6406c966` | ✅ Controller 已注册 |
| SR Prefab | `MerchantPainting.prefab` | ✅ | ❌ | 同左 | 同左（场景用树） |
| UI Mask | `MerchantMaskPainting.prefab` | ✅ | ❌ | Image ✅ 同 guid | ✅ Mask 已注册 |

| 驱动 | 现网 |
|------|------|
| Debug | F1=Normal / F2=Red / **F3=YinXian** |
| CSV | 允许 `Normal`/`Red`/`YinXian`；空=继承或表无列→Normal |
| 店句运行时 | `DialogueTMPUGUI` → Registry.Apply + Presenter.ApplyShopkeeperPortrait |

**裁定**：YinXian = **资产与代码已完备**；施工重点是 **复位生命周期** +（可选）台本试填验收，**不是**新建枚举/节点。

---

### C. 生命周期与推荐方案

```
进店 OnEnterScene / 合层 Awake
  → 现网：靠 Prefab Active（通常 Normal+Face1）
  → 推荐：再调一次 ResetDefault（防 Debug/残留）

对白句
  → Apply(CSV Body/Face) 合层+Mask   ✅ 已有

对白结束（ShopStart / Head / Chest）
  → 现网：只 Show UI + 开热区
  → 推荐：+ Registry.Instance?.ResetDefault()
         （Mask 随 Panel 关闭；可选 Presenter 关前 Reset，非 Idle 必需）

离场
  → 合层随场景卸；无额外需求
```

| 方案 | 做法 | 优点 | 风险 | 裁定 |
|------|------|------|------|------|
| **A** | 序列化默认保持；**进店 + 对白结束**显式 `ResetDefault` | Idle 稳定，对齐产品「默认=Face1+Normal」 | 结束瞬间末句脸→默认，可能闪一下（黑幕期内可掩盖 ShopStart） | **✅ 本期推荐** |
| B | 结束保留末句脸，仅进店/Awake 默认 | 少一次切 | 买卖 Idle 长期红脸/阴险 | ❌ 违背产品 Idle 默认 |
| C | 仅 Editor 校正 | — | 运行必漂 | ❌ |

**挂点（最小 diff · 不改点头胸触发逻辑）**

| 文件 | 调用点 |
|------|--------|
| `Village_ShopSceneManager.cs` | `OnShopStartStoryEnd`（显 UI 前或黑幕 hold 内） |
| 同上 | `OnShopkeeperSpecialStoryEnd` |
| 同上（P1） | `OnEnterScene` 二进宫路径开头 `ResetDefault` 兜底 |
| **不改** | `ShopkeeperBodyHotspot` / TriggerStory 名 / CSV 剧情 |

辅助方法建议：

```csharp
// 伪代码 · GSM 私有
private static void ResetShopkeeperPortraitDefault()
{
    ShopkeeperFaceRegistry.Instance?.ResetDefault();
}
```

若 `ShopkeeperFaceRegistry` 尚无转发，可直接 `Instance.ResetDefault()`（Controller 已有公开 API）。

**Mask**：Idle 不显示 Mask；结束复位 Mask **非必须**。若要对齐双轨卫生，可在 Panel 关闭路径调一次 `merchantMaskPainting.ResetDefault()`（P2）。

---

### D. 与对白 / 点头胸关系（只挂钩）

| 问题 | 答复 |
|------|------|
| CSV `BodyType=YinXian` 能否驱动合层+Mask？ | **能**（架构已通）；缺的是验收行 |
| 点头/点胸要阴险身？ | **只改 CSV 单元格**，无需改架构 |
| 本期改特殊交互触发？ | **禁止**；仅在既有 `onStoryEnd` 回调加 Reset |

---

### E. 最小施工清单（本阶段不执行）

| # | 文件/物体 | 动作 | 优先级 |
|---|-----------|------|--------|
| 1 | `Village_ShopSceneManager.cs` | ShopStart / Special 的 `onStoryEnd` 调 `ResetDefault` | **P0** |
| 2 | 同上 | `OnEnterScene`（或合层可见时）兜底 `ResetDefault` | **P1** |
| 3 | Hierarchy 抽查 | 确认三载体默认 Active 仍为 Normal+Face1（一般不用改） | P0 核验 |
| 4 | （可选）店 CSV 一行 `YinXian` 冒烟 | 或仅用 F3 验收，不强制改正式台本 | P1 |
| 5 | `ShopkeeperFaceRegistry` | 若无便捷 API，可加 `ResetDefault()` 静态转发（薄封装） | P2 |
| 6 | 短技术说明 / 对照表 | Face1+Normal 默认；Body 三角色 | P2 |

**明确排除**：新建 YinXian 枚举/节点；扩 `DialogueFaceType`；改 Face1～5 语义；重做点头胸/首次进店剧情；改旧 `商店界面合层.prefab` 美术树；`Update` 轮询默认脸。

---

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进店 Idle | 大立绘 = Normal + Face1 |
| 2 | F3 → 阴险身；再切脸 | 合层正确；无 YinXian Warning |
| 3 | 首次进店或点头/点胸结束 | Idle **回到** Normal + Face1 |
| 4 | （可选）CSV `BodyType=YinXian` 店句 | Import OK；运行合层+Mask 一致 |
| 5 | Console | 无「未找到 YinXian」 |

---

### G. 开放问题 → `OPEN_QUESTIONS.md`

| ID | 问题 | 侦探倾向 |
|----|------|----------|
| Q1 | 对白结束是否**强制**回默认？ | **是（方案 A）**；与产品 Idle 默认一致 |
| Q2 | `商店界面合层.prefab`（旧中文树）是否双写？ | **否**；运行真源是场景 ` MerchantPainting` |
| Q3 | 阴险身是否必须搭配固定脸？ | **正交自由组合**（现网 Apply 独立）；台本自选 Face |
| Q4 | ShopStart 结束黑幕期间 Reset 会否被察觉？ | 倾向在 **显 UI 前 / hold 内** Reset；肉眼无闪 |

---

### H. 命名速查（勿混）

| Hierarchy GO | 枚举 | CSV `BodyType` |
|--------------|------|----------------|
| `Normal` | `Normal` | `Normal` 或空 |
| `Red` | `Blush` | **`Red`** |
| `YinXian` | `Sinister` | **`YinXian`** |

| 脸 | 枚举 / CSV |
|----|------------|
| 默认 | `Face1` |
| 其它 | `Face2`～`Face5` |

禁止：CSV 写 `Sinister`；把 YinXian 塞进 `DialogueFaceType`；用换 Sprite 替代 Toggle。

---

### I. 预期 diff（施工员）

| 文件 | 动作 |
|------|------|
| `Village_ShopSceneManager.cs` | +Reset 调用（P0/P1） |
| （可选）`ShopkeeperFaceRegistry.cs` | +静态 `ResetDefault()` |
| Prefab/场景 Active | **通常不改**（已正确） |
| CSV | **可选**验收行；正式台本是否用 YinXian 由策划定 |

---

**报告结束 · 待拍板方案 A 后交【施工员】执行。**
