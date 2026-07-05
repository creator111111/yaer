# InitPanel(Clone) 启用逻辑

说明 **`InitPanel` 在何时被打开、何时算「展示结束」、与 `ProcedurePreload` 如何衔接**，便于在正确位置接入 **ESC 跳过** 或其它调试入口。

---

## 1. 为什么 Hierarchy 里叫 `InitPanel(Clone)`

Game Framework 的 UI 系统在运行时 **`OpenUIForm` 会实例化预制体**，Unity 默认把克隆体命名为 **`原名(Clone)`**。逻辑类仍是 `InitFormLogic`，与名称中的 `(Clone)` 无关。

- 预制体路径：`Assets/GameRes/Prefabs/UI/InitPanel.prefab`（`UIPrefabPath.InitPanel`）
- 分组：`EUIGroup.System`（见 `ProcedurePreload`）

---

## 2. 在启动流程里处于哪一段

整体顺序（与 `project_context` 一致）：

1. **`ProcedureLaunch`**：初始化资源 `ResourceComponent.InitResources`，完成后 `GameManager.OnInit()` / `OnEnter()`，再 **`ChangeState<ProcedurePreload>`**。
2. **`ProcedurePreload`**：打开 **InitPanel**，并行把「预加载完成」标记置真；等 **Init 界面展示流程结束** 后，黑幕关闭表单并 **加载 `StartScene`**，再 **`ChangeState<ProcedureMenu>`**（主菜单）。

```mermaid
flowchart LR
  launch[ProcedureLaunch]
  preload[ProcedurePreload]
  menu[ProcedureMenu]
  launch --> preload
  preload -->|"LoadScene StartScene + callback"| menu
```

---

## 3. 谁打开 InitPanel、何时算「可以进下一段」

### 3.1 打开

进入 **`ProcedurePreload.OnEnter`** 时调用 `UIComponentGM.OpenUIForm(UIPrefabPath.InitPanel, ...)`，并在 **`OpenFormArgs.callBack`** 里拿到 **`InitFormLogic`**，给 **`InitFormProxy.onHideEnd`** 赋值：

```35:50:f:\Yaer\yaer\Yaer\Assets\Scripts\Game\GameRuntime\Procedure\ProcedurePreload.cs
            // 打开初始化面板
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.InitPanel, EUIGroup.System, new OpenFormArgs()
            {
                callBack = logic =>
                {
                    if (logic is InitFormLogic initFormLogic)
                    {
                        this.initFormLogic = initFormLogic;
                        // 初始化面板加载完成
                        initFormLogic.GetProxy<InitFormProxy>().onHideEnd = () =>
                        {
                            formDone = true;
                        };
                    }
                }
            });
```

含义：**当 Init 界面内部判定「整段展示播完」时**，应调用 **`InitFormProxy.OnHideEnd()`**（见下），从而 **`formDone = true`**，预加载流程才能继续。

### 3.2 `ProcedurePreload` 里两个条件

- **`isPreloadEnd`**：在 `OnEnter` 末尾被设为 **`true`**（当前实现里预加载与开界面同时结束，未做异步等待）。
- **`formDone`**：仅当 **`InitFormProxy.onHideEnd`** 被调用后为 **`true`**。

二者在 **`OnUpdate`** 里同时满足时，执行 **`FadeCloseForm` → `LoadScene(StartScene)` → `ProcedureMenu`**：

```56:75:f:\Yaer\yaer\Yaer\Assets\Scripts\Game\GameRuntime\Procedure\ProcedurePreload.cs
            // 加载结束开始淡入黑幕
            if (isPreloadEnd && formDone)
            {
                initFormLogic.FadeCloseForm(() =>
                {
                    // 切换场景
                    GameManager.GetGMComponent<ChangeSceneComponentGM>().LoadScene(new LoadSceneArgs()
                    {
                        sceneName = SceneName.StartScene,
                        callBack = () => { ChangeState<ProcedureMenu>(procedureOwner); } // 切换到主菜单
                    });
                });
                
                formDone = false;
                isPreloadEnd = false;
            }
```

注意：若 **`callBack` 未执行到**（例如 `InitFormLogic` 未正确绑定为 `InitFormLogic`），**`initFormLogic` 为空**，后续 **`FadeCloseForm` 会空引用**。

---

## 4. InitFormLogic 内部：展示链与 `OnHideEnd` 的触发点

### 4.1 `DisplaySeq` 轮播

- **`DisplaySeq`**：Inspector 中配置的若干 **GameObject**，依次显示，中间用 **黑幕淡入淡出**（`BlackFadeComponent`）衔接。
- **`FinishDisplay`**：当 `DisplaySeq` 为空或已播到 **最后一项**（`DisplayProgress >= DisplaySeq.Count - 1`）时为真。

每一轮大致为：**淡出黑幕 → 显示当前页 → 若不是最后一页则淡入黑幕再进下一页；若是最后一页，淡出黑幕后回调 `InitFormProxy.OnHideEnd`**（即通知 `ProcedurePreload` 置 `formDone`）。

```75:93:f:\Yaer\yaer\Yaer\Assets\Scripts\Game\GameRuntime\UI\FormLogic\Init\InitFormLogic.cs
        private void DisplayNextGO()
        {
            if (CurrentDisplayGO != null)
            {
                CurrentDisplayGO.SetActive(false);
            }
            DisplayProgress++;
            if (CurrentDisplayGO != null)
            {
                CurrentDisplayGO.SetActive(true);
                if (!FinishDisplay)
                {
                    blackFade.HideFade(HideCurrentDisplay);
                }
                else
                {
                    blackFade.HideFade(GetProxy<InitFormProxy>().OnHideEnd);
                }
            }
        }
```

`HideCurrentDisplay` → `ShowFade` → 再 `DisplayNextGO`，形成链式播放。

### 4.2 `InitFormProxy` 仅负责把「结束」往外传

```6:11:f:\Yaer\yaer\Yaer\Assets\Scripts\Game\GameRuntime\UI\FormLogic\Init\InitFormProxy.cs
    public class InitFormProxy: BaseFormProxy
    {
        public Action onHideEnd;

        public void OnHideEnd() => onHideEnd?.Invoke();
    }
```

**`ProcedurePreload` 把 `onHideEnd` 设成 `() => formDone = true`**，因此 **`OnHideEnd()` 被调用 = Init 展示阶段结束**。

### 4.3 关闭 Init 界面并切场景

展示结束后由 **`ProcedurePreload`** 调用 **`InitFormLogic.FadeCloseForm`**，内部使用 **`BlackFadeComponent.CloseFormShowFade`**：先黑幕过渡再关 UI，再执行加载 `StartScene` 的回调。

---

## 5. 与调试工具的关系（避免误以为已接好）

`GameTool.SkipInitScene` 会发送 **`NotificationName.UI.HIDE_INIT_PANEL`**，但当前工程内 **仅有发送、未见其它脚本注册该通知并驱动 `InitFormLogic` 结束**。若要做「调试跳过」，应 **在 `InitFormLogic`（或统一输入处）显式完成与 `OnHideEnd` 等价的结束路径**，而不是只依赖该通知。

---

## 6. ESC 跳过（已实现：仅编辑器）

在 **`InitFormLogic`** 中已接入：**仅在 `UNITY_EDITOR` 下**，运行中按 **ESC** 会跳过 `DisplaySeq` 轮播，并调用与正常结束相同的 **`CompleteInitSequence()` → `InitFormProxy.OnHideEnd()`**，从而 **`ProcedurePreload`** 继续 **`FadeCloseForm` → StartScene**。

实现要点：

- **`abortDisplaySequence`**：阻断链式回调里对 **`DisplayNextGO` / `HideCurrentDisplay`** 的继续执行。
- **`sequenceEndReported` / `CompleteInitSequence()`**：保证 **`OnHideEnd` 只触发一次**（避免 ESC 与黑幕动画晚到回调重复）。
- 跳过后会 **`ResetHideState()`** 并重置展示物体显隐；正式构建中不包含 `TryEditorSkipInitSequence`，行为与改前一致。

若仍希望 **Player 包体**也支持跳过，需去掉 `#if UNITY_EDITOR` 并评估误触（例如与菜单 ESC 冲突）。

---

### 6.1 历史说明（接入前曾建议的手动要点）

目标：**尽快触发与正常播完相同的结束条件**，即 **`InitFormProxy.OnHideEnd()`**，让 **`formDone == true`**。

曾需注意：黑幕回调叠加、**`BlackFadeComponent.IsBusy`**、以及 **`DisplaySeq` 为空** 时可能不触发结束（编辑器 ESC 已对空序列跳过路径兜底 **`CompleteInitSequence`**）。

---

## 7. 关键文件索引

| 说明 | 路径 |
|------|------|
| 打开 Init 与 `formDone` | `Assets/Scripts/Game/GameRuntime/Procedure/ProcedurePreload.cs` |
| 展示链与黑幕 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Init/InitFormLogic.cs` |
| 结束回调桥 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Init/InitFormProxy.cs` |
| 黑幕与关界面 | `Assets/Scripts/Game/GameRuntime/UI/Component/BlackFade/BlackFadeComponent.cs` |
| 进入 Preload 之前 | `Assets/Scripts/Game/GameRuntime/Procedure/ProcedureLaunch.cs` |
| 预制体常量 | `Assets/Scripts/Game/Static/Path/UIPrefabPath.cs` |

---

## 8. 文档维护

- 若修改 **`ProcedurePreload`** 与 **`InitFormLogic`** 的衔接方式（例如预加载改为异步），请同步更新 §3、§4。
- 若 **`HIDE_INIT_PANEL`** 将来接入 **Mediator/Command**，请在 §5 注明监听方与行为。
