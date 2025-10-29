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

        private InputActions inputActions;

        private float axisX;
        private float axisY;
        private bool cantFlip;
        private bool cantMove;
        private bool cantJump;
        private bool cantLeft;
        private bool cantRight;

        private float lastRealTime = 0;
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
                if (Input.GetKeyDown(KeyCode.A))
                {
                    Debug.Log("============curPlayerAllCmds" + curPlayerAllCmds);
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
            // 获取不同指令对应的输入方法并执行
            if (controlInputFuncDict.TryGetValue(curPlayerCmd, out Action curInputAction))
            {
                curInputAction?.Invoke();
                // 如果有指令组合就继续往下添加逻辑
            }
        }

        void ParseMoveCmd(ControlInputType curPlayerCmd)
        {
            if (PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>().IsMoveUp ||
                PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>().IsMoveDown)
            {
                return;// 跳跃过程中不允许移动
            }
            // 
            if (cantLeft && axisX < 0) axisX = 0;
            if (cantRight && axisX > 0) axisX = 0;

            // 是否允许翻转
            if (cantFlip) axisX = 0;
            // 获取不同指令对应的输入方法并执行
            if (moveInputFuncDict.TryGetValue(curPlayerCmd, out Action<bool> curInputAction))
            {
                PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>().moveDirs.Clear();
                curInputAction?.Invoke(true);
                // 添加人物的移动方向
                if (moveCmdDirData.TryGetValue(curPlayerCmd, out EDirectionType moveDir))
                {
                    PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>().moveDirs.Add(moveDir);
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
                            PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>().moveDirs.Add(moveDir2);
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