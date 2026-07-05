使用预制体SystemTipsPanel2
需要知道对话系统
预制体是NormalDialogueNewPanel
![[Pasted image 20260405203823.png]]
所以是GoblinAndGushaStoryTrigger来触发的
使用StoryPrefabName来触发的
这里触发的是WestRappRoadGoblinAndGusha
然而simplestorytrigger里面已有 `StoryComponentGSM.onStoryEnd`
现在解法是在GoblinAndGushaStoryTrigger上面挂一个脚本来触发
然后发现使用nodecanvas
## 第一步：写一个自定义 `ActionTask`（脚本）
 新建脚本，放在和你们其它任务同一习惯的路径，例如：  
`Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/UIPanel/`
## 第二步：让 Unity 认出这个任务

## 第三步：在对话图里「插入」节点

1. 打开 `WestRappRoadGoblinAndGusha.prefab`。
2. 选中 Dialogue Tree Controller，用 NodeCanvas 打开绑定的 Dialogue Tree（和平时改 Statement 一样）。
3. 在需要 弹窗之前 的那条连线上：
    - 右键空白处 → 添加节点（具体文案因版本可能是 Action / Perform Action / Execute Action 等）。
4. 在新 Action 节点里，Task 下拉选你刚写的类（会出现在你设的 `Category` 下）。
5. 连线：
    - 上一句 Statement → 你的 Action → 下一句 Statement（或下一个分支）。  
        这样执行顺序就是：说完上一句 → 进 Action（弹窗并阻塞）→ 点确定 → `EndAction()` → 再进下一句。

若你想在 某句台词播到一半 弹窗，一般要改字幕流程或拆成两句 Statement，中间插 Action；最省事仍是：上一句 Statement 结束 → Action 弹窗等待 → 下一句 Statement。