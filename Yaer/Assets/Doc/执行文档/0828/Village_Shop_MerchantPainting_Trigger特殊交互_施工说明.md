# Village_Shop · MerchantPainting 特殊交互 — 施工说明（0828）

**角色**：施工员  
**依据**：`执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md` §G

## 已落盘

| 项 | 路径 / 说明 |
|----|-------------|
| 热区脚本 | `ShopkeeperBodyHotspot.cs` |
| GSM | `TryTriggerShopkeeperSpecial` / `SetShopkeeperHotspotsEnabled` |
| 场景热区 | `Village_Shop.unity` → ` MerchantPainting/Trigger/Head|Chest` + Main Camera `Physics2DRaycaster` |
| CSV | `Assets/Dialog/Village_商店点头交互.csv`、`Village_商店点胸交互.csv`（§D 表情；胸止于 C5） |
| Editor 一键 | `Tools/Dialogue/Setup Shopkeeper Special Click (Hotspots + Prefabs)` |

## 你必须做的一步（Unity 内）

打开工程后执行菜单：

**Tools → Dialogue → Setup Shopkeeper Special Click (Hotspots + Prefabs)**

会：确认热区 / Raycaster、从 `Village_ShopStart` 复制并去古莎、Import CSV 写入  
`Village_ShopKeeper_HeadClick` / `Village_ShopKeeper_ChestClick` Prefab。

未跑菜单前点头胸会 Trigger 失败（Prefab 尚未生成），属预期。

## 验收（报告 §H）

1. Idle 点 Head → 点头对白；店脸 Face1～5 + Mask；雅脸变  
2. 点 Chest → C1～C5；**不**切树屋  
3. 对白中再点 → 不开第二段  
4. 对白中 UI_Shop 隐藏，结束后恢复  
5. 首次进店中点头胸无干扰  
6. Console 无 NRE / Missing Actor / Face 校验失败  

## 分期

C6 黑屏转树屋 / C7～C8：**下期**（开放问题 Q3）。
