using System;
using Game.GameRuntime.Entities.Component.Move;
using System.Collections.Generic;
using Game.GameRuntime.GameSceneManager.Base;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;
using Game.Static.Enum;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr;

namespace Game.GameRuntime.Entities.Player.Components
{
    public class PlayerInputComponent : BaseGFComponentMono, IPlayerComponent
    {
        public enum AutoInputMove
        {
            Left = -1,
            None = 0,
            Right = 1,
        }

        public PlayerLogic PlayerLogic { get; set; }

        /// <summary>
        /// 当前移动/输入语义（村庄 2.5D 时丢弃部分战斗指令，见策划文档 AC-04）。
        /// </summary>
        public PlayerLocomotionMode LocomotionMode { get; private set; } = PlayerLocomotionMode.Default;

        /// <summary>
        /// 由 <see cref="PlayerLogic.SetVillageExplorationMode"/> 切换；会清理已入队的被禁指令，避免残留一帧触发。
        /// </summary>
        /// <param name="mode">新模式</param>
        public void SetLocomotionMode(PlayerLocomotionMode mode)
        {
            LocomotionMode = mode;
            if (mode == PlayerLocomotionMode.Village2_5D)
            {
                curPlayerAllCmds.RemoveAll(IsBlockedInVillageExploration);
            }
        }

        /// <summary>村庄探索下不允许入队、不应触发回调的指令（与策划裁剪表一致；DNF 式移动禁止跳跃）。</summary>
        private static bool IsBlockedInVillageExploration(ControlInputType cmd)
        {
            return cmd == ControlInputType.Squat
                   || cmd == ControlInputType.NormalAttack
                   || cmd == ControlInputType.SmashAttack
                   || cmd == ControlInputType.DashAttack
                   || cmd == ControlInputType.Jump;
        }

        private InputActions inputActions;

        private float axisX;
        private float axisY;
        private bool cantFlip;
        private bool cantMove;
        private bool cantJump;
        private bool cantLeft;
        private bool cantRight;

        private float lastRealTime = 0;
        /// <summary>最近一次按下普攻键的 Time.time，用于落地缓冲；与指令队列独立——队列在跳跃中可能被 Parse 清掉，时间戳仍保留。</summary>
        private float lastNormalAttackInputTime = -999f;
        /// <summary>同 <see cref="lastNormalAttackInputTime"/>，用于重击（K/鼠标右键等映射）。</summary>
        private float lastSmashAttackInputTime = -999f;
        /// <summary>同 <see cref="lastNormalAttackInputTime"/>，用于冲击/冲刺攻击（L/Shift 等映射）。</summary>
        private float lastDashAttackInputTime = -999f;
        public bool canInputContorll { get; set; } = true;// 是否接受控制输入
        //public Action<bool> onRightInput;
        //public Action<bool> onLeftInput;
        //public Action onJumpInput;
        public Action<bool> onRightInput { get => moveInputFuncDict[ControlInputType.Right]; set => moveInputFuncDict[ControlInputType.Right] = value; }
        public Action<bool> onLeftInput { get => moveInputFuncDict[ControlInputType.Left]; set => moveInputFuncDict[ControlInputType.Left] = value; }
        public Action<bool> onJumpInput { get => moveInputFuncDict[ControlInputType.Jump]; set => moveInputFuncDict[ControlInputType.Jump] = value; }
        public Action onSquatInput { get => controlInputFuncDict[ControlInputType.Squat]; set => controlInputFuncDict[ControlInputType.Squat] = value; }
        public Action onSitDownInput { get => controlInputFuncDict[ControlInputType.SitDown]; set => controlInputFuncDict[ControlInputType.SitDown] = value; }
        public Action onNormalAtkInput { get => controlInputFuncDict[ControlInputType.NormalAttack]; set => controlInputFuncDict[ControlInputType.NormalAttack] = value; }
        public Action onSmashAtkInput { get => controlInputFuncDict[ControlInputType.SmashAttack]; set => controlInputFuncDict[ControlInputType.SmashAttack] = value; }
        public Action onDashAtkInput { get => controlInputFuncDict[ControlInputType.DashAttack]; set => controlInputFuncDict[ControlInputType.DashAttack] = value; }
        public Action onInteractInput { get => controlInputFuncDict[ControlInputType.Interact]; set => controlInputFuncDict[ControlInputType.Interact] = value; }
        List<ControlInputType> curPlayerAllCmds = new List<ControlInputType>(); // 当前玩家输入的所有指令
        // 默认玩家控制相关指令映射
        Dictionary<KeyCode, ControlInputType> keyCodeToCmdDict = new Dictionary<KeyCode, ControlInputType>() {
            { KeyCode.A, ControlInputType.Left}, { KeyCode.D , ControlInputType.Right },
            { KeyCode.Mouse0 , ControlInputType.NormalAttack }, { KeyCode.Mouse1 , ControlInputType.SmashAttack },
            { KeyCode.C , ControlInputType.Squat }, { KeyCode.LeftShift , ControlInputType.DashAttack },
            { KeyCode.Space , ControlInputType.Jump }, {KeyCode.LeftControl, ControlInputType.SitDown},
            { KeyCode.E, ControlInputType.Interact },
        };
        // 移动指令对应的执行函数
        public Dictionary<ControlInputType, Action<bool>> moveInputFuncDict = new Dictionary<ControlInputType, Action<bool>>() {
            { ControlInputType.Left, null}, { ControlInputType.Right, null},
            { ControlInputType.Jump, null}
        };
        // 其他指令对应的执行函数
        public Dictionary<ControlInputType, Action> controlInputFuncDict = new Dictionary<ControlInputType, Action>() {
            { ControlInputType.Squat, null}, { ControlInputType.SitDown, null},{ ControlInputType.NormalAttack, null},
            { ControlInputType.DashAttack, null},{ ControlInputType.SmashAttack, null}, {ControlInputType.Interact, null},
        };
        // 移动指令对应的移动方向
        Dictionary<ControlInputType, EDirectionType> moveCmdDirData = new Dictionary<ControlInputType, EDirectionType>() {
            { ControlInputType.Left, EDirectionType.Left}, { ControlInputType.Right, EDirectionType.Right},
        };
        // 指令间的组合
        Dictionary<ControlInputType, List<ControlInputType>> moveCmdLinkData = new Dictionary<ControlInputType, List<ControlInputType>>()
        {
            { ControlInputType.Left, new List<ControlInputType>{ 
                ControlInputType.Jump, ControlInputType.NormalAttack, ControlInputType.DashAttack,ControlInputType.SmashAttack,} 
            },
            { ControlInputType.Right, new List<ControlInputType>{
                ControlInputType.Jump, ControlInputType.NormalAttack, ControlInputType.DashAttack,ControlInputType.SmashAttack,}
            },
            { ControlInputType.Jump, new List<ControlInputType>{ ControlInputType.Left, ControlInputType.Right } },
        };
        // 需要持续按键执行的指令
        List<ControlInputType> longPressCmd = new List<ControlInputType>()
        {
            ControlInputType.Left, ControlInputType.Right,ControlInputType.Squat
        };
        // 获取玩家当前输入的第X个指令
        public ControlInputType GetPlayerCurInputCmd(int cmdIndex = 0)
        {
            return curPlayerAllCmds.Count > cmdIndex ? curPlayerAllCmds[cmdIndex] : ControlInputType.None;
        }

        public void SetInputAciton(ControlInputType cmd, Action<bool> aciton)
        {
            moveInputFuncDict[cmd] = aciton;
        }
        public AutoInputMove AutoMoveState = AutoInputMove.None;

        /// <summary>
        /// 有XY输入
        /// </summary>
        //public bool HasMoveInput => axisX != 0 || axisY != 0;
        public bool HasMoveInput()
        {
            var moveCmd = new List<ControlInputType>() {
                ControlInputType.Left, ControlInputType.Right
            };
            var curCmd = GetPlayerCurInputCmd();
            return moveCmd.Contains(curCmd);
        }

        /// <summary>
        /// 扫描 <see cref="curPlayerAllCmds"/> 是否任意位置含左/右。
        /// 说明：队首常被 Jump、Interact 等单次指令占位，此时玩家仍按住 A/D 但 <see cref="HasMoveInput"/> 为假；
        /// 村庄 CombatRun 若仅依赖队首会与纵深 Raw 轴组合误判，导致每帧 <c>StopMoveInX</c>（见执行文档 0513）。
        /// </summary>
        /// <returns>队列中存在 Left 或 Right 则 true。</returns>
        public bool HasHorizontalMoveCommandInQueue()
        {
            for (int i = 0; i < curPlayerAllCmds.Count; i++)
            {
                ControlInputType c = curPlayerAllCmds[i];
                if (c == ControlInputType.Left || c == ControlInputType.Right)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 村庄 2.5D 下「横向位移意图」：与 <see cref="TownPlayerLocomotion.HasVillageDepthMoveForHomeStateMachine"/> 使用 Raw Vertical 对称，
        /// 综合队首、整队左/右、以及 <c>Input.GetAxisRaw("Horizontal")</c>，避免仅凭 <see cref="HasMoveInput"/> 队首语义误伤横移。
        /// 非村庄模式时退化为 <see cref="HasMoveInput"/>，供状态机分支外调用不致行为分叉。
        /// </summary>
        /// <returns>判定为仍有横向探索意图则 true。</returns>
        public bool HasVillageExploreHorizontalMoveIntent()
        {
            if (LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return HasMoveInput();
            }

            if (HasMoveInput())
            {
                return true;
            }

            if (HasHorizontalMoveCommandInQueue())
            {
                return true;
            }

            // 与纵深意图一致走 Raw 轴，覆盖键位表与队列短暂不一致的帧
            const float horizontalDeadZone = 0.01f;
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > horizontalDeadZone)
            {
                return true;
            }

            // 兜底：部分工程里 Horizontal 轴未绑或双机位下 Raw 恒为 0，但物理键仍有效；避免 CombatRun 误判为「无横向」而每帧 StopMoveInX（0513/0514 现场：能上下不能左右）
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)
                                        || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 村庄纵深键位意图：Raw Vertical + W/S/方向键。不含松键后的纵深惯性（惯性清 X 会误伤 0513）。
        /// </summary>
        public bool HasVillageExploreVerticalMoveIntent()
        {
            const float deadZone = 0.01f;
            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > deadZone)
            {
                return true;
            }

            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)
                   || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);
        }

        /// <summary>
        /// 村庄横向符号：轴 → 物理键/键位表 → 队列第一条 Left/Right。禁止用默认朝右当第一手（0818）。
        /// 左右同时按下跟队列先后。轴恒为 0 时仍能给出 ±1（本工程 Horizontal 可能未绑）。
        /// </summary>
        /// <returns>-1 左、+1 右、0 解析不出。</returns>
        public float GetVillageExploreHorizontalSign()
        {
            const float deadZone = 0.01f;
            float axis = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(axis) > deadZone)
            {
                return Mathf.Sign(axis);
            }

            bool leftHeld = IsVillageHorizontalKeyHeld(-1);
            bool rightHeld = IsVillageHorizontalKeyHeld(1);
            if (leftHeld != rightHeld)
            {
                return leftHeld ? -1f : 1f;
            }

            ControlInputType queued = FindFirstHorizontalCommandInQueue();
            if (queued == ControlInputType.Left)
            {
                return -1f;
            }

            if (queued == ControlInputType.Right)
            {
                return 1f;
            }

            return 0f;
        }

        /// <summary>村庄纵深符号：轴优先，否则 W/↑=+1、S/↓=-1。</summary>
        public float GetVillageExploreVerticalSign()
        {
            const float deadZone = 0.01f;
            float axis = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(axis) > deadZone)
            {
                return Mathf.Sign(axis);
            }

            bool upHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool downHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            if (upHeld != downHeld)
            {
                return upHeld ? 1f : -1f;
            }

            return 0f;
        }

        /// <summary>队列从前到后第一条 Left/Right，与 Parse 队首语义一致。</summary>
        public ControlInputType FindFirstHorizontalCommandInQueue()
        {
            for (int i = 0; i < curPlayerAllCmds.Count; i++)
            {
                ControlInputType c = curPlayerAllCmds[i];
                if (c == ControlInputType.Left || c == ControlInputType.Right)
                {
                    return c;
                }
            }

            return ControlInputType.None;
        }

        /// <param name="sign">-1 查左，+1 查右。</param>
        private bool IsVillageHorizontalKeyHeld(int sign)
        {
            if (sign < 0)
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                {
                    return true;
                }

                return IsBoundCommandKeyHeld(ControlInputType.Left);
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                return true;
            }

            return IsBoundCommandKeyHeld(ControlInputType.Right);
        }

        private bool IsBoundCommandKeyHeld(ControlInputType cmd)
        {
            foreach (var kv in keyCodeToCmdDict)
            {
                if (kv.Value == cmd && Input.GetKey(kv.Key))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 同时XY输入
        /// </summary>
        //public bool HasXYInput => axisX != 0 && axisY != 0;
        public bool HasXYInput()
        {
            // 目前游戏暂无Y轴移动
            return false; 
        }

        void Start()
        {
            lastRealTime = Time.realtimeSinceStartup;
        }

        protected override void OnInit()
        {
            inputActions = new InputActions();
            inputActions.Player.Enable();
            AutoMoveState = AutoInputMove.None;
            // 设置当前按键对应的指令
            var configData = GameManager.GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
            keyCodeToCmdDict.Clear();
            var ingoreKeyList = new List<ControlInputType>() { 
                ControlInputType.NextSentence, ControlInputType.SkipDialogue,
            };
            foreach (var data in configData.KeyboardMouseInputConfig)
            {
                var key = data.Value;
                var cmd = data.Key;
                if (ingoreKeyList.Contains(cmd)) { continue; }
                keyCodeToCmdDict[key] = cmd;
            }
            
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            checkGameHasPasueByOther();
            
            if (!canInputContorll) { return; }
            // 检测按键输入
            if (!cantMove)
            {
                
                // 如果当前按下指定按键则添加对应的指令
                foreach (var keyCode in keyCodeToCmdDict.Keys)
                {

                    if (GetKeyDown(keyCode))
                    {
                        var cmd = keyCodeToCmdDict[keyCode];
                        // 村庄模式：键位表若把蹲绑在 S，仍通过 Vertical 走纵深 Y；此处不入队战斗/蹲指令（AC-04）
                        if (LocomotionMode == PlayerLocomotionMode.Village2_5D && IsBlockedInVillageExploration(cmd))
                        {
                            continue;
                        }
                        if (cmd == ControlInputType.NormalAttack)
                        {
                            lastNormalAttackInputTime = Time.time;
                        }
                        else if (cmd == ControlInputType.SmashAttack)
                        {
                            lastSmashAttackInputTime = Time.time;
                        }
                        else if (cmd == ControlInputType.DashAttack)
                        {
                            lastDashAttackInputTime = Time.time;
                        }
                        if (curPlayerAllCmds.Contains(cmd)) continue;
                        curPlayerAllCmds.Insert(0, cmd);// 新的指令要放在最前面
                    }
                    else if (GetKeyUp(keyCode))
                    {
                        // 松开某个指定按键时，则移除对应的指令
                        var cmd = keyCodeToCmdDict[keyCode];
                        if (longPressCmd.Contains(cmd))
                        {
                            // 只有持续触发的按键才需要在松开键的时候移除指令
                            foreach(var oldCmd in new List<ControlInputType>(curPlayerAllCmds))
                            {
                                if (cmd == oldCmd)
                                {
                                    curPlayerAllCmds.Remove(oldCmd);
                                }
                            }
                        }
                    }
                }
            }

            ParsePlayerCmd();
            //Move();
            //Jump();
        }

        public void SetAllowMove(bool value)
        {
            if (PlayerLogic.hasInStoryEventState) { value = false; }// 玩家处于故事对话中时默认不能移动
            cantMove = !value;
            cantJump = !value;
            if (!value)
            {
                curPlayerAllCmds.Clear();// 不能移动时清除所有指令
            }
        }

        public void SetAllowFlip(bool value) => cantFlip = value;

        private void Jump()
        {
            if (GetKeyDown(KeyCode.Space) && !cantJump)
            {
                onJumpInput?.Invoke(true);
            }
        }

        // 旧移动方法，现在弃用
        //private void Move()
        //{
        //    axisX = Input.GetAxisRaw("Horizontal");
        //    axisY = Input.GetAxisRaw("Vertical");

        //    switch (AutoMoveState)
        //    {
        //        case AutoInputMove.Left:
        //            axisX = -1;
        //            break;
        //        case AutoInputMove.Right:
        //            axisX = 1;
        //            break;
        //        case AutoInputMove.None:
        //            if (cantMove)
        //            {
        //                axisX = 0;
        //                axisY = 0;
        //                return;
        //            }
        //            break;
        //    }

        //    // 
        //    if (cantLeft && axisX < 0) axisX = 0;
        //    if (cantRight && axisX > 0) axisX = 0;

        //    // 是否允许翻转
        //    if (cantFlip) axisX = 0;

        //    if (axisX > 0) onRightInput?.Invoke(true);
        //    if (axisX < 0) onLeftInput?.Invoke(true);
        //}

        private void ParsePlayerCmd()
        {
            // 自动移动时自动设置对应的指令
            switch (AutoMoveState)
            {
                case AutoInputMove.Left:
                    if (!curPlayerAllCmds.Contains(ControlInputType.Left))
                    {
                        curPlayerAllCmds.Add(ControlInputType.Left);
                    }
                    break;
                case AutoInputMove.Right:
                    if (!curPlayerAllCmds.Contains(ControlInputType.Right))
                    {
                        curPlayerAllCmds.Add(ControlInputType.Right);
                    }
                    break;
                case AutoInputMove.None:
                    if (cantMove)
                    {
                        curPlayerAllCmds.Clear();
                        return;
                    }
                    break;
            }
            var curPlayerCmd = curPlayerAllCmds.Count > 0 ? curPlayerAllCmds[0] : ControlInputType.None;
            if (curPlayerCmd == ControlInputType.None) { return; }
            if (curPlayerAllCmds.Count <= 0) { return; } // 当前玩家没有输入任何指令则直接返回
            ParseMoveCmd(curPlayerCmd);
            ParseOtherCmd(curPlayerCmd);
            if (!longPressCmd.Contains(curPlayerCmd))
            {
                curPlayerAllCmds.Remove(curPlayerCmd);
            }
        }

        private void ParseOtherCmd(ControlInputType curPlayerCmd)
        {
            if (LocomotionMode == PlayerLocomotionMode.Village2_5D && IsBlockedInVillageExploration(curPlayerCmd))
            {
                return;
            }
            // 获取不同指令对应的输入方法并执行
            if (controlInputFuncDict.TryGetValue(curPlayerCmd, out Action curInputAction))
            {
                curInputAction?.Invoke();
                // 如果有指令组合就继续往下添加逻辑
            }
        }

        void ParseMoveCmd(ControlInputType curPlayerCmd)
        {
            // 跳跃在 moveInputFuncDict 中走本路径（不走 ParseOtherCmd）；村内必须在此一并拦截，否则 Space 仍会触发 onJumpInput。
            if (LocomotionMode == PlayerLocomotionMode.Village2_5D && IsBlockedInVillageExploration(curPlayerCmd))
            {
                return;
            }

            var playerMove = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            // IsMoveUp/Down 实为 Velocity.y 正负，并非仅「跳跃状态机」；贴地微弹跳、重力与 FixedUpdate 相位都可能使 y≠0，
            // 此处整段 return 会跳过本帧 Left/Right 的 MoveLeft/MoveRight 刷新，与 CombatRun 清 X 叠加后加重「横移迟滞」（执行文档 0513 修订 §1 次要因素）。
            // 替代方案：改为读 Animator 跳跃子态再短路，耦合面大；村庄 2.5D 下仅对横移指令放行，纵深仍由 TownPlayerLocomotion 写权威 Y。
            bool blockByVerticalVelocity = playerMove.IsMoveUp || playerMove.IsMoveDown;
            bool villageHorizontalRefresh =
                LocomotionMode == PlayerLocomotionMode.Village2_5D
                && (curPlayerCmd == ControlInputType.Left || curPlayerCmd == ControlInputType.Right);
            if (blockByVerticalVelocity && !villageHorizontalRefresh)
            {
                return;
            }

            // 
            if (cantLeft && axisX < 0) axisX = 0;
            if (cantRight && axisX > 0) axisX = 0;

            // 是否允许翻转
            if (cantFlip) axisX = 0;
            // 获取不同指令对应的输入方法并执行
            if (moveInputFuncDict.TryGetValue(curPlayerCmd, out Action<bool> curInputAction))
            {
                playerMove.moveDirs.Clear();
                curInputAction?.Invoke(true);
                // 添加人物的移动方向
                if (moveCmdDirData.TryGetValue(curPlayerCmd, out EDirectionType moveDir))
                {
                    playerMove.moveDirs.Add(moveDir);
                }
                var lastCmd = GetPlayerCurInputCmd(1); // 获取当前指令的上一个指令进行指令组合操作
                if (lastCmd != ControlInputType.None && moveCmdLinkData.TryGetValue(curPlayerCmd, out List<ControlInputType> extraEmdList))
                {
                    // 能够进行指令组合并且存在已经输入可组合的指令
                    if (extraEmdList.Contains(lastCmd) && (moveInputFuncDict.TryGetValue(lastCmd, out Action<bool> lastInputAction)))
                    {
                        // 添加人物的移动方向
                        if (moveCmdDirData.TryGetValue(lastCmd, out EDirectionType moveDir2))
                        {
                            playerMove.moveDirs.Add(moveDir2);
                        }
                        lastInputAction?.Invoke(true);
                    }
                }
            }
        }

        public bool HasSquatInput()
        {
            // 是否有蹲下按键输入
            return curPlayerAllCmds.Contains(ControlInputType.Squat);
        }

        public bool HasAnyInput()
        {
            // 是否有任何指令输入
            return curPlayerAllCmds.Count > 0;
        }

        public bool HasRecentNormalAttackInput(float bufferWindow)
        {
            if (bufferWindow <= 0f) { return false; }
            return Time.time - lastNormalAttackInputTime <= bufferWindow;
        }

        public void ConsumeNormalAttackInput()
        {
            lastNormalAttackInputTime = -999f;
            curPlayerAllCmds.Remove(ControlInputType.NormalAttack);
        }

        public bool HasRecentSmashAttackInput(float bufferWindow)
        {
            if (bufferWindow <= 0f) { return false; }
            return Time.time - lastSmashAttackInputTime <= bufferWindow;
        }

        public void ConsumeSmashAttackInput()
        {
            lastSmashAttackInputTime = -999f;
            curPlayerAllCmds.Remove(ControlInputType.SmashAttack);
        }

        public bool HasRecentDashAttackInput(float bufferWindow)
        {
            if (bufferWindow <= 0f) { return false; }
            return Time.time - lastDashAttackInputTime <= bufferWindow;
        }

        public void ConsumeDashAttackInput()
        {
            lastDashAttackInputTime = -999f;
            curPlayerAllCmds.Remove(ControlInputType.DashAttack);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // 游戏失去焦点时，清空所有输入并停止人物移动
                curPlayerAllCmds.Clear();
                Input.ResetInputAxes();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                curPlayerAllCmds.Clear();
                Input.ResetInputAxes();
            }
        }

        // 检测游戏是否因为其他原因暂停了
        private void checkGameHasPasueByOther()
        {
            float delta = Time.realtimeSinceStartup - lastRealTime;
            if (delta > 0.1f) // 阈值可调
            {
                curPlayerAllCmds.Clear();
                Input.ResetInputAxes();
            }
            lastRealTime = Time.realtimeSinceStartup;

        }

        #region 键鼠

        public bool GetKeyDown(KeyCode key)
        {
            return !cantMove && Input.GetKeyDown(key);
        }

        public bool GetKey(KeyCode key)
        {
            return !cantMove && Input.GetKey(key);
        }

        public bool GetKeyUp(KeyCode key)
        {
            return !cantMove && Input.GetKeyUp(key);
        }

        public bool GetMouseDown(int key)
        {
            return !cantMove && Input.GetMouseButtonDown(key);
        }

        public bool GetMouse(int key)
        {
            return !cantMove && Input.GetMouseButton(key);
        }

        public bool GetMouseUp(int key)
        {
            return !cantMove && Input.GetMouseButtonUp(key);
        }

        #endregion

    }
}