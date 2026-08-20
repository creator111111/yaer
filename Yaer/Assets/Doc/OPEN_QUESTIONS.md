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
| Q3 | `UnlockRoad` = 地图路线贴图点亮（≠ 关卡能否点） | **本期不做**；只 `UnlockPlace` 关卡点 | ✅ 已决议 |
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
| Q2 | 旧 `actorPortrait` / 图集路径是否保持完全关闭？ | **是**；维持 `useMaskAvatar=1` | ✅ 已决议·保持 |
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
**已决议并施工（2026-08-07）**：方案 **D**——Prefab 串行 Wait0.5→YaerPainting Fade0.5→UIAlpha Delay0.5+Dur0.5(+PrepareMask)；`NewGameSceneManager` 成对：漫画全黑 → System `BlackPanel` RawShow → 关漫画 → Gate Reset → Trigger → Prepare 白名单 → HideFade 拍1 → Signal。已去前奏 BlackMask/YaerShow；`Start.anim` 去 alpha 曲线；默认态 Write Defaults=OFF；Prepare 关故事 Animator（淡入后由 CanvasGroupAlpha 落到 YaerShow 末帧）。

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 是否丢掉前奏 BlackMask / YaerShow？ | **主路径丢掉**；Fade 后落到 YaerShow 末帧供 KingMove | ✅ 已施工 |
| Q2 | NewGame 是否必须补 Gate 旁路才有「只见 BG」？ | **必须**（方案 D）；拍1 用 System BlackPanel | ✅ 已施工 |
| Q3 | 0806「已施工」与现网并行是否回退/误标？ | 半施工误标 + B 回退后，本期 D 真正成对落地 | ✅ 已结案 |
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

| ID | 问题 | 施工默认建议 | 状态 |
|----|------|--------------|------|
| Q1 | 动画播完是否自动进下一句？ | **否**；等玩家点继续（字幕仍显示） | ✅ 已按此 |
| Q2 | 帧率 / 循环 / 播完？ | **8～12fps、不循环、播完隐藏 Anim_*** | ✅ 已按此（10fps） |
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
| Q5 | 出售是否同 PR？ | **否**：出售 Tab 点决定仅 Log「出售结算未接入」 | 待确认 |
| Q6 | 假购买「成功购买生命球」文案 | 改为「购买成功，扣除金币 {total}」 | 待确认 |

---

## 村民家室内 DayLight 动画 · 2026-08-18

详见：`Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md`  
**产品已决议（2026-08-18）**：Q1 龙宫不开；Q2 House4 + 磁盘 HomeScene3 算村民家要开；Q3 屋里眨眼仍用现网 `Bink`。侦探不再征求这三项。  
**推荐施工**：方案 B（进屋运行时只换 Idle/Walk 片子，状态名不动）；方案 E 已否决。

| ID | 问题 | 决议 / 施工默认 | 状态 |
|----|------|-----------------|------|
| Q1 | 龙宫 `HomeScene1/2` 是否开 DayLight？ | **否**；共用 Idle/Walk Clip 也不许改 | ✅ 已决议 |
| Q2 | `Village_House4`、磁盘 `Village_HomeScene3` 是否算村民家？ | **算，要开**；进不去也要把场景名写入白名单 | ✅ 已决议 |
| Q3 | 屋里眨眼是否做 `Bink_DayLight`？ | **否**；继续现网 `Bink` | ✅ 已决议 |
| T1 | `Village_HomeScene3` 无 `SceneName` 常量，白名单怎么写？ | **先加** `SceneName.Village_HomeScene3`，白名单用场景文件名（勿用该屋当前错误的 `nowSceneName=HomeScene1`） | ✅ 已施工（2026-08-18 方案 B） |
| T2 | House4 `.unity` 缺失时名单写谁？ | **`Village_House4`**（与门 Next、已有常量一致）；补场景另案 | ✅ 已施工（白名单已写入） |
| T3 | 方案 B 的运行时 Clone Override 若换装/裙子验收失败？ | **改走方案 C**（复制 Dress+三套白天控制器）；仍禁止 E | 待验收确认 |

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
