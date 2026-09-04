# Village_Shop · 关 FightingPanel + ShopPanel / 黑幕节奏 — 文档修订说明

**性质**：原执行说明修订（**不再按原文施工**）  
**修订日期**：2026-07-13  
**原文路径**：同文件（曾主张：关血条 + 0704 收 ShopPanel + 与黑幕同 `OpenUIForm`）

---

## 1. 当前结论（以本修订为准）

| 议题 | 状态 | 怎么处理 |
|------|------|----------|
| **黑幕渐入渐出不同步** | ✅ **已解决** | 不必再为「同节奏」做 ShopPanel / OpenUIForm 改造 |
| **0704 合层搬进 ShopPanel** | ❌ **弃用** | 实机验证会 **UI 位置错乱**；继续用场景内 **合层 + `UI_Shop` 双轨** + 相机对准合层 |
| **进店血条 / FightingPanel 不关** | ⏳ **另文施工** | 见下方专用文档，**只做显式 Close** |

---

## 2. 原文哪些段落作废

以下 **全部作废，勿再排期**：

- 按 0704 新建 `ShopPanel.prefab`、合层拆成 UGUI Image  
- `ShopFormLogic` 改继承 `BaseUIFormLogic` 仅为接 `OpenUIForm`  
- 全黑阶段 `OpenUIForm(ShopPanel)` 与 BlackPanel 绑节奏  
- 删除场景合层 / `UI_Shop`「去双轨」  

**仍有效的唯一技术点**（已拆到专文）：

- 纯 UI 场景 `canCreatePlayer=false` → 不跑 `InitPlayer` → 基类不会自动关 FightingPanel → 进店要 **显式 `CloseUIForm(FightingPanel)`**

---

## 3. 现行商店进店形态（保持）

```
Door_Shop → LoadScene(Village_Shop) + BlackPanel（节奏已 OK）
  → Village_ShopSceneManager：世界合层 + UI_Shop + 相机锁死对准合层
  → 【待做】CloseUIForm(FightingPanel)   ← 仅此缺口
```

对齐文档：

- 换场 / 无玩家：`0713/Village_KenMuNi1_Door_Shop换场Village_Shop纯UI_…`
- 合层相机：`0713/Village_Shop_合层不显示_摄像机_短执行说明.md`
- **血条**：`0713/Village_Shop_进店关闭FightingPanel血条_执行说明.md`（新建）
- 0704 合层转 UI：**已标废弃**，见该文文首修订条

---

## 4. 版本记录

| 版本 | 日期 | 说明 |
|------|------|------|
| 0.1 | 2026-07-13 | 初稿：关血条 + ShopPanel 同黑幕节奏 |
| 0.2 | 2026-07-13 | **修订**：黑幕已解决；0704 弃用（位置错乱）；血条改专文 |

**文档路径**：`Assets/Doc/执行文档/0713/Village_Shop_关FightingPanel与ShopPanel同黑幕节奏_执行说明.md`
