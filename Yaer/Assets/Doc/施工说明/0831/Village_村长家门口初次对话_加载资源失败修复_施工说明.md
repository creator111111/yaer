# Village_村长家门口初次对话 — 加载资源失败修复 — 施工说明

**日期**：2026-08-31  
**依据**：`执行文档/0831/Village_村长家门口初次对话_加载资源失败修复_施工执行说明.md`  
**性质**：H1 成品 Prefab 补盘；完整台本须 Setup 菜单覆盖

---

## ① 结论

**根因 H1**：`Village_村长家门口初次对话.prefab` 从未落盘。  
已在磁盘 **拷贝壳** `Village_KenMuNiStart` → 目标路径，消除「加载资源失败」。  
**请再跑一次** `Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab`，把台本换成门口 CSV（三立绘 + Face123）；菜单会覆盖拷贝壳。

---

## ② 原因

`TriggerStory("Village_村长家门口初次对话")` → `DialoguePath` → 目标 Prefab 不存在 → ResMgr 报错。  
三人立绘施工只合了 Setup 脚本，未在本机执行菜单。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Project 可见 `…/Village_村长家门口初次对话.prefab` | ✅ 已拷贝壳 |
| 2 | Play 靠近 `Npc_Chief` | **无**「加载资源失败」 |
| 3 | `Tools / Dialogue / Setup Village 村长家门口初次对话 Prefab` | Console `[ChiefDoorSetup] Prefab 已写入…` |
| 4 | 再 Play | 播门口台本（非开场 KenMuNi 句） |
| 5 | （可选）场景 KenMuNiStart Missing 重挂 | P1 |

---

## ④ 程序清单

| 项 | 说明 |
|----|------|
| 落盘 | `Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab`（壳拷贝应急） |
| Setup 增强 | `Library/ChiefDoorSetup.request` 自动跑菜单（Unity 编译后）；路径锚 `Application.dataPath` |
| 须用户 | 点 Setup 菜单做正式 Import（Agent 无法在已开工程上 batchmode） |

**勿改**：ResMgr、Trigger 名、空 Prefab 无图。
