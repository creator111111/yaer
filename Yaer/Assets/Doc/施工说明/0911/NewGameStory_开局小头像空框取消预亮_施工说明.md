# NewGameStory — 开局小头像空框取消预亮 — 施工说明

**文档版本**：v1.0（2026-09-11）  
**文档性质**：【施工员】按侦探报告方案 **F1** 最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0911/NewGameStory_开局小头像表情闪错_架构溯源报告.md`  
**根因**：H1 — UIAlpha `PrepareMaskAvatarOnFadeIn=true` 预亮 Yaer/`Laugh(6)`，首句才是 `Unhappy(1)` → 闪错脸  
**产品**：框淡入时小头像不显示；第一句再一次到位（对齐 0902 门口空框，**不对齐** KenMuNi 同拍）

---

## 沟通摘要

### ① 结论一句话

**仅关掉 `NewGameStory` 框 FadeIn 的小头像预亮；大立绘串行与进村同拍均未动。**

### ② 原因（通俗）

修大立绘时照抄了进村「框和头像一起淡出」的预亮开关，预亮脸还写了 Laugh。  
序章首句实际是 Unhappy，所以会先闪笑脸再切不高兴。  
序章产品要空框，跟村长家门口一样——关掉预亮即可。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 完整新游戏 → 对话框刚淡入（字未出/刚出） | 左侧 Mask **无**小头像 |
| 2 | 第一句正文出现 | **直接** Unhappy（或台本该脸），**无** Laugh→Unhappy |
| 3 | 场景大立绘 `YaerPainting` | 仍正常淡入（0911-A 未回退） |
| 4 | 进村 `Village_KenMuNiStart` | 框+头像同拍无回归 |
| 5 | 若关预亮后仍闪 Smile | 转验收查 H2；勿先动村线 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **F1** | `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab` | 首 ActionList 第 3 拍 UIAlpha：`PrepareMaskAvatarOnFadeIn` **true→false** |

### 未改（禁止项）

| 项 | 说明 |
|----|------|
| `MaskAvatarRole` / `MaskAvatarFace` | 可保留（不再生效）；未改成 Unhappy 冒充结案（禁 F2） |
| 首句 `FaceType=1`（Unhappy） | 未改 |
| Wait / CanvasGroupAlpha 串行 | 大立绘节奏保留 |
| `Village_KenMuNiStart` | PrepareMask 仍为同拍 true |
| Presenter / PNG / useMaskAvatar | 未动 |

---

## 与 0911 大立绘施工的关系

此前 `NewGameStory_主角大立绘恢复_施工说明.md` 写过 UIAlpha「PrepareMask=true 对齐 KenMuNi」。  
**本案解绑**：序章小头像产品是 **空框**，进村才是同拍；大立绘 Wait→Fade→UIAlpha **节奏保留**，仅关掉预亮字段。

---

## 时序（修后）

```
Wait → YaerPainting Fade → UIAlpha FadeIn
                              ├─ ClearSubtitleTexts（空字）
                              ├─ PrepareMask=false → 不调 Presenter.Apply
                              └─ 框渐出，Mask 空
首句 Statement → OnGetNewStatement(Yaer, Unhappy) → Apply 一次到位
```

---

## 剩余风险

| 风险 | 说明 |
|------|------|
| CSV 重导 | 可能冲回 PrepareMask=true；验收前核对 |
| F1 后仍闪 Smile | 查 H2（首次 Activate→SetDefault）；限定 NewGame，勿全局伤 KenMuNi |
| 其它 Prefab 误对齐 KenMuNi | 按产品逐条关，勿改字段默认值 |

---

## 验收对照

- [ ] 框淡入无小头像  
- [ ] 首句直接正确脸，无 Laugh 预告  
- [ ] 大立绘仍淡入  
- [ ] KenMuNiStart 同拍无回归  
