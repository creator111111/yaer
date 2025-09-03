using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Static.Enum;

namespace Game.GameMgr.Manager.Settings.Helper
{
    public class SettingsConfigData
    {
        public enum EResolvingPower
        {
            _640x360,
            _720x405,
            _800x450,
            _1152x648,
            _1176x661,
            _1280x720,
            _1360x765,
            _1365x768,
            _1440x810,
            _1600x900,
            _1680x945,
            _1920x1080,
            _2560x1440
        }

        public enum EWindowMode
        {
            Windowed,
            FullScreen
        }
        /// <summary>
        /// 全体音量
        /// </summary>
        public float allVolume = 0.5f;
        /// <summary>
        /// BGM音量
        /// </summary>
        public float bgmVolume { get;set;} = 0.5f;
        public EResolvingPower resolvingPower = EResolvingPower._1920x1080;
        public bool showBattleImage = true;
        public bool showWound = true;
        /// <summary>
        /// 音效音量
        /// </summary>
        public float soundVolume = 0.5f;
        /// <summary>
        /// 文本显示速度
        /// </summary>
        public float textSpeed = 0.075f;
		public float autoPlaySpeed = 0.85f;
		public EWindowMode windowMode = EWindowMode.FullScreen;

        public Dictionary<ControlInputType, KeyCode> KeyboardMouseInputConfig = new Dictionary<ControlInputType, KeyCode>() 
        {
            { ControlInputType.Left, KeyCode.LeftArrow },
            { ControlInputType.Right, KeyCode.RightArrow },
            { ControlInputType.Squat, KeyCode.E },
            { ControlInputType.Jump, KeyCode.Space },
            { ControlInputType.NormalAttack, KeyCode.Q },
            { ControlInputType.SmashAttack, KeyCode.W },
            { ControlInputType.DashAttack, KeyCode.R },
            { ControlInputType.Interact, KeyCode.T },
            { ControlInputType.SitDown, KeyCode.LeftShift },
            { ControlInputType.NextSentence, KeyCode.Space },
            { ControlInputType.SkipDialogue, KeyCode.LeftAlt }
        };

        public static (int width, int height) GetResolution(EResolvingPower _resolvingPower)
        {
            switch (_resolvingPower)
            {
                case EResolvingPower._640x360:
                    return (640, 360);
                case EResolvingPower._720x405:
                    return (720, 405);
                case EResolvingPower._800x450:
                    return (800, 450);
                case EResolvingPower._1152x648:
                    return (1152, 648);
                case EResolvingPower._1176x661:
                    return (1176, 661);
                case EResolvingPower._1280x720:
                    return (1280, 720);
                case EResolvingPower._1360x765:
                    return (1360, 765);
                case EResolvingPower._1365x768:
                    return (1365, 768);
                case EResolvingPower._1440x810:
                    return (1440, 810);
                case EResolvingPower._1600x900:
                    return (1600, 900);
                case EResolvingPower._1680x945:
                    return (1680, 945);
                case EResolvingPower._1920x1080:
                    return (1920, 1080);
                case EResolvingPower._2560x1440:
                    return (2560, 1440);
                default:
                    return (1920, 1080);
            }
        }

        public static EResolvingPower GetResolvingEnum(int w, int h)
        {
            string enumStr = $"_{w}x{h}";
            try
            {
                var result = (EResolvingPower)Enum.Parse(typeof(EResolvingPower), enumStr);
                return result;
            }
            catch (Exception e) 
            {
                Debug.LogException(e);
                return EResolvingPower._1920x1080;
            }
        }
    }
}