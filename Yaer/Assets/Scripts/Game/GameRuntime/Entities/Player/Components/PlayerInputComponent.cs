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
        /// 村庄探索移动键族：由设置 Left/Right 推导；纵深不另开设置槽（0922）。
        /// </summary>
        public enum VillageMoveKeyFamily
        {
            /// <summary>Left=A、Right=D → 纵深 W/S。</summary>
            Wasd = 0,
            /// <summary>Left=←、Right=→ → 纵深 ↑/↓。</summary>
            ArrowKeys = 1,
            /// <summary>其它绑定：仅认表内 Left/Right，纵深 0。</summary>
            Custom = 2,
        }

        /// <summary>当前缓存的键族；改键后须 <see cref="RebuildKeyBindingsFromSettings"/>。</summary>
        private VillageMoveKeyFamily _cachedVillageMoveKeyFamily = VillageMoveKeyFamily.Wasd;

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
        /// 村庄 2.5D「横向位移意图」：队首/队列 Left·Right，或当前键族绑定的左右键按住。
        /// <para>0922：去掉 <c>GetAxisRaw(Horizontal)</c> 与 WASD∪箭头双族硬编码，避免设置 A/D 时方向键仍能动。</para>
        /// 非村庄模式退化为 <see cref="HasMoveInput"/>。
        /// </summary>
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

            // 仅认设置绑定的 Left/Right 键（键族单通道）
            return IsVillageHorizontalKeyHeld(-1) || IsVillageHorizontalKeyHeld(1);
        }

        /// <summary>
        /// 村庄纵深意图：由 Left/Right 键族推导（WASD→W/S，箭头→↑↓；Custom→无）。
        /// 不含松键后纵深惯性。0922 去掉裸 Vertical 轴与双族硬编码。
        /// </summary>
        public bool HasVillageExploreVerticalMoveIntent()
        {
            return Mathf.Abs(GetVillageExploreVerticalSign()) > 0.01f;
        }

        /// <summary>
        /// 村庄横向符号：绑定左右键按住 → 队列第一条 Left/Right。
        /// 禁止 Axis / 另一族硬编码；禁止默认朝右当第一手。
        /// </summary>
        /// <returns>-1 左、+1 右、0 解析不出。</returns>
        public float GetVillageExploreHorizontalSign()
        {
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

        /// <summary>
        /// 村庄纵深符号：键族推导。WASD：W=+1、S=-1；箭头：↑=+1、↓=-1；Custom：0。
        /// 0922 进对话禁移 C：禁移时强制 0，禁止裸 GetKey 绕过 cantMove 灌进 Town。
        /// </summary>
        public float GetVillageExploreVerticalSign()
        {
            if (cantMove)
            {
                return 0f;
            }

            bool upHeld;
            bool downHeld;
            switch (ResolveVillageMoveKeyFamily())
            {
                case VillageMoveKeyFamily.Wasd:
                    upHeld = Input.GetKey(KeyCode.W);
                    downHeld = Input.GetKey(KeyCode.S);
                    break;
                case VillageMoveKeyFamily.ArrowKeys:
                    upHeld = Input.GetKey(KeyCode.UpArrow);
                    downHeld = Input.GetKey(KeyCode.DownArrow);
                    break;
                default:
                    return 0f;
            }

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

        /// <summary>
        /// 当前村庄移动键族（缓存）。改键后须 Rebuild。
        /// </summary>
        public VillageMoveKeyFamily ResolveVillageMoveKeyFamily()
        {
            return _cachedVillageMoveKeyFamily;
        }

        /// <summary>
        /// Idle→Walk/Run 当帧补横向：队列 → 绑定 Left/Right 按住。
        /// 0922：不再 GetKey(A/D/箭头) 与 Axis，避免另一族漏进来。
        /// </summary>
        public ControlInputType ResolveVillageEnterHorizontalCommand()
        {
            ControlInputType queued = FindFirstHorizontalCommandInQueue();
            if (queued == ControlInputType.Left || queued == ControlInputType.Right)
            {
                return queued;
            }

            if (IsVillageHorizontalKeyHeld(-1))
            {
                return ControlInputType.Left;
            }

            if (IsVillageHorizontalKeyHeld(1))
            {
                return ControlInputType.Right;
            }

            return ControlInputType.None;
        }

        /// <param name="sign">-1 查左，+1 查右。</param>
        /// <remarks>0922：只认设置表绑定键，不再硬编码 A/D∪箭头双通道。</remarks>
        private bool IsVillageHorizontalKeyHeld(int sign)
        {
            if (sign < 0)
            {
                return IsBoundCommandKeyHeld(ControlInputType.Left);
            }

            return IsBoundCommandKeyHeld(ControlInputType.Right);
        }

        private bool IsBoundCommandKeyHeld(ControlInputType cmd)
        {
            // 0922 进对话禁移 C：绑定键按住也须认 cantMove（键族单通道不回退）
            if (cantMove)
            {
                return false;
            }

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
            RebuildKeyBindingsFromSettings();
        }

        /// <summary>
        /// 从设置重建 <c>keyCodeToCmdDict</c> 并刷新村庄键族缓存。
        /// 进场 OnInit 与设置改键/重置后都必须调用（验收项 7）。
        /// </summary>
        public void RebuildKeyBindingsFromSettings()
        {
            var configData = GameManager.GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
            keyCodeToCmdDict.Clear();
            var ignoreKeyList = new List<ControlInputType>()
            {
                ControlInputType.NextSentence, ControlInputType.SkipDialogue,
            };
            foreach (var data in configData.KeyboardMouseInputConfig)
            {
                var key = data.Value;
                var cmd = data.Key;
                if (ignoreKeyList.Contains(cmd)) { continue; }
                keyCodeToCmdDict[key] = cmd;
            }

            RefreshCachedVillageMoveKeyFamily(configData);
        }

        /// <summary>
        /// 按设置 Left/Right 判定键族。A+D→Wasd；←+→→ArrowKeys；其余 Custom（纵深 0）。
        /// </summary>
        private void RefreshCachedVillageMoveKeyFamily(SettingsConfigData configData)
        {
            KeyCode left = KeyCode.None;
            KeyCode right = KeyCode.None;
            if (configData != null && configData.KeyboardMouseInputConfig != null)
            {
                configData.KeyboardMouseInputConfig.TryGetValue(ControlInputType.Left, out left);
                configData.KeyboardMouseInputConfig.TryGetValue(ControlInputType.Right, out right);
            }

            if (left == KeyCode.A && right == KeyCode.D)
            {
                _cachedVillageMoveKeyFamily = VillageMoveKeyFamily.Wasd;
            }
            else if (left == KeyCode.LeftArrow && right == KeyCode.RightArrow)
            {
                _cachedVillageMoveKeyFamily = VillageMoveKeyFamily.ArrowKeys;
            }
            else
            {
                _cachedVillageMoveKeyFamily = VillageMoveKeyFamily.Custom;
            }
        }

        /// <summary>
        /// 设置改键后通知场景中所有玩家输入组件重建键表（局外改键时可能尚无玩家，安全空操作）。
        /// </summary>
        public static void RebuildKeyBindingsOnAllPlayers()
        {
            var inputs = UnityEngine.Object.FindObjectsOfType<PlayerInputComponent>();
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i] != null)
                {
                    inputs[i].RebuildKeyBindingsFromSettings();
                }
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

        /// <summary>
        /// 是否允许位移意图。故事/锁区 <c>SetAllowMove(false)</c> 后为 false。
        /// Town FixedUpdate 门闩读此属性（0922 进对话禁移 B）。
        /// </summary>
        public bool AllowMoveIntent => !cantMove;

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