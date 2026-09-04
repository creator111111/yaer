# Village_Chief_House — 续聊结束正面古莎未出现 — 施工说明（H2 补场景实例）

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按验收排查报告最小修复  
**依据**：`执行文档/0901/Village_Chief_House_续聊结束正面古莎未出现_验收排查报告.md`  
**范围**：**只**在场景 `Design/村长家合层` 预置 `古莎动画合层`（+ Setup 扩写场景）；**不改** WalkArea / 楼梯 / 出屋戏 / 其它你手改过的场景设置。

---

## ① 结论

主因 H2+H8：玩的是拆包合层，缺动画实例。Setup 现同时写 **场景合层** + Prefab 资产，并绑 GSM 两引用。

## ② 你要做的

1. 开 Unity（或菜单 `Tools / Scene / Setup Chief House 古莎动画合层预置`）  
2. Hierarchy：`Design/村长家合层/古莎动画合层` 存在且默认灰  
3. 续聊结束 → 黑幕 → 待机关、正面合层可见；Console **无**「未找到古莎动画合层」

## ③ 改动

| 项 | 说明 |
|----|------|
| `ChiefHouseGushaAnimStandbySetupEditor` | 新增 `SetupIntoChiefHouseScene`（只动合层下动画实例 + GSM 两字段） |
| `Village_Chief_HouseSceneManager` | Resolve 失败时多打路径指纹（便于再验 H2） |
| Request | `Library/ChiefHouseGushaAnimSetup.request` |

**未动**：出屋送树屋、WalkArea2、UI `GushaPainting`、`Village/村长家合层`（f77da）。
