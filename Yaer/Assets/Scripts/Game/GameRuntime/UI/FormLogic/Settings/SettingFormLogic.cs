using System;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameMgr.Manager.Settings;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.Control;
using UnityEngine;
using UnityEngine.UI;
using Game.GameMgr;
using Game.Static.Enum;
using System.Runtime.InteropServices;
using GameFramework.Setting;
using UnityEngine.U2D;
using Game.GameMgr.Component;
using System.Collections.Generic;
using Game.Static.Name.Settings;

namespace Game.GameRuntime.UI.FormLogic.Settings
{
	public class KeyBindingHelper {
        public ControlInputType currentTarget;
		public static void SetKeyBinding(ControlInputType controlInputType, KeyCode keyCode) {
			var configData = GameManager.GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
			if (configData.KeyboardMouseInputConfig.ContainsKey(controlInputType)) {
				configData.KeyboardMouseInputConfig[controlInputType] = keyCode;
			}
			else {
				configData.KeyboardMouseInputConfig.Add(controlInputType, keyCode);
			}
			// 保存配置
			GameManager.GetManager<SettingManager>().SaveSetting(configData);	
			
			Debug.Log($"按键绑定已保存: {controlInputType} -> {keyCode}");
		}
		
		/// <summary>
		/// 重置按键绑定为默认配置
		/// </summary>
		public static void ResetKeyBindingToDefault()
		{
			var settingManager = GameManager.GetManager<SettingManager>();
			var currentConfig = settingManager.LoadSetting<SettingsConfigData>();
			var defaultConfig = new SettingsConfigData();
			
			// 重置按键配置
			currentConfig.KeyboardMouseInputConfig.Clear();
			foreach (var kvp in defaultConfig.KeyboardMouseInputConfig)
			{
				//currentConfig.KeyboardMouseInputConfig[kvp.Key] = kvp.Value;
                currentConfig.KeyboardMouseInputConfig.Add(kvp.Key, kvp.Value);
			}
			
			settingManager.SaveSetting(currentConfig);			
			
			Debug.Log("按键绑定已重置为默认配置");
		}
	}

	public class SettingFormLogic : BaseUIFormLogic
    {
        [Space] [SerializeField] private Toggle TopTogSG;
        [SerializeField] private Toggle TopTogCK;
        [SerializeField] private GameObject SG;
        [SerializeField] private GameObject CK;

        [SerializeField] private Toggle windowedTg;
        [SerializeField] private Toggle fullscreenTg;

        [SerializeField] private SettingFormResolutionSelector resolutionSelector;
        [SerializeField] private Slider allVolumeSld;
        [SerializeField] private Slider bgmVolumeSld;
        [SerializeField] private Slider soundVolumeSld;
        [SerializeField] private Slider textSpeedSld;
        [SerializeField] private Slider autoPlaySpeedSld;
        [SerializeField] private Toggle showBattleImageTog;
        [SerializeField] private Toggle closeBattleImageTog;
        [SerializeField] private Toggle showWoundTog;
        [SerializeField] private Toggle closeWoundTog;
        [SerializeField] private ToggleGroupScript tgsWindowMode;
        [SerializeField] private Button defaultSettingBtn;
        [SerializeField] private Button defaultSettingBtn2;
        [SerializeField] private Button btnBack;

        // 图集
        SpriteAtlas controlsKeyAtlas;
        SpriteAtlas controlsKeyAtlas_en;
        SpriteAtlas controlsKeyAtlas_jp;
        SpriteAtlas soundAtlas;
        SpriteAtlas soundAtlas_en;
        SpriteAtlas soundAtlas_jp;
        SpriteAtlas tipsAtlas;
        
        // 需要改变UI的图片
        public GameObject imgWindow;
        public GameObject imgWindow_select;
        public GameObject imgFullScreen;
        public GameObject imgFullScreen_select;
        public GameObject imgWindowMode;
        public GameObject imgWindowMode_select;
        public GameObject imgWindowSize;
        public GameObject imgWindowSize_select;
        public GameObject imgAllVolum;
        public GameObject imgAllVolum_select;
        public GameObject imgBgmVolum;
        public GameObject imgBgmVolum_select;
        public GameObject imgSfxVolum;
        public GameObject imgSfxVolum_select;
        public GameObject imgTextShowTime;
        public GameObject imgTextShowTime_select;
        public GameObject imgAutoShowTime;
        public GameObject imgAutoShowTime_select;
        public GameObject imgShowBigRole;
        public GameObject imgShowBigRole_select;
        public GameObject imgShowWound;
        public GameObject imgShowWound_select;
        public GameObject imgYes_1;
        public GameObject imgYes_2;
        public GameObject imgNo_1;
        public GameObject imgNo_2;
        public GameObject imgSceneTag;
        public GameObject imgKeyTag;
        public GameObject imgSceneTag_select;
        public GameObject imgKeyTag_select;
        public Button btnReset;
        public Button btnReset_2; // 按键界面的按钮
        public GameObject imgKeyAndInputTips;
        public GameObject imgLeft;
        public GameObject imgLeft_select;
        public GameObject imgRight;
        public GameObject imgRight_select;
        public GameObject imgSquat;
        public GameObject imgSquat_select;
        public GameObject imgJump;
        public GameObject imgJump_select;
        public GameObject imgNorAtk_1;
        public GameObject imgNorAtk_1_select;
        public GameObject imgNorAtk_2;
        public GameObject imgNorAtk_2_select;
        public GameObject imgDashAtk;
        public GameObject imgDashAtk_select;
        public GameObject imgInteractive;
        public GameObject imgInteractive_select;
        public GameObject imgSit;
        public GameObject imgSit_select;
        public GameObject imgNextText;
        public GameObject imgNextText_select;
        public GameObject imgSkipText;
        public GameObject imgSkipText_select;
        public GameObject imgUnOpenTips;
        // 其他
        public GameObject textShowFast;
        public GameObject textShowSlow;
        public GameObject textAutoFast;
        public GameObject textAutoSlow;

        Dictionary<LanguageEnumType, string> otherTextDatasFast = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "快"}, { LanguageEnumType.English, "Fast"},{ LanguageEnumType.Japanese, "Fast"},
        };
        Dictionary<LanguageEnumType, string> otherTextDatasSlow = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "慢"}, { LanguageEnumType.English, "Slow"},{ LanguageEnumType.Japanese, "Slow"},
        };

        private SettingManager settingManager;

        private KeyboardMouseInputKeyConfigItem[] keyConfigItems;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            settingManager = GameManager.GetManager<SettingManager>();
            keyConfigItems = CK.GetComponentsInChildren<KeyboardMouseInputKeyConfigItem>(true);
            foreach (var keyConfigItem in keyConfigItems)
            {
                keyConfigItem.parentUILogic = this;
            }

            TopTogSG.onValueChanged.AddListener(ShowSG);
            TopTogCK.onValueChanged.AddListener(ShowCK);
            btnBack.onClick.AddListener(OnBtnBack);
            defaultSettingBtn.onClick.AddListener(OnClickDefaultSettingBtn);
            defaultSettingBtn2.onClick.AddListener(OnClickDefaultSettingKeyBtn);
            windowedTg.onValueChanged.AddListener(OnWindowedTgChanged);
            fullscreenTg.onValueChanged.AddListener(OnFullscreenTgChanged);

            resolutionSelector.OnResolutionChanged.AddListener(SetResolution);

            allVolumeSld.onValueChanged.AddListener(OnAllVolumeSldChanged);
            bgmVolumeSld.onValueChanged.AddListener(OnBgmVolumeSldChanged);
            soundVolumeSld.onValueChanged.AddListener(OnSoundVolumeSldChanged);

            textSpeedSld.onValueChanged.AddListener(OnTextSpeedSldChanged);
            autoPlaySpeedSld.onValueChanged.AddListener(OnAutoPlaySpeedSldChanged);

            showBattleImageTog.onValueChanged.AddListener(OnShowBattleImageTogChanged);
            closeBattleImageTog.onValueChanged.AddListener(OnCloseBattleImageTogChanged);
            showWoundTog.onValueChanged.AddListener(OnShowWoundTogChanged);
            closeWoundTog.onValueChanged.AddListener(OnCloseWoundTogChanged);

            ShowSG(true);

            LoadAtlas(7);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var resCpnGM = GameManager.GetGMComponent<ResComponentGM>();
            var path = "Assets/GameRes/Atlas/SettingPanel/controlsKey.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (controlsKeyAtlas != null) { return; }
                controlsKeyAtlas = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/SettingPanel/controlsKey_en.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (controlsKeyAtlas_en != null) { return; }
                controlsKeyAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/SettingPanel/controlsKey_jp.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (controlsKeyAtlas_jp != null) { return; }
                controlsKeyAtlas_jp = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/SettingPanel/soundAndGraphic.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (soundAtlas != null) { return; }
                soundAtlas = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/SettingPanel/soundAndGraphic_en.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (soundAtlas_en != null) { return; }
                soundAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/SettingPanel/soundAndGraphic_jp.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (soundAtlas_jp != null) { return; }
                soundAtlas_jp = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/SettingPanel/tipImg.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (tipsAtlas != null) { return; }
                tipsAtlas = atlas;
                loadAtlasCallFunc();
            });
        }

        // 多语言修改图片UI
        public override void UpdateUI()
        {
            base.UpdateUI();

            // 设置部分不需要加载图集的翻译
            var curLaunageType = GameManager.Instance.language;
            if (otherTextDatasFast.ContainsKey(curLaunageType))
            {
                var text = otherTextDatasFast[curLaunageType];
                GameTools.setText(textShowFast, text);
                GameTools.setText(textAutoFast, text);
            }
            if (otherTextDatasSlow.ContainsKey(curLaunageType))
            {
                var text = otherTextDatasSlow[curLaunageType];
                GameTools.setText(textShowSlow, text);
                GameTools.setText(textAutoSlow, text);
            }
            controlsKeyAtlas_jp = controlsKeyAtlas_jp == null ? controlsKeyAtlas_en : controlsKeyAtlas_jp;

            Dictionary<LanguageEnumType, SpriteAtlas> spriteAtlasData1 = new Dictionary<LanguageEnumType, SpriteAtlas>() {
                { LanguageEnumType.Chinese, controlsKeyAtlas }, {  LanguageEnumType.English, controlsKeyAtlas_en },
                {  LanguageEnumType.Japanese, controlsKeyAtlas_jp },
            };
            Dictionary<LanguageEnumType, SpriteAtlas> spriteAtlasData2 = new Dictionary<LanguageEnumType, SpriteAtlas>() {
                { LanguageEnumType.Chinese, soundAtlas }, {  LanguageEnumType.English, soundAtlas_en },
                {  LanguageEnumType.Japanese, soundAtlas_jp },
            };
            // 设置界面日语全部使用英文配置
            curLaunageType = curLaunageType == LanguageEnumType.Japanese ? LanguageEnumType.English : curLaunageType;
            SpriteAtlas curControlsAtlas;
            SpriteAtlas curSoundAtlas;
            if (!spriteAtlasData1.ContainsKey(curLaunageType))
            {
                // 不存在的语言一律使用英文
                curControlsAtlas = controlsKeyAtlas_en;
            }
            else
            {
                curControlsAtlas = spriteAtlasData1[curLaunageType];
            }
            if (!spriteAtlasData2.ContainsKey(curLaunageType))
            {
                // 不存在的语言一律使用英文
                curSoundAtlas = soundAtlas_en;
            }
            else
            {
                curSoundAtlas = spriteAtlasData2[curLaunageType];
            }
            // ===============音画部分
            GameTools.loadTextureByAtlas(imgSceneTag, curSoundAtlas, "音画");
            GameTools.loadTextureByAtlas(imgKeyTag, curSoundAtlas, "按键");
            GameTools.loadTextureByAtlas(imgSceneTag_select, curSoundAtlas, "音画选择");
            GameTools.loadTextureByAtlas(imgKeyTag_select, curSoundAtlas, "按键选择");
            GameTools.loadTextureByAtlas(imgWindow, curSoundAtlas, "窗口");
            GameTools.loadTextureByAtlas(imgWindow_select, curSoundAtlas, "窗口选择");
            GameTools.loadTextureByAtlas(imgFullScreen, curSoundAtlas, "全屏");
            GameTools.loadTextureByAtlas(imgFullScreen_select, curSoundAtlas, "全屏选择");
            GameTools.loadTextureByAtlas(imgWindowMode, curSoundAtlas, "画面模式");
            GameTools.loadTextureByAtlas(imgWindowMode_select, curSoundAtlas, "画面模式选择");
            GameTools.loadTextureByAtlas(imgWindowSize, curSoundAtlas, "分辨率");
            GameTools.loadTextureByAtlas(imgWindowSize_select, curSoundAtlas, "分辨率选择");
            GameTools.loadTextureByAtlas(imgAllVolum, curSoundAtlas, "全体音量");
            GameTools.loadTextureByAtlas(imgAllVolum_select, curSoundAtlas, "全体音量选择");
            GameTools.loadTextureByAtlas(imgBgmVolum, curSoundAtlas, "BGM");
            GameTools.loadTextureByAtlas(imgBgmVolum_select, curSoundAtlas, "BGM选择");
            GameTools.loadTextureByAtlas(imgSfxVolum, curSoundAtlas, "音效");
            GameTools.loadTextureByAtlas(imgSfxVolum_select, curSoundAtlas, "音效选择");
            GameTools.loadTextureByAtlas(imgTextShowTime, curSoundAtlas, "文本显示时间");
            GameTools.loadTextureByAtlas(imgTextShowTime_select, curSoundAtlas, "文本显示时间选择");
            GameTools.loadTextureByAtlas(imgAutoShowTime, curSoundAtlas, "自动显示时间");
            GameTools.loadTextureByAtlas(imgAutoShowTime_select, curSoundAtlas, "自动显示时间选择");
            GameTools.loadTextureByAtlas(imgShowBigRole, curSoundAtlas, "显示战斗立绘");
            GameTools.loadTextureByAtlas(imgShowBigRole_select, curSoundAtlas, "显示战斗立绘选择");
            GameTools.loadTextureByAtlas(imgShowWound, curSoundAtlas, "显示伤口");
            GameTools.loadTextureByAtlas(imgShowWound_select, curSoundAtlas, "显示伤口选择");
            GameTools.loadTextureByAtlas(imgYes_1, curSoundAtlas, "是");
            GameTools.loadTextureByAtlas(imgYes_2, curSoundAtlas, "是");
            GameTools.loadTextureByAtlas(imgNo_1, curSoundAtlas, "否");
            GameTools.loadTextureByAtlas(imgNo_2, curSoundAtlas, "否");
            
            var baseKeyName = "按键绑定未开放{0}";
            var tag = LanguageType.GetLanaguageResTag(curLaunageType);
            var realKeyName = string.Format(baseKeyName, tag);
            GameTools.loadTextureByAtlas(imgUnOpenTips, tipsAtlas, realKeyName);
            var resetSpriteNor = curSoundAtlas.GetSprite("初始化");
            var resetSpriteSelect = curSoundAtlas.GetSprite("初始化选择");
            var resetSpriteClick = curSoundAtlas.GetSprite("初始化点");
            GameTools.loadBtnSprite(btnReset, resetSpriteNor, resetSpriteSelect, resetSpriteClick);
            GameTools.loadBtnSprite(btnReset_2, resetSpriteNor, resetSpriteSelect, resetSpriteClick);
            //================================按键部分
            GameTools.loadTextureByAtlas(imgKeyAndInputTips, curControlsAtlas, "键鼠 手柄");
            GameTools.loadTextureByAtlas(imgLeft, curControlsAtlas, "向左");
            GameTools.loadTextureByAtlas(imgLeft_select, curControlsAtlas, "向左选择");
            GameTools.loadTextureByAtlas(imgRight, curControlsAtlas, "向右");
            GameTools.loadTextureByAtlas(imgRight_select, curControlsAtlas, "向右选择");
            GameTools.loadTextureByAtlas(imgSquat, curControlsAtlas, "下蹲");
            GameTools.loadTextureByAtlas(imgSquat_select, curControlsAtlas, "下蹲选择");
            GameTools.loadTextureByAtlas(imgJump, curControlsAtlas, "跳跃");
            GameTools.loadTextureByAtlas(imgJump_select, curControlsAtlas, "跳跃选择");
            GameTools.loadTextureByAtlas(imgNorAtk_1, curControlsAtlas, "轻击");
            GameTools.loadTextureByAtlas(imgNorAtk_1_select, curControlsAtlas, "轻击选择");
            GameTools.loadTextureByAtlas(imgNorAtk_2, curControlsAtlas, "重击");
            GameTools.loadTextureByAtlas(imgNorAtk_2_select, curControlsAtlas, "重击选择");
            GameTools.loadTextureByAtlas(imgDashAtk, curControlsAtlas, "冲锋");
            GameTools.loadTextureByAtlas(imgDashAtk_select, curControlsAtlas, "冲锋选择");
            GameTools.loadTextureByAtlas(imgInteractive, curControlsAtlas, "交互");
            GameTools.loadTextureByAtlas(imgInteractive_select, curControlsAtlas, "交互选择");
            GameTools.loadTextureByAtlas(imgSit, curControlsAtlas, "坐下");
            GameTools.loadTextureByAtlas(imgSit_select, curControlsAtlas, "坐下选择");
            GameTools.loadTextureByAtlas(imgNextText, curControlsAtlas, "下一文本");
            GameTools.loadTextureByAtlas(imgNextText_select, curControlsAtlas, "下一文本选择");
            GameTools.loadTextureByAtlas(imgSkipText, curControlsAtlas, "跳过文本");
            GameTools.loadTextureByAtlas(imgSkipText_select, curControlsAtlas, "跳过文本选择");
            imgLeft.SetActive(false);
            imgRight.SetActive(false);
            imgSquat.SetActive(false);
            imgJump.SetActive(false);
            imgNorAtk_1.SetActive(false);
            imgNorAtk_2.SetActive(false);
            imgDashAtk.SetActive(false);
            imgInteractive.SetActive(false);
            imgSit.SetActive(false);
            imgNextText.SetActive(false);
            imgSkipText.SetActive(false);

            // 部分UI不同语言需要不同的适配
            adaptUI();
        }
        void adaptUI()
        {
            //var curLanageType = GameManager.Instance.language;
            //if (curLanageType == LanguageEnumType.Chinese)
            //{
            //    imgTextShowTime.transform.localScale = Vector3.one;
            //    imgTextShowTime_select.transform.localScale = Vector3.one;
            //    imgAutoShowTime.transform.localScale = Vector3.one;
            //    imgAutoShowTime_select.transform.localScale = Vector3.one;
            //}
            //else
            //{
            //    imgTextShowTime.transform.localScale = new Vector2(0.8f, 0.8f);
            //    imgTextShowTime_select.transform.localScale = new Vector2(0.8f, 0.8f);
            //    imgAutoShowTime.transform.localScale = new Vector2(0.8f, 0.8f);
            //    imgAutoShowTime_select.transform.localScale = new Vector2(0.8f, 0.8f);
            //}
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            AllowOpenMenu(false);
            if (settingManager.SettingData == null)
            {
                settingManager.SettingData = settingManager.LoadSetting<SettingsConfigData>();
            }
            UpdateView(settingManager.SettingData);
            componentSystemUI.GetComponent<BlackFadeComponent>().HideFade();
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            AllowOpenMenu(true);
            /* 退出方法这里有个非常奇怪的BUG，在非开始游戏场景中打开界面，然后修改SettingData成员变量的数据，
             * 如果没有修改SettingData本身而直接退出界面时走到这里的时候
             * 原本修改过的SettingData成员变量的数据又变回刚打开界面时的数据了。。。2025/7/25
             * 复现BUG时可注释BGM变化时的SaveSetting方法，然后修改BGM音量大小之后直接关闭界面调试查看
             * 现在改成在所有有修改的地方都调用SaveSetting,而不是只在关闭界面调用
             */
            settingManager.SaveSetting(settingManager.SettingData);
            componentSystemUI.GetComponent<BlackFadeComponent>().ResetHideState();
        }

		private void ShowSG(bool isOn)
        {
            SG.SetActive(isOn);
        }

        private void ShowCK(bool isOn)
        {
            CK.SetActive(isOn);
        }

        public void UpdateView(SettingsConfigData data)
        {
            // 分辨率选项始终可见，允许全屏下也调整。
            resolutionSelector.UseResolution(true);
            if (data.windowMode == SettingsConfigData.EWindowMode.Windowed)
            {
                tgsWindowMode.ActiveOption("Windowed");
            }
            else
            {
                tgsWindowMode.ActiveOption("FullScreen");
            }

            resolutionSelector.SetResolutionImage(SettingsConfigData.GetResolution(data.resolvingPower));
            allVolumeSld.value = data.allVolume;
            bgmVolumeSld.value = data.bgmVolume;
            soundVolumeSld.value = data.soundVolume;
            textSpeedSld.value = data.textSpeed;
            autoPlaySpeedSld.value = data.autoPlaySpeed;
            if (data.showBattleImage)
            {
                showBattleImageTog.isOn = true;
                closeBattleImageTog.isOn = false;
            }
            else
            {
                showBattleImageTog.isOn = false;
                closeBattleImageTog.isOn = true;
            }

            if (data.showWound)
            {
                showWoundTog.isOn = true;
                closeWoundTog.isOn = false;
            }
            else
            {
                showWoundTog.isOn = false;
                closeWoundTog.isOn = true;
            }
        }

        private void SetWindowMode(bool windowed)
        {
            // 分辨率选项始终可见，允许全屏下也调整。
            resolutionSelector.UseResolution(true);
            if (windowed)
            {
                tgsWindowMode.ActiveOption("Windowed");
            }
            else
            {
                tgsWindowMode.ActiveOption("FullScreen");
            }

            settingManager.SettingData.windowMode = windowed ? SettingsConfigData.EWindowMode.Windowed : SettingsConfigData.EWindowMode.FullScreen;
            ApplyDisplaySettings();
            settingManager.SaveSetting(settingManager.SettingData);
        }

        private void SetResolution(int w, int h)
        {
            Debug.Log($"修改分辨率：{w} * {h}");
            settingManager.SettingData.resolvingPower = SettingsConfigData.GetResolvingEnum(w, h);
            ApplyDisplaySettings();

            settingManager.SaveSetting(settingManager.SettingData);
        }

        private void ApplyDisplaySettings()
        {
            var data = settingManager.SettingData;
            var (width, height) = SettingsConfigData.GetResolution(data.resolvingPower);
            var isWindowed = data.windowMode == SettingsConfigData.EWindowMode.Windowed;
            var fullScreenMode = isWindowed ? FullScreenMode.Windowed : FullScreenMode.ExclusiveFullScreen;

            Screen.SetResolution(width, height, fullScreenMode);
            Debug.Log($"应用显示设置: {width}x{height}, mode={Screen.fullScreenMode}, current={Screen.currentResolution.width}x{Screen.currentResolution.height}");
        }

        private void OnAllVolumeSldChanged(float value)
        {
            settingManager.SettingData.allVolume = value;
            settingManager.OnVolumeChange?.Invoke(
                new VolumeChangedArgs(
                    settingManager.SettingData.allVolume, settingManager.SettingData.bgmVolume, settingManager.SettingData.soundVolume
                )
            );
            settingManager.SaveSetting(settingManager.SettingData);
        }
        private void OnBgmVolumeSldChanged(float value)
        {
            settingManager.SettingData.bgmVolume = value;
            settingManager.OnVolumeChange?.Invoke(
                new VolumeChangedArgs(
                    settingManager.SettingData.allVolume, settingManager.SettingData.bgmVolume, settingManager.SettingData.soundVolume
                )
            );
            settingManager.SaveSetting(settingManager.SettingData);
        }
        private void OnSoundVolumeSldChanged(float value)
        {
            settingManager.SettingData.soundVolume = value;
            settingManager.OnVolumeChange?.Invoke(
                new VolumeChangedArgs(
                    settingManager.SettingData.allVolume, settingManager.SettingData.bgmVolume, settingManager.SettingData.soundVolume
                )
            );
            settingManager.SaveSetting(settingManager.SettingData);
        }

        private void OnTextSpeedSldChanged(float time)
        {
            settingManager.SettingData.textSpeed = time;
            settingManager.OnTextShowTimeChange?.Invoke(time);

            settingManager.SaveSetting(settingManager.SettingData);
        }

        private void OnAutoPlaySpeedSldChanged(float time)
        {
            settingManager.SettingData.autoPlaySpeed = time;
            settingManager.OnAutoShowTimeChange?.Invoke(time);

            settingManager.SaveSetting(settingManager.SettingData);
        }

        private void OnWindowedTgChanged(bool value)
        {
            if (value)
            {
                SetWindowMode(true);
            }
        }

        private void OnFullscreenTgChanged(bool value)
        {
            if (value)
            {
                SetWindowMode(false);
            }
        }

        private void OnShowBattleImageTogChanged(bool value)
        {
            if (value)
            {
                SetShowBattleImage(true);
            }
        }

        private void OnCloseBattleImageTogChanged(bool value)
        {
            if (value)
            {
                SetShowBattleImage(false);
            }
        }

        private void OnShowWoundTogChanged(bool value)
        {
            if (value)
            {
                SetShowWound(true);
            }
        }

        private void OnCloseWoundTogChanged(bool value)
        {
            if (value)
            {
                SetShowWound(false);
            }
        }

        private void SetShowBattleImage(bool b)
        {
            settingManager.SettingData.showBattleImage = b;
            settingManager.OnBattleImageChange?.Invoke(b);

            settingManager.SaveSetting(settingManager.SettingData);
        }

        private void SetShowWound(bool b)
        {
            settingManager.SettingData.showWound = b;
            settingManager.OnShowWoundChange?.Invoke(b);

            settingManager.SaveSetting(settingManager.SettingData);
        }

        public void SetKeyDefault()
        {
            // 使用KeyBindingHelper重置按键配置
            KeyBindingHelper.ResetKeyBindingToDefault();
            
            // 刷新所有按键配置项的显示
            foreach (var keyConfigItem in keyConfigItems)
            {
                keyConfigItem.ResetToDefault();
            }
            
            // 触发设置管理器的重置事件
            settingManager.OnSetKeyDefault?.Invoke();
            
            Debug.Log("所有按键配置已重置为默认值");
        }

        private void OnClickDefaultSettingBtn()
        {
            UIUtils.PlayBtnAudio(this);
            settingManager.SettingData = new SettingsConfigData();
            UpdateView(settingManager.SettingData);

            settingManager.OnSetSettingsDefault?.Invoke();
        }
        private void OnClickDefaultSettingKeyBtn()
		{
            UIUtils.PlayBtnAudio(this);
            SetKeyDefault();
		}

        private void OnBtnBack()
        {
            UIUtils.PlayBtnAudio(this);
            componentSystemUI.GetComponent<BlackFadeComponent>().CloseFormShowFade(UIForm);
        }
    }
}