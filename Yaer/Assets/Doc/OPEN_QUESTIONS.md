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
