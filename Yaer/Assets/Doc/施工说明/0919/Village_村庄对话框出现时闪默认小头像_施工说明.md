# Village · 对话框出现时闪默认小头像 — 施工说明

**文档版本**：v1.0（2026-09-19）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0919/Village_村庄对话框出现时闪默认小头像_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**框先空着淡入，第一句话出来的同时才出这一句的小头像；不再提前亮上一张脸。**

### ② 原因（通俗）

有 4 张村庄对话图在出字之前就把小头像点亮了。另外，对话面板会留下来复用，上一场的脸没藏掉，下一场框一打开就会闪一下。两处都改了：淡入和对话开始先藏脸，这 4 张图不再预亮。

### ③ 用户检查清单

进游戏看，不要只看一张图。

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 新档进 `Village_KenMuNi1`，看开场 | 框开始出现到第一句字出来之前，左边是空的。第一句「好漂亮的村子。」和雅儿大笑一起出，不要先闪别的脸 |
| 2 | 商店点头 `Village_ShopHead` | 第一句直接是老板娘，不要先闪雅儿得意 |
| 3 | `Village_村长家门口初次对话` | 开关本来就是关的。第一句古莎「奶奶。」，框空的时候不要有雅儿 |
| 4 | 可选：`Village_ShopRepeat`（没有淡入） | 「欢迎~」要有老板娘头像，不能被藏起弄丢 |
| 5 | 后面句子换脸、大立绘淡入 | 和现在一样，大立绘不要丢 |

### ④ 程序补充

见下文。

---

## 改动清单

| 文件 | 改了什么 |
|------|----------|
| `DialogueMaskAvatarPresenter.cs` | 新增 `HideAllMaskAvatars`：藏全部 Painting，并清店/村长占用标记 |
| `NormalDialogueUIAlphaAnimationTaskAction.cs` | 判定为淡入之后、可选预亮之前，先藏 Mask。预亮开关保留 |
| `DialogueTMPUGUI.cs` | `OnDialogueStarted` 再藏一次，照顾没有淡入节点的图 |
| 4 张村庄对话 Prefab | 只把 `PrepareMaskAvatarOnFadeIn` 从 true 改成 false |
| `DialoguePreludeOptions.cs` | 编辑器前奏默认值 true → false |
| `DialoguePreludeBuilder.cs` | 只改了一行过时注释（「默认 true」）。Role=Yaer、Face=Smug 占位没动 |

4 张图：

- `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`（占位仍是雅儿 Laugh）
- `Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab`（占位仍是雅儿 Smug）
- `Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab`（占位仍是雅儿 Smug）
- `Assets/GameRes/Prefabs/Dialogue/Village_出村长家送树屋.prefab`（占位仍是雅儿 Smug）

没改：母体面板、Painting 默认脸、台词节点、分层显现节点、门口/续聊 Setup、其它对话 Prefab。

---

## 为什么这样改

只改开场一张图，商店点头、宝箱、送树屋仍会先闪雅儿得意；面板复用时其它村庄图仍会带出上一张脸。只改代码、不去掉那 4 张的 true，淡入仍会马上再亮一张脸。所以两件都做。

预亮代码没删。以后某张图真要「框和头像一起出」，把开关显式设回 true 即可。
