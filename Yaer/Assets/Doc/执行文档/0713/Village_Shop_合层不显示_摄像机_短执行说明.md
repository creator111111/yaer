# Village_Shop · 商店界面合层不显示（摄像机）— 短执行说明

**性质**：架构侦探 · 短文档（本阶段不改代码）  
**日期**：2026-07-13  
**关联**：`0713/Village_KenMuNi1_Door_Shop换场Village_Shop纯UI_…`（已能进店）

---

## 1. 结论

**能进店、能点 `UI_Shop`，但看不到「商店界面合层」，是因为合层是世界空间贴图，必须靠场景主相机拍到；当前商店场景的相机管线不完整（缺正规 `CameraComponent` / Cinemachine），Overlay 的 UI 不靠相机所以还在，合层就丢了。**

生活类比：柜台上的**触控平板**（`UI_Shop`）自己发光；墙上的**海报**（合层）要房间里的**灯/摄像头**对着才看得见。灯没摆好 → 平板还能点，海报一片黑。

---

## 2. 现象对照

| 你看到的 | 属于哪条轨 | 要不要相机 |
|----------|------------|------------|
| 购买/出售列表、按钮还能点 | **`UI_Shop`**（Canvas · Screen Space **Overlay**） | ❌ 不靠场景相机 |
| 背景大图、老板娘立绘没有 | **`商店界面合层`**（`SpriteRenderer` · 世界坐标） | ✅ **必须**场景主相机对着拍 |

场景里本来就是**双轨**（0704 / 0706 文档写过）：合层 = 美术海报；`UI_Shop` = 可点控件。

---

## 3. 根因（查了什么）

| 检查项 | 结果 |
|--------|------|
| `UI_Shop` Canvas `m_RenderMode` | **0 = Overlay** → 进店仍显示，正常 |
| `商店界面合层` | 世界坐标 Sprite；根约 `(-8.95, -5.54)`，背景中心约在原点附近 |
| 场景 `Main Camera` | 有，正交 size **5**，位在 `(0,0,-10)` |
| `CameraComponentGSM`（GSM 自动挂） | **未绑** `CameraComponent` / 虚拟相机 → 会打 Error：`CameraComponentGSM未挂载…` / `CinemachineBrain未绑定` |
| 正规室内（如 HomeScene2） | Main Camera 上有 **CinemachineBrain**，并挂到 `CameraComponentGSM` |

从 InitScene 换场进店后，常驻还有 **UICamera**（只渲 UI 层、Depth Clear），**世界合层只能指望本场景 Main Camera**。商店相机链没配齐 → 合层不进画面。

---

## 4. 施工（最短路径）

> 目标：进店后 **合层海报 + UI_Shop 同时可见**；无玩家，相机锁死不跟拍。

| 步 | 做什么 | 验收 |
|----|--------|------|
| **1** | 打开 `Village_HomeScene2`（或 KenMuNi），**复制**整套相机相关物体：`Main Camera`（含 CinemachineBrain）+ VirtualCamera + 挂在 SceneManager 下的 **Camera / CameraComponent** | 结构与正规室内一致 |
| **2** | 粘到 `Village_Shop`：删掉或替换旧的裸 `Main Camera`；把 `CameraComponent` 赋给自动生成的 **`CameraComponentGSM.cameraComponent`** | Play 后 Console **无**相机未绑定 Error |
| **3** | 机位对准合层：主相机 / vcam 世界坐标约 **`(0.65, -0.14, -10)`**（合层背景中心附近）；正交尺寸建议 **5.4**（对齐 HomeScene2） | Scene 视图里合层完整入框 |
| **4** | 在 `Village_ShopSceneManager.OnEnterScene` 末尾加：`CancelFollow()` + `SetLock(true)`（无玩家，禁止跟拍） | 进店相机不乱飘 |
| **5** | InitScene → Door_Shop 进店 | **合层可见** + UI 可点；离开回村正常 |

**不要**：为了省事关掉 `UI_Shop`；也不要在本任务把合层整页搬进 Canvas（那是 0704 长线，另开）。

**备选（更粗暴）**：若短期不想拷 Cinemachine，至少保证场景 **Main Camera 启用、Tag=MainCamera、对准合层、Depth=-1、ClearFlags=SolidColor、CullingMask 含 Default**；并处理 `CameraComponentGSM` 空引用（绑最小 `CameraComponent` 或接受 Error 但主相机必须真的在画）。**仍推荐步 1～4 一次配齐。**

---

## 5. 验收（3 条）

| # | 操作 | 通过 |
|---|------|------|
| 1 | InitScene → 进 `Village_Shop` | 能看见合层背景 / 老板娘 |
| 2 | 同屏 | `UI_Shop` 列表、Tab、决定仍可点 |
| 3 | Console | 无 `CameraComponentGSM未挂载` / `CinemachineBrain未绑定` |

---

## 6. 锚点

| 项 | 路径 |
|----|------|
| 合层 | `Village_Shop` → `商店界面合层` |
| UI | `Village_Shop` → `UI_Shop`（Overlay） |
| 相机模块 | `CameraComponentGSM.cs` / `CameraComponent.cs` |
| 室内相机样板 | `Village_HomeScene2` 的 Main Camera + CameraComponent |
| 双轨说明 | `0704/商店界面合层转UI组件_…`、`0706/Village_Shop_Play闪退_…` |

---

**文档路径**：`Assets/Doc/执行文档/0713/Village_Shop_合层不显示_摄像机_短执行说明.md`
