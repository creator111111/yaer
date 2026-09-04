# Cursor Agent Prompt · 雅儿白天待机帧图重命名（Crown / None / Armor）

> **角色**：【架构侦探】只核实、不改名 / 不改 meta / 不改 Clip  
> **日期**：2026-08-07  
> **依据预检**：`Assets/Doc/执行文档/0807/雅儿白天待机帧图重命名_Crown_None_Armor_架构侦探执行说明.md`  
> **本阶段**：核对分类规则与抽查实图，确认可交给施工员批量重命名

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0807/雅儿白天待机帧图重命名_Crown_None_Armor_架构侦探执行说明.md
@Assets/Animation/Object/Yaer/Home/白天待机1_0608/战斗服待机 拷贝

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止重命名、移动、删除任何 png/meta，禁止改 AnimationClip / Animator。
只读核实上述执行说明，必要时追加短报告。

## 背景

白天待机导出帧文件名混乱（图层/曲线/拷贝）。开发者口径：
- 纯「图层 {编号}」→ Crown
- 「图层拷贝 / 含拷贝或副本的图层」→ None
- 「曲线…」→ Armor
帧文件夹 1 已改为 Armor1/Crown1/None1。

## 侦探任务

1. 结论一句话：规则是否可施工；有无误判风险。
2. 抽查帧 2、12、20（及任选一帧）：三文件分类是否与执行说明 §4.3 一致。
3. 强调：必须先匹配「曲线」，再匹配拷贝/副本（曲线名本身含副本/拷贝）。
4. 确认走路目录本期不动。
5. 若有例外文件，列出完整路径，追加 OPEN。
6. 输出：可在执行说明文末追加「侦探确认」小节；或另写
   `Assets/Doc/执行文档/0807/雅儿白天待机帧图重命名_架构核实补记.md`（仅当有差异时）。

口头汇报用 MASTER 四段式。
```

---

## 施工员续跑（侦探确认后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0807/雅儿白天待机帧图重命名_Crown_None_Armor_架构侦探执行说明.md

你现在是【施工员】。按执行说明 §4.3～§4.6，仅在
`Assets/Animation/Object/Yaer/Home/白天待机1_0608/战斗服待机 拷贝/`
批量重命名 png（及对应 meta 文件名），跳过已规范的帧 1。
禁止改走路目录、禁止重排文件夹号、禁止做 Clip。
完成后列出改名对照表并说明如何在 Unity 验收无丢引用。
```
