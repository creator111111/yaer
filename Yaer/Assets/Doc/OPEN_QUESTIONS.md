# OPEN_QUESTIONS

未拍板项先记此处，避免施工员擅自改核心方向。有结论后可删或改「已决议」。

---

## MainItem · CostItem 三语 shopNameSprite 配置 · 2026-07-21

详见：`Assets/Doc/执行文档/0721/MainItem_CostItem_ShopNameSprite三语配置_架构溯源与施工执行说明.md`

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 精灵「秘药」=HP、「灵药」=MP 是否与美术一致？ | 是（对齐 Icon 命名） | 待确认 |
| Q2 | 是否迁目录/改名为 `Item/ShopName/{itemId}{_en|_jp}.png` 以启用 Provider 兜底？ | 本期不迁；手拖 Database | 待确认 |
| Q3 | MaterialItem（虫喙等）是否同批挂三语名图？ | 否，可选后续 | 待确认 |
| Q4 | displayName 与 PNG 中文名是否统一？ | 不强制 | 待确认 |

---

## Village_Shop · Bake 只写 ShopPanel 场景 Name 仍 None · 2026-07-21

详见：`Assets/Doc/执行文档/0721/Village_Shop_Bake只写ShopPanel_场景Name仍None_架构溯源与修复执行说明.md`

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | ShopPanel.prefab 是否每次双烤作镜像？ | 是；进店仍以场景 UI_Shop 为准 | 待确认 |
| Q2 | 是否删除 ShopPanel 以免误导？ | 本期不删，只改 Bake 目标与文案 | 待确认 |
| Q3 | Play 是否允许仅靠 Resolve、不 Bake 场景？ | 底线允许；Editor 验收仍要求场景 Bake | 待确认 |

---

## 序章结束 · 恢复地图选肯姆尼 · 2026-07-21

详见：`Assets/Doc/执行文档/0721/序章结束_恢复地图选肯姆尼_架构溯源与施工执行说明.md`

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 点选后直跳村还是先对白？ | **直跳 `Village_KenMuNi1`** | ✅ 已决议 |
| Q2 | 「二次确认弹窗」= 点关卡后再问「确定去吗？」 | **不要**；点了就进 | ✅ 已决议 |
| Q3 | `UnlockRoad` = 地图路线贴图点亮（≠ 关卡能否点） | **0721 曾决议本期不做** → **0914 正式开做**（见下节） | ⛔ 已撤销 / 见 0914 |
| Q4 | 地图按钮是否显示「肯姆尼」文案 | 逻辑键仍 JingLingVillage；美术另案 | 可选 |

---

## 离开拉普路西 · 章末流程被跳过 · 2026-07-22

详见：`Assets/Doc/执行文档/0722/离开拉普路西_章末流程被跳过_架构溯源报告.md`（v1.1）

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 「下一关」是肯姆尼还是东郊？ | **肯姆尼 `Village_KenMuNi1`**；且 Console 过滤 `MapSelect` 为空 | ✅ 已决议 |
| Q1b | 无 `MapSelect` 却进村时，Editor 里 `RightDoor` 是否与仓库（禁用+空名）一致？ | 先核 Hierarchy；不一致则清脏再复测 | 待验收 |
| Q2 | 字幕滚动期间地图关卡是否必须不可点？ | 对齐 0721 V4；本案主因已非地图误触 | 待确认 |
| Q3 | `SelectPlaceLight` + Submit 是否需防误触？ | 0721 已否二次确认；本案主因已非此项 | 待确认 |

---

## ForestScene · 普通跳跃跳出屏幕 · 2026-07-22

## ForestScene · 普通跳跃跳出屏幕 · 2026-07-22

详见：`Assets/Doc/执行文档/0722/ForestScene_普通跳跃跳出屏幕_架构溯源报告.md`（v1.3 结案）  
施工：`Assets/Doc/执行文档/0722/ForestScene_跳跃飞出场景_解耦施工说明.md`

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1～Q3 | 根因 / 相机 / GravityScale | 见报告 v1.3 | ✅ |
| Q4 | 是否另调 jumpHeight 手感？ | 与本案解耦 | 待确认 |
| Q5 | 修复是否允许改 TownPlayerLocomotion？ | **否**；只改落地检测 + Mask | ✅ 已按此施工 |

---

## 村庄 DNF · 禁止跳跃 · 2026-07-23

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 村内是否允许 Space 跳跃？ | **否**；DNF 式移动禁止跳跃 | ✅ 已施工 |

---

## ForestEast · 史莱姆站上卡住 & 树洞卵卡住 · 2026-07-23

详见：`Assets/Doc/执行文档/0723/ForestEast_史莱姆站上卡住_树洞卵卡住_架构溯源报告.md`

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | PlayerFoot↔OnlyMapObj 矩阵不对称，运行时是否确有接触？ | 本期不改矩阵；史莱姆侧改 Trigger 规避 | ✅ 已按此施工 |
| Q2 | 修史莱姆卡住：改矩阵 Ignore，还是改 `GroundCld`？ | **不改矩阵**（保卵/天琬挡板）；史莱姆 `GroundCld.isTrigger=true`；`BaseMonster.OnDead` 关 `groundCld`；不改 GroundLayerMask | ✅ 已按此施工 |
| Q3 | 树洞爬行中能否直接攻击卵？ | 本期对 WormEgg 取消 `OnCollisionMonster`→`StopMove`；仍靠蹲停普攻打碎 | ✅ 已按此施工 |
| Q4 | 卵前裂缝 E 旁白：保留 / 挪开 / 改文案以免误导开路？ | 建议与卵错开或改提示，避免以为 E 能开路 | 待确认 |
| Q5 | 是否恢复 `PlayerBodyCollider` 挤出订阅？ | **本期不恢复**；先解决 GroundCld / StopMove | ✅ 已决议 |
| Q6 | 可否改 `TownPlayerLocomotion`？ | **否**（与本案无关） | ✅ 已决议 |

---

## ForestEast · 跳跃落到藤蔓（TenWan）身上卡住 · 2026-07-23

详见：`Assets/Doc/执行文档/0723/ForestEast_藤蔓站上卡住_架构溯源报告.md`

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 截图藤蔓是战斗 `TenWan` 还是场景 `TenWanSceneObj`？ | **战斗 `TenWanLogic`**（场景 `Tenwan` 为挡路障碍，另案） | ✅ 侦探结论 |
| Q2 | 是否套用史莱姆同款 `GroundCld.isTrigger=true`？ | **是**（仅 `TenWanLogic`）；不改矩阵、不恢复挤出、不改 GroundLayerMask | 待施工确认 |
| Q3 | 场景障碍 `TenWanSceneObj` 存活态是否也改 Trigger？ | **否**；砍断前实心挡路是设计，误改会拆开路 | ✅ 已决议（勿动） |
| Q4 | 击飞落到藤蔓是否单独修？ | **否**；与跳跃下落同源（死等 IsGrounded） | ✅ 侦探结论 |
| Q5 | 可否改 `TownPlayerLocomotion` / 0722 落地 Mask？ | **否** | ✅ 已决议 |

---

## 对话系统 · 主角对话框表情（小头像 → Prefab 截图）· 2026-07-27

详见：
- 溯源：`Assets/Doc/执行文档/0727/对话系统_主角对话框表情显示_架构溯源报告.md`
- Prefab 技术说明：`Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 截图替换静态图，挂点选哪一层？ | **`DialogueAvatarLoader.GetAvatar`（P0）**；历史同步受益 | 待确认（接线轮） |
| Q2 | 对话历史头像是否也走 Prefab 截图？ | 本期 Prefab **不同步历史**；接线轮再定 | Prefab 阶段否 |
| Q3 | 小头像服装跟谁？存档实时 / 对话线固定 / 截图 Prefab 自带？ | 建议对齐现 Loader：跟 **`PlayerClothesData` 衣服+头饰** | 待确认（接线轮） |
| Q4 | 头像窗分辨率与裁切？ | 字幕条 Mask≈**282×282**、对齐旧 Portrait；**方形** Mask（UISprite）；`Show Mask Graphic=关`；历史≈140 另案 | Prefab 阶段已摆 |
| Q5 | 是否缓存截图 Sprite？缓存键？ | 建议缓存 `(role, clothes, headwear, faceType)`；换装清缓存 | 待确认（接线轮） |
| Q6 | 直接复用 `GoOutStoryYaerPainting` / `YaerPainting`，还是新建「截图专用」Prefab？ | Prefab 阶段：**直接嵌两套母体实例**于 `YaerAvatarRoot` 下互斥；不 Unpack | Prefab 已按此摆 |
| Q7 | `YaerPainting` 脸子物体键名与脚本是否不一致？ | 侦探标风险；接线前核实 | 待核实 |
| Q8 | 雅儿小头像对 `Normal` 是否回退 Smile？ | 建议与 CSV 默认 Smile 对齐 | 待确认 |
| Q9 | 各立绘 Pos/Scale 能否统一？ | **否**；各自定稿。GoOut `(-13.8,-90)/0.65`；YaerPainting `(46.2,-250.7)/0.65`；Amy `(136.1,-264.2)/0.8`；Aliy `(-83.6,-269.7)/0.8`；Gusha `(43,-391)/0.7` | ✅ 全员已确认 |

---

## 雅儿立绘 · 新增 Happy 接入表情系统 · 2026-08-03

详见：`Assets/Doc/执行文档/0803/雅儿立绘新增Happy表情_接入表情系统_架构溯源报告.md`

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 连衣裙线 / `NewGameYaerPainting` 是否本期必做 `Dress_Crown_Happy`？ | **否**；本期只保村线 GoOut `Armor_NoHeadWear_Happy` | ✅ 本期按此执行 |
| Q2 | 小头像四套 `Avatar_Yaer_*` 是否必须同步上 `Happy`？ | 仅大立绘可先不做；要字幕条/历史同步则四套补 `Happy.png` + Pack | ✅ 本期不做（方案 A） |
| Q3 | Mask 内嵌立绘（`YaerAvatarRoot`）是否本期跟 FaceType 变脸？ | **否**；属 0727 接线轮，勿塞进 Happy 最小闭环 | ✅ 本期不做 |
| Q4 | GoOut Happy 节点暂绑 Dress 小裁切 `开心.png`、SizeDelta≈166×134 是否可长期保留？ | **否**；已改为全尺寸 `Face/Dress/0_0009_开心.png`（按 Smile 中心合成）+ Rect 对齐 1078×1497 | ✅ 已施工 |
| Q5 | 是否借本次顺带修 `YaerPainting` 裸枚举键 vs `Dress_Crown_*`（0727 Q7）？ | **否**；禁止借题发挥，另案 | ✅ 本期不修 |

---

## 对话框小头像 · Mask 立绘接线启用 · 2026-08-03

详见：`Assets/Doc/执行文档/0803/对话框小头像_Mask立绘接线状态与启用方案_架构溯源报告.md`

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 挂点是否采用 `DialogueMaskAvatarPresenter` + `OnGetNewStatement`？ | **是**（推荐）；退路为 TMPUGUI 内直接调 Presenter | ✅ 已按此施工 |
| Q2 | 雅儿本期服装：固定 GoOut，还是 Dress↔GoOut 跟存档？ | ~~MVP 固定 GoOut~~ → **第二小步已施工**：按 `PlayerClothesData` 切 Dress↔GoOut（见 0806 Dress 启用） | ✅ 已结案 |
| Q3 | 历史头像是否同期改 Mask？ | **否**；历史继续 `DialogueAvatarLoader` 图集 | ✅ 已按此施工 |
| Q4 | Mask 真源后，四套图集 Happy 是否仍要补？ | **字幕可不补**；仅当历史也要 Happy 时再补 | ✅ 本期不补 |
| Q5 | `OnGetAvatar` 是否禁止再激活旧 `Yaer` Image？ | **是**；避免与 Mask 双影 | ✅ 已按此施工 |
| Q6 | Prefab 默认仅 Gusha Active=1 是否改为全关？ | **是**；由 Presenter 首句驱动 | ✅ 已按此施工 |

---

## 古莎立绘 · 新增 LuoMo 接入表情系统 · 2026-08-03

详见：`Assets/Doc/执行文档/0803/古莎立绘新增LuoMo表情_接入表情系统_架构溯源报告.md`  
**已决议（2026-08-04 与 LuoMo2 同轮施工）**：枚举末尾已追加 `LuoMo`；不入 `spcFaces`；字幕头像走已接线 Mask（不补 Avatar 图集）；Prefab 绑图以现网为准验收。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | LuoMo 中文含义是否为「落寞」？ | **是**（对齐 `DialogueProtrait/落寞.png`） | ✅ 已决议 |
| Q2 | 是否把 LuoMo 加入 `spcFaces` 切 `clothes_other`？ | **否**；与 Happy/Smile 同走正常衣 | ✅ 已决议·不做 |
| Q3 | 小头像 `Avatar_Gusha` + `LuoMo.png` 是否本期必做？ | **否**；字幕用 Mask；图集旧路径/历史 | ✅ 已决议·不做 |
| Q4 | Mask 内 Gusha 是否本期跟 FaceType？ | **是**（Presenter 已接线；加枚举即可跟） | ✅ 已决议·靠现网 |
| Q5 | LuoMo 节点 SizeDelta 是否校正？ | Prefab 现网已与同级脸对齐；验收肉眼 | ✅ 已决议·不动 Prefab |

---

## 古莎立绘 · 新增 LuoMo2 接入表情系统 · 2026-08-04

详见：`Assets/Doc/执行文档/0804/古莎立绘新增LuoMo2表情_接入表情系统_架构溯源报告.md`  
**已决议（2026-08-04）**：枚举末尾 `LuoMo`→`LuoMo2`；不入 `spcFaces`；不补 Avatar 图集；Mask 跟脸靠现网 Presenter；Prefab 已调好不动。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | LuoMo2 中文含义/策划名？与 LuoMo「落寞」关系？ | **变体「落寞2」**（对齐 `落寞2.png`） | ✅ 已决议 |
| Q2 | 是否把 LuoMo2 加入 `spcFaces` 切 `clothes_other`？ | **否** | ✅ 已决议·不做 |
| Q3 | 小头像图集是否本期必做？ | **否**；字幕用 Mask | ✅ 已决议·不做 |
| Q4 | Mask 内 Gusha 是否本期跟 FaceType？ | **是**；Presenter 已接线，加枚举验收 | ✅ 已决议·靠现网 |
| Q5 | LuoMo2 节点 Rect/绑图是否还需校正？ | **否**；用户已调好 | ✅ 已决议·不动 |
| Q6 | 是否同轮顺带补 `DialogueFaceType.LuoMo`？ | **是**；顺序 `LuoMo`→`LuoMo2` | ✅ 已决议·已施工 |

---

## 雅儿立绘 · 批量新增表情接入表情系统 · 2026-08-04

详见：`Assets/Doc/执行文档/0804/雅儿立绘批量新增表情_接入表情系统_架构溯源报告.md`  
**已决议（2026-08-04）**：仅 `DialogueFaceType` 末尾追加 10 项；Prefab/绑图不动；不补图集与 Dress；Mask 靠现网 Presenter；NanGuo/ZhenJing 独立频道。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 新表情中文正式名？ | 对齐源图即可；施工不改名、不改图 | ✅ 已决议 |
| Q2 | `ChiBie2/3`、`GanGa` 是否同轮必做？ | **是**；10 项全加 | ✅ 已决议·已施工 |
| Q3 | 小头像四套是否补？ | **否**；以后也不补图集小头像 | ✅ 已决议·不做 |
| Q4 | Dress `YaerPainting` 是否扩？ | **否** | ✅ 已决议·不做 |
| Q5 | Mask 是否跟新 FaceType？ | **是**；Presenter 已接线，加枚举即可 | ✅ 已决议·靠现网 |
| Q6 | NanGuo/ZhenJing 是否独立频道？ | **是**；不合并 Sad / VerySurprised | ✅ 已决议 |
| Q7 | VerySurprised 错绑是否校正？ | **否**；用户已绑好，禁止改 Prefab | ✅ 已决议·不动 |

---

## 对话框小表情 · 首句未跟 FaceType · 2026-08-04

详见：`Assets/Doc/执行文档/0804/对话框小表情_首句未跟FaceType_架构溯源报告.md`  
**已决议并施工（2026-08-04）**：`GoOutStoryYaerPainting.SetDefaultPainting` 在无 Actor 时跳过强制 Smile；头饰逻辑保留；旧 Portrait 保持关；古莎等暂不改。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | Mask 内 GoOut 是否应在无 `DialogueActorEx` 时跳过强制 Smile？ | **是**；表情交给 Presenter，头饰可保留 | ✅ 已施工 |
| Q2 | 旧 `actorPortrait` / 图集路径是否保持完全关闭？ | ~~**是**；维持 `useMaskAvatar=1`~~ → **0911 混合回退已施工**（Mask 角色仍关；King/Lai/Xiaer/LinEn 可亮旧槽） | ✅ 0911-A′ 已取代 |
| Q3 | 古莎等其它 Mask Painting 是否同有「首次 Start 盖脸」？ | 现网主风险为 GoOut；古莎空 SetDefault，暂不改 | ✅ 已决议·本期仅 GoOut |

---

## 第一章进村 · Village_KenMuNiStart · 2026-08-04

详见：`Assets/Doc/执行文档/0804/第一章进村插入Village_KenMuNiStart_架构溯源报告.md`  
**已决议并施工（2026-08-04）**：`StoryTriggerCountData` 只播一次。  
**时序修订**：主路径改为黑幕 Ready 时 Trigger（见下节）；`OnEnterScene` 仅兜底。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 是否仅同档首次进村播？ | **是，只播一次**；`StoryTriggerCountData` | ✅ 已施工 |
| Q2 | 挂点 SceneManager vs 场景 Trigger？ | **SceneManager**（现为 Ready 全黑时） | ✅ 已施工 |
| Q3 | Prefab 前奏与换场黑幕叠感？ | ~~可接受~~ → **作废**；见下节「遮罩时序」 | ⛔ 被取代 |
| Q4 | 是否与 `homeDoorStoryComplete` 共用一旗？ | **否** | ✅ 已决议 |

---

## 进村开场遮罩时序 · 禁止露景漏缝 · 2026-08-04

详见：`Assets/Doc/执行文档/0804/进村开场对话遮罩时序_禁止露景漏缝_架构溯源报告.md`  
**已决议并施工（2026-08-04）**：A′ — `BaseGameSceneManager.TryDeferBlackFadeForCover` + 村覆写；全黑 Trigger → 前奏幕后 1.8s + snap → CloseFormFade；其它换场默认不变。  
**后续修订（2026-08-06）**：Q3「幕后播完 + snap」导致玩家看不见分层显现 → 见下节「开场分层显现」；零漏缝精神保留，Snap/1.8s 等待须按新报告改。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | Ready 即淡出 vs 对白整段结束才露景？ | **A′ Ready 即淡出** | ✅ 已施工 |
| Q2 | 通用 LoadScene 参数 vs 村专用？ | **村专用旁路** | ✅ 已施工 |
| Q3 | Prefab 前奏？ | ~~幕后播完 + snap~~ → **被 0806 分层显现取代** | ⛔ 被取代 |

---

## Village_KenMuNiStart 开场分层显现 · 2026-08-06

详见：`Assets/Doc/执行文档/0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md`  
**已决议并施工（2026-08-06）**：方案 A+C — 黑幕在仅 BG 盖满后淡出；`PrepareVillageStartLayeredReveal` 取代满不透明 Snap；Prefab 前奏重排为 **框(Delay1+Fade1) → 立绘并行 Fade1**，Fade 阻塞后立刻首句。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 间隔严格 1.0s 还是 Serialized 可调？ | 默认 **1.0**，Prefab Duration/Delay 可调 | ✅ 已施工 |
| Q2 | 立绘后再等 1s 才首句，还是落定立刻可点？ | **落定立刻可点** | ✅ 已施工 |
| Q3 | BG 是否必须全屏盖住村景？ | **是**；现网 Prefab `BG` 1920×1080；旁路确保 Active | ✅ 已施工 |
| Q4 | DialogDebug 是否与正式进村同一套三拍？ | **是**（节奏挂 Prefab；旁路只管黑幕点） | ✅ 已施工 |

---

## 分层显现 · Mask 小头像回归 · 2026-08-06

详见：`Assets/Doc/执行文档/0806/Village_KenMuNiStart_分层后小头像不显示_架构溯源报告.md`（v1.1）  
**已决议并施工（2026-08-06）**：方案 B 白名单 — `PrepareVillageStartLayeredReveal` 只动字幕条 + `DialogueSceneContainer` 下场景大立绘；禁止名字广扫。Presenter.Apply Activate 时补 `alpha=1` 加厚。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Prepare 用排除子树还是白名单？ | **B 白名单**：只动字幕条 + 明确场景大立绘；禁止名字广扫整棵 Panel | ✅ 已施工 |
| Q2 | 是否顺手给 Presenter「Activate 时 alpha=1」防再误伤？ | **是（加厚）**；主修仍在 Prepare 白名单 | ✅ 已施工 |

---

## Village_KenMuNiStart 对话框渐入渐出对齐 · 2026-08-06

详见：`Assets/Doc/执行文档/0806/Village_KenMuNiStart_对话框渐入渐出对齐_架构溯源报告.md`  
**已决议并施工（2026-08-06）**：方案 A — `NormalDialogueUIAlphaAnimationTaskAction` 渐入前 `SetActive(true)`（alpha 仍从 Start）；Prefab 显式 `StartAlpha=0`、Duration=1；结尾 0.7 / BG Fade 本期不动。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 「渐出」是否包含对白结束成对对齐？ | **本期先修开场可见渐入**；结尾 0.7 另拍 | ✅ 已施工（仅开场） |
| Q2 | 框对齐立绘 Duration=1，还是对齐黑幕淡出？ | **对齐立绘 Duration=1** | ✅ 已施工 |
| Q3 | BG 是否补 CanvasGroup Fade 才算三层一致？ | **本期否**；拍1 保持黑幕露 BG | ✅ 已决议不补 |

---

## 雅儿 Mask 小立绘 Dress 启用 · 2026-08-06

详见：`Assets/Doc/执行文档/0806/雅儿Mask小立绘_室内Dress未启用_架构溯源报告.md`  
**已决议并施工（2026-08-06）**：方案 A——`DialogueMaskAvatarPresenter` 按存档 `PlayerClothesData` Clothes 切 GoOut↔Dress + 对应 Face 键；`yaerUseGoOutOnly` 保留为调试强制 GoOut，Prefab/默认均为 false；切 GoOut 时补 `SyncHeadwearFromArchive`。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 服装真源：存档 `PlayerClothesData` vs 镜像大立绘？ | **存档（方案 A）** | ✅ 已施工 |
| Q2 | 皇冠/头饰是否跟 Clothes 子状态？ | GoOut 跟 Headwear；Dress 暂 `Dress_Crown_*` | ✅ 已施工 |
| Q3 | `yaerUseGoOutOnly` 删除还是调试强制？ | **保留调试强制 GoOut，默认 false** | ✅ 已施工 |

---

## 雅儿小头像分类触发机制溯源 · 2026-08-07

详见：`Assets/Doc/执行文档/0807/雅儿小头像_GoOut与Dress分类触发_盔冠机制_架构溯源报告.md`（**v1.1**）  
**侦探结论**：原图集四套 = Mask「Dress 人偶 + GoOut 三头饰态」；机制已在。  
**产品拍板（2026-08-07）**：**继续跟存档**（方案 A）；四态对照表写入报告 §3.1；镜像大立绘（方案 C）本期不做。本期不施工。

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 小头像服装真源：存档 vs 镜像场景大立绘？ | **跟存档（方案 A）** | ✅ 已决议 2026-08-07 |
| Q2 | Dress 是否长期只有 Crown 脸键？ | 现状是；无冠/其它头饰另案 | ⏸ 待确认 |
| Q3 | GoOut 戴冠：只显隐 crown，还是改 Face 前缀？ | **现状显隐**（对齐四态 #3） | ✅ 按定稿；改前缀另案 |
| Q4 | 旧四套 atlas 是否还需与 Mask 一一对应？ | **字幕不必**（人偶+Heads 覆盖四态） | ✅ 倾向不必；历史按需 |

---

## 白天待机帧图重命名 · 2026-08-07

详见：`Assets/Doc/执行文档/0807/雅儿白天待机帧图重命名_Crown_None_Armor_架构侦探执行说明.md`（§⑦ 侦探确认 √）  
**已施工（2026-08-07）**：`战斗服待机 拷贝` 下除帧 1 外共 45 张按「曲线→拷贝/副本→图层」改为 `Armor{N}/Crown{N}/None{N}.png`（含 `.meta`）；验收全帧三态齐全、无 PS 乱名残留。走路目录未改。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | None 层「图层 1367 拷贝」与「图层 81 副本\*」是否都算 None？ | **是**（规则 2） | ✅ 已施工 |
| Q2 | `Armor` 对应运行时「护头」还是「铠甲身体」？ | 另案；本期只统一文件名 | ⏸ 待确认 |
| Q3 | 走路 `冠/护头/无` 是否与待机英文对齐？ | **本期不改走路** | ✅ 已决议 |
| Q4 | 缺帧 5/7/15/17 是否补空/重排？ | **否** | ✅ 已决议 |
---

## NewGameStory 开场分层对齐 KenMuNi 标准 · 2026-08-06

详见：`Assets/Doc/执行文档/0806/NewGameStory_开场分层对齐KenMuNi标准_架构溯源报告.md`  
**原标「已施工」有误**；0807-B 曾回退可玩并行态。  
**2026-08-07 方案 D 已真正成对落地**（见「开场间隔对齐 KenMuNi」节）：System BlackPanel 拍1 + Prefab 串行 0.5。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 漫画结束与拍1：关漫画即露景，还是 System HideFade？ | **System BlackPanel HideFade=拍1** | ✅ 已施工（0807-D） |
| Q2 | Gate/Wait 是否抽成通用名（去 Village 前缀）？ | 功能先复用现闸门；命名可二期 | ⏸ 二期 |
| Q3 | 是否仍保留前奏 BlackMask / YaerShow？ | **主路径去掉**；Fade 后落到 YaerShow 末帧 | ✅ 已施工（0807-D） |

---

## NewGameStory 对话卡死与开场异常 · 2026-08-07

详见：`Assets/Doc/执行文档/0807/NewGameStory_对话卡死与开场异常_架构溯源报告.md`  
**曾决议 B 保可玩**；同日间隔对齐改走 **方案 D**（见下节），B 并行前奏已被 D 串行取代。

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 0806「已施工」是否回滚/误标？ | **误标/半施工**（Prefab 有、旁路无） | ✅ 已结案 |
| Q2 | 本期选 A 补旁路还是 B 回退 Prefab？ | 先 B 保可玩 → 再 D 成对对齐 | ✅ 已被 D 取代 |
| Q3 | Gate 共用是否 NewGame 专用 Reset/Signal？ | 本期复用村 Gate；命名二期 | ✅ 已施工（复用） |

---

## NewGameStory 开场间隔对齐 KenMuNi · 2026-08-07

详见：`Assets/Doc/执行文档/0807/NewGameStory_开场间隔对齐KenMuNi_架构溯源报告.md`  
**决议仍为方案 D**；**旁路脚本已落地**。  
**磁盘漂移曾于 2026-09-11 侦探核实**（Prefab 回 B）；**同日施工员已按方案 A 成对修回**（见 `施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md`）。验收以完整新游戏链路为准。

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 是否丢掉前奏 BlackMask / YaerShow？ | **主路径丢掉**；Fade 后落到 YaerShow 末帧供 KingMove | ✅ Prefab 已施工（0911-A） |
| Q2 | NewGame 是否必须补 Gate 旁路才有「只见 BG」？ | **必须**（方案 D）；拍1 用 System BlackPanel | ✅ 旁路已施工 |
| Q3 | 0806「已施工」与现网并行是否回退/误标？ | 半施工误标 + B 回退后，决议改走 D；0911 已成对修 Prefab | ✅ 已闭环（待 Play 验收） |

---

## NewGameStory 主角大立绘不出现 · Prepare/Prefab 不成对 · 2026-09-11

详见：`Assets/Doc/执行文档/0911/NewGameStory_主角大立绘不出现_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md`

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 终局修法？ | **方案 A**：Prefab 对齐 D 串行 Wait→YaerPainting Fade→UIAlpha；成对现网 Prepare；禁只强 alpha=1 | ✅ 已施工（待 Play 验收） |
| Q2 | 是否允许紧急回退 B（去掉 Prepare 藏立绘/关 Animator）？ | **未采用**（已走 A） | ✅ 不需要 |
| Q3 | DialogDebug 拖同 Prefab 可能「有立绘」是否算通过？ | **否**；验收须完整新游戏漫画链路 | ✅ 已决议（侦探） |

---

## NewGameStory 开局小头像表情闪错 · 2026-09-11

详见：`Assets/Doc/执行文档/0911/NewGameStory_开局小头像表情闪错_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0911/NewGameStory_开局小头像空框取消预亮_施工说明.md`

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 框淡入是否允许预亮小头像？ | **否**；空框 → 首句再出脸（对齐 0902 门口，不对齐 KenMuNi 同拍） | ✅ 已决议 |
| Q2 | 修法？ | **F1**：仅 `NewGameStory` `PrepareMaskAvatarOnFadeIn=false`；禁 F2 改预亮脸冒充 | ✅ 已施工（待 Play 验收） |
| Q3 | 是否改 KenMuNiStart / Presenter / 回退大立绘？ | **否** | ✅ 已决议 |

---

## 旧角色图集小头像全空 · Mask 混合回退 · 2026-09-11

详见：`Assets/Doc/执行文档/0911/旧角色图集小头像全空_Mask模式回退_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0911/旧角色图集小头像_Mask混合回退_施工说明.md`

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 通用「Resolve=null 就亮旧槽」（A）还是仅四人白名单（A′）？ | **A′**：仅 **King / Lai / Xiaer / LinEn** | ✅ 已施工（待 Play 验收） |
| Q2 | 修订 0804「旧 Portrait 完全关闭」？ | **是** → Mask 角色关；白名单旧角色可亮 | ✅ 已施工 |
| Q3 | 其它未点名 Role 是否顺带回退？ | **本期不扩** | ✅ 已决议 |
| Q4 | 哥布林小头像？ | **不要求**；不验收；勿进白名单 | ✅ 已决议 |
| Q5 | 「小孩」是谁？ | **夏尔 Xiaer**（不是哥布林） | ✅ 已钉死 |

---

## 对话结束重复最后一句语音 · 2026-09-11

详见：`Assets/Doc/执行文档/0911/对话结束重复最后一句语音_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0911/对话结束停干净语音_施工说明.md`

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | VO 是否禁止挂 Actor 源、只走 UI localSource / 人声通道？ | 本期先 **A 停干净**；架构倾向只走 UI | 待确认 |
| Q2 | Finished/Paused 是否都 `clip=null` + `playOnAwake=false`？ | **是** | ✅ 已施工（待 Play 验收） |
| Q3 | `OnStoryEnd` 是否再兜一层停 VO？ | **在 Form `OnDialogueEnd` 关壳前兜**（等价、更贴 UI 源） | ✅ 已施工 |
| Q4 | 是否允许在每个 NPC/门入口 Stop？ | **否**（禁止作终局） | ✅ 已决议 |

---

## LoadGamePanel 列表遮罩 · 2026-08-06

详见：`Assets/Doc/执行文档/0806/LoadGamePanel_列表遮罩失效_架构溯源报告.md`  
**已决议并施工（2026-08-06）**：初判 Softness 过大 → 先归零；验收后确认主因是 **ButtonArchive 动态字体 Font Material 非 UI Shader**（背景裁字不裁）。已：`ButtonArchive` 运行时换成 `UI/Default Font` 可遮罩材质；Load/Save Softness **Y=100** 恢复。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | Softness 硬切 `(0,0)` 还是轻软边 `(0, 24～32)`？ | 验收后恢复 **Y=100**（策划要软边） | ✅ 已施工 |
| Q2 | SaveGamePanel 是否一并修？ | **是** | ✅ 已施工 |
| Q3 | Mask 与 RectMask2D 最终只留哪个？ | 主修字体材质后暂双留 | ⏸ 观察 |
| — | 字漏遮罩、背景正常 | `ButtonArchive` → `UI/Default Font` 材质 | ✅ 已施工 |

---

## Village_HomeScene23 进屋无主角 · 2026-08-04

详见：`Assets/Doc/执行文档/0804/Village_HomeScene23_进屋主角不出现_架构溯源报告.md`  
**已决议并施工（2026-08-04）**：策略 A 民居可玩——专用 Manager/Config、EnterPos→LeftBorn、左门回村、右门禁用、Build 登记、村回村落点补 HomeScene4。`Village_House4.unity` 缺失另案。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 本场景正式定位？ | **可玩民居**；商店走 `Village_Shop` | ✅ 已施工 |
| Q2 | 新建 Manager 还是改 House4 复用？ | **新建 `Village_HomeScene23SceneManager`** | ✅ 已施工 |
| Q3 | 与 `Village_Shop` 是否区分？ | **是** | ✅ 已决议 |
| Q4 | `Village_House4.unity` 缺失？ | **另案** | ✅ 本期不修 |

---

## HomeScene4 右门+Npc4 · 2026-08-04

详见：`Assets/Doc/执行文档/0804/Village_HomeScene23_右门回村与Npc4对话组件_架构溯源报告.md`  
**已决议并施工（2026-08-04）**：右门回村 + EnterPos→RightBorn；左门关闭；Npc4 仿 Npc1，`StoryPrefabName=HomeScene1Npc4`。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | Npc4 对话 Prefab 正式名？ | 先用 **`HomeScene1Npc4`** | ✅ 已施工 |
| Q2 | 左门是否永久禁用？ | **是** | ✅ 已施工 |
| Q3 | `nowSceneName` 纠正是否本轮必做？ | **否**；已对齐 | ✅ 已有 |
| Q4 | 进屋无主角是否本轮前置？ | **否**；已施工 | ✅ 已有 |
| Q5 | RightDoor 位置是否先校正？ | 先启用验踩门；踩不到再改 | ✅ 验收时看 |

---

## Village_HomeScene4 → Village_HomeScene23 改名 · 2026-08-04

详见：`Assets/Doc/执行文档/0804/Village_HomeScene4改名Village_HomeScene23_架构溯源报告.md`  
**已按默认建议施工（2026-08-04）**：运行时三位一体 + 文档同轮；旧档不兼容。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 文档（技术说明/0804/0601/OPEN 及正文提及）是否同轮改文件名+替换为 23？ | **是**；历史商店文档可替换并注「曾用名 HomeScene4」 | ✅ 已施工 |
| Q2 | 旧存档 `LastSceneName=Village_HomeScene4` 是否兼容？ | **默认不兼容**；未双写 EnterPos | ✅ 已按此施工 |
| Q3 | 新名是否确认为 `Village_HomeScene23`（非 3、非 2）？ | **是** | ✅ 已施工 |

---

## Village_HomeScene1 进屋 · 2026-08-04

详见：`Assets/Doc/执行文档/0804/Village_HomeScene1_进屋残缺对齐HomeScene4_架构溯源报告.md`  
**已按默认建议施工（2026-08-04）**：专用 Manager/Config + Build + 右门回村 + 双侧 EnterPos；未改龙宫。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 出门主链用左门还是右门？ | **右门**回村；左门保持禁用（缺 Interactive，勿盲开） | ✅ 已施工 |
| Q2 | 从村进屋 Born？ | **`Village_KenMuNi1` → RightBorn** | ✅ 已施工 |
| Q3 | Born Y 是否本轮必调到贴地？ | 先按现坐标验收；飘空/入地再调（参考 23） | ✅ 按此（未调 Y） |
| Q4 | 是否改龙宫 `HomeScene1Manager` 将就村屋？ | **否**；新建 `Village_HomeScene1SceneManager` + 专用 Config | ✅ 已施工 |

---

## Village_KenMuNiStart · 角翅膀帧动画 · 2026-08-04

详见：`Assets/Doc/执行文档/0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构溯源与执行说明.md`  
**已施工（2026-08-04）**：Clip/Controller + `PlayUiAnimatorActionTask` + CSV `Type=Anim` 导入器；Prefab 默认隐藏 Anim_*。  
**须在 Unity 执行一次**：`Tools/Dialogue/Setup KenMuNiStart Horn Wing Anim`（装 Animator/BB/删多余帧），再 `Tools/Dialogue/Import CSV` 导 Generated，合并进成品 Prefab 图。  
**Q2 循环/隐藏已改口（2026-09-18）**：用户只要「这句话还在、没点继续就一直循环；点了再藏」。帧率仍 10fps，等点击再进下一句仍有效。详见 `Assets/Doc/执行文档/0918/Village_KenMuNiStart_角翅膀循环到点击_架构溯源报告.md`。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 动画播完是否自动进下一句？ | **否**；等玩家点继续（字幕仍显示） | ✅ 已按此 |
| Q2 | 帧率 / 循环 / 播完？ | ~~不循环、播完隐藏~~ → **字幕期间循环，点继续再藏**；帧率仍 10fps | ⛔ 0918 已改口 |
| Q3 | CSV Type 正式名与 Extra？ | **`Type=Anim`，`Extra=Anim_Gusha` / `Anim_Yaer`** | ✅ 已施工 |
| Q4 | ID9/17 Text 是否仍作出字幕？ | **是**（Play → Statement） | ✅ 已施工 |
| Q5 | 本期是否坚持 CSV Type 自动生成（非仅手插）？ | **是**；阶段 2 可选手插先验 | ✅ 已施工 |

---

## Shop · 货币金币对接（购买扣款闭环）· 2026-07-13

| ID | 问题 | 施工默认（已按此实现） | 状态 |
|----|------|------------------------|------|
| Q1 | 金币不足用 Console / TipsForm / 两者？ | `[ShopDebug]` Warning；Tips 图集键未齐，暂不调 TipsForm | 待确认 |
| Q2 | 堆叠将超 10：整单失败 vs 买到上限？ | **整单失败**（预校验 held+qty ≤ 10） | 待确认 |
| Q3 | 商店 UI 是否常驻显示持有金币？ | 本阶段不做 | 待确认 |
| Q4 | 成功后数量是否清零？ | **是**（ResetToDefault + RefreshTotal2） | 待确认 |
| Q5 | 出售是否同 PR？ | ~~否：仅 Log「出售结算未接入」~~ → **0920 改口：出售结算另票要做**（出包+加币+双落盘；见下节） | ♻️ 0920 改口 |
| Q6 | 假购买「成功购买生命球」文案 | 改为「购买成功，扣除金币 {total}」 | 待确认 |

---

## Village_Shop 贩卖结算从未接入 · 2026-09-20

详见：`Assets/Doc/执行文档/0920/Village_Shop_贩卖无法卖出素材_架构溯源报告.md`

**侦探结论**：主因是设计债，不是回归。`OnConfirmClick` 在 `!_isBuyTabActive` 时只 `LogSellNotImplemented` 后 return。列表 / 卖价 / 合计是次要；修好后购买分支一行语义都不要动。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 出售成败要不要播 `Village_ShopYes` / `ShopNo`？ | **本期默认不播**；产品另定再接线。禁止擅自复用购买 Yes | 待产品 |
| Q2 | 背包不够时是否播任何对白？ | **否**（与堆叠失败不播 No 同口径）；仅 Console Warning | ✅ 已按此施工 |
| Q3 | 0713 货币节 Q5「出售不同 PR」？ | **作废为工期边界**；本票起要做真实结算 | ✅ 0920 已施工 |

---

## 村民家室内 DayLight 动画 · 2026-08-18

详见：`Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md`  
**产品已决议（2026-08-18）**：Q1 龙宫不开；Q2 House4 + 磁盘 HomeScene3 算村民家要开。  
**Q3 已被 2026-08-22 推翻**（见下节「村民家室内 Bink_DayLight」）。  
**推荐施工**：方案 B（进屋运行时只换 Idle/Walk 片子，状态名不动）；方案 E 已否决。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 龙宫 `HomeScene1/2` 是否开 DayLight？ | **否**；共用 Idle/Walk Clip 也不许改 | ✅ 已决议 |
| Q2 | `Village_House4`、磁盘 `Village_HomeScene3` 是否算村民家？ | **算，要开**；`HomeScene3` 已改名为 `Village_HomeScene45` | ✅ 已决议 |
| Q3 | 屋里眨眼是否做 `Bink_DayLight`？ | ~~否~~ → **2026-08-22 改口：要**；见下节 | ⛔ 被取代 |
| T1 | `Village_HomeScene3` 无 `SceneName` 常量，白名单怎么写？ | 已改名为 `Village_HomeScene45` 并写入白名单 | ✅ 已施工 |
| T2 | House4 `.unity` 缺失时名单写谁？ | **`Village_House4`**（与门 Next、已有常量一致）；补场景另案 | ✅ 已施工（白名单已写入） |
| T3 | 方案 B 的运行时 Clone Override 若换装/裙子验收失败？ | **改走方案 C**（复制 Dress+三套白天控制器）；仍禁止 E | 待验收确认 |

---

## 村民家室内 Bink_DayLight · 2026-08-22

详见：`Assets/Doc/执行文档/0822/村民家室内_Bink_DayLight_架构溯源报告.md`  
**产品已决议（2026-08-22）**：推翻 0818 Q3；村民家白名单内眨眼改 `Bink_DayLight`；C# 状态名仍 `Bink`；龙宫/村街道/Combat 零误伤。  
**推荐施工**：方案 B′（扩 `VillageHomeDayLightAnimApplier` + 补 `Bink_DayLight` 孤岛/Override 行 + 重接 Clip）；方案 E 已否决。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 龙宫 / 村街道是否播 `Bink_DayLight`？ | **否**；仅村民家白名单 | ✅ 已决议 |
| Q2 | C# `RegisterState` 是否改 `Bink_DayLight`？ | **否**；只 remap `Bink` 槽片子 | ✅ 已决议 |
| T1 | 裙子 `Dress/Bink_DayLight` 无 `ArtRes/.../Blink/` 帧，怎么办？ | 侦探默认：**沿用旧 Dress 裙眨眼帧** 或产品另补素材后再接；施工前须肉眼确认 | 待确认 |
| T2 | 四套 `*_Bink_DayLight.anim` 是否必须重接 `Blink/` 九图？ | **是**（现网为旧片复制件，零引用新 GUID） | 待施工 |
| T3 | 底图 + 三套 Override 是否加 `Bink_DayLight` 行？ | **是**（仿 `Idle_DayLight`，否则 Applier `FindEffectiveClip` 取不到铠甲片） | 待施工 |
| T4 | 新 Clip `LoopTime` / `StopTime` 对齐策略？ | **StopTime 对齐旧 Bink**（3.08s / 1.54s）；**`LoopTime=0`** 减 Console Warning | 待施工 |
| T5 | 白名单是否补字面量 `Village_HomeScene3`？ | **可选**；已改名为 45；旧门若仍写 `3` 再补 | 待确认 |
| T6 | Applier 缺 `Bink_DayLight` 行时 fallback？ | 建议与 Idle/Walk 一致：**缺则整单不换或跳过 Bink 并 Warning** | 待施工 |

---

## 白天 Blink 锚点对齐暗版 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/白天眨眼Blink_锚点对齐暗版_架构溯源报告.md`  
**侦探结论（2026-08-22）**：9 张 `Blink/` 全为同宽不同高（353 宽，白天比暗版高 5px），统一 YSCALE；无宽不同例外。护头暗版参考有 `spriteBorder z:155`，施工默认不拷。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| T1 | 9 帧是否全部 YSCALE、无 COPY？ | **是**（None/Crown/Armor 三套均 dest 高 +5px） | ✅ 侦探已核实 |
| T2 | 护头暗版 `spriteBorder z:155` 是否拷到白天 `Armor*.png`？ | **否**（对齐 0818 Idle/Walk 不拷 border） | 待施工 |
| T3 | 眨完切 `Idle_DayLight` 脚位与 Idle1 pivot 不完全相同？ | **接受**；Blink 对齐前摇、Idle 对齐待机，暗版亦如此；验收只要求眨眼三帧内部不跳 | 待验收 |
| T4 | 裙子 `Dress/Bink_DayLight` 是否本期一起改锚点？ | **否**；仍用 `Dress/Idle/Bink/01~03` | ✅ 已决议 |

---

## Village_HomeScene45 NPC45 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_NPC45配置与GSM绑定_架构溯源报告.md`  
**侦探结论（2026-08-22）**：磁盘场景无 `NPC45`（须先保存）；对话 Prefab 已存在为 **`Village_Npc45`**；`Village_HomeScene45SceneManager.cs` 不必改；施工 Duplicate `Npc1` + `StoryPrefabName=Village_Npc45` + Z=0。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | `StoryPrefabName` 写 `Village_NPC45` 还是 `Village_Npc45`？ | **以磁盘为准：`Village_Npc45`**（Import 产出名） | ✅ 侦探已核实 |
| Q2 | Hierarchy 有 NPC45 但磁盘无，施工前要不要保存？ | **要**；或施工员直接 Duplicate `Npc1` 新建 | 待施工 |
| Q3 | NPC45 用哪张场景立绘 Sprite？ | 策划/美术指定；施工时替换 `SpriteRenderer`，侦探不裁定 | 待确认 |
| Q4 | 同屋 `Npc1` 的 `HomeScene1Npc1` 是否本期一并改 `Village_Npc1`？ | **否**；本期只配 NPC45 | ✅ 已决议 |
| T1 | Prefab 内 `npc4`/`npc5` 的 `DialogueActor._name` 仍为 `NPC2` | 图参数键已是 NPC4/NPC5；**对白能播**；显示名不对再改 Prefab | 待验收 |
| T2 | `sceneObjs` YAML 漏写是否挡 Play？ | **不挡**（`OnInit` 重扫）；仍建议保存时同步列表 | ✅ 侦探结论 |

---

## Village_HomeScene45 面包饼干 Item 替换 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_面包饼干Item替换与GSM绑定_架构溯源报告.md`  
**侦探结论（2026-08-22）**：`Object` 下面包/饼干为空壳；`sceneObjs` 仅 NPC45；施工删空壳 + 实例化 `Item/面包`/`饼干` + `sceneObjs` 增至 3；合层装饰须 Disable Renderer；SceneManager.cs 不改。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 合层 `村民家3合层/面包|饼干` 去重方式？ | **场景实例 Disable SpriteRenderer**；不改源 Prefab | ✅ 侦探已裁定 |
| Q2 | Item 摆位用空壳坐标还是 HomeScene1 预制体默认坐标？ | **优先 §4.2 空壳坐标**（与 45 合层对齐） | 待施工 |
| Q3 | 是否改 Item 预制体源？ | **否**；场景侧 PrefabInstance 即可 | ✅ 已决议 |
| T1 | 叠图验收：合层关 Renderer 后是否仍偏位？ | 偏则只调 Item 实例 XY，勿恢复合层 Renderer | 待验收 |

---

## Village_HomeScene45 RightDoor 回村 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_RightDoor回村_架构溯源报告.md`  
**侦探结论（2026-08-22）**：产品主出口 **RightDoor**；现网 LeftDoor 已通（0821 补齐 Interactive）、RightDoor `SceneChangeDoor` 仍 Disable；施工 = 启用右门 + 按 HomeScene23 禁用左门；EnterPos / Manager / Build 已齐。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 主出口 Left 还是 Right？ | **RightDoor**（取代 0821 LeftDoor 决议） | ✅ 产品已拍板 |
| Q2 | LeftDoor 如何处理防双出口？ | **Disable SceneChangeDoor + 清空 Next + Trigger=0**（对齐 HomeScene23） | 待施工 |
| Q3 | 删室内 `ForestScene` EnterPos 残留？ | **可选**；不影响右门回村 | 待施工 |
| T1 | 布局改后 RightDoor Trigger 是否盖住通道？ | Play 踩门；偏了只调 RightDoor Collider | 待验收 |

---

## Village_HomeScene45 隔断墙半透明 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_隔断墙靠近半透明_架构溯源报告.md`  
**侦探结论（2026-08-22）**：`Object/隔断墙` 仅 SpriteRenderer；无 Trigger/脚本；推荐新建 `SpriteFadeOnPlayerFootTrigger` + 子物体 `ProximityTrigger`；不挂 `VillageSceneObjectDepthSort`；现网合层实例无隔断墙叠图。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 靠近时目标 alpha？ | **`nearAlpha=0.4`**（可调 0.35～0.5） | 待施工 |
| Q2 | 是否平滑过渡？ | **是**，`fadeDuration=0.2s` | 待施工 |
| Q3 | 合层 `隔断墙` 去重？ | **现网无需**；若 Prefab 合并复现则 Disable 合层 Renderer | ✅ 侦探已裁定 |
| T1 | Trigger 尺寸是否够大？ | 默认 Box **5.5×14**；Play 踩不进再调 Offset/Size | 待验收 |

---

## Village_HomeScene45 回村门口落点 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_回村门口落点_架构溯源报告.md`  
**侦探结论（2026-08-22）**：`KenMuNi1` EnterPos `Village_HomeScene45` 误绑 `LeftBorn`（x≈62）；`House_Npc45` 在 (-4.39, 5.67)；施工新建 `ExitFrom_HomeScene45` + 改 EnterPos；室内 `RightBorn` 已齐。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | ExitFrom 初始坐标？ | **(-4.30, -2.33, 0)**（按 HomeScene1 门↔Exit 偏移）；Scene 微调 | 待施工 |
| Q2 | `HomeScene23` 是否也要独立 Exit？ | **本期不动**；23 仍绑 LeftBorn | 另案 |
| Q3 | 是否改 `House_Npc45` / 室内 EnterPos？ | **否** | ✅ 已决议 |
| T1 | 落点与门 Trigger 是否重叠卡死？ | Play 往返 3 次；偏则只调 Exit Y | 待验收 |

---

## ExitFrom_HomeScene45 落点纵深 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_ExitFrom落点Y轴_架构溯源报告.md`  
**侦探结论**：EnterPos 已绑 ExitFrom；**H2** `VillageWalkArea` 校正覆盖 Y；用户拖 Y 到树屋视觉高度（5～7）在多边形外故无效；现网 ExitFrom **(7.67,-6.47)** x 错。施工 **(-4.30, 2.90)** 贴 Walk 平台条带。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | ExitFrom 坐标？ | **(-4.30, 2.90, 0)**（WalkArea 内）；勿 y=5～7 | 待施工 |
| Q2 | 是否改 TownPlayerLocomotion？ | **本期否**；场景摆点优先 | ✅ 已决议 |
| Q3 | 是否扩 VillageWalkArea？ | 仅当要坚持站楼梯视觉高度 | 另案 |
| T1 | 拖 ExitFrom Y 是否跟手？ | Walk 带内 Δy<0.15 | 待验收 |

---

## Village_HomeScene45 进屋闪回村 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告.md`（v1）  
**v1 结论**：R1 落点过近 RightDoor。**v1 施工后用户反馈仍闪回** → 见 v2。

---

## Village_HomeScene45 进屋闪回村 · v2 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告_v2.md`（**已被 v3 取代**）

---

## Village_HomeScene45 进屋闪回村 · v3 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告_v3.md`  
**侦探结论（v3 终版）**：**R0 原点踩门** — `MapRight`(18.36)+`RightDoor`(-18.16) 使 Trigger 横跨 x≈0；玩家 `CreatePlayer` 默认 (0,0) 在 `SetPos` 前触发 `RightDoor`；HomeScene1 门在 x≈-1.87 故无事。Play：`[SceneLoad]` 双条；禁用 RightDoor 不闪。施工 **方案 A**：`MapRight.x→28.8`、`RightDoor.x→-30.67` + `EnterFrom_Village`(-24.12) + `leftBornTsf`。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | MapRight / RightDoor 对齐？ | **MapRight.x=28.8；RightDoor.x=-30.67**（对齐 HomeScene1） | 待施工 |
| Q2 | EnterFrom_Village / DefaultBornPos？ | **(-24.12, -3.65)** | 待施工 |
| Q3 | Map.leftBornTsf？ | **手绑 EnterFrom_Village** | 待施工 |
| Q4 | 长期禁用 RightDoor？ | **否**（仅诊断用） | ✅ 已决议 |
| Q5 | LoadSceneComponentGSM 判空？ | 可选顺手修 MissingReference | 待施工 |
| T1 | 进村仅 1 条 SceneLoad？ | 无 2s 内第二条 KenMuNi1 | 待验收 |

---

## KenMuNi1 第三部分相机纵深跟随 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_KenMuNi1_第三部分相机纵深跟随_架构溯源报告.md`  
**侦探结论**：**H1** `FramingTransposer.m_DeadZoneHeight=1` 纵深死区满屏 → W/S 不跟 Y；**H2** 放大 `CameraArea` 只扩 Confiner、不开启跟拍。对照 HomeScene1：`DeadZoneHeight=0`、`YDamping=1`。施工 **方案 B+D**：左翼高台 Trigger 进出切换 Framing；右街恢复现网。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 是否全村统一开 Y 跟？ | **否**；仅第三部分 Zone | ✅ |
| Q2 | OrthographicSize 是否随区变化？ | **本期否** | ✅ |
| Q3 | `CameraArea` 多边形是否再扩？ | **本期不改** | ✅ |
| Q4 | Trigger 初值？ | 曾建议 (-133,21)/(80,58)；**磁盘现 (-55,21)/(200,58) 过大** | ♻️ 见下节缩盒 |
| Q5 | C# API？ | `SetKenMuNiPart3CameraMode` + `VillageCameraDepthFollowZone` 已落地 | ✅ 已施工 |
| T1～T3 | 跟 Y / 右街 / 边界 | — | ♻️ **判定改口中** |

**再改口（2026-08-31）**：触发条件由「玩家进区」改为「**摄像机白框完全 ⊆ Zone**」；Part3 Body 改用户实测表（**ScreenY=0.88**）。  
详见：`执行文档/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_架构溯源报告.md`

---

## KenMuNi1 Part3 · 摄像机框完全进入才切 Body · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_施工说明.md`  
**侦探结论**：现网玩家 Trigger/Contains 切 Profile ❌；应正交相机 AABB ⊆ Zone + 滞回；Part3 Profile 换 ScreenY=0.88 等；磁盘 Zone 宽 200 盖右街**必须缩**；离开 SoftH 倾向 **1** 对齐 VCam YAML。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 用哪台 Camera 算框？ | **`Brain.OutputCamera` / `Camera.main`** | ✅ **已施工**（后被双 VCam 改口覆盖判定源） |
| Q2 | Zone 是否必须缩？ | **已缩**：Center (-133,21) Size (80,58)，右缘≈-93 | ✅ **已施工** |
| Q3 | YDamp=0 瞬时跟 Y？ | **照用户表** | ✅ |
| Q4 | 离开 SoftZoneHeight？ | **1**（静态默认 + 场景 streetProfile） | ✅ **已施工** |
| Q5 | 滞回幅度？ | **0.35** 世界单位（进内缩、出外扩） | ✅ **已施工** |

**返修后仍不稳**：单机 Apply 与白框判定反馈环 → 冷却/重申仅压症状。  
**再改口（2026-08-31）**：**启用双 VCam（原 0822 方案 C）**，停用主路径 Apply。  
详见：`执行文档/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构溯源报告.md`

---

## KenMuNi1 Part3 · 双 VirtualCamera 切换 · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_施工说明.md`  
**侦探结论**：弃单机改 Body；落地 `VCam_Street` + `VCam_Part3`，Zone 只切 Priority；判定曾拍板 **A1=街道路算框**；`CameraComponent` 双写 Follow/Confiner/Size/CancelFollow；删每帧 Reassert Apply；Blend Custom 0.4s。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 判定 A1 还是 A2？ | **A1 街道路算框** | ♻️ **被再改口覆盖**（见下节） |
| Q2 | Blend 时长？ | **0.4s EaseInOut**（`KenMuNi1_StreetPart3_Blends`） | ✅ **已施工** |
| Q3 | 旧 Apply API？ | **保留**；Part3 Zone 改切 Priority | ✅ **已施工** |
| Q4 | Part3 复制 Impulse/Confiner？ | **要**（Confiner 同 CameraArea；Impulse Init 双挂） | ✅ **已施工** |
| Q5 | 场景旧 Profile 字段？ | 留文档对照；运行时以两台 Inspector 为准 | ✅ **已施工** |
| Q6 | 手推双机？ | **E2** 两台 Transform 对齐 | ✅ **已施工** |

**再改口（2026-08-31）**：A1 实测难切上 Part3 → 进区改为 **玩家位置在 Zone 内**；双机与 Priority **保留**。  
详见：`执行文档/0831/Village_KenMuNi1_Part3_玩家进区切双VCam_施工执行说明.md`  
施工：`施工说明/0831/Village_KenMuNi1_Part3_玩家进区切双VCam_施工说明.md`

---

## KenMuNi1 Part3 · 玩家进区切双 VCam · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_玩家进区切双VCam_施工执行说明.md`  
**施工结论**：`VillageCameraDepthFollowZone` 主条件 = `Contains(玩家)` + 滞回；废弃 A1；仍只切 Priority，不 Apply Framing。

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 进区主条件？ | **玩家在 Zone 内**（P1 Contains） | ✅ **已施工** |
| Q2 | 是否删双机？ | **否** | ✅ |
| Q3 | 是否恢复 Apply Body？ | **否** | ✅ |

---

## KenMuNi1 两户门换场 · 2026-08-22

详见：`Assets/Doc/执行文档/0822/Village_KenMuNi1_House_NPC2与村长门无法进屋_架构溯源报告.md`  
**侦探结论**：**House_NPC2** 磁盘七件套 + 室内 GSM/双侧 EnterPos **已齐**（0606/0608 已修）；若仍进不去查 **交互/Collider**（第三部分 y=8.5）。**村长门**：村 YAML **无 `House_Chlef`**（须 Ctrl+S）；室内 **`ForestSceneManager`** + 无 `SceneName` + 无村 EnterPos → **方案 A 全链新建**。

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | `House_Chlef` 是否改名？ | **`House_Chief`**（辨认用） | ✅ 已施工 |
| Q2 | Chief 室内出门用哪扇门？ | ~~LeftDoor；RightDoor 禁用~~ → **2026-09-20：最右也要回村**（开 RightDoor→KenMuNi1，同键 `Village_Chief_House_Door`；左门暂保留） | ✅ **已施工**（见 `施工说明/0920/Village_Chief_House_最右边没法回村_施工说明.md`） |
| Q3 | NPC2 仍进不去是否改 GSM？ | **否**；先 Play 查 E/Collider | ✅ 已决议 |
| Q4 | Chief 复用 Stairs 预制体？ | **是**（对齐 Npc1/NPC2） | ✅ 已施工 |
| Q5 | 村场景是否已保存？ | **House_Chief** 已写入磁盘 YAML | ✅ 已施工 |
| T1 | House_NPC2 按 E 进屋？ | 进 HomeScene2 | 待验收 |
| T2 | HouseDoor 出屋回村？ | ExitFrom_HomeScene2 | 待验收 |
| T3 | 村长门按 E 进屋？ | 进 Chief_House（施工后） | 待验收 |
| T4 | Chief 出门回村？ | ExitFrom_HomeSceneChief 对称；**右门现网仍 Inactive+ForestEast** | ⏳ 见 0920 |

---

## 白天待机走路锚点对齐战斗服 · 2026-08-18

详见：`Assets/Doc/执行文档/0818/白天待机走路_按战斗服锚点对齐_架构溯源报告.md`  
**侦探结论**：72 帧均可按「同尺寸 COPY / 同宽不同高 YSCALE（距底像素不变）」改白天 `.png.meta`；无宽不同、无脚不在底边。Idle 5/7/15/17 两边都缺，不补。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| T1 | 走路参考的 `spriteBorder` 是否必须拷到白天？ | **否**；角色 Sprite 走 Simple，白天保持 `0,0,0,0` | ✅ 已施工（2026-08-18，未拷 border） |
| T2 | 换算后 `pivot.y` 略负（参考走路已有）是否 clamp 到 0？ | **否**；保留负值，避免脚被抬起 | ✅ 已施工（负 y 原样写入） |
| T3 | 裙子 `Dress/*_DayLight` 是否本期对齐？ | **否**；用户未纳入 | ✅ 已决议本期不做 |

---

## 进村点A往右走 · 2026-08-18

详见：`Assets/Doc/执行文档/0818/村庄进村点A往右走_架构溯源报告.md`  
**侦探结论**：Combat Idle 未订左右、进跑按默认朝右 `SetRunSpeed`；点一下 A 在 KeyUp 帧灌了右速后队列被清，`MoveLeft` 赶不上。推荐方案 B′（仅 `Village2_5D` 进跑时按 A/D 同步转向，禁止默认朝右灌速）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| T1 | 村民家 Home（`HomeWalkState.Enter` 无条件 `SetWalkSpeed`）点 A 是否同 Bug？ | **本期不修村屋**；验收若家里也反，另开任务 | 待确认 |
| T2 | 长按 A 是否必须「Enter 当帧物理也绝不出现 +X」？ | **是**；B′ 先转向再写速，避免先右后左 | ✅ 已施工（2026-08-18，仅 CombatRunState.Enter 村庄分支） |

---

## 村庄斜向合速度 · 2026-08-18

详见：`Assets/Doc/执行文档/0818/村庄斜向移动速度叠加_架构溯源报告.md`  
**侦探结论**：村街 Combat 横向 `runSpeed=11.2` 与 Town 纵深 `depthMaxSpeed=5.5` 各自满给、无平面归一；斜向欧氏约 12.48。推荐方案 A（只在 `TownPlayerLocomotion.OnFixedUpdate` 一处按目标走速归一）。禁止改两个 max 冒充修复、禁止斜向清 X、禁止回退 0818 点 A 补丁。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| T1 | 「一样快」是八向同速（纯 D / 纯 W / D+W 欧氏距离接近），还是只要斜向不超过较快轴（纯 W 仍 5.5）？ | **八向同速**；选方案 A。若只想压斜向，可改方案 B | ✅ 已施工（2026-08-18 方案 A） |
| T2 | 目标走速用 `runSpeed` 11.2、`walkSpeed` 4.2、`depthMaxSpeed` 5.5，还是新字段？ | **新字段** `villagePlanarMoveSpeed`，初值 **11.2**（保现网左右；纯 W/S 会变快）。旧 Prefab 序列化为 0 时回退 11.2 | ✅ 已施工（字段 11.2，≤0 回退） |
| T3 | 村民家 Home 是否同期做合速度？ | **本期不改**。家里不开 Town，没有纵深，斜向叠加不存在 | ✅ 已按默认：本期不改家里 |

---

## 村庄斜向横向仍满速 · 2026-08-18

详见：`Assets/Doc/执行文档/0818/村庄斜向横向仍满速_归一未生效_架构溯源报告.md`  
**侦探结论**：`ApplyVillagePlanarMoveSpeedNormalization` 的 `hasH` 只认 `GetAxisRaw("Horizontal")`，村里走路认队列/`GetKey(A/D)`；轴为 0 时进不了 `hasH&&hasV`，横向保持 `SetRunSpeed` 的 11.2，纵深仍满给。推荐方案 A：`hasH` 对齐 `HasVillageExploreHorizontalMoveIntent`，符号用轴否则用键/队列，禁止再用默认朝右当第一数据源。产品「斜向横向必须比纯左右慢（约 0.707）」**已拍板**。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| T1 | 斜向时横向是否必须慢于纯左右？ | **是**；约 0.707×目标走速，合速度仍等于单轴 | ✅ 已决议（开发者 2026-08-18） |
| T2 | 本机 `GetAxisRaw("Horizontal")` 按住 D 是否恒为 0？ | **不挡施工**；归一口径改为与村里意图对齐。可选开 `acceptanceDebugLog` 核实 | ✅ 已施工（hasH 对齐意图；日志含 branch） |

---

## 村庄斜向走路惯性 · 2026-08-19

详见：`Assets/Doc/执行文档/0819/村庄斜向走路惯性_架构溯源报告.md`（**v1.1**）  
**侦探结论（根因）**：斜向松双手后 Town 的 `NONE` 不清横向；Combat 因 `|depthVelocity|` 惯性不退 Idle、不走 `StopMove`。vx≈7.92 叠纵深摩擦 → 斜着滑。  
**已拍板（开发者 2026-08-19）**：**全部不要滑行，松手一律立刻停**（纯横 / 纯纵 / 斜向 / 只松一轴）。v1.0 方案 A（只刹横向、留 0512 摩擦）**作废**。推荐 **方案 A′**：Town 无纵深意图则 `depthVelocity=0`；无横向意图则写 vx=0（`NONE` + `DEPTH_ONLY`）。只改 Combat 退 Idle 刹不住权威 Y。禁止回退 0818 归一、禁止用含惯性的 DepthIntent 每帧清 X、禁止按住 W 时也清纵深。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| T1 | 纯 W/S 松手后的纵深摩擦滑行是否保持？ | **否；立刻停**。覆盖 0512 AC-02 在村街的走路手感 | ✅ 已决议（开发者 2026-08-19） |
| T2 | 斜着走时只松一轴：松开的轴要摩擦还是立刻停？ | **立刻停**。松 W 仍按 D → 纵深立刻 0；松 D 仍按 W → 横向立刻 0 | ✅ 已决议（开发者 2026-08-19） |

---

## 树屋下边围栏穿模 · 2026-08-19

详见：`Assets/Doc/执行文档/0819/Village_KenMuNi1_树屋下边围栏穿模卡住_架构溯源报告.md`  
**侦探结论**：斜围栏只被横竖分开拦 + 纵深默认底边射线易漏扫斜墙 → 穿进 Composite；进去后「重叠且 X-Cast 空则锁 vx」加上 `TryDepenetrate` 只搜 Y → 焊死。推荐方案 A（Distance 推出 + last-free 回滚）。禁止恢复物理硬碰、禁止用锁死速度冒充保险、禁止只加厚这一块当唯一修复。0819 惯性 A′ 已合入，不是本案主因。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| T1 | 贴着围栏走：允许切向滑，还是碰到就硬停？ | **允许贴边滑**。方案 A 放宽「重叠且沿 X Cast 空则锁 vx」，避免再焊死 | 待确认 |
| T2 | last-free 无效（开局就嵌在墙里 / 记录点也重叠）时，是否闪回楼梯中线？ | **本期否**。继续 Distance 法向推 + 日志；写死中线坐标另案 | 待确认 |

---

## CSV Speaker 2/3 映射 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/CSV导入_Speaker2与3映射缺失_架构溯源报告.md`  
**侦探结论**：Import 中止因映射表缺 `2`/`3`（安全中止，非 CSV 解析 bug）。推荐补 `2→NPC2`、`3→NPC3`（对齐现网 `Village_NpcChairChild` / `HomeScene1Npc3` 与 0601 台本）；内置默认与 Default.asset 两处同步。默认不改 CSV 数字 Speaker。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Actor 最终叫 `NPC2`/`NPC3` 还是中文「孩子/妈妈」？ | **NPC2 / NPC3**（方案 A） | 待确认（侦探推荐） |
| Q2 | 立绘图集本期是否占位？ | **否**；空 FaceType → Normal Warning，字幕可播 | 待确认 |
| Q3 | 是否允许策划继续用数字 Speaker，还是规范成简称？ | **本期允许** `2`/`3`；新台本可另议写 `NPC2`/`NPC3` 恒等映射 | 待确认 |

---

## NPC23 接任务对话选项 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_对话末接任务选项_架构溯源报告.md`  
**补丁**：结尾拓扑以 `0820/Village_QuestOffer_NPC23_选项后NPC结尾对白_架构溯源报告.md` 为准（雅尔复读作废）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 接受后雅尔是否要说「好呀！」？ | **作废**；改为 NPC3「太感谢了」（见结尾对白补丁） | ✅ 已否决雅尔复读 |
| Q2 | questId / 采集 objectiveType 何时做？ | 见「藤蔓果任务卡」OPEN；现网图上已有 Accept(Quest_002) | 待确认 |
| Q3 | 场景哪个 NPC 挂 `Village_QuestOffer_NPC23`？ | **`NpcChair`**（现网已挂本 Prefab） | ✅ 已对拍 |

---

## NPC23 选项后 NPC 结尾对白 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Village_QuestOffer_NPC23_选项后NPC结尾对白_架构溯源报告.md`  
**侦探结论**：产品改结尾——拒「我有些忙」后 NPC3「没关系我一会自己去吧」；接「好呀」后 NPC3「太感谢了」。现网拒直接收尾、接仍是雅尔「好呀！」+ Accept。施工：拒插新 Statement；接改 `#14` Actor/文案；Accept 仍在道谢句之后。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 按钮文案要不要句号？ | **保持无句号**（现网「我有些忙 / 好呀」） | 待确认 |
| Q2 | FaceType？ | **12**（与图内 NPC3 请托句一致） | 待确认 |
| Q3 | 「太感谢了」要不要感叹号？ | **不要**（按产品表） | 待确认 |

---

## NPC23 藤蔓果任务卡 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Quest_NPC23_提交藤蔓果任务卡_架构溯源报告.md`  
**侦探结论**：真正接取 = `QuestConfig` 新行（建议 `Quest_002` / `CollectItem` / 藤蔓果×5 / Gold50）+「好呀！」后挂 `QuestAcceptAction`。现网已有 MC「我有些忙/好呀」且 `NpcChair` 已挂 `Village_QuestOffer_NPC23`，缺配置行与 Accept。交 5 果主推交付时查背包扣 `TenWangFruit`（方案 A）；现网 TurnIn 不扣物品、且须 Complete——交付另批。禁止 KillMonster 假行 / 复用 Quest_001。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | questId 是否 `Quest_002`？ | **是** | 待确认（侦探推荐） |
| Q2 | 任务中文标题？ | 草案「妈妈的藤蔓果」；英日见报告 | 待确认 |
| Q3 | 交付时查背包 vs 拾取计数？ | **交付时查背包（方案 A）**；接取批不做扣果 | 待确认 |
| Q4 | 是否要独立 TurnIn Prefab？ | **建议要**（仿埃吉尔）；接取批不做 | 待确认 |
| Q5 | 是否新增 `targetItem` 字段？ | **推荐要**；否则 CollectItem + `targetMonster=TenWangFruit` 临时 | 待确认 |

---

## Quest_002 交时查背包逻辑 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Quest_002_交时查背包逻辑对拍_架构溯源报告.md`  
**侦探结论**：现网**不符合**「交时查背包」。Accept 后 CollectItem 停在 InProgress（无 Complete 来源）；`TurnInQuest`/`QuestTurnInAction` 只认 Complete、不查包不扣果。主推方案 A：InProgress + 背包≥5 → 扣 `TenWangFruit` → TurnedIn + Grant50。禁止刷果推进度。交付对白属任务②。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | CollectItem 是否保留 Complete 状态？ | **可不保留**（交成功 InProgress→TurnedIn） | 待确认 |
| Q2 | 扣果失败怎么办？ | **整次失败**：不 TurnIn、不 Grant | 待确认 |
| Q3 | 扩展旧 TurnInAction 还是新 Action？ | 倾向按 `objectiveType` 分支或新 Action，勿误伤 Quest_001 | 待确认 |
| Q4 | `CanTurnInQuest` 是否对 CollectItem 改查背包？ | 建议新方法 / 按 type 分支，供任务②触发器用 | 待确认 |

---

## Editor PlayerStatsTool 中文乱码 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Editor_PlayerStatsTool中文乱码_架构溯源报告.md`  
**侦探结论**：`Tools` 菜单乱码项与窗口 `δ╬╘PlayerLogic` 均来自 `PlayerStatsEditorWindow.cs` 源码中文编码损坏（含 `U+FFFD` + GBK 碎片）；`AddDateMenuItem`「增加日期」为完好 UTF-8 无 BOM。非 Unity 字体问题。修复：按技术文档恢复中文并以 UTF-8 无 BOM 保存。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Editor 脚本编码规范？ | **UTF-8 无 BOM**（对齐 AddDateMenuItem） | 待确认 |
| Q2 | 是否扫其它已损坏 Editor 文件？ | `Tool/` 下仅本文件含 FFFD；全 `Assets/Editor` 卫生扫描可另批 | 待确认 |

---

## Quest_002 接取后仍播 Offer · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Quest_002_接取后仍播Offer应切循环对白_架构溯源报告.md`  
**侦探结论**：Accept 已通；仍播 Offer 因 `NpcChair`=`SimpleStoryTrigger` 写死 `Village_QuestOffer_NPC23`。须仿埃吉尔做 Trigger 子类，按 `InProgress`+背包切 Prefab（**不用 Complete**）。现网无 Thanks Prefab，至少新建 `Village_QuestThanks_NPC23`（「感谢你」）。任务②报告未出，命名与其提示词对齐。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Thanks Prefab 名？ | **`Village_QuestThanks_NPC23`** | 待确认 |
| Q2 | Success/TurnIn Prefab 名？ | **`Village_QuestTurnIn_NPC23`** | 待确认 |
| Q3 | TurnedIn 后再按 E？ | 首版可暂 Thanks；或另短句 | 待确认 |
| Q4 | 与扣果发奖是否同批？ | **可先只切 Thanks** 修本失败；果够+扣果跟①② | 待确认 |

---

## CSV Speaker 1/4/5 映射 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/CSV导入_Speaker1与4与5映射缺失_架构溯源报告.md`  
**侦探结论**：Import 中止因映射缺 `1`（现场 `Village_NPC1_对话交互.csv` ID1）；开发者要求顺带预留 `4`/`5`。修法同 2/3：两处补 `1→NPC1`、`4→NPC4`、`5→NPC5`。物品交互 CSV 用「雅」已映射。禁止改 CSV 数字为中文当首选。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | FaceType 是否为 NPC1/4/5 加默认？ | **可选**；空列 Warning+Normal 即可 | 待确认 |
| Q2 | NPC5 立绘本期是否占位？ | **否**；无 Prefab 也可先映射 | 待确认 |
| Q3 | 是否继续允许数字 Speaker？ | **本期允许**（对齐 2/3） | 待确认 |

---

## 物品远程点击 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md`  
**侦探结论**：「必须走近」= `RaycastListener.OnClick` 强制与玩家 InteractiveCollider overlap。物品远程点击主推 Listener 增加忽略距离开关（NPC 默认仍要靠近）。对话仍走 SimpleStoryTrigger；HomeScene23 尚无物品实体、仅有 CSV 无 Prefab。禁止放大碰撞冒充远程。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 对话中能否再点物品？ | 跟现网 `Procedure.Pause`（Pause 中 Raycast 直接 return） | 待确认 |
| Q2 | 多物体重叠优先谁？ | 现网可对多个 Listener 各触发；首版可接受或只取第一命中 | 待确认 |
| Q3 | 是否要鼠标手型？ | **本期可不做** | 待确认 |
| Q4 | 物品要不要 E 键提示？ | 以远程点击为主；E 仍近距，可不挂 KeyTips | 待确认 |

---

## Village_HomeScene1 Object 全量配置 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md`  
**侦探结论**：Object 下 7 物（Npc1+六物品）皆为 Sprite 空壳，无 SceneEntity/交互/Story。`objRoot` 已指 Object；`sceneObjs` 仅脏 `None`，重扫亦空。对话 Prefab `Village_Npc1*` 已齐。远程开关已进 `RaycastListener`。施工：仿 NpcChair 补三件套；Npc1 近距 + `Village_Npc1`；物品远程 + 对应 `Village_Npc1_*`。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 物品要不要 E 提示？ | **可不挂** | 待确认 |
| Q2 | Collider 尺寸？ | 对齐可点精灵区 | 待确认 |
| Q3 | Object 是否还有未列子物体？ | 现网仅 7 个 | ✅ 已对拍 |
| Q4 | Npc1 结构是否仿 NpcChair？ | **建议是** | 待确认 |

---

## Village_HomeScene1 进屋黑屏与未注册 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md`  
**侦探结论**：半套配置后 `componentsList` 有 6 处 `None`（饼干干净）。`InitComponents` 对 null NRE 打断 SceneManager → 黑屏；「未注册」为连带（Hierarchical 上饼干已 Init 成功）。GSM/`objRoot`/SceneEntity 已通，非主因。Speaker「1」导入无关。最小修：删 None；可选给 Sync/Init 加 null 防护。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | `InitComponents` 是否加 null 防护？ | **建议加**（治标）；现网仍须清 None | 待确认 |
| Q2 | None 来源与规范？ | 加完 Interactive 后 List 不得留空槽 | 待确认 |
| Q3 | 黑屏是黑幕还是相机？ | 先按 Init 中断修；再验 Fade/Camera | 待验收 |

---

## Village_HomeScene1 Npc1 无 E（对照 HomeScene23）· 2026-08-20

详见：`Assets/Doc/执行文档/0820/Village_HomeScene1_Npc1无E对照HomeScene23_架构溯源报告.md`  
**侦探结论**：三件套/canTouch/Story 与 HS23 大体一致，None 已清。无 E 主差为 Npc1 根 **Z≈0.77**（样板 Z=0）；`Bounds.Intersects` 含 Z → overlap 永假。最小修：Npc1 Z→0；保持近距。对话 Prefab 次要。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 远程物品高 Z 是否也改 0？ | **仅近距要 E 的改 0** | 待确认 |
| Q2 | Body 是否强制对齐 NpcChair 尺寸？ | 先 Z=0；不够再加大 | 待确认 |
| Q3 | overlap 是否忽略 Z（代码加固）？ | 可选；本期优先改资产 | 待确认 |

---

## Village_HomeScene3 → 45 改名与进屋黑屏 · 2026-08-20

详见：`Assets/Doc/执行文档/0820/Village_HomeScene3改名45与进屋黑屏_架构溯源报告.md`  
**侦探结论**：须三位一体改名为 `Village_HomeScene45` + 新建专用 Manager/Config。进不去：未进 Build + 无门指 3（`House_Npc45`→缺失的 `Village_House4`）。黑屏/不可玩：误挂龙宫 `HomeScene1Manager`（Xiaer NRE 风险）+ 错 Config/右门 Forest。非 Object None 型。建议 `House_Npc45` 改指 45。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 哪扇村门进 45？ | **`House_Npc45`** | 待确认 |
| Q2 | 旧档 LastScene=3/House4 兼容？ | **不兼容可接受** | 待确认 |
| Q3 | 文档同轮改名？ | 运行时先；文档可后 | 待确认 |
| Q4 | 白名单是否留 `Village_House4`？ | 可暂留占位 | 待确认 |

---

## Village_HomeScene45 · LeftDoor 无法退出 · 2026-08-21

详见：`Assets/Doc/执行文档/0821/Village_HomeScene45_LeftDoor无法退出_架构溯源报告.md`  
**侦探结论**：进屋/改名侧已通；现网**两扇门都出不了**。LeftDoor 主因：`componentsList: []` 缺 Interactive → `SceneChangeDoor.OnInit` 跳过，走进不调 `LoadScene`。RightDoor：`SceneChangeDoor` 组件 Disable。  
**施工决议（2026-08-21）**：主出口 **LeftDoor**（续完现网半成品：已填 Next/启用换场，仅缺 Interactive）；按 HomeScene23 左门样板补齐 Interactive 子物体 + Listener + EntityControl；RightDoor 保持 `SceneChangeDoor` Disable。EnterPos 仍绑 `RightBorn`（未改）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 45 号屋主出口是 LeftDoor 还是 RightDoor？ | ~~LeftDoor~~ → **已由 0822 产品改 RightDoor**（见下节） | ⚠️ 待重施工 |
| Q2 | 走出后唯一目标是否 `Village_KenMuNi1`？ | **是**；禁止再指 ForestScene | ✅ 已确认 |
| Q3 | 走进即走还是按 E？ | `TriggerWhenMoveIn:1`（走进即换场） | ✅ 已确认 |

---

## Village_Shop · MerchantPainting Trigger 特殊交互对话 · 2026-08-28

详见：`Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md`  
**侦探结论**：点头/点胸走独立 Prefab；店脸 Face1～5；点击方案 B（Collider2D）；胸部 C6+ 分期。磁盘上 Trigger/Head/Chest 尚未落盘。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 0601 Smile/Angry → Face1～5 正式对照是否策划签字？ | **先按报告 §D 表施工**；可事后改 CSV | 待确认 |
| Q2 | 点头/点胸是否可重复触发（存档旗标）？ | **可重复**；不做 CheckStoryUsed | 待确认 |
| Q3 | 胸部线树屋场景名 / 落点是否已有资产？ | **未定**；工程无「树屋」SceneName；C6+ 下期 | 待确认 |
| Q4 | Trigger 是否同步进 `MerchantPainting.prefab` / 合层 Prefab？ | **场景必做**；建议同步 MerchantPainting.prefab | 待确认 |
| Q5 | 若 UI_Shop 挡住头/胸热区怎么处理？ | 调 Collider；禁止全屏挡板 Raycast | 待确认 |

---

## Village_Shop · 商人默认 Face1+Normal 与 Body·YinXian · 2026-08-28

详见：`Assets/Doc/执行文档/0828/Village_Shop_商人默认Face1Normal与Body_YinXian_架构溯源报告.md`  
**侦探结论**：三载体 YinXian 与默认 Active 已齐；缺口是对白结束不 `ResetDefault`（Idle 残留末句 Red）。推荐方案 A。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 对白结束是否强制回 Face1+Normal？ | **是（方案 A）** | 待确认 |
| Q2 | 旧 `商店界面合层.prefab` 是否双写默认 Active？ | **否**；真源=场景 MerchantPainting | 待确认 |
| Q3 | YinXian 是否必须搭配固定脸？ | **否**；Body×Face 正交 | 待确认 |
| Q4 | ShopStart 黑幕期内 Reset 时机？ | 显 UI 前 / hold 内 | 待确认 |

---

## Village_Shop · Head 热区安装 Village_ShopHead · 2026-08-29

详见：`Assets/Doc/执行文档/0829/Village_Shop_Head热区安装Village_ShopHead对话_架构溯源报告.md`  
**侦探结论**：热区/GSM/ResetDefault 已齐；P0 是故事名 `Village_ShopKeeper_HeadClick` ≠ Prefab `Village_ShopHead`（方案 A 改常量）。Prefab 图对齐旧「头_对白台本」而非 `Village_商店点头交互.csv`，须重 Import。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 对外故事名最终以谁为准？ | **`Village_ShopHead`（方案 A）** | ✅ 本报告拍板 |
| Q2 | Prefab 与点头 CSV 不一致谁覆盖？ | **CSV Import 覆盖 Prefab** | 待确认 |
| Q3 | 点胸是否仍用 `…_ChestClick`？ | **是；本期不施工** | ✅ 保持 |
| Q4 | Trigger 是否写回 `MerchantPainting.prefab`？ | 场景已有可验；写回 P2 | 待确认 |
| Q5 | 旧「头_对白台本.csv」是否标注废弃？ | 建议标注勿再 Import | 待确认 |

---

## Village_Shop · Head 悬停光标变化 · 2026-08-29

详见：`执行文档/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md`  
施工：`执行文档/0829/Village_Shop_Head悬停光标Catch_施工说明.md`  
**侦探结论**：复用四态；方案 A 挂 `CursorChangeTrigger`。  
**施工决议（2026-08-29）**：用户选 **Catch**；场景 Head 已挂；`SetShopkeeperHotspotsEnabled` 同步开关 CursorChangeTrigger。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 悬停用哪一档 CursorState？ | **Catch（选项 2）** | ✅ 已施工 |
| Q2 | 是否要第五种新图？ | **否** | ✅ |
| Q3 | 点胸是否同期挂？ | **否** | ✅ |
| Q4 | 对白关热区是否强制 Exit？ | **是**（disable CursorChangeTrigger） | ✅ 已施工 |
| Q5 | OverlapPoint 依赖 MainCamera？ | 商店已 Tag；保持 | ✅ |
| Q6 | Trigger 是否写回 MerchantPainting.prefab？ | 场景已够验；Prefab 同步可选 P1 | 待确认 |

---

## Village_Shop · ESC 退出商店回村 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/Village_Shop_ESC退出商店回村_架构溯源报告.md`  
**侦探结论**：店内 ESC 开菜单为 0713 有意施工；改口为禁菜单 + GSM 订 ESC → 复用 `OnExitClick`。`EnterFrom_Shop` 已齐免动。对白中默认禁 ESC 离店。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 对白中 ESC 是否强退离店？ | **否**（HasRunningStory 忽略） | ✅ 已按默认施工 |
| Q2 | 店内是否完全不要菜单？ | **是**（回村再 ESC） | ✅ 产品 |
| Q3 | EnterPos 与脚底差半步是否接受？ | **接受门外固定点** | 待确认 |
| Q4 | OnClose 是否仍 AllowOpenMenu(true)？ | **保留 true 利回村**；OnOpen 已改 false | ✅ 已施工 |

---

## Village_ShopHead · 雅儿大立绘 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/Village_ShopHead_雅儿大立绘_架构溯源报告.md`  
**侦探结论**：GoOut 已嵌但 alpha=0、无淡入、BB 未绑；方案 A 绑 BB + 短淡入/Alpha=1。默认不改代码。Mask≠大立绘。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 短淡入还是直接显示？ | **A1 短淡入**；序被后续报告改为 **先立绘后框（T1）** | ✅ 时序单已纠正 |
| Q2 | Pos 是否对齐 ShopStart？ | **否**；保留 (-835,52) | 待确认 |
| Q3 | 本单是否顺带 CSV Import？ | **否**；跟 Head 安装单 | 待确认 |

---

## Village_ShopHead · 先立绘后对话框时序 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/Village_ShopHead_先立绘后对话框时序_架构溯源报告.md`  
**侦探结论**：标准为立绘→框（KenMuNi/ShopStart）；ShopHead 现为 Fighting→UIAlpha 先出框。拍板 **T1**；**改拍**前序雅儿大立绘报告 A1「先框后立绘」。BB 已绑；补串行雅 CanvasGroupAlpha。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 立绘与框空拍？ | **Delay=0.5** | ✅ 已施工 |
| Q2 | Duration 0.5 还是 1.0？ | **均 0.5**（对齐 KenMuNi） | ✅ 已施工 |
| Q3 | 若已按错误顺序施工？ | **本单纠正 T1** | ✅ 已施工 |
| Q4 | PrepareMask 是否开？ | **开**（Role=Yaer） | ✅ 已施工 |

---

## MenuPanel · Money 对接商店图片数字与真实货币 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/MenuPanel_Money对接商店图片数字与真实货币_架构溯源报告.md`  
**侦探结论**：复用 `UiSpriteNumberDisplay`；Money 现为静态 `0.png`/`Z.png` 占位。方案 A：挂 DigitStrip + OnOpen 读 `PlayerGoldData.gold`。禁用日历双位。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Money / Money(1) 占位如何处理？ | **隐藏 Money(0.png)**；保留 Money(1) 币标靠左；新建 Money_Digits | ✅ 已施工 |
| Q2 | 最大位数？ | **6**（对齐 Total2） | ✅ 已施工 |
| Q3 | 刷新是否只靠 OnOpen？ | **OnOpen + OnReveal** | ✅ 已施工 |
| Q4 | 是否新建 MenuMoneySpacing？ | **否**，复用 ShopTotalSpacing | ✅ 已施工 |

---

## MenuPanel · Money 显示上限 6 位数 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/MenuPanel_Money显示上限6位数_架构溯源报告.md`  
**侦探结论（当时）**：池/Prefab 已是 6；**C1 仅显示钳制、不钳存档**。  
**⚠️ 改口**：C1 存档不钳 / Q2·Q3 **已废止** → 见下方「游戏金币数据上限 999999」。显示 6 位池仍有效。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 显示上限位数？ | **6（0～999999）** | ✅ 产品（仍有效） |
| Q2 | 溢出策略？ | ~~C1 仅显示钳制~~ → **数据硬顶** | ⚠️ 已改口 |
| Q3 | 存档/刷金是否软顶？ | ~~否~~ → **硬顶 MaxGold** | ⚠️ 已改口 |
| Q4 | 6 位是否裁切需改 Prefab？ | Play 验后再定 | 待验收 |

---

## 开发工具 · 一键加 9999 金币 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/开发工具_一键加9999金币_架构溯源报告.md`  
**侦探结论**：无现成刷金工具；API 齐。方案 A：`Tools/Debug/Add 9999 Player Gold`（仅 Play）→ AddGold+Save；刷新已开 Menu Money。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 菜单名中/英？ | **Add 9999 Player Gold** | ✅ 已施工 |
| Q2 | 是否做 F9 热键？ | **否**（P1） | ✅ 本期不做 |
| Q3 | RefreshMoney 如何公开？ | **private→public** | ✅ 已施工 |

---

## 开发工具 · 刷金自定义金额 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/开发工具_刷金自定义金额_架构溯源报告.md`  
**侦探结论**：现网仅写死 +9999。拍板 **W1 EditorWindow**（`Player Gold Tool…`）自填金额累加；旧 +9999 保留快捷；「设为」P1；输入不钳 6 位。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 入口方案？ | **W1 EditorWindow** | ✅ 本报告 |
| Q2 | 旧 +9999 MenuItem？ | **保留作快捷** | ✅ |
| Q3 | 「设为」本期？ | **P1，非硬门槛** | ✅ |
| Q4 | 并入人物状态窗？ | **否** | ✅ |
| Q5 | 输入是否钳 ≤999999？ | ~~否~~ → **结果不得超 MaxGold**（输入可大，Add 吃顶） | ⚠️ 随数据硬顶改口 |

---

## Village_Shop · 购买成败对话 ShopYes / ShopNo · 2026-08-29

详见：`Assets/Doc/执行文档/0829/Village_Shop_购买成败对话_ShopYes_ShopNo_架构溯源报告.md`  
**侦探结论（0829 当时）**：现网决定只 Log。拍板经 GSM `TryTriggerShopkeeperSpecial`：入包成功→Yes；仅金币不足→No；关旁路才能验 No；成败 Prefab 无大立绘，本期不抄 Head 时序。  
**0830 更新**：Yes/No 接线**已落地**（`TryNotifyPurchaseDialogue` → `TryTriggerPurchaseResult`）。「没钱仍能买」主因是旁路仍开，见 `执行文档/0830/Village_Shop_没钱仍能购买与成败对白验收_架构溯源报告.md`。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 接线方案？ | **A 经 GSM Special** | ✅ 已施工落地 |
| Q2 | 哪些失败播 No？ | **仅金币不足** | ✅ |
| Q3 | bypass 成功是否播 Yes？ | **是** | ✅ |
| Q4 | 堆叠失败播 No？ | **否** | ✅ |
| Q5 | Yes/No 雅大立绘分层？ | **本期否** | ✅ |
| Q6 | 出售播 No？ | **否** | ✅ |

---

## 开发工具 · 刷金支持减少 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/开发工具_刷金支持减少_架构溯源报告.md`  
**侦探结论**：现窗仅累加。拍板 **U1 双按钮**；减少走 `TrySpendPlayerGold`（勿双 Save、不钳 0）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | UI 方案？ | **U1 双按钮** | ✅ 本报告 |
| Q2 | 减少 API？ | **TrySpendPlayerGold（勿双 Save）** | ✅ |
| Q3 | 不足是否钳 0？ | **否** | ✅ |
| Q4 | 一键减 MenuItem？ | **否** | ✅ |

---

## 游戏金币 · 数据上限 999999 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/游戏金币数据上限999999_架构溯源报告.md`  
**侦探结论**：废止「仅显示钳制、存档不钳」。**`PlayerGoldData.MaxGold=999999` 硬顶**；AddGold 触顶丢弃多余；F1 读档修超标（如 21100219）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 上限层？ | **数据硬顶 999999** | ✅ 产品 |
| Q2 | 收口点？ | **PlayerGoldData.AddGold + MaxGold** | ✅ |
| Q3 | 超标档？ | **F1 读档钳**；F2 工具补充 | ✅ |
| Q4 | 触顶多余？ | **丢弃** | ✅ |
| Q5 | 钳回是否立刻 Save？ | **建议钳后 Save 一次** | ✅ 倾向 |

---

## 开发工具 · 商店货单背包数量调试 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/开发工具_商店货单背包数量调试_架构溯源报告.md`  
**侦探结论**：列表=**Buy∪Sell Candidates**（勿用全主道具一键）。拍板 **W1 Shop Bag Tool**；补 Set/delta；清空仅店货；MaxStack=10。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 列表范围？ | **Buy ∪ Sell** | ✅ 本报告 |
| Q2 | UI？ | **W1 EditorWindow** | ✅ |
| Q3 | 补 Set API？ | **推荐 Bag Set 或 Util delta** | ✅ |
| Q4 | ExpectedBuy 7 vs 磁盘 8？ | **以 API 为准** | 待核对 |
| Q5 | 出售测？ | **P2** | ✅ |

---

## Village_Shop · 购买堆叠上限 Console 提示 · 2026-08-29

详见：`Assets/Doc/执行文档/0829/Village_Shop_购买堆叠上限Console提示_架构溯源报告.md`  
**侦探结论**：现网已有 `LogStackOverflow`（Warning + `[ShopDebug]`），扣款前、不播 No。**L0 免施工**，只验满堆点决定必出黄字。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 现网够不够？ | **L0 免施工** | ✅ 本报告 |
| Q2 | Tips UI？ | **本期否** | ✅ |
| Q3 | 多行全报？ | **首行即可** | ✅ |
| Q4 | L1 显示名？ | **P2 可选** | ✅ |

---

## Village_Shop · 没钱仍能购买与成败对白验收 · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_Shop_没钱仍能购买与成败对白验收_架构溯源报告.md`  
**侦探结论**：主因 `bypassGoldCheckForBagJoint` 三处为 true（脚本 / ShopPanel / Village_Shop 场景）；扣款 API 正常；Yes/No 已接线但失败支被旁路挡死。拍板正式默认关旁路（三处同步）；成功对白=ShopYes（≠ShopStart）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 旁路是否仅 Editor / 开发菜单？ | **P1 可选**；本期先默认 false | ⏳ 待产品 |
| Q2 | Menu 与存档金币错觉？ | 现网同源；再报错对 Console | ⏳ 观察 |
| Q3 | 0829「只 Log 无 Trigger」？ | **已过时**；接线已落地 | ✅ 本报告 |
| Q4 | 正式旁路默认？ | **false**（脚本+Prefab+场景） | ✅ **已施工**（见 `施工说明/0830/Village_Shop_关货币旁路默认_施工说明.md`） |

---

## Village_Shop · 数量输入取消闪烁光标 · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_Shop_数量输入取消闪烁光标_架构溯源报告.md`  
**侦探结论**：闪的是 TMP caret；现网仅 caretColor a=0，仍 `caretWidth=1` + `blinkRate=0.85`。拍板方案 A：Helper `ApplyInvisibleInputTextStyle` 一处关死 width/blink/selection；运行时覆盖旧 Prefab，不必拆 InputField。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否保留选区高亮？ | **否**（selection 全透明） | ✅ 本报告 |
| Q2 | 无障碍依赖 caret？ | **忽略** | ✅ |
| Q3 | Prefab 预绑早退不 Apply？ | 施工堵上（已有引用也 Apply） | ✅ **已施工** |
| Q4 | 关闪挂点？ | **Helper 一处**（方案 A） | ✅ **已施工**（见 `施工说明/0830/Village_Shop_数量输入取消闪烁光标_施工说明.md`） |

---

## Village_Shop · 买卖数量输入自动钳上限 · 2026-09-23

施工说明：`Assets/Doc/施工说明/0922/商店买卖数量输入自动钳上限_施工说明.md`  
**结论**：输入层原先只 ≥0；现按行钳——卖=持有，买=`min(gold/price, 堆叠空位)`（读 `PlayerGoldData.gold`）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否读当前金币？ | **是**（`GetPlayerGoldData().gold`） | ✅ 已施工 |
| Q2 | 旁路开时买 max？ | **只钳堆叠空位** | ✅ 已施工 |
| Q3 | 整单预检保留？ | **是**（双保险） | ✅ |
| Q4 | 多行联合总价？ | **是**：`remaining=gold−Σ其它行`；改一行回扫全部买行 | ✅ 2026-09-23 |

---

## Village_Shop · 非首次进店 Village_ShopRepeat · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_Shop_非首次进店Village_ShopRepeat_架构溯源报告.md`  
**侦探结论**：现网二进宫静默（0827 旧产品作废）。拍板 R1：DeferCover 分支 Start vs Repeat；Repeat **每次**播、不写 used；进店黑幕防闪要、结束慢黑幕不要（对齐 Special）；Prefab 仅 Merchant 4 句可播。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 结束慢黑幕？ | **否**（对齐 Special） | ✅ 本报告 |
| Q2 | Debug 进店也播？ | **是** | ✅ |
| Q3 | 0827 二进宫静默？ | **作废**（改 Repeat） | ✅ **已施工** |
| Q4 | Prefab 第 4 句？ | **保留** | ✅ 倾向 |
| Q5 | 每 N 次才播？ | **否，每次非首次** | ✅ **已施工**（见 `施工说明/0830/Village_Shop_非首次进店Village_ShopRepeat_施工说明.md`） |

---

## Village_Shop 再进店对话不显示 · 2026-09-20

详见：`Assets/Doc/执行文档/0920/Village_Shop_再次进店对话不显示_架构溯源报告.md`

**侦探结论**：主类型 **B**（有 Trigger，字幕条 alpha 仍为 0）。`Village_ShopRepeat` 无 UI 淡入；上场 `DialogueEnd` 把 `subtitlesCanvasGroup` 淡到 0。0919 HideAll **不是**主因（脸会在第一句 Apply 回来）。推荐只给 Repeat Prefab 补淡入且 `PrepareMaskAvatarOnFadeIn=false`。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 是否改回二进宫静默？ | **否** | ✅ |
| Q2 | 是否删 0919 HideAll？ | **否** | ✅ |
| Q3 | 是否在共用 `OnSubtitlesRequest` 一律强制 alpha=1？ | **本票否**；只改 Repeat Prefab。其它无淡入图另开 | ✅ **已施工**（见 `施工说明/0920/Village_Shop_再次进店对话不显示_施工说明.md`） |

---

## Village_Shop · Chest 热区安装 Village_ShopChest · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_Shop_Chest热区安装Village_ShopChest对话_架构溯源报告.md`  
**侦探结论**：热区/Special 管线已齐；常量仍指向不存在的 `Village_ShopKeeper_ChestClick`。拍板方案 A：改为 `Village_ShopChest`（对齐 Head）；同步 Editor 路径；Prefab 已 Bind C1～C5、无 C6；默认不做 Chest Catch。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Chest Catch 光标？ | **本期否**（P2） | ✅ |
| Q2 | 先 Bind 还是先改常量？ | 已 Bind → **先改常量** | ✅ **已施工** |
| Q3 | 旧名 ChestClick 作废？ | 本报告 + 0601 建议名过时 | ✅ |
| Q4 | 仅 Rebuild Chest 菜单？ | ✅ **已加** `Rebuild Shopkeeper Chest Prefab Only` | ✅ **已施工** |
| Q5 | 命名方案？ | **A = Village_ShopChest** | ✅ **已施工**（见 `施工说明/0830/Village_Shop_Chest热区安装Village_ShopChest_施工说明.md`） |

---

## Village_ShopChest · 对齐 Head 光标立绘与对话框 Bug · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_ShopChest_对齐Head光标立绘与对话框Bug_架构溯源报告.md`  
**侦探结论**：对话框不出现主因 D1——点胸图无 UIAlpha/壳层（仅 Statement）；立绘 alpha0 无淡入；Chest 无 Catch。拍板对齐 Head T1 + Rebuild（`fadeYaerPortrait=true`）+ 场景挂 Catch；前序「Catch 本期否」改口。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 壳层写哪？ | **Rebuild → Prefab bound** | ✅ **已施工** |
| Q2 | UIAlpha 1:1 抄 Head？ | **是** | ✅ **已施工** |
| Q3 | Fighting 必须？ | **建议有**（金样对齐） | ✅ **已施工**（含 Fighting） |
| Q4 | Chest Catch？ | **本期要做**（改口） | ✅ **已施工** |
| Q5 | EnsureHotspot 挂 Catch？ | **是** | ✅ **已施工**（见 `施工说明/0830/Village_ShopChest_对齐Head光标立绘与对话框Bug_施工说明.md`） |

---

## Village_KenMuNi1 · House_Tree 交互 Village_TreeHouseLock · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_KenMuNi1_House_Tree交互Village_TreeHouseLock_架构溯源报告.md`  
**侦探结论**：磁盘 `Objects` **无** `House_Tree`（须新建/存盘）；方案 A 物体交互三件套 + `StoryPrefabName=Village_TreeHouseLock`；远程 `requirePlayerOverlap=0`；可重复；Prefab 缺 UIAlpha 须补壳；勿做成换景门 / Chest C6。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 可重复？ | **是**（本期） | ✅ **已施工** |
| Q2 | 点击区？ | **门口小盒** | ✅ **已施工** |
| Q3 | 光标 View/Chat？ | **View** | ✅ **已施工** |
| Q4 | Hierarchy 有磁盘无？ | 磁盘已新建落盘 | ✅ **已施工** |
| Q5 | 落点坐标？ | 约 `(9.23,-7.5)`；可 Scene 微调 | ✅ **已施工**（见 `施工说明/0830/Village_KenMuNi1_House_Tree交互Village_TreeHouseLock_施工说明.md`） |

---

## Village_KenMuNi1 · 精灵池中对齐青石围栏遮挡 · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md`  
**侦探结论**：围栏已有 DepthSort；池中缺脚本且 SR 钉死 SceneObject。拍板只改场景：池中挂 `VillageSceneObjectDepthSort`，字段先抄围栏 6/0；不改 C#；精灵池上默认不做。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 精灵池上同挂？ | **本期否**（P1） | ✅ |
| Q2 | 锚点？ | **先自 Transform** | ✅ **已施工** |
| Q3 | Order 必须 6/0？ | **先抄再调** | ✅ **已施工** |
| Q4 | 初始 SceneObject 还原？ | 施工验出村 | ⏳ 验收（见 `施工说明/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_施工说明.md`） |

---

## Village_KenMuNi1 · 老农基础对话交互 · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_KenMuNi1_老农基础对话交互_架构溯源报告.md`  
**侦探结论**：Import 因 Speaker「老人」未映射中止；合层 `农` 仅装饰且 Z≠0。拍板 M1 `老人→老人` + Import `Village_老农打水任务` + Objects/`Npc_Farmer`（Z=0、近距）；本期无 Choice/接任务。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 交互实体名？ | **`Npc_Farmer`** | ✅ **已施工**（见 `施工说明/0830/Village_KenMuNi1_老农基础对话交互_施工说明.md`） |
| Q2 | 合层 `农` Disable Renderer？ | **否** | ✅ |
| Q3 | 下期 QuestId / 打水道具？ | 待策划 | ⏳ |
| Q4 | 老人立绘入库？ | **产品改口：不要立绘（取消 P1）**；见 `执行文档/0830/Village_老农打水任务_取消立绘_架构溯源报告.md` | ✅ 关闭 |

---

## Village_老农打水任务 · 取消立绘 · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_老农打水任务_取消立绘_架构溯源报告.md`  
**侦探结论**：图无立绘淡入节点，但 Yaer 仍嵌 GoOut 大立绘且 BB 已绑、未覆写 alpha=0（源默认 1）。拍板方案 A 删嵌套+清 BB；保留 UIAlpha+双 Actor；PrepareMask 保持关；Mask 小头像对照 TreeHouseLock、本期不改全局。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Mask 小头像是否也必须关？ | **本期否**（对照 TreeHouseLock）；坚持无 Mask → P1 | ✅ |
| Q2 | 下期接受/拒绝 Prefab 也无立绘？ | **是** | ⏳ |
| Q3 | 老人大立绘还会做？ | **产品取消** | ✅ **已施工**（见 `施工说明/0830/Village_老农打水任务_取消立绘_施工说明.md`） |

---

## 获得道具 Tips 横幅 · 艾琳之剑溯源与老农复用 · 2026-08-30

详见：`Assets/Doc/执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md`  
**侦探结论**：剑横幅 = `AddMainItem` + `OpenTipsForm("GetAiLinSword")`（Item）；字在图集 Sprite；`GetItemActionTask` 不弹窗。老农须同两步 + 新 Tip 图；发什么/何时发待产品。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 发道具还是金币？道具 ID？ | 待策划（文案偏钱） | ⏳ |
| Q2 | TipKey / 三语图 / 占位？ | 新图 P0；占位须书面接受 | ⏳ |
| Q3 | 发奖时机？ | **默认完成结算句后** | ⏳ |
| Q4 | 对话内 Tips Action？ | **A1 新 OpenTipsFormActionTask(Item)** | ✅ **已施工**（见 `施工说明/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_施工说明.md`） |
| Q5 | 动态拼字？ | **本期否** | ✅ |

---

## Village_KenMuNi1 · 老农打水空满桶·井交互·接任务 · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_老农打水_空满桶道具与井交互及接任务_架构溯源报告.md`  
**侦探结论**：现网无空/满桶；拍板 I1 两 ID+数量4；Objects/`Well` 换桶+Tips；Quest_003 CollectItem 满桶×4；帮/不帮+Accept；Trigger 对照 Npc23。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 4 空桶 Tips 一次还是四次？ | **一次 ×4 图** | ✅ |
| Q2 | 未接任务点井？ | **可点不成兑换+短反馈** | ✅ **已施工** |
| Q3 | questId？ | **`Quest_003`** | ✅ **已施工** |
| Q4 | 报酬金额？ | 暂定 **Gold 40**（待策划改数） | ⏳ 占位 |
| Q5 | 拒后再谈 `_拒绝之后接受`？ | 本期仍回 Offer（可再帮） | ✅ 简化 |
| Q6 | 井 overlap？ | **先远程 0** | ✅ **已施工** |
| Q7 | 满桶>4 继续打？ | **允许至空桶尽** | ✅ **已施工** |

施工说明：`施工说明/0830/Village_老农打水_空满桶道具与井交互及接任务_施工说明.md`（P0～P2 已落；Tip/Icon 占位）。

---

## Village_老农打水 · 任务进度与存档不同步 · 2026-08-30

详见：`Assets/Doc/执行文档/0830/Village_老农打水_任务进度与存档不同步_架构溯源报告.md`  
**侦探结论**：主因并列——(A) 井/发空不 SavePlayerBag，Accept 的 SaveSpcData 易落旧背包；(B) Collect 真进度在背包，questProgress 假 0/4；(C) TurnedIn 不回 Offer 为设计。修：井与发空后存包。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 每次点井自动存包？ | **是** | ✅ **已施工** |
| Q2 | GetQuestProgress Collect 改读包？ | **P1 建议** | ✅ **已施工** |
| Q3 | 交完无任务循环打水？ | ~~交完可再接~~ → **再改口：当日不可再接**（见下节） | ♻️ **被覆盖** → ✅ 见 0831 施工 |
| Q4 | 交完 Debug 重置？ | 改为预留 **`ResetQuest`**（跳日/Debug） | ✅ **已施工**（`QuestManager.ResetQuest` + Editor 菜单） |
| Q5 | SaveSpcData 架构债？ | 本期止血不重构 | ⏳ |

**改口（2026-08-31 上午）**：交完不必读接取前档；`FarmerQuestStoryTrigger` TurnedIn→Offer；`Quest_003.repeatable=true`；`AcceptQuest` 允许 TurnedIn 重接。  
施工说明：`施工说明/0830/Village_老农打水_交完可再接任务_施工说明.md`

**再改口（2026-08-31 下午）**：**覆盖**上条——交完（TurnedIn）后当日不可再接；再谈走短循环；禁空跑 `_完成结算`；预留 `ResetQuest`。  
详见：`执行文档/0831/Village_老农打水_当日不可再接与禁空跑结算_架构溯源报告.md`

---

## Village_老农打水 · 当日不可再接 + 禁空跑结算 · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_老农打水_当日不可再接与禁空跑结算_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0831/Village_老农打水_当日不可再接与禁空跑结算_施工说明.md`  
**侦探结论**：空跑=结算对白先于 TurnIn，失败仍播完；现网 TurnedIn→Offer 为旧改口须废。拍板 Trigger 对齐 Npc23 短循环；Accept 不再 TurnedIn 重接；新增 `ResetQuest`；无真实日期系统。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 锁粒度？ | **TurnedIn 后锁**（进行中仍催促） | ✅ **已施工** |
| Q2 | 短循环 Prefab？ | **新建 `_今日已完成`** | ✅ **已施工** |
| Q3 | `repeatable`？ | **保持 true**，仅 Reset 后可再接 | ✅ **已施工** |
| Q4 | Reset 清不清桶？ | **只清任务状态/进度** | ✅ **已施工** |
| Q5 | Accept 收紧是否动 Quest_001？ | **方案 A 一刀切**（TurnedIn 一律拒；001 重接也走 Reset） | ✅ **已施工** |

---

## Village_老农打水 · Tips 新图替换空桶与满桶 · 2026-08-31

详见：`Assets/Doc/执行文档/0830/Village_老农打水_Tips新图替换空桶与满桶_架构溯源报告.md`  
**侦探结论**：TipKey/挂点正确；Key 路径仍血珠占位；新图已落盘但中文文件名不会被 OpenTipsForm 取到。施工覆盖三语 GetEmptyWaterBucketx4 / GetFullWaterBucket 后 Pack。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 英日长期共用中文图？ | **暂用中文** | ✅ **已施工** |
| Q2 | 中文文件名源图移出 Atlas？ | 源图仍留备份；可选移走 | ⏳ 可选 |
| Q3 | en 1.7MB 异常图？ | 覆盖即消 | ✅ **已施工** |

施工说明：`施工说明/0830/Village_老农打水_Tips新图替换空桶与满桶_施工说明.md`  
**提醒**：进 Unity 后请 Pack `tipsInfo` / `_en` / `_jp` 再 Play 验收。

---

## 精灵村长立绘 · UI 版 Mask 小表情 Face1/2/3 · 2026-08-31

详见：`Assets/Doc/执行文档/0831/精灵村长立绘_UI版Mask小表情Face123_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0831/精灵村长立绘_UI版Mask小表情Face123_施工说明.md`  
**侦探结论**：SR 源不可直嵌 Mask；新建 `ChiefMaskPainting`（Face1←组2 底 + Face2/3 互斥贴脸）；扩 `DialogueRoleName.Chief`（晚宴 Leader 现为 None）；CSV 用 F2 映射 Smile→Face3、CloseEyes→Face2；勿污染 `DialogueFaceType`。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 叠法底+贴脸还是三张互斥？ | **底+贴脸** | ✅ **已施工**（肉眼可改） |
| Q2 | CSV 策略？ | **F2 运行时映射** | ✅ **已施工** |
| Q3 | Smile→Face3、CloseEyes→Face2？ | **是**（按文件名） | ✅ 施工默认 / ⏳ 产品确认 |
| Q4 | Sad / Laugh →？ | 暂 **Face1** | ✅ 施工默认 / ⏳ 产品确认 |
| Q5 | Prefab 名？ | **ChiefMaskPainting** | ✅ **已施工** |
| Q6 | 本期场景大立绘？ | **否，只 Mask**（门口对白另案要 UI 大立绘） | ♻️ 见下节 |
| Q7 | Actor GO「Leader」改名？ | **否**；RoleName=Chief | ✅ **已施工** |

---

## Village_村长家门口初次对话 · 三人大立绘 + Face123 Import · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md`  
**侦探结论**：Import 红字=仅店行认 Face1～5；须 **C1** 村长分流（`UseChiefPortrait`+`ChiefFace`）；`ChiefPainting` 磁盘仍 SR 须 UI 化；成品 Prefab 挂雅+古+村三立绘+三路淡入；门口 CSV 已写 Face1～3，不走晚宴 F2；场景触发见下节「靠近村长黑幕」。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 门口对白触发点？ | **合层 `村长` 旁新建 `Objects/Npc_Chief`**（Enter+黑幕；**非** `House_Chief`） | ✅ 侦探已拍板（见靠近报告） |
| Q2 | 三立绘站位？ | 施工默认：雅左 / 古中 / 村右（Setup 菜单占位，产品可再调） | ✅ 施工默认 |
| Q3 | 大立绘脚本？ | **复用 `ChiefMaskPainting.Apply`**；Prefab `ChiefPainting` 分离 | ✅ |
| Q4 | 前奏淡入三立绘？ | **是**（Setup 菜单 Prelude 三路 CanvasGroup） | ✅ |
| Q5 | 存档单次？ | **是**（`SingleUseInArchive`）；与 House_Chief 进屋解耦 | ✅ 侦探已拍板 |

---

## Village_KenMuNi1 靠近合层「村长」黑幕播门口初次对话 · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构溯源报告.md`  
**侦探结论**：合层 `村长` 仅 SR、Z≈2.8 → 新建 **`Npc_Chief`** Z=0；**Enter** → BlackPanel Show→全黑 `TriggerStory("Village_村长家门口初次对话")`→壳就绪 HideFade；与 `House_Chief` 进屋解耦；单次存档。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Enter 立刻 vs Stay？ | **Enter** | ✅ |
| Q2 | 黑幕时长？ | 默认 BlackPanel；hold 0.1s / 超时 8s（可调序列化） | ✅ 施工默认 |
| Q3 | 单次键？ | **SingleUseInArchive** + Prefab 名 | ✅ |
| Q4 | 结束后自动提示进屋？ | 否；门手动 | ✅ |
| Q5 | Prefab 未完工先合场景？ | **可**；联调等 Prefab | ✅ |
| Q6 | 碰撞避开门热区？ | Collider 2×2、偏右 offset；Scene 可再微调 | ✅ 施工默认 |

---

## Village_KenMuNi1 靠近黑幕插入女二侧面涂层 · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近黑幕插入女二侧面涂层_架构溯源报告.md`  
**侦探结论**：方案 **C**——与 `Npc_Chief` 同一黑幕；全黑启用预置 **`GushaSidePortrait`**（世界 SR、钉 **SceneObject**）再播 `Village_村长家门口初次对话`；「老人」=**村长/奶奶**（≠老农）；侧面≠ UI `GushaPainting`；禁止二次黑幕。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 对白结束侧面去留？ | **onStoryEnd 关闭**（`hideSideOnStoryEnd=true`） | ✅ 施工默认 |
| Q2 | 对白中走动改遮挡？ | 跟现网 Pause；F1 钉 SceneObject 即可 | ✅ |
| Q3 | 是否挂 DepthSort？ | **本期否** | ✅ |
| Q4 | 精确 XY/Scale？ | 占位 `(-157,-1.55)`；换正图后 Scene 微调 | ✅ 施工占位 |
| Q5 | 世界侧面 + UI 正脸双重古莎？ | 默认同场；嫌多再 P1 隐侧面 | ⏳ |
| Q6 | A/B/C 身份？ | **C**（门口三人戏 + 场景氛围侧面） | ✅ |

---

## 门口三人立绘对白结束 → Loading 进 Village_Chief_House · 2026-08-31

详见：`Assets/Doc/执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md`  
**侦探结论（0831 当时）**：「树屋外」= **A 村长门口三人戏**；**L2** → LoadingPanel 进屋。  
**⚠️ 0902 产品改口**：日常进屋改为 **BlackPanel**；Loading 仅时间跳转。见下节「改黑屏切场」。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 「树屋外」含义？ | **A** 村长门口戏 | ✅ |
| Q2 | 自动进屋后门是否保留？ | **保留** `House_Chief` | ✅ |
| Q3 | 挂点 L1 vs L2？ | **L2** onStoryEnd | ✅ |
| Q4 | 进屋主表现？ | ~~LoadingPanel~~ → **0902 改 BlackPanel** | ♻️ 改口 |
| Q5 | 手动门 ShowLoadingUI？ | ~~建议勾~~ → **0902 改 false** | ♻️ 改口 |
| Q6 | Prefab 未好先合 Load？ | 可先合代码；联调等 Prefab | ✅ 已过 |

---

## Village_村长家门口初次对话 · 加载资源失败修复 · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_村长家门口初次对话_加载资源失败修复_施工执行说明.md`  
施工说明：`Assets/Doc/施工说明/0831/Village_村长家门口初次对话_加载资源失败修复_施工说明.md`  
**结论**：根因 **H1**（Prefab 未落盘）；修法 = Unity 菜单 Setup；壳磁盘在、场景 guid 断链为 P1 不挡 Setup。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 根因 H1～H4？ | **H1** | ✅ |
| Q2 | 场景 KenMuNiStart Missing？ | guid 断链；P1 重挂；不挡 Setup | ⏳ |
| Q3 | 是否改 ResMgr？ | **否** | ✅ |

---

## Village_村长家门口初次对话 · 村长大立绘丢失修复 · 2026-08-31

详见：`Assets/Doc/执行文档/0831/Village_村长家门口初次对话_村长大立绘丢失修复_施工执行说明.md`  
施工说明：`Assets/Doc/施工说明/0831/Village_村长家门口初次对话_村长大立绘丢失修复_施工说明.md`  
**结论**：**H1** 母体 `ChiefPainting` 三脸 Sprite 空；**H1b** 曾缺 png（现已回）；重跑 Setup Chief Painting；门口实例继承母体，非 Alpha 主因。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 根因？ | **H1**（空 Sprite）；H1b 历史 | ✅ |
| Q2 | 须重跑门口 Setup？ | 默认否；母体修好即继承 | ⏳ |
| Q3 | png 来源？ | 磁盘已补齐；保留 meta guid | ✅ |

---

## 门口对白结束 → Loading 进村长家 → 自动播继续对话 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_施工说明.md`  
**侦探结论（0901）**：**C1** `OnEnterScene` + 门闩 → 自动续聊；当时依赖 Loading 盖景。  
**⚠️ 0902**：进屋改黑幕后，遮罩改为 **BlackPanel + 建议 TryDeferBlackFadeForCover（F1′）**；门闩逻辑保留。见「改黑屏切场」。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 挂点 C1/C2/C3？ | **C1**（0902 可迁到 defer 全黑内） | ✅ 已施工 / ♻️ 时序微调 |
| Q2 | 手动 `House_Chief` 是否补播续聊？ | **门闩补播一次** | ⏳ |
| Q3 | 遮罩？ | ~~靠 Loading~~ → **0902 靠 Black / F1′** | ♻️ 改口 |
| Q4 | 续聊 Prefab 三人立绘？ | **是** | ✅ 已 Setup |
| Q5 | Prefab Setup 路径？ | 最小复用门口 Setup 改名 | ✅ 已落地菜单 |

---

## Village_村长家继续对话 · 中途获得针线包 Tips · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_村长家继续对话_中途获得针线包Tips_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_村长家继续对话_中途获得针线包Tips_施工说明.md`  
**侦探结论**：锚句 `$id:36`（CSV 34）后挂 `GetItem(SewingKit,1)` → `OpenTipsForm(GetSewingKit, Item)` → `SavePlayerBag`；补枚举/库/Icon；中文「获得了针线包.png」须覆盖为三语 `GetSewingKit.png` 再 Pack；本期不建 Quest。  
**施工状态（2026-09-01）**：枚举/库/三语图/图集 Pack/续聊 Prefab 三连已落地；Play 验收待用户。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 枚举名？ | **`SewingKit`** | ✅ 已施工 |
| Q2 | TipKey？ | **`GetSewingKit`** | ✅ 已施工 |
| Q3 | 本期建 Quest？ | **否**；只入包+Tips | ✅ |
| Q4 | 续聊 Prefab？ | **已存在**，可直接挂 | ✅ |
| Q5 | 英日 Tip 图？ | 暂共用中文像素 | ✅ |
| Q6 | GetItem 后 Save？ | **要** SavePlayerBag | ✅ |
| Q7 | 中文源图移出 Atlas？ | 施工后建议移走 | ⏳ |

---

## Village_村长家对话 · 村长大立绘 Scale 过小修复 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_村长家对话_村长大立绘Scale过小修复_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_村长家对话_村长大立绘Scale过小修复_施工说明.md`  
**侦探结论**：母体 0.32；门口 0.65 Override **fileID 断链**回落；继续从未写 Scale。施工：两 Prefab Override→当前 RT=`7950…` Scale **0.65** + Setup Nudge 防回潮；勿改母体默认/雅古。  
**施工状态（2026-09-01）**：门口+继续 Scale=0.65；门口断链旧 Override 已清；Door/Continue Setup Nudge 写 Scale；Fix 菜单可重跑。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 母体默认改 0.65？ | **否**；仅对话 Override | ✅ 已施工 |
| Q2 | Y 微调？ | 先只 Scale | ⏳ |
| Q3 | 其它嵌 Chief？ | Dialogue 仅门口+继续 | ✅ |

---

## ChiefPainting · Face2/Face3 贴脸偏离修复 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/ChiefPainting_Face2Face3贴脸偏离修复_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/ChiefPainting_Face2Face3贴脸偏离修复_施工说明.md`  
**侦探结论**：**H1** Face1 Size=`880×2048`（sprite）未满框，Mask Face1=`1128×2625`；Face2/3 Pos 已对齐。施工：Face1 抄 Mask 满框 + Setup 强制防回潮；勿瞎挪 Face2/3；勿改 Mask/SR。  
**施工状态（2026-09-01）**：母体 Face1=`1128×2625`；Setup A+ 已落地。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Face1 强制满框是否拉伸？ | 对齐 Mask；preserveAspect 同 Mask | ✅ 已施工 |
| Q2 | 同步重跑 Mask Setup？ | **否** | ✅ |
| Q3 | 肉眼 1～2px？ | 以 Mask 为准微调 | ⏳ |

---

## Village_Chief_House · 续聊结束黑幕换古莎动画待机 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_施工说明.md`  
**侦探结论**：**G1** GSM 订续聊 `onStoryEnd` → BlackPanel 全黑关 `古莎待机`、开预置 `古莎动画合层`（关「背景」）；改场景引用的 **`Prefab/村长家合层`**；无 Animator 先 SR 合层；读档已换静默 Active。  
**施工状态（2026-09-01）**：合层已预置；GSM 换人+旗已挂；Play 验收待用户。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 对话名？ | **Village_村长家继续对话** | ✅ |
| Q2 | Animator？ | 先 SR 合层；帧动画另案 | ✅ |
| Q3 | 改哪份合层？ | **Prefab/村长家合层**（场景引用） | ✅ 已施工 |
| Q4 | 关「背景」？ | **是** | ✅ 已施工 |
| Q5 | 读档跳过黑幕？ | **是** | ✅ 已施工 |
| Q6 | 替玩家？ | **否** | ✅ |
| Q7 | 续聊已用但旗未立？ | 静默 Apply | ✅ 已按此施工 |

---

## Village_Chief_House · 续聊结束正面古莎未出现（验收排查）· 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_Chief_House_续聊结束正面古莎未出现_验收排查报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_Chief_House_续聊结束正面古莎未出现_施工说明.md`  
**排查结论**：**主因 H2+H8**。场景 `Design/村长家合层` 为**拆包 GO**（未挂 `5cad`），有 `古莎待机` **无** `古莎动画合层`；Setup 只写入 Prefab 资产。`Apply(true)` 关待机、Find 失败 → 正面空白。GSM 换人链本身已落地。  
**施工状态（2026-09-01）**：Setup 已同时 patch **场景合层**（仅动画实例 + GSM 两引用）；菜单/request 跑一次即可。未动其它手改场景设置。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 主因？ | **场景缺动画实例** | ✅ |
| Q2 | 修场景 vs 重挂 Prefab？ | **场景补实例**（免冲手改） | ✅ |
| Q3 | 旧档？ | 修场景后 Q7 恢复 | ✅ |
| Q4 | 缺 Animator？ | 否 | ✅ |
| Q5 | 测档 H1？ | 验收填三键 | ⏳ |

---
## Village_Chief_House · 室内划区 2.5D 与楼梯树屋化 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_施工说明.md`  
**侦探结论（2026-09-01）**：村模式只认 `KenMuNi1` → 进屋无纵深。**A1** 白名单仅加 `Village_Chief_House` + 窄 `VillageWalkArea`（含进门→楼梯条带）+ Y 标尺；再方案1障碍 + 可选 DepthZone；Gate 默认不上；合层 `楼梯` 仅美术；其它 Home 不开。  
**施工状态（2026-09-01）**：`IsVillageExplorationScene` 已落地；场景已摆 WalkArea / DepthY / Obstacles / DepthZone_StairsUpper；Setup 菜单可重跑。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | A1 vs A2？ | **A1** | ✅ 已施工 |
| Q2 | WalkArea 名？ | **VillageWalkArea** | ✅ 已施工 |
| Q3 | 双 Trigger Gate？ | 先 Zone+障碍 | ✅ 已按此施工（无 Gate） |
| Q4 | 区外？ | **夹死** | ✅ 已施工 |
| Q5 | 禁跳？ | **是** | ✅（村模式） |
| Q6 | 与续聊/换古莎？ | 正交；落点进区 | ✅ |
| Q7 | WalkArea 覆盖范围？ | 尽量窄（进门条带+楼梯+小平台）；**Scene 肉眼调点** | ⏳ 验收调点 |

---

## Village_Chief_House · 进场飞出 / 不在 DefaultBornPos（验收排查）· 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_Chief_House_进场飞出DefaultBornPos_验收排查报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_Chief_House_进场飞出DefaultBornPos_施工说明.md`  
**排查结论**：**H1+H2**。从村进屋走 `EnterFrom_Village`（**不用** DefaultBorn）；手调后 WalkArea 底带上沿≈Y−6.4，原 EnterFrom `(17.42,-3.65)` **形外** → ClosestPoint 吸入底带。  
**施工状态**：`EnterFrom_Village` 已对齐 DefaultBorn `(17.1,-6.61)`；`[ChiefEnterPos]` 可关。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 落点锚？ | **EnterFrom**；与 DefaultBorn 对齐 | ✅ 已施工 |
| Q2 | 飞出主因？ | **区外 + ClosestPoint** | ✅ |
| Q3 | 移落点 vs 扩多边形？ | **已移 EnterFrom**（未改多边形） | ✅ |
| Q4 | 读档也飞？ | 验收对比 | ⏳ |
| Q5 | EnterPos 改绑 DefaultBorn？ | 不必须 | ✅ |

---

## Village_Chief_House · 进场落点吸到楼梯（验收排查）· 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_Chief_House_进场落点吸到楼梯_验收排查报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_Chief_House_进场落点吸到楼梯_施工说明.md`  
**排查结论**：**H1+H2**。EnterFrom 已形内仍站楼梯：OnInit 过早 Flush + `SetPos` 不同步 Rb。  
**施工状态**：F1 `TeleportAuthoritativeVillagePos`；F2 权威落点前跳过夹区；F3 Chief 再 Flush；村模式 `SetPos` 走 Teleport。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 主因？ | **过早 Flush + SetPos/Rb 脱节** | ✅ |
| Q2 | F1/F2/F3？ | **F1+F2+F3** | ✅ 已施工 |
| Q3 | 全局 SetPos vs Town API？ | **Town API**；村模式 SetPos 转调 | ✅ |
| Q4 | 读档？ | 同权威 Teleport | ✅ |

---

## Village_Chief_House · 室内走路过快对齐村民家 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_Chief_House_室内走路过快对齐村民家_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_Chief_House_室内走路过快对齐村民家_施工说明.md`  
**侦探结论**：**H1**。Chief 开 `Village2_5D` 吃 `villagePlanarMoveSpeed=11.2`；其它 Home 仍 `walkSpeed=4.2`。**S1**：仅 Chief 覆写 Town 平面目标速为 walkSpeed；村街不变；不撤白名单。Animator 本期不动。  
**施工状态（2026-09-01）**：`IsIndoorVillageExplorationScene` + `ResolveVillagePlanarMoveSpeed` 已落地。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 改 Animator 片？ | **本期否** | ✅ |
| Q2 | 仅 Chief？ | **是** | ✅ 已施工 |
| Q3 | Inspector 另调速？ | 默认同 walkSpeed | ✅ |
| Q4 | 测试面板改速？ | 验收排除 | ⏳ |

---

## Village_村长家继续对话 · 三人大立绘摆位对齐门口 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_村长家继续对话_三人大立绘摆位对齐门口_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_村长家继续对话_三人大立绘摆位对齐门口_施工说明.md`  
**侦探结论**：续聊雅 Painting `(-380,52)` vs 门口 `(348,52)`；Actor 村长续聊 `(0,0)` vs 门口 `(1156,-232)+Y180`。Setup 仍写死 `-380` 会冲门口。施工：整树抄门口 + Door/Continue Nudge 改定稿。  
**施工状态（2026-09-01）**：Continue Prefab 已对齐；共享 `VillageChiefDialoguePortraitLayout`；Door/Continue Nudge 同源。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 门口雅 348 最终？ | **是** | ✅ 已施工 |
| Q2 | Door Setup 一并改？ | **是** | ✅ 已施工 |
| Q3 | 共享 Layout 类？ | **是**（`VillageChiefDialoguePortraitLayout`） | ✅ 已施工 |

---

## Village_Chief_House · 楼梯上楼换场巨树 2 楼 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_Chief_House_楼梯上楼换场巨树2楼_施工说明.md`  
**侦探结论**：EnterPos `Village_Chief_House`→`ExitFrom_HomeSceneChief2f` 已配对；缺楼梯顶门 + **W1** 切 `VillageWalkArea2`（禁止改其形状）。1 楼 `LeftDoor` 冲突用 **E3′**（`enterPosKey` + 新建 1f `ExitFrom_HomeSceneChief`）。Trigger 走进即切；黑幕对齐 LeftDoor；回程原「本期不做」。依赖室内划区 A1。  
**施工状态（2026-09-01）**：enterPosKey / W1 Override / 1f ExitFrom+EnterPos / LeftDoor 键已落地；楼梯门靠 Setup 菜单摆 `StairsDoor_ToTree2f`（须开 Unity 跑一次）。  
**验收（2026-09-03）**：现网曾卡 2 楼 —— 根因见 **0903 DepthGap**；**已施工补标尺 + F_D2/F_Order**，请重验「2 楼可达+W1」。  
**回程（2026-09-20）**：产品要做。见 `执行文档/0920/Village_KenMuNi1_树洞上面没法回村长家_架构溯源报告.md`——2 楼树洞旁无换场门；`House_Chief` 在 Y≈1.9 够不着。推荐 2 楼新门 + Chief 楼梯顶新 EnterPos 键。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 上楼 TriggerWhenMoveIn？ | **true** | ✅ 已施工 |
| Q2 | 1 楼门 vs 2 楼落点？ | **E3′** | ✅ 已施工 |
| Q3 | WalkArea2 生效？ | **W1**；不改形状 | ✅ 代码已施工；**联调见 0903 重验** |
| Q4 | 黑幕 / Loading？ | **黑幕** | ✅ |
| Q5 | 2 楼回程进村长家？ | **要做**：2 楼树洞旁新门 + 楼梯顶落点键（0920 报告） | ✅ **已施工**（见 `施工说明/0920/Village_KenMuNi1_树洞上面没法回村长家_施工说明.md`） |
| Q6 | 同场景下树切回 WalkArea？ | 最小：仅进 2f 绑 2 | ✅ |
| Q7 | 室内 A1 可玩？ | 依赖前案 | ⏳ 验收 |
| Q8 | 2 楼可达联调？ | DepthGap 已补 F_D1/D2/Order | ⏳ 待重验 |
| Q9 | 回程落到大门还是楼梯顶？ | **楼梯顶**（勿复用 `EnterFrom_Village`） | ⏳ 产品默认按 0920 |

---

## Village_KenMuNi1 · 巨树 2 楼 WalkArea2 宝箱 Hp/Mp×3 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_KenMuNi1_巨树2楼WalkArea2宝箱HpMp×3_施工说明.md`  
**侦探结论**：**B1** 仿 WestRapp；新建 `VillageKenMuNi1HpMpBox`（默认 3/3，无 Story）+ `VillageKenMuNi1Data.tree2fHpMpBoxOpened`；实例化 `Prefabs/Box.prefab` 放 WalkArea2 内（建议近 ExitFrom 错开）；`GetHpBall`→`GetMpBall`；**禁止**改 WalkArea2、禁止挂西境脚本。  
**施工状态（2026-09-01）**：Data + 村脚本已落地；场景箱靠菜单 `Setup KenMuNi1 巨树2楼 WalkArea2 宝箱 HpMp×3`（或 request 自动跑）。  
**验收（2026-09-03）**：磁盘 **已有** `Objects/Tree2fHpMpBox@(-152,41.2)` 且∈WalkArea2——见 **0903 宝箱看不见验收排查**；「看不见」≠未摆。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 交互？ | **点交互** | ✅ 已施工 |
| Q2 | ×3 专图？ | **否** | ✅ |
| Q3 | 脚本？ | **新建村用类** | ✅ 已施工 |
| Q4 | 存档？ | **VillageKenMuNi1Data** | ✅ 已施工 |
| Q5 | 对白？ | **无** | ✅ |
| Q6 | 开箱强制 SaveBag？ | 否（对齐 West） | ⏳ 产品另开 |
| Q7 | 上游 2 楼可达+W1？ | 依赖 | ⏳ **0903 DepthGap 已施工**；待重验 |
| Q8 | 场景箱可见/可互动？ | 磁盘已摆；**V1 Sorting=50** 已施工 | ⏳ Play 开箱待重验 |

---

## Village_KenMuNi1 · 巨树 2 楼 WalkArea2 宝箱看不见未摆放 · 2026-09-03

详见：`Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查报告.md`  
提示词：`Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_验收排查提示词.md`  
施工：`Assets/Doc/施工说明/0903/Village_KenMuNi1_巨树2楼WalkArea2宝箱看不见未摆放_施工说明.md`  
**侦探结论**：**非未摆**。磁盘已有 `Tree2fHpMpBox` @ `(-152,41.2)`，PIP∈WalkArea2，村脚本/sceneObjs/去 HomeScene2Box 齐全。用户「看不见」优先 **H0 本机不同步 ∪ H6 视口/卡住误判（箱在 ExitFrom 东侧）∪ 可选 H1 Sorting**。推荐先 Hierarchy 搜+Frame；无箱再 V2 Setup；Game 被挡再 V1 抬 Order。**严禁**改 WalkArea2 / 挂西境箱。与卡住案解耦。  
**施工状态（2026-09-03）**：**V1** 实例 `SortingOrder=50` + Setup 幂等写入已落地；本机无箱仍跑 V2 Setup。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 磁盘是否已摆？ | **是** @(-152,41.2) | ✅ 本报告 |
| Q2 | 「看不见」主因？ | **H0∪H6（±H1）** | ✅ |
| Q3 | 是否改 WalkArea2？ | **否** | ✅ |
| Q4 | 施工？ | **V1 Sorting=50**；无箱→V2；开箱 Play 待重验 | ✅ V1 已施工 |
| Q5 | 与卡住案？ | **解耦**；走不到≠未摆 | ✅ |

---

## Village_KenMuNi1 · 上楼巨树 2 楼 WalkArea2 卡住不动 · 2026-09-03

详见：`Assets/Doc/执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md`  
提示词：`Assets/Doc/提示词/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构侦探提示词.md`  
施工：`Assets/Doc/施工说明/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_施工说明.md`  
**侦探结论**：**主因 DepthGap**：KenMuNi1 **无** `VillageDepthY_Min/Max`，Prefab `depthYMaxWorld=8`，ExitFrom/WalkArea2 在 Y≈41；权威 Teleport/每帧 Clamp 与 W1 ClosestPoint **撕扯** → 未稳 ExitFrom、区里卡死。W1/门/EnterPos **非缺席**；ExitFrom **在** WalkArea2 形内。推荐 **F_D1** 摆标尺（Max≥45）+ **F_D2** W1 按 bounds 抬 Max；可选先 Override 再 Teleport。**严禁**改 WalkArea2 形状 / 关 ClosestPoint。  
**施工状态（2026-09-03）**：F_D1（场景 Min=−20 / Max=46 + Editor 菜单）+ F_D2（W1 按 poly.bounds 抬 Max）+ F_Order（先 Override 再 Teleport）已落地。  
**验收复测（2026-09-03）**：见 `执行文档/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查报告.md`——**WalkArea2 形状不是主因**；磁盘标尺/楼梯路径已在；用户「仍卡」优先查本机 DepthY + Console `[Village2f]`（待 Play）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 主因？ | **DepthGap（maxY=8 vs Y≈41）+ WalkArea2 撕扯** | ✅ 侦探拍板 |
| Q2 | 方案？ | **F_D1 + F_D2 + F_Order** | ✅ 已施工 |
| Q3 | 是否改 WalkArea2 形状？ | **否** | ✅；验收复申 **否** |
| Q4 | W1 是否重写？ | **否**；保留 Override，只补标尺/时序 | ✅ |
| Q5 | 障碍 H4？ | 标尺 Play 通过后再复测 | ⏳ |
| Q6 | 修后仍卡？ | 磁盘施工在；先本机 A1/A2/A3 Play | ⏳ 用户自检 |

---

## Village_KenMuNi1 · 巨树 2 楼仍卡住 WalkArea2 嫌疑 · 2026-09-03

详见：`Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查报告.md`  
提示词：`Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼仍卡住_WalkArea2嫌疑_验收排查提示词.md`  
**验收结论**：**不是 WalkArea2 多边形坏了**。仓库已有 `VillageDepthY_Min=-20` / `Max=46`；楼梯 `SetPlayerPos` 有 F_D2+Override+Teleport；WalkArea2 点集未改；ExitFrom≈`(-157.65,41.66)` 仍 PIP∈区。用户仍卡 → 先 Hierarchy/Console 证本机生效；日志齐全仍卡再查 A4。**禁止改 WalkArea2 形状。**

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 是 WalkArea2 形状问题？ | **否** | ✅ |
| Q2 | 磁盘施工是否在？ | **是**（F_D1/D2/Order） | ✅ |
| Q3 | 用户仍卡下一步？ | 本机 DepthY + `[Village2f]` 自检 | ⏳ Play |
| Q4 | 可否改多边形？ | **否** | ✅ |

---

## Village_KenMuNi1 · 巨树 2 楼进层对白与开箱 Tips · 2026-09-03

详见：`Assets/Doc/执行文档/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_架构溯源报告.md`  
提示词：`Assets/Doc/提示词/0903/Village_KenMuNi1_巨树2楼_进层对白与开箱Tips_架构侦探提示词.md`  
**侦探结论**：Tree/CSV **已有**；缺 **Dialogue Prefab×2**、缺进层挂点、开箱图无发奖/开箱节点；箱仍 `useStoryOnOpen=0` 直发。拍板 **E1**（`LastScene==Village_Chief_House` + `StoryTriggerCount` 单次播进层）+ **B1**（`useStoryOnOpen=true` + ExecuteFunction→`OnOpenBox`/`OnGetHpMp`）。Tips 复用 GetHpBall→GetMpBall，**否**×3 专图；存档仍 `tree2fHpMpBoxOpened`。大门路径不播进层；同场景爬楼另案。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 进层触发？ | **E1** GSM 楼梯键 | ✅ 侦探拍板；待施工 |
| Q2 | 开箱？ | **B1** Story + ExecuteFunction | ✅ 待施工 |
| Q3 | Prefab 壳？ | **须新建两壳** | 待施工 |
| Q4 | 单次键？ | 进层=戏名；开箱=`tree2fHpMpBoxOpened` | ✅ |
| Q5 | 非楼梯进 2 楼也播？ | **本期否**（仅 Chief 楼梯） | ✅ 默认 |
| Q6 | Tips×3 专图？ | **否** | ✅ |
| Q7 | HasRunningStory 跳过？ | 施工须防「永跳」 | ⏳ |

---

## Village · 出村长家古雅对白转场树屋门口 · 2026-09-01

详见：`Assets/Doc/执行文档/0901/Village_出村长家_古雅对白转场树屋门口_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0901/Village_出村长家_古雅对白转场树屋门口_施工说明.md`  
**侦探结论**：**O1+G1+T1**。1 楼出门须 **E3′** 落门前 `ExitFrom_HomeSceneChief`（勿与 2 楼抢 EnterPos）；KenMuNi1 `OnEnterScene` 认门前键播一段 `Village_出村长家送树屋`；中段新建 BlackPanel 传送 Action → `House_Tree` 旁 Walk 内点再段 B。2 楼回来不播；不进树屋 Scene；晚宴旗本期不接。  
**施工状态（2026-09-01）**：`BlackFadeTeleportPlayerActionTask` + CSV + G1 已落地；Prefab/传送点靠菜单 `Setup Village 出村长家送树屋`（或 request）。E3′ 已依赖上游。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 出门落点 O1？ | **是** + E3′ | ✅ 已施工 |
| Q2 | 转场？ | **BlackPanel** | ✅ 已施工 |
| Q3 | Prefab？ | **一段 + 中段传送** | ✅ Setup |
| Q4 | 树屋落点？ | `TeleportTo_YaerTreeHouseDoor`；近 House_Tree；Y∈Walk | ✅ Setup 摆点 |
| Q5 | 场景古莎跟随？ | **不强制** | ✅ |
| Q6 | 晚宴旗？ | **本期否** | ✅ |
| Q7 | E3′ 已施工？ | 本戏依赖 | ✅ 上游已有 |
| Q8 | 转场保持对白壳？ | **倾向保持** | ✅ Action 不关壳 |

---

## Village_村长家门口初次对话 · 框出时空头像 · 2026-09-02

详见：`Assets/Doc/执行文档/0902/Village_村长家门口初次对话_框出时空头像_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0902/Village_村长家门口初次对话_框出时空头像_施工说明.md`  
**侦探结论**：**H1 成立**。门口 Prefab 框 FadeIn 勾了 `PrepareMaskAvatarOnFadeIn=true`（Yaer/Smug），与清字同拍 → 空字却有雅儿 Mask；首句实为古莎「奶奶。」Happy。施工默认 **F1**：仅门口关预亮；**禁止**动 KenMuNiStart「框+头像同拍」。H2/H3/H4 非主因。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 框出是否允许预亮小头像？ | **否**；空框 → 首句再出 | ✅ 产品钉死 |
| Q2 | 根因？ | **PrepareMaskAvatarOnFadeIn**（非 Portrait / 非 SetDefault Smile） | ✅ 侦探已拍板 |
| Q3 | 方案？ | **F1** 门口 Prefab 关预亮；F2 仅兜底 | ✅ 已施工 |
| Q4 | KenMuNiStart 预亮？ | **0902 曾保留** → **0919 作废**，开场也要空框 | ⛔ 见下节 0919 |
| Q5 | Setup 工具是否会回潮写预亮=true？ | Door Setup 钉 `PrepareMaskAvatarOnFadeIn=false`；Prelude options 默认仍 true | ✅ 已施工核对 |

---

## Village · 门口进村长家改黑屏切场 · 2026-09-02

详见：`Assets/Doc/执行文档/0902/Village_门口进村长家_改黑屏切场_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0902/Village_门口进村长家_改黑屏切场_施工说明.md`  
**侦探结论**：推翻 0831「进屋=Loading」。**H1+H2**：自动 `LoadSceneWithLoadingPanel` + `House_Chief.ShowLoadingUI=1`。施工 **F1+F2** → 日常 `LoadScene` 黑幕；**保留**续聊门闩；**H3** 黑幕淡出后才 `OnEnterScene` → 推荐同批 **F1′** `TryDeferBlackFadeForCover` 全黑 Trigger。Loading API 留给时间跳转，勿删。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 进屋主表现？ | **BlackPanel**（非蛋糕读条） | ✅ 产品钉死 |
| Q2 | Loading 留给谁？ | **仅时间跳转**；日常进门不算 | ✅ |
| Q3 | 自动进屋改法？ | **F1** `LoadScene(Chief_House)` | ✅ 已施工 |
| Q4 | 手动 House_Chief？ | **F2** ShowLoadingUI=false | ✅ 已施工 |
| Q5 | 续聊？ | **保留**门闩；遮罩 **F1′** defer | ✅ 已施工 |
| Q6 | 删 LoadSceneWithLoadingPanel？ | **否** | ✅ |
| Q7 | 其它场景已勾读条门？ | 本期不扫；另案 | ⏳ 可选 |

---

## Village_村长家继续对话 · 开场分层淡入对齐门口 · 2026-09-02

详见：`Assets/Doc/执行文档/0902/Village_村长家继续对话_开场分层淡入对齐门口_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0902/Village_村长家继续对话_开场分层淡入对齐门口_施工说明.md`  
**侦探结论**：续聊与门口 **前奏图头同构**（非缺节点）。硬切主因 **H1b/H4**：`TryDeferBlackFadeForCover` 在 `onStoryTriggered` 后 0.1s 揭换场黑幕，但 `StartDialogue` 仍 `Yield+Instantiate`，先露空房再齐活。另续聊 **PrepareMask=true** 须关。推荐 **T1′**（全黑内 alpha=0 备好再揭）+ **T3**；本期不做结束淡出；禁止整壳覆盖/Loading。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 缺前奏节点？ | **否**；同构 | ✅ |
| Q2 | 硬切主因？ | **揭黑早于树就绪**（H1b/H4） | ✅ |
| Q3 | 方案？ | **T1′ + T3** | ✅ 已施工 |
| Q4 | 续聊 PrepareMask？ | **false**（对齐门口空框） | ✅ 已施工 |
| Q5 | 本期结束淡出？ | **否**（门口图亦无；换古莎另案） | ✅ |
| Q6 | 整 Prefab 覆盖门口？ | **否**（保 Tips） | ✅ |
| Q7 | 二次对话专用黑幕（T1）？ | T1′ 优先；验收不够再上 | ⏳ 验收决定 |

---

## Village_Chief_House · 续聊战斗待机与室内主角显隐 · 2026-09-02

详见：`Assets/Doc/执行文档/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_施工说明.md`  
**侦探结论**：载体 **A** 合层预置「雅儿战斗待机」（勿 B 切真 Combat）。开场 **S1** 全黑内关玩家 `SpriteRenderer` + 亮待机；结束 **扩展**既有 `OnBlackFullyShownForGushaSwap` 同一次黑幕关战斗待机+古莎待机、恢复主角；**默认仍开**古莎动画合层。有 Combat Idle 帧、**无**现成待机 Prefab → Setup/美术补壳；场景拆包合层须双写。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 结束仍开古莎动画合层？ | **是**（默认） | ✅ 已施工 |
| Q2 | 待机帧选型？ | **铠甲基本无** 第 1 帧；可选跟存档头饰另案 | ⏳ 美术确认 |
| Q3 | 单帧 vs Animator？ | **单帧 SR 先** | ✅ 已施工 |
| Q4 | 门口初次同套？ | **否** | ✅ |
| Q5 | 玩家显隐方式？ | **SR.enabled**；禁 HideEntity/整根关 | ✅ 已施工 |
| Q6 | 合层写哪？ | 场景拆包 + Prefab 资产双写（Setup 菜单） | ⏳ 须 Unity 跑 Setup |

---

## Village_Chief_House · 自由移动光亮 DayLight · 2026-09-02

详见：`Assets/Doc/执行文档/0902/Village_Chief_House_自由移动光亮DayLight_架构溯源报告.md`  
施工说明：`Assets/Doc/施工说明/0902/Village_Chief_House_自由移动光亮DayLight_施工说明.md`  
**侦探结论**：**H1**——`VillageHomeDayLightAnimApplier` 白名单无 `Village_Chief_House`，进房 Home 暗版。产品钉死村长家算村民家光亮。施工 **F1** 加白名单；非 Combat；龙宫/村街勿进名单。与续聊战斗涂层解耦：自由移动还控后仍须 DayLight Home。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 村长家算不算 DayLight 白名单？ | **算** | ✅ 产品钉死 |
| Q2 | 方案？ | **F1** 加 `Village_Chief_House` | ✅ 已施工 |
| Q3 | 龙宫 / 村街？ | **不加** | ✅ |
| Q4 | 改状态名 / 方案 E？ | **否** | ✅ |

---

## HomeScene1 ↔ HomeScene2 · Stairs 换场相机闪烁 / 左右滑动 · 2026-09-12

详见：`Assets/Doc/执行文档/0912/HomeScene1_2_Stairs换场相机闪烁与滑动_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0912/HomeScene1_2_Stairs换场相机定格_施工说明.md`  
**侦探结论**：**同源**——进场 `SetFollow(forceSnap)` + `smoothTime≈0.3` 手推未收束，黑幕 hold **0.3s** 即 `CloseFormFade`。2→1 默认 VCam x≈-19.5 → 落点 x≈0.17（Δx≈20）→左→右滑；1→2 X 已贴齐、主差 Y≈3.2 →偏闪。剧情二次改相机已排除。推荐 **方案 A**：两侧龙宫 `CameraComponent.smoothTime=0`。  
**已施工（2026-09-12）**：方案 A — HS1/HS2 `smoothTime=0`（场景序列化）；未改换场代码 / Framing。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否采用方案 A（两侧 smoothTime=0）？ | **是** | ✅ 已施工（待 Play 验收） |
| Q2 | Forest→HS1、换装→HS2 是否允许同样瞬切？ | **允许** | ✅ 按默认（抽测回归） |

---

## Village_Shop 出店回村相机闪滑 · 2026-09-20

详见：`Assets/Doc/执行文档/0920/Village_Shop_出店回村相机闪滑_架构溯源报告.md`

**侦探结论**：**与 Stairs 同源**（`SetFollow` 手推 + hold 0.3 早揭幕）。KenMuNi1 磁盘 `smoothTime: 0.3`；默认 `VCam_Street`(32.56,0) → `EnterFrom_Shop`(-29.04,-6.5)，Δx≈−61.6。推荐 **方案 A**：仅 KenMuNi1 `smoothTime=0`。店场景 / 换场契约 / Part3 / EnterPos 不动。日常跟拍仍靠 CM Damping。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 是否采用方案 A（KenMuNi1 smoothTime=0）？ | **是** | 待施工 |
| Q2 | 其它入口进村（民居/东城郊）是否允许同样瞬切？ | **允许**（与 Stairs Q2 同口径） | ✅ 侦探默认 |
| Q3 | HS2 Framing DeadZoneH=1 是否本期一并改？ | **否**；A 后仍闪再开 | 待验收决定 |
| Q4 | 是否上方案 B（对齐再揭幕，全项目）？ | **本期否** | ✅ 本期不做 |

---

## HomeScene1 出门地图 ·「这么远!!!!」VerySurprised 对接 · 2026-09-12

详见：`Assets/Doc/执行文档/0912/HomeScene1出门地图对白_这么远表情丢失_架构溯源报告.md`  
**侦探结论**：用户已在 `GoOutStoryYaerPainting/Faces` 补 `Armor_NoHeadWear_VerySurprised` 且 **Image 已绑 `震惊.png`**；Resolve 键逐字符一致；小头像四套 atlas 已含 `VerySurprised`。**主路径对接闭合 → 无需改代码**（可选回写 0601）。旁路：`HomeScene1GetMap` 拆包 `YaerPainting` 仍为裸枚举 Faces + GoOut Resolve，磁盘默认未激活——仅当 Play 仍空脸再修该副本。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否还需改代码？ | **否** | ✅ 侦探裁定 |
| Q2 | GetMap 拆包内嵌是否本期对齐 Armor_ 键？ | **否**；Play 失败再开 | 待 Play |
| Q3 | VerySurprised 与 ZhenJing 同用震惊图是否换独立贴图？ | 可选后续 | 待产品 |
| Q4 | 是否回写 0601 对照表一行？ | **建议是**（仅文档） | 待确认 |

---

## ForestScene 演出结束 · 相机位置不对 + 屏闪 · 2026-09-12

详见：`Assets/Doc/执行文档/0912/ForestScene_演出结束相机位置与屏闪_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0912/ForestScene_演出结束相机定格去二次snap_施工说明.md`  
**侦探结论**：门口林恩链收束——`OnDialogueEnd`→`SetFollow(forceSnap)`+场景 **`smoothTime=0.3` 无黑幕手推**（位置错/闪主因）；NodeCanvas **id42 再调 `OnCameraMoveEnd`** → 二次 `forceSnap`（双闪/顿挫）。与龙宫 Stairs 机制同源，勿混场景施工。推荐 **B′（去/幂等二次 OnCameraMoveEnd）+ A′（收束瞬切）**。  
**已施工（2026-09-12）**：`ForestSceneLinEnStory` — A′ 临时 `smoothTime=0`；B′ `OnCameraMoveEnd` 本轮幂等；未改场景/Prefab 图/黑幕。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否采用 B′+A′？ | **是** | ✅ 已施工（待 Play 验收） |
| Q2 | A′ 仅 LinEn 临时 smoothTime=0，还是整 Forest 场景=0？ | **仅 LinEn** | ✅ 已按优先默认 |
| Q3 | 是否上黑幕方案 C？ | **本期否** | ✅ 本期不做 |
| Q4 | 现象是否确认门口林恩链？ | **默认是** | 待 Play |

---

## ForestScene 门口 · 保留相机移动 + 消闪 + 接话中断 · 2026-09-12

详见：`Assets/Doc/执行文档/0912/ForestScene_保留相机移动_消闪与接话中断_架构溯源报告.md`  
前置施工：`施工说明/0912/ForestScene_演出结束相机定格去二次snap_施工说明.md`（A′+B′，已被本条纠偏）  
施工：`Assets/Doc/施工说明/0912/ForestScene_保留相机移动_消闪并修复接话_施工说明.md`（M3）  
迭加：`Assets/Doc/施工说明/0912/ForestScene_M1短黑幕掩护手推消闪_施工说明.md`（M1，验收仍闪后）  
**侦探结论（产品纠偏）**：**必须保留平滑移动**；否决 A′ 瞬切作终态。接话断因 A′ `smoothTime=0` → `onComplete` **同步** `TryNotify` 早于图 id41 Register，事件丢弃；B′ 幂等又使后续 `OnCameraMoveEnd` SKIP 不再发事件 → 卡死等 `CameraMoveEnd`，`YaerAfterLinEn` 不 Trigger。推荐 **M3**：回滚 A′ +「已注册再 Notify」；B′ 改语义只挡二次 snap；闪重再 **M1 短黑幕**。禁止再套龙宫 Stairs 定格。  
**已施工（2026-09-12）**：M3 — 回滚 A′；`EnsureNotify` 等 Register；B′ 只挡二次 snap。  
**已迭加（2026-09-12）**：M1 — 手推前短 BlackPanel，到位揭幕再 Notify。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 产品是否钉死「保留移动」？ | **是** | ✅ 纠偏 |
| Q2 | 是否回滚 A′ 瞬切默认路径？ | **是** | ✅ 已施工 |
| Q3 | 首发 M3，闪重再 M1？ | **是** | ✅ M3 已施工；**M1 已迭加**（待 Play） |
| Q4 | B′ 是否保留？ | **保留但改语义**（不得挡唯一有效 Notify） | ✅ 已施工 |
| Q5 | 是否删 Prefab id42？ | 可选；非必须 | ✅ 本期不删 |

---

## 三种敌人掉落物消失 · Sprite GUID 断裂 · 2026-09-12

详见：`Assets/Doc/执行文档/0912/三种敌人掉落物消失_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0912/三种敌人掉落物恢复_施工说明.md`  
**侦探结论**：Slime / TenWan / 常驻 WoodWorm 掉落链仍通；「不见了」因商店提交 `c8d44392` 删除英文 Icon（`SlimeCore`/`TenWangFruit`/`InsectBeak`.png）换中文名新 GUID，**MainItemDatabase 已跟、三怪 Prefab `dropItem` 未跟** → Missing Sprite。推荐 **A 重绑**现网 `史莱姆核/藤蔓果/虫喙.png`；可选一并修 `WoodWorm_1.monsterLogic`。勿给巢生/卵/Boss 开掉落。  
**已施工（2026-09-12）**：方案 A — 四 Prefab `dropItem` Sprite 重绑中文 Icon；`WoodWorm_1.monsterLogic` 因根上无 `WoodWormLogic` 未绑（巢生不掉，另案）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 世界掉落是否继续共用 UI Item Icon？ | **是**（现网本就共用；重绑中文 Icon） | ✅ 已按此施工 |
| Q2 | 是否本批修 `WoodWorm_1.monsterLogic=null`？ | **本期否**（Prefab 缺 Logic 组件；巢生不掉） | ✅ 已决议（施工） |
| Q3 | Tips「获得道具」是否本期恢复？ | **否**（默认以进包为底线） | ✅ 本期不做 |
| Q4 | 是否改走运行时从 Database 赋 Sprite（方案 C）？ | **本期否**；先 A | ✅ 本期不做 |

---

## 史莱姆 Y 轴同轴对齐（掉树 + 移动）· 2026-09-12

详见：`Assets/Doc/执行文档/0912/史莱姆Y轴同轴对齐_掉树与移动_架构溯源报告.md`  
施工：`Assets/Doc/施工说明/0912/史莱姆Y轴同轴对齐_施工说明.md`  
**侦探结论**：掉树偏与移动偏同源。树上实例 Y≈7.41 vs 玩家轴 Y≈−6.61；Born 只靠短距 `IsGrounded`、**不 Snap**；Move 只改 X → 落偏则一直偏。推荐 **A：地面态 Snap 到玩家 y（或场景轴线）**；JumpAtk 升空前解冻 Y。勿套村庄纵深；勿恢复 0723 GroundCld 实心。  
**已施工（2026-09-12）**：方案 A — `SnapToCombatAxisY` 于 BornDown / JumpAtkDown / Idle / Move Enter；Idle 冻 Y；未改 BornFall。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 权威 Y = 实时玩家 `position.y` 还是场景常量（East≈−6.61）？ | **实时玩家 y**；常量 −6.61 作无玩家兜底 | ✅ 已施工 |
| Q2 | 已摆在 −6.61 的地面怪是否也进态 Snap？ | **是**（统一地面态） | ✅ 已施工 |
| Q3 | JumpAtk 空中是否允许不同轴、仅落地后对齐？ | **是** | ✅ 已施工 |
| Q4 | 是否本批重写 BornFall 去掉 MovePosition 硬降（方案 C）？ | **本期否**；先 A | ✅ 本期不做 |
---

## 史莱姆死亡掉出屏幕 · 尸体与掉落丢失 · 2026-09-13

详见：`Assets/Doc/执行文档/0913/史莱姆死亡掉出屏幕_尸体与掉落丢失_架构溯源报告.md`  
**侦探结论**：「无尸体/无掉落」实为 **Dead 未 FreezeAll + canGravity 仍开**，`MoveComponent` Gravity.y=-100 把整只怪（含 `dropItem` 子物体）拽出屏外；非 0912 Sprite 断链。WoodWorm 同样注释关重力却因 Dead 已冻而不掉。0912 Idle 冻 Y / JumpAtk 解冻 **放大概率对比**，非根因。推荐 **方案 A**：`SlimeDeadState` 对齐 TenWan/WoodWorm（FreezeAll + kinematic；建议清速度 + 可选 canGravity=false）。禁止 Dead 调 `SnapToCombatAxisY`；禁止恢复 0723 GroundCld 实心。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 空中击杀尸体停致死点还是落回地面？ | **停致死点**（冻当前 pose；Dead 禁止 Snap 玩家轴） | ✅ 已按默认施工 |
| Q2 | 是否同时恢复 OnDead `canGravity=false`？ | **建议是**（双保险） | ✅ 已按默认施工 |
| Q3 | 是否本期修 JumpAtk 死亡 Exit 子 SM 特判（现网误要求 isFallDownAtk）？ | **本期否**；先 A | ✅ 本期不做 |
| Q4 | kinematic 与尸体期 `isProtect=false` 可砍是否冲突？ | **默认不冲突**；抽测砍尸 | 待 Play |

施工说明：`Assets/Doc/施工说明/0913/史莱姆死亡定格留尸_施工说明.md`

---

## VerdantCorridor · 史莱姆吃羊无对白 · 2026-09-13

详见：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊无对白_架构溯源报告.md`  
**侦探结论**：对话文件**未丢**；`VerdantCorridorSlimeEatSheep.prefab` 在盘且可加载，但图内 **0× StatementNodeEx**（入库起即空）。触发链仍可镜头/黑幕/`Action2("start")` 刷怪。东城郊对照 6 句。推荐 **方案 A** 只补走廊 Prefab 台词并**保持 Action2**。禁止把 `StoryPrefabName` 改成东城郊名。Mgr 存档键串台另案。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 走廊是否照搬东城郊 6 句原文（含「龙城郊外」）？ | **是**（无新台本则照搬） | ✅ 已按默认施工 |
| Q2 | 台词插在镜头后、黑幕前？ | **是** | ✅ 已按默认施工 |
| Q3 | 是否本期补 InitSomeEventState→Mgr2？ | **本期否** | ✅ 本期不做 |
| Q4 | Mgr1/Mgr2 共用存档键是否拆开？ | **另案** | 待立项 |

施工说明：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊补对白_施工说明.md`

---

## VerdantCorridor · 史莱姆吃羊镜头对白时序错乱 · 2026-09-13

详见：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序错乱_架构溯源报告.md`  
**侦探结论**：台词已有；乱序因走廊 Prefab connections 为 `0→1→2→8…13→3…7`（先推镜再全说完再拉回）。金标准东城郊为「第一句→推镜→Wait→拉回 Follow→其余句→黑幕→start」。来自同日补对白施工（含「定格 Pos2 说话」）。推荐 **方案 A** 只改连线为 `0→8→1→2→3→4→9…13→5→6→7`，**保持 Action2**。禁止改东城郊图 / Action1 / CameraMove 源码。覆盖前案 Q2「镜头后说话」的施工默认。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 是否以东城郊序为唯一金标准（覆盖「定格 Pos2 说话」）？ | **是** | ✅ 已按默认施工 |
| Q2 | 是否只改 connections、不改 CameraPos2 摆位？ | **是** | ✅ 已按默认施工 |
| Q3 | 第一句「那是。。。。」是否改省略号？ | **本期否**（与东城郊 Prefab 实际同） | ✅ 本期不做 |

施工说明：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序对齐_施工说明.md`

---

## 史莱姆同轴 · 权威 Y 改场景常量 · 2026-09-13

详见：`Assets/Doc/执行文档/0913/史莱姆同轴_权威Y改场景常量_架构溯源报告.md`  
**侦探结论**：玩家跳史莱姆飞，因 0912 `TryResolveCombatAxisY` 优先实时玩家 y；Idle Snap 后冻 Y 会钉在空中。**推翻** 0912 OPEN Q1。权威改为场景常量 **−6.61f**（ForestEast / VerdantCorridor / WestRapp 地面轴同值）。推荐 **A**：只返回常量，去掉玩家/atkTarget y；建议 **A′** JumpAtk `endPos.y` 用常量、x 仍跟玩家。禁止跟跳、禁止关 JumpAtk、禁止村庄纵深。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 常量取值？ | **−6.61f**（三场共用） | ✅ 已按默认施工 |
| Q2 | 是否 Config 分场景轴高？ | **本期否** | ✅ 本期不做 |
| Q3 | JumpAtk endPos.y 是否本期改常量？ | **建议是（A′）** | ✅ 已做 A′ |
| Q4 | 0912 Q1 是否标推翻？ | **是** | ✅ 已改写 0912 施工说明 |

施工说明：`Assets/Doc/施工说明/0913/史莱姆同轴_权威Y改场景常量_施工说明.md`

---

## VerdantCorridor · 史莱姆吃羊运镜抖闪不丝滑 · 2026-09-13

详见：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_运镜抖闪不丝滑_架构溯源报告.md`  
**侦探结论**：时序已对齐；不丝滑因 Duration=1 横推 ≈20u（太急）+ 开推 `SetFollow(go)` 默认 forceSnap 与 DOMove 抢位（抖/闪）+ 无 InOut Ease。推荐 **B+A**：`CameraMoveTaskAction` InOutSine / 开推 forceSnap=false / Destroy 延帧；走廊 Prefab 推拉 **2.2s**；可选跳过重复 FollowPlayer。禁止 `smoothTime=0` 瞬切。黑幕等满再 Action2，非闪主因。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | Duration 2.2 还是 2.5？ | **2.2s** | ✅ 已按默认施工 |
| Q2 | 东城郊是否一并加长？ | **本期否** | ✅ 本期不做 |
| Q3 | 是否跳过 id4 FollowPlayer？ | **建议跳过** | ✅ 已跳过（3→9） |
| Q4 | 开推 false snap 若起点没贴上？ | **仍先 false** | 待 Play |

施工说明：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊_运镜丝滑_施工说明.md`

---

## 史莱姆尸体 · 死亡贴场景常量轴 · 2026-09-13

详见：`Assets/Doc/执行文档/0913/史莱姆尸体_死亡贴场景常量轴_架构溯源报告.md`  
**侦探结论**：尸体高低错落因定格留尸冻在致死瞬间 Y（击飞 bounce / JumpAtk 空中 / 掉树 Fall），活体已贴 `CombatAxisY=-6.61`。权威已是常量，**推翻**留尸案「Dead 禁止 Snap」（那是防跟玩家实时 y）。推荐 **方案 A**：OnDead 先 `StopKnockBack` + 退出子 SM（JumpAtk/Born 否则 Dead.Enter 可能不跑）→ Dead.Enter **先 Snap 再 FreezeAll**。禁止去掉 FreezeAll、禁止改回玩家 y、禁止改击飞数值。

覆盖：0913 留尸 OPEN Q1「停致死点」→ 改为贴常量轴。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 空中死贴常量轴还是停致死点？ | **贴常量轴**（推翻留尸 Q1） | ✅ 已按默认施工 |
| Q2 | 是否 OnDead 停击飞？ | **是** | ✅ 已施工 |
| Q3 | JumpAtk/Born 死是否退出子 SM？ | **是** | ✅ 已施工 |
| Q4 | 是否改 breakHight？ | **本期否** | ✅ 本期不做 |
| Q5 | OnDead 是否也 Snap？ | **本期否**（先保证 Enter） | ✅ 本期不做 |

施工说明：`Assets/Doc/施工说明/0913/史莱姆尸体_死亡贴场景常量轴_施工说明.md`

---

## VerdantCorridor · 木虫击飞踩上无法落地 · 2026-09-13

详见：`Assets/Doc/执行文档/0913/VerdantCorridor_木虫击飞踩上无法落地_架构溯源报告.md`  
**侦探结论**：0723 同族残留。`GroundCld` 设计给怪碰地图，Prefab 实心大盒托住 PlayerFoot，落地 Mask 不认 → JumpFall/DamageFlyFall 死等。碰撞原来就有，挡人是副作用。史莱姆/战斗藤蔓已 Trigger，木虫未跟。推荐 **方案 A**：`WoodWormLogic.OnInit` `groundCld.isTrigger=true`（覆盖 WoodWorm / _1 / 巢生）。禁止改矩阵、禁止 Mask 加 OnlyMapObj、禁止恢复挤出。Root 同族建议同期 Trigger。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | WoodWormRoot GroundCld 是否本期 Trigger？ | **建议是** | ✅ 已施工（用户要求虫巢一并改） |
| Q2 | 是否同步 Prefab m_IsTrigger=1？ | **建议是**（OnInit 仍权威） | ✅ 已双写三份 Prefab |
| Q3 | 是否抽到 BaseMonster？ | **本期否** | ✅ 本期不做 |
| Q4 | 矩阵不对称是否另案？ | **本期否** | ✅ 本期不做 |

施工说明：`Assets/Doc/施工说明/0913/VerdantCorridor_木虫GroundCld不挡落地_施工说明.md`

---

## VerdantCorridor · 底座史莱姆专用空气墙 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/VerdantCorridor_底座_史莱姆专用空气墙_架构溯源报告.md`  
**侦探结论**：活体史莱姆三个盒运行时全是 Trigger，物理墙挡不住；击退/跳攻走 `MovePosition` 脚本曲线，会穿薄墙。推荐 **方案 C**（Trigger 检测 + FixedUpdate 权威夹紧，身份 `ISlime`/`Slime`）。不改全局矩阵、不复用 `MapLimit`、不写死「底座」。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 死后尸体是否仍挡？ | **否**（`IsDead` 跳过夹紧） | ✅ 已按默认施工 |
| Q2 | 睡眠史莱姆是否挡？ | **是**（仍是 `ISlime`，避免击退穿过去） | ✅ 已按默认施工 |
| Q3 | 墙默认单向还是双向？ | **双向**（体积挤出）；走廊若只要「不能往底座内侧过」可改单向法线 | ✅ 已按默认施工 |
| Q4 | 撞墙是否 `StopKnockBackEffect`？ | **是**（否则击退曲线每帧抢 `MovePosition`，贴墙会抖） | ✅ 已按默认施工 |
| Q5 | 是否上方案 D（再给史莱姆加实心探测盒）？ | **本期否**；Play 仍穿再开 | ✅ 本期不做 |

施工说明：`Assets/Doc/施工说明/0914/VerdantCorridor_底座_史莱姆专用空气墙_施工说明.md`

---

## ForestEastScene · 倒树换新合层保留图层 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_倒树换新合层保留图层_架构溯源报告.md`  
**侦探结论**：不要整棵替换。场景 `倒树` 已在用 `SuburbEast/4.5/` **散图**；指定真源是 Prefab `倒树合层` 引用的 **`4.5/倒树合层/` 嵌套图**（另一套 GUID）。推荐 **A**：只换 `内/光/外/遮罩` 的 Sprite，SortingLayer/Order/Z/XY 全留场景值。缝 Prefab 没挂但磁盘有图。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 缝：留旧图 / Disable / 等补层？ | **留节点**；Sprite 换成 `倒树合层/缝.png`（与其它层同一套导出）。若和外层叠缝再 Disable | ✅ 已按默认施工 |
| Q2 | 遮罩现用 `树洞新遮罩.png`，是否改成合层 `遮罩只影响人物.png`？ | **是**（跟 Prefab 真源）；若人物裁切变差再改回 | ✅ 已按默认施工 |
| Q3 | 光变高（6.02→9.24）是否微调 LocalXY？ | **先 A 不挪**；Play 对不齐再 B 只动光 | ✅ 已按默认施工（未挪） |
| Q4 | `Village_OutSide` / `WestRappRoad` 的「倒树」是否同期换？ | **否**（无 `TreeBridgeLogic`，非东郊树桥） | ✅ 本期不做 |
| Q5 | 是否动旧资源 `ArtRes/Scene/倒树合层.prefab`？ | **否** | ✅ 已决议 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_倒树换新合层保留图层_施工说明.md`

---

## ForestEast · 倒树进洞相机贴 CameraTreeInArea 底边 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_倒树进洞相机贴边界底边_架构溯源报告.md`  
**侦探结论**：偏高因 `DeadZoneHeight=1` 不跟 Y + 进洞只换 Confiner/Size、不压 Y，合法带内留在偏上位置。**不改边界**。推荐 **A**：`ChangeCamera(true)` 在切盒+Size 后按 `bounds.min.y + orthoSize` Force 只改 Y。ForestEast **无 Part3**。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | Force 后是否还要每帧夹紧？ | **否**；DeadZoneHeight=1 + Confiner Damping=0，一次够 | ✅ 已按默认施工 |
| Q2 | 爬行 `CameraAction` 抖完是否再贴底？ | **建议是**（`StopCameraAction` 末若仍 `playerIsInTreeBridge` 再 Force 一次） | ✅ 已按默认施工 |
| Q3 | `StopCameraAction` 把 MainCamera 拽到 `(0,0)` 是否另修？ | **本期否**（Brain 下帧会盖回；另案） | ✅ 本期不做 |
| Q4 | 公式是否加 confiner padding？ | **否**；Damping=0、无额外 padding | ✅ 已按默认施工 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_倒树进洞相机贴边界底边_施工说明.md`

---

## SystemTipsPanel2 · Missing Script · 2026-09-14

详见：`Assets/Doc/执行文档/0914/SystemTipsPanel2_MissingScript_架构溯源报告.md`  
**侦探结论**：Missing GUID `8f4e2a1b…` 全库无 `.meta`，是无效占位孤儿 YAML。`imgTipsContent` 空且缺 `ImageContent`，与 Missing 无关但开面板会 NRE。推荐 **A**：删孤儿 + 按 Panel1 补绑 ImageContent。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 是否同期补 `ImageContent` 并绑 `imgTipsContent`？ | **是**（方案 A） | ✅ 已按默认施工 |
| Q2 | 是否把禁用全屏 `Image` 当文案槽？ | **否** | ✅ 已按默认施工 |
| Q3 | 是否新建假 GUID 脚本消黄？ | **否** | ✅ 本期不做 |
| Q4 | 是否用 Panel1 整份覆盖 Panel2？ | **否** | ✅ 本期不做 |

施工说明：`Assets/Doc/施工说明/0914/SystemTipsPanel2_MissingScript_施工说明.md`

---

## ForestEast · 洞口爬完对白不触发卡死 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_洞口爬完对白不触发卡死_架构溯源报告.md`  
**侦探结论**：截图是左口**外侧、未进黑幕**。缺的对白是进洞后的 `ForestEastSceneEnterTreeBridge`（盒在传送点 x≈264），不是 `PassTreeBridge`。卡死主层 **A**：`CanNotSomeActionArea`（SquatUp）盒宽约 80，洞口仍在区内则可能永不 `StopAutoCrawl`。0914 贴底在黑幕之后，**无关**。洞外地板是 `GroundLeft_1`，不是 `GroundCenter`。推荐 **方案 A**（AutoEnter 先停爬 + Sign 超时 + 修正 Out Right 双写）。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 卡死帧以谁为准：无 `StopAutoCrawl` / `hasFindPlayer` 死等 / `HasRunningStory` 壳卡？ | Play 打日志后以 **A** 为默认修；C 用 2s 超时兜住；H 无证据不改壳 | ✅ 已按默认施工 |
| Q2 | BeforeEnter / Enter 存档 SingleUse 是否已消耗？ | **不改存档逻辑**；无词但能走则不是本票主修 | ✅ 本期不做 |
| Q3 | 用户复现是左口还是右口？ | 两边都验；Right 双写 **一并改正** | ✅ 已改正 Right 双写 |
| Q4 | Sign 超时后强制进洞还是解锁让玩家重试？ | **强制走完进洞黑幕**（避免停在半锁）；仍死等再改为解锁 | ✅ 已按默认施工 |
| Q5 | 用户改的是 `GroundCenter` 还是 `GroundLeft_1`？ | 洞外只认 **GroundLeft_1**；未证明擦边则 **本期不改地板** | ✅ 本期不改地板 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_洞口爬完对白不触发卡死_施工说明.md`

---

## ForestEast · 死羊演出被改 · 吸羊动画没了 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_死羊演出被改_吸羊动画没了_架构溯源报告.md`  
**侦探结论**：`Part3/死羊` 是静图；循环在 `Objects` 两套 Prefab。v1.0 Doc 未改代码；用户反馈仍无循环 → **坐实共用存档键串台**，改走方案 **Key + Start 闸门**（施工说明 v1.1）。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 未播东郊吃羊走到 X≈175 能否看见循环？ | **是**（闸门 + 强制 ON） | 待用户验收 |
| Q2 | 已播东郊开战/立坟是否关动画？ | **是（设计）**；闸门后仍关 | ✅ |
| Q3 | 走廊 Mgr2 与东郊是否串档？ | **已拆键** `SlimeEatSheepStory2_*`；走廊剧情已播才迁旧键 | ✅ 已施工 |
| Q4 | 运镜手感是否要修？ | **另开票** | ✅ 本期不做 |
| Q5 | 是否改 `Part3/死羊` 静图？ | **否** | ✅ 未改 Part3 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_死羊演出_吸羊动画_施工说明.md`（v1.1）

---

## ForestEast · 树洞内主角再降 1 单位 Y · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_树洞内主角再降1单位Y_架构溯源报告.md`  
**侦探结论**：进洞传送只抄 X、Y 保留进洞前脚高；Player `gravityScale=0`，拖 `GroundCenter/Up/Down` 无效。推荐 **方案 A**：进 `oldY-1`、出 `oldY+1`。相机贴底本期不动。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 是否再拖红框三块地板？ | **否**（无重力吸附） | ✅ 未改地板 |
| Q2 | 是否抄 In/OutPos 完整 XY（方案 B）？ | **否**；OutPos.y=-6.3 ≠ 洞外 ≈-6.6 | ✅ 用 ±offset |
| Q3 | 人降 1 后头顶空/穿帮是否改贴底或 `CameraTreeInArea`？ | **否**；另开相机案 | ✅ 本期不改相机 |
| Q4 | 胶囊顶 `GroundUp` / 对白盒不相交？ | Play 抽测；挤出或碰不到再记，**勿先抬地板/Trigger** | 待用户验收 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_树洞内主角再降1单位Y_施工说明.md`

---

## ForestEast · 树洞第一虫卵打碎后卡住 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_树洞第一虫卵打碎后卡住_架构溯源报告.md`  
**侦探结论**：第一卵 = Type3/`spcWormEgg`≈276。E=`ViewBrokenEgg` Click，不点不应锁。`OnDead` 已关 `GroundCld`（须卡死帧复核）。**打碎后特有**：孵虫在卵左侧≈273，`PlayerBodyCollider` 对木虫仍 `StopMove`。可叠自动爬锁、人 Y−1 顶板夹。推荐卡死帧三分后最小修；禁止删 E / 无因果回滚贴底。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 卡死主层是 GroundCld / 孵虫 StopMove / 自动爬 / 对白？ | Play 打日志后定；默认优先查 **D 再 A 再 B**，本期 **三件组合** | ✅ 已按默认施工 |
| Q2 | 不按 E 是否同样卡？ | 期望是；若只有按 E 才卡再修对白解锁 | ✅ 未改成强播；不点 E 应能走 |
| Q3 | 碎后 `GroundCld.enabled` 是否仍为 false？ | 应为 **false**；死后及碎壳结束再断言 | ✅ 已断言 |
| Q4 | 人 Y−1 是否夹在 GroundUp？ | **已坐实风险**：Player↔GroundUp 碰撞；offset **从 1 改为 0.35**；仍夹再改 0 | ✅ 已减幅 |
| Q5 | 孵虫出生是否改到卵右侧 / 短时免 StopMove？ | **禁止 Away（会堵前进）**；改 **Behind（玩家同侧）** + 免刹车 4s | ✅ v1.1 已改 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_树洞第一虫卵打碎后卡住_施工说明.md`  
**验收**：用户仍报卡住 → **未过**；见复验票。

---

## ForestEast · 树洞第一卵打碎后仍卡住（复验）· 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_树洞第一卵打碎后仍卡住_复验溯源报告.md`  
**侦探结论**：**主因 R1** — 现网 `InTreePlayerYOffset=0.35` 时 Body 底仍切入 `GroundUp`（顶≈−6.35）约 **0.69**。v1.1（Behind/关盒/4s skip）源码已在，勿重复当新修。施工默认 offset→**0**；仍夹再只改洞内 `GroundUp` 净空。本机未 Play；须补 Console `[WormEggBreak]` + offset=0 对比（H）。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 主因是否 R1？ | 静态嵌深坐实；**offset→0 + GroundUp 净空** | ✅ 已按 R1 施工 |
| Q2 | offset=0 后≈0.34 嵌深是否仍卡？ | 同步下移 `GroundUp`（顶≈−6.85）消净空 | ✅ 已做步 2 |
| Q3 | GroundUp 净空目标 | 脚≈−6.6 时 Body 底≈−6.69；顶 ≤ −6.69−ε → 现顶 **−6.85** | ✅ |
| Q4 | 是否改判 R3（前方故事虫）？ | 仅本票验收仍卡 | 待命 |
| Q5 | 是否修爬区盒 R2？ | 本期默认否 | 降级 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_树洞第一卵打碎后仍卡住_复验施工说明.md`  
**本机未 Play；须用户本地越过第一碎壳才算过验收。**

---

## ForestEast · 爬出树洞无对白 + Fall 空引用 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_爬出树洞无对白_Fall空引用_架构溯源报告.md`  
**侦探结论**：主因 **AttachedGameObject[4]=None** → `Fall` L119 抛 Unassigned → Pass 对白链断。遮罩仍在（`遮罩只影响人物` / `1773642872123289953`），旧 `树洞新遮罩`/`399899081` 已删未重绑。推荐 **A 补绑 + B null 守卫**。与洞口停爬施工无关。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | `[4]` 补绑还是删空槽？ | **补绑** `1773642872123289953`（遮罩只影响人物） | ✅ 已补绑 |
| Q2 | 是否加 Fall/CheckFall null 守卫？ | **是**（A+B） | ✅ 已加 |
| Q3 | 合层是否弄丢 Attached？ | 旧 ID 删、新遮罩在、槽→0 | ✅ 已坐实 |
| Q4 | 读档 CheckFall 一并护？ | **是** | ✅ 已护 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_爬出树洞无对白_Fall空引用_施工说明.md`

---

## ForestEast · 树洞爬行嘎吱音效丢失 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_树洞爬行嘎吱音效丢失_架构溯源报告.md`  
**侦探结论**：主因 **A** — `PlayTreeBridgeMoveSfx` 资源名编码损坏，对不上磁盘 `木头嘎吱嘎吱声 .mp3`（含空格）。触发链与 `soundSfxCpn` 完好。推荐改回精确字面量；`AfterFallDown` 落水声同乱码，建议同票修。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 嘎吱名改回含空格真名？ | **是** | ✅ 已施工 |
| Q2 | AfterFallDown 落水声同票修？ | **是** | ✅ 已施工 |
| Q3 | 改名后仍无声再查音量/引用？ | 待命 | 待命 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_树洞爬行嘎吱音效补全_施工说明.md`

---

## ForestEast · 倒树遮罩只影响主角 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_倒树遮罩只影响主角_架构溯源报告.md`  
**侦探结论**：现网是 Effect 假遮罩（普通 SR），盖全场。语义 **S1**；方案 **A**（SpriteMask + 主角 **Visible Outside Mask**）；否决只改 Sorting 的 B。Attached 已绑新遮罩，勿丢。  
**施工**：场景加真 SpriteMask、关原 SR、Active 开；Player `Animation`/`ShadowAnimator` → Outside。  
**v1.1 改口**：同屏卵/虫也要被盖 → `WormEggType1/2/3`、`WoodWorm`/`WoodWorm_1` 全部 SR → Outside。  
**v1.2 再改口**：要的是 **EnvironmentShadow 压暗**（人仍可见），不是裁切不可见 → 撤回 SpriteMask/Outside；遮罩改 Layer12；卵补环境阴影材质+组件。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | S1 裁切还是环境阴影压暗？ | **压暗（EnvironmentShadow）** | ✅ v1.2 |
| Q2 | Inside / Outside？ | **作废**（不再用 SpriteMask） | ⛔ |
| Q3 | Prefab 全局 vs 进洞运行时？ | 环境阴影材质/组件 | ✅ |
| Q4 | 遮罩 Active？ | 未倒下开；Layer12 | ✅ |
| Q5 | 必须 S2 Stencil？ | 不需要（现网有 EnvShadow） | 降级 |
| Q6 | 卵/虫是否受阴影影响？ | **是**（压暗，非裁切） | ✅ v1.2 |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_倒树遮罩只影响主角_施工说明.md`（v1.2）

---

## ForestEast · 倒树外壳透明改切镜头后 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/ForestEast_倒树外壳透明改切镜头后_架构溯源报告.md`  
**侦探结论**：现网 Interactive 靠近即 `OuterSpriteFade`；切镜是 `ChangeCamera`。推荐 **方案 A**：去 Interactive 订阅；`ChangeCamera` 末尾按进/出调 Fade(0)/(1)；读档同源。否决双淡 C。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | A 还是 A+B（揭幕后淡）？ | **A** | ✅ 已施工 |
| Q2 | 旁路进洞？ | 现网无 | 降级 |
| Q3 | 「外」Active=0？ | 待 Play 核 | 待核 |
| Q4 | Fall 后外壳？ | 树销毁，无外壳 | ✅ |

施工说明：`Assets/Doc/施工说明/0914/ForestEast_倒树外壳透明改切镜头后_施工说明.md`

---

## WestRappRoad · 宝箱不能互动 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/WestRappRoad_宝箱不能互动_架构溯源报告.md`  
**侦探结论**：现象类型 **1**（无键提示）。**主因 F**：Box 在 `Map/Design/Near`，不在 `Objects`（objRoot）→ 永不 OnInit、不进可互列表。**次因 B**：Y≈−3.44 vs 玩家≈−6.61，Body 不相交。推荐挂回 Objects + 降 Y；勿改 Interactive 全局。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 现象是否类型 1？ | 静态判 1；Play 见 OnInit 坐实 | 待 Play |
| Q2 | 只挪父级不降 Y？ | **否**；F+B 都做 | ✅ 已施工 |
| Q3 | 目标 Y？ | **−6.61**（对齐 LeftBorn） | ✅ 已施工 |
| Q4 | 已开档？ | 勿清旗强开 | 文档 |

施工说明：`Assets/Doc/施工说明/0914/WestRappRoad_宝箱不能互动_施工说明.md`

---

## 章末 · ImageHomeToJingLingVillage 未点亮 · 2026-09-14

详见：`Assets/Doc/执行文档/0914/章末_ImageHomeToJingLingVillage未点亮_架构溯源报告.md`  
**侦探结论**：主因 **A** — 0721 章末只 `UnlockPlace`、故意不做 `UnlockRoad`；`ShowUnlockRoad` 全关后再按存档开 → 无 `HomeToJingLingVillage` 则路线灰。推荐 **R3**（章末补 UnlockRoad + 保留出门 GetMap）。**正式撤销** 0721「本期不做 UnlockRoad」。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 撤销 0721 不做 UnlockRoad？ | **是** | ✅ 已决议 |
| Q2 | R1 / R2 / R3？ | **R3**（章末 UnlockRoad + 保留 GetMap） | ✅ 已施工 |
| Q3 | PlayerMapData 空 Serialize？ | 另票；本期章末 R1 兜底 | 待开票 |
| Q4 | 是否亮 ImageAllRoad？ | **否** | 降级 |

施工说明：`Assets/Doc/施工说明/0914/章末_ImageHomeToJingLingVillage未点亮_施工说明.md`

---

## Village · 对话框出现时不要预亮小头像 · 2026-09-19

详见：`Assets/Doc/执行文档/0919/Village_村庄对话框出现时闪默认小头像_架构溯源报告.md`

**产品改口（作废 0902 Q4 / 开场技术说明里的「框淡入预亮 Mask」）：** 村庄对话（含 `Village_KenMuNiStart`）框刚出现时不要小头像；小头像跟第一句话一起出。不是只修门口。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 开场还要不要「框+头像同拍」？ | **不要** | ✅ 用户已改口 |
| Q2 | 是改全部村庄 Prefab 还是共用代码？ | **4 张 true 改 false + 淡入/对话开始先藏 Mask**。其余村庄图不用改字段 | ✅ 侦探已定，待施工 |
| Q3 | 森林 / 新游戏是否跟着删预亮能力？ | **不删字段**。现网没有村庄以外的图勾着 true。`NewGameStory` 保持 false，不要改那张图 | ✅ |

---

## Village_HomeScene23 大立绘 · Npc1 挂错龙宫对话 · 2026-09-20

详见：`Assets/Doc/执行文档/0920/Village_HomeScene23_对话不要大立绘_架构溯源报告.md`

椅子三张任务对话的大立绘已有施工默认（只改那三张 Prefab）。下面这条**不是**立绘票，不要拿去改 `HomeScene1Npc1` 的立绘。

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 本场 `Npc1` 仍播龙宫 `HomeScene1Npc1`（「被父亲训斥了」）。0601 民居台本 Prefab 磁盘上不存在。要不要另做民居 NPC1 台词并改场景上的 `StoryPrefabName`？ | **本票不改。** 立绘施工不要动这张龙宫 Prefab | 待产品 |

---

## Village · 村庄遮罩层盖顶 · 2026-09-20

详见：`Assets/Doc/执行文档/0920/Village_村庄遮罩层盖顶盖住玩家_架构溯源报告.md`  
**侦探结论**：民居 1/2/3/4 ↔ HomeScene1/2/45/23；肯姆尼 1/2/3 三段都在 KenMuNi1。遮罩作合层**兄弟**实例，关 `背景`，其余抬 **Effect / Order≥10** 盖住 Player。禁止嵌合层源、禁止 SpriteMask、禁止夜景。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 村长家？ | **本期跳过**（仅 `村长家.psd`，无 Prefab） | ⏳ 待美术出 Prefab |
| Q2 | 中间层（护头等）是否保留？ | **默认全留**（只关背景）；Play 过亮再关 | ✅ **已按此施工**（场景已挂实例） |
| Q3 | 夜景？ | **不做** | ✅ |

---

## Village_Chief_House · 大树进屋后向后走不转身 · 2026-09-20

详见：`Assets/Doc/执行文档/0920/Village_Chief_House_大树进屋后向后走不转身_架构溯源报告.md`  
**侦探结论**：偶发不转身 = Home Idle→Walk 当帧丢 A/D 订阅（与 Combat 进跑同构）；大树落点左侧更易立刻按 A。最小改：`HomeWalkState.Enter` 补横向 Move；禁止 W/S 翻面、禁止拆护栏。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否改 EnterFrom_Tree2f 朝向？ | **否**（治标；大门同构仍在） | ✅ 已按报告不改 |
| Q2 | W/S 是否翻面？ | **否** | ✅ |
| Q3 | 是否拆转身护栏？ | **否** | ✅ |

---

## Village_HomeScene2 · 右走自动回村 · 2026-09-20

详见：`Assets/Doc/执行文档/0920/Village_HomeScene2_右走自动回村_架构溯源报告.md`
**侦探结论**：右门关着且盒在世界 X≈27，室内走不到。挪到右墙（本地 X≈-31.07）、打开走进触发、NextScene=`Village_KenMuNi1`、EnterPosKey 留空。左门保持关。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否新 EnterPosKey？ | **否**（空键已对上 `ExitFrom_HomeScene2`） | ✅ 已按此施工 |
| Q2 | 左门是否也走进触发？ | **否** | ✅ |

---

## ForestEast ↔ VerdantCorridor · 换场相机闪滑 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/ForestEast_VerdantCorridor_换场相机闪滑_架构溯源报告.md`  
**侦探结论**：与 Stairs / 出店同源——`SetFollow(forceSnap)` + 目标场景 `smoothTime=0.3` + 黑幕 hold≈0.3s 早揭幕。首次进走廊剧情无二次改相机。推荐方案 A：必改 `VerdantCorridor.smoothTime=0`；反向 Δx≈129，建议同改 `ForestEastScene`（旧「Forest 禁止 smoothTime=0」针对林恩可见运镜，不套死本案黑幕定格）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否同票把 `ForestEastScene.smoothTime` 也改为 0？ | **建议是**（盖走廊→东郊大滑） | ✅ **已同票改为 0** |
| Q2 | 是否拉长黑幕 / 改公共换场契约？ | **否**（对齐 Stairs/出店方案 A） | ✅ 侦探已定 |
| Q3 | 首次进走廊剧情是否要保留可见推镜？ | **否**（现网 Prefab 无相机节点） | ✅ |

---

## 换场 / 桥图 / BOSS / 读档 · 相机闪滑全量盘点 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/换场与读档_相机闪滑_全量盘点_架构溯源报告.md`  
**侦探结论**：BOSS 图进出与读档（仍 0.3 场景）= Stairs 同源；树桥 = **另缝**（ChangeCamera + 即揭幕，东郊已 0 仍可能闪）。P0：`WestRappRoad→0` + 读档可达场景批改/可选 A2；P1：树桥策略 T；**禁止**改 ForestScene。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 读档是否上公共 A2（`archiveStart` 强制当帧对齐）？ | **建议 P0/P1 补强**；先字段改西境+村常用图 | ✅ **已上 A2**（仅 archiveStart） |
| Q2 | Village_Home* / Chief / OutSide / night 是否同票批改 smoothTime=0？ | **建议 P1 同册** | ✅ **已批改**（含 Shop） |
| Q3 | 树桥是否同改 `StopCameraAction` 拽 MainCamera→0？ | **建议评估同改** | ✅ **已去掉拽 (0,0)** + Align |
| Q4 | ForestScene.smoothTime？ | **禁止改** | ✅ 未改（仍 0.3） |

---

## Village_KenMuNi1 · 精灵池路灯对齐池中遮挡 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/Village_KenMuNi1_精灵池路灯对齐池中遮挡_架构溯源报告.md`  
**侦探结论**：场景已有 `肯姆尼2合层/精灵池路灯`（仅 SR，钉 SceneObject）；无 DepthSort。方案 A：场景实例挂 `VillageSceneObjectDepthSort`，字段整表抄 `精灵池中`（6/0）。禁止挂 `路灯蘑菇`；不做 `精灵池上`；不改 Prefab/C#。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 精灵池上本期？ | **否** | ✅ 未做 |
| Q2 | 写回合层 Prefab？ | **否** | ✅ 只改场景 |
| Q3 | 第一版就加灯脚锚点？ | **否**（先自 Transform） | ✅ 已按此施工 |
| Q4 | 路灯蘑菇同挂？ | **否** | ✅ 未挂 |

---

## Village · 移动操作只认按键设置（键族单通道）· 2026-09-22

详见：`Assets/Doc/执行文档/0922/Village_移动操作只认按键设置_架构溯源报告.md`  
**侦探结论**：村探索双通道（Axis + WASD∪箭头硬编码）未对接设置键族；Chief 与村外同 `Village2_5D`。推荐方案 A 公共键族推导；否决室内补 WASD、否决 UI 前后槽。探索禁蹲已挡 Squat 入队。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 村内 Squat=S 与 S=下冲突？ | **保持探索禁蹲**（现网已有） | ✅ 未改禁蹲 |
| Q2 | 是否加设置向前/向后？ | **否**（键族推导） | ✅ |
| Q3 | Custom 键（非 A/D、非箭头）纵深？ | **降级纵深 0** | ✅ 已按此施工 |

---

## Village_KenMuNi1 · 树根外对齐路灯遮挡 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/Village_KenMuNi1_树根外对齐路灯遮挡_架构溯源报告.md`  
**侦探结论**：`肯姆尼3合层/树根外` 仅 SR（SceneObject/30），无 DepthSort。方案 A：场景实例挂 DepthSort，字段整表抄路灯（9/3）。不做树根内；不改 Prefab/C#。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 树根内本期？ | **否** | ✅ 未做 |
| Q2 | 9/3 与 3 合层静图穿帮？ | **先抄路灯，验收再调 Order** | ✅ 已挂 9/3 |
| Q3 | 写回合层 Prefab？ | **否** | ✅ 只改场景 |
| Q4 | 第一版灯脚/根脚锚点？ | **否**（先自 Transform） | ✅ |

---

## Village · 纵深移动发抖与室内拖慢 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/Village_纵深移动发抖与室内拖慢_架构溯源报告.md`  
**侦探结论**：抖与拖慢两刀。抖=纵深步 WalkArea/障碍硬写掐插值（0919 WriteRoot 纯左右仍成立）；速=Chief `≈4.2` 相对村街 `≈11.2` 是 0901 设计。推荐方案 A 收口写位；否决先动 CM / 室内改回 11.2 治抖；0922 键族非主因。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 产品是否推翻 0901，室内要接近村街 11.2？ | **否**（保持 walk≈4.2 对齐其它 Home）；偏慢按手感收口 | ✅ 0922 速票：室内纵深瞬时满速 + scale=1 |
| Q2 | Play 后相对 4.2 是否仍异常更慢（发闷/贴障）？ | 有则开方案 B；无则只做方案 A | ✅ 已做纵深手感收口；贴障仍待 Play |
| Q3 | 是否允许先动 CM XDamping？ | **否**（最后手段） | ✅ 未动 |
| Q4 | 是否回退 0922 键族？ | **否** | ✅ |

---

## SewingKit · 非消耗唯一一件 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/SewingKit_非消耗唯一一件_架构溯源报告.md`  
**侦探结论**：Database 已是 TaskItem；角标「3」= `AddMainItem` 无唯一门控叠数 + UI 一律画 num。推荐 **B+C**（唯一白名单钳 1 + 修旧档）+ JSON 同步 0；**D** 隐藏唯一件角标推荐同票。空桶/满桶排除；否决全局 MaxStack=1。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 唯一白名单是否含剑/地图/项链/夏尔牵挂？ | **是**（同类一件家当）；含 SewingKit | ✅ 已按建议施工 |
| Q2 | 方案 D：隐藏「1」还是仅保证不出现 &gt;1？ | **隐藏唯一件角标**（对拍剑） | ✅ 已施工 |
| Q3 | 二次 GetItem 时 Tips 是否仍弹？ | **默认可仍弹**；包内保持 1 | ✅ |
| Q4 | 全局 MaxStack=1？ | **否** | ✅ |

---

## 游戏左下角版本号 1.0.5 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/游戏左下角版本号1.0.5_架构溯源报告.md`  
**侦探结论**：方案 A — 新 `VersionPanel` @ System，文案=`Application.version`，`bundleVersion`→`1.0.5`；须抗 `CloseAllUIForm`。否决只挂主菜单。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 文案是否带 `v` 前缀？ | **否**（纯 `1.0.5`） | ✅ 侦探已定 |
| Q2 | CloseAll 后 Ensure 还是 filter？ | **均可**；施工选一，勿叠多份 | ⏳ 待施工 |
| Q3 | 战斗左下与血条重叠？ | 极小字贴角 + 关 Raycast | ✅ 侦探已定 |

---

## Village_HomeScene1 · 墙上画彩蛋 20% · 2026-09-22

详见：`Assets/Doc/执行文档/0922/Village_HomeScene1_墙上画彩蛋20_架构溯源报告.md`  
**侦探结论**：场景内 `Map/Design/村民家1合层/画` 下彩蛋/正常双 Active=1 会叠图。推荐方案 A 挂互斥随机组件，OnEnable 每次进场景重掷 20%/80%；不写存档；勿改画框/旧 ArtRes Prefab。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 每次进场景重掷 vs 存档只掷一次？ | **每次重掷**；不写 Archive | ✅ 已施工 |
| Q2 | 中途读档？ | 场景重载则再掷 | ✅ |
| Q3 | 改 ArtRes 合层 Prefab？ | **否**（场景已展开；旧 Prefab 无彩蛋对） | ✅ |

---

## ForestEast · 树洞行走镜头上漂 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/ForestEast_树洞行走镜头上漂_架构溯源报告.md`  
**侦探结论**：0914 贴底仍在；上漂主因是 `CameraAction` DOMove **Camera 根 rig**（含 Confiner），0922 去掉 Stop→(0,0) 后 Y 残留，Snap 只 Force VCam 拉不回父节点。  
**产品口径（2026-09-23）**：**要原版抖动，不要上漂**。Framing 方案几乎看不见 → 已回退。现网 = **原版 DOMove 根抖动** + Stop 仅 `ResetCameraRigLocalY`（方案 A）。禁止恢复全轴 (0,0)、禁止挪盒子。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 纯走路不上爬也会上漂？ | **推演否** | ✅ |
| Q2 | Stop 是否允许只复位 rig Y？ | **是**（勿全轴 0,0） | ✅ 已施工 |
| Q3 | 是否回退 0922 Align / 去掉 (0,0)？ | **否**（闪滑会回潮） | ✅ |
| Q4 | 挪 CameraTreeInArea？ | **否** | ✅ |
| Q5 | 晃动实现？ | **原版 DOMove 根**（产品要抖）+ Stop 清 Y；Framing 方案已回退 | ✅ 2026-09-23 改回 |

---

## 序章郊区链 · 城堡到东郊换场屏闪 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/序章郊区链_城堡到东郊_换场屏闪_架构溯源报告.md`  
**侦探结论**：段① HS1→Forest = Stairs 同源（Forest 仍 0.3）；**禁止**整景改 0。段② Forest→East 磁盘已 0，残差查 Loading + FirstEnter AutoMove/Alpha。推荐 P0=进场 instantSnap（不改 Forest 字段）；P1=FirstEnter/Alpha。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | Forest 整景 smoothTime→0？ | **否** | ✅ |
| Q2 | 东郊再改一遍 smoothTime？ | **否**（已 0） | ✅ |
| Q3 | P0 用 A2 式进门 instant（LastScene 条件）？ | **是** | ✅ 已施工 |
| Q4 | 截图半透明是否另票 Alpha？ | Play 分裁后定 | ⏳ 待 Play |

---

## 背包点地图后系统 UI 不显示 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/背包点地图后系统UI不显示_架构溯源报告.md`  
**侦探结论**：系统 UI=MenuPanel。ESC 门闩=`isOpenMenu\|\|cantOpenMenu`。附图 35% 优先证伪 Loading+CantResponse（C）；纯关图查 flag 未清（A）与 ItemMap 二次点击只关菜单（B）。Resume 不对称为次因（D）。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 用户路径是纯关图还是点了关卡/Home？ | Play 看有无 Loading / ESC 日志 | ⏳ 待 Play |
| Q2 | P0 是否先做 A+B（清 flag + ItemMap）？ | **是**（+P1 Resume）；**2026-09-23 复验失败** | ✅ 已施工 → 回修 |
| Q3 | 店内 ESC 语义？ | **保持离店** | ✅ |
| Q4 | 章末地图链？ | **不动** | ✅ |
| Q5 | 附图 35% Loading 残留（方案 C）？ | 仅当关图后仍卡进度再票 | ⏳ 待 Play |
| Q6 | 复验失败根因？ | **OnClose 未退订 CloseFormOnEsc** → 池化同步 Open 后同 ESC 立刻关菜单 | ✅ 2026-09-23 已修 |
| Q7 | 再 ESC 只剩金币、按钮全无？ | **Center CanvasGroup alpha=0 残留**（ItemMap 先关 Menu 错过 OnPanelClosed） | ✅ 2026-09-23 已修 |

---

## 进对话禁止玩家移动 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/进对话禁止玩家移动_架构溯源报告.md`  
**侦探结论**：门口对白会走 `StoryTriggeredHandle`，但 Town 只认 `AllowControl`；村意图裸 `Input.GetKey` 绕过 `cantMove`；`StopMove` 不清 `depthVelocity`。推荐 **B+A+C**（Town 门闩 + 进故事 Halt + 意图认 cantMove）。否决只改门口 Trigger / 减速。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否只修门口 Trigger？ | **否**（公共路径） | ✅ |
| Q2 | 黑幕阶段是否也要停？ | **默认可仅 onStoryTriggered**；产品要更早另补 | ⏳ 待产品 |
| Q3 | 键族是否回退？ | **否**；只加 cantMove | ✅ |
| Q4 | 方案 E 摩擦停？ | **否** | ✅ |
| Q5 | P0 做 B+A+C？ | **是** | ✅ 已施工 |

---

## 纵深区主角影子消失 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/纵深区主角影子消失_架构溯源报告.md`  
**侦探结论**：脚底 Blob，非 EnvironmentShadow。主因：纵深 `vy` 易触发 `OnUnIsGround` 关影子；且深度排序只写身体、影子 Order 钉 -1 易被地片盖。推荐 **A+B**。截图古莎像须分表。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 截图是雅尔还是古莎贴纸？ | Play Hierarchy 钉死 | ⏳ 待 Play |
| Q2 | P0 是否 A+B 同做？ | **是** | ✅ 已施工 |
| Q3 | 改 EnvironmentShadow？ | **否** | ✅ |
| Q4 | 影子 Order 用 body−2？ | **是**（对拍怪物） | ✅ 已施工 |

---

## 门口剧情后隐藏村长并可进屋 · 2026-09-22

详见：`Assets/Doc/执行文档/0922/门口剧情后隐藏村长并可进屋_架构溯源报告.md`  
**侦探结论**：从未藏村长；`OnStoryFinished` 只自动进屋。须关 `Npc_Chief`+合层`村长`（结束+进村）。保留自动进屋（A+B）。`House_Chief` 未锁；`TriggerWhenMoveIn` 默认 0 可能造成「走进进不去」。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 是否关掉自动进屋？ | **否**（保留 B） | ✅ |
| Q2 | 藏谁？ | **Npc_Chief + 合层村长** | ✅ 已施工 |
| Q3 | House_Chief 是否改 TriggerWhenMoveIn=1？ | Play 证实走进无反应再做 F | ⏳ 待 Play |
| Q4 | 只关侧面古莎充数？ | **否** | ✅ |
| Q5 | P0 做 A+B？ | **是** | ✅ 已施工 |
| Q6 | 藏人时机穿帮？ | **黑幕全黑后再藏人/露门**（stayAction） | ✅ 2026-09-23 |
| Q7 | 出屋回村仍见消失？ | **TryDefer 开头（亮屏前）再 Apply；Find 绑本场景** | ✅ 2026-09-23 |
