# Village_HomeScene23 — 屋内 NPC 对白台本（执行说明）

**用途**：策划/程序按本文录入 CSV 或做对话 prefab，挂到各 NPC 的 `SimpleStoryTrigger.StoryPrefabName`。  
**场景**：`Village_HomeScene23`（屋内）  
**玩家角色名**：雅（运行时 Actor 建议绑「雅尔」）

---

## 1. 对白一览（按策划图提取）

### 1.1 屋子里的 NPC1（首次点击）

| 序号 | 说话人 | 台词 |
|------|--------|------|
| 1 | NPC1 | 哎呀，你怎么自己就进来了，吓我一跳。 |
| 2 | 雅 | 不好意思。。。 |

**建议资源名**：`Village_HomeScene23_Npc1`

---

### 1.2 屋子里的 NPC2、NPC3（首次点击，一段里多人轮流说）

| 序号 | 说话人 | 台词 |
|------|--------|------|
| 1 | NPC2 | 妈妈，有外人！ |
| 2 | NPC3 | 是谁呀？ |
| 3 | NPC2 | 是怪人！她还长角！！ |
| 4 | NPC3 | 不能这么说话！太没礼貌了！ |
| 5 | NPC3 | 不好意思有什么事吗？ |
| 6 | 雅 | 啊啊。。。。我只是随便转转。。。。 |
| 7 | NPC3 | 欢迎做客。 |

**建议资源名**：`Village_HomeScene23_Npc2_3_First`

---

### 1.3 重复点 NPC2（再次点击，短对话）

| 序号 | 说话人 | 台词 |
|------|--------|------|
| 1 | NPC2 | 姐姐好！ |
| 2 | 雅 | 你好~ |

**建议资源名**：`Village_HomeScene23_Npc2_Repeat`  
**说明**：与 §1.2 分开两段剧情；NPC2 上可用存档「已播过首次」后改触发名，或程序分支（首版可先只做首次）。

---

### 1.4 屋子里的 NPC4、NPC5（首次点击）

| 序号 | 说话人 | 台词 |
|------|--------|------|
| 1 | NPC4 | 我和她曾经有一个孩子。 |
| 2 | NPC4 | 那场战争让我们失去了他。 |
| 3 | NPC5 | 别说了。 |
| 4 | NPC4 | 我。。。。。。 |
| 5 | 雅 | 别难过了。 |
| 6 | NPC4 | 随便坐坐吧，如果他还在应该也和你一样大了吧。 |

**建议资源名**：`Village_HomeScene23_Npc4_5`

---

## 2. 场景物体对应（施工时对照）

| 策划称呼 | 场景 Hierarchy 建议名 | 对话 prefab 名（§1） |
|----------|----------------------|----------------------|
| 屋子里的 NPC1 | `Entity/Npc1` | `Village_HomeScene23_Npc1` |
| NPC2 | `Entity/Npc2` | 首次：`Village_HomeScene23_Npc2_3_First`；再点：`Village_HomeScene23_Npc2_Repeat` |
| NPC3 | `Entity/Npc3` | 与 NPC2 同一段首次剧情（挂在 NPC3 或只挂 NPC2 其一，避免重复触发） |
| NPC4 | `Entity/Npc4` | `Village_HomeScene23_Npc4_5` |
| NPC5 | `Entity/Npc5` | 与 NPC4 同一段（只挂一个触发器即可） |

---

## 3. 制作步骤（简版）

1. 按 §1 表格做 CSV → `Tools → Dialogue → Import CSV` 生成图，或手搭 NodeCanvas。  
2. 每个 prefab 保存到 **`Assets/GameRes/Prefabs/Dialogue/` 根目录**（文件名 = 上表「建议资源名」）。  
3. 复制场景里已有 **`Npc1`**（带 `SimpleStoryTrigger`），改成 Npc2～5，填对应 **Story Prefab Name**。  
4. 从 **InitScene** 进游戏，进 `Village_HomeScene23`，逐个点 NPC 验收台词顺序。

更细的 NPC 挂载说明见：`Village_HomeScene23_NPC对话配置_执行说明.md`。

---

## 4. CSV 导入参考（可选）

Speaker 列可用简称，导入后 Actor 映射为工程内全名（示例）：

| CSV Speaker | 图内 Actor 建议 |
|-------------|-----------------|
| 雅 | 雅尔 |
| NPC1～NPC5 | 按立绘资源分别建 Actor，或暂用占位名 |

**雅** 带表情（惊吓 / 开心 / 难过）时，在 Statement 节点或立绘参数里切换，本文未写具体资源名，由美术/程序按句配置。

---

## 5. 修订

| 日期 | 说明 |
|------|------|
| 2026-06-01 | 从策划交互说明图提取屋内 NPC1～5 对白 |

**路径**：`Assets/Doc/执行文档/0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md`
