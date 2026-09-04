# Village_KenMuNi1 — 巨树 2 楼 WalkArea2 宝箱 Hp/Mp×3 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md`  
**依赖**：楼梯换场巨树 2 楼 + W1（WalkArea2 生效）

---

## 沟通摘要

### ① 结论一句话

**仿 WestRapp：WalkArea2 内摆 `Box.prefab` + 新建 `VillageKenMuNi1HpMpBox`（默认 3/3、无 Story）+ 存档 `VillageKenMuNi1Data.tree2fHpMpBoxOpened`；开箱入包双球并依次弹 GetHpBall→GetMpBall；不改 WalkArea2 形状。**

### ② 原因（通俗）

西境箱子已经会开箱发球+两张横幅，数量改成 3 就能用。  
不能直接挂西境脚本，否则会读错存档。  
箱子必须在 2 楼可走多边形里，可走区大小已定死，只挪箱子。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 开 Unity 跑菜单 `Tools / Scene / Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3`（或等 request 自动跑） | |
| 2 | Hierarchy：`Objects/Tree2fHpMpBox` 存在，且脚在 WalkArea2 内 | |
| 3 | 上 2 楼 → 点 E / 点击开箱 | |
| 4 | 背包生命球 +3、体力球 +3 | |
| 5 | 依次 GetHpBall、GetMpBall 横幅 + 物品音效 | |
| 6 | 同档再点不再发奖；读档为打开态 | |
| 7 | `VillageWalkArea2` 多边形未改；剑/空桶/针线包 Tips 正常 | |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| `VillageKenMuNi1Data` | `ArchiveDataClass/Scene/` | `tree2fHpMpBoxOpened` |
| `VillageKenMuNi1HpMpBox` | `SceneEntities/Village_KenMuNi/` | 拷贝 West；默认 3/3；无 Story |
| Editor | `KenMuNi1Tree2fHpMpBoxSetupEditor` | 摆 `Tree2fHpMpBox` + 登记 sceneObjs |
| Request | `Library/KenMuNi1Tree2fHpMpBoxSetup.request` | Unity 打开时自动跑一次 |

**未改**：`VillageWalkArea2` 多边形；开箱强制 `SavePlayerBag`（Q6 对齐 West 内存脏写）；西境/卧室箱脚本。

---

## 开箱流水

```
OpenBox（未开）
  → tree2fHpMpBoxOpened=true；关 canTouch
  → Animator Open；SFX
  → AddMainItem(HpBall, 3) + AddMainItem(MpBall, 3)
  → OpenTipsForm("GetHpBall") → OpenTipsForm("GetMpBall")
```

---

## 摆放

| 项 | 值 |
|----|-----|
| 名 | `Tree2fHpMpBox` |
| 父 | `Objects` |
| 建议坐标 | `(-152, 41.2, 0)`（不在区内则试 `(-165, 40.8, 0)`） |
| Prefab | `Assets/Prefabs/Box.prefab`（去 HomeScene2Box，挂村脚本） |
